# 战场分析基站配送支持 Mod

## 简介

本 Mod 让**物流配送器**（箱子上的无人机港）能够从**战场分析基站**的战利品仓库中取物。

### 使用场景
- 战场分析基站里堆满了敌人掉落的物品（硅块、晶格硅等）
- 在箱子上放置物流配送器，设置为「需求」模式，选择需要的物品
- 配送器会自动派出无人机前往基站取货，然后返回配送给其他建筑或机甲

### 核心特性
- ✅ 无人机从配送器起飞，飞向战场分析基站取货，返回配送器
- ✅ 完整的飞行动画和物流逻辑
- ✅ 支持现有存档，无需重建基站
- ✅ 每秒自动派出无人机，直到基站清空或配送器无空闲无人机
- ✅ 基站拆除后，飞行中的空载无人机立即掉头返回

### 已知问题

#### 1. 拆除配送器时物品丢失（游戏原版Bug）

⚠️ **问题描述**：
- 这是**游戏原版的Bug**（不是本mod引入）
- 影响所有配送器间物流（包括原版游戏）
- 拆除配送器时，飞行中无人机携带的物品会丢失

**建议**：拆除配送器前，等待所有无人机返回（idle = 10）

**不修复原因**：修复复杂，Harmony Patch无法正确应用，影响范围有限

#### 2. 虚拟配送器在监控面板中显示

✅ **已完美修复**：
- 使用双重 UI Patch 拦截机制
- 虚拟配送器**完全不显示**在监控面板中 ✅
- **没有任何报错**（包括滚动时） ✅
- 游戏可以正常继续运行 ✅

**修复方案**：
1. **`TakeObjectEntryFromPool` Prefix Patch**：
   - 在 UI 创建配送器条目时拦截
   - 检测到虚拟配送器时直接跳过
   
2. **`DetermineEntryVisible` Prefix Patch**：
   - 在 UI 确定可见条目时过滤
   - 从列表中移除虚拟配送器

**效果**：
- ✅ 打开监控面板正常
- ✅ 滚动列表完全正常
- ✅ 只显示真实配送器
- ✅ 没有任何 NullReferenceException
- ✅ 所有配送功能正常

---

## 实现方法

### 核心原理

本 Mod 通过 Harmony 补丁，在游戏的本地物流系统中添加对战场分析基站的支持。关键思路是：
1. **配对阶段**：将战场分析基站伪装成「物流配送器」的供货源
2. **派出阶段**：手动创建无人机（`CourierData`），设置从配送器飞向基站
3. **取货阶段**：在无人机到达前拦截，从基站取货并设置返回
4. **返回阶段**：游戏接管，无人机返回配送器并自动卸货

### 技术实现

#### 1. 配对补丁 (`PlanetTransport.RefreshDispenserTraffic`)

在游戏刷新配送器配对时，扫描所有战场分析基站：

```csharp
// 遍历战场分析基站的战利品仓库
foreach (battleBase in defenseSystem.battleBases) {
    foreach (grid in battleBase.storage.grids) {
        if (grid.itemId matches dispenser.filter) {
            // 使用特殊负数ID：-(battleBaseId * 10000 + gridIdx)
            dispenser.AddPair(specialSupplyId, gridIdx, dispenserId, 0);
        }
    }
}
```

**关键点**：使用负数ID（如 `-10000`）标记战场分析基站，避免与正常配送器ID冲突。

#### 2. 派出补丁 (`DispenserComponent.InternalTick` - Prefix)

每60帧（约1秒）检查一次，派出无人机：

```csharp
// 检查配对中是否有战场分析基站
if (pair.supplyId <= -10000 && idleCourierCount > 0) {
    // 手动创建 CourierData
    courier.begin = dispenserPos;  // 起点：配送器
    courier.end = basePos;         // 终点：基站
    courier.endId = 0;             // 重要：设为0避免游戏处理
    courier.direction = 1f;        // 正向飞行
    courier.t = 0f;                // 从起点开始
    courier.maxt = CalculateDistance(dispenserPos, basePos);
    courier.itemCount = 0;         // 空载
    
    // 在 order.otherId 中保存特殊ID，用于识别
    order.otherId = -(battleBaseId * 10000 + gridIdx);
}
```

**关键点**：
- `endId = 0` 防止游戏尝试访问 `dispenserPool[endId]` 导致错误
- `order.otherId` 保存特殊ID，用于后续识别这是我们的无人机
- 正确计算球面距离作为 `maxt`

#### 3. 取货拦截 (`DispenserComponent.InternalTick` - Prefix)

在无人机到达基站前（`t >= maxt - 0.2`），拦截并处理：

```csharp
if (order.otherId <= -10000 && courier.t >= courier.maxt - 0.2f) {
    // 解析特殊ID
    int battleBaseId = (-order.otherId) / 10000;
    int gridIdx = (-order.otherId) % 10000;
    
    // 从基站取货
    battleBase.storage.TakeItem(gridIdx, courierCarries, out actualCount, out inc);
    
    // 设置返回状态
    courier.itemCount = actualCount;  // 装载货物
    courier.direction = -1f;          // 反向（返回）
    courier.t = maxt;                 // 重置 t
    courier.endId = 0;                // 保持为0
    order.otherId = 0;                // 清除特殊标记
}
```

