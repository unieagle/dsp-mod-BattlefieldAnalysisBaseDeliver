using HarmonyLib;
using System;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// Patch UIControlPanel 相关方法以跳过虚拟配送器
    /// 虚拟配送器不是真实建筑，不应该显示在监控面板中
    /// </summary>
    
    // Patch UIControlPanelDispenserEntry.OnSetTarget
    [HarmonyPatch(typeof(UIControlPanelDispenserEntry), "OnSetTarget")]
    public static class UIControlPanelDispenserEntry_OnSetTarget_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(UIControlPanelDispenserEntry __instance, int _index, ControlPanelTarget _target)
        {
            try
            {
                // 检查是否是虚拟配送器
                if (_index > 0 && VirtualDispenserManager.IsVirtualDispenser(_index))
                {
                    // 跳过虚拟配送器，不显示在UI中
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] UIControlPanelDispenserEntry.OnSetTarget Patch 异常: {ex.Message}");
            }
            
            return true;  // 继续执行原方法
        }
    }

    // Patch UIControlPanelObjectEntry.InitFromPool
    [HarmonyPatch(typeof(UIControlPanelObjectEntry), "InitFromPool")]
    public static class UIControlPanelObjectEntry_InitFromPool_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(int _index, ControlPanelTarget _target)
        {
            try
            {
                // 检查是否是配送器类型且是虚拟配送器
                if (_target != null && _index > 0 && VirtualDispenserManager.IsVirtualDispenser(_index))
                {
                    // 跳过虚拟配送器
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] UIControlPanelObjectEntry.InitFromPool Patch 异常: {ex.Message}");
            }
            
            return true;  // 继续执行原方法
        }
    }

    // Patch UIControlPanelWindow.TakeObjectEntryFromPool
    [HarmonyPatch(typeof(UIControlPanelWindow), "TakeObjectEntryFromPool")]
    public static class UIControlPanelWindow_TakeObjectEntryFromPool_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(int _index, ControlPanelTarget _target, ref UIControlPanelObjectEntry __result)
        {
            try
            {
                // 检查是否是虚拟配送器
                if (_index > 0 && VirtualDispenserManager.IsVirtualDispenser(_index))
                {
                    // 返回 null，不创建UI条目
                    __result = null!;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] UIControlPanelWindow.TakeObjectEntryFromPool Patch 异常: {ex.Message}");
            }
            
            return true;  // 继续执行原方法
        }
    }
}
