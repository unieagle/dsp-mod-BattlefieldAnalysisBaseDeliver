# Patch 策略总结

## 🎯 虚拟配送器的参数配置

### 核心配置（决定行为）

```csharp
虚拟配送器 = new DispenserComponent {
    // === 标识 ===
    id = virtualDispenserId,        // 正数ID (例如: 2, 3, 4...)
    entityId = battleBase.entityId, // 战场基站的实体ID (用于定位)
    
    // === 模式和过滤器（核心！）===
    filter = 0,                     // 0 = 不过滤，所有物品都可供应
    playerMode = Supply (1),        // 供应模式（作为供应源）
    storageMode = None (0),         // 不使用存储模式
    
    // === 无人机（关键！）===
    idleCourierCount = 0,           // 0 个空闲无人机
    workCourierCount = 0,           // 0 个工作中无人机
    workCourierDatas = [],          // 空数组（不是 null）
    orders = [],                    // 空数组
    
    // === 其他必需字段 ===
    pairs = [],                     // 空数组（游戏不会向虚拟配送器添加配对）
    playerPairCount = 0,
    holdupPackage = [],             // 空数组（防止 OnRematchPairs 崩溃）
    storage = null,                 // 虚拟配送器没有存储
    deliveryPackage = null          // 虚拟配送器没有玩家背包
};
```

---

## 📊 参数影响分析

### 1. filter = 0（不过滤）

**含义**：虚拟配送器可以提供任何物品

**游戏逻辑**：
```csharp
// RefreshDispenserTraffic 中的配对条件
if (需求配送器.filter == 供应配送器.filter)  // 如果供应方 filter=0，这个条件如何判断？
{
    AddPair(供应配送器, 需求配送器);
}
```

**问题**：
- ❌ `filter=0` 可能导致游戏**不匹配**任何需求！
- ✅ 我们的 Patch 中手动检查 `battleBase.grids[x].itemId == 需求配送器.filter`

**解决方案**：
我们的 Patch 绕过了游戏原生的 filter 匹配逻辑，直接检查基站是否有该物品。

---

### 2. playerMode = Supply（供应模式）

**含义**：虚拟配送器作为供应源

**游戏逻辑**：
```csharp
// RefreshDispenserTraffic 中的配对条件
if (供应配送器.playerMode == Supply && 需求配送器.playerMode == Demand)
{
    AddPair(...);
}
```

**影响**：
- ✅ 正确：虚拟配送器作为供应源
- ✅ 需求配送器必须设置为 Demand 模式

---

### 3. idleCourierCount = 0（无空闲无人机）

**含义**：虚拟配送器没有无人机

**游戏逻辑**：
```csharp
// InternalTick 中的派遣条件
if (配送器.idleCourierCount > 0)
{
    派出无人机();
}
```

**影响**：
- ❌ 虚拟配送器**永远不会主动派出无人机**
- ✅ **需求方配送器**必须有空闲无人机
- ✅ 我们的逻辑：检查**需求方**的 `idleCourierCount`

**这是设计的关键**：
```
虚拟配送器 = 被动的供应源（不派出无人机）
需求配送器 = 主动派出无人机去虚拟配送器（战场基站）取货
```

---

### 4. deliveryPackage = null（无玩家背包）

**含义**：虚拟配送器不是真实建筑，没有玩家背包关联

**游戏逻辑**：
```csharp
// OnRematchPairs 中
DeliveryPackage.GRID[] grids = this.deliveryPackage.grids;  // ❌ NullReferenceException!
```

**影响**：
- ❌ 游戏调用 `OnRematchPairs` 会崩溃
- ✅ 已通过 Patch 跳过虚拟配送器

---

## 🔧 需要 Patch 的游戏方法总结

| 游戏方法 | 访问的问题字段 | Patch 策略 | 状态 |
|---------|---------------|-----------|------|
| `OnRematchPairs` | `deliveryPackage` | Prefix 跳过虚拟配送器 | ✅ 已实现 |
| `UIControlPanelDispenserEntry.OnSetTarget` | 尝试显示虚拟配送器 | Prefix 跳过 | ✅ 已实现 |
| `UIControlPanelObjectEntry.InitFromPool` | 尝试初始化虚拟配送器UI | Prefix 跳过 | ✅ 已实现 |
| `UIControlPanelWindow.TakeObjectEntryFromPool` | 尝试创建虚拟配送器条目 | Prefix 返回 null | ✅ 已实现 |
| `InternalTick` | 所有字段 | Prefix 主动控制派遣 | ✅ 已实现 |

---

## 📋 完整 Patch 列表

