using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 战场基站物流核心：派遣无人机、更新飞行、送货处理
    /// </summary>
    [HarmonyPatch(typeof(BattleBaseComponent), "InternalUpdate")]
    public static class BattleBaseComponent_InternalUpdate_Patch
    {
        [HarmonyPostfix]
        static void Postfix(BattleBaseComponent __instance, PlanetFactory factory)
        {
            try
            {
                if (__instance == null || factory == null) return;
                if (__instance.id <= 0 || __instance.entityId <= 0) return;

                int battleBaseId = __instance.id;
                int planetId = factory.planetId;

                // 获取或创建物流系统
                var logistics = BattleBaseLogisticsManager.GetOrCreate(planetId, battleBaseId);

                // 更新所有飞行中的无人机
                UpdateCouriers(logistics, __instance, factory);

                // 冷却计数
                logistics.cooldownCounter++;
                if (logistics.cooldownCounter < BaseLogisticSystem.DISPATCH_INTERVAL)
                    return;

                logistics.cooldownCounter = 0;

                // 获取当前库存
                var currentInventory = BattleBaseLogisticsManager.GetBaseInventory(__instance);

                // 检测库存是否变化
                if (!BattleBaseLogisticsManager.HasInventoryChanged(logistics, currentInventory))
                    return;

                // 库存变化了，更新记录
                logistics.lastInventory = new Dictionary<int, int>(currentInventory);

                // 如果没有空闲无人机，不派遣
                if (logistics.idleCount <= 0)
                    return;

                // 获取基站位置
                Vector3 basePosition = Vector3.zero;
                if (__instance.entityId < factory.entityPool.Length)
                {
                    basePosition = factory.entityPool[__instance.entityId].pos;
                }

                // 扫描配送器需求
                var demands = BattleBaseLogisticsManager.ScanDispenserDemands(factory, basePosition, currentInventory);

                if (demands.Count == 0)
                    return;

                // 派遣无人机（按优先级）
                int dispatched = 0;
                foreach (var demand in demands)
                {
                    if (logistics.idleCount <= 0)
                        break;

                    // 派遣一个无人机
                    if (DispatchCourier(logistics, __instance, factory, demand, basePosition))
                    {
                        dispatched++;
                        
                        if (Plugin.DebugLog())
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 派遣无人机: 基站[{battleBaseId}] → 配送器[{demand.dispenserId}] 物品={demand.itemId} 紧急度={demand.urgency:F2}");
                        }
                    }
                }

                if (dispatched > 0 && Plugin.DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 基站[{battleBaseId}] 共派遣 {dispatched} 个无人机，剩余空闲={logistics.idleCount}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] BattleBaseComponent.InternalUpdate 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 派遣一个无人机
        /// </summary>
        private static bool DispatchCourier(BaseLogisticSystem logistics, BattleBaseComponent battleBase, PlanetFactory factory, DispenserDemand demand, Vector3 basePosition)
        {
            try
            {
                // 从基站存储取出物品
                int itemId = demand.itemId;
                int maxAmount = 100; // 无人机容量（可以从配置读取）
                int actualAmount = 0;
                int inc = 0;

                if (!TakeItemFromBase(battleBase, itemId, maxAmount, out actualAmount, out inc))
                    return false;

                if (actualAmount <= 0)
                    return false;

                // 计算路径
                Vector3 targetPosition = demand.position;
                float distance = Vector3.Distance(basePosition, targetPosition);

                // 找到一个空闲无人机
                int courierIndex = -1;
                for (int i = 0; i < logistics.couriers.Length; i++)
                {
                    if (logistics.couriers[i].maxt <= 0f) // 空闲标志
                    {
                        courierIndex = i;
                        break;
                    }
                }

                if (courierIndex < 0)
                {
                    // 没有空闲无人机，返还物品
                    ReturnItemToBase(battleBase, itemId, actualAmount, inc);
                    return false;
                }

                // 设置无人机数据
                logistics.couriers[courierIndex] = new CourierData
                {
                    begin = basePosition,
                    end = targetPosition,
                    endId = demand.dispenserId,  // 存储目标配送器ID
                    direction = 1f,              // 1 = 去，-1 = 回
                    maxt = distance,
                    t = 0f,
                    itemId = itemId,
                    itemCount = actualAmount,
                    inc = inc,
                    gene = courierIndex
                };

                logistics.idleCount--;
                logistics.workingCount++;

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DispatchCourier 异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新所有飞行中的无人机
        /// </summary>
        private static void UpdateCouriers(BaseLogisticSystem logistics, BattleBaseComponent battleBase, PlanetFactory factory)
        {
            try
            {
                if (logistics.workingCount <= 0)
                    return;

                float courierSpeed = GameMain.history.logisticCourierSpeedModified;
                float deltaT = courierSpeed * 0.016666668f; // 1帧的移动距离

                for (int i = 0; i < logistics.couriers.Length; i++)
                {
                    ref CourierData courier = ref logistics.couriers[i];

                    if (courier.maxt <= 0f) // 空闲
                        continue;

                    // 更新位置
                    courier.t += deltaT * courier.direction;

                    // 检查是否到达目标点（去程）
                    if (courier.direction > 0f && courier.t >= courier.maxt)
                    {
                        courier.t = courier.maxt;

                        // 送货到配送器
                        if (DeliverToDispenser(factory, courier.endId, courier.itemId, courier.itemCount, courier.inc))
                        {
                            if (Plugin.DebugLog())
                            {
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📬 送货成功: 配送器[{courier.endId}] 物品={courier.itemId}x{courier.itemCount}");
                            }
                        }

                        // 清空货物，准备返回
                        courier.itemId = 0;
                        courier.itemCount = 0;
                        courier.inc = 0;
                        courier.direction = -1f;
                    }
                    // 检查是否返回基站（回程）
                    else if (courier.direction < 0f && courier.t <= 0f)
                    {
                        courier.t = 0f;

                        // 回收无人机
                        courier.maxt = 0f; // 标记为空闲
                        courier.begin = Vector3.zero;
                        courier.end = Vector3.zero;
                        courier.endId = 0;
                        courier.direction = 0f;

                        logistics.workingCount--;
                        logistics.idleCount++;

                        if (Plugin.DebugLog())
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🏠 无人机返回: 基站[{battleBase.id}] 空闲={logistics.idleCount}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] UpdateCouriers 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 从基站取出物品
        /// </summary>
        private static bool TakeItemFromBase(BattleBaseComponent battleBase, int itemId, int maxCount, out int actualCount, out int inc)
        {
            actualCount = 0;
            inc = 0;

            try
            {
                if (battleBase.storage == null) return false;

                var takeItemMethod = battleBase.storage.GetType().GetMethod("TakeItem", BindingFlags.Public | BindingFlags.Instance);
                if (takeItemMethod == null) return false;

                object[] takeItemParams = new object[] { itemId, maxCount, 0 };
                object? takeResult = takeItemMethod.Invoke(battleBase.storage, takeItemParams);

                if (takeResult == null) return false;

                actualCount = (int)takeResult;
                inc = (int)takeItemParams[2];

                return actualCount > 0;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] TakeItemFromBase 异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 返还物品到基站
        /// </summary>
        private static void ReturnItemToBase(BattleBaseComponent battleBase, int itemId, int count, int inc)
        {
            try
            {
                if (battleBase.storage == null) return;

                var addItemMethod = battleBase.storage.GetType().GetMethod("AddItem", 
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(int), typeof(int), typeof(int) },
                    null);

                if (addItemMethod == null) return;

                addItemMethod.Invoke(battleBase.storage, new object[] { itemId, count, inc });
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ReturnItemToBase 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 送货到配送器
        /// </summary>
        private static bool DeliverToDispenser(PlanetFactory factory, int dispenserId, int itemId, int count, int inc)
        {
            try
            {
                if (factory?.transport == null) return false;

                var dispenserPoolField = factory.transport.GetType().GetField("dispenserPool",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (dispenserPoolField == null) return false;

                Array? dispenserPool = dispenserPoolField.GetValue(factory.transport) as Array;
                if (dispenserPool == null || dispenserId <= 0 || dispenserId >= dispenserPool.Length)
                    return false;

                object? dispenserObj = dispenserPool.GetValue(dispenserId);
                DispenserComponent? dispenser = dispenserObj as DispenserComponent;
                if (dispenser == null || dispenser.id != dispenserId)
                    return false;

                // 获取配送器的底部存储ID
                if (dispenser.storage?.bottomStorage == null) return false;

                var storageIdField = dispenser.storage.bottomStorage.GetType().GetField("id");
                if (storageIdField == null) return false;

                int storageId = (int)storageIdField.GetValue(dispenser.storage.bottomStorage)!;

                // 插入到配送器存储
                int inserted = factory.InsertIntoStorage(storageId, itemId, count, inc, out int _, true);

                if (inserted > 0)
                {
                    // 触发配送器的脉冲信号（视觉反馈）
                    dispenser.pulseSignal = 2;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DeliverToDispenser 异常: {ex.Message}");
                return false;
            }
        }
    }
}
