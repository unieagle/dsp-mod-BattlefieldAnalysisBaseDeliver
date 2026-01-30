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

            // ✅ 新方案：直接从 BattleBaseComponent 读取物品，不创建 StationComponent
            
            // Patch 1: PlanetTransport.RefreshDispenserTraffic - 添加配对
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
            
        // Patch 2: DispenserComponent.InternalTick - 方案C：Prefix 派出空载无人机
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

            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 加载完成！战场分析基站 protoId = {Patches.BattlefieldBaseHelper.BattlefieldAnalysisBaseProtoId}");
        }
    }
}
