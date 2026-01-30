using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020001D0 RID: 464
public class Mecha
{
	// Token: 0x06001224 RID: 4644 RVA: 0x0014FBA4 File Offset: 0x0014DDA4
	public void Init(GameData _gameData, Player _player)
	{
		this.player = _player;
		this.gameData = _gameData;
		this.sector = this.gameData.spaceSector;
		this.history = this.gameData.history;
		this.coreEnergyCap = 1.0;
		this.coreEnergy = this.coreEnergyCap;
		this.corePowerGen = 0.0;
		this.reactorPowerGen = 0.0;
		this.reactorEnergy = 0.0;
		this.reactorItemId = 0;
		this.reactorItemInc = 0;
		this.autoReplenishFuel = false;
		this.autoReplenishWarper = false;
		this.reactorStorage = new StorageComponent(4);
		this.reactorStorage.type = EStorageType.Fuel;
		this.warpStorage = new StorageComponent(1);
		this.warpStorage.type = EStorageType.Filtered;
		this.warpStorage.SetFilterDirect(0, 1210, 20);
		this.energyConsumptionCoef = 1.0;
		this.walkPower = 0.0;
		this.jumpEnergy = 0.0;
		this.thrustPowerPerAcc = 0.0;
		this.warpKeepingPowerPerSpeed = 0.0;
		this.warpStartPowerPerSpeed = 0.0;
		this.miningPower = 0.0;
		this.replicatePower = 0.0;
		this.researchPower = 0.0;
		this.droneEjectEnergy = 0.0;
		this.droneEnergyPerMeter = 0.0;
		this.instantBuildEnergy = 0.0;
		this.coreLevel = 0;
		this.thrusterLevel = 0;
		this.miningSpeed = 0f;
		this.replicateSpeed = 0f;
		this.walkSpeed = 0f;
		this.jumpSpeed = 0f;
		this.maxSailSpeed = 0f;
		this.maxWarpSpeed = 0f;
		this.buildArea = 0f;
		this.forge = new MechaForge();
		this.forge.Init(this);
		this.lab = new MechaLab();
		this.lab.Init(this);
		this.constructionModule = new ConstructionModuleComponent();
		this.hp = 0;
		this.hpRecoverCD = 0;
		this.energyShieldRecoverCD = 0;
		this.energyShieldEnergy = 0L;
		this.hpMax = 0;
		this.hpMaxUpgrade = 0;
		this.hpRecover = 0;
		this.energyShieldUnlocked = false;
		this.energyShieldRechargeEnabled = true;
		this.energyShieldRechargeSpeed = 0f;
		this.energyShieldRadius = 0f;
		this.energyShieldCapacity = 0L;
		this.energyShieldEnergyRate = 0L;
		this.energyShieldBurstUnlocked = false;
		this.energyShieldBurstDamageRate = 0L;
		this.energyShieldBurstProgress = 0.0;
		this.ammoItemId = 0;
		this.ammoInc = 0;
		this.ammoBulletCount = 0;
		this.ammoSelectSlot = 0;
		this.ammoSelectSlotState = 0;
		this.ammoMuzzleFire = 0;
		this.ammoRoundFire = 0;
		this.ammoMuzzleIndex = 0;
		this.laserActive = false;
		this.laserActiveState = 0;
		this.laserRecharging = false;
		this.laserEnergy = 0L;
		this.laserEnergyCapacity = 0L;
		this.laserFire = 0;
		this.bombActive = false;
		this.bombFire = 0;
		this.autoReplenishAmmo = false;
		this.ammoStorage = new StorageComponent(3);
		this.ammoStorage.type = EStorageType.Ammo;
		this.bombStorage = new StorageComponent(1);
		this.bombStorage.type = EStorageType.Bomb;
		this.ammoHatredTarget = default(EnemyHatredTarget);
		this.laserHatredTarget = default(EnemyHatredTarget);
		this.bulletLocalAttackRange = 0f;
		this.bulletSpaceAttackRange = 0f;
		this.bulletEnergyCost = 0;
		this.bulletDamageScale = 0f;
		this.bulletROF = 0;
		this.bulletMuzzleCount = 0;
		this.bulletMuzzleInterval = 0;
		this.bulletRoundInterval = 0;
		this.cannonLocalAttackRange = 0f;
		this.cannonSpaceAttackRange = 0f;
		this.cannonEnergyCost = 0;
		this.cannonDamageScale = 0f;
		this.cannonROF = 0;
		this.cannonMuzzleCount = 0;
		this.cannonMuzzleInterval = 0;
		this.cannonRoundInterval = 0;
		this.plasmaLocalAttackRange = 0f;
		this.plasmaSpaceAttackRange = 0f;
		this.plasmaEnergyCost = 0;
		this.plasmaDamageScale = 0f;
		this.plasmaROF = 0;
		this.plasmaMuzzleCount = 0;
		this.plasmaMuzzleInterval = 0;
		this.plasmaRoundInterval = 0;
		this.missileLocalAttackRange = 0f;
		this.missileSpaceAttackRange = 0f;
		this.missileEnergyCost = 0;
		this.missileDamageScale = 0f;
		this.missileROF = 0;
		this.missileMuzzleCount = 0;
		this.missileMuzzleInterval = 0;
		this.missileRoundInterval = 0;
		this.laserLocalAttackRange = 0f;
		this.laserSpaceAttackRange = 0f;
		this.laserLocalEnergyCost = 0;
		this.laserSpaceEnergyCost = 0;
		this.laserLocalDamage = 0;
		this.laserSpaceDamage = 0;
		this.laserLocalInterval = 0;
		this.laserSpaceInterval = 0;
		this.autoReplenishHangar = false;
		this.fighterStorage = new StorageComponent(5);
		this.fighterStorage.type = EStorageType.Fighter;
		this.groundCombatModule = new CombatModuleComponent();
		this.groundCombatModule.Init(_gameData);
		this.spaceCombatModule = new CombatModuleComponent();
		this.spaceCombatModule.Init(_gameData);
		this.appearance = new MechaAppearance();
		this.appearance.Init();
		this.diyAppearance = new MechaAppearance();
		this.diyAppearance.Init();
		this.diyItems = new ItemBundle();
		this.diyItems.Clear();
		this.energyChanges = new double[32];
		this.chargerDevice = new int[256];
		this.chargerCount = 0;
		this.player.onPackageAddItem += this.OnPlayerPackageAddItem;
		this.player.onReplenishPreferred += this.OnPlayerReplenishPreferred;
	}

	// Token: 0x06001225 RID: 4645 RVA: 0x00150148 File Offset: 0x0014E348
	public void SetForNewGame()
	{
		ModeConfig freeMode = Configs.freeMode;
		this.coreEnergyCap = freeMode.mechaCoreEnergyCap;
		this.coreEnergy = this.coreEnergyCap;
		this.corePowerGen = freeMode.mechaCorePowerGen;
		this.reactorPowerGen = freeMode.mechaReactorPowerGen;
		this.reactorEnergy = 0.0;
		this.reactorItemId = 0;
		this.reactorItemInc = 0;
		this.autoReplenishFuel = false;
		this.autoReplenishWarper = false;
		this.reactorStorage.Clear();
		this.reactorStorage.type = EStorageType.Fuel;
		this.warpStorage.Clear();
		this.warpStorage.type = EStorageType.Filtered;
		this.warpStorage.SetFilterDirect(0, 1210, 20);
		this.energyConsumptionCoef = 1.0;
		this.walkPower = freeMode.mechaWalkPower;
		this.jumpEnergy = freeMode.mechaJumpEnergy;
		this.thrustPowerPerAcc = freeMode.mechaThrustPowerPerAcc;
		this.warpKeepingPowerPerSpeed = freeMode.mechaWarpKeepingPowerPerSpeed;
		this.warpStartPowerPerSpeed = freeMode.mechaWarpStartPowerPerSpeed;
		this.miningPower = freeMode.mechaMiningPower;
		this.replicatePower = freeMode.mechaReplicatePower;
		this.researchPower = freeMode.mechaResearchPower;
		this.droneEjectEnergy = freeMode.droneEjectEnergy;
		this.droneEnergyPerMeter = freeMode.droneEnergyPerMeter;
		this.instantBuildEnergy = freeMode.mechaInstantBuildEnergy;
		this.coreLevel = freeMode.mechaCoreLevel;
		this.thrusterLevel = freeMode.mechaThrusterLevel;
		this.miningSpeed = freeMode.mechaMiningSpeed;
		this.replicateSpeed = freeMode.mechaReplicateSpeed;
		this.walkSpeed = freeMode.mechaWalkSpeed;
		this.jumpSpeed = freeMode.mechaJumpSpeed;
		this.maxSailSpeed = freeMode.mechaSailSpeedMax;
		this.maxWarpSpeed = freeMode.mechaWarpSpeedMax;
		this.buildArea = freeMode.mechaBuildArea;
		this.constructionModule.Setup(freeMode);
		this.hpMax = freeMode.mechaHpMax;
		this.hpMaxUpgrade = 0;
		this.hpRecover = freeMode.mechaHpRecover;
		this.energyShieldUnlocked = freeMode.unlockEnergyShield;
		this.energyShieldRechargeEnabled = true;
		this.energyShieldRechargeSpeed = 2f;
		this.energyShieldRadius = freeMode.energyShieldRadius;
		this.energyShieldCapacity = freeMode.energyShieldCapacity;
		this.energyShieldEnergyRate = freeMode.energyShieldEnergyRate;
		this.hp = this.hpMax;
		this.hpRecoverCD = 0;
		this.energyShieldRecoverCD = 0;
		this.energyShieldEnergy = this.energyShieldCapacity;
		this.energyShieldBurstUnlocked = freeMode.unlockEnergyShieldBurst;
		this.energyShieldBurstDamageRate = freeMode.energyShieldBurstDamageRate;
		this.energyShieldBurstProgress = 0.0;
		this.ammoItemId = 0;
		this.ammoInc = 0;
		this.ammoBulletCount = 0;
		this.ammoSelectSlot = 1;
		this.ammoSelectSlotState = 1;
		this.ammoMuzzleFire = 0;
		this.ammoRoundFire = 0;
		this.ammoMuzzleIndex = 0;
		this.laserActive = freeMode.mechaLaserActive;
		this.laserActiveState = 0;
		this.laserRecharging = false;
		this.laserEnergy = (long)freeMode.mechaLaserEnergyCapacity;
		this.laserEnergyCapacity = (long)freeMode.mechaLaserEnergyCapacity;
		this.laserFire = 0;
		this.bombActive = false;
		this.bombFire = 0;
		this.autoReplenishAmmo = true;
		this.ammoStorage.Clear();
		this.ammoStorage.type = EStorageType.Ammo;
		this.bombStorage.Clear();
		this.bombStorage.type = EStorageType.Bomb;
		this.ammoHatredTarget.SetNull();
		this.laserHatredTarget.SetNull();
		this.bulletLocalAttackRange = freeMode.mechaLocalBulletAttackRange;
		this.bulletSpaceAttackRange = freeMode.mechaSpaceBulletAttackRange;
		this.bulletEnergyCost = freeMode.mechaBulletEnergyCost;
		this.bulletDamageScale = freeMode.mechaBulletDamageScale;
		this.bulletROF = freeMode.mechaBulletROF;
		this.bulletMuzzleCount = freeMode.mechaBulletMuzzleCount;
		this.bulletMuzzleInterval = freeMode.mechaBulletMuzzleInterval;
		this.bulletRoundInterval = freeMode.mechaBulletRoundInterval;
		this.cannonLocalAttackRange = freeMode.mechaLocalCannonAttackRange;
		this.cannonSpaceAttackRange = freeMode.mechaSpaceCannonAttackRange;
		this.cannonEnergyCost = freeMode.mechaCannonEnergyCost;
		this.cannonDamageScale = freeMode.mechaCannonDamageScale;
		this.cannonROF = freeMode.mechaCannonROF;
		this.cannonMuzzleCount = freeMode.mechaCannonMuzzleCount;
		this.cannonMuzzleInterval = freeMode.mechaCannonMuzzleInterval;
		this.cannonRoundInterval = freeMode.mechaCannonRoundInterval;
		this.plasmaLocalAttackRange = freeMode.mechaLocalPlasmaAttackRange;
		this.plasmaSpaceAttackRange = freeMode.mechaSpacePlasmaAttackRange;
		this.plasmaEnergyCost = freeMode.mechaPlasmaEnergyCost;
		this.plasmaDamageScale = freeMode.mechaPlasmaDamageScale;
		this.plasmaROF = freeMode.mechaPlasmaROF;
		this.plasmaMuzzleCount = freeMode.mechaPlasmaMuzzleCount;
		this.plasmaMuzzleInterval = freeMode.mechaPlasmaMuzzleInterval;
		this.plasmaRoundInterval = freeMode.mechaPlasmaRoundInterval;
		this.missileLocalAttackRange = freeMode.mechaLocalMissileAttackRange;
		this.missileSpaceAttackRange = freeMode.mechaSpaceMissileAttackRange;
		this.missileEnergyCost = freeMode.mechaMissileEnergyCost;
		this.missileDamageScale = freeMode.mechaMissileDamageScale;
		this.missileROF = freeMode.mechaMissileROF;
		this.missileMuzzleCount = freeMode.mechaMissileMuzzleCount;
		this.missileMuzzleInterval = freeMode.mechaMissileMuzzleInterval;
		this.missileRoundInterval = freeMode.mechaMissileRoundInterval;
		this.laserLocalAttackRange = freeMode.mechaLocalLaserAttackRange;
		this.laserSpaceAttackRange = freeMode.mechaSpaceLaserAttackRange;
		this.laserLocalEnergyCost = freeMode.mechaLocalLaserEnergyCost;
		this.laserSpaceEnergyCost = freeMode.mechaSpaceLaserEnergyCost;
		this.laserLocalDamage = freeMode.mechaLocalLaserDamage;
		this.laserSpaceDamage = freeMode.mechaSpaceLaserDamage;
		this.laserLocalInterval = freeMode.mechaLocalLaserInterval;
		this.laserSpaceInterval = freeMode.mechaSpaceLaserInterval;
		this.autoReplenishHangar = true;
		this.fighterStorage.Clear();
		this.fighterStorage.type = EStorageType.Fighter;
		this.groundCombatModule.Setup(1, this.gameData);
		this.spaceCombatModule.Setup(3, this.gameData);
		this.appearance.ResetAppearance();
		this.diyAppearance.ResetAppearance();
		this.diyItems.Clear();
	}

