using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 双重保险：在 UI 层拦截虚拟配送器，防止 OnSetTarget 访问无效配送器导致崩溃
    /// 注：正常情况下第一层过滤（DetermineFilterResults）已经移除虚拟配送器，
    /// 此补丁作为防御性编程的额外保护层
    /// </summary>
    [HarmonyPatch(typeof(UIControlPanelDispenserEntry), "OnSetTarget")]
    public static class UIControlPanelDispenserEntry_OnSetTarget_Safety_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(UIControlPanelDispenserEntry __instance)
        {
            try
            {
                // 获取 target 字段
                var baseType = typeof(UIControlPanelObjectEntry);
                var targetField = baseType.GetField("target", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (targetField == null) return true;

                object? targetObj = targetField.GetValue(__instance);
                if (targetObj == null) return true;

                // 获取 target.objId 和 target.astroId
                var targetType = targetObj.GetType();
                var objIdField = targetType.GetField("objId");
                var astroIdField = targetType.GetField("astroId");
                if (objIdField == null || astroIdField == null) return true;

                int objId = (int)objIdField.GetValue(targetObj)!;
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
                if (entityPool == null || objId <= 0 || objId >= entityPool.Length) return false;

                // 获取 entity.dispenserId
                object? entity = entityPool.GetValue(objId);
                if (entity == null) return false;

                var entityType = entity.GetType();
                var dispenserIdField = entityType.GetField("dispenserId");
                if (dispenserIdField == null) return true;

                int dispenserId = (int)dispenserIdField.GetValue(entity)!;

                // 检查是否是虚拟配送器或无效配送器（dispenserId == 0）
                bool isInvalid = dispenserId == 0 || VirtualDispenserManager.IsVirtualDispenser(dispenserId);

                // 阻止访问无效配送器
                return !isInvalid;
            }
            catch
            {
                // 出错时允许执行原方法（游戏自己会处理错误）
                return true;
            }
        }
    }
}
