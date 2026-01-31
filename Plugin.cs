using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace BattlefieldAnalysisBaseDeliver
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource? Log;
        public static ConfigEntry<bool> EnableDebugLog = null!;

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 正在加载... (GUID: {PluginInfo.PLUGIN_GUID})");

            EnableDebugLog = Config.Bind(
                "General",
                "EnableDebugLog",
                false,
                "为 true 时在日志中输出详细的调试信息，用于排查问题。正常使用时建议设置为 false。");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);

            // ✅ 方案C：为战场分析基站创建虚拟配送器，使用正数ID
            
            // Patch 1: PlanetFactory初始化 - 创建虚拟配送器
            var planetFactoryInitMethod = AccessTools.Method(typeof(PlanetFactory), "Init");
            if (planetFactoryInitMethod != null)
            {
                harmony.Patch(
                    original: planetFactoryInitMethod,
                    postfix: new HarmonyMethod(typeof(Patches.PlanetFactory_Init_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 PlanetFactory.Init 应用补丁（创建虚拟配送器）。");
            }

            var planetFactoryImportMethod = AccessTools.Method(typeof(PlanetFactory), "Import");
            if (planetFactoryImportMethod != null)
            {
                harmony.Patch(
                    original: planetFactoryImportMethod,
                    postfix: new HarmonyMethod(typeof(Patches.PlanetFactory_Import_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 PlanetFactory.Import 应用补丁（存档加载后创建虚拟配送器）。");
            }

            var planetFactoryFreeMethod = AccessTools.Method(typeof(PlanetFactory), "Free");
            if (planetFactoryFreeMethod != null)
            {
                harmony.Patch(
                    original: planetFactoryFreeMethod,
                    prefix: new HarmonyMethod(typeof(Patches.PlanetFactory_Free_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 PlanetFactory.Free 应用补丁（清理虚拟配送器映射）。");
            }
            
            // Patch 2: PlanetTransport.RefreshDispenserTraffic - 添加配对（使用虚拟配送器ID）
            var refreshDispenserMethod = AccessTools.Method(typeof(PlanetTransport), nameof(PlanetTransport.RefreshDispenserTraffic));
            if (refreshDispenserMethod != null)
            {
                harmony.Patch(
                    original: refreshDispenserMethod,
                    postfix: new HarmonyMethod(typeof(Patches.PlanetTransport_RefreshDispenserTraffic_NEW_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 PlanetTransport.RefreshDispenserTraffic 应用补丁（添加配对）。");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 未找到 PlanetTransport.RefreshDispenserTraffic 方法！");
            }
            
            // Patch 5: DispenserComponent.InternalTick - 方案C：Prefix 派出空载无人机
            var internalTickMethod = AccessTools.Method(typeof(DispenserComponent), "InternalTick");
            if (internalTickMethod != null)
            {
                harmony.Patch(
                    original: internalTickMethod,
                    prefix: new HarmonyMethod(typeof(Patches.DispenserComponent_InternalTick_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 DispenserComponent.InternalTick 应用补丁（Prefix：派出空载无人机）。");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 未找到 DispenserComponent.InternalTick 方法！");
            }

            // Patch 6: DispenserComponent.OnRematchPairs - 跳过虚拟配送器
            var onRematchPairsMethod = AccessTools.Method(typeof(DispenserComponent), "OnRematchPairs");
            if (onRematchPairsMethod != null)
            {
                harmony.Patch(
                    original: onRematchPairsMethod,
                    prefix: new HarmonyMethod(typeof(Patches.DispenserComponent_OnRematchPairs_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 DispenserComponent.OnRematchPairs 应用补丁（跳过虚拟配送器）。");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 未找到 DispenserComponent.OnRematchPairs 方法！");
            }

            // Patch 7: BattleBaseComponent.InternalUpdate - 监控基站物品变化（包括手动放入）
            var internalUpdateMethod = AccessTools.Method(typeof(BattleBaseComponent), "InternalUpdate");
            if (internalUpdateMethod != null)
            {
                harmony.Patch(
                    original: internalUpdateMethod,
                    postfix: new HarmonyMethod(typeof(Patches.BattleBaseComponent_InternalUpdate_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 BattleBaseComponent.InternalUpdate 应用补丁（监控物品变化，包括手动放入）。");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 未找到 BattleBaseComponent.InternalUpdate 方法！");
            }

        // Patch 8: UIControlPanelWindow.DetermineFilterResults - 完全隐藏虚拟配送器（方案A）
        var determineFilterResultsMethod = AccessTools.Method(typeof(UIControlPanelWindow), "DetermineFilterResults");
        if (determineFilterResultsMethod != null)
        {
            harmony.Patch(
                original: determineFilterResultsMethod,
                postfix: new HarmonyMethod(typeof(Patches.UIControlPanelWindow_DetermineFilterResults_Patch), "Postfix")
            );
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 UIControlPanelWindow.DetermineFilterResults 应用补丁（方案A：完全隐藏虚拟配送器）。");
        }
        else
        {
            Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 未找到 UIControlPanelWindow.DetermineFilterResults 方法！");
        }

        // Patch 9: UIControlPanelDispenserEntry.OnSetTarget - 双重保险：在 UI 层拦截虚拟配送器
        var onSetTargetMethod = AccessTools.Method(typeof(UIControlPanelDispenserEntry), "OnSetTarget");
        if (onSetTargetMethod != null)
        {
            harmony.Patch(
                original: onSetTargetMethod,
                prefix: new HarmonyMethod(typeof(Patches.UIControlPanelDispenserEntry_OnSetTarget_Safety_Patch), "Prefix")
            );
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 UIControlPanelDispenserEntry.OnSetTarget 应用补丁（双重保险：拦截虚拟配送器）。");
        }
        else
        {
            Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 未找到 UIControlPanelDispenserEntry.OnSetTarget 方法！");
        }

        Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 加载完成！使用虚拟配送器方案（双重保险：数据源过滤 + UI 层拦截）。");
        }
    }
}
