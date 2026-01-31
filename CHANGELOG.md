# 更新日志

## [2.0.0] - 2026-01-31 - 重大架构升级 🎉

### 完全重写：基站直接派遣架构

这是一个**重大版本升级**，完全重写了底层架构。从虚拟配送器方案改为基站直接派遣方案。

### 🚀 核心变更

#### 架构重写
- ❌ **移除虚拟配送器**：不再创建虚拟配送器参与游戏物流系统
- ✅ **基站直接派遣**：战场基站拥有独立的 10 个无人机
- ✅ **智能调度**：基于紧急度（库存百分比）和距离的优先级算法
- ✅ **事件驱动**：只在基站物品变化时工作，零性能损耗

#### 新增功能
- ✅ **全局管理器**：`BattleBaseLogisticsManager` 统一管理所有星球的所有基站
- ✅ **存档安全**：
  - 存档前：自动返还所有在途物品到基站
  - 加载后：自动重新派遣无人机
- ✅ **旧版本兼容**：
  - 自动识别并清理 v1.x 遗留的虚拟配送器
  - 避免坏档
  - 无缝升级

#### 渲染系统
- ✅ **完整的无人机渲染**：
  - Hook `LogisticCourierRenderer.Update`
  - 将基站无人机注入游戏渲染系统
  - 视觉效果与游戏原生一致

### 🔧 优化

#### 代码质量
- ✅ **大幅精简**：
  - 代码量：~1500 行 → ~1200 行（-20%）
  - Patch 文件：9 个 → 5 个（-44%）
  - 删除 768 行冗余 Helper 代码
- ✅ **命名统一**：所有 Patch 文件统一使用 `_Patch` 后缀
- ✅ **0 编译警告**：修复所有可空引用警告
- ✅ **移除所有 UI Patch**：不再修改任何 UI，零干扰

#### 性能优化
- ✅ **检测周期**：60 游戏帧（约 1 秒）
- ✅ **触发条件**：仅在基站物品变化时扫描
- ✅ **时间复杂度**：O(N)，N = 配送器数量

### 📝 文档

- ✅ **完全重写 README**：反映新架构
- ✅ **添加技术说明**：详细解释实现原理
- ✅ **添加 FAQ**：常见问题解答
- ✅ **添加性能说明**：详细的性能分析
- ✅ **添加调试指南**：如何启用调试日志

### 🗑️ 删除的文件

以下旧方案的文件已删除：
- `VirtualDispenserManager.cs`
- `PlanetTransport_RefreshDispenserTraffic_NEW.cs`
- `DispenserComponent_InternalTick_Patch.cs`
- `DispenserComponent_OnRematchPairs_Patch.cs`
- `BattleBaseComponent_AutoPickTrash_Patch.cs`
- `UIControlPanelDispenserEntry_OnSetTarget_Patch.cs`
- `UIControlPanelDispenserEntry_OnSetTarget_Safety_Patch.cs`
- `UIControlPanelWindow_DetermineFilterResults_Patch.cs`
- `BattlefieldBaseHelper.cs`（768 行冗余代码）

### 📦 新增的文件

- `BattleBaseLogisticsManager.cs` - 全局管理器
- `BattleBaseComponent_InternalUpdate_Patch.cs` - 核心逻辑
- `LogisticCourierRenderer_Update_Patch.cs` - 渲染支持
- `GameData_ExportImport_Patch.cs` - 存档安全
- `PlanetFactory_Lifecycle_Patch.cs` - 资源管理 + 旧数据清理

### ⚠️ 重大变更

#### 升级注意事项
- ✅ **自动升级**：从 v1.x 升级到 v2.0 无需任何操作
- ✅ **存档兼容**：加载旧存档时自动清理虚拟配送器
- ✅ **无缝切换**：新架构会自动接管物流逻辑

#### API 变更（如果你是开发者）
- ❌ `VirtualDispenserManager` 已删除
- ✅ 新增：`BattleBaseLogisticsManager.GetOrCreate(planetId, battleBaseId)`
- ✅ 新增：`Plugin.DebugLog()` 方法

---

## [1.2.0] - 2026-01-31

### 修复
- ✅ **彻底修复监控面板 UI 错误**
  - 使用双层防护：数据源过滤 + 安全检查
  - 虚拟配送器完全隐藏，不会出现在监控面板
  - 修复 `EControlPanelEntryType.Dispenser` 枚举值错误（5 不是 4）

### 改进
- ✅ **改进物品检测**
  - 正确处理同一物品占用多个仓库格子的情况
  - 配送不会中途停止
- ✅ **基站拆除保护**
  - 拆除基站时，飞行中的无人机立即返回
  - 避免无人机飞向"幽灵基站"

### 代码
- ✅ 移除所有调试日志，保持静默运行
- ✅ 精简代码结构，提高可维护性

---

## [1.1.0] - 2026-01-30

### 新增
- ✅ 实现虚拟配送器架构（方案 C）
- ✅ 支持配送器-配送器物流
- ✅ 改进存档兼容性

---

## [1.0.0] - 2026-01-28

### 初始版本
- ✅ 实现基本功能：配送器从战场分析基站取货
- ✅ 完整的无人机飞行动画
- ✅ 支持现有存档

---

## 版本命名规则

本项目遵循 [语义化版本](https://semver.org/lang/zh-CN/) 规范：

- **主版本号（Major）**：不兼容的 API 变更或重大架构变更
- **次版本号（Minor）**：向下兼容的功能新增
- **修订号（Patch）**：向下兼容的问题修正

### 示例
- `1.0.0` → `1.1.0`：新增功能，向下兼容
- `1.1.0` → `1.1.1`：修复 Bug，向下兼容
- `1.x.x` → `2.0.0`：重大架构变更（虚拟配送器 → 基站直接派遣）

---

**感谢使用本 Mod！如有问题，请提交 Issue。** 🎮✨
