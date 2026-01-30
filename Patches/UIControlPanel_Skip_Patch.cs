using HarmonyLib;
using System;
using System.Reflection;

namespace BattlefieldAnalysisBaseDeliver.Patches
{
    /// <summary>
    /// UI 补丁：跳过虚拟配送器，防止 UI 访问导致 NullReferenceException
    /// 策略：在 TakeObjectEntryFromPool 时检测虚拟配送器并返回 null
    /// </summary>
    [HarmonyPatch]
    public static class UIControlPanelWindow_TakeObjectEntryFromPool_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIControlPanelWindow), "TakeObjectEntryFromPool")]
        static bool Prefix(UIControlPanelWindow __instance, int _index, object _target, ref UIControlPanelObjectEntry __result)
        {
            try
            {
                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [1] Patch 开始");

                if (_target == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [2] _target 为 null");
                    return true;
                }

                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [3] _target 类型: {_target.GetType().Name}");

                // 使用反射获取 type 和 index
                var targetType = _target.GetType();
                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [4] 获取 Type 成功");

                var typeField = targetType.GetField("type", BindingFlags.Public | BindingFlags.Instance);
                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [5] typeField={(typeField == null ? "null" : "found")}");

                var indexField = targetType.GetField("index", BindingFlags.Public | BindingFlags.Instance);
                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [6] indexField={(indexField == null ? "null" : "found")}");

                if (typeField == null || indexField == null)
                {
                    Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [7] 未找到字段");
                    return true;
                }

                int type = (int)typeField.GetValue(_target)!;
                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [8] type={type}");

                int index = (int)indexField.GetValue(_target)!;
                //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [9] index={index}");

                // 只对配送器类型 (type == 8) 进行检查
                if (type == 8)
                {
                    bool isVirtual = VirtualDispenserManager.IsVirtualDispenser(index);
                    //Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [10] IsVirtual={isVirtual}");

                    if (isVirtual)
                    {
                        Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] ⚠️ 拦截虚拟配送器[{index}]");
                        __result = null!;
                        return false; // 阻止原方法执行
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[{PluginInfo.PLUGIN_NAME}] Patch 异常: {ex.Message}");
            }

            return true;
        }
    }

    /// <summary>
    /// UI 补丁：在 DetermineEntryVisible 中跳过虚拟配送器
    /// 这是第二道防线，防止 UI 尝试显示虚拟配送器
    /// </summary>
    [HarmonyPatch]
    public static class UIControlPanelWindow_DetermineEntryVisible_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIControlPanelWindow), "DetermineEntryVisible")]
        static void Prefix(UIControlPanelWindow __instance)
        {
            try
            {
                // 使用反射访问 targetList
                var targetListField = typeof(UIControlPanelWindow).GetField("targetList", BindingFlags.NonPublic | BindingFlags.Instance);
                if (targetListField == null) return;

                var targetList = targetListField.GetValue(__instance) as System.Collections.IList;
                if (targetList == null || targetList.Count == 0) return;

                // 过滤掉虚拟配送器
                int removedCount = 0;
                for (int i = targetList.Count - 1; i >= 0; i--)
                {
                    var target = targetList[i];
                    if (target == null) continue;

                    // 使用反射获取 type 和 index
                    var typeField = target.GetType().GetField("type", BindingFlags.Public | BindingFlags.Instance);
                    var indexField = target.GetType().GetField("index", BindingFlags.Public | BindingFlags.Instance);

                    if (typeField == null || indexField == null) continue;

                    int targetType = (int)typeField.GetValue(target)!;
                    int targetIndex = (int)indexField.GetValue(target)!;

                    // 检查是否是配送器类型 (type == 8) 且是虚拟配送器
                    if (targetType == 8 && VirtualDispenserManager.IsVirtualDispenser(targetIndex))
                    {
                        targetList.RemoveAt(i);
                        removedCount++;
                    }
                }

                if (removedCount > 0 && BattlefieldBaseHelper.DebugLog())
                {
                    Plugin.Log?.LogInfo($"[{PluginInfo.PLUGIN_NAME}] 已从监控面板过滤 {removedCount} 个虚拟配送器");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] DetermineEntryVisible Patch 异常: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Finalizer 方案：捕获 OnSetTarget 的异常并防止游戏崩溃
    /// 这是最后一道防线，即使虚拟配送器没有被过滤掉，也能防止游戏报错
    /// </summary>
    [HarmonyPatch]
    public static class UIControlPanelDispenserEntry_OnSetTarget_Patch
    {
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(UIControlPanelDispenserEntry), "OnSetTarget")]
        static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                Plugin.Log?.LogWarning($"[{PluginInfo.PLUGIN_NAME}] [FINALIZER] 捕获并吞掉 OnSetTarget 异常，防止游戏崩溃: {__exception.Message}");
                // 返回 null 表示吞掉异常，防止游戏崩溃
                return null!;
            }
            return __exception;
        }
    }
}
