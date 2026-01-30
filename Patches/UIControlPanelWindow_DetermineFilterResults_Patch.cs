using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 在监控面板中完全隐藏虚拟配送器（方案A）
    /// 策略：在 DetermineFilterResults 中，扫描配送器时跳过虚拟配送器，避免添加到 results
    /// </summary>
    [HarmonyPatch(typeof(UIControlPanelWindow), "DetermineFilterResults")]
    public static class UIControlPanelWindow_DetermineFilterResults_Patch
    {
        private static int _callCount = 0;
        
        /// <summary>
        /// Transpiler：修改 IL 代码，在添加配送器到列表前检查是否是虚拟配送器
        /// 这个方案太复杂，改用 Postfix 方案
        /// </summary>
        
        /// <summary>
        /// Postfix：在 DetermineFilterResults 执行后，从 results 中移除虚拟配送器
        /// 关键：同时移除 results、resultPositions 中的对应项
        /// </summary>
        [HarmonyPostfix]
        static void Postfix(UIControlPanelWindow __instance)
        {
            _callCount++;
            
            try
            {
                // 【强制刷新日志】确保日志立即写入
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ========== DetermineFilterResults Postfix 第 {_callCount} 次调用 ==========");
                
                // 获取必要的字段
                var windowType = typeof(UIControlPanelWindow);
                var resultsField = windowType.GetField("results", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var resultPositionsField = windowType.GetField("resultPositions", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (resultsField == null || resultPositionsField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] DetermineFilterResults Postfix: 无法获取 results 或 resultPositions 字段");
                    return;
                }

                var results = resultsField.GetValue(__instance) as System.Collections.IList;
                var resultPositions = resultPositionsField.GetValue(__instance) as System.Collections.IList;
                
                if (results == null || resultPositions == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] DetermineFilterResults Postfix: results 或 resultPositions 为 null");
                    return;
                }

                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] results.Count={results.Count}, resultPositions.Count={resultPositions.Count}");

                // 收集需要移除的索引（从后往前）
                List<int> indicesToRemove = new List<int>();
                
                var gameData = GameMain.data;
                if (gameData == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] gameData 为 null，退出");
                    return;
                }

                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] 开始遍历 results，总数={results.Count}");

                // 先统计 entryType 分布
                var typeCounter = new System.Collections.Generic.Dictionary<int, int>();
                for (int i = 0; i < results.Count; i++)
                {
                    object? result = results[i];
                    if (result == null) continue;
                    var resultType = result.GetType();
                    var entryTypeField = resultType.GetField("entryType");
                    if (entryTypeField != null)
                    {
                        int entryType = Convert.ToInt32(entryTypeField.GetValue(result));
                        if (!typeCounter.ContainsKey(entryType))
                            typeCounter[entryType] = 0;
                        typeCounter[entryType]++;
                    }
                }
                
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] entryType 分布: " + string.Join(", ", typeCounter.Select(kv => $"type{kv.Key}={kv.Value}")));

                // 遍历 results，找出虚拟配送器
                for (int i = results.Count - 1; i >= 0; i--)
                {
                    object? result = results[i];
                    if (result == null)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] results[{i}] 为 null，跳过");
                        continue;
                    }

                    // 获取 ControlPanelTarget 的字段
                    var resultType = result.GetType();
                    var entryTypeField = resultType.GetField("entryType");
                    var objIdField = resultType.GetField("objId");
                    var astroIdField = resultType.GetField("astroId");

                    if (entryTypeField == null || objIdField == null || astroIdField == null)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] results[{i}] 缺少必要字段，跳过");
                        continue;
                    }

                    // 检查是否是配送器类型
                    int entryType = Convert.ToInt32(entryTypeField.GetValue(result));
                    
                    if (entryType != 5) continue; // EControlPanelEntryType.Dispenser = 5 ✅

                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️⚠️⚠️ 找到配送器！index={i}");


                    int objId = (int)objIdField.GetValue(result)!; // entityId
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

                    // 检查是否是虚拟配送器
                    bool isVirtual = VirtualDispenserManager.IsVirtualDispenser(dispenserId);

                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] 检查配送器: index={i}, objId={objId}, dispenserId={dispenserId}, isVirtual={isVirtual}");

                    // 如果 dispenserId == 0 或者是虚拟配送器，标记为需要移除
                    if (dispenserId == 0 || isVirtual)
                    {
                        indicesToRemove.Add(i);
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🗑️ 标记移除虚拟配送器: index={i}, dispenserId={dispenserId}");
                    }
                }

                // 从后往前移除（保持索引有效）
                foreach (int index in indicesToRemove)
                {
                    // 移除 results[index]
                    results.RemoveAt(index);
                    
                    // 移除 resultPositions[index]
                    // 注意：resultPositions 有 results.Count + 1 个元素（最后一个是总高度）
                    resultPositions.RemoveAt(index);
                    
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 已移除虚拟配送器: index={index}");
                }

                if (indicesToRemove.Count > 0)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ✅✅✅ 监控面板：共隐藏 {indicesToRemove.Count} 个虚拟配送器，剩余 {results.Count} 个配送器");
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [DEBUG] 移除后 results.Count={results.Count}, resultPositions.Count={resultPositions.Count}");
                }
                else
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚪ 本次调用未发现虚拟配送器");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DetermineFilterResults Postfix 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
