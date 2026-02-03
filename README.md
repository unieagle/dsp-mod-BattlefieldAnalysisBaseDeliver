# 戴森球计划 - 战场分析基站配送支持 Mod

## 简介

本 Mod 让**战场分析基站**能够**直接派遣无人机**向**机甲**和**物流配送器**和**物流塔**配送战利品，实现全自动化战利品物流。

### ✨ 核心特性

- 🚀 **战场基站直接派遣**：每个基站拥有独立的 20 个 2 倍速无人机（可配置）
- 📦 **智能优先级调度**：优先配送给库存紧急的配送器
- ✈️ **完整的飞行动画**：无人机可见，视觉效果完整
- 💾 **存档安全**：自动兼容旧版本，支持无缝升级
- 🎯 **零性能损耗**：基于事件驱动，只在需要时工作
- 🔧 **零 UI 干扰**：不修改任何 UI，不影响游戏体验

### 使用场景

战场分析基站在击败敌人后会积累大量战利品（硅块、晶格硅、电路板等）。使用本 Mod 后：

基站会自动派出无人机给以下目标送货：

1. 机甲：在配送栏设置了物品需求；
2. 配送器：设置了物品需求；
3. 物流塔：行星或者星际，设置了物品本地需求；

---

## 安装

1. 确保已安装 **BepInEx 5.x** (x64)
2. 将 `BattlefieldAnalysisBaseDeliver.dll` 放入 `BepInEx\plugins\` 文件夹
3. 启动游戏

---

### 配置选项

在 `BepInEx\config\com.yourname.battlefieldanalysisbasedeliver.cfg` 中：

```ini
[General]
## 启用调试日志（排查问题时使用）
EnableDebugLog = false
```

---

## 技术说明

### v2.0 全新架构：基站直接派遣

本 Mod 采用**基站直接派遣**架构，完全不同于传统的物流配送器逻辑：

#### 核心设计

1. **每个战场基站拥有独立的 20 个无人机**
   - 存储在 `BaseLogisticSystem` 中
   - 无人机状态：空闲 / 飞行中（去程 / 回程）

2. **基于事件驱动的派遣逻辑**
   - Hook: `BattleBaseComponent.InternalUpdate`
   - 检测：基站物品变化时触发扫描
   - 派遣：找到最优配送器并派遣无人机

3. **智能调度算法**
   ```csharp
   // 紧急度 = 当前库存 / 最大库存
   urgency = currentStock / maxStock
   
   // 排序：紧急度优先，距离次之
   demands.Sort((a, b) => {
       if (a.urgency != b.urgency)
           return a.urgency.CompareTo(b.urgency);
       return a.distance.CompareTo(b.distance);
   });
   ```

4. **无人机飞行与送货**
   - 飞行：`courier.t += deltaT * direction`
   - 到达：`t >= maxt` 时送货
   - 返回：`direction = -1`，回到基站

5. **渲染支持**
   - Hook: `LogisticCourierRenderer.Update`
   - 将基站的无人机数据注入游戏渲染系统
   - 无人机完全可见，视觉效果与游戏原生一致

#### 存档安全机制

1. **存档前**（`GameData.Export`）
   - 返还所有在途物品到基站
   - 清空无人机状态

2. **存档加载后**（`GameData.Import`）
   - 自动检测基站物品变化
   - 重新派遣无人机

3. **旧版本兼容**（`PlanetFactory.Import`）
   - 自动识别并删除旧方案遗留的虚拟配送器
   - 避免坏档

---

## 项目结构

```
BattlefieldAnalysisBaseDeliver/
├── Plugin.cs                                        # 主插件入口 + DebugLog()
├── PluginInfo.cs                                   # 插件信息（版本号等）
├── Patches/
│   ├── BattleBaseComponent_InternalUpdate_Patch.cs  # [核心] 派遣、飞行、送货逻辑
│   ├── BattleBaseLogisticsManager.cs                # [核心] 全局管理器
│   ├── LogisticCourierRenderer_Update_Patch.cs      # [核心] 无人机渲染支持
│   ├── GameData_ExportImport_Patch.cs               # [核心] 存档安全
│   └── PlanetFactory_Lifecycle_Patch.cs             # [核心] 资源清理 + 旧数据清理
└── GameCodeReference/                              # 反编译的游戏代码（参考）
```

**总计：5 个核心 Patch 文件 + 1 个主类**

---

## 开发和调试

### 查看日志

日志位置：`BepInEx\LogOutput.log`

启用调试日志：
```ini
[General]
EnableDebugLog = true
```

调试日志会输出：
- 🚀 无人机派遣信息
- 📬 送货成功信息
- 🏠 无人机返回信息
- 💾 存档操作信息

### 构建

```bash
dotnet build -c Release
```

输出：`bin\Release\net472\BattlefieldAnalysisBaseDeliver.dll`

### 反编译工具

- **dnSpy**：推荐，支持调试
- **ILSpy**：仅查看代码

反编译目标：`DSPGAME_Data\Managed\Assembly-CSharp.dll`

---

## 兼容性

- ✅ **完全兼容现有存档**
- ✅ **可与其他 Mod 共存**（除非该 Mod 也修改了 `BattleBaseComponent` 或 `LogisticCourierRenderer`）
- ✅ **移除 Mod 后不会影响存档**（无人机数据不保存到存档）
- ✅ **自动兼容旧版本**（v1.x 的虚拟配送器会自动清理）

---

如遇到问题，请：
1. 启用 `EnableDebugLog = true`
2. 查看 `BepInEx\LogOutput.log`
3. 提交 Issue 并附上日志
