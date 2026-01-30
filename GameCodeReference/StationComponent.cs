using System;
using System.IO;
using UnityEngine;

// Token: 0x02000153 RID: 339
public class StationComponent
{
	// Token: 0x1700016D RID: 365
	// (get) Token: 0x06000A23 RID: 2595 RVA: 0x0008E91E File Offset: 0x0008CB1E
	public int remotePairTotalCount
	{
		get
		{
			if (this.remotePairOffsets != null)
			{
				return this.remotePairOffsets[1];
			}
			return 0;
		}
	}

	// Token: 0x06000A24 RID: 2596 RVA: 0x0008E934 File Offset: 0x0008CB34
	public void Init(int _id, int _entityId, int _pcId, PrefabDesc _desc, EntityData[] _entityPool, int _extraStorage, bool _logisticShipWarpDrive)
	{
		this.id = _id;
		this.entityId = _entityId;
		this.pcId = _pcId;
		this.minerId = 0;
		if (_desc.isStellarStation)
		{
			this.gene = _id % 60;
		}
		else
		{
			this.gene = _id % 20;
		}
		this.droneDock = _entityPool[_entityId].pos + _entityPool[_entityId].rot * _desc.stationDronePos;
		this.shipDockPos = _entityPool[_entityId].pos + _entityPool[_entityId].rot * _desc.stationShipPos;
		this.shipDockRot = _entityPool[_entityId].rot;
		this.isStellar = _desc.isStellarStation;
		this.isCollector = _desc.isCollectStation;
		this.isVeinCollector = _desc.isVeinCollector;
		this.energy = 0L;
		this.energyPerTick = _desc.workEnergyPerTick;
		this.energyMax = _desc.stationMaxEnergyAcc;
		this.warperCount = 0;
		this.warperMaxCount = (this.isStellar ? (_logisticShipWarpDrive ? 50 : 0) : 0);
		this.idleDroneCount = 0;
		this.workDroneCount = 0;
		this.workDroneDatas = new DroneData[_desc.stationMaxDroneCount];
		this.workDroneOrders = new LocalLogisticOrder[_desc.stationMaxDroneCount];
		this.idleShipCount = 0;
		this.workShipCount = 0;
		this.idleShipIndices = 0UL;
		this.workShipIndices = 0UL;
		this.workShipDatas = new ShipData[_desc.stationMaxShipCount];
		this.workShipOrders = new RemoteLogisticOrder[_desc.stationMaxShipCount];
		this.shipRenderers = new ShipRenderingData[_desc.stationMaxShipCount];
		this.shipUIRenderers = new ShipUIRenderingData[_desc.stationMaxShipCount];
		this.storage = new StationStore[_desc.stationMaxItemKinds];
		this.priorityLocks = new StationPriorityLock[_desc.stationMaxItemKinds];
		this.slots = new SlotData[_desc.portPoses.Length];
		this.localPairProcess = 0;
		this.remotePairProcesses = new int[6];
		this.nextShipIndex = 0;
		this.needs = new int[6];
		this.localPairs = null;
		this.localPairCount = 0;
		this.remotePairs = null;
		this.remotePairOffsets = null;
		this.shipDiskPos = new Vector3[_desc.stationMaxShipCount];
		this.shipDiskRot = new Quaternion[_desc.stationMaxShipCount];
		if (this.isStellar)
		{
			int num = this.workShipDatas.Length;
			for (int i = 0; i < num; i++)
			{
				this.shipDiskRot[i] = Quaternion.Euler(0f, 360f / (float)num * (float)i, 0f);
				this.shipDiskPos[i] = this.shipDiskRot[i] * new Vector3(0f, 0f, 11.5f);
			}
			for (int j = 0; j < num; j++)
			{
				this.shipDiskRot[j] = this.shipDockRot * this.shipDiskRot[j];
				this.shipDiskPos[j] = this.shipDockPos + this.shipDockRot * this.shipDiskPos[j];
			}
		}
		if (this.isCollector)
		{
			this.collectSpeed = _desc.stationCollectSpeed;
			int num2 = this.collectionIds.Length;
			this.currentCollections = new float[num2];
			int num3 = 0;
			while (num3 < num2 && num3 <= _desc.stationMaxItemKinds - 1)
			{
				this.storage[num3].itemId = this.collectionIds[num3];
				this.storage[num3].count = 0;
				this.storage[num3].inc = 0;
				this.storage[num3].remoteLogic = ELogisticStorage.Supply;
				this.storage[num3].max = _desc.stationMaxItemCount + _extraStorage;
				this.storage[num3].keepMode = 0;
				this.storage[num3].keepIncRatio = 0f;
				this.currentCollections[num3] = 0f;
				num3++;
			}
		}
		if (this.isVeinCollector)
		{
			int num4 = this.collectionIds.Length;
			this.currentCollections = new float[num4];
			int num5 = 0;
			while (num5 < num4 && num5 <= _desc.stationMaxItemKinds - 1)
			{
				this.storage[num5].itemId = this.collectionIds[num5];
				this.storage[num5].count = 0;
				this.storage[num5].inc = 0;
				this.storage[num5].localLogic = ELogisticStorage.Supply;
				this.storage[num5].max = _desc.stationMaxItemCount + _extraStorage;
				this.storage[num5].keepMode = 0;
				this.storage[num5].keepIncRatio = 0f;
				this.currentCollections[num5] = 0f;
				num5++;
			}
		}
		this.outSlotOffset = 0;
		this.droneDispatchStatus = new byte[30];
		this.droneStatusCursor = 0;
		this.droneTaskInterval = 20;
		this.InitSettings();
		_entityPool[_entityId].stationId = _id;
	}

	// Token: 0x06000A25 RID: 2597 RVA: 0x0008EE88 File Offset: 0x0008D088
	public void PatchDroneArray(int newCnt)
	{
		if (newCnt != this.workDroneDatas.Length)
		{
			DroneData[] array = this.workDroneDatas;
			this.workDroneDatas = new DroneData[newCnt];
			Array.Copy(array, this.workDroneDatas, (array.Length < newCnt) ? array.Length : newCnt);
		}
		if (newCnt != this.workDroneOrders.Length)
		{
			LocalLogisticOrder[] array2 = this.workDroneOrders;
			this.workDroneOrders = new LocalLogisticOrder[newCnt];
			Array.Copy(array2, this.workDroneOrders, (array2.Length < newCnt) ? array2.Length : newCnt);
		}
	}

	// Token: 0x06000A26 RID: 2598 RVA: 0x0008EF04 File Offset: 0x0008D104
	public void InitSettings()
	{
		this.tripRangeDrones = -1.0;
		this.tripRangeShips = 24000000000.0;
		this.includeOrbitCollector = true;
		this.warpEnableDist = 480000.0;
		this.warperNecessary = true;
		this.deliveryDrones = 10;
		this.deliveryShips = 100;
		this.pilerCount = 0;
		this.droneAutoReplenish = false;
		this.shipAutoReplenish = false;
		this.remoteGroupMask = 0L;
		this.routePriority = (this.isStellar ? ERemoteRoutePriority.Prioritize : ERemoteRoutePriority.Ignore);
	}

	// Token: 0x06000A27 RID: 2599 RVA: 0x0008EF8B File Offset: 0x0008D18B
	public void Free()
	{
		this.Reset();
	}

	// Token: 0x06000A28 RID: 2600 RVA: 0x0008EF94 File Offset: 0x0008D194
	public void Reset()
	{
		this.id = 0;
		this.gid = 0;
		this.entityId = 0;
		this.planetId = 0;
		this.pcId = 0;
		this.gene = 0;
		this.isStellar = false;
		this.isCollector = false;
		this.isVeinCollector = false;
		this.energy = 0L;
		this.energyPerTick = 0L;
		this.energyMax = 0L;
		this.warperCount = 0;
		this.warperMaxCount = 0;
		this.idleDroneCount = 0;
		this.workDroneCount = 0;
		this.workDroneDatas = null;
		this.workDroneOrders = null;
		this.idleShipCount = 0;
		this.workShipCount = 0;
		this.idleShipIndices = 0UL;
		this.workShipIndices = 0UL;
		this.workShipDatas = null;
		this.workShipOrders = null;
		this.shipRenderers = null;
		this.shipUIRenderers = null;
		this.storage = null;
		this.priorityLocks = null;
		this.slots = null;
		this.localPairProcess = 0;
		this.remotePairProcesses = null;
		this.nextShipIndex = 0;
		this.needs = null;
		this.localPairs = null;
		this.localPairCount = 0;
		this.remotePairs = null;
		this.remotePairOffsets = null;
		this.collectionIds = null;
		this.collectionPerTick = null;
		this.currentCollections = null;
		this.collectSpeed = 0;
		this.outSlotOffset = 0;
		this.droneDispatchStatus = null;
		this.droneStatusCursor = 0;
		this.droneTaskInterval = 0;
		this.tripRangeDrones = 0.0;
		this.tripRangeShips = 0.0;
		this.includeOrbitCollector = false;
		this.warpEnableDist = 0.0;
		this.warperNecessary = false;
		this.deliveryDrones = 0;
		this.deliveryShips = 0;
		this.pilerCount = 0;
		this.droneAutoReplenish = false;
		this.shipAutoReplenish = false;
		this.remoteGroupMask = 0L;
		this.routePriority = ERemoteRoutePriority.Ignore;
	}

