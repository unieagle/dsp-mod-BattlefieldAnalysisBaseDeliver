# UI 错误根本修复说明

## 问题描述

打开监控面板（UIControlPanelWindow）并滚动到虚拟配送器时，游戏抛出 `System.NullReferenceException`：

```
System.NullReferenceException: Object reference not set to an instance of an object
  at UIControlPanelDispenserEntry.OnSetTarget () [0x0005c]
  at UIControlPanelObjectEntry.InitFromPool (System.Int32 _index, ControlPanelTarget _target) [0x00016]
  at UIControlPanelWindow.TakeObjectEntryFromPool (System.Int32 _index, ControlPanelTarget _target) [0x0005c]
  at UIControlPanelWindow.DetermineEntryVisible () [0x001fe]
  at UIControlPanelWindow._OnUpdate () [0x00043]
```

## 根本原因

### 1. UI 代码流程（反编译分析）

`UIControlPanelWindow.DetermineFilterResults()` 方法（第477-509行）：
```csharp
DispenserComponent[] dispenserPool = planetFactory.transport.dispenserPool;
int dispenserCursor = planetFactory.transport.dispenserCursor;
for (int m = 1; m < dispenserCursor; m++)
{
    DispenserComponent dispenserComponent = dispenserPool[m];
    if (dispenserComponent != null && dispenserComponent.id == m)
    {
        // ... 过滤逻辑 ...
        
        // 【关键】第504行：创建 UI 目标
        ControlPanelTarget controlPanelTarget3 = new ControlPanelTarget(
            EObjectType.None, 
            dispenserComponent.entityId,  // ← 使用配送器的 entityId
            planet.astroId, 
            EControlPanelEntryType.Dispenser
        );
        this.AddFilterResult(ref controlPanelTarget3, visible);
    }
}
```

`UIControlPanelDispenserEntry.OnSetTarget()` 方法（第218-231行）：
```csharp
public override void OnSetTarget()
{
    this.factory = GameMain.data.galaxy.PlanetById(this.target.astroId).factory;
    
    // 【关键错误点】第221行
    this.dispenser = this.factory.transport.dispenserPool[
        this.factory.entityPool[this.target.objId].dispenserId  // ← 💥 这里出错！
    ];
    
    this.id = this.dispenser.id;
    // ...
}
```

### 2. 虚拟配送器的问题

**原有实现**（`VirtualDispenserManager.CreateVirtualDispensers`）：
```csharp
virtualDispenser.id = virtualDispenserId;
virtualDispenser.entityId = entityId;  // ← 这里设置为战场基站的实体ID
```

**问题链**：
1. 虚拟配送器的 `entityId` = 战场基站的实体ID（例如：150）
2. UI 传入 `target.objId` = 150（战场基站的实体ID）
3. `entityPool[150]` 获取战场基站实体（不是配送器实体！）
4. 战场基站实体的 `dispenserId` 字段 = **0**（因为战场基站不是配送器）
5. `dispenserPool[0]` = **null** → 💥 `NullReferenceException`

### 3. 为什么之前的修复方案都失败了？

| 修复尝试 | 方法 | 失败原因 |
|----------|------|----------|
| Postfix 移除虚拟配送器 | 从 `results` 列表移除虚拟配送器 | UI 状态不一致，导致真实配送器不可选 |
| Prefix 跳过虚拟配送器 | 返回 `null` 或 `false` | 破坏 UI 状态管理，反射访问困难 |
| Finalizer 捕获异常 | 吞掉 `NullReferenceException` | 治标不治本，仍有错误堆栈 |
| 初始化 `deliveryPackage` | 确保字段非 null | 问题不在 `deliveryPackage`，在实体访问 |

## 解决方案：创建假实体（Dummy Entity）

### 核心思路

为每个虚拟配送器创建一个**假实体（Dummy Entity）**，并设置 `entity.dispenserId = virtualDispenserId`。

