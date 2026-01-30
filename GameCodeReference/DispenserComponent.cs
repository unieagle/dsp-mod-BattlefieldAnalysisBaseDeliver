using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020000CD RID: 205
public class DispenserComponent
{
	// Token: 0x0600071A RID: 1818 RVA: 0x0003CE44 File Offset: 0x0003B044
	public void Init(int _id, int _entityId, int _pcId, PrefabDesc _desc)
	{
		this.id = _id;
		this.entityId = _entityId;
		this.pcId = _pcId;
		this.gene = this.id % 3;
		this.energy = 0L;
		this.energyPerTick = _desc.workEnergyPerTick;
		this.energyMax = _desc.dispenserMaxEnergyAcc;
		this.playerMode = EPlayerDeliveryMode.Both;
		this.storageMode = EStorageDeliveryMode.None;
		this.filter = 0;
		this.idleCourierCount = 0;
		this.workCourierCount = 0;
		this.courierAutoReplenish = false;
		this.workCourierDatas = new CourierData[_desc.dispenserMaxCourierCount];
		this.orders = new DeliveryLogisticOrder[_desc.dispenserMaxCourierCount];
		this.holdupItemCount = 0;
		this.holdupPackage = new DispenserStore[_desc.dispenserMaxCourierCount * 2];
		this.playerOrdered = 0;
		this.storageOrdered = 0;
		this.pairProcess = 0;
		this.pulseSignal = -1;
		this.storage = null;
		this.pairs = null;
		this.playerPairCount = 0;
		this.pairCount = 0;
		this.insertStorageSearchStart = null;
		this.insertGridSearchStart = 0;
		this.pickStorageSearchStart = null;
		this.pickGridSearchStart = 0;
		this.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.None;
	}

	// Token: 0x0600071B RID: 1819 RVA: 0x0003CF5C File Offset: 0x0003B15C
	public void Free()
	{
		this.id = 0;
		this.entityId = 0;
		this.pcId = 0;
		this.storageId = 0;
		this.gene = 0;
		this.energy = 0L;
		this.energyPerTick = 0L;
		this.energyMax = 0L;
		this.playerMode = EPlayerDeliveryMode.None;
		this.storageMode = EStorageDeliveryMode.None;
		this.filter = 0;
		this.idleCourierCount = 0;
		this.workCourierCount = 0;
		this.courierAutoReplenish = false;
		this.workCourierDatas = null;
		this.orders = null;
		this.holdupItemCount = 0;
		this.holdupPackage = null;
		this.playerOrdered = 0;
		this.storageOrdered = 0;
		this.pairProcess = 0;
		this.pulseSignal = 0;
		this.storage = null;
		this.pairs = null;
		this.playerPairCount = 0;
		this.pairCount = 0;
		this.insertStorageSearchStart = null;
		this.insertGridSearchStart = 0;
		this.pickStorageSearchStart = null;
		this.pickGridSearchStart = 0;
	}

