using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 监控战场分析基站物品变化（包括自动收集和手动放入），触发配送器刷新
    /// </summary>
    [HarmonyPatch(typeof(BattleBaseComponent), "InternalUpdate")]
    public static class BattleBaseComponent_InternalUpdate_Patch
    {
        private static System.Collections.Generic.Dictionary<int, int> _lastItemCounts = new System.Collections.Generic.Dictionary<int, int>();
        private static System.Collections.Generic.Dictionary<int, int> _triggerThrottles = new System.Collections.Generic.Dictionary<int, int>();
        private const int TRIGGER_INTERVAL = 120; // 每120帧（约2秒）最多触发一次

        [HarmonyPostfix]
        static void Postfix(BattleBaseComponent __instance, PlanetFactory factory)
        {
            try
            {
                int battleBaseId = __instance.id;
                
                // 限流：避免频繁触发（每个基站独立限流）
                if (!_triggerThrottles.ContainsKey(battleBaseId))
                    _triggerThrottles[battleBaseId] = 0;
                
                _triggerThrottles[battleBaseId]++;
                if (_triggerThrottles[battleBaseId] < TRIGGER_INTERVAL)
                    return;

                _triggerThrottles[battleBaseId] = 0;

                // 检查基站是否有物品
                if (__instance.storage == null)
                    return;

                // 获取 storage.grids
                var gridsField = __instance.storage.GetType().GetField("grids");
                if (gridsField == null)
                    return;

                Array? grids = gridsField.GetValue(__instance.storage) as Array;
                if (grids == null)
                    return;

                // 统计物品种类数量
                int itemTypeCount = 0;
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
                        itemTypeCount++;
                    }
                }

                // 获取上次的物品数量
                if (!_lastItemCounts.ContainsKey(battleBaseId))
                    _lastItemCounts[battleBaseId] = 0;
                
                int lastItemCount = _lastItemCounts[battleBaseId];

                // 如果物品种类发生变化（增加或从0变为非0），触发刷新
                bool shouldRefresh = false;
                if (itemTypeCount > lastItemCount)
                {
                    // 物品种类增加
                    shouldRefresh = true;
                }
                else if (lastItemCount == 0 && itemTypeCount > 0)
                {
                    // 从没有物品变为有物品（即使种类数相同）
                    shouldRefresh = true;
                }
                
                if (shouldRefresh)
                {
                    _lastItemCounts[battleBaseId] = itemTypeCount;

                    // 触发所有配送器刷新配对
                    if (factory?.transport != null)
                    {
                        try
                        {
                            var transportType = factory.transport.GetType();
                            var dispenserPoolField = transportType.GetField("dispenserPool", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            var dispenserCursorField = transportType.GetField("dispenserCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            if (dispenserPoolField == null || dispenserCursorField == null)
                            {
                                if (BattlefieldBaseHelper.DebugLog())
                                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 无法获取 dispenserPool 或 dispenserCursor 字段");
                                return;
                            }

                            object? dispenserPoolObj = dispenserPoolField.GetValue(factory.transport);
                            object? dispenserCursorObj = dispenserCursorField.GetValue(factory.transport);

                            if (dispenserPoolObj == null || dispenserCursorObj == null)
                            {
                                if (BattlefieldBaseHelper.DebugLog())
                                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] dispenserPool 或 dispenserCursor 为 null");
                                return;
                            }

                            if (dispenserPoolObj is Array allDispensers)
                            {
                                int dispenserCursor = Convert.ToInt32(dispenserCursorObj);

                                // 刷新所有配送器（因为不知道哪个配送器需要这个新物品）
                                int refreshCount = 0;
                                for (int i = 1; i < dispenserCursor && i < allDispensers.Length; i++)
                                {
                                    object? disp = allDispensers.GetValue(i);
                                    if (disp == null) continue;

                                    var idField = disp.GetType().GetField("id");
                                    int dispId = idField != null ? (int)idField.GetValue(disp)! : 0;
                                    if (dispId != i) continue;
                                    
                                    // 跳过虚拟配送器（它们不需要刷新）
                                    if (VirtualDispenserManager.IsVirtualDispenser(i))
                                        continue;

                                    factory.transport.RefreshDispenserTraffic(i);
                                    refreshCount++;
                                }

                                if (BattlefieldBaseHelper.DebugLog())
                                {
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🔄 战场分析基站物品变化（{itemTypeCount} 种），触发刷新 {refreshCount} 个配送器");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 基站物品变化触发刷新失败: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }
                else if (itemTypeCount < lastItemCount)
                {
                    // 物品种类减少（取完了），也更新记录
                    _lastItemCounts[battleBaseId] = itemTypeCount;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] BattleBaseComponent.AutoPickTrash Postfix 异常: {ex.Message}");
            }
        }
    }
}
