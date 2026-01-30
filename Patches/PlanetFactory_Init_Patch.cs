using HarmonyLib;
using System;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 在星球工厂初始化后创建虚拟配送器
    /// </summary>
    [HarmonyPatch(typeof(PlanetFactory), "Init")]
    public static class PlanetFactory_Init_Patch
    {
        [HarmonyPostfix]
        static void Postfix(PlanetFactory __instance)
        {
            try
            {
                if (__instance?.transport == null)
                    return;

                // 为所有战场分析基站创建虚拟配送器
                VirtualDispenserManager.CreateVirtualDispensers(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] PlanetFactory.Init Postfix 异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 在星球工厂被销毁时清理映射
    /// </summary>
    [HarmonyPatch(typeof(PlanetFactory), "Free")]
    public static class PlanetFactory_Free_Patch
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            try
            {
                VirtualDispenserManager.Clear();
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] PlanetFactory.Free Prefix 异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 存档加载后也需要创建虚拟配送器
    /// </summary>
    [HarmonyPatch(typeof(PlanetFactory), "Import")]
    public static class PlanetFactory_Import_Patch
    {
        [HarmonyPostfix]
        static void Postfix(PlanetFactory __instance)
        {
            try
            {
                if (__instance?.transport == null)
                    return;

                // 清理旧映射
                VirtualDispenserManager.Clear();

                // 重新创建虚拟配送器
                VirtualDispenserManager.CreateVirtualDispensers(__instance);

                if (BattlefieldBaseHelper.DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 存档加载后已创建虚拟配送器");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] PlanetFactory.Import Postfix 异常: {ex.Message}");
            }
        }
    }
}