	// Token: 0x06000A29 RID: 2601 RVA: 0x0008F150 File Offset: 0x0008D350
	public void UpdateOutputSlots(CargoTraffic traffic, SignData[] signPool, int maxPilerCount, bool active)
	{
		StationStore[] obj = this.storage;
		lock (obj)
		{
			int num = (this.pilerCount == 0) ? maxPilerCount : this.pilerCount;
			int num2 = this.slots.Length;
			int num3 = this.storage.Length;
			int num4 = -1;
			if (!this.isVeinCollector)
			{
				for (int i = 0; i < num2; i++)
				{
					ref SlotData ptr = ref this.slots[i];
					if (ptr.dir == IODir.Output)
					{
						if (ptr.counter > 0)
						{
							ptr.counter--;
						}
						else if (ptr.beltId != 0)
						{
							CargoPath cargoPath = traffic.GetCargoPath(traffic.beltPool[ptr.beltId].segPathId);
							if (cargoPath != null && cargoPath.buffer[9] == 0)
							{
								int num5 = ptr.storageIdx - 1;
								int num6 = 0;
								if (num5 >= 0)
								{
									if (num5 < num3)
									{
										num6 = this.storage[num5].itemId;
										if (num6 > 0 && this.storage[num5].count > 0)
										{
											int num7 = (this.storage[num5].count < num) ? this.storage[num5].count : num;
											int count = this.storage[num5].count;
											int inc = this.storage[num5].inc;
											int num8 = this.split_inc(ref count, ref inc, num7);
											if (cargoPath.TryInsertItemAtHeadAndFillBlank(num6, (byte)num7, (byte)num8))
											{
												this.storage[num5].count = count;
												this.storage[num5].inc = inc;
												ptr.counter = 1;
											}
										}
									}
									else
									{
										num6 = 1210;
										if (this.warperCount > 0 && cargoPath.TryInsertItemAtHeadAndFillBlank(num6, 1, 0))
										{
											this.warperCount--;
											ptr.counter = 1;
										}
									}
								}
								if (active)
								{
									int num9 = traffic.beltPool[ptr.beltId].entityId;
									signPool[num9].iconType = 1U;
									signPool[num9].iconId0 = (uint)num6;
								}
							}
						}
					}
					else if (ptr.dir != IODir.Input)
					{
						ptr.beltId = 0;
						ptr.counter = 0;
					}
				}
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num2; k++)
					{
						int num10 = (this.outSlotOffset + k) % num2;
						ref SlotData ptr2 = ref this.slots[num10];
						if (ptr2.dir == IODir.Output)
						{
							if (ptr2.beltId != 0)
							{
								CargoPath cargoPath2 = traffic.GetCargoPath(traffic.beltPool[ptr2.beltId].segPathId);
								if (cargoPath2 != null)
								{
									int num11 = 0;
									if (num11 >= 0 && num11 < num3)
									{
										int itemId = this.storage[num11].itemId;
										if (itemId > 0 && this.storage[num11].count > 0)
										{
											int num12 = this.storage[num11].inc / this.storage[num11].count;
											if (cargoPath2.TryUpdateItemAtHeadAndFillBlank(itemId, num, 1, (byte)num12))
											{
												StationStore[] array = this.storage;
												int num13 = num11;
												array[num13].count = array[num13].count - 1;
												StationStore[] array2 = this.storage;
												int num14 = num11;
												array2[num14].inc = array2[num14].inc - num12;
												num4 = (num10 + 1) % num2;
											}
										}
									}
									if (active)
									{
										int num15 = traffic.beltPool[ptr2.beltId].entityId;
										signPool[num15].iconType = 0U;
										signPool[num15].iconId0 = 0U;
									}
								}
							}
						}
						else if (ptr2.dir != IODir.Input)
						{
							ptr2.beltId = 0;
							ptr2.counter = 0;
						}
					}
				}
				if (num4 >= 0)
				{
					this.outSlotOffset = num4;
				}
			}
		}
	}

	// Token: 0x06000A2A RID: 2602 RVA: 0x0008F57C File Offset: 0x0008D77C
	public void UpdateInputSlots(CargoTraffic traffic, SignData[] signPool, bool active)
	{
		StationStore[] obj = this.storage;
		lock (obj)
		{
			int num = this.slots.Length;
			int num2 = this.storage.Length;
			BeltComponent[] beltPool = traffic.beltPool;
			int num3 = this.needs[0] + this.needs[1] + this.needs[2] + this.needs[3] + this.needs[4] + this.needs[5];
			for (int i = 0; i < num; i++)
			{
				ref SlotData ptr = ref this.slots[i];
				if (ptr.dir == IODir.Input)
				{
					if (ptr.counter > 0)
					{
						ptr.counter--;
					}
					else if (num3 != 0 && ptr.beltId != 0)
					{
						ref BeltComponent ptr2 = ref beltPool[ptr.beltId];
						CargoPath cargoPath = traffic.GetCargoPath(ptr2.segPathId);
						if (cargoPath != null)
						{
							int num4 = -1;
							byte stack;
							byte inc;
							int num5 = cargoPath.TryPickItemAtRear(this.needs, out num4, out stack, out inc);
							if (num4 >= 0)
							{
								this.InputItem(num5, num4, (int)stack, (int)inc);
								ptr.storageIdx = num4 + 1;
								ptr.counter = 1;
							}
							if (active)
							{
								if (this.isVeinCollector)
								{
									int num6 = ptr2.entityId;
									signPool[num6].iconType = 0U;
									signPool[num6].iconId0 = 0U;
								}
								else if (num5 > 0)
								{
									int num7 = ptr2.entityId;
									signPool[num7].iconType = 1U;
									signPool[num7].iconId0 = (uint)num5;
								}
							}
						}
					}
				}
				else if (ptr.dir != IODir.Output)
				{
					ptr.beltId = 0;
					ptr.counter = 0;
				}
			}
		}
	}

	// Token: 0x06000A2B RID: 2603 RVA: 0x0008F73C File Offset: 0x0008D93C
	public void SetLocalPairCapacity(int newCap)
	{
		if (this.localPairs == null)
		{
			this.localPairs = new SupplyDemandPair[newCap];
			return;
		}
		if (newCap <= this.localPairs.Length)
		{
			return;
		}
		SupplyDemandPair[] array = this.localPairs;
		this.localPairs = new SupplyDemandPair[newCap];
		Array.Copy(array, this.localPairs, array.Length);
	}

	// Token: 0x06000A2C RID: 2604 RVA: 0x0008F78C File Offset: 0x0008D98C
	public void AddLocalPair(int sId, int sIdx, int dId, int dIdx)
	{
		if (this.localPairs == null)
		{
			this.SetLocalPairCapacity(8);
		}
		if (this.localPairCount == this.localPairs.Length)
		{
			this.SetLocalPairCapacity(this.localPairs.Length * 2);
		}
		this.localPairs[this.localPairCount].supplyId = sId;
		this.localPairs[this.localPairCount].supplyIndex = sIdx;
		this.localPairs[this.localPairCount].demandId = dId;
		this.localPairs[this.localPairCount].demandIndex = dIdx;
		this.localPairCount++;
	}

	// Token: 0x06000A2D RID: 2605 RVA: 0x0008F833 File Offset: 0x0008DA33
	public void ClearLocalPairs()
	{
		this.localPairCount = 0;
		if (this.localPairs != null)
		{
			Array.Clear(this.localPairs, 0, this.localPairs.Length);
		}
	}

	// Token: 0x06000A2E RID: 2606 RVA: 0x0008F858 File Offset: 0x0008DA58
	public void SetRemotePairCapacity(int newCap)
	{
		if (this.remotePairs == null)
		{
			this.remotePairs = new SupplyDemandPair[newCap];
			this.remotePairOffsets = new int[7];
			return;
		}
		if (newCap <= this.remotePairs.Length)
		{
			return;
		}
		SupplyDemandPair[] array = this.remotePairs;
		this.remotePairs = new SupplyDemandPair[newCap];
		Array.Copy(array, this.remotePairs, array.Length);
	}

	// Token: 0x06000A2F RID: 2607 RVA: 0x0008F8B4 File Offset: 0x0008DAB4
	public void AddRemotePair(int sId, int sIdx, int dId, int dIdx)
	{
		if (this.remotePairs == null)
		{
			this.SetRemotePairCapacity(128);
		}
		if (this.remotePairOffsets[6] == this.remotePairs.Length)
		{
			this.SetRemotePairCapacity(this.remotePairs.Length * 2);
		}
		int num = this.remotePairOffsets[1];
		int num2 = this.remotePairOffsets[6] - num;
		if (num2 > 0)
		{
			Array.Copy(this.remotePairs, num, this.remotePairs, num + 1, num2);
		}
		this.remotePairs[num].supplyId = sId;
		this.remotePairs[num].supplyIndex = sIdx;
		this.remotePairs[num].demandId = dId;
		this.remotePairs[num].demandIndex = dIdx;
		for (int i = 1; i < 7; i++)
		{
			this.remotePairOffsets[i]++;
		}
	}

	// Token: 0x06000A30 RID: 2608 RVA: 0x0008F98C File Offset: 0x0008DB8C
	public void AddRouteRemotePair(int sId, int sIdx, int dId, int dIdx, int offsetIndex)
	{
		if (this.remotePairs == null)
		{
			this.SetRemotePairCapacity(128);
		}
		if (this.remotePairOffsets[6] == this.remotePairs.Length)
		{
			this.SetRemotePairCapacity(this.remotePairs.Length * 2);
		}
		int num = this.remotePairOffsets[offsetIndex];
		if (this.remotePairOffsets[6] - num > 0)
		{
			Array.Copy(this.remotePairs, num, this.remotePairs, num + 1, this.remotePairOffsets[6] - num);
		}
		this.remotePairs[num].supplyId = sId;
		this.remotePairs[num].supplyIndex = sIdx;
		this.remotePairs[num].demandId = dId;
		this.remotePairs[num].demandIndex = dIdx;
		for (int i = offsetIndex; i < 7; i++)
		{
			this.remotePairOffsets[i]++;
		}
	}

	// Token: 0x06000A31 RID: 2609 RVA: 0x0008FA6B File Offset: 0x0008DC6B
	public void ClearRemotePairs()
	{
		if (this.remotePairs != null)
		{
			Array.Clear(this.remotePairs, 0, this.remotePairs.Length);
		}
		if (this.remotePairOffsets != null)
		{
			Array.Clear(this.remotePairOffsets, 0, this.remotePairOffsets.Length);
		}
	}

	// Token: 0x06000A32 RID: 2610 RVA: 0x0008FAA8 File Offset: 0x0008DCA8
	public void RematchLocalPairs(StationComponent[] stationPool, int stationCursor, int keyStationId, int droneCarries)
	{
		int num = this.storage.Length;
		for (int i = 0; i < num; i++)
		{
			if (this.storage[i].localLogic == ELogisticStorage.Supply)
			{
				int itemId = this.storage[i].itemId;
				for (int j = this.id + 1; j < stationCursor; j++)
				{
					if (stationPool[j] != null && stationPool[j].id == j)
					{
						StationStore[] array = stationPool[j].storage;
						for (int k = 0; k < array.Length; k++)
						{
							if (itemId == array[k].itemId && array[k].localLogic == ELogisticStorage.Demand)
							{
								this.AddLocalPair(this.id, i, j, k);
								stationPool[j].AddLocalPair(this.id, i, j, k);
							}
						}
					}
				}
			}
			else if (this.storage[i].localLogic == ELogisticStorage.Demand)
			{
				int itemId2 = this.storage[i].itemId;
				for (int l = this.id + 1; l < stationCursor; l++)
				{
					if (stationPool[l] != null && stationPool[l].id == l)
					{
						StationStore[] array2 = stationPool[l].storage;
						for (int m = 0; m < array2.Length; m++)
						{
							if (itemId2 == array2[m].itemId && array2[m].localLogic == ELogisticStorage.Supply)
							{
								this.AddLocalPair(l, m, this.id, i);
								stationPool[l].AddLocalPair(l, m, this.id, i);
							}
						}
					}
				}
			}
		}
		if (keyStationId > 0)
		{
			for (int n = 0; n < this.workDroneCount; n++)
			{
				StationComponent stationComponent = stationPool[this.workDroneDatas[n].endId];
				StationStore[] array3 = (stationComponent == null) ? null : stationComponent.storage;
				if (keyStationId == this.id)
				{
					if (this.workDroneDatas[n].itemCount == 0 && this.workDroneDatas[n].direction > 0f && this.workDroneDatas[n].endId > 0)
					{
						int itemId3 = this.workDroneDatas[n].itemId;
						if (this.HasLocalDemand(itemId3, -10000000) == -1)
						{
							if (this.workDroneOrders[n].itemId > 0)
							{
								if (this.storage[this.workDroneOrders[n].thisIndex].itemId == this.workDroneOrders[n].itemId)
								{
									StationStore[] array4 = this.storage;
									int thisIndex = this.workDroneOrders[n].thisIndex;
									array4[thisIndex].localOrder = array4[thisIndex].localOrder - this.workDroneOrders[n].thisOrdered;
								}
								if (array3 != null && array3[this.workDroneOrders[n].otherIndex].itemId == this.workDroneOrders[n].itemId)
								{
									StationStore[] array5 = array3;
									int otherIndex = this.workDroneOrders[n].otherIndex;
									array5[otherIndex].localOrder = array5[otherIndex].localOrder - this.workDroneOrders[n].otherOrdered;
								}
								this.workDroneOrders[n].ClearThis();
								this.workDroneOrders[n].ClearOther();
							}
							this.workDroneDatas[n].itemId = 0;
							for (int num2 = 0; num2 < this.storage.Length; num2++)
							{
								if (this.storage[num2].localLogic == ELogisticStorage.Demand)
								{
									int num3 = stationComponent.HasLocalSupply(this.storage[num2].itemId, 1);
									if (num3 >= 0 && this.storage[num2].localDemandCount > 0)
									{
										this.workDroneDatas[n].itemId = this.storage[num2].itemId;
										this.workDroneDatas[n].direction = 1f;
										this.workDroneOrders[n].itemId = this.workDroneDatas[n].itemId;
										this.workDroneOrders[n].otherStationId = this.workDroneDatas[n].endId;
										this.workDroneOrders[n].thisIndex = num2;
										this.workDroneOrders[n].otherIndex = num3;
										this.workDroneOrders[n].thisOrdered = droneCarries;
										this.workDroneOrders[n].otherOrdered = -droneCarries;
										StationStore[] array6 = this.storage;
										int num4 = num2;
										array6[num4].localOrder = array6[num4].localOrder + droneCarries;
										StationStore[] array7 = array3;
										int num5 = num3;
										array7[num5].localOrder = array7[num5].localOrder - droneCarries;
										break;
									}
								}
							}
							if (this.workDroneDatas[n].itemId == 0)
							{
								this.workDroneDatas[n].endId = 0;
								this.workDroneDatas[n].direction = -1f;
							}
						}
					}
					if (this.workDroneDatas[n].itemCount != 0 && this.workDroneDatas[n].direction < 0f)
					{
						int itemId4 = this.workDroneDatas[n].itemId;
						if (this.HasLocalDemand(itemId4, -10000000) == -1 && this.workDroneOrders[n].itemId > 0)
						{
							if (this.storage[this.workDroneOrders[n].thisIndex].itemId == this.workDroneOrders[n].itemId)
							{
								StationStore[] array8 = this.storage;
								int thisIndex2 = this.workDroneOrders[n].thisIndex;
								array8[thisIndex2].localOrder = array8[thisIndex2].localOrder - this.workDroneOrders[n].thisOrdered;
							}
							if (array3 != null && array3[this.workDroneOrders[n].otherIndex].itemId == this.workDroneOrders[n].itemId)
							{
								StationStore[] array9 = array3;
								int otherIndex2 = this.workDroneOrders[n].otherIndex;
								array9[otherIndex2].localOrder = array9[otherIndex2].localOrder - this.workDroneOrders[n].otherOrdered;
							}
							this.workDroneOrders[n].ClearThis();
							this.workDroneOrders[n].ClearOther();
							this.workDroneOrders[n].itemId = itemId4;
						}
					}
				}
				if (keyStationId == this.workDroneDatas[n].endId)
				{
					if ((stationPool[this.workDroneDatas[n].endId] == null || stationPool[this.workDroneDatas[n].endId].id == 0) && this.workDroneDatas[n].direction > 0f)
					{
						if (this.workDroneOrders[n].itemId > 0)
						{
							if (this.storage[this.workDroneOrders[n].thisIndex].itemId == this.workDroneOrders[n].itemId)
							{
								StationStore[] array10 = this.storage;
								int thisIndex3 = this.workDroneOrders[n].thisIndex;
								array10[thisIndex3].localOrder = array10[thisIndex3].localOrder - this.workDroneOrders[n].thisOrdered;
							}
							this.workDroneOrders[n].ClearThis();
							this.workDroneOrders[n].ClearOther();
						}
						this.workDroneDatas[n].endId = 0;
						this.workDroneDatas[n].direction = -1f;
					}
					else if (this.workDroneDatas[n].itemCount > 0 && this.workDroneDatas[n].direction > 0f && this.workDroneDatas[n].endId > 0)
					{
						if (stationComponent.HasLocalDemand(this.workDroneDatas[n].itemId, 0) == -1)
						{
							if (this.workDroneOrders[n].itemId > 0)
							{
								if (this.storage[this.workDroneOrders[n].thisIndex].itemId == this.workDroneOrders[n].itemId)
								{
									StationStore[] array11 = this.storage;
									int thisIndex4 = this.workDroneOrders[n].thisIndex;
									array11[thisIndex4].localOrder = array11[thisIndex4].localOrder - this.workDroneOrders[n].thisOrdered;
								}
								if (array3[this.workDroneOrders[n].otherIndex].itemId == this.workDroneOrders[n].itemId)
								{
									StationStore[] array12 = array3;
									int otherIndex3 = this.workDroneOrders[n].otherIndex;
									array12[otherIndex3].localOrder = array12[otherIndex3].localOrder - this.workDroneOrders[n].otherOrdered;
								}
								this.workDroneOrders[n].ClearThis();
								this.workDroneOrders[n].ClearOther();
							}
							this.workDroneDatas[n].endId = 0;
							this.workDroneDatas[n].direction = -1f;
						}
					}
					else if (this.workDroneDatas[n].itemCount == 0 && this.workDroneDatas[n].direction > 0f && this.workDroneDatas[n].endId > 0)
					{
						int itemId5 = this.workDroneDatas[n].itemId;
						if (stationComponent.HasLocalSupply(itemId5, 0) == -1)
						{
							if (this.workDroneOrders[n].itemId > 0)
							{
								if (this.storage[this.workDroneOrders[n].thisIndex].itemId == this.workDroneOrders[n].itemId)
								{
									StationStore[] array13 = this.storage;
									int thisIndex5 = this.workDroneOrders[n].thisIndex;
									array13[thisIndex5].localOrder = array13[thisIndex5].localOrder - this.workDroneOrders[n].thisOrdered;
								}
								if (array3[this.workDroneOrders[n].otherIndex].itemId == this.workDroneOrders[n].itemId)
								{
									StationStore[] array14 = array3;
									int otherIndex4 = this.workDroneOrders[n].otherIndex;
									array14[otherIndex4].localOrder = array14[otherIndex4].localOrder - this.workDroneOrders[n].otherOrdered;
								}
								this.workDroneOrders[n].ClearThis();
								this.workDroneOrders[n].ClearOther();
							}
							this.workDroneDatas[n].itemId = 0;
							for (int num6 = 0; num6 < this.storage.Length; num6++)
							{
								if (this.storage[num6].localLogic == ELogisticStorage.Demand)
								{
									int num7 = stationComponent.HasLocalSupply(this.storage[num6].itemId, 1);
									if (num7 >= 0 && this.storage[num6].localDemandCount > 0)
									{
										this.workDroneDatas[n].itemId = this.storage[num6].itemId;
										this.workDroneDatas[n].direction = 1f;
										this.workDroneOrders[n].itemId = this.workDroneDatas[n].itemId;
										this.workDroneOrders[n].otherStationId = this.workDroneDatas[n].endId;
										this.workDroneOrders[n].thisIndex = num6;
										this.workDroneOrders[n].otherIndex = num7;
										this.workDroneOrders[n].thisOrdered = droneCarries;
										this.workDroneOrders[n].otherOrdered = -droneCarries;
										StationStore[] array15 = this.storage;
										int num8 = num6;
										array15[num8].localOrder = array15[num8].localOrder + droneCarries;
										StationStore[] array16 = array3;
										int num9 = num7;
										array16[num9].localOrder = array16[num9].localOrder - droneCarries;
										break;
									}
								}
							}
							if (this.workDroneDatas[n].itemId == 0)
							{
								this.workDroneDatas[n].endId = 0;
								this.workDroneDatas[n].direction = -1f;
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06000A33 RID: 2611 RVA: 0x00090790 File Offset: 0x0008E990
	public void RematchRemotePairs(GalacticTransport galacticTransport, int gStationCursor, int keyStationGId, int shipCarries)
	{
		int num = this.storage.Length;
		StationComponent[] stationPool = galacticTransport.stationPool;
		for (int i = 0; i < num; i++)
		{
			if (this.storage[i].remoteLogic == ELogisticStorage.Supply)
			{
				int itemId = this.storage[i].itemId;
				for (int j = this.gid + 1; j < gStationCursor; j++)
				{
					if (stationPool[j] != null && stationPool[j].gid == j && stationPool[j].planetId != this.planetId)
					{
						StationStore[] array = stationPool[j].storage;
						for (int k = 0; k < array.Length; k++)
						{
							if (itemId == array[k].itemId && array[k].remoteLogic == ELogisticStorage.Demand)
							{
								int num2 = this.planetId;
								int num3 = stationPool[j].planetId;
								int astroId = num2 / 100 * 100;
								int astroId2 = num3 / 100 * 100;
								if (!galacticTransport.IsAstro2AstroBanExist(num2, num3, itemId) && !galacticTransport.IsAstro2AstroBanExist(astroId, astroId2, itemId))
								{
									bool flag = stationPool[j].routePriority == ERemoteRoutePriority.Designated || this.routePriority == ERemoteRoutePriority.Designated;
									if (!flag)
									{
										this.AddRemotePair(this.gid, i, j, k);
										stationPool[j].AddRemotePair(this.gid, i, j, k);
									}
									bool flag2 = false;
									if (galacticTransport.IsStation2StationRouteExist(this.gid, stationPool[j].gid))
									{
										this.AddRouteRemotePair(this.gid, i, j, k, 2);
										stationPool[j].AddRouteRemotePair(this.gid, i, j, k, 2);
										flag2 = true;
									}
									if (galacticTransport.IsAstro2AstroRouteEnable(num2, num3, itemId))
									{
										this.AddRouteRemotePair(this.gid, i, j, k, 3);
										stationPool[j].AddRouteRemotePair(this.gid, i, j, k, 3);
										flag2 = true;
									}
									if (galacticTransport.IsAstro2AstroRouteEnable(astroId, astroId2, itemId))
									{
										this.AddRouteRemotePair(this.gid, i, j, k, 4);
										stationPool[j].AddRouteRemotePair(this.gid, i, j, k, 4);
										flag2 = true;
									}
									if ((this.remoteGroupMask & stationPool[j].remoteGroupMask) > 0L)
									{
										this.AddRouteRemotePair(this.gid, i, j, k, 5);
										stationPool[j].AddRouteRemotePair(this.gid, i, j, k, 5);
										flag2 = true;
									}
									if (!flag2 && !flag)
									{
										this.AddRouteRemotePair(this.gid, i, j, k, 6);
										stationPool[j].AddRouteRemotePair(this.gid, i, j, k, 6);
									}
								}
							}
						}
					}
				}
			}
			else if (this.storage[i].remoteLogic == ELogisticStorage.Demand)
			{
				int itemId2 = this.storage[i].itemId;
				for (int l = this.gid + 1; l < gStationCursor; l++)
				{
					if (stationPool[l] != null && stationPool[l].gid == l && stationPool[l].planetId != this.planetId)
					{
						StationStore[] array2 = stationPool[l].storage;
						for (int m = 0; m < array2.Length; m++)
						{
							if (itemId2 == array2[m].itemId && array2[m].remoteLogic == ELogisticStorage.Supply)
							{
								int num4 = this.planetId;
								int num5 = stationPool[l].planetId;
								int astroId3 = num4 / 100 * 100;
								int astroId4 = num5 / 100 * 100;
								if (!galacticTransport.IsAstro2AstroBanExist(num4, num5, itemId2) && !galacticTransport.IsAstro2AstroBanExist(astroId3, astroId4, itemId2))
								{
									bool flag3 = stationPool[l].routePriority == ERemoteRoutePriority.Designated || this.routePriority == ERemoteRoutePriority.Designated;
									if (!flag3)
									{
										this.AddRemotePair(l, m, this.gid, i);
										stationPool[l].AddRemotePair(l, m, this.gid, i);
									}
									bool flag4 = false;
									if (galacticTransport.IsStation2StationRouteExist(this.gid, stationPool[l].gid))
									{
										this.AddRouteRemotePair(l, m, this.gid, i, 2);
										stationPool[l].AddRouteRemotePair(l, m, this.gid, i, 2);
										flag4 = true;
									}
									if (galacticTransport.IsAstro2AstroRouteEnable(num4, num5, itemId2))
									{
										this.AddRouteRemotePair(l, m, this.gid, i, 3);
										stationPool[l].AddRouteRemotePair(l, m, this.gid, i, 3);
										flag4 = true;
									}
									if (galacticTransport.IsAstro2AstroRouteEnable(astroId3, astroId4, itemId2))
									{
										this.AddRouteRemotePair(l, m, this.gid, i, 4);
										stationPool[l].AddRouteRemotePair(l, m, this.gid, i, 4);
										flag4 = true;
									}
									if ((this.remoteGroupMask & stationPool[l].remoteGroupMask) > 0L)
									{
										this.AddRouteRemotePair(l, m, this.gid, i, 5);
										stationPool[l].AddRouteRemotePair(l, m, this.gid, i, 5);
										flag4 = true;
									}
									if (!flag4 && !flag3)
									{
										this.AddRouteRemotePair(l, m, this.gid, i, 6);
										stationPool[l].AddRouteRemotePair(l, m, this.gid, i, 6);
									}
								}
							}
						}
					}
				}
			}
		}
		if (keyStationGId > 0)
		{
			for (int n = 0; n < this.workShipCount; n++)
			{
				StationComponent stationComponent = stationPool[this.workShipDatas[n].otherGId];
				StationStore[] array3 = (stationComponent == null) ? null : stationComponent.storage;
				if (keyStationGId == this.gid)
				{
					if (this.workShipDatas[n].itemCount == 0 && this.workShipDatas[n].direction > 0 && this.workShipDatas[n].otherGId > 0)
					{
						int itemId3 = this.workShipDatas[n].itemId;
						if (this.HasRemoteDemand(itemId3, -10000000) == -1)
						{
							if (this.workShipOrders[n].itemId > 0)
							{
								if (this.storage[this.workShipOrders[n].thisIndex].itemId == this.workShipOrders[n].itemId)
								{
									StationStore[] array4 = this.storage;
									int thisIndex = this.workShipOrders[n].thisIndex;
									array4[thisIndex].remoteOrder = array4[thisIndex].remoteOrder - this.workShipOrders[n].thisOrdered;
								}
								if (array3 != null && array3[this.workShipOrders[n].otherIndex].itemId == this.workShipOrders[n].itemId)
								{
									StationStore[] array5 = array3;
									int otherIndex = this.workShipOrders[n].otherIndex;
									array5[otherIndex].remoteOrder = array5[otherIndex].remoteOrder - this.workShipOrders[n].otherOrdered;
								}
								this.workShipOrders[n].ClearThis();
								this.workShipOrders[n].ClearOther();
							}
							this.workShipDatas[n].itemId = 0;
							for (int num6 = 0; num6 < this.storage.Length; num6++)
							{
								if (this.storage[num6].remoteLogic == ELogisticStorage.Demand)
								{
									int num7 = stationComponent.HasRemoteSupply(this.storage[num6].itemId, 1);
									if (num7 >= 0 && this.storage[num6].remoteDemandCount > 0)
									{
										this.workShipDatas[n].itemId = this.storage[num6].itemId;
										this.workShipDatas[n].direction = 1;
										this.workShipOrders[n].itemId = this.workShipDatas[n].itemId;
										this.workShipOrders[n].otherStationGId = this.workShipDatas[n].otherGId;
										this.workShipOrders[n].thisIndex = num6;
										this.workShipOrders[n].otherIndex = num7;
										this.workShipOrders[n].thisOrdered = shipCarries;
										this.workShipOrders[n].otherOrdered = -shipCarries;
										StationStore[] array6 = this.storage;
										int num8 = num6;
										array6[num8].remoteOrder = array6[num8].remoteOrder + shipCarries;
										StationStore[] array7 = array3;
										int num9 = num7;
										array7[num9].remoteOrder = array7[num9].remoteOrder - shipCarries;
										break;
									}
								}
							}
							if (this.workShipDatas[n].itemId == 0)
							{
								this.workShipDatas[n].otherGId = 0;
								this.workShipDatas[n].direction = -1;
								if (this.workShipDatas[n].stage == -1)
								{
									this.workShipDatas[n].pPosTemp = this.shipDiskPos[this.workShipDatas[n].shipIndex] + this.shipDiskPos[this.workShipDatas[n].shipIndex].normalized * 25f;
								}
							}
						}
					}
					if (this.workShipDatas[n].itemCount != 0 && this.workShipDatas[n].direction < 0)
					{
						int itemId4 = this.workShipDatas[n].itemId;
						if (this.HasRemoteDemand(itemId4, -10000000) == -1 && this.workShipOrders[n].itemId > 0)
						{
							if (this.storage[this.workShipOrders[n].thisIndex].itemId == this.workShipOrders[n].itemId)
							{
								StationStore[] array8 = this.storage;
								int thisIndex2 = this.workShipOrders[n].thisIndex;
								array8[thisIndex2].remoteOrder = array8[thisIndex2].remoteOrder - this.workShipOrders[n].thisOrdered;
							}
							if (array3 != null && array3[this.workShipOrders[n].otherIndex].itemId == this.workShipOrders[n].itemId)
							{
								StationStore[] array9 = array3;
								int otherIndex2 = this.workShipOrders[n].otherIndex;
								array9[otherIndex2].remoteOrder = array9[otherIndex2].remoteOrder - this.workShipOrders[n].otherOrdered;
							}
							this.workShipOrders[n].ClearThis();
							this.workShipOrders[n].ClearOther();
							this.workShipOrders[n].itemId = itemId4;
						}
					}
				}
				if (keyStationGId == this.workShipDatas[n].otherGId)
				{
					if ((stationPool[this.workShipDatas[n].otherGId] == null || stationPool[this.workShipDatas[n].otherGId].gid == 0) && this.workShipDatas[n].direction > 0)
					{
						if (this.workShipOrders[n].itemId > 0)
						{
							if (this.storage[this.workShipOrders[n].thisIndex].itemId == this.workShipOrders[n].itemId)
							{
								StationStore[] array10 = this.storage;
								int thisIndex3 = this.workShipOrders[n].thisIndex;
								array10[thisIndex3].remoteOrder = array10[thisIndex3].remoteOrder - this.workShipOrders[n].thisOrdered;
							}
							this.workShipOrders[n].ClearThis();
							this.workShipOrders[n].ClearOther();
						}
						this.workShipDatas[n].otherGId = 0;
						this.workShipDatas[n].direction = -1;
					}
					else if ((stationPool[this.workShipDatas[n].otherGId] == null || stationPool[this.workShipDatas[n].otherGId].gid == 0) && this.workShipDatas[n].direction < 0)
					{
						this.workShipDatas[n].otherGId = 0;
						this.workShipDatas[n].direction = -1;
					}
					else if (this.workShipDatas[n].itemCount > 0 && this.workShipDatas[n].direction > 0 && this.workShipDatas[n].otherGId > 0)
					{
						if (stationComponent.HasRemoteDemand(this.workShipDatas[n].itemId, 0) == -1)
						{
							if (this.workShipOrders[n].itemId > 0)
							{
								if (this.storage[this.workShipOrders[n].thisIndex].itemId == this.workShipOrders[n].itemId)
								{
									StationStore[] array11 = this.storage;
									int thisIndex4 = this.workShipOrders[n].thisIndex;
									array11[thisIndex4].remoteOrder = array11[thisIndex4].remoteOrder - this.workShipOrders[n].thisOrdered;
								}
								if (array3[this.workShipOrders[n].otherIndex].itemId == this.workShipOrders[n].itemId)
								{
									StationStore[] array12 = array3;
									int otherIndex3 = this.workShipOrders[n].otherIndex;
									array12[otherIndex3].remoteOrder = array12[otherIndex3].remoteOrder - this.workShipOrders[n].otherOrdered;
								}
								this.workShipOrders[n].ClearThis();
								this.workShipOrders[n].ClearOther();
							}
							this.workShipDatas[n].otherGId = 0;
							this.workShipDatas[n].direction = -1;
						}
					}
					else if (this.workShipDatas[n].itemCount == 0 && this.workShipDatas[n].direction > 0 && this.workShipDatas[n].otherGId > 0)
					{
						int itemId5 = this.workShipDatas[n].itemId;
						if (stationComponent.HasRemoteSupply(itemId5, 0) == -1)
						{
							if (this.workShipOrders[n].itemId > 0)
							{
								if (this.storage[this.workShipOrders[n].thisIndex].itemId == this.workShipOrders[n].itemId)
								{
									StationStore[] array13 = this.storage;
									int thisIndex5 = this.workShipOrders[n].thisIndex;
									array13[thisIndex5].remoteOrder = array13[thisIndex5].remoteOrder - this.workShipOrders[n].thisOrdered;
								}
								if (array3[this.workShipOrders[n].otherIndex].itemId == this.workShipOrders[n].itemId)
								{
									StationStore[] array14 = array3;
									int otherIndex4 = this.workShipOrders[n].otherIndex;
									array14[otherIndex4].remoteOrder = array14[otherIndex4].remoteOrder - this.workShipOrders[n].otherOrdered;
								}
								this.workShipOrders[n].ClearThis();
								this.workShipOrders[n].ClearOther();
							}
							this.workShipDatas[n].itemId = 0;
							for (int num10 = 0; num10 < this.storage.Length; num10++)
							{
								if (this.storage[num10].remoteLogic == ELogisticStorage.Demand)
								{
									int num11 = stationComponent.HasRemoteSupply(this.storage[num10].itemId, 1);
									if (num11 >= 0 && this.storage[num10].remoteDemandCount > 0)
									{
										this.workShipDatas[n].itemId = this.storage[num10].itemId;
										this.workShipDatas[n].direction = 1;
										this.workShipOrders[n].itemId = this.workShipDatas[n].itemId;
										this.workShipOrders[n].otherStationGId = this.workShipDatas[n].otherGId;
										this.workShipOrders[n].thisIndex = num10;
										this.workShipOrders[n].otherIndex = num11;
										this.workShipOrders[n].thisOrdered = shipCarries;
										this.workShipOrders[n].otherOrdered = -shipCarries;
										StationStore[] array15 = this.storage;
										int num12 = num10;
										array15[num12].remoteOrder = array15[num12].remoteOrder + shipCarries;
										StationStore[] array16 = array3;
										int num13 = num11;
										array16[num13].remoteOrder = array16[num13].remoteOrder - shipCarries;
										break;
									}
								}
							}
							if (this.workShipDatas[n].itemId == 0)
							{
								this.workShipDatas[n].otherGId = 0;
								this.workShipDatas[n].direction = -1;
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06000A34 RID: 2612 RVA: 0x000918AC File Offset: 0x0008FAAC
	public int HasLocalSupply(int itemId, int countAtLeast = 0)
	{
		int num = this.storage.Length;
		if (0 < num && this.storage[0].itemId == itemId && this.storage[0].localLogic == ELogisticStorage.Supply && this.storage[0].count >= countAtLeast)
		{
			return 0;
		}
		if (1 < num && this.storage[1].itemId == itemId && this.storage[1].localLogic == ELogisticStorage.Supply && this.storage[1].count >= countAtLeast)
		{
			return 1;
		}
		if (2 < num && this.storage[2].itemId == itemId && this.storage[2].localLogic == ELogisticStorage.Supply && this.storage[2].count >= countAtLeast)
		{
			return 2;
		}
		if (3 < num && this.storage[3].itemId == itemId && this.storage[3].localLogic == ELogisticStorage.Supply && this.storage[3].count >= countAtLeast)
		{
			return 3;
		}
		if (4 < num && this.storage[4].itemId == itemId && this.storage[4].localLogic == ELogisticStorage.Supply && this.storage[4].count >= countAtLeast)
		{
			return 4;
		}
		if (5 < num && this.storage[5].itemId == itemId && this.storage[5].localLogic == ELogisticStorage.Supply && this.storage[5].count >= countAtLeast)
		{
			return 5;
		}
		return -1;
	}

	// Token: 0x06000A35 RID: 2613 RVA: 0x00091A50 File Offset: 0x0008FC50
	public int HasLocalDemand(int itemId, int countAtLeast = -10000000)
	{
		int num = this.storage.Length;
		if (0 < num && this.storage[0].itemId == itemId && this.storage[0].localLogic == ELogisticStorage.Demand && this.storage[0].max - this.storage[0].count >= countAtLeast)
		{
			return 0;
		}
		if (1 < num && this.storage[1].itemId == itemId && this.storage[1].localLogic == ELogisticStorage.Demand && this.storage[1].max - this.storage[1].count >= countAtLeast)
		{
			return 1;
		}
		if (2 < num && this.storage[2].itemId == itemId && this.storage[2].localLogic == ELogisticStorage.Demand && this.storage[2].max - this.storage[2].count >= countAtLeast)
		{
			return 2;
		}
		if (3 < num && this.storage[3].itemId == itemId && this.storage[3].localLogic == ELogisticStorage.Demand && this.storage[3].max - this.storage[3].count >= countAtLeast)
		{
			return 3;
		}
		if (4 < num && this.storage[4].itemId == itemId && this.storage[4].localLogic == ELogisticStorage.Demand && this.storage[4].max - this.storage[4].count >= countAtLeast)
		{
			return 4;
		}
		if (5 < num && this.storage[5].itemId == itemId && this.storage[5].localLogic == ELogisticStorage.Demand && this.storage[5].max - this.storage[5].count >= countAtLeast)
		{
			return 5;
		}
		return -1;
	}

	// Token: 0x06000A36 RID: 2614 RVA: 0x00091C60 File Offset: 0x0008FE60
	public int HasRemoteSupply(int itemId, int countAtLeast = 0)
	{
		int num = this.storage.Length;
		if (0 < num && this.storage[0].itemId == itemId && this.storage[0].remoteLogic == ELogisticStorage.Supply && this.storage[0].count >= countAtLeast)
		{
			return 0;
		}
		if (1 < num && this.storage[1].itemId == itemId && this.storage[1].remoteLogic == ELogisticStorage.Supply && this.storage[1].count >= countAtLeast)
		{
			return 1;
		}
		if (2 < num && this.storage[2].itemId == itemId && this.storage[2].remoteLogic == ELogisticStorage.Supply && this.storage[2].count >= countAtLeast)
		{
			return 2;
		}
		if (3 < num && this.storage[3].itemId == itemId && this.storage[3].remoteLogic == ELogisticStorage.Supply && this.storage[3].count >= countAtLeast)
		{
			return 3;
		}
		if (4 < num && this.storage[4].itemId == itemId && this.storage[4].remoteLogic == ELogisticStorage.Supply && this.storage[4].count >= countAtLeast)
		{
			return 4;
		}
		if (5 < num && this.storage[5].itemId == itemId && this.storage[5].remoteLogic == ELogisticStorage.Supply && this.storage[5].count >= countAtLeast)
		{
			return 5;
		}
		return -1;
	}

	// Token: 0x06000A37 RID: 2615 RVA: 0x00091E04 File Offset: 0x00090004
	public int HasRemoteDemand(int itemId, int countAtLeast = -10000000)
	{
		int num = this.storage.Length;
		if (0 < num && this.storage[0].itemId == itemId && this.storage[0].remoteLogic == ELogisticStorage.Demand && this.storage[0].max - this.storage[0].count >= countAtLeast)
		{
			return 0;
		}
		if (1 < num && this.storage[1].itemId == itemId && this.storage[1].remoteLogic == ELogisticStorage.Demand && this.storage[1].max - this.storage[1].count >= countAtLeast)
		{
			return 1;
		}
		if (2 < num && this.storage[2].itemId == itemId && this.storage[2].remoteLogic == ELogisticStorage.Demand && this.storage[2].max - this.storage[2].count >= countAtLeast)
		{
			return 2;
		}
		if (3 < num && this.storage[3].itemId == itemId && this.storage[3].remoteLogic == ELogisticStorage.Demand && this.storage[3].max - this.storage[3].count >= countAtLeast)
		{
			return 3;
		}
		if (4 < num && this.storage[4].itemId == itemId && this.storage[4].remoteLogic == ELogisticStorage.Demand && this.storage[4].max - this.storage[4].count >= countAtLeast)
		{
			return 4;
		}
		if (5 < num && this.storage[5].itemId == itemId && this.storage[5].remoteLogic == ELogisticStorage.Demand && this.storage[5].max - this.storage[5].count >= countAtLeast)
		{
			return 5;
		}
		return -1;
	}

	// Token: 0x06000A38 RID: 2616 RVA: 0x00092014 File Offset: 0x00090214
	public void SetPCState(PowerConsumerComponent[] pcPool)
	{
		if (this.energy == this.energyMax)
		{
			pcPool[this.pcId].SetRequiredEnergy(false);
		}
		else
		{
			double num = 1.05 - (double)this.energy / (double)this.energyMax;
			if (num > 1.0)
			{
				num = 1.0;
			}
			pcPool[this.pcId].SetRequiredEnergy(num);
		}
		this.energyPerTick = pcPool[this.pcId].requiredEnergy;
	}

	// Token: 0x06000A39 RID: 2617 RVA: 0x0009209C File Offset: 0x0009029C
	public void IdleShipGetToWork(int index)
	{
		this.idleShipIndices &= ~(1UL << index);
		this.workShipIndices |= 1UL << index;
	}

	// Token: 0x06000A3A RID: 2618 RVA: 0x000920C7 File Offset: 0x000902C7
	public void WorkShipBackToIdle(int index)
	{
		this.idleShipIndices |= 1UL << index;
		this.workShipIndices &= ~(1UL << index);
	}

	// Token: 0x06000A3B RID: 2619 RVA: 0x000920F2 File Offset: 0x000902F2
	public void AddIdleShip(int index)
	{
		this.idleShipIndices |= 1UL << index;
	}

	// Token: 0x06000A3C RID: 2620 RVA: 0x00092108 File Offset: 0x00090308
	public void RemoveIdleShip(int index)
	{
		this.idleShipIndices &= ~(1UL << index);
	}

	// Token: 0x06000A3D RID: 2621 RVA: 0x0009211F File Offset: 0x0009031F
	public bool HasWorkShipIndex(int index)
	{
		return (this.workShipIndices & 1UL << index) > 0UL;
	}

	// Token: 0x06000A3E RID: 2622 RVA: 0x00092133 File Offset: 0x00090333
	public bool HasIdleShipIndex(int index)
	{
		return (this.idleShipIndices & 1UL << index) > 0UL;
	}

	// Token: 0x06000A3F RID: 2623 RVA: 0x00092147 File Offset: 0x00090347
	public bool HasShipIndex(int index)
	{
		return (this.idleShipIndices & 1UL << index) != 0UL || (this.workShipIndices & 1UL << index) > 0UL;
	}

	// Token: 0x06000A40 RID: 2624 RVA: 0x00092170 File Offset: 0x00090370
	public int QueryIdleShip(int qIdx)
	{
		for (int i = qIdx; i < qIdx + this.workShipDatas.Length; i++)
		{
			int num = i % this.workShipDatas.Length;
			if ((this.idleShipIndices & 1UL << num) != 0UL)
			{
				return num;
			}
		}
		return -1;
	}

	// Token: 0x06000A41 RID: 2625 RVA: 0x000921B0 File Offset: 0x000903B0
	public void ShipRenderersOnTick(AstroData[] astroPoses, ref VectorLF3 rPos, ref Quaternion rRot)
	{
		int num = 0;
		int num2 = 0;
		int num3 = this.workShipDatas.Length;
		for (int i = 0; i < num3; i++)
		{
			if ((this.idleShipIndices & 1UL << i) != 0UL)
			{
				num++;
			}
		}
		int num4 = this.idleShipCount - num;
		if (num4 > 0)
		{
			for (int j = 0; j < num3; j++)
			{
				if (!this.HasShipIndex(j))
				{
					this.AddIdleShip(j);
					num4--;
					if (num4 == 0)
					{
						break;
					}
				}
			}
		}
		else if (num4 < 0)
		{
			for (int k = num3 - 1; k >= 0; k--)
			{
				if ((this.idleShipIndices & 1UL << k) != 0UL)
				{
					this.RemoveIdleShip(k);
					num4++;
					if (num4 == 0)
					{
						break;
					}
				}
			}
		}
		Assert.Zero(num4);
		ref VectorLF3 upos = ref astroPoses[this.planetId].uPos;
		ref Quaternion ptr = ref astroPoses[this.planetId].uRot;
		VectorLF3 vectorLF = new VectorLF3(0f, 0f, 0f);
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Quaternion quaternion = new Quaternion(0f, 0f, 0f, 1f);
		for (int l = 0; l < num3; l++)
		{
			ref ShipRenderingData ptr2 = ref this.shipRenderers[l];
			ref ShipUIRenderingData ptr3 = ref this.shipUIRenderers[l];
			if (this.HasIdleShipIndex(l))
			{
				ptr2.gid = this.gid;
				StationComponent.lpos2upos_ref(ref upos, ref ptr, ref this.shipDiskPos[l], ref vectorLF);
				Maths.QMultiply_ref(ref ptr, ref this.shipDiskRot[l], out quaternion);
				ptr2.SetPose(ref vectorLF, ref quaternion, ref rPos, ref rRot, ref vector, 0);
				num++;
				num2 = l + 1;
				ptr2.anim.x = 0f;
				ptr2.anim.y = 0f;
				ptr2.anim.z = 0f;
				ptr2.anim.w = 0f;
				ptr3.gid = 0;
			}
			else if (this.HasWorkShipIndex(l))
			{
				ptr2.gid = this.gid;
				num2 = l + 1;
				ptr3.gid = this.gid;
			}
			else
			{
				ptr2.gid = 0;
				ptr2.anim = Vector3.zero;
				ptr3.gid = 0;
			}
		}
		this.renderShipCount = num2;
	}

	// Token: 0x06000A42 RID: 2626 RVA: 0x00092408 File Offset: 0x00090608
	public void InternalTickLocal(PlanetFactory factory, int timeGene, float power, float droneSpeed, int droneCarries, StationComponent[] stationPool)
	{
		this.energy += (long)((int)((float)this.energyPerTick * power));
		this.energy -= 1000L;
		if (this.energy > this.energyMax)
		{
			this.energy = this.energyMax;
		}
		else if (this.energy < 0L)
		{
			this.energy = 0L;
		}
		float num = Mathf.Sqrt(droneSpeed / 8f);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int itemId = 0;
		int num7 = 0;
		int num8 = 0;
		int itemId2 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		int num13 = 0;
		int num14 = 0;
		int num15 = 0;
		int num16 = this.workDroneCount + this.idleDroneCount;
		if (timeGene % this.droneTaskInterval == this.id % this.droneTaskInterval)
		{
			int num17 = this.droneDispatchStatus.Length;
			ref byte ptr = ref this.droneDispatchStatus[this.droneStatusCursor];
			ptr = 0;
			this.droneStatusCursor++;
			if (this.droneStatusCursor == num17)
			{
				this.droneStatusCursor = 0;
			}
			this._tmp_iter_local++;
			if (this.localPairCount > 0 && this.idleDroneCount > 0 && this.energy > 800000L)
			{
				this.localPairProcess %= this.localPairCount;
				int num18 = this.localPairProcess;
				int num19 = (droneCarries - 1) * this.deliveryDrones / 100;
				SupplyDemandPair ptr2;
				StationComponent stationComponent;
				float x;
				float y;
				float z;
				float x2;
				float y2;
				float z2;
				double num23;
				double num24;
				StationComponent stationComponent2;
				float x3;
				float y3;
				float z3;
				float x4;
				float y4;
				float z4;
				double num27;
				double num28;
				for (;;)
				{
					int num20 = num19;
					ptr2 = ref this.localPairs[this.localPairProcess];
					if (ptr2.supplyId == this.id)
					{
						StationStore[] obj = this.storage;
						lock (obj)
						{
							num2 = this.storage[ptr2.supplyIndex].max;
							num3 = this.storage[ptr2.supplyIndex].count;
							num4 = this.storage[ptr2.supplyIndex].inc;
							num5 = this.storage[ptr2.supplyIndex].localSupplyCount;
							num6 = this.storage[ptr2.supplyIndex].totalSupplyCount;
						}
						if (num2 <= num20)
						{
							num20 = num2 - 1;
							if (num20 < 0)
							{
								num20 = 0;
							}
						}
						if (num3 > num20 && num5 > num20 && num6 > num20)
						{
							stationComponent = stationPool[ptr2.demandId];
							if (stationComponent != null)
							{
								x = this.droneDock.x;
								y = this.droneDock.y;
								z = this.droneDock.z;
								x2 = stationComponent.droneDock.x;
								y2 = stationComponent.droneDock.y;
								z2 = stationComponent.droneDock.z;
								double num21 = Math.Sqrt((double)(x * x + y * y + z * z));
								double num22 = Math.Sqrt((double)(x2 * x2 + y2 * y2 + z2 * z2));
								num23 = (num21 + num22) * 0.5;
								num24 = (double)(x * x2 + y * y2 + z * z2) / (num21 * num22);
								if (num24 < -1.0)
								{
									num24 = -1.0;
								}
								else if (num24 > 1.0)
								{
									num24 = 1.0;
								}
								if (num24 >= this.tripRangeDrones - 1E-06)
								{
									obj = stationComponent.storage;
									lock (obj)
									{
										num14 = stationComponent.storage[ptr2.demandIndex].localDemandCount;
										num15 = stationComponent.storage[ptr2.demandIndex].totalDemandCount;
									}
								}
								if (num24 >= this.tripRangeDrones - 1E-06 && num14 > 0 && num15 > 0)
								{
									break;
								}
							}
						}
					}
					else
					{
						StationStore[] obj = this.storage;
						lock (obj)
						{
							num7 = this.storage[ptr2.demandIndex].localDemandCount;
							num8 = this.storage[ptr2.demandIndex].totalDemandCount;
						}
						if (num7 > 0 && num8 > 0)
						{
							stationComponent2 = stationPool[ptr2.supplyId];
							if (stationComponent2 != null)
							{
								obj = stationComponent2.storage;
								lock (obj)
								{
									num9 = stationComponent2.storage[ptr2.supplyIndex].max;
									num10 = stationComponent2.storage[ptr2.supplyIndex].count;
									num11 = stationComponent2.storage[ptr2.supplyIndex].inc;
									num12 = stationComponent2.storage[ptr2.supplyIndex].localSupplyCount;
									num13 = stationComponent2.storage[ptr2.supplyIndex].totalSupplyCount;
								}
								if (num9 <= num20)
								{
									num20 = num9 - 1;
									if (num20 < 0)
									{
										num20 = 0;
									}
								}
								if (num10 > 0 && num10 <= num20 && stationComponent2.isVeinCollector && factory.factorySystem.minerPool[stationComponent2.minerId].veinCount == 0)
								{
									num20 = 0;
								}
								if (num10 > num20 && num12 > num20 && num13 > num20)
								{
									x3 = this.droneDock.x;
									y3 = this.droneDock.y;
									z3 = this.droneDock.z;
									x4 = stationComponent2.droneDock.x;
									y4 = stationComponent2.droneDock.y;
									z4 = stationComponent2.droneDock.z;
									double num25 = Math.Sqrt((double)(x3 * x3 + y3 * y3 + z3 * z3));
									double num26 = Math.Sqrt((double)(x4 * x4 + y4 * y4 + z4 * z4));
									num27 = (num25 + num26) * 0.5;
									num28 = (double)(x3 * x4 + y3 * y4 + z3 * z4) / (num25 * num26);
									if (num28 < -1.0)
									{
										num28 = -1.0;
									}
									else if (num28 > 1.0)
									{
										num28 = 1.0;
									}
									if (num28 >= this.tripRangeDrones - 1E-06)
									{
										goto Block_44;
									}
								}
							}
						}
					}
					this.localPairProcess++;
					if (this.localPairProcess == this.localPairCount)
					{
						this.localPairProcess = 0;
					}
					if (num18 == this.localPairProcess)
					{
						goto IL_11A5;
					}
				}
				double num29 = Math.Acos(num24);
				double num30 = num23 * num29;
				long num31 = (long)(num30 * 20000.0 * 2.0 + 800000.0);
				if (this.energy >= num31)
				{
					StationStore[] obj = this.storage;
					lock (obj)
					{
						num3 = this.storage[ptr2.supplyIndex].count;
						num4 = this.storage[ptr2.supplyIndex].inc;
						itemId = this.storage[ptr2.supplyIndex].itemId;
					}
					int num32 = (droneCarries < num3) ? droneCarries : num3;
					int num33 = num3;
					int num34 = num4;
					int num35 = this.split_inc(ref num33, ref num34, num32);
					this.workDroneDatas[this.workDroneCount].begin = new Vector3(x, y, z);
					this.workDroneDatas[this.workDroneCount].end = new Vector3(x2, y2, z2);
					this.workDroneDatas[this.workDroneCount].endId = stationComponent.id;
					this.workDroneDatas[this.workDroneCount].direction = 1f;
					this.workDroneDatas[this.workDroneCount].maxt = (float)num30;
					this.workDroneDatas[this.workDroneCount].t = -1.5f;
					this.workDroneDatas[this.workDroneCount].itemId = (this.workDroneOrders[this.workDroneCount].itemId = itemId);
					this.workDroneDatas[this.workDroneCount].itemCount = num32;
					this.workDroneDatas[this.workDroneCount].inc = num35;
					this.workDroneDatas[this.workDroneCount].gene = this._tmp_iter_local;
					this.workDroneOrders[this.workDroneCount].otherStationId = stationComponent.id;
					this.workDroneOrders[this.workDroneCount].thisIndex = ptr2.supplyIndex;
					this.workDroneOrders[this.workDroneCount].otherIndex = ptr2.demandIndex;
					this.workDroneOrders[this.workDroneCount].thisOrdered = 0;
					this.workDroneOrders[this.workDroneCount].otherOrdered = num32;
					obj = stationComponent.storage;
					lock (obj)
					{
						StationStore[] array = stationComponent.storage;
						int demandIndex = ptr2.demandIndex;
						array[demandIndex].localOrder = array[demandIndex].localOrder + num32;
					}
					this.workDroneCount++;
					this.idleDroneCount--;
					ptr += 1;
					obj = this.storage;
					lock (obj)
					{
						StationStore[] array2 = this.storage;
						int supplyIndex = ptr2.supplyIndex;
						array2[supplyIndex].count = array2[supplyIndex].count - num32;
						StationStore[] array3 = this.storage;
						int supplyIndex2 = ptr2.supplyIndex;
						array3[supplyIndex2].inc = array3[supplyIndex2].inc - num35;
						factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, this.storage[ptr2.supplyIndex].itemId, num32);
					}
					this.energy -= num31;
					goto IL_11A5;
				}
				goto IL_11A5;
				Block_44:
				double num36 = Math.Acos(num28);
				double num37 = num27 * num36;
				long num38 = (long)(num37 * 20000.0 * 2.0 + 800000.0);
				bool flag2 = false;
				this.localPairProcess %= this.localPairCount;
				int num39 = this.localPairProcess + 1;
				int num40 = this.localPairProcess;
				num39 %= this.localPairCount;
				SupplyDemandPair supplyDemandPair;
				for (;;)
				{
					supplyDemandPair = this.localPairs[num39];
					if (supplyDemandPair.supplyId == this.id && supplyDemandPair.demandId == stationComponent2.id)
					{
						StationStore[] obj = this.storage;
						lock (obj)
						{
							num3 = this.storage[supplyDemandPair.supplyIndex].count;
							num4 = this.storage[supplyDemandPair.supplyIndex].inc;
							num5 = this.storage[supplyDemandPair.supplyIndex].localSupplyCount;
							num6 = this.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
							itemId = this.storage[supplyDemandPair.supplyIndex].itemId;
						}
					}
					if (supplyDemandPair.supplyId == this.id && supplyDemandPair.demandId == stationComponent2.id)
					{
						StationStore[] obj = stationComponent2.storage;
						lock (obj)
						{
							num14 = stationComponent2.storage[supplyDemandPair.demandIndex].localDemandCount;
							num15 = stationComponent2.storage[supplyDemandPair.demandIndex].totalDemandCount;
						}
					}
					int num20;
					if (supplyDemandPair.supplyId == this.id && supplyDemandPair.demandId == stationComponent2.id && num3 > num20 && num5 > num20 && num6 > num20 && num14 > 0 && num15 > 0)
					{
						break;
					}
					num39++;
					num39 %= this.localPairCount;
					if (num40 == num39)
					{
						goto IL_ED4;
					}
				}
				if (this.energy >= num38)
				{
					int num41 = (droneCarries < num3) ? droneCarries : num3;
					int num42 = num3;
					int num43 = num4;
					int num44 = this.split_inc(ref num42, ref num43, num41);
					this.workDroneDatas[this.workDroneCount].begin = new Vector3(x3, y3, z3);
					this.workDroneDatas[this.workDroneCount].end = new Vector3(x4, y4, z4);
					this.workDroneDatas[this.workDroneCount].endId = stationComponent2.id;
					this.workDroneDatas[this.workDroneCount].direction = 1f;
					this.workDroneDatas[this.workDroneCount].maxt = (float)num37;
					this.workDroneDatas[this.workDroneCount].t = -1.5f;
					this.workDroneDatas[this.workDroneCount].itemId = (this.workDroneOrders[this.workDroneCount].itemId = itemId);
					this.workDroneDatas[this.workDroneCount].itemCount = num41;
					this.workDroneDatas[this.workDroneCount].inc = num44;
					this.workDroneDatas[this.workDroneCount].gene = this._tmp_iter_local;
					this.workDroneOrders[this.workDroneCount].otherStationId = stationComponent2.id;
					this.workDroneOrders[this.workDroneCount].thisIndex = supplyDemandPair.supplyIndex;
					this.workDroneOrders[this.workDroneCount].otherIndex = supplyDemandPair.demandIndex;
					this.workDroneOrders[this.workDroneCount].thisOrdered = 0;
					this.workDroneOrders[this.workDroneCount].otherOrdered = num41;
					StationStore[] obj = stationComponent2.storage;
					lock (obj)
					{
						StationStore[] array4 = stationComponent2.storage;
						int demandIndex2 = supplyDemandPair.demandIndex;
						array4[demandIndex2].localOrder = array4[demandIndex2].localOrder + num41;
					}
					this.workDroneCount++;
					this.idleDroneCount--;
					ptr += 1;
					obj = this.storage;
					lock (obj)
					{
						StationStore[] array5 = this.storage;
						int supplyIndex3 = supplyDemandPair.supplyIndex;
						array5[supplyIndex3].count = array5[supplyIndex3].count - num41;
						StationStore[] array6 = this.storage;
						int supplyIndex4 = supplyDemandPair.supplyIndex;
						array6[supplyIndex4].inc = array6[supplyIndex4].inc - num44;
						factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, this.storage[supplyDemandPair.supplyIndex].itemId, num41);
					}
					this.energy -= num38;
					flag2 = true;
				}
				IL_ED4:
				if (!flag2 && this.energy >= num38)
				{
					StationStore[] obj = this.storage;
					lock (obj)
					{
						itemId2 = this.storage[ptr2.demandIndex].itemId;
					}
					this.workDroneDatas[this.workDroneCount].begin = new Vector3(x3, y3, z3);
					this.workDroneDatas[this.workDroneCount].end = new Vector3(x4, y4, z4);
					this.workDroneDatas[this.workDroneCount].endId = stationComponent2.id;
					this.workDroneDatas[this.workDroneCount].direction = 1f;
					this.workDroneDatas[this.workDroneCount].maxt = (float)num37;
					this.workDroneDatas[this.workDroneCount].t = -1.5f;
					this.workDroneDatas[this.workDroneCount].itemId = (this.workDroneOrders[this.workDroneCount].itemId = itemId2);
					this.workDroneDatas[this.workDroneCount].itemCount = 0;
					this.workDroneDatas[this.workDroneCount].gene = this._tmp_iter_local;
					this.workDroneOrders[this.workDroneCount].otherStationId = stationComponent2.id;
					this.workDroneOrders[this.workDroneCount].thisIndex = ptr2.demandIndex;
					this.workDroneOrders[this.workDroneCount].otherIndex = ptr2.supplyIndex;
					this.workDroneOrders[this.workDroneCount].thisOrdered = droneCarries;
					this.workDroneOrders[this.workDroneCount].otherOrdered = -droneCarries;
					obj = this.storage;
					lock (obj)
					{
						StationStore[] array7 = this.storage;
						int demandIndex3 = ptr2.demandIndex;
						array7[demandIndex3].localOrder = array7[demandIndex3].localOrder + droneCarries;
					}
					obj = stationComponent2.storage;
					lock (obj)
					{
						StationStore[] array8 = stationComponent2.storage;
						int supplyIndex5 = ptr2.supplyIndex;
						array8[supplyIndex5].localOrder = array8[supplyIndex5].localOrder - droneCarries;
					}
					this.workDroneCount++;
					this.idleDroneCount--;
					ptr += 1;
					this.energy -= num38;
				}
				IL_11A5:
				this.localPairProcess++;
				if (this.localPairProcess == this.localPairCount)
				{
					this.localPairProcess = 0;
				}
			}
			if (this.droneStatusCursor == 0)
			{
				int num45 = 0;
				for (int i = 0; i < num17; i++)
				{
					num45 += (int)this.droneDispatchStatus[i];
				}
				float num46 = (float)num45 / (float)num17;
				if (num46 < 0.75f)
				{
					float num47 = num46 * 0.25f + 0.75f;
					this.droneTaskInterval = Mathf.CeilToInt((float)this.droneTaskInterval / num47 - 0.01f);
				}
				else if (num46 > 0.9f)
				{
					this.droneTaskInterval = Mathf.FloorToInt((float)this.droneTaskInterval * 0.8f + 0.1f);
				}
				if (this.droneTaskInterval < 1)
				{
					this.droneTaskInterval = 1;
				}
				if (num16 >= 75)
				{
					if (this.droneTaskInterval > 10)
					{
						this.droneTaskInterval = 10;
					}
				}
				else if (this.droneTaskInterval > 20)
				{
					this.droneTaskInterval = 20;
				}
			}
		}
		float num48 = 0.016666668f * droneSpeed;
		float num49 = 0.016666668f * num;
		for (int j = 0; j < this.workDroneCount; j++)
		{
			if (this.workDroneDatas[j].t > 0f && this.workDroneDatas[j].t < this.workDroneDatas[j].maxt)
			{
				DroneData[] array9 = this.workDroneDatas;
				int num50 = j;
				array9[num50].t = array9[num50].t + num48 * this.workDroneDatas[j].direction;
				if (this.workDroneDatas[j].t <= 0f)
				{
					this.workDroneDatas[j].t = -0.0001f;
				}
				else if (this.workDroneDatas[j].t >= this.workDroneDatas[j].maxt)
				{
					this.workDroneDatas[j].t = this.workDroneDatas[j].maxt + 0.0001f;
				}
			}
			else
			{
				DroneData[] array10 = this.workDroneDatas;
				int num51 = j;
				array10[num51].t = array10[num51].t + num49 * this.workDroneDatas[j].direction;
				if (this.workDroneDatas[j].t >= this.workDroneDatas[j].maxt + 1.5f)
				{
					this.workDroneDatas[j].direction = -1f;
					this.workDroneDatas[j].t = this.workDroneDatas[j].maxt + 1.5f;
					StationComponent stationComponent3 = stationPool[this.workDroneDatas[j].endId];
					StationStore[] array11 = stationComponent3.storage;
					if (this.workDroneDatas[j].itemCount > 0)
					{
						stationComponent3.AddItem(this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount, this.workDroneDatas[j].inc);
						TrafficStatistics traffic = factory.gameData.statistics.traffic;
						traffic.RegisterPlanetInternalStat(factory.planetId, this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount);
						this.workDroneDatas[j].itemCount = 0;
						this.workDroneDatas[j].inc = 0;
						factory.NotifyDroneDelivery(factory, this, stationComponent3, this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount);
						if (this.workDroneOrders[j].otherStationId > 0)
						{
							StationStore[] obj = array11;
							lock (obj)
							{
								if (array11[this.workDroneOrders[j].otherIndex].itemId == this.workDroneOrders[j].itemId)
								{
									StationStore[] array12 = array11;
									int otherIndex = this.workDroneOrders[j].otherIndex;
									array12[otherIndex].localOrder = array12[otherIndex].localOrder - this.workDroneOrders[j].otherOrdered;
								}
							}
							this.workDroneOrders[j].ClearOther();
						}
						if (this.localPairCount > 0)
						{
							this.localPairProcess %= this.localPairCount;
							int num52 = this.localPairProcess;
							int num53 = this.localPairProcess;
							do
							{
								SupplyDemandPair supplyDemandPair2 = this.localPairs[num53];
								if (supplyDemandPair2.demandId == this.id && supplyDemandPair2.supplyId == stationComponent3.id)
								{
									StationStore[] obj = this.storage;
									lock (obj)
									{
										num7 = this.storage[supplyDemandPair2.demandIndex].localDemandCount;
										num8 = this.storage[supplyDemandPair2.demandIndex].totalDemandCount;
										itemId2 = this.storage[supplyDemandPair2.demandIndex].itemId;
									}
								}
								if (supplyDemandPair2.demandId == this.id && supplyDemandPair2.supplyId == stationComponent3.id)
								{
									StationStore[] obj = array11;
									lock (obj)
									{
										num10 = array11[supplyDemandPair2.supplyIndex].count;
										num11 = array11[supplyDemandPair2.supplyIndex].inc;
										num12 = array11[supplyDemandPair2.supplyIndex].localSupplyCount;
										num13 = array11[supplyDemandPair2.supplyIndex].totalSupplyCount;
									}
								}
								if (supplyDemandPair2.demandId == this.id && supplyDemandPair2.supplyId == stationComponent3.id && num7 > 0 && num8 > 0 && num10 > 0 && num12 > 0 && num13 > 0)
								{
									int num54 = (droneCarries < num10) ? droneCarries : num10;
									int num55 = num10;
									int num56 = num11;
									int num57 = this.split_inc(ref num55, ref num56, num54);
									this.workDroneDatas[j].itemId = (this.workDroneOrders[j].itemId = itemId2);
									this.workDroneDatas[j].itemCount = num54;
									this.workDroneDatas[j].inc = num57;
									traffic.RegisterPlanetInternalStat(factory.planetId, this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount);
									StationStore[] obj = array11;
									lock (obj)
									{
										StationStore[] array13 = array11;
										int supplyIndex6 = supplyDemandPair2.supplyIndex;
										array13[supplyIndex6].count = array13[supplyIndex6].count - num54;
										StationStore[] array14 = array11;
										int supplyIndex7 = supplyDemandPair2.supplyIndex;
										array14[supplyIndex7].inc = array14[supplyIndex7].inc - num57;
									}
									this.workDroneOrders[j].otherStationId = stationComponent3.id;
									this.workDroneOrders[j].thisIndex = supplyDemandPair2.demandIndex;
									this.workDroneOrders[j].otherIndex = supplyDemandPair2.supplyIndex;
									this.workDroneOrders[j].thisOrdered = num54;
									this.workDroneOrders[j].otherOrdered = 0;
									obj = this.storage;
									lock (obj)
									{
										StationStore[] array15 = this.storage;
										int demandIndex4 = supplyDemandPair2.demandIndex;
										array15[demandIndex4].localOrder = array15[demandIndex4].localOrder + num54;
										break;
									}
								}
								num53++;
								num53 %= this.localPairCount;
							}
							while (num52 != num53);
						}
					}
					else
					{
						int itemId3 = this.workDroneDatas[j].itemId;
						int num58 = droneCarries;
						int inc;
						stationComponent3.TakeItem(ref itemId3, ref num58, out inc);
						factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, itemId3, num58);
						this.workDroneDatas[j].itemCount = num58;
						this.workDroneDatas[j].inc = inc;
						StationStore[] obj;
						if (this.workDroneOrders[j].otherStationId > 0)
						{
							obj = array11;
							lock (obj)
							{
								if (array11[this.workDroneOrders[j].otherIndex].itemId == this.workDroneOrders[j].itemId)
								{
									StationStore[] array16 = array11;
									int otherIndex2 = this.workDroneOrders[j].otherIndex;
									array16[otherIndex2].localOrder = array16[otherIndex2].localOrder - this.workDroneOrders[j].otherOrdered;
								}
							}
							this.workDroneOrders[j].ClearOther();
						}
						obj = this.storage;
						lock (obj)
						{
							if (this.storage[this.workDroneOrders[j].thisIndex].itemId == this.workDroneOrders[j].itemId && this.workDroneOrders[j].thisOrdered != num58)
							{
								int num59 = num58 - this.workDroneOrders[j].thisOrdered;
								StationStore[] array17 = this.storage;
								int thisIndex = this.workDroneOrders[j].thisIndex;
								array17[thisIndex].localOrder = array17[thisIndex].localOrder + num59;
								LocalLogisticOrder[] array18 = this.workDroneOrders;
								int num60 = j;
								array18[num60].thisOrdered = array18[num60].thisOrdered + num59;
							}
						}
					}
				}
				if (this.workDroneDatas[j].t < -1.5f)
				{
					this.AddItem(this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount, this.workDroneDatas[j].inc);
					factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount);
					StationComponent srcStation = stationPool[this.workDroneDatas[j].endId];
					factory.NotifyDroneDelivery(factory, srcStation, this, this.workDroneDatas[j].itemId, this.workDroneDatas[j].itemCount);
					if (this.workDroneOrders[j].itemId > 0)
					{
						StationStore[] obj = this.storage;
						lock (obj)
						{
							if (this.storage[this.workDroneOrders[j].thisIndex].itemId == this.workDroneOrders[j].itemId)
							{
								StationStore[] array19 = this.storage;
								int thisIndex2 = this.workDroneOrders[j].thisIndex;
								array19[thisIndex2].localOrder = array19[thisIndex2].localOrder - this.workDroneOrders[j].thisOrdered;
							}
						}
						this.workDroneOrders[j].ClearThis();
					}
					Array.Copy(this.workDroneDatas, j + 1, this.workDroneDatas, j, this.workDroneDatas.Length - j - 1);
					Array.Copy(this.workDroneOrders, j + 1, this.workDroneOrders, j, this.workDroneOrders.Length - j - 1);
					this.workDroneCount--;
					this.idleDroneCount++;
					Array.Clear(this.workDroneDatas, this.workDroneCount, this.workDroneDatas.Length - this.workDroneCount);
					Array.Clear(this.workDroneOrders, this.workDroneCount, this.workDroneOrders.Length - this.workDroneCount);
					j--;
				}
			}
		}
	}

	// Token: 0x06000A43 RID: 2627 RVA: 0x000943D8 File Offset: 0x000925D8
	public void InternalTickRemote(PlanetFactory factory, int timeGene, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroData[] astroPoses, ref VectorLF3 relativePos, ref Quaternion relativeRot, bool starmap, int[] consumeRegister)
	{
		bool flag = shipWarpSpeed > shipSailSpeed + 1f;
		this.warperFree = DSPGame.IsMenuDemo;
		if (this.warperCount < this.warperMaxCount)
		{
			StationStore[] obj = this.storage;
			lock (obj)
			{
				for (int i = 0; i < this.storage.Length; i++)
				{
					if (this.storage[i].itemId == 1210 && this.storage[i].count > 0)
					{
						this.warperCount++;
						int num = this.storage[i].inc / this.storage[i].count;
						StationStore[] array = this.storage;
						int num2 = i;
						array[num2].count = array[num2].count - 1;
						StationStore[] array2 = this.storage;
						int num3 = i;
						array2[num3].inc = array2[num3].inc - num;
						break;
					}
				}
			}
		}
		int num4 = 0;
		int num5 = 0;
		int itemId = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		float num10 = shipSailSpeed / 600f;
		float num11 = Mathf.Pow(num10, 0.4f);
		float num12 = num11;
		if (num12 > 1f)
		{
			num12 = Mathf.Log(num12) + 1f;
		}
		if (num10 > 500f)
		{
			num10 = 500f;
		}
		ref AstroData ptr = ref astroPoses[this.planetId];
		float num13 = shipSailSpeed * 0.03f;
		float num14 = shipSailSpeed * 0.12f * num12;
		float num15 = shipSailSpeed * 0.4f * num10;
		float num16 = num11 * 0.006f + 1E-05f;
		Vector3 vector = new Vector3(0f, 0f, 0f);
		VectorLF3 vectorLF = new VectorLF3(0f, 0f, 0f);
		double num17 = 0.0;
		Quaternion uRot = new Quaternion(0f, 0f, 0f, 1f);
		int j = 0;
		while (j < this.workShipCount)
		{
			ref ShipData ptr2 = ref this.workShipDatas[j];
			ref ShipRenderingData ptr3 = ref this.shipRenderers[ptr2.shipIndex];
			bool flag3 = false;
			uRot.x = (uRot.y = (uRot.z = 0f));
			uRot.w = 1f;
			ref AstroData ptr4 = ref astroPoses[ptr2.planetB];
			vectorLF.x = ptr.uPos.x - ptr4.uPos.x;
			vectorLF.y = ptr.uPos.y - ptr4.uPos.y;
			vectorLF.z = ptr.uPos.z - ptr4.uPos.z;
			num17 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
			if (ptr2.otherGId <= 0)
			{
				ptr2.direction = -1;
				if (ptr2.stage > 0)
				{
					ptr2.stage = 0;
				}
			}
			if (ptr2.stage < -1)
			{
				if (ptr2.direction > 0)
				{
					ptr2.t += 0.03335f;
					if (ptr2.t > 1f)
					{
						ptr2.t = 0f;
						ptr2.stage = -1;
					}
				}
				else
				{
					ptr2.t -= 0.03335f;
					if (ptr2.t < 0f)
					{
						ptr2.t = 0f;
						this.AddItem(ptr2.itemId, ptr2.itemCount, ptr2.inc);
						TrafficStatistics traffic = factory.gameData.statistics.traffic;
						traffic.RegisterPlanetInputStat(this.planetId, ptr2.itemId, ptr2.itemCount);
						int num18 = ptr2.planetB / 100;
						int num19 = this.planetId / 100;
						if (num18 != num19)
						{
							traffic.RegisterStarInputStat(num19, ptr2.itemId, ptr2.itemCount);
						}
						else
						{
							traffic.RegisterStarInternalStat(num19, ptr2.itemId, ptr2.itemCount);
						}
						factory.NotifyShipDelivery(ptr2.planetB, gStationPool[ptr2.otherGId], ptr2.planetA, this, ptr2.itemId, ptr2.itemCount);
						if (this.workShipOrders[j].itemId > 0)
						{
							StationStore[] obj = this.storage;
							lock (obj)
							{
								if (this.storage[this.workShipOrders[j].thisIndex].itemId == this.workShipOrders[j].itemId)
								{
									StationStore[] array3 = this.storage;
									int thisIndex = this.workShipOrders[j].thisIndex;
									array3[thisIndex].remoteOrder = array3[thisIndex].remoteOrder - this.workShipOrders[j].thisOrdered;
								}
							}
							this.workShipOrders[j].ClearThis();
						}
						int shipIndex = ptr2.shipIndex;
						Array.Copy(this.workShipDatas, j + 1, this.workShipDatas, j, this.workShipDatas.Length - j - 1);
						Array.Copy(this.workShipOrders, j + 1, this.workShipOrders, j, this.workShipOrders.Length - j - 1);
						this.workShipCount--;
						this.idleShipCount++;
						this.WorkShipBackToIdle(shipIndex);
						Array.Clear(this.workShipDatas, this.workShipCount, this.workShipDatas.Length - this.workShipCount);
						Array.Clear(this.workShipOrders, this.workShipCount, this.workShipOrders.Length - this.workShipCount);
						j--;
						goto IL_3068;
					}
				}
				ptr2.uPos = ptr.uPos + Maths.QRotateLF(ptr.uRot, this.shipDiskPos[ptr2.shipIndex]);
				ptr2.uVel.x = 0f;
				ptr2.uVel.y = 0f;
				ptr2.uVel.z = 0f;
				ptr2.uSpeed = 0f;
				ptr2.uRot = ptr.uRot * this.shipDiskRot[ptr2.shipIndex];
				ptr2.uAngularVel.x = 0f;
				ptr2.uAngularVel.y = 0f;
				ptr2.uAngularVel.z = 0f;
				ptr2.uAngularSpeed = 0f;
				ptr2.pPosTemp = new VectorLF3(0f, 0f, 0f);
				ptr2.pRotTemp = new Quaternion(0f, 0f, 0f, 1f);
				ptr3.anim.z = 0f;
				goto IL_2F08;
			}
			if (ptr2.stage == -1)
			{
				if (ptr2.direction > 0)
				{
					ptr2.t += num16;
					float num20 = ptr2.t;
					if (ptr2.t > 1f)
					{
						ptr2.t = 1f;
						num20 = 1f;
						ptr2.stage = 0;
					}
					ptr3.anim.z = num20;
					num20 = (3f - num20 - num20) * num20 * num20;
					ptr2.uPos = ptr.uPos + Maths.QRotateLF(ptr.uRot, this.shipDiskPos[ptr2.shipIndex] + this.shipDiskPos[ptr2.shipIndex].normalized * (25f * num20));
					ptr2.uRot = ptr.uRot * this.shipDiskRot[ptr2.shipIndex];
				}
				else
				{
					ptr2.t -= num16 * 0.6666667f;
					float num20 = ptr2.t;
					if (ptr2.t < 0f)
					{
						ptr2.t = 1f;
						num20 = 0f;
						ptr2.stage = -2;
					}
					ptr3.anim.z = num20;
					num20 = (3f - num20 - num20) * num20 * num20;
					VectorLF3 lhs = ptr.uPos + Maths.QRotateLF(ptr.uRot, this.shipDiskPos[ptr2.shipIndex]);
					VectorLF3 lhs2 = ptr.uPos + Maths.QRotateLF(ptr.uRot, ptr2.pPosTemp);
					ptr2.uPos = lhs * (double)(1f - num20) + lhs2 * (double)num20;
					ptr2.uRot = ptr.uRot * Quaternion.Slerp(this.shipDiskRot[ptr2.shipIndex], ptr2.pRotTemp, num20 * 2f - 1f);
				}
				ptr2.uVel.x = 0f;
				ptr2.uVel.y = 0f;
				ptr2.uVel.z = 0f;
				ptr2.uSpeed = 0f;
				ptr2.uAngularVel.x = 0f;
				ptr2.uAngularVel.y = 0f;
				ptr2.uAngularVel.z = 0f;
				ptr2.uAngularSpeed = 0f;
				goto IL_2F08;
			}
			if (ptr2.stage == 0)
			{
				VectorLF3 vectorLF2;
				if (ptr2.direction > 0)
				{
					vector = gStationPool[ptr2.otherGId].shipDockPos;
					float num21 = (float)Math.Sqrt((double)(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z));
					num21 = 1f + 25f / num21;
					vector.x *= num21;
					vector.y *= num21;
					vector.z *= num21;
					StationComponent.lpos2upos_out(ref ptr4.uPos, ref ptr4.uRot, ref vector, out vectorLF2);
				}
				else
				{
					vector = this.shipDiskPos[ptr2.shipIndex];
					float num22 = (float)Math.Sqrt((double)(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z));
					num22 = 1f + 25f / num22;
					vector.x *= num22;
					vector.y *= num22;
					vector.z *= num22;
					StationComponent.lpos2upos_out(ref ptr.uPos, ref ptr.uRot, ref vector, out vectorLF2);
				}
				VectorLF3 vectorLF3;
				vectorLF3.x = vectorLF2.x - ptr2.uPos.x;
				vectorLF3.y = vectorLF2.y - ptr2.uPos.y;
				vectorLF3.z = vectorLF2.z - ptr2.uPos.z;
				double num23 = Math.Sqrt(vectorLF3.x * vectorLF3.x + vectorLF3.y * vectorLF3.y + vectorLF3.z * vectorLF3.z);
				VectorLF3 vectorLF4;
				if (ptr2.direction > 0)
				{
					vectorLF4.x = ptr.uPos.x - ptr2.uPos.x;
					vectorLF4.y = ptr.uPos.y - ptr2.uPos.y;
					vectorLF4.z = ptr.uPos.z - ptr2.uPos.z;
				}
				else
				{
					vectorLF4.x = ptr4.uPos.x - ptr2.uPos.x;
					vectorLF4.y = ptr4.uPos.y - ptr2.uPos.y;
					vectorLF4.z = ptr4.uPos.z - ptr2.uPos.z;
				}
				double num24 = vectorLF4.x * vectorLF4.x + vectorLF4.y * vectorLF4.y + vectorLF4.z * vectorLF4.z;
				bool flag4 = num24 <= (double)(ptr.uRadius * ptr.uRadius) * 2.25;
				bool flag5 = false;
				if (num23 < (double)(6f * num12))
				{
					ptr2.t = 1f;
					ptr2.stage = ptr2.direction;
					flag5 = true;
				}
				int num25 = 1;
				float num26 = 0f;
				if (flag)
				{
					double num27 = num17 * 2.0;
					double num28 = ((double)shipWarpSpeed < num27) ? ((double)shipWarpSpeed) : num27;
					double num29 = this.warpEnableDist * 0.5;
					if (ptr2.warpState <= 0f)
					{
						ptr2.warpState = 0f;
						if (num24 > 25000000.0 && num23 > num29 && ptr2.uSpeed >= shipSailSpeed && (ptr2.warperCnt > 0 || this.warperFree))
						{
							ptr2.warperCnt--;
							ptr2.warpState += 0.016666668f;
						}
					}
					else
					{
						num26 = (float)(num28 * ((Math.Pow(1001.0, (double)ptr2.warpState) - 1.0) / 1000.0));
						double num30 = (double)num26 * 0.0449 + 5000.0 + (double)shipSailSpeed * 0.25;
						double num31 = num23 - num30;
						if (num31 < 0.0)
						{
							num31 = 0.0;
						}
						if (num23 < num30)
						{
							ptr2.warpState -= 0.06666667f;
						}
						else
						{
							ptr2.warpState += 0.016666668f;
						}
						if (ptr2.warpState < 0f)
						{
							ptr2.warpState = 0f;
						}
						else if (ptr2.warpState > 1f)
						{
							ptr2.warpState = 1f;
						}
						if (ptr2.warpState > 0f)
						{
							num26 = (float)(num28 * ((Math.Pow(1001.0, (double)ptr2.warpState) - 1.0) / 1000.0));
							if ((double)num26 * 0.016666666666666666 > num31)
							{
								num26 = (float)(num31 / 0.016666666666666666 * 1.01);
							}
						}
					}
				}
				if (num24 > 1000000000000.0 && num24 > (double)(shipSailSpeed * shipSailSpeed * 4900f))
				{
					if (num23 > 1000000.0 + (double)num26 * 0.55)
					{
						num25 = 30;
					}
					else if (num23 > 1000000.0 + (double)num26 * 0.2)
					{
						num25 = 10;
					}
				}
				VectorLF3 vectorLF5 = new VectorLF3(0f, 0f, 0f);
				if (num25 == 1 || (this.gene + j + timeGene) % num25 == 0)
				{
					double num32 = num23 / ((double)ptr2.uSpeed + 0.1) * 0.382;
					float num33;
					if (ptr2.warpState > 0f)
					{
						num33 = (ptr2.uSpeed = shipSailSpeed + num26);
						if (num33 > shipSailSpeed)
						{
							num33 = shipSailSpeed;
						}
					}
					else
					{
						float num34 = (float)((double)ptr2.uSpeed * num32 * (double)num12) + 6f * num11 + 0.15f * num10;
						if (num34 > shipSailSpeed)
						{
							num34 = shipSailSpeed;
						}
						float num35 = 0.016666668f * (flag4 ? num13 : num14);
						if (ptr2.uSpeed < num34 - num35)
						{
							ptr2.uSpeed += num35;
						}
						else if (ptr2.uSpeed > num34 + num15)
						{
							ptr2.uSpeed -= num15;
						}
						else
						{
							ptr2.uSpeed = num34;
						}
						num33 = ptr2.uSpeed;
					}
					int num36 = -1;
					double rhs = 0.0;
					double num37 = 40000000000.0;
					if (num25 == 1)
					{
						int num38 = ptr2.planetA / 100 * 100;
						int num39 = ptr2.planetB / 100 * 100;
						float num40 = 5000f + num33;
						VectorLF3 vectorLF6;
						for (int k = num38; k < num38 + 10; k++)
						{
							float uRadius = astroPoses[k].uRadius;
							if (uRadius >= 1f)
							{
								ref VectorLF3 ptr5 = ref astroPoses[k].uPos;
								float num41 = uRadius + num40;
								vectorLF6.x = ptr2.uPos.x - ptr5.x;
								vectorLF6.y = ptr2.uPos.y - ptr5.y;
								vectorLF6.z = ptr2.uPos.z - ptr5.z;
								double num42 = vectorLF6.x * vectorLF6.x + vectorLF6.y * vectorLF6.y + vectorLF6.z * vectorLF6.z;
								double num43 = -((double)ptr2.uVel.x * vectorLF6.x + (double)ptr2.uVel.y * vectorLF6.y + (double)ptr2.uVel.z * vectorLF6.z);
								if ((num43 > 0.0 || num42 < (double)(uRadius * uRadius * 7f)) && num42 < num37 && num42 < (double)(num41 * num41))
								{
									rhs = ((num43 < 0.0) ? 0.0 : num43);
									num36 = k;
									num37 = num42;
								}
							}
						}
						if (num39 != num38)
						{
							for (int l = num39; l < num39 + 10; l++)
							{
								float uRadius2 = astroPoses[l].uRadius;
								if (uRadius2 >= 1f)
								{
									ref VectorLF3 ptr6 = ref astroPoses[l].uPos;
									float num44 = uRadius2 + num40;
									vectorLF6.x = ptr2.uPos.x - ptr6.x;
									vectorLF6.y = ptr2.uPos.y - ptr6.y;
									vectorLF6.z = ptr2.uPos.z - ptr6.z;
									double num45 = vectorLF6.x * vectorLF6.x + vectorLF6.y * vectorLF6.y + vectorLF6.z * vectorLF6.z;
									double num46 = -((double)ptr2.uVel.x * vectorLF6.x + (double)ptr2.uVel.y * vectorLF6.y + (double)ptr2.uVel.z * vectorLF6.z);
									if ((num46 > 0.0 || num45 < (double)(uRadius2 * uRadius2 * 7f)) && num45 < num37 && num45 < (double)(num44 * num44))
									{
										rhs = ((num46 < 0.0) ? 0.0 : num46);
										num36 = l;
										num37 = num45;
									}
								}
							}
						}
					}
					Vector3 vector2 = new VectorLF3(0f, 0f, 0f);
					Vector3 vector3 = new VectorLF3(0f, 0f, 0f);
					float num47 = 0f;
					if (num36 > 0)
					{
						ref AstroData ptr7 = ref astroPoses[num36];
						float num48 = ptr7.uRadius;
						if (num36 % 100 == 0)
						{
							num48 *= 2.5f;
						}
						double num49 = Math.Max(1.0, ((ptr7.uPosNext - ptr7.uPos).magnitude - 0.5) * 0.6);
						double num50 = 1.0 + 1600.0 / (double)num48;
						double num51 = 1.0 + 250.0 / (double)num48;
						num50 *= num49 * num49;
						double num52 = (double)((num36 == ptr2.planetA || num36 == ptr2.planetB) ? 1.25f : 1.5f);
						double num53 = Math.Sqrt(num37);
						double num54 = (double)num48 / num53 * 1.6 - 0.1;
						if (num54 > 1.0)
						{
							num54 = 1.0;
						}
						else if (num54 < 0.0)
						{
							num54 = 0.0;
						}
						double num55 = num53 - (double)num48 * 0.82;
						if (num55 < 1.0)
						{
							num55 = 1.0;
						}
						double num56 = (double)(num33 - 6f) / (num55 * (double)num12) * 0.6 - 0.01;
						if (num56 > 1.5)
						{
							num56 = 1.5;
						}
						else if (num56 < 0.0)
						{
							num56 = 0.0;
						}
						VectorLF3 vectorLF7 = ptr2.uPos + ptr2.uVel * rhs - ptr7.uPos;
						double num57 = vectorLF7.magnitude / (double)num48;
						if (num57 < num52)
						{
							double num58 = (num57 - 1.0) / (num52 - 1.0);
							if (num58 < 0.0)
							{
								num58 = 0.0;
							}
							num58 = 1.0 - num58 * num58;
							vector3 = vectorLF7.normalized * (num56 * num56 * num58 * 2.0 * (double)(1f - ptr2.warpState));
						}
						VectorLF3 vectorLF8;
						vectorLF8.x = ptr2.uPos.x - ptr7.uPos.x;
						vectorLF8.y = ptr2.uPos.y - ptr7.uPos.y;
						vectorLF8.z = ptr2.uPos.z - ptr7.uPos.z;
						double num59 = num54 / num53;
						vector2.x = (float)(vectorLF8.x * num59);
						vector2.y = (float)(vectorLF8.y * num59);
						vector2.z = (float)(vectorLF8.z * num59);
						num47 = (float)num54;
						double num60 = num53 / (double)num48;
						num60 *= num60;
						num60 = (num50 - num60) / (num50 - num51);
						if (num60 > 1.0)
						{
							num60 = 1.0;
						}
						else if (num60 < 0.0)
						{
							num60 = 0.0;
						}
						if (num60 > 0.0)
						{
							VectorLF3 vectorLF9;
							Maths.QInvRotateLF_refout(ref ptr7.uRot, ref vectorLF8, out vectorLF9);
							VectorLF3 vectorLF10;
							StationComponent.lpos2upos_out(ref ptr7.uPosNext, ref ptr7.uRotNext, ref vectorLF9, out vectorLF10);
							num60 = (3.0 - num60 - num60) * num60 * num60;
							vectorLF5.x = (vectorLF10.x - ptr2.uPos.x) * num60;
							vectorLF5.y = (vectorLF10.y - ptr2.uPos.y) * num60;
							vectorLF5.z = (vectorLF10.z - ptr2.uPos.z) * num60;
						}
					}
					Vector3 vector4;
					ptr2.uRot.ForwardUp(out ptr2.uVel, out vector4);
					float num61 = 1f - num47;
					Vector3 vector5;
					vector5.x = vector4.x * num61 + vector2.x * num47;
					vector5.y = vector4.y * num61 + vector2.y * num47;
					vector5.z = vector4.z * num61 + vector2.z * num47;
					float num62 = vector5.x * ptr2.uVel.x + vector5.y * ptr2.uVel.y + vector5.z * ptr2.uVel.z;
					vector5.x -= num62 * ptr2.uVel.x;
					vector5.y -= num62 * ptr2.uVel.y;
					vector5.z -= num62 * ptr2.uVel.z;
					float num63 = (float)Math.Sqrt((double)(vector5.x * vector5.x + vector5.y * vector5.y + vector5.z * vector5.z));
					if (num63 > 0f)
					{
						vector5.x /= num63;
						vector5.y /= num63;
						vector5.z /= num63;
					}
					Vector3 vector6;
					vector6.x = vector3.x;
					vector6.y = vector3.y;
					vector6.z = vector3.z;
					if (num23 > 0.0)
					{
						vector6.x += (float)(vectorLF3.x / num23);
						vector6.y += (float)(vectorLF3.y / num23);
						vector6.z += (float)(vectorLF3.z / num23);
					}
					Vector3 vector7;
					StationComponent.Vector3Cross_ref(ref ptr2.uVel, ref vector6, out vector7);
					float num64 = ptr2.uVel.x * vector6.x + ptr2.uVel.y * vector6.y + ptr2.uVel.z * vector6.z;
					Vector3 vector8;
					StationComponent.Vector3Cross_ref(ref vector4, ref vector5, out vector8);
					float num65 = vector4.x * vector5.x + vector4.y * vector5.y + vector4.z * vector5.z;
					if (num64 < 0f)
					{
						float num66 = (float)Math.Sqrt((double)(vector7.x * vector7.x + vector7.y * vector7.y + vector7.z * vector7.z));
						if (num66 > 0f)
						{
							vector7.x /= num66;
							vector7.y /= num66;
							vector7.z /= num66;
						}
					}
					if (num65 < 0f)
					{
						float num67 = (float)Math.Sqrt((double)(vector8.x * vector8.x + vector8.y * vector8.y + vector8.z * vector8.z));
						if (num67 > 0f)
						{
							vector8.x /= num67;
							vector8.y /= num67;
							vector8.z /= num67;
						}
					}
					float d = (num32 < 3.0) ? ((3.25f - (float)num32) * 4f) : (num33 / shipSailSpeed * (flag4 ? 0.2f : 1f));
					vector7 = vector7 * d + vector8 * 2f;
					Vector3 vector9;
					vector9.x = vector7.x - ptr2.uAngularVel.x;
					vector9.y = vector7.y - ptr2.uAngularVel.y;
					vector9.z = vector7.z - ptr2.uAngularVel.z;
					float num68 = (vector9.x * vector9.x + vector9.y * vector9.y + vector9.z * vector9.z < 0.1f) ? 1f : (0.05f * num12);
					if (num68 > 1f)
					{
						num68 = 1f;
					}
					ptr2.uAngularVel.x = ptr2.uAngularVel.x + vector9.x * num68;
					ptr2.uAngularVel.y = ptr2.uAngularVel.y + vector9.y * num68;
					ptr2.uAngularVel.z = ptr2.uAngularVel.z + vector9.z * num68;
					float num69 = (float)Math.Sqrt((double)(ptr2.uAngularVel.x * ptr2.uAngularVel.x + ptr2.uAngularVel.y * ptr2.uAngularVel.y + ptr2.uAngularVel.z * ptr2.uAngularVel.z));
					if (num69 > 0f)
					{
						double num70 = (double)num69 * 0.016666666666666666 * (double)num25 * 0.5;
						float w = (float)Math.Cos(num70);
						float num71 = (float)Math.Sin(num70) / num69;
						Quaternion lhs3 = new Quaternion(ptr2.uAngularVel.x * num71, ptr2.uAngularVel.y * num71, ptr2.uAngularVel.z * num71, w);
						ptr2.uRot = lhs3 * ptr2.uRot;
					}
					if (ptr2.warpState > 0f)
					{
						float num72 = ptr2.warpState * ptr2.warpState * ptr2.warpState;
						ptr2.uRot = Quaternion.Slerp(ptr2.uRot, Quaternion.LookRotation(vector6, vector5), num72);
						ptr2.uAngularVel *= 1f - num72;
					}
					if (num23 < 100.0)
					{
						float num73 = 1f - (float)num23 / 100f;
						num73 = (3f - num73 - num73) * num73 * num73;
						num73 *= num73;
						if (ptr2.direction > 0)
						{
							uRot = Quaternion.Slerp(ptr2.uRot, ptr4.uRot * (gStationPool[ptr2.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f)), num73);
						}
						else
						{
							Vector3 vector10 = (ptr2.uPos - ptr.uPos).normalized;
							Vector3 normalized = (ptr2.uVel - Vector3.Dot(ptr2.uVel, vector10) * vector10).normalized;
							uRot = Quaternion.Slerp(ptr2.uRot, Quaternion.LookRotation(normalized, vector10), num73);
						}
						flag3 = true;
					}
				}
				double num74 = (double)ptr2.uSpeed * 0.016666666666666666;
				ptr2.uPos.x = ptr2.uPos.x + (double)ptr2.uVel.x * num74 + vectorLF5.x;
				ptr2.uPos.y = ptr2.uPos.y + (double)ptr2.uVel.y * num74 + vectorLF5.y;
				ptr2.uPos.z = ptr2.uPos.z + (double)ptr2.uVel.z * num74 + vectorLF5.z;
				if (flag5)
				{
					ptr2.uRot = uRot;
					if (ptr2.direction > 0)
					{
						ptr2.pPosTemp = Maths.QInvRotateLF(ptr4.uRot, ptr2.uPos - ptr4.uPos);
						ptr2.pRotTemp = Quaternion.Inverse(ptr4.uRot) * ptr2.uRot;
					}
					else
					{
						ptr2.pPosTemp = Maths.QInvRotateLF(ptr.uRot, ptr2.uPos - ptr.uPos);
						ptr2.pRotTemp = Quaternion.Inverse(ptr.uRot) * ptr2.uRot;
					}
					uRot.x = (uRot.y = (uRot.z = 0f));
					uRot.w = 1f;
					flag3 = false;
				}
				if (ptr3.anim.z > 1f)
				{
					ref ShipRenderingData ptr8 = ref ptr3;
					ptr8.anim.z = ptr8.anim.z - 0.0050000004f;
				}
				else
				{
					ptr3.anim.z = 1f;
				}
				ptr3.anim.w = ptr2.warpState;
				goto IL_2F08;
			}
			if (ptr2.stage == 1)
			{
				float num75;
				if (ptr2.direction > 0)
				{
					ptr2.t -= num16 * 0.6666667f;
					num75 = ptr2.t;
					if (ptr2.t < 0f)
					{
						ptr2.t = 1f;
						num75 = 0f;
						ptr2.stage = 2;
					}
					num75 = (3f - num75 - num75) * num75 * num75;
					float num76 = num75 * 2f;
					float num77 = num75 * 2f - 1f;
					VectorLF3 lhs4 = ptr4.uPos + Maths.QRotateLF(ptr4.uRot, gStationPool[ptr2.otherGId].shipDockPos + gStationPool[ptr2.otherGId].shipDockPos.normalized * 7.2700005f);
					if (num75 > 0.5f)
					{
						VectorLF3 lhs5 = ptr4.uPos + Maths.QRotateLF(ptr4.uRot, ptr2.pPosTemp);
						ptr2.uPos = lhs4 * (double)(1f - num77) + lhs5 * (double)num77;
						ptr2.uRot = ptr4.uRot * Quaternion.Slerp(gStationPool[ptr2.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f), ptr2.pRotTemp, num77 * 1.5f - 0.5f);
					}
					else
					{
						VectorLF3 lhs6 = ptr4.uPos + Maths.QRotateLF(ptr4.uRot, gStationPool[ptr2.otherGId].shipDockPos + gStationPool[ptr2.otherGId].shipDockPos.normalized * -14.4f);
						ptr2.uPos = lhs6 * (double)(1f - num76) + lhs4 * (double)num76;
						ptr2.uRot = ptr4.uRot * (gStationPool[ptr2.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
					}
				}
				else
				{
					ptr2.t += num16;
					num75 = ptr2.t;
					if (ptr2.t > 1f)
					{
						ptr2.t = 1f;
						num75 = 1f;
						ptr2.stage = 0;
					}
					num75 = (3f - num75 - num75) * num75 * num75;
					ptr2.uPos = ptr4.uPos + Maths.QRotateLF(ptr4.uRot, gStationPool[ptr2.otherGId].shipDockPos + gStationPool[ptr2.otherGId].shipDockPos.normalized * (-14.4f + 39.4f * num75));
					ptr2.uRot = ptr4.uRot * (gStationPool[ptr2.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
				}
				ptr2.uVel.x = 0f;
				ptr2.uVel.y = 0f;
				ptr2.uVel.z = 0f;
				ptr2.uSpeed = 0f;
				ptr2.uAngularVel.x = 0f;
				ptr2.uAngularVel.y = 0f;
				ptr2.uAngularVel.z = 0f;
				ptr2.uAngularSpeed = 0f;
				ptr3.anim.z = num75 * 1.7f - 0.7f;
				goto IL_2F08;
			}
			if (ptr2.direction > 0)
			{
				ptr2.t -= 0.0334f;
				if (ptr2.t < 0f)
				{
					ptr2.t = 0f;
					StationComponent stationComponent = gStationPool[ptr2.otherGId];
					StationStore[] array4 = stationComponent.storage;
					if (num17 > this.warpEnableDist && ptr2.warperCnt == 0 && stationComponent.warperCount > 0)
					{
						lock (consumeRegister)
						{
							ptr2.warperCnt++;
							stationComponent.warperCount--;
							consumeRegister[1210]++;
						}
					}
					if (ptr2.itemCount > 0)
					{
						stationComponent.AddItem(ptr2.itemId, ptr2.itemCount, ptr2.inc);
						TrafficStatistics traffic2 = factory.gameData.statistics.traffic;
						traffic2.RegisterPlanetInputStat(stationComponent.planetId, ptr2.itemId, ptr2.itemCount);
						int num78 = stationComponent.planetId / 100;
						int num79 = this.planetId / 100;
						if (num78 != num79)
						{
							traffic2.RegisterStarInputStat(num78, ptr2.itemId, ptr2.itemCount);
						}
						else
						{
							traffic2.RegisterStarInternalStat(num78, ptr2.itemId, ptr2.itemCount);
						}
						factory.NotifyShipDelivery(ptr2.planetA, this, ptr2.planetB, stationComponent, ptr2.itemId, ptr2.itemCount);
						ptr2.itemCount = 0;
						ptr2.inc = 0;
						if (this.workShipOrders[j].otherStationGId > 0)
						{
							StationStore[] obj = array4;
							lock (obj)
							{
								if (array4[this.workShipOrders[j].otherIndex].itemId == this.workShipOrders[j].itemId)
								{
									StationStore[] array5 = array4;
									int otherIndex = this.workShipOrders[j].otherIndex;
									array5[otherIndex].remoteOrder = array5[otherIndex].remoteOrder - this.workShipOrders[j].otherOrdered;
								}
							}
							this.workShipOrders[j].ClearOther();
						}
						if (this.remotePairOffsets != null && this.remotePairOffsets[6] > 0)
						{
							int num80;
							int num81;
							if (this.routePriority == ERemoteRoutePriority.Prioritize)
							{
								num80 = 1;
								num81 = 5;
							}
							else if (this.routePriority == ERemoteRoutePriority.Only || this.routePriority == ERemoteRoutePriority.Designated)
							{
								num80 = 1;
								num81 = 4;
							}
							else
							{
								num80 = 0;
								num81 = 0;
							}
							bool flag6 = true;
							for (int m = num80; m <= num81; m++)
							{
								int num82 = this.remotePairOffsets[m + 1] - this.remotePairOffsets[m];
								if (num82 > 0)
								{
									int num83 = this.remotePairOffsets[m];
									this.remotePairProcesses[m] = this.remotePairProcesses[m] % num82;
									int num84 = this.remotePairProcesses[m];
									int num85 = this.remotePairProcesses[m];
									StationStore[] obj;
									SupplyDemandPair supplyDemandPair;
									for (;;)
									{
										supplyDemandPair = this.remotePairs[num85 + num83];
										if (supplyDemandPair.demandId != this.gid || supplyDemandPair.supplyId != stationComponent.gid)
										{
											goto IL_2848;
										}
										if ((int)this.priorityLocks[supplyDemandPair.demandIndex].priorityIndex < m && this.priorityLocks[supplyDemandPair.demandIndex].lockTick > 0)
										{
											num85++;
											num85 %= num82;
										}
										else
										{
											if ((int)stationComponent.priorityLocks[supplyDemandPair.supplyIndex].priorityIndex >= m || stationComponent.priorityLocks[supplyDemandPair.supplyIndex].lockTick <= 0)
											{
												obj = this.storage;
												lock (obj)
												{
													num4 = this.storage[supplyDemandPair.demandIndex].remoteDemandCount;
													num5 = this.storage[supplyDemandPair.demandIndex].totalDemandCount;
													itemId = this.storage[supplyDemandPair.demandIndex].itemId;
												}
												goto IL_2848;
											}
											num85++;
											num85 %= num82;
										}
										IL_2B4C:
										if (num84 == num85)
										{
											break;
										}
										continue;
										IL_2848:
										if (supplyDemandPair.demandId == this.gid && supplyDemandPair.supplyId == stationComponent.gid)
										{
											obj = array4;
											lock (obj)
											{
												num6 = array4[supplyDemandPair.supplyIndex].count;
												num7 = array4[supplyDemandPair.supplyIndex].inc;
												num8 = array4[supplyDemandPair.supplyIndex].remoteSupplyCount;
												num9 = array4[supplyDemandPair.supplyIndex].totalSupplyCount;
											}
										}
										if (supplyDemandPair.demandId == this.gid && supplyDemandPair.supplyId == stationComponent.gid)
										{
											if (num4 > 0 && num5 > 0)
											{
												if (num6 >= shipCarries && num8 >= shipCarries && num9 >= shipCarries)
												{
													goto Block_126;
												}
												stationComponent.SetPriorityLock(supplyDemandPair.supplyIndex, m);
											}
											else if (num6 <= shipCarries || num8 <= shipCarries || num9 <= shipCarries)
											{
												stationComponent.SetPriorityLock(supplyDemandPair.supplyIndex, m);
											}
										}
										num85++;
										num85 %= num82;
										goto IL_2B4C;
									}
									IL_2B55:
									if (flag6)
									{
										goto IL_2B5C;
									}
									break;
									Block_126:
									int num86 = (shipCarries < num6) ? shipCarries : num6;
									int num87 = num6;
									int num88 = num7;
									int num89 = this.split_inc(ref num87, ref num88, num86);
									ptr2.itemId = (this.workShipOrders[j].itemId = itemId);
									ptr2.itemCount = num86;
									ptr2.inc = num89;
									obj = array4;
									lock (obj)
									{
										StationStore[] array6 = array4;
										int supplyIndex = supplyDemandPair.supplyIndex;
										array6[supplyIndex].count = array6[supplyIndex].count - num86;
										StationStore[] array7 = array4;
										int supplyIndex2 = supplyDemandPair.supplyIndex;
										array7[supplyIndex2].inc = array7[supplyIndex2].inc - num89;
										traffic2.RegisterPlanetOutputStat(stationComponent.planetId, array4[supplyDemandPair.supplyIndex].itemId, num86);
										if (num78 != num79)
										{
											traffic2.RegisterStarOutputStat(num78, array4[supplyDemandPair.supplyIndex].itemId, num86);
										}
										else
										{
											traffic2.RegisterStarInternalStat(num78, array4[supplyDemandPair.supplyIndex].itemId, num86);
										}
									}
									this.workShipOrders[j].otherStationGId = stationComponent.gid;
									this.workShipOrders[j].thisIndex = supplyDemandPair.demandIndex;
									this.workShipOrders[j].otherIndex = supplyDemandPair.supplyIndex;
									this.workShipOrders[j].thisOrdered = num86;
									this.workShipOrders[j].otherOrdered = 0;
									obj = this.storage;
									lock (obj)
									{
										StationStore[] array8 = this.storage;
										int demandIndex = supplyDemandPair.demandIndex;
										array8[demandIndex].remoteOrder = array8[demandIndex].remoteOrder + num86;
									}
									this.SetPriorityLock(supplyDemandPair.demandIndex, m);
									stationComponent.SetPriorityLock(supplyDemandPair.supplyIndex, m);
									flag6 = false;
									goto IL_2B55;
								}
								IL_2B5C:;
							}
						}
					}
					else
					{
						int itemId2 = ptr2.itemId;
						int num90 = shipCarries;
						int inc;
						stationComponent.TakeItem(ref itemId2, ref num90, out inc);
						TrafficStatistics traffic3 = factory.gameData.statistics.traffic;
						traffic3.RegisterPlanetOutputStat(stationComponent.planetId, itemId2, num90);
						int num91 = stationComponent.planetId / 100;
						int num92 = this.planetId / 100;
						if (num91 != num92)
						{
							traffic3.RegisterStarOutputStat(num91, itemId2, num90);
						}
						else
						{
							traffic3.RegisterStarInternalStat(num91, itemId2, num90);
						}
						ptr2.itemCount = num90;
						ptr2.inc = inc;
						StationStore[] obj;
						if (this.workShipOrders[j].otherStationGId > 0)
						{
							obj = array4;
							lock (obj)
							{
								if (array4[this.workShipOrders[j].otherIndex].itemId == this.workShipOrders[j].itemId)
								{
									StationStore[] array9 = array4;
									int otherIndex2 = this.workShipOrders[j].otherIndex;
									array9[otherIndex2].remoteOrder = array9[otherIndex2].remoteOrder - this.workShipOrders[j].otherOrdered;
								}
							}
							this.workShipOrders[j].ClearOther();
						}
						obj = this.storage;
						lock (obj)
						{
							if (this.storage[this.workShipOrders[j].thisIndex].itemId == this.workShipOrders[j].itemId && this.workShipOrders[j].thisOrdered != num90)
							{
								int num93 = num90 - this.workShipOrders[j].thisOrdered;
								StationStore[] array10 = this.storage;
								int thisIndex2 = this.workShipOrders[j].thisIndex;
								array10[thisIndex2].remoteOrder = array10[thisIndex2].remoteOrder + num93;
								RemoteLogisticOrder[] array11 = this.workShipOrders;
								int num94 = j;
								array11[num94].thisOrdered = array11[num94].thisOrdered + num93;
							}
						}
					}
					ptr2.direction = -1;
				}
			}
			else
			{
				ptr2.t += 0.0334f;
				if (ptr2.t > 1f)
				{
					ptr2.t = 0f;
					ptr2.stage = 1;
				}
			}
			ptr2.uPos = ptr4.uPos + Maths.QRotateLF(ptr4.uRot, gStationPool[ptr2.otherGId].shipDockPos + gStationPool[ptr2.otherGId].shipDockPos.normalized * -14.4f);
			ptr2.uVel.x = 0f;
			ptr2.uVel.y = 0f;
			ptr2.uVel.z = 0f;
			ptr2.uSpeed = 0f;
			ptr2.uRot = ptr4.uRot * (gStationPool[ptr2.otherGId].shipDockRot * new Quaternion(0.70710677f, 0f, 0f, -0.70710677f));
			ptr2.uAngularVel.x = 0f;
			ptr2.uAngularVel.y = 0f;
			ptr2.uAngularVel.z = 0f;
			ptr2.uAngularSpeed = 0f;
			ptr2.pPosTemp = new VectorLF3(0f, 0f, 0f);
			ptr2.pRotTemp = new Quaternion(0f, 0f, 0f, 1f);
			ptr3.anim.z = 0f;
			goto IL_2F08;
			IL_3068:
			j++;
			continue;
			IL_2F08:
			Vector3 vector11;
			vector11.x = ptr2.uVel.x * ptr2.uSpeed;
			vector11.y = ptr2.uVel.y * ptr2.uSpeed;
			vector11.z = ptr2.uVel.z * ptr2.uSpeed;
			if (flag3)
			{
				ptr3.SetPose(ref ptr2.uPos, ref uRot, ref relativePos, ref relativeRot, ref vector11, (ptr2.itemCount > 0) ? ptr2.itemId : 0);
				if (starmap)
				{
					this.shipUIRenderers[ptr2.shipIndex].SetPose(ref ptr2.uPos, ref uRot, (float)num17, ptr2.uSpeed, (ptr2.itemCount > 0) ? ptr2.itemId : 0);
				}
			}
			else
			{
				ptr3.SetPose(ref ptr2.uPos, ref ptr2.uRot, ref relativePos, ref relativeRot, ref vector11, (ptr2.itemCount > 0) ? ptr2.itemId : 0);
				if (starmap)
				{
					this.shipUIRenderers[ptr2.shipIndex].SetPose(ref ptr2.uPos, ref ptr2.uRot, (float)num17, ptr2.uSpeed, (ptr2.itemCount > 0) ? ptr2.itemId : 0);
				}
			}
			if (ptr3.anim.z < 0f)
			{
				ptr3.anim.z = 0f;
				goto IL_3068;
			}
			goto IL_3068;
		}
		this.ShipRenderersOnTick(astroPoses, ref relativePos, ref relativeRot);
		for (int n = 0; n < this.priorityLocks.Length; n++)
		{
			if (this.priorityLocks[n].priorityIndex >= 0)
			{
				if (this.priorityLocks[n].lockTick > 0)
				{
					StationPriorityLock[] array12 = this.priorityLocks;
					int num95 = n;
					array12[num95].lockTick = array12[num95].lockTick - 1;
				}
				else
				{
					this.priorityLocks[n].lockTick = 0;
					this.priorityLocks[n].priorityIndex = 0;
				}
			}
		}
	}

	// Token: 0x06000A44 RID: 2628 RVA: 0x0009756C File Offset: 0x0009576C
	public int CalcLocalSingleTripTime(StationComponent otherStation, float droneSpeed)
	{
		Vector3 lhs = otherStation.droneDock;
		Vector3 vector = this.droneDock;
		float magnitude = lhs.magnitude;
		float magnitude2 = vector.magnitude;
		double num = (double)(Mathf.Acos(Vector3.Dot(lhs, this.droneDock) / magnitude2 / magnitude) * (magnitude2 + magnitude) / 2f);
		int num2 = (int)(1.5 / (Math.Sqrt((double)(droneSpeed / 8f)) * 0.016666666666666666) + 1.0);
		return (int)(num / ((double)droneSpeed * 0.016666666666666666) + 1.0) + 2 * num2;
	}

	// Token: 0x06000A45 RID: 2629 RVA: 0x00097604 File Offset: 0x00095804
	public int CalcRemoteSingleTripTime(AstroData[] astroPoses, StationComponent targetStation, float shipSailSpeed, float shipWarpSpeed, bool canWarp, int direction, int shipIndex = 0)
	{
		AstroData astroData = astroPoses[this.planetId];
		AstroData astroData2 = astroPoses[targetStation.planetId];
		Vector3 a = this.shipDiskPos[shipIndex];
		a += 25f * a.normalized;
		VectorLF3 lhs;
		StationComponent.lpos2upos_out(ref astroData.uPos, ref astroData.uRot, ref a, out lhs);
		a = targetStation.shipDockPos;
		a += 25f * a.normalized;
		VectorLF3 rhs;
		StationComponent.lpos2upos_out(ref astroData2.uPos, ref astroData2.uRot, ref a, out rhs);
		double magnitude = (lhs - rhs).magnitude;
		float num = shipSailSpeed / 600f;
		float num2 = Mathf.Pow(num, 0.4f);
		float num3 = (num2 > 1f) ? (Mathf.Log(num2) + 1f) : num2;
		if (num > 500f)
		{
			num = 500f;
		}
		float num4 = num2 * 0.006f + 1E-05f;
		float num5 = shipSailSpeed * 0.03f;
		float num6 = shipSailSpeed * 0.12f * num3;
		float num7 = Mathf.Sqrt(((direction == 1) ? 2f : 0.5f) * astroData.uRadius * num5);
		float num8 = (shipSailSpeed + num7) * (shipSailSpeed - num7) / num6 * 0.5f;
		float num9 = 0.382f * num3;
		float num10 = 0.15f * num + 6f * num2;
		float num11 = (shipSailSpeed - num10) / num9;
		int num12 = (int)((Math.Log((double)(num9 * num11 / num10 + 1f)) - Math.Log((double)(num9 * 6f * num3 / num10 + 1f))) / (double)num9 * 60.0 + 1.0);
		int num13 = 60;
		num13 += (int)(1f / num4 + 1f) + (int)(1.5f / num4 + 1f);
		double num14 = 5000.0 + 0.25 * (double)shipSailSpeed;
		if (magnitude - (double)num8 - num14 <= 0.0)
		{
			canWarp = false;
		}
		if (!canWarp)
		{
			if (magnitude > (double)(num8 + num11))
			{
				num13 += (int)((double)(num7 / num5) * 60.0 + 1.0);
				num13 += (int)((double)((shipSailSpeed - num7) / num6) * 60.0 + 0.5);
				num13 += (int)((magnitude - (double)num8 - (double)num11) / (double)shipSailSpeed * 60.0 + 0.5);
				num13 += num12;
			}
			else if ((double)((num7 - num10) / num9) < magnitude)
			{
				num13 += (int)((double)(num7 / num5) * 60.0 + 1.0);
				float num15 = Mathf.Sqrt(2f * num6 * ((float)magnitude + num10 / num9) + num7 * num7 + num6 * num6 / num9 / num9) - num6 / num9;
				num13 += (int)((double)((num15 - num7) / num6) * 60.0 + 0.5);
				num11 = (num15 - num10) / num9;
				num12 = (int)((Math.Log((double)(num9 * num11 / num10 + 1f)) - Math.Log((double)(num9 * 6f * num3 / num10 + 1f))) / (double)num9 * 60.0 + 1.0);
				num13 += num12;
			}
			else
			{
				float num16 = (float)((double)num9 * magnitude + (double)num10);
				num13 += (int)((double)(num16 / num5) * 60.0 + 1.0);
				num11 = (num16 - num10) / num9;
				num12 = (int)((Math.Log((double)(num9 * num11 / num10 + 1f)) - Math.Log((double)(num9 * 6f * num3 / num10 + 1f))) / (double)num9 * 60.0 + 1.0);
				num13 += num12;
			}
		}
		else
		{
			double num17 = (magnitude * 2.0 < (double)shipWarpSpeed) ? (magnitude * 2.0) : ((double)shipWarpSpeed);
			double num18 = num17 * 0.18775714286 + (double)shipSailSpeed * 1.33333333333;
			num13 += (int)((double)(num7 / num5) * 60.0 + 1.0);
			num13 += (int)((double)((shipSailSpeed - num7) / num6) * 60.0 + 0.5);
			if (num8 < 5000f)
			{
				num13 += (int)((double)((5000f - num8) / shipSailSpeed) * 60.0);
			}
			num8 = ((num8 > 5000f) ? num8 : 5000f);
			if (magnitude > num18 + (double)num8 + num14)
			{
				num13 += 80;
				num13 += (int)((magnitude - num18 - (double)num8 - num14) / (num17 + (double)shipSailSpeed) * 60.0);
			}
			else
			{
				double num19 = (magnitude - (double)num8 - num14) / num18;
				num13 += (int)(6400.0 / (79.0 / Math.Pow(num19, 0.0) + 1.0 / num19));
			}
			if (num14 > (double)num11)
			{
				num13 += (int)((num14 - (double)num11) / (double)shipSailSpeed * 60.0);
				num13 += num12;
			}
			else
			{
				num13 += (int)((Math.Log((double)num9 * num14 / (double)num10 + 1.0) - Math.Log((double)(num9 * 6f * num3 / num10 + 1f))) / (double)num9 * 60.0 + 1.0);
			}
		}
		return num13;
	}

	// Token: 0x06000A46 RID: 2630 RVA: 0x00097B9C File Offset: 0x00095D9C
	public int CalcArrivalRemainingTime(AstroData[] astroPoses, StationComponent targetStation, float shipSailSpeed, float shipWarpSpeed, int shipIndex)
	{
		int num = 0;
		ShipData shipData = this.workShipDatas[shipIndex];
		if (shipData.stage != 0)
		{
			if (shipData.stage * shipData.direction < 0)
			{
				num = this.CalcRemoteSingleTripTime(astroPoses, targetStation, shipSailSpeed, shipWarpSpeed, shipData.warperCnt > 0, shipData.direction, shipIndex);
				switch (shipData.stage)
				{
				case -2:
					num -= (int)(shipData.t * 30f);
					break;
				case -1:
					num -= (int)(shipData.t / (Mathf.Pow(shipSailSpeed / 600f, 0.4f) * 0.006f + 1E-05f) + 30f);
					break;
				case 1:
					num -= (int)(shipData.t / (Mathf.Pow(shipSailSpeed / 600f, 0.4f) * 0.006f + 1E-05f) + 30f);
					break;
				case 2:
					num -= (int)(shipData.t * 30f);
					break;
				}
			}
			else
			{
				switch (shipData.stage)
				{
				case -2:
					num = (int)(shipData.t * 30f);
					break;
				case -1:
					num = (int)(1.5f * shipData.t / (Mathf.Pow(shipSailSpeed / 600f, 0.4f) * 0.006f + 1E-05f) + 30f);
					break;
				case 1:
					num = (int)(1.5f * shipData.t / (Mathf.Pow(shipSailSpeed / 600f, 0.4f) * 0.006f + 1E-05f) + 30f);
					break;
				case 2:
					num = (int)(shipData.t * 30f);
					break;
				}
			}
		}
		else
		{
			AstroData astroData = astroPoses[this.planetId];
			AstroData astroData2 = astroPoses[targetStation.planetId];
			Vector3 a = this.shipDiskPos[shipIndex];
			a += 25f * a.normalized;
			VectorLF3 lhs;
			StationComponent.lpos2upos_out(ref astroData.uPos, ref astroData.uRot, ref a, out lhs);
			a = targetStation.shipDockPos;
			a += 25f * a.normalized;
			VectorLF3 vectorLF;
			StationComponent.lpos2upos_out(ref astroData2.uPos, ref astroData2.uRot, ref a, out vectorLF);
			double magnitude = (lhs - vectorLF).magnitude;
			VectorLF3 vectorLF2 = (shipData.direction > 0) ? (astroData.uPos - shipData.uPos) : (astroData2.uPos - shipData.uPos);
			VectorLF3 vectorLF3 = (shipData.direction > 0) ? (vectorLF - shipData.uPos) : (lhs - shipData.uPos);
			float num2 = shipSailSpeed / 600f;
			float num3 = Mathf.Pow(num2, 0.4f);
			float num4 = (num3 > 1f) ? (Mathf.Log(num3) + 1f) : num3;
			if (num2 > 500f)
			{
				num2 = 500f;
			}
			float num5 = num3 * 0.006f + 1E-05f;
			float num6 = shipSailSpeed * 0.03f;
			float num7 = shipSailSpeed * 0.12f * num4;
			double magnitude2 = vectorLF2.magnitude;
			double num8 = vectorLF3.magnitude;
			num = 30;
			num += (int)(1.5f / num5 + 1f);
			float num9 = shipSailSpeed;
			if (1.5 * (double)astroData.uRadius > magnitude2)
			{
				num9 = Mathf.Sqrt((float)((double)(shipData.uSpeed * shipData.uSpeed) + (double)((shipData.direction == 1) ? 8f : 2f) * (1.5 * (double)astroData.uRadius - magnitude2) * (double)num6));
			}
			num9 = ((num9 < shipSailSpeed) ? num9 : shipSailSpeed);
			float num10 = (shipSailSpeed + num9) * (shipSailSpeed - num9) / num7 * 0.5f;
			float num11 = 0.382f * num4;
			float num12 = 0.15f * num2 + 6f * num3;
			float num13 = (shipSailSpeed - num12) / num11;
			int num14 = (int)((Math.Log((double)(num11 * num13 / num12 + 1f)) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
			bool flag = shipData.warperCnt > 0;
			if (flag)
			{
				float num15 = (shipSailSpeed + shipData.uSpeed) * (shipSailSpeed - shipData.uSpeed) / num7 * 0.5f;
				num15 = ((num15 < num10) ? num15 : num10);
				if (num15 < 0f)
				{
					num15 = 0f;
				}
				double num16 = 5000.0 + 0.25 * (double)shipSailSpeed;
				if (num8 < 5000.0 + 0.25 * (double)shipSailSpeed + (double)num15 || num8 < this.warpEnableDist * 0.5 + (double)num15 || num8 < (double)(num13 + num15) || num8 < num16 + (double)num15)
				{
					flag = false;
				}
			}
			if (!(flag | shipData.warpState > 0f))
			{
				if (magnitude2 < (double)(astroData.uRadius * 1.5f))
				{
					if (num8 > (double)(num10 + num13))
					{
						num += (int)((double)((num9 - shipData.uSpeed) / num6) * 60.0 + 1.0);
						num += (int)((double)((shipSailSpeed - num9) / num7) * 60.0 + 0.5);
						num += (int)((num8 - (double)num10 - (double)num13) / (double)shipSailSpeed * 60.0 + 0.5);
						num += num14;
					}
					else if ((double)((num9 - num12) / num11) < num8)
					{
						num += (int)((double)((num9 - shipData.uSpeed) / num6) * 60.0 + 1.0);
						float num17 = Mathf.Sqrt(2f * num7 * ((float)num8 + num12 / num11) + num9 * num9 + num7 * num7 / num11 / num11) - num7 / num11;
						num += (int)((double)((num17 - num9) / num7) * 60.0 + 0.5);
						num13 = (num17 - num12) / num11;
						num14 = (int)((Math.Log((double)(num11 * num13 / num12 + 1f)) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
						num += num14;
					}
					else
					{
						float num18 = (float)((double)num11 * num8 + (double)num12);
						if (num18 > shipData.uSpeed)
						{
							num += (int)((double)((num18 - shipData.uSpeed) / num6) * 60.0 + 1.0);
						}
						num13 = (num18 - num12) / num11;
						num14 = (int)((Math.Log((double)(num11 * num13 / num12 + 1f)) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
						num += num14;
					}
				}
				else
				{
					float num19 = (float)((double)shipData.uSpeed - 0.15 * (double)num2 - (double)(6f * num3)) * 2.6178f / num4;
					if (num8 > (double)num19)
					{
						num13 = (shipSailSpeed - num12) / num11;
						if (shipData.uSpeed < shipSailSpeed)
						{
							num10 = (shipSailSpeed + shipData.uSpeed) * (shipSailSpeed - shipData.uSpeed) / num7 * 0.5f;
							if (num8 > (double)(num10 + num13))
							{
								num += (int)((double)((shipSailSpeed - shipData.uSpeed) / num7) * 60.0 + 0.5);
								num += (int)((num8 - (double)num10 - (double)num13) / (double)shipSailSpeed * 60.0 + 0.5);
								num += num14;
							}
							else
							{
								float num20 = Mathf.Sqrt(2f * num7 * ((float)num8 + num12 / num11) + shipData.uSpeed * shipData.uSpeed + num7 * num7 / num11 / num11) - num7 / num11;
								num13 = (num20 - num12) / num11;
								num14 = (int)((Math.Log((double)(num11 * num13 / num12 + 1f)) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
								num += (int)((double)((num20 - shipData.uSpeed) / num7) * 60.0 + 0.5);
								num += num14;
							}
						}
						else
						{
							num += (int)((num8 - (double)num13) / (double)shipSailSpeed * 60.0 + 0.5);
							num += num14;
						}
					}
					else
					{
						num += (int)((Math.Log((double)num11 * num8 / (double)num12 + 1.0) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0);
					}
				}
			}
			else if (magnitude2 < (double)(astroData.uRadius * 1.5f))
			{
				double num21 = (magnitude * 2.0 < (double)shipWarpSpeed) ? (magnitude * 2.0) : ((double)shipWarpSpeed);
				double num22 = num21 * 0.18775714286 + (double)shipSailSpeed * 1.33333333333;
				double num23 = 5000.0 + 0.25 * (double)shipSailSpeed;
				if (num9 > shipData.uSpeed)
				{
					num += (int)((double)((num9 - shipData.uSpeed) / num6) * 60.0 + 1.0);
				}
				num += (int)((double)((shipSailSpeed - num9) / num7) * 60.0 + 0.5);
				num10 = (shipSailSpeed + num9) * (shipSailSpeed - num9) / num7 * 0.5f;
				if (num10 < 5000f)
				{
					num += (int)((double)((5000f - num10) / shipSailSpeed) * 60.0);
				}
				num10 = ((num10 > 5000f) ? num10 : 5000f);
				if (num8 > num22 + (double)num10 + (double)num13)
				{
					num += 80;
					num += (int)((num8 - num22 - (double)num10 - ((num23 > (double)num13) ? num23 : ((double)num13))) / (num21 + (double)shipSailSpeed) * 60.0);
				}
				else
				{
					double num24 = (num8 - num23) / num22;
					num += (int)(6400.0 / (79.0 * Math.Pow(num24, 0.0) + 1.0 / num24) - (double)shipData.warpState * 60.0);
				}
				if (num23 > (double)num13)
				{
					num += (int)((num23 - (double)num13) / (double)shipSailSpeed * 60.0);
					num += num14;
				}
				else
				{
					num += (int)((Math.Log((double)num11 * num23 / (double)num12 + 1.0) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
				}
			}
			else if (shipData.warpState <= 0f)
			{
				double num25 = (magnitude * 2.0 < (double)shipWarpSpeed) ? (magnitude * 2.0) : ((double)shipWarpSpeed);
				double num26 = num25 * 0.18775714286 + (double)shipSailSpeed * 1.33333333333;
				double num27 = 5000.0 + 0.25 * (double)shipSailSpeed;
				if (shipData.uSpeed < shipSailSpeed)
				{
					num += (int)((double)((shipSailSpeed - shipData.uSpeed) / num7) * 60.0 + 0.5);
					num10 = (shipSailSpeed + shipData.uSpeed) * (shipSailSpeed - shipData.uSpeed) / num7 * 0.5f;
					if (magnitude2 + (double)num10 < 5000.0)
					{
						num += (int)(((double)(5000f - num10) - magnitude2) / (double)shipSailSpeed * 60.0);
					}
					num10 = (float)(((double)num10 > 5000.0 - magnitude2) ? ((double)num10) : (5000.0 - magnitude2));
					if (num8 > num26 + (double)num10 + num27)
					{
						num += 80;
						num += (int)((num8 - num26 - (double)num10 - num27) / (num25 + (double)shipSailSpeed) * 60.0);
					}
					else
					{
						double num28 = (num8 - (double)num10 - num27) / num26;
						num += (int)(6400.0 / (79.0 * Math.Pow(num28, 0.0) + 1.0 / num28) - (double)shipData.warpState * 60.0);
					}
				}
				else
				{
					if (magnitude2 < 5000.0)
					{
						num += (int)((5000.0 - magnitude2) / (double)shipSailSpeed * 60.0);
						num8 -= 5000.0 - magnitude2;
					}
					if (num8 > num26 + num27)
					{
						num += 80;
						num += (int)((num8 - num26 - num27) / (num25 + (double)shipSailSpeed) * 60.0);
					}
					else
					{
						double num29 = (num8 - num27) / num26;
						num += (int)(6400.0 / (79.0 * Math.Pow(num29, 0.0) + 1.0 / num29) - (double)shipData.warpState * 60.0);
					}
				}
				if (num27 > (double)num13)
				{
					num += (int)((num27 - (double)num13) / (double)shipSailSpeed * 60.0);
					num += num14;
				}
				else
				{
					num += (int)((Math.Log((double)num11 * num27 / (double)num12 + 1.0) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
				}
			}
			else
			{
				double num30 = (magnitude * 2.0 < (double)shipWarpSpeed) ? (magnitude * 2.0) : ((double)shipWarpSpeed);
				double num31 = num30 * 0.0449 + 5000.0 + (double)shipSailSpeed * 0.25;
				double num32 = 5000.0 + 0.25 * (double)shipSailSpeed;
				double num33 = num30 * 0.18775714286 + (double)shipSailSpeed * 1.33333333333;
				if (num8 < num31)
				{
					if (num8 > num30 * ((Math.Pow(1001.0, (double)shipData.warpState + 0.2) - 1.0) / 1000.0) * 0.0449 + 5000.0 + (double)shipSailSpeed * 0.25)
					{
						double num34 = num30 * (double)Mathf.Pow(shipData.warpState, 7f) / 7.0 + (double)(shipSailSpeed * shipData.warpState);
						double num35 = (num8 + num34 - num32) / num33;
						num += (int)(6400.0 / (79.0 * Math.Pow(num35, 0.0) + 1.0 / num35) - (double)shipData.warpState * 60.0);
					}
					else
					{
						num += (int)(shipData.warpState * 20f + 0.5f);
					}
				}
				else if (shipData.warpState < 1f)
				{
					double num36 = num30 * (double)Mathf.Pow(shipData.warpState, 7f) / 7.0 + (double)(shipSailSpeed * shipData.warpState);
					if (num8 > num33 - num36 + num32)
					{
						num += 80;
						num += (int)((num8 - num33 + num36 - num32) / (num30 + (double)shipSailSpeed) * 60.0);
					}
					else
					{
						double num37 = (num8 + num36 - num32) / num33;
						num += (int)(6400.0 / (79.0 * Math.Pow(num37, 0.0) + 1.0 / num37) - (double)shipData.warpState * 60.0);
					}
				}
				else
				{
					num += (int)((num8 - num31) / (num30 + (double)shipSailSpeed) * 60.0);
					num += 20;
				}
				if (num32 > (double)num13)
				{
					num += (int)((num32 - (double)num13) / (double)shipSailSpeed * 60.0);
					num += num14;
				}
				else
				{
					num += (int)((Math.Log((double)num11 * num32 / (double)num12 + 1.0) - Math.Log((double)(num11 * 6f * num4 / num12 + 1f))) / (double)num11 * 60.0 + 1.0);
				}
			}
		}
		return num;
	}

	// Token: 0x06000A47 RID: 2631 RVA: 0x00098C6C File Offset: 0x00096E6C
	public void DetermineDispatch(float shipSailSpeed, float shipWarpSpeed, int shipCarries, int priorityIndex, StationComponent[] gStationPool, FactoryProductionStat[] factoryStatPool, PlanetFactory[] factories, GalaxyData galaxy, TrafficStatistics tstat)
	{
		bool flag = shipWarpSpeed > shipSailSpeed + 1f;
		int num = 0;
		int num2 = 0;
		int storagePairSupplyInc = 0;
		int num3 = 0;
		int num4 = 0;
		int storagePairSupplyItemId = 0;
		int num5 = 0;
		int num6 = 0;
		int storagePairDemandItemId = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		int num12 = 0;
		this._tmp_iter_remote++;
		if (this.remotePairOffsets != null && this.remotePairOffsets[6] > 0)
		{
			int num13 = this.remotePairOffsets[priorityIndex + 1] - this.remotePairOffsets[priorityIndex];
			if (num13 <= 0)
			{
				return;
			}
			int num14 = this.remotePairOffsets[priorityIndex];
			this.remotePairProcesses[priorityIndex] = this.remotePairProcesses[priorityIndex] % num13;
			int num15 = this.remotePairProcesses[priorityIndex];
			int num16 = (shipCarries - 1) * this.deliveryShips / 100;
			AstroData[] astrosData = galaxy.astrosData;
			int factoryIndex = galaxy.PlanetById(this.planetId).factoryIndex;
			int[] consumeRegister = factoryStatPool[factoryIndex].consumeRegister;
			PowerSystem powerSystem = factories[factoryIndex].powerSystem;
			float num17 = powerSystem.networkServes[powerSystem.consumerPool[this.pcId].networkId];
			SupplyDemandPair ptr;
			StationComponent stationComponent;
			StationComponent stationComponent3;
			double num21;
			bool flag7;
			for (;;)
			{
				int num18 = num16;
				ptr = ref this.remotePairs[this.remotePairProcesses[priorityIndex] + num14];
				if (ptr.supplyId == this.gid)
				{
					if ((int)this.priorityLocks[ptr.supplyIndex].priorityIndex < priorityIndex && this.priorityLocks[ptr.supplyIndex].lockTick > 0)
					{
						this.remotePairProcesses[priorityIndex]++;
						if (this.remotePairProcesses[priorityIndex] == num13)
						{
							this.remotePairProcesses[priorityIndex] = 0;
						}
					}
					else
					{
						StationStore[] obj = this.storage;
						lock (obj)
						{
							num = this.storage[ptr.supplyIndex].max;
							num2 = this.storage[ptr.supplyIndex].count;
							storagePairSupplyInc = this.storage[ptr.supplyIndex].inc;
							num3 = this.storage[ptr.supplyIndex].remoteSupplyCount;
							num4 = this.storage[ptr.supplyIndex].totalSupplyCount;
							storagePairSupplyItemId = this.storage[ptr.supplyIndex].itemId;
						}
						if (num <= num18)
						{
							num18 = num - 1;
							if (num18 < 0)
							{
								num18 = 0;
							}
						}
						if (num2 > num18 && num3 > num18 && num4 > num18)
						{
							stationComponent = gStationPool[ptr.demandId];
							if (stationComponent == null)
							{
								goto IL_CD8;
							}
							if ((int)stationComponent.priorityLocks[ptr.demandIndex].priorityIndex < priorityIndex && stationComponent.priorityLocks[ptr.demandIndex].lockTick > 0)
							{
								this.remotePairProcesses[priorityIndex]++;
								if (this.remotePairProcesses[priorityIndex] == num13)
								{
									this.remotePairProcesses[priorityIndex] = 0;
								}
							}
							else
							{
								obj = stationComponent.storage;
								lock (obj)
								{
									num7 = stationComponent.storage[ptr.demandIndex].remoteDemandCount;
									num8 = stationComponent.storage[ptr.demandIndex].totalDemandCount;
								}
								if (num7 <= 0 || num8 <= 0)
								{
									stationComponent.SetPriorityLock(ptr.demandIndex, priorityIndex);
									goto IL_CD8;
								}
								double num19 = (astrosData[this.planetId].uPos - astrosData[stationComponent.planetId].uPos).magnitude + (double)astrosData[this.planetId].uRadius + (double)astrosData[stationComponent.planetId].uRadius;
								bool flag3 = num19 < this.tripRangeShips;
								bool flag4 = num19 >= this.warpEnableDist;
								if (this.warperNecessary && flag4 && (this.warperCount < 2 || !flag))
								{
									flag3 = false;
								}
								if (this.idleShipCount == 0 || this.energy <= 6000000L)
								{
									flag3 = false;
								}
								if (!flag3)
								{
									goto IL_CD8;
								}
								long num20 = this.CalcTripEnergyCost(num19, shipSailSpeed, flag);
								bool flag5;
								if (this.energy >= num20)
								{
									int carryCnt = (shipCarries < num2) ? shipCarries : num2;
									if (this.DispatchSupplyShip(carryCnt, num2, storagePairSupplyInc, storagePairSupplyItemId, flag4, stationComponent, consumeRegister, ref ptr, tstat))
									{
										this.energy -= num20;
									}
									flag5 = true;
								}
								else
								{
									flag5 = (num17 > 0.1f);
								}
								if (flag5)
								{
									break;
								}
								goto IL_CD8;
							}
						}
						else
						{
							StationComponent stationComponent2 = gStationPool[ptr.demandId];
							if (stationComponent2 == null)
							{
								goto IL_CD8;
							}
							obj = stationComponent2.storage;
							lock (obj)
							{
								num7 = stationComponent2.storage[ptr.demandIndex].remoteDemandCount;
								num8 = stationComponent2.storage[ptr.demandIndex].totalDemandCount;
							}
							if (num7 <= 0 || num8 <= 0)
							{
								stationComponent2.SetPriorityLock(ptr.demandIndex, priorityIndex);
								goto IL_CD8;
							}
							goto IL_CD8;
						}
					}
				}
				else if ((int)this.priorityLocks[ptr.demandIndex].priorityIndex < priorityIndex && this.priorityLocks[ptr.demandIndex].lockTick > 0)
				{
					this.remotePairProcesses[priorityIndex]++;
					if (this.remotePairProcesses[priorityIndex] == num13)
					{
						this.remotePairProcesses[priorityIndex] = 0;
					}
				}
				else
				{
					StationStore[] obj = this.storage;
					lock (obj)
					{
						num5 = this.storage[ptr.demandIndex].remoteDemandCount;
						num6 = this.storage[ptr.demandIndex].totalDemandCount;
					}
					if (num5 > 0 && num6 > 0)
					{
						stationComponent3 = gStationPool[ptr.supplyId];
						if (stationComponent3 == null)
						{
							goto IL_CD8;
						}
						if ((int)stationComponent3.priorityLocks[ptr.supplyIndex].priorityIndex < priorityIndex && stationComponent3.priorityLocks[ptr.supplyIndex].lockTick > 0)
						{
							this.remotePairProcesses[priorityIndex]++;
							if (this.remotePairProcesses[priorityIndex] == num13)
							{
								this.remotePairProcesses[priorityIndex] = 0;
							}
						}
						else
						{
							obj = stationComponent3.storage;
							lock (obj)
							{
								num9 = stationComponent3.storage[ptr.supplyIndex].max;
								num10 = stationComponent3.storage[ptr.supplyIndex].count;
								StationStore[] array = stationComponent3.storage;
								int supplyIndex = ptr.supplyIndex;
								num11 = stationComponent3.storage[ptr.supplyIndex].remoteSupplyCount;
								num12 = stationComponent3.storage[ptr.supplyIndex].totalSupplyCount;
							}
							if (num9 <= num18)
							{
								num18 = num9 - 1;
								if (num18 < 0)
								{
									num18 = 0;
								}
							}
							if (num10 <= num18 || num11 <= num18 || num12 <= num18)
							{
								stationComponent3.SetPriorityLock(ptr.supplyIndex, priorityIndex);
								goto IL_CD8;
							}
							num21 = (astrosData[this.planetId].uPos - astrosData[stationComponent3.planetId].uPos).magnitude + (double)astrosData[this.planetId].uRadius + (double)astrosData[stationComponent3.planetId].uRadius;
							bool flag6 = num21 < this.tripRangeShips;
							if (flag6 && !this.includeOrbitCollector && stationComponent3.isCollector)
							{
								flag6 = false;
							}
							flag7 = (num21 >= this.warpEnableDist);
							if (this.warperNecessary && flag7 && (this.warperCount < 2 || !flag))
							{
								flag6 = false;
							}
							if (this.idleShipCount == 0 || this.energy <= 6000000L)
							{
								flag6 = false;
							}
							if (flag6)
							{
								goto Block_54;
							}
							goto IL_CD8;
						}
					}
					else
					{
						StationComponent stationComponent4 = gStationPool[ptr.supplyId];
						if (stationComponent4 == null)
						{
							goto IL_CD8;
						}
						obj = stationComponent4.storage;
						lock (obj)
						{
							num9 = stationComponent4.storage[ptr.supplyIndex].max;
							num10 = stationComponent4.storage[ptr.supplyIndex].count;
							StationStore[] array2 = stationComponent4.storage;
							int supplyIndex2 = ptr.supplyIndex;
							num11 = stationComponent4.storage[ptr.supplyIndex].remoteSupplyCount;
							num12 = stationComponent4.storage[ptr.supplyIndex].totalSupplyCount;
						}
						if (num9 <= num18)
						{
							num18 = num9 - 1;
							if (num18 < 0)
							{
								num18 = 0;
							}
						}
						if (num10 <= num18 || num11 <= num18 || num12 <= num18)
						{
							stationComponent4.SetPriorityLock(ptr.supplyIndex, priorityIndex);
							goto IL_CD8;
						}
						goto IL_CD8;
					}
				}
				IL_D01:
				if (num15 == this.remotePairProcesses[priorityIndex])
				{
					goto IL_D11;
				}
				continue;
				IL_CD8:
				this.remotePairProcesses[priorityIndex]++;
				if (this.remotePairProcesses[priorityIndex] == num13)
				{
					this.remotePairProcesses[priorityIndex] = 0;
					goto IL_D01;
				}
				goto IL_D01;
			}
			this.SetPriorityLock(ptr.supplyIndex, priorityIndex);
			stationComponent.SetPriorityLock(ptr.demandIndex, priorityIndex);
			goto IL_D11;
			Block_54:
			long num22 = this.CalcTripEnergyCost(num21, shipSailSpeed, flag);
			if (!stationComponent3.isCollector && !stationComponent3.isVeinCollector)
			{
				bool flag8 = false;
				this.remotePairProcesses[priorityIndex] = this.remotePairProcesses[priorityIndex] % num13;
				int num23 = this.remotePairProcesses[priorityIndex] + 1;
				int num24 = this.remotePairProcesses[priorityIndex];
				num23 %= num13;
				SupplyDemandPair supplyDemandPair;
				for (;;)
				{
					supplyDemandPair = this.remotePairs[num23 + num14];
					if (supplyDemandPair.supplyId != this.gid || supplyDemandPair.demandId != stationComponent3.gid)
					{
						goto IL_9D7;
					}
					if ((int)this.priorityLocks[ptr.supplyIndex].priorityIndex < priorityIndex && this.priorityLocks[ptr.supplyIndex].lockTick > 0)
					{
						num23++;
						num23 %= num13;
					}
					else
					{
						if ((int)stationComponent3.priorityLocks[ptr.demandIndex].priorityIndex >= priorityIndex || stationComponent3.priorityLocks[ptr.demandIndex].lockTick <= 0)
						{
							StationStore[] obj = this.storage;
							lock (obj)
							{
								num2 = this.storage[supplyDemandPair.supplyIndex].count;
								storagePairSupplyInc = this.storage[supplyDemandPair.supplyIndex].inc;
								num3 = this.storage[supplyDemandPair.supplyIndex].remoteSupplyCount;
								num4 = this.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
								storagePairSupplyItemId = this.storage[supplyDemandPair.supplyIndex].itemId;
							}
							goto IL_9D7;
						}
						num23++;
						num23 %= num13;
					}
					IL_B61:
					if (num24 == num23)
					{
						goto IL_B6A;
					}
					continue;
					IL_9D7:
					if (supplyDemandPair.supplyId == this.gid && supplyDemandPair.demandId == stationComponent3.gid)
					{
						StationStore[] obj = stationComponent3.storage;
						lock (obj)
						{
							num7 = stationComponent3.storage[supplyDemandPair.demandIndex].remoteDemandCount;
							num8 = stationComponent3.storage[supplyDemandPair.demandIndex].totalDemandCount;
						}
					}
					if (supplyDemandPair.supplyId == this.gid && supplyDemandPair.demandId == stationComponent3.gid)
					{
						int num18;
						if (num2 >= num18 && num3 >= num18 && num4 >= num18)
						{
							if (num7 > 0 && num8 > 0)
							{
								break;
							}
							stationComponent3.SetPriorityLock(supplyDemandPair.demandIndex, priorityIndex);
						}
						else if (num7 <= 0 || num8 <= 0)
						{
							stationComponent3.SetPriorityLock(supplyDemandPair.demandIndex, priorityIndex);
						}
					}
					num23++;
					num23 %= num13;
					goto IL_B61;
				}
				if (this.energy >= num22)
				{
					int carryCnt2 = (shipCarries < num2) ? shipCarries : num2;
					if (this.DispatchSupplyShip(carryCnt2, num2, storagePairSupplyInc, storagePairSupplyItemId, flag7, stationComponent3, consumeRegister, ref supplyDemandPair, tstat))
					{
						this.energy -= num22;
						flag8 = true;
						this.SetPriorityLock(supplyDemandPair.supplyIndex, priorityIndex);
						stationComponent3.SetPriorityLock(supplyDemandPair.demandIndex, priorityIndex);
					}
				}
				else if (num17 > 0.1f)
				{
					this.SetPriorityLock(supplyDemandPair.supplyIndex, priorityIndex);
					stationComponent3.SetPriorityLock(supplyDemandPair.demandIndex, priorityIndex);
				}
				IL_B6A:
				if (flag8)
				{
					goto IL_D11;
				}
			}
			bool flag9;
			if (this.energy >= num22)
			{
				if (this.DispatchDemandShip(shipCarries, storagePairDemandItemId, flag7, stationComponent3, consumeRegister, ref ptr))
				{
					this.energy -= num22;
				}
				flag9 = true;
			}
			else
			{
				flag9 = (num17 > 0.1f);
			}
			if (flag9)
			{
				this.SetPriorityLock(ptr.demandIndex, priorityIndex);
				stationComponent3.SetPriorityLock(ptr.supplyIndex, priorityIndex);
			}
			IL_D11:
			this.remotePairProcesses[priorityIndex]++;
			if (this.remotePairProcesses[priorityIndex] == num13)
			{
				this.remotePairProcesses[priorityIndex] = 0;
			}
		}
	}

	// Token: 0x06000A48 RID: 2632 RVA: 0x00099A18 File Offset: 0x00097C18
	public static bool DetermineFramingDispatchTime(long time, int priorityIndex)
	{
		if (priorityIndex == 1)
		{
			return time % 10L == 0L;
		}
		if (priorityIndex == 2)
		{
			return time % 30L == 0L;
		}
		if (priorityIndex == 3)
		{
			return time % 30L == 0L;
		}
		return time % 60L == 0L;
	}

	// Token: 0x06000A49 RID: 2633 RVA: 0x00099A50 File Offset: 0x00097C50
	private void SetPriorityLock(int setIndex, int priorityIndex)
	{
		if (priorityIndex == 0 || priorityIndex > 4)
		{
			return;
		}
		if (this.priorityLocks[setIndex].priorityIndex != 0 && (int)this.priorityLocks[setIndex].priorityIndex < priorityIndex)
		{
			return;
		}
		byte lockTick;
		if (priorityIndex == 1)
		{
			lockTick = 10;
		}
		else if (priorityIndex == 2)
		{
			lockTick = 30;
		}
		else if (priorityIndex == 3)
		{
			lockTick = 30;
		}
		else
		{
			lockTick = 60;
		}
		StationPriorityLock[] obj = this.priorityLocks;
		lock (obj)
		{
			this.priorityLocks[setIndex].priorityIndex = (byte)priorityIndex;
			this.priorityLocks[setIndex].lockTick = lockTick;
		}
	}

	// Token: 0x06000A4A RID: 2634 RVA: 0x00099B00 File Offset: 0x00097D00
	private bool DispatchSupplyShip(int carryCnt, int storagePairSupplyCount, int storagePairSupplyInc, int storagePairSupplyItemId, bool takeWarper, StationComponent other, int[] consumeRegister, ref SupplyDemandPair pair, TrafficStatistics tstat)
	{
		int num = storagePairSupplyCount;
		int num2 = storagePairSupplyInc;
		int num3 = this.split_inc(ref num, ref num2, carryCnt);
		int num4 = this.QueryIdleShip(this.nextShipIndex);
		if (num4 >= 0)
		{
			this.nextShipIndex = (num4 + 1) % this.workShipDatas.Length;
			this.workShipDatas[this.workShipCount].stage = -2;
			this.workShipDatas[this.workShipCount].planetA = this.planetId;
			this.workShipDatas[this.workShipCount].planetB = other.planetId;
			this.workShipDatas[this.workShipCount].otherGId = other.gid;
			this.workShipDatas[this.workShipCount].direction = 1;
			this.workShipDatas[this.workShipCount].t = 0f;
			ShipData[] array = this.workShipDatas;
			int num5 = this.workShipCount;
			this.workShipOrders[this.workShipCount].itemId = storagePairSupplyItemId;
			array[num5].itemId = storagePairSupplyItemId;
			this.workShipDatas[this.workShipCount].itemCount = carryCnt;
			this.workShipDatas[this.workShipCount].inc = num3;
			this.workShipDatas[this.workShipCount].gene = this._tmp_iter_remote;
			this.workShipDatas[this.workShipCount].shipIndex = num4;
			this.workShipOrders[this.workShipCount].otherStationGId = other.gid;
			this.workShipOrders[this.workShipCount].thisIndex = pair.supplyIndex;
			this.workShipOrders[this.workShipCount].otherIndex = pair.demandIndex;
			this.workShipOrders[this.workShipCount].thisOrdered = 0;
			this.workShipOrders[this.workShipCount].otherOrdered = carryCnt;
			if (takeWarper)
			{
				lock (consumeRegister)
				{
					if (this.warperCount >= 2)
					{
						ShipData[] array2 = this.workShipDatas;
						int num6 = this.workShipCount;
						array2[num6].warperCnt = array2[num6].warperCnt + 2;
						this.warperCount -= 2;
						consumeRegister[1210] += 2;
					}
					else if (this.warperCount >= 1)
					{
						ShipData[] array3 = this.workShipDatas;
						int num7 = this.workShipCount;
						array3[num7].warperCnt = array3[num7].warperCnt + 1;
						this.warperCount--;
						consumeRegister[1210]++;
					}
					else if (this.warperFree)
					{
						ShipData[] array4 = this.workShipDatas;
						int num8 = this.workShipCount;
						array4[num8].warperCnt = array4[num8].warperCnt + 2;
					}
				}
			}
			StationStore[] obj = other.storage;
			lock (obj)
			{
				StationStore[] array5 = other.storage;
				int demandIndex = pair.demandIndex;
				array5[demandIndex].remoteOrder = array5[demandIndex].remoteOrder + carryCnt;
			}
			this.workShipCount++;
			this.idleShipCount--;
			this.IdleShipGetToWork(num4);
			obj = this.storage;
			lock (obj)
			{
				StationStore[] array6 = this.storage;
				int supplyIndex = pair.supplyIndex;
				array6[supplyIndex].count = array6[supplyIndex].count - carryCnt;
				StationStore[] array7 = this.storage;
				int supplyIndex2 = pair.supplyIndex;
				array7[supplyIndex2].inc = array7[supplyIndex2].inc - num3;
				tstat.RegisterPlanetOutputStat(this.planetId, this.storage[pair.supplyIndex].itemId, carryCnt);
				int num9 = other.planetId / 100;
				int num10 = this.planetId / 100;
				if (num9 != num10)
				{
					tstat.RegisterStarOutputStat(num10, this.storage[pair.supplyIndex].itemId, carryCnt);
				}
				else
				{
					tstat.RegisterStarInternalStat(num10, this.storage[pair.supplyIndex].itemId, carryCnt);
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000A4B RID: 2635 RVA: 0x00099F38 File Offset: 0x00098138
	private bool DispatchDemandShip(int shipCarries, int storagePairDemandItemId, bool takeWarper, StationComponent other, int[] consumeRegister, ref SupplyDemandPair pair)
	{
		int num = this.QueryIdleShip(this.nextShipIndex);
		if (num >= 0)
		{
			StationStore[] obj = this.storage;
			lock (obj)
			{
				storagePairDemandItemId = this.storage[pair.demandIndex].itemId;
			}
			this.nextShipIndex = (num + 1) % this.workShipDatas.Length;
			this.workShipDatas[this.workShipCount].stage = -2;
			this.workShipDatas[this.workShipCount].planetA = this.planetId;
			this.workShipDatas[this.workShipCount].planetB = other.planetId;
			this.workShipDatas[this.workShipCount].otherGId = other.gid;
			this.workShipDatas[this.workShipCount].direction = 1;
			this.workShipDatas[this.workShipCount].t = 0f;
			this.workShipDatas[this.workShipCount].itemId = (this.workShipOrders[this.workShipCount].itemId = storagePairDemandItemId);
			this.workShipDatas[this.workShipCount].itemCount = 0;
			this.workShipDatas[this.workShipCount].inc = 0;
			this.workShipDatas[this.workShipCount].gene = this._tmp_iter_remote;
			this.workShipDatas[this.workShipCount].shipIndex = num;
			this.workShipOrders[this.workShipCount].otherStationGId = other.gid;
			this.workShipOrders[this.workShipCount].thisIndex = pair.demandIndex;
			this.workShipOrders[this.workShipCount].otherIndex = pair.supplyIndex;
			this.workShipOrders[this.workShipCount].thisOrdered = shipCarries;
			this.workShipOrders[this.workShipCount].otherOrdered = -shipCarries;
			if (takeWarper)
			{
				lock (consumeRegister)
				{
					if (this.warperCount >= 2)
					{
						ShipData[] array = this.workShipDatas;
						int num2 = this.workShipCount;
						array[num2].warperCnt = array[num2].warperCnt + 2;
						this.warperCount -= 2;
						consumeRegister[1210] += 2;
					}
					else if (this.warperCount >= 1)
					{
						ShipData[] array2 = this.workShipDatas;
						int num3 = this.workShipCount;
						array2[num3].warperCnt = array2[num3].warperCnt + 1;
						this.warperCount--;
						consumeRegister[1210]++;
					}
					else if (this.warperFree)
					{
						ShipData[] array3 = this.workShipDatas;
						int num4 = this.workShipCount;
						array3[num4].warperCnt = array3[num4].warperCnt + 2;
					}
				}
			}
			obj = this.storage;
			lock (obj)
			{
				StationStore[] array4 = this.storage;
				int demandIndex = pair.demandIndex;
				array4[demandIndex].remoteOrder = array4[demandIndex].remoteOrder + shipCarries;
			}
			obj = other.storage;
			lock (obj)
			{
				StationStore[] array5 = other.storage;
				int supplyIndex = pair.supplyIndex;
				array5[supplyIndex].remoteOrder = array5[supplyIndex].remoteOrder - shipCarries;
			}
			this.workShipCount++;
			this.idleShipCount--;
			this.IdleShipGetToWork(num);
			return true;
		}
		return false;
	}

	// Token: 0x06000A4C RID: 2636 RVA: 0x0009A2F8 File Offset: 0x000984F8
	public long CalcTripEnergyCost(double trip, float maxSpeed, bool canWarp)
	{
		double num = trip * 0.03 + 100.0;
		if (num > (double)maxSpeed)
		{
			num = (double)maxSpeed;
		}
		if (num > 3000.0)
		{
			num = 3000.0;
		}
		double num2 = num * 200000.0;
		if (canWarp && trip > this.warpEnableDist)
		{
			num2 += 100000000.0;
		}
		return (long)(6000000.0 + trip * 30.0 + num2);
	}

	// Token: 0x06000A4D RID: 2637 RVA: 0x0009A378 File Offset: 0x00098578
	public void UpdateKeepMode()
	{
		StationStore[] obj = this.storage;
		lock (obj)
		{
			int num = this.storage.Length;
			if (0 < num && this.storage[0].keepMode > 0 && this.storage[0].itemId > 0)
			{
				this.storage[0].count = ((this.storage[0].keepMode > 2) ? 0 : (this.storage[0].max / this.storage[0].keepMode));
				this.storage[0].inc = ((this.storage[0].keepMode > 2) ? 0 : ((int)((float)this.storage[0].count * this.storage[0].keepIncRatio + 1E-05f)));
			}
			if (1 < num && this.storage[1].keepMode > 0 && this.storage[1].itemId > 0)
			{
				this.storage[1].count = ((this.storage[1].keepMode > 2) ? 0 : (this.storage[1].max / this.storage[1].keepMode));
				this.storage[1].inc = ((this.storage[1].keepMode > 2) ? 0 : ((int)((float)this.storage[1].count * this.storage[1].keepIncRatio + 1E-05f)));
			}
			if (2 < num && this.storage[2].keepMode > 0 && this.storage[2].itemId > 0)
			{
				this.storage[2].count = ((this.storage[2].keepMode > 2) ? 0 : (this.storage[2].max / this.storage[2].keepMode));
				this.storage[2].inc = ((this.storage[2].keepMode > 2) ? 0 : ((int)((float)this.storage[2].count * this.storage[2].keepIncRatio + 1E-05f)));
			}
			if (3 < num && this.storage[3].keepMode > 0 && this.storage[3].itemId > 0)
			{
				this.storage[3].count = ((this.storage[3].keepMode > 2) ? 0 : (this.storage[3].max / this.storage[3].keepMode));
				this.storage[3].inc = ((this.storage[3].keepMode > 2) ? 0 : ((int)((float)this.storage[3].count * this.storage[3].keepIncRatio + 1E-05f)));
			}
			if (4 < num && this.storage[4].keepMode > 0 && this.storage[4].itemId > 0)
			{
				this.storage[4].count = ((this.storage[4].keepMode > 2) ? 0 : (this.storage[4].max / this.storage[4].keepMode));
				this.storage[4].inc = ((this.storage[4].keepMode > 2) ? 0 : ((int)((float)this.storage[4].count * this.storage[4].keepIncRatio + 1E-05f)));
			}
			if (this.droneAutoReplenish)
			{
				this.idleDroneCount = this.workDroneDatas.Length - this.workDroneCount;
			}
			if (this.shipAutoReplenish)
			{
				this.idleShipCount = this.workShipDatas.Length - this.workShipCount;
			}
		}
	}

	// Token: 0x06000A4E RID: 2638 RVA: 0x0009A820 File Offset: 0x00098A20
	public void UpdateNeeds()
	{
		StationStore[] obj = this.storage;
		lock (obj)
		{
			int num = this.storage.Length;
			this.needs[0] = ((0 < num && this.storage[0].count < this.storage[0].max) ? this.storage[0].itemId : 0);
			this.needs[1] = ((1 < num && this.storage[1].count < this.storage[1].max) ? this.storage[1].itemId : 0);
			this.needs[2] = ((2 < num && this.storage[2].count < this.storage[2].max) ? this.storage[2].itemId : 0);
			this.needs[3] = ((3 < num && this.storage[3].count < this.storage[3].max) ? this.storage[3].itemId : 0);
			this.needs[4] = ((4 < num && this.storage[4].count < this.storage[4].max) ? this.storage[4].itemId : 0);
			this.needs[5] = ((this.isStellar && this.warperCount < this.warperMaxCount) ? 1210 : 0);
		}
	}

	// Token: 0x06000A4F RID: 2639 RVA: 0x0009A9EC File Offset: 0x00098BEC
	public void UpdateCollection(PlanetFactory factory, float collectSpeedRate, int[] productRegister)
	{
		if (this.collectionPerTick == null)
		{
			return;
		}
		for (int i = 0; i < this.collectionIds.Length; i++)
		{
			StationStore[] obj = this.storage;
			lock (obj)
			{
				if (this.storage[i].count < this.storage[i].max)
				{
					this.currentCollections[i] += this.collectionPerTick[i] * collectSpeedRate;
					int num = (int)this.currentCollections[i];
					if (num != 0)
					{
						lock (productRegister)
						{
							StationStore[] array = this.storage;
							int num2 = i;
							array[num2].count = array[num2].count + num;
							productRegister[this.storage[i].itemId] += num;
							this.currentCollections[i] -= (float)num;
							factory.AddMiningFlagUnsafe(LDB.veins.GetVeinTypeByItemId(this.storage[i].itemId));
						}
					}
				}
			}
		}
	}

	// Token: 0x06000A50 RID: 2640 RVA: 0x0009AB2C File Offset: 0x00098D2C
	public void UpdateVeinCollection(PlanetFactory factory, int[] productRegister)
	{
		StationStore[] obj = this.storage;
		lock (obj)
		{
			if (this.storage[0].localSupplyCount < this.storage[0].max)
			{
				MinerComponent[] minerPool = factory.factorySystem.minerPool;
				if (minerPool[this.minerId].productId != 0 && minerPool[this.minerId].productId == this.collectionIds[0] && minerPool[this.minerId].productCount > 0)
				{
					int productCount = minerPool[this.minerId].productCount;
					int num = this.storage[0].count;
					int max = this.storage[0].max;
					if (this.storage[0].localOrder < -max / 2)
					{
						if (this.storage[0].localOrder < -max / 2 - max)
						{
							num -= this.storage[0].max;
						}
						else
						{
							num += this.storage[0].localOrder + max / 2;
						}
					}
					int num2 = this.storage[0].max - num;
					num2 = ((num2 > productCount) ? productCount : num2);
					if (num2 > 0)
					{
						StationStore[] array = this.storage;
						int num3 = 0;
						array[num3].count = array[num3].count + num2;
						MinerComponent[] array2 = minerPool;
						int num4 = this.minerId;
						array2[num4].productCount = array2[num4].productCount - num2;
						if (minerPool[this.minerId].productCount == 0)
						{
							minerPool[this.minerId].productId = 0;
						}
						factory.AddMiningFlagUnsafe(LDB.veins.GetVeinTypeByItemId(this.storage[0].itemId));
					}
				}
			}
		}
	}

	// Token: 0x06000A51 RID: 2641 RVA: 0x0009AD2C File Offset: 0x00098F2C
	public void Export(BinaryWriter w)
	{
		w.Write(8);
		w.Write(this.id);
		w.Write(this.gid);
		w.Write(this.entityId);
		w.Write(this.planetId);
		w.Write(this.pcId);
		w.Write(this.minerId);
		w.Write(this.gene);
		w.Write(this.droneDock.x);
		w.Write(this.droneDock.y);
		w.Write(this.droneDock.z);
		w.Write(this.shipDockPos.x);
		w.Write(this.shipDockPos.y);
		w.Write(this.shipDockPos.z);
		w.Write(this.shipDockRot.x);
		w.Write(this.shipDockRot.y);
		w.Write(this.shipDockRot.z);
		w.Write(this.shipDockRot.w);
		w.Write(this.isStellar);
		w.Write(this.energy);
		w.Write(this.energyPerTick);
		w.Write(this.energyMax);
		w.Write(this.warperCount);
		w.Write(this.warperMaxCount);
		w.Write(this.idleDroneCount);
		w.Write(this.workDroneCount);
		w.Write(this.workDroneDatas.Length);
		for (int i = 0; i < this.workDroneCount; i++)
		{
			this.workDroneDatas[i].Export(w);
		}
		for (int j = 0; j < this.workDroneCount; j++)
		{
			this.workDroneOrders[j].Export(w);
		}
		w.Write(this.idleShipCount);
		w.Write(this.workShipCount);
		w.Write(this.idleShipIndices);
		w.Write(this.workShipIndices);
		w.Write(this.workShipDatas.Length);
		for (int k = 0; k < this.workShipCount; k++)
		{
			this.workShipDatas[k].Export(w);
		}
		for (int l = 0; l < this.workShipCount; l++)
		{
			this.workShipOrders[l].Export(w);
		}
		w.Write(this.storage.Length);
		for (int m = 0; m < this.storage.Length; m++)
		{
			this.storage[m].Export(w);
		}
		w.Write(this.priorityLocks.Length);
		for (int n = 0; n < this.priorityLocks.Length; n++)
		{
			this.priorityLocks[n].Export(w);
		}
		w.Write(this.slots.Length);
		for (int num = 0; num < this.slots.Length; num++)
		{
			w.Write((int)this.slots[num].dir);
			w.Write(this.slots[num].beltId);
			w.Write(this.slots[num].storageIdx);
			w.Write(this.slots[num].counter);
		}
		w.Write(this.localPairProcess);
		w.Write(this.remotePairProcesses.Length);
		for (int num2 = 0; num2 < this.remotePairProcesses.Length; num2++)
		{
			w.Write(this.remotePairProcesses[num2]);
		}
		w.Write(this.nextShipIndex);
		w.Write(this.isCollector);
		w.Write(this.isVeinCollector);
		int num3 = 0;
		if (this.collectionIds != null)
		{
			num3 = this.collectionIds.Length;
		}
		w.Write(num3);
		for (int num4 = 0; num4 < num3; num4++)
		{
			w.Write(this.collectionIds[num4]);
		}
		if (this.collectionPerTick != null)
		{
			num3 = this.collectionPerTick.Length;
		}
		w.Write(num3);
		for (int num5 = 0; num5 < num3; num5++)
		{
			w.Write(this.collectionPerTick[num5]);
		}
		if (this.currentCollections != null)
		{
			num3 = this.currentCollections.Length;
		}
		w.Write(num3);
		for (int num6 = 0; num6 < num3; num6++)
		{
			w.Write(this.currentCollections[num6]);
		}
		w.Write(this.collectSpeed);
		num3 = this.droneDispatchStatus.Length;
		w.Write((byte)num3);
		w.Write(this.droneDispatchStatus, 0, num3);
		w.Write((byte)this.droneStatusCursor);
		w.Write((byte)this.droneTaskInterval);
		w.Write(this.tripRangeDrones);
		w.Write(this.tripRangeShips);
		w.Write(this.includeOrbitCollector);
		w.Write(this.warpEnableDist);
		w.Write(this.warperNecessary);
		w.Write(this.deliveryDrones);
		w.Write(this.deliveryShips);
		w.Write(this.pilerCount);
		w.Write(this.droneAutoReplenish);
		w.Write(this.shipAutoReplenish);
		w.Write(this.remoteGroupMask);
		w.Write((int)this.routePriority);
	}

	// Token: 0x06000A52 RID: 2642 RVA: 0x0009B250 File Offset: 0x00099450
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.id = r.ReadInt32();
		this.gid = r.ReadInt32();
		this.entityId = r.ReadInt32();
		this.planetId = r.ReadInt32();
		this.pcId = r.ReadInt32();
		if (num >= 3)
		{
			this.minerId = r.ReadInt32();
		}
		else
		{
			this.minerId = 0;
		}
		this.gene = r.ReadInt32();
		this.droneDock.x = r.ReadSingle();
		this.droneDock.y = r.ReadSingle();
		this.droneDock.z = r.ReadSingle();
		this.shipDockPos.x = r.ReadSingle();
		this.shipDockPos.y = r.ReadSingle();
		this.shipDockPos.z = r.ReadSingle();
		this.shipDockRot.x = r.ReadSingle();
		this.shipDockRot.y = r.ReadSingle();
		this.shipDockRot.z = r.ReadSingle();
		this.shipDockRot.w = r.ReadSingle();
		this.isStellar = r.ReadBoolean();
		if (num < 6 && r.ReadInt32() > 0)
		{
			r.ReadString();
		}
		this.energy = r.ReadInt64();
		this.energyPerTick = r.ReadInt64();
		this.energyMax = r.ReadInt64();
		this.warperCount = r.ReadInt32();
		this.warperMaxCount = r.ReadInt32();
		this.idleDroneCount = r.ReadInt32();
		this.workDroneCount = r.ReadInt32();
		int num2 = r.ReadInt32();
		this.workDroneDatas = new DroneData[num2];
		this.workDroneOrders = new LocalLogisticOrder[num2];
		for (int i = 0; i < this.workDroneCount; i++)
		{
			this.workDroneDatas[i].Import(r);
		}
		for (int j = 0; j < this.workDroneCount; j++)
		{
			this.workDroneOrders[j].Import(r);
		}
		this.idleShipCount = r.ReadInt32();
		this.workShipCount = r.ReadInt32();
		this.idleShipIndices = r.ReadUInt64();
		this.workShipIndices = r.ReadUInt64();
		num2 = r.ReadInt32();
		this.workShipDatas = new ShipData[num2];
		this.shipRenderers = new ShipRenderingData[num2];
		this.shipUIRenderers = new ShipUIRenderingData[num2];
		this.workShipOrders = new RemoteLogisticOrder[num2];
		for (int k = 0; k < this.workShipCount; k++)
		{
			this.workShipDatas[k].Import(r);
		}
		for (int l = 0; l < this.workShipCount; l++)
		{
			this.workShipOrders[l].Import(r);
		}
		num2 = r.ReadInt32();
		this.storage = new StationStore[num2];
		for (int m = 0; m < num2; m++)
		{
			this.storage[m].Import(r);
		}
		if (num >= 7)
		{
			num2 = r.ReadInt32();
			this.priorityLocks = new StationPriorityLock[num2];
			for (int n = 0; n < num2; n++)
			{
				this.priorityLocks[n].Import(r);
			}
		}
		else
		{
			this.priorityLocks = new StationPriorityLock[this.storage.Length];
		}
		if (num >= 1)
		{
			num2 = r.ReadInt32();
			this.slots = new SlotData[num2];
			for (int num3 = 0; num3 < num2; num3++)
			{
				this.slots[num3].dir = (IODir)r.ReadInt32();
				this.slots[num3].beltId = r.ReadInt32();
				this.slots[num3].storageIdx = r.ReadInt32();
				this.slots[num3].counter = r.ReadInt32();
			}
		}
		else
		{
			this.slots = new SlotData[12];
		}
		this.localPairProcess = r.ReadInt32();
		if (num >= 7)
		{
			num2 = r.ReadInt32();
			this.remotePairProcesses = new int[num2];
			for (int num4 = 0; num4 < num2; num4++)
			{
				this.remotePairProcesses[num4] = r.ReadInt32();
			}
		}
		else
		{
			this.remotePairProcesses = new int[6];
			this.remotePairProcesses[0] = r.ReadInt32();
		}
		this.nextShipIndex = r.ReadInt32();
		this.needs = new int[6];
		this.localPairs = null;
		this.localPairCount = 0;
		this.shipDiskPos = new Vector3[this.workShipDatas.Length];
		this.shipDiskRot = new Quaternion[this.workShipDatas.Length];
		if (this.isStellar)
		{
			int num5 = this.workShipDatas.Length;
			for (int num6 = 0; num6 < num5; num6++)
			{
				this.shipDiskRot[num6] = Quaternion.Euler(0f, 360f / (float)num5 * (float)num6, 0f);
				this.shipDiskPos[num6] = this.shipDiskRot[num6] * new Vector3(0f, 0f, 11.5f);
			}
			for (int num7 = 0; num7 < num5; num7++)
			{
				this.shipDiskRot[num7] = this.shipDockRot * this.shipDiskRot[num7];
				this.shipDiskPos[num7] = this.shipDockPos + this.shipDockRot * this.shipDiskPos[num7];
			}
		}
		this.isCollector = r.ReadBoolean();
		if (num >= 3)
		{
			this.isVeinCollector = r.ReadBoolean();
		}
		else
		{
			this.isVeinCollector = false;
		}
		num2 = r.ReadInt32();
		if (num2 != 0)
		{
			this.collectionIds = new int[num2];
			for (int num8 = 0; num8 < num2; num8++)
			{
				this.collectionIds[num8] = r.ReadInt32();
			}
		}
		num2 = r.ReadInt32();
		if (num2 != 0)
		{
			this.collectionPerTick = new float[num2];
			for (int num9 = 0; num9 < num2; num9++)
			{
				this.collectionPerTick[num9] = r.ReadSingle();
			}
		}
		num2 = r.ReadInt32();
		if (num2 != 0)
		{
			this.currentCollections = new float[num2];
			for (int num10 = 0; num10 < num2; num10++)
			{
				this.currentCollections[num10] = r.ReadSingle();
			}
		}
		this.collectSpeed = r.ReadInt32();
		if (num >= 8)
		{
			num2 = (int)r.ReadByte();
			if (num2 > 0)
			{
				this.droneDispatchStatus = r.ReadBytes(num2);
			}
			if (num2 < 30)
			{
				this.droneDispatchStatus = new byte[30];
			}
			this.droneStatusCursor = (int)r.ReadByte();
			this.droneTaskInterval = (int)r.ReadByte();
			if (this.droneStatusCursor < 0)
			{
				this.droneStatusCursor = 0;
			}
			else if (this.droneStatusCursor >= num2)
			{
				this.droneStatusCursor = num2 - 1;
			}
			if (this.droneTaskInterval < 1)
			{
				this.droneTaskInterval = 20;
			}
			else if (this.droneTaskInterval > 20)
			{
				this.droneTaskInterval = 20;
			}
		}
		else
		{
			this.droneDispatchStatus = new byte[30];
			this.droneStatusCursor = 0;
			this.droneTaskInterval = 20;
		}
		if (num < 2)
		{
			this.InitSettings();
			return;
		}
		this.tripRangeDrones = r.ReadDouble();
		this.tripRangeShips = r.ReadDouble();
		this.includeOrbitCollector = r.ReadBoolean();
		this.warpEnableDist = r.ReadDouble();
		this.warperNecessary = r.ReadBoolean();
		this.deliveryDrones = r.ReadInt32();
		this.deliveryShips = r.ReadInt32();
		if (num >= 4)
		{
			this.pilerCount = r.ReadInt32();
		}
		else
		{
			this.pilerCount = 0;
		}
		if (num >= 5)
		{
			this.droneAutoReplenish = r.ReadBoolean();
			this.shipAutoReplenish = r.ReadBoolean();
		}
		else
		{
			this.droneAutoReplenish = false;
			this.shipAutoReplenish = false;
		}
		if (num >= 7)
		{
			this.remoteGroupMask = r.ReadInt64();
			this.routePriority = (ERemoteRoutePriority)r.ReadInt32();
			return;
		}
		this.remoteGroupMask = 0L;
		this.routePriority = (this.isStellar ? ERemoteRoutePriority.Prioritize : ERemoteRoutePriority.Ignore);
	}

	// Token: 0x06000A53 RID: 2643 RVA: 0x0009BA10 File Offset: 0x00099C10
	public int AddItem(int itemId, int count, int inc)
	{
		if (itemId <= 0)
		{
			return 0;
		}
		StationStore[] obj = this.storage;
		lock (obj)
		{
			int num = this.storage.Length;
			if (0 < num && this.storage[0].itemId == itemId)
			{
				StationStore[] array = this.storage;
				int num2 = 0;
				array[num2].count = array[num2].count + count;
				StationStore[] array2 = this.storage;
				int num3 = 0;
				array2[num3].inc = array2[num3].inc + inc;
				return count;
			}
			if (1 < num && this.storage[1].itemId == itemId)
			{
				StationStore[] array3 = this.storage;
				int num4 = 1;
				array3[num4].count = array3[num4].count + count;
				StationStore[] array4 = this.storage;
				int num5 = 1;
				array4[num5].inc = array4[num5].inc + inc;
				return count;
			}
			if (2 < num && this.storage[2].itemId == itemId)
			{
				StationStore[] array5 = this.storage;
				int num6 = 2;
				array5[num6].count = array5[num6].count + count;
				StationStore[] array6 = this.storage;
				int num7 = 2;
				array6[num7].inc = array6[num7].inc + inc;
				return count;
			}
			if (3 < num && this.storage[3].itemId == itemId)
			{
				StationStore[] array7 = this.storage;
				int num8 = 3;
				array7[num8].count = array7[num8].count + count;
				StationStore[] array8 = this.storage;
				int num9 = 3;
				array8[num9].inc = array8[num9].inc + inc;
				return count;
			}
			if (4 < num && this.storage[4].itemId == itemId)
			{
				StationStore[] array9 = this.storage;
				int num10 = 4;
				array9[num10].count = array9[num10].count + count;
				StationStore[] array10 = this.storage;
				int num11 = 4;
				array10[num11].inc = array10[num11].inc + inc;
				return count;
			}
			if (5 < num && this.storage[5].itemId == itemId)
			{
				StationStore[] array11 = this.storage;
				int num12 = 5;
				array11[num12].count = array11[num12].count + count;
				StationStore[] array12 = this.storage;
				int num13 = 5;
				array12[num13].inc = array12[num13].inc + inc;
				return count;
			}
		}
		return 0;
	}

	// Token: 0x06000A54 RID: 2644 RVA: 0x0009BC24 File Offset: 0x00099E24
	public void InputItem(int itemId, int needIdx, int stack, int inc)
	{
		if (itemId <= 0)
		{
			return;
		}
		StationStore[] obj = this.storage;
		lock (obj)
		{
			if (needIdx < this.storage.Length && this.storage[needIdx].itemId == itemId)
			{
				StationStore[] array = this.storage;
				array[needIdx].count = array[needIdx].count + stack;
				StationStore[] array2 = this.storage;
				array2[needIdx].inc = array2[needIdx].inc + inc;
			}
			else if (itemId == 1210)
			{
				this.warperCount += stack;
			}
		}
	}

	// Token: 0x06000A55 RID: 2645 RVA: 0x0009BCC8 File Offset: 0x00099EC8
	public void TakeItem(ref int itemId, ref int count, out int inc)
	{
		inc = 0;
		if (itemId > 0 && count > 0)
		{
			int num = this.storage.Length;
			for (int i = 0; i < num; i++)
			{
				StationStore[] obj = this.storage;
				lock (obj)
				{
					if (this.storage[i].itemId == itemId && this.storage[i].count > 0)
					{
						count = ((count < this.storage[i].count) ? count : this.storage[i].count);
						itemId = this.storage[i].itemId;
						inc = this.split_inc(ref this.storage[i].count, ref this.storage[i].inc, count);
						return;
					}
				}
			}
		}
		itemId = 0;
		count = 0;
		inc = 0;
	}

	// Token: 0x06000A56 RID: 2646 RVA: 0x0009BDD4 File Offset: 0x00099FD4
	public void TakeItem(ref int _itemId, ref int _count, int[] _needs, out int _inc)
	{
		_inc = 0;
		if (_itemId > 0 && _count > 0 && (_needs == null || _needs[0] == _itemId || _needs[1] == _itemId || _needs[2] == _itemId || _needs[3] == _itemId || _needs[4] == _itemId || _needs[5] == _itemId))
		{
			StationStore[] obj = this.storage;
			lock (obj)
			{
				int num = this.storage.Length;
				for (int i = 0; i < num; i++)
				{
					if (this.storage[i].itemId == _itemId && this.storage[i].count > 0)
					{
						_count = ((_count < this.storage[i].count) ? _count : this.storage[i].count);
						_itemId = this.storage[i].itemId;
						_inc = this.split_inc(ref this.storage[i].count, ref this.storage[i].inc, _count);
						return;
					}
				}
			}
		}
		_itemId = 0;
		_count = 0;
		_inc = 0;
	}

	// Token: 0x06000A57 RID: 2647 RVA: 0x0009BF14 File Offset: 0x0009A114
	private int split_inc(ref int n, ref int m, int p)
	{
		if (n == 0)
		{
			return 0;
		}
		int num = m / n;
		int num2 = m - num * n;
		n -= p;
		num2 -= n;
		num = ((num2 > 0) ? (num * p + num2) : (num * p));
		m -= num;
		return num;
	}

	// Token: 0x06000A58 RID: 2648 RVA: 0x0009BF58 File Offset: 0x0009A158
	public static void lpos2upos_ref(ref VectorLF3 _upos, ref Quaternion _urot, ref Vector3 _lpos, ref VectorLF3 _result)
	{
		double num = 2.0 * (double)_lpos.x;
		double num2 = 2.0 * (double)_lpos.y;
		double num3 = 2.0 * (double)_lpos.z;
		double num4 = (double)(_urot.w * _urot.w) - 0.5;
		double num5 = (double)_urot.x * num + (double)_urot.y * num2 + (double)_urot.z * num3;
		_result.x = num * num4 + ((double)_urot.y * num3 - (double)_urot.z * num2) * (double)_urot.w + (double)_urot.x * num5 + _upos.x;
		_result.y = num2 * num4 + ((double)_urot.z * num - (double)_urot.x * num3) * (double)_urot.w + (double)_urot.y * num5 + _upos.y;
		_result.z = num3 * num4 + ((double)_urot.x * num2 - (double)_urot.y * num) * (double)_urot.w + (double)_urot.z * num5 + _upos.z;
	}

	// Token: 0x06000A59 RID: 2649 RVA: 0x0009C078 File Offset: 0x0009A278
	public static void lpos2upos_ref(ref VectorLF3 _upos, ref Quaternion _urot, ref VectorLF3 _lpos, ref VectorLF3 _result)
	{
		double num = 2.0 * _lpos.x;
		double num2 = 2.0 * _lpos.y;
		double num3 = 2.0 * _lpos.z;
		double num4 = (double)(_urot.w * _urot.w) - 0.5;
		double num5 = (double)_urot.x * num + (double)_urot.y * num2 + (double)_urot.z * num3;
		_result.x = num * num4 + ((double)_urot.y * num3 - (double)_urot.z * num2) * (double)_urot.w + (double)_urot.x * num5 + _upos.x;
		_result.y = num2 * num4 + ((double)_urot.z * num - (double)_urot.x * num3) * (double)_urot.w + (double)_urot.y * num5 + _upos.y;
		_result.z = num3 * num4 + ((double)_urot.x * num2 - (double)_urot.y * num) * (double)_urot.w + (double)_urot.z * num5 + _upos.z;
	}

	// Token: 0x06000A5A RID: 2650 RVA: 0x0009C198 File Offset: 0x0009A398
	public static void lpos2upos_out(ref VectorLF3 _upos, ref Quaternion _urot, ref Vector3 _lpos, out VectorLF3 _result)
	{
		double num = 2.0 * (double)_lpos.x;
		double num2 = 2.0 * (double)_lpos.y;
		double num3 = 2.0 * (double)_lpos.z;
		double num4 = (double)(_urot.w * _urot.w) - 0.5;
		double num5 = (double)_urot.x * num + (double)_urot.y * num2 + (double)_urot.z * num3;
		_result.x = num * num4 + ((double)_urot.y * num3 - (double)_urot.z * num2) * (double)_urot.w + (double)_urot.x * num5 + _upos.x;
		_result.y = num2 * num4 + ((double)_urot.z * num - (double)_urot.x * num3) * (double)_urot.w + (double)_urot.y * num5 + _upos.y;
		_result.z = num3 * num4 + ((double)_urot.x * num2 - (double)_urot.y * num) * (double)_urot.w + (double)_urot.z * num5 + _upos.z;
	}

	// Token: 0x06000A5B RID: 2651 RVA: 0x0009C2B8 File Offset: 0x0009A4B8
	public static void lpos2upos_out(ref VectorLF3 _upos, ref Quaternion _urot, ref VectorLF3 _lpos, out VectorLF3 _result)
	{
		double num = 2.0 * _lpos.x;
		double num2 = 2.0 * _lpos.y;
		double num3 = 2.0 * _lpos.z;
		double num4 = (double)(_urot.w * _urot.w) - 0.5;
		double num5 = (double)_urot.x * num + (double)_urot.y * num2 + (double)_urot.z * num3;
		_result.x = num * num4 + ((double)_urot.y * num3 - (double)_urot.z * num2) * (double)_urot.w + (double)_urot.x * num5 + _upos.x;
		_result.y = num2 * num4 + ((double)_urot.z * num - (double)_urot.x * num3) * (double)_urot.w + (double)_urot.y * num5 + _upos.y;
		_result.z = num3 * num4 + ((double)_urot.x * num2 - (double)_urot.y * num) * (double)_urot.w + (double)_urot.z * num5 + _upos.z;
	}

	// Token: 0x06000A5C RID: 2652 RVA: 0x0009C3D8 File Offset: 0x0009A5D8
	public static void Vector3Cross_ref(ref Vector3 a, ref Vector3 b, out Vector3 result)
	{
		result.x = a.y * b.z - b.y * a.z;
		result.y = a.z * b.x - b.z * a.x;
		result.z = a.x * b.y - b.x * a.y;
	}

	// Token: 0x04000BEF RID: 3055
	public int id;

	// Token: 0x04000BF0 RID: 3056
	public int gid;

	// Token: 0x04000BF1 RID: 3057
	public int entityId;

	// Token: 0x04000BF2 RID: 3058
	public int planetId;

	// Token: 0x04000BF3 RID: 3059
	public int pcId;

	// Token: 0x04000BF4 RID: 3060
	public int minerId;

	// Token: 0x04000BF5 RID: 3061
	public int gene;

	// Token: 0x04000BF6 RID: 3062
	public Vector3 droneDock;

	// Token: 0x04000BF7 RID: 3063
	public Vector3 shipDockPos;

	// Token: 0x04000BF8 RID: 3064
	public Quaternion shipDockRot;

	// Token: 0x04000BF9 RID: 3065
	public bool isStellar;

	// Token: 0x04000BFA RID: 3066
	public bool isCollector;

	// Token: 0x04000BFB RID: 3067
	public bool isVeinCollector;

	// Token: 0x04000BFC RID: 3068
	public long energy;

	// Token: 0x04000BFD RID: 3069
	public long energyPerTick;

	// Token: 0x04000BFE RID: 3070
	public long energyMax;

	// Token: 0x04000BFF RID: 3071
	public int warperCount;

	// Token: 0x04000C00 RID: 3072
	public int warperMaxCount;

	// Token: 0x04000C01 RID: 3073
	public int idleDroneCount;

	// Token: 0x04000C02 RID: 3074
	public int workDroneCount;

	// Token: 0x04000C03 RID: 3075
	public DroneData[] workDroneDatas;

	// Token: 0x04000C04 RID: 3076
	public LocalLogisticOrder[] workDroneOrders;

	// Token: 0x04000C05 RID: 3077
	public int idleShipCount;

	// Token: 0x04000C06 RID: 3078
	public int workShipCount;

	// Token: 0x04000C07 RID: 3079
	public ulong idleShipIndices;

	// Token: 0x04000C08 RID: 3080
	public ulong workShipIndices;

	// Token: 0x04000C09 RID: 3081
	public ShipData[] workShipDatas;

	// Token: 0x04000C0A RID: 3082
	public RemoteLogisticOrder[] workShipOrders;

	// Token: 0x04000C0B RID: 3083
	public int renderShipCount;

	// Token: 0x04000C0C RID: 3084
	public ShipRenderingData[] shipRenderers;

	// Token: 0x04000C0D RID: 3085
	public ShipUIRenderingData[] shipUIRenderers;

	// Token: 0x04000C0E RID: 3086
	public StationStore[] storage;

	// Token: 0x04000C0F RID: 3087
	public StationPriorityLock[] priorityLocks;

	// Token: 0x04000C10 RID: 3088
	public SlotData[] slots;

	// Token: 0x04000C11 RID: 3089
	public int localPairProcess;

	// Token: 0x04000C12 RID: 3090
	public int[] remotePairProcesses;

	// Token: 0x04000C13 RID: 3091
	public int nextShipIndex;

	// Token: 0x04000C14 RID: 3092
	public int[] collectionIds;

	// Token: 0x04000C15 RID: 3093
	public float[] collectionPerTick;

	// Token: 0x04000C16 RID: 3094
	public float[] currentCollections;

	// Token: 0x04000C17 RID: 3095
	public int collectSpeed;

	// Token: 0x04000C18 RID: 3096
	public int[] veinCollectionIds;

	// Token: 0x04000C19 RID: 3097
	private int outSlotOffset;

	// Token: 0x04000C1A RID: 3098
	public byte[] droneDispatchStatus;

	// Token: 0x04000C1B RID: 3099
	public int droneStatusCursor;

	// Token: 0x04000C1C RID: 3100
	public int droneTaskInterval = 20;

	// Token: 0x04000C1D RID: 3101
	public double tripRangeDrones;

	// Token: 0x04000C1E RID: 3102
	public double tripRangeShips;

	// Token: 0x04000C1F RID: 3103
	public bool includeOrbitCollector;

	// Token: 0x04000C20 RID: 3104
	public double warpEnableDist;

	// Token: 0x04000C21 RID: 3105
	public bool warperNecessary;

	// Token: 0x04000C22 RID: 3106
	public int deliveryDrones;

	// Token: 0x04000C23 RID: 3107
	public int deliveryShips;

	// Token: 0x04000C24 RID: 3108
	public int pilerCount;

	// Token: 0x04000C25 RID: 3109
	public bool droneAutoReplenish;

	// Token: 0x04000C26 RID: 3110
	public bool shipAutoReplenish;

	// Token: 0x04000C27 RID: 3111
	public long remoteGroupMask;

	// Token: 0x04000C28 RID: 3112
	public ERemoteRoutePriority routePriority;

	// Token: 0x04000C29 RID: 3113
	public int[] needs;

	// Token: 0x04000C2A RID: 3114
	public SupplyDemandPair[] localPairs;

	// Token: 0x04000C2B RID: 3115
	public int localPairCount;

	// Token: 0x04000C2C RID: 3116
	public SupplyDemandPair[] remotePairs;

	// Token: 0x04000C2D RID: 3117
	public int[] remotePairOffsets;

	// Token: 0x04000C2E RID: 3118
	public Vector3[] shipDiskPos;

	// Token: 0x04000C2F RID: 3119
	public Quaternion[] shipDiskRot;

	// Token: 0x04000C30 RID: 3120
	public Mutex station_mx = new Mutex();

	// Token: 0x04000C31 RID: 3121
	public const int WARPER_ITEMID = 1210;

	// Token: 0x04000C32 RID: 3122
	private const long shipEnergyPerMeter = 30L;

	// Token: 0x04000C33 RID: 3123
	private const long shipEnergyPerMaxSpeed = 200000L;

	// Token: 0x04000C34 RID: 3124
	private const long shipEnergyTakeOff = 6000000L;

	// Token: 0x04000C35 RID: 3125
	private int _tmp_iter_local;

	// Token: 0x04000C36 RID: 3126
	private const double TICK_DELTA_TIME = 0.016666666666666666;

	// Token: 0x04000C37 RID: 3127
	private int _tmp_iter_remote;

	// Token: 0x04000C38 RID: 3128
	private bool warperFree;

	// Token: 0x04000C39 RID: 3129
	private const float liftAlt = 25f;
}
