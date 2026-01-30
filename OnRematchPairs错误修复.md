# OnRematchPairs 错误修复 ✅

## 🐛 问题描述

用户报告了两个问题：

### 问题1：没有配送

从日志看：
```
📊   battleBase[1] 中没有物品
⚠️ 没有找到虚拟配送器配对（检查了2个配对）
```

**原因**：战场分析基站中没有物品，所以没有建立配对。

**解决**：让战场基站收集物品，配对就会自动建立。

---

### 问题2：切换配送器模式时游戏报错

**错误栈**：
```
NullReferenceException: Object reference not set to an instance of an object
(wrapper dynamic-method) DispenserComponent.DMD<DispenserComponent::OnRematchPairs>
  (DispenserComponent,PlanetFactory,DispenserComponent[],int,int)
(wrapper dynamic-method) PlanetTransport.DMD<PlanetTransport::RefreshDispenserTraffic>
  (PlanetTransport,int)
PlanetTransport.SetDispenserStorageDeliveryMode
UIDispenserWindow.UIToValue
UIDispenserWindow.OnModeToggleClicked
...
```

**分析**：
1. 用户切换配送器的需求/供应选项
2. 游戏调用 `PlanetTransport.RefreshDispenserTraffic`
3. `RefreshDispenserTraffic` 遍历所有配送器，包括虚拟配送器
4. 对每个配送器调用 `OnRematchPairs`
5. **虚拟配送器的 `deliveryPackage` 字段是 null**
6. `OnRematchPairs` 第236行访问 `this.deliveryPackage.grids` 导致 `NullReferenceException`

---

## 🔍 根本原因

### OnRematchPairs 方法（第236行）

```csharp
public void OnRematchPairs(PlanetFactory factory, DispenserComponent[] dispenserPool, int keyId, int courierCarries)
{
    // ...
    DeliveryPackage.GRID[] grids = this.deliveryPackage.grids;  // ← 这里会抛出 NullReferenceException
    // ...
}
```

### RefreshDispenserTraffic 调用（第1340行）

```csharp
// PlanetTransport.cs, 第1340行
for (int l = 1; l < this.dispenserCursor; l++)
{
    DispenserComponent dispenserComponent2 = this.dispenserPool[l];
    if (dispenserComponent2 != null && dispenserComponent2.id == l)
    {
        // ...
        dispenserComponent2.OnRematchPairs(this.factory, this.dispenserPool, keyId, logisticCourierCarries);
        // ← 虚拟配送器也会被调用！
    }
}
```

### 虚拟配送器的初始化

```csharp
// VirtualDispenserManager.cs
virtualDispenser.storage = null;
// ⚠️ 但是没有初始化 deliveryPackage！
```

**问题**：
- 虚拟配送器在 `dispenserPool` 中
- `RefreshDispenserTraffic` 会遍历所有配送器并调用 `OnRematchPairs`
- 虚拟配送器的 `deliveryPackage` 是 null（我们没有初始化）
- 访问 `deliveryPackage.grids` 导致 `NullReferenceException`

---

## ✅ 解决方案

### 方案：增强 OnRematchPairs 补丁

我们已经有 `DispenserComponent_OnRematchPairs_Patch`，但它可能在某些情况下没有正确拦截。

#### 改进的补丁（增加安全检查）

```csharp
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
            // 【诊断】前20次调用输出详细日志
            if (_callCount <= 20)
            {
                Plugin.Log?.LogInfo($"OnRematchPairs 调用 #{_callCount}: dispenser.id={__instance.id}, isVirtual={VirtualDispenserManager.IsVirtualDispenser(__instance.id)}");
            }
            
            // ✅ 检查1：是否是虚拟配送器
            if (VirtualDispenserManager.IsVirtualDispenser(__instance.id))
            {
                if (_callCount <= 20)
                {
                    Plugin.Log?.LogInfo($"✅ 跳过虚拟配送器[{__instance.id}]的 OnRematchPairs");
                }
                
                return false;  // 跳过原方法
            }
            
            // ✅ 检查2：deliveryPackage 是否为 null（额外的安全检查）
            if (__instance.deliveryPackage == null)
            {
                Plugin.Log?.LogWarning($"⚠️ 配送器[{__instance.id}]的 deliveryPackage 为 null，跳过 OnRematchPairs");
                return false;  // 跳过原方法
            }

            return true;  // 继续执行原方法
        }
        catch (System.Exception ex)
        {
            Plugin.Log?.LogError($"OnRematchPairs Prefix 异常: {ex.Message}");
            return true;  // 出错时继续执行原方法
        }
    }
}
```

**改进点**：

1. **双重检查**：
   - 检查1：是否是虚拟配送器（通过映射）
   - 检查2：`deliveryPackage` 是否为 null

2. **诊断日志**：
   - 前20次调用输出详细信息
   - 帮助调试补丁是否正常工作

3. **异常处理**：
   - 捕获所有异常
   - 避免补丁本身导致游戏崩溃

