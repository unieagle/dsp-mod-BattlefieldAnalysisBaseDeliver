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
        /// <summary> 物流塔普通槽位目标时 endId = STATION_ENDID_OFFSET + stationId </summary>
        private const int STATION_ENDID_OFFSET = 20000;
        /// <summary> 物流塔翘曲器小格目标时 endId = STATION_WARPER_STORAGE_ENDID_OFFSET + stationId，与普通槽位区分以避免误改 localOrder </summary>
        private const int STATION_WARPER_STORAGE_ENDID_OFFSET = 30000;
        /// <summary> 拉货任务：去供应配送器取货，endId = SUPPLY_FETCH_ENDID_OFFSET + dispenserId </summary>
        private const int SUPPLY_FETCH_ENDID_OFFSET = 40000;

        private const int COURIER_CAPACITY = 100;

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
                if (demand.IsSupplyFetch)
                    return DispatchSupplyFetchCourier(logistics, battleBase, factory, demand, basePosition);

                int itemId = demand.itemId;
                int maxAmount = COURIER_CAPACITY;
                if (demand.needCount > 0 && demand.needCount < maxAmount)
                    maxAmount = demand.needCount;

                int beforeAmount = currentInventory.ContainsKey(itemId) ? currentInventory[itemId] : 0;
                int actualAmount = 0;
                int inc = 0;

                if (!TakeItemFromBase(battleBase, itemId, maxAmount, out actualAmount, out inc))
                    return false;

                if (actualAmount <= 0)
                    return false;

                int afterAmount = beforeAmount - actualAmount;

                Vector3 targetPosition = demand.position;
                float distance = Vector3.Distance(basePosition, targetPosition);

                int courierIndex = -1;
                for (int i = 0; i < logistics.couriers.Length; i++)
                {
                    if (logistics.couriers[i].maxt <= 0f)
                    {
                        courierIndex = i;
                        break;
                    }
                }

                if (courierIndex < 0)
                {
                    ReturnItemToBase(battleBase, itemId, actualAmount, inc);
                    return false;
                }

                int endId = demand.IsMechaSlot ? -(demand.slotIndex + 1)
                    : (demand.IsStationTower ? ((demand.IsWarperStorageDemand ? STATION_WARPER_STORAGE_ENDID_OFFSET : STATION_ENDID_OFFSET) + demand.stationId)
                    : demand.dispenserId);
                float maxt = distance;
                Vector3 beginPos = basePosition;
                Vector3 endPos = targetPosition;
                if (demand.IsMechaSlot)
                {
                    beginPos = basePosition;
                    endPos = basePosition;
                    maxt = 1f;
                }

                logistics.couriers[courierIndex] = new CourierData
                {
                    begin = beginPos,
                    end = endPos,
                    endId = endId,
                    direction = 1f,
                    maxt = maxt,
                    t = 0f,
                    itemId = itemId,
                    itemCount = actualAmount,
                    inc = inc,
                    gene = courierIndex
                };

                logistics.idleCount--;
                logistics.workingCount++;

                if (demand.IsMechaSlot)
                {
                    var pkg = GameMain.mainPlayer?.deliveryPackage;
                    if (pkg?.grids != null && demand.slotIndex >= 0 && demand.slotIndex < pkg.grids.Length)
                        pkg.grids[demand.slotIndex].ordered += actualAmount;
                }
                else if (!demand.IsStationTower && demand.dispenserId > 0)
                {
                    var targetDispenser = BattleBaseLogisticsManager.GetDispenser(factory, demand.dispenserId);
                    if (targetDispenser != null)
                        targetDispenser.storageOrdered += actualAmount;
                }
                else if (demand.IsStationTower && !demand.IsWarperStorageDemand)
                {
                    var station = factory.transport.GetStationComponent(demand.stationId);
                    if (station?.storage != null && demand.StationStorageIndex >= 0 && demand.StationStorageIndex < station.storage.Length)
                        station.storage[demand.StationStorageIndex].localOrder += actualAmount;
                }

                if (Plugin.DebugLog())
                {
                    var entity = factory.entityPool[battleBase.entityId];
                    Quaternion q = entity.rot;
                    Vector3 euler = q.eulerAngles;
                    string itemName = GetItemName(itemId);
                    string targetDesc = demand.IsMechaSlot ? $"机甲槽位[{demand.slotIndex}]" : (demand.IsStationTower ? $"物流塔[{demand.stationId}]" : $"配送器[{demand.dispenserId}]");
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 派遣: 基站[{battleBase.id}] → {targetDesc} 物品={itemName}(ID:{itemId}) 派遣={actualAmount} 剩余={afterAmount} 紧急度={demand.urgency:F2} | 基站pos=({entity.pos.x:F2},{entity.pos.y:F2},{entity.pos.z:F2}) mag={entity.pos.magnitude:F2} rot=({q.x:F4},{q.y:F4},{q.z:F4},{q.w:F4}) 欧拉=({euler.x:F1},{euler.y:F1},{euler.z:F1})°");
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
        /// 派遣拉货无人机：空手去供应配送器取货，gene 存 reservedAmount 供到达时回退用。
        /// </summary>
        private static bool DispatchSupplyFetchCourier(BaseLogisticSystem logistics, BattleBaseComponent battleBase, PlanetFactory factory, DispenserDemand demand, Vector3 basePosition)
        {
            int reservedAmount = Math.Min(demand.needCount, COURIER_CAPACITY);
            if (reservedAmount <= 0) return false;

            var supplyDispenser = BattleBaseLogisticsManager.GetDispenser(factory, demand.dispenserId);
            if (supplyDispenser == null) return false;
            int stock = BattleBaseLogisticsManager.GetDispenserStockPublic(supplyDispenser, demand.itemId);
            if (stock <= 0) return false;
            reservedAmount = Math.Min(reservedAmount, stock);

            int courierIndex = -1;
            for (int i = 0; i < logistics.couriers.Length; i++)
            {
                if (logistics.couriers[i].maxt <= 0f) { courierIndex = i; break; }
            }
            if (courierIndex < 0) return false;

            supplyDispenser.storageOrdered -= reservedAmount;
            BattleBaseLogisticsManager.AddBaseFetchInFlight(factory.planetId, battleBase.id, demand.itemId, reservedAmount);

            float distance = Vector3.Distance(basePosition, demand.position);
            logistics.couriers[courierIndex] = new CourierData
            {
                begin = basePosition,
                end = demand.position,
                endId = SUPPLY_FETCH_ENDID_OFFSET + demand.dispenserId,
                direction = 1f,
                maxt = distance,
                t = 0f,
                itemId = demand.itemId,
                itemCount = 0,
                inc = 0,
                gene = reservedAmount
            };
            logistics.idleCount--;
            logistics.workingCount++;

            if (Plugin.DebugLog())
            {
                string itemName = GetItemName(demand.itemId);
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 拉货派遣: 基站[{battleBase.id}] → 供应配送器[{demand.dispenserId}] 物品={itemName}(ID:{demand.itemId}) 预留={reservedAmount}");
            }
            return true;
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
                courierSpeed *= Plugin.GetBattleBaseCourierSpeedMultiplier();
                float deltaT = courierSpeed * 0.016666668f; // 1帧的移动距离

                var entity = factory.entityPool[battleBase.entityId];
                Vector3 up = entity.pos.sqrMagnitude < 1E-6f ? Vector3.up : entity.pos.normalized;
                Vector3 basePos = entity.pos + up * BaseLogisticSystem.DRONE_AT_BASE_HEIGHT_OFFSET;
                Vector3? playerPosNullable = GameMain.mainPlayer != null ? GameMain.mainPlayer.position : (Vector3?)null;

                for (int i = 0; i < logistics.couriers.Length; i++)
                {
                    ref CourierData courier = ref logistics.couriers[i];

                    if (courier.maxt <= 0f) // 空闲
                        continue;

                    // 目标为机甲时：与原版一致，每帧用“追踪玩家”逻辑更新 begin/end/t，不按线性 t+=deltaT
                    // 物流塔（endId >= STATION_ENDID_OFFSET）与配送器：线性 t += deltaT
                    if (courier.endId < 0 && courier.direction > 0f && playerPosNullable.HasValue)
                    {
                        UpdateCourierToMecha(ref courier, basePos, playerPosNullable.Value, courierSpeed);
                    }
                    else
                    {
                        courier.t += deltaT * courier.direction;
                    }

                    // 检查是否到达目标点（去程）
                    if (courier.direction > 0f && courier.t >= courier.maxt)
                    {
                        courier.t = courier.maxt;

                        if (courier.endId >= SUPPLY_FETCH_ENDID_OFFSET)
                        {
                            int dispenserId = courier.endId - SUPPLY_FETCH_ENDID_OFFSET;
                            int reservedAmount = courier.gene;
                            TakeItemFromDispenser(factory, dispenserId, courier.itemId, Math.Min(reservedAmount, COURIER_CAPACITY), out int actualCount, out int incOut);
                            var supplyDispenser = BattleBaseLogisticsManager.GetDispenser(factory, dispenserId);
                            if (actualCount > 0)
                            {
                                if (actualCount < reservedAmount && supplyDispenser != null)
                                    supplyDispenser.storageOrdered += (reservedAmount - actualCount);
                                courier.itemCount = actualCount;
                                courier.inc = incOut;
                                courier.direction = -1f;
                            }
                            else
                            {
                                if (supplyDispenser != null)
                                    supplyDispenser.storageOrdered += reservedAmount;
                                BattleBaseLogisticsManager.AddBaseFetchInFlight(factory.planetId, battleBase.id, courier.itemId, -reservedAmount);
                                courier.maxt = 0f;
                                courier.begin = Vector3.zero;
                                courier.end = Vector3.zero;
                                courier.endId = 0;
                                courier.direction = 0f;
                                courier.itemId = 0;
                                courier.itemCount = 0;
                                courier.inc = 0;
                                courier.gene = 0;
                                logistics.workingCount--;
                                logistics.idleCount++;
                            }
                            continue;
                        }

                        bool delivered;
                        if (courier.endId < 0)
                        {
                            // 送往机甲配送栏：slotIndex = -(endId+1)
                            int slotIndex = -(courier.endId + 1);
                            delivered = DeliverToMecha(slotIndex, courier.itemId, courier.itemCount, courier.inc);
                            // 在途数量：到达后从该槽位 ordered 扣减（与派遣时增加对应，无论是否成功放入）
                            var pkg = GameMain.mainPlayer?.deliveryPackage;
                            if (pkg?.grids != null && slotIndex >= 0 && slotIndex < pkg.grids.Length)
                                pkg.grids[slotIndex].ordered -= courier.itemCount;
                            if (Plugin.DebugLog() && delivered)
                            {
                                string itemName = GetItemName(courier.itemId);
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📬 送货成功: 机甲槽位[{slotIndex}] 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}");
                            }
                        }
                        else if (courier.endId >= STATION_ENDID_OFFSET)
                        {
                            bool isWarperStorage = courier.endId >= STATION_WARPER_STORAGE_ENDID_OFFSET;
                            int stationId = isWarperStorage ? courier.endId - STATION_WARPER_STORAGE_ENDID_OFFSET : courier.endId - STATION_ENDID_OFFSET;
                            int originalCount = courier.itemCount;
                            int accepted = DeliverToStation(factory, stationId, courier.itemId, courier.itemCount, courier.inc);
                            delivered = (accepted >= courier.itemCount);
                            // 仅普通槽位需扣减 localOrder（翘曲器小格在派遣时未增加 localOrder）
                            if (!isWarperStorage)
                                BattleBaseLogisticsManager.DecrementStationSlotLocalOrder(factory, stationId, courier.itemId, originalCount);
                            if (accepted > 0)
                            {
                                if (Plugin.DebugLog())
                                {
                                    string itemName = GetItemName(courier.itemId);
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📬 送货成功: 物流塔[{stationId}] 物品={itemName}(ID:{courier.itemId})x{accepted}" + (accepted < courier.itemCount ? $" 剩余{courier.itemCount - accepted}返还" : ""));
                                }
                                if (accepted >= courier.itemCount)
                                {
                                    courier.itemId = 0;
                                    courier.itemCount = 0;
                                    courier.inc = 0;
                                }
                                else
                                {
                                    courier.itemCount -= accepted;
                                    courier.inc = 0;
                                }
                            }
                        }
                        else
                        {
                            delivered = DeliverToDispenser(factory, courier.endId, courier.itemId, courier.itemCount, courier.inc);
                            // 配送器在途：到达后扣减目标 storageOrdered（与派遣时增加对应，无论是否成功放入）
                            var targetDispenser = BattleBaseLogisticsManager.GetDispenser(factory, courier.endId);
                            if (targetDispenser != null)
                                targetDispenser.storageOrdered -= courier.itemCount;
                            if (Plugin.DebugLog() && delivered)
                            {
                                string itemName = GetItemName(courier.itemId);
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📬 送货成功: 配送器[{courier.endId}] 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}");
                            }
                        }

                        if (delivered)
                        {
                            courier.itemId = 0;
                            courier.itemCount = 0;
                            courier.inc = 0;
                        }
                        else
                        {
                            if (Plugin.DebugLog())
                            {
                                string itemName = GetItemName(courier.itemId);
                                string targetDesc = courier.endId < 0 ? $"机甲槽位[{-courier.endId - 1}]"
                                    : (courier.endId >= STATION_WARPER_STORAGE_ENDID_OFFSET ? $"物流塔[{courier.endId - STATION_WARPER_STORAGE_ENDID_OFFSET}]翘曲器小格"
                                    : (courier.endId >= STATION_ENDID_OFFSET ? $"物流塔[{courier.endId - STATION_ENDID_OFFSET}]"
                                    : $"配送器[{courier.endId}]"));
                                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 送货失败: {targetDesc} 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}，将返还到基站");
                            }
                        }

                        courier.direction = -1f;
                    }
                    // 检查是否返回基站（回程）
                    else if (courier.direction < 0f && courier.t <= 0f)
                    {
                        courier.t = 0f;

                        if (courier.endId >= SUPPLY_FETCH_ENDID_OFFSET)
                        {
                            if (courier.itemId > 0 && courier.itemCount > 0)
                            {
                                DeliverToBaseInputPartition(battleBase, factory, courier.itemId, courier.itemCount, courier.inc);
                                BattleBaseLogisticsManager.AddBaseFetchInFlight(factory.planetId, battleBase.id, courier.itemId, -courier.itemCount);
                                var supplyDispenser = BattleBaseLogisticsManager.GetDispenser(factory, courier.endId - SUPPLY_FETCH_ENDID_OFFSET);
                                if (supplyDispenser != null)
                                    supplyDispenser.storageOrdered += courier.itemCount;
                                if (Plugin.DebugLog())
                                {
                                    string itemName = GetItemName(courier.itemId);
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📥 拉货送达: 基站[{battleBase.id}] 输入区 物品={itemName}(ID:{courier.itemId})x{courier.itemCount}");
                                }
                            }
                            courier.maxt = 0f;
                            courier.begin = Vector3.zero;
                            courier.end = Vector3.zero;
                            courier.endId = 0;
                            courier.direction = 0f;
                            courier.itemId = 0;
                            courier.itemCount = 0;
                            courier.inc = 0;
                            courier.gene = 0;
                            logistics.workingCount--;
                            logistics.idleCount++;
                            if (Plugin.DebugLog())
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🏠 无人机返回: 基站[{battleBase.id}] 空闲={logistics.idleCount}");
                            continue;
                        }

                        // 仅物流塔普通槽位（非翘曲器小格）且携带物品返还时，扣减对应 localOrder
                        if (courier.endId >= STATION_ENDID_OFFSET && courier.endId < STATION_WARPER_STORAGE_ENDID_OFFSET && courier.itemId > 0 && courier.itemCount > 0)
                        {
                            int stationId = courier.endId - STATION_ENDID_OFFSET;
                            BattleBaseLogisticsManager.DecrementStationSlotLocalOrder(factory, stationId, courier.itemId, courier.itemCount);
                        }

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
        /// 目标为机甲时，按原版 DispenserComponent 逻辑每帧更新 courier 的 begin/end/maxt/t，
        /// 使无人机视觉上从基站飞向移动中的玩家（end 每帧向玩家靠近，t 表示当前 end 到玩家的距离）。
        /// </summary>
        private static void UpdateCourierToMecha(ref CourierData courier, Vector3 basePos, Vector3 playerPos, float courierSpeed)
        {
            Vector3 end = courier.end;
            float dx = playerPos.x - end.x;
            float dy = playerPos.y - end.y;
            float dz = playerPos.z - end.z;
            float num33 = (float)Math.Sqrt((double)(dx * dx + dy * dy + dz * dz));
            float num34 = (float)Math.Sqrt((double)((playerPos.x - basePos.x) * (playerPos.x - basePos.x) + (playerPos.y - basePos.y) * (playerPos.y - basePos.y) + (playerPos.z - basePos.z) * (playerPos.z - basePos.z)));
            float num35 = (float)Math.Sqrt((double)(end.x * end.x + end.y * end.y + end.z * end.z));
            float num36 = (float)Math.Sqrt((double)(playerPos.x * playerPos.x + playerPos.y * playerPos.y + playerPos.z * playerPos.z));

            if (num33 < 1.4f)
            {
                // 当前 end 已接近玩家，视为到达：设 begin=基站、maxt=弧线距离、t=maxt，本帧会触发 t>=maxt 送货
                double num37 = Math.Sqrt((double)(basePos.x * basePos.x + basePos.y * basePos.y + basePos.z * basePos.z));
                double num38 = Math.Sqrt((double)(playerPos.x * playerPos.x + playerPos.y * playerPos.y + playerPos.z * playerPos.z));
                double num39 = (double)(basePos.x * playerPos.x + basePos.y * playerPos.y + basePos.z * playerPos.z) / (num37 * num38);
                if (num39 < -1.0) num39 = -1.0;
                else if (num39 > 1.0) num39 = 1.0;
                courier.begin = basePos;
                courier.maxt = (float)(Math.Acos(num39) * ((num37 + num38) * 0.5));
                courier.maxt = (float)Math.Sqrt((double)(courier.maxt * courier.maxt) + (num37 - num38) * (num37 - num38));
                courier.t = courier.maxt;
            }
            else
            {
                courier.begin = end;
                float num40 = courierSpeed * 0.016666668f / num33;
                if (num40 > 1f) num40 = 1f;
                float stepX = dx * num40;
                float stepY = dy * num40;
                float stepZ = dz * num40;
                float num41 = num33 / courierSpeed;
                if (num41 < 0.03333333f) num41 = 0.03333333f;
                float num42 = (num36 - num35) / num41 * 0.016666668f;
                end.x += stepX;
                end.y += stepY;
                end.z += stepZ;
                float len = (float)Math.Sqrt((double)(end.x * end.x + end.y * end.y + end.z * end.z));
                if (len > 1E-05f)
                {
                    float scale = (num35 + num42) / len;
                    end.x *= scale;
                    end.y *= scale;
                    end.z *= scale;
                }
                courier.end = end;
                if (num34 > courier.maxt) courier.maxt = num34;
                courier.t = num33;
                if (courier.t >= courier.maxt * 0.99f) courier.t = courier.maxt * 0.99f;
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
        /// 返还物品到基站（与 StorageComponent.AddItem(int, int, int, out int, bool) 签名一致）
        /// </summary>
        private static void ReturnItemToBase(BattleBaseComponent battleBase, int itemId, int count, int inc)
        {
            try
            {
                if (battleBase.storage == null) return;

                // StorageComponent.AddItem(int itemId, int count, int inc, out int remainInc, bool useBan = false)
                var addItemMethod = battleBase.storage.GetType().GetMethod("AddItem",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(int), typeof(int), typeof(int), typeof(int).MakeByRefType(), typeof(bool) },
                    null);

                if (addItemMethod == null) return;

                object[] args = new object[] { itemId, count, inc, 0, false };
                addItemMethod.Invoke(battleBase.storage, args);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ReturnItemToBase 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 送货到机甲（玩家）配送栏槽位：调用 packageUtility.AddItemToAllPackages，与游戏配送器逻辑一致。
        /// </summary>
        private static bool DeliverToMecha(int slotIndex, int itemId, int count, int inc)
        {
            try
            {
                var player = GameMain.mainPlayer;
                if (player?.packageUtility == null) return false;
                if (itemId <= 0 || count <= 0 || itemId == 1099) return false;

                int added = player.packageUtility.AddItemToAllPackages(itemId, count, slotIndex, inc, out int remainInc, 0);
                if (added > 0)
                {
                    player.NotifyReplenishPreferred(itemId, added);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DeliverToMecha 异常: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 送货到物流塔：普通物品走 StationComponent.AddItem（仅写入已配置的本地需求槽位）。
        /// 空间翘曲器(itemId=1210)：先填满塔的翘曲器小存储点(warperCount)，剩余再写入已配置翘曲器的槽位(storage)。
        /// 返回实际接受的数量，0 表示失败或无法放入。
        /// </summary>
        private static int DeliverToStation(PlanetFactory factory, int stationId, int itemId, int count, int inc)
        {
            try
            {
                if (factory?.transport == null) return 0;
                if (stationId <= 0) return 0;

                StationComponent? station = factory.transport.GetStationComponent(stationId);
                if (station == null || station.id != stationId) return 0;

                const int ITEMID_WARPER = 1210;
                if (itemId == ITEMID_WARPER)
                {
                    if (!station.isStellar || station.warperMaxCount <= 0) return 0;
                    int toWarper = Math.Min(count, Math.Max(0, station.warperMaxCount - station.warperCount));
                    station.warperCount += toWarper;
                    int remainder = count - toWarper;
                    if (remainder <= 0)
                        return toWarper;
                    int toSlot = station.AddItem(ITEMID_WARPER, remainder, inc);
                    return toWarper + toSlot;
                }

                return station.AddItem(itemId, count, inc);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DeliverToStation 异常: {ex.Message}\n{ex.StackTrace}");
                return 0;
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

                // 获取配送器连接箱堆的底部箱子实体 ID（InsertIntoStorage 第一个参数是 entityId，不是 storage 池 id）
                if (dispenser.storage?.bottomStorage == null)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: dispenser[{dispenserId}].storage.bottomStorage 为 null");
                    return false;
                }

                var entityIdField = dispenser.storage.bottomStorage.GetType().GetField("entityId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (entityIdField == null)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: entityIdField 为 null");
                    return false;
                }

                int bottomEntityId = (int)entityIdField.GetValue(dispenser.storage.bottomStorage)!;
                if (bottomEntityId <= 0)
                {
                    if (Plugin.DebugLog())
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 送货失败: bottomStorage.entityId 无效");
                    return false;
                }

                // 插入到该配送器连接的箱堆（游戏内部用 entityId 查 storageId 并沿 nextStorage 链放入）
                int inserted = factory.InsertIntoStorage(bottomEntityId, itemId, count, inc, out int incOut, true);
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
        /// 从供应配送器连接箱堆取货，沿 nextStorage 链对每个箱子调用 TakeItem。
        /// </summary>
        private static void TakeItemFromDispenser(PlanetFactory factory, int dispenserId, int itemId, int maxCount, out int actualCount, out int inc)
        {
            actualCount = 0;
            inc = 0;
            try
            {
                var dispenser = BattleBaseLogisticsManager.GetDispenser(factory, dispenserId);
                if (dispenser?.storage?.bottomStorage == null) return;

                var storageType = dispenser.storage.bottomStorage.GetType();
                var takeItemMethod = storageType.GetMethod("TakeItem", BindingFlags.Public | BindingFlags.Instance);
                var nextStorageField = storageType.GetField("nextStorage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (takeItemMethod == null) return;

                object? current = dispenser.storage.bottomStorage;
                int remaining = maxCount;
                int lastInc = 0;
                while (current != null && remaining > 0)
                {
                    object[] args = new object[] { itemId, remaining, 0 };
                    object? result = takeItemMethod.Invoke(current, args);
                    if (result is int taken && taken > 0)
                    {
                        actualCount += taken;
                        remaining -= taken;
                        lastInc = (int)args[2];
                    }
                    current = nextStorageField?.GetValue(current) as object;
                }
                inc = lastInc;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] TakeItemFromDispenser 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 放入基站输入区（与爪子一致，走 InsertIntoStorage，游戏内部对基站用 AddItemFilteredBanOnly）。
        /// </summary>
        private static void DeliverToBaseInputPartition(BattleBaseComponent battleBase, PlanetFactory factory, int itemId, int count, int inc)
        {
            try
            {
                if (battleBase == null || factory == null || itemId <= 0 || count <= 0) return;
                factory.InsertIntoStorage(battleBase.entityId, itemId, count, inc, out int _, true);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DeliverToBaseInputPartition 异常: {ex.Message}");
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
