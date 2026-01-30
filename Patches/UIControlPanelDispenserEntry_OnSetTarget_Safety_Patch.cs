using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 双重保险：在 UI 层拦截虚拟配送器，防止 OnSetTarget 访问 dispenserPool[0] 崩溃
    /// </summary>
    [HarmonyPatch(typeof(UIControlPanelDispenserEntry), "OnSetTarget")]
    public static class UIControlPanelDispenserEntry_OnSetTarget_Safety_Patch
    {
        /// <summary>
        /// 在 OnSetTarget 之前检查：如果 entity.dispenserId == 0 或是虚拟配送器，跳过原方法
        /// </summary>
        [HarmonyPrefix]
        static bool Prefix(UIControlPanelDispenserEntry __instance)
        {
            try
            {
                // 获取 target 字段（基类 UIControlPanelObjectEntry 中的字段）
                var baseType = typeof(UIControlPanelObjectEntry);
                var targetField = baseType.GetField("target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (targetField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] OnSetTarget Safety: 无法获取 target 字段");
                    return true; // 继续执行原方法
                }

                object? targetObj = targetField.GetValue(__instance);
                if (targetObj == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] OnSetTarget Safety: target 为 null");
                    return true;
                }

                // 获取 target.objId 和 target.astroId
                var targetType = targetObj.GetType();
                var objIdField = targetType.GetField("objId");
                var astroIdField = targetType.GetField("astroId");

                if (objIdField == null || astroIdField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] OnSetTarget Safety: 无法获取 target 字段");
                    return true;
                }

                int objId = (int)objIdField.GetValue(targetObj)!;
                int astroId = (int)astroIdField.GetValue(targetObj)!;

                // 获取 factory
                var gameData = GameMain.data;
                if (gameData == null || gameData.galaxy == null)
                {
                    return true;
                }

                var planet = gameData.galaxy.PlanetById(astroId);
                if (planet == null || planet.factory == null)
                {
                    return true;
                }

                var factory = planet.factory;

                // 获取 entityPool
                var factoryType = factory.GetType();
                var entityPoolField = factoryType.GetField("entityPool", BindingFlags.Public | BindingFlags.Instance);
                if (entityPoolField == null)
                {
                    return true;
                }

                Array? entityPool = entityPoolField.GetValue(factory) as Array;
                if (entityPool == null || objId <= 0 || objId >= entityPool.Length)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] OnSetTarget Safety: objId={objId} 越界");
                    return false; // 阻止原方法执行
                }

                // 获取 entity.dispenserId
                object? entity = entityPool.GetValue(objId);
                if (entity == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] OnSetTarget Safety: entity 为 null, objId={objId}");
                    return false;
                }

                var entityType = entity.GetType();
                var dispenserIdField = entityType.GetField("dispenserId");
                if (dispenserIdField == null)
                {
                    return true;
                }

                int dispenserId = (int)dispenserIdField.GetValue(entity)!;

                // 检查是否是虚拟配送器或无效配送器
                bool isVirtual = VirtualDispenserManager.IsVirtualDispenser(dispenserId);
                bool isInvalid = dispenserId == 0;

                if (isInvalid || isVirtual)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 🛡️ OnSetTarget Safety: 拦截虚拟/无效配送器，objId={objId}, dispenserId={dispenserId}, isVirtual={isVirtual}");
                    
                    // 阻止原方法执行，避免访问 dispenserPool[0] 导致崩溃
                    return false;
                }

                return true; // 允许原方法执行
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] OnSetTarget Safety 异常: {ex.Message}\n{ex.StackTrace}");
                return true; // 出错时仍然执行原方法，避免更严重的问题
            }
        }
    }
}