	// Token: 0x06001226 RID: 4646 RVA: 0x001506C0 File Offset: 0x0014E8C0
	public void GameTick(long time, float dt)
	{
		if (this.hp > 0)
		{
			CombatUpgradeData combatUpgradeData = default(CombatUpgradeData);
			this.history.GetCombatUpgradeData(ref combatUpgradeData);
			PlanetFactory factory = this.player.factory;
			if (factory != null)
			{
				DeepProfiler.BeginSample(DPEntry.Construction, -1, -1L);
				this.constructionModule.GameTick(time, factory, this.player);
				DeepProfiler.EndSample(-1, -2L);
				DeepProfiler.BeginSample(DPEntry.CBGCombatModule, -1, 0L);
				this.groundCombatModule.GameTick(time, factory, this.player, ref combatUpgradeData);
				DeepProfiler.EndSample(-1, -2L);
				DeepProfiler.BeginSample(DPEntry.CBSCombatModule, -1, 0L);
				this.spaceCombatModule.GameTick(time, null, this.player, ref combatUpgradeData);
				DeepProfiler.EndSample(-1, -2L);
			}
			else
			{
				DeepProfiler.BeginSample(DPEntry.CBSCombatModule, -1, 0L);
				this.spaceCombatModule.GameTick(time, null, this.player, ref combatUpgradeData);
				DeepProfiler.EndSample(-1, -2L);
			}
			DeepProfiler.BeginSample(DPEntry.Icarus, -1, 21L);
			this.forge.GameTick(time, dt);
			DeepProfiler.EndSample(-1, -2L);
			DeepProfiler.BeginSample(DPEntry.Icarus, -1, 22L);
			this.lab.GameTick(time, dt);
			DeepProfiler.EndSample(-1, -2L);
		}
		this.UpdateCombatStats((double)dt);
		this.UpdateSkillColliders();
	}

	// Token: 0x06001227 RID: 4647 RVA: 0x001507F0 File Offset: 0x0014E9F0
	public void GenerateEnergy(double dt)
	{
		this.ClearEnergyChange();
		this.ClearChargerDevice();
		if (!this.player.isAlive)
		{
			return;
		}
		double num = this.corePowerGen * dt;
		this.coreEnergy += num;
		if (this.coreEnergy > this.coreEnergyCap)
		{
			this.coreEnergy = this.coreEnergyCap;
		}
		this.MarkEnergyChange(0, num);
		double num2 = 1.0;
		ItemProto itemProto = (this.reactorItemId > 0) ? LDB.items.Select(this.reactorItemId) : null;
		if (itemProto != null)
		{
			num2 = (double)(itemProto.ReactorInc + 1f);
			if (this.reactorItemInc > 0)
			{
				if (itemProto.Productive)
				{
					num2 *= 1.0 + Cargo.incTableMilli[this.reactorItemInc];
				}
				else
				{
					num2 *= 1.0 + Cargo.accTableMilli[this.reactorItemInc];
				}
			}
		}
		double num3 = this.coreEnergyCap - this.coreEnergy;
		double num4 = this.reactorPowerGen * num2 * dt;
		if (num4 > num3)
		{
			num4 = num3;
		}
		while (this.reactorEnergy < num4)
		{
			if (this.reactorItemId == 2207)
			{
				this.player.TryAddItemToPackage(2206, 1, this.reactorItemInc, true, 0, false);
				UIItemup.Up(2206, 1);
			}
			int num5 = -1;
			for (int i = this.reactorStorage.size - 1; i >= 0; i--)
			{
				if (this.reactorStorage.grids[i].count > 0)
				{
					num5 = i;
					break;
				}
			}
			if (num5 == -1 && this.autoReplenishFuel)
			{
				this.AutoReplenishFuelAll();
				for (int j = this.reactorStorage.size - 1; j >= 0; j--)
				{
					if (this.reactorStorage.grids[j].count > 0)
					{
						num5 = j;
						break;
					}
				}
			}
			int num6 = 0;
			int num7 = 1;
			int num8;
			this.reactorStorage.TakeTailItems(ref num6, ref num7, out num8, false);
			if (num7 <= 0 || num6 <= 0)
			{
				this.reactorItemId = 0;
				this.reactorItemInc = 0;
				break;
			}
			this.AddConsumptionStat(num6, num7, this.player.nearestFactory);
			this.reactorItemId = num6;
			ItemProto itemProto2 = LDB.items.Select(num6);
			this.reactorItemInc = ((num8 > 10) ? 10 : num8);
			if (itemProto2 != null)
			{
				this.reactorEnergy += (double)itemProto2.HeatValue * (1.0 + (itemProto2.Productive ? Cargo.incTableMilli[this.reactorItemInc] : 0.0));
			}
			this.history.AddFeatureValue(2100000 + num6, num7);
			if (this.autoReplenishFuel && this.reactorStorage.grids[num5].count == 0)
			{
				if (num5 == 0)
				{
					if (!this.AutoReplenishFuel(this.reactorItemId, 0))
					{
						this.AutoReplenishFuelAll();
					}
				}
				else
				{
					this.AutoReplenishFuel(this.reactorItemId, num5);
				}
			}
		}
		if (this.reactorEnergy > 0.0)
		{
			this.MarkEnergyChange(1, this.reactorPowerGen * num2 * dt);
			if (num4 > this.reactorEnergy)
			{
				num4 = this.reactorEnergy;
			}
			this.coreEnergy += num4;
			this.reactorEnergy -= num4;
		}
		if (this.history.HasFeatureKey(1100000) && GameMain.sandboxToolsEnabled)
		{
			this.coreEnergy = this.coreEnergyCap;
		}
	}

	// Token: 0x06001228 RID: 4648 RVA: 0x00150B57 File Offset: 0x0014ED57
	public void OnDraw()
	{
	}

	// Token: 0x06001229 RID: 4649 RVA: 0x00150B5C File Offset: 0x0014ED5C
	public void Free()
	{
		if (this.reactorStorage != null)
		{
			this.reactorStorage.Free();
			this.reactorStorage = null;
		}
		if (this.warpStorage != null)
		{
			this.warpStorage.Free();
			this.warpStorage = null;
		}
		if (this.forge != null)
		{
			this.forge.Free();
			this.forge = null;
		}
		if (this.lab != null)
		{
			this.lab.Free();
			this.lab = null;
		}
		this.constructionModule.Reset();
		if (this.ammoStorage != null)
		{
			this.ammoStorage.Free();
			this.ammoStorage = null;
		}
		if (this.bombStorage != null)
		{
			this.bombStorage.Free();
			this.bombStorage = null;
		}
		if (this.fighterStorage != null)
		{
			this.fighterStorage.Free();
			this.fighterStorage = null;
		}
		if (this.groundCombatModule != null)
		{
			this.groundCombatModule.Free();
			this.groundCombatModule = null;
		}
		if (this.spaceCombatModule != null)
		{
			this.spaceCombatModule.Free();
			this.spaceCombatModule = null;
		}
		if (this.appearance != null)
		{
			this.appearance.Free();
			this.appearance = null;
		}
		if (this.diyAppearance != null)
		{
			this.diyAppearance.Free();
			this.diyAppearance = null;
		}
		this.player.onPackageAddItem -= this.OnPlayerPackageAddItem;
		this.player.onReplenishPreferred -= this.OnPlayerReplenishPreferred;
	}

	// Token: 0x0600122A RID: 4650 RVA: 0x00150CC0 File Offset: 0x0014EEC0
	public void Export(BinaryWriter w)
	{
		w.Write(11);
		w.Write(this.coreEnergyCap);
		w.Write(this.coreEnergy);
		w.Write(this.corePowerGen);
		w.Write(this.reactorPowerGen);
		w.Write(this.reactorEnergy);
		w.Write(this.reactorItemId);
		w.Write(this.reactorItemInc);
		w.Write(this.autoReplenishFuel);
		w.Write(this.autoReplenishWarper);
		this.reactorStorage.Export(w);
		this.warpStorage.Export(w);
		w.Write(this.energyConsumptionCoef);
		w.Write(this.walkPower);
		w.Write(this.jumpEnergy);
		w.Write(this.thrustPowerPerAcc);
		w.Write(this.warpKeepingPowerPerSpeed);
		w.Write(this.warpStartPowerPerSpeed);
		w.Write(this.miningPower);
		w.Write(this.replicatePower);
		w.Write(this.researchPower);
		w.Write(this.droneEjectEnergy);
		w.Write(this.droneEnergyPerMeter);
		w.Write(this.instantBuildEnergy);
		w.Write(this.coreLevel);
		w.Write(this.thrusterLevel);
		w.Write(this.miningSpeed);
		w.Write(this.replicateSpeed);
		w.Write(this.walkSpeed);
		w.Write(this.jumpSpeed);
		w.Write(this.maxSailSpeed);
		w.Write(this.maxWarpSpeed);
		w.Write(this.buildArea);
		this.forge.Export(w);
		this.lab.Export(w);
		this.constructionModule.Export(w);
		w.Write(this.autoReconstructLastSearchPos.x);
		w.Write(this.autoReconstructLastSearchPos.y);
		w.Write(this.autoReconstructLastSearchPos.z);
		w.Write(this.autoReconstructLastSearchAstroId);
		w.Write(this.buildLastSearchPos.x);
		w.Write(this.buildLastSearchPos.y);
		w.Write(this.buildLastSearchPos.z);
		w.Write(this.buildLastSearchAstroId);
		w.Write(this.repairLastSearchPos.x);
		w.Write(this.repairLastSearchPos.y);
		w.Write(this.repairLastSearchPos.z);
		w.Write(this.repairLastSearchAstroId);
		w.Write(this.hpMax);
		w.Write(this.hpMaxUpgrade);
		w.Write(this.hpRecover);
		w.Write(this.energyShieldUnlocked);
		w.Write(this.energyShieldRechargeEnabled);
		w.Write(this.energyShieldRechargeSpeed);
		w.Write(this.energyShieldRadius);
		w.Write(this.energyShieldCapacity);
		w.Write(this.energyShieldEnergyRate);
		w.Write(this.hp);
		w.Write(this.hpRecoverCD);
		w.Write(this.energyShieldRecoverCD);
		w.Write(this.energyShieldEnergy);
		w.Write(this.energyShieldBurstUnlocked);
		w.Write(this.energyShieldBurstDamageRate);
		w.Write(this.ammoItemId);
		w.Write(this.ammoInc);
		w.Write(this.ammoBulletCount);
		w.Write(this.ammoSelectSlot);
		w.Write(this.ammoSelectSlotState);
		w.Write(this.ammoMuzzleFire);
		w.Write(this.ammoRoundFire);
		w.Write(this.ammoMuzzleIndex);
		w.Write(this.laserActive);
		w.Write(this.laserActiveState);
		w.Write(this.laserRecharging);
		w.Write(this.laserEnergy);
		w.Write(this.laserEnergyCapacity);
		w.Write(this.laserFire);
		w.Write(this.bombActive);
		w.Write(this.bombFire);
		w.Write(this.autoReplenishAmmo);
		this.ammoStorage.Export(w);
		this.bombStorage.Export(w);
		this.ammoHatredTarget.Export(w);
		this.laserHatredTarget.Export(w);
		w.Write(this.bulletLocalAttackRange);
		w.Write(this.bulletSpaceAttackRange);
		w.Write(this.bulletEnergyCost);
		w.Write(this.bulletDamageScale);
		w.Write(this.bulletROF);
		w.Write(this.bulletMuzzleCount);
		w.Write(this.bulletMuzzleInterval);
		w.Write(this.bulletRoundInterval);
		w.Write(this.cannonLocalAttackRange);
		w.Write(this.cannonSpaceAttackRange);
		w.Write(this.cannonEnergyCost);
		w.Write(this.cannonDamageScale);
		w.Write(this.cannonROF);
		w.Write(this.cannonMuzzleCount);
		w.Write(this.cannonMuzzleInterval);
		w.Write(this.cannonRoundInterval);
		w.Write(this.plasmaLocalAttackRange);
		w.Write(this.plasmaSpaceAttackRange);
		w.Write(this.plasmaEnergyCost);
		w.Write(this.plasmaDamageScale);
		w.Write(this.plasmaROF);
		w.Write(this.plasmaMuzzleCount);
		w.Write(this.plasmaMuzzleInterval);
		w.Write(this.plasmaRoundInterval);
		w.Write(this.missileLocalAttackRange);
		w.Write(this.missileSpaceAttackRange);
		w.Write(this.missileEnergyCost);
		w.Write(this.missileDamageScale);
		w.Write(this.missileROF);
		w.Write(this.missileMuzzleCount);
		w.Write(this.missileMuzzleInterval);
		w.Write(this.missileRoundInterval);
		w.Write(this.laserLocalAttackRange);
		w.Write(this.laserSpaceAttackRange);
		w.Write(this.laserLocalEnergyCost);
		w.Write(this.laserSpaceEnergyCost);
		w.Write(this.laserLocalDamage);
		w.Write(this.laserSpaceDamage);
		w.Write(this.laserLocalInterval);
		w.Write(this.laserSpaceInterval);
		w.Write(this.autoReplenishHangar);
		this.fighterStorage.Export(w);
		this.groundCombatModule.Export(w);
		this.spaceCombatModule.Export(w);
		if (this.energyShieldResistHistory != null)
		{
			w.Write(this.energyShieldResistHistory.Length);
			for (int i = 0; i < this.energyShieldResistHistory.Length; i++)
			{
				w.Write(this.energyShieldResistHistory[i]);
			}
		}
		else
		{
			w.Write(0);
		}
		this.appearance.Export(w);
		this.diyAppearance.Export(w);
		w.Write(this.diyItems.items.Count);
		foreach (KeyValuePair<int, int> keyValuePair in this.diyItems.items)
		{
			w.Write(keyValuePair.Key);
			w.Write(keyValuePair.Value);
		}
		w.Write(2119973658);
	}

