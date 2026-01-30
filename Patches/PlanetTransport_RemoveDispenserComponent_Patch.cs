using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// 拆除配送器时，退还飞行中无人机携带的物品
    /// </summary>
    [HarmonyPatch(typeof(PlanetTransport), "RemoveDispenserComponent")]
    public static class PlanetTransport_RemoveDispenserComponent_Patch
    {
        [HarmonyPrefix]
        static void Prefix(PlanetTransport __instance, int id)
        {
            try
            {
                // 检查配送器是否存在
                if (id <= 0 || id >= __instance.dispenserPool.Length) return;
                
                var dispenser = __instance.dispenserPool[id];
                if (dispenser == null || dispenser.id == 0) return;
                
                // 跳过虚拟配送器（它们没有实体物品）
                if (VirtualDispenserManager.IsVirtualDispenser(id))
                    return;
                
                // 检查是否有飞行中的无人机携带物品
                if (dispenser.workCourierDatas == null || dispenser.workCourierCount == 0)
                    return;
                
                Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 检测到配送器[{id}]即将被拆除，检查飞行中的无人机...");
                
                int itemsReturned = 0;
                int couriersWithItems = 0;
                
                // 遍历所有飞行中的无人机
                for (int i = 0; i < dispenser.workCourierCount; i++)
                {
                    var courier = dispenser.workCourierDatas[i];
                    
                    // 如果无人机携带物品
                    if (courier.itemCount > 0 && courier.itemId > 0)
                    {
                        couriersWithItems++;
                        itemsReturned += courier.itemCount;
                        
                        // 尝试退还物品
                        // 1. 如果无人机是从虚拟配送器（基站）返回的，退还到基站
                        // 2. 否则退还到玩家背包
                        bool returned = ReturnItemsToOrigin(__instance.factory, courier, dispenser);
                        
                        string itemName = BattlefieldBaseHelper.GetItemName(courier.itemId);
                        if (returned)
                        {
                            Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ✅ 已退还物品：{itemName} x{courier.itemCount}");
                        }
                        else
                        {
                            Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 无法退还物品：{itemName} x{courier.itemCount}，物品可能丢失！");
                        }
                    }
                }
                
                if (couriersWithItems > 0)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 配送器[{id}]拆除：共退还 {itemsReturned} 个物品（{couriersWithItems} 个无人机）");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] RemoveDispenserComponent Prefix 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 退还物品到来源地
        /// </summary>
        private static bool ReturnItemsToOrigin(PlanetFactory factory, CourierData courier, DispenserComponent dispenser)
        {
            try
            {
                // 判断物品来源：
                // 1. 如果 endId 是虚拟配送器，说明无人机正在飞向基站（去取货），但此时应该是空载的
                // 2. 如果 endId 是配送器ID（等于dispenser.id），说明无人机正在返回，物品可能来自基站
                // 3. 检查配送器的配对，看看是否有虚拟配送器配对
                
                // 遍历配送器的配对，检查是否有虚拟配送器
                if (dispenser.pairs != null && dispenser.pairCount > 0)
                {
                    for (int i = 0; i < dispenser.pairCount && i < dispenser.pairs.Length; i++)
                    {
                        var pair = dispenser.pairs[i];
                        
                        // 如果这是一个虚拟配送器配对，且配送器是需求方
                        if (pair.supplyId > 0 && 
                            VirtualDispenserManager.IsVirtualDispenser(pair.supplyId) &&
                            pair.demandId == dispenser.id)
                        {
                            // 物品来自基站，尝试退还到基站
                            if (VirtualDispenserManager.TryGetBattleBaseId(pair.supplyId, out int battleBaseId))
                            {
                                if (ReturnItemsToBattleBase(factory, battleBaseId, courier.itemId, courier.itemCount, courier.inc))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                
                // 否则，或者退还到基站失败，退还到玩家背包
                return ReturnItemsToPlayer(courier.itemId, courier.itemCount, courier.inc);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ReturnItemsToOrigin 异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 退还物品到战场基站
        /// </summary>
        private static bool ReturnItemsToBattleBase(PlanetFactory factory, int battleBaseId, int itemId, int count, int inc)
        {
            try
            {
                var defenseSystem = factory.defenseSystem;
                if (defenseSystem == null) return false;
                
                var battleBases = defenseSystem.battleBases.buffer;
                if (battleBaseId <= 0 || battleBaseId >= battleBases.Length) return false;
                
                var battleBase = battleBases[battleBaseId];
                if (battleBase == null) return false;
                
                // 检查基站是否存在
                if (battleBase.entityId <= 0) return false;
                
                // 添加物品到基站
                var storageField = battleBase.GetType().GetField("storage");
                object? storage = storageField?.GetValue(battleBase);
                if (storage == null) return false;
                
                // 调用 StorageComponent.AddItem
                var addItemMethod = storage.GetType().GetMethod("AddItem", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int).MakeByRefType(), typeof(bool) });
                if (addItemMethod == null) return false;
                
                object[] parameters = new object[] { itemId, count, inc, 0, false };
                var result = addItemMethod.Invoke(storage, parameters);
                
                if (result is int added && added > 0)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已将物品退还到战场基站[{battleBaseId}]");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ReturnItemsToBattleBase 异常: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 退还物品到玩家背包
        /// </summary>
        private static bool ReturnItemsToPlayer(int itemId, int count, int inc)
        {
            try
            {
                // 获取玩家对象
                var player = GameMain.mainPlayer;
                if (player == null) return false;
                
                // 获取玩家背包
                var package = player.package;
                if (package == null) return false;
                
                // 添加物品到背包
                int remainInc;
                int added = package.AddItem(itemId, count, inc, out remainInc, false);
                
                if (added > 0)
                {
                    string itemName = BattlefieldBaseHelper.GetItemName(itemId);
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已将物品 {itemName} x{added} 退还到玩家背包");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] ReturnItemsToPlayer 异常: {ex.Message}");
                return false;
            }
        }
    }
}
