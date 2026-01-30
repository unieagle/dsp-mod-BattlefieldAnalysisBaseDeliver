using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x0200016C RID: 364
public class DefenseSystem
{
	// Token: 0x06000BC6 RID: 3014 RVA: 0x000B0A3C File Offset: 0x000AEC3C
	public DefenseSystem(PlanetData _planet)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.spaceSector = this.factory.sector;
		this.turrets = new DataPool<TurretComponent>();
		this.beacons = new DataPool<BeaconComponent>();
		this.fieldGenerators = new DataPool<FieldGeneratorComponent>();
		this.battleBases = new ObjectPool<BattleBaseComponent>();
		this.SetGlobalTargetsCapacity(8);
		this.spaceUniqueGlobalTargets = new HashSet<int>();
		this.localGlobalTargetCursor = 0;
		this.otherGlobalTargetCursor = 0;
		this.turrets.Reset();
		this.beacons.Reset();
		this.fieldGenerators.Reset();
		this.fieldGenerators.SetCapacity(81);
		this.battleBases.Reset();
		this.matchHiveLevel0 = new List<int>();
	}

	// Token: 0x06000BC7 RID: 3015 RVA: 0x000B0B14 File Offset: 0x000AED14
	public DefenseSystem(PlanetData _planet, bool import)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.spaceSector = this.factory.sector;
		this.turrets = new DataPool<TurretComponent>();
		this.beacons = new DataPool<BeaconComponent>();
		this.fieldGenerators = new DataPool<FieldGeneratorComponent>();
		this.battleBases = new ObjectPool<BattleBaseComponent>();
		this.spaceUniqueGlobalTargets = new HashSet<int>();
		this.matchHiveLevel0 = new List<int>();
	}

	// Token: 0x06000BC8 RID: 3016 RVA: 0x000B0BA0 File Offset: 0x000AEDA0
	public void Free()
	{
		if (this.turrets != null)
		{
			this.turrets.Free();
			this.turrets = null;
		}
		if (this.beacons != null)
		{
			this.beacons.Free();
			this.beacons = null;
		}
		if (this.fieldGenerators != null)
		{
			this.fieldGenerators.Free();
			this.fieldGenerators = null;
		}
		if (this.battleBases != null)
		{
			this.battleBases.Free();
			this.battleBases = null;
		}
		if (this.globalTargets != null)
		{
			this.localGlobalTargetCursor = 0;
			this.otherGlobalTargetCursor = 0;
			this.spaceUniqueGlobalTargets.Clear();
			this.spaceUniqueGlobalTargets = null;
			this.globalTargets = null;
		}
		this.engagingGaussCount = 0;
		this.engagingLaserCount = 0;
		this.engagingCannonCount = 0;
		this.engagingMissileCount = 0;
		this.engagingPlasmaCount = 0;
		this.engagingLocalPlasmaCount = 0;
		this.engagingTurretTotalCount = 0;
		this.turretEnableDefenseSpace = false;
		this.planet = null;
		this.factory = null;
		this.spaceSector = null;
		this.turretSearchPairCount = 0;
		this.turretSearchPairs = null;
		this.matchHiveLevel0 = null;
	}

	// Token: 0x06000BC9 RID: 3017 RVA: 0x000B0CA8 File Offset: 0x000AEEA8
	public void Export(BinaryWriter w)
	{
		w.Write(2);
		this.turrets.Export(w);
		this.beacons.Export(w);
		this.fieldGenerators.Export(w);
		this.battleBases.Export(w);
		w.Write(this.localGlobalTargetCursor);
		w.Write(this.otherGlobalTargetCursor);
		for (int i = 0; i < this.otherGlobalTargetCursor; i++)
		{
			this.globalTargets[i].Export(w);
		}
		if (this.turrets.count > 0)
		{
			w.Write(this.engagingGaussCount);
			w.Write(this.engagingLaserCount);
			w.Write(this.engagingCannonCount);
			w.Write(this.engagingMissileCount);
			w.Write(this.engagingPlasmaCount);
			w.Write(this.engagingLocalPlasmaCount);
			w.Write(this.engagingTurretTotalCount);
			w.Write(this.turretEnableDefenseSpace);
		}
	}

	// Token: 0x06000BCA RID: 3018 RVA: 0x000B0D98 File Offset: 0x000AEF98
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.turrets.Import(r);
		this.AfterTurretsImport();
		this.beacons.Import(r);
		this.fieldGenerators.Import(r);
		this.battleBases.Import(r);
		this.AfterBattleBaseImport();
		if (num >= 1)
		{
			this.localGlobalTargetCursor = r.ReadInt32();
			this.otherGlobalTargetCursor = r.ReadInt32();
			int num2 = this.otherGlobalTargetCursor;
			int num3 = 8;
			int num4 = 21;
			while (num2 > num3 && num4 > 0)
			{
				num3 *= 2;
				num4--;
			}
			this.SetGlobalTargetsCapacity(num3);
			for (int i = 0; i < num2; i++)
			{
				this.globalTargets[i].Import(r, this.planet.astroId);
			}
		}
		else
		{
			int num5 = r.ReadInt32();
			int num6 = 8;
			int num7 = 21;
			while (num5 > num6 && num7 > 0)
			{
				num6 *= 2;
				num7--;
			}
			this.SetGlobalTargetsCapacity(num6);
			for (int j = 0; j < num5; j++)
			{
				this.globalTargets[j].Import(r, this.planet.astroId);
			}
		}
		if (this.factory.gameData.patch < 14)
		{
			Array.Clear(this.globalTargets, 0, this.globalTargets.Length);
			this.localGlobalTargetCursor = 0;
			this.otherGlobalTargetCursor = 0;
		}
		if (this.turrets.count > 0)
		{
			this.engagingGaussCount = r.ReadInt32();
			this.engagingLaserCount = r.ReadInt32();
			this.engagingCannonCount = r.ReadInt32();
			this.engagingMissileCount = r.ReadInt32();
			this.engagingPlasmaCount = r.ReadInt32();
			if (num >= 2)
			{
				this.engagingLocalPlasmaCount = r.ReadInt32();
			}
			this.engagingTurretTotalCount = r.ReadInt32();
			this.turretEnableDefenseSpace = r.ReadBoolean();
		}
	}

	// Token: 0x06000BCB RID: 3019 RVA: 0x000B0F60 File Offset: 0x000AF160
	public int NewTurretComponent(int entityId, PrefabDesc desc)
	{
		ref TurretComponent ptr = ref this.turrets.Add();
		ptr.entityId = entityId;
		ptr.target.type = ETargetType.Enemy;
		ptr.activeSearch = true;
		ptr.type = desc.turretType;
		ptr.ammoType = desc.turretAmmoType;
		ptr.vsSettings = (ptr.vsCaps = desc.turretVSCaps);
		if ((ptr.vsCaps & VSLayerMask.OrbitAndSpace) == VSLayerMask.OrbitAndSpace)
		{
			ptr.vsSettings &= VSLayerMask.GroundAndAirAndSpace;
		}
		else if (ptr.type == ETurretType.Cannon)
		{
			ptr.vsSettings &= VSLayerMask.CannonDefaultSetting;
		}
		else if (ptr.type == ETurretType.LocalPlasma)
		{
			ptr.vsSettings &= VSLayerMask.CannonDefaultSetting;
		}
		ptr.localTargetDir = (ptr.localDir = desc.turretDefaultDir);
		ref EntityData ptr2 = ref this.factory.entityPool[entityId];
		ptr.pcId = ptr2.powerConId;
		ptr.muzzleWPos = ptr.CalculateMuzzleWorldPosition(desc.turretMuzzleY, ref ptr2);
		if (ptr.type == ETurretType.Laser)
		{
			ptr.bulletCount = 1;
		}
		ptr2.turretId = ptr.id;
		if (this.factory.planet.factoryLoaded)
		{
			this.factory.planet.factoryModel.RefreshTurrets();
		}
		this.MatchTurretPair(ref ptr);
		return ptr.id;
	}

	// Token: 0x06000BCC RID: 3020 RVA: 0x000B10A8 File Offset: 0x000AF2A8
	public void RemoveTurretComponent(int id)
	{
		this.turrets.buffer[id].BeforeRemove(this.spaceSector.skillSystem);
		this.turrets.Remove(id);
		if (id != 0 && this.factory.planet.factoryLoaded)
		{
			this.factory.planet.factoryModel.RefreshTurrets();
		}
		this.RefreshTurretSearchPair();
	}

	// Token: 0x06000BCD RID: 3021 RVA: 0x000B1114 File Offset: 0x000AF314
	public void AfterTurretsImport()
	{
		EntityData[] entityPool = this.factory.entityPool;
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ptr.muzzleWPos = ptr.CalculateMuzzleWorldPosition(PlanetFactory.PrefabDescByModelIndex[(int)entityPool[ptr.entityId].modelIndex].turretMuzzleY, ref entityPool[ptr.entityId]);
				ptr.lastTotalKillCount = ptr.totalKillCount;
				this.MatchTurretPair(ref ptr);
			}
		}
	}

	// Token: 0x06000BCE RID: 3022 RVA: 0x000B11B4 File Offset: 0x000AF3B4
	public int NewBeaconComponent(int entityId, PrefabDesc desc)
	{
		ref BeaconComponent ptr = ref this.beacons.Add();
		ptr.entityId = entityId;
		this.factory.entityPool[entityId].beaconId = ptr.id;
		ptr.pnId = this.factory.entityPool[entityId].powerNodeId;
		return ptr.id;
	}

	// Token: 0x06000BCF RID: 3023 RVA: 0x000B1212 File Offset: 0x000AF412
	public void RemoveBeaconComponent(int id)
	{
		this.beacons.Remove(id);
	}

	// Token: 0x06000BD0 RID: 3024 RVA: 0x000B1220 File Offset: 0x000AF420
	public int NewFieldGeneratorComponent(int entityId, PrefabDesc desc)
	{
		ref FieldGeneratorComponent ptr = ref this.fieldGenerators.Add();
		ptr.entityId = entityId;
		ref EntityData ptr2 = ref this.factory.entityPool[entityId];
		ptr.pcId = ptr2.powerConId;
		ptr.holder.x = ptr2.pos.x;
		ptr.holder.y = ptr2.pos.y;
		ptr.holder.z = ptr2.pos.z;
		ptr.energy = 0L;
		ptr.energyCapacity = desc.fieldGenEnergyCapacity;
		ptr.energyRequire0 = desc.fieldGenEnergyRequire0;
		ptr.energyRequire1 = desc.fieldGenEnergyRequire1;
		ptr2.fieldGenId = ptr.id;
		this.factory.planetATField.SetPhysicsChangeSensitivity(10000f);
		return ptr.id;
	}

	// Token: 0x06000BD1 RID: 3025 RVA: 0x000B12F3 File Offset: 0x000AF4F3
	public void RemoveFieldGeneratorComponent(int id)
	{
		this.fieldGenerators.Remove(id);
		this.factory.planetATField.SetPhysicsChangeSensitivity(10000f);
	}

	// Token: 0x06000BD2 RID: 3026 RVA: 0x000B1318 File Offset: 0x000AF518
	public int NewBattleBaseComponent(int entityId, PrefabDesc desc)
	{
		BattleBaseComponent battleBaseComponent = this.battleBases.Add();
		battleBaseComponent.Init(entityId, desc, this.factory);
		this.factory.entityPool[entityId].battleBaseId = battleBaseComponent.id;
		return battleBaseComponent.id;
	}

	// Token: 0x06000BD3 RID: 3027 RVA: 0x000B1364 File Offset: 0x000AF564
	public void RemoveBattleBaseComponent(int id)
	{
		BattleBaseComponent battleBaseComponent = this.battleBases.buffer[id];
		if (battleBaseComponent != null)
		{
			battleBaseComponent.Free();
		}
		this.battleBases.Remove(id);
	}

	// Token: 0x06000BD4 RID: 3028 RVA: 0x000B1394 File Offset: 0x000AF594
	public void AfterBattleBaseImport()
	{
		int cursor = this.battleBases.cursor;
		BattleBaseComponent[] buffer = this.battleBases.buffer;
		for (int i = 1; i < cursor; i++)
		{
			BattleBaseComponent battleBaseComponent = buffer[i];
			if (battleBaseComponent != null && battleBaseComponent.id == i)
			{
				battleBaseComponent.storage = this.factory.factoryStorage.storagePool[battleBaseComponent.storageId];
				battleBaseComponent.combatModule = this.factory.combatGroundSystem.combatModules.buffer[battleBaseComponent.combatModuleId];
				battleBaseComponent.history = this.factory.gameData.history;
			}
		}
	}

	// Token: 0x06000BD5 RID: 3029 RVA: 0x000B142C File Offset: 0x000AF62C
	public void TakeBackItems_BattleBase(Player player, int battleBaseId)
	{
		if (battleBaseId == 0)
		{
			return;
		}
		if (player == null)
		{
			return;
		}
		BattleBaseComponent battleBaseComponent = this.battleBases.buffer[battleBaseId];
		if (battleBaseComponent == null || battleBaseComponent.id != battleBaseId)
		{
			return;
		}
		CombatModuleComponent combatModule = battleBaseComponent.combatModule;
		if (combatModule == null || combatModule.moduleFleets[0].fleetId > 0)
		{
			return;
		}
		ModuleFighter[] fighters = combatModule.moduleFleets[0].fighters;
		for (int i = 0; i < fighters.Length; i++)
		{
			if (fighters[i].itemId > 0 && fighters[i].count > 0)
			{
				int num = player.TryAddItemToPackage(fighters[i].itemId, fighters[i].count, 0, true, battleBaseComponent.entityId, false);
				if (num > 0)
				{
					UIItemup.Up(fighters[i].itemId, num);
				}
				fighters[i].count = 0;
				fighters[i].itemId = 0;
			}
		}
	}

	// Token: 0x06000BD6 RID: 3030 RVA: 0x000B1518 File Offset: 0x000AF718
	public bool TryTakeBackItems_BattleBase(StorageComponent package, int battleBaseId)
	{
		if (battleBaseId == 0)
		{
			return false;
		}
		if (package == null)
		{
			return false;
		}
		BattleBaseComponent battleBaseComponent = this.battleBases.buffer[battleBaseId];
		if (battleBaseComponent == null || battleBaseComponent.id != battleBaseId)
		{
			return false;
		}
		CombatModuleComponent combatModule = battleBaseComponent.combatModule;
		if (combatModule == null || combatModule.moduleFleets[0].fleetId > 0)
		{
			return true;
		}
		StorageComponent storageComponent = new StorageComponent(package.size);
		Array.Copy(package.grids, storageComponent.grids, package.size);
		int num = 0;
		int num2 = 0;
		ModuleFighter[] fighters = combatModule.moduleFleets[0].fighters;
		for (int i = 0; i < fighters.Length; i++)
		{
			if (fighters[i].itemId > 0 && fighters[i].count > 0)
			{
				int num3;
				num += storageComponent.AddItemStacked(fighters[i].itemId, fighters[i].count, 0, out num3);
				num2 += fighters[i].count;
			}
		}
		return num == num2;
	}

	// Token: 0x06000BD7 RID: 3031 RVA: 0x000B1620 File Offset: 0x000AF820
	public void ThrowItems_BattleBase(int battleBaseId, float dropRate)
	{
		if (battleBaseId == 0)
		{
			return;
		}
		BattleBaseComponent battleBaseComponent = this.battleBases.buffer[battleBaseId];
		if (battleBaseComponent == null || battleBaseComponent.id != battleBaseId)
		{
			return;
		}
		CombatModuleComponent combatModule = battleBaseComponent.combatModule;
		if (combatModule == null || combatModule.moduleFleets[0].fleetId > 0)
		{
			return;
		}
		TrashSystem trashSystem = GameMain.data.trashSystem;
		ModuleFighter[] fighters = combatModule.moduleFleets[0].fighters;
		for (int i = 0; i < fighters.Length; i++)
		{
			float num = Random.Range(0f, 1f);
			ref ModuleFighter ptr = ref fighters[i];
			if (dropRate >= num && ptr.count > 0 && ptr.itemId > 0)
			{
				trashSystem.AddTrashOnPlanet(ptr.itemId, ptr.count, 0, battleBaseComponent.entityId, this.planet);
				ptr.count = 0;
				ptr.itemId = 0;
			}
		}
	}

	// Token: 0x06000BD8 RID: 3032 RVA: 0x000B1700 File Offset: 0x000AF900
	public void ClearItems_BattleBase(int battleBaseId)
	{
		if (battleBaseId == 0)
		{
			return;
		}
		BattleBaseComponent battleBaseComponent = this.battleBases.buffer[battleBaseId];
		if (battleBaseComponent == null || battleBaseComponent.id != battleBaseId)
		{
			return;
		}
		CombatModuleComponent combatModule = battleBaseComponent.combatModule;
		if (combatModule == null || combatModule.moduleFleets[0].fleetId > 0)
		{
			return;
		}
		ModuleFighter[] fighters = combatModule.moduleFleets[0].fighters;
		for (int i = 0; i < fighters.Length; i++)
		{
			if (fighters[i].itemId > 0 && fighters[i].count > 0)
			{
				fighters[i].count = 0;
				fighters[i].itemId = 0;
			}
		}
	}

	// Token: 0x06000BD9 RID: 3033 RVA: 0x000B17A4 File Offset: 0x000AF9A4
	public void GameTickBeforePower(long time)
	{
		PowerSystem powerSystem = this.factory.powerSystem;
		float[] networkServes = powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
		PowerNetwork[] netPool = powerSystem.netPool;
		PlanetATField planetATField = this.factory.planetATField;
		planetATField.atFieldRechargeCurrent = 0L;
		planetATField.UpdateFillDemandRatio();
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i].id == i)
			{
				float power = networkServes[consumerPool[buffer[i].pcId].networkId];
				buffer[i].SetPCState(consumerPool, power);
			}
		}
		int cursor2 = this.fieldGenerators.cursor;
		FieldGeneratorComponent[] buffer2 = this.fieldGenerators.buffer;
		for (int j = 1; j < cursor2; j++)
		{
			if (buffer2[j].id == j)
			{
				int networkId = consumerPool[buffer2[j].pcId].networkId;
				float num = networkServes[networkId];
				buffer2[j].SetPCState(consumerPool);
				PowerNetwork powerNetwork = netPool[networkId];
				powerNetwork.exportDemandRatio = 1.0 - (1.0 - powerNetwork.exportDemandRatio) * (1.0 - planetATField.fillDemandRatio);
			}
		}
		int cursor3 = this.battleBases.cursor;
		BattleBaseComponent[] buffer3 = this.battleBases.buffer;
		for (int k = 1; k < cursor3; k++)
		{
			if (buffer3[k] != null && buffer3[k].id == k)
			{
				buffer3[k].SetPCState(consumerPool);
			}
		}
	}

	// Token: 0x06000BDA RID: 3034 RVA: 0x000B194C File Offset: 0x000AFB4C
	public void ParallelGameTickBeforePower(long time, int _usedThreadCnt, int _curThreadIdx, int _minimumMissionCnt)
	{
		PowerSystem powerSystem = this.factory.powerSystem;
		float[] networkServes = powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
		PowerNetwork[] netPool = powerSystem.netPool;
		PlanetATField planetATField = this.factory.planetATField;
		planetATField.atFieldRechargeCurrent = 0L;
		planetATField.UpdateFillDemandRatio();
		int num;
		int num2;
		if (ParallelUtils.CalculateWorkSegmentOldFunction(1, this.turrets.cursor - 1, _usedThreadCnt, _curThreadIdx, _minimumMissionCnt, out num, out num2))
		{
			TurretComponent[] buffer = this.turrets.buffer;
			for (int i = num; i < num2; i++)
			{
				if (buffer[i].id == i)
				{
					float power = networkServes[consumerPool[buffer[i].pcId].networkId];
					buffer[i].SetPCState(consumerPool, power);
				}
			}
		}
		int num3;
		int num4;
		if (ParallelUtils.CalculateWorkSegmentOldFunction(1, this.fieldGenerators.cursor - 1, _usedThreadCnt, _curThreadIdx, _minimumMissionCnt, out num3, out num4))
		{
			FieldGeneratorComponent[] buffer2 = this.fieldGenerators.buffer;
			for (int j = num3; j < num4; j++)
			{
				if (buffer2[j].id == j)
				{
					int networkId = consumerPool[buffer2[j].pcId].networkId;
					float num5 = networkServes[networkId];
					buffer2[j].SetPCState(consumerPool);
					PowerNetwork powerNetwork = netPool[networkId];
					PlanetATField obj = planetATField;
					lock (obj)
					{
						powerNetwork.exportDemandRatio = 1.0 - (1.0 - powerNetwork.exportDemandRatio) * (1.0 - planetATField.fillDemandRatio);
					}
				}
			}
		}
		int num6;
		int num7;
		if (ParallelUtils.CalculateWorkSegmentOldFunction(1, this.battleBases.cursor - 1, _usedThreadCnt, _curThreadIdx, _minimumMissionCnt, out num6, out num7))
		{
			BattleBaseComponent[] buffer3 = this.battleBases.buffer;
			for (int k = num6; k < num7; k++)
			{
				if (buffer3[k] != null && buffer3[k].id == k)
				{
					buffer3[k].SetPCState(consumerPool);
				}
			}
		}
	}

	// Token: 0x06000BDB RID: 3035 RVA: 0x000B1B5C File Offset: 0x000AFD5C
	public void GameTick(long tick, bool isActive)
	{
		GameHistoryData history = GameMain.history;
		int[] productRegister = GameMain.statistics.production.factoryStatPool[this.factory.index].productRegister;
		PowerSystem powerSystem = this.factory.powerSystem;
		float[] networkServes = powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
		PowerNodeComponent[] nodePool = powerSystem.nodePool;
		EntityData[] entityPool = this.factory.entityPool;
		AnimData[] entityAnimPool = this.factory.entityAnimPool;
		SignData[] entitySignPool = this.factory.entitySignPool;
		ref CombatSettings ptr = ref history.combatSettings;
		VectorLF3 relativePos = this.factory.gameData.relativePos;
		Quaternion relativeRot = this.factory.gameData.relativeRot;
		TrashSystem trashSystem = this.factory.gameData.trashSystem;
		bool flag = trashSystem.trashCount > 0;
		float dt = 0.016666668f;
		this.UpdateMatchSpaceEnemies();
		if (this.beacons.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.Beacon, -1, (long)this.factory.planetId);
			EAggressiveLevel aggressiveLevel = ptr.aggressiveLevel;
			int cursor = this.beacons.cursor;
			BeaconComponent[] buffer = this.beacons.buffer;
			for (int i = 1; i < cursor; i++)
			{
				ref BeaconComponent ptr2 = ref buffer[i];
				if (ptr2.id == i)
				{
					float power = networkServes[nodePool[ptr2.pnId].networkId];
					PrefabDesc pdesc = PlanetFactory.PrefabDescByModelIndex[(int)entityPool[ptr2.entityId].modelIndex];
					ptr2.GameTick(this.factory, pdesc, aggressiveLevel, power, tick);
					if (ptr2.DeterminActiveEnemyUnits(false, tick))
					{
						ptr2.ActiveEnemyUnits_Ground(this.factory, pdesc);
					}
					if (ptr2.DeterminActiveEnemyUnits(true, tick))
					{
						ptr2.ActiveEnemyUnits_Space(this.factory, pdesc);
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		bool flag2 = false;
		for (int j = this.localGlobalTargetCursor - 1; j >= 0; j--)
		{
			TimedSkillTarget[] array = this.globalTargets;
			int num = j;
			array[num].lifeTick = array[num].lifeTick - 1;
			if (this.globalTargets[j].lifeTick <= 0)
			{
				this.RemoveGlobalTargets(j);
				flag2 = true;
			}
		}
		if (flag2)
		{
			this.ArrangeGlobalTargets();
		}
		this.UpdateSpaceUniqueGlobalTargets();
		this.UpdateOtherGlobalTargets();
		this.engagingGaussCount = 0;
		this.engagingLaserCount = 0;
		this.engagingCannonCount = 0;
		this.engagingMissileCount = 0;
		this.engagingPlasmaCount = 0;
		this.engagingLocalPlasmaCount = 0;
		this.engagingTurretTotalCount = 0;
		this.turretEnableDefenseSpace = false;
		this.incomingSupernovaTime = 300;
		if (this.fieldGenerators.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.PlanetATFieldGenerator, -1, (long)this.factory.planetId);
			int cursor2 = this.fieldGenerators.cursor;
			FieldGeneratorComponent[] buffer2 = this.fieldGenerators.buffer;
			for (int k = 1; k < cursor2; k++)
			{
				ref FieldGeneratorComponent ptr3 = ref buffer2[k];
				if (ptr3.id == k)
				{
					int entityId = ptr3.entityId;
					ref PowerConsumerComponent ptr4 = ref consumerPool[ptr3.pcId];
					float num2 = networkServes[ptr4.networkId];
					ptr3.InternalUpdate(this.factory, (double)num2, ref ptr4, ref entityAnimPool[ptr3.entityId]);
					if (entitySignPool[entityId].signType == 0U || entitySignPool[entityId].signType > 3U)
					{
						double num3 = ptr3.fieldHolding * 0.699999988079071 + 0.3;
						if (num3 < 0.3001)
						{
							num3 = 0.0;
						}
						else if (num3 > 0.99999)
						{
							num3 = 1.0;
						}
						entitySignPool[entityId].signType = ((num3 < 0.9999) ? ((num3 < 0.3001) ? 15U : 16U) : 0U);
					}
					if (this.factory.planetATField.energy <= 1L)
					{
						entitySignPool[entityId].signType = 17U;
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		if (this.battleBases.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.BattleBase, -1, (long)this.factory.planetId);
			int num4 = (int)(tick % 4L);
			int cursor3 = this.battleBases.cursor;
			BattleBaseComponent[] buffer3 = this.battleBases.buffer;
			for (int l = 1; l < cursor3; l++)
			{
				BattleBaseComponent battleBaseComponent = buffer3[l];
				if (battleBaseComponent != null && battleBaseComponent.id == l)
				{
					ref PowerConsumerComponent ptr5 = ref consumerPool[battleBaseComponent.pcId];
					float power2 = networkServes[ptr5.networkId];
					battleBaseComponent.InternalUpdate(dt, this.factory, power2, ref entityAnimPool[battleBaseComponent.entityId]);
					if (flag && battleBaseComponent.autoPickEnabled && battleBaseComponent.energy > 0L && (long)l % 4L == (long)num4)
					{
						battleBaseComponent.AutoPickTrash(this.factory, trashSystem, tick, ref relativePos, ref relativeRot, productRegister);
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x06000BDC RID: 3036 RVA: 0x000B203C File Offset: 0x000B023C
	public void GameTick_Turret(long tick)
	{
		GameHistoryData history = GameMain.history;
		int[] consumeRegister = GameMain.statistics.production.factoryStatPool[this.factory.index].consumeRegister;
		PowerSystem powerSystem = this.factory.powerSystem;
		SkillSystem skillSystem = this.spaceSector.skillSystem;
		float[] networkServes = powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
		EntityData[] entityPool = this.factory.entityPool;
		AnimData[] entityAnimPool = this.factory.entityAnimPool;
		EnemyData[] enemyPool = this.spaceSector.enemyPool;
		SignData[] entitySignPool = this.factory.entitySignPool;
		CombatUpgradeData combatUpgradeData = default(CombatUpgradeData);
		history.GetCombatUpgradeData(ref combatUpgradeData);
		float num = 0.016666668f;
		int num2 = 10000;
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		if (this.turrets.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.Turret, -1, (long)this.factory.planetId);
			for (int i = 1; i < cursor; i++)
			{
				ref TurretComponent ptr = ref buffer[i];
				if (ptr.id == i)
				{
					float num3 = networkServes[consumerPool[ptr.pcId].networkId];
					PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)entityPool[ptr.entityId].modelIndex];
					ptr.InternalUpdate(tick, num3, this.factory, skillSystem, prefabDesc, false);
					ptr.Aim(this.factory, enemyPool, prefabDesc, num3);
					ptr.Shoot(this.factory, enemyPool, prefabDesc, consumeRegister, num3, tick, ref combatUpgradeData);
					if (ptr.supernovaTick < 0)
					{
						int num4 = (int)(-(int)ptr.supernovaTick);
						if (num4 < num2)
						{
							num2 = num4;
						}
					}
					if (ptr.isLockingTarget)
					{
						switch (ptr.type)
						{
						case ETurretType.Gauss:
							this.engagingGaussCount++;
							break;
						case ETurretType.Laser:
							this.engagingLaserCount++;
							break;
						case ETurretType.Cannon:
							this.engagingCannonCount++;
							break;
						case ETurretType.Plasma:
							this.engagingPlasmaCount++;
							break;
						case ETurretType.Missile:
							this.engagingMissileCount++;
							break;
						case ETurretType.LocalPlasma:
							this.engagingLocalPlasmaCount++;
							break;
						}
					}
					VSLayerMask vslayerMask = ptr.vsCaps & ptr.vsSettings;
					if ((vslayerMask & VSLayerMask.OrbitAndSpace) > VSLayerMask.None)
					{
						this.turretEnableDefenseSpace = true;
						if (ptr.DeterminActiveEnemyUnits(true, tick))
						{
							ptr.ActiveEnemyUnits_Space(this.factory, prefabDesc);
						}
					}
					if ((vslayerMask & VSLayerMask.GroundAndAir) > VSLayerMask.None && ptr.DeterminActiveEnemyUnits(false, tick))
					{
						ptr.ActiveEnemyUnits_Ground(this.factory, prefabDesc);
					}
					int entityId = ptr.entityId;
					if (ptr.type == ETurretType.Disturb)
					{
						entityAnimPool[entityId].state = 1U;
						if (ptr.CalculateAnimState(num3) > 1U)
						{
							float anim_working_length = prefabDesc.anim_working_length;
							entityAnimPool[entityId].working_length = anim_working_length;
							if (entityAnimPool[entityId].time < anim_working_length)
							{
								AnimData[] array = entityAnimPool;
								int num5 = entityId;
								array[num5].time = array[num5].time + num;
							}
							if (entityAnimPool[entityId].time > anim_working_length)
							{
								entityAnimPool[entityId].time = anim_working_length - 0.01f;
							}
						}
						else
						{
							float anim_working_length2 = prefabDesc.anim_working_length;
							entityAnimPool[entityId].working_length = anim_working_length2;
							if (entityAnimPool[entityId].time > 0f)
							{
								AnimData[] array2 = entityAnimPool;
								int num6 = entityId;
								array2[num6].time = array2[num6].time - num;
							}
							if (entityAnimPool[entityId].time < 0f)
							{
								entityAnimPool[entityId].time = 0f;
							}
						}
						entityAnimPool[entityId].power = num3;
					}
					else
					{
						bool flag = ptr.isWorking && num3 >= 0.1f;
						float num7 = entityAnimPool[entityId].state / 100000U / 100f;
						int num8 = (int)((num7 - (float)((int)num7)) * 100f + 0.5f);
						if (flag)
						{
							num8++;
							if (num8 > 40)
							{
								num8 = 40;
							}
						}
						else
						{
							num8--;
							if (num8 < 0)
							{
								num8 = 0;
							}
						}
						uint num9 = ptr.CalculateAnimState(num3);
						entityAnimPool[entityId].prepare_length = ptr.localDir.x;
						entityAnimPool[entityId].working_length = ptr.localDir.y;
						entityAnimPool[entityId].power = ptr.localDir.z;
						entityAnimPool[entityId].state = num9 + (ptr.supernovaBursting ? (10U + (uint)(ptr.supernova_strength * 10f + 0.5f) * 100U) : ((uint)(ptr.supernovaCharging ? (-ptr.supernovaTick * 100) : 0))) + (uint)(num8 * 100000) + (uint)ptr.muzzleIndex * 10000000U;
						entityAnimPool[entityId].time = ((prefabDesc.turretMuzzleCount == 1) ? ((1f - (float)ptr.roundFire / (float)prefabDesc.turretRoundInterval) * 10f) : ((1f - (float)ptr.muzzleFire / (float)prefabDesc.turretMuzzleInterval) * 10f));
					}
					if ((entitySignPool[entityId].signType == 0U || entitySignPool[entityId].signType > 3U) && ptr.type != ETurretType.Laser)
					{
						entitySignPool[entityId].signType = ((ptr.bulletCount <= 0 && ptr.itemCount <= 0) ? 14U : 0U);
					}
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
		this.engagingTurretTotalCount = this.engagingGaussCount + this.engagingLaserCount + this.engagingCannonCount + this.engagingPlasmaCount + this.engagingMissileCount + this.engagingLocalPlasmaCount;
		if (num2 < this.incomingSupernovaTime)
		{
			this.incomingSupernovaTime = num2;
		}
	}

	// Token: 0x06000BDD RID: 3037 RVA: 0x000B2604 File Offset: 0x000B0804
	public void PostGameTick(long tick)
	{
		bool flag = false;
		EnemyData[] enemyPool = this.spaceSector.enemyPool;
		for (int i = this.localGlobalTargetCursor - 1; i >= 0; i--)
		{
			TimedSkillTarget timedSkillTarget = this.globalTargets[i];
			if (timedSkillTarget.type == ETargetType.Enemy)
			{
				int astroId = timedSkillTarget.astroId;
				if ((astroId > 1000000 || astroId == 0) && (timedSkillTarget.id == 0 || enemyPool[timedSkillTarget.id].id != timedSkillTarget.id || enemyPool[timedSkillTarget.id].isInvincible))
				{
					this.RemoveGlobalTargets(i);
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.ArrangeGlobalTargets();
		}
		if (this.turrets.count > 0)
		{
			DeepProfiler.BeginSample(DPEntry.Turret, -1, -1L);
			EntityData[] entityPool = this.factory.entityPool;
			AnimData[] entityAnimPool = this.factory.entityAnimPool;
			PowerSystem powerSystem = this.factory.powerSystem;
			float[] networkServes = powerSystem.networkServes;
			PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
			int cursor = this.turrets.cursor;
			TurretComponent[] buffer = this.turrets.buffer;
			for (int j = 1; j < cursor; j++)
			{
				ref TurretComponent ptr = ref buffer[j];
				if (ptr.id == j)
				{
					ptr.PostGameTick(this.factory, PlanetFactory.PrefabDescByModelIndex[(int)entityPool[ptr.entityId].modelIndex], entityAnimPool, tick, networkServes[consumerPool[ptr.pcId].networkId]);
				}
			}
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x06000BDE RID: 3038 RVA: 0x000B277C File Offset: 0x000B097C
	public void UpdateMatchSpaceEnemies()
	{
		this.matchHiveLevel0.Clear();
		if (!this.turretEnableDefenseSpace)
		{
			return;
		}
		AstroData[] astros = this.spaceSector.astros;
		EnemyData[] enemyPool = this.spaceSector.enemyPool;
		int astroId = this.factory.planet.astroId;
		EnemyDFHiveSystem enemyDFHiveSystem = this.spaceSector.dfHives[this.planet.star.index];
		VectorLF3 uPosition = this.planet.uPosition;
		int id = this.planet.star.id;
		double num = 169000000.0;
		while (enemyDFHiveSystem != null)
		{
			if (!enemyDFHiveSystem.isEmpty && enemyDFHiveSystem.starData.id == id && (astros[enemyDFHiveSystem.hiveAstroId - 1000000].uPos - uPosition).sqrMagnitude < num)
			{
				this.matchHiveLevel0.Add(enemyDFHiveSystem.hiveAstroId);
			}
			enemyDFHiveSystem = enemyDFHiveSystem.nextSibling;
		}
	}

	// Token: 0x06000BDF RID: 3039 RVA: 0x000B2868 File Offset: 0x000B0A68
	public void UpdateOtherGlobalTargets()
	{
		this.otherGlobalTargetCursor = this.localGlobalTargetCursor;
		if (!this.turretEnableDefenseSpace)
		{
			return;
		}
		PlanetData orbitAroundPlanet = this.planet.orbitAroundPlanet;
		if (orbitAroundPlanet != null)
		{
			double num = (double)(PlanetFactory.PrefabDescByModelIndex[407].turretSpaceAttackRange - 400f);
			double num2 = num * num;
			int planetCount = this.planet.star.planetCount;
			PlanetData[] planets = this.planet.star.planets;
			for (int i = 0; i < planetCount; i++)
			{
				if (planets[i].orbitAroundPlanet == orbitAroundPlanet && planets[i] != this.planet)
				{
					PlanetFactory planetFactory = planets[i].factory;
					if (planetFactory != null && (this.planet.uPosition - planets[i].uPosition).sqrMagnitude <= num2)
					{
						DefenseSystem defenseSystem = planetFactory.defenseSystem;
						TimedSkillTarget[] array = defenseSystem.globalTargets;
						int num3 = defenseSystem.localGlobalTargetCursor;
						for (int j = 0; j < num3; j++)
						{
							this.AddOtherGlobalTargets(ref array[j]);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000BE0 RID: 3040 RVA: 0x000B2974 File Offset: 0x000B0B74
	public void SetGroupTurretsSupernova(int group)
	{
		if (group == 0)
		{
			return;
		}
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i && (int)ptr.group == group)
			{
				ptr.SetSupernova();
			}
		}
	}

	// Token: 0x06000BE1 RID: 3041 RVA: 0x000B29CC File Offset: 0x000B0BCC
	public void CancelGroupTurretSupernova(int group)
	{
		if (group == 0)
		{
			return;
		}
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i && (int)ptr.group == group)
			{
				ptr.CancelSupernova();
			}
		}
	}

	// Token: 0x06000BE2 RID: 3042 RVA: 0x000B2A24 File Offset: 0x000B0C24
	public int CalculateGroupTurretsCanStartSupernovaCount(int group)
	{
		if (group == 0)
		{
			return 0;
		}
		int num = 0;
		PowerSystem powerSystem = this.factory.powerSystem;
		float[] networkServes = powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i && (int)ptr.group == group && ptr.canStartSupernova && networkServes[consumerPool[ptr.pcId].networkId] > 0.3f)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000BE3 RID: 3043 RVA: 0x000B2AC4 File Offset: 0x000B0CC4
	public void SetGlobalTurretsSupernova()
	{
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ptr.SetSupernova();
			}
		}
	}

	// Token: 0x06000BE4 RID: 3044 RVA: 0x000B2B0C File Offset: 0x000B0D0C
	public void CancelGlobalTurretSupernova()
	{
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i)
			{
				ptr.CancelSupernova();
			}
		}
	}

	// Token: 0x06000BE5 RID: 3045 RVA: 0x000B2B54 File Offset: 0x000B0D54
	public int CalculateGlobalTurretsCanStartSupernovaCount()
	{
		int num = 0;
		PowerSystem powerSystem = this.factory.powerSystem;
		float[] networkServes = powerSystem.networkServes;
		PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			ref TurretComponent ptr = ref buffer[i];
			if (ptr.id == i && ptr.canStartSupernova && networkServes[consumerPool[ptr.pcId].networkId] > 0.3f)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000BE6 RID: 3046 RVA: 0x000B2BE3 File Offset: 0x000B0DE3
	public void ConnectToTurret(int turretId, int targetBeltId)
	{
		if (turretId != 0 && this.turrets.buffer[turretId].id == turretId && targetBeltId != 0)
		{
			this.turrets.buffer[turretId].SetTargetBelt(targetBeltId);
		}
	}

	// Token: 0x06000BE7 RID: 3047 RVA: 0x000B2C1C File Offset: 0x000B0E1C
	public void DisconnectToTurret(int turretId, int removingBeltId)
	{
		if (turretId == 0)
		{
			return;
		}
		if (this.turrets.buffer[turretId].id != turretId)
		{
			return;
		}
		if (removingBeltId == 0)
		{
			return;
		}
		if (this.turrets.buffer[turretId].targetBeltId == removingBeltId)
		{
			this.turrets.buffer[turretId].SetTargetBelt(0);
		}
	}

	// Token: 0x06000BE8 RID: 3048 RVA: 0x000B2C7C File Offset: 0x000B0E7C
	public void TakeBackItems_Turret(Player player, int turretId)
	{
		if (turretId == 0)
		{
			return;
		}
		ref TurretComponent ptr = ref this.turrets.buffer[turretId];
		if (ptr.id == turretId)
		{
			if (ptr.type == ETurretType.Laser)
			{
				return;
			}
			int itemId = (int)ptr.itemId;
			int itemCount = (int)ptr.itemCount;
			int itemInc = (int)ptr.itemInc;
			if (itemCount > 0)
			{
				int upCount = player.TryAddItemToPackage(itemId, itemCount, itemInc, true, ptr.entityId, false);
				UIItemup.Up(itemId, upCount);
			}
			ptr.ClearItem();
		}
	}

	// Token: 0x06000BE9 RID: 3049 RVA: 0x000B2CEC File Offset: 0x000B0EEC
	public void ClearItems_Turret(int turretId)
	{
		if (turretId == 0)
		{
			return;
		}
		ref TurretComponent ptr = ref this.turrets.buffer[turretId];
		if (ptr.id == turretId)
		{
			if (ptr.type == ETurretType.Laser)
			{
				return;
			}
			ptr.ClearItem();
		}
	}

	// Token: 0x06000BEA RID: 3050 RVA: 0x000B2D28 File Offset: 0x000B0F28
	public void SetGlobalTargetsCapacity(int newCapacity)
	{
		if (this.globalTargets == null)
		{
			this.globalTargets = new TimedSkillTarget[newCapacity];
			return;
		}
		if (newCapacity <= this.globalTargets.Length)
		{
			return;
		}
		TimedSkillTarget[] array = this.globalTargets;
		this.globalTargets = new TimedSkillTarget[newCapacity];
		Array.Copy(array, this.globalTargets, array.Length);
	}

	// Token: 0x06000BEB RID: 3051 RVA: 0x000B2D78 File Offset: 0x000B0F78
	public void AddGlobalTargets(ref TimedSkillTarget target)
	{
		if (this.localGlobalTargetCursor == this.globalTargets.Length)
		{
			this.SetGlobalTargetsCapacity(this.localGlobalTargetCursor * 2);
		}
		TimedSkillTarget[] array = this.globalTargets;
		int num = this.localGlobalTargetCursor;
		this.localGlobalTargetCursor = num + 1;
		array[num] = target;
	}

	// Token: 0x06000BEC RID: 3052 RVA: 0x000B2DC8 File Offset: 0x000B0FC8
	private void AddOtherGlobalTargets(ref TimedSkillTarget target)
	{
		if (this.otherGlobalTargetCursor == this.globalTargets.Length)
		{
			this.SetGlobalTargetsCapacity(this.otherGlobalTargetCursor * 2);
		}
		TimedSkillTarget[] array = this.globalTargets;
		int num = this.otherGlobalTargetCursor;
		this.otherGlobalTargetCursor = num + 1;
		array[num] = target;
	}

	// Token: 0x06000BED RID: 3053 RVA: 0x000B2E15 File Offset: 0x000B1015
	public void RemoveGlobalTargets(int index)
	{
		this.globalTargets[index].id = 0;
	}

	// Token: 0x06000BEE RID: 3054 RVA: 0x000B2E2C File Offset: 0x000B102C
	public void RemoveGlobalTargets(ETargetType type, int id)
	{
		bool flag = false;
		int astroId = this.planet.astroId;
		for (int i = this.localGlobalTargetCursor - 1; i >= 0; i--)
		{
			if (this.globalTargets[i].astroId == astroId && type == this.globalTargets[i].type && id == this.globalTargets[i].id)
			{
				this.RemoveGlobalTargets(i);
				flag = true;
			}
		}
		if (flag)
		{
			this.ArrangeGlobalTargets();
		}
	}

	// Token: 0x06000BEF RID: 3055 RVA: 0x000B2EA8 File Offset: 0x000B10A8
	public void ArrangeGlobalTargets()
	{
		int num = 0;
		for (int i = 0; i < this.localGlobalTargetCursor; i++)
		{
			if (this.globalTargets[i].id > 0)
			{
				if (i == num)
				{
					num++;
				}
				else
				{
					this.globalTargets[num++] = this.globalTargets[i];
					this.globalTargets[i].id = 0;
				}
			}
		}
		this.localGlobalTargetCursor = num;
	}

	// Token: 0x06000BF0 RID: 3056 RVA: 0x000B2F1C File Offset: 0x000B111C
	private void UpdateSpaceUniqueGlobalTargets()
	{
		if (this.globalTargets == null)
		{
			return;
		}
		this.spaceUniqueGlobalTargets.Clear();
		int astroId = this.planet.astroId;
		for (int i = 0; i < this.localGlobalTargetCursor; i++)
		{
			if (this.globalTargets[i].astroId != astroId && !this.spaceUniqueGlobalTargets.Contains(this.globalTargets[i].id))
			{
				this.spaceUniqueGlobalTargets.Add(this.globalTargets[i].id);
			}
		}
	}

	// Token: 0x06000BF1 RID: 3057 RVA: 0x000B2FAC File Offset: 0x000B11AC
	public void RefreshTurretSearchPair()
	{
		if (this.turrets.count == 0)
		{
			return;
		}
		this.ClearTurretPairs();
		int cursor = this.turrets.cursor;
		TurretComponent[] buffer = this.turrets.buffer;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i].id == i)
			{
				this.MatchTurretPair(ref buffer[i]);
			}
		}
	}

	// Token: 0x06000BF2 RID: 3058 RVA: 0x000B3010 File Offset: 0x000B1210
	public void SetTurretPairsCapacity(int newCapacity)
	{
		if (this.turretSearchPairs == null)
		{
			this.turretSearchPairs = new TurretSearchPair[newCapacity];
			return;
		}
		if (newCapacity <= this.turretSearchPairs.Length)
		{
			return;
		}
		TurretSearchPair[] array = this.turretSearchPairs;
		this.turretSearchPairs = new TurretSearchPair[newCapacity];
		Array.Copy(array, this.turretSearchPairs, array.Length);
	}

	// Token: 0x06000BF3 RID: 3059 RVA: 0x000B3060 File Offset: 0x000B1260
	public void AddTurretPair(ref TurretComponent turret, ESearchType sType, int sId, float value = 0f)
	{
		if (this.turretSearchPairs == null)
		{
			this.SetTurretPairsCapacity(8);
		}
		if (this.turretSearchPairCount == this.turretSearchPairs.Length)
		{
			this.SetTurretPairsCapacity(this.turretSearchPairs.Length * 2);
		}
		TurretSearchPair[] array = this.turretSearchPairs;
		int num = this.turretSearchPairCount;
		this.turretSearchPairCount = num + 1;
		array[num] = new TurretSearchPair(turret.id, sType, sId, value);
		turret.searchPairCount++;
	}

	// Token: 0x06000BF4 RID: 3060 RVA: 0x000B30D2 File Offset: 0x000B12D2
	public void ClearTurretPairs()
	{
		this.turretSearchPairCount = 0;
		if (this.turretSearchPairs != null)
		{
			Array.Clear(this.turretSearchPairs, 0, this.turretSearchPairs.Length);
		}
	}

	// Token: 0x06000BF5 RID: 3061 RVA: 0x000B30F8 File Offset: 0x000B12F8
	private void MatchTurretPair(ref TurretComponent turret)
	{
		turret.searchPairBeginIndex = this.turretSearchPairCount;
		turret.searchPairCount = 0;
		if ((turret.vsCaps & VSLayerMask.GroundAndAir) == VSLayerMask.None)
		{
			return;
		}
		EntityData[] entityPool = this.factory.entityPool;
		EnemyData[] enemyPool = this.factory.enemyPool;
		ref EntityData ptr = ref entityPool[turret.entityId];
		Vector3 pos = ptr.pos;
		Quaternion rot = ptr.rot;
		Vector3 vector = default(Vector3);
		PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
		float turretMinAttackRange = prefabDesc.turretMinAttackRange;
		float num = prefabDesc.turretMaxAttackRange + 40f;
		float num2 = turretMinAttackRange * turretMinAttackRange;
		float num3 = num * num;
		float realRadius = this.factory.planet.realRadius;
		DFGBaseComponent[] buffer = this.factory.enemySystem.bases.buffer;
		int cursor = this.factory.enemySystem.bases.cursor;
		for (int i = 1; i < cursor; i++)
		{
			if (buffer[i] != null && buffer[i].id == i)
			{
				int enemyId = buffer[i].enemyId;
				ref EnemyData ptr2 = ref enemyPool[enemyId];
				if (ptr2.id == enemyId)
				{
					Vector3 vector2 = ptr2.pos - pos;
					Maths.QInvRotate_ref(ref rot, ref vector2, ref vector);
					if (vector.y >= -realRadius)
					{
						float num4 = vector.x * vector.x + vector.z * vector.z;
						if (num4 <= num3 && num4 >= num2)
						{
							bool flag = false;
							for (int j = turret.searchPairBeginIndex; j < turret.searchPairCount; j++)
							{
								if (this.turretSearchPairs[j].searchType == ESearchType.EnemyBase && this.turretSearchPairs[j].searchId == ptr2.id)
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								this.AddTurretPair(ref turret, ESearchType.EnemyBase, i, 0f);
							}
						}
					}
				}
			}
		}
		Vector3 vector3 = pos;
		float num5 = num;
		float cellSize = HashSystem.cellSize;
		int num6 = 32;
		int num7 = 5;
		HashSystem.Cell[] bucketMap = HashSystem.bucketMap;
		int num8 = (int)((vector3.x + 270f) / cellSize);
		int num9 = (int)((vector3.y + 270f) / cellSize);
		int num10 = (int)((vector3.z + 270f) / cellSize);
		num8 = ((num8 < 99) ? ((num8 < 0) ? 0 : num8) : 99);
		num9 = ((num9 < 99) ? ((num9 < 0) ? 0 : num9) : 99);
		num10 = ((num10 < 99) ? ((num10 < 0) ? 0 : num10) : 99);
		int num11 = num10 * 10000 + num9 * 100 + num8 << num7;
		for (int k = 0; k < num6; k++)
		{
			HashSystem.Cell cell = bucketMap[num11 + k];
			if ((float)cell.dist > num5)
			{
				break;
			}
			this.AddTurretPair(ref turret, ESearchType.HashBlock, (int)cell.bucketIndex, 0f);
		}
	}

	// Token: 0x06000BF6 RID: 3062 RVA: 0x000B33CC File Offset: 0x000B15CC
	public void UnderAttack(int id, int astroId, ETargetType type)
	{
		TimedSkillTarget timedSkillTarget = default(TimedSkillTarget);
		timedSkillTarget.id = id;
		timedSkillTarget.astroId = astroId;
		timedSkillTarget.type = type;
		timedSkillTarget.lifeTick = 600;
		if (this.globalTargets != null)
		{
			bool flag = false;
			for (int i = 0; i < this.localGlobalTargetCursor; i++)
			{
				if (this.globalTargets[i].id == timedSkillTarget.id && this.globalTargets[i].astroId == timedSkillTarget.astroId && this.globalTargets[i].type == timedSkillTarget.type)
				{
					flag = true;
					this.globalTargets[i] = timedSkillTarget;
					break;
				}
			}
			if (!flag)
			{
				this.AddGlobalTargets(ref timedSkillTarget);
			}
		}
	}

	// Token: 0x04000D6B RID: 3435
	public PlanetData planet;

	// Token: 0x04000D6C RID: 3436
	public PlanetFactory factory;

	// Token: 0x04000D6D RID: 3437
	public SpaceSector spaceSector;

	// Token: 0x04000D6E RID: 3438
	public DataPool<TurretComponent> turrets;

	// Token: 0x04000D6F RID: 3439
	public DataPool<BeaconComponent> beacons;

	// Token: 0x04000D70 RID: 3440
	public DataPool<FieldGeneratorComponent> fieldGenerators;

	// Token: 0x04000D71 RID: 3441
	public ObjectPool<BattleBaseComponent> battleBases;

	// Token: 0x04000D72 RID: 3442
	public TimedSkillTarget[] globalTargets;

	// Token: 0x04000D73 RID: 3443
	public HashSet<int> spaceUniqueGlobalTargets;

	// Token: 0x04000D74 RID: 3444
	public int localGlobalTargetCursor;

	// Token: 0x04000D75 RID: 3445
	public int otherGlobalTargetCursor;

	// Token: 0x04000D76 RID: 3446
	public int engagingGaussCount;

	// Token: 0x04000D77 RID: 3447
	public int engagingLaserCount;

	// Token: 0x04000D78 RID: 3448
	public int engagingCannonCount;

	// Token: 0x04000D79 RID: 3449
	public int engagingMissileCount;

	// Token: 0x04000D7A RID: 3450
	public int engagingPlasmaCount;

	// Token: 0x04000D7B RID: 3451
	public int engagingLocalPlasmaCount;

	// Token: 0x04000D7C RID: 3452
	public int engagingTurretTotalCount;

	// Token: 0x04000D7D RID: 3453
	public bool turretEnableDefenseSpace;

	// Token: 0x04000D7E RID: 3454
	public int incomingSupernovaTime;

	// Token: 0x04000D7F RID: 3455
	public readonly object incoming_supernova_time_lock = new object();

	// Token: 0x04000D80 RID: 3456
	public int turretSearchPairCount;

	// Token: 0x04000D81 RID: 3457
	public TurretSearchPair[] turretSearchPairs;

	// Token: 0x04000D82 RID: 3458
	public List<int> matchHiveLevel0;

	// Token: 0x04000D83 RID: 3459
	private const double HIVE_LEVEL_0_RANGE = 13000.0;

	// Token: 0x04000D84 RID: 3460
	public const int INCOMING_SUPERNOVA_THRESHOLD = 300;
}
