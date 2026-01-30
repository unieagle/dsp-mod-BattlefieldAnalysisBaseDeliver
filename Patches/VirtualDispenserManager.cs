using System;
using System.Collections.Generic;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 虚拟配送器管理器
    /// 为每个战场分析基站创建虚拟配送器，使其能够无缝集成到游戏的物流系统中
    /// </summary>
    public static class VirtualDispenserManager
    {
        /// <summary>
        /// 虚拟配送器ID → 战场分析基站ID 的映射
        /// </summary>
        private static Dictionary<int, int> virtualDispenserToBattleBase = new Dictionary<int, int>();

        /// <summary>
        /// 战场分析基站ID → 虚拟配送器ID 的映射
        /// </summary>
        private static Dictionary<int, int> battleBaseToVirtualDispenser = new Dictionary<int, int>();

        /// <summary>
        /// 检查配送器是否是虚拟配送器（对应战场分析基站）
        /// </summary>
        public static bool IsVirtualDispenser(int dispenserId)
        {
            return virtualDispenserToBattleBase.ContainsKey(dispenserId);
        }

        /// <summary>
        /// 获取虚拟配送器对应的战场分析基站ID
        /// </summary>
        public static bool TryGetBattleBaseId(int virtualDispenserId, out int battleBaseId)
        {
            return virtualDispenserToBattleBase.TryGetValue(virtualDispenserId, out battleBaseId);
        }

        /// <summary>
        /// 获取战场分析基站对应的虚拟配送器ID
        /// </summary>
        public static bool TryGetVirtualDispenserId(int battleBaseId, out int virtualDispenserId)
        {
            return battleBaseToVirtualDispenser.TryGetValue(battleBaseId, out virtualDispenserId);
        }

        /// <summary>
        /// 为星球的所有战场分析基站创建虚拟配送器
        /// </summary>
        public static void CreateVirtualDispensers(PlanetFactory factory)
        {
            if (factory?.transport == null)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] CreateVirtualDispensers: factory 或 transport 为 null");
                return;
            }

            try
            {
                // 获取 dispenserPool 和 dispenserCursor
                var dispenserPoolField = factory.transport.GetType().GetField("dispenserPool", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var dispenserCursorField = factory.transport.GetType().GetField("dispenserCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (dispenserPoolField == null || dispenserCursorField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] CreateVirtualDispensers: dispenserPool/Cursor 字段未找到");
                    return;
                }

                Array? dispenserPool = dispenserPoolField.GetValue(factory.transport) as Array;
                object? dispenserCursorObj = dispenserCursorField.GetValue(factory.transport);

                if (dispenserPool == null || dispenserCursorObj == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] CreateVirtualDispensers: dispenserPool 或 dispenserCursor 为 null");
                    return;
                }

                int dispenserCursor = Convert.ToInt32(dispenserCursorObj);

                // 获取战场分析基站
                var defenseSystemField = factory.GetType().GetField("defenseSystem", BindingFlags.Public | BindingFlags.Instance);
                if (defenseSystemField == null) return;

                object? defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null) return;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null) return;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return;

                var bufferField = battleBasesPool.GetType().GetField("buffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null) return;

                Array? battleBases = bufferField.GetValue(battleBasesPool) as Array;
                if (battleBases == null) return;

                int createdCount = 0;
                int recoveredCount = 0;

                // 【第一步】从存档中恢复旧的虚拟配送器映射
                // 遍历dispenserPool，找到所有虚拟配送器（通过entityId匹配战场基站）
                for (int dispenserId = 1; dispenserId < dispenserCursor && dispenserId < dispenserPool.Length; dispenserId++)
                {
                    object? dispenser = dispenserPool.GetValue(dispenserId);
                    if (dispenser == null) continue;

                    var idField = dispenser.GetType().GetField("id");
                    var entityIdField = dispenser.GetType().GetField("entityId");
                    
                    if (idField == null || entityIdField == null) continue;
                    
                    int id = (int)idField.GetValue(dispenser)!;
                    int entityId = (int)entityIdField.GetValue(dispenser)!;
                    
                    if (id != dispenserId || entityId <= 0) continue;

                    // 检查这个entityId是否对应一个战场基站
                    for (int battleBaseId = 1; battleBaseId < battleBases.Length; battleBaseId++)
                    {
                        object? battleBase = battleBases.GetValue(battleBaseId);
                        if (battleBase == null) continue;

                        var bbEntityIdField = battleBase.GetType().GetField("entityId");
                        if (bbEntityIdField == null) continue;
                        
                        int bbEntityId = (int)bbEntityIdField.GetValue(battleBase)!;
                        
                        if (bbEntityId == entityId)
                        {
                            // 找到了！这是一个虚拟配送器，恢复映射
                            virtualDispenserToBattleBase[dispenserId] = battleBaseId;
                            battleBaseToVirtualDispenser[battleBaseId] = dispenserId;
                            recoveredCount++;
                            
                            if (BattlefieldBaseHelper.DebugLog())
                            {
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ♻️ 从存档恢复虚拟配送器映射：dispenser[{dispenserId}] → battleBase[{battleBaseId}] (entityId={entityId})");
                            }
                            break;
                        }
                    }
                }

                // 【第二步】为没有虚拟配送器的战场基站创建新的
                for (int battleBaseId = 1; battleBaseId < battleBases.Length; battleBaseId++)
                {
                    object? battleBase = battleBases.GetValue(battleBaseId);
                    if (battleBase == null) continue;

                    // 检查 entityId 来判断战场基站是否存在（更可靠的检查）
                    var entityIdField = battleBase.GetType().GetField("entityId");
                    if (entityIdField == null) continue;
                    int entityId = (int)entityIdField.GetValue(battleBase)!;
                    if (entityId <= 0) continue;  // entityId <= 0 说明这个战场基站不存在

                    // 检查是否已经创建过虚拟配送器
                    if (battleBaseToVirtualDispenser.ContainsKey(battleBaseId))
                        continue;

                    // 检查 dispenserPool 是否还有空间
                    if (dispenserCursor >= dispenserPool.Length)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] CreateVirtualDispensers: dispenserPool 已满，无法创建更多虚拟配送器");
                        break;
                    }
                    
                    // 创建虚拟配送器
                    var virtualDispenser = new DispenserComponent();
                    int virtualDispenserId = dispenserCursor++;

                    // 初始化虚拟配送器的必要字段

                    // 【关键】完整初始化所有字段，模拟游戏的 Init 方法
                    virtualDispenser.id = virtualDispenserId;
                    virtualDispenser.entityId = entityId;  // 使用战场基站的 entityId（UI 会通过 UI 过滤补丁隐藏）
                    virtualDispenser.pcId = 0;  // 虚拟配送器没有产线连接
                    virtualDispenser.storageId = 0;  // 虚拟配送器没有存储
                    virtualDispenser.gene = virtualDispenserId % 3;  // 基因（用于动画偏移）
                    
                    // 能量相关
                    virtualDispenser.energy = 0L;
                    virtualDispenser.energyPerTick = 0L;
                    virtualDispenser.energyMax = 0L;
                    
                    // 模式和过滤器
                    virtualDispenser.filter = 0;         // 不过滤（所有物品都可供应）
                    virtualDispenser.playerMode = (EPlayerDeliveryMode)1;  // 供应模式（配送器-机甲）
                    virtualDispenser.storageMode = (EStorageDeliveryMode)1;  // 供应模式（配送器-配送器）✅
                    
                    // 无人机相关
                    virtualDispenser.workCourierCount = 0;
                    virtualDispenser.idleCourierCount = 0;
                    virtualDispenser.courierAutoReplenish = false;
                    virtualDispenser.workCourierDatas = new CourierData[0];  // 空数组（不是 null！）
                    virtualDispenser.orders = new DeliveryLogisticOrder[0]; // 空数组
                    
                    // 暂存包裹（重要！游戏会访问）
                    virtualDispenser.holdupItemCount = 0;
                    virtualDispenser.holdupPackage = new DispenserStore[0];  // 空数组
                    
                    // 配送包裹（关键！UI 会访问这个字段）
                    // 需要创建一个空的 DeliveryPackage
                    try
                    {
                        var deliveryPackageType = typeof(DispenserComponent).Assembly.GetType("DeliveryPackage");
                        if (deliveryPackageType != null)
                        {
                            var deliveryPackageConstructor = deliveryPackageType.GetConstructor(new Type[] { typeof(int) });
                            if (deliveryPackageConstructor != null)
                            {
                                // 创建容量为 0 的空 DeliveryPackage
                                object? emptyDeliveryPackage = deliveryPackageConstructor.Invoke(new object[] { 0 });
                                virtualDispenser.deliveryPackage = (DeliveryPackage)emptyDeliveryPackage!;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 初始化 deliveryPackage 失败: {ex.Message}");
                        // 如果失败，设置为 null（可能会导致 UI 错误，但不会崩溃）
                        virtualDispenser.deliveryPackage = null!;
                    }
                    
                    // 订单和配对
                    virtualDispenser.playerOrdered = 0;
                    virtualDispenser.storageOrdered = 0;
                    virtualDispenser.pairProcess = 0;
                    virtualDispenser.pulseSignal = -1;
                    virtualDispenser.pairs = new SupplyDemandPair[0];       // 空数组
                    virtualDispenser.playerPairCount = 0;
                    virtualDispenser.pairCount = 0;
                    
                    // 存储和搜索起点
                    virtualDispenser.storage = null;
                    virtualDispenser.insertStorageSearchStart = null;
                    virtualDispenser.insertGridSearchStart = 0;
                    virtualDispenser.pickStorageSearchStart = null;
                    virtualDispenser.pickGridSearchStart = 0;
                    
                    // 配送条件
                    virtualDispenser.playerDeliveryCondition = (DispenserComponent.EPlayerDeliveryCondition)0;  // None

                    // 加入 dispenserPool
                    dispenserPool.SetValue(virtualDispenser, virtualDispenserId);

                    // 建立映射
                    virtualDispenserToBattleBase[virtualDispenserId] = battleBaseId;
                    battleBaseToVirtualDispenser[battleBaseId] = virtualDispenserId;

                    createdCount++;

                    if (BattlefieldBaseHelper.DebugLog())
                    {
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 为战场分析基站 [{battleBaseId}] 创建虚拟配送器 [{virtualDispenserId}]");
                    }
                }

                // 更新 dispenserCursor
                dispenserCursorField.SetValue(factory.transport, dispenserCursor);

                if (recoveredCount > 0 || createdCount > 0)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 虚拟配送器：从存档恢复 {recoveredCount} 个，新创建 {createdCount} 个");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] CreateVirtualDispensers 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 检查战场分析基站是否存在（entityId > 0）
        /// </summary>
        public static bool CheckBattleBaseExists(PlanetFactory factory, int battleBaseId)
        {
            try
            {
                var defenseSystem = factory?.defenseSystem;
                if (defenseSystem == null) return false;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null) return false;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return false;

                var bufferField = battleBasesPool.GetType().GetField("buffer",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null) return false;

                Array? battleBases = bufferField.GetValue(battleBasesPool) as Array;
                if (battleBases == null || battleBaseId <= 0 || battleBaseId >= battleBases.Length)
                    return false;

                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null) return false;

                var entityIdField = battleBase.GetType().GetField("entityId");
                if (entityIdField == null) return false;

                int entityId = (int)entityIdField.GetValue(battleBase)!;
                return entityId > 0;  // entityId > 0 说明基站存在
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] CheckBattleBaseExists 异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查虚拟配送器是否有效（对应的基站是否存在）
        /// </summary>
        public static bool IsVirtualDispenserValid(PlanetFactory factory, int virtualDispenserId)
        {
            // 1. 检查是否是虚拟配送器
            if (!TryGetBattleBaseId(virtualDispenserId, out int battleBaseId))
                return false;

            // 2. 检查基站是否存在
            return CheckBattleBaseExists(factory, battleBaseId);
        }

        /// <summary>
        /// 清理映射（星球切换时）
        /// </summary>
        public static void Clear()
        {
            virtualDispenserToBattleBase.Clear();
            battleBaseToVirtualDispenser.Clear();
        }
    }
}