**关键点**：
- 提前拦截（`t >= maxt - 0.2`），在游戏的到达处理之前
- 游戏的到达逻辑会尝试 `grids[-(endId+1)]`，如果 `endId = -10000` 会导致数组越界
- 通过提前拦截并清除 `endId` 和 `otherId`，避免游戏处理这个特殊无人机

#### 4. 定期刷新

每300帧（约5秒）自动调用 `RefreshDispenserTraffic`，确保：
- 物品放回基站后能重新创建配对
- 支持动态变化的战场分析基站内容

---

## 学习到的游戏细节

### 1. 物流配送器（DispenserComponent）架构

#### 配对系统（Pairs）
- `pairs[]`：供需配对数组
- `supplyId` / `demandId`：供货方/需求方的ID
- 正数ID：指向其他配送器（`dispenserPool[id]`）
- 负数ID：原本用于机甲物流槽（`-(slotIndex + 1)`）
- 我们利用更小的负数（`-10000`）标记战场分析基站

#### 无人机数据（CourierData）
- `begin` / `end`：起点/终点的3D坐标（Vector3）
- `direction`：`1f` = 正向（`end → begin`），`-1f` = 反向（`begin → end`）
- `t` / `maxt`：飞行进度，`t` 从 `0` 到 `maxt`
- `endId`：目标ID，游戏用于查找目标实体
- 结构体（struct），需要通过反射修改后重新赋值

#### 订单数据（DeliveryLogisticOrder）
- `otherId`：配对的另一方ID
- `thisOrdered` / `otherOrdered`：预订数量
- 用于追踪物流状态

### 2. 坐标系统

#### 球面坐标
戴森球计划的行星是球体，距离计算使用球面几何：

```csharp
// 计算两点间的球面距离
double r1 = Vector3.Magnitude(pos1);  // 到行星中心的距离
double r2 = Vector3.Magnitude(pos2);
double cosAngle = Vector3.Dot(pos1, pos2) / (r1 * r2);
double arcDist = Math.Acos(cosAngle) * ((r1 + r2) * 0.5);  // 弧长
double maxt = Math.Sqrt(arcDist^2 + (r1 - r2)^2);          // 3D距离
```

这个距离用作无人机的 `maxt`，决定飞行时间。

### 3. InternalTick 执行时序

#### 关键发现
游戏在 `InternalTick` 中处理无人机到达（`t >= maxt`）：

```csharp
if (courier.t >= courier.maxt) {
    if (courier.endId < 0) {
        // 认为是机甲物流槽
        int slotIndex = -(endId + 1);
        grids[slotIndex]  // 如果 endId = -10000，会导致数组越界！
    }
}
```

**解决方案**：
- 在 **Prefix** 中提前拦截（`t >= maxt - 0.2`）
- 处理完成后设置 `endId = 0`，游戏会跳过到达处理
- 利用 `direction = -1f` 触发返回逻辑

### 4. "跟踪玩家"机制

当 `endId < 0 && direction > 0f` 时，游戏认为无人机正在飞向玩家：

```csharp
if (endId < 0 && direction > 0f) {
    // 每帧更新 end = playerPos（跟踪机甲位置）
}
```

**影响**：
- 如果使用负数 `endId` 且 `direction = 1f`，无人机会被强制飞向机甲
- 我们通过 `endId = 0` 避免触发这个逻辑

### 5. 战场分析基站（BattleBaseComponent）

#### 结构
- `storage`：`StorageComponent`，战利品仓库
- `storage.grids[]`：物品格子数组（60个格子）
- 每个 grid：`itemId`, `count`, `inc`（增产）

#### 访问路径
```csharp
PlanetFactory.defenseSystem
  → DefenseSystem.battleBases (ObjectPool<BattleBaseComponent>)
    → ObjectPool.buffer[] (实际的数组)
      → BattleBaseComponent
        → storage.grids[]
```

### 6. 反射技巧

由于很多字段是 `private` 或跨程序集，需要使用反射：

```csharp
// 获取字段
var field = type.GetField("fieldName", 
    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

// 修改 struct（重要！）
object courierData = workCourierDatas.GetValue(index);
courierData.GetType().GetField("t")?.SetValue(courierData, newValue);
workCourierDatas.SetValue(courierData, index);  // 必须重新设置！
```

**关键点**：`CourierData` 是值类型（struct），修改后必须重新赋值回数组。

### 7. Harmony 补丁策略

#### Prefix vs Postfix
- **Prefix**：在原方法之前执行，可以修改参数、阻止原方法执行
- **Postfix**：在原方法之后执行，可以修改返回值