### 实现细节

修改 `VirtualDispenserManager.CreateVirtualDispensers` 方法（第170-250行）：

```csharp
// 【新增】为虚拟配送器创建假实体
int dummyEntityId = 0;
bool needCreateDummyEntity = true;

// 检查 entityPool 中是否已有可用的假实体（从存档加载时）
// ... [检查逻辑] ...

// 如果需要，创建假实体
if (needCreateDummyEntity)
{
    var factoryType = factory.GetType();
    var entityPoolFieldFactory = factoryType.GetField("entityPool", ...);
    var entityCursorField = factoryType.GetField("entityCursor", ...);
    
    var entityPool = entityPoolFieldFactory.GetValue(factory) as Array;
    int entityCursor = (int)entityCursorField.GetValue(factory)!;
    
    // 创建假实体
    var entityType = entityPool.GetType().GetElementType();
    var dummyEntity = Activator.CreateInstance(entityType);
    
    // 设置实体字段
    idField.SetValue(dummyEntity, entityCursor);
    protoIdField.SetValue(dummyEntity, (short)0);     // 无原型
    modelIndexField.SetValue(dummyEntity, (short)-1); // 无模型
    posField.SetValue(dummyEntity, bbPos);            // 使用战场基站位置
    rotField.SetValue(dummyEntity, bbRot);            // 使用战场基站旋转
    dispenserIdField.SetValue(dummyEntity, dispenserCursor);  // ← 关键！
    
    // 将假实体添加到 entityPool
    entityPool.SetValue(dummyEntity, entityCursor);
    dummyEntityId = entityCursor;
    entityCursorField.SetValue(factory, entityCursor + 1);
}

// 创建虚拟配送器
var virtualDispenser = new DispenserComponent();
int virtualDispenserId = dispenserCursor++;

// 【关键修改】使用假实体ID
virtualDispenser.entityId = (dummyEntityId > 0) ? dummyEntityId : entityId;
```

### 修复效果

现在当 UI 访问虚拟配送器时：
1. `target.objId` = 假实体ID（例如：500）
2. `entityPool[500]` 获取假实体
3. 假实体的 `dispenserId` = 虚拟配送器ID（例如：26）
4. `dispenserPool[26]` = 虚拟配送器 ✅ 成功！

## 优势

1. **彻底解决根本问题**：不再需要拦截、跳过或捕获异常
2. **不破坏 UI 状态**：虚拟配送器可以正常显示在列表中
3. **不影响游戏逻辑**：假实体没有模型（`modelIndex = -1`），不会在游戏中可见
4. **兼容存档加载**：从存档加载时会检查并重用已有的假实体
5. **简洁优雅**：让虚拟配送器"看起来"像真实配送器，符合游戏架构

## 测试要点

1. ✅ 打开监控面板，不应有 `NullReferenceException`
2. ✅ 滚动到虚拟配送器，UI 应正常显示
3. ✅ 所有真实配送器都应可选
4. ✅ 虚拟配送器的配送功能正常工作
5. ✅ 存档加载后，虚拟配送器正常工作

## 相关文件

- `Patches/VirtualDispenserManager.cs`：虚拟配送器创建逻辑（含假实体创建）
- `GameCodeReference/UIControlPanelDispenserEntry.cs`：反编译的 UI 入口类
- `GameCodeReference/UIControlPanelWindow.cs`：反编译的监控面板类

## 总结

通过分析反编译的游戏代码和堆栈跟踪，我们找到了 UI 错误的根本原因：**虚拟配送器没有对应的实体，导致 UI 通过 `entityPool[entityId].dispenserId` 访问失败**。

解决方案是为每个虚拟配送器创建一个假实体，让虚拟配送器完全融入游戏的架构中，而不是作为"例外"被各种补丁排除。这是一个**彻底的根本性修复**，而不是"头痛医头、脚痛医脚"的临时方案。
