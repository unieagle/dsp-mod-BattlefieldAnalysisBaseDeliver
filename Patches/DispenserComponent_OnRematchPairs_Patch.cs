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
        [HarmonyPrefix]
        static bool Prefix(DispenserComponent __instance)
        {
            // 检查是否是虚拟配送器
            if (VirtualDispenserManager.IsVirtualDispenser(__instance.id))
            {
                // 虚拟配送器不需要处理 OnRematchPairs
                // 因为它没有 deliveryPackage（玩家背包）
                return false;  // 跳过原方法
            }

            return true;  // 继续执行原方法
        }
    }
}
