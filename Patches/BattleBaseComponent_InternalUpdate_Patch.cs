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

                // 由 Manager 统一判断：cooldown、库存变化跳过、无空闲/无需求等；仅在本帧应派遣时返回上下文
                var ctx = BattleBaseLogisticsManager.TryGetDispatchContext(logistics, __instance, factory, __instance.entityId);
                if (ctx == null)
                    return;

                // 派遣无人机（按优先级）
                int dispatched = 0;
                foreach (var demand in ctx.Demands)
                {
                    if (logistics.idleCount <= 0)
                        break;

                    if (DispatchCourier(logistics, __instance, factory, demand, ctx.BasePosition, ctx.CurrentInventory))
                        dispatched++;
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
        private static bool DispatchCourier(BaseLogisticSystem logistics, BattleBaseComponent battleBase, PlanetFactory factory, DispenserDemand demand, Vector3 basePosition, Dictionary<int, int> currentInventory)
        {
            try
            {
                // 从基站存储取出物品
                int itemId = demand.itemId;
                int maxAmount = 100; // 无人机容量（可以从配置读取）
                int beforeAmount = currentInventory.ContainsKey(itemId) ? currentInventory[itemId] : 0;
                int actualAmount = 0;
                int inc = 0;

                if (!TakeItemFromBase(battleBase, itemId, maxAmount, out actualAmount, out inc))
                    return false;

                if (actualAmount <= 0)
                    return false;

                int afterAmount = beforeAmount - actualAmount;

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

                // 打印派遣日志
                if (Plugin.DebugLog())
                {
                    string itemName = GetItemName(itemId);
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 派遣无人机: 基站[{battleBase.id}] → 配送器[{demand.dispenserId}] 物品={itemName}(ID:{itemId}) 派遣={actualAmount} 剩余={afterAmount} 紧急度={demand.urgency:F2}");
                }

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
                        bool delivered = DeliverToDispenser(factory, courier.endId, courier.itemId, courier.itemCount, courier.inc);
                        
                        if (delivered)
                        {
                            if (Plugin.DebugLog())
                            {
                                string itemName = GetItemName(courier.itemId);
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📬 送货成功: 配送器[{courier.endId}] 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}");
                            }
                            
                            // 清空货物，准备返回
                            courier.itemId = 0;
                            courier.itemCount = 0;
                            courier.inc = 0;
                        }
                        else
                        {
                            // 送货失败，记录日志（物品保留，返回基站时退还）
                            if (Plugin.DebugLog())
                            {
                                string itemName = GetItemName(courier.itemId);
                                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 送货失败: 配送器[{courier.endId}] 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}，将返还到基站");
                            }
                        }
                        
                        // 准备返回
                        courier.direction = -1f;
                    }
                    // 检查是否返回基站（回程）
                    else if (courier.direction < 0f && courier.t <= 0f)
                    {
                        courier.t = 0f;

                        // 如果无人机还携带物品，返还到基站
                        if (courier.itemId > 0 && courier.itemCount > 0)
                        {
                            ReturnItemToBase(battleBase, courier.itemId, courier.itemCount, courier.inc);
                            
                            if (Plugin.DebugLog())
                            {
                                string itemName = GetItemName(courier.itemId);
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 返还物品: 基站[{battleBase.id}] 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}");
                            }
                        }

                        // 回收无人机
                        courier.maxt = 0f; // 标记为空闲
                        courier.begin = Vector3.zero;
                        courier.end = Vector3.zero;
                        courier.endId = 0;
                        courier.direction = 0f;
                        courier.itemId = 0;
                        courier.itemCount = 0;
                        courier.inc = 0;

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
                if (factory?.transport == null)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: factory.transport 为 null");
                    return false;
                }

                var dispenserPoolField = factory.transport.GetType().GetField("dispenserPool",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (dispenserPoolField == null)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: dispenserPoolField 为 null");
                    return false;
                }

                Array? dispenserPool = dispenserPoolField.GetValue(factory.transport) as Array;
                if (dispenserPool == null || dispenserId <= 0 || dispenserId >= dispenserPool.Length)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: dispenserId={dispenserId} 无效（范围: 1-{dispenserPool?.Length ?? 0}）");
                    return false;
                }

                object? dispenserObj = dispenserPool.GetValue(dispenserId);
                DispenserComponent? dispenser = dispenserObj as DispenserComponent;
                if (dispenser == null || dispenser.id != dispenserId)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: dispenser[{dispenserId}] 为 null 或 id 不匹配");
                    return false;
                }

                // 获取配送器的底部存储ID
                if (dispenser.storage?.bottomStorage == null)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: dispenser[{dispenserId}].storage.bottomStorage 为 null");
                    return false;
                }

                var storageIdField = dispenser.storage.bottomStorage.GetType().GetField("id");
                if (storageIdField == null)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: storageIdField 为 null");
                    return false;
                }

                int storageId = (int)storageIdField.GetValue(dispenser.storage.bottomStorage)!;

                // 插入到配送器存储
                int inserted = factory.InsertIntoStorage(storageId, itemId, count, inc, out int incOut, true);
                int remaining = count - inserted;

                // 如果有物品未能插入，放到 holdupPackage 中（模拟游戏逻辑）
                if (remaining > 0)
                {
                    var holdupPackageField = dispenser.GetType().GetField("holdupPackage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var holdupItemCountField = dispenser.GetType().GetField("holdupItemCount", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (holdupPackageField != null && holdupItemCountField != null)
                    {
                        Array? holdupPackage = holdupPackageField.GetValue(dispenser) as Array;
                        int holdupItemCount = (int)holdupItemCountField.GetValue(dispenser)!;
                        
                        if (holdupPackage != null && holdupItemCount < holdupPackage.Length)
                        {
                            // 查找是否已有该物品
                            bool found = false;
                            for (int i = 0; i < holdupItemCount; i++)
                            {
                                object? item = holdupPackage.GetValue(i);
                                if (item != null)
                                {
                                    var itemIdField = item.GetType().GetField("itemId");
                                    if (itemIdField != null && (int)itemIdField.GetValue(item)! == itemId)
                                    {
                                        // 找到相同物品，增加数量
                                        var countField = item.GetType().GetField("count");
                                        var incField = item.GetType().GetField("inc");
                                        if (countField != null && incField != null)
                                        {
                                            int oldCount = (int)countField.GetValue(item)!;
                                            int oldInc = (int)incField.GetValue(item)!;
                                            countField.SetValue(item, oldCount + remaining);
                                            incField.SetValue(item, oldInc + incOut);
                                            holdupPackage.SetValue(item, i);
                                            found = true;
                                            
                                            if (Plugin.DebugLog())
                                            {
                                                string itemName = GetItemName(itemId);
                                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 送货到缓存区: 配送器[{dispenserId}] 物品={itemName}(ID:{itemId}) 直接插入={inserted} 缓存={remaining}");
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            // 如果没找到，添加新物品
                            if (!found)
                            {
                                // 创建 DispenserStore 结构体
                                var dispenserStoreType = holdupPackage.GetType().GetElementType();
                                if (dispenserStoreType != null)
                                {
                                    object newItem = Activator.CreateInstance(dispenserStoreType)!;
                                    var itemIdField = newItem.GetType().GetField("itemId");
                                    var countField = newItem.GetType().GetField("count");
                                    var incField = newItem.GetType().GetField("inc");
                                    
                                    if (itemIdField != null && countField != null && incField != null)
                                    {
                                        itemIdField.SetValue(newItem, itemId);
                                        countField.SetValue(newItem, remaining);
                                        incField.SetValue(newItem, incOut);
                                        holdupPackage.SetValue(newItem, holdupItemCount);
                                        holdupItemCountField.SetValue(dispenser, holdupItemCount + 1);
                                        
                                        if (Plugin.DebugLog())
                                        {
                                            string itemName = GetItemName(itemId);
                                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 送货到缓存区（新增）: 配送器[{dispenserId}] 物品={itemName}(ID:{itemId}) 直接插入={inserted} 缓存={remaining}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (Plugin.DebugLog())
                {
                    string itemName = GetItemName(itemId);
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📬 送货成功（直接插入）: 配送器[{dispenserId}] 物品={itemName}(ID:{itemId})x{count}");
                }

                // 触发配送器的脉冲信号（视觉反馈）
                dispenser.pulseSignal = 2;
                
                // 无论如何都返回 true，因为物品已经交给配送器了（直接插入或放到缓存）
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DeliverToDispenser 异常: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 获取物品名称
        /// </summary>
        private static string GetItemName(int itemId)
        {
            try
            {
                var itemProto = LDB.items.Select(itemId);
                if (itemProto != null && !string.IsNullOrEmpty(itemProto.name))
                {
                    return itemProto.name.Translate();
                }
            }
            catch
            {
                // 忽略异常
            }
            return $"item_{itemId}";
        }
    }
}
