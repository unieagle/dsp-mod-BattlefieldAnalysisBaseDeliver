# 战场分析基站无法打开 UI 的修复思路

## 现象

为让战场分析基站参与本地配送，Mod 在建造时为其也创建了 `StationComponent`（`CreateEntityLogicComponents` 补丁），于是该建筑的 entity 同时拥有 `stationId` 和 `battleBaseId`。  
点击建筑时，游戏很可能**先根据 `stationId > 0` 打开物流站 UI**，而不再打开战场分析基站的 UI。

## 验证方式

在 dnSpy 中打开 `Assembly-CSharp.dll`：

1. **搜索 `stationId`**，在“引用”里找到**打开建筑 UI / 选择建筑**相关的逻辑（例如 `PlayerController`、`UIBuilding`、`UIStation`、`SelectTarget`、`OnBuildingClick` 等）。
2. 查看**同一方法或同一类**里是否也使用了 **`battleBaseId`**。
3. 若逻辑是「先判断 `stationId` 再判断 `battleBaseId`」，则改为**先判断 `battleBaseId`**：当 `entity.battleBaseId > 0` 时优先打开战场分析基站 UI，否则再按 `stationId` 等打开物流站 UI。

## 补丁思路（需根据实际反编译结果调整）

- **思路 A**：在“决定打开哪个建筑 UI”的方法上做 **Prefix**：  
  若当前选中的 entity 满足 `battleBaseId > 0`，则在该方法内**临时**把 `entity.stationId` 置 0，让游戏走“无物流站”分支从而打开战场分析基站 UI，**Postfix** 中再恢复 `stationId`。  
  （注意：临时改 `stationId` 可能影响同一帧内其他逻辑，需在最小范围内恢复。）

- **思路 B**：找到“若 `stationId > 0` 则打开物流站 UI”的分支，改为：  
  **若 `battleBaseId > 0` 则打开战场分析基站 UI，否则若 `stationId > 0` 再打开物流站 UI**。  
  这样无需改 entity 数据，只改分支顺序。

## 需要你从游戏中确认的信息

在 dnSpy 中定位到“点击建筑后决定打开哪个 UI”的类名与方法名（例如 `XXXController.OnEntityClick` 或 `UIBuilding.SetTarget`），把**类名、方法名、以及判断 `stationId`/`battleBaseId` 的代码片段**记下来，即可据此写具体的 Harmony 补丁（Prefix/Postfix 或 Transpiler）。
