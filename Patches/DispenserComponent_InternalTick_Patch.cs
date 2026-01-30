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
        private static Dictionary<int, int> _dispenserCounters = new Dictionary<int, int>(); // 每个配送器独立的计数器（派遣频率）
        private static Dictionary<int, int> _checkCounters = new Dictionary<int, int>(); // 每个配送器的检查次数（用于诊断日志）
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
                
                // 【诊断】每300帧（5秒）输出配送器状态
                if (_logThrottle % 300 == 0 && __instance.pairCount > 0)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🔍 配送器[{__instance.id}] 状态: idle={__instance.idleCourierCount}, work={__instance.workCourierCount}, pairCount={__instance.pairCount} (playerPairCount={__instance.playerPairCount})");
                    
                    // 输出所有配对（最多5个）
                    int maxPairs = Math.Min(__instance.pairCount, Math.Min(__instance.pairs.Length, 5));
                    for (int i = 0; i < maxPairs; i++)
                    {
                        var pair = __instance.pairs[i];
                        bool isVirtual = VirtualDispenserManager.IsVirtualDispenser(pair.supplyId);
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}]   pair[{i}]: supplyId={pair.supplyId} (虚拟={isVirtual}), demandId={pair.demandId}");
                    }
                }

                // 【关键】在游戏处理之前，拦截我们的特殊 courier
                // 防止游戏访问 grids[-(endId+1)] 导致数组越界
                if (__instance.workCourierDatas != null && __instance.orders != null)
                {
                    for (int i = 0; i < __instance.workCourierCount; i++)
                    {
                        var courier = __instance.workCourierDatas[i];
                        var order = __instance.orders[i];
                        
                        // 【新方案】识别飞向虚拟配送器的无人机
                        // 检查 endId 是否是虚拟配送器
                        if (courier.endId > 0 && VirtualDispenserManager.IsVirtualDispenser(courier.endId))
                        {
                            // 诊断：输出状态
                            if (debugLog && _logThrottle <= 10)
                            {
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 📊 courier[{i}] 飞向虚拟配送器: endId={courier.endId}, t={courier.t:F2}/{courier.maxt:F2}, dir={courier.direction:F1}, itemCount={courier.itemCount}");
                            }
                            
                            // 在无人机到达虚拟配送器前拦截（从对应的战场分析基站取货）
                            if (courier.t >= courier.maxt - 0.2f && courier.itemCount == 0 && courier.direction > 0f)
                            {
                                // 获取对应的战场分析基站ID
                                if (!VirtualDispenserManager.TryGetBattleBaseId(courier.endId, out int battleBaseId))
                                {
                                    if (debugLog)
                                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 无法找到虚拟配送器 {courier.endId} 对应的战场分析基站");
                                    continue;
                                }
                                
                                // 从订单中获取 gridIdx
                                var supplyIndexField = order.GetType().GetField("supplyIndex");
                                int gridIdx = supplyIndexField != null ? (int)supplyIndexField.GetValue(order)! : 0;
                                
                                if (debugLog)
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🎯 courier[{i}] 即将到达虚拟配送器[{courier.endId}]，对应战场基站[{battleBaseId}] gridIdx={gridIdx}, t={courier.t:F2}/{courier.maxt:F2}");
                                
                                // 从基站取货
                                int actualCount = 0;
                                int inc = 0;
                                if (TryPickFromBattleBase(factory, battleBaseId, gridIdx, courier.itemId, courierCarries, out actualCount, out inc, debugLog))
                                {
                                    // 设置返回状态
                                    __instance.workCourierDatas[i].itemCount = actualCount;  // 设置货物
                                    __instance.workCourierDatas[i].inc = inc;
                                    __instance.workCourierDatas[i].direction = -1f;          // 返回模式
                                    __instance.workCourierDatas[i].t = courier.maxt;         // t = maxt，开始返回
                                    
                                    if (debugLog)
                                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 从战场基站[{battleBaseId}]取货成功！数量={actualCount}，开始返回配送器");
                                }
                                else
                                {
                                    // 如果取货失败（没货了），空载返回
                                    __instance.workCourierDatas[i].direction = -1f;
                                    __instance.workCourierDatas[i].t = courier.maxt;
                                    
                                    if (debugLog)
                                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 战场基站[{battleBaseId}]无货，courier[{i}] 空载返回");
                                }
                            }
                        }
                    }
                }

                // 派出新的空载无人机（限制频率）
                // 注意：不再主动调用 RefreshDispenserTraffic，依赖游戏原生调用
                // 游戏会在配送器 filter 改变、物品变化等情况下自动调用
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
                    
                    // 增加检查次数
                    if (!_checkCounters.ContainsKey(dispenserId))
                    {
                        _checkCounters[dispenserId] = 0;
                    }
                    _checkCounters[dispenserId]++;
                    
                    // 【诊断】记录派遣检查状态（前20次或有配对时）
                    // ⚠️ 注意：我们的虚拟配送器配对使用正数ID，不计入 playerPairCount，而是在 pairCount 中
                    if (__instance.pairCount > 0)
                    {
                        // 每次检查都记录（前20次）
                        if (_checkCounters[dispenserId] <= 20)
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🔍 派遣检查 #{_checkCounters[dispenserId]}: dispenser[{__instance.id}] idle={__instance.idleCourierCount}, work={__instance.workCourierCount}, pairCount={__instance.pairCount} (playerPairCount={__instance.playerPairCount})");
                        }
                    }
                    
                    // 只在有空闲 courier 时派出
                    // ⚠️ 检查 pairCount 而不是 playerPairCount，因为虚拟配送器配对使用正数ID
                    if (__instance.idleCourierCount > 0 && __instance.pairs != null && __instance.pairCount > 0)
                    {
                        // 【新方案】检查是否有虚拟配送器的配对
                        bool hasVirtualDispenserPair = false;
                        int virtualPairIndex = -1;
                        // ✅ 遍历 pairCount 而不是 playerPairCount
                        for (int i = 0; i < __instance.pairCount && i < __instance.pairs.Length; i++)
                        {
                            var pair = __instance.pairs[i];
                            
                            // 【诊断】输出每个配对（前20次检查）
                            if (_checkCounters[dispenserId] <= 20)
                            {
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}]   检查 pair[{i}]: supplyId={pair.supplyId}, demandId={pair.demandId}, isVirtual={VirtualDispenserManager.IsVirtualDispenser(pair.supplyId)}");
                            }
                            
                            // ✅ 关键检查：
                            // 1. supplyId 是虚拟配送器（供应方）
                            // 2. demandId 是当前配送器（需求方）- 这才是正确的配送器-配送器规则！
                            if (pair.supplyId > 0 && 
                                VirtualDispenserManager.IsVirtualDispenser(pair.supplyId) &&
                                pair.demandId == __instance.id)  // ← 检查配对方向，而不是 playerMode
                            {
                                hasVirtualDispenserPair = true;
                                virtualPairIndex = i;
                                
                                // 【诊断】找到虚拟配送器配对（前20次检查或每5秒）
                                if (_checkCounters[dispenserId] <= 20 || _checkCounters[dispenserId] % 5 == 0)
                                {
                                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 发现虚拟配送器配对! dispenser[{__instance.id}] pair[{i}]: supplyId={pair.supplyId}");
                                }
                                break;
                            }
                        }
                        
                        if (hasVirtualDispenserPair)
                        {
                            // 【关键诊断】输出派遣信息（前20次检查或每5秒）
                            if (_checkCounters[dispenserId] <= 20 || _checkCounters[dispenserId] % 5 == 0)
                            {
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚀 准备派出无人机! dispenser[{__instance.id}] virtualPair[{virtualPairIndex}] idleCouriers={__instance.idleCourierCount}");
                            }
                            
                            // 只派出1个 courier
                            DispatchOneCourierToBattleBase(__instance, factory, entityPool, courierCarries, debugLog);
                        }
                        else if (_checkCounters[dispenserId] <= 20)
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 没有找到虚拟配送器配对（检查了{__instance.pairCount}个配对）");
                        }
                    }
                    else if (__instance.pairCount > 0 && _checkCounters[dispenserId] <= 20)
                    {
                        // 【诊断】为什么不派遣？
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 不满足派遣条件: idle={__instance.idleCourierCount}, pairs={__instance.pairs != null}, pairCount={__instance.pairCount}");
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
                // 【新方案】遍历所有配对，找到虚拟配送器的配对，只派出一个
                // ⚠️ 必须使用 pairCount 而不是 playerPairCount，因为虚拟配送器使用正数ID
                for (int i = 0; i < dispenser.pairCount && i < dispenser.pairs.Length; i++)
                {
                    if (dispenser.idleCourierCount <= 0) break;

                    var pair = dispenser.pairs[i];
                    
                    // 检查 supplyId 是否是虚拟配送器
                    if (pair.supplyId > 0 && VirtualDispenserManager.IsVirtualDispenser(pair.supplyId))
                    {
                        int virtualDispenserId = pair.supplyId;
                        int gridIdx = pair.supplyIndex;
                        
                        // 获取对应的战场分析基站ID
                        if (!VirtualDispenserManager.TryGetBattleBaseId(virtualDispenserId, out int battleBaseId))
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] 无法找到虚拟配送器 {virtualDispenserId} 对应的战场分析基站");
                            continue;
                        }

                        // ✅ 检查基站是否仍然存在（防止基站拆除后仍派遣）
                        if (!VirtualDispenserManager.CheckBattleBaseExists(factory, battleBaseId))
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 战场基站[{battleBaseId}]不存在，取消派遣");
                            continue;
                        }

                        // 检查基站是否有货
                        if (!CheckBattleBaseHasItem(factory, battleBaseId, gridIdx, dispenser.filter, debugLog))
                        {
                            if (_logThrottle % 600 == 0)  // 每10秒记录一次
                                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 战场基站[{battleBaseId}] gridIdx={gridIdx} 暂无货物");
                            continue;
                        }

                        // 【关键】始终输出派遣日志
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 🚁 开始派遣! 配送器[{dispenser.id}] → 虚拟配送器[{virtualDispenserId}](战场基站[{battleBaseId}]), filter={dispenser.filter}");
                        
                        // 派出空载无人机（飞向虚拟配送器的位置，即战场分析基站）
                        bool success = DispatchEmptyCourier(factory, dispenser, entityPool, battleBaseId, gridIdx, courierCarries, debugLog);
                        
                        if (success)
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 派遣成功! 空载courier飞向战场基站[{battleBaseId}]，剩余空闲={dispenser.idleCourierCount}");
                        }
                        else
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ❌ 派遣失败!");
                        }
                        
                        // 只派出一个就返回
                        if (success) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] DispatchOneCourierToBattleBase 异常: {ex.Message}\n{ex.StackTrace}");
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

                // 获取 entityId 检查战场基站是否存在
                var entityIdField = battleBase.GetType().GetField("entityId");
                if (entityIdField == null) return false;
                int baseEntityId = (int)entityIdField.GetValue(battleBase)!;
                if (baseEntityId <= 0) return false;  // 战场基站不存在或已被拆除

                // 获取位置
                Vector3 dispenserPos = entityPool[dispenser.entityId].pos;
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
                    
                    // 【新方案】设置 courier 数据
                    // endId = 虚拟配送器ID（正数！），游戏可以正常处理
                    
                    // 获取虚拟配送器ID
                    if (!VirtualDispenserManager.TryGetVirtualDispenserId(battleBaseId, out int virtualDispenserId))
                    {
                        Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 无法找到战场基站 {battleBaseId} 对应的虚拟配送器");
                        return false;
                    }
                    
                    cdType.GetField("begin")?.SetValue(courierData, dispenserPos);    // begin = 配送器（起点）
                    cdType.GetField("end")?.SetValue(courierData, basePos);           // end = 基站（终点）
                    cdType.GetField("endId")?.SetValue(courierData, virtualDispenserId); // endId = 虚拟配送器ID（正数！）
                    cdType.GetField("direction")?.SetValue(courierData, 1f);          // 1f = 正向
                    cdType.GetField("t")?.SetValue(courierData, 0f);                  // 从 0 开始
                    cdType.GetField("maxt")?.SetValue(courierData, maxt);             // 飞行距离
                    cdType.GetField("itemId")?.SetValue(courierData, dispenser.filter);
                    cdType.GetField("itemCount")?.SetValue(courierData, 0);           // 空载！
                    cdType.GetField("inc")?.SetValue(courierData, 0);
                    cdType.GetField("gene")?.SetValue(courierData, 0);
                    
                    workCourierDatas.SetValue(courierData, courierIndex);
                }

                // 设置 Order
                object? order = orders.GetValue(courierIndex);
                if (order != null)
                {
                    var orderType = order.GetType();
                    
                    // 获取虚拟配送器ID
                    if (!VirtualDispenserManager.TryGetVirtualDispenserId(battleBaseId, out int virtualDispenserId))
                    {
                        Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] 无法找到战场基站 {battleBaseId} 对应的虚拟配送器");
                        return false;
                    }
                    
                    orderType.GetField("itemId")?.SetValue(order, dispenser.filter);
                    orderType.GetField("otherId")?.SetValue(order, virtualDispenserId);  // otherId也是虚拟配送器ID
                    orderType.GetField("supplyIndex")?.SetValue(order, gridIdx);  // 保存gridIdx以便后续取货
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
        /// <summary>
        /// 检查战场基站是否有指定物品（检查所有格子，而不是只检查特定gridIdx）
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

                // 检查 entityId 来判断战场基站是否存在（更可靠）
                var entityIdField = battleBase.GetType().GetField("entityId");
                if (entityIdField == null) return false;
                int entityId = (int)entityIdField.GetValue(battleBase)!;
                if (entityId <= 0) return false;  // 战场基站不存在或已被拆除

                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                if (storage == null) return false;

                var gridsField = storage.GetType().GetField("grids");
                object? gridsObj = gridsField?.GetValue(storage);
                if (gridsObj is not Array grids) return false;

                // ✅ 修复：检查所有格子，而不是只检查特定的gridIdx
                // 因为同一个物品可能分布在多个格子里
                for (int i = 0; i < grids.Length; i++)
                {
                    object? grid = grids.GetValue(i);
                    if (grid == null) continue;

                    var itemIdField = grid.GetType().GetField("itemId");
                    var countField = grid.GetType().GetField("count");
                    int itemId = itemIdField != null ? (int)itemIdField.GetValue(grid)! : 0;
                    int count = countField != null ? (int)countField.GetValue(grid)! : 0;

                    // 找到任何一个格子有这个物品就返回 true
                    if (itemId == filterItemId && count > 0)
                    {
                        return true;
                    }
                }

                // 所有格子都没有这个物品
                return false;
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

                // 检查 entityId 来判断战场基站是否存在（更可靠）
                var entityIdField = battleBase.GetType().GetField("entityId");
                if (entityIdField == null) return false;
                int entityId = (int)entityIdField.GetValue(battleBase)!;
                if (entityId <= 0) return false;  // 战场基站不存在或已被拆除

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