**本 Mod 的选择**：
- `RefreshDispenserTraffic`：**Postfix**（添加配对）
- `InternalTick`：**Prefix**（拦截无人机，避免游戏错误处理）

#### 补丁时机
- **太早**：游戏还未初始化，数据为 null
- **太晚**：游戏已经处理，无法拦截
- **正确时机**：`t >= maxt - 0.2`，在游戏检查 `t >= maxt` 之前

### 8. 多配送器处理

#### 全局计数器问题
最初的错误：每个配送器的 `InternalTick` 都调用 `RefreshDispenserTraffic`，导致：
- 每帧被调用多次
- 频繁修改 `pairs[]` 数组
- 引发竞态条件和数组越界

**解决方案**：
```csharp
if (__instance.id == 1) {  // 只在第一个配送器中处理
    _refreshCounter++;
    if (_refreshCounter >= REFRESH_INTERVAL) {
        RefreshDispenserTraffic(...);
    }
}
```

---

## 开发过程总结

### 架构演进

1. **方案0（失败）**：尝试为战场分析基站创建虚拟的 `StationComponent`
   - 问题：与游戏UI冲突，打开战场分析基站会崩溃

2. **方案A（成功）**：直接操作 `DispenserComponent`
   - 手动创建 `CourierData`
   - 使用负数ID标记
   - Prefix 拦截到达处理

3. **调试历程**：
   - 无人机从基站起飞 → `begin/end` 顺序错误
   - 无人机飞向机甲 → `endId < 0` 触发跟踪玩家
   - 无人机不可见 → `maxt` 计算错误
   - 游戏报错 → 数组越界，需要提前拦截
   - 所有无人机被占用 → 需要限制频率

### 关键技术难点

1. **理解 `direction` 的含义**
   - 不是飞行方向，而是 `t` 的变化方向
   - `direction = 1f`：`t++`，从 `end` 飞向 `begin`
   - `direction = -1f`：`t--`，从 `begin` 飞向 `end`

2. **球面距离计算**
   - 不能使用简单的欧几里得距离
   - 必须考虑行星曲率

3. **Struct 的反射修改**
   - 值类型需要获取、修改、重新赋值三步

4. **时序控制**
   - 太早：数据未初始化
   - 太晚：游戏已处理，产生错误
   - 找到 `t >= maxt - 0.2` 的甜蜜点

---

## 项目结构

```
BattlefieldAnalysisBaseDeliver/
├── BattlefieldAnalysisBaseDeliver.csproj
├── Plugin.cs                                          # 主插件入口
├── PluginInfo.cs                                     # 插件信息
├── Patches/
│   ├── BattlefieldBaseHelper.cs                      # 辅助方法
│   ├── PlanetTransport_RefreshDispenserTraffic_NEW.cs  # 配对补丁
│   └── DispenserComponent_InternalTick_Patch.cs      # 派出/取货补丁
├── GameCodeReference/                                # 反编译的游戏代码（参考）
│   ├── DispenserComponent.cs
│   ├── CourierData.cs
│   ├── BattleBaseComponent.cs
│   └── ...
└── README.md
```

---

## 使用说明

### 安装

1. 确保已安装 **BepInEx 5.x** (x64)
2. 将 `BattlefieldAnalysisBaseDeliver.dll` 放入 `BepInEx\plugins\` 文件夹
3. 启动游戏

### 使用

1. 建造战场分析基站，等待战利品积累
2. 在附近的箱子上放置物流配送器
3. 设置配送器为「需求」模式，选择战场分析基站里有的物品
4. 无人机会自动前往基站取货

### 配置

无需配置，Mod 会自动识别战场分析基站（`protoId = 2318`）。

---

## 调试和开发

### 查看日志

日志位置：`BepInEx\LogOutput.log`

启用详细日志：在 `BepInEx\config\` 中修改插件配置。

### 反编译工具

- **dnSpy**：推荐，支持调试
- **ILSpy / dotPeek**：仅查看代码

反编译文件：`DSPGAME_Data\Managed\Assembly-CSharp.dll`

### 推荐的辅助 Mod

- **UnityExplorer**：实时查看游戏对象（F7）
- **ScriptEngine**：热重载代码（F6）
- **CommonAPI**：提供常用API

---

## 已知问题

1. **性能**：大量战场分析基站可能影响性能（每5秒扫描一次）
2. **兼容性**：与其他修改物流系统的 Mod 可能冲突
3. **存档兼容**：完全兼容现有存档

---

## 贡献

欢迎提交 Issue 和 Pull Request！

特别感谢：
- BepInEx 和 HarmonyX 项目
- DSP Modding 社区

---

## 许可证

本项目采用 MIT 许可证。

---

## 更新日志

### v1.0.0 (2026-01-28)
- ✅ 实现基本功能：配送器从战场分析基站取货
- ✅ 完整的无人机飞行动画
- ✅ 支持现有存档
- ✅ 自动派出无人机（每秒1个）
- ✅ 修复游戏报错（数组越界）
- ✅ 定期刷新配对（每5秒）
