using System;
using System.Collections.Generic;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 供 HasLocalSupply 与 RematchLocalPairs 共用的战场分析基站识别与工厂获取。
    /// </summary>
    public static class BattlefieldBaseHelper
    {
        /// <summary>
        /// 战场分析基站的 protoId（固定值，不会变化）
        /// </summary>
        public const int BattlefieldAnalysisBaseProtoId = 3009;
        
        static readonly HashSet<int> LoggedNullFactoryPlanets = new HashSet<int>();
        
        /// <summary>
        /// 调试日志开关：由配置文件控制
        /// </summary>
        public static bool DebugLog() => Plugin.EnableDebugLog?.Value ?? false;

        public static bool IsBattlefieldAnalysisBase(StationComponent station, out int entityProtoId)
        {
            entityProtoId = 0;
            try
            {
                object? factory = GetFactoryForPlanet(station.planetId);
                if (factory == null)
                {
                    lock (LoggedNullFactoryPlanets)
                    {
                        if (LoggedNullFactoryPlanets.Add(station.planetId))
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 无法获取星球工厂 planetId={station.planetId}，无法识别战场分析基站。");
                    }
                    return false;
                }

                return IsBattlefieldAnalysisBase(factory, station, out entityProtoId);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] IsBattlefieldAnalysisBase: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 识别战场分析基站（接受 factory 参数，避免重复获取）
        /// </summary>
        public static bool IsBattlefieldAnalysisBase(object factory, StationComponent station, out int entityProtoId)
        {
            entityProtoId = 0;
            try
            {
                object? entityPool = factory.GetType().GetProperty("entityPool")?.GetValue(factory);
                if (entityPool is not Array pool || station.entityId < 0 || station.entityId >= pool.Length) return false;

                object? entity = pool.GetValue(station.entityId);
                if (entity == null) return false;

                PropertyInfo? protoProp = entity.GetType().GetProperty("protoId");
                if (protoProp == null) return false;

                object? proto = protoProp.GetValue(entity);
                if (proto == null) return false;

                entityProtoId = Convert.ToInt32(proto);
                bool isBattleBase = (entityProtoId == BattlefieldAnalysisBaseProtoId);
                
                if (DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 IsBattlefieldAnalysisBase: stationId={station.id}, entityId={station.entityId}, protoId={entityProtoId}, 目标protoId={BattlefieldAnalysisBaseProtoId}, 结果={isBattleBase}");
                }
                
                return isBattleBase;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] IsBattlefieldAnalysisBase(factory, station): {ex.Message}");
                return false;
            }
        }

        public static object? GetFactoryForPlanet(int planetId)
        {
            try
            {
                Type? gameMain = typeof(StationComponent).Assembly.GetType("GameMain");
                if (gameMain == null)
                {
                    if (DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: GameMain 类型未找到");
                    return null;
                }

                object? data = gameMain.GetProperty("data", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (data == null)
                {
                    if (DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: GameMain.data 为 null");
                    return null;
                }

                // 尝试 1: localPlanet.factory（如果是当前星球）
                object? localPlanet = data.GetType().GetProperty("localPlanet")?.GetValue(data);
                if (localPlanet != null)
                {
                    PropertyInfo? localPlanetIdProp = localPlanet.GetType().GetProperty("id");
                    int localPlanetId = localPlanetIdProp != null ? Convert.ToInt32(localPlanetIdProp.GetValue(localPlanet)) : -1;
                    
                    if (DebugLog())
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: localPlanet.id={localPlanetId}, 目标 planetId={planetId}");
                    
                    if (localPlanetId == planetId)
                    {
                        object? factory = localPlanet.GetType().GetProperty("factory")?.GetValue(localPlanet);
                        if (factory != null)
                        {
                            if (DebugLog())
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: ✓ 通过 localPlanet.factory 成功获取");
                            return factory;
                        }
                    }
                }

                // 尝试 2: data.factory (单工厂模式)
                object? singleFactory = data.GetType().GetProperty("factory")?.GetValue(data);
                if (singleFactory != null)
                {
                    if (DebugLog())
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: ✓ 通过 data.factory 获取");
                    return singleFactory;
                }

                // 尝试 3: data.factories[planetId]
                PropertyInfo? factoriesProp = data.GetType().GetProperty("factories");
                if (factoriesProp != null)
                {
                    object? factories = factoriesProp.GetValue(data);
                    if (factories == null)
                    {
                        if (DebugLog())
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: data.factories 为 null");
                        return null;
                    }
                    
                    if (DebugLog())
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: factories 类型={factories.GetType().Name}");
                    
                    if (factories is Array arr)
                    {
                        if (DebugLog())
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: factories 是数组，长度={arr.Length}, planetId={planetId}");
                        
                        if (planetId >= 0 && planetId < arr.Length)
                        {
                            object? result = arr.GetValue(planetId);
                            if (DebugLog())
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: factories[{planetId}] = {(result != null ? "成功" : "null")}");
                            return result;
                        }
                        else
                        {
                            if (DebugLog())
                                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: planetId={planetId} 超出数组范围 [0, {arr.Length})");
                        }
                    }
                    else
                    {
                        // 尝试通过索引器访问
                        MethodInfo? getAt = factories.GetType().GetMethod("get_Item", new[] { typeof(int) })
                            ?? factories.GetType().GetMethod("Get", new[] { typeof(int) });
                        if (getAt != null)
                        {
                            if (DebugLog())
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: 尝试通过 get_Item/Get 方法访问");
                            return getAt.Invoke(factories, new object[] { planetId });
                        }
                    }
                }

                if (DebugLog())
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet: 所有方法都失败，无法获取 planetId={planetId} 的 factory");
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] GetFactoryForPlanet({planetId}) 异常: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// 将战场分析基站的战利品（BattleBaseComponent.storage = StorageComponent）同步到其 StationComponent.storage，便于本地配送匹配。
        /// 参考 BattleBaseComponent.cs：战利品在 battleBase.storage（factory.factoryStorage.storagePool[storageId]），AutoPickTrash 也写入此 storage；storage.grids 为物品格（itemId/count）。
        /// </summary>
        public static void SyncBattleBaseStorageToStation(object? factory, int entityId, StationComponent station)
        {
            bool debugLog = DebugLog();
            if (debugLog)
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] === SyncBattleBaseStorageToStation 开始: entityId={entityId}, station.id={station?.id ?? -1} ===");
            
            if (factory == null)
            {
                if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: factory 为 null");
                return;
            }
            if (station?.storage == null || station.storage.Length == 0)
            {
                if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: station.storage 为 null 或空");
                return;
            }
            
            try
            {
                object? entityPool = factory.GetType().GetProperty("entityPool")?.GetValue(factory);
                if (entityPool is not Array pool || entityId < 0 || entityId >= pool.Length)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: entityPool 无效或 entityId={entityId} 越界");
                    return;
                }
                object? entity = pool.GetValue(entityId);
                if (entity == null)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: entity[{entityId}] 为 null");
                    return;
                }

                var battleBaseIdProp = entity.GetType().GetProperty("battleBaseId");
                if (battleBaseIdProp == null)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: entity.battleBaseId 属性不存在");
                    return;
                }
                object? bbIdObj = battleBaseIdProp.GetValue(entity);
                if (bbIdObj == null)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: entity.battleBaseId 为 null");
                    return;
                }
                int battleBaseId = Convert.ToInt32(bbIdObj);
                if (battleBaseId <= 0)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: battleBaseId={battleBaseId} 无效");
                    return;
                }
                if (debugLog) Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: battleBaseId={battleBaseId}");

                object? defenseSystem = factory.GetType().GetProperty("defenseSystem")?.GetValue(factory);
                if (defenseSystem == null)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: factory.defenseSystem 为 null");
                    return;
                }

                // 池子名为 battleBases（ObjectPool），用 .buffer 取数组
                object? battleBasesObj = defenseSystem.GetType().GetProperty("battleBases")?.GetValue(defenseSystem)
                    ?? defenseSystem.GetType().GetField("battleBases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(defenseSystem);
                if (battleBasesObj == null)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: defenseSystem.battleBases 为 null");
                    return;
                }
                object? bbBuffer = battleBasesObj.GetType().GetProperty("buffer")?.GetValue(battleBasesObj)
                    ?? battleBasesObj.GetType().GetField("buffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(battleBasesObj);
                if (bbBuffer is not Array bbArr || battleBaseId >= bbArr.Length)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: battleBases.buffer 无效或 battleBaseId={battleBaseId} 越界 (length={((bbBuffer as Array)?.Length ?? 0)})");
                    return;
                }

                object? battleBase = bbArr.GetValue(battleBaseId);
                if (battleBase == null)
                {
                    if (debugLog) Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: battleBase[{battleBaseId}] 为 null");
                    return;
                }
                if (debugLog) Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: 成功获取 battleBase[{battleBaseId}]");

                // 战利品在 BattleBaseComponent.storage（StorageComponent），见 BattleBaseComponent.Init 与 AutoPickTrash
                object? storageComponent = battleBase.GetType().GetProperty("storage")?.GetValue(battleBase)
                    ?? battleBase.GetType().GetField("storage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(battleBase);
                if (storageComponent == null)
                {
                    if (debugLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: battleBase.storage 为 null");
                    return;
                }
                if (debugLog) Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: 成功获取 battleBase.storage");

                object? gridsObj = storageComponent.GetType().GetProperty("grids")?.GetValue(storageComponent)
                    ?? storageComponent.GetType().GetProperty("Grids")?.GetValue(storageComponent)
                    ?? storageComponent.GetType().GetField("grids", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(storageComponent);
                if (gridsObj is not Array gridsArr)
                {
                    if (debugLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] Sync: storage.grids 未取到或非数组");
                    return;
                }
                if (debugLog) Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: 成功获取 grids，长度={gridsArr.Length}");

                // 先清空 station.storage 的本地供应标记
                for (int i = 0; i < station.storage.Length; i++)
                {
                    station.storage[i].itemId = 0;
                    station.storage[i].count = 0;
                    station.storage[i].localLogic = ELogisticStorage.None;
                }

                int slotIdx = 0;
                int nonEmptyGrids = 0;
                for (int i = 0; i < gridsArr.Length && slotIdx < station.storage.Length; i++)
                {
                    object? grid = gridsArr.GetValue(i);
                    if (grid == null) continue;
                    // C# 结构体可能是 itemId/count 或 ItemId/Count
                    var itemIdProp = grid.GetType().GetProperty("itemId") ?? grid.GetType().GetProperty("ItemId");
                    var countProp = grid.GetType().GetProperty("count") ?? grid.GetType().GetProperty("Count");
                    var itemIdField = grid.GetType().GetField("itemId", BindingFlags.Public | BindingFlags.Instance);
                    var countField = grid.GetType().GetField("count", BindingFlags.Public | BindingFlags.Instance);
                    int itemId = itemIdProp != null ? Convert.ToInt32(itemIdProp.GetValue(grid) ?? 0) : (itemIdField != null ? Convert.ToInt32(itemIdField.GetValue(grid) ?? 0) : 0);
                    int count = countProp != null ? Convert.ToInt32(countProp.GetValue(grid) ?? 0) : (countField != null ? Convert.ToInt32(countField.GetValue(grid) ?? 0) : 0);
                    
                    if (itemId > 0 || count > 0)
                    {
                        nonEmptyGrids++;
                        if (debugLog && nonEmptyGrids <= 5)
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: grids[{i}] itemId={itemId}, count={count}");
                    }
                    
                    if (itemId <= 0 || count <= 0) continue;
                    station.storage[slotIdx].itemId = itemId;
                    station.storage[slotIdx].count = count;
                    station.storage[slotIdx].inc = 0;
                    station.storage[slotIdx].localOrder = 0;
                    station.storage[slotIdx].remoteOrder = 0;
                    station.storage[slotIdx].max = count * 2; // 设为 count 的 2 倍，避免游戏认为已满
                    station.storage[slotIdx].localLogic = ELogisticStorage.Supply;
                    station.storage[slotIdx].remoteLogic = ELogisticStorage.None;
                    // localSupplyCount/totalSupplyCount 是计算属性 (count+localOrder+remoteOrder)，会自动有值
                    if (debugLog && slotIdx < 5)
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: 已复制到 station.storage[{slotIdx}]: itemId={itemId}, count={count}");
                    slotIdx++;
                }
                if (debugLog)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: 完成。grids 中非空格={nonEmptyGrids}，同步到 station={slotIdx} 格");
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] Sync: station.storage 最终={FormatStorageSummary(station)}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] SyncBattleBaseStorageToStation: {ex.Message}");
            }
        }

        /// <summary>
        /// 用于验证：将 Station 的 storage 内容格式化为简短字符串（DebugLog 时输出）。
        /// </summary>
        public static string FormatStorageSummary(StationComponent? station)
        {
            if (station?.storage == null || station.storage.Length == 0) return "[]";
            var parts = new System.Collections.Generic.List<string>();
            for (int i = 0; i < station.storage.Length; i++)
            {
                var s = station.storage[i];
                if (s.itemId != 0 || s.count != 0)
                    parts.Add($"{s.itemId}:{s.count}");
            }
            return "[" + string.Join(",", parts) + "]";
        }
        
        /// <summary>
        /// 📊 诊断：输出战场分析基站的 BattleBaseComponent.storage 和 StationComponent.storage 内容
        /// </summary>
        public static void DiagnoseBattleBaseStorage(object factory, int entityId, StationComponent station)
        {
            if (!DebugLog()) return;  // 只在调试模式下执行
            
            try
            {
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 诊断 entityId={entityId} 的存储内容：");
                
                // 1. 获取 BattleBaseComponent（使用字段而不是属性）
                var defenseSystemField = factory.GetType().GetField("defenseSystem", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (defenseSystemField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 defenseSystem 字段未找到");
                    return;
                }
                
                var defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 defenseSystem 为 null（可能还未初始化，稍后会更新）");
                    // 不 return，继续检查其他信息
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 跳过 BattleBaseComponent.storage 检查，仅输出 StationComponent.storage");
                }
                
                // 2. 如果 defenseSystem 存在，检查 BattleBaseComponent.storage
                if (defenseSystem != null)
                {
                    var battleBasesField = defenseSystem.GetType().GetField("battleBases", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (battleBasesField == null)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBases 字段未找到");
                    }
                    else
                    {
                        object? battleBasesObj = battleBasesField.GetValue(defenseSystem);
                        if (battleBasesObj is not Array battleBases)
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBases 不是数组");
                        }
                        else
                        {
                            DiagnoseBattleBaseComponentStorage(factory, entityId, battleBases);
                        }
                    }
                }
                
                // 3. 输出 StationComponent.storage 内容
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 StationComponent.storage.Length={station.storage?.Length ?? 0}");
                if (station.storage != null)
                {
                    int stationItemCount = 0;
                    for (int i = 0; i < station.storage.Length && i < 10; i++)
                    {
                        if (station.storage[i].itemId > 0)
                        {
                            string itemName = GetItemName(station.storage[i].itemId);
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   Station.storage[{i}]: itemId={station.storage[i].itemId} ({itemName}), count={station.storage[i].count}, localLogic={station.storage[i].localLogic}");
                            stationItemCount++;
                        }
                    }
                    
                    if (stationItemCount == 0)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 StationComponent.storage 中没有物品（storage 数组已初始化但为空）");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 📊 DiagnoseBattleBaseStorage 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 📊 内部方法：诊断 BattleBaseComponent.storage
        /// </summary>
        private static void DiagnoseBattleBaseComponentStorage(object factory, int entityId, Array battleBases)
        {
            try
            {
                var entityPoolProp = factory.GetType().GetProperty("entityPool");
                object? entityPoolObj = entityPoolProp?.GetValue(factory);
                if (entityPoolObj is not Array entityPool || entityId < 0 || entityId >= entityPool.Length)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 entityPool 无效");
                    return;
                }
                
                object? entity = entityPool.GetValue(entityId);
                if (entity == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 entity 为 null");
                    return;
                }
                
                var battleBaseIdField = entity.GetType().GetField("battleBaseId");
                int battleBaseId = battleBaseIdField != null ? (int)battleBaseIdField.GetValue(entity)! : 0;
                
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 entity.battleBaseId={battleBaseId}");
                
                if (battleBaseId <= 0 || battleBaseId >= battleBases.Length)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBaseId={battleBaseId} 无效（范围：1-{battleBases.Length - 1}）");
                    return;
                }
                
                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBase 为 null");
                    return;
                }
                
                // 获取 BattleBaseComponent.storage (StorageComponent)
                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                
                if (storage == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 BattleBaseComponent.storage 为 null");
                    return;
                }
                
                // 获取 StorageComponent.grids
                var gridsField = storage.GetType().GetField("grids");
                object? gridsObj = gridsField?.GetValue(storage);
                
                if (gridsObj is not Array grids)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 StorageComponent.grids 不是数组");
                    return;
                }
                
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 BattleBaseComponent.storage.grids.Length={grids.Length}");
                
                // 输出前10个格子的内容
                int itemCount = 0;
                for (int i = 0; i < grids.Length && i < 10; i++)
                {
                    object? grid = grids.GetValue(i);
                    if (grid != null)
                    {
                        var itemIdField = grid.GetType().GetField("itemId");
                        var countField = grid.GetType().GetField("count");
                        int itemId = itemIdField != null ? (int)itemIdField.GetValue(grid)! : 0;
                        int count = countField != null ? (int)countField.GetValue(grid)! : 0;
                        
                        if (itemId > 0 && count > 0)
                        {
                            string itemName = GetItemName(itemId);
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   BattleBase.grids[{i}]: itemId={itemId} ({itemName}), count={count}");
                            itemCount++;
                        }
                    }
                }
                
                if (itemCount == 0)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 BattleBaseComponent.storage 中没有物品！");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 📊 DiagnoseBattleBaseComponentStorage 异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 📊 诊断：输出 DispenserComponent 的需求信息
        /// </summary>
        public static void DiagnoseDispenserDemands(Array dispenserPool, int dispenserCursor)
        {
            if (!DebugLog()) return;  // 只在调试模式下执行
            
            try
            {
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 Dispenser 需求诊断：");
                
                for (int i = 1; i < dispenserCursor && i < dispenserPool.Length; i++)
                {
                    object? dispenser = dispenserPool.GetValue(i);
                    if (dispenser == null) continue;
                    
                    var idField = dispenser.GetType().GetField("id");
                    int id = idField != null ? (int)idField.GetValue(dispenser)! : 0;
                    if (id != i) continue;
                    
                    var filterField = dispenser.GetType().GetField("filter");
                    var playerModeField = dispenser.GetType().GetField("playerMode");
                    var storageModeField = dispenser.GetType().GetField("storageMode");
                    
                    int filter = filterField != null ? (int)filterField.GetValue(dispenser)! : 0;
                    int playerMode = playerModeField != null ? (int)playerModeField.GetValue(dispenser)! : 0;
                    int storageMode = storageModeField != null ? (int)storageModeField.GetValue(dispenser)! : 0;
                    
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   dispenser[{i}]: filter(itemId)={filter}, playerMode={playerMode}, storageMode={storageMode}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 📊 DiagnoseDispenserDemands 异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 根据 itemId 获取物品名称（通过 LDB）
        /// </summary>
        public static string GetItemName(int itemId)
        {
            try
            {
                // 使用反射访问 LDB.items.Select(itemId)
                var ldbType = Type.GetType("LDB, Assembly-CSharp");
                if (ldbType == null) return $"item_{itemId}";
                
                var itemsProperty = ldbType.GetProperty("items", BindingFlags.Public | BindingFlags.Static);
                if (itemsProperty == null) return $"item_{itemId}";
                
                object? items = itemsProperty.GetValue(null);
                if (items == null) return $"item_{itemId}";
                
                var selectMethod = items.GetType().GetMethod("Select", new[] { typeof(int) });
                if (selectMethod == null) return $"item_{itemId}";
                
                object? itemProto = selectMethod.Invoke(items, new object[] { itemId });
                if (itemProto == null) return $"item_{itemId}";
                
                var nameProperty = itemProto.GetType().GetProperty("name") ?? itemProto.GetType().GetProperty("Name");
                if (nameProperty == null) return $"item_{itemId}";
                
                object? name = nameProperty.GetValue(itemProto);
                return name?.ToString() ?? $"item_{itemId}";
            }
            catch
            {
                return $"item_{itemId}";
            }
        }
        
        /// <summary>
        /// 📊 诊断：输出站点详细信息（包括名称）
        /// </summary>
        public static void DiagnoseStationInfo(object factory, StationComponent station)
        {
            if (!DebugLog()) return;  // 只在调试模式下执行
            
            try
            {
                var entityPoolProp = factory.GetType().GetProperty("entityPool");
                object? entityPoolObj = entityPoolProp?.GetValue(factory);
                if (entityPoolObj is not Array entityPool || station.entityId < 0 || station.entityId >= entityPool.Length)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 无法获取 entityId={station.entityId} 的详细信息");
                    return;
                }
                
                object? entity = entityPool.GetValue(station.entityId);
                if (entity == null) return;
                
                var protoIdProp = entity.GetType().GetProperty("protoId");
                if (protoIdProp == null) return;
                
                object? protoIdObj = protoIdProp.GetValue(entity);
                if (protoIdObj == null) return;
                
                int protoId = Convert.ToInt32(protoIdObj);
                string itemName = GetItemName(protoId);
                
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊     └─ 站点名称: {itemName}, protoId={protoId}");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 DiagnoseStationInfo 异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 📊 诊断：通过 battleBaseId 直接读取战场分析基站的存储内容（Import 时使用）
        /// </summary>
        public static void DiagnoseBattleBaseStorageByBattleBaseId(object factory, int battleBaseId)
        {
            if (!DebugLog()) return;  // 只在调试模式下执行
            
            try
            {
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 Import 时诊断 battleBaseId={battleBaseId} 的存储：");
                
                // 获取 defenseSystem
                var defenseSystemField = factory.GetType().GetField("defenseSystem", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (defenseSystemField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 defenseSystem 字段未找到");
                    return;
                }
                
                var defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 defenseSystem 为 null（Import 时还未初始化）");
                    return;
                }
                
                var battleBasesField = defenseSystem.GetType().GetField("battleBases", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (battleBasesField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBases 字段未找到");
                    return;
                }
                
                object? battleBasesObj = battleBasesField.GetValue(defenseSystem);
                if (battleBasesObj is not Array battleBases)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBases 不是数组");
                    return;
                }
                
                if (battleBaseId <= 0 || battleBaseId >= battleBases.Length)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBaseId={battleBaseId} 超出范围 (1-{battleBases.Length - 1})");
                    return;
                }
                
                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 battleBases[{battleBaseId}] 为 null");
                    return;
                }
                
                // 获取 storage
                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                
                if (storage == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 BattleBaseComponent.storage 为 null");
                    return;
                }
                
                // 获取 grids
                var gridsField = storage.GetType().GetField("grids");
                object? gridsObj = gridsField?.GetValue(storage);
                
                if (gridsObj is not Array grids)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 StorageComponent.grids 不是数组");
                    return;
                }
                
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 Import: BattleBase.storage.grids.Length={grids.Length}");
                
                int itemCount = 0;
                for (int i = 0; i < grids.Length && i < 20; i++)
                {
                    object? grid = grids.GetValue(i);
                    if (grid != null)
                    {
                        var itemIdField = grid.GetType().GetField("itemId");
                        var countField = grid.GetType().GetField("count");
                        int itemId = itemIdField != null ? (int)itemIdField.GetValue(grid)! : 0;
                        int count = countField != null ? (int)countField.GetValue(grid)! : 0;
                        
                        if (itemId > 0 && count > 0)
                        {
                            string itemName = GetItemName(itemId);
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 Import:   grids[{i}]: itemId={itemId} ({itemName}), count={count}");
                            itemCount++;
                        }
                    }
                }
                
                if (itemCount == 0)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 Import: 战场分析基站存储中没有物品！");
                }
                else
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 Import: 共找到 {itemCount} 种物品");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 📊 DiagnoseBattleBaseStorageByBattleBaseId 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
