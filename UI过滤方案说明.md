# UI 过滤方案说明

## 问题背景

虚拟配送器使用战场基站的 `entityId`，但战场基站的实体 `dispenserId=0`（不是配送器）。这导致：

1. **监控面板显示虚拟配送器**：UI 会尝试显示虚拟配送器，但查找失败
2. **UI 错误**：`UIControlPanelDispenserEntry.OnSetTarget()` 访问 `entityPool[entityId].dispenserId` 返回 `0`，导致 `dispenserPool[0]` 访问失败（`NullReferenceException`）

## 解决方案：UI 过滤

### 核心思路

**在监控面板的数据源层面过滤掉虚拟配送器**，让 UI 完全看不到它们的存在。

### 实现方式

**Patch：`UIControlPanelWindow.DetermineFilterResults`**

该方法负责扫描所有星球、工厂、配送器等，构建监控面板的显示列表 `results`。

#### 1. Prefix（准备阶段）

- **时机**：在 `DetermineFilterResults` 执行前
- **目标**：收集所有虚拟配送器的 `entityId`
- **逻辑**：
  1. 遍历所有工厂的 `dispenserPool`
  2. 使用 `VirtualDispenserManager.IsVirtualDispenser(dispenserId)` 检查是否是虚拟配送器
  3. 如果是，记录其 `entityId` 到缓存 `HashSet<int> _virtualDispenserEntityIds`

#### 2. Postfix（过滤阶段）

- **时机**：在 `DetermineFilterResults` 执行后
- **目标**：从 `results` 列表中移除虚拟配送器
- **逻辑**：
  1. 从后往前遍历 `results` 列表
  2. 检查每个 `ControlPanelTarget` 的 `entryType` 是否为 `Dispenser` (4)
  3. 如果是配送器，检查其 `objId` (即 `entityId`) 是否在 `_virtualDispenserEntityIds` 中
  4. 如果是虚拟配送器，从 `results` 列表中移除

### 为什么从后往前遍历？

从后往前遍历是因为移除元素时，不会影响后续元素的索引：
- 正向遍历：移除 `i` 后，`i+1` 变成了 `i`，导致跳过元素
- 反向遍历：移除 `i` 后，只影响 `i` 之前的元素，而我们已经遍历过了

### 为什么使用 entityId 而不是 dispenserId？

因为 `ControlPanelTarget.objId` 存储的是 `entityId`，而不是 `dispenserId`。游戏的逻辑是：
1. `DetermineFilterResults` 扫描 `dispenserPool`，创建 `ControlPanelTarget(entityId, ...)`
2. `UIControlPanelDispenserEntry.OnSetTarget()` 通过 `entityId` 查找 `entity`，再通过 `entity.dispenserId` 查找配送器

## 方案优势

1. **简单直接**：直接操作数据源，不需要创建假实体
2. **无 UI 副作用**：UI 完全看不到虚拟配送器，不会触发任何相关逻辑
3. **易于维护**：只需要一个补丁，逻辑清晰
4. **兼容性好**：不修改游戏数据结构，只过滤显示

## 方案对比

### 方案 A：假实体（已放弃）

- **思路**：为每个虚拟配送器创建一个假 `EntityData`，设置 `dispenserId` 指向虚拟配送器
- **问题**：
  - 实现复杂，需要管理假实体的生命周期
  - 可能与游戏的其他系统冲突
  - 需要处理存档加载、星球切换等边界情况
  - 仍然会显示虚拟配送器（不符合需求）

### 方案 B：UI 过滤（当前方案）✅

- **思路**：在 `DetermineFilterResults` 中过滤掉虚拟配送器
- **优点**：
  - 实现简单，只需要一个补丁
  - 不修改游戏数据，只影响显示
  - 虚拟配送器完全对用户不可见
  - 易于调试和维护

## 测试要点

1. **监控面板正常打开**：不应该有任何错误
2. **真实配送器可见**：所有真实配送器应该正常显示和选择
3. **虚拟配送器不可见**：监控面板中看不到任何虚拟配送器
4. **功能正常**：配送功能不受影响（虚拟配送器在后台正常工作）
5. **存档加载**：加载存档后，虚拟配送器仍然被正确过滤
6. **多星球**：切换星球时，过滤逻辑正确应用

## 日志检查

启用 Debug 日志后，应该看到：
```
[Info] 🗑️ 从监控面板移除虚拟配送器 (entityId=91)
[Info] 监控面板：已隐藏 1 个虚拟配送器
```

## 代码位置

- **补丁文件**：`Patches/UIControlPanelWindow_DetermineFilterResults_Patch.cs`
- **注册位置**：`Plugin.cs` 的 `Awake()` 方法（Patch 8）
