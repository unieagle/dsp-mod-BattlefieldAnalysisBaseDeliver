# 战场分析基站配送支持 Mod

## 功能说明

本 Mod 让**物流配送器**（箱子上的小配送器）和**机甲物流槽**能从**战场分析基站**取物。

- **场景**：战场分析基站里放了硅块等物品；箱子上的物流配送器或机甲物流槽配置了「需求硅块」。
- **效果**：配送器/机甲的本地配送会匹配到战场分析基站，从中取物。

## 技术方案

1. **RematchLocalPairs 补丁**：在本地配送匹配时，把「战场分析基站且有对应物品」也当作供应源加入配对（原逻辑只认 `localLogic == Supply` 的格子）。
2. **HasLocalSupply 补丁**：当游戏询问某站是否有本地供应时，若该站是战场分析基站且格子里有对应物品，则返回该格子索引。
3. **识别战场分析基站**：通过 `factory.entityPool[entityId].protoId` 识别，建筑 ID 可在配置中修改（默认 2318）。

## 开发环境配置

### 1. 安装 BepInEx

1. 下载 BepInEx x64 版本（DSP 是 64 位游戏）
2. 解压到游戏安装目录：`Steam\steamapps\common\Dyson Sphere Program\`
3. 首次启动游戏，BepInEx 会自动生成 `BepInEx` 文件夹

### 2. 反编译游戏代码（推荐）

使用 **dnSpy** 或 **dotPeek** 反编译游戏代码：

- 文件位置：`DSPGAME_Data\Managed\Assembly-CSharp.dll`
- 需要查找的关键类和方法：
  - `StationComponent` 类
  - `FindItemSource` 方法
  - `CanPickupItem` 方法
  - 战场分析基站的建筑 ID

### 3. 配置项目

1. **设置游戏路径环境变量**（可选）
   - 在项目文件中，`$(DSPGamePath)` 变量用于自动复制编译后的 DLL
   - 可以在项目属性中设置，或手动修改 `.csproj` 文件

2. **安装 NuGet 包**
   ```bash
   dotnet restore
   ```

3. **编译项目**
   ```bash
   dotnet build -c Release
   ```

4. **手动安装 Mod**
   - 将编译生成的 `BattlefieldAnalysisBaseDeliver.dll` 复制到：
   - `游戏目录\BepInEx\plugins\BattlefieldAnalysisBaseDeliver\`

## 项目结构

```
BattlefieldAnalysisBaseDeliver/
├── BattlefieldAnalysisBaseDeliver.csproj  # 项目文件
├── Plugin.cs                               # 主插件入口
├── PluginInfo.cs                          # 插件信息
├── Patches/
│   └── DroneLogicPatch.cs                 # Harmony 补丁实现
└── README.md                              # 本文件
```

## 使用说明

### 安装

1. 确保已安装 BepInEx（见上方开发环境配置）
2. 将 `BattlefieldAnalysisBaseDeliver.dll` 放入 `BepInEx\plugins\` 文件夹
3. 启动游戏

### 功能

- 配送无人机现在可以从战场分析基站取物品
- 无需额外配置，Mod 会自动识别战场分析基站

## 调试和开发

### 查看日志

Mod 的日志会输出到：`BepInEx\LogOutput.log`

### 需要调整的参数

1. **战场分析基站的建筑 ID**
   - 在 `Patches/DroneLogicPatch.cs` 中修改 `battlefieldStationId` 变量
   - 需要通过反编译或游戏内测试确定正确的 ID

2. **方法名和类名**
   - 如果游戏更新导致类名或方法名改变，需要更新补丁代码
   - 使用反射可以自动查找方法，但如果找不到，需要手动指定

### 推荐的调试工具

- **UnityExplorer**：查看游戏对象（F7 打开）
- **ScriptEngine**：热重载测试（F6），无需重启游戏
- **dnSpy**：反编译和调试游戏代码

## 注意事项

1. **游戏版本兼容性**
   - 游戏更新可能导致 Mod 失效
   - 需要根据新版本的游戏代码更新补丁

2. **性能影响**
   - 物品源查找会遍历附近的建筑，可能对性能有轻微影响
   - 建议在大量战场分析基站的情况下进行性能测试

3. **平衡性**
   - 这个 Mod 改变了游戏机制，可能影响游戏平衡性
   - 请根据个人喜好使用

## 开发路线图

- [x] 基础框架搭建
- [x] Harmony 补丁实现
- [ ] 确定战场分析基站的正确建筑 ID
- [ ] 实现完整的物品源查找逻辑
- [ ] 性能优化
- [ ] 添加配置选项（如搜索范围等）

## 贡献

欢迎提交 Issue 和 Pull Request！

## 许可证

本项目采用 MIT 许可证。
