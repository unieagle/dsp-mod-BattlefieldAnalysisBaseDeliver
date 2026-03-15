# 戴森球计划 - 战场分析基站配送支持 Mod

## 简介

本 Mod 让**战场分析基站**能够**直接派遣无人机**

- 向**机甲**和**物流配送器**和**物流塔**配送战利品，实现全自动化战利品物流;
    - 优先级: 机甲 -> 配送器 -> 物流塔
- 并且在有库存的时候给星际物流塔自动送空间翘曲器;
- 从供应物流配送器取货放入设定了物品的输入区。
- 可配置的基站 stack size 修改以及可配置的配送无人机数量，速度和运载量以提供足够的吞吐量；
- 优化基站的游戏原本拾取逻辑，避免还有需求的掉落物品由于时间到期而来不及拾取的问题；

### ✨ 核心特性

- 🚀 **战场基站直接派遣**：每个基站拥有独立的 20 个 2 倍速无人机（可配置）
- 📦 **智能优先级调度**：优先配送给库存紧急的配送器
- ✈️ **完整的飞行动画**：无人机可见，视觉效果完整
- 💾 **存档安全**：自动兼容旧版本，支持无缝升级
- 🎯 **零性能损耗**：基于事件驱动，只在需要时工作
- 🔧 **零 UI 干扰**：不修改任何 UI，不影响游戏体验

This mod enables **Battlefield Analysis Bases** to

- **dispatch drones** to deliver loot to the **Mecha**, **Logistic Dispensers**, and **Logistic Stations**, automating loot logistics.
- automatically delivers **Space Warpers** to interstellar logistics stations when the base has them in stock.
- retrieve from Logistic Dispensers which supplies the specified object in input area of BAB.

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

- **dnSpy**：反编译游戏源代码

反编译目标：`DSPGAME_Data\Managed\Assembly-CSharp.dll`

---

如遇到问题，请：
1. 启用 `EnableDebugLog = true`
2. 查看 `BepInEx\LogOutput.log`
3. 提交 Issue 并附上日志
