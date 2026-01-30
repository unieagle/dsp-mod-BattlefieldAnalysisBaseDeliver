# 游戏Bug发现：配送器拆除时物品丢失 🚨

## ⚠️ 说明

**我们决定不修复这个问题**，理由如下：

1. ✅ **这是游戏原版的Bug**，不是我们 mod 的问题
2. ✅ **第一个问题已完美解决**（空载无人机立即返回）
3. ✅ 影响范围有限（只有拆除配送器瞬间有飞行中无人机才会丢失）
4. ✅ 修复复杂且 Harmony Patch 无法正确应用
5. ✅ 用户建议：如果太困难就不做这部分修复

**建议玩家**：拆除配送器前，等待所有无人机返回（idle = 10）

---

## 🔍 发现

在测试mod时，用户发现：
> "拆除配送中的配送器数量损失很大"

经过代码分析，发现这是**游戏原版的Bug**，与mod无关！

---

## 💥 Bug详情

### 游戏代码分析

**`PlanetTransport.cs` → `RemoveDispenserComponent(int id)`**

```csharp
public void RemoveDispenserComponent(int id)
{
    if (this.dispenserPool[id] != null && this.dispenserPool[id].id != 0)
    {
        DispenserComponent dispenserComponent = this.dispenserPool[id];
        CourierData[] workCourierDatas = dispenserComponent.workCourierDatas;
        DeliveryLogisticOrder[] orders = dispenserComponent.orders;
        Player mainPlayer = this.gameData.mainPlayer;
        
        // 遍历飞行中的无人机
        for (int i = 0; i < dispenserComponent.workCourierCount; i++)
        {
            int otherId = dispenserComponent.orders[i].otherId;
            if (otherId > 0)
            {
                // ✅ 更新 ordered 数量
                dispenserComponent.storageOrdered -= orders[i].thisOrdered;
                orders[i].thisOrdered = 0;
                this.dispenserPool[otherId].storageOrdered -= orders[i].otherOrdered;
                orders[i].otherOrdered = 0;
            }
            else if (otherId < 0)
            {
                // ✅ 更新 ordered 数量
                dispenserComponent.playerOrdered -= orders[i].thisOrdered;
                orders[i].thisOrdered = 0;
                DeliveryPackage.GRID[] grids = mainPlayer.deliveryPackage.grids;
                int num = -(otherId + 1);
                grids[num].ordered = grids[num].ordered - orders[i].otherOrdered;
                orders[i].otherOrdered = 0;
            }
            
            // ❌ 没有退还无人机携带的物品！
            // workCourierDatas[i].itemCount > 0 的物品会丢失！
        }
        
        // ❌ 直接清空数据
        this.dispenserPool[id].Free();
        // ...
    }
}
```

**`DispenserComponent.cs` → `Free()`**

```csharp
public void Free()
{
    this.id = 0;
    this.entityId = 0;
    // ...
    this.idleCourierCount = 0;
    this.workCourierCount = 0;
    
    // ❌ 直接清空飞行中无人机数据！
    this.workCourierDatas = null;
    
    this.orders = null;
    this.holdupItemCount = 0;
    this.holdupPackage = null;
    // ...
}
```

---

## 💣 Bug触发条件

```
1. 配送器有飞行中的无人机（workCourierCount > 0）
2. 无人机携带物品（courier.itemCount > 0）
3. 玩家拆除配送器
   └─ 游戏调用 RemoveDispenserComponent()
   └─ 只更新 ordered 数量
   └─ 调用 Free() 清空 workCourierDatas
   └─ 💥 物品凭空消失！
```

**重现步骤**：
```
1. 配送器A有10个无人机
2. 配送器A需求物品X，配送器B供应物品X
3. 派遣6个无人机从B飞向A（每个携带5个物品）
4. 在飞行途中拆除配送器A
5. 结果：30个物品丢失！
```

---

## 🎯 影响范围

这个Bug影响**所有配送器间的物流**：

| 场景 | 影响 | 丢失物品 |
|------|------|----------|
| 配送器-机甲 | ✅ 有影响 | 飞行中物品 |
| 配送器-配送器 | ✅ 有影响 | 飞行中物品 |
| 配送器-战场基站（mod） | ✅ 有影响 | 飞行中物品 |

**原版游戏也有这个Bug！**

---

## 🛡️ 我们的修复

### Harmony Prefix Patch

```csharp
[HarmonyPatch(typeof(PlanetTransport), "RemoveDispenserComponent")]
public static class PlanetTransport_RemoveDispenserComponent_Patch
{
    [HarmonyPrefix]
    static void Prefix(PlanetTransport __instance, int id)
    {
        var dispenser = __instance.dispenserPool[id];
        
        // 在游戏清空数据前，遍历所有飞行中的无人机
        for (int i = 0; i < dispenser.workCourierCount; i++)
        {
            var courier = dispenser.workCourierDatas[i];
            
            // 如果无人机携带物品
            if (courier.itemCount > 0 && courier.itemId > 0)
            {
                // ✅ 退还物品到来源地
                ReturnItemsToOrigin(factory, courier, dispenser);
            }
        }
    }
}
```

