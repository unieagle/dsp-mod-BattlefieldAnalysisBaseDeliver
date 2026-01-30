# 问题修复：playerMode 检查和配对持久化

## 🐛 用户报告的问题

### 问题1：第一轮配送完成后，手动放回物品不触发配送
**现象**：
1. 第一轮配送成功，所有燃料棒被配送到箱子
2. 用户手动将燃料棒放回战场基站
3. 等待很长时间，没有继续配送

### 问题2：无论配送器设置为需求还是供应，都会派遣无人机
**现象**：
1. 切换配送器的需求/供应选项
2. 无论设置为什么模式，都会从基站取货
3. 设置为"供应"时这是错误的（供应方不应该去取货）

---

## 🔍 问题分析

### 问题1的根本原因

**配对建立的条件过于严格**：

```csharp
// RefreshDispenserTraffic 中的原始代码（第249行）
if (itemId <= 0 || count <= 0) continue;  // ❌ 没有物品就跳过
```

**导致的流程**：
```
1. 第一轮配送：基站有50个燃料棒
   → RefreshDispenserTraffic 建立配对 ✅
   → 派遣无人机取货 ✅
   → 所有物品都被取走

2. 基站现在没有物品了（count = 0）
   → RefreshDispenserTraffic 再次调用
   → if (count <= 0) continue ❌
   → 跳过建立配对 ❌
   → 配对可能被游戏清除

3. 用户手动放回物品
   → AutoPickTrash 不会触发（只在基站自动拾取时触发）
   → RefreshDispenserTraffic 没有被调用
   → 没有配对，无法派遣 ❌
```

---

### 问题2的根本原因

**派遣逻辑没有检查 playerMode**：

```csharp
// InternalTick 中的原始代码（第152行）
if (__instance.idleCourierCount > 0 && __instance.pairCount > 0)  // ❌ 没有检查 playerMode
{
    派遣无人机();
}
```

**playerMode 的含义**：
- `Demand (2)` = 需求模式，需要从其他地方取货
- `Supply (1)` = 供应模式，向其他地方提供货物
- `Both (3)` = 双向模式

**问题**：
- 我们没有检查配送器的模式
- 即使配送器设置为"供应"（Supply），也会派遣无人机去取货
- 这是错误的！供应方不应该主动去取货

---

## ✅ 修复方案

### 修复1：配对持久化

**原理**：只要配送器需要该物品，就建立配对，**无论基站当前是否有货**

```csharp
// 修改前（第249行）
if (itemId <= 0 || count <= 0) continue;  // ❌ 检查 count

// 修改后
if (itemId <= 0) continue;  // ✅ 只检查 itemId，不检查 count
```

**好处**：
1. 配对会一直存在，只要配送器需要该物品
2. 即使基站暂时没货（count = 0），配对仍然保持
3. 一旦基站有货，立即派遣（因为配对已经存在）
4. 派遣时会检查 `CheckBattleBaseHasItem`，所以不会派遣到没货的基站

**流程改进**：
```
1. 第一轮配送：基站有50个燃料棒
   → 建立配对 ✅
   → 派遣无人机取货 ✅
   → 所有物品都被取走

2. 基站现在没有物品了（count = 0）
   → RefreshDispenserTraffic 再次调用
   → if (itemId > 0) 仍然成立 ✅
   → 配对依然存在 ✅

3. 用户手动放回物品
   → 配对已经存在 ✅
   → InternalTick 持续检查
   → CheckBattleBaseHasItem 返回 true ✅
   → 派遣无人机 ✅
```

---

### 修复2：检查 playerMode

**原理**：只在配送器为**需求模式**时才派遣无人机

```csharp
// 修改前（第152行）
if (__instance.idleCourierCount > 0 && __instance.pairCount > 0)

// 修改后
if (__instance.idleCourierCount > 0 && __instance.pairCount > 0 && (int)__instance.playerMode == 2)
//                                                                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
//                                                                 只在 Demand (2) 模式时派遣
```

**好处**：
1. 只有需求方（Demand）才会派遣无人机去取货
2. 供应方（Supply）不会派遣无人机
3. 符合游戏逻辑和用户预期

**不同模式的行为**：
```
配送器设置为"需求"（Demand, playerMode=2）：
  ✅ 有配对 → 派遣无人机去基站取货 → 返回配送器 → 配送给机甲

配送器设置为"供应"（Supply, playerMode=1）：
  ❌ 不派遣无人机（条件不满足）
  ✅ 游戏原生逻辑：供应方等待需求方来取货

配送器设置为"双向"（Both, playerMode=3）：
  ❌ 不派遣无人机（我们只检查 playerMode == 2）
  ⚠️ 未来可以扩展支持
```

---

## 📊 修复前后对比

### 场景1：配送完成后手动放回物品

#### 修复前
```
1. 第一轮配送完成，基站没货
2. RefreshDispenserTraffic: count=0 → 跳过建立配对
3. 配对被清除
4. 手动放回物品
5. 没有触发 RefreshDispenserTraffic
6. ❌ 没有配对，无法派遣
```

