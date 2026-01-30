using System;
using UnityEngine;

// Token: 0x02000081 RID: 129
public class BattleBaseDesc : MonoBehaviour
{
	// Token: 0x04000460 RID: 1120
	public long maxEnergyAcc;

	// Token: 0x04000461 RID: 1121
	public float pickRange;
}

using System;
using System.IO;
using UnityEngine;

// Token: 0x02000080 RID: 128
public class BattleBaseComponent : IPoolElement
{
	// Token: 0x170000F2 RID: 242
	// (get) Token: 0x06000595 RID: 1429 RVA: 0x0002D011 File Offset: 0x0002B211
	// (set) Token: 0x06000596 RID: 1430 RVA: 0x0002D019 File Offset: 0x0002B219
	public int ID
	{
		get
		{
			return this.id;
		}
		set
		{
			this.id = value;
		}
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x0002D024 File Offset: 0x0002B224
	public void Init(int _entityId, PrefabDesc _desc, PlanetFactory factory)
	{
		this.entityId = _entityId;
		ref EntityData ptr = ref factory.entityPool[_entityId];
		this.pcId = ptr.powerConId;
		this.storageId = ptr.storageId;
		this.constructionModuleId = ptr.constructionModuleId;
		this.constructionModule = factory.constructionSystem.constructionModules.buffer[this.constructionModuleId];
		this.constructionModule.battleBaseId = this.id;
		this.combatModuleId = ptr.combatModuleId;
		this.combatModule = factory.combatGroundSystem.combatModules.buffer[this.combatModuleId];
		this.combatModule.battleBaseId = this.id;
		this.energy = 0L;
		this.energyPerTick = _desc.workEnergyPerTick;
		this.energyMax = _desc.battleBaseMaxEnergyAcc;
		this.autoPickEnabled = true;
		this.constructRange = _desc.constructionRange;
		this.pickRange = _desc.battleBasePickRange;
		this.storage = factory.factoryStorage.storagePool[this.storageId];
		this.storageStartTakeFrom = null;
		this.ejectPos = this.constructionModule.baseEjectPos;
		this.history = factory.gameData.history;
	}

	// Token: 0x06000598 RID: 1432 RVA: 0x0002D154 File Offset: 0x0002B354
	public void Reset()
	{
		this.id = 0;
		this.entityId = 0;
		this.pcId = 0;
		this.storageId = 0;
		this.constructionModuleId = 0;
		this.combatModuleId = 0;
		this.energy = 0L;
		this.energyPerTick = 0L;
		this.energyMax = 0L;
		this.autoPickEnabled = false;
		this.constructRange = 0f;
		this.pickRange = 0f;
	}

	// Token: 0x06000599 RID: 1433 RVA: 0x0002D1C0 File Offset: 0x0002B3C0
	public void Free()
	{
		this.storage = null;
		this.storageStartTakeFrom = null;
		this.constructionModule = null;
		this.combatModule = null;
		this.history = null;
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x0002D1E8 File Offset: 0x0002B3E8
	public void Export(BinaryWriter w)
	{
		w.Write(1);
		w.Write(this.id);
		w.Write(this.entityId);
		w.Write(this.pcId);
		w.Write(this.storageId);
		w.Write(this.constructionModuleId);
		w.Write(this.combatModuleId);
		w.Write(this.energy);
		w.Write(this.energyPerTick);
		w.Write(this.energyMax);
		w.Write(this.autoPickEnabled);
		w.Write(this.constructRange);
		w.Write(this.pickRange);
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x0002D28C File Offset: 0x0002B48C
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.id = r.ReadInt32();
		this.entityId = r.ReadInt32();
		this.pcId = r.ReadInt32();
		this.storageId = r.ReadInt32();
		this.constructionModuleId = r.ReadInt32();
		this.combatModuleId = r.ReadInt32();
		this.energy = r.ReadInt64();
		this.energyPerTick = r.ReadInt64();
		this.energyMax = r.ReadInt64();
		this.autoPickEnabled = r.ReadBoolean();
		this.constructRange = r.ReadSingle();
		if (num >= 1)
		{
			this.pickRange = r.ReadSingle();
			return;
		}
		ModelProto modelProto = LDB.models.modelArray[453];
		if (modelProto != null)
		{
			this.pickRange = modelProto.prefabDesc.battleBasePickRange;
		}
	}

	// Token: 0x0600059C RID: 1436 RVA: 0x0002D358 File Offset: 0x0002B558
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

	// Token: 0x0600059D RID: 1437 RVA: 0x0002D3E0 File Offset: 0x0002B5E0
	public void InternalUpdate(float dt, PlanetFactory factory, float power, ref AnimData anim)
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
		anim.power = power;
		anim.state = ((this.energy > 0L) ? 1U : 0U);
		anim.Step(anim.state, dt * power * 0.5f);
	}

	// Token: 0x0600059E RID: 1438 RVA: 0x0002D47C File Offset: 0x0002B67C
	public void AutoPickTrash(PlanetFactory factory, TrashSystem trashSystem, long time, ref VectorLF3 relativePos, ref Quaternion relativeRot, int[] productRegister)
	{
		ref EntityData ptr = ref factory.entityPool[this.entityId];
		if (ptr.id != this.entityId)
		{
			return;
		}
		Vector3 pos = ptr.pos;
		float x = pos.x;
		float y = pos.y;
		float z = pos.z;
		int astroId = factory.planet.astroId;
		TrashContainer container = trashSystem.container;
		TrashObject[] trashObjPool = container.trashObjPool;
		TrashData[] trashDataPool = container.trashDataPool;
		int trashCursor = container.trashCursor;
		float num = this.pickRange * this.pickRange;
		int num2 = 4;
		int num3 = (int)((time * (long)num2 + (long)this.id) % 1000000000L);
		int num4 = num3 + trashCursor;
		int num5 = 4;
		for (int i = num3; i < num4; i++)
		{
			int num6 = i % trashCursor;
			if (trashObjPool[num6].item > 0 && trashObjPool[num6].expire < 0 && trashDataPool[num6].nearPlanetId == astroId)
			{
				float num7 = trashDataPool[num6].lPos.x - x;
				float num8 = num7 * num7;
				if (num8 < num)
				{
					float num9 = trashDataPool[num6].lPos.y - y;
					float num10 = num9 * num9;
					if (num10 < num)
					{
						float num11 = trashDataPool[num6].lPos.z - z;
						float num12 = num11 * num11;
						if (num12 < num && num8 + num10 + num12 < num)
						{
							int item = trashObjPool[num6].item;
							int count = trashObjPool[num6].count;
							int inc = trashObjPool[num6].inc;
							if (item == 1099)
							{
								Player mainPlayer = factory.gameData.mainPlayer;
								Player obj = mainPlayer;
								lock (obj)
								{
									mainPlayer.SetSandCount(mainPlayer.sandCount + (long)count, ESandSource.DarkFog);
									mainPlayer.NotifySandCollectFromTrash(count);
									trashSystem.RemoveTrash(num6);
									goto IL_2AD;
								}
							}
							int num13 = count;
							int inc2 = inc;
							StorageComponent nextStorage = this.storage;
							while (num13 > 0 && nextStorage != null)
							{
								int num15;
								int num14 = nextStorage.AddItemFiltered(item, num13, inc2, out num15, true);
								num13 -= num14;
								inc2 = num15;
								nextStorage = nextStorage.nextStorage;
							}
							trashObjPool[num6].count = num13;
							trashObjPool[num6].inc = inc2;
							if (trashDataPool[num6].life > 0)
							{
								lock (productRegister)
								{
									productRegister[item] += count - num13;
								}
							}
							if (num13 == 0)
							{
								trashSystem.RemoveTrash(num6);
							}
							if (--num5 <= 0)
							{
								break;
							}
						}
					}
				}
			}
			IL_2AD:;
		}
	}

	// Token: 0x0600059F RID: 1439 RVA: 0x0002D764 File Offset: 0x0002B964
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

	// Token: 0x060005A0 RID: 1440 RVA: 0x0002D7A0 File Offset: 0x0002B9A0
	public bool EnergyIsLow()
	{
		return (float)this.energy / (float)this.energyMax < 0.1f;
	}

	// Token: 0x0400044E RID: 1102
	public int id;

	// Token: 0x0400044F RID: 1103
	public int entityId;

	// Token: 0x04000450 RID: 1104
	public int pcId;

	// Token: 0x04000451 RID: 1105
	public int storageId;

	// Token: 0x04000452 RID: 1106
	public int constructionModuleId;

	// Token: 0x04000453 RID: 1107
	public int combatModuleId;

	// Token: 0x04000454 RID: 1108
	public long energy;

	// Token: 0x04000455 RID: 1109
	public long energyPerTick;

	// Token: 0x04000456 RID: 1110
	public long energyMax;

	// Token: 0x04000457 RID: 1111
	public bool autoPickEnabled;

	// Token: 0x04000458 RID: 1112
	public float constructRange;

	// Token: 0x04000459 RID: 1113
	public float pickRange;

	// Token: 0x0400045A RID: 1114
	public StorageComponent storage;

	// Token: 0x0400045B RID: 1115
	public StorageComponent storageStartTakeFrom;

	// Token: 0x0400045C RID: 1116
	public ConstructionModuleComponent constructionModule;

	// Token: 0x0400045D RID: 1117
	public CombatModuleComponent combatModule;

	// Token: 0x0400045E RID: 1118
	public Vector3 ejectPos;

	// Token: 0x0400045F RID: 1119
	public GameHistoryData history;
}

using System;

// Token: 0x020002A3 RID: 675
public class BatchTaskContext
{
	// Token: 0x06001DFB RID: 7675 RVA: 0x00201716 File Offset: 0x001FF916
	public void Init(int maxBatchCount, int maxWorkerThreadCount)
	{
		this.batchCount = 0;
		this.workerThreadCount = 0;
		this.batchValues = new int[maxBatchCount];
		this.batchIndices = new int[maxBatchCount];
		this.batchCursor = 0;
	}

