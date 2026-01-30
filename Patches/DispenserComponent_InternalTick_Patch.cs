using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// Patch DispenserComponent.InternalTick 实现完整的方案A：
    /// 1. 派出空载无人机去基站
    /// 2. 主动监控无人机到达基站
    /// 3. 从基站取货并转向返回
    /// </summary>
    [HarmonyPatch(typeof(DispenserComponent), "InternalTick")]
    public static class DispenserComponent_InternalTick_Patch
    {
        private static int _logThrottle = 0;
        private static int _globalRefreshCounter = 0; // 全局刷新计数器
        private const int REFRESH_INTERVAL = 300; // 每300帧（约5秒）检查一次
        private static Dictionary<int, int> _dispenserCounters = new Dictionary<int, int>(); // 每个配送器独立的计数器
        private const int DISPATCH_INTERVAL = 60; // 每60帧（约1秒）派出一次

        [HarmonyPrefix]
        static void Prefix(DispenserComponent __instance, PlanetFactory factory, EntityData[] entityPool, DispenserComponent[] dispenserPool, long time, float courierSpeed, int courierCarries)
        {
            try
            {
                // 安全检查
                if (__instance == null || factory == null || entityPool == null)
                    return;

                _logThrottle++;
                bool debugLog = BattlefieldBaseHelper.DebugLog() && _logThrottle <= 100;

                // 【关键】在游戏处理之前，拦截我们的特殊 courier
                // 防止游戏访问 grids[-(endId+1)] 导致数组越界
                if (__instance.workCourierDatas != null && __instance.orders != null)
                {
                    for (int i = 0; i < __instance.workCourierCount; i++)
                    {
                        var courier = __instance.workCourierDatas[i];
                        var order = __instance.orders[i];
                        
                        // 诊断：输出所有特殊 courier 的状态
                        if (order.otherId <= -10000 && debugLog && _logThrottle <= 10)
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 courier[{i}]: otherId={order.otherId}, t={courier.t:F2}/{courier.maxt:F2}, dir={courier.direction:F1}, itemCount={courier.itemCount}");
                        }
                        
                        // 【关键修改】：在 courier 到达前就处理，避免游戏的到达逻辑
                        // 只要 t > maxt - 0.2（留一点余量），就认为即将到达
                        if (order.otherId <= -10000 && courier.t >= courier.maxt - 0.2f && courier.itemCount == 0 && courier.direction > 0f)
                        {
                            int specialId = -order.otherId;
                            int battleBaseId = specialId / 10000;
                            int gridIdx = specialId % 10000;
                            
                            if (debugLog)
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🎯 courier[{i}] 即将到达基站，battleBaseId={battleBaseId}, gridIdx={gridIdx}, t={courier.t:F2}/{courier.maxt:F2}");
                            
                            // 从基站取货
                            int actualCount = 0;
                            int inc = 0;
                            if (TryPickFromBattleBase(factory, battleBaseId, gridIdx, courier.itemId, courierCarries, out actualCount, out inc, debugLog))
                            {
                                // 【关键】设置返回状态，让游戏跳过"到达"处理
                                __instance.workCourierDatas[i].itemCount = actualCount;  // 设置货物
                                __instance.workCourierDatas[i].inc = inc;
                                __instance.workCourierDatas[i].direction = -1f;          // 返回模式
                                __instance.workCourierDatas[i].t = courier.maxt;         // t = maxt，开始返回
                                __instance.workCourierDatas[i].endId = 0;                // 清除 endId，游戏不会处理
                                __instance.orders[i].otherId = 0;                        // 清除特殊标记
                                
                                if (debugLog)
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 取货成功！数量={actualCount}，开始返回配送器");
                            }
                            else
                            {
                                // 如果取货失败（没货了），直接让 courier 返回
                                __instance.workCourierDatas[i].direction = -1f;
                                __instance.workCourierDatas[i].t = courier.maxt;
                                __instance.workCourierDatas[i].endId = 0;
                                __instance.orders[i].otherId = 0;
                                
                                if (debugLog)
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 基站无货，courier[{i}] 空载返回");
                            }
                        }
                    }
                }

                // 定期刷新配对（确保物品放回基站后能重新配对）
                // 使用全局计数器，只在第一个 dispenser 中刷新所有配送器，避免重复调用
                if (__instance.id == 1)
                {
                    _globalRefreshCounter++;
                    if (_globalRefreshCounter >= REFRESH_INTERVAL)
                    {
                        _globalRefreshCounter = 0;
                        if (factory.transport != null)
                        {
                            try
                            {
                                // 遍历所有配送器，刷新配对
                                var dispenserPoolField = factory.transport.GetType().GetField("dispenserPool", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                var dispenserCursorField = factory.transport.GetType().GetField("dispenserCursor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                
                                if (dispenserPoolField != null && dispenserCursorField != null)
                                {
                                    object? dispenserPoolObj = dispenserPoolField.GetValue(factory.transport);
                                    object? dispenserCursorObj = dispenserCursorField.GetValue(factory.transport);
                                    
                                    if (dispenserPoolObj is Array allDispensers && dispenserCursorObj != null)
                                    {
                                        int dispenserCursor = Convert.ToInt32(dispenserCursorObj);
                                        
                                        // 刷新所有配送器
                                        for (int i = 1; i < dispenserCursor && i < allDispensers.Length; i++)
                                        {
                                            object? disp = allDispensers.GetValue(i);
                                            if (disp == null) continue;
                                            
                                            var idField = disp.GetType().GetField("id");
                                            int dispId = idField != null ? (int)idField.GetValue(disp)! : 0;
                                            if (dispId != i) continue;
                                            
                                            factory.transport.RefreshDispenserTraffic(i);
                                        }
                                        
                                        if (debugLog)
                                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🔄 定期刷新所有配送器的配对");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 刷新配对失败: {ex.Message}");
                            }
                        }
                    }
                }

                // 派出新的空载无人机（限制频率）
                // 每个 dispenser 独立维护计数器
                int dispenserId = __instance.id;
                if (!_dispenserCounters.ContainsKey(dispenserId))
                {
                    _dispenserCounters[dispenserId] = 0;
                }
                
                _dispenserCounters[dispenserId]++;
                
                // 每 DISPATCH_INTERVAL 帧检查一次
                if (_dispenserCounters[dispenserId] >= DISPATCH_INTERVAL)
                {
                    _dispenserCounters[dispenserId] = 0;
                    
                    // 只在有空闲 courier 时派出
                    if (__instance.idleCourierCount > 0 && __instance.pairs != null)
                    {
                        // 检查是否有战场分析基站的配对（supplyId <= -10000）
                        bool hasBattleBasePair = false;
                        for (int i = 0; i < __instance.pairs.Length; i++)
                        {
                            var pair = __instance.pairs[i];
                            if (pair.supplyId <= -10000)
                            {
                                hasBattleBasePair = true;
                                break;
                            }
                        }
                        
                        if (hasBattleBasePair)
                        {
                            // 只派出1个 courier
                            DispatchOneCourierToBattleBase(__instance, factory, entityPool, courierCarries, debugLog);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] InternalTick Prefix 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 监控已派出的无人机，检测是否到达基站并需要取货
        /// </summary>
        private static void MonitorCouriersForPickup(DispenserComponent dispenser, PlanetFactory factory, EntityData[] entityPool, int courierCarries, bool debugLog)
        {
            try
            {
                var workCourierDatasField = dispenser.GetType().GetField("workCourierDatas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var ordersField = dispenser.GetType().GetField("orders", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (workCourierDatasField == null || ordersField == null) return;

                Array? workCourierDatas = workCourierDatasField.GetValue(dispenser) as Array;
                Array? orders = ordersField.GetValue(dispenser) as Array;

                if (workCourierDatas == null || orders == null) return;

                // 遍历所有工作中的无人机
                for (int i = 0; i < dispenser.workCourierCount; i++)
                {
                    object? courierData = workCourierDatas.GetValue(i);
                    object? order = orders.GetValue(i);

                    if (courierData == null || order == null) continue;

                    var cdType = courierData.GetType();
                    var orderType = order.GetType();

                    // 读取状态
                    float t = (float)(cdType.GetField("t")?.GetValue(courierData) ?? 0f);
                    float direction = (float)(cdType.GetField("direction")?.GetValue(courierData) ?? 0f);
                    int itemCount = (int)(cdType.GetField("itemCount")?.GetValue(courierData) ?? 0);
                    int endId = (int)(cdType.GetField("endId")?.GetValue(courierData) ?? 0);
                    int otherId = (int)(orderType.GetField("otherId")?.GetValue(order) ?? 0);

                    // 检测：空载无人机（itemCount=0）正在去取货（direction>0）且快到达（t>=0.95）且是去战场基站（endId<=-10000）
                    if (itemCount == 0 && direction > 0f && t >= 0.95f && endId <= -10000)
                    {
                        // 解析特殊ID
                        int specialId = -endId;
                        int battleBaseId = specialId / 10000;
                        int gridIdx = specialId % 10000;

                        if (debugLog)
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🎯 courier[{i}] 到达 battleBase[{battleBaseId}]，开始取货...");

                        // 从战场基站取货
                        int actualCount = 0;
                        int inc = 0;
                        bool success = TryPickFromBattleBase(factory, battleBaseId, gridIdx, dispenser.filter, courierCarries, out actualCount, out inc, debugLog);

                        if (success && actualCount > 0)
                        {
                            // 装载物品到无人机
                            cdType.GetField("itemId")?.SetValue(courierData, dispenser.filter);
                            cdType.GetField("itemCount")?.SetValue(courierData, actualCount);
                            cdType.GetField("inc")?.SetValue(courierData, inc);
                            cdType.GetField("direction")?.SetValue(courierData, -1f); // 转向返回

                            // 交换 begin 和 end，让无人机从基站返回配送器
                            Vector3 begin = (Vector3)(cdType.GetField("begin")?.GetValue(courierData) ?? Vector3.zero);
                            Vector3 end = (Vector3)(cdType.GetField("end")?.GetValue(courierData) ?? Vector3.zero);
                            cdType.GetField("begin")?.SetValue(courierData, end);   // 新起点 = 基站
                            cdType.GetField("end")?.SetValue(courierData, begin);   // 新终点 = 配送器
                            cdType.GetField("endId")?.SetValue(courierData, dispenser.id); // 目标改为配送器
                            cdType.GetField("t")?.SetValue(courierData, 0f);        // 重置进度

                            // 写回
                            workCourierDatas.SetValue(courierData, i);

                            if (debugLog)
                            {
                                string itemName = BattlefieldBaseHelper.GetItemName(dispenser.filter);
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ courier[{i}] 装载 {itemName} x{actualCount}，转向返回配送器");
                            }
                        }
                        else if (debugLog)
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ❌ courier[{i}] 从基站取货失败");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] MonitorCouriersForPickup 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 派出一个空载无人机去战场分析基站
        /// </summary>
        private static void DispatchOneCourierToBattleBase(DispenserComponent dispenser, PlanetFactory factory, EntityData[] entityPool, int courierCarries, bool debugLog)
        {
            try
            {
                // 遍历所有战场分析基站配对，只派出一个
                for (int i = 0; i < dispenser.playerPairCount; i++)
                {
                    if (dispenser.idleCourierCount <= 0) break;

                    // 我们的特殊ID格式：-(battleBaseId * 10000 + gridIdx)
                    if (dispenser.pairs[i].supplyId <= -10000)
                    {
                        int specialId = -dispenser.pairs[i].supplyId;
                        int battleBaseId = specialId / 10000;
                        int gridIdx = specialId % 10000;

                        // 检查基站是否有货
                        if (!CheckBattleBaseHasItem(factory, battleBaseId, gridIdx, dispenser.filter, debugLog))
                            continue;

                        // 派出空载无人机
                        bool success = DispatchEmptyCourier(factory, dispenser, entityPool, battleBaseId, gridIdx, courierCarries, debugLog);
                        
                        if (debugLog && success)
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 派出空载courier！剩余空闲={dispenser.idleCourierCount}");
                        }
                        
                        // 只派出一个就返回
                        if (success) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DispatchOneCourierToBattleBase 异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 派出单个空载无人机
        /// </summary>
        private static bool DispatchEmptyCourier(PlanetFactory factory, DispenserComponent dispenser, EntityData[] entityPool, int battleBaseId, int gridIdx, int courierCarries, bool debugLog)
        {
            try
            {
                // 获取战场基站
                var defenseSystemField = factory.GetType().GetField("defenseSystem", BindingFlags.Public | BindingFlags.Instance);
                if (defenseSystemField == null) return false;

                object? defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null) return false;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null) return false;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return false;

                var bufferField = battleBasesPool.GetType().GetField("buffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null) return false;

                object? battleBasesObj = bufferField.GetValue(battleBasesPool);
                if (battleBasesObj is not Array battleBases) return false;

                if (battleBaseId <= 0 || battleBaseId >= battleBases.Length) return false;

                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null) return false;

                var idField = battleBase.GetType().GetField("id");
                if (idField == null) return false;
                int id = (int)idField.GetValue(battleBase)!;
                if (id != battleBaseId) return false;

                // 获取位置
                Vector3 dispenserPos = entityPool[dispenser.entityId].pos;
                
                var entityIdField = battleBase.GetType().GetField("entityId");
                int baseEntityId = entityIdField != null ? (int)entityIdField.GetValue(battleBase)! : 0;
                if (baseEntityId <= 0) return false;
                
                Vector3 basePos = entityPool[baseEntityId].pos;

                // 创建空载courier
                int courierIndex = dispenser.workCourierCount;
                
                var workCourierDatasField = dispenser.GetType().GetField("workCourierDatas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var ordersField = dispenser.GetType().GetField("orders", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (workCourierDatasField == null || ordersField == null) return false;

                Array? workCourierDatas = workCourierDatasField.GetValue(dispenser) as Array;
                Array? orders = ordersField.GetValue(dispenser) as Array;

                if (workCourierDatas == null || orders == null || courierIndex >= workCourierDatas.Length)
                    return false;

                // 设置 CourierData（空载去基站）
                object? courierData = workCourierDatas.GetValue(courierIndex);
                if (courierData != null)
                {
                    var cdType = courierData.GetType();
                    
                    // 计算 maxt（球面距离）
                    double r1 = Math.Sqrt(dispenserPos.x * dispenserPos.x + dispenserPos.y * dispenserPos.y + dispenserPos.z * dispenserPos.z);
                    double r2 = Math.Sqrt(basePos.x * basePos.x + basePos.y * basePos.y + basePos.z * basePos.z);
                    double cosAngle = (dispenserPos.x * basePos.x + dispenserPos.y * basePos.y + dispenserPos.z * basePos.z) / (r1 * r2);
                    if (cosAngle < -1.0) cosAngle = -1.0;
                    else if (cosAngle > 1.0) cosAngle = 1.0;
                    double arcDist = Math.Acos(cosAngle) * ((r1 + r2) * 0.5);
                    float maxt = (float)Math.Sqrt(arcDist * arcDist + (r1 - r2) * (r1 - r2));
                    
                    // 设置 courier 数据
                    // 使用 endId = 0（无目标），避免触发"跟踪玩家"或数组越界
                    // 但在 order.otherId 中保存特殊ID，用于识别我们的 courier
                    
                    cdType.GetField("begin")?.SetValue(courierData, dispenserPos);    // begin = 配送器（起点）
                    cdType.GetField("end")?.SetValue(courierData, basePos);           // end = 基站（终点）
                    cdType.GetField("endId")?.SetValue(courierData, 0);               // endId = 0（避免特殊逻辑）
                    cdType.GetField("direction")?.SetValue(courierData, 1f);          // 1f = 正向
                    cdType.GetField("t")?.SetValue(courierData, 0f);                  // 从 0 开始
                    cdType.GetField("maxt")?.SetValue(courierData, maxt);             // 飞行距离
                    cdType.GetField("itemId")?.SetValue(courierData, dispenser.filter);
                    cdType.GetField("itemCount")?.SetValue(courierData, 0);           // 空载！
                    cdType.GetField("inc")?.SetValue(courierData, 0);
                    cdType.GetField("gene")?.SetValue(courierData, 0);
                    
                    workCourierDatas.SetValue(courierData, courierIndex);
                }

                // 设置 Order（在 otherId 中保存特殊ID，用于识别我们的 courier）
                int specialOrderId = -(battleBaseId * 10000 + gridIdx);
                object? order = orders.GetValue(courierIndex);
                if (order != null)
                {
                    var orderType = order.GetType();
                    orderType.GetField("itemId")?.SetValue(order, dispenser.filter);
                    orderType.GetField("otherId")?.SetValue(order, specialOrderId);  // 特殊ID保存在这里
                    orderType.GetField("thisOrdered")?.SetValue(order, 0);
                    orderType.GetField("otherOrdered")?.SetValue(order, 0);
                    
                    orders.SetValue(order, courierIndex);
                }

                // 更新计数器
                dispenser.workCourierCount++;
                dispenser.idleCourierCount--;

                if (debugLog)
                {
                    string itemName = BattlefieldBaseHelper.GetItemName(dispenser.filter);
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚁 派出空载courier[{courierIndex}] 去取 {itemName}, {dispenserPos} → {basePos}");
                    
                    // 验证数据是否写入成功
                    object? verifyData = workCourierDatas.GetValue(courierIndex);
                    if (verifyData != null)
                    {
                        var vType = verifyData.GetType();
                        float verifyT = (float)(vType.GetField("t")?.GetValue(verifyData) ?? -999f);
                        float verifyMaxt = (float)(vType.GetField("maxt")?.GetValue(verifyData) ?? -999f);
                        float verifyDir = (float)(vType.GetField("direction")?.GetValue(verifyData) ?? -999f);
                        int verifyItemId = (int)(vType.GetField("itemId")?.GetValue(verifyData) ?? 0);
                        int verifyItemCount = (int)(vType.GetField("itemCount")?.GetValue(verifyData) ?? -999);
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 验证courier[{courierIndex}]: t={verifyT:F2}, maxt={verifyMaxt:F2}, dir={verifyDir:F2}, itemId={verifyItemId}, itemCount={verifyItemCount}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DispatchEmptyCourier 异常: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查战场基站是否有指定物品
        /// </summary>
        private static bool CheckBattleBaseHasItem(PlanetFactory factory, int battleBaseId, int gridIdx, int filterItemId, bool debugLog)
        {
            try
            {
                var defenseSystemField = factory.GetType().GetField("defenseSystem", BindingFlags.Public | BindingFlags.Instance);
                if (defenseSystemField == null) return false;

                object? defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null) return false;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null) return false;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return false;

                var bufferField = battleBasesPool.GetType().GetField("buffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null) return false;

                object? battleBasesObj = bufferField.GetValue(battleBasesPool);
                if (battleBasesObj is not Array battleBases) return false;

                if (battleBaseId <= 0 || battleBaseId >= battleBases.Length) return false;

                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null) return false;

                var idField = battleBase.GetType().GetField("id");
                if (idField == null) return false;
                int id = (int)idField.GetValue(battleBase)!;
                if (id != battleBaseId) return false;

                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                if (storage == null) return false;

                var gridsField = storage.GetType().GetField("grids");
                object? gridsObj = gridsField?.GetValue(storage);
                if (gridsObj is not Array grids) return false;

                if (gridIdx < 0 || gridIdx >= grids.Length) return false;

                object? grid = grids.GetValue(gridIdx);
                if (grid == null) return false;

                var itemIdField = grid.GetType().GetField("itemId");
                var countField = grid.GetType().GetField("count");
                int itemId = itemIdField != null ? (int)itemIdField.GetValue(grid)! : 0;
                int count = countField != null ? (int)countField.GetValue(grid)! : 0;

                return itemId == filterItemId && count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 从战场分析基站取物品
        /// </summary>
        private static bool TryPickFromBattleBase(PlanetFactory factory, int battleBaseId, int gridIdx, int itemId, int maxCount, out int actualCount, out int inc, bool debugLog)
        {
            actualCount = 0;
            inc = 0;

            try
            {
                var defenseSystemField = factory.GetType().GetField("defenseSystem", BindingFlags.Public | BindingFlags.Instance);
                if (defenseSystemField == null) return false;

                object? defenseSystem = defenseSystemField.GetValue(factory);
                if (defenseSystem == null) return false;

                var battleBasesField = defenseSystem.GetType().GetField("battleBases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (battleBasesField == null) return false;

                object? battleBasesPool = battleBasesField.GetValue(defenseSystem);
                if (battleBasesPool == null) return false;

                var bufferField = battleBasesPool.GetType().GetField("buffer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (bufferField == null) return false;

                object? battleBasesObj = bufferField.GetValue(battleBasesPool);
                if (battleBasesObj is not Array battleBases) return false;

                if (battleBaseId <= 0 || battleBaseId >= battleBases.Length) return false;

                object? battleBase = battleBases.GetValue(battleBaseId);
                if (battleBase == null) return false;

                var idField = battleBase.GetType().GetField("id");
                if (idField == null) return false;
                int id = (int)idField.GetValue(battleBase)!;
                if (id != battleBaseId) return false;

                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                if (storage == null) return false;

                // 调用 StorageComponent.TakeItem
                var takeItemMethod = storage.GetType().GetMethod("TakeItem", BindingFlags.Public | BindingFlags.Instance);
                if (takeItemMethod == null) return false;

                object[] takeItemParams = new object[] { itemId, maxCount, 0 };
                object? takeResult = takeItemMethod.Invoke(storage, takeItemParams);
                if (takeResult == null) return false;

                actualCount = (int)takeResult;
                inc = (int)takeItemParams[2];

                if (debugLog && actualCount > 0)
                {
                    string itemName = BattlefieldBaseHelper.GetItemName(itemId);
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📦 从 battleBase[{battleBaseId}] 取得 {itemName} x{actualCount} (inc={inc})");
                }

                return actualCount > 0;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] TryPickFromBattleBase 异常: {ex.Message}");
                return false;
            }
        }
    }
}
