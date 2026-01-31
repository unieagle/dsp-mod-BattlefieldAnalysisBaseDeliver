using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 在监控面板中完全隐藏虚拟配送器
    /// 策略：在 DetermineFilterResults 执行后，从 results 中移除虚拟配送器
    /// </summary>
    [HarmonyPatch(typeof(UIControlPanelWindow), "DetermineFilterResults")]
    public static class UIControlPanelWindow_DetermineFilterResults_Patch
    {
        /// <summary>
        /// Postfix：在 DetermineFilterResults 执行后，从 results 中移除虚拟配送器
        /// 关键：同时移除 results、resultPositions 中的对应项，保持列表同步
        /// </summary>
        [HarmonyPostfix]
        static void Postfix(UIControlPanelWindow __instance)
        {
            try
            {
                // 获取必要的字段
                var windowType = typeof(UIControlPanelWindow);
                var resultsField = windowType.GetField("results", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var resultPositionsField = windowType.GetField("resultPositions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (resultsField == null || resultPositionsField == null) return;

                var results = resultsField.GetValue(__instance) as System.Collections.IList;
                var resultPositions = resultPositionsField.GetValue(__instance) as System.Collections.IList;
                
                if (results == null || resultPositions == null) return;

                // 收集需要移除的索引（从后往前）
                List<int> indicesToRemove = new List<int>();
                
                var gameData = GameMain.data;
                if (gameData == null) return;

                // 遍历 results，找出虚拟配送器
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    object? result = results[i];
                    if (result == null) continue;

                    // 获取 ControlPanelTarget 的字段
                    var resultType = result.GetType();
                    var entryTypeField = resultType.GetField("entryType");
                    var objIdField = resultType.GetField("objId");
                    var astroIdField = resultType.GetField("astroId");

                    if (entryTypeField == null || objIdField == null || astroIdField == null) continue;

                    // 检查是否是配送器类型（EControlPanelEntryType.Dispenser = 5）
                    int entryType = Convert.ToInt32(entryTypeField.GetValue(result));
                    if (entryType != 5) continue;

                    int objId = (int)objIdField.GetValue(result)!;
                    int astroId = (int)astroIdField.GetValue(result)!;

                    // 获取 planet 和 factory
                    var planet = gameData.galaxy?.PlanetById(astroId);
                    if (planet == null || planet.factory == null) continue;

                    var factory = planet.factory;

                    // 获取 entityPool
                    var factoryType = factory.GetType();
                    var entityPoolField = factoryType.GetField("entityPool", BindingFlags.Public | BindingFlags.Instance);
                    if (entityPoolField == null) continue;

                    Array? entityPool = entityPoolField.GetValue(factory) as Array;
                    if (entityPool == null || objId <= 0 || objId >= entityPool.Length) continue;

                    // 获取 entity
                    object? entity = entityPool.GetValue(objId);
                    if (entity == null) continue;

                    // 获取 entity.dispenserId
                    var entityType = entity.GetType();
                    var dispenserIdField = entityType.GetField("dispenserId");
                    if (dispenserIdField == null) continue;

                    int dispenserId = (int)dispenserIdField.GetValue(entity)!;

                    // 检查是否是虚拟配送器（dispenserId == 0 表示非配送器实体，如战场基站）
                    bool isVirtual = dispenserId == 0 || VirtualDispenserManager.IsVirtualDispenser(dispenserId);

                    if (isVirtual)
                    {
                        indicesToRemove.Add(i);
                    }
                }

                // 从后往前移除（保持索引有效）
                foreach (int index in indicesToRemove)
                {
                    results.RemoveAt(index);
                    resultPositions.RemoveAt(index);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DetermineFilterResults Postfix 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
