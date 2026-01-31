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

        /// <summary>
        /// 调试日志开关：由配置文件控制
        /// </summary>
        public static bool DebugLog() => EnableDebugLog?.Value ?? false;

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

            // ========== 基站直接派遣方案 ==========
            
            // Patch 1: PlanetFactory 生命周期管理
            var planetFactoryInitMethod = AccessTools.Method(typeof(PlanetFactory), "Init");
            if (planetFactoryInitMethod != null)
            {
                harmony.Patch(
                    original: planetFactoryInitMethod,
                    postfix: new HarmonyMethod(typeof(Patches.PlanetFactory_Init_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ PlanetFactory.Init 补丁已应用");
            }

            var planetFactoryImportMethod = AccessTools.Method(typeof(PlanetFactory), "Import");
            if (planetFactoryImportMethod != null)
            {
                harmony.Patch(
                    original: planetFactoryImportMethod,
                    postfix: new HarmonyMethod(typeof(Patches.PlanetFactory_Import_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ PlanetFactory.Import 补丁已应用（清理旧虚拟配送器）");
            }

            var planetFactoryFreeMethod = AccessTools.Method(typeof(PlanetFactory), "Free");
            if (planetFactoryFreeMethod != null)
            {
                harmony.Patch(
                    original: planetFactoryFreeMethod,
                    prefix: new HarmonyMethod(typeof(Patches.PlanetFactory_Free_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ PlanetFactory.Free 补丁已应用");
            }
            
            // Patch 2: BattleBaseComponent.InternalUpdate - 核心：派遣、飞行、送货
            var internalUpdateMethod = AccessTools.Method(typeof(BattleBaseComponent), "InternalUpdate");
            if (internalUpdateMethod != null)
            {
                harmony.Patch(
                    original: internalUpdateMethod,
                    postfix: new HarmonyMethod(typeof(Patches.BattleBaseComponent_InternalUpdate_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ BattleBaseComponent.InternalUpdate 补丁已应用（核心逻辑）");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠ 未找到 BattleBaseComponent.InternalUpdate 方法！");
            }

            // Patch 3: LogisticCourierRenderer.Update - 渲染基站派遣的无人机
            var rendererUpdateMethod = AccessTools.Method(typeof(LogisticCourierRenderer), "Update");
            if (rendererUpdateMethod != null)
            {
                harmony.Patch(
                    original: rendererUpdateMethod,
                    postfix: new HarmonyMethod(typeof(Patches.LogisticCourierRenderer_Update_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ LogisticCourierRenderer.Update 补丁已应用（无人机可见）");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠ 未找到 LogisticCourierRenderer.Update 方法！");
            }

            // Patch 4: GameData.Export - 存档前返还在途物品
            var gameDataExportMethod = AccessTools.Method(typeof(GameData), "Export");
            if (gameDataExportMethod != null)
            {
                harmony.Patch(
                    original: gameDataExportMethod,
                    prefix: new HarmonyMethod(typeof(Patches.GameData_Export_Patch), "Prefix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ GameData.Export 补丁已应用（存档安全）");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠ 未找到 GameData.Export 方法！");
            }

            // Patch 5: GameData.Import - 存档加载后清理数据
            var gameDataImportMethod = AccessTools.Method(typeof(GameData), "Import");
            if (gameDataImportMethod != null)
            {
                harmony.Patch(
                    original: gameDataImportMethod,
                    postfix: new HarmonyMethod(typeof(Patches.GameData_Import_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ GameData.Import 补丁已应用（自动重新派遣）");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠ 未找到 GameData.Import 方法！");
            }

            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ========================================");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 加载完成！基站直接派遣方案");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 战场基站拥有独立的10个无人机");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 无需虚拟配送器，性能优化");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 💾 存档安全，自动兼容旧方案");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ========================================");
        }
    }
}
