using HarmonyLib;
using System;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 星球工厂初始化 - 不需要特殊处理，基站会自动检测库存变化
    /// </summary>
    [HarmonyPatch(typeof(PlanetFactory), "Init")]
    public static class PlanetFactory_Init_Patch
    {
        [HarmonyPostfix]
        static void Postfix(PlanetFactory __instance)
        {
            try
            {
                if (__instance == null) return;

                if (Plugin.DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 星球工厂初始化：行星[{__instance.planetId}]");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] PlanetFactory.Init Postfix 异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 在星球工厂被销毁时清理数据
    /// </summary>
    [HarmonyPatch(typeof(PlanetFactory), "Free")]
    public static class PlanetFactory_Free_Patch
    {
        [HarmonyPrefix]
        static void Prefix(PlanetFactory __instance)
        {
            try
            {
                if (__instance != null)
                {
                    // 清理基站物流系统数据
                    BattleBaseLogisticsManager.Clear(__instance.planetId);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] PlanetFactory.Free Prefix 异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 存档加载后 - 清理旧的虚拟配送器，基站会自动检测库存并重新派遣
    /// </summary>
    [HarmonyPatch(typeof(PlanetFactory), "Import")]
    public static class PlanetFactory_Import_Patch
    {
        [HarmonyPostfix]
        static void Postfix(PlanetFactory __instance)
        {
            try
            {
                if (__instance == null) return;

                // 清理旧方案遗留的虚拟配送器
                CleanupVirtualDispensers(__instance);

                if (Plugin.DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 存档加载完成：行星[{__instance.planetId}]");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] PlanetFactory.Import Postfix 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 清理旧方案遗留的虚拟配送器
        /// 识别方法：entityId 指向战场基站的配送器
        /// </summary>
        private static void CleanupVirtualDispensers(PlanetFactory factory)
        {
            try
            {
                if (factory?.transport == null) return;

                var dispenserPoolField = factory.transport.GetType().GetField("dispenserPool",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dispenserCursorField = factory.transport.GetType().GetField("dispenserCursor",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (dispenserPoolField == null || dispenserCursorField == null) return;

                Array? dispenserPool = dispenserPoolField.GetValue(factory.transport) as Array;
                object? dispenserCursorObj = dispenserCursorField.GetValue(factory.transport);

                if (dispenserPool == null || dispenserCursorObj == null) return;

                int dispenserCursor = Convert.ToInt32(dispenserCursorObj);

                // 获取所有战场基站的 entityId
                var battleBaseEntityIds = GetBattleBaseEntityIds(factory);
                if (battleBaseEntityIds.Count == 0) return;

                int removedCount = 0;

                // 遍历配送器，找出虚拟配送器
                for (int i = 1; i < dispenserCursor && i < dispenserPool.Length; i++)
                {
                    object? dispenserObj = dispenserPool.GetValue(i);
                    if (dispenserObj == null) continue;

                    DispenserComponent? dispenser = dispenserObj as DispenserComponent;
                    if (dispenser == null || dispenser.id != i) continue;

                    // 检查是否是虚拟配送器（entityId 指向战场基站）
                    if (battleBaseEntityIds.Contains(dispenser.entityId))
                    {
                        // 这是旧的虚拟配送器，清理它
                        dispenserPool.SetValue(null, i);
                        removedCount++;

                        if (Plugin.DebugLog())
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🧹 清理虚拟配送器[{i}]：entityId={dispenser.entityId}");
                        }
                    }
                }

                if (removedCount > 0)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 清理完成：删除 {removedCount} 个旧虚拟配送器，存档已兼容新方案");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] CleanupVirtualDispensers 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有战场基站的 entityId
        /// </summary>
        private static System.Collections.Generic.HashSet<int> GetBattleBaseEntityIds(PlanetFactory factory)
        {
            var entityIds = new System.Collections.Generic.HashSet<int>();

            try
            {
                var defenseSystem = factory?.defenseSystem;
                if (defenseSystem == null) return entityIds;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (battleBasesField == null) return entityIds;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return entityIds;

                var bufferField = battleBasesPool.GetType().GetField("buffer",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (bufferField == null) return entityIds;

                Array? battleBases = bufferField.GetValue(battleBasesPool) as Array;
                if (battleBases == null) return entityIds;

                // 收集所有战场基站的 entityId
                for (int i = 1; i < battleBases.Length; i++)
                {
                    object? battleBase = battleBases.GetValue(i);
                    if (battleBase == null) continue;

                    var entityIdField = battleBase.GetType().GetField("entityId");
                    if (entityIdField == null) continue;

                    int entityId = (int)entityIdField.GetValue(battleBase)!;
                    if (entityId > 0)
                    {
                        entityIds.Add(entityId);
                    }
                }
            }
            catch
            {
                // 忽略异常
            }

            return entityIds;
        }
    }
}