	// Token: 0x0600122B RID: 4651 RVA: 0x001513D0 File Offset: 0x0014F5D0
	public void Import(BinaryReader r)
	{
		ModeConfig freeMode = Configs.freeMode;
		int num = r.ReadInt32();
		this.coreEnergyCap = r.ReadDouble();
		this.coreEnergy = r.ReadDouble();
		this.corePowerGen = r.ReadDouble();
		this.reactorPowerGen = r.ReadDouble();
		this.reactorEnergy = r.ReadDouble();
		this.reactorItemId = r.ReadInt32();
		if (num >= 4)
		{
			this.reactorItemInc = r.ReadInt32();
		}
		else
		{
			this.reactorItemInc = 0;
		}
		if (num >= 7)
		{
			this.autoReplenishFuel = r.ReadBoolean();
		}
		else
		{
			this.autoReplenishFuel = false;
		}
		if (num >= 11)
		{
			this.autoReplenishWarper = r.ReadBoolean();
		}
		else
		{
			this.autoReplenishWarper = false;
		}
		this.reactorStorage.Import(r);
		this.warpStorage.Import(r);
		if (num >= 8)
		{
			this.energyConsumptionCoef = r.ReadDouble();
		}
		else
		{
			this.energyConsumptionCoef = 1.0;
		}
		this.walkPower = r.ReadDouble();
		this.jumpEnergy = r.ReadDouble();
		this.thrustPowerPerAcc = r.ReadDouble();
		this.warpKeepingPowerPerSpeed = r.ReadDouble();
		this.warpStartPowerPerSpeed = r.ReadDouble();
		this.miningPower = r.ReadDouble();
		this.replicatePower = r.ReadDouble();
		this.researchPower = r.ReadDouble();
		this.droneEjectEnergy = r.ReadDouble();
		this.droneEnergyPerMeter = r.ReadDouble();
		if (num >= 7)
		{
			this.instantBuildEnergy = r.ReadDouble();
		}
		else
		{
			this.instantBuildEnergy = freeMode.mechaInstantBuildEnergy;
		}
		this.coreLevel = r.ReadInt32();
		this.thrusterLevel = r.ReadInt32();
		this.miningSpeed = r.ReadSingle();
		this.replicateSpeed = r.ReadSingle();
		this.walkSpeed = r.ReadSingle();
		this.jumpSpeed = r.ReadSingle();
		this.maxSailSpeed = r.ReadSingle();
		this.maxWarpSpeed = r.ReadSingle();
		this.buildArea = r.ReadSingle();
		this.forge.Import(r);
		this.lab.Import(r);
		if (num >= 7)
		{
			this.constructionModule.Import(r);
			this.constructionModule.AfterImport(null);
			if (num >= 10)
			{
				this.autoReconstructLastSearchPos.x = r.ReadSingle();
				this.autoReconstructLastSearchPos.y = r.ReadSingle();
				this.autoReconstructLastSearchPos.z = r.ReadSingle();
				this.autoReconstructLastSearchAstroId = r.ReadInt32();
				this.buildLastSearchPos.x = r.ReadSingle();
				this.buildLastSearchPos.y = r.ReadSingle();
				this.buildLastSearchPos.z = r.ReadSingle();
				this.buildLastSearchAstroId = r.ReadInt32();
				this.repairLastSearchPos.x = r.ReadSingle();
				this.repairLastSearchPos.y = r.ReadSingle();
				this.repairLastSearchPos.z = r.ReadSingle();
				this.repairLastSearchAstroId = r.ReadInt32();
			}
			else
			{
				this.autoReconstructLastSearchPos = Vector3.zero;
				this.autoReconstructLastSearchAstroId = 0;
				this.buildLastSearchPos = Vector3.zero;
				this.buildLastSearchAstroId = 0;
				this.repairLastSearchPos = Vector3.zero;
				this.repairLastSearchAstroId = 0;
			}
			this.hpMax = r.ReadInt32();
			if (num >= 8)
			{
				this.hpMaxUpgrade = r.ReadInt32();
			}
			else
			{
				this.hpMaxUpgrade = 0;
			}
			this.hpRecover = r.ReadInt32();
			this.energyShieldUnlocked = r.ReadBoolean();
			this.energyShieldRechargeEnabled = r.ReadBoolean();
			this.energyShieldRechargeSpeed = r.ReadSingle();
			this.energyShieldRadius = r.ReadSingle();
			this.energyShieldCapacity = r.ReadInt64();
			this.energyShieldEnergyRate = r.ReadInt64();
			this.hp = r.ReadInt32();
			this.hpRecoverCD = r.ReadInt32();
			this.energyShieldRecoverCD = r.ReadInt32();
			this.energyShieldEnergy = r.ReadInt64();
			if (num >= 10)
			{
				this.energyShieldBurstUnlocked = r.ReadBoolean();
				this.energyShieldBurstDamageRate = r.ReadInt64();
			}
			else
			{
				this.energyShieldBurstUnlocked = this.history.techStates[2803].unlocked;
				this.energyShieldBurstDamageRate = freeMode.energyShieldBurstDamageRate;
			}
			this.energyShieldBurstDamageRate = freeMode.energyShieldBurstDamageRate;
			this.ammoItemId = r.ReadInt32();
			this.ammoInc = r.ReadInt32();
			this.ammoBulletCount = r.ReadInt32();
			this.ammoSelectSlot = r.ReadInt32();
			if (num >= 9)
			{
				this.ammoSelectSlotState = r.ReadInt32();
			}
			else
			{
				this.ammoSelectSlotState = 0;
			}
			this.ammoMuzzleFire = r.ReadInt32();
			this.ammoRoundFire = r.ReadInt32();
			this.ammoMuzzleIndex = r.ReadInt32();
			this.laserActive = r.ReadBoolean();
			if (num >= 9)
			{
				this.laserActiveState = r.ReadInt32();
			}
			else
			{
				this.laserActiveState = 1;
			}
			this.laserRecharging = r.ReadBoolean();
			this.laserEnergy = r.ReadInt64();
			this.laserEnergyCapacity = r.ReadInt64();
			this.laserFire = r.ReadInt32();
			if (num >= 9)
			{
				this.bombActive = r.ReadBoolean();
			}
			else
			{
				this.bombActive = false;
			}
			this.bombFire = r.ReadInt32();
			this.autoReplenishAmmo = r.ReadBoolean();
			this.ammoStorage.Import(r);
			this.bombStorage.Import(r);
			this.ammoHatredTarget.Import(r);
			this.laserHatredTarget.Import(r);
			this.bulletLocalAttackRange = r.ReadSingle();
			this.bulletSpaceAttackRange = r.ReadSingle();
			this.bulletEnergyCost = r.ReadInt32();
			this.bulletDamageScale = r.ReadSingle();
			this.bulletROF = r.ReadInt32();
			this.bulletMuzzleCount = r.ReadInt32();
			this.bulletMuzzleInterval = r.ReadInt32();
			this.bulletRoundInterval = r.ReadInt32();
			this.cannonLocalAttackRange = r.ReadSingle();
			this.cannonSpaceAttackRange = r.ReadSingle();
			this.cannonEnergyCost = r.ReadInt32();
			this.cannonDamageScale = r.ReadSingle();
			this.cannonROF = r.ReadInt32();
			this.cannonMuzzleCount = r.ReadInt32();
			this.cannonMuzzleInterval = r.ReadInt32();
			this.cannonRoundInterval = r.ReadInt32();
			this.plasmaLocalAttackRange = r.ReadSingle();
			this.plasmaSpaceAttackRange = r.ReadSingle();
			this.plasmaEnergyCost = r.ReadInt32();
			this.plasmaDamageScale = r.ReadSingle();
			this.plasmaROF = r.ReadInt32();
			this.plasmaMuzzleCount = r.ReadInt32();
			this.plasmaMuzzleInterval = r.ReadInt32();
			this.plasmaRoundInterval = r.ReadInt32();
			this.missileLocalAttackRange = r.ReadSingle();
			this.missileSpaceAttackRange = r.ReadSingle();
			this.missileEnergyCost = r.ReadInt32();
			this.missileDamageScale = r.ReadSingle();
			this.missileROF = r.ReadInt32();
			this.missileMuzzleCount = r.ReadInt32();
			this.missileMuzzleInterval = r.ReadInt32();
			this.missileRoundInterval = r.ReadInt32();
			this.laserLocalAttackRange = r.ReadSingle();
			this.laserSpaceAttackRange = r.ReadSingle();
			this.laserLocalEnergyCost = r.ReadInt32();
			this.laserSpaceEnergyCost = r.ReadInt32();
			this.laserLocalDamage = r.ReadInt32();
			this.laserSpaceDamage = r.ReadInt32();
			this.laserLocalInterval = r.ReadInt32();
			this.laserSpaceInterval = r.ReadInt32();
			this.autoReplenishHangar = r.ReadBoolean();
			this.fighterStorage.Import(r);
			this.groundCombatModule.Import(r);
			this.groundCombatModule.AfterImport(this.gameData);
			this.spaceCombatModule.Import(r);
			this.spaceCombatModule.AfterImport(this.gameData);
			int num2 = r.ReadInt32();
			if (num2 > 0)
			{
				this.energyShieldResistHistory = new long[num2];
				for (int i = 0; i < num2; i++)
				{
					this.energyShieldResistHistory[i] = r.ReadInt64();
				}
			}
		}
		else
		{
			this.constructionModule.Setup(freeMode);
			this.autoReconstructLastSearchPos = Vector3.zero;
			this.autoReconstructLastSearchAstroId = 0;
			this.buildLastSearchPos = Vector3.zero;
			this.buildLastSearchAstroId = 0;
			this.repairLastSearchPos = Vector3.zero;
			this.repairLastSearchAstroId = 0;
			int num3 = r.ReadInt32();
			this.constructionModule.droneCount = num3;
			this.constructionModule.droneAliveCount = num3;
			this.constructionModule.droneIdleCount = num3;
			this.history.constructionDroneSpeed = r.ReadSingle();
			this.history.constructionDroneMovement = r.ReadInt32();
			for (int j = 0; j < num3; j++)
			{
				r.ReadInt32();
				r.ReadInt32();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadInt32();
				r.ReadInt32();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
				r.ReadSingle();
			}
			this.hpMax = freeMode.mechaHpMax;
			this.hpMaxUpgrade = 0;
			this.hpRecover = freeMode.mechaHpRecover;
			this.energyShieldUnlocked = freeMode.unlockEnergyShield;
			this.energyShieldRechargeEnabled = true;
			this.energyShieldRechargeSpeed = 2f;
			this.energyShieldRadius = freeMode.energyShieldRadius;
			this.energyShieldCapacity = freeMode.energyShieldCapacity;
			this.energyShieldEnergyRate = freeMode.energyShieldEnergyRate;
			this.hp = this.hpMaxApplied;
			this.hpRecoverCD = 0;
			this.energyShieldRecoverCD = 0;
			this.energyShieldEnergy = this.energyShieldCapacity;
			this.energyShieldBurstProgress = 0.0;
			this.energyShieldBurstUnlocked = freeMode.unlockEnergyShieldBurst;
			this.energyShieldBurstDamageRate = freeMode.energyShieldBurstDamageRate;
			this.ammoItemId = 0;
			this.ammoInc = 0;
			this.ammoBulletCount = 0;
			this.ammoSelectSlot = 1;
			this.ammoSelectSlotState = 0;
			this.ammoMuzzleFire = 0;
			this.ammoRoundFire = 0;
			this.ammoMuzzleIndex = 0;
			this.laserActive = freeMode.mechaLaserActive;
			this.laserActiveState = 1;
			this.laserRecharging = false;
			this.laserEnergy = (long)freeMode.mechaLaserEnergyCapacity;
			this.laserEnergyCapacity = (long)freeMode.mechaLaserEnergyCapacity;
			this.laserFire = 0;
			this.bombActive = false;
			this.bombFire = 0;
			this.autoReplenishAmmo = true;
			this.ammoStorage.Clear();
			this.ammoStorage.type = EStorageType.Ammo;
			this.bombStorage.Clear();
			this.bombStorage.type = EStorageType.Bomb;
			this.ammoHatredTarget.SetNull();
			this.laserHatredTarget.SetNull();
			this.bulletLocalAttackRange = freeMode.mechaLocalBulletAttackRange;
			this.bulletSpaceAttackRange = freeMode.mechaSpaceBulletAttackRange;
			this.bulletEnergyCost = freeMode.mechaBulletEnergyCost;
			this.bulletDamageScale = freeMode.mechaBulletDamageScale;
			this.bulletROF = freeMode.mechaBulletROF;
			this.bulletMuzzleCount = freeMode.mechaBulletMuzzleCount;
			this.bulletMuzzleInterval = freeMode.mechaBulletMuzzleInterval;
			this.bulletRoundInterval = freeMode.mechaBulletRoundInterval;
			this.cannonLocalAttackRange = freeMode.mechaLocalCannonAttackRange;
			this.cannonSpaceAttackRange = freeMode.mechaSpaceCannonAttackRange;
			this.cannonEnergyCost = freeMode.mechaCannonEnergyCost;
			this.cannonDamageScale = freeMode.mechaCannonDamageScale;
			this.cannonROF = freeMode.mechaCannonROF;
			this.cannonMuzzleCount = freeMode.mechaCannonMuzzleCount;
			this.cannonMuzzleInterval = freeMode.mechaCannonMuzzleInterval;
			this.cannonRoundInterval = freeMode.mechaCannonRoundInterval;
			this.plasmaLocalAttackRange = freeMode.mechaLocalPlasmaAttackRange;
			this.plasmaSpaceAttackRange = freeMode.mechaSpacePlasmaAttackRange;
			this.plasmaEnergyCost = freeMode.mechaPlasmaEnergyCost;
			this.plasmaDamageScale = freeMode.mechaPlasmaDamageScale;
			this.plasmaROF = freeMode.mechaPlasmaROF;
			this.plasmaMuzzleCount = freeMode.mechaPlasmaMuzzleCount;
			this.plasmaMuzzleInterval = freeMode.mechaPlasmaMuzzleInterval;
			this.plasmaRoundInterval = freeMode.mechaPlasmaRoundInterval;
			this.missileLocalAttackRange = freeMode.mechaLocalMissileAttackRange;
			this.missileSpaceAttackRange = freeMode.mechaSpaceMissileAttackRange;
			this.missileEnergyCost = freeMode.mechaMissileEnergyCost;
			this.missileDamageScale = freeMode.mechaMissileDamageScale;
			this.missileROF = freeMode.mechaMissileROF;
			this.missileMuzzleCount = freeMode.mechaMissileMuzzleCount;
			this.missileMuzzleInterval = freeMode.mechaMissileMuzzleInterval;
			this.missileRoundInterval = freeMode.mechaMissileRoundInterval;
			this.laserLocalAttackRange = freeMode.mechaLocalLaserAttackRange;
			this.laserSpaceAttackRange = freeMode.mechaSpaceLaserAttackRange;
			this.laserLocalEnergyCost = freeMode.mechaLocalLaserEnergyCost;
			this.laserSpaceEnergyCost = freeMode.mechaSpaceLaserEnergyCost;
			this.laserLocalDamage = freeMode.mechaLocalLaserDamage;
			this.laserSpaceDamage = freeMode.mechaSpaceLaserDamage;
			this.laserLocalInterval = freeMode.mechaLocalLaserInterval;
			this.laserSpaceInterval = freeMode.mechaSpaceLaserInterval;
			this.autoReplenishHangar = true;
			this.fighterStorage.Clear();
			this.fighterStorage.type = EStorageType.Fighter;
			this.groundCombatModule.Setup(1, this.gameData);
			this.spaceCombatModule.Setup(3, this.gameData);
		}
		this.appearance.ResetAppearance();
		this.diyAppearance.ResetAppearance();
		this.diyItems.Clear();
		if (num < 5)
		{
			if (num >= 1)
			{
				int num4 = r.ReadInt32();
				for (int k = 0; k < num4; k++)
				{
					if (k < 8)
					{
						this.appearance.mainColors[k].r = r.ReadByte();
						this.appearance.mainColors[k].g = r.ReadByte();
						this.appearance.mainColors[k].b = r.ReadByte();
						this.appearance.mainColors[k].a = r.ReadByte();
					}
					else
					{
						r.ReadByte();
						r.ReadByte();
						r.ReadByte();
						r.ReadByte();
					}
				}
				if (num >= 3)
				{
					for (int l = 0; l < 64; l++)
					{
						for (int m = 0; m < num4; m++)
						{
							if (m < 8)
							{
								this.appearance.partColors[l, m].r = r.ReadByte();
								this.appearance.partColors[l, m].g = r.ReadByte();
								this.appearance.partColors[l, m].b = r.ReadByte();
								this.appearance.partColors[l, m].a = r.ReadByte();
							}
							else
							{
								r.ReadByte();
								r.ReadByte();
								r.ReadByte();
								r.ReadByte();
							}
						}
					}
				}
			}
			if (num >= 2)
			{
				this.appearance.partHideMask = r.ReadUInt64();
				if (num >= 3)
				{
					this.appearance.partCustomMask = r.ReadUInt64();
				}
				this.appearance.customArmor.Import(r);
				if (r.ReadInt32() != 2119973658)
				{
					throw new Exception("Corrupted Mecha Data");
				}
			}
			this.appearance.CopyTo(this.diyAppearance);
			return;
		}
		this.appearance.Import(r);
		this.diyAppearance.Import(r);
		if (num >= 6)
		{
			int num5 = r.ReadInt32();
			for (int n = 0; n < num5; n++)
			{
				int key = r.ReadInt32();
				int value = r.ReadInt32();
				this.diyItems.items[key] = value;
			}
		}
		if (this.gameData.patch < 21)
		{
			double[] array = new double[8];
			double[] array2 = new double[8];
			double[] array3 = new double[8];
			int[] array4 = new int[8];
			MechaMaterialSetting[] mechaArmorMaterials = Configs.builtin.mechaArmorMaterials;
			for (int num6 = 0; num6 < 8; num6++)
			{
				int protoId = this.appearance.customArmor.materials[num6].protoId;
				array4[num6] = mechaArmorMaterials[protoId].itemId;
				array3[num6] = (double)mechaArmorMaterials[protoId].cost;
			}
			foreach (BoneArmor boneArmor in this.appearance.customArmor.boneArmors)
			{
				if (boneArmor != null)
				{
					for (int num8 = 0; num8 < boneArmor.voxels.Length - 1; num8 += 2)
					{
						if (boneArmor.voxels[num8] > 0)
						{
							int num9 = (int)boneArmor.voxels[num8 + 1];
							int num10 = boneArmor.matIds[num9];
							array[num10] += array3[num10];
							if (!boneArmor.hide)
							{
								array2[num10] += array3[num10];
							}
						}
					}
				}
			}
			for (int num11 = 0; num11 < 8; num11++)
			{
				int add_or_sub = (int)(array[num11] + 0.5) - (int)(array2[num11] + 0.5);
				this.diyItems.Alter(array4[num11], add_or_sub);
			}
		}
		if (r.ReadInt32() != 2119973658)
		{
			throw new Exception("Corrupted Mecha Data");
		}
	}

