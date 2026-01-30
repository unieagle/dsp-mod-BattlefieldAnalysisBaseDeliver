using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 阻止虚拟配送器在监控面板中引发 NullReferenceException
    /// 策略：在 OnSetTarget 中检查 entity.dispenserId，如果为 0（虚拟配送器）则跳过
    /// </summary>
    [HarmonyPatch(typeof(UIControlPanelDispenserEntry), "OnSetTarget")]
    public static class UIControlPanelDispenserEntry_OnSetTarget_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(UIControlPanelDispenserEntry __instance)
        {
            try
            {
                // 获取 target 字段（不是方法参数，而是实例字段）
                var targetField = typeof(UIControlPanelDispenserEntry).BaseType?.GetField("target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (targetField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 无法获取 target 字段");
                    return true;
                }

                object? targetObj = targetField.GetValue(__instance);
                if (targetObj == null) return true;

                // 获取 target.entryType
                var targetType = targetObj.GetType();
                var entryTypeField = targetType.GetField("entryType");
                if (entryTypeField == null) return true;

                int entryType = Convert.ToInt32(entryTypeField.GetValue(targetObj));
                
                // 检查是否是配送器类型
                if (entryType != 4) // EControlPanelEntryType.Dispenser = 4
                    return true; // 不是配送器，继续执行原方法

                // 获取 target.objId (entityId)
                var objIdField = targetType.GetField("objId");
                if (objIdField == null) return true;

                int entityId = (int)objIdField.GetValue(targetObj)!;

                // 获取 target.astroId
                var astroIdField = targetType.GetField("astroId");
                if (astroIdField == null) return true;

                int astroId = (int)astroIdField.GetValue(targetObj)!;

                // 获取 factory
                var gameData = GameMain.data;
                if (gameData == null || gameData.galaxy == null) return true;

                var planet = gameData.galaxy.PlanetById(astroId);
                if (planet == null || planet.factory == null) return true;

                var factory = planet.factory;

                // 获取 entityPool
                var factoryType = factory.GetType();
                var entityPoolField = factoryType.GetField("entityPool", BindingFlags.Public | BindingFlags.Instance);
                if (entityPoolField == null) return true;

                Array? entityPool = entityPoolField.GetValue(factory) as Array;
                if (entityPool == null || entityId <= 0 || entityId >= entityPool.Length)
                    return true;

                // 获取 entity
                object? entity = entityPool.GetValue(entityId);
                if (entity == null) return true;

                // 获取 entity.dispenserId
                var entityType = entity.GetType();
                var dispenserIdField = entityType.GetField("dispenserId");
                if (dispenserIdField == null) return true;

                int dispenserId = (int)dispenserIdField.GetValue(entity)!;

                // 如果 dispenserId == 0，说明这个 entity 不是配送器（可能是战场基站）
                // 跳过执行，避免 NullReferenceException
                if (dispenserId == 0)
                {
                    if (BattlefieldBaseHelper.DebugLog())
                    {
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚫 跳过虚拟配送器 OnSetTarget (entityId={entityId}, dispenserId=0)");
                    }
                    return false; // 跳过原方法执行
                }

                // dispenserId 有效，继续执行原方法
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] UIControlPanelDispenserEntry.OnSetTarget Prefix 异常: {ex.Message}");
                // 出错时跳过原方法，避免崩溃
                return false;
            }
        }
    }
}
