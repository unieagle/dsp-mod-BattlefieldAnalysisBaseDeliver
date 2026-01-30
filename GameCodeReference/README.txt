此文件夹用于存放从 dnSpy 导出的游戏参考代码。

操作步骤：
1. 用 dnSpy 打开游戏 DLL：
   C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\DSPGAME_Data\Managed\Assembly-CSharp.dll

2. 搜索 StationComponent 类，右键 -> Export to Project... -> 选择本文件夹保存

3. 在本文件夹中创建或编辑「关键信息记录.txt」，记录：
   - FindItemSource 方法的完整签名
   - CanPickupItem 方法的完整签名
   - 战场分析基站的建筑 ID (protoId)
   - 物品存储相关属性/方法名

4. 根据导出代码和记录，修改项目中的 Patches\DroneLogicPatch.cs

详细说明见项目根目录的「继续操作.md」。
