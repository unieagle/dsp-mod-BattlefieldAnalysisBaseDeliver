using System;
using System.IO;
using UnityEngine;

// Token: 0x02000181 RID: 385
public class PlanetTransport
{
	// Token: 0x06000DBC RID: 3516 RVA: 0x000CBAF8 File Offset: 0x000C9CF8
	public PlanetTransport(GameData _gameData, PlanetData _planet)
	{
		this.gameData = _gameData;
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.powerSystem = this.factory.powerSystem;
		this.droneRenderer = new LogisticDroneRenderer(this);
		this.courierRenderer = new LogisticCourierRenderer(this);
		this.player = this.gameData.mainPlayer;
		this.playerDeliveryPackage = this.player.deliveryPackage;
		this.SetStationCapacity(16);
		this.SetDispenserCapacity(8);
	}

	// Token: 0x06000DBD RID: 3517 RVA: 0x000CBBA0 File Offset: 0x000C9DA0
	public PlanetTransport(GameData _gameData, PlanetData _planet, bool import)
	{
		this.gameData = _gameData;
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.powerSystem = this.factory.powerSystem;
		this.droneRenderer = new LogisticDroneRenderer(this);
		this.courierRenderer = new LogisticCourierRenderer(this);
		this.player = this.gameData.mainPlayer;
		this.playerDeliveryPackage = this.player.deliveryPackage;
	}

	// Token: 0x06000DBE RID: 3518 RVA: 0x000CBC36 File Offset: 0x000C9E36
	public void Init()
	{
		this.CalcCollectorsWorkCost();
		if (this.gameData.history != null)
		{
			this.gameData.history.onFunctionUnlocked += this.OnTechFunctionUnlocked;
		}
		this.playerDeliveryEnabled = true;
	}

	// Token: 0x06000DBF RID: 3519 RVA: 0x000CBC70 File Offset: 0x000C9E70
	public void Free()
	{
		if (this.gameData.history != null)
		{
			this.gameData.history.onFunctionUnlocked -= this.OnTechFunctionUnlocked;
		}
		for (int i = 0; i < this.stationPool.Length; i++)
		{
			if (this.stationPool[i] != null)
			{
				this.stationPool[i].Free();
				this.stationPool[i] = null;
			}
		}
		this.stationPool = null;
		this.stationCursor = 1;
		this.stationCapacity = 0;
		this.stationRecycle = null;
		this.stationRecycleCursor = 0;
		this.dispenserPool = null;
		this.dispenserCursor = 1;
		this.dispenserCapacity = 0;
		this.dispenserRecycle = null;
		this.dispenserRecycleCursor = 0;
		this.playerDeliveryEnabled = false;
		if (this.droneRenderer != null)
		{
			this.droneRenderer.Destroy();
			this.droneRenderer = null;
		}
		if (this.courierRenderer != null)
		{
			this.courierRenderer.Destroy();
			this.courierRenderer = null;
		}
		this.player = null;
		this.playerDeliveryPackage = null;
	}

	// Token: 0x170001B4 RID: 436
	// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x000CBD68 File Offset: 0x000C9F68
	public int stationCount
	{
		get
		{
			return this.stationCursor - this.stationRecycleCursor - 1;
		}
	}

	// Token: 0x170001B5 RID: 437
	// (get) Token: 0x06000DC1 RID: 3521 RVA: 0x000CBD79 File Offset: 0x000C9F79
	public int workerThreadWeight
	{
		get
		{
			return (this.stationCursor - 1) * 2 + this.dispenserCursor - 1;
		}
	}

	// Token: 0x170001B6 RID: 438
	// (get) Token: 0x06000DC2 RID: 3522 RVA: 0x000CBD8E File Offset: 0x000C9F8E
	public int dispenserCount
	{
		get
		{
			return this.dispenserCursor - this.dispenserRecycleCursor - 1;
		}
	}

	// Token: 0x06000DC3 RID: 3523 RVA: 0x000CBDA0 File Offset: 0x000C9FA0
	private void CalcCollectorsWorkCost()
	{
		PrefabDesc prefabDesc = LDB.items.Select(ItemProto.stationCollectorId).prefabDesc;
		this.collectorWorkEnergyPerTick = prefabDesc.workEnergyPerTick;
		this.collectorsWorkCost = (double)prefabDesc.workEnergyPerTick * 60.0;
		this.collectorsWorkCost /= (double)prefabDesc.stationCollectSpeed;
	}