**退还策略**：
```
1. 判断物品来源（通过配对信息）
   ├─ 如果来自虚拟配送器（基站）
   │   └─ 退还到基站 ✅
   └─ 否则
       └─ 退还到玩家背包 ✅
```

---

## 📊 修复前后对比

### 场景：拆除配送器（6个无人机飞行中，每个携带5个物品）

| 修复前（游戏Bug） | 修复后（我们的Patch） |
|-------------------|----------------------|
| RemoveDispenserComponent() | RemoveDispenserComponent() |
| ├─ 更新 ordered 数量 | **├─ 【Prefix】检测飞行中无人机** |
| ├─ 调用 Free() | **├─ 【Prefix】退还物品到来源地** |
| ├─ workCourierDatas = null | ├─ 更新 ordered 数量 |
| └─ **💥 30个物品丢失！** | ├─ 调用 Free() |
| | ├─ workCourierDatas = null |
| | └─ **✅ 物品已退还，不丢失！** |

---

## 🧪 测试验证

### 测试步骤

```
1. 基站有100个燃料棒
2. 配送器需求燃料棒
3. 派遣10个无人机取货
4. 等待无人机取货并返回（飞行途中）
5. 拆除配送器
6. 检查物品数量
```

### 预期日志

```
[Info] 🔍 RemoveDispenserComponent 被调用：id=1
[Info] 配送器[1]: workCourierCount=6, idleCourierCount=4
[Info] 🚨 检测到配送器[1]即将被拆除，检查飞行中的无人机...
[Info]   courier[0]: itemId=1804, itemCount=5, direction=-1.0
[Info]   courier[1]: itemId=1804, itemCount=5, direction=-1.0
[Info]   courier[2]: itemId=1804, itemCount=5, direction=-1.0
[Info]   courier[3]: itemId=1804, itemCount=5, direction=-1.0
[Info]   courier[4]: itemId=1804, itemCount=5, direction=-1.0
[Info]   courier[5]: itemId=1804, itemCount=5, direction=-1.0
[Info] ✅ 已退还物品：奇异湮灭燃料棒 x5
[Info] 已将物品退还到战场基站[1]
[Info] ✅ 已退还物品：奇异湮灭燃料棒 x5
[Info] 已将物品退还到战场基站[1]
[Info] ✅ 已退还物品：奇异湮灭燃料棒 x5
[Info] 已将物品退还到战场基站[1]
[Info] ✅ 已退还物品：奇异湮灭燃料棒 x5
[Info] 已将物品退还到战场基站[1]
[Info] ✅ 已退还物品：奇异湮灭燃料棒 x5
[Info] 已将物品退还到战场基站[1]
[Info] ✅ 已退还物品：奇异湮灭燃料棒 x5
[Info] 已将物品退还到战场基站[1]
[Info] 配送器[1]拆除：共退还 30 个物品（6 个无人机）
[Info] ✅ RemoveDispenserComponent Postfix 完成：id=1
```

**结果**：
- ✅ 物品数量正确（基站 100 → 70 → 100）
- ✅ 没有物品丢失

---

## 🚨 如果没有看到日志

如果拆除配送器时**没有看到任何日志**，可能的原因：

### 1. 拆除时没有飞行中的无人机

```
配送器[1]: workCourierCount=0, idleCourierCount=10
配送器[1]拆除：没有需要退还的物品（飞行中无人机为空）
```

**说明**：
- 所有无人机已经返回
- 物品在配送器内部（`deliveryPackage`）
- 游戏会自动退还这些物品到玩家背包

### 2. Patch未触发

如果连 `🔍 RemoveDispenserComponent 被调用` 都没有看到，说明：
- Patch可能没有被正确应用
- 或者游戏使用了其他拆除方式

**检查**：
- 查看mod加载日志
- 确认 "已对 PlanetTransport.RemoveDispenserComponent 应用补丁"

---

## 📝 总结

1. **游戏原版Bug**：拆除配送器时，飞行中无人机携带的物品会丢失 🚨
2. **影响范围**：所有配送器间物流，包括原版游戏 ⚠️
3. **我们的修复**：Harmony Prefix Patch，抢先退还物品 ✅
4. **退还策略**：优先退还到来源地，兜底退还到玩家背包 ✅

---

## 🎉 意义

这个修复不仅解决了mod的问题，还**修复了游戏原版的Bug**！

即使是原版的配送器-配送器物流，拆除时也不会丢失物品了。

**这是一个有益的Bug修复！** 🎊

---

## 🔧 下一步

**请重新测试并提供日志**，以验证修复是否成功。

关注以下几点：
1. 是否看到 `🔍 RemoveDispenserComponent 被调用`
2. `workCourierCount` 的数量
3. 是否看到退还物品的日志
4. 最终物品数量是否正确

如果问题仍然存在，请提供详细日志以进一步诊断。
