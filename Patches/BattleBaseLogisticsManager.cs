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
        
        // 无人机数据（与游戏的 CourierData 完全兼容）
        public CourierData[] couriers = new CourierData[10];
        public int idleCount = 10;
        public int workingCount = 0;
        
        // 库存追踪（用于检测变化）
        public Dictionary<int, int> lastInventory = new Dictionary<int, int>();
        
        // 派遣冷却（避免频繁派遣，每60帧=1秒检查一次）
        public int cooldownCounter = 0;
        public const int DISPATCH_INTERVAL = 60;
    }

    /// <summary>
    /// 配送器需求信息
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
        public float distance;          // 距离基站的距离
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
                    _systems[planetId][battleBaseId] = new BaseLogisticSystem
                    {
                        battleBaseId = battleBaseId,
                        planetId = planetId
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
        /// 获取配送器最大容量
        /// </summary>
        private static int GetDispenserMaxStock(DispenserComponent dispenser)
        {
            try
            {
                if (dispenser.storage?.bottomStorage == null) return 0;

                var storage = dispenser.storage.bottomStorage;
                var gridsField = storage.GetType().GetField("grids");
                if (gridsField?.GetValue(storage) is not Array grids) return 0;

                // 假设每格1000（实际应该从物品配置获取，但简化处理）
                return grids.Length * 1000;
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