	// Token: 0x17000209 RID: 521
	// (get) Token: 0x0600122C RID: 4652 RVA: 0x00152440 File Offset: 0x00150640
	public double reactorPowerGenEnhanced
	{
		get
		{
			double num = 1.0;
			ItemProto itemProto = (this.reactorItemId > 0) ? LDB.items.Select(this.reactorItemId) : null;
			if (itemProto != null)
			{
				num = (double)(itemProto.ReactorInc + 1f);
				if (this.reactorItemInc > 0)
				{
					if (itemProto.Productive)
					{
						num *= 1.0 + Cargo.incTableMilli[this.reactorItemInc];
					}
					else
					{
						num *= 1.0 + Cargo.accTableMilli[this.reactorItemInc];
					}
				}
			}
			return this.reactorPowerGen * num;
		}
	}

	// Token: 0x1700020A RID: 522
	// (get) Token: 0x0600122D RID: 4653 RVA: 0x001524D4 File Offset: 0x001506D4
	public double reactorPowerForWeaponEnhanced
	{
		get
		{
			ItemProto itemProto = (this.reactorItemId > 0) ? LDB.items.Select(this.reactorItemId) : null;
			double num;
			if (itemProto != null)
			{
				num = (double)(itemProto.ReactorInc + 1f);
				if (this.reactorItemInc > 0)
				{
					if (itemProto.Productive)
					{
						num *= 1.0 + Cargo.incTableMilli[this.reactorItemInc];
					}
					else
					{
						num *= 1.0 + Cargo.accTableMilli[this.reactorItemInc];
					}
				}
			}
			else
			{
				num = 0.5;
			}
			return this.reactorPowerGen * num;
		}
	}

	// Token: 0x1700020B RID: 523
	// (get) Token: 0x0600122E RID: 4654 RVA: 0x00152574 File Offset: 0x00150774
	public double reactorPowerGenRatio
	{
		get
		{
			double num = 1.0;
			ItemProto itemProto = (this.reactorItemId > 0) ? LDB.items.Select(this.reactorItemId) : null;
			if (itemProto != null)
			{
				num = (double)(itemProto.ReactorInc + 1f);
				if (this.reactorItemInc > 0)
				{
					if (itemProto.Productive)
					{
						num *= 1.0 + Cargo.incTableMilli[this.reactorItemInc];
					}
					else
					{
						num *= 1.0 + Cargo.accTableMilli[this.reactorItemInc];
					}
				}
			}
			return num;
		}
	}

	// Token: 0x1700020C RID: 524
	// (get) Token: 0x0600122F RID: 4655 RVA: 0x001525FF File Offset: 0x001507FF
	public double reactorPowerConsRatio
	{
		get
		{
			return this.energyConsumptionCoef;
		}
	}

	// Token: 0x06001230 RID: 4656 RVA: 0x00152607 File Offset: 0x00150807
	public float UseEnergy(double energyUse)
	{
		if (this.coreEnergy >= energyUse)
		{
			this.coreEnergy -= energyUse;
			return 1f;
		}
		float result = (float)(this.coreEnergy / energyUse);
		this.coreEnergy = 0.0;
		return result;
	}

	// Token: 0x06001231 RID: 4657 RVA: 0x0015263E File Offset: 0x0015083E
	public void QueryEnergy(double energyWant, out double energyGet, out float ratio)
	{
		if (this.coreEnergy >= energyWant)
		{
			energyGet = energyWant;
			ratio = 1f;
			return;
		}
		energyGet = this.coreEnergy;
		ratio = (float)(energyGet / energyWant);
	}

	// Token: 0x06001232 RID: 4658 RVA: 0x00152664 File Offset: 0x00150864
	public bool UseWarper()
	{
		int num = 1210;
		int num2 = 1;
		int num3;
		this.warpStorage.TakeTailItems(ref num, ref num2, out num3, false);
		if (num == 1210 && num2 == 1)
		{
			this.AddConsumptionStat(num, num2, this.player.nearestFactory);
			return true;
		}
		return false;
	}

	// Token: 0x06001233 RID: 4659 RVA: 0x001526B0 File Offset: 0x001508B0
	public bool HasWarper()
	{
		return this.warpStorage != null && this.warpStorage.grids.Length != 0 && this.warpStorage.grids[0].itemId == 1210 && this.warpStorage.grids[0].count > 0;
	}

	// Token: 0x06001234 RID: 4660 RVA: 0x0015270C File Offset: 0x0015090C
	public bool AutoReplenishWarper()
	{
		int inc;
		int num = this.player.package.TakeItem(1210, 20, out inc);
		if (num > 0)
		{
			int inc2;
			int num2 = this.warpStorage.AddItem(1210, num, 0, 1, inc, out inc2);
			int num3 = num - num2;
			if (num3 > 0)
			{
				this.player.TryAddItemToPackage(1210, num3, inc2, true, 0, false);
			}
			return true;
		}
		return false;
	}

	// Token: 0x06001235 RID: 4661 RVA: 0x00152774 File Offset: 0x00150974
	public bool AutoReplenishFuel(int itemId, int grid)
	{
		int inc;
		int num = this.player.package.TakeItem(itemId, StorageComponent.itemStackCount[itemId], out inc);
		if (num > 0)
		{
			int inc2;
			int num2 = this.reactorStorage.AddItem(itemId, num, grid, 1, inc, out inc2);
			int num3 = num - num2;
			if (num3 > 0)
			{
				this.player.TryAddItemToPackage(itemId, num3, inc2, true, 0, false);
			}
			return true;
		}
		return false;
	}