	// Token: 0x06000DC4 RID: 3524 RVA: 0x000CBDFC File Offset: 0x000C9FFC
	public void Export(BinaryWriter w)
	{
		w.Write(1);
		w.Write(this.stationCursor);
		w.Write(this.stationCapacity);
		w.Write(this.stationRecycleCursor);
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].id == i)
			{
				w.Write(i);
				this.stationPool[i].Export(w);
			}
			else
			{
				w.Write(0);
			}
		}
		for (int j = 0; j < this.stationRecycleCursor; j++)
		{
			w.Write(this.stationRecycle[j]);
		}
		w.Write(this.dispenserCapacity);
		w.Write(this.dispenserCursor);
		w.Write(this.dispenserRecycleCursor);
		for (int k = 1; k < this.dispenserCursor; k++)
		{
			if (this.dispenserPool[k] != null && this.dispenserPool[k].id == k)
			{
				w.Write(k);
				this.dispenserPool[k].Export(w);
			}
			else
			{
				w.Write(0);
			}
		}
		for (int l = 0; l < this.dispenserRecycleCursor; l++)
		{
			w.Write(this.dispenserRecycle[l]);
		}
	}

	// Token: 0x06000DC5 RID: 3525 RVA: 0x000CBF28 File Offset: 0x000CA128
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.stationCursor = r.ReadInt32();
		this.SetStationCapacity(r.ReadInt32());
		this.stationRecycleCursor = r.ReadInt32();
		for (int i = 1; i < this.stationCursor; i++)
		{
			int num2 = r.ReadInt32();
			if (num2 != 0)
			{
				Assert.True(num2 == i);
				this.stationPool[i] = new StationComponent();
				this.stationPool[i].Import(r);
				if (!this.stationPool[i].isVeinCollector && !this.stationPool[i].isCollector && this.stationPool[i].isStellar)
				{
					short modelIndex = this.factory.entityPool[this.stationPool[i].entityId].modelIndex;
					int stationMaxDroneCount = LDB.models.Select((int)modelIndex).prefabDesc.stationMaxDroneCount;
					this.stationPool[i].PatchDroneArray(stationMaxDroneCount);
				}
				if (this.stationPool[i].gid > 0)
				{
					this.gameData.galacticTransport.AddStationComponent(this.planet.id, this.stationPool[i]);
				}
			}
		}
		if (this.gameData.patch < 7)
		{
			for (int j = 1; j < this.stationCursor; j++)
			{
				if (this.stationPool[j] != null && this.stationPool[j].id == j)
				{
					StationComponent stationComponent = this.stationPool[j];
					if (!stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector && stationComponent.storage.Length < 4)
					{
						Array storage = stationComponent.storage;
						StationStore[] array = new StationStore[4];
						Array.Copy(storage, array, 3);
						stationComponent.storage = array;
					}
				}
			}
		}
		if (this.gameData.patch < 9 && !this.gameData.history.logisticShipWarpDrive)
		{
			for (int k = 1; k < this.stationCursor; k++)
			{
				if (this.stationPool[k] != null && this.stationPool[k].id == k)
				{
					StationComponent stationComponent2 = this.stationPool[k];
					if (stationComponent2.isStellar)
					{
						stationComponent2.warperMaxCount = 0;
					}
				}
			}
		}
		for (int l = 0; l < this.stationRecycleCursor; l++)
		{
			this.stationRecycle[l] = r.ReadInt32();
		}
		if (num >= 1)
		{
			int num3 = r.ReadInt32();
			this.SetDispenserCapacity(num3);
			this.dispenserCursor = r.ReadInt32();
			this.dispenserRecycleCursor = r.ReadInt32();
			PlayerPackageUtility packageUtility = this.player.packageUtility;
			for (int m = 1; m < this.dispenserCursor; m++)
			{
				int num4 = r.ReadInt32();
				if (num4 != 0)
				{
					Assert.True(num4 == m);
					this.dispenserPool[m] = new DispenserComponent();
					this.dispenserPool[m].Import(r);
					this.dispenserPool[m].deliveryPackage = this.playerDeliveryPackage;
					this.dispenserPool[m].packageUtility = packageUtility;
					this.ConnectToDispenser(this.dispenserPool[m].id, this.dispenserPool[m].storageId);
				}
			}
			for (int n = 0; n < this.dispenserRecycleCursor; n++)
			{
				this.dispenserRecycle[n] = r.ReadInt32();
			}
		}
		else
		{
			this.SetDispenserCapacity(8);
		}
		this.RefreshStationTraffic(0);
		this.RefreshDispenserTraffic(0);
	}

	// Token: 0x06000DC6 RID: 3526 RVA: 0x000CC288 File Offset: 0x000CA488
	private void SetStationCapacity(int newCapacity)
	{
		StationComponent[] array = this.stationPool;
		this.stationPool = new StationComponent[newCapacity];
		this.stationRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.stationPool, (newCapacity > this.stationCapacity) ? this.stationCapacity : newCapacity);
		}
		this.stationCapacity = newCapacity;
	}

	// Token: 0x06000DC7 RID: 3527 RVA: 0x000CC2DC File Offset: 0x000CA4DC
	public StationComponent NewStationComponent(int _entityId, int _pcId, PrefabDesc _desc)
	{
		int extraStorage;
		if (_desc.isCollectStation)
		{
			extraStorage = GameMain.history.localStationExtraStorage;
		}
		else if (_desc.isVeinCollector)
		{
			extraStorage = GameMain.history.localStationExtraStorage;
		}
		else if (_desc.isStellarStation)
		{
			extraStorage = GameMain.history.remoteStationExtraStorage;
		}
		else
		{
			extraStorage = GameMain.history.localStationExtraStorage;
		}
		bool logisticShipWarpDrive = GameMain.history.logisticShipWarpDrive;
		int num;
		if (this.stationRecycleCursor > 0)
		{
			int[] array = this.stationRecycle;
			num = this.stationRecycleCursor - 1;
			this.stationRecycleCursor = num;
			int num2 = array[num];
			StationComponent stationComponent = this.stationPool[num2];
			if (stationComponent == null)
			{
				stationComponent = new StationComponent();
				this.stationPool[num2] = stationComponent;
			}
			if (_desc.isStellarStation)
			{
				this.gameData.galacticTransport.AddStationComponent(this.planet.id, stationComponent);
			}
			if (_desc.isCollectStation)
			{
				PlanetData planetData = this.factory.planet;
				int num3 = planetData.gasItems.Length;
				stationComponent.collectionIds = new int[num3];
				stationComponent.collectionPerTick = new float[num3];
				for (int i = 0; i < num3; i++)
				{
					stationComponent.collectionIds[i] = planetData.gasItems[i];
					double num4 = 0.0;
					if ((double)_desc.stationCollectSpeed * planetData.gasTotalHeat != 0.0)
					{
						num4 = 1.0 - (double)_desc.workEnergyPerTick / ((double)_desc.stationCollectSpeed * planetData.gasTotalHeat * 0.016666666666666666);
					}
					if (num4 == 0.0)
					{
						stationComponent.collectionPerTick[i] = planetData.gasSpeeds[i] * 0.016666668f * (float)_desc.stationCollectSpeed;
					}
					else
					{
						stationComponent.collectionPerTick[i] = planetData.gasSpeeds[i] * 0.016666668f * (float)_desc.stationCollectSpeed * (float)num4;
					}
				}
			}
			else if (_desc.isVeinCollector)
			{
				stationComponent.collectionIds = new int[1];
				stationComponent.collectionPerTick = new float[1];
			}
			stationComponent.Init(num2, _entityId, _pcId, _desc, this.factory.entityPool, extraStorage, logisticShipWarpDrive);
			if (_desc.isCollectStation)
			{
				this.gameData.galacticTransport.RefreshTraffic(stationComponent.gid);
			}
			return stationComponent;
		}
		num = this.stationCursor;
		this.stationCursor = num + 1;
		int num5 = num;
		if (num5 == this.stationCapacity)
		{
			this.SetStationCapacity(this.stationCapacity * 2);
		}
		StationComponent stationComponent2 = this.stationPool[num5];
		if (stationComponent2 == null)
		{
			stationComponent2 = new StationComponent();
			this.stationPool[num5] = stationComponent2;
		}
		if (_desc.isStellarStation)
		{
			this.gameData.galacticTransport.AddStationComponent(this.planet.id, stationComponent2);
		}
		if (_desc.isCollectStation)
		{
			PlanetData planetData2 = this.factory.planet;
			int num6 = planetData2.gasItems.Length;
			stationComponent2.collectionIds = new int[num6];
			stationComponent2.collectionPerTick = new float[num6];
			for (int j = 0; j < num6; j++)
			{
				stationComponent2.collectionIds[j] = planetData2.gasItems[j];
				double num7 = 0.0;
				if ((double)_desc.stationCollectSpeed * planetData2.gasTotalHeat != 0.0)
				{
					num7 = 1.0 - (double)_desc.workEnergyPerTick / ((double)_desc.stationCollectSpeed * planetData2.gasTotalHeat * 0.016666666666666666);
				}
				if (num7 == 0.0)
				{
					stationComponent2.collectionPerTick[j] = planetData2.gasSpeeds[j] * 0.016666668f * (float)_desc.stationCollectSpeed;
				}
				else
				{
					stationComponent2.collectionPerTick[j] = planetData2.gasSpeeds[j] * 0.016666668f * (float)_desc.stationCollectSpeed * (float)num7;
				}
			}
		}
		else if (_desc.isVeinCollector)
		{
			stationComponent2.collectionIds = new int[1];
			stationComponent2.collectionPerTick = new float[]
			{
				0.016666668f * (float)_desc.stationCollectSpeed
			};
		}
		stationComponent2.Init(num5, _entityId, _pcId, _desc, this.factory.entityPool, extraStorage, logisticShipWarpDrive);
		if (_desc.isCollectStation)
		{
			this.gameData.galacticTransport.RefreshTraffic(stationComponent2.gid);
		}
		return stationComponent2;
	}

	// Token: 0x06000DC8 RID: 3528 RVA: 0x000CC708 File Offset: 0x000CA908
	public void RemoveStationComponent(int id)
	{
		int num = 0;
		if (this.stationPool[id] != null && this.stationPool[id].id != 0)
		{
			StationComponent stationComponent = this.stationPool[id];
			num = stationComponent.gid;
			for (int i = 0; i < stationComponent.workDroneCount; i++)
			{
				int endId = stationComponent.workDroneDatas[i].endId;
				if (endId > 0)
				{
					StationComponent stationComponent2 = this.stationPool[endId];
					StationStore[] array = (stationComponent2 == null) ? null : stationComponent2.storage;
					LocalLogisticOrder[] workDroneOrders = stationComponent.workDroneOrders;
					if (stationComponent.storage[workDroneOrders[i].thisIndex].itemId == workDroneOrders[i].itemId)
					{
						StationStore[] storage = stationComponent.storage;
						int thisIndex = workDroneOrders[i].thisIndex;
						storage[thisIndex].localOrder = storage[thisIndex].localOrder - workDroneOrders[i].thisOrdered;
					}
					if (array != null && array[workDroneOrders[i].otherIndex].itemId == workDroneOrders[i].itemId)
					{
						StationStore[] array2 = array;
						int otherIndex = workDroneOrders[i].otherIndex;
						array2[otherIndex].localOrder = array2[otherIndex].localOrder - workDroneOrders[i].otherOrdered;
					}
					workDroneOrders[i].ClearThis();
					workDroneOrders[i].ClearOther();
				}
			}
			for (int j = 0; j < stationComponent.workShipCount; j++)
			{
				int otherGId = stationComponent.workShipDatas[j].otherGId;
				if (otherGId > 0)
				{
					StationComponent stationComponent3 = this.gameData.galacticTransport.stationPool[otherGId];
					StationStore[] array3 = (stationComponent3 == null) ? null : stationComponent3.storage;
					RemoteLogisticOrder[] workShipOrders = stationComponent.workShipOrders;
					if (stationComponent.storage[workShipOrders[j].thisIndex].itemId == workShipOrders[j].itemId)
					{
						StationStore[] storage2 = stationComponent.storage;
						int thisIndex2 = workShipOrders[j].thisIndex;
						storage2[thisIndex2].remoteOrder = storage2[thisIndex2].remoteOrder - workShipOrders[j].thisOrdered;
					}
					if (array3 != null && array3[workShipOrders[j].otherIndex].itemId == workShipOrders[j].itemId)
					{
						StationStore[] array4 = array3;
						int otherIndex2 = workShipOrders[j].otherIndex;
						array4[otherIndex2].remoteOrder = array4[otherIndex2].remoteOrder - workShipOrders[j].otherOrdered;
					}
					workShipOrders[j].ClearThis();
					workShipOrders[j].ClearOther();
				}
			}
			for (int k = 0; k < stationComponent.storage.Length; k++)
			{
				if (stationComponent.storage[k].itemId != 0)
				{
					this.SetStationStorage(id, k, 0, 0, ELogisticStorage.None, ELogisticStorage.None, this.player);
				}
			}
			stationComponent.entityId = 0;
			stationComponent.id = 0;
			stationComponent.Reset();
			int[] array5 = this.stationRecycle;
			int num2 = this.stationRecycleCursor;
			this.stationRecycleCursor = num2 + 1;
			array5[num2] = id;
		}
		this.RefreshStationTraffic(id);
		if (num > 0)
		{
			this.gameData.galacticTransport.RemoveStationComponent(num);
		}
	}

	// Token: 0x06000DC9 RID: 3529 RVA: 0x000CCA24 File Offset: 0x000CAC24
	public StationComponent GetStationComponent(int id)
	{
		if (id <= 0 && id >= this.stationCursor)
		{
			return null;
		}
		StationComponent stationComponent = this.stationPool[id];
		if (stationComponent != null && stationComponent.id == id)
		{
			return stationComponent;
		}
		return null;
	}

	// Token: 0x06000DCA RID: 3530 RVA: 0x000CCA58 File Offset: 0x000CAC58
	public void TakeBackItems_Station(Player player, int stationId)
	{
		if (stationId == 0)
		{
			return;
		}
		if (player == null)
		{
			return;
		}
		StationComponent stationComponent = this.GetStationComponent(stationId);
		if (stationComponent == null || stationComponent.id != stationId)
		{
			return;
		}
		for (int i = 0; i < stationComponent.storage.Length; i++)
		{
			if (stationComponent.storage[i].count > 0 && stationComponent.storage[i].itemId > 0)
			{
				int upCount = player.TryAddItemToPackage(stationComponent.storage[i].itemId, stationComponent.storage[i].count, stationComponent.storage[i].inc, true, stationComponent.entityId, false);
				UIItemup.Up(stationComponent.storage[i].itemId, upCount);
				stationComponent.storage[i].count = 0;
				stationComponent.storage[i].inc = 0;
			}
		}
		if (stationComponent.idleDroneCount > 0)
		{
			int upCount2 = player.TryAddItemToPackage(5001, stationComponent.idleDroneCount, 0, true, stationComponent.entityId, false);
			UIItemup.Up(5001, upCount2);
			stationComponent.idleDroneCount = 0;
		}
		if (stationComponent.idleShipCount > 0)
		{
			int upCount3 = player.TryAddItemToPackage(5002, stationComponent.idleShipCount, 0, true, stationComponent.entityId, false);
			UIItemup.Up(5002, upCount3);
			stationComponent.idleShipCount = 0;
		}
		if (stationComponent.warperCount > 0)
		{
			int upCount4 = player.TryAddItemToPackage(1210, stationComponent.warperCount, 0, true, stationComponent.entityId, false);
			UIItemup.Up(1210, upCount4);
			stationComponent.warperCount = 0;
		}
	}

	// Token: 0x06000DCB RID: 3531 RVA: 0x000CCBEC File Offset: 0x000CADEC
	public bool TryTakeBackItems_Station(StorageComponent package, int stationId)
	{
		if (stationId == 0)
		{
			return false;
		}
		if (package == null)
		{
			return false;
		}
		StationComponent stationComponent = this.GetStationComponent(stationId);
		if (stationComponent == null || stationComponent.id != stationId)
		{
			return false;
		}
		StorageComponent storageComponent = new StorageComponent(package.size);
		Array.Copy(package.grids, storageComponent.grids, package.size);
		for (int i = 0; i < stationComponent.storage.Length; i++)
		{
			int num;
			if (stationComponent.storage[i].count > 0 && stationComponent.storage[i].itemId > 0 && storageComponent.AddItemStacked(stationComponent.storage[i].itemId, stationComponent.storage[i].count, stationComponent.storage[i].inc, out num) < stationComponent.storage[i].count)
			{
				return false;
			}
		}
		int num2;
		int num3;
		int num4;
		return (stationComponent.idleDroneCount <= 0 || storageComponent.AddItemStacked(5001, stationComponent.idleDroneCount, 0, out num2) >= stationComponent.idleDroneCount) && (stationComponent.idleShipCount <= 0 || storageComponent.AddItemStacked(5002, stationComponent.idleShipCount, 0, out num3) >= stationComponent.idleShipCount) && (stationComponent.warperCount <= 0 || storageComponent.AddItemStacked(1210, stationComponent.warperCount, 0, out num4) >= stationComponent.warperCount);
	}

	// Token: 0x06000DCC RID: 3532 RVA: 0x000CCD40 File Offset: 0x000CAF40
	public void ThrowItems_Station(int stationId, float dropRate)
	{
		if (stationId == 0)
		{
			return;
		}
		StationComponent stationComponent = this.GetStationComponent(stationId);
		if (stationComponent == null || stationComponent.id != stationId)
		{
			return;
		}
		TrashSystem trashSystem = GameMain.data.trashSystem;
		for (int i = 0; i < stationComponent.storage.Length; i++)
		{
			if (stationComponent.storage[i].count > 0 && stationComponent.storage[i].itemId > 0)
			{
				float num = dropRate * Random.Range(0.8f, 1.2f);
				if (num > 1f)
				{
					num = 1f;
				}
				int num2 = (int)((float)stationComponent.storage[i].count * num);
				float num3 = (float)num2 / (float)stationComponent.storage[i].count;
				trashSystem.AddTrashOnPlanet(stationComponent.storage[i].itemId, num2, (int)((float)stationComponent.storage[i].inc * num3), stationComponent.entityId, this.planet);
				stationComponent.storage[i].count = 0;
				stationComponent.storage[i].inc = 0;
			}
		}
		if (stationComponent.idleDroneCount > 0)
		{
			float num4 = dropRate * Random.Range(0.8f, 1.2f);
			if (num4 > 1f)
			{
				num4 = 1f;
			}
			trashSystem.AddTrashOnPlanet(5001, (int)((float)stationComponent.idleDroneCount * num4), 0, stationComponent.entityId, this.planet);
			stationComponent.idleDroneCount = 0;
		}
		if (stationComponent.idleShipCount > 0)
		{
			float num5 = dropRate * Random.Range(0.8f, 1.2f);
			if (num5 > 1f)
			{
				num5 = 1f;
			}
			trashSystem.AddTrashOnPlanet(5002, (int)((float)stationComponent.idleShipCount * num5), 0, stationComponent.entityId, this.planet);
			stationComponent.idleShipCount = 0;
		}
		if (stationComponent.warperCount > 0)
		{
			float num6 = dropRate * Random.Range(0.8f, 1.2f);
			if (num6 > 1f)
			{
				num6 = 1f;
			}
			trashSystem.AddTrashOnPlanet(1210, (int)((float)stationComponent.warperCount * num6), 0, stationComponent.entityId, this.planet);
			stationComponent.warperCount = 0;
		}
	}

	// Token: 0x06000DCD RID: 3533 RVA: 0x000CCF68 File Offset: 0x000CB168
	public void ClearItems_Station(int stationId)
	{
		if (stationId == 0)
		{
			return;
		}
		StationComponent stationComponent = this.GetStationComponent(stationId);
		if (stationComponent == null || stationComponent.id != stationId)
		{
			return;
		}
		for (int i = 0; i < stationComponent.storage.Length; i++)
		{
			if (stationComponent.storage[i].count > 0 && stationComponent.storage[i].itemId > 0)
			{
				stationComponent.storage[i].count = 0;
				stationComponent.storage[i].inc = 0;
			}
		}
		if (stationComponent.idleDroneCount > 0)
		{
			stationComponent.idleDroneCount = 0;
		}
		if (stationComponent.idleShipCount > 0)
		{
			stationComponent.idleShipCount = 0;
		}
		if (stationComponent.warperCount > 0)
		{
			stationComponent.warperCount = 0;
		}
	}

	// Token: 0x06000DCE RID: 3534 RVA: 0x000CD020 File Offset: 0x000CB220
	private void SetDispenserCapacity(int newCapacity)
	{
		DispenserComponent[] array = this.dispenserPool;
		this.dispenserPool = new DispenserComponent[newCapacity];
		this.dispenserRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.dispenserPool, (newCapacity > this.dispenserCapacity) ? this.dispenserCapacity : newCapacity);
		}
		this.dispenserCapacity = newCapacity;
	}

	// Token: 0x06000DCF RID: 3535 RVA: 0x000CD074 File Offset: 0x000CB274
	public int NewDispenserComponent(int _entityId, int _pcId, PrefabDesc _desc)
	{
		int num2;
		if (this.dispenserRecycleCursor > 0)
		{
			int[] array = this.dispenserRecycle;
			int num = this.dispenserRecycleCursor - 1;
			this.dispenserRecycleCursor = num;
			num2 = array[num];
		}
		else
		{
			int num = this.dispenserCursor;
			this.dispenserCursor = num + 1;
			num2 = num;
			if (num2 == this.dispenserCapacity)
			{
				this.SetDispenserCapacity(this.dispenserCapacity * 2);
			}
		}
		if (this.dispenserPool[num2] == null)
		{
			DispenserComponent dispenserComponent = new DispenserComponent();
			this.dispenserPool[num2] = dispenserComponent;
		}
		this.dispenserPool[num2].Init(num2, _entityId, _pcId, _desc);
		Player mainPlayer = this.gameData.mainPlayer;
		this.dispenserPool[num2].deliveryPackage = mainPlayer.deliveryPackage;
		this.dispenserPool[num2].packageUtility = mainPlayer.packageUtility;
		this.factory.entityPool[_entityId].dispenserId = num2;
		return num2;
	}

	// Token: 0x06000DD0 RID: 3536 RVA: 0x000CD148 File Offset: 0x000CB348
	public void RemoveDispenserComponent(int id)
	{
		if (this.dispenserPool[id] != null && this.dispenserPool[id].id != 0)
		{
			DispenserComponent dispenserComponent = this.dispenserPool[id];
			CourierData[] workCourierDatas = dispenserComponent.workCourierDatas;
			DeliveryLogisticOrder[] orders = dispenserComponent.orders;
			Player mainPlayer = this.gameData.mainPlayer;
			for (int i = 0; i < dispenserComponent.workCourierCount; i++)
			{
				int otherId = dispenserComponent.orders[i].otherId;
				if (otherId > 0)
				{
					dispenserComponent.storageOrdered -= orders[i].thisOrdered;
					orders[i].thisOrdered = 0;
					this.dispenserPool[otherId].storageOrdered -= orders[i].otherOrdered;
					orders[i].otherOrdered = 0;
				}
				else if (otherId < 0)
				{
					dispenserComponent.playerOrdered -= orders[i].thisOrdered;
					orders[i].thisOrdered = 0;
					DeliveryPackage.GRID[] grids = mainPlayer.deliveryPackage.grids;
					int num = -(otherId + 1);
					grids[num].ordered = grids[num].ordered - orders[i].otherOrdered;
					orders[i].otherOrdered = 0;
				}
			}
			this.dispenserPool[id].Free();
			int[] array = this.dispenserRecycle;
			int num2 = this.dispenserRecycleCursor;
			this.dispenserRecycleCursor = num2 + 1;
			array[num2] = id;
		}
		this.RefreshDispenserTraffic(id);
	}

	// Token: 0x06000DD1 RID: 3537 RVA: 0x000CD2B0 File Offset: 0x000CB4B0
	public DispenserComponent GetDispenserComponent(int id)
	{
		if (id <= 0 && id >= this.dispenserCapacity)
		{
			return null;
		}
		DispenserComponent dispenserComponent = this.dispenserPool[id];
		if (dispenserComponent != null && dispenserComponent.id == id)
		{
			return dispenserComponent;
		}
		return null;
	}

	// Token: 0x06000DD2 RID: 3538 RVA: 0x000CD2E4 File Offset: 0x000CB4E4
	public void TakeBackItems_Dispenser(Player player, int dispenserId)
	{
		if (dispenserId == 0)
		{
			return;
		}
		if (player == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.GetDispenserComponent(dispenserId);
		if (dispenserComponent == null || dispenserComponent.id != dispenserId)
		{
			return;
		}
		DispenserStore[] holdupPackage = dispenserComponent.holdupPackage;
		for (int i = 0; i < dispenserComponent.holdupItemCount; i++)
		{
			if (holdupPackage[i].count > 0 && holdupPackage[i].itemId > 0)
			{
				int upCount = player.TryAddItemToPackage(holdupPackage[i].itemId, holdupPackage[i].count, holdupPackage[i].inc, true, dispenserComponent.entityId, false);
				UIItemup.Up(holdupPackage[i].itemId, upCount);
				holdupPackage[i].itemId = 0;
				holdupPackage[i].count = 0;
				holdupPackage[i].inc = 0;
			}
		}
		if (dispenserComponent.idleCourierCount > 0)
		{
			int upCount2 = player.TryAddItemToPackage(5003, dispenserComponent.idleCourierCount, 0, true, dispenserComponent.entityId, false);
			UIItemup.Up(5003, upCount2);
			dispenserComponent.idleCourierCount = 0;
		}
	}

	// Token: 0x06000DD3 RID: 3539 RVA: 0x000CD3F0 File Offset: 0x000CB5F0
	public void ClearItems_Dispenser(int dispenserId)
	{
		if (dispenserId == 0)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.GetDispenserComponent(dispenserId);
		if (dispenserComponent == null || dispenserComponent.id != dispenserId)
		{
			return;
		}
		DispenserStore[] holdupPackage = dispenserComponent.holdupPackage;
		for (int i = 0; i < dispenserComponent.holdupItemCount; i++)
		{
			if (holdupPackage[i].count > 0 && holdupPackage[i].itemId > 0)
			{
				holdupPackage[i].itemId = 0;
				holdupPackage[i].count = 0;
				holdupPackage[i].inc = 0;
			}
		}
		if (dispenserComponent.idleCourierCount > 0)
		{
			dispenserComponent.idleCourierCount = 0;
		}
	}

	// Token: 0x06000DD4 RID: 3540 RVA: 0x000CD484 File Offset: 0x000CB684
	public void ConnectToDispenser(int dispenserId, int storageId)
	{
		if (dispenserId != 0 && this.dispenserPool[dispenserId].id == dispenserId && storageId != 0)
		{
			this.dispenserPool[dispenserId].storageId = storageId;
			this.dispenserPool[dispenserId].storage = this.factory.factoryStorage.storagePool[storageId];
			this.dispenserPool[dispenserId].ResetSearchStart();
		}
	}

	// Token: 0x06000DD5 RID: 3541 RVA: 0x000CD4E4 File Offset: 0x000CB6E4
	public void GameTick(long time, bool isActive, bool multithreaded, int threadOrdinal)
	{
		int num = (int)(time % 1163962800L);
		if (num < 0)
		{
			num += 1163962800;
		}
		int num2 = (int)(time % 60L);
		if (num2 < 0)
		{
			num2 += 60;
		}
		GameHistoryData history = GameMain.history;
		FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[this.factory.index];
		int[] productRegister = factoryProductionStat.productRegister;
		int[] consumeRegister = factoryProductionStat.consumeRegister;
		float[] networkServes = this.powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = this.powerSystem.consumerPool;
		float logisticDroneSpeedModified = history.logisticDroneSpeedModified;
		int logisticDroneCarries = history.logisticDroneCarries;
		float logisticShipSailSpeedModified = history.logisticShipSailSpeedModified;
		float shipWarpSpeed = history.logisticShipWarpDrive ? history.logisticShipWarpSpeedModified : logisticShipSailSpeedModified;
		int logisticShipCarries = history.logisticShipCarries;
		StationComponent[] gStationPool = this.gameData.galacticTransport.stationPool;
		AstroData[] astrosData = this.gameData.galaxy.astrosData;
		VectorLF3 relativePos = this.gameData.relativePos;
		Quaternion relativeRot = this.gameData.relativeRot;
		AnimData[] entityAnimPool = this.factory.entityAnimPool;
		double num3 = (double)history.miningSpeedScale;
		double num4 = this.collectorsWorkCost;
		double gasTotalHeat = this.planet.gasTotalHeat;
		float collectSpeedRate = (gasTotalHeat - num4 <= 0.0) ? 1f : ((float)((num3 * gasTotalHeat - num4) / (gasTotalHeat - num4)));
		bool starmap = UIGame.viewMode == EViewMode.Starmap;
		this.GameTick_SandboxMode(threadOrdinal);
		if (multithreaded)
		{
			DeepProfiler.BeginSample(DPEntry.Station, threadOrdinal, -1L);
		}
		for (int i = 1; i < this.stationCursor; i++)
		{
			StationComponent stationComponent = this.stationPool[i];
			if (stationComponent != null && stationComponent.id == i)
			{
				float power = networkServes[consumerPool[stationComponent.pcId].networkId];
				stationComponent.InternalTickLocal(this.factory, num, power, logisticDroneSpeedModified, logisticDroneCarries, this.stationPool);
				if (stationComponent.isCollector)
				{
					stationComponent.UpdateCollection(this.factory, collectSpeedRate, productRegister);
					FactoryProductionStat obj = factoryProductionStat;
					lock (obj)
					{
						factoryProductionStat.energyConsumption += this.collectorWorkEnergyPerTick;
					}
				}
				if (stationComponent.isVeinCollector)
				{
					stationComponent.UpdateVeinCollection(this.factory, productRegister);
				}
				if (stationComponent.isStellar)
				{
					stationComponent.InternalTickRemote(this.factory, num2, logisticShipSailSpeedModified, shipWarpSpeed, logisticShipCarries, gStationPool, astrosData, ref relativePos, ref relativeRot, starmap, consumeRegister);
				}
				if (isActive && !stationComponent.isVeinCollector)
				{
					entityAnimPool[stationComponent.entityId].power = ((stationComponent.energy > 0L) ? 1f : 0f);
					entityAnimPool[stationComponent.entityId].state = 1U;
				}
				if (!this.stationPool[i].isCollector && !this.stationPool[i].isVeinCollector)
				{
					stationComponent.SetPCState(consumerPool);
				}
			}
		}
		this.GameTick_UpdateNeeds();
		if (multithreaded)
		{
			DeepProfiler.EndSample(threadOrdinal, -2L);
			DeepProfiler.BeginSample(DPEntry.Dispensor, threadOrdinal, -1L);
		}
		double num5 = Math.Cos((double)history.dispenserDeliveryMaxAngle * 3.141592653589793 / 180.0);
		if (num5 < -0.999)
		{
			num5 = -1.0;
		}
		Vector3 vector = multithreaded ? this.multithreadPlayerPos : this.player.position;
		vector += vector.normalized * 2.66666f;
		bool flag2 = this.playerDeliveryEnabled;
		this.DeterminePlayerDeliveryEnabled(this.factory);
		if (flag2 != this.playerDeliveryEnabled)
		{
			this.RefreshDispenserTraffic(-10000);
		}
		if (isActive)
		{
			for (int j = 1; j < this.dispenserCursor; j++)
			{
				if (this.dispenserPool[j] != null && this.dispenserPool[j].id == j)
				{
					float power2 = networkServes[consumerPool[this.dispenserPool[j].pcId].networkId];
					this.dispenserPool[j].InternalTick(this.factory, this.factory.entityPool, this.dispenserPool, vector, time, power2, history.logisticCourierSpeedModified, history.logisticCourierCarries, num5);
					int entityId = this.dispenserPool[j].entityId;
					entityAnimPool[entityId].power = ((this.dispenserPool[j].energy > 0L) ? 1f : 0f);
					entityAnimPool[entityId].state = ((this.dispenserPool[j].holdupItemCount > 0) ? (entityAnimPool[entityId].state | 2U) : (entityAnimPool[entityId].state & 4294967293U));
					entityAnimPool[entityId].time = ((this.dispenserPool[j].pulseSignal > 0) ? 4f : 1f);
					this.dispenserPool[j].SetPCState(consumerPool);
				}
			}
		}
		else
		{
			for (int k = 1; k < this.dispenserCursor; k++)
			{
				if (this.dispenserPool[k] != null && this.dispenserPool[k].id == k)
				{
					float power3 = networkServes[consumerPool[this.dispenserPool[k].pcId].networkId];
					this.dispenserPool[k].InternalTick(this.factory, this.factory.entityPool, this.dispenserPool, vector, time, power3, history.logisticCourierSpeedModified, history.logisticCourierCarries, num5);
					this.dispenserPool[k].SetPCState(consumerPool);
				}
			}
		}
		if (multithreaded)
		{
			DeepProfiler.EndSample(threadOrdinal, -2L);
		}
	}

	// Token: 0x06000DD6 RID: 3542 RVA: 0x000CDA78 File Offset: 0x000CBC78
	public void GameTick_UpdateNeeds()
	{
		int[][] entityNeeds = this.factory.entityNeeds;
		for (int i = 1; i < this.stationCursor; i++)
		{
			StationComponent stationComponent = this.stationPool[i];
			if (stationComponent != null && stationComponent.id == i)
			{
				stationComponent.UpdateNeeds();
				entityNeeds[stationComponent.entityId] = stationComponent.needs;
			}
		}
	}

	// Token: 0x06000DD7 RID: 3543 RVA: 0x000CDACC File Offset: 0x000CBCCC
	public void GameTick_InputFromBelt(long time)
	{
		CargoTraffic cargoTraffic = this.factory.cargoTraffic;
		SignData[] entitySignPool = this.factory.entitySignPool;
		bool active = (time + (long)this.factory.index) % 30L == 0L || this.planet == this.gameData.localPlanet;
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].id == i)
			{
				this.stationPool[i].UpdateInputSlots(cargoTraffic, entitySignPool, active);
			}
		}
	}

	// Token: 0x06000DD8 RID: 3544 RVA: 0x000CDB58 File Offset: 0x000CBD58
	public void GameTick_OutputToBelt(int maxPilerCount, long time)
	{
		CargoTraffic cargoTraffic = this.factory.cargoTraffic;
		SignData[] entitySignPool = this.factory.entitySignPool;
		bool active = (time + (long)this.factory.index) % 30L == 0L || this.planet == this.gameData.localPlanet;
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].id == i)
			{
				this.stationPool[i].UpdateOutputSlots(cargoTraffic, entitySignPool, maxPilerCount, active);
			}
		}
	}

	// Token: 0x06000DD9 RID: 3545 RVA: 0x000CDBE4 File Offset: 0x000CBDE4
	public void GameTick_SandboxMode(int threadOrdinal)
	{
		if (GameMain.sandboxToolsEnabled)
		{
			if (threadOrdinal >= 0)
			{
				DeepProfiler.BeginSample(DPEntry.Station, threadOrdinal, -1L);
			}
			for (int i = 1; i < this.stationCursor; i++)
			{
				if (this.stationPool[i] != null && this.stationPool[i].id == i)
				{
					this.stationPool[i].UpdateKeepMode();
				}
			}
			if (threadOrdinal >= 0)
			{
				DeepProfiler.EndSample(threadOrdinal, -2L);
				DeepProfiler.BeginSample(DPEntry.Dispensor, threadOrdinal, -1L);
			}
			for (int j = 1; j < this.dispenserCursor; j++)
			{
				if (this.dispenserPool[j] != null && this.dispenserPool[j].id == j)
				{
					this.dispenserPool[j].UpdateKeepMode();
				}
			}
			if (threadOrdinal >= 0)
			{
				DeepProfiler.EndSample(threadOrdinal, -2L);
			}
		}
	}

	// Token: 0x06000DDA RID: 3546 RVA: 0x000CDCA0 File Offset: 0x000CBEA0
	public void SetStationStorage(int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, Player player)
	{
		if (itemId != 0 && LDB.items.Select(itemId) == null)
		{
			itemId = 0;
		}
		bool flag = false;
		bool flag2 = false;
		StationComponent stationComponent = this.GetStationComponent(stationId);
		if (stationComponent != null)
		{
			if (!stationComponent.isStellar)
			{
				remoteLogic = ELogisticStorage.None;
			}
			if (itemId <= 0)
			{
				itemId = 0;
				itemCountMax = 0;
				localLogic = ELogisticStorage.None;
				remoteLogic = ELogisticStorage.None;
			}
			int modelIndex = (int)this.factory.entityPool[stationComponent.entityId].modelIndex;
			ModelProto modelProto = LDB.models.Select(modelIndex);
			int num = 0;
			if (modelProto != null)
			{
				num = modelProto.prefabDesc.stationMaxItemCount;
			}
			int num2;
			if (stationComponent.isCollector)
			{
				num2 = GameMain.history.localStationExtraStorage;
			}
			else if (stationComponent.isVeinCollector)
			{
				num2 = GameMain.history.localStationExtraStorage;
			}
			else if (stationComponent.isStellar)
			{
				num2 = GameMain.history.remoteStationExtraStorage;
			}
			else
			{
				num2 = GameMain.history.localStationExtraStorage;
			}
			if (itemCountMax > num + num2)
			{
				itemCountMax = num + num2;
			}
			if (storageIdx >= 0 && storageIdx < stationComponent.storage.Length)
			{
				StationStore stationStore = stationComponent.storage[storageIdx];
				if (stationStore.localLogic != localLogic)
				{
					flag = true;
				}
				if (stationStore.remoteLogic != remoteLogic)
				{
					flag2 = true;
				}
				if (stationStore.itemId == itemId)
				{
					stationComponent.storage[storageIdx].max = itemCountMax;
					stationComponent.storage[storageIdx].localLogic = localLogic;
					stationComponent.storage[storageIdx].remoteLogic = remoteLogic;
				}
				else
				{
					if (stationStore.localLogic != ELogisticStorage.None || localLogic != ELogisticStorage.None)
					{
						flag = true;
					}
					if (stationStore.remoteLogic != ELogisticStorage.None || remoteLogic != ELogisticStorage.None)
					{
						flag2 = true;
					}
					if (stationStore.count > 0 && stationStore.itemId > 0 && player != null)
					{
						int num3 = player.TryAddItemToPackage(stationStore.itemId, stationStore.count, stationStore.inc, true, 0, false);
						UIItemup.Up(stationStore.itemId, num3);
						if (num3 < stationStore.count)
						{
							UIRealtimeTip.Popup("无法收回仓储物品".Translate(), true, 0);
						}
					}
					stationComponent.storage[storageIdx].itemId = itemId;
					stationComponent.storage[storageIdx].count = 0;
					stationComponent.storage[storageIdx].inc = 0;
					stationComponent.storage[storageIdx].localOrder = 0;
					stationComponent.storage[storageIdx].remoteOrder = 0;
					stationComponent.storage[storageIdx].max = itemCountMax;
					stationComponent.storage[storageIdx].localLogic = localLogic;
					stationComponent.storage[storageIdx].remoteLogic = remoteLogic;
				}
				if (itemId == 0)
				{
					stationComponent.storage[storageIdx] = default(StationStore);
					for (int i = 0; i < stationComponent.slots.Length; i++)
					{
						if (stationComponent.slots[i].dir == IODir.Output && stationComponent.slots[i].storageIdx - 1 == storageIdx)
						{
							stationComponent.slots[i].counter = 0;
							stationComponent.slots[i].storageIdx = 0;
							stationComponent.slots[i].dir = IODir.Output;
						}
					}
				}
			}
			if (!stationComponent.isStellar)
			{
				flag2 = false;
			}
		}
		if (flag)
		{
			this.RefreshStationTraffic(stationId);
		}
		if (flag2)
		{
			this.gameData.galacticTransport.RefreshTraffic(stationComponent.gid);
		}
	}

	// Token: 0x06000DDB RID: 3547 RVA: 0x000CDFE8 File Offset: 0x000CC1E8
	public void RefreshStationTraffic(int keyStationId = 0)
	{
		int logisticDroneCarries = GameMain.history.logisticDroneCarries;
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].id == i)
			{
				this.stationPool[i].ClearLocalPairs();
			}
		}
		for (int j = 1; j < this.stationCursor; j++)
		{
			if (this.stationPool[j] != null && this.stationPool[j].id == j)
			{
				this.stationPool[j].RematchLocalPairs(this.stationPool, this.stationCursor, keyStationId, logisticDroneCarries);
			}
		}
	}

	// Token: 0x06000DDC RID: 3548 RVA: 0x000CE080 File Offset: 0x000CC280
	public void SetDispenserFilter(int dispenserId, int filter)
	{
		if (dispenserId == 0)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.dispenserPool[dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != dispenserId)
		{
			return;
		}
		if (filter == 1099)
		{
			return;
		}
		if (dispenserComponent.filter != filter)
		{
			dispenserComponent.filter = filter;
			this.RefreshDispenserTraffic(dispenserId);
		}
	}

	// Token: 0x06000DDD RID: 3549 RVA: 0x000CE0C8 File Offset: 0x000CC2C8
	public void SetDispenserPlayerDeliveryMode(int dispenserId, EPlayerDeliveryMode playerDeliveryMode)
	{
		if (dispenserId == 0)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.dispenserPool[dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != dispenserId)
		{
			return;
		}
		if (dispenserComponent.playerMode != playerDeliveryMode)
		{
			dispenserComponent.playerMode = playerDeliveryMode;
			this.RefreshDispenserTraffic(dispenserId);
		}
	}

	// Token: 0x06000DDE RID: 3550 RVA: 0x000CE108 File Offset: 0x000CC308
	public void SetDispenserStorageDeliveryMode(int dispenserId, EStorageDeliveryMode storageDeliveryMode)
	{
		if (dispenserId == 0)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.dispenserPool[dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != dispenserId)
		{
			return;
		}
		if (dispenserComponent.storageMode != storageDeliveryMode)
		{
			dispenserComponent.storageMode = storageDeliveryMode;
			this.RefreshDispenserTraffic(dispenserId);
		}
	}

	// Token: 0x06000DDF RID: 3551 RVA: 0x000CE148 File Offset: 0x000CC348
	public void RefreshDispenserTraffic(int keyId = 0)
	{
		int logisticCourierCarries = GameMain.history.logisticCourierCarries;
		this.playerDeliveryPackage.ClearPairs();
		for (int i = 1; i < this.dispenserCursor; i++)
		{
			if (this.dispenserPool[i] != null && this.dispenserPool[i].id == i)
			{
				this.dispenserPool[i].ClearPairs();
			}
		}
		if (this.playerDeliveryEnabled)
		{
			DeliveryPackage.GRID[] grids = this.playerDeliveryPackage.grids;
			for (int j = 0; j < grids.Length; j++)
			{
				this.playerDeliveryPackage.gridsPairOffsets[j] = this.playerDeliveryPackage.pairCount;
				if (grids[j].itemId > 0 && this.playerDeliveryPackage.IsGridActive(j))
				{
					int itemId = grids[j].itemId;
					if (itemId != 1099)
					{
						for (int k = 1; k < this.dispenserCursor; k++)
						{
							DispenserComponent dispenserComponent = this.dispenserPool[k];
							if (dispenserComponent != null && dispenserComponent.id == k)
							{
								if ((dispenserComponent.playerMode == EPlayerDeliveryMode.Supply || dispenserComponent.playerMode == EPlayerDeliveryMode.Both) && itemId == dispenserComponent.filter)
								{
									dispenserComponent.AddPair(k, 0, -(j + 1), j);
									this.playerDeliveryPackage.AddGridPair(k, 0, -(j + 1), j);
								}
								if ((dispenserComponent.playerMode == EPlayerDeliveryMode.Recycle || dispenserComponent.playerMode == EPlayerDeliveryMode.Both) && (itemId == dispenserComponent.filter || (dispenserComponent.filter < 0 && itemId > 0)))
								{
									dispenserComponent.AddPair(-(j + 1), j, k, 0);
									this.playerDeliveryPackage.AddGridPair(-(j + 1), j, k, 0);
								}
							}
						}
					}
				}
			}
		}
		for (int l = 1; l < this.dispenserCursor; l++)
		{
			DispenserComponent dispenserComponent2 = this.dispenserPool[l];
			if (dispenserComponent2 != null && dispenserComponent2.id == l)
			{
				int filter = dispenserComponent2.filter;
				if (filter > 0 && filter != 1099)
				{
					if (dispenserComponent2.storageMode == EStorageDeliveryMode.Supply)
					{
						for (int m = l + 1; m < this.dispenserCursor; m++)
						{
							DispenserComponent dispenserComponent3 = this.dispenserPool[m];
							if (dispenserComponent3 != null && dispenserComponent3.id == m && dispenserComponent3.storageMode == EStorageDeliveryMode.Demand && dispenserComponent3.filter == filter)
							{
								dispenserComponent2.AddPair(l, 0, m, 0);
								dispenserComponent3.AddPair(l, 0, m, 0);
							}
						}
					}
					else if (dispenserComponent2.storageMode == EStorageDeliveryMode.Demand)
					{
						for (int n = l + 1; n < this.dispenserCursor; n++)
						{
							DispenserComponent dispenserComponent4 = this.dispenserPool[n];
							if (dispenserComponent4 != null && dispenserComponent4.id == n && dispenserComponent4.storageMode == EStorageDeliveryMode.Supply && dispenserComponent4.filter == filter)
							{
								dispenserComponent2.AddPair(n, 0, l, 0);
								dispenserComponent4.AddPair(n, 0, l, 0);
							}
						}
					}
				}
				dispenserComponent2.OnRematchPairs(this.factory, this.dispenserPool, keyId, logisticCourierCarries);
			}
		}
		if (PlanetTransport.onFactoryRefreshDispenserTraffic != null)
		{
			PlanetTransport.onFactoryRefreshDispenserTraffic(this.factory, keyId);
		}
	}

	// Token: 0x06000DE0 RID: 3552 RVA: 0x000CE44C File Offset: 0x000CC64C
	public void DeterminePlayerDeliveryEnabled(PlanetFactory factory)
	{
		PlanetData localPlanet = this.gameData.localPlanet;
		this.playerDeliveryEnabled = (this.player.deliveryPackage.unlockedAndEnabled && localPlanet == factory.planet && localPlanet.loaded && localPlanet.factoryLoaded && !this.player.sailing && this.player.isAlive);
	}

	// Token: 0x06000DE1 RID: 3553 RVA: 0x000CE4B4 File Offset: 0x000CC6B4
	public void RefreshDispenserOnStoragePrebuildBuild(int prebuildId)
	{
		if (prebuildId < 0)
		{
			prebuildId = -prebuildId;
			bool flag;
			int num;
			int num2;
			this.factory.ReadObjectConn(-prebuildId, 14, out flag, out num, out num2);
			int num3 = num;
			if (num3 != 0)
			{
				this.factory.ReadObjectConn(num3, 13, out flag, out num, out num2);
				if (num != 0)
				{
					PrebuildData[] prebuildPool = this.factory.prebuildPool;
					EntityData[] entityPool = this.factory.entityPool;
					ItemProto itemProto = LDB.items.Select((int)prebuildPool[prebuildId].protoId);
					if (itemProto != null)
					{
						PrefabDesc prefabDesc = itemProto.prefabDesc;
						float num4 = prefabDesc.colliders[0].pos.y + prefabDesc.colliders[0].ext.y;
						Vector3 pos = prebuildPool[prebuildId].pos;
						Vector3 pos2 = pos.normalized * (pos.magnitude + num4);
						Quaternion rot = prebuildPool[prebuildId].rot;
						int num5 = (num < 0) ? (-num) : num;
						int num6;
						if (num > 0)
						{
							entityPool[num5].pos = pos2;
							entityPool[num5].rot = rot;
							entityPool[num5].alt = pos2.magnitude;
							int modelIndex = (int)entityPool[num5].modelIndex;
							int modelId = entityPool[num5].modelId;
							this.factory.planet.factoryModel.gpuiManager.AlterModel(modelIndex, modelId, num5, pos2, rot, true);
							num6 = entityPool[num5].colliderId;
						}
						else
						{
							prebuildPool[num5].pos = pos2;
							prebuildPool[num5].rot = rot;
							int modelIndex2 = (int)prebuildPool[num5].modelIndex;
							int modelId2 = prebuildPool[num5].modelId;
							this.factory.planet.factoryModel.gpuiManager.AlterPrebuildModel(modelIndex2, modelId2, num5, pos2, rot, true);
							num6 = prebuildPool[num5].colliderId;
						}
						int num7 = num6 >> 20;
						num6 &= 1048575;
						PlanetPhysics physics = this.factory.planet.physics;
						ColliderData[] colliderPool = physics.colChunks[num7].colliderPool;
						Vector3 pos3 = colliderPool[num6].pos;
						colliderPool[num6].pos = pos3.normalized * (pos3.magnitude + num4);
						physics.SetPlanetPhysicsColliderDirty();
						this.factory.ClearObjectConn(num3, 13);
						this.factory.WriteObjectConn(-prebuildId, 13, true, num, 0);
					}
					if (num > 0)
					{
						int dispenserId = entityPool[num].dispenserId;
						if (dispenserId > 0 && this.dispenserPool[dispenserId] != null)
						{
							DispenserComponent dispenserComponent = this.dispenserPool[dispenserId];
							CourierData[] workCourierDatas = dispenserComponent.workCourierDatas;
							DeliveryLogisticOrder[] orders = dispenserComponent.orders;
							for (int i = 0; i < dispenserComponent.workCourierCount; i++)
							{
								if (orders[i].otherId > 0)
								{
									workCourierDatas[i].begin = entityPool[num].pos;
								}
							}
							SupplyDemandPair[] pairs = dispenserComponent.pairs;
							for (int j = dispenserComponent.playerPairCount; j < dispenserComponent.pairCount; j++)
							{
								int num8 = (pairs[j].demandId == dispenserId) ? pairs[j].supplyId : pairs[j].demandId;
								DispenserComponent dispenserComponent2 = this.dispenserPool[num8];
								workCourierDatas = dispenserComponent2.workCourierDatas;
								orders = dispenserComponent2.orders;
								for (int k = 0; k < dispenserComponent2.workCourierCount; k++)
								{
									if (workCourierDatas[k].direction > 0f && orders[k].otherId > 0)
									{
										workCourierDatas[k].end = entityPool[num].pos;
									}
								}
							}
							this.player.gizmo.RefreshDispenserPairing(dispenserId);
						}
						ItemProto itemProto2 = LDB.items.Select((int)entityPool[num].protoId);
						if (itemProto2 != null)
						{
							this.factory.entitySignPool[num].Reset(entityPool[num].pos, entityPool[num].rot, itemProto2.prefabDesc.signHeight, itemProto2.prefabDesc.signSize);
						}
					}
				}
			}
			return;
		}
	}

	// Token: 0x06000DE2 RID: 3554 RVA: 0x000CE91C File Offset: 0x000CCB1C
	public void OnTechFunctionUnlocked(int _funcId, double _valuelf, int _level)
	{
		if (_funcId == 30)
		{
			for (int i = 1; i < this.stationCursor; i++)
			{
				if (this.stationPool[i] != null && this.stationPool[i].id == i && (!this.stationPool[i].isStellar || this.stationPool[i].isCollector || this.stationPool[i].isVeinCollector))
				{
					short modelIndex = this.factory.entityPool[this.stationPool[i].entityId].modelIndex;
					int stationMaxItemCount = LDB.models.Select((int)modelIndex).prefabDesc.stationMaxItemCount;
					double num = (double)(stationMaxItemCount + GameMain.history.localStationExtraStorage) / ((double)(stationMaxItemCount + GameMain.history.localStationExtraStorage) - _valuelf);
					StationStore[] storage = this.stationPool[i].storage;
					int num2 = storage.Length;
					for (int j = 0; j < num2; j++)
					{
						storage[j].max = Mathf.RoundToInt((float)((double)storage[j].max * num / 50.0)) * 50;
					}
				}
			}
			return;
		}
		if (_funcId == 31)
		{
			for (int k = 1; k < this.stationCursor; k++)
			{
				if (this.stationPool[k] != null && this.stationPool[k].id == k && this.stationPool[k].isStellar && !this.stationPool[k].isCollector && !this.stationPool[k].isVeinCollector)
				{
					short modelIndex2 = this.factory.entityPool[this.stationPool[k].entityId].modelIndex;
					int stationMaxItemCount2 = LDB.models.Select((int)modelIndex2).prefabDesc.stationMaxItemCount;
					double num3 = (double)(stationMaxItemCount2 + GameMain.history.remoteStationExtraStorage) / ((double)(stationMaxItemCount2 + GameMain.history.remoteStationExtraStorage) - _valuelf);
					StationStore[] storage2 = this.stationPool[k].storage;
					int num4 = storage2.Length;
					for (int l = 0; l < num4; l++)
					{
						storage2[l].max = Mathf.RoundToInt((float)((double)storage2[l].max * num3 / 100.0)) * 100;
					}
				}
			}
		}
	}

	// Token: 0x04000EEA RID: 3818
	public GameData gameData;

	// Token: 0x04000EEB RID: 3819
	public PlanetData planet;

	// Token: 0x04000EEC RID: 3820
	public PlanetFactory factory;

	// Token: 0x04000EED RID: 3821
	public PowerSystem powerSystem;

	// Token: 0x04000EEE RID: 3822
	public Player player;

	// Token: 0x04000EEF RID: 3823
	public DeliveryPackage playerDeliveryPackage;

	// Token: 0x04000EF0 RID: 3824
	public StationComponent[] stationPool;

	// Token: 0x04000EF1 RID: 3825
	public int stationCursor = 1;

	// Token: 0x04000EF2 RID: 3826
	private int stationCapacity;

	// Token: 0x04000EF3 RID: 3827
	private int[] stationRecycle;

	// Token: 0x04000EF4 RID: 3828
	private int stationRecycleCursor;

	// Token: 0x04000EF5 RID: 3829
	public DispenserComponent[] dispenserPool;

	// Token: 0x04000EF6 RID: 3830
	public int dispenserCursor = 1;

	// Token: 0x04000EF7 RID: 3831
	private int dispenserCapacity;

	// Token: 0x04000EF8 RID: 3832
	private int[] dispenserRecycle;

	// Token: 0x04000EF9 RID: 3833
	private int dispenserRecycleCursor;

	// Token: 0x04000EFA RID: 3834
	public bool playerDeliveryEnabled;

	// Token: 0x04000EFB RID: 3835
	public Vector3 multithreadPlayerPos = Vector3.zero;

	// Token: 0x04000EFC RID: 3836
	public LogisticDroneRenderer droneRenderer;

	// Token: 0x04000EFD RID: 3837
	public LogisticCourierRenderer courierRenderer;

	// Token: 0x04000EFE RID: 3838
	public static Action<PlanetFactory, int> onFactoryRefreshDispenserTraffic;

	// Token: 0x04000EFF RID: 3839
	public double collectorsWorkCost;

	// Token: 0x04000F00 RID: 3840
	public long collectorWorkEnergyPerTick;
}
