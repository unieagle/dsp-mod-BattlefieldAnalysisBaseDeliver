using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 新方案：不创建StationComponent，直接让DispenserComponent从BattleBaseComponent读取物品
    /// </summary>
    [HarmonyPatch(typeof(PlanetTransport), nameof(PlanetTransport.RefreshDispenserTraffic))]
    public static class PlanetTransport_RefreshDispenserTraffic_NEW_Patch
    {
        private static int _callCount = 0;
        private static System.Collections.Generic.Dictionary<string, int> _pairAddCounts = new System.Collections.Generic.Dictionary<string, int>();

        [HarmonyPostfix]
        static void Postfix(PlanetTransport __instance, int keyId)
        {
            try
            {
                _callCount++;
                bool debugLog = BattlefieldBaseHelper.DebugLog();
                bool verboseLog = _callCount <= 50;

                if (debugLog && verboseLog)
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] === RefreshDispenserTraffic(NEW) 第 {_callCount} 次调用 keyId={keyId} ===");

                // 【关键】确保虚拟配送器已创建（解决时序问题）
                // 如果 RefreshDispenserTraffic 在 Import Postfix 之前被调用，这里会先创建虚拟配送器
                if (__instance.factory != null)
                {
                    VirtualDispenserManager.CreateVirtualDispensers(__instance.factory);
                }

                // 获取 dispenserPool 和 dispenserCursor
                var dispenserPoolField = typeof(PlanetTransport).GetField("dispenserPool", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var dispenserCursorField = typeof(PlanetTransport).GetField("dispenserCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (dispenserPoolField == null || dispenserCursorField == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): dispenserPool/Cursor 字段未找到");
                    return;
                }

                object? dispenserPoolObj = dispenserPoolField.GetValue(__instance);
                object? dispenserCursorObj = dispenserCursorField.GetValue(__instance);

                if (dispenserPoolObj is not Array dispenserPool || dispenserCursorObj == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): dispenserPool 为 null 或非数组");
                    return;
                }

                int dispenserCursor = Convert.ToInt32(dispenserCursorObj);

                // 检查 factory
                if (__instance.factory == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): __instance.factory 为 null！");
                    return;
                }

                if (__instance.factory.planetId == 0)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): planetId 为 0！");
                    return;
                }

                object factory = __instance.factory;
                int planetId = __instance.factory.planetId;

                if (debugLog && verboseLog)
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): planetId={planetId}，开始检查战场分析基站...");

                // 获取 defenseSystem
                var defenseSystemField = factory.GetType().GetField("defenseSystem", BindingFlags.Public | BindingFlags.Instance);
                if (defenseSystemField == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): defenseSystem 字段未找到");
                    return;
                }

                object? defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): defenseSystem 为 null（可能还未初始化）");
                    return;
                }

                // 获取 battleBases (ObjectPool<BattleBaseComponent>)
                var battleBasesField = defenseSystem.GetType().GetField("battleBases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): battleBases 字段未找到");
                    return;
                }

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): battleBases 为 null");
                    return;
                }

                // 获取 ObjectPool.buffer (这才是数组)
                var bufferField = battleBasesPool.GetType().GetField("buffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): ObjectPool.buffer 字段未找到");
                    return;
                }

                object? battleBasesObj = bufferField.GetValue(battleBasesPool);
                if (battleBasesObj is not Array battleBases)
                {
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): buffer 不是数组");
                    return;
                }

                if (debugLog && verboseLog)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): battleBases.Length={battleBases.Length}");
                    
                    // 📊 诊断：输出所有配送器的信息
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): dispenserCursor={dispenserCursor}, 开始诊断配送器...");
                    for (int i = 1; i < dispenserCursor && i < dispenserPool.Length; i++)
                    {
                        object? disp = dispenserPool.GetValue(i);
                        if (disp == null) continue;
                        
                        var idF = disp.GetType().GetField("id");
                        int dispId = idF != null ? (int)idF.GetValue(disp)! : 0;
                        if (dispId != i) continue;
                        
                        var filterF = disp.GetType().GetField("filter");
                        var playerModeF = disp.GetType().GetField("playerMode");
                        int filter = filterF != null ? (int)filterF.GetValue(disp)! : 0;
                        int playerMode = playerModeF != null ? (int)playerModeF.GetValue(disp)! : 0;
                        
                        string itemName = filter > 0 ? BattlefieldBaseHelper.GetItemName(filter) : "无";
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   dispenser[{i}]: filter={filter} ({itemName}), playerMode={playerMode} (2=需求)");
                    }
                }

                int battleBaseCount = 0;
                int pairCount = 0;
                int totalItemsFound = 0;

                // 遍历所有战场分析基站
                for (int battleBaseId = 1; battleBaseId < battleBases.Length; battleBaseId++)
                {
                    object? battleBase = battleBases.GetValue(battleBaseId);
                    if (battleBase == null) continue;

                    // 检查id是否匹配
                    var idField = battleBase.GetType().GetField("id");
                    if (idField == null) continue;
                    int id = (int)idField.GetValue(battleBase)!;
                    if (id != battleBaseId) continue;

                    battleBaseCount++;
                    
                    if (debugLog && verboseLog)
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): ✓ 找到 battleBaseId={battleBaseId}");

                    // 获取 storage
                    var storageField = battleBase.GetType().GetField("storage");
                    object? storage = storageField?.GetValue(battleBase);
                    if (storage == null)
                    {
                        if (debugLog && verboseLog)
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): battleBaseId={battleBaseId} 的 storage 为 null");
                        continue;
                    }

                    // 获取 grids
                    var gridsField = storage.GetType().GetField("grids");
                    object? gridsObj = gridsField?.GetValue(storage);
                    if (gridsObj is not Array grids)
                    {
                        if (debugLog && verboseLog)
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): battleBaseId={battleBaseId} 的 grids 不是数组");
                        continue;
                    }

                    if (debugLog && verboseLog)
                    {
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): battleBaseId={battleBaseId}, storage.grids.Length={grids.Length}");
                        
                        // 📊 输出 battleBase 的其他字段信息
                        var entityIdField = battleBase.GetType().GetField("entityId");
                        var pcIdField = battleBase.GetType().GetField("pcId");
                        int entityId = entityIdField != null ? (int)entityIdField.GetValue(battleBase)! : 0;
                        int pcId = pcIdField != null ? (int)pcIdField.GetValue(battleBase)! : 0;
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   battleBase[{battleBaseId}]: entityId={entityId}, pcId={pcId}");
                    }

                    // 📊 统计这个基站有多少物品
                    int itemsInThisBase = 0;
                    for (int i = 0; i < grids.Length; i++)
                    {
                        object? g = grids.GetValue(i);
                        if (g == null) continue;
                        var itemIdF = g.GetType().GetField("itemId");
                        var countF = g.GetType().GetField("count");
                        int iid = itemIdF != null ? (int)itemIdF.GetValue(g)! : 0;
                        int cnt = countF != null ? (int)countF.GetValue(g)! : 0;
                        if (iid > 0 && cnt > 0)
                        {
                            itemsInThisBase++;
                            totalItemsFound++;
                            if (debugLog && verboseLog)
                            {
                                string iname = BattlefieldBaseHelper.GetItemName(iid);
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   battleBase[{battleBaseId}].grids[{i}]: itemId={iid} ({iname}), count={cnt}");
                            }
                        }
                    }

                    if (debugLog && verboseLog)
                    {
                        if (itemsInThisBase == 0)
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   battleBase[{battleBaseId}] 中没有物品");
                        else
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   battleBase[{battleBaseId}] 共有 {itemsInThisBase} 种物品");
                    }

                    // 遍历战场分析基站的物品格子
                    for (int gridIdx = 0; gridIdx < grids.Length; gridIdx++)
                    {
                        object? grid = grids.GetValue(gridIdx);
                        if (grid == null) continue;

                        var itemIdField = grid.GetType().GetField("itemId");
                        var countField = grid.GetType().GetField("count");
                        int itemId = itemIdField != null ? (int)itemIdField.GetValue(grid)! : 0;
                        int count = countField != null ? (int)countField.GetValue(grid)! : 0;

                        // ✅ 改进：只要格子有物品ID（即使count=0），就建立配对
                        // 这样即使暂时没货，配对也会保持，有货就立即派遣
                        if (itemId <= 0) continue;

                        // 📊 这个格子有物品
                        bool foundMatch = false;

                        // 检查是否有配送器需要它
                        for (int dispenserId = 1; dispenserId < dispenserCursor && dispenserId < dispenserPool.Length; dispenserId++)
                        {
                            object? dispenser = dispenserPool.GetValue(dispenserId);
                            if (dispenser == null) continue;

                            var dispIdField = dispenser.GetType().GetField("id");
                            int dispId = dispIdField != null ? (int)dispIdField.GetValue(dispenser)! : 0;
                            if (dispId != dispenserId) continue;

                            // 检查配送器是否需要这个物品
                            var filterField = dispenser.GetType().GetField("filter");
                            var playerModeField = dispenser.GetType().GetField("playerMode");
                            int filter = filterField != null ? (int)filterField.GetValue(dispenser)! : 0;
                            int playerMode = playerModeField != null ? (int)playerModeField.GetValue(dispenser)! : 0;

                            // 只处理需求模式（playerMode=2表示需求）
                            if (playerMode != 2) continue;
                            if (filter != itemId) continue; // 配送器不需要这个物品

                            // 找到匹配！
                            foundMatch = true;
                            
                            // 【新方案】使用虚拟配送器ID（正数）
                            // 获取战场分析基站对应的虚拟配送器ID
                            if (!VirtualDispenserManager.TryGetVirtualDispenserId(battleBaseId, out int virtualDispenserId))
                            {
                                if (debugLog && verboseLog)
                                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 战场分析基站 {battleBaseId} 没有对应的虚拟配送器");
                                continue;
                            }

                            // ✅ 检查基站是否仍然存在（防止基站拆除后仍建立配对）
                            if (!VirtualDispenserManager.CheckBattleBaseExists(__instance.factory, battleBaseId))
                            {
                                if (debugLog)
                                {
                                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 战场基站[{battleBaseId}]不存在，跳过虚拟配送器[{virtualDispenserId}]");
                                }
                                continue;
                            }

                            try
                            {
                                // 【关键】检查配对是否已存在（幂等性）
                                // ⚠️ 注意：AddPair 只在 supplyId < 0 或 demandId < 0 时增加 playerPairCount
                                // 我们的虚拟配送器使用正数ID，所以配对在 pairCount 中，但不在 playerPairCount 中
                                // 因此必须遍历 pairCount 而不是 playerPairCount
                                var pairsField = dispenser.GetType().GetField("pairs");
                                var pairCountField = dispenser.GetType().GetField("pairCount");
                                
                                if (pairsField != null && pairCountField != null)
                                {
                                    Array? existingPairs = pairsField.GetValue(dispenser) as Array;
                                    int existingPairCount = (int)pairCountField.GetValue(dispenser)!;
                                    bool alreadyExists = false;
                                    
                                    if (existingPairs != null && existingPairCount > 0)
                                    {
                                        // ✅ 遍历所有配对（pairCount），而不只是 playerPairCount
                                        for (int pairIdx = 0; pairIdx < existingPairCount && pairIdx < existingPairs.Length; pairIdx++)
                                        {
                                            object? pair = existingPairs.GetValue(pairIdx);
                                            if (pair == null) continue;
                                            
                                            var pairType = pair.GetType();
                                            var supplyIdField = pairType.GetField("supplyId");
                                            var demandIdField = pairType.GetField("demandId");
                                            
                                            int existingSupplyId = supplyIdField != null ? (int)supplyIdField.GetValue(pair)! : 0;
                                            int existingDemandId = demandIdField != null ? (int)demandIdField.GetValue(pair)! : 0;
                                            
                                            // 检查是否已经存在相同的配对
                                            if (existingSupplyId == virtualDispenserId && existingDemandId == dispenserId)
                                            {
                                                alreadyExists = true;
                                                if (debugLog && verboseLog)
                                                {
                                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🔍 发现已存在的配对 at index {pairIdx}/{existingPairCount}: supplyId={existingSupplyId}, demandId={existingDemandId}");
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    
                                    // 只在不存在时添加
                                    if (!alreadyExists)
                                    {
                                        // 调用 dispenser.AddPair(supplyId, supplyIdx, demandId, demandIdx)
                                        var addPairMethod = dispenser.GetType().GetMethod("AddPair", BindingFlags.Public | BindingFlags.Instance);
                                        if (addPairMethod != null)
                                        {
                                            // supplyId = virtualDispenserId (正数，虚拟配送器ID！)
                                            // supplyIdx = gridIdx
                                            // demandId = dispenserId
                                            // demandIdx = 0 (配送器的槽位)
                                            addPairMethod.Invoke(dispenser, new object[] { virtualDispenserId, gridIdx, dispenserId, 0 });

                                            pairCount++;
                                            
                                            // 【诊断】记录配对添加次数
                                            string pairKey = $"v{virtualDispenserId}_d{dispenserId}_i{itemId}";
                                            if (!_pairAddCounts.ContainsKey(pairKey))
                                            {
                                                _pairAddCounts[pairKey] = 0;
                                            }
                                            _pairAddCounts[pairKey]++;

                                            if (debugLog && (verboseLog || pairCount <= 5))
                                            {
                                                string itemName = BattlefieldBaseHelper.GetItemName(itemId);
                                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ 已添加配对（第{_pairAddCounts[pairKey]}次）：虚拟配送器[{virtualDispenserId}] (战场基站{battleBaseId}) gridIdx={gridIdx} itemId={itemId} ({itemName}) → 配送器[{dispenserId}]");
                                            }
                                        }
                                    }
                                    else if (debugLog && verboseLog)
                                    {
                                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ⏭️ 跳过已存在的配对：虚拟配送器[{virtualDispenserId}] → 配送器[{dispenserId}]");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): AddPair 失败: {ex.Message}");
                            }
                        }
                        
                        // 📊 如果这个物品没有找到匹配的配送器，输出诊断信息
                        if (!foundMatch && debugLog && verboseLog)
                        {
                            string itemName = BattlefieldBaseHelper.GetItemName(itemId);
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊   battleBase[{battleBaseId}].grids[{gridIdx}] 的物品 {itemId} ({itemName}) 没有找到匹配的配送器");
                        }
                    }
                }

                if (debugLog && verboseLog)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenser(NEW): 总结 - 检查了 {battleBaseCount} 个战场分析基站，发现 {totalItemsFound} 个物品格子，添加了 {pairCount} 个配对");
                    
                    if (battleBaseCount == 0)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 没有找到任何战场分析基站！可能原因：");
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊   1. 这个星球上没有战场分析基站");
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊   2. 战场分析基站的 id 字段不匹配");
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊   3. battleBases.buffer 中的对象为 null");
                    }
                    else if (totalItemsFound == 0)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 找到了 {battleBaseCount} 个战场分析基站，但都没有物品！");
                    }
                    else if (pairCount == 0)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 📊 战场分析基站有 {totalItemsFound} 个物品，但没有配送器需要它们！");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] RefreshDispenserTraffic(NEW) Postfix 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