	// Token: 0x06001236 RID: 4662 RVA: 0x001527D4 File Offset: 0x001509D4
	public void AutoReplenishFuelAll()
	{
		int[] kFuelAutoReplenishIds = ItemProto.kFuelAutoReplenishIds;
		IDCNTINC[] array = new IDCNTINC[this.reactorStorage.size];
		int num = this.reactorStorage.size - 1;
		for (int i = kFuelAutoReplenishIds.Length - 1; i >= 0; i--)
		{
			int num2 = kFuelAutoReplenishIds[i];
			int inc;
			int num3 = this.player.package.TakeItem(num2, StorageComponent.itemStackCount[num2], out inc);
			if (num3 > 0)
			{
				array[num].id = num2;
				array[num].count = num3;
				array[num].inc = inc;
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].count > 0)
			{
				int inc2;
				int num4 = this.reactorStorage.AddItemStacked(array[j].id, array[j].count, array[j].inc, out inc2);
				int num5 = array[j].count - num4;
				if (num5 > 0)
				{
					this.player.TryAddItemToPackage(array[j].id, num5, inc2, true, 0, false);
				}
			}
		}
	}

	// Token: 0x06001237 RID: 4663 RVA: 0x00152908 File Offset: 0x00150B08
	public bool EnergyIsLow()
	{
		bool flag = GameMain.data.history.HasFeatureKey(1100000) && GameMain.sandboxToolsEnabled;
		return ((this.coreEnergyCap == 0.0) ? 0f : (flag ? 1f : ((float)(this.coreEnergy / this.coreEnergyCap)))) < 0.2f;
	}

	// Token: 0x06001238 RID: 4664 RVA: 0x0015296C File Offset: 0x00150B6C
	public bool EnergyNearEmpty()
	{
		bool flag = GameMain.data.history.HasFeatureKey(1100000) && GameMain.sandboxToolsEnabled;
		return ((this.coreEnergyCap == 0.0) ? 0f : (flag ? 1f : ((float)(this.coreEnergy / this.coreEnergyCap)))) < 0.01f;
	}

	// Token: 0x06001239 RID: 4665 RVA: 0x001529D0 File Offset: 0x00150BD0
	public bool CheckEjectConstructionDroneCondition()
	{
		return this.coreEnergy >= this.droneEjectEnergy && this.player.movementState < EMovementState.Sail && this.player.factory != null && (this.player.factory.planet.type == EPlanetType.Gas || this.player.movementState < EMovementState.Fly || this.player.controller.horzSpeed <= this.walkSpeed * 1.2f);
	}

	// Token: 0x1700020D RID: 525
	// (get) Token: 0x0600123A RID: 4666 RVA: 0x00152A53 File Offset: 0x00150C53
	public int hpMaxUpgradeApplied
	{
		get
		{
			return (int)((double)((float)this.hpMaxUpgrade * Mathf.Clamp01((float)this.hpMax / 150000f)) + 0.5);
		}
	}

	// Token: 0x1700020E RID: 526
	// (get) Token: 0x0600123B RID: 4667 RVA: 0x00152A7C File Offset: 0x00150C7C
	public int hpMaxApplied
	{
		get
		{
			double num = (double)(this.hpMax + this.hpMaxUpgradeApplied) * this.history.globalHPScale;
			if (num > 2100000000.0)
			{
				return 2100000000;
			}
			return (int)(num + 0.5);
		}
	}

	// Token: 0x1700020F RID: 527
	// (get) Token: 0x0600123C RID: 4668 RVA: 0x00152AC4 File Offset: 0x00150CC4
	public float energyShieldRadiusMultiplier
	{
		get
		{
			PlanetData planetData = this.player.planetData;
			if (planetData != null)
			{
				float num = Mathf.Clamp01((this.player.position.magnitude - planetData.realRadius - 50f) / 750f);
				return 1f + num * num * (3f - 2f * num) * 9f;
			}
			return 10f;
		}
	}

	// Token: 0x17000210 RID: 528
	// (get) Token: 0x0600123D RID: 4669 RVA: 0x00152B2F File Offset: 0x00150D2F
	public double energyShieldPercentage
	{
		get
		{
			if (this.energyShieldCapacity != 0L)
			{
				return (double)this.energyShieldEnergy / (double)this.energyShieldCapacity;
			}
			return 0.0;
		}
	}

	// Token: 0x17000211 RID: 529
	// (get) Token: 0x0600123E RID: 4670 RVA: 0x00152B52 File Offset: 0x00150D52
	public bool energyShieldBurstReady
	{
		get
		{
			return this.energyShieldPercentage >= 0.2 && this.energyShieldBurstProgress >= 0.2;
		}
	}

	// Token: 0x17000212 RID: 530
	// (get) Token: 0x0600123F RID: 4671 RVA: 0x00152B7C File Offset: 0x00150D7C
	public int energyShieldBurstCurrentDamage
	{
		get
		{
			long num = (long)(this.energyShieldBurstProgress * (double)this.energyShieldCapacity + 0.5);
			if (num > this.energyShieldEnergy)
			{
				num = this.energyShieldEnergy;
			}
			long num2 = (long)((float)(num / this.energyShieldBurstDamageRate) * this.history.energyDamageScale);
			if (num2 > 2100000000L)
			{
				num2 = 2100000000L;
			}
			return (int)num2;
		}
	}

	// Token: 0x17000213 RID: 531
	// (get) Token: 0x06001240 RID: 4672 RVA: 0x00152BDC File Offset: 0x00150DDC
	public Vector3 skillTargetLCenter
	{
		get
		{
			return this.player.animator.skillTargetBone.position;
		}
	}

	// Token: 0x17000214 RID: 532
	// (get) Token: 0x06001241 RID: 4673 RVA: 0x00152BF4 File Offset: 0x00150DF4
	public VectorLF3 skillTargetUCenter
	{
		get
		{
			Vector3 position = this.player.animator.skillTargetBone.position;
			Vector3 vec = this.player.controller.model.transform.InverseTransformPoint(position);
			return this.player.uPosition + Maths.QRotateLF(this.player.uRotation, vec);
		}
	}

	// Token: 0x17000215 RID: 533
	// (get) Token: 0x06001242 RID: 4674 RVA: 0x00152C59 File Offset: 0x00150E59
	public Vector3 skillCastLeftL
	{
		get
		{
			return this.player.animator.skillLeftTurretBone.position;
		}
	}

	// Token: 0x17000216 RID: 534
	// (get) Token: 0x06001243 RID: 4675 RVA: 0x00152C70 File Offset: 0x00150E70
	public VectorLF3 skillCastLeftU
	{
		get
		{
			Vector3 position = this.player.animator.skillLeftTurretBone.position;
			Vector3 vec = this.player.controller.model.transform.InverseTransformPoint(position);
			return this.player.uPosition + Maths.QRotateLF(this.player.uRotation, vec);
		}
	}

	// Token: 0x17000217 RID: 535
	// (get) Token: 0x06001244 RID: 4676 RVA: 0x00152CD5 File Offset: 0x00150ED5
	public Vector3 skillCastRightL
	{
		get
		{
			return this.player.animator.skillRightTurretBone.position;
		}
	}

	// Token: 0x17000218 RID: 536
	// (get) Token: 0x06001245 RID: 4677 RVA: 0x00152CEC File Offset: 0x00150EEC
	public VectorLF3 skillCastRightU
	{
		get
		{
			Vector3 position = this.player.animator.skillRightTurretBone.position;
			Vector3 vec = this.player.controller.model.transform.InverseTransformPoint(position);
			return this.player.uPosition + Maths.QRotateLF(this.player.uRotation, vec);
		}
	}

	// Token: 0x17000219 RID: 537
	// (get) Token: 0x06001246 RID: 4678 RVA: 0x00152D51 File Offset: 0x00150F51
	public VectorLF3 skillBombingLCenter
	{
		get
		{
			return this.player.animator.skillBombingBone.position;
		}
	}

	// Token: 0x1700021A RID: 538
	// (get) Token: 0x06001247 RID: 4679 RVA: 0x00152D70 File Offset: 0x00150F70
	public VectorLF3 skillBombingUCenter
	{
		get
		{
			Vector3 position = this.player.animator.skillBombingBone.position;
			Vector3 vec = this.player.controller.model.transform.InverseTransformPoint(position);
			return this.player.uPosition + Maths.QRotateLF(this.player.uRotation, vec);
		}
	}

	// Token: 0x1700021B RID: 539
	// (get) Token: 0x06001248 RID: 4680 RVA: 0x00152DD5 File Offset: 0x00150FD5
	public VectorLF3 skillShieldBurstLCenter
	{
		get
		{
			return this.player.animator.skillShieldBurstBone.position;
		}
	}

	// Token: 0x1700021C RID: 540
	// (get) Token: 0x06001249 RID: 4681 RVA: 0x00152DF4 File Offset: 0x00150FF4
	public VectorLF3 skillShieldBurstUCenter
	{
		get
		{
			Vector3 position = this.player.animator.skillShieldBurstBone.position;
			Vector3 vec = this.player.controller.model.transform.InverseTransformPoint(position);
			return this.player.uPosition + Maths.QRotateLF(this.player.uRotation, vec);
		}
	}

	// Token: 0x0600124A RID: 4682 RVA: 0x00152E5C File Offset: 0x0015105C
	public void UpdateSkillColliders()
	{
		Vector3 position = this.player.animator.skillTargetCapsuleA.position;
		Vector3 position2 = this.player.animator.skillTargetCapsuleB.position;
		Vector3 vector = (position + position2) * 0.5f;
		Vector3 vector2 = position - vector;
		Transform transform = this.player.controller.model.transform;
		this.skillColliderL.idType = 503316481;
		this.skillColliderL.pos = vector;
		this.skillColliderL.ext = vector2;
		this.skillColliderL.radius = 0.85f;
		this.skillColliderL.q = Quaternion.identity;
		this.skillColliderL.link = 0;
		this.skillColliderU.idType = 503316481;
		this.skillColliderU.astroId = 0;
		this.skillColliderU.pos = this.player.uPosition + Maths.QRotateLF(this.player.uRotation, transform.InverseTransformPoint(vector));
		this.skillColliderU.ext = Maths.QRotateLF(this.player.uRotation, transform.InverseTransformVector(vector2));
		this.skillColliderU.radius = 0.85f;
		this.skillColliderU.q = Quaternion.identity;
		this.skillColliderU.link = 0;
	}

	// Token: 0x0600124B RID: 4683 RVA: 0x00152FC4 File Offset: 0x001511C4
	public void TakeDamage(int damage)
	{
		if (!this.player.invincible)
		{
			this.hpRecoverCD = 360;
			this.hp -= damage;
		}
		if (this.hp <= 0)
		{
			this.hp = 0;
			this.hpRecoverCD = 0;
			this.energyShieldRecoverCD = 0;
			this.player.Kill();
		}
	}

	// Token: 0x0600124C RID: 4684 RVA: 0x00153020 File Offset: 0x00151220
	public bool EnergyShieldResist(ref int damage)
	{
		bool result = false;
		lock (this)
		{
			if (this.player.invincible)
			{
				damage = 0;
			}
			if (this.energyShieldEnergy > 0L)
			{
				long num = this.energyShieldEnergy / this.energyShieldEnergyRate;
				num = ((num < (long)damage) ? num : ((long)damage));
				this.energyShieldEnergy -= num * this.energyShieldEnergyRate;
				this.MarkShieldResist(damage);
				damage -= (int)num;
				if (this.energyShieldEnergy < this.energyShieldEnergyRate)
				{
					this.energyShieldEnergy = 0L;
					if (this.energyShieldRecoverCD < 150)
					{
						this.energyShieldRecoverCD = 150;
					}
				}
				this.gameData.spaceSector.skillSystem.audio.AddPlayerAudio(405, 0.7f, this.skillTargetLCenter, 1);
			}
			result = (this.energyShieldEnergy > 0L);
		}
		return result;
	}

	// Token: 0x0600124D RID: 4685 RVA: 0x0015311C File Offset: 0x0015131C
	public bool EnergyShieldResist(ref int damage, out int resist)
	{
		bool result = false;
		resist = 0;
		lock (this)
		{
			if (this.energyShieldEnergy > 0L && !this.player.invincible)
			{
				long num = this.energyShieldEnergy / this.energyShieldEnergyRate;
				resist = (int)((num < (long)damage) ? num : ((long)damage));
				this.energyShieldEnergy -= (long)resist * this.energyShieldEnergyRate;
				this.MarkShieldResist(damage);
				damage -= resist;
				if (this.energyShieldEnergy < this.energyShieldEnergyRate)
				{
					this.energyShieldEnergy = 0L;
					if (this.energyShieldRecoverCD < 150)
					{
						this.energyShieldRecoverCD = 150;
					}
				}
			}
			result = (this.energyShieldEnergy > 0L);
		}
		return result;
	}

	// Token: 0x0600124E RID: 4686 RVA: 0x001531EC File Offset: 0x001513EC
	public void ChargeShieldBurst()
	{
		if (!this.energyShieldBurstUnlocked)
		{
			this.energyShieldBurstProgress = 0.0;
			return;
		}
		this.energyShieldBurstProgress += 0.004166666883975267 * Math.Pow(1.5 - this.energyShieldBurstProgress, 1.5);
		if (this.energyShieldBurstProgress > this.energyShieldPercentage)
		{
			this.energyShieldBurstProgress = this.energyShieldPercentage;
		}
		if (this.energyShieldBurstProgress > 1.0)
		{
			this.energyShieldBurstProgress = 1.0;
			return;
		}
		if (this.energyShieldBurstProgress < 0.0)
		{
			this.energyShieldBurstProgress = 0.0;
		}
	}

	// Token: 0x0600124F RID: 4687 RVA: 0x001532A4 File Offset: 0x001514A4
	public void ShieldBurstComsumeEnergy()
	{
		long num = (long)(this.energyShieldBurstProgress * (double)this.energyShieldCapacity + 0.5);
		this.energyShieldEnergy -= num;
		if (this.energyShieldEnergy < 0L)
		{
			this.energyShieldEnergy = 0L;
		}
	}

	// Token: 0x06001250 RID: 4688 RVA: 0x001532EB File Offset: 0x001514EB
	public void ResetShieldBurstProgress()
	{
		this.energyShieldBurstProgress = 0.0;
	}

	// Token: 0x06001251 RID: 4689 RVA: 0x001532FC File Offset: 0x001514FC
	public void UpdateCombatStats(double dt)
	{
		this.energyShieldRechargeCurrent = 0L;
		if (this.hp <= 0)
		{
			this.hp = 0;
			this.hpRecoverCD = 0;
			this.energyShieldRecoverCD = 0;
			this.energyShieldEnergy = 0L;
			this.energyShieldBurstProgress = 0.0;
			this.player.Kill();
			return;
		}
		if (this.hpRecoverCD <= 0)
		{
			this.hpRecoverCD = 0;
			this.hp += this.hpRecover;
		}
		else
		{
			this.hpRecoverCD--;
		}
		double num = this.coreEnergy / this.coreEnergyCap;
		if (this.energyShieldRecoverCD <= 0)
		{
			this.energyShieldRecoverCD = 0;
			if (this.energyShieldCapacity > 0L && this.energyShieldRechargeEnabled)
			{
				double num2 = this.reactorPowerForWeaponEnhanced * dt * num * (double)this.energyShieldRechargeSpeed;
				double num3 = num2;
				if (num3 > (double)(this.energyShieldCapacity - this.energyShieldEnergy))
				{
					num3 = (double)(this.energyShieldCapacity - this.energyShieldEnergy);
				}
				if (num3 > this.coreEnergy)
				{
					num3 = this.coreEnergy;
				}
				num3 = (double)((long)(num3 + 0.01));
				if (num3 > 0.0)
				{
					this.coreEnergy -= num3;
					this.energyShieldEnergy += (long)(num3 + 0.01);
					this.MarkEnergyChange(10, -num3);
				}
				this.energyShieldRechargeCurrent = (long)(num2 + 0.01);
				if ((double)this.energyShieldRechargeCurrent > this.coreEnergy)
				{
					this.energyShieldRechargeCurrent = (long)(this.coreEnergy + 0.01);
				}
				this.energyShieldRechargeCurrent *= 60L;
			}
		}
		else
		{
			this.energyShieldRecoverCD--;
		}
		this.ScrollShieldResistHistory();
		if (this.hp > this.hpMaxApplied)
		{
			this.hp = this.hpMaxApplied;
		}
		if (this.laserActive)
		{
			double num4 = num * 0.5 + 0.7;
			if (num4 > 1.0)
			{
				num4 = 1.0;
			}
			double num5 = this.reactorPowerForWeaponEnhanced * dt * num4 * 1.8;
			if (num5 > (double)(this.laserEnergyCapacity - this.laserEnergy))
			{
				num5 = (double)(this.laserEnergyCapacity - this.laserEnergy);
			}
			if (num5 > this.coreEnergy)
			{
				num5 = this.coreEnergy;
			}
			num5 = (double)((long)(num5 + 0.01));
			if (num5 > 0.0)
			{
				this.coreEnergy -= num5;
				this.laserEnergy += (long)(num5 + 0.01);
				this.MarkEnergyChange(11, -num5);
			}
		}
		if (this.laserEnergy == this.laserEnergyCapacity)
		{
			this.laserRecharging = false;
		}
		if (this.autoReplenishHangar)
		{
			this.AutoReplenishHangar();
		}
		if (this.groundCombatModule.autoReplenishFleet)
		{
			this.groundCombatModule.AutoReplenishAllFleets(this.fighterStorage);
		}
		if (this.spaceCombatModule.autoReplenishFleet)
		{
			this.spaceCombatModule.AutoReplenishAllFleets(this.fighterStorage);
		}
	}

	// Token: 0x06001252 RID: 4690 RVA: 0x001535F8 File Offset: 0x001517F8
	public void Respawn()
	{
		this.coreEnergy = this.coreEnergyCap;
		this.hp = this.hpMaxApplied;
		this.hpRecoverCD = 0;
		this.energyShieldRecoverCD = 0;
		this.energyShieldEnergy = 0L;
		this.energyShieldBurstProgress = 0.0;
		this.ammoMuzzleFire = (this.ammoRoundFire = (this.ammoMuzzleIndex = 0));
		this.laserRecharging = false;
		this.laserEnergy = this.laserEnergyCapacity;
		this.laserFire = 0;
		this.bombFire = 0;
		this.ammoHatredTarget.SetNull();
		this.laserHatredTarget.SetNull();
		PlanetData localPlanet = GameMain.localPlanet;
		PlanetFactory planetFactory = (localPlanet != null) ? localPlanet.factory : null;
		if (this.player.movementState < EMovementState.Sail && planetFactory != null)
		{
			this.groundCombatModule.CancelAllFleetsOrder();
		}
		this.spaceCombatModule.CancelAllFleetsOrder();
	}

	// Token: 0x06001253 RID: 4691 RVA: 0x001536CC File Offset: 0x001518CC
	public void PrepareRedeploy()
	{
		this.reactorEnergy = 0.0;
		this.reactorItemId = 0;
		this.reactorItemInc = 0;
		this.ammoItemId = 0;
		this.ammoInc = 0;
		this.ammoBulletCount = 0;
		this.ammoMuzzleFire = (this.ammoRoundFire = (this.ammoMuzzleIndex = 0));
		this.laserRecharging = false;
		this.laserFire = 0;
		this.bombFire = 0;
		this.ammoHatredTarget.SetNull();
		this.laserHatredTarget.SetNull();
		this.forge.CancelAllTasks();
		this.lab.ManageTakeback();
	}

	// Token: 0x06001254 RID: 4692 RVA: 0x00153768 File Offset: 0x00151968
	public void Kill()
	{
		this.coreEnergy = 0.0;
		this.hp = 0;
		this.hpRecoverCD = 0;
		this.energyShieldEnergy = 0L;
		this.energyShieldRecoverCD = 0;
		this.energyShieldBurstProgress = 0.0;
		this.laserEnergy = 0L;
		this.ammoHatredTarget.SetNull();
		this.laserHatredTarget.SetNull();
		if (this.player.factory != null)
		{
			this.constructionModule.RecycleAllDrones(this.player.factory);
		}
		this.ClearShieldResistHistory();
	}

	// Token: 0x1700021D RID: 541
	// (get) Token: 0x06001255 RID: 4693 RVA: 0x001537F8 File Offset: 0x001519F8
	public EAmmoType activeAmmoType
	{
		get
		{
			if (this.ammoItemId > 0)
			{
				ItemProto itemProto = LDB.items.Select(this.ammoItemId);
				if (itemProto == null)
				{
					return EAmmoType.None;
				}
				return itemProto.AmmoType;
			}
			else
			{
				int num = this.ammoSelectSlot - 1;
				if (num < 0)
				{
					return EAmmoType.None;
				}
				int itemId = this.ammoStorage.grids[num % this.ammoStorage.size].itemId;
				ItemProto itemProto2 = (itemId > 0) ? LDB.items.Select(itemId) : null;
				if (itemProto2 == null)
				{
					return EAmmoType.None;
				}
				return itemProto2.AmmoType;
			}
		}
	}

	// Token: 0x1700021E RID: 542
	// (get) Token: 0x06001256 RID: 4694 RVA: 0x0015387C File Offset: 0x00151A7C
	public ItemProto activeAmmoProto
	{
		get
		{
			if (this.ammoItemId > 0)
			{
				return LDB.items.Select(this.ammoItemId);
			}
			int num = this.ammoSelectSlot - 1;
			if (num >= 0)
			{
				int itemId = this.ammoStorage.grids[num % this.ammoStorage.size].itemId;
				if (itemId > 0)
				{
					return LDB.items.Select(itemId);
				}
			}
			return null;
		}
	}

	// Token: 0x1700021F RID: 543
	// (get) Token: 0x06001257 RID: 4695 RVA: 0x001538E4 File Offset: 0x00151AE4
	public int selectedAmmoItemId
	{
		get
		{
			int num = this.ammoSelectSlot - 1;
			if (num >= 0)
			{
				return this.ammoStorage.grids[num % this.ammoStorage.size].itemId;
			}
			return 0;
		}
	}

	// Token: 0x17000220 RID: 544
	// (get) Token: 0x06001258 RID: 4696 RVA: 0x00153924 File Offset: 0x00151B24
	public EAmmoType selectedAmmoType
	{
		get
		{
			int num = this.ammoSelectSlot - 1;
			if (num < 0)
			{
				return EAmmoType.None;
			}
			int itemId = this.ammoStorage.grids[num % this.ammoStorage.size].itemId;
			ItemProto itemProto = LDB.items.Select(itemId);
			if (itemProto == null)
			{
				return EAmmoType.None;
			}
			return itemProto.AmmoType;
		}
	}

	// Token: 0x17000221 RID: 545
	// (get) Token: 0x06001259 RID: 4697 RVA: 0x0015397C File Offset: 0x00151B7C
	public bool ammoChangeRequired
	{
		get
		{
			if (this.ammoItemId > 0 && this.ammoBulletCount > 0)
			{
				int num = this.ammoSelectSlot - 1;
				if (num >= 0)
				{
					int itemId = this.ammoStorage.grids[num % this.ammoStorage.size].itemId;
					return itemId > 0 && itemId != this.ammoItemId;
				}
			}
			return false;
		}
	}

	// Token: 0x17000222 RID: 546
	// (get) Token: 0x0600125A RID: 4698 RVA: 0x001539E0 File Offset: 0x00151BE0
	public float ammoLocalAttackRange
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletLocalAttackRange;
			case EAmmoType.Cannon:
				return this.cannonLocalAttackRange;
			case EAmmoType.Plasma:
				return this.plasmaLocalAttackRange;
			case EAmmoType.Missile:
				return this.missileLocalAttackRange;
			}
			return 0f;
		}
	}

	// Token: 0x17000223 RID: 547
	// (get) Token: 0x0600125B RID: 4699 RVA: 0x00153A34 File Offset: 0x00151C34
	public float ammoSpaceAttackRange
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletSpaceAttackRange;
			case EAmmoType.Cannon:
				return this.cannonSpaceAttackRange;
			case EAmmoType.Plasma:
				return this.plasmaSpaceAttackRange;
			case EAmmoType.Missile:
				return this.missileSpaceAttackRange;
			}
			return 0f;
		}
	}

	// Token: 0x17000224 RID: 548
	// (get) Token: 0x0600125C RID: 4700 RVA: 0x00153A88 File Offset: 0x00151C88
	public int ammoEnergyCost
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletEnergyCost;
			case EAmmoType.Cannon:
				return this.cannonEnergyCost;
			case EAmmoType.Plasma:
				return this.plasmaEnergyCost;
			case EAmmoType.Missile:
				return this.missileEnergyCost;
			}
			return 0;
		}
	}

	// Token: 0x17000225 RID: 549
	// (get) Token: 0x0600125D RID: 4701 RVA: 0x00153AD8 File Offset: 0x00151CD8
	public int ammoDamage
	{
		get
		{
			ItemProto activeAmmoProto = this.activeAmmoProto;
			if (activeAmmoProto == null)
			{
				return 0;
			}
			int ability = activeAmmoProto.Ability;
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return (int)((float)ability * this.bulletDamageScale * this.history.kineticDamageScale + 0.5f);
			case EAmmoType.Cannon:
				return (int)((float)ability * this.cannonDamageScale * (this.history.kineticDamageScale + this.history.blastDamageScale) * 0.5f + 0.5f);
			case EAmmoType.Plasma:
				return (int)((float)ability * this.plasmaDamageScale * this.history.energyDamageScale + 0.5f);
			case EAmmoType.Missile:
				return (int)((float)ability * this.missileDamageScale * this.history.blastDamageScale + 0.5f);
			}
			return 0;
		}
	}

	// Token: 0x17000226 RID: 550
	// (get) Token: 0x0600125E RID: 4702 RVA: 0x00153BA7 File Offset: 0x00151DA7
	public int laserLocalActualDamage
	{
		get
		{
			return (int)((float)this.laserLocalDamage * this.history.energyDamageScale + 0.5f);
		}
	}

	// Token: 0x17000227 RID: 551
	// (get) Token: 0x0600125F RID: 4703 RVA: 0x00153BC3 File Offset: 0x00151DC3
	public int laserSpaceActualDamage
	{
		get
		{
			return (int)((float)this.laserSpaceDamage * this.history.energyDamageScale + 0.5f);
		}
	}

	// Token: 0x17000228 RID: 552
	// (get) Token: 0x06001260 RID: 4704 RVA: 0x00153BE0 File Offset: 0x00151DE0
	public int ammoROF
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletROF;
			case EAmmoType.Cannon:
				return this.cannonROF;
			case EAmmoType.Plasma:
				return this.plasmaROF;
			case EAmmoType.Missile:
				return this.missileROF;
			}
			return 0;
		}
	}

	// Token: 0x17000229 RID: 553
	// (get) Token: 0x06001261 RID: 4705 RVA: 0x00153C30 File Offset: 0x00151E30
	public int ammoMuzzleCount
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletMuzzleCount;
			case EAmmoType.Cannon:
				return this.cannonMuzzleCount;
			case EAmmoType.Plasma:
				return this.plasmaMuzzleCount;
			case EAmmoType.Missile:
				return this.missileMuzzleCount;
			}
			return 0;
		}
	}

	// Token: 0x1700022A RID: 554
	// (get) Token: 0x06001262 RID: 4706 RVA: 0x00153C80 File Offset: 0x00151E80
	public int ammoMuzzleInterval
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletMuzzleInterval;
			case EAmmoType.Cannon:
				return this.cannonMuzzleInterval;
			case EAmmoType.Plasma:
				return this.plasmaMuzzleInterval;
			case EAmmoType.Missile:
				return this.missileMuzzleInterval;
			}
			return 0;
		}
	}

	// Token: 0x1700022B RID: 555
	// (get) Token: 0x06001263 RID: 4707 RVA: 0x00153CD0 File Offset: 0x00151ED0
	public int ammoRoundInterval
	{
		get
		{
			switch (this.activeAmmoType)
			{
			case EAmmoType.Bullet:
				return this.bulletRoundInterval;
			case EAmmoType.Cannon:
				return this.cannonRoundInterval;
			case EAmmoType.Plasma:
				return this.plasmaRoundInterval;
			case EAmmoType.Missile:
				return this.missileRoundInterval;
			}
			return 0;
		}
	}

	// Token: 0x06001264 RID: 4708 RVA: 0x00153D20 File Offset: 0x00151F20
	public void TickAmmoFireCondition(Mecha.DFireFunc fireCallback)
	{
		int num;
		double num2;
		this.GenAmmoTurretROF(out num, out num2);
		if (num <= 0)
		{
			return;
		}
		int i = num;
		int ammoMuzzleCount = this.ammoMuzzleCount;
		int ammoMuzzleInterval = this.ammoMuzzleInterval;
		int ammoRoundInterval = this.ammoRoundInterval;
		int num3 = 20;
		while (i > 0)
		{
			if (this.ammoRoundFire <= 0)
			{
				this.ammoMuzzleIndex = 0;
			}
			if (this.ammoMuzzleIndex < ammoMuzzleCount && this.ammoMuzzleFire == 0)
			{
				if (fireCallback().id <= 0)
				{
					int num4 = (this.ammoRoundFire < i) ? this.ammoRoundFire : i;
					this.ammoRoundFire -= num4;
					i -= num4;
					break;
				}
				this.ammoMuzzleFire = ammoMuzzleInterval;
				this.ammoRoundFire = ammoRoundInterval;
				this.ammoMuzzleIndex++;
			}
			int num5 = (this.ammoMuzzleIndex >= ammoMuzzleCount) ? this.ammoRoundFire : this.ammoMuzzleFire;
			int num6 = (num5 < i) ? num5 : i;
			this.ammoMuzzleFire -= num6;
			this.ammoRoundFire -= num6;
			i -= num6;
			if (this.ammoMuzzleFire < 0)
			{
				this.ammoMuzzleFire = 0;
			}
			num3--;
			if (num3 == 0)
			{
				Debug.LogWarning("TickAmmoFireCondition 达到最大循环次数");
				break;
			}
		}
		double num7 = (double)(num - i) / (double)num;
		num2 *= num7;
		this.coreEnergy -= num2;
		if (this.coreEnergy < 0.0)
		{
			this.coreEnergy = 0.0;
		}
		this.MarkEnergyChange(14, -num2);
	}

	// Token: 0x06001265 RID: 4709 RVA: 0x00153E94 File Offset: 0x00152094
	public void TickLaserFireCondition(Mecha.DFireFunc fireCallback)
	{
		if (this.laserFire > 0)
		{
			this.laserFire--;
		}
		int num = (this.player.factory == null) ? 0 : this.player.factory.planet.astroId;
		if (!this.laserRecharging && this.laserFire == 0)
		{
			SkillTarget skillTarget = fireCallback();
			if (skillTarget.id > 0)
			{
				if (skillTarget.astroId == 0 || skillTarget.astroId != num)
				{
					this.laserFire = this.laserSpaceInterval;
					this.laserEnergy -= (long)this.laserSpaceEnergyCost;
					return;
				}
				this.laserFire = this.laserLocalInterval;
				this.laserEnergy -= (long)this.laserLocalEnergyCost;
			}
		}
	}

	// Token: 0x06001266 RID: 4710 RVA: 0x00153F50 File Offset: 0x00152150
	public void TickBombFireCondition()
	{
		if (this.bombFire > 0)
		{
			this.bombFire--;
		}
	}

	// Token: 0x06001267 RID: 4711 RVA: 0x00153F6C File Offset: 0x0015216C
	private void GenAmmoTurretROF(out int rof, out double energy_take)
	{
		int ammoEnergyCost = this.ammoEnergyCost;
		if (this.coreEnergy < (double)ammoEnergyCost)
		{
			rof = (int)(this.coreEnergy / (double)ammoEnergyCost * (double)this.ammoROF + 0.5);
			energy_take = this.coreEnergy;
			return;
		}
		rof = this.ammoROF;
		energy_take = (double)ammoEnergyCost;
	}

	// Token: 0x06001268 RID: 4712 RVA: 0x00153FBD File Offset: 0x001521BD
	public bool TestLaserTurretEnergy(bool isLocal)
	{
		if (isLocal)
		{
			if (this.laserEnergy < (long)this.laserLocalEnergyCost)
			{
				this.laserRecharging = true;
				return false;
			}
		}
		else if (this.laserEnergy < (long)this.laserSpaceEnergyCost)
		{
			this.laserRecharging = true;
			return false;
		}
		return true;
	}

	// Token: 0x06001269 RID: 4713 RVA: 0x00153FF4 File Offset: 0x001521F4
	public void LoadAmmo(bool findAnother = true)
	{
		if (this.ammoSelectSlot <= 0)
		{
			return;
		}
		StorageComponent.GRID[] grids = this.ammoStorage.grids;
		int num = this.ammoSelectSlot - 1;
		int size = this.ammoStorage.size;
		ItemProto itemProto = (this.ammoItemId == 0) ? null : LDB.items.Select(this.ammoItemId);
		if (grids[num].count == 0)
		{
			if (this.autoReplenishAmmo)
			{
				this.AutoReplenishAmmo();
			}
			int num2 = (grids[num].count > 0) ? num : -1;
			if (itemProto != null && findAnother)
			{
				if (num2 < 0)
				{
					for (int i = num; i < num + size; i++)
					{
						int num3 = i % size;
						if (grids[num3].itemId == this.ammoItemId && grids[num3].count > 0)
						{
							num2 = num3;
							break;
						}
					}
				}
				if (num2 < 0)
				{
					for (int j = num; j < num + size; j++)
					{
						int num4 = j % size;
						ItemProto itemProto2 = (grids[num4].itemId == 0) ? null : LDB.items.Select(grids[num4].itemId);
						if (itemProto2 != null && itemProto2.AmmoType == itemProto.AmmoType && grids[num4].count > 0)
						{
							num2 = num4;
							break;
						}
					}
				}
			}
			if (findAnother && num2 < 0)
			{
				for (int k = num; k < num + size; k++)
				{
					int num5 = k % size;
					if (grids[num5].itemId > 0 && grids[num5].count > 0)
					{
						num2 = num5;
						break;
					}
				}
			}
			if (num2 >= 0)
			{
				this.ammoSelectSlot = num2 + 1;
				num = num2;
			}
		}
		if (grids[num].count == 0)
		{
			return;
		}
		int num6 = 0;
		int num7 = 1;
		int num8;
		this.ammoStorage.TakeItemFromGrid(num, ref num6, ref num7, out num8);
		if (num7 > 0)
		{
			this.AddConsumptionStat(num6, num7, this.player.nearestFactory);
			ItemProto itemProto3 = LDB.items.Select(num6);
			if (itemProto3 != null)
			{
				this.ammoItemId = num6;
				this.ammoInc = ((num8 > 10) ? 10 : num8);
				int num9 = itemProto3.HpMax;
				int num10 = (int)((double)num9 * ((double)Cargo.incTable[num8] * 0.001) + ((num9 < 12) ? 0.51 : 0.1));
				this.ammoBulletCount = num9 + num10;
				if (grids[num].count == 0 && this.autoReplenishAmmo)
				{
					this.AutoReplenishAmmo();
				}
			}
		}
	}

	// Token: 0x0600126A RID: 4714 RVA: 0x00154270 File Offset: 0x00152470
	public void AutoReplenishAmmo()
	{
		for (int i = 0; i < this.ammoStorage.size; i++)
		{
			ref StorageComponent.GRID ptr = ref this.ammoStorage.grids[i];
			int itemId = ptr.itemId;
			if (ptr.count == 0 && ptr.filter > 0)
			{
				int count = (ptr.itemId > 0) ? ptr.stackSize : StorageComponent.itemStackCount[itemId];
				int inc;
				int num = this.player.package.TakeItem(itemId, count, out inc);
				if (num > 0)
				{
					int num2;
					this.ammoStorage.AddItem(itemId, num, i, 1, inc, out num2);
				}
				else
				{
					ItemProto itemProto = LDB.items.Select(itemId);
					if (itemProto != null)
					{
						EAmmoType ammoType = itemProto.AmmoType;
						if (ammoType != EAmmoType.None && ammoType != EAmmoType.Laser)
						{
							ItemProto[] dataArray = LDB.items.dataArray;
							for (int j = dataArray.Length - 1; j >= 0; j--)
							{
								int id = dataArray[j].ID;
								if (dataArray[j].AmmoType == ammoType && id != itemId)
								{
									num = this.player.package.TakeItem(id, StorageComponent.itemStackCount[id], out inc);
									if (num > 0)
									{
										ptr.itemId = id;
										ptr.filter = id;
										ptr.stackSize = StorageComponent.itemStackCount[id];
										ptr.count = num;
										ptr.inc = inc;
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

	// Token: 0x0600126B RID: 4715 RVA: 0x001543D8 File Offset: 0x001525D8
	public void AutoReplenishBomb()
	{
		if (this.bombStorage == null || this.bombStorage.size == 0)
		{
			return;
		}
		ref StorageComponent.GRID ptr = ref this.bombStorage.grids[0];
		int itemId = ptr.itemId;
		if (ptr.count == 0 && ptr.filter > 0)
		{
			int count = (ptr.itemId > 0) ? ptr.stackSize : StorageComponent.itemStackCount[itemId];
			int inc;
			int num = this.player.package.TakeItem(itemId, count, out inc);
			if (num > 0)
			{
				int num2;
				this.bombStorage.AddItem(itemId, num, 0, 1, inc, out num2);
			}
		}
	}

	// Token: 0x0600126C RID: 4716 RVA: 0x00154470 File Offset: 0x00152670
	public void HateAmmoTarget(int targetAstroId, int targetId, int value, int maxValue, float changeTargetThreshold)
	{
		if (targetId == 0)
		{
			return;
		}
		if (this.ammoHatredTarget.astroId == targetAstroId && this.ammoHatredTarget.id == targetId)
		{
			this.ammoHatredTarget.HateTarget(value, maxValue, EHatredOperation.Add);
			return;
		}
		if ((float)value > (float)this.ammoHatredTarget.value * changeTargetThreshold)
		{
			this.ammoHatredTarget.HateTarget(targetAstroId, targetId, value, maxValue, EHatredOperation.Set);
		}
	}

	// Token: 0x0600126D RID: 4717 RVA: 0x001544D4 File Offset: 0x001526D4
	public void HateLaserTarget(int targetAstroId, int targetId, int value, int maxValue, float changeTargetThreshold)
	{
		if (targetId == 0)
		{
			return;
		}
		if (this.laserHatredTarget.astroId == targetAstroId && this.laserHatredTarget.id == targetId)
		{
			this.laserHatredTarget.HateTarget(value, maxValue, EHatredOperation.Add);
			return;
		}
		if ((float)value > (float)this.laserHatredTarget.value * changeTargetThreshold)
		{
			this.laserHatredTarget.HateTarget(targetAstroId, targetId, value, maxValue, EHatredOperation.Set);
		}
	}

	// Token: 0x0600126E RID: 4718 RVA: 0x00154535 File Offset: 0x00152735
	public void OnPlayerPackageAddItem(int itemId, int count, int inc)
	{
		if (this.autoReplenishAmmo && count > 0)
		{
			this.AutoReplenishAmmo();
			this.AutoReplenishBomb();
		}
	}

	// Token: 0x0600126F RID: 4719 RVA: 0x0015454F File Offset: 0x0015274F
	public void OnPlayerReplenishPreferred(int itemId, int count)
	{
		if (this.autoReplenishHangar && count > 0)
		{
			this.ReplenishHangar(itemId, count);
		}
	}

	// Token: 0x1700022C RID: 556
	// (get) Token: 0x06001270 RID: 4720 RVA: 0x00154565 File Offset: 0x00152765
	public bool bombCDReadied
	{
		get
		{
			return this.bombFire == 0;
		}
	}

	// Token: 0x06001271 RID: 4721 RVA: 0x00154570 File Offset: 0x00152770
	public void AutoReplenishHangar()
	{
		for (int i = 0; i < this.fighterStorage.size; i++)
		{
			ref StorageComponent.GRID ptr = ref this.fighterStorage.grids[i];
			int itemId = ptr.itemId;
			if (ptr.count == 0 && ptr.filter > 0)
			{
				int num = StorageComponent.itemStackCount[itemId];
				int inc;
				this.player.packageUtility.TryTakeItemFromAllPackages(ref itemId, ref num, out inc, false);
				int num2 = num;
				if (num2 > 0)
				{
					int num3;
					this.fighterStorage.AddItem(ptr.itemId, num2, i, 1, inc, out num3);
				}
			}
		}
	}

	// Token: 0x06001272 RID: 4722 RVA: 0x00154600 File Offset: 0x00152800
	public void ReplenishHangar(int itemId, int count)
	{
		if (count == 0 || itemId == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < this.fighterStorage.size; i++)
		{
			ref StorageComponent.GRID ptr = ref this.fighterStorage.grids[i];
			if (itemId == ptr.itemId || itemId == ptr.filter)
			{
				num += StorageComponent.itemStackCount[itemId] - ptr.count;
			}
		}
		if (count > num)
		{
			count = num;
		}
		if (count == 0)
		{
			return;
		}
		int number;
		this.player.packageUtility.TryTakeItemFromAllPackages(ref itemId, ref count, out number, false);
		int num2 = 0;
		while (num2 < this.fighterStorage.size && count > 0)
		{
			ref StorageComponent.GRID ptr2 = ref this.fighterStorage.grids[num2];
			if (itemId == ptr2.itemId || itemId == ptr2.filter)
			{
				int num3 = StorageComponent.itemStackCount[itemId] - ptr2.count;
				int num4 = (count > num3) ? num3 : count;
				int inc = this.split_inc(ref count, ref number, num4);
				if (num4 > 0)
				{
					int num5;
					this.fighterStorage.AddItem(ptr2.itemId, num4, num2, 1, inc, out num5);
				}
			}
			num2++;
		}
		Assert.Zero(count);
		Assert.Zero(number);
	}

	// Token: 0x06001273 RID: 4723 RVA: 0x00154724 File Offset: 0x00152924
	public bool CheckLaunchFleetCondition(bool isSpace)
	{
		if (!isSpace)
		{
			if (this.player.movementState >= EMovementState.Sail)
			{
				return false;
			}
			if (this.player.factory == null || !this.player.factory.planet.factoryLoaded)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06001274 RID: 4724 RVA: 0x00154760 File Offset: 0x00152960
	public void ClearEnergyChange()
	{
		Array.Clear(this.energyChanges, 0, 32);
	}

	// Token: 0x06001275 RID: 4725 RVA: 0x00154770 File Offset: 0x00152970
	public void MarkEnergyChange(int func, double change)
	{
		this.energyChanges[func] += change;
	}

	// Token: 0x1700022D RID: 557
	// (get) Token: 0x06001276 RID: 4726 RVA: 0x00154784 File Offset: 0x00152984
	public double totalEnergyChange
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < 32; i++)
			{
				num += this.energyChanges[i];
			}
			return num;
		}
	}

	// Token: 0x1700022E RID: 558
	// (get) Token: 0x06001277 RID: 4727 RVA: 0x001547B4 File Offset: 0x001529B4
	public double totalEnergyConsume
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < 32; i++)
			{
				if (this.energyChanges[i] < 0.0)
				{
					num += this.energyChanges[i];
				}
			}
			return -num;
		}
	}

	// Token: 0x06001278 RID: 4728 RVA: 0x001547F8 File Offset: 0x001529F8
	public void ClearChargerDevice()
	{
		Array.Clear(this.chargerDevice, 0, 256);
		this.chargerCount = 0;
	}

	// Token: 0x06001279 RID: 4729 RVA: 0x00154814 File Offset: 0x00152A14
	public void AddChargerDevice(int entityId)
	{
		if (this.chargerCount < 256)
		{
			int[] array = this.chargerDevice;
			int num = this.chargerCount;
			this.chargerCount = num + 1;
			array[num] = entityId;
		}
	}

	// Token: 0x1700022F RID: 559
	// (get) Token: 0x0600127A RID: 4730 RVA: 0x00154848 File Offset: 0x00152A48
	public long energyShieldResistCurrent
	{
		get
		{
			if (this.energyShieldResistHistory == null)
			{
				return 0L;
			}
			long num = 0L;
			int num2 = this.energyShieldResistHistory.Length;
			for (int i = 0; i < num2; i++)
			{
				num += this.energyShieldResistHistory[i];
			}
			return num;
		}
	}

	// Token: 0x0600127B RID: 4731 RVA: 0x00154884 File Offset: 0x00152A84
	public void MarkShieldResist(int damage)
	{
		if (this.energyShieldResistHistory == null)
		{
			this.energyShieldResistHistory = new long[60];
		}
		this.energyShieldResistHistory[0] += (long)damage * this.energyShieldEnergyRate;
	}

	// Token: 0x0600127C RID: 4732 RVA: 0x001548B4 File Offset: 0x00152AB4
	public void ScrollShieldResistHistory()
	{
		if (this.energyShieldResistHistory != null)
		{
			Array.Copy(this.energyShieldResistHistory, 0, this.energyShieldResistHistory, 1, this.energyShieldResistHistory.Length - 1);
			this.energyShieldResistHistory[0] = 0L;
		}
	}

	// Token: 0x0600127D RID: 4733 RVA: 0x001548E5 File Offset: 0x00152AE5
	private void ClearShieldResistHistory()
	{
		this.energyShieldResistHistory = null;
	}

	// Token: 0x0600127E RID: 4734 RVA: 0x001548EE File Offset: 0x00152AEE
	public void AddProductionStat(int itemId, int itemCount, PlanetFactory factory)
	{
		if (this.gameProduction == null)
		{
			this.gameProduction = this.gameData.statistics.production;
		}
		this.gameProduction.factoryStatPool[factory.index].AddProductionToTotalArray(itemId, itemCount);
	}

	// Token: 0x0600127F RID: 4735 RVA: 0x00154927 File Offset: 0x00152B27
	public void AddConsumptionStat(int itemId, int itemCount, PlanetFactory factory)
	{
		if (this.gameProduction == null)
		{
			this.gameProduction = this.gameData.statistics.production;
		}
		this.gameProduction.factoryStatPool[factory.index].AddConsumptionToTotalArray(itemId, itemCount);
	}

	// Token: 0x06001280 RID: 4736 RVA: 0x00154960 File Offset: 0x00152B60
	public void CheckCombatModuleDataIsValidPatch()
	{
		int patch = this.gameData.patch;
		if (this.groundCombatModule != null && this.groundCombatModule.id == 0 && this.groundCombatModule.entityId == 0 && this.groundCombatModule.battleBaseId == 0)
		{
			ModuleFleet[] moduleFleets = this.groundCombatModule.moduleFleets;
			int fleetCount = this.groundCombatModule.fleetCount;
			for (int i = 0; i < fleetCount; i++)
			{
				int fleetId = moduleFleets[i].fleetId;
				if (fleetId != 0)
				{
					PlanetData planetData = this.gameData.galaxy.PlanetById(moduleFleets[i].fleetAstroId);
					if (planetData == null)
					{
						moduleFleets[i].ClearFleetForeignKey();
					}
					else
					{
						PlanetFactory factory = planetData.factory;
						if (factory == null)
						{
							moduleFleets[i].ClearFleetForeignKey();
						}
						else
						{
							ref FleetComponent ptr = ref factory.combatGroundSystem.fleets.buffer[fleetId];
							if (ptr.owner != -1)
							{
								moduleFleets[i].ClearFleetForeignKey();
							}
							else
							{
								ref CraftData ptr2 = ref factory.craftPool[ptr.craftId];
								if (ptr2.id != ptr.craftId || ptr2.owner != ptr.owner)
								{
									moduleFleets[i].ClearFleetForeignKey();
								}
								else
								{
									ModuleFighter[] fighters = moduleFleets[i].fighters;
									int num = fighters.Length;
									for (int j = 0; j < num; j++)
									{
										int craftId = fighters[j].craftId;
										if (craftId != 0)
										{
											ref CraftData ptr3 = ref factory.craftPool[craftId];
											if (ptr3.id != craftId || ptr3.unitId == 0)
											{
												fighters[j].ClearFighterForeignKey();
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		if (this.spaceCombatModule != null && this.spaceCombatModule.id == 0 && this.spaceCombatModule.entityId == 0 && this.spaceCombatModule.battleBaseId == 0)
		{
			ModuleFleet[] moduleFleets2 = this.spaceCombatModule.moduleFleets;
			int fleetCount2 = this.spaceCombatModule.fleetCount;
			DataPool<FleetComponent> fleets = this.sector.combatSpaceSystem.fleets;
			for (int k = 0; k < fleetCount2; k++)
			{
				int fleetId2 = moduleFleets2[k].fleetId;
				if (fleetId2 != 0)
				{
					ref FleetComponent ptr4 = ref fleets.buffer[fleetId2];
					if (ptr4.owner != -1)
					{
						moduleFleets2[k].ClearFleetForeignKey();
					}
					else
					{
						ref CraftData ptr5 = ref this.sector.craftPool[ptr4.craftId];
						if (ptr5.id != ptr4.craftId || ptr5.owner != ptr4.owner)
						{
							moduleFleets2[k].ClearFleetForeignKey();
						}
						else
						{
							ModuleFighter[] fighters2 = moduleFleets2[k].fighters;
							int num2 = fighters2.Length;
							for (int l = 0; l < num2; l++)
							{
								int craftId2 = fighters2[l].craftId;
								if (craftId2 != 0)
								{
									ref CraftData ptr6 = ref this.sector.craftPool[craftId2];
									if (ptr6.id != craftId2 || ptr6.unitId == 0)
									{
										fighters2[l].ClearFighterForeignKey();
									}
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001281 RID: 4737 RVA: 0x00154CA4 File Offset: 0x00152EA4
	private int split_inc(ref int n, ref int m, int p)
	{
		if (n <= 0)
		{
			n = (m = 0);
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

	// Token: 0x040015FC RID: 5628
	private const int _data_end = 2136882202;

	// Token: 0x040015FD RID: 5629
	public Player player;

	// Token: 0x040015FE RID: 5630
	public GameData gameData;

	// Token: 0x040015FF RID: 5631
	public SpaceSector sector;

	// Token: 0x04001600 RID: 5632
	public GameHistoryData history;

	// Token: 0x04001601 RID: 5633
	public double coreEnergyCap;

	// Token: 0x04001602 RID: 5634
	public double coreEnergy;

	// Token: 0x04001603 RID: 5635
	public double corePowerGen;

	// Token: 0x04001604 RID: 5636
	public double reactorPowerGen;

	// Token: 0x04001605 RID: 5637
	public double reactorEnergy;

	// Token: 0x04001606 RID: 5638
	public int reactorItemId;

	// Token: 0x04001607 RID: 5639
	public int reactorItemInc;

	// Token: 0x04001608 RID: 5640
	public bool autoReplenishFuel;

	// Token: 0x04001609 RID: 5641
	public bool autoReplenishWarper;

	// Token: 0x0400160A RID: 5642
	public StorageComponent reactorStorage;

	// Token: 0x0400160B RID: 5643
	public StorageComponent warpStorage;

	// Token: 0x0400160C RID: 5644
	public double energyConsumptionCoef;

	// Token: 0x0400160D RID: 5645
	public double walkPower;

	// Token: 0x0400160E RID: 5646
	public double jumpEnergy;

	// Token: 0x0400160F RID: 5647
	public double thrustPowerPerAcc;

	// Token: 0x04001610 RID: 5648
	public double warpKeepingPowerPerSpeed;

	// Token: 0x04001611 RID: 5649
	public double warpStartPowerPerSpeed;

	// Token: 0x04001612 RID: 5650
	public double miningPower;

	// Token: 0x04001613 RID: 5651
	public double replicatePower;

	// Token: 0x04001614 RID: 5652
	public double researchPower;

	// Token: 0x04001615 RID: 5653
	public double droneEjectEnergy;

	// Token: 0x04001616 RID: 5654
	public double droneEnergyPerMeter;

	// Token: 0x04001617 RID: 5655
	public double instantBuildEnergy;

	// Token: 0x04001618 RID: 5656
	public const int WARPER_ITEMID = 1210;

	// Token: 0x04001619 RID: 5657
	public const int WARPER_COST = 1;

	// Token: 0x0400161A RID: 5658
	public const int WARPER_GRIDCNT = 20;

	// Token: 0x0400161B RID: 5659
	public int coreLevel;

	// Token: 0x0400161C RID: 5660
	public int thrusterLevel;

	// Token: 0x0400161D RID: 5661
	public float miningSpeed;

	// Token: 0x0400161E RID: 5662
	public float replicateSpeed;

	// Token: 0x0400161F RID: 5663
	public float walkSpeed;

	// Token: 0x04001620 RID: 5664
	public float jumpSpeed;

	// Token: 0x04001621 RID: 5665
	public float maxSailSpeed;

	// Token: 0x04001622 RID: 5666
	public float maxWarpSpeed;

	// Token: 0x04001623 RID: 5667
	public float buildArea;

	// Token: 0x04001624 RID: 5668
	public MechaForge forge;

	// Token: 0x04001625 RID: 5669
	public MechaLab lab;

	// Token: 0x04001626 RID: 5670
	public ConstructionModuleComponent constructionModule;

	// Token: 0x04001627 RID: 5671
	public Vector3 autoReconstructLastSearchPos;

	// Token: 0x04001628 RID: 5672
	public int autoReconstructLastSearchAstroId;

	// Token: 0x04001629 RID: 5673
	public Vector3 buildLastSearchPos;

	// Token: 0x0400162A RID: 5674
	public int buildLastSearchAstroId;

	// Token: 0x0400162B RID: 5675
	public Vector3 repairLastSearchPos;

	// Token: 0x0400162C RID: 5676
	public int repairLastSearchAstroId;

	// Token: 0x0400162D RID: 5677
	public int hp;

	// Token: 0x0400162E RID: 5678
	public int hpRecoverCD;

	// Token: 0x0400162F RID: 5679
	public int energyShieldRecoverCD;

	// Token: 0x04001630 RID: 5680
	public long energyShieldEnergy;

	// Token: 0x04001631 RID: 5681
	public int hpMax;

	// Token: 0x04001632 RID: 5682
	public int hpMaxUpgrade;

	// Token: 0x04001633 RID: 5683
	public int hpRecover;

	// Token: 0x04001634 RID: 5684
	public bool energyShieldUnlocked;

	// Token: 0x04001635 RID: 5685
	public bool energyShieldRechargeEnabled;

	// Token: 0x04001636 RID: 5686
	public float energyShieldRechargeSpeed;

	// Token: 0x04001637 RID: 5687
	public float energyShieldRadius;

	// Token: 0x04001638 RID: 5688
	public long energyShieldCapacity;

	// Token: 0x04001639 RID: 5689
	public long energyShieldEnergyRate;

	// Token: 0x0400163A RID: 5690
	public bool energyShieldBurstUnlocked;

	// Token: 0x0400163B RID: 5691
	public double energyShieldBurstProgress;

	// Token: 0x0400163C RID: 5692
	public long energyShieldBurstDamageRate;

	// Token: 0x0400163D RID: 5693
	public int energyShieldBurstTick;

	// Token: 0x0400163E RID: 5694
	public const double energyShieldBurstReadyProgress = 0.2;

	// Token: 0x0400163F RID: 5695
	public ColliderData skillColliderL;

	// Token: 0x04001640 RID: 5696
	public ColliderDataLF skillColliderU;

	// Token: 0x04001641 RID: 5697
	public int ammoItemId;

	// Token: 0x04001642 RID: 5698
	public int ammoInc;

	// Token: 0x04001643 RID: 5699
	public int ammoBulletCount;

	// Token: 0x04001644 RID: 5700
	public int ammoSelectSlot;

	// Token: 0x04001645 RID: 5701
	public int ammoSelectSlotState;

	// Token: 0x04001646 RID: 5702
	public int ammoMuzzleFire;

	// Token: 0x04001647 RID: 5703
	public int ammoRoundFire;

	// Token: 0x04001648 RID: 5704
	public int ammoMuzzleIndex;

	// Token: 0x04001649 RID: 5705
	public bool laserActive;

	// Token: 0x0400164A RID: 5706
	public int laserActiveState;

	// Token: 0x0400164B RID: 5707
	public bool laserRecharging;

	// Token: 0x0400164C RID: 5708
	public long laserEnergy;

	// Token: 0x0400164D RID: 5709
	public long laserEnergyCapacity;

	// Token: 0x0400164E RID: 5710
	public int laserFire;

	// Token: 0x0400164F RID: 5711
	public bool bombActive;

	// Token: 0x04001650 RID: 5712
	public int bombFire;

	// Token: 0x04001651 RID: 5713
	public const int BOMB_FIRE_INTERVAL = 10;

	// Token: 0x04001652 RID: 5714
	public bool autoReplenishAmmo;

	// Token: 0x04001653 RID: 5715
	public StorageComponent ammoStorage;

	// Token: 0x04001654 RID: 5716
	public StorageComponent bombStorage;

	// Token: 0x04001655 RID: 5717
	public EnemyHatredTarget ammoHatredTarget;

	// Token: 0x04001656 RID: 5718
	public EnemyHatredTarget laserHatredTarget;

	// Token: 0x04001657 RID: 5719
	public const float CHANGE_TARGET_THRESHOLD = 0.667f;

	// Token: 0x04001658 RID: 5720
	public float bulletLocalAttackRange;

	// Token: 0x04001659 RID: 5721
	public float bulletSpaceAttackRange;

	// Token: 0x0400165A RID: 5722
	public int bulletEnergyCost;

	// Token: 0x0400165B RID: 5723
	public float bulletDamageScale;

	// Token: 0x0400165C RID: 5724
	public int bulletROF;

	// Token: 0x0400165D RID: 5725
	public int bulletMuzzleCount;

	// Token: 0x0400165E RID: 5726
	public int bulletMuzzleInterval;

	// Token: 0x0400165F RID: 5727
	public int bulletRoundInterval;

	// Token: 0x04001660 RID: 5728
	public float cannonLocalAttackRange;

	// Token: 0x04001661 RID: 5729
	public float cannonSpaceAttackRange;

	// Token: 0x04001662 RID: 5730
	public int cannonEnergyCost;

	// Token: 0x04001663 RID: 5731
	public float cannonDamageScale;

	// Token: 0x04001664 RID: 5732
	public int cannonROF;

	// Token: 0x04001665 RID: 5733
	public int cannonMuzzleCount;

	// Token: 0x04001666 RID: 5734
	public int cannonMuzzleInterval;

	// Token: 0x04001667 RID: 5735
	public int cannonRoundInterval;

	// Token: 0x04001668 RID: 5736
	public float plasmaLocalAttackRange;

	// Token: 0x04001669 RID: 5737
	public float plasmaSpaceAttackRange;

	// Token: 0x0400166A RID: 5738
	public int plasmaEnergyCost;

	// Token: 0x0400166B RID: 5739
	public float plasmaDamageScale;

	// Token: 0x0400166C RID: 5740
	public int plasmaROF;

	// Token: 0x0400166D RID: 5741
	public int plasmaMuzzleCount;

	// Token: 0x0400166E RID: 5742
	public int plasmaMuzzleInterval;

	// Token: 0x0400166F RID: 5743
	public int plasmaRoundInterval;

	// Token: 0x04001670 RID: 5744
	public float missileLocalAttackRange;

	// Token: 0x04001671 RID: 5745
	public float missileSpaceAttackRange;

	// Token: 0x04001672 RID: 5746
	public int missileEnergyCost;

	// Token: 0x04001673 RID: 5747
	public float missileDamageScale;

	// Token: 0x04001674 RID: 5748
	public int missileROF;

	// Token: 0x04001675 RID: 5749
	public int missileMuzzleCount;

	// Token: 0x04001676 RID: 5750
	public int missileMuzzleInterval;

	// Token: 0x04001677 RID: 5751
	public int missileRoundInterval;

	// Token: 0x04001678 RID: 5752
	public float laserLocalAttackRange;

	// Token: 0x04001679 RID: 5753
	public float laserSpaceAttackRange;

	// Token: 0x0400167A RID: 5754
	public int laserLocalEnergyCost;

	// Token: 0x0400167B RID: 5755
	public int laserSpaceEnergyCost;

	// Token: 0x0400167C RID: 5756
	public int laserLocalDamage;

	// Token: 0x0400167D RID: 5757
	public int laserSpaceDamage;

	// Token: 0x0400167E RID: 5758
	public int laserLocalInterval;

	// Token: 0x0400167F RID: 5759
	public int laserSpaceInterval;

	// Token: 0x04001680 RID: 5760
	public bool autoReplenishHangar;

	// Token: 0x04001681 RID: 5761
	public StorageComponent fighterStorage;

	// Token: 0x04001682 RID: 5762
	public CombatModuleComponent groundCombatModule;

	// Token: 0x04001683 RID: 5763
	public CombatModuleComponent spaceCombatModule;

	// Token: 0x04001684 RID: 5764
	public MechaAppearance appearance;

	// Token: 0x04001685 RID: 5765
	public MechaAppearance diyAppearance;

	// Token: 0x04001686 RID: 5766
	public ItemBundle diyItems;

	// Token: 0x04001687 RID: 5767
	public double[] energyChanges;

	// Token: 0x04001688 RID: 5768
	public const int EC_CORE_GEN = 0;

	// Token: 0x04001689 RID: 5769
	public const int EC_REACTOR_GEN = 1;

	// Token: 0x0400168A RID: 5770
	public const int EC_CHARGE = 2;

	// Token: 0x0400168B RID: 5771
	public const int EC_WALK = 3;

	// Token: 0x0400168C RID: 5772
	public const int EC_THRUSTER = 4;

	// Token: 0x0400168D RID: 5773
	public const int EC_MINE = 5;

	// Token: 0x0400168E RID: 5774
	public const int EC_REPLICATE = 6;

	// Token: 0x0400168F RID: 5775
	public const int EC_RESEARCH = 7;

	// Token: 0x04001690 RID: 5776
	public const int EC_DRONE = 8;

	// Token: 0x04001691 RID: 5777
	public const int EC_WARPDRIVE = 9;

	// Token: 0x04001692 RID: 5778
	public const int EC_ENERGY_SHIELD = 10;

	// Token: 0x04001693 RID: 5779
	public const int EC_LASER = 11;

	// Token: 0x04001694 RID: 5780
	public const int EC_COMBAT_DRONE = 12;

	// Token: 0x04001695 RID: 5781
	public const int EC_COMBAT_SHIP = 13;

	// Token: 0x04001696 RID: 5782
	public const int EC_AMMO = 14;

	// Token: 0x04001697 RID: 5783
	public const int EC_INSTANT_BUILD = 15;

	// Token: 0x04001698 RID: 5784
	public const int EC_MAX = 32;

	// Token: 0x04001699 RID: 5785
	public int[] chargerDevice;

	// Token: 0x0400169A RID: 5786
	public int chargerCount;

	// Token: 0x0400169B RID: 5787
	public const int CHARGER_MAX = 256;

	// Token: 0x0400169C RID: 5788
	public long energyShieldRechargeCurrent;

	// Token: 0x0400169D RID: 5789
	public long[] energyShieldResistHistory;

	// Token: 0x0400169E RID: 5790
	private ProductionStatistics gameProduction;

	// Token: 0x02000C0B RID: 3083
	// (Invoke) Token: 0x06007D5E RID: 32094
	public delegate SkillTarget DFireFunc();
}
