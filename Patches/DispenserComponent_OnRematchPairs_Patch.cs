using HarmonyLib;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// Patch DispenserComponent.OnRematchPairs 以跳过虚拟配送器
    /// 虚拟配送器没有 deliveryPackage，访问会导致 NullReferenceException
    /// </summary>
    [HarmonyPatch(typeof(DispenserComponent), "OnRematchPairs")]
    public static class DispenserComponent_OnRematchPairs_Patch
    {
        private static int _callCount = 0;
        
        [HarmonyPrefix]
        static bool Prefix(DispenserComponent __instance, PlanetFactory factory)
        {
            _callCount++;
            
            try
            {
                bool isVirtual = VirtualDispenserManager.IsVirtualDispenser(__instance.id);
                
                // 调试日志
                if (BattlefieldBaseHelper.DebugLog() && _callCount <= 50)
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] OnRematchPairs 调用 #{_callCount}: dispenser.id={__instance.id}, isVirtual={isVirtual}");
                }
                
                // 检查是否是虚拟配送器
                if (isVirtual)
                {
                    if (BattlefieldBaseHelper.DebugLog())
                    {
                        Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] ⏭️ 跳过虚拟配送器[{__instance.id}]的 OnRematchPairs");
                    }
                    return false;  // 跳过原方法
                }
                
                // 额外的安全检查：如果 deliveryPackage 是 null，也跳过
                if (__instance.deliveryPackage == null)
                {
                    if (BattlefieldBaseHelper.DebugLog())
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 配送器[{__instance.id}]的 deliveryPackage 为 null，跳过 OnRematchPairs");
                    }
                    return false;
                }

                return true;  // 继续执行原方法
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] OnRematchPairs Prefix 异常: {ex.Message}\n{ex.StackTrace}");
                return true;  // 出错时继续执行原方法（避免完全破坏游戏）
            }
        }
    }
}
