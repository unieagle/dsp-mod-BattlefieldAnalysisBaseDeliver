using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 渲染基站的无人机
    /// 在游戏的无人机渲染数组中追加基站的无人机数据
    /// </summary>
    [HarmonyPatch(typeof(LogisticCourierRenderer), "Update")]
    public static class LogisticCourierRenderer_Update_Patch
    {
        [HarmonyPostfix]
        static void Postfix(LogisticCourierRenderer __instance)
        {
            try
            {
                if (__instance == null) return;

                // 获取 transport
                var transportField = typeof(LogisticCourierRenderer).GetField("transport", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (transportField == null) return;

                PlanetTransport? transport = transportField.GetValue(__instance) as PlanetTransport;
                if (transport?.factory == null) return;

                int planetId = transport.factory.planetId;

                // 获取当前已收集的无人机数量
                var courierCountField = typeof(LogisticCourierRenderer).GetField("courierCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (courierCountField == null) return;

                int currentCount = (int)courierCountField.GetValue(__instance)!;

                // 获取渲染数组
                var couriersArrField = typeof(LogisticCourierRenderer).GetField("couriersArr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (couriersArrField == null) return;

                CourierData[]? couriersArr = couriersArrField.GetValue(__instance) as CourierData[];
                if (couriersArr == null) return;

                // 获取所有基站的物流系统
                var baseLogistics = BattleBaseLogisticsManager.GetAllForPlanet(planetId);
                
                int addedCouriers = 0;

                foreach (var logistics in baseLogistics)
                {
                    // 复制基站的无人机到渲染数组
                    for (int i = 0; i < logistics.couriers.Length; i++)
                    {
                        ref CourierData courier = ref logistics.couriers[i];

                        // 跳过空闲的无人机
                        if (courier.maxt <= 0f)
                            continue;
                        
                        addedCouriers++;

                        // 检查数组容量
                        if (couriersArr == null || currentCount >= couriersArr.Length)
                        {
                            // 扩展数组
                            ExpandCouriersArray(__instance);
                            couriersArr = couriersArrField.GetValue(__instance) as CourierData[];
                            if (couriersArr == null) break;
                        }

                        // 复制无人机数据
                        couriersArr[currentCount] = courier;
                        currentCount++;
                    }
                }

                // 更新计数
                courierCountField.SetValue(__instance, currentCount);

                // 更新 GPU 缓冲区
                UpdateBuffer(__instance, currentCount);
                
                // 调试日志
                if (addedCouriers > 0 && Plugin.DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🎨 渲染无人机: 行星[{planetId}] 基站无人机数={addedCouriers} 总无人机数={currentCount}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] LogisticCourierRenderer.Update Postfix 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 扩展渲染数组容量
        /// </summary>
        private static void ExpandCouriersArray(LogisticCourierRenderer renderer)
        {
            try
            {
                var expand2xMethod = typeof(LogisticCourierRenderer).GetMethod("Expand2x", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (expand2xMethod != null)
                {
                    expand2xMethod.Invoke(renderer, null);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ExpandCouriersArray 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新 GPU 缓冲区
        /// </summary>
        private static void UpdateBuffer(LogisticCourierRenderer renderer, int count)
        {
            try
            {
                var couriersBufferField = typeof(LogisticCourierRenderer).GetField("couriersBuffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var couriersArrField = typeof(LogisticCourierRenderer).GetField("couriersArr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (couriersBufferField == null || couriersArrField == null) return;

                UnityEngine.ComputeBuffer? buffer = couriersBufferField.GetValue(renderer) as UnityEngine.ComputeBuffer;
                CourierData[]? arr = couriersArrField.GetValue(renderer) as CourierData[];

                if (buffer != null && arr != null && count > 0)
                {
                    buffer.SetData(arr, 0, 0, count);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] UpdateBuffer 异常: {ex.Message}");
            }
        }
    }
}