| # | 目标方法 | 类型 | 作用 |
|---|---------|------|------|
| 1 | `PlanetFactory.Init` | Postfix | 创建虚拟配送器 |
| 2 | `PlanetFactory.Import` | Postfix | 加载存档创建虚拟配送器 |
| 3 | `PlanetFactory.Free` | Prefix | 清理虚拟配送器映射 |
| 4 | `PlanetTransport.RefreshDispenserTraffic` | Postfix | 建立配对 |
| 5 | `DispenserComponent.InternalTick` | Prefix | 派遣和取货 |
| 6 | `DispenserComponent.OnRematchPairs` | Prefix | 跳过虚拟配送器 |
| 7 | `BattleBaseComponent.AutoPickTrash` | Postfix | 物品变化监控 |
| 8 | `UIControlPanelDispenserEntry.OnSetTarget` | Prefix | 跳过虚拟配送器UI |
| 9 | `UIControlPanelObjectEntry.InitFromPool` | Prefix | 跳过虚拟配送器UI |
| 10 | `UIControlPanelWindow.TakeObjectEntryFromPool` | Prefix | 跳过虚拟配送器UI |

---

## 🎯 虚拟配送器的"最小化设计"

### 设计原则
**虚拟配送器只提供两个功能：**
1. ✅ **作为配对系统的供应源标识**（通过正数ID）
2. ✅ **提供战场基站的位置**（通过 entityId）

**不需要的功能：**
- ❌ 派出无人机（无人机由需求方派出）
- ❌ 存储物品（物品在战场基站的 storage 中）
- ❌ UI显示（通过 Patch 跳过）
- ❌ 玩家背包交互（通过 Patch 跳过）

### 为什么这样设计安全

```
游戏系统的视角：
虚拟配送器看起来是一个"正常的配送器"
- 有正数ID ✅
- 在 dispenserPool 中 ✅
- entityId 指向有效实体 ✅
- 基本字段都已初始化 ✅

但实际上：
- 它不派出无人机（idleCourierCount = 0）
- 它不显示在UI（被 Patch 跳过）
- 它不处理复杂逻辑（被 Patch 跳过）
- 它只是一个"占位符"，代表战场基站
```

---

## 🔍 当前问题诊断

### 从日志看到

```
✅ 虚拟配送器创建成功：dispenser[2] (战场基站1)
✅ 配对成功：虚拟配送器[2] → 配送器[1]
✅ 配送器[1]设置正确：filter=1804 (氢燃料棒), playerMode=2 (需求)
✅ 战场基站有货：itemId=1804, count=50

❌ 但是没有派遣无人机！
```

### 可能的原因

1. **配送器[1]没有无人机**
   - 需要检查 `dispenser[1].idleCourierCount`
   - 可能用户没有给配送器添加无人机

2. **InternalTick 没有执行到派遣逻辑**
   - 频率控制问题？
   - 条件检查失败？

3. **配对数据存储位置问题**
   - 配对是否真的添加到了 `dispenser[1].pairs[]`？
   - `playerPairCount` 是否正确？

---

## 🚀 下一步

### 1. 重启游戏并测试

新的诊断日志会输出：
```
✅ 派遣检查 #1: dispenser[1] idle=X, work=Y, pairCount=Z
✅ 检查 pair[0]: supplyId=2, isVirtual=true
✅ 发现虚拟配送器配对!
✅ 准备派出无人机!
```

### 2. 检查配送器的无人机

在游戏中：
1. 打开配送器[1]的UI
2. 查看右上角的无人机数量
3. 确认是否有空闲无人机

**如果没有无人机：**
- 点击"+"按钮添加无人机
- 或者启用"自动补充无人机"

### 3. 查看新的日志

新的日志会详细显示：
- 每次派遣检查的状态
- 配对数据
- 为什么不派遣（如果不满足条件）

---

## 📖 虚拟配送器参数总结

我已经创建了 `虚拟配送器参数说明.md`，详细解释了所有参数：

### 关键参数
- ✅ `filter = 0`：不过滤（通过我们的 Patch 处理）
- ✅ `playerMode = Supply`：供应模式
- ✅ `idleCourierCount = 0`：无无人机（需求方负责派遣）
- ✅ `entityId = battleBase.entityId`：指向战场基站

### 需要 Patch 的场景
- ✅ `OnRematchPairs`：访问 `deliveryPackage`
- ✅ UI方法：尝试显示虚拟配送器
- ✅ `InternalTick`：主动控制派遣逻辑

---

请**重启游戏**并测试，新的诊断日志会告诉我们为什么没有派遣无人机。同时，请**检查配送器是否有无人机**（这是最可能的原因）！🚀