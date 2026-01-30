using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 监控战场分析基站捡取物品，触发配送器刷新
    /// </summary>
    [HarmonyPatch(typeof(BattleBaseComponent), "AutoPickTrash")]
    public static class BattleBaseComponent_AutoPickTrash_Patch
    {
        private static int _lastItemCount = 0;
        private static int _triggerThrottle = 0;
        private const int TRIGGER_INTERVAL = 180; // 每180帧（约3秒）最多触发一次

        [HarmonyPostfix]
        static void Postfix(BattleBaseComponent __instance, PlanetFactory factory)
        {
            try
            {
                // 限流：避免频繁触发
                _triggerThrottle++;
                if (_triggerThrottle < TRIGGER_INTERVAL)
                    return;

                _triggerThrottle = 0;

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

                // 如果物品种类发生变化（新增了物品类型），触发刷新
                if (itemTypeCount > _lastItemCount)
                {
                    _lastItemCount = itemTypeCount;

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
                else if (itemTypeCount < _lastItemCount)
                {
                    // 物品种类减少（取完了），也更新记录
                    _lastItemCount = itemTypeCount;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] BattleBaseComponent.AutoPickTrash Postfix 异常: {ex.Message}");
            }
        }
    }
}
