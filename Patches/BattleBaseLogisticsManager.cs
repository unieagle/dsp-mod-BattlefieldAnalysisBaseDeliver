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

        /// <summary>
        /// 清理星球数据
        /// </summary>
        public static void Clear(int planetId)
        {
            lock (_lock)
            {
                _systems.Remove(planetId);
                
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

                    // 获取配送器的当前库存
                    int currentStock = GetDispenserStock(dispenser, dispenser.filter);
                    int maxStock = GetDispenserMaxStock(dispenser);

                    // 如果库存充足（>80%），跳过
                    if (maxStock > 0 && (float)currentStock / maxStock > 0.8f)
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

                    // 计算紧急度（0=最紧急，1=不紧急）
                    float urgency = maxStock > 0 ? (float)currentStock / maxStock : 1f;

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
                basePosition = factory.entityPool[entityId].pos;

            var mechaDemands = ScanMechaDemands(factory, basePosition, currentInventory);
            var stationDemands = ScanStationDemands(factory, basePosition, currentInventory);
            var dispenserDemands = ScanDispenserDemands(factory, basePosition, currentInventory);
            var demands = new List<DispenserDemand>(mechaDemands.Count + stationDemands.Count + dispenserDemands.Count);
            demands.AddRange(mechaDemands);
            demands.AddRange(stationDemands);
            demands.AddRange(dispenserDemands);
            // 优先顺序：机甲 > 物流塔 > 配送器，同类型内按紧急度、距离
            demands.Sort((a, b) =>
            {
                int priorityA = a.IsMechaSlot ? 0 : (a.IsStationTower ? 1 : 2);
                int priorityB = b.IsMechaSlot ? 0 : (b.IsStationTower ? 1 : 2);
                int pri = priorityA.CompareTo(priorityB);
                if (pri != 0) return pri;
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
        /// 获取配送器当前库存
        /// </summary>
        private static int GetDispenserStock(DispenserComponent dispenser, int itemId)
        {
            try
            {
                if (dispenser.storage?.bottomStorage == null) return 0;

                var storage = dispenser.storage.bottomStorage;
                var gridsField = storage.GetType().GetField("grids");
                if (gridsField?.GetValue(storage) is not Array grids) return 0;

                int totalCount = 0;
                for (int i = 0; i < grids.Length; i++)
                {
                    object? grid = grids.GetValue(i);
                    if (grid == null) continue;

                    var gridItemIdField = grid.GetType().GetField("itemId");
                    var gridCountField = grid.GetType().GetField("count");

                    int gridItemId = gridItemIdField != null ? (int)gridItemIdField.GetValue(grid)! : 0;
                    int gridCount = gridCountField != null ? (int)gridCountField.GetValue(grid)! : 0;

                    if (gridItemId == itemId)
                    {
                        totalCount += gridCount;
                    }
                }

                return totalCount;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取配送器「可接收自动货物的最大容量」：仅统计允许自动放入的格子数（size - bans）× 每格堆叠数。
        /// 与游戏 InsertIntoStorage(useBan=true) 一致，避免箱子只开 1 格接收时被误判为 1/30 库存而高优先级不断送货导致滞留。
        /// </summary>
        private static int GetDispenserMaxStock(DispenserComponent dispenser)
        {
            try
            {
                if (dispenser.storage?.bottomStorage == null) return 0;

                var storage = dispenser.storage.bottomStorage;
                if (storage is not StorageComponent sc) return 0;

                int size = sc.size;
                int bans = sc.bans;
                int receivableSlots = Math.Max(0, size - bans);
                if (receivableSlots == 0) return 0;

                int stackSize = 1000;
                var itemProto = LDB.items.Select(dispenser.filter);
                if (itemProto != null) stackSize = itemProto.StackSize;

                return receivableSlots * stackSize;
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
