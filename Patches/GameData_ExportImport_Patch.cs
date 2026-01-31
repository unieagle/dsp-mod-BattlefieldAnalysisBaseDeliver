using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 存档时返还所有在途物品，加载后自动重新派遣
    /// </summary>
    [HarmonyPatch(typeof(GameData), "Export")]
    public static class GameData_Export_Patch
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            try
            {
                if (Plugin.DebugLog())
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 💾 存档开始：返还所有基站在途物品");

                int totalReturned = 0;
                int totalCouriers = 0;

                // 遍历所有星球
                if (GameMain.data?.factories == null) return;

                foreach (var factory in GameMain.data.factories)
                {
                    if (factory == null) continue;

                    int planetId = factory.planetId;
                    var baseLogistics = BattleBaseLogisticsManager.GetAllForPlanet(planetId);

                    foreach (var logistics in baseLogistics)
                    {
                        // 返还所有在途物品
                        for (int i = 0; i < logistics.couriers.Length; i++)
                        {
                            ref CourierData courier = ref logistics.couriers[i];

                            if (courier.maxt <= 0f) // 空闲
                                continue;

                            totalCouriers++;

                            // 如果无人机携带物品，返还到基站
                            if (courier.itemId > 0 && courier.itemCount > 0)
                            {
                                if (ReturnItemToBase(factory, logistics.battleBaseId, courier.itemId, courier.itemCount, courier.inc))
                                {
                                    totalReturned++;
                                    
                                    if (Plugin.DebugLog())
                                    {
                                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 返还物品: 基站[{logistics.battleBaseId}] 物品={courier.itemId}x{courier.itemCount}");
                                    }
                                }
                            }

                            // 清空无人机
                            courier.maxt = 0f;
                            courier.begin = UnityEngine.Vector3.zero;
                            courier.end = UnityEngine.Vector3.zero;
                            courier.endId = 0;
                            courier.direction = 0f;
                            courier.t = 0f;
                            courier.itemId = 0;
                            courier.itemCount = 0;
                            courier.inc = 0;
                        }

                        // 重置计数
                        logistics.workingCount = 0;
                        logistics.idleCount = 10;
                    }
                }

                if (Plugin.DebugLog())
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 存档准备完成：回收 {totalCouriers} 个无人机，返还 {totalReturned} 批物品");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] GameData.Export Prefix 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 返还物品到基站
        /// </summary>
        private static bool ReturnItemToBase(PlanetFactory factory, int battleBaseId, int itemId, int count, int inc)
        {
            try
            {
                var defenseSystem = factory?.defenseSystem;
                if (defenseSystem == null) return false;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null) return false;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return false;

                var bufferField = battleBasesPool.GetType().GetField("buffer",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null) return false;

                Array? battleBases = bufferField.GetValue(battleBasesPool) as Array;
                if (battleBases == null || battleBaseId <= 0 || battleBaseId >= battleBases.Length)
                    return false;

                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null) return false;

                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                if (storage == null) return false;

                var addItemMethod = storage.GetType().GetMethod("AddItem",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(int), typeof(int), typeof(int) },
                    null);

                if (addItemMethod == null) return false;

                addItemMethod.Invoke(storage, new object[] { itemId, count, inc });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 加载存档后，基站会自动检测库存并重新派遣
    /// </summary>
    [HarmonyPatch(typeof(GameData), "Import")]
    public static class GameData_Import_Patch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            try
            {
                if (Plugin.DebugLog())
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📂 存档加载完成：基站将自动重新派遣无人机");

                // 不需要手动触发，InternalUpdate 会自动检测库存变化并派遣
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] GameData.Import Postfix 异常: {ex.Message}");
            }
        }
    }
}