	// Token: 0x0600071C RID: 1820 RVA: 0x0003D040 File Offset: 0x0003B240
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(this.id);
		w.Write(this.entityId);
		w.Write(this.pcId);
		w.Write(this.storageId);
		w.Write(this.gene);
		w.Write(this.energy);
		w.Write(this.energyPerTick);
		w.Write(this.energyMax);
		w.Write((int)this.playerMode);
		w.Write((int)this.storageMode);
		w.Write(this.filter);
		w.Write(this.idleCourierCount);
		w.Write(this.workCourierCount);
		w.Write(this.courierAutoReplenish);
		w.Write(this.workCourierDatas.Length);
		for (int i = 0; i < this.workCourierCount; i++)
		{
			this.workCourierDatas[i].Export(w);
		}
		for (int j = 0; j < this.workCourierCount; j++)
		{
			this.orders[j].Export(w);
		}
		w.Write(this.holdupItemCount);
		w.Write(this.holdupPackage.Length);
		for (int k = 0; k < this.holdupItemCount; k++)
		{
			this.holdupPackage[k].Export(w);
		}
		w.Write(this.playerOrdered);
		w.Write(this.storageOrdered);
		w.Write(this.pairProcess);
		w.Write(this.pulseSignal);
	}

	// Token: 0x0600071D RID: 1821 RVA: 0x0003D1C0 File Offset: 0x0003B3C0
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.id = r.ReadInt32();
		this.entityId = r.ReadInt32();
		this.pcId = r.ReadInt32();
		this.storageId = r.ReadInt32();
		this.gene = r.ReadInt32();
		this.energy = r.ReadInt64();
		this.energyPerTick = r.ReadInt64();
		this.energyMax = r.ReadInt64();
		this.playerMode = (EPlayerDeliveryMode)r.ReadInt32();
		this.storageMode = (EStorageDeliveryMode)r.ReadInt32();
		this.filter = r.ReadInt32();
		this.idleCourierCount = r.ReadInt32();
		this.workCourierCount = r.ReadInt32();
		this.courierAutoReplenish = r.ReadBoolean();
		int num = r.ReadInt32();
		this.workCourierDatas = new CourierData[num];
		this.orders = new DeliveryLogisticOrder[num];
		for (int i = 0; i < this.workCourierCount; i++)
		{
			this.workCourierDatas[i].Import(r);
		}
		for (int j = 0; j < this.workCourierCount; j++)
		{
			this.orders[j].Import(r);
		}
		this.holdupItemCount = r.ReadInt32();
		num = r.ReadInt32();
		this.holdupPackage = new DispenserStore[num];
		for (int k = 0; k < this.holdupItemCount; k++)
		{
			this.holdupPackage[k].Import(r);
		}
		this.playerOrdered = r.ReadInt32();
		this.storageOrdered = r.ReadInt32();
		this.pairProcess = r.ReadInt32();
		this.pulseSignal = r.ReadInt32();
		if (this.pulseSignal > 3)
		{
			this.pulseSignal = 3;
			return;
		}
		this.pulseSignal = 0;
	}

	// Token: 0x0600071E RID: 1822 RVA: 0x0003D36B File Offset: 0x0003B56B
	public void UpdateKeepMode()
	{
		if (this.courierAutoReplenish)
		{
			this.idleCourierCount = this.workCourierDatas.Length - this.workCourierCount;
		}
	}

	// Token: 0x0600071F RID: 1823 RVA: 0x0003D38C File Offset: 0x0003B58C
	public void SetPairCapacity(int newCap)
	{
		if (this.pairs == null)
		{
			this.pairs = new SupplyDemandPair[newCap];
			return;
		}
		if (newCap <= this.pairs.Length)
		{
			return;
		}
		SupplyDemandPair[] array = this.pairs;
		this.pairs = new SupplyDemandPair[newCap];
		Array.Copy(array, this.pairs, array.Length);
	}

	// Token: 0x06000720 RID: 1824 RVA: 0x0003D3DC File Offset: 0x0003B5DC
	public void AddPair(int sId, int sIdx, int dId, int dIdx)
	{
		if (this.pairs == null)
		{
			this.SetPairCapacity(8);
		}
		if (this.pairCount == this.pairs.Length)
		{
			this.SetPairCapacity(this.pairs.Length * 2);
		}
		this.pairs[this.pairCount].supplyId = sId;
		this.pairs[this.pairCount].supplyIndex = sIdx;
		this.pairs[this.pairCount].demandId = dId;
		this.pairs[this.pairCount].demandIndex = dIdx;
		this.pairCount++;
		if (sId < 0 || dId < 0)
		{
			this.playerPairCount++;
		}
	}

	// Token: 0x06000721 RID: 1825 RVA: 0x0003D499 File Offset: 0x0003B699
	public void ClearPairs()
	{
		this.pairCount = 0;
		this.playerPairCount = 0;
		if (this.pairs != null)
		{
			Array.Clear(this.pairs, 0, this.pairs.Length);
		}
	}

	// Token: 0x06000722 RID: 1826 RVA: 0x0003D4C8 File Offset: 0x0003B6C8
	public void OnRematchPairs(PlanetFactory factory, DispenserComponent[] dispenserPool, int keyId, int courierCarries)
	{
		if (this.pairProcess < this.playerPairCount)
		{
			this.pairProcess = this.playerPairCount;
		}
		else if (this.pairProcess > this.pairCount - 1)
		{
			this.pairProcess = this.pairCount - 1;
		}
		DeliveryPackage.GRID[] grids = this.deliveryPackage.grids;
		for (int i = 0; i < this.workCourierCount; i++)
		{
			if (keyId == this.id)
			{
				if (this.workCourierDatas[i].itemCount == 0 && this.workCourierDatas[i].direction > 0f)
				{
					if (this.orders[i].otherId > 0)
					{
						DispenserComponent dispenserComponent = dispenserPool[this.orders[i].otherId];
						if (this.storageMode != EStorageDeliveryMode.Demand || dispenserComponent.storageMode != EStorageDeliveryMode.Supply || this.filter <= 0 || this.filter != dispenserComponent.filter)
						{
							this.storageOrdered -= this.orders[i].thisOrdered;
							this.orders[i].thisOrdered = 0;
							dispenserPool[this.orders[i].otherId].storageOrdered -= this.orders[i].otherOrdered;
							this.orders[i].otherOrdered = 0;
							this.workCourierDatas[i].endId = 0;
							this.workCourierDatas[i].direction = -1f;
						}
					}
					else if (this.orders[i].otherId < 0)
					{
						this.playerOrdered -= this.orders[i].thisOrdered;
						this.orders[i].thisOrdered = 0;
						DeliveryPackage.GRID[] grids2 = this.deliveryPackage.grids;
						int num = -(this.orders[i].otherId + 1);
						grids2[num].ordered = grids2[num].ordered - this.orders[i].otherOrdered;
						this.orders[i].otherOrdered = 0;
						bool flag = true;
						if ((this.playerMode == EPlayerDeliveryMode.Recycle || this.playerMode == EPlayerDeliveryMode.Both) && this.filter != 0 && this.holdupItemCount < 6)
						{
							for (int j = 0; j < this.playerPairCount; j++)
							{
								if (this.pairs[j].supplyId < 0)
								{
									int supplyIndex = this.pairs[j].supplyIndex;
									int itemId = grids[supplyIndex].itemId;
									if (this.filter > 0 && itemId != this.filter)
									{
										Assert.CannotBeReached();
									}
									else if (this.holdupItemCount <= 0 || (itemId != this.holdupPackage[0].itemId && itemId != this.holdupPackage[1].itemId && itemId != this.holdupPackage[2].itemId && itemId != this.holdupPackage[3].itemId && itemId != this.holdupPackage[4].itemId))
									{
										int packageItemCount = this.packageUtility.GetPackageItemCount(itemId);
										int num2 = grids[supplyIndex].modifiedCount + packageItemCount;
										int num3 = grids[supplyIndex].count + packageItemCount;
										int num4 = (num2 < num3) ? num2 : num3;
										if (num4 > grids[supplyIndex].recycleCount && num4 > 0)
										{
											int num5 = this.InsertIntoStoragePrecalc(itemId, courierCarries + this.playerOrdered, false) - this.playerOrdered;
											if (num5 > 0)
											{
												this.workCourierDatas[i].itemId = itemId;
												this.workCourierDatas[i].direction = 1f;
												this.workCourierDatas[i].endId = this.pairs[j].supplyId;
												this.orders[i].itemId = itemId;
												this.orders[i].otherId = -(supplyIndex + 1);
												this.orders[i].thisOrdered = num5;
												this.orders[i].otherOrdered = -num5;
												this.playerOrdered += num5;
												DeliveryPackage.GRID[] array = grids;
												int num6 = supplyIndex;
												array[num6].ordered = array[num6].ordered - num5;
												flag = false;
												break;
											}
										}
									}
								}
							}
						}
						if (flag)
						{
							this.CourierTurnbackFromPlayer(factory, ref this.workCourierDatas[i]);
						}
					}
				}
				if (this.workCourierDatas[i].itemCount != 0 && this.workCourierDatas[i].direction < 0f && this.orders[i].itemId != this.filter && this.workCourierDatas[i].itemId > 0)
				{
					if (this.orders[i].otherId > 0)
					{
						this.storageOrdered -= this.orders[i].thisOrdered;
						this.orders[i].thisOrdered = 0;
						dispenserPool[this.orders[i].otherId].storageOrdered -= this.orders[i].otherOrdered;
						this.orders[i].otherOrdered = 0;
					}
					else if (this.orders[i].otherId < 0)
					{
						this.playerOrdered -= this.orders[i].thisOrdered;
						this.orders[i].thisOrdered = 0;
						DeliveryPackage.GRID[] grids3 = this.deliveryPackage.grids;
						int num7 = -(this.orders[i].otherId + 1);
						grids3[num7].ordered = grids3[num7].ordered - this.orders[i].otherOrdered;
						this.orders[i].otherOrdered = 0;
					}
				}
			}
			else if ((keyId == this.orders[i].otherId || (keyId < -100 && this.orders[i].otherId < 0)) && this.workCourierDatas[i].direction > 0f)
			{
				if (this.orders[i].otherId > 0 && (dispenserPool[this.orders[i].otherId] == null || dispenserPool[this.orders[i].otherId].id == 0))
				{
					this.storageOrdered -= this.orders[i].thisOrdered;
					this.orders[i].thisOrdered = 0;
					this.orders[i].otherOrdered = 0;
					this.workCourierDatas[i].endId = 0;
					this.workCourierDatas[i].direction = -1f;
				}
				else if (this.workCourierDatas[i].itemCount > 0)
				{
					if (this.orders[i].otherId > 0)
					{
						DispenserComponent dispenserComponent2 = dispenserPool[this.orders[i].otherId];
						if (this.storageMode != EStorageDeliveryMode.Supply || dispenserComponent2.storageMode != EStorageDeliveryMode.Demand || this.filter <= 0 || this.filter != dispenserComponent2.filter)
						{
							this.storageOrdered -= this.orders[i].thisOrdered;
							this.orders[i].thisOrdered = 0;
							dispenserComponent2.storageOrdered -= this.orders[i].otherOrdered;
							this.orders[i].otherOrdered = 0;
							this.workCourierDatas[i].endId = 0;
							this.workCourierDatas[i].direction = -1f;
						}
					}
					else
					{
						this.playerOrdered -= this.orders[i].thisOrdered;
						this.orders[i].thisOrdered = 0;
						if (this.deliveryPackage.grids[-(this.orders[i].otherId + 1)].itemId == this.orders[i].itemId)
						{
							DeliveryPackage.GRID[] grids4 = this.deliveryPackage.grids;
							int num8 = -(this.orders[i].otherId + 1);
							grids4[num8].ordered = grids4[num8].ordered - this.orders[i].otherOrdered;
						}
						this.orders[i].otherOrdered = 0;
						this.CourierTurnbackFromPlayer(factory, ref this.workCourierDatas[i]);
					}
				}
				else if (this.workCourierDatas[i].itemCount == 0)
				{
					if (this.orders[i].otherId > 0)
					{
						DispenserComponent dispenserComponent3 = dispenserPool[this.orders[i].otherId];
						if (this.storageMode != EStorageDeliveryMode.Demand || dispenserComponent3.storageMode != EStorageDeliveryMode.Supply || this.filter <= 0 || this.filter != dispenserComponent3.filter)
						{
							this.storageOrdered -= this.orders[i].thisOrdered;
							this.orders[i].thisOrdered = 0;
							dispenserComponent3.storageOrdered -= this.orders[i].otherOrdered;
							this.orders[i].otherOrdered = 0;
							this.workCourierDatas[i].endId = 0;
							this.workCourierDatas[i].direction = -1f;
						}
					}
					else
					{
						this.playerOrdered -= this.orders[i].thisOrdered;
						this.orders[i].thisOrdered = 0;
						if (this.deliveryPackage.grids[-(this.orders[i].otherId + 1)].itemId == this.orders[i].itemId)
						{
							DeliveryPackage.GRID[] grids5 = this.deliveryPackage.grids;
							int num9 = -(this.orders[i].otherId + 1);
							grids5[num9].ordered = grids5[num9].ordered - this.orders[i].otherOrdered;
						}
						this.orders[i].otherOrdered = 0;
						bool flag2 = true;
						if ((this.playerMode == EPlayerDeliveryMode.Recycle || this.playerMode == EPlayerDeliveryMode.Both) && this.filter != 0 && this.holdupItemCount < 6)
						{
							for (int k = 0; k < this.playerPairCount; k++)
							{
								if (this.pairs[k].supplyId < 0)
								{
									int supplyIndex2 = this.pairs[k].supplyIndex;
									int itemId2 = grids[supplyIndex2].itemId;
									if (this.filter > 0 && itemId2 != this.filter)
									{
										Assert.CannotBeReached();
									}
									else if (this.holdupItemCount <= 0 || (itemId2 != this.holdupPackage[0].itemId && itemId2 != this.holdupPackage[1].itemId && itemId2 != this.holdupPackage[2].itemId && itemId2 != this.holdupPackage[3].itemId && itemId2 != this.holdupPackage[4].itemId))
									{
										int packageItemCount2 = this.packageUtility.GetPackageItemCount(itemId2);
										int num10 = grids[supplyIndex2].modifiedCount + packageItemCount2;
										int num11 = grids[supplyIndex2].count + packageItemCount2;
										int num12 = (num10 < num11) ? num10 : num11;
										if (num12 > grids[supplyIndex2].recycleCount && num12 > 0)
										{
											int num13 = this.InsertIntoStoragePrecalc(itemId2, courierCarries + this.playerOrdered, false) - this.playerOrdered;
											if (num13 > 0)
											{
												this.workCourierDatas[i].itemId = itemId2;
												this.workCourierDatas[i].direction = 1f;
												this.workCourierDatas[i].endId = this.pairs[k].supplyId;
												this.orders[i].itemId = itemId2;
												this.orders[i].otherId = -(supplyIndex2 + 1);
												this.orders[i].thisOrdered = num13;
												this.orders[i].otherOrdered = -num13;
												this.playerOrdered += num13;
												DeliveryPackage.GRID[] array2 = grids;
												int num14 = supplyIndex2;
												array2[num14].ordered = array2[num14].ordered - num13;
												flag2 = false;
												break;
											}
										}
									}
								}
							}
						}
						if (flag2)
						{
							this.CourierTurnbackFromPlayer(factory, ref this.workCourierDatas[i]);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000723 RID: 1827 RVA: 0x0003E218 File Offset: 0x0003C418
	private void CourierTurnbackFromPlayer(PlanetFactory factory, ref CourierData workCourier)
	{
		ref Vector3 ptr = ref factory.entityPool[this.entityId].pos;
		ref Vector3 ptr2 = ref workCourier.end;
		double num = Math.Sqrt((double)(ptr.x * ptr.x + ptr.y * ptr.y + ptr.z * ptr.z));
		double num2 = Math.Sqrt((double)(ptr2.x * ptr2.x + ptr2.y * ptr2.y + ptr2.z * ptr2.z));
		double num3 = (double)(ptr.x * ptr2.x + ptr.y * ptr2.y + ptr.z * ptr2.z) / (num * num2);
		if (num3 < -1.0)
		{
			num3 = -1.0;
		}
		else if (num3 > 1.0)
		{
			num3 = 1.0;
		}
		workCourier.begin = ptr;
		workCourier.maxt = (float)(Math.Acos(num3) * ((num + num2) * 0.5));
		workCourier.maxt = (float)Math.Sqrt((double)(workCourier.maxt * workCourier.maxt) + (num - num2) * (num - num2));
		workCourier.t = workCourier.maxt;
		workCourier.endId = 0;
		workCourier.direction = -1f;
	}

	// Token: 0x06000724 RID: 1828 RVA: 0x0003E370 File Offset: 0x0003C570
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

	// Token: 0x06000725 RID: 1829 RVA: 0x0003E3F8 File Offset: 0x0003C5F8
	public void InternalTick(PlanetFactory factory, EntityData[] entityPool, DispenserComponent[] dispenserPool, Vector3 playerPos, long time, float power, float courierSpeed, int courierCarries, double deliveryRange)
	{
		if (this.filter == 1099)
		{
			factory.transport.SetDispenserFilter(this.id, 0);
		}
		this.energy += (long)((int)((float)this.energyPerTick * power));
		this.energy -= 300L;
		if (this.energy > this.energyMax)
		{
			this.energy = this.energyMax;
		}
		else if (this.energy < 0L)
		{
			this.energy = 0L;
		}
		if (this.storage == null)
		{
			return;
		}
		int num = this.storage.bottomStorage.entityId;
		Vector3 pos = entityPool[this.entityId].pos;
		DeliveryPackage.GRID[] grids = this.deliveryPackage.grids;
		int num2 = (int)(time % 3L);
		if (num2 < 0)
		{
			num2 += 3;
		}
		if (num2 == this.gene)
		{
			this.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.Traversed;
			this._tmp_iter++;
			if (this.idleCourierCount > 0)
			{
				this.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.HasCourier;
				bool flag = false;
				if (this.playerPairCount > 0)
				{
					this.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.HasPair;
					double num3;
					bool flag2 = this.CheckDeliveryRange(pos, playerPos, deliveryRange, out num3);
					long num4 = (long)(num3 * 10000.0 * 2.0 + 100000.0);
					if (flag2)
					{
						this.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.InRange;
					}
					if (this.energy >= num4 && flag2)
					{
						this.playerDeliveryCondition = DispenserComponent.EPlayerDeliveryCondition.EnergyEnough;
						for (int i = 0; i < this.playerPairCount; i++)
						{
							ref SupplyDemandPair ptr = ref this.pairs[i];
							ptr.runtimeState = 0;
							if (ptr.supplyId == this.id)
							{
								int demandIndex = ptr.demandIndex;
								if (demandIndex >= 100)
								{
									Assert.CannotBeReached();
								}
								else
								{
									Assert.True(-(ptr.demandId + 1) == demandIndex);
									int itemId = grids[demandIndex].itemId;
									if (itemId == this.filter)
									{
										ptr.runtimeState = 1;
										int packageItemCountIncludeHandItem = this.packageUtility.GetPackageItemCountIncludeHandItem(itemId);
										int num5 = grids[demandIndex].modifiedCount + packageItemCountIncludeHandItem;
										int num6 = grids[demandIndex].count + packageItemCountIncludeHandItem;
										int num7 = (num5 > num6) ? num5 : num6;
										int num8 = grids[demandIndex].stackSizeModified - grids[demandIndex].modifiedCount + this.packageUtility.GetPackageItemCapacity(itemId);
										int clampedRequireCount = grids[demandIndex].clampedRequireCount;
										if (num7 < clampedRequireCount)
										{
											ptr.runtimeState = 2;
											if (num8 > 0)
											{
												ptr.runtimeState = 3;
												int num9 = clampedRequireCount - num7;
												num9 = ((num9 > courierCarries) ? courierCarries : num9);
												num9 = ((num9 > num8) ? num8 : num9);
												int num10 = (this.storageOrdered > 0) ? 0 : this.storageOrdered;
												num9 = this.PickFromStoragePrecalc(this.filter, num9 - num10);
												num9 += num10;
												if (num9 > 0)
												{
													int inc;
													int num11 = factory.PickFromStorage(num, itemId, num9, out inc);
													if (num11 > 0)
													{
														ptr.runtimeState = 4;
														this.workCourierDatas[this.workCourierCount].begin = pos;
														this.workCourierDatas[this.workCourierCount].end = pos;
														this.workCourierDatas[this.workCourierCount].endId = ptr.demandId;
														this.workCourierDatas[this.workCourierCount].direction = 1f;
														this.workCourierDatas[this.workCourierCount].maxt = 1f;
														this.workCourierDatas[this.workCourierCount].t = 0f;
														this.workCourierDatas[this.workCourierCount].itemId = itemId;
														this.workCourierDatas[this.workCourierCount].itemCount = num11;
														this.workCourierDatas[this.workCourierCount].inc = inc;
														this.workCourierDatas[this.workCourierCount].gene = this._tmp_iter;
														this.orders[this.workCourierCount].itemId = itemId;
														this.orders[this.workCourierCount].otherId = ptr.demandId;
														this.orders[this.workCourierCount].thisOrdered = 0;
														this.orders[this.workCourierCount].otherOrdered = num11;
														this.playerOrdered = this.playerOrdered;
														DeliveryPackage.GRID[] array = grids;
														int num12 = demandIndex;
														array[num12].ordered = array[num12].ordered + num11;
														this.workCourierCount++;
														this.idleCourierCount--;
														this.energy -= num4;
														this.pulseSignal = 2;
														flag = true;
														break;
													}
												}
											}
										}
									}
								}
							}
							else if (ptr.demandId == this.id)
							{
								ptr.runtimeState = 1;
								if (this.holdupItemCount < 6)
								{
									int supplyIndex = ptr.supplyIndex;
									if (supplyIndex >= 100)
									{
										Assert.CannotBeReached();
									}
									else
									{
										Assert.True(-(ptr.supplyId + 1) == supplyIndex);
										int itemId2 = grids[supplyIndex].itemId;
										if (this.filter > 0 && itemId2 != this.filter)
										{
											Assert.CannotBeReached();
										}
										else if (this.holdupItemCount <= 0 || (itemId2 != this.holdupPackage[0].itemId && itemId2 != this.holdupPackage[1].itemId && itemId2 != this.holdupPackage[2].itemId && itemId2 != this.holdupPackage[3].itemId && itemId2 != this.holdupPackage[4].itemId))
										{
											ptr.runtimeState = 2;
											int packageItemCount = this.packageUtility.GetPackageItemCount(itemId2);
											int num13 = grids[supplyIndex].modifiedCount + packageItemCount;
											int num14 = grids[supplyIndex].count + packageItemCount;
											if (((num13 < num14) ? num13 : num14) > grids[supplyIndex].recycleCount)
											{
												ptr.runtimeState = 3;
												int num15 = (this.storageOrdered < 0) ? 0 : this.storageOrdered;
												int num16 = this.InsertIntoStoragePrecalc(itemId2, courierCarries + this.playerOrdered + num15, false) - this.playerOrdered - num15;
												if (num16 > 0)
												{
													ptr.runtimeState = 4;
													this.workCourierDatas[this.workCourierCount].begin = pos;
													this.workCourierDatas[this.workCourierCount].end = pos;
													this.workCourierDatas[this.workCourierCount].endId = ptr.supplyId;
													this.workCourierDatas[this.workCourierCount].direction = 1f;
													this.workCourierDatas[this.workCourierCount].maxt = 1f;
													this.workCourierDatas[this.workCourierCount].t = 0f;
													this.workCourierDatas[this.workCourierCount].itemId = itemId2;
													this.workCourierDatas[this.workCourierCount].itemCount = 0;
													this.workCourierDatas[this.workCourierCount].inc = 0;
													this.workCourierDatas[this.workCourierCount].gene = this._tmp_iter;
													this.orders[this.workCourierCount].itemId = itemId2;
													this.orders[this.workCourierCount].otherId = ptr.supplyId;
													this.orders[this.workCourierCount].thisOrdered = num16;
													this.orders[this.workCourierCount].otherOrdered = -num16;
													this.playerOrdered += num16;
													DeliveryPackage.GRID[] array2 = grids;
													int num17 = supplyIndex;
													array2[num17].ordered = array2[num17].ordered - num16;
													this.workCourierCount++;
													this.idleCourierCount--;
													this.energy -= num4;
													this.pulseSignal = 2;
													flag = true;
													break;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				if (!flag)
				{
					if (this.pairProcess < this.playerPairCount)
					{
						this.pairProcess = this.playerPairCount;
					}
					else if (this.pairProcess >= this.pairCount)
					{
						this.pairProcess = this.playerPairCount;
					}
					if (this.storageMode != EStorageDeliveryMode.None && this.filter > 0 && this.pairCount > this.playerPairCount)
					{
						int num18 = this.pairProcess;
						int num19 = 10;
						SupplyDemandPair ptr2;
						DispenserComponent dispenserComponent;
						Vector3 pos2;
						double num20;
						long num21;
						int inc2;
						int num26;
						int num28;
						DispenserComponent dispenserComponent2;
						Vector3 pos3;
						double num30;
						long num31;
						for (;;)
						{
							ptr2 = ref this.pairs[this.pairProcess];
							this.pairProcess++;
							if (this.pairProcess == this.pairCount)
							{
								this.pairProcess = this.playerPairCount;
							}
							num19--;
							if (ptr2.supplyId == this.id)
							{
								if (ptr2.demandId <= 0)
								{
									Assert.Positive(ptr2.demandId);
								}
								else
								{
									dispenserComponent = dispenserPool[ptr2.demandId];
									if (dispenserComponent == null || dispenserComponent.id != ptr2.demandId)
									{
										Assert.CannotBeReached();
									}
									else if (dispenserComponent.holdupItemCount < 6 && (dispenserComponent.holdupItemCount <= 0 || (dispenserComponent.holdupPackage[0].itemId != this.filter && dispenserComponent.holdupPackage[1].itemId != this.filter && dispenserComponent.holdupPackage[2].itemId != this.filter && dispenserComponent.holdupPackage[3].itemId != this.filter && dispenserComponent.holdupPackage[4].itemId != this.filter)))
									{
										pos2 = entityPool[dispenserComponent.entityId].pos;
										if (this.CheckDeliveryRange(pos, pos2, deliveryRange, out num20))
										{
											num21 = (long)(num20 * 10000.0 * 2.0 + 100000.0);
											if (this.energy >= num21)
											{
												int num22 = (dispenserComponent.storageOrdered < 0) ? 0 : dispenserComponent.storageOrdered;
												int num23 = dispenserComponent.InsertIntoStoragePrecalc(this.filter, courierCarries + num22 + dispenserComponent.playerOrdered, true);
												num23 = num23 - num22 - dispenserComponent.playerOrdered;
												if (num23 > 0)
												{
													int num24 = (this.storageOrdered > 0) ? 0 : this.storageOrdered;
													int num25 = this.PickFromStoragePrecalc(this.filter, num23 - num24);
													num25 += num24;
													if (num25 > 0)
													{
														num26 = factory.PickFromStorage(num, this.filter, num25, out inc2);
														if (num26 > 0)
														{
															break;
														}
													}
												}
											}
										}
									}
								}
							}
							else if (ptr2.demandId == this.id)
							{
								if (ptr2.supplyId <= 0)
								{
									Assert.Positive(ptr2.supplyId);
								}
								else if (this.holdupItemCount < 6 && (this.holdupItemCount <= 0 || (this.holdupPackage[0].itemId != this.filter && this.holdupPackage[1].itemId != this.filter && this.holdupPackage[2].itemId != this.filter && this.holdupPackage[3].itemId != this.filter && this.holdupPackage[4].itemId != this.filter)))
								{
									int num27 = (this.storageOrdered < 0) ? 0 : this.storageOrdered;
									num28 = this.InsertIntoStoragePrecalc(this.filter, courierCarries + num27 + this.playerOrdered, true);
									num28 = num28 - num27 - this.playerOrdered;
									if (num28 > 0)
									{
										dispenserComponent2 = dispenserPool[ptr2.supplyId];
										if (dispenserComponent2 == null || dispenserComponent2.id != ptr2.supplyId)
										{
											Assert.CannotBeReached();
										}
										else
										{
											int num29 = (dispenserComponent2.storageOrdered > 0) ? 0 : dispenserComponent2.storageOrdered;
											if (dispenserComponent2.PickFromStoragePrecalc(this.filter, num28 - num29) + num29 > 0)
											{
												pos3 = entityPool[dispenserComponent2.entityId].pos;
												if (this.CheckDeliveryRange(pos, pos3, deliveryRange, out num30))
												{
													num31 = (long)(num30 * 10000.0 * 2.0 + 100000.0);
													if (this.energy >= num31)
													{
														goto Block_73;
													}
												}
											}
										}
									}
								}
							}
							if (num18 == this.pairProcess || num19 <= 0)
							{
								goto IL_1049;
							}
						}
						this.workCourierDatas[this.workCourierCount].begin = pos;
						this.workCourierDatas[this.workCourierCount].end = pos2;
						this.workCourierDatas[this.workCourierCount].endId = ptr2.demandId;
						this.workCourierDatas[this.workCourierCount].direction = 1f;
						this.workCourierDatas[this.workCourierCount].maxt = (float)num20;
						this.workCourierDatas[this.workCourierCount].t = 0f;
						this.workCourierDatas[this.workCourierCount].itemId = this.filter;
						this.workCourierDatas[this.workCourierCount].itemCount = num26;
						this.workCourierDatas[this.workCourierCount].inc = inc2;
						this.workCourierDatas[this.workCourierCount].gene = this._tmp_iter;
						this.orders[this.workCourierCount].itemId = this.filter;
						this.orders[this.workCourierCount].otherId = ptr2.demandId;
						this.orders[this.workCourierCount].thisOrdered = 0;
						this.orders[this.workCourierCount].otherOrdered = num26;
						this.storageOrdered = this.storageOrdered;
						dispenserComponent.storageOrdered += num26;
						factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, this.filter, num26);
						this.workCourierCount++;
						this.idleCourierCount--;
						this.energy -= num21;
						this.pulseSignal = 2;
						goto IL_1049;
						Block_73:
						this.workCourierDatas[this.workCourierCount].begin = pos;
						this.workCourierDatas[this.workCourierCount].end = pos3;
						this.workCourierDatas[this.workCourierCount].endId = ptr2.supplyId;
						this.workCourierDatas[this.workCourierCount].direction = 1f;
						this.workCourierDatas[this.workCourierCount].maxt = (float)num30;
						this.workCourierDatas[this.workCourierCount].t = 0f;
						this.workCourierDatas[this.workCourierCount].itemId = this.filter;
						this.workCourierDatas[this.workCourierCount].itemCount = 0;
						this.workCourierDatas[this.workCourierCount].inc = 0;
						this.workCourierDatas[this.workCourierCount].gene = this._tmp_iter;
						this.orders[this.workCourierCount].itemId = this.filter;
						this.orders[this.workCourierCount].otherId = ptr2.supplyId;
						this.orders[this.workCourierCount].thisOrdered = num28;
						this.orders[this.workCourierCount].otherOrdered = -num28;
						this.storageOrdered += num28;
						dispenserComponent2.storageOrdered -= num28;
						this.workCourierCount++;
						this.idleCourierCount--;
						this.energy -= num31;
						this.pulseSignal = 2;
					}
				}
			}
		}
		IL_1049:
		float num32 = 0.016666668f * courierSpeed;
		for (int j = 0; j < this.workCourierCount; j++)
		{
			if (this.workCourierDatas[j].maxt > 0f)
			{
				if (this.workCourierDatas[j].endId < 0 && this.workCourierDatas[j].direction > 0f)
				{
					Vector3 pos4 = entityPool[this.entityId].pos;
					ref Vector3 ptr3 = ref this.workCourierDatas[j].end;
					ref Vector3 ptr4 = ref playerPos;
					Vector3 vector = new Vector3(ptr4.x - ptr3.x, ptr4.y - ptr3.y, ptr4.z - ptr3.z);
					Vector3 vector2 = new Vector3(ptr4.x - pos4.x, ptr4.y - pos4.y, ptr4.z - pos4.z);
					float num33 = (float)Math.Sqrt((double)(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z));
					float num34 = (float)Math.Sqrt((double)(vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z));
					float num35 = (float)Math.Sqrt((double)(ptr3.x * ptr3.x + ptr3.y * ptr3.y + ptr3.z * ptr3.z));
					float num36 = (float)Math.Sqrt((double)(ptr4.x * ptr4.x + ptr4.y * ptr4.y + ptr4.z * ptr4.z));
					if (num33 < 1.4f)
					{
						double num37 = Math.Sqrt((double)(pos4.x * pos4.x + pos4.y * pos4.y + pos4.z * pos4.z));
						double num38 = Math.Sqrt((double)(ptr4.x * ptr4.x + ptr4.y * ptr4.y + ptr4.z * ptr4.z));
						double num39 = (double)(pos4.x * ptr4.x + pos4.y * ptr4.y + pos4.z * ptr4.z) / (num37 * num38);
						if (num39 < -1.0)
						{
							num39 = -1.0;
						}
						else if (num39 > 1.0)
						{
							num39 = 1.0;
						}
						this.workCourierDatas[j].begin = pos4;
						this.workCourierDatas[j].maxt = (float)(Math.Acos(num39) * ((num37 + num38) * 0.5));
						this.workCourierDatas[j].maxt = (float)Math.Sqrt((double)(this.workCourierDatas[j].maxt * this.workCourierDatas[j].maxt) + (num37 - num38) * (num37 - num38));
						this.workCourierDatas[j].t = this.workCourierDatas[j].maxt;
					}
					else
					{
						this.workCourierDatas[j].begin = ptr3;
						float num40 = courierSpeed * 0.016666668f / num33;
						if (num40 > 1f)
						{
							num40 = 1f;
						}
						Vector3 vector3 = new Vector3(vector.x * num40, vector.y * num40, vector.z * num40);
						float num41 = num33 / courierSpeed;
						if (num41 < 0.03333333f)
						{
							num41 = 0.03333333f;
						}
						float num42 = (num36 - num35) / num41 * 0.016666668f;
						ptr3.x += vector3.x;
						ptr3.y += vector3.y;
						ptr3.z += vector3.z;
						ptr3 = ptr3.normalized * (num35 + num42);
						if (num34 > this.workCourierDatas[j].maxt)
						{
							this.workCourierDatas[j].maxt = num34;
						}
						this.workCourierDatas[j].t = num33;
						if (this.workCourierDatas[j].t >= this.workCourierDatas[j].maxt * 0.99f)
						{
							this.workCourierDatas[j].t = this.workCourierDatas[j].maxt * 0.99f;
						}
					}
				}
				else
				{
					CourierData[] array3 = this.workCourierDatas;
					int num43 = j;
					array3[num43].t = array3[num43].t + num32 * this.workCourierDatas[j].direction;
				}
				if (this.workCourierDatas[j].t >= this.workCourierDatas[j].maxt)
				{
					this.workCourierDatas[j].t = this.workCourierDatas[j].maxt;
					int endId = this.workCourierDatas[j].endId;
					if (this.workCourierDatas[j].itemCount > 0)
					{
						int itemId3 = this.workCourierDatas[j].itemId;
						int itemCount = this.workCourierDatas[j].itemCount;
						if (endId < 0)
						{
							int num44 = -(endId + 1);
							DeliveryPackage.GRID[] array4 = grids;
							int num45 = num44;
							array4[num45].ordered = array4[num45].ordered - this.orders[j].otherOrdered;
							this.orders[j].otherOrdered = 0;
							int num46 = grids[num44].clampedRequireCount - (grids[num44].count + this.packageUtility.GetPackageItemCountIncludeHandItem(itemId3));
							int num47 = grids[num44].stackSizeModified - grids[num44].count + this.packageUtility.GetPackageItemCapacity(itemId3);
							if (num46 > 0 && num47 > 0)
							{
								num46 = ((num46 > num47) ? num47 : num46);
								int num48 = 0;
								int num49;
								if (num46 < itemCount)
								{
									int inc3 = this.split_inc(ref this.workCourierDatas[j].itemCount, ref this.workCourierDatas[j].inc, num46);
									num49 = this.packageUtility.AddItemToAllPackages(itemId3, num46, num44, inc3, out num48, 0);
									CourierData[] array5 = this.workCourierDatas;
									int num50 = j;
									array5[num50].inc = array5[num50].inc + num48;
								}
								else
								{
									num49 = this.packageUtility.AddItemToAllPackages(itemId3, itemCount, num44, this.workCourierDatas[j].inc, out num48, 0);
									this.workCourierDatas[j].inc = num48;
								}
								this.packageUtility.player.NotifyReplenishPreferred(itemId3, num49);
								this.workCourierDatas[j].itemCount = itemCount - num49;
								this.orders[j].thisOrdered = this.workCourierDatas[j].itemCount;
								this.playerOrdered += this.workCourierDatas[j].itemCount;
							}
						}
						else
						{
							DispenserComponent dispenserComponent3 = dispenserPool[endId];
							dispenserComponent3.storageOrdered -= this.orders[j].otherOrdered;
							this.orders[j].otherOrdered = 0;
							int num52;
							int num51 = factory.InsertIntoStorage(dispenserComponent3.storage.bottomStorage.entityId, itemId3, itemCount, this.workCourierDatas[j].inc, out num52, true);
							int num53 = itemCount - num51;
							if (num53 > 0)
							{
								bool flag3 = true;
								DispenserStore[] array6 = dispenserComponent3.holdupPackage;
								for (int k = 0; k < dispenserComponent3.holdupItemCount; k++)
								{
									if (array6[k].itemId == itemId3)
									{
										DispenserStore[] array7 = array6;
										int num54 = k;
										array7[num54].count = array7[num54].count + num53;
										DispenserStore[] array8 = array6;
										int num55 = k;
										array8[num55].inc = array8[num55].inc + num52;
										flag3 = false;
										break;
									}
								}
								if (flag3)
								{
									int num56 = dispenserComponent3.holdupItemCount;
									Assert.True(array6.Length >= num56);
									array6[num56].itemId = itemId3;
									array6[num56].count = num53;
									array6[num56].inc = num52;
									dispenserComponent3.holdupItemCount++;
								}
							}
							factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, this.workCourierDatas[j].itemId, this.workCourierDatas[j].itemCount);
							this.workCourierDatas[j].itemCount = 0;
							this.workCourierDatas[j].inc = 0;
							dispenserComponent3.pulseSignal = 2;
						}
						this.workCourierDatas[j].direction = -1f;
					}
					else
					{
						int itemId4 = this.orders[j].itemId;
						int num57 = this.orders[j].thisOrdered;
						if (endId < 0)
						{
							int num58 = -(endId + 1);
							this.playerOrdered -= this.orders[j].thisOrdered;
							this.orders[j].thisOrdered = 0;
							DeliveryPackage.GRID[] array9 = grids;
							int num59 = num58;
							array9[num59].ordered = array9[num59].ordered - this.orders[j].otherOrdered;
							this.orders[j].otherOrdered = 0;
							int num60 = grids[num58].count + this.packageUtility.GetPackageItemCount(itemId4);
							if (num60 > grids[num58].recycleCount && num60 > 0)
							{
								int num61 = num60 - grids[num58].recycleCount;
								if (num61 > 0)
								{
									if (num61 < num57)
									{
										num57 = num61;
									}
									int inc4;
									this.packageUtility.TakeItemFromAllPackages(num58, ref itemId4, ref num57, out inc4, false);
									this.workCourierDatas[j].itemId = itemId4;
									this.workCourierDatas[j].itemCount = num57;
									this.workCourierDatas[j].inc = inc4;
									this.playerOrdered += num57;
									this.orders[j].thisOrdered = num57;
								}
							}
							if (this.workCourierDatas[j].itemCount == 0 && (this.playerMode == EPlayerDeliveryMode.Recycle || this.playerMode == EPlayerDeliveryMode.Both) && this.filter != 0 && this.holdupItemCount < 6)
							{
								for (int l = 0; l < this.playerPairCount; l++)
								{
									if (this.pairs[l].supplyId < 0)
									{
										num58 = this.pairs[l].supplyIndex;
										int itemId5 = grids[num58].itemId;
										if (this.filter > 0 && itemId5 != this.filter)
										{
											Assert.CannotBeReached();
										}
										else if (this.holdupItemCount <= 0 || (itemId5 != this.holdupPackage[0].itemId && itemId5 != this.holdupPackage[1].itemId && itemId5 != this.holdupPackage[2].itemId && itemId5 != this.holdupPackage[3].itemId && itemId5 != this.holdupPackage[4].itemId))
										{
											int packageItemCount2 = this.packageUtility.GetPackageItemCount(itemId5);
											int num62 = grids[num58].modifiedCount + packageItemCount2;
											num60 = grids[num58].count + packageItemCount2;
											int num63 = (num62 < num60) ? num62 : num60;
											if (num63 > grids[num58].recycleCount && num63 > 0)
											{
												int num64 = this.InsertIntoStoragePrecalc(itemId5, courierCarries + this.playerOrdered, false) - this.playerOrdered;
												if (num64 > 0)
												{
													int inc5;
													this.packageUtility.TakeItemFromAllPackages(num58, ref itemId5, ref num64, out inc5, false);
													if (num64 > 0)
													{
														this.workCourierDatas[j].itemId = itemId5;
														this.workCourierDatas[j].itemCount = num64;
														this.workCourierDatas[j].inc = inc5;
														this.workCourierDatas[j].endId = this.pairs[l].supplyId;
														this.orders[j].itemId = itemId4;
														this.orders[j].otherId = this.pairs[l].supplyId;
														this.orders[j].thisOrdered = num64;
														this.playerOrdered += num64;
														break;
													}
												}
											}
										}
									}
								}
							}
						}
						else
						{
							DispenserComponent dispenserComponent4 = dispenserPool[endId];
							this.storageOrdered -= this.orders[j].thisOrdered;
							this.orders[j].thisOrdered = 0;
							dispenserComponent4.storageOrdered -= this.orders[j].otherOrdered;
							this.orders[j].otherOrdered = 0;
							int inc6;
							int num65 = factory.PickFromStorage(dispenserComponent4.storage.bottomStorage.entityId, itemId4, num57, out inc6);
							this.workCourierDatas[j].itemId = itemId4;
							this.workCourierDatas[j].itemCount = num65;
							this.workCourierDatas[j].inc = inc6;
							factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, itemId4, num65);
							this.storageOrdered += num65;
							this.orders[j].thisOrdered = num65;
							dispenserComponent4.pulseSignal = 2;
						}
						this.workCourierDatas[j].direction = -1f;
					}
				}
				else if (this.workCourierDatas[j].t <= 0f)
				{
					int itemId6 = this.workCourierDatas[j].itemId;
					int itemCount2 = this.workCourierDatas[j].itemCount;
					if (itemId6 > 0 && itemCount2 > 0)
					{
						factory.gameData.statistics.traffic.RegisterPlanetInternalStat(factory.planetId, itemId6, itemCount2);
						bool useBan = this.orders[j].otherId >= 0;
						int num67;
						int num66 = factory.InsertIntoStorage(num, itemId6, itemCount2, this.workCourierDatas[j].inc, out num67, useBan);
						int num68 = itemCount2 - num66;
						if (num68 > 0)
						{
							bool flag4 = true;
							for (int m = 0; m < this.holdupItemCount; m++)
							{
								if (this.holdupPackage[m].itemId == itemId6)
								{
									DispenserStore[] array10 = this.holdupPackage;
									int num69 = m;
									array10[num69].count = array10[num69].count + num68;
									DispenserStore[] array11 = this.holdupPackage;
									int num70 = m;
									array11[num70].inc = array11[num70].inc + num67;
									flag4 = false;
									break;
								}
							}
							if (flag4)
							{
								this.holdupPackage[this.holdupItemCount].itemId = itemId6;
								this.holdupPackage[this.holdupItemCount].count = num68;
								this.holdupPackage[this.holdupItemCount].inc = num67;
								this.holdupItemCount++;
							}
						}
					}
					if (this.orders[j].otherId < 0)
					{
						this.playerOrdered -= this.orders[j].thisOrdered;
					}
					else if (this.orders[j].otherId > 0)
					{
						this.storageOrdered -= this.orders[j].thisOrdered;
					}
					this.orders[j].thisOrdered = 0;
					Array.Copy(this.workCourierDatas, j + 1, this.workCourierDatas, j, this.workCourierDatas.Length - j - 1);
					Array.Copy(this.orders, j + 1, this.orders, j, this.orders.Length - j - 1);
					this.workCourierCount--;
					this.idleCourierCount++;
					Array.Clear(this.workCourierDatas, this.workCourierCount, this.workCourierDatas.Length - this.workCourierCount);
					Array.Clear(this.orders, this.workCourierCount, this.orders.Length - this.workCourierCount);
					j--;
					this.pulseSignal = 2;
				}
			}
		}
		for (int n = 0; n < this.holdupItemCount; n++)
		{
			int count = this.holdupPackage[n].count;
			int inc7 = this.holdupPackage[n].inc;
			int inc8;
			int num71 = factory.InsertIntoStorage(this.storage.bottomStorage.entityId, this.holdupPackage[n].itemId, count, inc7, out inc8, true);
			this.holdupPackage[n].count = count - num71;
			this.holdupPackage[n].inc = inc8;
			if (this.holdupPackage[n].count == 0)
			{
				Assert.Zero(this.holdupPackage[n].inc);
				this.RemoveHoldupItem(n);
				n--;
			}
		}
		if (this.filter > 0)
		{
			num2 = (int)(time % 600L);
			if (num2 < 0)
			{
				num2 += 600;
			}
			if (num2 == this.gene)
			{
				if (this.storage.bottomStorage.grids[0].itemId == this.filter)
				{
					this.ResetPickSearchStart();
				}
				if (this.storage.topStorage.grids[this.storage.size - 1].itemId == 0)
				{
					this.ResetInsertSearchStart();
				}
			}
		}
		this.pulseSignal--;
	}

	// Token: 0x06000726 RID: 1830 RVA: 0x000406D8 File Offset: 0x0003E8D8
	public bool CheckDeliveryRange(Vector3 begin, Vector3 end, double deliveryRange, out double distance)
	{
		float x = begin.x;
		float y = begin.y;
		float z = begin.z;
		float x2 = end.x;
		float y2 = end.y;
		float z2 = end.z;
		double num = Math.Sqrt((double)(x * x + y * y + z * z));
		double num2 = Math.Sqrt((double)(x2 * x2 + y2 * y2 + z2 * z2));
		double num3 = (double)(x * x2 + y * y2 + z * z2) / (num * num2);
		if (num3 < -1.0)
		{
			num3 = -1.0;
		}
		else if (num3 > 1.0)
		{
			num3 = 1.0;
		}
		distance = Math.Acos(num3) * ((num + num2) * 0.5);
		distance = Math.Sqrt(distance * distance + (num - num2) * (num - num2));
		return num3 >= deliveryRange - 1E-06;
	}

	// Token: 0x06000727 RID: 1831 RVA: 0x000407C4 File Offset: 0x0003E9C4
	public int PickFromStoragePrecalc(int itemId, int needCnt)
	{
		if (this.pickStorageSearchStart == null)
		{
			return 0;
		}
		StorageComponent storageComponent = this.pickStorageSearchStart;
		StorageComponent storageComponent2 = storageComponent;
		int num = this.pickGridSearchStart;
		int num2 = num;
		int num3 = needCnt;
		int num4 = num3;
		int size = storageComponent2.size;
		StorageComponent.GRID[] grids = storageComponent2.grids;
		bool flag = true;
		bool flag2 = false;
		if (storageComponent2.lastEmptyItem == itemId)
		{
			flag2 = true;
		}
		else
		{
			if (grids[num2].itemId == itemId)
			{
				flag = false;
				needCnt -= grids[num2].count;
				if (needCnt <= 0)
				{
					return num3;
				}
			}
			else
			{
				if (grids[num2].itemId > 0)
				{
					flag = false;
				}
				flag2 = true;
			}
			num2++;
			if (num2 >= size)
			{
				num2 = 0;
			}
			for (;;)
			{
				if (grids[num2].itemId == itemId)
				{
					flag = false;
					if (flag2)
					{
						flag2 = false;
						this.pickStorageSearchStart = storageComponent2;
						this.pickGridSearchStart = num2;
					}
					needCnt -= grids[num2].count;
					if (needCnt <= 0)
					{
						break;
					}
				}
				else if (grids[num2].itemId > 0)
				{
					flag = false;
				}
				num2++;
				if (num2 >= size)
				{
					num2 = 0;
				}
				if (num2 == num)
				{
					goto Block_12;
				}
			}
			return num3;
			Block_12:
			if (needCnt == num4)
			{
				if (flag)
				{
					storageComponent2.lastEmptyItem = 0;
				}
				else
				{
					storageComponent2.lastEmptyItem = itemId;
				}
			}
			num4 = needCnt;
		}
		if (storageComponent2.nextStorage != null)
		{
			storageComponent2 = storageComponent2.nextStorage;
		}
		else
		{
			storageComponent2 = storageComponent2.bottomStorage;
		}
		grids = storageComponent2.grids;
		while (storageComponent2 != storageComponent)
		{
			flag = true;
			if (storageComponent2.lastEmptyItem != itemId)
			{
				for (int i = 0; i < size; i++)
				{
					if (grids[i].itemId == itemId)
					{
						flag = false;
						if (flag2)
						{
							flag2 = false;
							this.pickStorageSearchStart = storageComponent2;
							this.pickGridSearchStart = i;
						}
						needCnt -= grids[i].count;
						if (needCnt <= 0)
						{
							return num3;
						}
					}
					else if (grids[num2].itemId > 0)
					{
						flag = false;
					}
				}
				if (needCnt == num4)
				{
					if (flag)
					{
						storageComponent2.lastEmptyItem = 0;
					}
					else
					{
						storageComponent2.lastEmptyItem = itemId;
					}
				}
				num4 = needCnt;
			}
			if (storageComponent2.nextStorage != null)
			{
				storageComponent2 = storageComponent2.nextStorage;
			}
			else
			{
				storageComponent2 = storageComponent2.bottomStorage;
			}
			grids = storageComponent2.grids;
		}
		if (num3 == needCnt)
		{
			this.ResetPickSearchStart();
		}
		return num3 - needCnt;
	}

	// Token: 0x06000728 RID: 1832 RVA: 0x000409D4 File Offset: 0x0003EBD4
	public int InsertIntoStoragePrecalc(int itemId, int needCnt, bool useBan)
	{
		if (this.insertStorageSearchStart == null)
		{
			return 0;
		}
		StorageComponent storageComponent = this.insertStorageSearchStart;
		StorageComponent storageComponent2 = storageComponent;
		int num = needCnt;
		int num2 = num;
		int size = storageComponent2.size;
		StorageComponent.GRID[] grids = storageComponent2.grids;
		int num3 = useBan ? (size - storageComponent2.bans) : size;
		bool flag = false;
		if (num3 > 0)
		{
			int num4 = (this.insertGridSearchStart < num3 - 1) ? this.insertGridSearchStart : (num3 - 1);
			int i = num4;
			if (useBan && storageComponent2.lastFullItem == itemId)
			{
				flag = true;
			}
			else
			{
				if (grids[i].itemId == 0)
				{
					needCnt -= StorageComponent.itemStackCount[itemId];
					if (needCnt <= 0)
					{
						return num;
					}
				}
				else if (grids[i].itemId == itemId)
				{
					needCnt -= StorageComponent.itemStackCount[itemId] - grids[i].count;
					if (needCnt <= 0)
					{
						return num;
					}
				}
				else
				{
					flag = true;
				}
				for (i--; i < 0; i += num3)
				{
				}
				for (;;)
				{
					if (grids[i].itemId == 0)
					{
						if (flag)
						{
							flag = false;
							this.insertStorageSearchStart = storageComponent2;
							this.insertGridSearchStart = i;
						}
						needCnt -= StorageComponent.itemStackCount[itemId];
						if (needCnt <= 0)
						{
							break;
						}
					}
					else if (grids[i].itemId == itemId)
					{
						int num5 = StorageComponent.itemStackCount[itemId] - grids[i].count;
						if (num5 > 0)
						{
							if (flag)
							{
								flag = false;
								this.insertStorageSearchStart = storageComponent2;
								this.insertGridSearchStart = i;
							}
							needCnt -= num5;
							if (needCnt <= 0)
							{
								return num;
							}
						}
					}
					for (i--; i < 0; i += num3)
					{
					}
					if (i == num4)
					{
						goto Block_19;
					}
				}
				return num;
				Block_19:
				if (needCnt == num2)
				{
					storageComponent2.lastFullItem = itemId;
				}
				num2 = needCnt;
			}
		}
		else
		{
			flag = true;
		}
		if (storageComponent2.previousStorage != null)
		{
			storageComponent2 = storageComponent2.previousStorage;
		}
		else
		{
			storageComponent2 = storageComponent2.topStorage;
		}
		grids = storageComponent2.grids;
		while (storageComponent2 != storageComponent)
		{
			if (!useBan || storageComponent2.lastFullItem != itemId)
			{
				num3 = (useBan ? (size - storageComponent2.bans) : size);
				for (int j = num3 - 1; j >= 0; j--)
				{
					if (grids[j].itemId == 0)
					{
						if (flag)
						{
							flag = false;
							this.insertStorageSearchStart = storageComponent2;
							this.insertGridSearchStart = j;
						}
						needCnt -= StorageComponent.itemStackCount[itemId];
						if (needCnt <= 0)
						{
							return num;
						}
					}
					else if (grids[j].itemId == itemId)
					{
						int num6 = StorageComponent.itemStackCount[itemId] - grids[j].count;
						if (num6 > 0)
						{
							if (flag)
							{
								flag = false;
								this.insertStorageSearchStart = storageComponent2;
								this.insertGridSearchStart = j;
							}
							needCnt -= num6;
							if (needCnt <= 0)
							{
								return num;
							}
						}
					}
				}
				if (needCnt == num2)
				{
					storageComponent2.lastFullItem = itemId;
				}
				num2 = needCnt;
			}
			if (storageComponent2.previousStorage != null)
			{
				storageComponent2 = storageComponent2.previousStorage;
			}
			else
			{
				storageComponent2 = storageComponent2.topStorage;
			}
			grids = storageComponent2.grids;
		}
		if (num == needCnt)
		{
			this.ResetInsertSearchStart();
		}
		return num - needCnt;
	}

	// Token: 0x06000729 RID: 1833 RVA: 0x00040CA0 File Offset: 0x0003EEA0
	public void RemoveHoldupItem(int holdupItemIndex)
	{
		Array.Copy(this.holdupPackage, holdupItemIndex + 1, this.holdupPackage, holdupItemIndex, this.holdupPackage.Length - holdupItemIndex - 1);
		this.holdupItemCount--;
		Array.Clear(this.holdupPackage, this.holdupItemCount, this.holdupPackage.Length - this.holdupItemCount);
	}

	// Token: 0x0600072A RID: 1834 RVA: 0x00040CFC File Offset: 0x0003EEFC
	public void ResetSearchStart()
	{
		if (this.storage == null)
		{
			return;
		}
		this.ResetInsertSearchStart();
		this.ResetPickSearchStart();
	}

	// Token: 0x0600072B RID: 1835 RVA: 0x00040D13 File Offset: 0x0003EF13
	private void ResetPickSearchStart()
	{
		this.pickStorageSearchStart = this.storage.bottomStorage;
		this.pickGridSearchStart = 0;
	}

	// Token: 0x0600072C RID: 1836 RVA: 0x00040D2D File Offset: 0x0003EF2D
	private void ResetInsertSearchStart()
	{
		this.insertStorageSearchStart = this.storage;
		this.insertGridSearchStart = this.storage.size - 1;
		if (this.insertGridSearchStart < 0)
		{
			this.insertGridSearchStart = 0;
		}
	}

	// Token: 0x0600072D RID: 1837 RVA: 0x00040D60 File Offset: 0x0003EF60
	public void GuessFilter(PlanetFactory factory)
	{
		if (this.filter != 0)
		{
			return;
		}
		if (this.storage != null)
		{
			StorageComponent previousStorage = this.storage;
			StorageComponent bottomStorage = previousStorage.bottomStorage;
			int size = previousStorage.size;
			int num = 0;
			while (previousStorage != null)
			{
				StorageComponent.GRID[] grids = previousStorage.grids;
				for (int i = 0; i < size; i++)
				{
					int itemId = grids[i].itemId;
					if (itemId > 0)
					{
						if (num == 0)
						{
							num = itemId;
						}
						else if (num != itemId)
						{
							num = -1;
							break;
						}
					}
				}
				if (num == -1 || previousStorage == bottomStorage)
				{
					break;
				}
				previousStorage = previousStorage.previousStorage;
			}
			if (num > 0)
			{
				factory.transport.SetDispenserFilter(this.id, num);
				return;
			}
			if (num < 0)
			{
				factory.transport.SetDispenserFilter(this.id, 0);
				return;
			}
		}
		EntityData[] entityPool = factory.entityPool;
		PrebuildData[] prebuildPool = factory.prebuildPool;
		CargoTraffic cargoTraffic = factory.cargoTraffic;
		BeltComponent[] beltPool = cargoTraffic.beltPool;
		Cargo[] cargoPool = cargoTraffic.container.cargoPool;
		InserterComponent[] inserterPool = factory.factorySystem.inserterPool;
		AssemblerComponent[] assemblerPool = factory.factorySystem.assemblerPool;
		LabComponent[] labPool = factory.factorySystem.labPool;
		int num2 = 0;
		int num3 = 0;
		bool flag;
		int num4;
		int num5;
		factory.ReadObjectConn(this.entityId, 0, out flag, out num4, out num5);
		for (int objId = num4; objId != 0; objId = num4)
		{
			for (int j = 0; j < 12; j++)
			{
				factory.ReadObjectConn(objId, j, out flag, out num4, out num5);
				int num6 = num4;
				if (num6 != 0)
				{
					int filterId;
					if (num6 > 0)
					{
						filterId = inserterPool[entityPool[num6].inserterId].filter;
					}
					else
					{
						filterId = prebuildPool[-num6].filterId;
					}
					if (flag)
					{
						if (filterId > 0)
						{
							if (num3 == 0)
							{
								num3 = filterId;
							}
							else if (num3 != filterId)
							{
								num3 = -1;
							}
						}
					}
					else
					{
						int num7 = 0;
						if (filterId > 0)
						{
							num7 = filterId;
						}
						else
						{
							factory.ReadObjectConn(num6, 1, out flag, out num4, out num5);
							int num8 = num4;
							if (num8 == 0)
							{
								goto IL_48B;
							}
							if (num8 > 0)
							{
								int beltId = entityPool[num8].beltId;
								if (beltId > 0)
								{
									CargoPath cargoPath = cargoTraffic.GetCargoPath(beltPool[beltId].segPathId);
									byte[] buffer = cargoPath.buffer;
									int num9 = beltPool[beltId].segIndex + beltPool[beltId].segPivotOffset;
									int num10 = num9 - 2000;
									int num11 = num9 + 2000;
									if (num10 < 4)
									{
										num10 = 4;
									}
									if (num11 >= cargoPath.pathLength - 5)
									{
										num11 = cargoPath.pathLength - 5 - 1;
									}
									for (int k = num10; k < num11; k++)
									{
										if (buffer[k] == 250)
										{
											int num12 = (int)(buffer[k + 1] - 1 + (buffer[k + 2] - 1) * 100) + (int)(buffer[k + 3] - 1) * 10000 + (int)(buffer[k + 4] - 1) * 1000000;
											int item = (int)cargoPool[num12].item;
											if (item > 0)
											{
												if (num7 == 0)
												{
													num7 = item;
												}
												else if (num7 != item)
												{
													num7 = -1;
													break;
												}
											}
											k += 9;
										}
										else if (buffer[k] == 0)
										{
											k += 4;
										}
									}
									if (num7 == -1)
									{
										factory.transport.SetDispenserFilter(this.id, 0);
										return;
									}
									if (num7 == 0)
									{
										List<int> belts = cargoPath.belts;
										int count = cargoPath.belts.Count;
										for (int l = 0; l < count; l++)
										{
											int num13 = beltPool[belts[l]].entityId;
											int iconId = (int)factory.entitySignPool[num13].iconId0;
											if (iconId > 0)
											{
												ItemProto itemProto = LDB.items.Select(iconId);
												if (itemProto != null)
												{
													if (num3 == 0)
													{
														num3 = itemProto.ID;
													}
													else if (num3 != itemProto.ID)
													{
														num3 = -1;
													}
												}
											}
										}
									}
								}
							}
							int num14 = 0;
							if (num8 > 0)
							{
								int assemblerId = entityPool[num8].assemblerId;
								if (assemblerId > 0)
								{
									num14 = assemblerPool[assemblerId].recipeId;
								}
								int labId = entityPool[num8].labId;
								if (labId > 0)
								{
									num14 = (labPool[labId].researchMode ? 0 : labPool[labId].recipeId);
								}
							}
							else if (num8 < 0)
							{
								num14 = prebuildPool[-num8].recipeId;
							}
							if (num14 > 0)
							{
								RecipeProto recipeProto = LDB.recipes.Select(num14);
								if (recipeProto != null && recipeProto.Results.Length == 1)
								{
									num7 = recipeProto.Results[0];
								}
							}
						}
						if (num7 > 0)
						{
							if (num2 == 0)
							{
								num2 = num7;
							}
							else if (num2 != num7)
							{
								num2 = -1;
							}
						}
						if (num2 == -1)
						{
							factory.transport.SetDispenserFilter(this.id, 0);
							return;
						}
					}
				}
				IL_48B:;
			}
			factory.ReadObjectConn(objId, 14, out flag, out num4, out num5);
		}
		if (num2 >= 0)
		{
			factory.transport.SetDispenserFilter(this.id, num2);
		}
		if (this.filter == 0 && num3 > 0)
		{
			factory.transport.SetDispenserFilter(this.id, num3);
		}
	}

	// Token: 0x0600072E RID: 1838 RVA: 0x0004125C File Offset: 0x0003F45C
	private int split_inc(ref int n, ref int m, int p)
	{
		int num = m / n;
		int num2 = m - num * n;
		n -= p;
		num2 -= n;
		num = ((num2 > 0) ? (num * p + num2) : (num * p));
		m -= num;
		return num;
	}

	// Token: 0x040005C7 RID: 1479
	public int id;

	// Token: 0x040005C8 RID: 1480
	public int entityId;

	// Token: 0x040005C9 RID: 1481
	public int pcId;

	// Token: 0x040005CA RID: 1482
	public int storageId;

	// Token: 0x040005CB RID: 1483
	public int gene;

	// Token: 0x040005CC RID: 1484
	public long energy;

	// Token: 0x040005CD RID: 1485
	public long energyPerTick;

	// Token: 0x040005CE RID: 1486
	public long energyMax;

	// Token: 0x040005CF RID: 1487
	public EPlayerDeliveryMode playerMode;

	// Token: 0x040005D0 RID: 1488
	public EStorageDeliveryMode storageMode;

	// Token: 0x040005D1 RID: 1489
	public int filter;

	// Token: 0x040005D2 RID: 1490
	public int idleCourierCount;

	// Token: 0x040005D3 RID: 1491
	public int workCourierCount;

	// Token: 0x040005D4 RID: 1492
	public bool courierAutoReplenish;

	// Token: 0x040005D5 RID: 1493
	public CourierData[] workCourierDatas;

	// Token: 0x040005D6 RID: 1494
	public DeliveryLogisticOrder[] orders;

	// Token: 0x040005D7 RID: 1495
	public int holdupItemCount;

	// Token: 0x040005D8 RID: 1496
	public DispenserStore[] holdupPackage;

	// Token: 0x040005D9 RID: 1497
	public int playerOrdered;

	// Token: 0x040005DA RID: 1498
	public int storageOrdered;

	// Token: 0x040005DB RID: 1499
	public int pairProcess;

	// Token: 0x040005DC RID: 1500
	public int pulseSignal;

	// Token: 0x040005DD RID: 1501
	public StorageComponent storage;

	// Token: 0x040005DE RID: 1502
	public SupplyDemandPair[] pairs;

	// Token: 0x040005DF RID: 1503
	public int playerPairCount;

	// Token: 0x040005E0 RID: 1504
	public int pairCount;

	// Token: 0x040005E1 RID: 1505
	public StorageComponent insertStorageSearchStart;

	// Token: 0x040005E2 RID: 1506
	public int insertGridSearchStart;

	// Token: 0x040005E3 RID: 1507
	public StorageComponent pickStorageSearchStart;

	// Token: 0x040005E4 RID: 1508
	public int pickGridSearchStart;

	// Token: 0x040005E5 RID: 1509
	public DeliveryPackage deliveryPackage;

	// Token: 0x040005E6 RID: 1510
	public PlayerPackageUtility packageUtility;

	// Token: 0x040005E7 RID: 1511
	public DispenserComponent.EPlayerDeliveryCondition playerDeliveryCondition;

	// Token: 0x040005E8 RID: 1512
	private const int MAX_PICK_COUNT_PER_TICK = 10;

	// Token: 0x040005E9 RID: 1513
	private int _tmp_iter;

	// Token: 0x040005EA RID: 1514
	private const float TICK_DELTA_TIME = 0.016666668f;

	// Token: 0x040005EB RID: 1515
	private const int PULSE_TICK = 2;

	// Token: 0x040005EC RID: 1516
	public const long courierEnergyPerMeter = 10000L;

	// Token: 0x040005ED RID: 1517
	public const long courierEnergyTakeOff = 100000L;

	// Token: 0x040005EE RID: 1518
	private const double eps = 1E-06;

	// Token: 0x02000BFC RID: 3068
	public enum EPlayerDeliveryCondition
	{
		// Token: 0x04007BFC RID: 31740
		None,
		// Token: 0x04007BFD RID: 31741
		Traversed,
		// Token: 0x04007BFE RID: 31742
		NoCourier = 1,
		// Token: 0x04007BFF RID: 31743
		HasCourier,
		// Token: 0x04007C00 RID: 31744
		NoPair = 2,
		// Token: 0x04007C01 RID: 31745
		HasPair,
		// Token: 0x04007C02 RID: 31746
		OutOfRange = 3,
		// Token: 0x04007C03 RID: 31747
		InRange,
		// Token: 0x04007C04 RID: 31748
		NoEnergy = 4,
		// Token: 0x04007C05 RID: 31749
		EnergyEnough,
		// Token: 0x04007C06 RID: 31750
		DoPair = 5
	}
}
