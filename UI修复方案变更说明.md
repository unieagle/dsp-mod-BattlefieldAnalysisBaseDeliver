# UI 修复方案变更说明

## 时间
2026-01-30 14:00

## 变更原因

用户报告：
1. 日志中仍然有 `DetermineFilterResults Postfix 被调用` 的输出
2. 日志显示 `entity.dispenserId=0` 的虚拟配送器仍然被检测到
3. 用户倾向于**在面板中隐藏虚拟配送器**，而不是让它们可见

经过分析，发现：
- **假实体（Dummy Entity）方案**虽然理论上可行，但：
  1. 实现复杂（需要管理假实体生命周期、存档恢复等）
  2. 虚拟配送器仍然会在 UI 中显示（不符合用户需求）
  3. 增加了不必要的游戏数据污染
- **UI 过滤方案**更符合用户需求：
  1. 虚拟配送器完全对用户不可见
  2. 实现简单，只需要一个 UI 补丁
  3. 不修改游戏数据结构

## 方案对比

### 假实体方案（已放弃）

```
虚拟配送器
  ↓ entityId
假实体（Dummy Entity）
  ↓ dispenserId → virtualDispenserId
UI 可以正确访问
但虚拟配送器会显示在面板中 ❌
```

**问题**：
- 用户不希望看到虚拟配送器
- 实现复杂，需要创建和管理假实体

### UI 过滤方案（当前方案）✅

```
虚拟配送器
  ↓ entityId → battleBaseEntityId
战场基站实体
  ↓ dispenserId = 0
    
UI Patch:
  1. Prefix: 收集所有虚拟配送器的 entityId
  2. Postfix: 从 results 列表中移除这些 entityId
    
结果：虚拟配送器完全不显示 ✅
```

**优点**：
- 虚拟配送器对用户完全不可见
- 实现简单，只需要一个 UI 补丁
- 不修改游戏数据，只过滤显示

## 代码变更

### 1. 新增文件

**`Patches/UIControlPanelWindow_DetermineFilterResults_Patch.cs`**
- Prefix：收集虚拟配送器的 entityId
- Postfix：从 results 列表中移除虚拟配送器

### 2. 修改文件

**`Plugin.cs`**
- 注册新的 UI 过滤补丁（Patch 8）
- 更新加载日志：`使用虚拟配送器方案（UI 过滤）`

**`Patches/VirtualDispenserManager.cs`**
- 移除假实体创建逻辑（lines 170-278）
- 简化虚拟配送器的 `entityId` 初始化：直接使用战场基站的 entityId

**`README.md`**
- 更新 "已知问题" 部分，说明 UI 过滤方案
- 更新版本日志

**`UI过滤方案说明.md`** (新增)
- 详细说明 UI 过滤方案的原理
- 对比假实体方案和 UI 过滤方案
- 提供测试要点和日志检查方法

## 测试要点

1. **编译成功** ✅
   - 无警告，无错误
   - DLL 文件生成：`bin\Debug\net472\BattlefieldAnalysisBaseDeliver.dll`

2. **需要用户测试**：
   - 关闭游戏
   - 复制新的 DLL 到 `r2modmanPlus-local\...\BepInEx\plugins\BattlefieldAnalysisBaseDeliver\`
   - 启动游戏
   - 打开监控面板，验证：
     - 面板正常打开，无崩溃
     - 所有真实配送器可见且可选择
     - **虚拟配送器不可见**
     - 配送功能正常（虚拟配送器在后台工作）

3. **日志检查**：
   - 应该看到：`✅ 加载完成！使用虚拟配送器方案（UI 过滤）。`
   - 应该看到：`已对 UIControlPanelWindow.DetermineFilterResults 应用补丁（隐藏虚拟配送器）。`
   - Debug 模式下，打开监控面板时应该看到：
     - `🗑️ 从监控面板移除虚拟配送器 (entityId=...)`
     - `监控面板：已隐藏 X 个虚拟配送器`

## 预期效果

- ✅ 监控面板正常打开
- ✅ 滚动无崩溃
- ✅ 真实配送器正常显示
- ✅ 虚拟配送器完全不可见
- ✅ 配送功能正常（虚拟配送器在后台工作）
- ✅ 无任何 `NullReferenceException`

## 下一步

等待用户测试反馈。如果仍有问题，可以：
1. 检查日志，确认 UI 补丁是否正确应用
2. 增加更详细的 Debug 日志
3. 检查 `results` 列表的移除逻辑是否正确