	// Token: 0x06001DFC RID: 7676 RVA: 0x00201749 File Offset: 0x001FF949
	public void Free()
	{
		this.batchCount = 0;
		this.workerThreadCount = 0;
		this.batchValues = null;
		this.batchIndices = null;
		this.batchCursor = 0;
	}

	// Token: 0x06001DFD RID: 7677 RVA: 0x00201774 File Offset: 0x001FF974
	public void ResetFrame(int _batchCount, int _workerThreadCount)
	{
		Array.Clear(this.batchValues, 0, this.batchValues.Length);
		Array.Clear(this.batchIndices, 0, this.batchIndices.Length);
		this.batchCursor = 0;
		for (int i = 0; i < this.batchIndices.Length; i++)
		{
			this.batchIndices[i] = i;
		}
		this.batchCount = _batchCount;
		this.workerThreadCount = _workerThreadCount;
	}

	// Token: 0x06001DFE RID: 7678 RVA: 0x002017E0 File Offset: 0x001FF9E0
	public void SortValues()
	{
		for (int i = 0; i < this.batchCount - 1; i++)
		{
			for (int j = i + 1; j < this.batchCount; j++)
			{
				if (this.batchValues[i] < this.batchValues[j])
				{
					int num = this.batchValues[i];
					this.batchValues[i] = this.batchValues[j];
					this.batchValues[j] = num;
					num = this.batchIndices[i];
					this.batchIndices[i] = this.batchIndices[j];
					this.batchIndices[j] = num;
				}
			}
		}
	}

	// Token: 0x0400254E RID: 9550
	public volatile int batchCount;

	// Token: 0x0400254F RID: 9551
	public volatile int workerThreadCount;

	// Token: 0x04002550 RID: 9552
	public int[] batchValues;

	// Token: 0x04002551 RID: 9553
	public int[] batchIndices;

	// Token: 0x04002552 RID: 9554
	public int batchCursor;

	// Token: 0x04002553 RID: 9555
	public SimpleLock occupy;
}
