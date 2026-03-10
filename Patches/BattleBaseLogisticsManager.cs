using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 战场基站物流系统
    /// 管理每个基站的无人机派遣和飞行
    /// </summary>
    public class BaseLogisticSystem
    {
        public int battleBaseId;
        public int planetId;
        
        // 无人机数据（与游戏的 CourierData 完全兼容），容量由配置 BattleBaseCourierCount 决定
        public CourierData[] couriers = new CourierData[20];
        public int idleCount = 20;
        /// <summary> 本基站无人机容量（创建时由配置决定，之后不变） </summary>
        public int CourierCapacity => couriers?.Length ?? 0;
        public int workingCount = 0;
        
        // 库存追踪（用于检测变化）
        public Dictionary<int, int> lastInventory = new Dictionary<int, int>();
        
        // 派遣冷却（避免频繁派遣，每60帧=1秒检查一次）
        public int cooldownCounter = 0;
        public const int DISPATCH_INTERVAL = 60;
        /// <summary> 无人机在基站端沿星球径向（pos.normalized）的偏移距离，单位米。 </summary>
        public const float DRONE_AT_BASE_HEIGHT_OFFSET = 7.5f;
    }

    /// <summary>
    /// 配送器需求信息（也用于机甲配送栏、物流塔：IsMechaSlot/IsStationTower 时表示送往对应目标）
    /// </summary>
    public class DispenserDemand
    {
        public int dispenserId;
        public int entityId;
        public int storageId;
        public int itemId;              // 需求的物品ID
        public int currentStock;        // 当前库存
        public int maxStock;            // 最大容量
        public float urgency;           // 紧急度 0-1（越小越紧急）
        public Vector3 position;        // 位置
        public float distance;         // 距离基站的距离

        /// <summary> true 表示送往机甲（玩家）配送栏槽位，此时 dispenserId 忽略，用 slotIndex 标识目标 </summary>
        public bool IsMechaSlot;
        /// <summary> 机甲配送栏槽位索引 0..gridLength-1，仅当 IsMechaSlot 时有效 </summary>
        public int slotIndex;
        /// <summary> 需要补货的数量（机甲槽位用；配送器需求可忽略） </summary>
        public int needCount;

        /// <summary> true 表示送往物流塔（本地需求槽位），此时 dispenserId 忽略，用 stationId 标识目标 </summary>
        public bool IsStationTower;
        /// <summary> 物流塔在 PlanetTransport.stationPool 中的 id，仅当 IsStationTower 时有效 </summary>
        public int stationId;
        /// <summary> 仅当 IsStationTower 且 itemId=翘曲器 时有效：true=送往塔的翘曲器小存储点(warperCount)，false=送往槽位(storage) </summary>
        public bool IsWarperStorageDemand;
        /// <summary> 物流塔 station.storage 的槽位索引，仅当 IsStationTower 且非翘曲器小格时有效，用于维护 localOrder </summary>
        public int StationStorageIndex;

        /// <summary> true 表示从供应配送器拉货回基站输入区，无人机去该配送器取货后送回基站 </summary>
        public bool IsSupplyFetch;
    }

    /// <summary>
    /// 本帧可派遣的上下文：由 Manager 内部完成 cooldown、库存变化、无空闲/无需求等判断后返回
    /// </summary>
    public class DispatchContext
    {
        public List<DispenserDemand> Demands;
        public Dictionary<int, int> CurrentInventory;
        public Vector3 BasePosition;

        public DispatchContext(List<DispenserDemand> demands, Dictionary<int, int> currentInventory, Vector3 basePosition)
        {
            Demands = demands;
            CurrentInventory = currentInventory;
            BasePosition = basePosition;
        }
    }

    /// <summary>
    /// 全局战场基站物流管理器
    /// </summary>
    public static class BattleBaseLogisticsManager
    {
        // planetId -> battleBaseId -> BaseLogisticSystem
        private static Dictionary<int, Dictionary<int, BaseLogisticSystem>> _systems = new Dictionary<int, Dictionary<int, BaseLogisticSystem>>();

        /// <summary> 基站拉货在途数量：(planetId, battleBaseId, itemId) -> 在途数量，派遣时+、回程到达或取货失败时- </summary>
        private static Dictionary<(int planetId, int battleBaseId, int itemId), int> _baseFetchInFlight = new Dictionary<(int, int, int), int>();
        private static object _inFlightLock = new object();

        private static object _lock = new object();

        /// <summary>
        /// 获取或创建基站物流系统
        /// </summary>
        public static BaseLogisticSystem GetOrCreate(int planetId, int battleBaseId)
        {
            lock (_lock)
            {
                if (!_systems.ContainsKey(planetId))
                    _systems[planetId] = new Dictionary<int, BaseLogisticSystem>();

                if (!_systems[planetId].ContainsKey(battleBaseId))
                {
                    int capacity = Plugin.GetBattleBaseCourierCount();
                    _systems[planetId][battleBaseId] = new BaseLogisticSystem
                    {
                        battleBaseId = battleBaseId,
                        planetId = planetId,
                        couriers = new CourierData[capacity],
                        idleCount = capacity,
                        workingCount = 0
                    };
                }

                return _systems[planetId][battleBaseId];
            }
        }

        /// <summary>
        /// 获取星球的所有基站物流系统
        /// </summary>
        public static IEnumerable<BaseLogisticSystem> GetAllForPlanet(int planetId)
        {
            lock (_lock)
            {
                if (_systems.TryGetValue(planetId, out var planetSystems))
                {
                    return planetSystems.Values.ToList();
                }
                return Enumerable.Empty<BaseLogisticSystem>();
            }
        }

        /// <summary> 物流塔普通槽位：endId = STATION_ENDID_OFFSET + stationId </summary>
        private const int STATION_ENDID_OFFSET = 20000;
        /// <summary> 物流塔翘曲器小格：endId = STATION_WARPER_STORAGE_ENDID_OFFSET + stationId </summary>
        private const int STATION_WARPER_STORAGE_ENDID_OFFSET = 30000;
        private const int ITEMID_WARPER = 1210;

        /// <summary>
        /// 本星球是否有任意无人机正在往该星际塔送翘曲器（去程在途，含普通槽位与翘曲器小格）。
        /// </summary>
        public static bool HasWarperCourierInTransitToStation(int planetId, int stationId)
        {
            int targetEndId = STATION_ENDID_OFFSET + stationId;
            int targetWarperEndId = STATION_WARPER_STORAGE_ENDID_OFFSET + stationId;
            foreach (var logistics in GetAllForPlanet(planetId))
            {
                if (logistics.couriers == null) continue;
                for (int i = 0; i < logistics.couriers.Length; i++)
                {
                    ref readonly var c = ref logistics.couriers[i];
                    if (c.maxt <= 0f || c.direction <= 0f) continue;
                    if (c.itemId == ITEMID_WARPER && (c.endId == targetEndId || c.endId == targetWarperEndId)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 本星球所有基站中，正在飞往指定配送器且携带指定物品的在途数量（用于目标配送器 UI 显示「即将收到」）。
        /// 仅统计 endId 为配送器 id（非物流塔、非机甲）且去程飞行中（maxt &gt; 0, direction &gt; 0）的无人机携带量之和。
        /// </summary>
        public static int GetIncomingToDispenser(int planetId, int dispenserId, int itemId)
        {
            if (dispenserId <= 0 || itemId <= 0) return 0;
            int sum = 0;
            foreach (var logistics in GetAllForPlanet(planetId))
            {
                if (logistics.couriers == null) continue;
                for (int i = 0; i < logistics.couriers.Length; i++)
                {
                    ref readonly var c = ref logistics.couriers[i];
                    if (c.maxt <= 0f || c.direction <= 0f) continue;
                    if (c.endId != dispenserId || c.itemId != itemId) continue;
                    sum += c.itemCount;
                }
            }
            return sum;
        }

        /// <summary>
        /// 从工厂的 transport.dispenserPool 获取配送器组件，供派遣/送达/存档时维护 storageOrdered 使用。
        /// </summary>
        public static DispenserComponent? GetDispenser(PlanetFactory factory, int dispenserId)
        {
            if (factory?.transport == null || dispenserId <= 0) return null;
            var transport = factory.transport;
            var dispenserPoolField = transport.GetType().GetField("dispenserPool",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (dispenserPoolField == null) return null;
            var dispenserPool = dispenserPoolField.GetValue(transport) as Array;
            if (dispenserPool == null || dispenserId >= dispenserPool.Length) return null;
            var dispenserObj = dispenserPool.GetValue(dispenserId);
            var dispenser = dispenserObj as DispenserComponent;
            return (dispenser != null && dispenser.id == dispenserId) ? dispenser : null;
        }

        /// <summary>
        /// 扣减物流塔指定物品槽位的 localOrder（用于到达/返还/存档时与派遣时增加的 localOrder 对应）。
        /// 按 itemId 查找该塔第一个匹配的 storage 槽位并扣减，仅支持普通槽位（非翘曲器小格）。
        /// </summary>
        public static void DecrementStationSlotLocalOrder(PlanetFactory factory, int stationId, int itemId, int amount)
        {
            if (factory?.transport == null || stationId <= 0 || itemId <= 0 || amount <= 0) return;
            StationComponent? station = factory.transport.GetStationComponent(stationId);
            if (station?.storage == null) return;
            for (int k = 0; k < station.storage.Length; k++)
            {
                if (station.storage[k].itemId == itemId)
                {
                    station.storage[k].localOrder -= amount;
                    break;
                }
            }
        }

        /// <summary>
        /// 获取基站某物品当前拉货在途数量
        /// </summary>
        public static int GetBaseFetchInFlight(int planetId, int battleBaseId, int itemId)
        {
            lock (_inFlightLock)
            {
                return _baseFetchInFlight.TryGetValue((planetId, battleBaseId, itemId), out int v) ? v : 0;
            }
        }

        /// <summary>
        /// 增加或扣减基站某物品的拉货在途数量（delta 可正可负）
        /// </summary>
        public static void AddBaseFetchInFlight(int planetId, int battleBaseId, int itemId, int delta)
        {
            if (delta == 0) return;
            lock (_inFlightLock)
            {
                var key = (planetId, battleBaseId, itemId);
                int cur = _baseFetchInFlight.TryGetValue(key, out int v) ? v : 0;
                cur += delta;
                if (cur <= 0)
                    _baseFetchInFlight.Remove(key);
                else
                    _baseFetchInFlight[key] = cur;
            }
        }

        /// <summary>
        /// 清理星球数据
        /// </summary>
        public static void Clear(int planetId)
        {
            lock (_lock)
            {
                _systems.Remove(planetId);
                lock (_inFlightLock)
                {
                    var toRemove = _baseFetchInFlight.Where(kv => kv.Key.Item1 == planetId).Select(kv => kv.Key).ToList();
                    foreach (var k in toRemove)
                        _baseFetchInFlight.Remove(k);
                }
                if (Plugin.DebugLog())
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 清理基站物流系统：行星[{planetId}]");
            }
        }

        /// <summary>
        /// 清理所有数据
        /// </summary>
        public static void ClearAll()
        {
            lock (_lock)
            {
                _systems.Clear();
                lock (_inFlightLock)
                    _baseFetchInFlight.Clear();
                if (Plugin.DebugLog())
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 清理所有基站物流系统");
            }
        }

        /// <summary>
        /// 检测库存变化
        /// </summary>
        public static bool HasInventoryChanged(BaseLogisticSystem logistics, Dictionary<int, int> currentInventory)
        {
            // 如果物品种类数不同，肯定变化了
            if (logistics.lastInventory.Count != currentInventory.Count)
                return true;

            // 检查每个物品的数量
            foreach (var kvp in currentInventory)
            {
                if (!logistics.lastInventory.TryGetValue(kvp.Key, out int lastCount) || lastCount != kvp.Value)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 扫描基站输入区需求（输入分区：index >= size - bans）。按 itemId 汇总缺口，并扣减在途。
        /// </summary>
        public static List<(int itemId, int needCount)> ScanBaseInputDemands(object battleBase, int planetId, int battleBaseId)
        {
            var result = new List<(int, int)>();
            try
            {
                var storageField = battleBase?.GetType().GetField("storage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                object? storage = storageField?.GetValue(battleBase!);
                if (storage == null) return result;

                var sizeField = storage.GetType().GetField("size", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bansField = storage.GetType().GetField("bans", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var gridsField = storage.GetType().GetField("grids", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (sizeField == null || bansField == null || gridsField == null) return result;

                int size = sizeField.GetValue(storage) is int s ? s : 0;
                int bans = bansField.GetValue(storage) is int b ? b : 0;
                int inputStart = size - bans;
                if (inputStart >= size || bans <= 0) return result;

                object? gridsObj = gridsField.GetValue(storage);
                if (gridsObj is not Array grids) return result;

                var gapByItem = new Dictionary<int, int>();
                for (int i = inputStart; i < size; i++)
                {
                    object? grid = grids.GetValue(i);
                    if (grid == null) continue;
                    var filterField = grid.GetType().GetField("filter");
                    var itemIdField = grid.GetType().GetField("itemId");
                    var countField = grid.GetType().GetField("count");
                    var stackSizeField = grid.GetType().GetField("stackSize");
                    if (filterField == null || countField == null || stackSizeField == null) continue;

                    int filter = filterField.GetValue(grid) is int f ? f : 0;
                    int itemId = itemIdField != null && itemIdField.GetValue(grid) is int id ? id : filter;
                    if (itemId <= 0) itemId = filter;
                    if (itemId <= 0) continue;

                    int count = countField.GetValue(grid) is int c ? c : 0;
                    int stackSize = stackSizeField.GetValue(grid) is int ss ? ss : 1000;
                    if (count >= stackSize) continue;

                    int need = stackSize - count;
                    if (!gapByItem.ContainsKey(itemId)) gapByItem[itemId] = 0;
                    gapByItem[itemId] += need;
                }

                foreach (var kv in gapByItem)
                {
                    int itemId = kv.Key;
                    int totalNeed = kv.Value;
                    int inFlight = GetBaseFetchInFlight(planetId, battleBaseId, itemId);
                    int needCount = Math.Max(0, totalNeed - inFlight);
                    if (needCount > 0)
                        result.Add((itemId, needCount));
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ScanBaseInputDemands 异常: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 扫描供应配送器（storageMode=Supply、有筛选、有库存），仅返回 neededItemIds 中的物品。
        /// </summary>
        public static List<DispenserDemand> ScanSupplyDispensers(PlanetFactory factory, Vector3 basePosition, HashSet<int> neededItemIds)
        {
            var list = new List<DispenserDemand>();
            if (neededItemIds == null || neededItemIds.Count == 0) return list;
            try
            {
                var transport = factory?.transport;
                if (transport == null) return list;

                var dispenserPoolField = transport.GetType().GetField("dispenserPool",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dispenserCursorField = transport.GetType().GetField("dispenserCursor",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (dispenserPoolField == null || dispenserCursorField == null) return list;

                Array? dispenserPool = dispenserPoolField.GetValue(transport) as Array;
                object? cursorObj = dispenserCursorField.GetValue(transport);
                if (dispenserPool == null || cursorObj == null) return list;

                int dispenserCursor = Convert.ToInt32(cursorObj);
                var entityPool = factory!.entityPool;
                if (entityPool == null) return list;

                for (int i = 1; i < dispenserCursor && i < dispenserPool.Length; i++)
                {
                    object? dispenserObj = dispenserPool.GetValue(i);
                    if (dispenserObj == null) continue;
                    DispenserComponent? dispenser = dispenserObj as DispenserComponent;
                    if (dispenser == null || dispenser.id != i) continue;

                    if (dispenser.storageMode != (EStorageDeliveryMode)1) continue; // Supply = 1
                    if (dispenser.filter <= 0) continue;
                    if (!neededItemIds.Contains(dispenser.filter)) continue;

                    int stock = GetDispenserStock(dispenser, dispenser.filter);
                    if (stock <= 0) continue;

                    // 已由本 mod 预留的数量 = -Min(0, storageOrdered)；可再派单取走的量 = 物理库存 - 已预留
                    int reservedByUs = Math.Max(0, -dispenser.storageOrdered);
                    int availableForFetch = Math.Max(0, stock - reservedByUs);
                    if (availableForFetch <= 0) continue;

                    Vector3 position = Vector3.zero;
                    if (dispenser.entityId > 0 && dispenser.entityId < entityPool.Length)
                    {
                        var entity = entityPool[dispenser.entityId];
                        if (entity.id > 0) position = entity.pos;
                    }
                    float distance = Vector3.Distance(basePosition, position);

                    list.Add(new DispenserDemand
                    {
                        IsSupplyFetch = true,
                        dispenserId = dispenser.id,
                        entityId = dispenser.entityId,
                        storageId = 0,
                        itemId = dispenser.filter,
                        currentStock = availableForFetch,
                        maxStock = 0,
                        needCount = 0,
                        urgency = 0f,
                        position = position,
                        distance = distance
                    });
                }

                list.Sort((a, b) => a.distance.CompareTo(b.distance));
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ScanSupplyDispensers 异常: {ex.Message}");
            }
            return list;
        }

        /// <summary>
        /// 扫描配送器需求
        /// </summary>
        public static List<DispenserDemand> ScanDispenserDemands(PlanetFactory factory, Vector3 basePosition, Dictionary<int, int> baseInventory)
        {
            var demands = new List<DispenserDemand>();

            try
            {
                if (factory == null) return demands;
                
                var transport = factory.transport;
                if (transport == null) return demands;

                var dispenserPoolField = transport.GetType().GetField("dispenserPool",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dispenserCursorField = transport.GetType().GetField("dispenserCursor",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (dispenserPoolField == null || dispenserCursorField == null) return demands;

                Array? dispenserPool = dispenserPoolField.GetValue(transport) as Array;
                object? dispenserCursorObj = dispenserCursorField.GetValue(transport);

                if (dispenserPool == null || dispenserCursorObj == null) return demands;

                int dispenserCursor = Convert.ToInt32(dispenserCursorObj);

                // 遍历所有配送器
                for (int i = 1; i < dispenserCursor && i < dispenserPool.Length; i++)
                {
                    object? dispenserObj = dispenserPool.GetValue(i);
                    if (dispenserObj == null) continue;

                    DispenserComponent? dispenser = dispenserObj as DispenserComponent;
                    if (dispenser == null || dispenser.id != i) continue;

                    // 只处理需求模式（Demand）的配送器
                    if (dispenser.storageMode != (EStorageDeliveryMode)2) continue;

                    // 只处理有筛选器的配送器
                    if (dispenser.filter <= 0) continue;

                    // 滞留区有物品时不向该配送器送货，等游戏逻辑把滞留区清空后再配送
                    if (dispenser.holdupItemCount > 0)
                        continue;

                    // 检查基站是否有该物品
                    if (!baseInventory.ContainsKey(dispenser.filter) || baseInventory[dispenser.filter] <= 0)
                        continue;

                    // 获取配送器的当前库存与在途数量（含游戏原生 + 本 mod 派遣的在途）
                    int currentStock = GetDispenserStock(dispenser, dispenser.filter);
                    int maxStock = GetDispenserMaxStock(dispenser);
                    int incoming = Math.Max(0, dispenser.storageOrdered);

                    // 只有「当前 + 在途」已满（连接箱堆无空位）才跳过；未满则视为有需求
                    if (maxStock > 0 && currentStock + incoming >= maxStock)
                        continue;

                    // 获取位置
                    Vector3 position = Vector3.zero;
                    var entityPool = factory.entityPool;
                    if (dispenser.entityId > 0 && entityPool != null && dispenser.entityId < entityPool.Length)
                    {
                        var entity = entityPool[dispenser.entityId];
                        if (entity.id > 0) // 确保实体有效
                        {
                            position = entity.pos;
                        }
                    }

                    float distance = Vector3.Distance(basePosition, position);

                    // 计算紧急度（0=最紧急，1=不紧急）；考虑在途，避免已派很多在途的仍排最前
                    float urgency = maxStock > 0 ? (float)(currentStock + incoming) / maxStock : 1f;

                    // 获取存储ID
                    int storageId = 0;
                    if (dispenser.storage?.bottomStorage != null)
                    {
                        var storageIdField = dispenser.storage.bottomStorage.GetType().GetField("id");
                        if (storageIdField != null)
                        {
                            storageId = (int)storageIdField.GetValue(dispenser.storage.bottomStorage)!;
                        }
                    }

                    demands.Add(new DispenserDemand
                    {
                        dispenserId = dispenser.id,
                        entityId = dispenser.entityId,
                        storageId = storageId,
                        itemId = dispenser.filter,
                        currentStock = currentStock,
                        maxStock = maxStock,
                        urgency = urgency,
                        position = position,
                        distance = distance
                    });
                }

                // 按紧急度排序（最紧急的优先）
                demands.Sort((a, b) =>
                {
                    int urgencyCompare = a.urgency.CompareTo(b.urgency);
                    if (urgencyCompare != 0) return urgencyCompare;
                    // 紧急度相同，按距离排序（近的优先）
                    return a.distance.CompareTo(b.distance);
                });
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ScanDispenserDemands 异常: {ex.Message}");
            }

            return demands;
        }

        /// <summary>
        /// 扫描物流塔需求：仅对「设置了对应物品本地需求且当前数量不足需求数量」的槽位生成需求。
        /// </summary>
        public static List<DispenserDemand> ScanStationDemands(PlanetFactory factory, Vector3 basePosition, Dictionary<int, int> baseInventory)
        {
            var demands = new List<DispenserDemand>();

            try
            {
                if (factory == null) return demands;

                var transport = factory.transport;
                if (transport == null) return demands;

                var stationPool = transport.stationPool;
                if (stationPool == null) return demands;

                int stationCursor = transport.stationCursor;
                var entityPool = factory.entityPool;
                if (entityPool == null) return demands;

                for (int i = 1; i < stationCursor && i < stationPool.Length; i++)
                {
                    StationComponent? station = stationPool[i];
                    if (station == null || station.id != i) continue;

                    if (station.storage == null) continue;

                    for (int k = 0; k < station.storage.Length; k++)
                    {
                        StationStore store = station.storage[k];
                        // 只处理：已配置物品、本地需求、数量不足（需求空间 > 0）
                        if (store.itemId <= 0) continue;
                        if (store.localLogic != ELogisticStorage.Demand) continue;

                        int currentPlusOrdered = store.count + store.localOrder;
                        if (currentPlusOrdered >= store.max) continue;

                        int needSpace = store.max - currentPlusOrdered;
                        if (needSpace <= 0) continue;

                        if (!baseInventory.ContainsKey(store.itemId) || baseInventory[store.itemId] <= 0)
                            continue;

                        Vector3 stationPos = Vector3.zero;
                        if (station.entityId > 0 && station.entityId < entityPool.Length)
                        {
                            var entity = entityPool[station.entityId];
                            if (entity.id > 0)
                                stationPos = entity.pos;
                        }

                        float distance = Vector3.Distance(basePosition, stationPos);
                        float urgency = store.max > 0 ? (float)currentPlusOrdered / store.max : 1f;

                        demands.Add(new DispenserDemand
                        {
                            IsStationTower = true,
                            stationId = station.id,
                            StationStorageIndex = k,
                            dispenserId = 0,
                            entityId = station.entityId,
                            storageId = 0,
                            itemId = store.itemId,
                            currentStock = currentPlusOrdered,
                            maxStock = store.max,
                            urgency = urgency,
                            position = stationPos,
                            distance = distance
                        });
                    }

                    // 星际物流塔：数量满足且当前没有任何无人机在往该塔送翘曲器时，才生成翘曲器需求
                    if (station.isStellar && station.warperMaxCount > 0 && station.warperCount < station.warperMaxCount
                        && baseInventory.TryGetValue(ITEMID_WARPER, out int warperStock) && warperStock > 0
                        && !HasWarperCourierInTransitToStation(factory.planetId, station.id))
                    {
                        int needWarper = station.warperMaxCount - station.warperCount;
                        Vector3 stationPosW = Vector3.zero;
                        if (station.entityId > 0 && station.entityId < entityPool.Length)
                        {
                            var entity = entityPool[station.entityId];
                            if (entity.id > 0)
                                stationPosW = entity.pos;
                        }
                        float distanceW = Vector3.Distance(basePosition, stationPosW);
                        float urgencyW = station.warperMaxCount > 0 ? (float)station.warperCount / station.warperMaxCount : 1f;
                        demands.Add(new DispenserDemand
                        {
                            IsStationTower = true,
                            stationId = station.id,
                            IsWarperStorageDemand = true,
                            dispenserId = 0,
                            entityId = station.entityId,
                            storageId = 0,
                            itemId = ITEMID_WARPER,
                            currentStock = station.warperCount,
                            maxStock = station.warperMaxCount,
                            needCount = needWarper,
                            urgency = urgencyW,
                            position = stationPosW,
                            distance = distanceW
                        });
                    }
                }

                demands.Sort((a, b) =>
                {
                    int c = a.urgency.CompareTo(b.urgency);
                    if (c != 0) return c;
                    return a.distance.CompareTo(b.distance);
                });
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ScanStationDemands 异常: {ex.Message}");
            }

            return demands;
        }

        /// <summary>
        /// 扫描机甲（玩家）配送栏需求：遍历 mainPlayer.deliveryPackage，找出需要补货的槽位。
        /// 仅在当前星球、工厂已加载、玩家未航行、存活、deliveryPackage.unlockedAndEnabled 时有效。
        /// </summary>
        public static List<DispenserDemand> ScanMechaDemands(PlanetFactory factory, Vector3 basePosition, Dictionary<int, int> baseInventory)
        {
            var demands = new List<DispenserDemand>();

            try
            {
                if (factory == null) return demands;
                var player = GameMain.mainPlayer;
                if (player == null || !player.isAlive || player.sailing) return demands;

                var deliveryPackage = player.deliveryPackage;
                var packageUtility = player.packageUtility;
                if (deliveryPackage == null || packageUtility == null) return demands;
                if (!deliveryPackage.unlockedAndEnabled) return demands;

                var planet = factory.planet;
                if (planet == null || GameMain.localPlanet != planet || !planet.loaded || !planet.factoryLoaded)
                    return demands;

                var grids = deliveryPackage.grids;
                if (grids == null) return demands;

                Vector3 playerPos = player.position;
                if (playerPos.sqrMagnitude < 0.01f) return demands;

                float baseDist = Vector3.Distance(basePosition, playerPos);

                for (int j = 0; j < deliveryPackage.gridLength; j++)
                {
                    if (!deliveryPackage.IsGridActive(j)) continue;
                    if (grids[j].itemId <= 0) continue;
                    int itemId = grids[j].itemId;
                    if (itemId == 1099) continue; // 游戏内 AddItemToAllPackages 会拒绝 1099

                    if (!baseInventory.ContainsKey(itemId) || baseInventory[itemId] <= 0)
                        continue;

                    int currentTotal = grids[j].count + packageUtility.GetPackageItemCountIncludeHandItem(itemId);
                    int clampedRequire = grids[j].clampedRequireCount;
                    if (currentTotal >= clampedRequire) continue;

                    int slotCapacity = grids[j].stackSizeModified - grids[j].count + packageUtility.GetPackageItemCapacity(itemId);
                    if (slotCapacity <= 0) continue;

                    int needCount = clampedRequire - currentTotal;
                    if (needCount <= 0) continue;

                    float urgency = clampedRequire > 0 ? (float)currentTotal / clampedRequire : 1f;

                    demands.Add(new DispenserDemand
                    {
                        IsMechaSlot = true,
                        slotIndex = j,
                        needCount = needCount,
                        dispenserId = 0,
                        entityId = 0,
                        storageId = 0,
                        itemId = itemId,
                        currentStock = currentTotal,
                        maxStock = clampedRequire,
                        urgency = urgency,
                        position = playerPos,
                        distance = baseDist
                    });
                }

                demands.Sort((a, b) =>
                {
                    int c = a.urgency.CompareTo(b.urgency);
                    if (c != 0) return c;
                    return a.distance.CompareTo(b.distance);
                });
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ScanMechaDemands 异常: {ex.Message}");
            }

            return demands;
        }

        /// <summary>
        /// 判断本帧是否应派遣并返回派遣上下文：内部处理 cooldown、库存变化跳过、无空闲/无需求等逻辑。
        /// 返回 null 表示本帧应跳过（冷却中、无变化且无待发可能、无空闲、无需求）；非 null 表示应执行派遣。
        /// </summary>
        public static DispatchContext? TryGetDispatchContext(BaseLogisticSystem logistics, object battleBase, PlanetFactory factory, int entityId)
        {
            if (logistics == null || battleBase == null || factory == null) return null;

            // 派遣冷却
            logistics.cooldownCounter++;
            if (logistics.cooldownCounter < BaseLogisticSystem.DISPATCH_INTERVAL)
                return null;
            logistics.cooldownCounter = 0;

            var currentInventory = GetBaseInventory(battleBase);

            // 仅在「无空闲」或「库存为空」时，才因“库存未变化”而跳过；否则（有空闲且有货）一律继续扫描
            bool inventoryChanged = HasInventoryChanged(logistics, currentInventory);
            if (!inventoryChanged && (logistics.idleCount <= 0 || currentInventory.Count == 0))
                return null;

            if (logistics.idleCount <= 0)
                return null;

            Vector3 basePosition = Vector3.zero;
            if (entityId >= 0 && factory.entityPool != null && entityId < factory.entityPool.Length)
            {
                var entity = factory.entityPool[entityId];
                Vector3 up = entity.pos.sqrMagnitude < 1E-6f ? Vector3.up : entity.pos.normalized;
                basePosition = entity.pos + up * BaseLogisticSystem.DRONE_AT_BASE_HEIGHT_OFFSET;
            }

            var mechaDemands = ScanMechaDemands(factory, basePosition, currentInventory);
            var stationDemands = ScanStationDemands(factory, basePosition, currentInventory);
            var dispenserDemands = ScanDispenserDemands(factory, basePosition, currentInventory);

            int planetId = factory.planetId;
            int battleBaseId = logistics.battleBaseId;
            var baseInputDemands = ScanBaseInputDemands(battleBase, planetId, battleBaseId);
            var neededItemIds = new HashSet<int>(baseInputDemands.Select(x => x.itemId));
            var supplyList = ScanSupplyDispensers(factory, basePosition, neededItemIds);

            var fetchDemands = new List<DispenserDemand>();
            const int courierCapacity = 100;
            foreach (var (itemId, needCount) in baseInputDemands)
            {
                if (needCount <= 0) continue;
                foreach (var supply in supplyList)
                {
                    if (supply.itemId != itemId) continue;
                    int takeCount = Math.Min(needCount, Math.Min(courierCapacity, supply.currentStock));
                    if (takeCount <= 0) continue;
                    fetchDemands.Add(new DispenserDemand
                    {
                        IsSupplyFetch = true,
                        dispenserId = supply.dispenserId,
                        entityId = supply.entityId,
                        storageId = 0,
                        itemId = itemId,
                        currentStock = supply.currentStock,
                        maxStock = 0,
                        needCount = takeCount,
                        urgency = 1f,
                        position = supply.position,
                        distance = supply.distance
                    });
                    break;
                }
            }

            var demands = new List<DispenserDemand>(mechaDemands.Count + stationDemands.Count + dispenserDemands.Count + fetchDemands.Count);
            demands.AddRange(mechaDemands);
            demands.AddRange(stationDemands);
            demands.AddRange(dispenserDemands);
            demands.AddRange(fetchDemands);
            // 优先顺序：机甲 > 物流塔 > 需求配送器 > 拉货
            demands.Sort((a, b) =>
            {
                int priorityA = a.IsMechaSlot ? 0 : (a.IsStationTower ? 1 : (a.IsSupplyFetch ? 3 : 2));
                int priorityB = b.IsMechaSlot ? 0 : (b.IsStationTower ? 1 : (b.IsSupplyFetch ? 3 : 2));
                int pri = priorityA.CompareTo(priorityB);
                if (pri != 0) return pri;
                if (a.IsStationTower && b.IsStationTower && a.stationId == b.stationId && a.itemId == ITEMID_WARPER && b.itemId == ITEMID_WARPER)
                {
                    if (a.IsWarperStorageDemand != b.IsWarperStorageDemand)
                        return a.IsWarperStorageDemand ? -1 : 1;
                }
                int c = a.urgency.CompareTo(b.urgency);
                if (c != 0) return c;
                return a.distance.CompareTo(b.distance);
            });

            if (demands.Count == 0)
            {
                logistics.lastInventory = new Dictionary<int, int>(currentInventory);
                return null;
            }

            return new DispatchContext(demands, currentInventory, basePosition);
        }

        /// <summary>
        /// 获取配送器当前库存（遍历整条堆叠链）。供 Patch 拉货派遣时使用。
        /// </summary>
        public static int GetDispenserStockPublic(DispenserComponent dispenser, int itemId)
        {
            return GetDispenserStock(dispenser, itemId);
        }

        /// <summary>
        /// 获取配送器当前库存（遍历整条堆叠链：配送器下方 1 到多个箱子的该物品数量总和）
        /// </summary>
        private static int GetDispenserStock(DispenserComponent dispenser, int itemId)
        {
            try
            {
                if (dispenser.storage?.bottomStorage == null) return 0;

                var storageType = dispenser.storage.bottomStorage.GetType();
                var gridsField = storageType.GetField("grids", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nextStorageField = storageType.GetField("nextStorage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (gridsField == null) return 0;

                int totalCount = 0;
                object? current = dispenser.storage.bottomStorage;
                while (current != null)
                {
                    if (gridsField.GetValue(current) is Array grids)
                    {
                        for (int i = 0; i < grids.Length; i++)
                        {
                            object? grid = grids.GetValue(i);
                            if (grid == null) continue;

                            var gridItemIdField = grid.GetType().GetField("itemId");
                            var gridCountField = grid.GetType().GetField("count");

                            int gridItemId = gridItemIdField != null ? (int)gridItemIdField.GetValue(grid)! : 0;
                            int gridCount = gridCountField != null ? (int)gridCountField.GetValue(grid)! : 0;

                            if (gridItemId == itemId)
                                totalCount += gridCount;
                        }
                    }
                    current = nextStorageField?.GetValue(current) as object;
                }

                return totalCount;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取配送器「可接收自动货物的最大容量」：遍历整条堆叠链（1 到多个箱子），
        /// 汇总每箱允许自动放入的格子数（size - bans）× 每格堆叠数。与游戏 InsertIntoStorage(useBan=true) 一致。
        /// </summary>
        private static int GetDispenserMaxStock(DispenserComponent dispenser)
        {
            try
            {
                if (dispenser.storage?.bottomStorage == null) return 0;

                var storageType = dispenser.storage.bottomStorage.GetType();
                var sizeField = storageType.GetField("size", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var bansField = storageType.GetField("bans", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var nextStorageField = storageType.GetField("nextStorage", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (sizeField == null || bansField == null) return 0;

                int totalReceivableSlots = 0;
                object? current = dispenser.storage.bottomStorage;
                while (current != null)
                {
                    int size = sizeField.GetValue(current) is int s ? s : 0;
                    int bans = bansField.GetValue(current) is int b ? b : 0;
                    totalReceivableSlots += Math.Max(0, size - bans);
                    current = nextStorageField?.GetValue(current) as object;
                }

                if (totalReceivableSlots == 0) return 0;

                int stackSize = 1000;
                var itemProto = LDB.items.Select(dispenser.filter);
                if (itemProto != null) stackSize = itemProto.StackSize;

                return totalReceivableSlots * stackSize;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取基站当前库存
        /// </summary>
        public static Dictionary<int, int> GetBaseInventory(object battleBase)
        {
            var inventory = new Dictionary<int, int>();

            try
            {
                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                if (storage == null) return inventory;

                var gridsField = storage.GetType().GetField("grids");
                object? gridsObj = gridsField?.GetValue(storage);
                if (gridsObj is not Array grids) return inventory;

                for (int i = 0; i < grids.Length; i++)
                {
                    object? grid = grids.GetValue(i);
                    if (grid == null) continue;

                    var itemIdField = grid.GetType().GetField("itemId");
                    var countField = grid.GetType().GetField("count");

                    int itemId = itemIdField != null ? (int)itemIdField.GetValue(grid)! : 0;
                    int count = countField != null ? (int)countField.GetValue(grid)! : 0;

                    if (itemId > 0 && count > 0)
                    {
                        if (!inventory.ContainsKey(itemId))
                            inventory[itemId] = 0;
                        inventory[itemId] += count;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] GetBaseInventory 异常: {ex.Message}");
            }

            return inventory;
        }
    }
}
