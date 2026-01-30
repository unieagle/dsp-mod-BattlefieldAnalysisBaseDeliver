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
                true,
                "为 true 时在日志中输出诊断信息，用于排查功能不生效的原因。默认 true。");

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

            // Patch 8-10: UIControlPanel 相关 - 跳过虚拟配送器的UI显示
            var uiDispenserOnSetTargetMethod = AccessTools.Method(typeof(UIControlPanelDispenserEntry), "OnSetTarget");
            if (uiDispenserOnSetTargetMethod != null)
            {
                harmony.Patch(
                    original: uiDispenserOnSetTargetMethod,
                    prefix: new HarmonyMethod(typeof(Patches.UIControlPanelDispenserEntry_OnSetTarget_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 UIControlPanelDispenserEntry.OnSetTarget 应用补丁（跳过虚拟配送器）。");
            }

            var uiObjectInitMethod = AccessTools.Method(typeof(UIControlPanelObjectEntry), "InitFromPool");
            if (uiObjectInitMethod != null)
            {
                harmony.Patch(
                    original: uiObjectInitMethod,
                    prefix: new HarmonyMethod(typeof(Patches.UIControlPanelObjectEntry_InitFromPool_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 UIControlPanelObjectEntry.InitFromPool 应用补丁（跳过虚拟配送器）。");
            }

            var uiWindowTakeMethod = AccessTools.Method(typeof(UIControlPanelWindow), "TakeObjectEntryFromPool");
            if (uiWindowTakeMethod != null)
            {
                harmony.Patch(
                    original: uiWindowTakeMethod,
                    prefix: new HarmonyMethod(typeof(Patches.UIControlPanelWindow_TakeObjectEntryFromPool_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已对 UIControlPanelWindow.TakeObjectEntryFromPool 应用补丁（跳过虚拟配送器）。");
            }

            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 加载完成！使用虚拟配送器方案。");
        }
    }
}