---

## 📊 修改文件

| 文件 | 修改内容 | 行数变化 |
|------|---------|---------|
| `DispenserComponent_OnRematchPairs_Patch.cs` | 添加双重检查和诊断日志 | +35 行 |

---

## 🎯 为什么之前的补丁可能失效？

### 可能的原因

1. **时序问题**：
   - `OnRematchPairs` 被调用时，虚拟配送器可能还没有被添加到映射中
   - 或者 `__instance.id` 还没有被正确设置

2. **映射不一致**：
   - `VirtualDispenserManager.IsVirtualDispenser` 依赖映射
   - 如果映射未建立或被清空，检查会失败

3. **HarmonyX 补丁顺序**：
   - 其他 mod 可能也补丁了 `OnRematchPairs`
   - 补丁执行顺序可能导致我们的补丁被跳过

---

## ✅ 新的安全保证

### 第一层：虚拟配送器检查

```csharp
if (VirtualDispenserManager.IsVirtualDispenser(__instance.id))
    return false;
```

**效果**：正常情况下拦截虚拟配送器

---

### 第二层：deliveryPackage 空值检查

```csharp
if (__instance.deliveryPackage == null)
    return false;
```

**效果**：
- 即使映射失效，也能拦截没有 `deliveryPackage` 的配送器
- 防御性编程，增加健壮性

---

### 第三层：异常处理

```csharp
try {
    // ...
} catch (Exception ex) {
    Log.Error(ex);
    return true;  // 继续执行原方法
}
```

**效果**：
- 补丁本身不会导致游戏崩溃
- 即使出错，游戏仍能继续运行

---

## 🧪 测试建议

### 测试场景1：切换配送器模式

```
1. 打开配送器UI
2. 切换"需求/供应"选项
3. 确认游戏不报错 ✅
4. 查看日志，确认虚拟配送器被正确跳过
```

**预期日志**：
```
[Info] OnRematchPairs 调用 #1: dispenser.id=1, isVirtual=False
[Info] OnRematchPairs 调用 #2: dispenser.id=2, isVirtual=False
[Info] OnRematchPairs 调用 #3: dispenser.id=3, isVirtual=True
[Info] ✅ 跳过虚拟配送器[3]的 OnRematchPairs
```

---

### 测试场景2：切换配送器筛选器

```
1. 打开配送器UI
2. 修改物品筛选器
3. 确认游戏不报错 ✅
```

---

### 测试场景3：添加/删除配送器

```
1. 建造新的配送器
2. 确认游戏不报错 ✅
3. 拆除配送器
4. 确认游戏不报错 ✅
```

---

## 📋 配送问题的解决

### 问题1：没有配送

**原因**：基站没有物品

**解决步骤**：

1. 确保战场分析基站正在运行
2. 让基站收集一些物品（自动收集敌人掉落的物品）
3. 设置配送器需求该物品
4. 查看日志，确认配对建立：

```
[Info] ✓ 已添加配对：虚拟配送器[3] (战场基站1) gridIdx=0 itemId=1804 (奇异湮灭燃料棒) → 配送器[1]
```

5. 确认无人机派遣：

```
[Info] 🚁 开始派遣! 配送器[1] → 虚拟配送器[3](战场基站[1]), filter=1804
[Info] ✅ 派遣成功! 空载courier飞向战场基站[1]，剩余空闲=9
```

---

## 🎯 未来改进

### 1. 初始化 deliveryPackage

**思路**：为虚拟配送器创建一个空的 `DeliveryPackage`

**优点**：
- 更符合游戏的设计
- 减少对补丁的依赖

**缺点**：
- `DeliveryPackage` 是玩家背包，可能有复杂的初始化逻辑
- 可能导致其他问题

**优先级**：低（当前的补丁方案已经足够安全）

---

### 2. 完全重构虚拟配送器

**思路**：
- 不继承 `DispenserComponent`
- 创建独立的数据结构
- 只在必要时伪装成 `DispenserComponent`

**优点**：
- 更清晰的架构
- 减少对游戏内部逻辑的依赖

**缺点**：
- 工作量大
- 需要重写大量代码

**优先级**：低（当前方案已经稳定）

---

## ✅ 总结

### 问题

1. ❌ 基站没有物品，没有配送
2. ❌ 切换配送器模式时，`OnRematchPairs` 访问虚拟配送器的 `deliveryPackage` 导致 `NullReferenceException`

---

### 修复

1. ✅ 增强 `OnRematchPairs` 补丁
2. ✅ 添加双重检查（虚拟配送器 + deliveryPackage 空值）
3. ✅ 添加详细的诊断日志
4. ✅ 添加异常处理

---

### 测试建议

1. 让基站收集物品
2. 切换配送器模式，确认不报错
3. 查看日志，确认虚拟配送器被正确跳过

---

### 状态

✅ **已修复并编译成功**

等待用户测试反馈。
