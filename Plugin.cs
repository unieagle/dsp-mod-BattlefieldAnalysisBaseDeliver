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
        public static ConfigEntry<int> BattleBaseCourierCount = null!;
        public static ConfigEntry<float> BattleBaseCourierSpeedMultiplier = null!;

        /// <summary>
        /// 调试日志开关：由配置文件控制
        /// </summary>
        public static bool DebugLog() => EnableDebugLog?.Value ?? false;

        /// <summary>
        /// 每个基站的无人机总数（已限制在 1～200 范围内）
        /// </summary>
        public static int GetBattleBaseCourierCount()
        {
            int v = BattleBaseCourierCount?.Value ?? 20;
            if (v < 1) return 1;
            if (v > 200) return 200;
            return v;
        }

        /// <summary>
        /// 基站无人机速度倍率（在游戏物流速度基础上的倍数，已限制在 0.1～10 范围内）
        /// </summary>
        public static float GetBattleBaseCourierSpeedMultiplier()
        {
            float v = BattleBaseCourierSpeedMultiplier?.Value ?? 2f;
            if (v < 0.1f) return 0.1f;
            if (v > 10f) return 10f;
            return v;
        }

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 正在加载... (GUID: {PluginInfo.PLUGIN_GUID})");

            EnableDebugLog = Config.Bind(
                "General",
                "EnableDebugLog",
                false,
                "为 true 时在日志中输出详细的调试信息，用于排查问题。正常使用时建议设置为 false。");

            BattleBaseCourierCount = Config.Bind(
                "General",
                "BattleBaseCourierCount",
                20,
                new BepInEx.Configuration.ConfigDescription(
                    "每个战场基站的无人机总数（1～200，默认 20）。修改后需重新进入存档或重新加载星球后生效。",
                    new BepInEx.Configuration.AcceptableValueRange<int>(1, 200)));

            BattleBaseCourierSpeedMultiplier = Config.Bind(
                "General",
                "BattleBaseCourierSpeedMultiplier",
                2f,
                new BepInEx.Configuration.ConfigDescription(
                    "基站无人机速度倍率（在游戏物流速度基础上的倍数，0.1～10，默认 2.0）。",
                    new BepInEx.Configuration.AcceptableValueRange<float>(0.1f, 10f)));

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

            // Patch 2.1: BattleBaseComponent.AutoPickTrash - 原版逻辑基础上，部分拾取后给该堆额外寿命
            var autoPickTrashMethod = AccessTools.Method(typeof(BattleBaseComponent), "AutoPickTrash");
            if (autoPickTrashMethod != null)
            {
                harmony.Patch(
                    original: autoPickTrashMethod,
                    prefix: new HarmonyMethod(typeof(Patches.BattleBaseComponent_AutoPickTrashLifeExtend_Patch), "Prefix"),
                    postfix: new HarmonyMethod(typeof(Patches.BattleBaseComponent_AutoPickTrashLifeExtend_Patch), "Postfix")
                );
                Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✓ BattleBaseComponent.AutoPickTrash 补丁已应用（部分拾取+30秒寿命）");
            }
            else
            {
                Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠ 未找到 BattleBaseComponent.AutoPickTrash 方法！");
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
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 战场基站无人机数量: {GetBattleBaseCourierCount()}，速度倍率: {GetBattleBaseCourierSpeedMultiplier()}（可在配置中修改）");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 无需虚拟配送器，性能优化");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 💾 存档安全，自动兼容旧方案");
            Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ========================================");
        }
    }
}