#### 修复后
```
1. 第一轮配送完成，基站没货
2. RefreshDispenserTraffic: itemId>0 → 保持配对 ✅
3. 配对依然存在
4. 手动放回物品
5. InternalTick 检测到有货
6. ✅ 立即派遣无人机
```

---

### 场景2：切换配送器模式

#### 修复前
```
配送器设置为"供应"（Supply）：
  ❌ 仍然派遣无人机去基站取货
  ❌ 违背了"供应"的含义
```

#### 修复后
```
配送器设置为"供应"（Supply）：
  ✅ 不派遣无人机
  ✅ 等待需求方来取货（游戏原生逻辑）

配送器设置为"需求"（Demand）：
  ✅ 派遣无人机去基站取货
  ✅ 符合预期
```

---

## 🎯 技术细节

### 配对的生命周期

**游戏原生逻辑**：
1. 配对在 `RefreshDispenserTraffic` 中建立
2. 配对在 `ClearPairs` 中清除（例如配送器 filter 改变时）
3. 配对在某些情况下可能被游戏自动清除

**我们的改进**：
1. 配对更加持久（只要 itemId 存在就保持）
2. 配对不受 count 变化影响
3. 派遣时才检查是否有货

---

### playerMode 的完整定义

```csharp
enum EPlayerDeliveryMode
{
    None = 0,      // 无
    Supply = 1,    // 供应
    Demand = 2,    // 需求
    Both = 3       // 双向
}
```

**我们的策略**：
- 只支持 `Demand (2)` 模式派遣到基站
- 其他模式使用游戏原生逻辑

---

## 🧪 测试场景

### 测试1：正常配送
1. 基站有物品（例如50个燃料棒）
2. 配送器设置为需求模式，过滤器设置为燃料棒
3. 配送器有无人机
4. ✅ 应该立即派遣无人机取货

### 测试2：配送完成后手动放回
1. 完成第一轮配送（基站物品清空）
2. 等待几秒（配对应该依然存在）
3. 手动将物品放回基站
4. ✅ 应该在60帧（1秒）内派遣无人机

### 测试3：切换配送器模式
1. 配送器设置为需求模式，正常配送
2. 切换为供应模式
3. ✅ 不应该派遣无人机
4. 切换回需求模式
5. ✅ 应该恢复派遣

### 测试4：基站暂时没货
1. 配对已建立
2. 基站物品被取完（count = 0）
3. ✅ 配对应该依然存在
4. ✅ 不会派遣无人机（CheckBattleBaseHasItem 返回 false）
5. 基站重新有货
6. ✅ 立即派遣无人机

---

## 📝 修改的文件

### 1. DispenserComponent_InternalTick_Patch.cs
**位置**：第153行

**修改**：
```csharp
// 添加 playerMode 检查
if (...  && (int)__instance.playerMode == 2)
```

---

### 2. PlanetTransport_RefreshDispenserTraffic_NEW.cs
**位置**：第249行

**修改**：
```csharp
// 修改前
if (itemId <= 0 || count <= 0) continue;

// 修改后
if (itemId <= 0) continue;
```

---

### 3. BattleBaseComponent_AutoPickTrash_Patch.cs
**位置**：第62行

**修改**：
```csharp
// 添加从0到非0的触发条件
bool shouldRefresh = false;
if (itemTypeCount > _lastItemCount)
{
    shouldRefresh = true;
}
else if (_lastItemCount == 0 && itemTypeCount > 0)
{
    shouldRefresh = true;
}
```

---

## ✅ 预期效果

### 日志输出

**配对建立**（无论是否有货）：
```log
RefreshDispenser: battleBaseId=1, itemId=1804 (count=0)
✓ 已添加配对（或跳过已存在的配对）
```

**派遣检查**（只在 Demand 模式）：
```log
🔍 派遣检查: dispenser[1] playerMode=2 (Demand)
✅ 发现虚拟配送器配对!
🚀 准备派出无人机!
```

**切换为 Supply 模式**：
```log
🔍 派遣检查: dispenser[1] playerMode=1 (Supply)
❌ 不满足派遣条件（playerMode != 2）
```

---

## 🎓 学到的经验

### 1. 配对应该持久化
- 不要过早清除配对
- 配对的存在不会造成性能问题
- 派遣时再检查实际条件

### 2. 必须检查所有相关状态
- `playerMode` 决定了配送器的角色
- 不能只检查"有配对"就派遣
- 需要确保行为符合用户预期

### 3. 自动化触发的局限性
- `AutoPickTrash` 只在特定时机触发
- 不能依赖它来捕获所有物品变化
- 持久化配对是更可靠的方案

---

## 🚀 总结

两个关键修复：

1. **配对持久化**：不检查 `count`，只检查 `itemId`
   - 配对更持久
   - 手动放回物品后立即生效

2. **playerMode 检查**：只在需求模式派遣
   - 符合游戏逻辑
   - 符合用户预期

现在 mod 应该能正确处理这两种情况了！🎉
