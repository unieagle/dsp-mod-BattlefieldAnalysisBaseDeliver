using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

// Token: 0x02000238 RID: 568
public class PlanetFactory
{
	// Token: 0x17000376 RID: 886
	// (get) Token: 0x0600181A RID: 6170 RVA: 0x0019AB7A File Offset: 0x00198D7A
	// (set) Token: 0x0600181B RID: 6171 RVA: 0x0019AB82 File Offset: 0x00198D82
	public int index { get; private set; }

	// Token: 0x17000377 RID: 887
	// (get) Token: 0x0600181C RID: 6172 RVA: 0x0019AB8B File Offset: 0x00198D8B
	// (set) Token: 0x0600181D RID: 6173 RVA: 0x0019AB93 File Offset: 0x00198D93
	public GameData gameData { get; private set; }

	// Token: 0x17000378 RID: 888
	// (get) Token: 0x0600181E RID: 6174 RVA: 0x0019AB9C File Offset: 0x00198D9C
	// (set) Token: 0x0600181F RID: 6175 RVA: 0x0019ABA4 File Offset: 0x00198DA4
	public PlanetData planet { get; private set; }

	// Token: 0x17000379 RID: 889
	// (get) Token: 0x06001820 RID: 6176 RVA: 0x0019ABAD File Offset: 0x00198DAD
	public int planetId
	{
		get
		{
			return this.planet.id;
		}
	}

	// Token: 0x1700037A RID: 890
	// (get) Token: 0x06001821 RID: 6177 RVA: 0x0019ABBA File Offset: 0x00198DBA
	public DysonSphere dysonSphere
	{
		get
		{
			return this.gameData.dysonSpheres[this.planet.star.index];
		}
	}

	// Token: 0x1700037B RID: 891
	// (get) Token: 0x06001822 RID: 6178 RVA: 0x0019ABD8 File Offset: 0x00198DD8
	public int entityCount
	{
		get
		{
			return this.entityCursor - this.entityRecycleCursor - 1;
		}
	}

	// Token: 0x1700037C RID: 892
	// (get) Token: 0x06001823 RID: 6179 RVA: 0x0019ABE9 File Offset: 0x00198DE9
	public int prebuildCount
	{
		get
		{
			return this.prebuildCursor - this.prebuildRecycleCursor - 1;
		}
	}

	// Token: 0x14000026 RID: 38
	// (add) Token: 0x06001824 RID: 6180 RVA: 0x0019ABFC File Offset: 0x00198DFC
	// (remove) Token: 0x06001825 RID: 6181 RVA: 0x0019AC34 File Offset: 0x00198E34
	public event Action<int, int> onPrebuildChange;

	// Token: 0x06001826 RID: 6182 RVA: 0x0019AC69 File Offset: 0x00198E69
	public void NotifyPrebuildChange(int prebuildId, int change)
	{
		if (this.onPrebuildChange != null)
		{
			this.onPrebuildChange(prebuildId, change);
		}
	}

	// Token: 0x1700037D RID: 893
	// (get) Token: 0x06001827 RID: 6183 RVA: 0x0019AC80 File Offset: 0x00198E80
	public int craftCount
	{
		get
		{
			return this.craftCursor - this.craftRecycleCursor - 1;
		}
	}

	// Token: 0x1700037E RID: 894
	// (get) Token: 0x06001828 RID: 6184 RVA: 0x0019AC91 File Offset: 0x00198E91
	public int enemyCount
	{
		get
		{
			return this.enemyCursor - this.enemyRecycleCursor - 1;
		}
	}

	// Token: 0x1700037F RID: 895
	// (get) Token: 0x06001829 RID: 6185 RVA: 0x0019ACA2 File Offset: 0x00198EA2
	public int veinCount
	{
		get
		{
			return this.veinCursor - this.veinRecycleCursor - 1;
		}
	}

	// Token: 0x17000380 RID: 896
	// (get) Token: 0x0600182A RID: 6186 RVA: 0x0019ACB3 File Offset: 0x00198EB3
	public int miningFlag
	{
		get
		{
			return this._miningFlag;
		}
	}

	// Token: 0x17000381 RID: 897
	// (get) Token: 0x0600182B RID: 6187 RVA: 0x0019ACBB File Offset: 0x00198EBB
	public int veinMiningFlag
	{
		get
		{
			return this._veinMiningFlag;
		}
	}

	// Token: 0x0600182C RID: 6188 RVA: 0x0019ACC3 File Offset: 0x00198EC3
	public void AddMiningFlagUnsafe(EVeinType addVeinType)
	{
		this._miningFlag |= 1 << (int)addVeinType;
	}

	// Token: 0x0600182D RID: 6189 RVA: 0x0019ACD8 File Offset: 0x00198ED8
	public void AddVeinMiningFlagUnsafe(EVeinType addVeinType)
	{
		this._veinMiningFlag |= 1 << (int)addVeinType;
	}

	// Token: 0x17000382 RID: 898
	// (get) Token: 0x0600182E RID: 6190 RVA: 0x0019ACED File Offset: 0x00198EED
	public int vegeCount
	{
		get
		{
			return this.vegeCursor - this.vegeRecycleCursor - 1;
		}
	}

	// Token: 0x17000383 RID: 899
	// (get) Token: 0x0600182F RID: 6191 RVA: 0x0019ACFE File Offset: 0x00198EFE
	public int ruinCount
	{
		get
		{
			return this.ruinCursor - this.ruinRecycleCursor - 1;
		}
	}

	// Token: 0x06001830 RID: 6192 RVA: 0x0019AD10 File Offset: 0x00198F10
	public static void InitPrefabDescArray()
	{
		if (PlanetFactory.PrefabDescByModelIndex == null)
		{
			ModelProto[] dataArray = LDB.models.dataArray;
			PlanetFactory.PrefabDescByModelIndex = new PrefabDesc[dataArray.Length + 64];
			for (int i = 0; i < dataArray.Length; i++)
			{
				PlanetFactory.PrefabDescByModelIndex[dataArray[i].ID] = dataArray[i].prefabDesc;
			}
		}
	}

	// Token: 0x06001831 RID: 6193 RVA: 0x0019AD64 File Offset: 0x00198F64
	public void Init(GameData _gameData, PlanetData _planet, int _index)
	{
		this.index = _index;
		this.gameData = _gameData;
		this.planet = _planet;
		this.planet.factory = this;
		this.sector = this.gameData.spaceSector;
		this.skillSystem = this.gameData.spaceSector.skillSystem;
		this.landed = false;
		if (this.planet.id == _gameData.galaxy.birthPlanetId)
		{
			this.landed = true;
		}
		bool isCombatMode = this.gameData.gameDesc.isCombatMode;
		this.SetEntityCapacity(1024);
		this.SetCraftCapacity(128);
		this.SetPrebuildCapacity(256);
		this.SetEnemyCapacity(isCombatMode ? 1024 : 32);
		this.hashSystemDynamic = new HashSystem();
		this.hashSystemStatic = new HashSystem();
		this.spaceHashSystemDynamic = new DFSDynamicHashSystem();
		this.spaceHashSystemDynamic.Init(this.planet);
		this.cargoContainer = new CargoContainer();
		this.cargoTraffic = new CargoTraffic(this.planet);
		this.blockContainer = new MiniBlockContainer();
		this.factoryStorage = new FactoryStorage(this.planet);
		this.powerSystem = new PowerSystem(this.planet);
		this.constructionSystem = new ConstructionSystem(this.planet);
		PlanetData planetData = null;
		PlanetData planetData2;
		if (this.planet.loaded)
		{
			planetData2 = this.planet;
		}
		else
		{
			planetData = (planetData2 = PlanetData.GetUnloadedCopy(this.planet));
		}
		if (planetData != null)
		{
			planetData.RegenerateRawDataImmediately();
		}
		this.SetVeinCapacity(planetData2.data.veinCursor + 2);
		this.veinCursor = planetData2.data.veinCursor;
		Array.Copy(planetData2.data.veinPool, this.veinPool, this.veinCursor);
		this.SetVegeCapacity(planetData2.data.vegeCursor + 2);
		this.vegeCursor = planetData2.data.vegeCursor;
		Array.Copy(planetData2.data.vegePool, this.vegePool, this.vegeCursor);
		this.InitVegeHashAddress();
		this.InitVeinHashAddress();
		this.InitVeinGroups(planetData2);
		PlanetData.ReleaseCopy(planetData);
		this.SetRuinCapacity(isCombatMode ? 1024 : 32);
		this.factorySystem = new FactorySystem(this.planet);
		this.enemySystem = new EnemyDFGroundSystem(this.planet);
		this.combatGroundSystem = new CombatGroundSystem(this.planet);
		this.defenseSystem = new DefenseSystem(this.planet);
		this.planetATField = new PlanetATField(this.planet);
		this.transport = new PlanetTransport(this.gameData, this.planet);
		this.transport.Init();
		this.platformSystem = new PlatformSystem(this.planet);
		this.enemySystem.RefreshPlanetReformState();
		this.digitalSystem = new DigitalSystem(this.planet);
	}

	// Token: 0x06001832 RID: 6194 RVA: 0x0019B030 File Offset: 0x00199230
	public void Free()
	{
		this.planet = null;
		this.entityPool = null;
		this.entityAnimPool = null;
		this.entitySignPool = null;
		this.entityConnPool = null;
		this.entityMutexs = null;
		this.entityNeeds = null;
		this.entityRecycle = null;
		this.craftPool = null;
		this.craftAnimPool = null;
		this.craftRecycle = null;
		if (this.prebuildPool != null)
		{
			for (int i = 0; i < this.prebuildPool.Length; i++)
			{
				this.prebuildPool[i].parameters = null;
			}
		}
		this.prebuildPool = null;
		this.prebuildRecycle = null;
		this.prebuildConnPool = null;
		this.enemyPool = null;
		this.enemyAnimPool = null;
		this.enemyRecycle = null;
		this.veinPool = null;
		this.veinAnimPool = null;
		this.veinRecycle = null;
		this.vegePool = null;
		this.vegeRecycle = null;
		this.ruinPool = null;
		this.ruinRecycle = null;
		if (this.hashSystemDynamic != null)
		{
			this.hashSystemDynamic.Free();
			this.hashSystemDynamic = null;
		}
		if (this.hashSystemStatic != null)
		{
			this.hashSystemStatic.Free();
			this.hashSystemStatic = null;
		}
		if (this.spaceHashSystemDynamic != null)
		{
			this.spaceHashSystemDynamic.Free();
			this.spaceHashSystemDynamic = null;
		}
		if (this.cargoContainer != null)
		{
			this.cargoContainer.Free();
			this.cargoContainer = null;
		}
		if (this.cargoTraffic != null)
		{
			this.cargoTraffic.Free();
			this.cargoTraffic = null;
		}
		if (this.blockContainer != null)
		{
			this.blockContainer.Free();
			this.blockContainer = null;
		}
		if (this.factoryStorage != null)
		{
			this.factoryStorage.Free();
			this.factoryStorage = null;
		}
		if (this.powerSystem != null)
		{
			this.powerSystem.Free();
			this.powerSystem = null;
		}
		if (this.constructionSystem != null)
		{
			this.constructionSystem.Free();
			this.constructionSystem = null;
		}
		if (this.factorySystem != null)
		{
			this.factorySystem.Free();
			this.factorySystem = null;
		}
		if (this.enemySystem != null)
		{
			this.enemySystem.Free();
			this.enemySystem = null;
		}
		if (this.combatGroundSystem != null)
		{
			this.combatGroundSystem.Free();
			this.combatGroundSystem = null;
		}
		if (this.defenseSystem != null)
		{
			this.defenseSystem.Free();
			this.defenseSystem = null;
		}
		if (this.planetATField != null)
		{
			this.planetATField.Free();
			this.planetATField = null;
		}
		if (this.transport != null)
		{
			this.transport.Free();
			this.transport = null;
		}
		if (this.platformSystem != null)
		{
			this.platformSystem.Free();
			this.platformSystem = null;
		}
		if (this.digitalSystem != null)
		{
			this.digitalSystem.Free();
			this.digitalSystem = null;
		}
	}

	// Token: 0x06001833 RID: 6195 RVA: 0x0019B2CC File Offset: 0x001994CC
	public void UnloadDisplay()
	{
		if (this.cargoTraffic != null)
		{
			this.cargoTraffic.DestroyRenderingBatches();
		}
		if (this.blockContainer != null)
		{
			this.blockContainer.Clear();
		}
		for (int i = 0; i < this.entityCursor; i++)
		{
			this.entityPool[i].modelId = 0;
			this.entityPool[i].mmblockId = 0;
			this.entityPool[i].colliderId = 0;
			this.entityPool[i].audioId = 0;
		}
		for (int j = 0; j < this.craftCursor; j++)
		{
			this.craftPool[j].modelId = 0;
			this.craftPool[j].mmblockId = 0;
			this.craftPool[j].colliderId = 0;
			this.craftPool[j].audioId = 0;
		}
		for (int k = 0; k < this.vegeCursor; k++)
		{
			this.vegePool[k].modelId = 0;
			this.vegePool[k].colliderId = 0;
		}
		for (int l = 0; l < this.veinCursor; l++)
		{
			this.veinPool[l].modelId = 0;
			this.veinPool[l].colliderId = 0;
			this.veinPool[l].minerBaseModelId = 0;
			this.veinPool[l].minerCircleModelId0 = 0;
			this.veinPool[l].minerCircleModelId1 = 0;
			this.veinPool[l].minerCircleModelId2 = 0;
			this.veinPool[l].minerCircleModelId3 = 0;
		}
		for (int m = 0; m < this.prebuildCursor; m++)
		{
			this.prebuildPool[m].modelId = 0;
			this.prebuildPool[m].colliderId = 0;
		}
		for (int n = 0; n < this.enemyCursor; n++)
		{
			this.enemyPool[n].modelId = 0;
			this.enemyPool[n].mmblockId = 0;
			this.enemyPool[n].colliderId = 0;
			this.enemyPool[n].audioId = 0;
		}
		for (int num = 0; num < this.ruinCursor; num++)
		{
			this.ruinPool[num].modelId = 0;
			this.ruinPool[num].mmblockId = 0;
			this.ruinPool[num].colliderId = 0;
		}
	}

	// Token: 0x06001834 RID: 6196 RVA: 0x0019B568 File Offset: 0x00199768
	public void FlushPools()
	{
		if (this.prebuildCount == 0 && this.prebuildCapacity >= 1024)
		{
			this.prebuildCursor = 1;
			this.prebuildRecycleCursor = 0;
			this.prebuildPool = null;
			this.prebuildRecycle = null;
			this.prebuildConnPool = null;
			this.SetPrebuildCapacity(256);
			this.constructionSystem.OnPrebuildPoolFlush();
		}
	}

	// Token: 0x06001835 RID: 6197 RVA: 0x0019B5C3 File Offset: 0x001997C3
	public DysonSphere CheckOrCreateDysonSphere()
	{
		return this.gameData.CreateDysonSphere(this.planet.star.index);
	}

	// Token: 0x06001836 RID: 6198 RVA: 0x0019B5E0 File Offset: 0x001997E0
	private void SetEntityCapacity(int newCapacity)
	{
		EntityData[] array = this.entityPool;
		this.entityPool = new EntityData[newCapacity];
		this.entityRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.entityPool, (newCapacity > this.entityCapacity) ? this.entityCapacity : newCapacity);
		}
		AnimData[] array2 = this.entityAnimPool;
		this.entityAnimPool = new AnimData[newCapacity];
		if (array2 != null)
		{
			Array.Copy(array2, this.entityAnimPool, (newCapacity > this.entityCapacity) ? this.entityCapacity : newCapacity);
		}
		SignData[] array3 = this.entitySignPool;
		this.entitySignPool = new SignData[newCapacity];
		if (array3 != null)
		{
			Array.Copy(array3, this.entitySignPool, (newCapacity > this.entityCapacity) ? this.entityCapacity : newCapacity);
		}
		int[] array4 = this.entityConnPool;
		this.entityConnPool = new int[newCapacity * 16];
		if (array4 != null)
		{
			Array.Copy(array4, this.entityConnPool, ((newCapacity > this.entityCapacity) ? this.entityCapacity : newCapacity) * 16);
		}
		Mutex[] array5 = this.entityMutexs;
		this.entityMutexs = new Mutex[newCapacity];
		if (array5 != null)
		{
			Array.Copy(array5, this.entityMutexs, this.entityCapacity);
		}
		int[][] array6 = this.entityNeeds;
		this.entityNeeds = new int[newCapacity][];
		if (array6 != null)
		{
			Array.Copy(array6, this.entityNeeds, (newCapacity > this.entityCapacity) ? this.entityCapacity : newCapacity);
		}
		this.entityCapacity = newCapacity;
	}

	// Token: 0x06001837 RID: 6199 RVA: 0x0019B738 File Offset: 0x00199938
	public int AddEntityData(EntityData entity)
	{
		if (this.entityRecycleCursor > 0)
		{
			int[] array = this.entityRecycle;
			int num = this.entityRecycleCursor - 1;
			this.entityRecycleCursor = num;
			entity.id = array[num];
		}
		else
		{
			int num = this.entityCursor;
			this.entityCursor = num + 1;
			entity.id = num;
		}
		if (entity.id == this.entityCapacity)
		{
			this.SetEntityCapacity(this.entityCapacity * 2);
		}
		this.entityPool[entity.id] = entity;
		this.entityPool[entity.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(entity.id, entity.pos, EObjectType.None);
		return entity.id;
	}

	// Token: 0x06001838 RID: 6200 RVA: 0x0019B7EC File Offset: 0x001999EC
	public void CreateEntityDisplayComponents(int entityId)
	{
		int modelIndex = (int)this.entityPool[entityId].modelIndex;
		ModelProto modelProto = LDB.models.modelArray[modelIndex];
		if (modelProto == null)
		{
			return;
		}
		PrefabDesc prefabDesc = modelProto.prefabDesc;
		if (prefabDesc == null)
		{
			return;
		}
		this.entityPool[entityId].modelId = GameMain.gpuiManager.AddModel(modelIndex, entityId, this.entityPool[entityId].pos, this.entityPool[entityId].rot, true);
		if (prefabDesc.minimapType > 0 && this.entityPool[entityId].mmblockId == 0)
		{
			if (this.entityPool[entityId].inserterId == 0)
			{
				this.entityPool[entityId].mmblockId = this.blockContainer.AddMiniBlock(entityId, prefabDesc.minimapType, this.entityPool[entityId].pos, this.entityPool[entityId].rot, prefabDesc.minimapScl);
			}
			else
			{
				Assert.Positive(this.factorySystem.inserterPool[this.entityPool[entityId].inserterId].id);
				ref InserterPose ptr = ref this.factorySystem.inserterPosePool[this.entityPool[entityId].inserterId];
				Vector3 pos = Vector3.Lerp(this.entityPool[entityId].pos, ptr.pos2, 0.5f);
				Quaternion rot = Quaternion.LookRotation(ptr.pos2 - this.entityPool[entityId].pos, pos.normalized);
				Vector4 scl = new Vector4(0.7f, 0.7f, Vector3.Distance(ptr.pos2, this.entityPool[entityId].pos) * 0.5f + 0.2f, 0f);
				this.entityPool[entityId].mmblockId = this.blockContainer.AddMiniBlock(entityId, prefabDesc.minimapType, pos, rot, scl);
			}
		}
		if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
		{
			for (int i = 0; i < prefabDesc.colliders.Length; i++)
			{
				if (this.entityPool[entityId].inserterId != 0)
				{
					ColliderData colliderData = prefabDesc.colliders[i];
					Assert.Positive(this.factorySystem.inserterPool[this.entityPool[entityId].inserterId].id);
					ref InserterPose ptr2 = ref this.factorySystem.inserterPosePool[this.entityPool[entityId].inserterId];
					Vector3 wpos = Vector3.Lerp(this.entityPool[entityId].pos, ptr2.pos2, 0.5f);
					Quaternion wrot = Quaternion.LookRotation(ptr2.pos2 - this.entityPool[entityId].pos, (ptr2.rot2 * Vector3.up + this.entityPool[entityId].rot * Vector3.up).normalized);
					colliderData.ext = new Vector3(colliderData.ext.x, colliderData.ext.y, Mathf.Max(0.1f, Vector3.Distance(ptr2.pos2, this.entityPool[entityId].pos) * 0.5f + colliderData.ext.z));
					this.entityPool[entityId].colliderId = this.planet.physics.AddColliderData(colliderData.BindToObject(entityId, this.entityPool[entityId].colliderId, EObjectType.None, wpos, wrot));
				}
				else if (this.entityPool[entityId].beltId != 0)
				{
					if (this.entityPool[entityId].colliderId == 0)
					{
						this.entityPool[entityId].colliderId = this.planet.physics.AddColliderData(prefabDesc.colliders[i].BindToObject(entityId, 0, EObjectType.None, this.entityPool[entityId].pos, this.entityPool[entityId].rot));
					}
				}
				else
				{
					this.entityPool[entityId].colliderId = this.planet.physics.AddColliderData(prefabDesc.colliders[i].BindToObject(entityId, this.entityPool[entityId].colliderId, EObjectType.None, this.entityPool[entityId].pos, this.entityPool[entityId].rot));
				}
			}
		}
		if (prefabDesc.hasAudio)
		{
			this.entityPool[entityId].audioId = this.planet.audio.AddAudioData(entityId, EObjectType.None, this.entityPool[entityId].pos, prefabDesc);
			if (this.entityPool[entityId].speakerId > 0)
			{
				SpeakerComponent speakerComponent = this.digitalSystem.speakerPool[this.entityPool[entityId].speakerId];
				if (speakerComponent.id == this.entityPool[entityId].speakerId)
				{
					this.planet.audio.ChangeAudioSpeakerInfo(this.entityPool[entityId].audioId, speakerComponent.audioId, speakerComponent.oneShotAudioId, speakerComponent.repeatTimes);
					this.planet.audio.ChangeAudioDataFalloff(this.entityPool[entityId].audioId, speakerComponent.falloffRadius0, speakerComponent.falloffRadius1);
				}
			}
		}
	}

	// Token: 0x06001839 RID: 6201 RVA: 0x0019BD9C File Offset: 0x00199F9C
	public void CreateEntityLogicComponents(int entityId, PrefabDesc desc, int prebuildId)
	{
		this.entitySignPool[entityId].Reset(this.entityPool[entityId].pos, this.entityPool[entityId].rot, desc.signHeight, desc.signSize);
		bool flag = prebuildId > 0 && this.prebuildPool[prebuildId].id == prebuildId;
		int pcId = 0;
		int num = 0;
		if (!desc.isBelt && !desc.isInserter && !desc.isSplitter && !desc.isMonitor && !desc.isSpraycoster && !desc.isPiler)
		{
			this.entityMutexs[entityId] = new Mutex(entityId);
		}
		if (desc.isBelt)
		{
			int num2 = this.cargoTraffic.NewBeltComponent(entityId, desc.beltSpeed);
			if (flag)
			{
				bool flag2;
				int num3;
				int slot;
				this.ReadObjectConn(entityId, 0, out flag2, out num3, out slot);
				int num4;
				int slot2;
				this.ReadObjectConn(entityId, 1, out flag2, out num4, out slot2);
				int num5;
				int num6;
				this.ReadObjectConn(entityId, 2, out flag2, out num5, out num6);
				int num7;
				this.ReadObjectConn(entityId, 3, out flag2, out num7, out num6);
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num4 < 0)
				{
					num4 = 0;
				}
				if (num5 < 0)
				{
					num5 = 0;
				}
				if (num7 < 0)
				{
					num7 = 0;
				}
				int splitterId = this.entityPool[num3].splitterId;
				int splitterId2 = this.entityPool[num4].splitterId;
				int minerId = this.entityPool[num4].minerId;
				int stationId = this.entityPool[num4].stationId;
				int tankId = this.entityPool[num3].tankId;
				int tankId2 = this.entityPool[num4].tankId;
				int fractionatorId = this.entityPool[num3].fractionatorId;
				int fractionatorId2 = this.entityPool[num4].fractionatorId;
				int powerExcId = this.entityPool[num3].powerExcId;
				int powerExcId2 = this.entityPool[num4].powerExcId;
				int pilerId = this.entityPool[num3].pilerId;
				int pilerId2 = this.entityPool[num4].pilerId;
				if (num3 > 0)
				{
					num3 = this.entityPool[num3].beltId;
				}
				if (num4 > 0)
				{
					num4 = this.entityPool[num4].beltId;
				}
				if (num5 > 0)
				{
					num5 = this.entityPool[num5].beltId;
				}
				if (num7 > 0)
				{
					num7 = this.entityPool[num7].beltId;
				}
				this.cargoTraffic.AlterBeltConnections(num2, num3, num4, num5, num7, false);
				if (splitterId > 0)
				{
					this.cargoTraffic.ConnectToSplitter(splitterId, num2, slot, true);
				}
				else if (splitterId2 > 0)
				{
					this.cargoTraffic.ConnectToSplitter(splitterId2, num2, slot2, false);
				}
				else if (pilerId > 0)
				{
					this.cargoTraffic.RematchPilerConnection(pilerId);
				}
				else if (pilerId2 > 0)
				{
					this.cargoTraffic.RematchPilerConnection(pilerId2);
				}
				else if (minerId > 0)
				{
					if (stationId == 0)
					{
						this.factorySystem.SetMinerInsertTarget(minerId, entityId);
					}
				}
				else if (tankId > 0)
				{
					this.factoryStorage.SetTankBelt(tankId, num2, slot, false);
				}
				else if (tankId2 > 0)
				{
					this.factoryStorage.SetTankBelt(tankId2, num2, slot2, true);
				}
				else if (fractionatorId > 0)
				{
					this.factorySystem.SetFractionatorBelt(fractionatorId, num2, slot, false);
				}
				else if (fractionatorId2 > 0)
				{
					this.factorySystem.SetFractionatorBelt(fractionatorId2, num2, slot2, true);
				}
				else if (powerExcId > 0)
				{
					this.powerSystem.SetExchangerBelt(powerExcId, num2, slot, false);
				}
				else if (powerExcId2 > 0)
				{
					this.powerSystem.SetExchangerBelt(powerExcId2, num2, slot2, true);
				}
			}
			else
			{
				this.cargoTraffic.AlterBeltConnections(num2, 0, 0, 0, 0, false);
			}
		}
		else if (desc.isSplitter)
		{
			int splitterId3 = this.cargoTraffic.NewSplitterComponent(entityId);
			if (flag)
			{
				bool flag3;
				int num8;
				int num9;
				this.ReadObjectConn(entityId, 0, out flag3, out num8, out num9);
				bool flag4;
				int num10;
				this.ReadObjectConn(entityId, 1, out flag4, out num10, out num9);
				bool flag5;
				int num11;
				this.ReadObjectConn(entityId, 2, out flag5, out num11, out num9);
				bool flag6;
				int num12;
				this.ReadObjectConn(entityId, 3, out flag6, out num12, out num9);
				if (num8 < 0)
				{
					num8 = 0;
				}
				if (num10 < 0)
				{
					num10 = 0;
				}
				if (num11 < 0)
				{
					num11 = 0;
				}
				if (num12 < 0)
				{
					num12 = 0;
				}
				if (num8 > 0)
				{
					num8 = this.entityPool[num8].beltId;
					this.cargoTraffic.ConnectToSplitter(splitterId3, num8, 0, !flag3);
				}
				else
				{
					this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 0, false);
				}
				if (num10 > 0)
				{
					num10 = this.entityPool[num10].beltId;
					this.cargoTraffic.ConnectToSplitter(splitterId3, num10, 1, !flag4);
				}
				else
				{
					this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 1, false);
				}
				if (num11 > 0)
				{
					num11 = this.entityPool[num11].beltId;
					this.cargoTraffic.ConnectToSplitter(splitterId3, num11, 2, !flag5);
				}
				else
				{
					this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 2, false);
				}
				if (num12 > 0)
				{
					num12 = this.entityPool[num12].beltId;
					this.cargoTraffic.ConnectToSplitter(splitterId3, num12, 3, !flag6);
				}
				else
				{
					this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 3, false);
				}
			}
			else
			{
				this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 0, false);
				this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 1, false);
				this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 2, false);
				this.cargoTraffic.ConnectToSplitter(splitterId3, 0, 3, false);
			}
		}
		if (desc.isPowerGen)
		{
			int num13 = this.powerSystem.NewGeneratorComponent(entityId, desc);
			if (num13 != 0 && flag && desc.geothermal)
			{
				PowerGeneratorComponent[] genPool = this.powerSystem.genPool;
				PrebuildData prebuildData = this.prebuildPool[prebuildId];
				if (prebuildData.paramCount != 0)
				{
					int num14 = prebuildData.parameters[0];
					if (num14 != 0)
					{
						genPool[num13].baseRuinId = num14;
						Vector3 pos = this.entityPool[entityId].pos;
						Quaternion rot = this.entityPool[entityId].rot;
						genPool[num13].gthStrength = this.powerSystem.CalculateGeothermalStrength(pos, rot, num14);
						genPool[num13].warmupSpeed = 0.0027777778f;
					}
				}
			}
		}
		if (desc.isPowerConsumer)
		{
			pcId = this.powerSystem.NewConsumerComponent(entityId, desc.workEnergyPerTick, desc.idleEnergyPerTick);
		}
		if (desc.isMonitor)
		{
			int num15 = this.cargoTraffic.NewMonitorComponent(entityId);
			this.cargoTraffic.monitorPool[num15].pcId = pcId;
			this.EnableEntityWarning(entityId);
		}
		if (desc.isSpeaker)
		{
			int speakerId = this.digitalSystem.NewSpeakerComponent(entityId);
			if (desc.isMonitor)
			{
				this.cargoTraffic.monitorPool[this.entityPool[entityId].monitorId].speakerId = speakerId;
			}
		}
		if (desc.isSpraycoster)
		{
			int num16 = this.cargoTraffic.NewSpraycoaterComponent(entityId, desc.incCapacity);
			this.cargoTraffic.spraycoaterPool[num16].pcId = pcId;
		}
		if (desc.isPiler)
		{
			int num17 = this.cargoTraffic.NewPilerComponent(entityId);
			this.cargoTraffic.pilerPool[num17].pcId = pcId;
			if (flag)
			{
				bool flag7;
				int num18;
				int num19;
				this.ReadObjectConn(entityId, 0, out flag7, out num18, out num19);
				bool flag8;
				int num20;
				this.ReadObjectConn(entityId, 1, out flag8, out num20, out num19);
				if (num18 < 0)
				{
					num18 = 0;
				}
				if (num20 < 0)
				{
					num20 = 0;
				}
				if (num18 > 0)
				{
					num18 = this.entityPool[num18].beltId;
				}
				if (num20 > 0)
				{
					num20 = this.entityPool[num20].beltId;
				}
				this.cargoTraffic.RematchPilerConnection(num17);
			}
			else
			{
				this.cargoTraffic.RematchPilerConnection(num17);
			}
		}
		if (desc.isAccumulator)
		{
			int num21 = this.powerSystem.NewAccumulatorComponent(entityId, desc);
			ItemProto itemProto = LDB.items.Select((int)this.entityPool[entityId].protoId);
			if (itemProto != null && itemProto.HeatValue > 0L)
			{
				int modelIndex = itemProto.ModelIndex;
				this.powerSystem.accPool[num21].curEnergy = itemProto.HeatValue;
				itemProto = LDB.items.Select((int)(this.entityPool[entityId].protoId - 1));
				if (itemProto != null && itemProto.HeatValue == 0L && itemProto.ModelIndex == modelIndex)
				{
					EntityData[] array = this.entityPool;
					array[entityId].protoId = array[entityId].protoId - 1;
				}
			}
		}
		if (desc.isPowerExchanger)
		{
			int excId = this.powerSystem.NewExchangerComponent(entityId, desc);
			if (flag)
			{
				bool isOutput;
				int num22;
				int num23;
				this.ReadObjectConn(entityId, 0, out isOutput, out num22, out num23);
				bool isOutput2;
				int num24;
				this.ReadObjectConn(entityId, 1, out isOutput2, out num24, out num23);
				bool isOutput3;
				int num25;
				this.ReadObjectConn(entityId, 2, out isOutput3, out num25, out num23);
				bool isOutput4;
				int num26;
				this.ReadObjectConn(entityId, 3, out isOutput4, out num26, out num23);
				if (num22 < 0)
				{
					num22 = 0;
				}
				if (num24 < 0)
				{
					num24 = 0;
				}
				if (num25 < 0)
				{
					num25 = 0;
				}
				if (num26 < 0)
				{
					num26 = 0;
				}
				if (num22 > 0)
				{
					num22 = this.entityPool[num22].beltId;
					this.powerSystem.SetExchangerBelt(excId, num22, 0, isOutput);
				}
				else
				{
					this.powerSystem.SetExchangerBelt(excId, 0, 0, false);
				}
				if (num24 > 0)
				{
					num24 = this.entityPool[num24].beltId;
					this.powerSystem.SetExchangerBelt(excId, num24, 1, isOutput2);
				}
				else
				{
					this.powerSystem.SetExchangerBelt(excId, 0, 1, false);
				}
				if (num25 > 0)
				{
					num25 = this.entityPool[num25].beltId;
					this.powerSystem.SetExchangerBelt(excId, num25, 2, isOutput3);
				}
				else
				{
					this.powerSystem.SetExchangerBelt(excId, 0, 2, false);
				}
				if (num26 > 0)
				{
					num26 = this.entityPool[num26].beltId;
					this.powerSystem.SetExchangerBelt(excId, num26, 3, isOutput4);
				}
				else
				{
					this.powerSystem.SetExchangerBelt(excId, 0, 3, false);
				}
			}
			else
			{
				this.powerSystem.SetExchangerBelt(excId, 0, 0, false);
				this.powerSystem.SetExchangerBelt(excId, 0, 1, false);
				this.powerSystem.SetExchangerBelt(excId, 0, 2, false);
				this.powerSystem.SetExchangerBelt(excId, 0, 3, false);
			}
		}
		if (desc.isPowerNode)
		{
			this.powerSystem.NewNodeComponent(entityId, desc.powerConnectDistance, desc.powerCoverRadius);
		}
		if (desc.isStorage)
		{
			StorageComponent storageComponent = this.factoryStorage.NewStorageComponent(entityId, desc.storageCol * desc.storageRow);
			storageComponent.InitConn();
			if (flag)
			{
				bool flag9;
				int num27;
				int num28;
				this.ReadObjectConn(entityId, 13, out flag9, out num27, out num28);
				if (num27 < 0)
				{
					num27 = 0;
				}
				if (num27 > 0)
				{
					num27 = this.entityPool[num27].dispenserId;
					if (num27 > 0)
					{
						this.transport.ConnectToDispenser(num27, storageComponent.id);
					}
				}
			}
		}
		if (desc.minerType != EMinerType.None && desc.minerPeriod > 0)
		{
			num = this.factorySystem.NewMinerComponent(entityId, desc);
			if (num != 0 && flag)
			{
				MinerComponent[] minerPool = this.factorySystem.minerPool;
				ref PrebuildData ptr = ref this.prebuildPool[prebuildId];
				int filterId = ptr.filterId;
				Vector3 pos2 = ptr.pos;
				Quaternion rot2 = ptr.rot;
				Pose lPose = new Pose(pos2, rot2);
				int num29 = this.prebuildPool[prebuildId].paramCount;
				int num30 = 0;
				if (desc.isVeinCollector)
				{
					num30 = 2048;
					num29 = this.prebuildPool[prebuildId].paramCount - 2048;
				}
				if (filterId != 0)
				{
					for (int i = num30 + num29 - 1; i >= num30; i--)
					{
						int num31 = ptr.parameters[i];
						if (num31 == 0)
						{
							Assert.CannotBeReached();
						}
						else if (this.veinPool[num31].productId != 0 && this.veinPool[num31].productId == filterId)
						{
							Vector3 vector = this.veinPool[num31].pos;
							if (vector.magnitude < this.planet.realRadius - 40f)
							{
								vector = (vector - new Vector3(0f, 0f, 0f)).normalized * this.planet.realRadius;
							}
							if (!MinerComponent.IsTargetVeinInRange(vector, lPose, desc))
							{
								ptr.RemoveParameterFromArray(i);
								num29--;
							}
						}
						else
						{
							ptr.RemoveParameterFromArray(i);
							num29--;
						}
					}
				}
				minerPool[num].InitVeinArray(num29);
				if (num29 > 0)
				{
					Array.Copy(this.prebuildPool[prebuildId].parameters, num30, minerPool[num].veins, 0, num29);
				}
				for (int j = 0; j < minerPool[num].veinCount; j++)
				{
					this.RefreshVeinMiningDisplay(minerPool[num].veins[j], entityId, 0);
				}
				minerPool[num].ArrangeVeinArray();
				minerPool[num].pcId = pcId;
				minerPool[num].GetMinimumVeinAmount(this, this.veinPool);
				if (minerPool[num].type == EMinerType.Vein || minerPool[num].type == EMinerType.Oil)
				{
					int num32 = (minerPool[num].veinCount == 0) ? 0 : minerPool[num].veins[0];
					this.entitySignPool[entityId].iconId0 = (uint)((num32 == 0) ? this.prebuildPool[prebuildId].filterId : this.veinPool[num32].productId);
					this.entitySignPool[entityId].iconType = ((this.entitySignPool[entityId].iconId0 > 0U) ? 1U : 0U);
				}
				else
				{
					this.entitySignPool[entityId].iconId0 = (uint)this.planet.waterItemId;
					if (this.entitySignPool[entityId].iconId0 > 12000U)
					{
						this.entitySignPool[entityId].iconId0 = 0U;
					}
					this.entitySignPool[entityId].iconType = ((this.entitySignPool[entityId].iconId0 > 0U) ? 1U : 0U);
				}
				if (minerPool[num].type == EMinerType.Vein && !this.gameData.gameDesc.isInfiniteResource)
				{
					minerPool[num].GetTotalVeinAmount(this.veinPool);
				}
			}
		}
		if (desc.isInserter)
		{
			if (flag)
			{
				Vector3 pos3 = this.prebuildPool[prebuildId].pos2;
				double num33 = (double)((this.prebuildPool[prebuildId].paramCount > 0) ? this.prebuildPool[prebuildId].parameters[0] : 1);
				if (this.prebuildPool[prebuildId].paramCount > 1)
				{
					num33 = (double)this.prebuildPool[prebuildId].paramCount;
					if (num33 < 1.0)
					{
						num33 = 1.0;
					}
					if (num33 > 4.0)
					{
						num33 = 4.0;
					}
					this.prebuildPool[prebuildId].paramCount = 1;
					this.prebuildPool[prebuildId].parameters = new int[1];
					this.prebuildPool[prebuildId].parameters[0] = (int)(num33 + 0.499);
				}
				if (num33 < 1.0)
				{
					num33 = 1.0;
				}
				if (num33 > 4.0)
				{
					num33 = 4.0;
				}
				int num34 = (int)((double)desc.inserterSTT * num33 + 0.499);
				if (num34 < 10000)
				{
					num34 = 10000;
				}
				int num35 = this.factorySystem.NewInserterComponent(entityId, desc.inserterGrade, num34, this.prebuildPool[prebuildId].filterId);
				this.factorySystem.inserterPosePool[num35].pos2 = pos3;
				this.factorySystem.inserterPosePool[num35].rot2 = this.prebuildPool[prebuildId].rot2;
				this.factorySystem.inserterPool[num35].pcId = pcId;
				Assert.Positive(this.prebuildPool[prebuildId].paramCount);
			}
			else
			{
				int num36 = this.factorySystem.NewInserterComponent(entityId, desc.inserterGrade, desc.inserterSTT, 0);
				this.factorySystem.inserterPosePool[num36].pos2 = this.entityPool[entityId].pos + this.entityPool[entityId].rot * new Vector3(0f, 0f, 2f);
				this.factorySystem.inserterPosePool[num36].rot2 = this.entityPool[entityId].rot;
				this.factorySystem.inserterPool[num36].pcId = pcId;
			}
		}
		if (desc.isAssembler)
		{
			int num37 = this.factorySystem.NewAssemblerComponent(entityId, desc.assemblerSpeed);
			this.factorySystem.assemblerPool[num37].pcId = pcId;
		}
		if (desc.isFractionator)
		{
			int num38 = this.factorySystem.NewFractionatorComponent(entityId, desc);
			this.factorySystem.fractionatorPool[num38].pcId = pcId;
			if (flag)
			{
				bool isOutput5;
				int num39;
				int num40;
				this.ReadObjectConn(entityId, 0, out isOutput5, out num39, out num40);
				bool isOutput6;
				int num41;
				this.ReadObjectConn(entityId, 1, out isOutput6, out num41, out num40);
				bool isOutput7;
				int num42;
				this.ReadObjectConn(entityId, 2, out isOutput7, out num42, out num40);
				if (num39 < 0)
				{
					num39 = 0;
				}
				if (num41 < 0)
				{
					num41 = 0;
				}
				if (num42 < 0)
				{
					num42 = 0;
				}
				if (num39 > 0)
				{
					num39 = this.entityPool[num39].beltId;
					this.factorySystem.SetFractionatorBelt(num38, num39, 0, isOutput5);
				}
				else
				{
					this.factorySystem.SetFractionatorBelt(num38, 0, 0, false);
				}
				if (num41 > 0)
				{
					num41 = this.entityPool[num41].beltId;
					this.factorySystem.SetFractionatorBelt(num38, num41, 1, isOutput6);
				}
				else
				{
					this.factorySystem.SetFractionatorBelt(num38, 0, 1, false);
				}
				if (num42 > 0)
				{
					num42 = this.entityPool[num42].beltId;
					this.factorySystem.SetFractionatorBelt(num38, num42, 2, isOutput7);
				}
				else
				{
					this.factorySystem.SetFractionatorBelt(num38, 0, 2, false);
				}
			}
			else
			{
				this.factorySystem.SetFractionatorBelt(num38, 0, 0, false);
				this.factorySystem.SetFractionatorBelt(num38, 0, 1, false);
				this.factorySystem.SetFractionatorBelt(num38, 0, 2, false);
			}
		}
		if (desc.isEjector)
		{
			int num43 = this.factorySystem.NewEjectorComponent(entityId, desc);
			this.factorySystem.ejectorPool[num43].pcId = pcId;
		}
		if (desc.isSilo)
		{
			int num44 = this.factorySystem.NewSiloComponent(entityId, desc);
			this.factorySystem.siloPool[num44].pcId = pcId;
		}
		if (desc.isLab)
		{
			int num45 = this.factorySystem.NewLabComponent(entityId, desc.labAssembleSpeed);
			this.factorySystem.labPool[num45].pcId = pcId;
		}
		if (desc.isTank)
		{
			int tankId3 = this.factoryStorage.NewTankComponent(entityId, desc.fluidStorageCount);
			if (flag)
			{
				bool isOutput8;
				int num46;
				int num47;
				this.ReadObjectConn(entityId, 0, out isOutput8, out num46, out num47);
				bool isOutput9;
				int num48;
				this.ReadObjectConn(entityId, 1, out isOutput9, out num48, out num47);
				bool isOutput10;
				int num49;
				this.ReadObjectConn(entityId, 2, out isOutput10, out num49, out num47);
				bool isOutput11;
				int num50;
				this.ReadObjectConn(entityId, 3, out isOutput11, out num50, out num47);
				if (num46 < 0)
				{
					num46 = 0;
				}
				if (num48 < 0)
				{
					num48 = 0;
				}
				if (num49 < 0)
				{
					num49 = 0;
				}
				if (num50 < 0)
				{
					num50 = 0;
				}
				if (num46 > 0)
				{
					num46 = this.entityPool[num46].beltId;
					this.factoryStorage.SetTankBelt(tankId3, num46, 0, isOutput8);
				}
				else
				{
					this.factoryStorage.SetTankBelt(tankId3, 0, 0, false);
				}
				if (num48 > 0)
				{
					num48 = this.entityPool[num48].beltId;
					this.factoryStorage.SetTankBelt(tankId3, num48, 1, isOutput9);
				}
				else
				{
					this.factoryStorage.SetTankBelt(tankId3, 0, 1, false);
				}
				if (num49 > 0)
				{
					num49 = this.entityPool[num49].beltId;
					this.factoryStorage.SetTankBelt(tankId3, num49, 2, isOutput10);
				}
				else
				{
					this.factoryStorage.SetTankBelt(tankId3, 0, 2, false);
				}
				if (num50 > 0)
				{
					num50 = this.entityPool[num50].beltId;
					this.factoryStorage.SetTankBelt(tankId3, num50, 3, isOutput11);
				}
				else
				{
					this.factoryStorage.SetTankBelt(tankId3, 0, 3, false);
				}
			}
			else
			{
				this.factoryStorage.SetTankBelt(tankId3, 0, 0, false);
				this.factoryStorage.SetTankBelt(tankId3, 0, 1, false);
				this.factoryStorage.SetTankBelt(tankId3, 0, 2, false);
				this.factoryStorage.SetTankBelt(tankId3, 0, 3, false);
			}
		}
		if (desc.isStation)
		{
			StationComponent stationComponent = this.transport.NewStationComponent(entityId, pcId, desc);
			if (desc.isVeinCollector)
			{
				stationComponent.minerId = num;
				MinerComponent[] minerPool2 = this.factorySystem.minerPool;
				if (minerPool2[num].veins == null || minerPool2[num].veins[0] == 0)
				{
					stationComponent.storage[0].itemId = this.prebuildPool[prebuildId].filterId;
					stationComponent.collectionIds[0] = stationComponent.storage[0].itemId;
				}
				else
				{
					stationComponent.storage[0].itemId = this.veinPool[minerPool2[num].veins[0]].productId;
					stationComponent.collectionIds[0] = stationComponent.storage[0].itemId;
				}
			}
		}
		if (desc.isDispenser)
		{
			int dispenserId = this.transport.NewDispenserComponent(entityId, pcId, desc);
			if (flag)
			{
				bool flag10;
				int num51;
				int num52;
				this.ReadObjectConn(entityId, 0, out flag10, out num51, out num52);
				if (num51 < 0)
				{
					num51 = 0;
				}
				if (num51 > 0)
				{
					num51 = this.entityPool[num51].storageId;
					this.transport.ConnectToDispenser(dispenserId, num51);
				}
				else
				{
					this.transport.ConnectToDispenser(dispenserId, 0);
				}
			}
			else
			{
				this.transport.ConnectToDispenser(dispenserId, 0);
			}
		}
		if (desc.isMarker)
		{
			this.digitalSystem.NewMarkerComponent(entityId, desc);
		}
		if (desc.isTurret)
		{
			this.defenseSystem.NewTurretComponent(entityId, desc);
		}
		if (desc.isBeacon)
		{
			this.defenseSystem.NewBeaconComponent(entityId, desc);
		}
		if (desc.isFieldGenerator)
		{
			this.defenseSystem.NewFieldGeneratorComponent(entityId, desc);
		}
		if (desc.isConstructionModule)
		{
			int constructionModuleId = this.constructionSystem.NewConstructionModuleComponent(entityId, desc);
			this.entityPool[entityId].constructionModuleId = constructionModuleId;
		}
		if (desc.isCombatModule)
		{
			int combatModuleId = this.combatGroundSystem.NewCombatModuleComponent(entityId, desc);
			this.entityPool[entityId].combatModuleId = combatModuleId;
		}
		if (desc.isBattleBase)
		{
			int battleBaseId = this.defenseSystem.NewBattleBaseComponent(entityId, desc);
			this.entityPool[entityId].battleBaseId = battleBaseId;
			if (this.entityPool[entityId].storageId > 0)
			{
				this.factoryStorage.storagePool[this.entityPool[entityId].storageId].SetBans(10);
			}
		}
		this.entityAnimPool[entityId].time = 0f;
		this.entityAnimPool[entityId].prepare_length = desc.anim_prepare_length;
		this.entityAnimPool[entityId].working_length = desc.anim_working_length;
		this.entityAnimPool[entityId].state = 0U;
		this.entityAnimPool[entityId].power = 0f;
		for (int k = 0; k < 16; k++)
		{
			bool flag11;
			int num53;
			int otherSlotId;
			this.ReadObjectConn(entityId, k, out flag11, out num53, out otherSlotId);
			if (num53 > 0)
			{
				if (flag11)
				{
					this.ApplyEntityOutput(entityId, num53, k, otherSlotId, (int)this.prebuildPool[prebuildId].insertOffset);
				}
				else
				{
					this.ApplyEntityInput(entityId, num53, k, otherSlotId, (int)this.prebuildPool[prebuildId].pickOffset);
				}
			}
		}
		if (flag)
		{
			BuildingParameters.ApplyPrebuildParametersToEntity(entityId, this.prebuildPool[prebuildId].recipeId, this.prebuildPool[prebuildId].filterId, this.prebuildPool[prebuildId].parameters, this.prebuildPool[prebuildId].content, this);
			if (this.prebuildPool[prebuildId].isWarningSetted)
			{
				this.EnableEntityWarning(entityId);
			}
			this.WriteExtraInfoOnEntity(entityId, this.ReadExtraInfoOnPrebuild(prebuildId));
		}
		if (desc.isLab)
		{
			this.factorySystem.FindLabFunctionsForBuild(this.entityPool[entityId].labId);
			this.factorySystem.SyncLabFunctions(this.gameData.mainPlayer, this.entityPool[entityId].labId);
			this.factorySystem.SyncLabForceAccMode(this.gameData.mainPlayer, this.entityPool[entityId].labId);
		}
		if (desc.isSpraycoster)
		{
			this.cargoTraffic.spraycoaterPool[this.entityPool[entityId].spraycoaterId].Reshape(this, this.entityAnimPool);
		}
		if (desc.isStorage)
		{
			bool flag11;
			int num53;
			int otherSlotId;
			this.ReadObjectConn(entityId, 14, out flag11, out num53, out otherSlotId);
			if (num53 > 0)
			{
				this.entityAnimPool[entityId].state = ((this.entityPool[num53].protoId == 3009) ? 2U : 0U);
			}
			else if (num53 < 0)
			{
				this.entityAnimPool[entityId].state = ((this.prebuildPool[-num53].protoId == 3009) ? 2U : 0U);
			}
		}
		if (desc.isPowerExchanger)
		{
			this.entityAnimPool[entityId].time = 2f;
		}
		if (desc.isMarker)
		{
			int warningId = this.entityPool[entityId].warningId;
			if (warningId > 0)
			{
				ref WarningData ptr2 = ref this.gameData.warningSystem.warningPool[warningId];
				ptr2.signalId = 2401;
				ptr2.localPos = this.entityPool[entityId].pos * (1f + 1.3f / this.planet.realRadius);
				ptr2.state = 1;
				int markerId = this.entityPool[entityId].markerId;
				if (markerId > 0)
				{
					ptr2.detailId1 = this.digitalSystem.markers[markerId].icon;
				}
			}
		}
		this.EntityAutoReplenishIfNeeded(entityId, Vector2.zero, false);
		if (desc.isPowerConsumer && this.planet.factoryLoaded)
		{
			this.planet.factoryModel.RefreshPowerConsumers();
		}
	}

	// Token: 0x0600183A RID: 6202 RVA: 0x0019D894 File Offset: 0x0019BA94
	public int AddEntityDataWithComponents(EntityData entity, int prebuildId)
	{
		ItemProto itemProto = LDB.items.Select((int)entity.protoId);
		if (itemProto == null || !itemProto.IsEntity)
		{
			return 0;
		}
		entity.simpleHash.InitHashBits(entity.pos.x, entity.pos.y, entity.pos.z);
		int num = this.AddEntityData(entity);
		if (entity.modelIndex == 0)
		{
			this.entityPool[num].modelIndex = (short)itemProto.ModelIndex;
		}
		ModelProto modelProto = LDB.models.modelArray[(int)this.entityPool[num].modelIndex];
		PrefabDesc prefabDesc = null;
		if (modelProto != null)
		{
			prefabDesc = modelProto.prefabDesc;
		}
		if (prefabDesc == null)
		{
			return num;
		}
		if (prebuildId > 0)
		{
			this.HandleObjectConnChangeWhenBuild(-prebuildId, num);
		}
		this.CreateEntityLogicComponents(num, prefabDesc, prebuildId);
		if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 4)
		{
			this.CreateEntityDisplayComponents(num);
		}
		return num;
	}

	// Token: 0x0600183B RID: 6203 RVA: 0x0019D97C File Offset: 0x0019BB7C
	public void RemoveEntityWithComponents(int id, bool returnItems)
	{
		bool flag = false;
		if (id != 0 && this.entityPool[id].id != 0)
		{
			for (int i = 0; i < 16; i++)
			{
				bool flag2;
				int num;
				int otherSlotId;
				this.ReadObjectConn(id, i, out flag2, out num, out otherSlotId);
				if (num > 0)
				{
					this.ApplyEntityDisconnection(num, id, otherSlotId, i);
				}
			}
			if (this.entityPool[id].beltId != 0)
			{
				this.cargoTraffic._return_items = returnItems;
				this.cargoTraffic.RemoveBeltComponent(this.entityPool[id].beltId);
				this.cargoTraffic._return_items = true;
				this.entityPool[id].beltId = 0;
			}
			if (this.entityPool[id].splitterId != 0)
			{
				this.cargoTraffic.RemoveSplitterComponent(this.entityPool[id].splitterId);
				this.entityPool[id].splitterId = 0;
			}
			if (this.entityPool[id].monitorId != 0)
			{
				this.cargoTraffic.RemoveMonitorComponent(this.entityPool[id].monitorId);
				this.entityPool[id].monitorId = 0;
			}
			if (this.entityPool[id].spraycoaterId != 0)
			{
				this.cargoTraffic.RemoveSpraycoaterComponent(this.entityPool[id].spraycoaterId);
				this.entityPool[id].spraycoaterId = 0;
			}
			if (this.entityPool[id].pilerId != 0)
			{
				this.cargoTraffic.RemovePilerComponent(this.entityPool[id].pilerId);
				this.entityPool[id].pilerId = 0;
			}
			if (this.entityPool[id].storageId != 0)
			{
				this.factoryStorage.RemoveStorageComponent(this.entityPool[id].storageId);
				this.entityPool[id].storageId = 0;
			}
			if (this.entityPool[id].tankId != 0)
			{
				this.factoryStorage.RemoveTankComponent(this.entityPool[id].tankId);
			}
			if (this.entityPool[id].minerId != 0)
			{
				this.factorySystem.RemoveMinerComponent(this.entityPool[id].minerId);
			}
			if (this.entityPool[id].inserterId != 0)
			{
				this.factorySystem.RemoveInserterComponent(this.entityPool[id].inserterId);
			}
			if (this.entityPool[id].assemblerId != 0)
			{
				this.factorySystem.RemoveAssemblerComponent(this.entityPool[id].assemblerId);
			}
			if (this.entityPool[id].fractionatorId != 0)
			{
				this.factorySystem.RemoveFractionatorComponent(this.entityPool[id].fractionatorId);
			}
			if (this.entityPool[id].ejectorId != 0)
			{
				this.factorySystem.RemoveEjectorComponent(this.entityPool[id].ejectorId);
			}
			if (this.entityPool[id].siloId != 0)
			{
				this.factorySystem.RemoveSiloComponent(this.entityPool[id].siloId);
			}
			if (this.entityPool[id].labId != 0)
			{
				this.factorySystem.RemoveLabComponent(this.entityPool[id].labId);
			}
			if (this.entityPool[id].stationId != 0)
			{
				this.transport.RemoveStationComponent(this.entityPool[id].stationId);
				this.entityPool[id].stationId = 0;
			}
			if (this.entityPool[id].dispenserId != 0)
			{
				this.transport.RemoveDispenserComponent(this.entityPool[id].dispenserId);
			}
			if (this.entityPool[id].turretId != 0)
			{
				this.defenseSystem.RemoveTurretComponent(this.entityPool[id].turretId);
			}
			if (this.entityPool[id].beaconId != 0)
			{
				this.defenseSystem.RemoveBeaconComponent(this.entityPool[id].beaconId);
			}
			if (this.entityPool[id].fieldGenId != 0)
			{
				this.defenseSystem.RemoveFieldGeneratorComponent(this.entityPool[id].fieldGenId);
			}
			if (this.entityPool[id].constructionModuleId != 0)
			{
				this.constructionSystem.RemoveConstructionModuleComponent(this.entityPool[id].constructionModuleId);
			}
			if (this.entityPool[id].combatModuleId != 0)
			{
				this.combatGroundSystem.RemoveCombatModuleComponent(this.entityPool[id].combatModuleId);
			}
			if (this.entityPool[id].battleBaseId != 0)
			{
				this.defenseSystem.RemoveBattleBaseComponent(this.entityPool[id].battleBaseId);
			}
			if (this.entityPool[id].powerNodeId != 0)
			{
				this.powerSystem.RemoveNodeComponent(this.entityPool[id].powerNodeId);
			}
			if (this.entityPool[id].powerGenId != 0)
			{
				this.powerSystem.RemoveGeneratorComponent(this.entityPool[id].powerGenId);
			}
			if (this.entityPool[id].powerAccId != 0)
			{
				this.powerSystem.RemoveAccumulatorComponent(this.entityPool[id].powerAccId);
			}
			if (this.entityPool[id].powerExcId != 0)
			{
				this.powerSystem.RemoveExchangerComponent(this.entityPool[id].powerExcId);
			}
			if (this.entityPool[id].powerConId != 0)
			{
				this.powerSystem.RemoveConsumerComponent(this.entityPool[id].powerConId);
			}
			if (this.entityPool[id].speakerId != 0)
			{
				this.digitalSystem.RemoveSpeakerComponent(this.entityPool[id].speakerId);
				this.entityPool[id].speakerId = 0;
			}
			if (this.entityPool[id].warningId != 0)
			{
				this.gameData.warningSystem.RemoveWarningData(this.entityPool[id].warningId);
				this.entityPool[id].warningId = 0;
			}
			if (this.entityPool[id].extraInfoId != 0)
			{
				this.digitalSystem.RemoveExtraInfoComponent(this.entityPool[id].extraInfoId);
				this.entityPool[id].extraInfoId = 0;
			}
			if (this.entityPool[id].combatStatId != 0)
			{
				int combatStatId = this.entityPool[id].combatStatId;
				int warningId = this.skillSystem.combatStats.buffer[combatStatId].warningId;
				if (warningId > 0)
				{
					this.gameData.warningSystem.RemoveWarningData(warningId);
				}
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(combatStatId);
				this.entityPool[id].combatStatId = 0;
			}
			if (this.entityPool[id].constructStatId != 0)
			{
				this.constructionSystem.RemoveConstructStat(this.entityPool[id].constructStatId);
				this.entityPool[id].constructStatId = 0;
			}
			if (this.entityPool[id].markerId != 0)
			{
				this.digitalSystem.RemoveMarkerComponent(this.entityPool[id].markerId);
				this.entityPool[id].markerId = 0;
			}
			if (this.entityPool[id].modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemoveModel((int)this.entityPool[id].modelIndex, this.entityPool[id].modelId, true);
				}
				this.entityPool[id].modelId = 0;
			}
			if (this.entityPool[id].mmblockId != 0)
			{
				if (this.blockContainer != null)
				{
					this.blockContainer.RemoveMiniBlock(this.entityPool[id].mmblockId);
				}
				this.entityPool[id].mmblockId = 0;
			}
			if (this.entityPool[id].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.entityPool[id].colliderId);
				}
				this.entityPool[id].colliderId = 0;
			}
			if (this.entityPool[id].audioId != 0)
			{
				flag = true;
				if (this.planet.audio != null)
				{
					this.planet.audio.RemoveAudioData(this.entityPool[id].audioId);
				}
				this.entityPool[id].audioId = 0;
			}
			this.hashSystemStatic.RemoveObjectFromBucket(this.entityPool[id].hashAddress);
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.None);
			this.entityPool[id].SetNull();
			this.entityAnimPool[id].time = 0f;
			this.entityAnimPool[id].prepare_length = 0f;
			this.entityAnimPool[id].working_length = 0f;
			this.entityAnimPool[id].state = 0U;
			this.entityAnimPool[id].power = 0f;
			this.entitySignPool[id].SetEmpty();
			this.ClearObjectConn(id);
			Array.Clear(this.entityConnPool, id * 16, 16);
			this.entityNeeds[id] = null;
			int[] array = this.entityRecycle;
			int num2 = this.entityRecycleCursor;
			this.entityRecycleCursor = num2 + 1;
			array[num2] = id;
		}
		if (this.planet.physics != null)
		{
			this.planet.physics.NotifyObjectRemove(EObjectType.None, id);
		}
		if (flag && this.planet.audio != null)
		{
			this.planet.audio.NotifyObjectRemove(EObjectType.None, id);
		}
	}

	// Token: 0x0600183C RID: 6204 RVA: 0x0019E3E4 File Offset: 0x0019C5E4
	public void UpgradeEntityWithComponents(int entityId, ItemProto newProto)
	{
		if (entityId != 0 && this.entityPool[entityId].id != 0)
		{
			if (this.entityPool[entityId].modelId != 0)
			{
				GameMain.gpuiManager.RemoveModel((int)this.entityPool[entityId].modelIndex, this.entityPool[entityId].modelId, true);
				this.entityPool[entityId].modelId = 0;
			}
			if (this.entityPool[entityId].mmblockId != 0)
			{
				this.blockContainer.RemoveMiniBlock(this.entityPool[entityId].mmblockId);
				this.entityPool[entityId].mmblockId = 0;
			}
			if (this.entityPool[entityId].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.entityPool[entityId].colliderId);
				}
				this.entityPool[entityId].colliderId = 0;
			}
			if (this.entityPool[entityId].audioId != 0)
			{
				if (this.planet.audio != null)
				{
					this.planet.audio.RemoveAudioData(this.entityPool[entityId].audioId);
				}
				this.entityPool[entityId].audioId = 0;
			}
			int beltId = this.entityPool[entityId].beltId;
			if (beltId > 0)
			{
				this.cargoTraffic.UpgradeBeltComponent(beltId, newProto.prefabDesc.beltSpeed);
			}
			int assemblerId = this.entityPool[entityId].assemblerId;
			if (assemblerId > 0)
			{
				this.factorySystem.assemblerPool[assemblerId].speed = newProto.prefabDesc.assemblerSpeed;
				this.factorySystem.assemblerPool[assemblerId].speedOverride = newProto.prefabDesc.assemblerSpeed;
				this.factorySystem.assemblerPool[assemblerId].time = 0;
				this.factorySystem.assemblerPool[assemblerId].extraTime = 0;
				this.entityAnimPool[entityId].prepare_length = newProto.prefabDesc.anim_prepare_length;
				this.entityAnimPool[entityId].working_length = newProto.prefabDesc.anim_working_length;
				this.entityAnimPool[entityId].time = 0f;
			}
			int labId = this.entityPool[entityId].labId;
			if (labId > 0)
			{
				this.factorySystem.labPool[labId].speed = newProto.prefabDesc.labAssembleSpeed;
				this.factorySystem.labPool[labId].speedOverride = newProto.prefabDesc.labAssembleSpeed;
				this.factorySystem.labPool[labId].time = 0;
				this.factorySystem.labPool[labId].extraTime = 0;
				this.entityAnimPool[entityId].prepare_length = newProto.prefabDesc.anim_prepare_length;
				this.entityAnimPool[entityId].working_length = newProto.prefabDesc.anim_working_length;
				this.entityAnimPool[entityId].time = 0f;
			}
			int powerConId = this.entityPool[entityId].powerConId;
			if (powerConId > 0)
			{
				this.powerSystem.consumerPool[powerConId].idleEnergyPerTick = newProto.prefabDesc.idleEnergyPerTick;
				this.powerSystem.consumerPool[powerConId].workEnergyPerTick = newProto.prefabDesc.workEnergyPerTick;
			}
			int inserterId = this.entityPool[entityId].inserterId;
			if (inserterId > 0)
			{
				int grade = newProto.Grade;
				bool flag = grade >= 3;
				int num = 1;
				int num2 = 1;
				bool bidirectional = false;
				bool flag2 = grade > 3;
				if (flag)
				{
					num = (flag2 ? GameMain.history.inserterStackInput : GameMain.history.inserterStackCountObsolete);
					num2 = (flag2 ? GameMain.history.inserterStackOutput : 1);
					bidirectional = (flag2 && GameMain.history.inserterBidirectional);
				}
				int delay = (num > 1) ? 40000 : 0;
				ItemProto itemProto = LDB.items.Select((int)this.entityPool[entityId].protoId);
				int num3 = this.factorySystem.inserterPool[inserterId].stt / itemProto.prefabDesc.inserterSTT;
				double num4 = (double)this.factorySystem.inserterPool[inserterId].time / (double)this.factorySystem.inserterPool[inserterId].stt;
				this.factorySystem.inserterPool[inserterId].stt = newProto.prefabDesc.inserterSTT * num3;
				this.factorySystem.inserterPool[inserterId].canStack = newProto.prefabDesc.inserterCanStack;
				this.factorySystem.inserterPool[inserterId].grade = (byte)grade;
				this.factorySystem.inserterPool[inserterId].stackInput = (byte)num;
				this.factorySystem.inserterPool[inserterId].stackOutput = (byte)num2;
				this.factorySystem.inserterPool[inserterId].bidirectional = bidirectional;
				this.factorySystem.inserterPool[inserterId].time = (int)(num4 * (double)this.factorySystem.inserterPool[inserterId].stt + 0.5);
				this.factorySystem.inserterPool[inserterId].delay = delay;
			}
			this.entityPool[entityId].protoId = (short)newProto.ID;
			this.entityPool[entityId].modelIndex = (short)newProto.ModelIndex;
			if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 4)
			{
				this.CreateEntityDisplayComponents(entityId);
			}
			if (beltId > 0 && this.planet.physics != null)
			{
				this.cargoTraffic.AlterBeltRenderer(beltId, this.entityPool, this.planet.physics.colChunks, false);
				this.cargoTraffic.RefreshBeltBatchesBuffers();
			}
		}
	}

	// Token: 0x0600183D RID: 6205 RVA: 0x0019EA12 File Offset: 0x0019CC12
	public EntityData GetEntityData(int id)
	{
		return this.entityPool[id];
	}

	// Token: 0x0600183E RID: 6206 RVA: 0x0019EA20 File Offset: 0x0019CC20
	public void DebugEntityGUI()
	{
		DGUI.Pool("Entities", this.entityPool, this.entityCapacity, this.entityCursor, this.entityRecycle, 0, this.entityRecycleCursor);
	}

	// Token: 0x0600183F RID: 6207 RVA: 0x0019EA4C File Offset: 0x0019CC4C
	private void SetCraftCapacity(int newCapacity)
	{
		CraftData[] array = this.craftPool;
		this.craftPool = new CraftData[newCapacity];
		this.craftRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.craftPool, (newCapacity > this.craftCapacity) ? this.craftCapacity : newCapacity);
		}
		AnimData[] array2 = this.craftAnimPool;
		this.craftAnimPool = new AnimData[newCapacity];
		if (array2 != null)
		{
			Array.Copy(array2, this.craftAnimPool, (newCapacity > this.craftCapacity) ? this.craftCapacity : newCapacity);
		}
		this.craftCapacity = newCapacity;
	}

	// Token: 0x06001840 RID: 6208 RVA: 0x0019EAD4 File Offset: 0x0019CCD4
	public int AddCraftData(ref CraftData craft)
	{
		if (this.craftRecycleCursor > 0)
		{
			int[] array = this.craftRecycle;
			int num = this.craftRecycleCursor - 1;
			this.craftRecycleCursor = num;
			craft.id = array[num];
		}
		else
		{
			int num = this.craftCursor;
			this.craftCursor = num + 1;
			craft.id = num;
		}
		if (craft.id == this.craftCapacity)
		{
			this.SetCraftCapacity(this.craftCapacity * 2);
		}
		this.craftPool[craft.id] = craft;
		if (craft.dynamic)
		{
			this.craftPool[craft.id].hashAddress = this.hashSystemDynamic.AddObjectToBucket(craft.id, craft.pos, EObjectType.Craft);
		}
		else
		{
			this.craftPool[craft.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(craft.id, craft.pos, EObjectType.Craft);
		}
		return craft.id;
	}

	// Token: 0x06001841 RID: 6209 RVA: 0x0019EBCC File Offset: 0x0019CDCC
	public void CreateCraftDisplayComponents(int craftId)
	{
		int modelIndex = (int)this.craftPool[craftId].modelIndex;
		ModelProto modelProto = LDB.models.modelArray[modelIndex];
		if (modelProto == null)
		{
			return;
		}
		PrefabDesc prefabDesc = modelProto.prefabDesc;
		if (prefabDesc == null)
		{
			return;
		}
		this.craftPool[craftId].modelId = GameMain.gpuiManager.AddModel(modelIndex, craftId, this.craftPool[craftId].pos, this.craftPool[craftId].rot, true);
		if (prefabDesc.minimapType > 0 && this.craftPool[craftId].mmblockId == 0)
		{
			this.craftPool[craftId].mmblockId = this.blockContainer.AddMiniBlock(craftId, prefabDesc.minimapType, this.craftPool[craftId].pos, this.craftPool[craftId].rot, prefabDesc.minimapScl);
		}
		if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
		{
			for (int i = 0; i < prefabDesc.colliders.Length; i++)
			{
				this.craftPool[craftId].colliderId = this.planet.physics.AddColliderData(prefabDesc.colliders[i].BindToObject(craftId, this.craftPool[craftId].colliderId, EObjectType.Craft, this.craftPool[craftId].pos, this.craftPool[craftId].rot));
			}
		}
		if (prefabDesc.hasAudio)
		{
			this.craftPool[craftId].audioId = this.planet.audio.AddAudioData(craftId, EObjectType.Craft, this.craftPool[craftId].pos, prefabDesc);
		}
	}

	// Token: 0x06001842 RID: 6210 RVA: 0x0019ED90 File Offset: 0x0019CF90
	public void CreateCraftLogicComponents(int craftId, PrefabDesc desc)
	{
		if (desc.isConstructionDrone)
		{
			this.constructionSystem.NewDroneComponent(craftId, desc);
		}
		if (desc.isFleet)
		{
			this.combatGroundSystem.NewFleetComponent(craftId, desc);
		}
		if (desc.isCraftUnit)
		{
			this.combatGroundSystem.NewUnitComponent(craftId, desc);
		}
		this.craftAnimPool[craftId].time = 0f;
		this.craftAnimPool[craftId].prepare_length = desc.anim_prepare_length;
		this.craftAnimPool[craftId].working_length = desc.anim_working_length;
		this.craftAnimPool[craftId].state = 0U;
		this.craftAnimPool[craftId].power = 0f;
	}

	// Token: 0x06001843 RID: 6211 RVA: 0x0019EE4C File Offset: 0x0019D04C
	public int AddCraftDataWithComponents(ref CraftData craft)
	{
		int num = this.AddCraftData(ref craft);
		PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)craft.modelIndex];
		if (prefabDesc == null)
		{
			return num;
		}
		this.CreateCraftLogicComponents(num, prefabDesc);
		if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 5)
		{
			this.CreateCraftDisplayComponents(num);
		}
		this.SetupReferenceOnCraftCreate(num);
		return num;
	}

	// Token: 0x06001844 RID: 6212 RVA: 0x0019EEA8 File Offset: 0x0019D0A8
	public void RemoveCraftWithComponents(int id)
	{
		bool flag = false;
		if (id != 0 && this.craftPool[id].id != 0)
		{
			this.ClearReferencesOnCraftRemove(id);
			ref CraftData ptr = ref this.craftPool[id];
			if (ptr.droneId != 0)
			{
				this.constructionSystem.RemoveDroneComponent(ptr.droneId);
				ptr.droneId = 0;
			}
			if (ptr.combatStatId != 0)
			{
				int combatStatId = ptr.combatStatId;
				int warningId = this.skillSystem.combatStats.buffer[combatStatId].warningId;
				if (warningId > 0)
				{
					this.gameData.warningSystem.RemoveWarningData(warningId);
				}
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(ptr.combatStatId);
				ptr.combatStatId = 0;
			}
			if (ptr.unitId != 0)
			{
				this.combatGroundSystem.RemoveUnitComponent(ptr.unitId);
				ptr.unitId = 0;
			}
			if (ptr.fleetId != 0)
			{
				this.combatGroundSystem.RemoveFleetComponent(ptr.fleetId);
				ptr.fleetId = 0;
			}
			if (ptr.modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemoveModel((int)ptr.modelIndex, ptr.modelId, true);
				}
				ptr.modelId = 0;
			}
			if (ptr.mmblockId != 0)
			{
				if (this.blockContainer != null)
				{
					this.blockContainer.RemoveMiniBlock(ptr.mmblockId);
				}
				ptr.mmblockId = 0;
			}
			if (ptr.colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(ptr.colliderId);
				}
				ptr.colliderId = 0;
			}
			if (ptr.audioId != 0)
			{
				flag = true;
				if (this.planet.audio != null)
				{
					this.planet.audio.RemoveAudioData(ptr.audioId);
				}
				ptr.audioId = 0;
			}
			if (ptr.dynamic)
			{
				this.hashSystemDynamic.RemoveObjectFromBucket(ptr.hashAddress);
			}
			else
			{
				this.hashSystemStatic.RemoveObjectFromBucket(ptr.hashAddress);
			}
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.Craft);
			ptr.SetEmpty();
			this.craftAnimPool[id].time = 0f;
			this.craftAnimPool[id].prepare_length = 0f;
			this.craftAnimPool[id].working_length = 0f;
			this.craftAnimPool[id].state = 0U;
			this.craftAnimPool[id].power = 0f;
			int[] array = this.craftRecycle;
			int num = this.craftRecycleCursor;
			this.craftRecycleCursor = num + 1;
			array[num] = id;
		}
		if (this.planet.physics != null)
		{
			this.planet.physics.NotifyObjectRemove(EObjectType.Craft, id);
		}
		if (flag && this.planet.audio != null)
		{
			this.planet.audio.NotifyObjectRemove(EObjectType.Craft, id);
		}
	}

	// Token: 0x06001845 RID: 6213 RVA: 0x0019F19F File Offset: 0x0019D39F
	public CraftData GetCraftData(int id)
	{
		return this.craftPool[id];
	}

	// Token: 0x06001846 RID: 6214 RVA: 0x0019F1B0 File Offset: 0x0019D3B0
	public void SetupReferenceOnCraftCreate(int newCraftId)
	{
		ref CraftData ptr = ref this.craftPool[newCraftId];
		if (ptr.droneId > 0)
		{
			ptr.isInvincible = true;
		}
		if (ptr.fleetId > 0)
		{
			ptr.isInvincible = true;
		}
		if (ptr.unitId > 0 && ptr.owner > 0)
		{
			UnitComponent[] buffer = this.combatGroundSystem.units.buffer;
			int unitId = ptr.unitId;
			ref CraftData ptr2 = ref this.craftPool[ptr.owner];
			if (ptr2.owner == -1)
			{
				this.gameData.mainPlayer.mecha.groundCombatModule.moduleFleets[(int)ptr2.port].fighters[(int)ptr.port].craftId = ptr.id;
				return;
			}
			if (ptr2.owner > 0)
			{
				ref EntityData ptr3 = ref this.entityPool[ptr2.owner];
				if (ptr3.id == ptr2.owner)
				{
					if (ptr3.combatModuleId > 0)
					{
						CombatModuleComponent combatModuleComponent = this.combatGroundSystem.combatModules.buffer[ptr3.combatModuleId];
						if (combatModuleComponent != null && combatModuleComponent.id == ptr3.combatModuleId)
						{
							ModuleFleet[] moduleFleets = combatModuleComponent.moduleFleets;
							if (moduleFleets != null)
							{
								ModuleFighter[] fighters = moduleFleets[(int)ptr2.port].fighters;
								if (fighters != null)
								{
									fighters[(int)ptr.port].craftId = ptr.id;
								}
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x06001847 RID: 6215 RVA: 0x0019F334 File Offset: 0x0019D534
	public void ClearReferencesOnCraftRemove(int removingCraftId)
	{
		ref CraftData ptr = ref this.craftPool[removingCraftId];
		if (ptr.unitId > 0 && ptr.owner > 0)
		{
			ref UnitComponent ptr2 = ref this.combatGroundSystem.units.buffer[ptr.unitId];
			ref CraftData ptr3 = ref this.craftPool[ptr.owner];
			if (ptr2.behavior == EUnitBehavior.Initialize)
			{
				FleetComponent[] buffer = this.combatGroundSystem.fleets.buffer;
				int fleetId = ptr3.fleetId;
				buffer[fleetId].currentAssembleUnitsCount = buffer[fleetId].currentAssembleUnitsCount - 1;
			}
			if (ptr2.isCharging)
			{
				ptr2.isCharging = false;
				FleetComponent[] buffer2 = this.combatGroundSystem.fleets.buffer;
				int fleetId2 = ptr3.fleetId;
				buffer2[fleetId2].currentChargingUnitsCount = buffer2[fleetId2].currentChargingUnitsCount - 1;
			}
			if (ptr3.owner == -1)
			{
				if (!ptr.isSpace)
				{
					this.gameData.mainPlayer.mecha.groundCombatModule.NotifyFighterRemoved((int)ptr3.port, (int)ptr.port, this);
				}
				else
				{
					this.gameData.mainPlayer.mecha.spaceCombatModule.NotifyFighterRemoved((int)ptr3.port, (int)ptr.port, this);
				}
			}
			else if (ptr3.owner > 0)
			{
				ref EntityData ptr4 = ref this.entityPool[ptr3.owner];
				if (ptr4.id == ptr3.owner && ptr4.combatModuleId > 0)
				{
					CombatModuleComponent combatModuleComponent = this.combatGroundSystem.combatModules.buffer[ptr4.combatModuleId];
					if (combatModuleComponent != null)
					{
						combatModuleComponent.NotifyFighterRemoved((int)ptr3.port, (int)ptr.port, this);
					}
				}
			}
		}
		if (ptr.droneId > 0)
		{
			ptr.isInvincible = false;
		}
		if (ptr.fleetId > 0)
		{
			ptr.isInvincible = false;
		}
	}

	// Token: 0x06001848 RID: 6216 RVA: 0x0019F4E1 File Offset: 0x0019D6E1
	public void RemoveCraftDeferred(int craftId)
	{
		if (this._rmv_id_list == null)
		{
			this._rmv_id_list = new HashSet<int>();
		}
		this._rmv_id_list.Add(craftId);
	}

	// Token: 0x06001849 RID: 6217 RVA: 0x0019F503 File Offset: 0x0019D703
	public void AddCraftDeferred(ECraftProto prototype, int protoId, int ownerId, int port, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		if (this._craft_add_id_list_0 == null)
		{
			this._craft_add_id_list_0 = new HashSet<ValueTuple<ECraftProto, int, int, int, Vector3, Quaternion, Vector3>>();
		}
		this._craft_add_id_list_0.Add(new ValueTuple<ECraftProto, int, int, int, Vector3, Quaternion, Vector3>(prototype, protoId, ownerId, port, pos, rot, vel));
	}

	// Token: 0x0600184A RID: 6218 RVA: 0x0019F534 File Offset: 0x0019D734
	public void AddCraftDeferred(int modelIndex, int ownerId, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		if (this._craft_add_id_list_1 == null)
		{
			this._craft_add_id_list_1 = new HashSet<ValueTuple<int, int, Vector3, Quaternion, Vector3>>();
		}
		this._craft_add_id_list_1.Add(new ValueTuple<int, int, Vector3, Quaternion, Vector3>(modelIndex, ownerId, pos, rot, vel));
	}

	// Token: 0x0600184B RID: 6219 RVA: 0x0019F564 File Offset: 0x0019D764
	public void ExecuteDeferredCraftChange()
	{
		if (this._rmv_id_list != null && this._rmv_id_list.Count > 0)
		{
			foreach (int id in this._rmv_id_list)
			{
				this.RemoveCraftWithComponents(id);
			}
			this._rmv_id_list.Clear();
		}
		if (this._craft_add_id_list_0 != null && this._craft_add_id_list_0.Count > 0)
		{
			foreach (ValueTuple<ECraftProto, int, int, int, Vector3, Quaternion, Vector3> valueTuple in this._craft_add_id_list_0)
			{
				this.CreateCraftFinally(valueTuple.Item1, valueTuple.Item2, valueTuple.Item3, valueTuple.Item4, valueTuple.Item5, valueTuple.Item6, valueTuple.Item7);
			}
			this._craft_add_id_list_0.Clear();
		}
		if (this._craft_add_id_list_1 != null && this._craft_add_id_list_1.Count > 0)
		{
			foreach (ValueTuple<int, int, Vector3, Quaternion, Vector3> valueTuple2 in this._craft_add_id_list_1)
			{
				this.CreateConstructionDroneFinally(valueTuple2.Item1, valueTuple2.Item2, valueTuple2.Item3, valueTuple2.Item4, valueTuple2.Item5);
			}
			this._craft_add_id_list_1.Clear();
		}
	}

	// Token: 0x0600184C RID: 6220 RVA: 0x0019F6F0 File Offset: 0x0019D8F0
	private void SetPrebuildCapacity(int newCapacity)
	{
		PrebuildData[] array = this.prebuildPool;
		this.prebuildPool = new PrebuildData[newCapacity];
		this.prebuildRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.prebuildPool, (newCapacity > this.prebuildCapacity) ? this.prebuildCapacity : newCapacity);
		}
		int[] array2 = this.prebuildConnPool;
		this.prebuildConnPool = new int[newCapacity * 16];
		if (array2 != null)
		{
			Array.Copy(array2, this.prebuildConnPool, ((newCapacity > this.prebuildCapacity) ? this.prebuildCapacity : newCapacity) * 16);
		}
		this.prebuildCapacity = newCapacity;
	}

	// Token: 0x0600184D RID: 6221 RVA: 0x0019F780 File Offset: 0x0019D980
	private int AddPrebuildData(PrebuildData prebuild)
	{
		if (this.prebuildRecycleCursor > 0)
		{
			int[] array = this.prebuildRecycle;
			int num = this.prebuildRecycleCursor - 1;
			this.prebuildRecycleCursor = num;
			prebuild.id = array[num];
		}
		else
		{
			int num = this.prebuildCursor;
			this.prebuildCursor = num + 1;
			prebuild.id = num;
		}
		if (prebuild.id == this.prebuildCapacity)
		{
			this.SetPrebuildCapacity(this.prebuildCapacity * 2);
		}
		this.prebuildPool[prebuild.id] = prebuild;
		this.prebuildPool[prebuild.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(prebuild.id, prebuild.pos, EObjectType.Prebuild);
		Array.Clear(this.prebuildConnPool, prebuild.id * 16, 16);
		return prebuild.id;
	}

	// Token: 0x0600184E RID: 6222 RVA: 0x0019F848 File Offset: 0x0019DA48
	public int AddPrebuildDataWithComponents(PrebuildData prebuild)
	{
		ItemProto itemProto = LDB.items.Select((int)prebuild.protoId);
		if (itemProto == null || !itemProto.IsEntity)
		{
			return 0;
		}
		prebuild.hash.InitHashBits(prebuild.pos.x, prebuild.pos.y, prebuild.pos.z);
		int num = this.AddPrebuildData(prebuild);
		PrefabDesc prefabDesc = itemProto.prefabDesc;
		ModelProto modelProto = LDB.models.modelArray[(int)prebuild.modelIndex];
		if (modelProto != null)
		{
			prefabDesc = modelProto.prefabDesc;
		}
		if (prefabDesc == null)
		{
			return num;
		}
		if (prebuild.itemRequired > 0)
		{
			this.AddPrebuildWarning(num);
		}
		this.constructionSystem.SetupReferenceOnPrebuildCreate(ref this.prebuildPool[num]);
		this.prebuildPool[num].modelIndex = (short)prefabDesc.modelIndex;
		if (this.planet.physics == null)
		{
			return num;
		}
		this.prebuildPool[num].modelId = GameMain.gpuiManager.AddPrebuildModel((int)this.prebuildPool[num].modelIndex, num, this.prebuildPool[num].pos, this.prebuildPool[num].rot, true);
		if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
		{
			for (int i = 0; i < prefabDesc.colliders.Length; i++)
			{
				if (prefabDesc.isInserter)
				{
					ColliderData colliderData = prefabDesc.colliders[i];
					Vector3 wpos = Vector3.Lerp(this.prebuildPool[num].pos, this.prebuildPool[num].pos2, 0.5f);
					Quaternion wrot = Quaternion.LookRotation(this.prebuildPool[num].pos2 - this.prebuildPool[num].pos, (prebuild.rot * Vector3.up + prebuild.rot2 * Vector3.up).normalized);
					colliderData.ext = new Vector3(colliderData.ext.x, colliderData.ext.y, Vector3.Distance(this.prebuildPool[num].pos2, this.prebuildPool[num].pos) * 0.5f + colliderData.ext.z);
					this.prebuildPool[num].colliderId = this.planet.physics.AddColliderData(colliderData.BindToObject(num, this.prebuildPool[num].colliderId, EObjectType.Prebuild, wpos, wrot));
				}
				else
				{
					this.prebuildPool[num].colliderId = this.planet.physics.AddColliderData(prefabDesc.colliders[i].BindToObject(num, this.prebuildPool[num].colliderId, EObjectType.Prebuild, this.prebuildPool[num].pos, this.prebuildPool[num].rot));
				}
			}
		}
		if (prefabDesc.isPowerNode && this.planet.factoryModel != null)
		{
			this.planet.factoryModel.RefreshPowerNodes();
		}
		this.NotifyPrebuildChange(num, 1);
		return num;
	}

	// Token: 0x0600184F RID: 6223 RVA: 0x0019FB84 File Offset: 0x0019DD84
	public void PostRefreshPrebuildDisplay(int prebuildId, bool setInserterCollider = false)
	{
		if (this.planet != this.gameData.localPlanet)
		{
			return;
		}
		if (this.planet.physics == null)
		{
			return;
		}
		int objId = -prebuildId;
		PrebuildData prebuildData = this.prebuildPool[prebuildId];
		ModelProto modelProto = LDB.models.modelArray[(int)prebuildData.modelIndex];
		if (modelProto == null)
		{
			return;
		}
		PrefabDesc prefabDesc = modelProto.prefabDesc;
		if (prefabDesc == null)
		{
			return;
		}
		if (!prefabDesc.isBelt)
		{
			if (setInserterCollider && prefabDesc.isInserter)
			{
				if (prebuildData.colliderId == 0)
				{
					Assert.Positive(prebuildData.colliderId);
					return;
				}
				bool flag;
				int num;
				int num2;
				this.ReadObjectConn(objId, 0, out flag, out num, out num2);
				int num3;
				this.ReadObjectConn(objId, 1, out flag, out num3, out num2);
				ColliderContainer[] colChunks = this.planet.physics.colChunks;
				int num4 = prebuildData.colliderId;
				int num5 = num4 >> 20;
				num4 &= 1048575;
				ColliderData[] colliderPool = colChunks[num5].colliderPool;
				ColliderData[] array = colliderPool;
				int num6 = num4;
				array[num6].pos = array[num6].pos + (prebuildData.pos2 - prebuildData.pos) * 0.5f;
				colliderPool[num4].q = Quaternion.LookRotation(prebuildData.pos2 - prebuildData.pos, (prebuildData.rot * Vector3.up + prebuildData.rot2 * Vector3.up).normalized);
				colliderPool[num4].ext = new Vector3(colliderPool[num4].ext.x, colliderPool[num4].ext.y, Vector3.Distance(prebuildData.pos, prebuildData.pos2) * 0.5f + colliderPool[num4].ext.z - 0.5f);
				if (this.ObjectIsBelt(num))
				{
					ColliderData[] array2 = colliderPool;
					int num7 = num4;
					array2[num7].pos.z = array2[num7].pos.z - 0.35f;
					ColliderData[] array3 = colliderPool;
					int num8 = num4;
					array3[num8].ext.z = array3[num8].ext.z + 0.35f;
				}
				else if (num == 0)
				{
					ColliderData[] array4 = colliderPool;
					int num9 = num4;
					array4[num9].pos.z = array4[num9].pos.z - 0.35f;
					ColliderData[] array5 = colliderPool;
					int num10 = num4;
					array5[num10].ext.z = array5[num10].ext.z + 0.35f;
				}
				if (this.ObjectIsBelt(num3))
				{
					ColliderData[] array6 = colliderPool;
					int num11 = num4;
					array6[num11].pos.z = array6[num11].pos.z + 0.35f;
					ColliderData[] array7 = colliderPool;
					int num12 = num4;
					array7[num12].ext.z = array7[num12].ext.z + 0.35f;
				}
				else if (num3 == 0)
				{
					ColliderData[] array8 = colliderPool;
					int num13 = num4;
					array8[num13].pos.z = array8[num13].pos.z + 0.35f;
					ColliderData[] array9 = colliderPool;
					int num14 = num4;
					array9[num14].ext.z = array9[num14].ext.z + 0.35f;
				}
				if (colliderPool[num4].ext.z < 0.1f)
				{
					colliderPool[num4].ext.z = 0.1f;
				}
			}
			return;
		}
		if (prebuildData.colliderId == 0)
		{
			Assert.Positive(prebuildData.colliderId);
			return;
		}
		Vector3 vector = prebuildData.pos;
		Vector3 vector2 = vector;
		Vector3 vector3 = vector;
		bool flag2;
		int num15;
		int num16;
		this.ReadObjectConn(objId, 0, out flag2, out num15, out num16);
		if (num15 > 0)
		{
			vector3 = this.entityPool[num15].pos;
		}
		else if (num15 < 0)
		{
			vector3 = this.prebuildPool[-num15].pos;
		}
		this.ReadObjectConn(objId, 1, out flag2, out num15, out num16);
		if (num15 > 0)
		{
			vector2 = this.entityPool[num15].pos;
		}
		else if (num15 < 0)
		{
			vector2 = this.prebuildPool[-num15].pos;
		}
		Vector3 vector4 = vector2 - vector;
		vector4 = Vector3.ClampMagnitude(vector4 * 0.5f, 1f);
		Vector3 vector5 = vector3 - vector;
		vector5 = Vector3.ClampMagnitude(vector5 * 0.5f, 1f);
		if (vector4.sqrMagnitude < 0.01f)
		{
			vector4 = vector5 * -0.75f;
		}
		if (vector5.sqrMagnitude < 0.01f)
		{
			vector5 = vector4 * -0.75f;
		}
		vector2 = vector + vector4;
		vector3 = vector + vector5;
		vector = (vector2 + vector3) / 2f;
		ColliderContainer[] colChunks2 = this.planet.physics.colChunks;
		Vector3 vector6;
		vector6.x = vector3.x - vector2.x;
		vector6.y = vector3.y - vector2.y;
		vector6.z = vector3.z - vector2.z;
		float num17 = Mathf.Sqrt(vector6.x * vector6.x + vector6.y * vector6.y + vector6.z * vector6.z);
		int num18 = prebuildData.colliderId;
		int num19 = num18 >> 20;
		num18 &= 1048575;
		ColliderData[] colliderPool2 = colChunks2[num19].colliderPool;
		colliderPool2[num18].pos = (vector + vector.normalized * 0.15f) * 1.00032f;
		if (num17 > 0.6f)
		{
			colliderPool2[num18].q = Quaternion.LookRotation(vector6, vector);
			colliderPool2[num18].ext.z = num17 * 0.5f;
		}
		else
		{
			colliderPool2[num18].q = Quaternion.LookRotation(vector) * Quaternion.Euler(90f, 0f, 0f);
			colliderPool2[num18].ext.z = 0.3f;
		}
		this.planet.physics.isPlanetPhysicsColliderDirty = true;
	}

	// Token: 0x06001850 RID: 6224 RVA: 0x001A0150 File Offset: 0x0019E350
	private bool ObjectIsBelt(int objId)
	{
		if (objId == 0)
		{
			return false;
		}
		if (objId > 0)
		{
			ItemProto itemProto = LDB.items.Select((int)this.entityPool[objId].protoId);
			return itemProto != null && itemProto.prefabDesc.isBelt;
		}
		ItemProto itemProto2 = LDB.items.Select((int)this.prebuildPool[-objId].protoId);
		return itemProto2 != null && itemProto2.prefabDesc.isBelt;
	}

	// Token: 0x06001851 RID: 6225 RVA: 0x001A01C0 File Offset: 0x0019E3C0
	public void RemovePrebuildWithComponents(int id)
	{
		if (id != 0 && this.prebuildPool[id].id != 0)
		{
			this.NotifyPrebuildChange(id, -1);
			this.constructionSystem.ClearReferenceOnPrebuildRemove(ref this.prebuildPool[id]);
			if (this.prebuildPool[id].modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemovePrebuildModel((int)this.prebuildPool[id].modelIndex, this.prebuildPool[id].modelId, true);
				}
				this.prebuildPool[id].modelId = 0;
			}
			if (this.prebuildPool[id].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.prebuildPool[id].colliderId);
				}
				this.prebuildPool[id].colliderId = 0;
			}
			if (this.prebuildPool[id].warningId != 0)
			{
				this.gameData.warningSystem.RemoveWarningData(this.prebuildPool[id].warningId);
				this.prebuildPool[id].warningId = 0;
			}
			if (this.prebuildPool[id].extraInfoId != 0)
			{
				this.digitalSystem.RemoveExtraInfoComponent(this.prebuildPool[id].extraInfoId);
				this.prebuildPool[id].extraInfoId = 0;
			}
			if (this.prebuildPool[id].ruinId != 0)
			{
				this.RemoveRuinWithComponet(this.prebuildPool[id].ruinId);
				this.prebuildPool[id].ruinId = 0;
			}
			if (this.prebuildPool[id].modelIndex > 0)
			{
				ModelProto modelProto = LDB.models.Select((int)this.prebuildPool[id].modelIndex);
				if (((modelProto != null) ? modelProto.prefabDesc : null).isPowerNode && this.planet.factoryModel != null)
				{
					this.planet.factoryModel.RefreshPowerNodes();
				}
			}
			this.hashSystemStatic.RemoveObjectFromBucket(this.prebuildPool[id].hashAddress);
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.Prebuild);
			this.prebuildPool[id].SetNull();
			this.ClearObjectConn(-id);
			Array.Clear(this.prebuildConnPool, id * 16, 16);
			int[] array = this.prebuildRecycle;
			int num = this.prebuildRecycleCursor;
			this.prebuildRecycleCursor = num + 1;
			array[num] = id;
			if (!PlanetFactory.batchBuild && this.planet.physics != null)
			{
				this.planet.physics.NotifyObjectRemove(EObjectType.Prebuild, id);
			}
			if (this.prebuildCount == 0)
			{
				this.prebuildCursor = 1;
				this.prebuildRecycleCursor = 0;
			}
		}
	}

	// Token: 0x06001852 RID: 6226 RVA: 0x001A0498 File Offset: 0x0019E698
	public void UpgradePrebuildWithComponents(int id, ItemProto newProto)
	{
		if (id != 0 && this.prebuildPool[id].id == id)
		{
			if (this.prebuildPool[id].modelId != 0)
			{
				GameMain.gpuiManager.RemovePrebuildModel((int)this.prebuildPool[id].modelIndex, this.prebuildPool[id].modelId, true);
				this.prebuildPool[id].modelId = 0;
			}
			this.prebuildPool[id].protoId = (short)newProto.ID;
			this.prebuildPool[id].modelIndex = (short)newProto.ModelIndex;
			this.prebuildPool[id].modelId = GameMain.gpuiManager.AddPrebuildModel((int)this.prebuildPool[id].modelIndex, id, this.prebuildPool[id].pos, this.prebuildPool[id].rot, true);
			this.NotifyPrebuildChange(id, 2);
		}
	}

	// Token: 0x06001853 RID: 6227 RVA: 0x001A059C File Offset: 0x0019E79C
	public PrebuildData GetPrebuildData(int id)
	{
		return this.prebuildPool[id];
	}

	// Token: 0x06001854 RID: 6228 RVA: 0x001A05AC File Offset: 0x0019E7AC
	public void ReadObjectConn(int objId, int slot, out bool isOutput, out int otherObjId, out int otherSlot)
	{
		isOutput = false;
		otherObjId = 0;
		otherSlot = 0;
		if (objId > 0)
		{
			int num = this.entityConnPool[objId * 16 + slot];
			if (num == 0)
			{
				return;
			}
			bool flag = num > 0;
			num = (flag ? num : (-num));
			isOutput = ((num & 536870912) == 0);
			otherObjId = (num & 16777215);
			otherSlot = (num & 536870911) >> 24;
			if (!flag)
			{
				otherObjId = -otherObjId;
				return;
			}
		}
		else if (objId < 0)
		{
			int num2 = this.prebuildConnPool[-objId * 16 + slot];
			if (num2 == 0)
			{
				return;
			}
			bool flag2 = num2 > 0;
			num2 = (flag2 ? num2 : (-num2));
			isOutput = ((num2 & 536870912) == 0);
			otherObjId = (num2 & 16777215);
			otherSlot = (num2 & 536870911) >> 24;
			if (!flag2)
			{
				otherObjId = -otherObjId;
			}
		}
	}

	// Token: 0x06001855 RID: 6229 RVA: 0x001A0664 File Offset: 0x0019E864
	private void WriteObjectConnDirect(int objId, int slot, bool isOutput, int otherObjId, int otherSlot)
	{
		if (objId == 0)
		{
			return;
		}
		int num = 0;
		if (otherObjId != 0)
		{
			bool flag = otherObjId > 0;
			otherObjId = (flag ? otherObjId : (-otherObjId));
			num = (otherObjId | otherSlot << 24 | (isOutput ? 0 : 1) << 29);
			if (!flag)
			{
				num = -num;
			}
		}
		if (objId > 0)
		{
			this.entityConnPool[objId * 16 + slot] = num;
			return;
		}
		if (objId < 0)
		{
			this.prebuildConnPool[-objId * 16 + slot] = num;
		}
	}

	// Token: 0x06001856 RID: 6230 RVA: 0x001A06CB File Offset: 0x0019E8CB
	private void ClearObjectConnDirect(int objId, int slot)
	{
		if (objId == 0)
		{
			return;
		}
		if (objId > 0)
		{
			this.entityConnPool[objId * 16 + slot] = 0;
			return;
		}
		if (objId < 0)
		{
			this.prebuildConnPool[-objId * 16 + slot] = 0;
		}
	}

	// Token: 0x06001857 RID: 6231 RVA: 0x001A06F8 File Offset: 0x0019E8F8
	public void ClearObjectConn(int objId, int slot)
	{
		bool flag;
		int objId2;
		int slot2;
		this.ReadObjectConn(objId, slot, out flag, out objId2, out slot2);
		this.ClearObjectConnDirect(objId, slot);
		this.ClearObjectConnDirect(objId2, slot2);
	}

	// Token: 0x06001858 RID: 6232 RVA: 0x001A0724 File Offset: 0x0019E924
	public void ClearObjectConn(int objId)
	{
		if (objId > 0)
		{
			int num = objId * 16;
			for (int i = 0; i < 16; i++)
			{
				if (this.entityConnPool[num + i] != 0)
				{
					this.ClearObjectConn(objId, i);
				}
			}
			return;
		}
		if (objId < 0)
		{
			int num2 = -objId * 16;
			for (int j = 0; j < 16; j++)
			{
				if (this.prebuildConnPool[num2 + j] != 0)
				{
					this.ClearObjectConn(objId, j);
				}
			}
		}
	}

	// Token: 0x06001859 RID: 6233 RVA: 0x001A0788 File Offset: 0x0019E988
	public void WriteObjectConn(int objId, int slot, bool isOutput, int otherObjId, int otherSlot)
	{
		if (otherSlot == -1)
		{
			if (otherObjId > 0)
			{
				for (int i = 4; i < 12; i++)
				{
					if (this.entityConnPool[otherObjId * 16 + i] == 0)
					{
						otherSlot = i;
						break;
					}
				}
			}
			else if (otherObjId < 0)
			{
				for (int j = 4; j < 12; j++)
				{
					if (this.prebuildConnPool[-otherObjId * 16 + j] == 0)
					{
						otherSlot = j;
						break;
					}
				}
			}
		}
		if (otherSlot < 0)
		{
			return;
		}
		this.ClearObjectConn(objId, slot);
		this.ClearObjectConn(otherObjId, otherSlot);
		this.WriteObjectConnDirect(objId, slot, isOutput, otherObjId, otherSlot);
		this.WriteObjectConnDirect(otherObjId, otherSlot, !isOutput, objId, slot);
	}

	// Token: 0x0600185A RID: 6234 RVA: 0x001A0820 File Offset: 0x0019EA20
	public void ValidateConns(int objId, bool inverseCheck)
	{
		for (int i = 0; i < 16; i++)
		{
			bool flag;
			int num;
			int slot;
			this.ReadObjectConn(objId, i, out flag, out num, out slot);
			if (num != 0)
			{
				bool flag2 = false;
				int num2 = 0;
				if (num == objId)
				{
					flag2 = true;
				}
				if (!flag2)
				{
					if (num > 0)
					{
						int num3 = num;
						if (num3 >= this.entityCursor || this.entityPool[num3].id != num3)
						{
							flag2 = true;
						}
					}
					else if (num < 0)
					{
						int num4 = -num;
						if (num4 >= this.prebuildCursor || this.prebuildPool[num4].id != num4)
						{
							flag2 = true;
						}
					}
				}
				if (!flag2 && inverseCheck)
				{
					bool flag3;
					int num5;
					int num6;
					this.ReadObjectConn(num, slot, out flag3, out num5, out num6);
					if (num5 != objId)
					{
						flag2 = true;
					}
					else if (flag == flag3)
					{
						num2++;
					}
					else if (num6 != i)
					{
						num2++;
					}
				}
				if (flag2)
				{
					this.ClearObjectConnDirect(objId, i);
					Debug.LogWarning("PlanetFactory.ValidateConns: invalid connection data found, cleared successfully");
				}
				else if (num2 > 0)
				{
					Debug.LogWarning("PlanetFactory.ValidateConns: invalid connection data found, cannot be repaired to avoid possible errors");
				}
			}
		}
	}

	// Token: 0x0600185B RID: 6235 RVA: 0x001A091C File Offset: 0x0019EB1C
	public void HandleObjectConnChangeWhenBuild(int oldId, int newId)
	{
		for (int i = 0; i < 16; i++)
		{
			bool isOutput;
			int num;
			int otherSlot;
			this.ReadObjectConn(oldId, i, out isOutput, out num, out otherSlot);
			if (num != 0)
			{
				this.WriteObjectConn(newId, i, isOutput, num, otherSlot);
			}
		}
		if (oldId > 0)
		{
			Array.Clear(this.entityConnPool, oldId * 16, 16);
			return;
		}
		Array.Clear(this.prebuildConnPool, -oldId * 16, 16);
	}

	// Token: 0x0600185C RID: 6236 RVA: 0x001A097C File Offset: 0x0019EB7C
	public void EnsureObjectConn(int objId)
	{
		for (int i = 0; i < 16; i++)
		{
			bool isOutput;
			int num;
			int otherSlot;
			this.ReadObjectConn(objId, i, out isOutput, out num, out otherSlot);
			if (num != 0)
			{
				this.WriteObjectConn(objId, i, isOutput, num, otherSlot);
			}
		}
	}

	// Token: 0x0600185D RID: 6237 RVA: 0x001A09B4 File Offset: 0x0019EBB4
	public int GetTopObjectId(int objId, bool includeAddon)
	{
		int num = objId;
		int num2 = 0;
		do
		{
			bool flag;
			int num3;
			int num4;
			this.ReadObjectConn(num, 15, out flag, out num3, out num4);
			if (flag && num3 != 0 && num4 == 14)
			{
				num = num3;
			}
			else
			{
				if (num3 != 0 || !includeAddon)
				{
					return num;
				}
				this.ReadObjectConn(num, 13, out flag, out num3, out num4);
				if (num3 == 0 || num4 != 0)
				{
					return num;
				}
				num = num3;
			}
		}
		while (num2++ <= 256);
		Assert.CannotBeReached();
		return num;
	}

	// Token: 0x0600185E RID: 6238 RVA: 0x001A0A18 File Offset: 0x0019EC18
	public void SetEnemyCapacity(int newCapacity)
	{
		EnemyData[] array = this.enemyPool;
		this.enemyPool = new EnemyData[newCapacity];
		this.enemyRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.enemyPool, (newCapacity > this.enemyCapacity) ? this.enemyCapacity : newCapacity);
		}
		AnimData[] array2 = this.enemyAnimPool;
		this.enemyAnimPool = new AnimData[newCapacity];
		if (array2 != null)
		{
			Array.Copy(array2, this.enemyAnimPool, (newCapacity > this.enemyCapacity) ? this.enemyCapacity : newCapacity);
		}
		this.enemyCapacity = newCapacity;
	}

	// Token: 0x0600185F RID: 6239 RVA: 0x001A0AA0 File Offset: 0x0019ECA0
	public EnemyData GetEnemyData(int id)
	{
		return this.enemyPool[id];
	}

	// Token: 0x06001860 RID: 6240 RVA: 0x001A0AB0 File Offset: 0x0019ECB0
	public int AddEnemyData(ref EnemyData enemy)
	{
		if (this.enemyRecycleCursor > 0)
		{
			int[] array = this.enemyRecycle;
			int num = this.enemyRecycleCursor - 1;
			this.enemyRecycleCursor = num;
			enemy.id = array[num];
		}
		else
		{
			int num = this.enemyCursor;
			this.enemyCursor = num + 1;
			enemy.id = num;
		}
		if (enemy.id == this.enemyCapacity)
		{
			this.SetEnemyCapacity(this.enemyCapacity * 2);
		}
		this.enemyPool[enemy.id] = enemy;
		if (enemy.dynamic)
		{
			this.enemyPool[enemy.id].hashAddress = this.hashSystemDynamic.AddObjectToBucket(enemy.id, enemy.pos, EObjectType.Enemy);
		}
		else
		{
			this.enemyPool[enemy.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(enemy.id, enemy.pos, EObjectType.Enemy);
		}
		return enemy.id;
	}

	// Token: 0x06001861 RID: 6241 RVA: 0x001A0BA8 File Offset: 0x0019EDA8
	public void CreateEnemyDisplayComponents(int enemyId)
	{
		int modelIndex = (int)this.enemyPool[enemyId].modelIndex;
		ModelProto modelProto = LDB.models.modelArray[modelIndex];
		if (modelProto == null)
		{
			return;
		}
		PrefabDesc prefabDesc = modelProto.prefabDesc;
		if (prefabDesc == null)
		{
			return;
		}
		if (this.enemyPool[enemyId].originAstroId == this.planet.id)
		{
			this.enemyPool[enemyId].modelId = GameMain.gpuiManager.AddModel(modelIndex, enemyId, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot, true);
		}
		if (prefabDesc.minimapType > 0 && this.enemyPool[enemyId].mmblockId == 0)
		{
			this.enemyPool[enemyId].mmblockId = this.blockContainer.AddMiniBlock(enemyId, prefabDesc.minimapType, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot, prefabDesc.minimapScl);
		}
		if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
		{
			int dfGConnectorId = this.enemyPool[enemyId].dfGConnectorId;
			if (dfGConnectorId > 0)
			{
				for (int i = 0; i < prefabDesc.colliders.Length; i++)
				{
					ColliderData colliderData = prefabDesc.colliders[i];
					float length = this.enemySystem.connectors.buffer[dfGConnectorId].length;
					colliderData.ext.z = (length - 1f) / 2f;
					colliderData.pos.z = length / 2f;
					this.enemyPool[enemyId].colliderId = this.planet.physics.AddColliderData(colliderData.BindToObject(enemyId, this.enemyPool[enemyId].colliderId, EObjectType.Enemy, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot));
				}
			}
			else
			{
				for (int j = 0; j < prefabDesc.colliders.Length; j++)
				{
					this.enemyPool[enemyId].colliderId = this.planet.physics.AddColliderData(prefabDesc.colliders[j].BindToObject(enemyId, this.enemyPool[enemyId].colliderId, EObjectType.Enemy, this.enemyPool[enemyId].pos, this.enemyPool[enemyId].rot));
				}
			}
		}
		if (prefabDesc.hasAudio)
		{
			this.enemyPool[enemyId].audioId = this.planet.audio.AddAudioData(enemyId, EObjectType.Enemy, this.enemyPool[enemyId].pos, prefabDesc);
		}
	}

	// Token: 0x06001862 RID: 6242 RVA: 0x001A0E88 File Offset: 0x0019F088
	public void CreateEnemyLogicComponents(int enemyId, PrefabDesc desc, int builderIndex)
	{
		if (desc.isEnemyBuilder)
		{
			this.enemySystem.NewEnemyBuilderComponent(enemyId, desc, builderIndex);
		}
		if (builderIndex > 0)
		{
			int dfGBaseId = this.enemyPool[enemyId].dfGBaseId;
			if (dfGBaseId > 0)
			{
				this.enemySystem.bases.buffer[dfGBaseId].builderId = this.enemyPool[enemyId].builderId;
				this.platformSystem.AddStateArea((uint)(16777216L | (long)dfGBaseId), this.enemyPool[enemyId].pos, 11.25f, 1);
			}
			if (desc.isDFGroundConnector)
			{
				this.enemySystem.NewDFGConnectorComponent(enemyId, desc);
			}
			if (desc.isDFGroundReplicator)
			{
				this.enemySystem.NewDFGReplicatorComponent(enemyId, desc);
			}
			if (desc.isDFGroundTurret)
			{
				this.enemySystem.NewDFGTurretComponent(enemyId, desc);
			}
			if (desc.isDFGroundShield)
			{
				this.enemySystem.NewDFGShieldComponent(enemyId, desc);
			}
		}
		if (desc.isEnemyUnit)
		{
			this.enemySystem.NewEnemyUnitComponent(enemyId);
		}
		this.enemyAnimPool[enemyId].time = 0f;
		this.enemyAnimPool[enemyId].prepare_length = desc.anim_prepare_length;
		this.enemyAnimPool[enemyId].working_length = desc.anim_working_length;
		this.enemyAnimPool[enemyId].state = 0U;
		this.enemyAnimPool[enemyId].power = 0f;
	}

	// Token: 0x06001863 RID: 6243 RVA: 0x001A0FFC File Offset: 0x0019F1FC
	private int AddEnemyDataWithComponents(ref EnemyData enemy, int builderIndex)
	{
		int num = this.AddEnemyData(ref enemy);
		PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)enemy.modelIndex];
		if (prefabDesc == null)
		{
			return num;
		}
		this.CreateEnemyLogicComponents(num, prefabDesc, builderIndex);
		if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 6)
		{
			this.CreateEnemyDisplayComponents(num);
		}
		this.enemySystem.SetupReferenceOnEnemyCreate(num);
		return num;
	}

	// Token: 0x06001864 RID: 6244 RVA: 0x001A105C File Offset: 0x0019F25C
	public void RemoveEnemyWithComponents(int id)
	{
		bool flag = false;
		if (id != 0 && this.enemyPool[id].id != 0)
		{
			this.enemySystem.ClearReferencesOnEnemyRemove(id);
			if (this.enemyPool[id].dfGBaseId != 0)
			{
				this.platformSystem.RemoveStateArea((uint)(16777216L | (long)this.enemyPool[id].dfGBaseId));
				this.enemySystem.RemoveDFGBaseComponent(this.enemyPool[id].dfGBaseId);
				this.enemyPool[id].dfGBaseId = 0;
			}
			if (this.enemyPool[id].dfGConnectorId != 0)
			{
				this.enemySystem.RemoveDFGConnectorComponent(this.enemyPool[id].dfGConnectorId);
				this.enemyPool[id].dfGConnectorId = 0;
			}
			if (this.enemyPool[id].dfGReplicatorId != 0)
			{
				this.enemySystem.RemoveDFGReplicatorComponent(this.enemyPool[id].dfGReplicatorId);
				this.enemyPool[id].dfGReplicatorId = 0;
			}
			if (this.enemyPool[id].dfGTurretId != 0)
			{
				this.enemySystem.RemoveDFGTurretComponent(this.enemyPool[id].dfGTurretId);
				this.enemyPool[id].dfGTurretId = 0;
			}
			if (this.enemyPool[id].dfGShieldId != 0)
			{
				this.enemySystem.RemoveDFGShieldComponent(this.enemyPool[id].dfGShieldId);
				this.enemyPool[id].dfGShieldId = 0;
			}
			if (this.enemyPool[id].combatStatId != 0)
			{
				int combatStatId = this.enemyPool[id].combatStatId;
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(combatStatId);
				this.enemyPool[id].combatStatId = 0;
			}
			if (this.enemyPool[id].unitId != 0)
			{
				this.enemySystem.RemoveEnemyUnitComponent(this.enemyPool[id].unitId);
				this.enemyPool[id].unitId = 0;
			}
			if (this.enemyPool[id].builderId != 0)
			{
				this.enemySystem.RemoveEnemyBuilderComponent(this.enemyPool[id].builderId);
				this.enemyPool[id].builderId = 0;
			}
			if (this.enemyPool[id].modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemoveModel((int)this.enemyPool[id].modelIndex, this.enemyPool[id].modelId, true);
				}
				this.enemyPool[id].modelId = 0;
			}
			if (this.enemyPool[id].mmblockId != 0)
			{
				if (this.blockContainer != null)
				{
					this.blockContainer.RemoveMiniBlock(this.enemyPool[id].mmblockId);
				}
				this.enemyPool[id].mmblockId = 0;
			}
			if (this.enemyPool[id].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.enemyPool[id].colliderId);
				}
				this.enemyPool[id].colliderId = 0;
			}
			if (this.enemyPool[id].audioId != 0)
			{
				flag = true;
				if (this.planet.audio != null)
				{
					this.planet.audio.RemoveAudioData(this.enemyPool[id].audioId);
				}
				this.enemyPool[id].audioId = 0;
			}
			if (this.enemyPool[id].dynamic)
			{
				this.hashSystemDynamic.RemoveObjectFromBucket(this.enemyPool[id].hashAddress);
			}
			else
			{
				this.hashSystemStatic.RemoveObjectFromBucket(this.enemyPool[id].hashAddress);
			}
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.Enemy);
			this.enemyPool[id].SetEmpty();
			this.enemyAnimPool[id].time = 0f;
			this.enemyAnimPool[id].prepare_length = 0f;
			this.enemyAnimPool[id].working_length = 0f;
			this.enemyAnimPool[id].state = 0U;
			this.enemyAnimPool[id].power = 0f;
			int[] array = this.enemyRecycle;
			int num = this.enemyRecycleCursor;
			this.enemyRecycleCursor = num + 1;
			array[num] = id;
		}
		if (this.planet.physics != null)
		{
			this.planet.physics.NotifyObjectRemove(EObjectType.Enemy, id);
		}
		if (flag && this.planet.audio != null)
		{
			this.planet.audio.NotifyObjectRemove(EObjectType.Enemy, id);
		}
	}

	// Token: 0x06001865 RID: 6245 RVA: 0x001A157C File Offset: 0x0019F77C
	public DFGBaseComponent CreateEnemyPlanetBase(DFRelayComponent relay, GrowthPattern_DFGround.Builder[] pbuilders)
	{
		if (relay.baseId > 0)
		{
			return null;
		}
		if (pbuilders == null || pbuilders.Length < 1 || pbuilders[1].protoId != 8120)
		{
			return null;
		}
		int protoId = pbuilders[1].protoId;
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto == null)
		{
			return null;
		}
		EnemyData enemyData = default(EnemyData);
		enemyData.protoId = (short)protoId;
		enemyData.modelIndex = (short)enemyProto.ModelIndex;
		enemyData.astroId = this.planet.id;
		enemyData.originAstroId = this.planet.id;
		enemyData.owner = 0;
		enemyData.port = 0;
		enemyData.dynamic = false;
		enemyData.isSpace = false;
		enemyData.localized = false;
		enemyData.stateFlags = 0;
		enemyData.pos = pbuilders[1].pos;
		enemyData.rot = pbuilders[1].rot;
		enemyData.hash.InitHashBits((float)enemyData.pos.x, (float)enemyData.pos.y, (float)enemyData.pos.z);
		PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)enemyData.modelIndex];
		if (prefabDesc == null || !prefabDesc.isDFGroundBase)
		{
			return null;
		}
		int num = this.AddEnemyData(ref enemyData);
		int num2 = this.enemySystem.NewDFGBaseComponent(num, prefabDesc, relay, pbuilders);
		this.enemyPool[num].owner = (short)num2;
		this.CreateEnemyLogicComponents(num, prefabDesc, 1);
		if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 6)
		{
			this.CreateEnemyDisplayComponents(num);
		}
		this.enemySystem.SetupReferenceOnEnemyCreate(num);
		this.defenseSystem.RefreshTurretSearchPair();
		return this.enemySystem.bases.buffer[num2];
	}

	// Token: 0x06001866 RID: 6246 RVA: 0x001A1744 File Offset: 0x0019F944
	public int CreateEnemyFinal(int baseId, int builderIndex)
	{
		ref GrowthPattern_DFGround.Builder ptr = ref this.enemySystem.bases.buffer[baseId].pbuilders[builderIndex];
		if (ptr.instId > 0)
		{
			return 0;
		}
		int protoId = ptr.protoId;
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = new EnemyData
			{
				protoId = (short)protoId,
				modelIndex = (short)enemyProto.ModelIndex,
				astroId = this.planet.id,
				originAstroId = this.planet.id,
				owner = (short)baseId,
				port = 0,
				dynamic = !enemyProto.IsBuilding,
				isSpace = false,
				localized = false,
				stateFlags = 0,
				pos = ptr.pos,
				rot = ptr.rot
			};
			if (!enemyData.dynamic)
			{
				enemyData.hash.InitHashBits((float)enemyData.pos.x, (float)enemyData.pos.y, (float)enemyData.pos.z);
			}
			return this.AddEnemyDataWithComponents(ref enemyData, builderIndex);
		}
		return 0;
	}

	// Token: 0x06001867 RID: 6247 RVA: 0x001A1874 File Offset: 0x0019FA74
	public int CreateEnemyFinal(int protoId, int baseId, int portId, int formTicks)
	{
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = default(EnemyData);
			enemyData.protoId = (short)protoId;
			enemyData.modelIndex = (short)enemyProto.ModelIndex;
			enemyData.astroId = this.planet.id;
			enemyData.originAstroId = this.planet.id;
			enemyData.owner = (short)baseId;
			enemyData.port = (short)portId;
			enemyData.dynamic = !enemyProto.IsBuilding;
			enemyData.isSpace = false;
			enemyData.localized = (this.planet == this.gameData.localPlanet && this.planet.factoryLoaded);
			enemyData.stateFlags = 0;
			enemyData.Formation(formTicks, ref this.enemyPool[this.enemySystem.bases.buffer[baseId].enemyId], this.planet.realRadius, ref enemyData.pos, ref enemyData.rot, ref enemyData.vel);
			if (!enemyData.dynamic)
			{
				enemyData.hash.InitHashBits((float)enemyData.pos.x, (float)enemyData.pos.y, (float)enemyData.pos.z);
			}
			return this.AddEnemyDataWithComponents(ref enemyData, 0);
		}
		return 0;
	}

	// Token: 0x06001868 RID: 6248 RVA: 0x001A19C0 File Offset: 0x0019FBC0
	public int CreateEnemyFinal(int protoId, int baseId, int portId, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		EnemyProto enemyProto = LDB.enemies.Select(protoId);
		if (enemyProto != null)
		{
			EnemyData enemyData = new EnemyData
			{
				protoId = (short)protoId,
				modelIndex = (short)enemyProto.ModelIndex,
				astroId = this.planet.id,
				originAstroId = this.planet.id,
				owner = (short)baseId,
				port = (short)portId,
				dynamic = !enemyProto.IsBuilding,
				isSpace = false,
				localized = (this.planet == this.gameData.localPlanet && this.planet.factoryLoaded),
				stateFlags = 0,
				pos = pos,
				rot = rot,
				vel = vel
			};
			if (!enemyData.dynamic)
			{
				enemyData.hash.InitHashBits((float)enemyData.pos.x, (float)enemyData.pos.y, (float)enemyData.pos.z);
			}
			return this.AddEnemyDataWithComponents(ref enemyData, 0);
		}
		return 0;
	}

	// Token: 0x06001869 RID: 6249 RVA: 0x001A1ADF File Offset: 0x0019FCDF
	public void RemoveEnemyFinal(int id)
	{
		this.RemoveEnemyWithComponents(id);
	}

	// Token: 0x0600186A RID: 6250 RVA: 0x001A1AE8 File Offset: 0x0019FCE8
	public void SetVeinCapacity(int newCapacity)
	{
		VeinData[] array = this.veinPool;
		AnimData[] array2 = this.veinAnimPool;
		this.veinPool = new VeinData[newCapacity];
		this.veinRecycle = new int[newCapacity];
		this.veinAnimPool = new AnimData[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.veinPool, (newCapacity > this.veinCapacity) ? this.veinCapacity : newCapacity);
		}
		if (array2 != null)
		{
			Array.Copy(array2, this.veinAnimPool, (newCapacity > this.veinCapacity) ? this.veinCapacity : newCapacity);
		}
		this.veinCapacity = newCapacity;
	}

	// Token: 0x0600186B RID: 6251 RVA: 0x001A1B70 File Offset: 0x0019FD70
	public int AddVeinData(VeinData vein)
	{
		if (this.veinRecycleCursor > 0)
		{
			int[] array = this.veinRecycle;
			int num = this.veinRecycleCursor - 1;
			this.veinRecycleCursor = num;
			vein.id = array[num];
		}
		else
		{
			int num = this.veinCursor;
			this.veinCursor = num + 1;
			vein.id = num;
		}
		if (vein.id == this.veinCapacity)
		{
			this.SetVeinCapacity(this.veinCapacity * 2);
		}
		this.veinPool[vein.id] = vein;
		this.veinPool[vein.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(vein.id, vein.pos, EObjectType.Vein);
		this.veinAnimPool[vein.id].time = ((vein.amount >= 20000) ? 0f : (1f - (float)vein.amount * 5E-05f));
		this.veinAnimPool[vein.id].prepare_length = 0f;
		this.veinAnimPool[vein.id].working_length = 1f;
		this.veinAnimPool[vein.id].state = (uint)vein.type;
		this.veinAnimPool[vein.id].power = 0f;
		return vein.id;
	}

	// Token: 0x0600186C RID: 6252 RVA: 0x001A1CCC File Offset: 0x0019FECC
	public void RefreshVeinMiningDisplay(int veinId, int addMinerEntityId, int removeMinerEntityId)
	{
		if (veinId == 0)
		{
			return;
		}
		if (this.veinPool[veinId].id != veinId)
		{
			return;
		}
		if (this.veinPool[veinId].minerCount < 0)
		{
			Assert.CannotBeReached();
			this.veinPool[veinId].minerCount = 0;
		}
		GPUInstancingManager gpuiManager = GameMain.gpuiManager;
		VeinProto veinProto = LDB.veins.Select((int)this.veinPool[veinId].type);
		if (veinProto == null)
		{
			return;
		}
		if (addMinerEntityId > 0)
		{
			this.veinPool[veinId].AddMiner(addMinerEntityId);
			gpuiManager.RemoveModel(veinProto.MinerBaseModelIndex, this.veinPool[veinId].minerBaseModelId, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId0, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId1, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId2, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId3, true);
			this.veinPool[veinId].minerBaseModelId = 0;
			this.veinPool[veinId].minerCircleModelId0 = 0;
			this.veinPool[veinId].minerCircleModelId1 = 0;
			this.veinPool[veinId].minerCircleModelId2 = 0;
			this.veinPool[veinId].minerCircleModelId3 = 0;
		}
		if (removeMinerEntityId > 0)
		{
			this.veinPool[veinId].RemoveMiner(removeMinerEntityId);
			gpuiManager.RemoveModel(veinProto.MinerBaseModelIndex, this.veinPool[veinId].minerBaseModelId, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId0, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId1, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId2, true);
			gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId3, true);
			this.veinPool[veinId].minerBaseModelId = 0;
			this.veinPool[veinId].minerCircleModelId0 = 0;
			this.veinPool[veinId].minerCircleModelId1 = 0;
			this.veinPool[veinId].minerCircleModelId2 = 0;
			this.veinPool[veinId].minerCircleModelId3 = 0;
		}
		Vector3 pos = this.veinPool[veinId].pos;
		float magnitude = pos.magnitude;
		Vector3 a = pos / magnitude;
		Quaternion rot = Maths.SphericalRotation(this.veinPool[veinId].pos, Random.value * 360f);
		if (veinProto.MinerBaseModelIndex > 0)
		{
			if (this.veinPool[veinId].minerCount > 0 && this.veinPool[veinId].minerBaseModelId == 0)
			{
				this.veinPool[veinId].minerBaseModelId = gpuiManager.AddModel(veinProto.MinerBaseModelIndex, this.veinPool[veinId].minerId0, a * (magnitude + 0.1f), rot, true);
			}
			else if (this.veinPool[veinId].minerCount == 0 && this.veinPool[veinId].minerBaseModelId > 0)
			{
				gpuiManager.RemoveModel(veinProto.MinerBaseModelIndex, this.veinPool[veinId].minerBaseModelId, true);
				this.veinPool[veinId].minerBaseModelId = 0;
			}
		}
		if (veinProto.MinerCircleModelIndex > 0)
		{
			if (this.veinPool[veinId].minerCount >= 1 && this.veinPool[veinId].minerCircleModelId0 == 0)
			{
				this.veinPool[veinId].minerCircleModelId0 = gpuiManager.AddModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerId0, a * (magnitude + 0.4f), rot, true);
			}
			else if (this.veinPool[veinId].minerCount < 1 && this.veinPool[veinId].minerCircleModelId0 > 0)
			{
				gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId0, true);
				this.veinPool[veinId].minerCircleModelId0 = 0;
			}
			if (this.veinPool[veinId].minerCount >= 2 && this.veinPool[veinId].minerCircleModelId1 == 0)
			{
				this.veinPool[veinId].minerCircleModelId1 = gpuiManager.AddModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerId1, a * (magnitude + 0.6f), rot, true);
			}
			else if (this.veinPool[veinId].minerCount < 2 && this.veinPool[veinId].minerCircleModelId1 > 0)
			{
				gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId1, true);
				this.veinPool[veinId].minerCircleModelId1 = 0;
			}
			if (this.veinPool[veinId].minerCount >= 3 && this.veinPool[veinId].minerCircleModelId2 == 0)
			{
				this.veinPool[veinId].minerCircleModelId2 = gpuiManager.AddModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerId2, a * (magnitude + 0.8f), rot, true);
			}
			else if (this.veinPool[veinId].minerCount < 3 && this.veinPool[veinId].minerCircleModelId2 > 0)
			{
				gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId2, true);
				this.veinPool[veinId].minerCircleModelId2 = 0;
			}
			if (this.veinPool[veinId].minerCount >= 4 && this.veinPool[veinId].minerCircleModelId3 == 0)
			{
				this.veinPool[veinId].minerCircleModelId3 = gpuiManager.AddModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerId3, a * (magnitude + 1f), rot, true);
				return;
			}
			if (this.veinPool[veinId].minerCount < 4 && this.veinPool[veinId].minerCircleModelId3 > 0)
			{
				gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[veinId].minerCircleModelId3, true);
				this.veinPool[veinId].minerCircleModelId3 = 0;
			}
		}
	}

	// Token: 0x0600186D RID: 6253 RVA: 0x001A2374 File Offset: 0x001A0574
	public void RemoveVeinData(int id)
	{
		if (this.veinPool[id].id != 0)
		{
			this.veinPool[id].SetNull();
			this.veinAnimPool[id].time = 0f;
			this.veinAnimPool[id].prepare_length = 0f;
			this.veinAnimPool[id].working_length = 0f;
			this.veinAnimPool[id].state = 0U;
			this.veinAnimPool[id].power = 0f;
			int[] array = this.veinRecycle;
			int num = this.veinRecycleCursor;
			this.veinRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x0600186E RID: 6254 RVA: 0x001A242C File Offset: 0x001A062C
	public void RemoveVeinWithComponents(int id)
	{
		if (this.veinPool[id].id != 0)
		{
			if (this.veinPool[id].combatStatId != 0)
			{
				int combatStatId = this.veinPool[id].combatStatId;
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(combatStatId);
				this.veinPool[id].combatStatId = 0;
			}
			if (this.veinPool[id].modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemoveModel((int)this.veinPool[id].modelIndex, this.veinPool[id].modelId, true);
				}
				this.veinPool[id].modelId = 0;
			}
			if (this.veinPool[id].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.veinPool[id].colliderId);
				}
				this.veinPool[id].colliderId = 0;
			}
			VeinProto veinProto = LDB.veins.Select((int)this.veinPool[id].type);
			if (veinProto == null)
			{
				return;
			}
			if (veinProto.MinerBaseModelIndex > 0 && this.veinPool[id].minerBaseModelId > 0)
			{
				GameMain.gpuiManager.RemoveModel(veinProto.MinerBaseModelIndex, this.veinPool[id].minerBaseModelId, true);
			}
			if (veinProto.MinerCircleModelIndex > 0 && this.veinPool[id].minerCircleModelId0 > 0)
			{
				GameMain.gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[id].minerCircleModelId0, true);
			}
			if (veinProto.MinerCircleModelIndex > 0 && this.veinPool[id].minerCircleModelId1 > 0)
			{
				GameMain.gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[id].minerCircleModelId1, true);
			}
			if (veinProto.MinerCircleModelIndex > 0 && this.veinPool[id].minerCircleModelId2 > 0)
			{
				GameMain.gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[id].minerCircleModelId2, true);
			}
			if (veinProto.MinerCircleModelIndex > 0 && this.veinPool[id].minerCircleModelId3 > 0)
			{
				GameMain.gpuiManager.RemoveModel(veinProto.MinerCircleModelIndex, this.veinPool[id].minerCircleModelId3, true);
			}
			this.hashSystemStatic.RemoveObjectFromBucket(this.veinPool[id].hashAddress);
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.Vein);
			this.veinPool[id].SetNull();
			this.veinAnimPool[id].time = 0f;
			this.veinAnimPool[id].prepare_length = 0f;
			this.veinAnimPool[id].working_length = 0f;
			this.veinAnimPool[id].state = 0U;
			this.veinAnimPool[id].power = 0f;
			int[] array = this.veinRecycle;
			int num = this.veinRecycleCursor;
			this.veinRecycleCursor = num + 1;
			array[num] = id;
			this.factorySystem.NotifyVeinRemoved(id);
			if (this.planet.physics != null)
			{
				this.planet.physics.NotifyObjectRemove(EObjectType.Vein, id);
			}
		}
	}

	// Token: 0x0600186F RID: 6255 RVA: 0x001A27C0 File Offset: 0x001A09C0
	public void KillVeinFinally(int id, int effect)
	{
		int groupIndex = (int)this.veinPool[id].groupIndex;
		this.RemoveVeinWithComponents(id);
		this.RecalculateVeinGroup(groupIndex);
	}

	// Token: 0x06001870 RID: 6256 RVA: 0x001A27F0 File Offset: 0x001A09F0
	public void InitVeinHashAddress()
	{
		for (int i = 1; i < this.veinCursor; i++)
		{
			if (this.veinPool[i].id == i)
			{
				this.veinPool[i].hashAddress = this.hashSystemStatic.AddObjectToBucket(i, this.veinPool[i].pos, EObjectType.Vein);
			}
		}
	}

	// Token: 0x06001871 RID: 6257 RVA: 0x001A2854 File Offset: 0x001A0A54
	public void SetRuinCapacity(int newCapacity)
	{
		RuinData[] array = this.ruinPool;
		this.ruinPool = new RuinData[newCapacity];
		this.ruinRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.ruinPool, (newCapacity > this.ruinCapacity) ? this.ruinCapacity : newCapacity);
		}
		this.ruinCapacity = newCapacity;
	}

	// Token: 0x06001872 RID: 6258 RVA: 0x001A28A8 File Offset: 0x001A0AA8
	public int AddRuinData(RuinData ruin)
	{
		if (this.ruinRecycleCursor > 0)
		{
			int[] array = this.ruinRecycle;
			int num = this.ruinRecycleCursor - 1;
			this.ruinRecycleCursor = num;
			ruin.id = array[num];
		}
		else
		{
			int num = this.ruinCursor;
			this.ruinCursor = num + 1;
			ruin.id = num;
		}
		if (ruin.id == this.ruinCapacity)
		{
			this.SetRuinCapacity(this.ruinCapacity * 2);
		}
		this.ruinPool[ruin.id] = ruin;
		this.ruinPool[ruin.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(ruin.id, ruin.pos, EObjectType.Ruin);
		return ruin.id;
	}

	// Token: 0x06001873 RID: 6259 RVA: 0x001A295C File Offset: 0x001A0B5C
	public int AddRuinDataWithComponent(RuinData ruin)
	{
		int num = this.AddRuinData(ruin);
		if (ruin.modelIndex == 406)
		{
			this.platformSystem.AddStateArea((uint)(33554432L | (long)num), ruin.pos, 11.25f, 1);
		}
		if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 7)
		{
			this.CreateRuinDisplayComponent(num);
		}
		return num;
	}

	// Token: 0x06001874 RID: 6260 RVA: 0x001A29C4 File Offset: 0x001A0BC4
	public void CreateRuinDisplayComponent(int ruinId)
	{
		int modelIndex = (int)this.ruinPool[ruinId].modelIndex;
		ModelProto modelProto = LDB.models.modelArray[modelIndex];
		if (modelProto == null)
		{
			return;
		}
		PrefabDesc prefabDesc = modelProto.prefabDesc;
		if (prefabDesc == null)
		{
			return;
		}
		this.ruinPool[ruinId].modelId = GameMain.gpuiManager.AddModel(modelIndex, ruinId, this.ruinPool[ruinId].pos, this.ruinPool[ruinId].rot, true);
		if (prefabDesc.minimapType > 0 && this.ruinPool[ruinId].mmblockId == 0)
		{
			this.ruinPool[ruinId].mmblockId = this.blockContainer.AddMiniBlock(ruinId, prefabDesc.minimapType, this.ruinPool[ruinId].pos, this.ruinPool[ruinId].rot, prefabDesc.minimapScl);
		}
		if (prefabDesc.colliders != null && prefabDesc.colliders.Length != 0)
		{
			for (int i = 0; i < prefabDesc.colliders.Length; i++)
			{
				this.ruinPool[ruinId].colliderId = this.planet.physics.AddColliderData(prefabDesc.colliders[i].BindToObject(ruinId, this.ruinPool[ruinId].colliderId, EObjectType.Ruin, this.ruinPool[ruinId].pos, this.ruinPool[ruinId].rot));
			}
		}
	}

	// Token: 0x06001875 RID: 6261 RVA: 0x001A2B34 File Offset: 0x001A0D34
	public void RemoveRuinWithComponet(int id)
	{
		if (id != 0 && this.ruinPool[id].id != 0)
		{
			if (this.ruinPool[id].modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemoveModel((int)this.ruinPool[id].modelIndex, this.ruinPool[id].modelId, true);
				}
				this.ruinPool[id].modelId = 0;
			}
			if (this.ruinPool[id].mmblockId != 0)
			{
				if (this.blockContainer != null)
				{
					this.blockContainer.RemoveMiniBlock(this.ruinPool[id].mmblockId);
				}
				this.ruinPool[id].mmblockId = 0;
			}
			if (this.ruinPool[id].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.ruinPool[id].colliderId);
				}
				this.ruinPool[id].colliderId = 0;
			}
			if (this.ruinPool[id].modelIndex == 406)
			{
				this.platformSystem.RemoveStateArea((uint)(33554432L | (long)id));
			}
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.Ruin);
			this.hashSystemStatic.RemoveObjectFromBucket(this.ruinPool[id].hashAddress);
			this.ruinPool[id].SetEmpty();
			int[] array = this.ruinRecycle;
			int num = this.ruinRecycleCursor;
			this.ruinRecycleCursor = num + 1;
			array[num] = id;
		}
		if (this.planet.physics != null)
		{
			this.planet.physics.NotifyObjectRemove(EObjectType.Ruin, id);
		}
	}

	// Token: 0x06001876 RID: 6262 RVA: 0x001A2D02 File Offset: 0x001A0F02
	public RuinData GetRuinData(int id)
	{
		return this.ruinPool[id];
	}

	// Token: 0x06001877 RID: 6263 RVA: 0x001A2D10 File Offset: 0x001A0F10
	public void RemoveRuinsInArea(Vector3 pos, float areaRadius)
	{
		if (this.tmp_ids == null)
		{
			this.tmp_ids = new int[1024];
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		int ruinsInAreaNonAlloc = this.planet.physics.nearColliderLogic.GetRuinsInAreaNonAlloc(pos, areaRadius, this.tmp_ids);
		if (ruinsInAreaNonAlloc > 0)
		{
			for (int i = 0; i < ruinsInAreaNonAlloc; i++)
			{
				this.RemoveRuinWithComponet(this.tmp_ids[i]);
			}
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
	}

	// Token: 0x06001878 RID: 6264 RVA: 0x001A2D9C File Offset: 0x001A0F9C
	public void NotifyVeinExhausted(int veinType, int veinGroupIndex, Vector3 pos)
	{
		try
		{
			if (veinType != 7)
			{
				if (GameMain.gameScenario != null)
				{
					GameMain.gameScenario.NotifyOnVeinExhausted();
				}
				if (this.gameData.warningSystem != null)
				{
					this.gameData.warningSystem.Broadcast(EBroadcastVocal.MineralDepleted, this, veinType, pos);
				}
				if (this.gameData.statistics != null && this.gameData.statistics.charts != null)
				{
					this.gameData.statistics.charts.NotifyOnVeinExhausted(this.planet.astroId, veinGroupIndex, pos);
				}
			}
			else if (this.gameData.warningSystem != null)
			{
				this.gameData.warningSystem.Broadcast(EBroadcastVocal.OilSeepDepleted, this, veinType, pos);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Vein exhausted event error!\n {0}", ex.Message));
		}
	}

	// Token: 0x06001879 RID: 6265 RVA: 0x001A2E70 File Offset: 0x001A1070
	public void NotifyDroneDelivery(PlanetFactory factory, StationComponent srcStation, StationComponent dstStation, int item, int count)
	{
		try
		{
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.NotifyOnDroneDelivery(factory, srcStation, dstStation, item, count);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Drone Delivery event error!\n {0}", ex.Message));
		}
	}

	// Token: 0x0600187A RID: 6266 RVA: 0x001A2EC0 File Offset: 0x001A10C0
	public void NotifyShipDelivery(int srcPlanet, StationComponent srcStation, int dstPlanet, StationComponent dstStation, int item, int count)
	{
		try
		{
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.NotifyOnShipDelivery(srcPlanet, srcStation, dstPlanet, dstStation, item, count);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Ship Delivery event error!\n {0}", ex.Message));
		}
	}

	// Token: 0x0600187B RID: 6267 RVA: 0x001A2F14 File Offset: 0x001A1114
	public VeinData GetVeinData(int id)
	{
		return this.veinPool[id];
	}

	// Token: 0x14000027 RID: 39
	// (add) Token: 0x0600187C RID: 6268 RVA: 0x001A2F24 File Offset: 0x001A1124
	// (remove) Token: 0x0600187D RID: 6269 RVA: 0x001A2F5C File Offset: 0x001A115C
	public event Action onVeinGroupsRecalculated;

	// Token: 0x0600187E RID: 6270 RVA: 0x001A2F94 File Offset: 0x001A1194
	public void InitVeinGroups(PlanetData planet)
	{
		Mutex veinGroupsLock = planet.veinGroupsLock;
		lock (veinGroupsLock)
		{
			int num = planet.veinGroups.Length;
			int num2 = (num >= 1) ? num : 1;
			this.veinGroups = new VeinGroup[num2];
			Array.Copy(planet.veinGroups, this.veinGroups, num);
			this.veinGroups[0].SetNull();
		}
	}

	// Token: 0x0600187F RID: 6271 RVA: 0x001A3010 File Offset: 0x001A1210
	public void InitVeinGroups(int count)
	{
		if (count < 0)
		{
			count = 0;
		}
		this.veinGroups = new VeinGroup[count + 1];
	}

	// Token: 0x06001880 RID: 6272 RVA: 0x001A3028 File Offset: 0x001A1228
	public int AddVeinGroup(VeinGroup vg)
	{
		if (vg.isNull)
		{
			return 0;
		}
		VeinGroup[] array = this.veinGroups;
		int num = (array == null) ? 0 : array.Length;
		int num2 = num + 1;
		this.veinGroups = new VeinGroup[num2];
		if (array != null)
		{
			Array.Copy(array, this.veinGroups, num);
		}
		this.veinGroups[num] = vg;
		return num;
	}

	// Token: 0x06001881 RID: 6273 RVA: 0x001A3080 File Offset: 0x001A1280
	public int AddVeinGroup(EVeinType vtype, Vector3 npos)
	{
		if (vtype == EVeinType.None)
		{
			return 0;
		}
		VeinGroup veinGroup = default(VeinGroup);
		veinGroup.type = vtype;
		veinGroup.pos = npos;
		VeinGroup[] array = this.veinGroups;
		int num = (array == null) ? 0 : array.Length;
		int num2 = num + 1;
		this.veinGroups = new VeinGroup[num2];
		if (array != null)
		{
			Array.Copy(array, this.veinGroups, num);
		}
		this.veinGroups[num] = veinGroup;
		return num;
	}

	// Token: 0x06001882 RID: 6274 RVA: 0x001A30EC File Offset: 0x001A12EC
	public void RemoveVeinGroup(int vgidx)
	{
		if (vgidx < 0 || vgidx >= this.veinGroups.Length)
		{
			return;
		}
		this.veinGroups[vgidx].SetNull();
		for (int i = 1; i < this.veinCursor; i++)
		{
			if ((int)this.veinPool[i].groupIndex == vgidx)
			{
				this.veinPool[i].groupIndex = 0;
			}
		}
		this.ArrangeVeinGroups();
	}

	// Token: 0x06001883 RID: 6275 RVA: 0x001A3158 File Offset: 0x001A1358
	private void RemoveEmptyVeinGroups()
	{
		bool flag = false;
		for (int i = 1; i < this.veinGroups.Length; i++)
		{
			if (this.veinGroups[i].count <= 0)
			{
				this.veinGroups[i].SetNull();
				flag = true;
			}
		}
		if (flag)
		{
			this.ArrangeVeinGroups();
		}
	}

	// Token: 0x06001884 RID: 6276 RVA: 0x001A31AC File Offset: 0x001A13AC
	private int MergeVeinGroups(bool[] merge_flags)
	{
		if (this.tmp_vein_group_merge == null)
		{
			this.tmp_vein_group_merge = new List<int>();
		}
		this.tmp_vein_group_merge.Clear();
		int num = this.veinGroups.Length;
		for (int i = 1; i < num; i++)
		{
			if (merge_flags[i])
			{
				this.tmp_vein_group_merge.Add(i);
			}
		}
		int count = this.tmp_vein_group_merge.Count;
		if (count == 0)
		{
			return 0;
		}
		int num2 = this.tmp_vein_group_merge[0];
		for (int j = 1; j < count; j++)
		{
			VeinGroup[] array = this.veinGroups;
			int num3 = num2;
			array[num3].amount = array[num3].amount + this.veinGroups[this.tmp_vein_group_merge[j]].amount;
			VeinGroup[] array2 = this.veinGroups;
			int num4 = num2;
			array2[num4].count = array2[num4].count + this.veinGroups[this.tmp_vein_group_merge[j]].count;
			this.veinGroups[this.tmp_vein_group_merge[j]].SetNull();
		}
		if (count > 1)
		{
			for (int k = 1; k < this.veinCursor; k++)
			{
				if (this.veinPool[k].id == k && merge_flags[(int)this.veinPool[k].groupIndex])
				{
					this.veinPool[k].groupIndex = (short)num2;
				}
			}
		}
		this.tmp_vein_group_merge.Clear();
		return num2;
	}

	// Token: 0x06001885 RID: 6277 RVA: 0x001A3318 File Offset: 0x001A1518
	public void ArrangeVeinGroups()
	{
		int num = this.veinGroups.Length;
		if (this.tmp_vein_group_idx_mapping == null || this.tmp_vein_group_idx_mapping.Length < num)
		{
			this.tmp_vein_group_idx_mapping = new int[num * 2];
		}
		int[] array = this.tmp_vein_group_idx_mapping;
		Array.Clear(array, 0, array.Length);
		int num2 = 1;
		for (int i = 1; i < num; i++)
		{
			if (this.veinGroups[i].isNull)
			{
				array[i] = 0;
			}
			else
			{
				if (i > num2)
				{
					this.veinGroups[num2] = this.veinGroups[i];
				}
				array[i] = num2;
				num2++;
			}
		}
		this.gameData.statistics.charts.NotifyVeinGroupStatPlanRemappingIndex(this.planet.astroId, array);
		if (num2 == num)
		{
			return;
		}
		Array sourceArray = this.veinGroups;
		this.veinGroups = new VeinGroup[num2];
		Array.Copy(sourceArray, this.veinGroups, num2);
		for (int j = 1; j < this.veinCursor; j++)
		{
			if (this.veinPool[j].id == j)
			{
				this.veinPool[j].groupIndex = (short)array[(int)this.veinPool[j].groupIndex];
			}
		}
	}

	// Token: 0x06001886 RID: 6278 RVA: 0x001A3444 File Offset: 0x001A1644
	public void AssignGroupIndexForNewVein(ref VeinData veinData)
	{
		if (this.tmp_vein_group_flags == null || this.tmp_vein_group_flags.Length < this.veinGroups.Length)
		{
			this.tmp_vein_group_flags = new bool[this.veinGroups.Length * 2];
		}
		bool[] array = this.tmp_vein_group_flags;
		Array.Clear(array, 0, array.Length);
		EVeinType type = veinData.type;
		for (int i = 1; i < this.veinCursor; i++)
		{
			if (this.veinPool[i].type == type && this.veinPool[i].id != 0 && !array[(int)this.veinPool[i].groupIndex] && (veinData.pos - this.veinPool[i].pos).sqrMagnitude < 100f)
			{
				array[(int)this.veinPool[i].groupIndex] = true;
			}
		}
		int num = this.MergeVeinGroups(array);
		if (num == 0)
		{
			num = this.AddVeinGroup(type, veinData.pos.normalized);
		}
		veinData.groupIndex = (short)num;
		VeinGroup[] array2 = this.veinGroups;
		int num2 = num;
		array2[num2].amount = array2[num2].amount + (long)veinData.amount;
		VeinGroup[] array3 = this.veinGroups;
		int num3 = num;
		array3[num3].count = array3[num3].count + 1;
		if (this.tmp_vein_group_merge.Count > 1)
		{
			this.ArrangeVeinGroups();
		}
	}

	// Token: 0x06001887 RID: 6279 RVA: 0x001A3598 File Offset: 0x001A1798
	public void RecalculateVeinGroup(int vgidx)
	{
		if (vgidx < 0 || vgidx >= this.veinGroups.Length)
		{
			return;
		}
		this.veinGroups[vgidx].pos = Vector3.zero;
		this.veinGroups[vgidx].count = 0;
		this.veinGroups[vgidx].amount = 0L;
		for (int i = 1; i < this.veinCursor; i++)
		{
			if ((int)this.veinPool[i].groupIndex == vgidx && this.veinPool[i].id == i)
			{
				VeinGroup[] array = this.veinGroups;
				array[vgidx].pos = array[vgidx].pos + this.veinPool[i].pos;
				VeinGroup[] array2 = this.veinGroups;
				array2[vgidx].count = array2[vgidx].count + 1;
				VeinGroup[] array3 = this.veinGroups;
				array3[vgidx].amount = array3[vgidx].amount + (long)this.veinPool[i].amount;
			}
		}
		this.veinGroups[vgidx].pos.Normalize();
		if (this.onVeinGroupsRecalculated != null)
		{
			this.onVeinGroupsRecalculated();
		}
	}

	// Token: 0x06001888 RID: 6280 RVA: 0x001A36CC File Offset: 0x001A18CC
	public void RecalculateAllVeinGroups()
	{
		for (int i = 1; i < this.veinGroups.Length; i++)
		{
			this.veinGroups[i].pos = Vector3.zero;
			this.veinGroups[i].count = 0;
			this.veinGroups[i].amount = 0L;
		}
		for (int j = 1; j < this.veinCursor; j++)
		{
			if (this.veinPool[j].id == j)
			{
				int groupIndex = (int)this.veinPool[j].groupIndex;
				this.veinGroups[groupIndex].type = this.veinPool[j].type;
				VeinGroup[] array = this.veinGroups;
				int num = groupIndex;
				array[num].pos = array[num].pos + this.veinPool[j].pos;
				VeinGroup[] array2 = this.veinGroups;
				int num2 = groupIndex;
				array2[num2].count = array2[num2].count + 1;
				VeinGroup[] array3 = this.veinGroups;
				int num3 = groupIndex;
				array3[num3].amount = array3[num3].amount + (long)this.veinPool[j].amount;
			}
		}
		this.veinGroups[0].type = EVeinType.None;
		for (int k = 0; k < this.veinGroups.Length; k++)
		{
			this.veinGroups[k].pos.Normalize();
		}
		this.RemoveEmptyVeinGroups();
		if (this.onVeinGroupsRecalculated != null)
		{
			this.onVeinGroupsRecalculated();
		}
	}

	// Token: 0x06001889 RID: 6281 RVA: 0x001A384C File Offset: 0x001A1A4C
	public void SetVegeCapacity(int newCapacity)
	{
		VegeData[] array = this.vegePool;
		this.vegePool = new VegeData[newCapacity];
		this.vegeRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.vegePool, (newCapacity > this.vegeCapacity) ? this.vegeCapacity : newCapacity);
		}
		this.vegeCapacity = newCapacity;
	}

	// Token: 0x0600188A RID: 6282 RVA: 0x001A38A0 File Offset: 0x001A1AA0
	public int AddVegeData(VegeData vege)
	{
		if (this.vegeRecycleCursor > 0)
		{
			int[] array = this.vegeRecycle;
			int num = this.vegeRecycleCursor - 1;
			this.vegeRecycleCursor = num;
			vege.id = array[num];
		}
		else
		{
			int num = this.vegeCursor;
			this.vegeCursor = num + 1;
			vege.id = num;
		}
		if (vege.id == this.vegeCapacity)
		{
			this.SetVegeCapacity(this.vegeCapacity * 2);
		}
		this.vegePool[vege.id] = vege;
		this.vegePool[vege.id].hashAddress = this.hashSystemStatic.AddObjectToBucket(vege.id, vege.pos, EObjectType.Vegetable);
		return vege.id;
	}

	// Token: 0x0600188B RID: 6283 RVA: 0x001A3954 File Offset: 0x001A1B54
	public void RemoveVegeData(int id)
	{
		if (this.vegePool[id].id != 0)
		{
			this.vegePool[id].SetNull();
			int[] array = this.vegeRecycle;
			int num = this.vegeRecycleCursor;
			this.vegeRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x0600188C RID: 6284 RVA: 0x001A39A0 File Offset: 0x001A1BA0
	public void RemoveVegeWithComponents(int id)
	{
		if (this.vegePool[id].id != 0)
		{
			if (this.vegePool[id].combatStatId != 0)
			{
				int combatStatId = this.vegePool[id].combatStatId;
				this.skillSystem.OnRemovingSkillTarget(combatStatId, this.skillSystem.combatStats.buffer[combatStatId].originAstroId, ETargetType.CombatStat);
				this.skillSystem.combatStats.Remove(combatStatId);
				this.vegePool[id].combatStatId = 0;
			}
			if (GameMain.isRunning && GameMain.gameScenario != null)
			{
				GameMain.gameScenario.NotifyOnVegetableRemoved((int)this.vegePool[id].protoId);
			}
			if (this.vegePool[id].modelId != 0)
			{
				if (GameMain.gpuiManager.activeFactory == this)
				{
					GameMain.gpuiManager.RemoveModel((int)this.vegePool[id].modelIndex, this.vegePool[id].modelId, true);
				}
				this.vegePool[id].modelId = 0;
			}
			if (this.vegePool[id].colliderId != 0)
			{
				if (this.planet.physics != null)
				{
					this.planet.physics.RemoveLinkedColliderData(this.vegePool[id].colliderId);
				}
				this.vegePool[id].colliderId = 0;
			}
			this.hashSystemStatic.RemoveObjectFromBucket(this.vegePool[id].hashAddress);
			this.skillSystem.OnRemovingSkillTarget(id, this.planet.astroId, ETargetType.Vegetable);
			this.vegePool[id].SetNull();
			int[] array = this.vegeRecycle;
			int num = this.vegeRecycleCursor;
			this.vegeRecycleCursor = num + 1;
			array[num] = id;
			if (this.planet.physics != null)
			{
				this.planet.physics.NotifyObjectRemove(EObjectType.Vegetable, id);
			}
		}
	}

	// Token: 0x0600188D RID: 6285 RVA: 0x001A3B8C File Offset: 0x001A1D8C
	public void KillVegeFinally(int id, int effect)
	{
		if (GameMain.gameTick > 0L && this.gameData.localLoadedPlanetFactory == this)
		{
			VegeProto vegeProto = LDB.veges.Select((int)this.vegePool[id].protoId);
			if (vegeProto != null)
			{
				VFEffectEmitter.Emit(vegeProto.MiningEffect, this.vegePool[id].pos, this.vegePool[id].rot);
				VFAudio.Create(vegeProto.MiningAudio, null, this.vegePool[id].pos, true, 1, -1, -1L);
			}
		}
		this.RemoveVegeWithComponents(id);
	}

	// Token: 0x0600188E RID: 6286 RVA: 0x001A3C28 File Offset: 0x001A1E28
	public void InitVegeHashAddress()
	{
		for (int i = 1; i < this.vegeCursor; i++)
		{
			if (this.vegePool[i].id == i)
			{
				this.vegePool[i].hashAddress = this.hashSystemStatic.AddObjectToBucket(i, this.vegePool[i].pos, EObjectType.Vegetable);
			}
		}
	}

	// Token: 0x0600188F RID: 6287 RVA: 0x001A3C89 File Offset: 0x001A1E89
	public VegeData GetVegeData(int id)
	{
		return this.vegePool[id];
	}

	// Token: 0x06001890 RID: 6288 RVA: 0x001A3C98 File Offset: 0x001A1E98
	public void RuinDataGameTick(long time)
	{
		if (this.ruinPool != null)
		{
			int num = this.ruinCursor - 1;
			long num2 = (time - 1L) * (long)num / 60L;
			long num3 = time * (long)num / 60L;
			for (long num4 = num2; num4 < num3; num4 += 1L)
			{
				int num5 = (int)(num4 % (long)num);
				num5++;
				if (this.ruinPool[num5].id == num5 && !this.ruinPool[num5].UpdateLifeTime())
				{
					this.planet.factory.RemoveRuinWithComponet(num5);
				}
			}
		}
	}

	// Token: 0x06001891 RID: 6289 RVA: 0x001A3D18 File Offset: 0x001A1F18
	public void ApplyEntityOutput(int entityId, int outputId, int thisSlotId, int otherSlotId, int offset)
	{
		this.ApplyInsertTarget(entityId, outputId, thisSlotId, offset);
		if (outputId > 0)
		{
			this.ApplyPickTarget(outputId, entityId, otherSlotId, 0);
		}
	}

	// Token: 0x06001892 RID: 6290 RVA: 0x001A3D34 File Offset: 0x001A1F34
	public void ApplyEntityInput(int entityId, int inputId, int thisSlotId, int otherSlotId, int offset)
	{
		this.ApplyPickTarget(entityId, inputId, thisSlotId, offset);
		if (inputId > 0)
		{
			this.ApplyInsertTarget(inputId, entityId, otherSlotId, 0);
		}
	}

	// Token: 0x06001893 RID: 6291 RVA: 0x001A3D50 File Offset: 0x001A1F50
	public void ApplyEntityDisconnection(int otherEntityId, int removingEntityId, int otherSlotId, int removingSlotId)
	{
		if (otherEntityId == 0)
		{
			return;
		}
		int inserterId = this.entityPool[otherEntityId].inserterId;
		if (inserterId > 0)
		{
			if (this.factorySystem.inserterPool[inserterId].insertTarget == removingEntityId)
			{
				this.factorySystem.SetInserterInsertTarget(inserterId, 0, 0);
			}
			if (this.factorySystem.inserterPool[inserterId].pickTarget == removingEntityId)
			{
				this.factorySystem.SetInserterPickTarget(inserterId, 0, 0);
			}
		}
		int minerId = this.entityPool[otherEntityId].minerId;
		if (minerId > 0 && this.factorySystem.minerPool[minerId].insertTarget == removingEntityId)
		{
			this.factorySystem.SetMinerInsertTarget(minerId, 0);
		}
		int beltId = this.entityPool[removingEntityId].beltId;
		int fractionatorId = this.entityPool[otherEntityId].fractionatorId;
		if (fractionatorId > 0 && beltId > 0)
		{
			this.factorySystem.DisconnectToFractionator(fractionatorId, beltId);
		}
		int labId = this.entityPool[otherEntityId].labId;
		if (labId > 0)
		{
			int labId2 = this.entityPool[removingEntityId].labId;
			if (this.factorySystem.labPool[labId].nextLabId == labId2)
			{
				this.factorySystem.SetLabNextTarget(labId, 0);
			}
		}
		int tankId = this.entityPool[otherEntityId].tankId;
		if (tankId > 0)
		{
			int tankId2 = this.entityPool[removingEntityId].tankId;
			if (this.factoryStorage.tankPool[tankId].nextTankId == tankId2)
			{
				this.factoryStorage.SetTankNextTarget(tankId, 0);
			}
			if (beltId > 0)
			{
				this.factoryStorage.DisconnectToTank(tankId, beltId);
			}
		}
		int storageId = this.entityPool[otherEntityId].storageId;
		if (storageId > 0)
		{
			int storageId2 = this.entityPool[removingEntityId].storageId;
			if (this.factoryStorage.storagePool[storageId] != null && this.factoryStorage.storagePool[storageId].next == storageId2)
			{
				this.factoryStorage.storagePool[storageId].CutNext();
			}
		}
		int splitterId = this.entityPool[otherEntityId].splitterId;
		if (splitterId > 0 && beltId > 0)
		{
			this.cargoTraffic.DisconnectToSplitter(splitterId, beltId);
		}
		int storageId3 = this.entityPool[removingEntityId].storageId;
		if (splitterId > 0 && storageId3 > 0)
		{
			if (this.cargoTraffic.splitterPool[splitterId].topId == removingEntityId)
			{
				this.cargoTraffic.splitterPool[splitterId].topId = 0;
			}
			if (this.cargoTraffic.splitterPool[splitterId].bottomId == removingEntityId)
			{
				this.cargoTraffic.splitterPool[splitterId].bottomId = 0;
			}
		}
		int monitorId = this.entityPool[otherEntityId].monitorId;
		if (monitorId > 0 && beltId > 0)
		{
			this.cargoTraffic.DisconnectToMonitor(monitorId, beltId);
		}
		int spraycoaterId = this.entityPool[otherEntityId].spraycoaterId;
		if (spraycoaterId > 0 && beltId > 0)
		{
			this.cargoTraffic.DisconnectToSpraycoater(spraycoaterId, beltId);
		}
		int turretId = this.entityPool[otherEntityId].turretId;
		if (turretId > 0 && beltId > 0)
		{
			this.defenseSystem.DisconnectToTurret(turretId, beltId);
		}
		int pilerId = this.entityPool[otherEntityId].pilerId;
		if (pilerId > 0 && beltId > 0)
		{
			this.cargoTraffic.DisconnectToPiler(pilerId, beltId);
			if (this.cargoTraffic.PilerConnCount(pilerId) == 0)
			{
				this.cargoTraffic.TakeBackItems_Piler(this.gameData.mainPlayer, pilerId);
			}
		}
		int powerExcId = this.entityPool[otherEntityId].powerExcId;
		if (powerExcId > 0 && beltId > 0)
		{
			this.powerSystem.DisconnectToExchanger(powerExcId, beltId);
		}
		int stationId = this.entityPool[otherEntityId].stationId;
		if (stationId > 0 && beltId > 0)
		{
			StationComponent stationComponent = this.transport.stationPool[stationId];
			stationComponent.slots[otherSlotId].dir = IODir.None;
			stationComponent.slots[otherSlotId].beltId = 0;
			stationComponent.slots[otherSlotId].counter = 0;
		}
	}

	// Token: 0x06001894 RID: 6292 RVA: 0x001A4160 File Offset: 0x001A2360
	public void ApplyInsertTarget(int entityId, int insertTarget, int slotId, int offset)
	{
		if (entityId == 0)
		{
			return;
		}
		if (insertTarget < 0)
		{
			Assert.CannotBeReached();
			insertTarget = 0;
		}
		int inserterId = this.entityPool[entityId].inserterId;
		if (inserterId > 0)
		{
			this.factorySystem.SetInserterInsertTarget(inserterId, insertTarget, offset);
		}
		int minerId = this.entityPool[entityId].minerId;
		if (minerId > 0 && this.entityPool[entityId].stationId == 0)
		{
			this.factorySystem.SetMinerInsertTarget(minerId, insertTarget);
		}
		int labId = this.entityPool[entityId].labId;
		int labId2 = this.entityPool[insertTarget].labId;
		if (labId > 0 && labId2 > 0)
		{
			this.factorySystem.SetLabNextTarget(labId, insertTarget);
		}
		int tankId = this.entityPool[entityId].tankId;
		int tankId2 = this.entityPool[insertTarget].tankId;
		if (tankId > 0 && tankId2 > 0)
		{
			this.factoryStorage.SetTankNextTarget(tankId, insertTarget);
		}
		int storageId = this.entityPool[entityId].storageId;
		int storageId2 = this.entityPool[insertTarget].storageId;
		if (storageId > 0 && storageId2 > 0)
		{
			this.factoryStorage.SetStorageNext(storageId, storageId2);
		}
		int splitterId = this.entityPool[insertTarget].splitterId;
		if (storageId > 0 && splitterId > 0)
		{
			this.cargoTraffic.splitterPool[splitterId].bottomId = entityId;
		}
		int stationId = this.entityPool[entityId].stationId;
		int beltId = this.entityPool[insertTarget].beltId;
		if (stationId > 0 && beltId > 0)
		{
			StationComponent stationComponent = this.transport.stationPool[stationId];
			stationComponent.slots[slotId].dir = IODir.Output;
			stationComponent.slots[slotId].beltId = beltId;
			stationComponent.slots[slotId].counter = 0;
		}
	}

	// Token: 0x06001895 RID: 6293 RVA: 0x001A4338 File Offset: 0x001A2538
	public void ApplyPickTarget(int entityId, int pickTarget, int slotId, int offset)
	{
		if (entityId == 0)
		{
			return;
		}
		if (pickTarget < 0)
		{
			Assert.CannotBeReached();
			pickTarget = 0;
		}
		int inserterId = this.entityPool[entityId].inserterId;
		if (inserterId > 0)
		{
			this.factorySystem.SetInserterPickTarget(inserterId, pickTarget, offset);
		}
		int stationId = this.entityPool[entityId].stationId;
		int beltId = this.entityPool[pickTarget].beltId;
		if (stationId > 0 && beltId > 0)
		{
			StationComponent stationComponent = this.transport.stationPool[stationId];
			stationComponent.slots[slotId].dir = IODir.Input;
			stationComponent.slots[slotId].beltId = beltId;
			stationComponent.slots[slotId].storageIdx = 0;
			stationComponent.slots[slotId].counter = 0;
		}
		int storageId = this.entityPool[entityId].storageId;
		int splitterId = this.entityPool[pickTarget].splitterId;
		if (storageId > 0 && splitterId > 0)
		{
			this.cargoTraffic.splitterPool[splitterId].topId = entityId;
		}
	}

	// Token: 0x06001896 RID: 6294 RVA: 0x001A443C File Offset: 0x001A263C
	public void WriteExtraInfoOnEntity(int entityId, string info)
	{
		if (entityId > 0 && this.entityPool[entityId].id == entityId)
		{
			int num = this.entityPool[entityId].extraInfoId;
			if (string.IsNullOrEmpty(info))
			{
				if (num > 0)
				{
					this.digitalSystem.RemoveExtraInfoComponent(num);
				}
				this.entityPool[entityId].extraInfoId = 0;
				return;
			}
			if (num == 0)
			{
				num = this.digitalSystem.NewExtraInfoComponent(EObjectType.None, entityId);
				Assert.True(num == this.entityPool[entityId].extraInfoId);
			}
			if (num > 0)
			{
				this.digitalSystem.extraInfoes.buffer[num].info = info;
			}
		}
	}

	// Token: 0x06001897 RID: 6295 RVA: 0x001A44EC File Offset: 0x001A26EC
	public string ReadExtraInfoOnEntity(int entityId)
	{
		string text = null;
		if (entityId > 0 && this.entityPool[entityId].id == entityId)
		{
			int extraInfoId = this.entityPool[entityId].extraInfoId;
			if (extraInfoId > 0)
			{
				text = this.digitalSystem.extraInfoes.buffer[extraInfoId].info;
			}
		}
		if (text != null)
		{
			return text;
		}
		return "";
	}

	// Token: 0x06001898 RID: 6296 RVA: 0x001A454C File Offset: 0x001A274C
	public void WriteExtraInfoOnPrebuild(int prebuildId, string info)
	{
		if (prebuildId > 0 && this.prebuildPool[prebuildId].id == prebuildId)
		{
			int num = this.prebuildPool[prebuildId].extraInfoId;
			if (string.IsNullOrEmpty(info))
			{
				if (num > 0)
				{
					this.digitalSystem.RemoveExtraInfoComponent(num);
				}
				this.prebuildPool[prebuildId].extraInfoId = 0;
				return;
			}
			if (num == 0)
			{
				num = this.digitalSystem.NewExtraInfoComponent(EObjectType.Prebuild, prebuildId);
				Assert.True(num == this.prebuildPool[prebuildId].extraInfoId);
			}
			if (num > 0)
			{
				this.digitalSystem.extraInfoes.buffer[num].info = info;
			}
		}
	}

	// Token: 0x06001899 RID: 6297 RVA: 0x001A45FC File Offset: 0x001A27FC
	public string ReadExtraInfoOnPrebuild(int prebuildId)
	{
		string text = null;
		if (prebuildId > 0 && this.prebuildPool[prebuildId].id == prebuildId)
		{
			int extraInfoId = this.prebuildPool[prebuildId].extraInfoId;
			if (extraInfoId > 0)
			{
				text = this.digitalSystem.extraInfoes.buffer[extraInfoId].info;
			}
		}
		if (text != null)
		{
			return text;
		}
		return "";
	}

	// Token: 0x0600189A RID: 6298 RVA: 0x001A465C File Offset: 0x001A285C
	public void WriteMarkerName(int id, string name)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			buffer[id].name = name;
			return;
		}
		Debug.LogError(string.Format("WriteMarkerName [{0}] Error", id));
	}

	// Token: 0x0600189B RID: 6299 RVA: 0x001A46B8 File Offset: 0x001A28B8
	public string ReadMarkerName(int id)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			if (buffer[id].name != null)
			{
				return buffer[id].name;
			}
		}
		else
		{
			Debug.LogError(string.Format("ReadMarkerName [{0}] Error", id));
		}
		return "";
	}

	// Token: 0x0600189C RID: 6300 RVA: 0x001A4720 File Offset: 0x001A2920
	public void WriteMarkerWord(int id, string word)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			buffer[id].word = word;
			return;
		}
		Debug.LogError(string.Format("WriteMarkerWord [{0}] Error", id));
	}

	// Token: 0x0600189D RID: 6301 RVA: 0x001A477C File Offset: 0x001A297C
	public string ReadMarkerWord(int id)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			if (buffer[id].word != null)
			{
				return buffer[id].word;
			}
		}
		else
		{
			Debug.LogError(string.Format("ReadMarkerWord [{0}] Error", id));
		}
		return "";
	}

	// Token: 0x0600189E RID: 6302 RVA: 0x001A47E4 File Offset: 0x001A29E4
	public void WriteMarkerTags(int id, string tags)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			buffer[id].tags = tags;
			return;
		}
		Debug.LogError(string.Format("WriteMarkerTags [{0}] Error", id));
	}

	// Token: 0x0600189F RID: 6303 RVA: 0x001A4840 File Offset: 0x001A2A40
	public string ReadMarkerTags(int id)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			if (buffer[id].tags != null)
			{
				return buffer[id].tags;
			}
		}
		else
		{
			Debug.LogError(string.Format("ReadMarkerTags [{0}] Error", id));
		}
		return "";
	}

	// Token: 0x060018A0 RID: 6304 RVA: 0x001A48A8 File Offset: 0x001A2AA8
	public void WriteMarkerComment(int id, string comment)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			if (buffer[id].todo == null)
			{
				buffer[id].todo = this.gameData.galacticDigital.AddTodoModule(ETodoModuleOwnerType.Entity, buffer[id].entityId);
			}
			buffer[id].todo.content = comment;
			return;
		}
		Debug.LogError(string.Format("WriteMarkerComment [{0}] Error", id));
	}

	// Token: 0x060018A1 RID: 6305 RVA: 0x001A4934 File Offset: 0x001A2B34
	public string ReadMarkerComment(int id)
	{
		ObjectPool<MarkerComponent> markers = this.digitalSystem.markers;
		MarkerComponent[] buffer = markers.buffer;
		if ((ulong)id < (ulong)((long)markers.cursor) && buffer[id] != null && buffer[id].id == id)
		{
			TodoModule todo = buffer[id].todo;
			if (((todo != null) ? todo.content : null) != null)
			{
				return buffer[id].todo.content;
			}
		}
		else
		{
			Debug.LogError(string.Format("ReadMarkerComment [{0}] Error", id));
		}
		return "";
	}

	// Token: 0x060018A2 RID: 6306 RVA: 0x001A49B0 File Offset: 0x001A2BB0
	public void EnableEntityWarning(int entityId)
	{
		if (entityId > 0 && this.entityPool[entityId].id == entityId && this.entityPool[entityId].warningId == 0)
		{
			Assert.True(this.gameData.warningSystem.NewWarningData(this.index, entityId, 0).id == this.entityPool[entityId].warningId);
		}
	}

	// Token: 0x060018A3 RID: 6307 RVA: 0x001A4A20 File Offset: 0x001A2C20
	public void DisableEntityWarning(int entityId)
	{
		if (entityId > 0 && this.entityPool[entityId].id == entityId)
		{
			this.gameData.warningSystem.RemoveWarningData(this.entityPool[entityId].warningId);
		}
		this.entityPool[entityId].warningId = 0;
	}

	// Token: 0x060018A4 RID: 6308 RVA: 0x001A4A78 File Offset: 0x001A2C78
	public void AddPrebuildWarning(int prebuildId)
	{
		if (prebuildId > 0 && this.prebuildPool[prebuildId].id == prebuildId && this.prebuildPool[prebuildId].warningId == 0)
		{
			int id;
			if (this.prebuildPool[prebuildId].isDestroyed)
			{
				id = this.gameData.warningSystem.NewWarningData(this.index, -prebuildId, 513).id;
			}
			else
			{
				id = this.gameData.warningSystem.NewWarningData(this.index, -prebuildId, 405).id;
			}
			Assert.True(id == this.prebuildPool[prebuildId].warningId);
		}
	}

	// Token: 0x060018A5 RID: 6309 RVA: 0x001A4B2C File Offset: 0x001A2D2C
	public void RemovePrebuildWarning(int prebuildId)
	{
		if (prebuildId > 0 && this.prebuildPool[prebuildId].id == prebuildId)
		{
			this.gameData.warningSystem.RemoveWarningData(this.prebuildPool[prebuildId].warningId);
		}
		this.prebuildPool[prebuildId].warningId = 0;
	}

	// Token: 0x060018A6 RID: 6310 RVA: 0x001A4B84 File Offset: 0x001A2D84
	public void AlterPrebuildModelState(int prebuildId)
	{
		if (prebuildId > 0 && this.prebuildPool[prebuildId].id == prebuildId)
		{
			GameMain.gpuiManager.AlterPrebuildModel((int)this.prebuildPool[prebuildId].modelIndex, this.prebuildPool[prebuildId].modelId, this.prebuildPool[prebuildId].id, this.prebuildPool[prebuildId].pos, this.prebuildPool[prebuildId].rot, true);
		}
	}

	// Token: 0x060018A7 RID: 6311 RVA: 0x001A4C0C File Offset: 0x001A2E0C
	public void CopyBuildingSetting(int objectId)
	{
		if (BuildingParameters.clipboard.CopyFromFactoryObject(objectId, this, true, false))
		{
			VFAudio.Create("equip-0", GameMain.mainPlayer.transform, Vector3.zero, true, 0, -1, -1L);
			string text = BuildingParameters.clipboard.CopiedTipText();
			if (!string.IsNullOrEmpty(text))
			{
				UIRealtimeTip.Popup(text, false, 0);
			}
			this.OnCopyBuildingSetting(objectId);
		}
	}

	// Token: 0x060018A8 RID: 6312 RVA: 0x001A4C6C File Offset: 0x001A2E6C
	public void PasteBuildingSetting(int objectId)
	{
		if (BuildingParameters.clipboard.PasteToFactoryObject(objectId, this))
		{
			VFAudio.Create("equip-1", GameMain.mainPlayer.transform, Vector3.zero, true, 4, -1, -1L);
			string text = BuildingParameters.clipboard.PastedTipText();
			if (!string.IsNullOrEmpty(text))
			{
				UIRealtimeTip.Popup(text, false, 0);
			}
			if (objectId > 0)
			{
				this.EntityAutoReplenishIfNeeded(objectId, Vector2.zero, true);
			}
			this.OnPasteBuildingSetting(objectId);
		}
	}

	// Token: 0x14000028 RID: 40
	// (add) Token: 0x060018A9 RID: 6313 RVA: 0x001A4CDC File Offset: 0x001A2EDC
	// (remove) Token: 0x060018AA RID: 6314 RVA: 0x001A4D14 File Offset: 0x001A2F14
	public event Action<int> onSinglyBuild;

	// Token: 0x14000029 RID: 41
	// (add) Token: 0x060018AB RID: 6315 RVA: 0x001A4D4C File Offset: 0x001A2F4C
	// (remove) Token: 0x060018AC RID: 6316 RVA: 0x001A4D84 File Offset: 0x001A2F84
	public event Action<int> onBuild;

	// Token: 0x1400002A RID: 42
	// (add) Token: 0x060018AD RID: 6317 RVA: 0x001A4DBC File Offset: 0x001A2FBC
	// (remove) Token: 0x060018AE RID: 6318 RVA: 0x001A4DF4 File Offset: 0x001A2FF4
	public event Action<int> onDismantle;

	// Token: 0x1400002B RID: 43
	// (add) Token: 0x060018AF RID: 6319 RVA: 0x001A4E2C File Offset: 0x001A302C
	// (remove) Token: 0x060018B0 RID: 6320 RVA: 0x001A4E64 File Offset: 0x001A3064
	public event Action<int> onKill;

	// Token: 0x1400002C RID: 44
	// (add) Token: 0x060018B1 RID: 6321 RVA: 0x001A4E9C File Offset: 0x001A309C
	// (remove) Token: 0x060018B2 RID: 6322 RVA: 0x001A4ED4 File Offset: 0x001A30D4
	public event Action<int, int, int> onUpgrade;

	// Token: 0x1400002D RID: 45
	// (add) Token: 0x060018B3 RID: 6323 RVA: 0x001A4F0C File Offset: 0x001A310C
	// (remove) Token: 0x060018B4 RID: 6324 RVA: 0x001A4F44 File Offset: 0x001A3144
	public event Action<int> beforeDismantle;

	// Token: 0x1400002E RID: 46
	// (add) Token: 0x060018B5 RID: 6325 RVA: 0x001A4F7C File Offset: 0x001A317C
	// (remove) Token: 0x060018B6 RID: 6326 RVA: 0x001A4FB4 File Offset: 0x001A31B4
	public event Action<int> beforeKill;

	// Token: 0x1400002F RID: 47
	// (add) Token: 0x060018B7 RID: 6327 RVA: 0x001A4FEC File Offset: 0x001A31EC
	// (remove) Token: 0x060018B8 RID: 6328 RVA: 0x001A5024 File Offset: 0x001A3224
	public event Action<int> onCraftKill;

	// Token: 0x14000030 RID: 48
	// (add) Token: 0x060018B9 RID: 6329 RVA: 0x001A505C File Offset: 0x001A325C
	// (remove) Token: 0x060018BA RID: 6330 RVA: 0x001A5094 File Offset: 0x001A3294
	public event Action<int> beforeCraftKill;

	// Token: 0x14000031 RID: 49
	// (add) Token: 0x060018BB RID: 6331 RVA: 0x001A50CC File Offset: 0x001A32CC
	// (remove) Token: 0x060018BC RID: 6332 RVA: 0x001A5100 File Offset: 0x001A3300
	public static event Action<PlanetFactory, int, int> onFactoryBuildEntitySingly;

	// Token: 0x14000032 RID: 50
	// (add) Token: 0x060018BD RID: 6333 RVA: 0x001A5134 File Offset: 0x001A3334
	// (remove) Token: 0x060018BE RID: 6334 RVA: 0x001A5168 File Offset: 0x001A3368
	public static event Action<PlanetFactory, int, int> onFactoryBuildEntity;

	// Token: 0x14000033 RID: 51
	// (add) Token: 0x060018BF RID: 6335 RVA: 0x001A519C File Offset: 0x001A339C
	// (remove) Token: 0x060018C0 RID: 6336 RVA: 0x001A51D0 File Offset: 0x001A33D0
	public static event Action<PlanetFactory, int> onFactoryDismantleObject;

	// Token: 0x14000034 RID: 52
	// (add) Token: 0x060018C1 RID: 6337 RVA: 0x001A5204 File Offset: 0x001A3404
	// (remove) Token: 0x060018C2 RID: 6338 RVA: 0x001A5238 File Offset: 0x001A3438
	public static event Action<PlanetFactory, int, int> onFactoryKillObject;

	// Token: 0x14000035 RID: 53
	// (add) Token: 0x060018C3 RID: 6339 RVA: 0x001A526C File Offset: 0x001A346C
	// (remove) Token: 0x060018C4 RID: 6340 RVA: 0x001A52A0 File Offset: 0x001A34A0
	public static event Action<PlanetFactory, int> beforeFactoryDismantleObject;

	// Token: 0x14000036 RID: 54
	// (add) Token: 0x060018C5 RID: 6341 RVA: 0x001A52D4 File Offset: 0x001A34D4
	// (remove) Token: 0x060018C6 RID: 6342 RVA: 0x001A5308 File Offset: 0x001A3508
	public static event Action<PlanetFactory, int> beforeFactoryKillEntity;

	// Token: 0x14000037 RID: 55
	// (add) Token: 0x060018C7 RID: 6343 RVA: 0x001A533C File Offset: 0x001A353C
	// (remove) Token: 0x060018C8 RID: 6344 RVA: 0x001A5370 File Offset: 0x001A3570
	public static event Action<PlanetFactory, int> onFactoryCopyBuildingSetting;

	// Token: 0x14000038 RID: 56
	// (add) Token: 0x060018C9 RID: 6345 RVA: 0x001A53A4 File Offset: 0x001A35A4
	// (remove) Token: 0x060018CA RID: 6346 RVA: 0x001A53D8 File Offset: 0x001A35D8
	public static event Action<PlanetFactory, int> onFactoryPasteBuildingSetting;

	// Token: 0x060018CB RID: 6347 RVA: 0x001A540B File Offset: 0x001A360B
	public static void ClearStaticEvents()
	{
		PlanetFactory.onFactoryBuildEntitySingly = null;
		PlanetFactory.onFactoryBuildEntity = null;
		PlanetFactory.onFactoryDismantleObject = null;
		PlanetFactory.onFactoryKillObject = null;
		PlanetFactory.beforeFactoryDismantleObject = null;
		PlanetFactory.beforeFactoryKillEntity = null;
		PlanetFactory.onFactoryCopyBuildingSetting = null;
		PlanetFactory.onFactoryPasteBuildingSetting = null;
	}

	// Token: 0x060018CC RID: 6348 RVA: 0x001A543D File Offset: 0x001A363D
	private void OnSinglyBuildEntity(int newEntityId, int oldPrebuildId)
	{
		if (this.onSinglyBuild != null)
		{
			this.onSinglyBuild(newEntityId);
		}
		if (PlanetFactory.onFactoryBuildEntitySingly != null)
		{
			PlanetFactory.onFactoryBuildEntitySingly(this, newEntityId, oldPrebuildId);
		}
	}

	// Token: 0x060018CD RID: 6349 RVA: 0x001A5467 File Offset: 0x001A3667
	private void OnBuildEntity(int newEntityId, int oldPrebuildId)
	{
		if (this.onBuild != null)
		{
			this.onBuild(newEntityId);
		}
		if (PlanetFactory.onFactoryBuildEntity != null)
		{
			PlanetFactory.onFactoryBuildEntity(this, newEntityId, oldPrebuildId);
		}
	}

	// Token: 0x060018CE RID: 6350 RVA: 0x001A5491 File Offset: 0x001A3691
	private void OnDismantleObject(int objectId)
	{
		if (this.onDismantle != null)
		{
			this.onDismantle(objectId);
		}
		if (PlanetFactory.onFactoryDismantleObject != null)
		{
			PlanetFactory.onFactoryDismantleObject(this, objectId);
		}
	}

	// Token: 0x060018CF RID: 6351 RVA: 0x001A54BA File Offset: 0x001A36BA
	private void OnKillObject(int newPrebuildId, int oldObjId)
	{
		if (this.onKill != null)
		{
			this.onKill(oldObjId);
		}
		if (PlanetFactory.onFactoryKillObject != null)
		{
			PlanetFactory.onFactoryKillObject(this, newPrebuildId, oldObjId);
		}
	}

	// Token: 0x060018D0 RID: 6352 RVA: 0x001A54E4 File Offset: 0x001A36E4
	private void BeforeDismantleObject(int objId)
	{
		if (this.beforeDismantle != null)
		{
			this.beforeDismantle(objId);
		}
		if (PlanetFactory.beforeFactoryDismantleObject != null)
		{
			PlanetFactory.beforeFactoryDismantleObject(this, objId);
		}
	}

	// Token: 0x060018D1 RID: 6353 RVA: 0x001A550D File Offset: 0x001A370D
	private void BeforeKillObject(int objId)
	{
		if (this.beforeKill != null)
		{
			this.beforeKill(objId);
		}
		if (PlanetFactory.beforeFactoryKillEntity != null)
		{
			PlanetFactory.beforeFactoryKillEntity(this, objId);
		}
	}

	// Token: 0x060018D2 RID: 6354 RVA: 0x001A5536 File Offset: 0x001A3736
	private void OnCopyBuildingSetting(int entityId)
	{
		if (PlanetFactory.onFactoryCopyBuildingSetting != null)
		{
			PlanetFactory.onFactoryCopyBuildingSetting(this, entityId);
		}
	}

	// Token: 0x060018D3 RID: 6355 RVA: 0x001A554B File Offset: 0x001A374B
	private void OnPasteBuildingSetting(int entityId)
	{
		if (PlanetFactory.onFactoryPasteBuildingSetting != null)
		{
			PlanetFactory.onFactoryPasteBuildingSetting(this, entityId);
		}
	}

	// Token: 0x060018D4 RID: 6356 RVA: 0x001A5560 File Offset: 0x001A3760
	public void BuildFinally(Player player, int prebuildId, bool autoRefresh = true, bool flattenTerrain = true)
	{
		if (prebuildId != 0)
		{
			PrebuildData prebuildData = this.prebuildPool[prebuildId];
			if (prebuildData.id == prebuildId)
			{
				ItemProto itemProto = LDB.items.Select((int)prebuildData.protoId);
				bool flag = this.planet.type == EPlanetType.Gas;
				if (itemProto != null && !flag && flattenTerrain)
				{
					int num = this.FlattenTerrain(prebuildData.pos, prebuildData.rot, new Bounds(itemProto.prefabDesc.buildCollider.pos, itemProto.prefabDesc.buildCollider.ext * 2f), 6f, 1f, false, false, true, autoRefresh, default(Bounds));
					player.SetSandCount(player.sandCount + (long)num, ESandSource.Build);
				}
				EntityData entity = default(EntityData);
				entity.protoId = prebuildData.protoId;
				entity.modelIndex = prebuildData.modelIndex;
				entity.pos = prebuildData.pos;
				entity.rot = prebuildData.rot;
				entity.alt = entity.pos.magnitude;
				entity.tilt = prebuildData.tilt;
				entity.localized = (this.planet == this.gameData.localPlanet && this.planet.factoryLoaded);
				int num2 = this.AddEntityDataWithComponents(entity, prebuildId);
				player.controller.actionBuild.NotifyBuilt(-prebuildId, num2);
				this.RemovePrebuildWithComponents(prebuildId);
				GameMain.history.MarkItemBuilt((int)prebuildData.protoId, 1);
				if (this.entityPool[num2].beltId > 0)
				{
					this.OnBeltBuilt(num2);
				}
				if (this.entityPool[num2].inserterId > 0)
				{
					this.OnInserterBuilt(num2);
				}
				if (itemProto.prefabDesc.addonType != EAddonType.None)
				{
					this.OnAddonBuilt(num2);
				}
				this.OnBuildEntity(num2, prebuildId);
				if (!PlanetFactory.batchBuild)
				{
					this.OnSinglyBuildEntity(num2, prebuildId);
				}
				GameMain.gameScenario.NotifyOnBuild(this.planet.id, (int)this.entityPool[num2].protoId, num2);
			}
		}
		if (this.entityCount > 1 && this.entityCount < 10)
		{
			GameMain.history.SetStarPin(this.planet.star.id, EPin.Show);
		}
		this.CheckDysonSphereConditionAfterConstruction();
	}

	// Token: 0x060018D5 RID: 6357 RVA: 0x001A57A8 File Offset: 0x001A39A8
	public void UpgradeFinally(Player player, int objId, ItemProto replace_item_proto)
	{
		if (objId == 0 || replace_item_proto == null)
		{
			return;
		}
		int num = -objId;
		if (objId > 0)
		{
			player.controller.actionInspect.NotifyObjectUpgrade(EObjectType.None, objId);
			int protoId = (int)this.entityPool[objId].protoId;
			int id = replace_item_proto.ID;
			this.UpgradeEntityWithComponents(objId, replace_item_proto);
			if (this.onUpgrade != null)
			{
				this.onUpgrade(objId, protoId, id);
			}
		}
		if (num > 0)
		{
			player.controller.actionInspect.NotifyObjectUpgrade(EObjectType.Prebuild, num);
			int protoId2 = (int)this.prebuildPool[num].protoId;
			int id2 = replace_item_proto.ID;
			this.UpgradePrebuildWithComponents(num, replace_item_proto);
			if (this.onUpgrade != null)
			{
				this.onUpgrade(objId, protoId2, id2);
			}
		}
	}

	// Token: 0x060018D6 RID: 6358 RVA: 0x001A5860 File Offset: 0x001A3A60
	public void DismantleFinally(Player player, int objId, ref int protoId)
	{
		if (objId == 0)
		{
			return;
		}
		int num = -objId;
		if (objId > 0)
		{
			this.BeforeDismantleObject(objId);
			player.controller.actionBuild.NotifyDismantled(objId);
			player.controller.actionInspect.NotifyObjectDismantle(EObjectType.None, objId);
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.achievementLogic.NotifyBeforeDismantleEntity(this.planet.id, (int)this.entityPool[objId].protoId, objId);
			}
			bool flag = GameMain.preferences.instantDismantle && GameMain.sandboxToolsEnabled;
			if (!flag)
			{
				this.TakeBackItemsInEntity(player, objId);
			}
			else
			{
				this.ClearItemsInEntity(objId);
			}
			protoId = (int)this.entityPool[objId].protoId;
			this.RemoveEntityWithComponents(objId, !flag);
			this.OnDismantleObject(objId);
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.NotifyOnDismantleEntity(this.planet.id, protoId, objId);
			}
		}
		if (num > 0)
		{
			this.BeforeDismantleObject(objId);
			player.controller.actionBuild.NotifyDismantled(objId);
			player.controller.actionInspect.NotifyObjectDismantle(EObjectType.Prebuild, num);
			this.RemovePrebuildWithComponents(num);
			this.OnDismantleObject(objId);
		}
	}

	// Token: 0x060018D7 RID: 6359 RVA: 0x001A5984 File Offset: 0x001A3B84
	public int CreateCraftFinally(ECraftProto prototype, int protoId, int ownerId, int port, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		bool isSpace = false;
		short modelIndex;
		bool dynamic;
		if (prototype == ECraftProto.Item)
		{
			ItemProto itemProto = LDB.items.Select(protoId);
			if (itemProto == null || !itemProto.isCraft)
			{
				return 0;
			}
			modelIndex = (short)itemProto.ModelIndex;
			dynamic = itemProto.isDynamicCraft;
			isSpace = itemProto.isSpaceCraft;
			if (ownerId == 0 || this.craftPool[ownerId].id != ownerId)
			{
				return 0;
			}
		}
		else
		{
			if (prototype != ECraftProto.Fleet)
			{
				return 0;
			}
			FleetProto fleetProto = LDB.fleets.Select(protoId);
			if (fleetProto == null)
			{
				return 0;
			}
			modelIndex = (short)fleetProto.ModelIndex;
			dynamic = true;
			if (ownerId == 0)
			{
				return 0;
			}
		}
		CraftData craftData = new CraftData
		{
			protoId = (short)protoId,
			modelIndex = modelIndex,
			astroId = this.planet.id,
			owner = ownerId,
			port = (short)port,
			prototype = prototype,
			dynamic = dynamic,
			isSpace = isSpace,
			pos = pos,
			rot = rot,
			vel = vel
		};
		if (!craftData.dynamic)
		{
			craftData.hash.InitHashBits(pos.x, pos.y, pos.z);
		}
		return this.AddCraftDataWithComponents(ref craftData);
	}

	// Token: 0x060018D8 RID: 6360 RVA: 0x001A5AB8 File Offset: 0x001A3CB8
	public int CreateConstructionDroneFinally(int modelIndex, int ownerId, Vector3 pos, Quaternion rot, Vector3 vel)
	{
		CraftData craftData = default(CraftData);
		craftData.protoId = 0;
		craftData.modelIndex = (short)modelIndex;
		craftData.astroId = this.planet.id;
		craftData.owner = ownerId;
		craftData.port = 0;
		craftData.prototype = ECraftProto.ConstructionDrone;
		craftData.dynamic = true;
		craftData.isSpace = false;
		craftData.pos = pos;
		craftData.rot = rot;
		craftData.vel = vel;
		return this.AddCraftDataWithComponents(ref craftData);
	}

	// Token: 0x060018D9 RID: 6361 RVA: 0x001A5B40 File Offset: 0x001A3D40
	public bool HasDestroyedPrebuildUnder(int objectId)
	{
		int num = 0;
		ModelProto modelProto = (objectId > 0) ? LDB.models.modelArray[(int)this.entityPool[objectId].modelIndex] : LDB.models.modelArray[(int)this.prebuildPool[-objectId].modelIndex];
		bool flag;
		int num2;
		int num3;
		this.ReadObjectConn(objectId, 0, out flag, out num2, out num3);
		if (num2 < 0 && num3 == 13 && modelProto.prefabDesc.addonType == EAddonType.Storage)
		{
			return this.prebuildPool[-num2].isDestroyed;
		}
		bool flag2;
		int num4;
		this.ReadObjectConn(objectId, 14, out flag2, out num, out num4);
		return num < 0 && this.prebuildPool[-num].isDestroyed;
	}

	// Token: 0x060018DA RID: 6362 RVA: 0x001A5BF4 File Offset: 0x001A3DF4
	public void ReconstructTargetFinally(int prebuildId)
	{
		ref PrebuildData ptr = ref this.prebuildPool[prebuildId];
		if (ptr.isDestroyed)
		{
			PrefabDesc prefabDesc = LDB.models.Select((int)ptr.modelIndex).prefabDesc;
			ptr.isDestroyed = false;
			this.NotifyPrebuildChange(ptr.id, 4);
			if (this.planet.factoryLoaded || this.planet.factingCompletedStage >= 3)
			{
				this.AlterPrebuildModelState(prebuildId);
			}
			this.RemovePrebuildWarning(prebuildId);
			if (ptr.itemRequired > 0)
			{
				this.AddPrebuildWarning(prebuildId);
			}
			if (ptr.ruinId > 0)
			{
				this.RemoveRuinWithComponet(ptr.ruinId);
				ptr.ruinId = 0;
			}
		}
	}

	// Token: 0x060018DB RID: 6363 RVA: 0x001A5C98 File Offset: 0x001A3E98
	public void KillEntityFinally(Player player, int objId, ref CombatStat combatStat)
	{
		if (objId <= 0)
		{
			return;
		}
		bool factoryLoaded = this.planet.factoryLoaded;
		int num = this.GetTopObjectId(objId, true);
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		do
		{
			int num4 = num;
			int num5 = -num;
			int newPrebuildId = 0;
			if (num4 > 0)
			{
				ref EntityData ptr = ref this.entityPool[num4];
				BuildingParameters zero = BuildingParameters.zero;
				zero.CopyFromFactoryObject(num, this.planet.factory, true, true);
				ModelProto modelProto = LDB.models.modelArray[(int)ptr.modelIndex];
				int ruinId = modelProto.RuinId;
				PrefabDesc prefabDesc = modelProto.prefabDesc;
				bool flag2;
				int num6;
				this.ReadObjectConn(num, 14, out flag2, out num2, out num6);
				if (num2 == 0)
				{
					int num7;
					this.ReadObjectConn(num, 0, out flag2, out num7, out num6);
					if (num7 != 0 && num6 == 13)
					{
						num2 = num7;
					}
				}
				this.BeforeKillObject(num);
				RuinData ruinData = default(RuinData);
				if (num2 == 0 && prefabDesc.addonType != EAddonType.Belt && ruinId > 0)
				{
					flag = true;
				}
				else if (prefabDesc.addonType == EAddonType.Belt)
				{
					float num8 = this.planet.realRadius + 0.2f + 0.1f;
					Vector3 pos = ptr.pos;
					if (pos.sqrMagnitude <= num8 * num8)
					{
						flag = true;
					}
				}
				if (flag)
				{
					ModelProto modelProto2 = LDB.models.modelArray[ruinId];
					ruinData.modelIndex = ((modelProto2 == null) ? 0 : ((short)modelProto2.prefabDesc.modelIndex));
					ruinData.lifeTime = ((modelProto2 == null) ? -1 : modelProto2.RuinLifeTime);
					ruinData.pos = ptr.pos;
					ruinData.rot = ptr.rot;
					ruinData.id = this.AddRuinDataWithComponent(ruinData);
				}
				if (factoryLoaded && modelProto.RuinType != ERuinType.None)
				{
					WreckageHandler wreckagePrefab = PlanetFactory.PrefabDescByModelIndex[(ruinId != 0) ? ruinId : modelProto.ID].wreckagePrefab;
					if (wreckagePrefab != null)
					{
						int stateIndex = 0;
						int powerExcId = ptr.powerExcId;
						if (powerExcId > 0)
						{
							float state = this.powerSystem.excPool[powerExcId].state;
							if ((double)state > 0.5)
							{
								stateIndex = 1;
							}
							else if ((double)state < -0.5)
							{
								stateIndex = 2;
							}
						}
						if (wreckagePrefab.transformType == WreckageHandler.ETransformType.None)
						{
							this.planet.factoryModel.AddWreckage(modelProto, stateIndex, ref ptr.pos, ref ptr.rot, ref combatStat.lastImpact);
						}
						else if (wreckagePrefab.transformType == WreckageHandler.ETransformType.HasTarget)
						{
							bool isTurret = prefabDesc.isTurret;
							bool isEjector = prefabDesc.isEjector;
							Vector3 data = Vector3.forward;
							if (isTurret)
							{
								data = this.defenseSystem.turrets.buffer[ptr.turretId].localDir;
							}
							else if (isEjector)
							{
								data = this.factorySystem.ejectorPool[ptr.ejectorId].localDir;
							}
							this.planet.factoryModel.AddWreckage(modelProto, stateIndex, ref ptr.pos, ref ptr.rot, data, ref combatStat.lastImpact, wreckagePrefab.transformType);
						}
						else if (wreckagePrefab.transformType == WreckageHandler.ETransformType.OrbitalSubstation)
						{
							ref PowerNodeComponent ptr2 = ref this.powerSystem.nodePool[ptr.powerNodeId];
							PowerNetwork powerNetwork = this.powerSystem.netPool[ptr2.networkId];
							AnimData animData = this.entityAnimPool[num4];
							float time = animData.time;
							float working_length = animData.working_length;
							if (Mathf.Abs(4f - time) < 0.005f)
							{
								stateIndex = 1;
							}
							this.planet.factoryModel.AddWreckage(modelProto, stateIndex, ref ptr.pos, ref ptr.rot, new Vector3(time, working_length, (float)powerNetwork.energyServed), ref combatStat.lastImpact, wreckagePrefab.transformType);
						}
					}
				}
				PrebuildData prebuildData = default(PrebuildData);
				prebuildData.ruinId = ruinData.id;
				prebuildData.protoId = ptr.protoId;
				prebuildData.modelIndex = (short)modelProto.ID;
				prebuildData.pos = ptr.pos;
				prebuildData.rot = ptr.rot;
				prebuildData.pos2 = ptr.pos;
				prebuildData.rot2 = ptr.rot;
				prebuildData.itemRequired = 1;
				prebuildData.isDestroyed = true;
				prebuildData.recipeId = zero.recipeId;
				prebuildData.filterId = zero.filterId;
				prebuildData.isWarningSetted = (ptr.warningId != 0);
				bool isInserter = prefabDesc.isInserter;
				bool isBelt = prefabDesc.isBelt;
				zero.ToParamsArray(ref prebuildData.parameters, ref prebuildData.paramCount);
				prebuildData.content = zero.content;
				if (isInserter)
				{
					int inserterId = this.entityPool[num].inserterId;
					Pose pose = new Pose(this.factorySystem.inserterPosePool[inserterId].pos2, this.factorySystem.inserterPosePool[inserterId].rot2);
					prebuildData.pos2 = pose.position;
					prebuildData.rot2 = pose.rotation;
				}
				if (prefabDesc.minerType != EMinerType.None && prefabDesc.minerPeriod > 0)
				{
					int destinationIndex = 0;
					ref MinerComponent ptr3 = ref this.factorySystem.minerPool[ptr.minerId];
					int veinCount = ptr3.veinCount;
					if (prefabDesc.isVeinCollector)
					{
						int stationId = ptr.stationId;
						int itemId = this.transport.stationPool[stationId].storage[0].itemId;
						prebuildData.filterId = itemId;
						destinationIndex = 2048;
						int cnt = veinCount + 2048;
						prebuildData.InitParametersArray(cnt);
						zero.ToParamsArray(ref prebuildData.parameters, ref prebuildData.paramCount);
					}
					else
					{
						prebuildData.filterId = (int)this.entitySignPool[num4].iconId0;
						prebuildData.InitParametersArray(veinCount);
					}
					if (veinCount > 0)
					{
						Array.Copy(ptr3.veins, 0, prebuildData.parameters, destinationIndex, veinCount);
					}
				}
				prebuildData.id = this.planet.factory.AddPrebuildDataWithComponents(prebuildData);
				newPrebuildId = prebuildData.id;
				for (int i = 0; i < 16; i++)
				{
					bool flag3;
					int num9;
					int num10;
					this.ReadObjectConn(num, i, out flag3, out num9, out num10);
					if (num9 != 0)
					{
						int num11 = 0;
						int num12 = 0;
						bool flag4 = false;
						if (isBelt && num9 > 0)
						{
							if (i == 0 && flag3)
							{
								int beltId = this.entityPool[num].beltId;
								num11 = this.entityPool[num9].splitterId;
								if (beltId > 0 && num11 > 0)
								{
									ref SplitterComponent ptr4 = ref this.cargoTraffic.splitterPool[num11];
									if (ptr4.inPriority && ptr4.input0 == beltId)
									{
										flag4 = true;
									}
								}
							}
							else if (i == 1 && !flag3)
							{
								int beltId2 = this.entityPool[num].beltId;
								num11 = this.entityPool[num9].splitterId;
								if (beltId2 > 0 && num11 > 0)
								{
									ref SplitterComponent ptr5 = ref this.cargoTraffic.splitterPool[num11];
									if (ptr5.outFilter > 0 && ptr5.output0 == beltId2)
									{
										num12 = ptr5.outFilter;
									}
								}
							}
						}
						if (num9 > 0)
						{
							this.ApplyEntityDisconnection(num9, num4, num10, i);
						}
						this.ClearObjectConn(num, i);
						this.ClearObjectConn(num9, num10);
						this.WriteObjectConn(-prebuildData.id, i, flag3, num9, num10);
						this.WriteObjectConn(num9, num10, !flag3, -prebuildData.id, i);
						if (num11 > 0)
						{
							ref SplitterComponent ptr6 = ref this.cargoTraffic.splitterPool[num11];
							if (num12 > 0 || flag4)
							{
								int filter = (num12 == 0) ? ptr6.outFilterPreset : num12;
								ptr6.SetPriority(num10, true, filter);
							}
						}
					}
				}
				if (isBelt)
				{
					this.planet.factory.prebuildPool[prebuildData.id].rot = Maths.SphericalRotation(this.planet.factory.prebuildPool[prebuildData.id].pos, 0f);
					this.planet.factory.prebuildPool[prebuildData.id].rot2 = this.planet.factory.prebuildPool[prebuildData.id].rot;
				}
				if (factoryLoaded)
				{
					player.controller.actionBuild.NotifyKilled(num, -prebuildData.id);
					player.controller.actionInspect.NotifyObjectKill(EObjectType.None, num4);
				}
				this.WriteExtraInfoOnPrebuild(prebuildData.id, this.ReadExtraInfoOnEntity(num4));
				this.ThrowItemsInEntity(num4);
				this.gameData.warningSystem.Broadcast(EBroadcastVocal.BuildingDestroyed, this, 0, ptr.pos);
				this.RemoveEntityWithComponents(num4, false);
			}
			else if (num5 > 0)
			{
				BuildingParameters zero2 = BuildingParameters.zero;
				zero2.CopyFromFactoryObject(num, this.planet.factory, true, false);
				ref PrebuildData ptr7 = ref this.prebuildPool[num5];
				ModelProto modelProto3 = LDB.models.modelArray[(int)ptr7.modelIndex];
				bool flag2;
				int num6;
				this.ReadObjectConn(num, 14, out flag2, out num2, out num6);
				if (num2 == 0)
				{
					int num13;
					this.ReadObjectConn(num, 0, out flag2, out num13, out num6);
					if (num13 != 0 && num6 == 13)
					{
						num2 = num13;
					}
				}
				this.BeforeKillObject(num);
				PrefabDesc prefabDesc2 = modelProto3.prefabDesc;
				PrebuildData prebuildData2 = default(PrebuildData);
				prebuildData2.protoId = ptr7.protoId;
				prebuildData2.modelIndex = (short)modelProto3.ID;
				prebuildData2.pos = ptr7.pos;
				prebuildData2.rot = ptr7.rot;
				prebuildData2.pos2 = ptr7.pos2;
				prebuildData2.rot2 = ptr7.rot2;
				prebuildData2.itemRequired = 1;
				prebuildData2.isDestroyed = true;
				prebuildData2.ruinId = ptr7.ruinId;
				prebuildData2.recipeId = zero2.recipeId;
				prebuildData2.filterId = zero2.filterId;
				zero2.ToParamsArray(ref prebuildData2.parameters, ref prebuildData2.paramCount);
				prebuildData2.content = zero2.content;
				prebuildData2.id = this.planet.factory.AddPrebuildDataWithComponents(prebuildData2);
				newPrebuildId = prebuildData2.id;
				for (int j = 0; j < 16; j++)
				{
					bool flag5;
					int num14;
					int num15;
					this.ReadObjectConn(num, j, out flag5, out num14, out num15);
					if (num14 != 0)
					{
						this.ClearObjectConn(num, j);
						this.ClearObjectConn(num14, num15);
						this.WriteObjectConn(-prebuildData2.id, j, flag5, num14, num15);
						this.WriteObjectConn(num14, num15, !flag5, -prebuildData2.id, j);
					}
				}
				if (factoryLoaded)
				{
					player.controller.actionBuild.NotifyKilled(num, -prebuildData2.id);
					player.controller.actionInspect.NotifyObjectKill(EObjectType.Prebuild, num5);
				}
				ptr7.ruinId = 0;
				this.RemovePrebuildWithComponents(num5);
			}
			this.OnKillObject(newPrebuildId, num);
			if (num == objId)
			{
				return;
			}
			num = num2;
		}
		while (num3++ <= 256);
		Assert.CannotBeReached();
	}

	// Token: 0x060018DC RID: 6364 RVA: 0x001A6778 File Offset: 0x001A4978
	public void KillCraftFinally(int craftId, ref CombatStat combatStat)
	{
		if (craftId <= 0)
		{
			return;
		}
		if (this.beforeCraftKill != null)
		{
			this.beforeCraftKill(craftId);
		}
		bool flag = true;
		bool factoryLoaded = this.planet.factoryLoaded;
		ref CraftData ptr = ref this.craftPool[craftId];
		if (ptr.unitId > 0)
		{
			ref CraftData ptr2 = ref this.craftPool[ptr.owner];
			if (ptr2.owner < 0)
			{
				this.gameData.mainPlayer.mecha.groundCombatModule.OnFighterKilled((int)ptr2.port, (int)ptr.port);
			}
			else if (ptr2.owner > 0)
			{
				ref EntityData ptr3 = ref this.entityPool[ptr2.owner];
				if (ptr3.id == ptr2.owner)
				{
					this.combatGroundSystem.combatModules.buffer[ptr3.combatModuleId].OnFighterKilled(0, (int)ptr.port);
				}
			}
		}
		if (ptr.dynamic)
		{
			flag = false;
		}
		ModelProto modelProto = LDB.models.Select((int)ptr.modelIndex);
		if (flag && modelProto.RuinId > 0)
		{
			ModelProto modelProto2 = LDB.models.Select(modelProto.RuinId);
			this.AddRuinDataWithComponent(new RuinData
			{
				modelIndex = (short)modelProto2.prefabDesc.modelIndex,
				lifeTime = modelProto2.RuinLifeTime,
				pos = ptr.pos,
				rot = ptr.rot
			});
		}
		if (factoryLoaded)
		{
			int num = 0;
			int num2 = (modelProto.prefabDesc.wreckagePrefab != null) ? modelProto.prefabDesc.wreckagePrefabs.Length : 0;
			if (num2 > 1)
			{
				num = (int)(Random.value * (float)num2);
				if (num >= num2)
				{
					num = num2 - 1;
				}
			}
			this.planet.factoryModel.AddWreckage(modelProto, num, ptr.pos, ref ptr.rot, ref ptr.vel, ref combatStat.lastImpact);
		}
		this.RemoveCraftWithComponents(craftId);
		if (this.onCraftKill != null)
		{
			this.onCraftKill(craftId);
		}
	}

	// Token: 0x060018DD RID: 6365 RVA: 0x001A6974 File Offset: 0x001A4B74
	public void KillEnemyFinally(int enemyId, ref CombatStat combatStat)
	{
		if (enemyId <= 0)
		{
			return;
		}
		bool flag = true;
		bool factoryLoaded = this.planet.factoryLoaded;
		ref EnemyData ptr = ref this.enemyPool[enemyId];
		int num = 0;
		int dfGBaseId = ptr.dfGBaseId;
		int builderId = ptr.builderId;
		if (dfGBaseId != 0)
		{
			DFGBaseComponent dfgbaseComponent = this.enemySystem.bases.buffer[dfGBaseId];
			if (dfgbaseComponent != null && dfgbaseComponent.ruinId != 0)
			{
				flag = false;
			}
			else if (this.enemySystem.builders.buffer[builderId].sp == 0)
			{
				flag = false;
			}
			if (dfgbaseComponent != null)
			{
				num = dfgbaseComponent.evolve.level;
			}
			ptr.isInvincible = true;
			this.gameData.warningSystem.Broadcast(EBroadcastVocal.DFBaseCoreDestroyed, -1, 0, 0);
			this.defenseSystem.RemoveGlobalTargets(ETargetType.Enemy, enemyId);
			this.skillSystem.OnRemovingSkillTarget(enemyId, this.planet.astroId, ETargetType.Enemy);
		}
		if (ptr.dynamic)
		{
			flag = false;
			int unitId = ptr.unitId;
			if (unitId > 0)
			{
				EnemyUnitComponent[] buffer = this.enemySystem.units.buffer;
				ref EnemyUnitComponent ptr2 = ref buffer[unitId];
				if (ptr2.id == unitId)
				{
					ptr2.BroadcastHatred(this, buffer, ref ptr, false, this.skillSystem.maxHatredGroundTmp, this.skillSystem.maxHatredGroundBaseTmp, -1);
				}
			}
		}
		int ruinId = 0;
		ModelProto modelProto = LDB.models.modelArray[(int)ptr.modelIndex];
		if (flag && modelProto.RuinId > 0)
		{
			ModelProto modelProto2 = LDB.models.modelArray[modelProto.RuinId];
			ruinId = this.AddRuinDataWithComponent(new RuinData
			{
				modelIndex = (short)modelProto2.prefabDesc.modelIndex,
				lifeTime = ((dfGBaseId == 0) ? modelProto2.RuinLifeTime : (-1 - num)),
				pos = ptr.pos,
				rot = ptr.rot
			});
		}
		if (factoryLoaded)
		{
			WreckageHandler wreckagePrefab = PlanetFactory.PrefabDescByModelIndex[(modelProto.RuinId != 0) ? modelProto.RuinId : modelProto.ID].wreckagePrefab;
			int num2 = 0;
			if (wreckagePrefab != null)
			{
				int num3 = (modelProto.prefabDesc.wreckagePrefabs != null) ? modelProto.prefabDesc.wreckagePrefabs.Length : 0;
				if (num3 > 1)
				{
					num2 = (int)(Random.value * (float)num3);
					if (num2 >= num3)
					{
						num2 = num3 - 1;
					}
				}
				if (wreckagePrefab.transformType == WreckageHandler.ETransformType.DFGBuilding)
				{
					EnemyBuilderComponent enemyBuilderComponent = this.enemySystem.builders.buffer[builderId];
					Vector3 vector = ptr.pos;
					this.planet.factoryModel.AddWreckage(modelProto, num2, ref vector, ref ptr.rot, new Vector3((float)enemyBuilderComponent.sp, (float)enemyBuilderComponent.spMax, 0f), ref combatStat.lastImpact, wreckagePrefab.transformType);
				}
				else if (wreckagePrefab.transformType == WreckageHandler.ETransformType.None)
				{
					this.planet.factoryModel.AddWreckage(modelProto, num2, ptr.pos, ref ptr.rot, ref ptr.vel, ref combatStat.lastImpact);
				}
			}
		}
		if (dfGBaseId != 0)
		{
			this.enemySystem.NotifyBaseKilled(dfGBaseId, ruinId);
		}
		if (ptr.owner > 0)
		{
			this.enemySystem.NotifyEnemyKilled(ref ptr);
		}
		if (dfGBaseId == 0)
		{
			this.RemoveEnemyWithComponents(enemyId);
		}
	}

	// Token: 0x060018DE RID: 6366 RVA: 0x001A6C9C File Offset: 0x001A4E9C
	public bool HasEnemyBuildingNear(int objectId)
	{
		if (objectId == 0)
		{
			Assert.CannotBeReached();
			return false;
		}
		bool flag = this.planet == this.gameData.localPlanet && this.planet.factoryLoaded;
		Vector3 vector = Vector3.zero;
		Quaternion orientation = Quaternion.identity;
		float num;
		float num2;
		PrefabDesc prefabDesc;
		if (objectId > 0)
		{
			ref EntityData ptr = ref this.entityPool[objectId];
			num = SkillSystem.RoughWidthByModelIndex[(int)ptr.modelIndex];
			num2 = num * 2f;
			vector = ptr.pos;
			prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
			orientation = ptr.rot;
		}
		else
		{
			ref PrebuildData ptr2 = ref this.prebuildPool[-objectId];
			num = SkillSystem.RoughWidthByModelIndex[(int)ptr2.modelIndex];
			num2 = num * 2f;
			vector = ptr2.pos;
			prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr2.modelIndex];
			orientation = ptr2.rot;
		}
		bool flag2 = false;
		float num3 = 40f + num * 2f;
		num3 *= num3;
		DFGBaseComponent[] buffer = this.enemySystem.bases.buffer;
		int cursor = this.enemySystem.bases.cursor;
		for (int i = 1; i < cursor; i++)
		{
			DFGBaseComponent dfgbaseComponent = buffer[i];
			if (dfgbaseComponent != null && dfgbaseComponent.id == i)
			{
				ref EnemyData ptr3 = ref this.enemyPool[dfgbaseComponent.enemyId];
				if (ptr3.id == dfgbaseComponent.enemyId)
				{
					float num4 = (float)ptr3.pos.x - vector.x;
					float num5 = num4 * num4;
					if (num5 <= num3)
					{
						float num6 = (float)ptr3.pos.y - vector.y;
						float num7 = num6 * num6;
						if (num7 <= num3)
						{
							float num8 = (float)ptr3.pos.z - vector.z;
							float num9 = num8 * num8;
							if (num9 <= num3 && num5 + num7 + num9 <= num3)
							{
								flag2 = true;
								break;
							}
						}
					}
				}
			}
		}
		if (flag2)
		{
			int[] hashPool = this.hashSystemStatic.hashPool;
			int[] bucketOffsets = this.hashSystemStatic.bucketOffsets;
			int[] bucketCursors = this.hashSystemStatic.bucketCursors;
			int num10 = 0;
			int num11 = (int)((vector.x + 270f) / HashSystem.cellSize);
			int num12 = (int)((vector.y + 270f) / HashSystem.cellSize);
			int num13 = (int)((vector.z + 270f) / HashSystem.cellSize);
			num11 = ((num11 < 99) ? ((num11 < 0) ? 0 : num11) : 99);
			num12 = ((num12 < 99) ? ((num12 < 0) ? 0 : num12) : 99);
			num13 = ((num13 < 99) ? ((num13 < 0) ? 0 : num13) : 99);
			int num14 = num13 * 10000 + num12 * 100 + num11 << 5;
			for (int j = 0; j < 32; j++)
			{
				HashSystem.Cell cell = HashSystem.bucketMap[num14 + j];
				if ((float)cell.dist > num2)
				{
					break;
				}
				this.Tmp_activeBuckets[num10++] = (int)cell.bucketIndex;
			}
			for (int k = 0; k < num10; k++)
			{
				int num15 = this.Tmp_activeBuckets[k];
				int num16 = bucketOffsets[num15];
				int num17 = num16 + bucketCursors[num15];
				for (int l = num16; l < num17; l++)
				{
					int num18 = hashPool[l];
					if (num18 != 0 && num18 >> 28 == 4)
					{
						int num19 = num18 & 268435455;
						if (num19 != 0)
						{
							ref EnemyData ptr4 = ref this.enemyPool[num19];
							if (ptr4.id == num19 && !ptr4.isInvincible)
							{
								float num20 = SkillSystem.RoughWidthByModelIndex[(int)ptr4.modelIndex];
								Vector3 vector2 = ptr4.pos;
								float num21 = vector2.x - vector.x;
								float num22 = vector2.y - vector.y;
								float num23 = vector2.z - vector.z;
								if ((double)Mathf.Sqrt(num21 * num21 + num22 * num22 + num23 * num23) < (double)num * 0.4 + (double)num20 * 0.55)
								{
									return true;
								}
							}
						}
					}
				}
			}
			if (flag)
			{
				ColliderData[] buildColliders = prefabDesc.buildColliders;
				this.planet.physics.nearColliderLogic.ActiveEnemyBuildingColliderInArea(vector, 10f);
				foreach (ColliderData colliderData in buildColliders)
				{
					int mask = 395264;
					if (prefabDesc.veinMiner || prefabDesc.oilMiner)
					{
						mask = 393216;
					}
					Array.Clear(this._tmp_cols, 0, this._tmp_cols.Length);
					int num24 = Physics.OverlapBoxNonAlloc(vector, colliderData.ext, this._tmp_cols, orientation, mask, QueryTriggerInteraction.Collide);
					if (num24 > 0)
					{
						PlanetPhysics physics = this.planet.physics;
						for (int n = 0; n < num24; n++)
						{
							ColliderData colliderData2;
							if (physics.GetColliderData(this._tmp_cols[n], out colliderData2) && colliderData2.isForBuild && colliderData2.objType == EObjectType.Enemy)
							{
								int objId = colliderData2.objId;
								ref EnemyData ptr5 = ref this.enemyPool[objId];
								if (ptr5.id == objId && !ptr5.isInvincible && !ptr5.dynamic)
								{
									return true;
								}
							}
						}
					}
				}
			}
		}
		return false;
	}

	// Token: 0x060018DF RID: 6367 RVA: 0x001A71BC File Offset: 0x001A53BC
	public void OnBeltBuilt(int beltEntityId)
	{
		int beltId = this.entityPool[beltEntityId].beltId;
		if (beltId == 0)
		{
			return;
		}
		if (this.planet.physics != null)
		{
			this.cargoTraffic.AlterBeltRenderer(beltId, this.entityPool, this.planet.physics.colChunks, false);
		}
		BeltComponent beltComponent = this.cargoTraffic.beltPool[beltId];
		if (beltComponent.id != beltId)
		{
			Assert.CannotBeReached();
			return;
		}
		CargoPath cargoPath = this.cargoTraffic.GetCargoPath(beltComponent.segPathId);
		Assert.NotNull(cargoPath);
		if (cargoPath == null)
		{
			return;
		}
		Vector3 pos = this.entityPool[beltEntityId].pos;
		Vector3[] pointPos = cargoPath.pointPos;
		int num = beltComponent.segIndex;
		int num2 = beltComponent.segIndex + beltComponent.segLength - 1;
		num -= 20;
		num2 += 20;
		if (num < 4)
		{
			num = 4;
		}
		if (num2 > cargoPath.pathLength - 5 - 1)
		{
			num2 = cargoPath.pathLength - 5 - 1;
		}
		if (this.tmp_ids == null)
		{
			this.tmp_ids = new int[1024];
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		int entitiesInAreaNonAlloc = this.hashSystemStatic.GetEntitiesInAreaNonAlloc(this.entityPool[beltEntityId].pos, 6f, ref this.tmp_ids, this.entityPool);
		for (int i = 0; i < entitiesInAreaNonAlloc; i++)
		{
			int num3 = this.tmp_ids[i];
			if (num3 != 0 && this.entityPool[num3].id == num3)
			{
				if (this.entityPool[num3].inserterId > 0)
				{
					int inserterId = this.entityPool[num3].inserterId;
					ref InserterComponent ptr = ref this.factorySystem.inserterPool[inserterId];
					Vector3 vector = this.entityPool[num3].pos;
					Vector3 vector2 = this.factorySystem.inserterPosePool[inserterId].pos2;
					vector += vector.normalized * 0.15f;
					vector2 += vector2.normalized * 0.15f;
					bool flag = false;
					float num4 = 0.02f;
					int num5 = -1;
					if (ptr.pickTarget == 0 && (vector - pos).sqrMagnitude < 2f)
					{
						for (int j = num; j <= num2; j++)
						{
							float sqrMagnitude = (pointPos[j] - vector).sqrMagnitude;
							if (sqrMagnitude < num4)
							{
								num4 = sqrMagnitude;
								num5 = j;
							}
						}
						if (num5 >= 0)
						{
							int num6 = 0;
							int num7 = 0;
							for (int k = 0; k < cargoPath.belts.Count; k++)
							{
								int segIndex = this.cargoTraffic.beltPool[cargoPath.belts[k]].segIndex;
								int num8 = this.cargoTraffic.beltPool[cargoPath.belts[k]].segIndex + this.cargoTraffic.beltPool[cargoPath.belts[k]].segLength;
								if (segIndex <= num5 && num5 < num8)
								{
									num6 = this.cargoTraffic.beltPool[cargoPath.belts[k]].entityId;
									num7 = segIndex + this.cargoTraffic.beltPool[cargoPath.belts[k]].segPivotOffset;
									break;
								}
							}
							Assert.Positive(num6);
							if (num6 > 0)
							{
								flag = true;
								this.WriteObjectConn(num3, 1, false, num6, -1);
								this.factorySystem.SetInserterPickTarget(inserterId, num6, num5 - num7);
								if (num4 > 0.02f)
								{
									this.entityPool[num3].pos = pointPos[num5] - pointPos[num5].normalized * 0.15f;
									this.entityPool[num3].alt = this.entityPool[num3].pos.magnitude;
									GameMain.gpuiManager.AlterModel((int)this.entityPool[num3].modelIndex, this.entityPool[num3].modelId, num3, this.entityPool[num3].pos, true);
								}
							}
						}
					}
					num4 = 0.02f;
					num5 = -1;
					if (!flag && ptr.insertTarget == 0 && (vector2 - pos).sqrMagnitude < 2f)
					{
						for (int l = num; l <= num2; l++)
						{
							float sqrMagnitude2 = (pointPos[l] - vector2).sqrMagnitude;
							if (sqrMagnitude2 < num4)
							{
								num4 = sqrMagnitude2;
								num5 = l;
							}
						}
						if (num5 >= 0)
						{
							int num9 = 0;
							int num10 = 0;
							for (int m = 0; m < cargoPath.belts.Count; m++)
							{
								int segIndex2 = this.cargoTraffic.beltPool[cargoPath.belts[m]].segIndex;
								int num11 = this.cargoTraffic.beltPool[cargoPath.belts[m]].segIndex + this.cargoTraffic.beltPool[cargoPath.belts[m]].segLength;
								if (segIndex2 <= num5 && num5 < num11)
								{
									num9 = this.cargoTraffic.beltPool[cargoPath.belts[m]].entityId;
									num10 = segIndex2 + this.cargoTraffic.beltPool[cargoPath.belts[m]].segPivotOffset;
									break;
								}
							}
							Assert.Positive(num9);
							if (num9 > 0)
							{
								this.WriteObjectConn(num3, 0, true, num9, -1);
								this.factorySystem.SetInserterInsertTarget(inserterId, num9, num5 - num10);
								if (num4 > 0.02f)
								{
									this.factorySystem.inserterPosePool[inserterId].pos2 = pointPos[num5] - pointPos[num5].normalized * 0.15f;
									GameMain.gpuiManager.AlterModel((int)this.entityPool[num3].modelIndex, this.entityPool[num3].modelId, num3, this.entityPool[num3].pos, true);
								}
							}
						}
					}
				}
				if (PlanetFactory.PrefabDescByModelIndex[(int)this.entityPool[num3].modelIndex].addonType == EAddonType.Belt)
				{
					this.TryConnAddonBuilding(num3, beltEntityId);
				}
			}
		}
	}

	// Token: 0x060018E0 RID: 6368 RVA: 0x001A7880 File Offset: 0x001A5A80
	public void OnInserterBuilt(int entityId)
	{
		bool flag;
		int num;
		int num2;
		this.ReadObjectConn(entityId, 0, out flag, out num, out num2);
		bool flag2;
		int num3;
		int num4;
		this.ReadObjectConn(entityId, 1, out flag2, out num3, out num4);
		Vector3 pos = this.entityPool[entityId].pos;
		Vector3 pos2 = this.factorySystem.inserterPosePool[this.entityPool[entityId].inserterId].pos2;
		bool flag3 = true;
		bool flag4 = true;
		if (Maths.VectorSqrDistance(ref pos, ref pos2) > 50f)
		{
			flag3 = false;
		}
		if (flag3 && num != 0)
		{
			int num5 = (int)((num > 0) ? this.entityPool[num].protoId : this.prebuildPool[-num].protoId);
			if (num5 == 2003 || num5 == 2002 || num5 == 2001)
			{
				Vector3 vector = (num > 0) ? this.entityPool[num].pos : this.prebuildPool[-num].pos;
				if (Maths.VectorSqrDistance(ref vector, ref pos2) > 5f)
				{
					flag3 = false;
				}
			}
		}
		if (flag4 && num3 != 0)
		{
			int num6 = (int)((num3 > 0) ? this.entityPool[num3].protoId : this.prebuildPool[-num3].protoId);
			if (num6 == 2003 || num6 == 2002 || num6 == 2001)
			{
				Vector3 vector2 = (num3 > 0) ? this.entityPool[num3].pos : this.prebuildPool[-num3].pos;
				if (Maths.VectorSqrDistance(ref vector2, ref pos) > 5f)
				{
					flag4 = false;
				}
			}
		}
		if (!flag3 || !flag4)
		{
			Debug.LogWarning("位移过大，将取消连接");
			int inserterId = this.entityPool[entityId].inserterId;
			if (!flag3)
			{
				this.ClearObjectConn(entityId, 0);
				if (this.factorySystem.inserterPool[inserterId].insertTarget == num)
				{
					this.factorySystem.SetInserterInsertTarget(inserterId, 0, 0);
				}
			}
			if (!flag4)
			{
				this.ClearObjectConn(entityId, 1);
				if (this.factorySystem.inserterPool[inserterId].pickTarget == num3)
				{
					this.factorySystem.SetInserterPickTarget(inserterId, 0, 0);
				}
			}
			return;
		}
	}

	// Token: 0x060018E1 RID: 6369 RVA: 0x001A7ABC File Offset: 0x001A5CBC
	public void OnAddonBuilt(int entityId)
	{
		ItemProto itemProto = LDB.items.Select((int)this.entityPool[entityId].protoId);
		if (itemProto == null)
		{
			return;
		}
		if (itemProto.prefabDesc.addonType == EAddonType.Belt)
		{
			if (this.tmp_ids == null)
			{
				this.tmp_ids = new int[1024];
			}
			int entitiesInAreaNonAlloc = this.hashSystemStatic.GetEntitiesInAreaNonAlloc(this.entityPool[entityId].pos, 6f, ref this.tmp_ids, this.entityPool);
			PrefabDesc prefabDesc = LDB.models.modelArray[(int)this.entityPool[entityId].modelIndex].prefabDesc;
			Pose[] addonAreaColPoses = prefabDesc.addonAreaColPoses;
			Vector3[] addonAreaSize = prefabDesc.addonAreaSize;
			for (int i = 0; i < prefabDesc.addonAreaPoses.Length; i++)
			{
				float num = float.MaxValue;
				int num2 = 0;
				Vector3 lineStart = this.entityPool[entityId].pos + this.entityPool[entityId].rot * (addonAreaColPoses[i].position + addonAreaColPoses[i].forward * addonAreaSize[i].z * 1.5f);
				Vector3 lineEnd = this.entityPool[entityId].pos + this.entityPool[entityId].rot * (addonAreaColPoses[i].position - addonAreaColPoses[i].forward * addonAreaSize[i].z * 1.5f);
				for (int j = 0; j < entitiesInAreaNonAlloc; j++)
				{
					int num3 = this.tmp_ids[j];
					if (num3 != 0 && this.entityPool[num3].id == num3 && this.entityPool[num3].beltId > 0)
					{
						Pose pose = prefabDesc.addonAreaPoses[i];
						Pose transformedBy = pose.GetTransformedBy(new Pose(this.entityPool[entityId].pos, this.entityPool[entityId].rot));
						float sqrMagnitude = (this.entityPool[num3].pos - transformedBy.position).sqrMagnitude;
						float num4 = Maths.DistancePointLine(this.entityPool[num3].pos, lineStart, lineEnd);
						if (sqrMagnitude < num && sqrMagnitude < 1f && num4 < 0.3f)
						{
							num2 = num3;
							num = sqrMagnitude;
						}
					}
				}
				if (num2 > 0)
				{
					this.WriteObjectConn(entityId, i, true, num2, 13);
					if (this.entityPool[entityId].monitorId > 0)
					{
						this.cargoTraffic.ConnectToMonitor(this.entityPool[entityId].monitorId, this.entityPool[num2].beltId, 0);
					}
					if (this.entityPool[entityId].spraycoaterId > 0)
					{
						int cargoBeltId = this.cargoTraffic.spraycoaterPool[this.entityPool[entityId].spraycoaterId].cargoBeltId;
						int incBeltId = this.cargoTraffic.spraycoaterPool[this.entityPool[entityId].spraycoaterId].incBeltId;
						this.cargoTraffic.ConnectToSpraycoater(this.entityPool[entityId].spraycoaterId, (i == 0) ? this.entityPool[num2].beltId : cargoBeltId, (i == 0) ? incBeltId : this.entityPool[num2].beltId);
					}
					if (this.entityPool[entityId].turretId > 0)
					{
						this.defenseSystem.ConnectToTurret(this.entityPool[entityId].turretId, this.entityPool[num2].beltId);
					}
				}
			}
			return;
		}
		if (itemProto.prefabDesc.addonType == EAddonType.Storage && itemProto.prefabDesc.isDispenser)
		{
			bool flag;
			int num5;
			int num6;
			this.ReadObjectConn(entityId, 0, out flag, out num5, out num6);
			if (num5 > 0)
			{
				this.entityAnimPool[entityId].state = ((this.entityPool[num5].protoId == 2101) ? 1U : 0U);
			}
			else if (num5 < 0)
			{
				this.entityAnimPool[entityId].state = ((this.prebuildPool[-num5].protoId == 2101) ? 1U : 0U);
			}
			DispenserComponent dispenserComponent = this.transport.dispenserPool[this.entityPool[entityId].dispenserId];
			if (dispenserComponent.filter == 0)
			{
				dispenserComponent.GuessFilter(this);
			}
		}
	}

	// Token: 0x060018E2 RID: 6370 RVA: 0x001A7F88 File Offset: 0x001A6188
	public void TryConnAddonBuilding(int addonEntityId, int beltEntityId)
	{
		PrefabDesc prefabDesc = LDB.models.modelArray[(int)this.entityPool[addonEntityId].modelIndex].prefabDesc;
		Pose[] addonAreaColPoses = prefabDesc.addonAreaColPoses;
		Vector3[] addonAreaSize = prefabDesc.addonAreaSize;
		for (int i = 0; i < prefabDesc.addonAreaPoses.Length; i++)
		{
			float maxValue = float.MaxValue;
			int num = 0;
			Vector3 lineStart = this.entityPool[addonEntityId].pos + this.entityPool[addonEntityId].rot * (addonAreaColPoses[i].position + addonAreaColPoses[i].forward * addonAreaSize[i].z * 1.5f);
			Vector3 lineEnd = this.entityPool[addonEntityId].pos + this.entityPool[addonEntityId].rot * (addonAreaColPoses[i].position - addonAreaColPoses[i].forward * addonAreaSize[i].z * 1.5f);
			if (beltEntityId != 0 && this.entityPool[beltEntityId].id == beltEntityId && this.entityPool[beltEntityId].beltId > 0)
			{
				Pose pose = prefabDesc.addonAreaPoses[i];
				Pose transformedBy = pose.GetTransformedBy(new Pose(this.entityPool[addonEntityId].pos, this.entityPool[addonEntityId].rot));
				float sqrMagnitude = (this.entityPool[beltEntityId].pos - transformedBy.position).sqrMagnitude;
				float num2 = Maths.DistancePointLine(this.entityPool[beltEntityId].pos, lineStart, lineEnd);
				if (sqrMagnitude < maxValue && sqrMagnitude < 1f && num2 < 0.3f)
				{
					num = beltEntityId;
				}
			}
			if (num > 0)
			{
				this.WriteObjectConn(addonEntityId, i, true, num, 13);
				if (this.entityPool[addonEntityId].monitorId > 0)
				{
					this.cargoTraffic.ConnectToMonitor(this.entityPool[addonEntityId].monitorId, this.entityPool[num].beltId, 0);
				}
				if (this.entityPool[addonEntityId].spraycoaterId > 0)
				{
					int cargoBeltId = this.cargoTraffic.spraycoaterPool[this.entityPool[addonEntityId].spraycoaterId].cargoBeltId;
					int incBeltId = this.cargoTraffic.spraycoaterPool[this.entityPool[addonEntityId].spraycoaterId].incBeltId;
					this.cargoTraffic.ConnectToSpraycoater(this.entityPool[addonEntityId].spraycoaterId, (i == 0) ? this.entityPool[num].beltId : cargoBeltId, (i == 0) ? incBeltId : this.entityPool[num].beltId);
				}
				if (this.entityPool[addonEntityId].turretId > 0)
				{
					this.defenseSystem.ConnectToTurret(this.entityPool[addonEntityId].turretId, this.entityPool[num].beltId);
				}
			}
		}
	}

	// Token: 0x060018E3 RID: 6371 RVA: 0x001A82D8 File Offset: 0x001A64D8
	public void BeginFlattenTerrain()
	{
		if (this.tmp_levelChanges == null)
		{
			this.tmp_levelChanges = new Dictionary<int, int>();
		}
		this.tmp_levelChanges.Clear();
	}

	// Token: 0x060018E4 RID: 6372 RVA: 0x001A82F8 File Offset: 0x001A64F8
	public void EndFlattenTerrain()
	{
		ushort[] heightData = this.planet.data.heightData;
		bool levelized = this.planet.levelized;
		int num = (int)((short)(this.planet.realRadius * 100f + 20f) - 60);
		float radius = this.planet.radius;
		foreach (KeyValuePair<int, int> keyValuePair in this.tmp_levelChanges)
		{
			if (keyValuePair.Value > 0)
			{
				ushort num2 = heightData[keyValuePair.Key];
				if (levelized)
				{
					if ((int)num2 >= num)
					{
						if (this.planet.data.GetModLevel(keyValuePair.Key) < 3)
						{
							this.planet.data.SetModPlane(keyValuePair.Key, 0);
						}
						this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
					}
				}
				else
				{
					this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}
		bool flag = this.planet.UpdateDirtyMeshes();
		if (GameMain.isRunning && flag)
		{
			this.RenderLocalPlanetHeightmap();
		}
		if (flag)
		{
			PlanetAlgorithm.CalcLandPercent(this.planet);
		}
		GameMain.gpuiManager.SyncAllGPUBuffer();
	}

	// Token: 0x060018E5 RID: 6373 RVA: 0x001A8448 File Offset: 0x001A6648
	public int FlattenTerrain(Vector3 pos, Quaternion rot, Bounds bound, float fade0 = 6f, float fade1 = 1f, bool removeVein = false, bool lift = false, bool emitEffects = true, bool autoRefresh = true, Bounds removeVegeBound = default(Bounds))
	{
		if (this.tmp_levelChanges == null)
		{
			this.tmp_levelChanges = new Dictionary<int, int>();
		}
		if (autoRefresh)
		{
			this.tmp_levelChanges.Clear();
		}
		bound.extents = new Vector3(bound.extents.x, bound.extents.y + 0.5f, bound.extents.z);
		Quaternion quaternion = rot;
		quaternion.w = -quaternion.w;
		Quaternion rotation = Maths.SphericalRotation(pos, 22.5f);
		float realRadius = this.planet.realRadius;
		float num = bound.extents.magnitude + fade0;
		float num2 = num * num;
		float num3 = realRadius * 3.1415927f / ((float)this.planet.precision * 2f);
		int num4 = Mathf.CeilToInt(num * 1.414f / num3 * 1.5f + 0.5f);
		Vector3[] array = new Vector3[]
		{
			pos,
			pos + rotation * (new Vector3(0f, 0f, 1.414f) * num),
			pos + rotation * (new Vector3(0f, 0f, -1.414f) * num),
			pos + rotation * (new Vector3(1.414f, 0f, 0f) * num),
			pos + rotation * (new Vector3(-1.414f, 0f, 0f) * num),
			pos + rotation * (new Vector3(1f, 0f, 1f) * num),
			pos + rotation * (new Vector3(-1f, 0f, -1f) * num),
			pos + rotation * (new Vector3(1f, 0f, -1f) * num),
			pos + rotation * (new Vector3(-1f, 0f, 1f) * num)
		};
		int stride = this.planet.data.stride;
		int dataLength = this.planet.data.dataLength;
		Vector3[] vertices = this.planet.data.vertices;
		ushort[] heightData = this.planet.data.heightData;
		short num5 = (short)(this.planet.realRadius * 100f + 20f);
		int num6 = 0;
		foreach (Vector3 vpos in array)
		{
			int num7 = this.planet.data.QueryIndex(vpos);
			for (int j = -num4; j <= num4; j++)
			{
				int num8 = num7 + j * stride;
				if (num8 >= 0 && num8 < dataLength)
				{
					for (int k = -num4; k <= num4; k++)
					{
						int num9 = num8 + k;
						if ((ulong)num9 < (ulong)((long)dataLength) && (lift || heightData[num9] > (ushort)num5))
						{
							Vector3 vector;
							vector.x = vertices[num9].x * realRadius;
							vector.y = vertices[num9].y * realRadius;
							vector.z = vertices[num9].z * realRadius;
							Vector3 vector2;
							vector2.x = vector.x - pos.x;
							vector2.y = vector.y - pos.y;
							vector2.z = vector.z - pos.z;
							if (vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z < num2 && ((autoRefresh && !this.tmp_levelChanges.ContainsKey(num9)) || (!autoRefresh && (!this.tmp_levelChanges.ContainsKey(num9) || this.tmp_levelChanges[num9] < 3))))
							{
								vector = quaternion * (vector - pos);
								if (bound.Contains(vector))
								{
									this.tmp_levelChanges[num9] = 3;
								}
								else
								{
									float num10 = Vector3.Distance(bound.ClosestPoint(vector), vector);
									int num11 = (int)((fade0 - num10) / (fade0 - fade1) * 3f + 0.5f);
									if (num11 < 0)
									{
										num11 = 0;
									}
									else if (num11 > 3)
									{
										num11 = 3;
									}
									int modLevel = this.planet.data.GetModLevel(num9);
									int num12 = num11 - modLevel;
									if (num11 >= modLevel && num12 != 0)
									{
										if (autoRefresh || !this.tmp_levelChanges.ContainsKey(num9))
										{
											this.tmp_levelChanges[num9] = num11;
										}
										else
										{
											Dictionary<int, int> dictionary = this.tmp_levelChanges;
											int key = num9;
											dictionary[key] += num11;
										}
										float num13 = (float)heightData[num9] * 0.01f;
										float num14 = realRadius + 0.2f - num13;
										float f = 100f * (float)num12 * num14 * 0.3333333f;
										num6 += -Mathf.FloorToInt(f);
									}
								}
							}
						}
					}
				}
			}
		}
		if (autoRefresh)
		{
			bool levelized = this.planet.levelized;
			int num15 = Mathf.RoundToInt((pos.magnitude - 0.2f - this.planet.realRadius) / 1.3333333f);
			int num16 = num15 * 133 + (int)num5 - 60;
			foreach (KeyValuePair<int, int> keyValuePair in this.tmp_levelChanges)
			{
				if (keyValuePair.Value > 0)
				{
					if (levelized)
					{
						if ((int)heightData[keyValuePair.Key] >= num16)
						{
							if (this.planet.data.GetModLevel(keyValuePair.Key) < 3)
							{
								this.planet.data.SetModPlane(keyValuePair.Key, num15);
							}
							this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
						}
					}
					else
					{
						this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
			bool flag = this.planet.UpdateDirtyMeshes();
			if (GameMain.isRunning && flag)
			{
				this.RenderLocalPlanetHeightmap();
			}
			if (flag)
			{
				PlanetAlgorithm.CalcLandPercent(this.planet);
			}
		}
		PlanetPhysics physics = this.planet.physics;
		bound.extents += new Vector3(1.5f, 1.5f, 1.5f);
		NearColliderLogic nearColliderLogic = physics.nearColliderLogic;
		if (this.tmp_ids == null)
		{
			this.tmp_ids = new int[1024];
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		if (removeVegeBound.extents.x == 0f && removeVegeBound.extents.y == 0f && removeVegeBound.extents.z == 0f)
		{
			removeVegeBound = bound;
		}
		int num17 = nearColliderLogic.GetVegetablesInAreaNonAlloc(pos, num, this.tmp_ids);
		for (int l = 0; l < num17; l++)
		{
			int num18 = this.tmp_ids[l];
			Vector3 vector = this.vegePool[num18].pos;
			vector = quaternion * (vector - pos);
			if (removeVegeBound.Contains(vector) && this.vegePool[num18].protoId < 9999)
			{
				if (emitEffects && GameMain.gameTick > 0L && this.gameData.localLoadedPlanetFactory == this)
				{
					VegeProto vegeProto = LDB.veges.Select((int)this.vegePool[num18].protoId);
					if (vegeProto != null)
					{
						VFEffectEmitter.Emit(vegeProto.MiningEffect, this.vegePool[num18].pos, this.vegePool[num18].rot);
						VFAudio.Create(vegeProto.MiningAudio, null, this.vegePool[num18].pos, true, 1, -1, -1L);
					}
				}
				this.RemoveVegeWithComponents(num18);
			}
			else
			{
				float d = this.planet.data.QueryModifiedHeight(this.vegePool[num18].pos) - 0.03f;
				this.vegePool[num18].pos = this.vegePool[num18].pos.normalized * d;
				GameMain.gpuiManager.AlterModel((int)this.vegePool[num18].modelIndex, this.vegePool[num18].modelId, num18, this.vegePool[num18].pos, this.vegePool[num18].rot, false);
			}
		}
		num17 = nearColliderLogic.GetVeinsInAreaNonAlloc(pos, num, ref this.tmp_ids);
		bool flag2 = false;
		for (int m = 0; m < num17; m++)
		{
			int num19 = this.tmp_ids[m];
			Vector3 vector = this.veinPool[num19].pos;
			if (removeVein && bound.Contains(vector))
			{
				this.RemoveVeinWithComponents(num19);
				flag2 = true;
			}
			else if (vector.magnitude > this.planet.realRadius)
			{
				float d2 = this.planet.data.QueryModifiedHeight(vector) - 0.13f;
				this.veinPool[num19].pos = vector.normalized * d2;
				Quaternion rot2 = Maths.SphericalRotation(this.veinPool[num19].pos, Random.value * 360f);
				if ((double)(vector - this.veinPool[num19].pos).sqrMagnitude > 0.01)
				{
					GameMain.gpuiManager.AlterModel((int)this.veinPool[num19].modelIndex, this.veinPool[num19].modelId, num19, this.veinPool[num19].pos, rot2, false);
				}
				else
				{
					GameMain.gpuiManager.AlterModel((int)this.veinPool[num19].modelIndex, this.veinPool[num19].modelId, num19, this.veinPool[num19].pos, false);
				}
				int num20 = this.veinPool[num19].colliderId;
				int num21 = num20 >> 20;
				num20 &= 1048575;
				physics.colChunks[num21].colliderPool[num20].pos = this.veinPool[num19].pos + this.veinPool[num19].pos.normalized * 0.4f;
				physics.SetPlanetPhysicsColliderDirty();
			}
		}
		if (flag2)
		{
			this.RecalculateAllVeinGroups();
		}
		if (autoRefresh)
		{
			this.tmp_levelChanges.Clear();
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		if (autoRefresh)
		{
			GameMain.gpuiManager.SyncAllGPUBuffer();
		}
		return num6;
	}

	// Token: 0x060018E6 RID: 6374 RVA: 0x001A8FC8 File Offset: 0x001A71C8
	public void FlattenTerrainOffline(Vector3 pos, Quaternion rot, Bounds bound, float fade0 = 6f, float fade1 = 1f, bool removeVein = false, bool lift = false)
	{
		if (this.tmp_levelChanges == null)
		{
			this.tmp_levelChanges = new Dictionary<int, int>();
		}
		bound.extents = new Vector3(bound.extents.x, bound.extents.y + 0.5f, bound.extents.z);
		Quaternion quaternion = rot;
		quaternion.w = -quaternion.w;
		Quaternion rotation = Maths.SphericalRotation(pos, 22.5f);
		float realRadius = this.planet.realRadius;
		float num = bound.extents.magnitude + fade0;
		float num2 = num * num;
		float num3 = realRadius * 3.1415927f / ((float)this.planet.precision * 2f);
		int num4 = Mathf.CeilToInt(num * 1.414f / num3 * 1.5f + 0.5f);
		Vector3[] array = new Vector3[]
		{
			pos,
			pos + rotation * (new Vector3(0f, 0f, 1.414f) * num),
			pos + rotation * (new Vector3(0f, 0f, -1.414f) * num),
			pos + rotation * (new Vector3(1.414f, 0f, 0f) * num),
			pos + rotation * (new Vector3(-1.414f, 0f, 0f) * num),
			pos + rotation * (new Vector3(1f, 0f, 1f) * num),
			pos + rotation * (new Vector3(-1f, 0f, -1f) * num),
			pos + rotation * (new Vector3(1f, 0f, -1f) * num),
			pos + rotation * (new Vector3(-1f, 0f, 1f) * num)
		};
		int stride = this.planet.data.stride;
		int dataLength = this.planet.data.dataLength;
		Vector3[] vertices = this.planet.data.vertices;
		ushort[] heightData = this.planet.data.heightData;
		short num5 = (short)(this.planet.realRadius * 100f + 20f);
		foreach (Vector3 vpos in array)
		{
			int num6 = this.planet.data.QueryIndex(vpos);
			for (int j = -num4; j <= num4; j++)
			{
				int num7 = num6 + j * stride;
				if (num7 >= 0 && num7 < dataLength)
				{
					for (int k = -num4; k <= num4; k++)
					{
						int num8 = num7 + k;
						if ((ulong)num8 < (ulong)((long)dataLength) && (lift || heightData[num8] > (ushort)num5))
						{
							Vector3 vector;
							vector.x = vertices[num8].x * realRadius;
							vector.y = vertices[num8].y * realRadius;
							vector.z = vertices[num8].z * realRadius;
							Vector3 vector2;
							vector2.x = vector.x - pos.x;
							vector2.y = vector.y - pos.y;
							vector2.z = vector.z - pos.z;
							if (vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z < num2 && (!this.tmp_levelChanges.ContainsKey(num8) || this.tmp_levelChanges[num8] < 3))
							{
								vector = quaternion * (vector - pos);
								if (bound.Contains(vector))
								{
									this.tmp_levelChanges[num8] = 3;
								}
								else
								{
									float num9 = Vector3.Distance(bound.ClosestPoint(vector), vector);
									int num10 = (int)((fade0 - num9) / (fade0 - fade1) * 3f + 0.5f);
									if (num10 < 0)
									{
										num10 = 0;
									}
									else if (num10 > 3)
									{
										num10 = 3;
									}
									int modLevel = this.planet.data.GetModLevel(num8);
									int num11 = num10 - modLevel;
									if (num10 >= modLevel && num11 != 0)
									{
										if (!this.tmp_levelChanges.ContainsKey(num8))
										{
											this.tmp_levelChanges[num8] = num10;
										}
										else
										{
											Dictionary<int, int> dictionary = this.tmp_levelChanges;
											int key = num8;
											dictionary[key] += num10;
										}
									}
								}
							}
						}
					}
				}
			}
		}
		bool levelized = this.planet.levelized;
		int num12 = Mathf.RoundToInt((pos.magnitude - 0.2f - this.planet.realRadius) / 1.3333333f);
		int num13 = num12 * 133 + (int)num5 - 60;
		foreach (KeyValuePair<int, int> keyValuePair in this.tmp_levelChanges)
		{
			if (keyValuePair.Value > 0)
			{
				if (levelized)
				{
					if ((int)heightData[keyValuePair.Key] >= num13)
					{
						if (this.planet.data.GetModLevel(keyValuePair.Key) < 3)
						{
							this.planet.data.SetModPlane(keyValuePair.Key, num12);
						}
						this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
					}
				}
				else
				{
					this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}
		this.planet.landPercentDirtyFlag = true;
		this.planet.UpdateDirtyMeshes();
		bound.extents += new Vector3(1.5f, 1.5f, 1.5f);
		if (this.tmp_ids == null)
		{
			this.tmp_ids = new int[1024];
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		uint num14 = SimpleHash.GenerateHashMask(pos.x, pos.y, pos.z, 1);
		int num15 = 0;
		for (int l = 1; l < this.vegeCursor; l++)
		{
			if (this.vegePool[l].id == l && this.vegePool[l].hash.MaskPass(num14))
			{
				this.tmp_ids[num15++] = l;
			}
		}
		for (int m = 0; m < num15; m++)
		{
			int num16 = this.tmp_ids[m];
			Vector3 vector = this.vegePool[num16].pos;
			vector = quaternion * (vector - pos);
			if (bound.Contains(vector) && this.vegePool[num16].protoId < 9999)
			{
				this.RemoveVegeWithComponents(num16);
			}
			else
			{
				float d = this.planet.data.QueryModifiedHeight(this.vegePool[num16].pos) - 0.03f;
				this.vegePool[num16].pos = this.vegePool[num16].pos.normalized * d;
			}
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		num15 = 0;
		for (int n = 1; n < this.veinCursor; n++)
		{
			if (this.veinPool[n].id == n)
			{
				uint num17 = SimpleHash.GenerateHashMask(this.veinPool[n].pos.x, this.veinPool[n].pos.y, this.veinPool[n].pos.z, 1);
				if ((num14 & num17) == num17)
				{
					this.tmp_ids[num15++] = n;
				}
			}
		}
		bool flag = false;
		for (int num18 = 0; num18 < num15; num18++)
		{
			int num19 = this.tmp_ids[num18];
			Vector3 vector = this.veinPool[num19].pos;
			if (removeVein && bound.Contains(vector))
			{
				this.RemoveVeinWithComponents(num19);
				flag = true;
			}
			else if (vector.magnitude > this.planet.realRadius)
			{
				float d2 = this.planet.data.QueryModifiedHeight(vector) - 0.13f;
				this.veinPool[num19].pos = vector.normalized * d2;
			}
		}
		if (flag)
		{
			this.RecalculateAllVeinGroups();
		}
	}

	// Token: 0x060018E7 RID: 6375 RVA: 0x001A98DC File Offset: 0x001A7ADC
	public void ComputeFlattenTerrainReform(Vector3[] points, Vector3 center, float radius, int pointsCount, ref int costSandCount, ref int getSandCount, float fade0 = 3f, float fade1 = 1f)
	{
		PlanetRawData data = this.planet.data;
		if (this.tmp_levelChanges == null)
		{
			this.tmp_levelChanges = new Dictionary<int, int>();
		}
		this.tmp_levelChanges.Clear();
		Quaternion rotation = Maths.SphericalRotation(center, 22.5f);
		float realRadius = this.planet.realRadius;
		int num = Mathf.RoundToInt((realRadius + 0.2f) * 100f);
		Vector3[] vertices = data.vertices;
		ushort[] heightData = data.heightData;
		float num2 = ((float)heightData[data.QueryIndex(center)] - this.planet.realRadius * 100f + 20f) * 0.01f * 2f;
		num2 = Mathf.Min(9f, Mathf.Abs(num2));
		fade0 += num2;
		float num3 = radius + fade0;
		float num4 = num3 * num3;
		float num5 = realRadius * 3.1415927f / ((float)this.planet.precision * 2f);
		int num6 = Mathf.CeilToInt(num3 * 1.414f / num5 * 1.5f + 0.5f);
		Vector3[] array = new Vector3[]
		{
			center,
			center + rotation * (new Vector3(0f, 0f, 1.414f) * num3),
			center + rotation * (new Vector3(0f, 0f, -1.414f) * num3),
			center + rotation * (new Vector3(1.414f, 0f, 0f) * num3),
			center + rotation * (new Vector3(-1.414f, 0f, 0f) * num3),
			center + rotation * (new Vector3(1f, 0f, 1f) * num3),
			center + rotation * (new Vector3(-1f, 0f, -1f) * num3),
			center + rotation * (new Vector3(1f, 0f, -1f) * num3),
			center + rotation * (new Vector3(-1f, 0f, 1f) * num3)
		};
		int stride = data.stride;
		int dataLength = data.dataLength;
		float num7 = 8f;
		foreach (Vector3 vpos in array)
		{
			int num8 = data.QueryIndex(vpos);
			for (int j = -num6; j <= num6; j++)
			{
				int num9 = num8 + j * stride;
				if (num9 >= 0 && num9 < dataLength)
				{
					for (int k = -num6; k <= num6; k++)
					{
						int num10 = num9 + k;
						if ((ulong)num10 < (ulong)((long)dataLength))
						{
							Vector3 vector;
							vector.x = vertices[num10].x * realRadius;
							vector.y = vertices[num10].y * realRadius;
							vector.z = vertices[num10].z * realRadius;
							Vector3 vector2;
							vector2.x = vector.x - center.x;
							vector2.y = vector.y - center.y;
							vector2.z = vector.z - center.z;
							if (vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z < num4 && !this.tmp_levelChanges.ContainsKey(num10))
							{
								float num11 = float.PositiveInfinity;
								for (int l = 0; l < pointsCount; l++)
								{
									float num12 = points[l].x - vector.x;
									float num13 = points[l].y - vector.y;
									float num14 = points[l].z - vector.z;
									float num15 = num12 * num12 + num13 * num13 + num14 * num14;
									num11 = ((num11 < num15) ? num11 : num15);
								}
								int modLevel = data.GetModLevel(num10);
								int num16;
								if (num11 <= num7)
								{
									num16 = 3;
								}
								else
								{
									num11 -= num7;
									if (num11 > fade0 * fade0)
									{
										goto IL_4F5;
									}
									float num17 = num11 / (fade0 * fade0);
									if (num17 <= 0.1111111f)
									{
										num16 = 2;
									}
									else if (num17 <= 0.4444444f)
									{
										num16 = 1;
									}
									else
									{
										if (num17 >= 1f)
										{
											goto IL_4F5;
										}
										num16 = 0;
									}
								}
								int num18 = num16 - modLevel;
								if (num16 >= modLevel && num18 != 0)
								{
									this.tmp_levelChanges[num10] = num16;
									int num19 = num - (int)heightData[num10];
									if (num19 < 0)
									{
										getSandCount -= Mathf.FloorToInt((float)(num18 * num19) * 0.6666667f);
									}
									else
									{
										costSandCount += Mathf.FloorToInt((float)(num18 * num19) * 0.3333333f);
									}
								}
							}
						}
						IL_4F5:;
					}
				}
			}
		}
	}

	// Token: 0x060018E8 RID: 6376 RVA: 0x001A9E10 File Offset: 0x001A8010
	public void FlattenTerrainReform(Vector3 center, float radius, int reformSize, bool veinBuried, float fade0 = 3f)
	{
		if (this.tmp_ids == null)
		{
			this.tmp_ids = new int[1024];
		}
		if (this.tmp_entity_ids == null)
		{
			this.tmp_entity_ids = new int[1024];
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		Array.Clear(this.tmp_entity_ids, 0, this.tmp_entity_ids.Length);
		Vector3 vector = Vector3.zero;
		PlanetRawData data = this.planet.data;
		ushort[] heightData = data.heightData;
		float num = ((float)heightData[data.QueryIndex(center)] - this.planet.realRadius * 100f + 20f) * 0.01f * 2f;
		num = Mathf.Min(9f, Mathf.Abs(num));
		fade0 += num;
		float areaRadius = radius + fade0;
		short num2 = (short)(this.planet.realRadius * 100f + 20f);
		bool levelized = this.planet.levelized;
		int num3 = Mathf.RoundToInt((center.magnitude - 0.2f - this.planet.realRadius) / 1.3333333f);
		int num4 = num3 * 133 + (int)num2 - 60;
		float radius2 = this.planet.radius;
		foreach (KeyValuePair<int, int> keyValuePair in this.tmp_levelChanges)
		{
			if (keyValuePair.Value > 0)
			{
				ushort num5 = heightData[keyValuePair.Key];
				if (levelized)
				{
					if ((int)num5 >= num4)
					{
						if (data.GetModLevel(keyValuePair.Key) < 3)
						{
							data.SetModPlane(keyValuePair.Key, num3);
						}
						this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
					}
				}
				else
				{
					this.planet.AddHeightMapModLevel(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}
		bool flag = this.planet.UpdateDirtyMeshes();
		if (GameMain.isRunning && flag)
		{
			this.RenderLocalPlanetHeightmap();
		}
		if (flag)
		{
			PlanetAlgorithm.CalcLandPercent(this.planet);
		}
		radius -= (float)reformSize * 0.15f;
		NearColliderLogic nearColliderLogic = this.planet.physics.nearColliderLogic;
		int num6 = nearColliderLogic.GetVegetablesInAreaNonAlloc(center, areaRadius, this.tmp_ids);
		float num7 = (radius + 1f) * (radius + 1f);
		float num8 = (radius + 2f) * (radius + 2f);
		float num9 = (radius + 10f) * (radius + 10f);
		for (int i = 0; i < num6; i++)
		{
			int num10 = this.tmp_ids[i];
			vector = this.vegePool[num10].pos;
			vector -= center;
			if (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z <= num8 && this.vegePool[num10].protoId < 9999)
			{
				if (this.gameData.localLoadedPlanetFactory == this)
				{
					VegeProto vegeProto = LDB.veges.Select((int)this.vegePool[num10].protoId);
					if (vegeProto != null)
					{
						VFEffectEmitter.Emit(vegeProto.MiningEffect, this.vegePool[num10].pos, this.vegePool[num10].rot);
						VFAudio.Create(vegeProto.MiningAudio, null, this.vegePool[num10].pos, true, 1, -1, -1L);
					}
				}
				this.RemoveVegeWithComponents(num10);
			}
			else
			{
				float d = data.QueryModifiedHeight(this.vegePool[num10].pos) - 0.03f;
				this.vegePool[num10].pos = this.vegePool[num10].pos.normalized * d;
				GameMain.gpuiManager.AlterModel((int)this.vegePool[num10].modelIndex, this.vegePool[num10].modelId, num10, this.vegePool[num10].pos, this.vegePool[num10].rot, false);
			}
		}
		float num11 = this.planet.realRadius - 50f;
		float num12 = this.planet.realRadius - 60f;
		float num13 = num11 + 5f;
		ObjectPool<DFGBaseComponent> bases = this.enemySystem.bases;
		int cursor = bases.cursor;
		float num14 = 136.5625f;
		bool flag2 = false;
		num6 = (veinBuried ? nearColliderLogic.GetVeinsInAreaNonAlloc(center, areaRadius, ref this.tmp_ids) : nearColliderLogic.GetVeinsInOceanInAreaNonAlloc(center, areaRadius, ref this.tmp_ids));
		for (int j = 0; j < num6; j++)
		{
			int num15 = this.tmp_ids[j];
			vector = this.veinPool[num15].pos;
			float num16 = vector.magnitude;
			float num17 = data.QueryModifiedHeight(this.veinPool[num15].pos) - 0.13f;
			if (num16 > num13)
			{
				num16 = num17;
			}
			else if (num16 < num11)
			{
				num16 = num12;
			}
			else
			{
				num16 = num11;
			}
			float num18 = num16;
			Vector3 vector2 = vector.normalized * num17 - center;
			float num19 = vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z;
			if (num19 <= num9)
			{
				PlanetPhysics physics = this.planet.physics;
				int num20 = this.veinPool[num15].colliderId;
				ColliderData colliderData = physics.GetColliderData(num20);
				if (num19 < num7)
				{
					if (veinBuried)
					{
						num18 = num11;
					}
					else
					{
						Vector3 vector3 = vector.normalized * this.planet.realRadius;
						int num21 = nearColliderLogic.GetEntitiesInAreaWhenReformNonAlloc(vector3, colliderData.radius, ref this.tmp_entity_ids);
						for (int k = 1; k < cursor; k++)
						{
							DFGBaseComponent dfgbaseComponent = bases[k];
							if (dfgbaseComponent != null && dfgbaseComponent.id == k && (vector3 - this.enemyPool[dfgbaseComponent.enemyId].pos).sqrMagnitude < num14)
							{
								num21++;
								break;
							}
						}
						if (num21 == 0)
						{
							num18 = num17;
						}
					}
				}
				Vector3 pos = colliderData.pos.normalized * (num18 + 0.4f);
				int num22 = num20 >> 20;
				num20 &= 1048575;
				physics.colChunks[num22].colliderPool[num20].pos = pos;
				this.veinPool[num15].pos = vector.normalized * num18;
				physics.SetPlanetPhysicsColliderDirty();
				Vector3 pos2 = this.veinPool[num15].pos;
				Quaternion rot = Maths.SphericalRotation(this.veinPool[num15].pos, Random.value * 360f);
				if ((double)(vector - this.veinPool[num15].pos).sqrMagnitude > 0.01)
				{
					GameMain.gpuiManager.AlterModel((int)this.veinPool[num15].modelIndex, this.veinPool[num15].modelId, num15, pos2, rot, false);
				}
				else
				{
					GameMain.gpuiManager.AlterModel((int)this.veinPool[num15].modelIndex, this.veinPool[num15].modelId, num15, this.veinPool[num15].pos, false);
				}
				VeinProto veinProto = LDB.veins.Select((int)this.veinPool[num15].type);
				if (veinProto != null)
				{
					float magnitude = pos2.magnitude;
					Vector3 a = pos2 / magnitude;
					if (this.veinPool[num15].minerId0 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerBaseModelIndex, this.veinPool[num15].minerBaseModelId, this.veinPool[num15].minerId0, a * (magnitude + 0.1f), false);
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num15].minerCircleModelId0, this.veinPool[num15].minerId0, a * (magnitude + 0.4f), false);
					}
					if (this.veinPool[num15].minerId1 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num15].minerCircleModelId1, this.veinPool[num15].minerId1, a * (magnitude + 0.6f), false);
					}
					if (this.veinPool[num15].minerId2 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num15].minerCircleModelId2, this.veinPool[num15].minerId2, a * (magnitude + 0.8f), false);
					}
					if (this.veinPool[num15].minerId3 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num15].minerCircleModelId3, this.veinPool[num15].minerId3, a * (magnitude + 1f), false);
					}
				}
				if ((num16 > num13 && num18 < num13) || (num16 < num13 && num18 > num13))
				{
					flag2 = true;
				}
			}
		}
		if (flag2 && GameMain.gameScenario != null)
		{
			GameMain.gameScenario.NotifyOnVeinReformed(veinBuried ? 1 : 2);
		}
		if (this.planet.waterItemId == -1)
		{
			int entitiesInAreaWhenReformNonAlloc = nearColliderLogic.GetEntitiesInAreaWhenReformNonAlloc(center, areaRadius, ref this.tmp_entity_ids);
			for (int l = 0; l < entitiesInAreaWhenReformNonAlloc; l++)
			{
				int powerGenId = this.entityPool[this.tmp_entity_ids[l]].powerGenId;
				if (powerGenId > 0 && this.powerSystem.genPool[powerGenId].id == powerGenId && this.powerSystem.genPool[powerGenId].geothermal)
				{
					this.powerSystem.genPool[powerGenId].gthStrength = this.powerSystem.CalculateGeothermalStrength(this.entityPool[this.tmp_entity_ids[l]].pos, this.entityPool[this.tmp_entity_ids[l]].rot, this.powerSystem.genPool[powerGenId].baseRuinId);
				}
			}
		}
		this.tmp_levelChanges.Clear();
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		Array.Clear(this.tmp_entity_ids, 0, this.tmp_entity_ids.Length);
		GameMain.gpuiManager.SyncAllGPUBuffer();
	}

	// Token: 0x060018E9 RID: 6377 RVA: 0x001AA918 File Offset: 0x001A8B18
	public void PlanetReformAll(int type, int color, bool bury)
	{
		if (this.planet.type == EPlanetType.Gas)
		{
			return;
		}
		type &= 7;
		color &= 31;
		byte b = (byte)(type * 32 + color);
		PlanetFactory factory = this.planet.factory;
		PlatformSystem platformSystem = factory.platformSystem;
		platformSystem.EnsureReformData();
		int num = platformSystem.maxReformCount;
		for (int i = 0; i < num; i++)
		{
			platformSystem.reformData[i] = b;
		}
		byte[] modData = this.planet.data.modData;
		num = modData.Length;
		for (int j = 0; j < num; j++)
		{
			modData[j] = 51;
		}
		bool[] dirtyFlags = this.planet.dirtyFlags;
		num = dirtyFlags.Length;
		for (int k = 0; k < num; k++)
		{
			dirtyFlags[k] = true;
		}
		if (this.planet.UpdateDirtyMeshes())
		{
			factory.RenderLocalPlanetHeightmap();
		}
		PlanetAlgorithm.CalcLandPercent(this.planet);
		for (int l = 1; l < this.vegeCursor; l++)
		{
			this.RemoveVegeWithComponents(l);
		}
		PlanetPhysics physics = this.planet.physics;
		float num2 = this.planet.realRadius - 50f;
		float num3 = this.planet.realRadius + 0.07f;
		float num4 = bury ? num2 : num3;
		VeinData[] array = factory.veinPool;
		num = factory.veinCursor;
		for (int m = 1; m < num; m++)
		{
			Vector3 pos = array[m].pos;
			int num5 = array[m].colliderId;
			ColliderData colliderData = physics.GetColliderData(num5);
			float num6 = num4;
			Vector3 pos2 = colliderData.pos.normalized * (num6 + 0.4f);
			int num7 = num5 >> 20;
			num5 &= 1048575;
			physics.colChunks[num7].colliderPool[num5].pos = pos2;
			array[m].pos = pos.normalized * num6;
			Quaternion rot = Maths.SphericalRotation(array[m].pos, Random.value * 360f);
			physics.SetPlanetPhysicsColliderDirty();
			GameMain.gpuiManager.AlterModel((int)array[m].modelIndex, array[m].modelId, m, array[m].pos, rot, false);
		}
		GameMain.gpuiManager.SyncAllGPUBuffer();
	}

	// Token: 0x060018EA RID: 6378 RVA: 0x001AAB74 File Offset: 0x001A8D74
	public void PlanetReformRevert()
	{
		if (this.planet.type == EPlanetType.Gas)
		{
			return;
		}
		PlanetRawData data = this.planet.data;
		int num = data.dataLength / 2;
		byte[] modData = data.modData;
		for (int i = 0; i < num; i++)
		{
			modData[i] = 0;
		}
		bool[] dirtyFlags = this.planet.dirtyFlags;
		num = dirtyFlags.Length;
		for (int j = 0; j < num; j++)
		{
			dirtyFlags[j] = true;
		}
		this.platformSystem.EnsureReformData();
		num = this.platformSystem.maxReformCount;
		for (int k = 0; k < num; k++)
		{
			this.platformSystem.SetReformType(k, 0);
			this.platformSystem.SetReformColor(k, 0);
		}
		this.planet.RegenerateVegetationImmediately();
		PlanetPhysics physics = this.planet.physics;
		for (int l = 1; l < this.vegeCursor; l++)
		{
			this.RemoveVegeWithComponents(l);
		}
		this.SetVegeCapacity(data.vegeCursor + 2);
		this.vegeCursor = data.vegeCursor;
		this.vegeRecycleCursor = 0;
		Array.Copy(data.vegePool, this.vegePool, this.vegeCursor);
		Debug.Log("vege count = " + (this.vegeCursor - 1).ToString());
		VegeProto[] vegeProtos = PlanetModelingManager.vegeProtos;
		for (int m = 1; m < this.vegeCursor; m++)
		{
			if (this.vegePool[m].id == m)
			{
				VegeProto vegeProto = vegeProtos[(int)this.vegePool[m].protoId];
				if (vegeProto != null)
				{
					this.vegePool[m].modelId = 0;
					this.vegePool[m].colliderId = 0;
					this.vegePool[m].modelId = GameMain.gpuiManager.AddModel((int)this.vegePool[m].modelIndex, m, this.vegePool[m].pos, this.vegePool[m].rot, false);
					ColliderData[] colliders = vegeProto.prefabDesc.colliders;
					int num2 = 0;
					while (colliders != null && num2 < colliders.Length)
					{
						this.vegePool[m].colliderId = physics.AddColliderData(colliders[num2].BindToObject(m, this.vegePool[m].colliderId, EObjectType.Vegetable, this.vegePool[m].pos, this.vegePool[m].rot, this.vegePool[m].scl));
						num2++;
					}
				}
				this.vegePool[m].hashAddress = this.hashSystemStatic.AddObjectToBucket(m, this.vegePool[m].pos, EObjectType.Vegetable);
			}
		}
		Debug.Log("vein count = " + (this.veinCursor - 1).ToString());
		VeinProto[] veinProtos = PlanetModelingManager.veinProtos;
		for (int n = 1; n < this.veinCursor; n++)
		{
			if (this.veinPool[n].id == n)
			{
				if (this.veinPool[n].amount <= 0)
				{
					this.RemoveVeinData(n);
				}
				else
				{
					float num3 = data.QueryModifiedHeight(this.veinPool[n].pos) - 0.13f;
					if (Mathf.Abs(num3 * num3 - this.veinPool[n].pos.sqrMagnitude) >= 20f)
					{
						int num4 = this.veinPool[n].colliderId;
						Vector3 pos = physics.GetColliderData(num4).pos.normalized * (num3 + 0.4f);
						int num5 = num4 >> 20;
						num4 &= 1048575;
						physics.colChunks[num5].colliderPool[num4].pos = pos;
						this.veinPool[n].pos = this.veinPool[n].pos.normalized * num3;
						Quaternion rot = Maths.SphericalRotation(this.veinPool[n].pos, Random.value * 360f);
						Vector3 pos2 = this.veinPool[n].pos;
						GameMain.gpuiManager.AlterModel((int)this.veinPool[n].modelIndex, this.veinPool[n].modelId, n, pos2, rot, false);
						VeinProto veinProto = LDB.veins.Select((int)this.veinPool[n].type);
						if (veinProto != null)
						{
							float magnitude = pos2.magnitude;
							Vector3 a = pos2 / magnitude;
							if (this.veinPool[n].minerId0 > 0)
							{
								GameMain.gpuiManager.AlterModel(veinProto.MinerBaseModelIndex, this.veinPool[n].minerBaseModelId, this.veinPool[n].minerId0, a * (magnitude + 0.1f), false);
								GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[n].minerCircleModelId0, this.veinPool[n].minerId0, a * (magnitude + 0.4f), false);
							}
							if (this.veinPool[n].minerId1 > 0)
							{
								GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[n].minerCircleModelId1, this.veinPool[n].minerId1, a * (magnitude + 0.6f), false);
							}
							if (this.veinPool[n].minerId2 > 0)
							{
								GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[n].minerCircleModelId2, this.veinPool[n].minerId2, a * (magnitude + 0.8f), false);
							}
							if (this.veinPool[n].minerId3 > 0)
							{
								GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[n].minerCircleModelId3, this.veinPool[n].minerId3, a * (magnitude + 1f), false);
							}
						}
					}
				}
			}
		}
		if (this.planet.UpdateDirtyMeshes())
		{
			this.RenderLocalPlanetHeightmap();
		}
		PlanetAlgorithm.CalcLandPercent(this.planet);
		GameMain.gpuiManager.SyncAllGPUBuffer();
		physics.SetPlanetPhysicsColliderDirty();
	}

	// Token: 0x060018EB RID: 6379 RVA: 0x001AB23C File Offset: 0x001A943C
	public HashSet<int> GetRemovableDFGBaseRuinIds()
	{
		if (PlanetFactory.tmpRemovableDFGBaseRuinId == null)
		{
			PlanetFactory.tmpRemovableDFGBaseRuinId = new HashSet<int>(8);
		}
		else
		{
			PlanetFactory.tmpRemovableDFGBaseRuinId.Clear();
		}
		for (int i = 1; i < this.ruinCursor; i++)
		{
			if (this.ruinPool[i].id == i && this.ruinPool[i].modelIndex == 406)
			{
				PlanetFactory.tmpRemovableDFGBaseRuinId.Add(i);
			}
		}
		DFGBaseComponent[] buffer = this.enemySystem.bases.buffer;
		int cursor = this.enemySystem.bases.cursor;
		for (int j = 1; j < cursor; j++)
		{
			DFGBaseComponent dfgbaseComponent = buffer[j];
			if (dfgbaseComponent != null && dfgbaseComponent.id == j && dfgbaseComponent.ruinId > 0 && this.enemySystem.CheckBaseCanRemoved(j) > 0)
			{
				PlanetFactory.tmpRemovableDFGBaseRuinId.Remove(dfgbaseComponent.ruinId);
			}
		}
		PowerGeneratorComponent[] genPool = this.powerSystem.genPool;
		int genCursor = this.powerSystem.genCursor;
		for (int k = 1; k < genCursor; k++)
		{
			ref PowerGeneratorComponent ptr = ref genPool[k];
			if (ptr.id == k && ptr.geothermal && ptr.baseRuinId != 0 && PlanetFactory.tmpRemovableDFGBaseRuinId.Contains(ptr.baseRuinId))
			{
				PlanetFactory.tmpRemovableDFGBaseRuinId.Remove(ptr.baseRuinId);
				break;
			}
		}
		for (int l = 1; l < this.prebuildCursor; l++)
		{
			ref PrebuildData ptr2 = ref this.prebuildPool[l];
			if (ptr2.id == l && PlanetFactory.PrefabDescByModelIndex[(int)ptr2.modelIndex].geothermal && ptr2.parameters[0] != 0 && PlanetFactory.tmpRemovableDFGBaseRuinId.Contains(ptr2.parameters[0]))
			{
				PlanetFactory.tmpRemovableDFGBaseRuinId.Remove(ptr2.parameters[0]);
				break;
			}
		}
		return PlanetFactory.tmpRemovableDFGBaseRuinId;
	}

	// Token: 0x060018EC RID: 6380 RVA: 0x001AB420 File Offset: 0x001A9620
	public void BuriedVeins(Vector3 center, float radius, float buriedOffset = 50f)
	{
		if (this.tmp_ids == null)
		{
			this.tmp_ids = new int[1024];
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
		float num = this.planet.realRadius - buriedOffset;
		float num2 = num + 5f;
		float num3 = (radius + 1f) * (radius + 1f);
		float num4 = (radius + 10f) * (radius + 10f);
		PlanetRawData data = this.planet.data;
		PlanetPhysics physics = this.planet.physics;
		if (physics == null)
		{
			return;
		}
		int veinsInAreaNonAlloc = this.planet.physics.nearColliderLogic.GetVeinsInAreaNonAlloc(center, radius, ref this.tmp_ids);
		for (int i = 0; i < veinsInAreaNonAlloc; i++)
		{
			int num5 = this.tmp_ids[i];
			Vector3 pos = this.veinPool[num5].pos;
			float num6 = pos.magnitude;
			float num7 = data.QueryModifiedHeight(this.veinPool[num5].pos) - 0.13f;
			if (num6 > num2)
			{
				num6 = num7;
			}
			else
			{
				num6 = num;
			}
			float num8 = num6;
			Vector3 vector = pos.normalized * num7 - center;
			float num9 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
			if (num9 <= num4)
			{
				int num10 = this.veinPool[num5].colliderId;
				ColliderData colliderData = physics.GetColliderData(num10);
				if (num9 < num3)
				{
					num8 = num;
				}
				Vector3 pos2 = colliderData.pos.normalized * (num8 + 0.4f);
				int num11 = num10 >> 20;
				num10 &= 1048575;
				physics.colChunks[num11].colliderPool[num10].pos = pos2;
				this.veinPool[num5].pos = pos.normalized * num8;
				physics.SetPlanetPhysicsColliderDirty();
				Vector3 pos3 = this.veinPool[num5].pos;
				Quaternion rot = Maths.SphericalRotation(this.veinPool[num5].pos, Random.value * 360f);
				if ((double)(pos - this.veinPool[num5].pos).sqrMagnitude > 0.01)
				{
					GameMain.gpuiManager.AlterModel((int)this.veinPool[num5].modelIndex, this.veinPool[num5].modelId, num5, pos3, rot, false);
				}
				else
				{
					GameMain.gpuiManager.AlterModel((int)this.veinPool[num5].modelIndex, this.veinPool[num5].modelId, num5, this.veinPool[num5].pos, false);
				}
				VeinProto veinProto = LDB.veins.Select((int)this.veinPool[num5].type);
				if (veinProto != null)
				{
					float magnitude = pos3.magnitude;
					Vector3 a = pos3 / magnitude;
					if (this.veinPool[num5].minerId0 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerBaseModelIndex, this.veinPool[num5].minerBaseModelId, this.veinPool[num5].minerId0, a * (magnitude + 0.1f), false);
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num5].minerCircleModelId0, this.veinPool[num5].minerId0, a * (magnitude + 0.4f), false);
					}
					if (this.veinPool[num5].minerId1 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num5].minerCircleModelId1, this.veinPool[num5].minerId1, a * (magnitude + 0.6f), false);
					}
					if (this.veinPool[num5].minerId2 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num5].minerCircleModelId2, this.veinPool[num5].minerId2, a * (magnitude + 0.8f), false);
					}
					if (this.veinPool[num5].minerId3 > 0)
					{
						GameMain.gpuiManager.AlterModel(veinProto.MinerCircleModelIndex, this.veinPool[num5].minerCircleModelId3, this.veinPool[num5].minerId3, a * (magnitude + 1f), false);
					}
				}
			}
		}
		Array.Clear(this.tmp_ids, 0, this.tmp_ids.Length);
	}

	// Token: 0x060018ED RID: 6381 RVA: 0x001AB904 File Offset: 0x001A9B04
	public void RenderLocalPlanetHeightmap()
	{
		PlanetData localPlanet = GameMain.localPlanet;
		if (localPlanet != null)
		{
			int cullingMask = PlanetModelingManager.heightmapCamera.cullingMask;
			PlanetModelingManager.heightmapCamera.cullingMask = 512;
			PlanetModelingManager.heightmapCamera.RenderToCubemap(localPlanet.heightmap, 63);
			PlanetModelingManager.heightmapCamera.cullingMask = cullingMask;
		}
	}

	// Token: 0x060018EE RID: 6382 RVA: 0x001AB954 File Offset: 0x001A9B54
	public void Export(Stream s, BinaryWriter w)
	{
		w.Write(10);
		PerformanceMonitor.BeginData(ESaveDataEntry.Planet);
		w.Write(this.planetId);
		w.Write(this.planet.theme);
		w.Write(this.planet.algoId);
		w.Write(this.planet.style);
		this.planet.ExportRuntime(w);
		w.Write(this.landed);
		PerformanceMonitor.EndData(ESaveDataEntry.Planet);
		PerformanceMonitor.BeginData(ESaveDataEntry.Factory);
		this.hashSystemDynamic.Export(w);
		this.hashSystemStatic.Export(w);
		PerformanceMonitor.BeginData(ESaveDataEntry.Entity);
		w.Write(this.entityCapacity);
		w.Write(this.entityCursor);
		w.Write(this.entityRecycleCursor);
		for (int i = 1; i < this.entityCursor; i++)
		{
			this.entityPool[i].Export(s, w);
		}
		UnsafeIO.WriteMassive<AnimData>(s, this.entityAnimPool, this.entityCursor);
		for (int j = 1; j < this.entityCursor; j++)
		{
			if (this.entityPool[j].id != 0)
			{
				bool flag = this.entitySignPool[j].count0 > 0.0001f;
				w.Write((byte)this.entitySignPool[j].signType);
				w.Write((byte)((ulong)this.entitySignPool[j].iconType + (ulong)(flag ? 128L : 0L)));
				w.Write((ushort)this.entitySignPool[j].iconId0);
				if (flag)
				{
					w.Write(this.entitySignPool[j].count0);
				}
				UnsafeIO.Write<SignData>(s, ref this.entitySignPool[j], 40, 16);
			}
		}
		for (int k = 16; k < this.entityCursor * 16; k++)
		{
			if (this.entityPool[k / 16].id != 0)
			{
				if (this.entityConnPool[k] == 0)
				{
					w.Write(0);
				}
				else
				{
					w.Write(1);
					w.Write(this.entityConnPool[k]);
				}
			}
		}
		for (int l = 0; l < this.entityRecycleCursor; l++)
		{
			w.Write(this.entityRecycle[l]);
		}
		w.Write(this.prebuildCapacity);
		w.Write(this.prebuildCursor);
		w.Write(this.prebuildRecycleCursor);
		for (int m = 1; m < this.prebuildCursor; m++)
		{
			this.prebuildPool[m].Export(w);
			if (this.prebuildPool[m].id != 0)
			{
				int num = m * 16;
				int num2 = num + 16;
				for (int n = num; n < num2; n++)
				{
					if (this.prebuildConnPool[n] == 0)
					{
						w.Write(0);
					}
					else
					{
						w.Write(1);
						w.Write(this.prebuildConnPool[n]);
					}
				}
			}
		}
		for (int num3 = 0; num3 < this.prebuildRecycleCursor; num3++)
		{
			w.Write(this.prebuildRecycle[num3]);
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Entity);
		w.Write(this.craftCapacity);
		w.Write(this.craftCursor);
		w.Write(this.craftRecycleCursor);
		for (int num4 = 1; num4 < this.craftCursor; num4++)
		{
			this.craftPool[num4].Export(w);
		}
		for (int num5 = 0; num5 < this.craftRecycleCursor; num5++)
		{
			w.Write(this.craftRecycle[num5]);
		}
		for (int num6 = 1; num6 < this.craftCursor; num6++)
		{
			w.Write(this.craftAnimPool[num6].time);
			w.Write(this.craftAnimPool[num6].prepare_length);
			w.Write(this.craftAnimPool[num6].working_length);
			w.Write(this.craftAnimPool[num6].state);
			w.Write(this.craftAnimPool[num6].power);
		}
		PerformanceMonitor.BeginData(ESaveDataEntry.Combat);
		w.Write(this.enemyCapacity);
		w.Write(this.enemyCursor);
		w.Write(this.enemyRecycleCursor);
		for (int num7 = 1; num7 < this.enemyCursor; num7++)
		{
			this.enemyPool[num7].Export(w);
		}
		for (int num8 = 0; num8 < this.enemyRecycleCursor; num8++)
		{
			w.Write(this.enemyRecycle[num8]);
		}
		for (int num9 = 1; num9 < this.enemyCursor; num9++)
		{
			w.Write(this.enemyAnimPool[num9].time);
			w.Write(this.enemyAnimPool[num9].prepare_length);
			w.Write(this.enemyAnimPool[num9].working_length);
			w.Write(this.enemyAnimPool[num9].state);
			w.Write(this.enemyAnimPool[num9].power);
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Combat);
		PerformanceMonitor.EndData(ESaveDataEntry.Factory);
		PerformanceMonitor.BeginData(ESaveDataEntry.Planet);
		w.Write(this.vegeCapacity);
		w.Write(this.vegeCursor);
		w.Write(this.vegeRecycleCursor);
		for (int num10 = 1; num10 < this.vegeCursor; num10++)
		{
			this.vegePool[num10].Export(w);
		}
		for (int num11 = 0; num11 < this.vegeRecycleCursor; num11++)
		{
			w.Write(this.vegeRecycle[num11]);
		}
		w.Write(this.veinCapacity);
		w.Write(this.veinCursor);
		w.Write(this.veinRecycleCursor);
		for (int num12 = 1; num12 < this.veinCursor; num12++)
		{
			this.veinPool[num12].Export(w);
		}
		for (int num13 = 0; num13 < this.veinRecycleCursor; num13++)
		{
			w.Write(this.veinRecycle[num13]);
		}
		for (int num14 = 1; num14 < this.veinCursor; num14++)
		{
			w.Write(this.veinAnimPool[num14].time);
			w.Write(this.veinAnimPool[num14].prepare_length);
			w.Write(this.veinAnimPool[num14].working_length);
			w.Write(this.veinAnimPool[num14].state);
			w.Write(this.veinAnimPool[num14].power);
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Planet);
		PerformanceMonitor.BeginData(ESaveDataEntry.Factory);
		PerformanceMonitor.BeginData(ESaveDataEntry.Ruin);
		w.Write(this.ruinCapacity);
		w.Write(this.ruinCursor);
		w.Write(this.ruinRecycleCursor);
		for (int num15 = 1; num15 < this.ruinCursor; num15++)
		{
			this.ruinPool[num15].Export(w);
		}
		for (int num16 = 0; num16 < this.ruinRecycleCursor; num16++)
		{
			w.Write(this.ruinRecycle[num16]);
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Ruin);
		PerformanceMonitor.BeginData(ESaveDataEntry.BeltAndCargo);
		this.cargoContainer.Export(w);
		this.cargoTraffic.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.BeltAndCargo);
		PerformanceMonitor.BeginData(ESaveDataEntry.Storage);
		this.factoryStorage.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Storage);
		PerformanceMonitor.BeginData(ESaveDataEntry.PowerSystem);
		this.powerSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.PowerSystem);
		PerformanceMonitor.BeginData(ESaveDataEntry.Facility);
		this.factorySystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Facility);
		PerformanceMonitor.BeginData(ESaveDataEntry.Combat);
		this.enemySystem.Export(w);
		PerformanceMonitor.BeginData(ESaveDataEntry.Craft);
		this.combatGroundSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Craft);
		PerformanceMonitor.BeginData(ESaveDataEntry.Defense);
		this.defenseSystem.Export(w);
		this.planetATField.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Defense);
		PerformanceMonitor.EndData(ESaveDataEntry.Combat);
		PerformanceMonitor.BeginData(ESaveDataEntry.Construction);
		this.constructionSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Construction);
		PerformanceMonitor.BeginData(ESaveDataEntry.Transport);
		this.transport.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Transport);
		PerformanceMonitor.BeginData(ESaveDataEntry.Platform);
		this.platformSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Platform);
		PerformanceMonitor.BeginData(ESaveDataEntry.Digital);
		this.digitalSystem.Export(w);
		PerformanceMonitor.EndData(ESaveDataEntry.Digital);
		PerformanceMonitor.EndData(ESaveDataEntry.Factory);
	}

	// Token: 0x060018EF RID: 6383 RVA: 0x001AC1B8 File Offset: 0x001AA3B8
	public void Import(int _index, GameData _gameData, Stream s, BinaryReader r)
	{
		this.index = _index;
		this.gameData = _gameData;
		this.sector = this.gameData.spaceSector;
		this.skillSystem = this.gameData.spaceSector.skillSystem;
		int num = r.ReadInt32();
		bool flag = num >= 2;
		bool flag2 = num >= 4;
		PerformanceMonitor.BeginData(ESaveDataEntry.Planet);
		int planetId = r.ReadInt32();
		this.planet = this.gameData.galaxy.PlanetById(planetId);
		this.planet.factory = this;
		this.planet.factoryIndex = _index;
		int[] savedThemeIds = this.gameData.gameDesc.savedThemeIds;
		if (num >= 5)
		{
			r.ReadInt32();
			r.ReadInt32();
			int style = r.ReadInt32();
			this.planet.style = style;
		}
		else
		{
			this.planet.style = 0;
		}
		this.planet.ImportRuntime(r);
		if (num >= 3)
		{
			this.landed = r.ReadBoolean();
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Planet);
		PerformanceMonitor.BeginData(ESaveDataEntry.Factory);
		if (num >= 8)
		{
			this.hashSystemDynamic = new HashSystem(true);
			this.hashSystemDynamic.Import(r);
			this.hashSystemStatic = new HashSystem(true);
			this.hashSystemStatic.Import(r);
		}
		else
		{
			this.hashSystemDynamic = new HashSystem();
			this.hashSystemStatic = new HashSystem();
		}
		this.spaceHashSystemDynamic = new DFSDynamicHashSystem();
		this.spaceHashSystemDynamic.Init(this.planet);
		PerformanceMonitor.BeginData(ESaveDataEntry.Entity);
		int num2;
		if (flag2)
		{
			num2 = r.ReadInt32();
			this.SetEntityCapacity(num2);
			this.entityCursor = r.ReadInt32();
			this.entityRecycleCursor = r.ReadInt32();
			if (num <= 8)
			{
				for (int i = 1; i < this.entityCursor; i++)
				{
					this.entityPool[i].Import(s, r);
					if (this.entityPool[i].id != 0)
					{
						bool flag3 = false;
						this.entityAnimPool[i].time = r.ReadSingle();
						this.entityAnimPool[i].prepare_length = r.ReadSingle();
						this.entityAnimPool[i].working_length = r.ReadSingle();
						this.entityAnimPool[i].state = r.ReadUInt32();
						this.entityAnimPool[i].power = r.ReadSingle();
						this.entitySignPool[i].signType = (uint)r.ReadByte();
						this.entitySignPool[i].iconType = (uint)r.ReadByte();
						if (this.entitySignPool[i].iconType >= 128U)
						{
							flag3 = true;
							SignData[] array = this.entitySignPool;
							int num3 = i;
							array[num3].iconType = array[num3].iconType - 128U;
						}
						this.entitySignPool[i].iconId0 = (uint)r.ReadUInt16();
						if (flag3)
						{
							this.entitySignPool[i].count0 = r.ReadSingle();
						}
						this.entitySignPool[i].x = r.ReadSingle();
						this.entitySignPool[i].y = r.ReadSingle();
						this.entitySignPool[i].z = r.ReadSingle();
						this.entitySignPool[i].w = r.ReadSingle();
						int num4 = i * 16;
						int num5 = num4 + 16;
						for (int j = num4; j < num5; j++)
						{
							if (r.ReadByte() == 0)
							{
								this.entityConnPool[j] = 0;
							}
							else
							{
								this.entityConnPool[j] = r.ReadInt32();
							}
						}
						if (this.entityPool[i].beltId == 0 && this.entityPool[i].inserterId == 0 && this.entityPool[i].splitterId == 0 && this.entityPool[i].monitorId == 0 && this.entityPool[i].spraycoaterId == 0 && this.entityPool[i].pilerId == 0)
						{
							this.entityMutexs[i] = new Mutex(i);
						}
					}
				}
			}
			else
			{
				for (int k = 1; k < this.entityCursor; k++)
				{
					this.entityPool[k].Import(s, r);
					if (this.entityPool[k].id != 0 && this.entityPool[k].beltId == 0 && this.entityPool[k].inserterId == 0 && this.entityPool[k].splitterId == 0 && this.entityPool[k].monitorId == 0 && this.entityPool[k].spraycoaterId == 0 && this.entityPool[k].pilerId == 0)
					{
						this.entityMutexs[k] = new Mutex(k);
					}
				}
				UnsafeIO.ReadMassive<AnimData>(s, this.entityAnimPool, this.entityCursor);
				for (int l = 1; l < this.entityCursor; l++)
				{
					if (this.entityPool[l].id != 0)
					{
						bool flag4 = false;
						this.entitySignPool[l].signType = (uint)r.ReadByte();
						this.entitySignPool[l].iconType = (uint)r.ReadByte();
						if (this.entitySignPool[l].iconType >= 128U)
						{
							flag4 = true;
							SignData[] array2 = this.entitySignPool;
							int num6 = l;
							array2[num6].iconType = array2[num6].iconType - 128U;
						}
						this.entitySignPool[l].iconId0 = (uint)r.ReadUInt16();
						if (flag4)
						{
							this.entitySignPool[l].count0 = r.ReadSingle();
						}
						UnsafeIO.Read<SignData>(s, ref this.entitySignPool[l], 40, 16);
					}
				}
				for (int m = 16; m < this.entityCursor * 16; m++)
				{
					if (this.entityPool[m / 16].id != 0)
					{
						if (r.ReadByte() == 0)
						{
							this.entityConnPool[m] = 0;
						}
						else
						{
							this.entityConnPool[m] = r.ReadInt32();
						}
					}
				}
			}
			for (int n = 0; n < this.entityRecycleCursor; n++)
			{
				this.entityRecycle[n] = r.ReadInt32();
			}
			num2 = r.ReadInt32();
			this.SetPrebuildCapacity(num2);
			this.prebuildCursor = r.ReadInt32();
			this.prebuildRecycleCursor = r.ReadInt32();
			for (int num7 = 1; num7 < this.prebuildCursor; num7++)
			{
				this.prebuildPool[num7].Import(r);
				if (this.prebuildPool[num7].id != 0)
				{
					int num8 = num7 * 16;
					int num9 = num8 + 16;
					for (int num10 = num8; num10 < num9; num10++)
					{
						if (r.ReadByte() == 0)
						{
							this.prebuildConnPool[num10] = 0;
						}
						else
						{
							this.prebuildConnPool[num10] = r.ReadInt32();
						}
					}
				}
			}
			for (int num11 = 0; num11 < this.prebuildRecycleCursor; num11++)
			{
				this.prebuildRecycle[num11] = r.ReadInt32();
			}
		}
		else
		{
			num2 = r.ReadInt32();
			this.SetEntityCapacity(num2);
			this.entityCursor = r.ReadInt32();
			this.entityRecycleCursor = r.ReadInt32();
			for (int num12 = 1; num12 < this.entityCursor; num12++)
			{
				this.entityPool[num12].Import(s, r);
				if (this.entityPool[num12].id != 0 && this.entityPool[num12].beltId == 0 && this.entityPool[num12].inserterId == 0 && this.entityPool[num12].splitterId == 0 && this.entityPool[num12].monitorId == 0 && this.entityPool[num12].spraycoaterId == 0 && this.entityPool[num12].pilerId == 0)
				{
					this.entityMutexs[num12] = new Mutex(num12);
				}
			}
			for (int num13 = 1; num13 < this.entityCursor; num13++)
			{
				this.entityAnimPool[num13].time = r.ReadSingle();
				this.entityAnimPool[num13].prepare_length = r.ReadSingle();
				this.entityAnimPool[num13].working_length = r.ReadSingle();
				this.entityAnimPool[num13].state = r.ReadUInt32();
				this.entityAnimPool[num13].power = r.ReadSingle();
			}
			if (flag)
			{
				for (int num14 = 1; num14 < this.entityCursor; num14++)
				{
					this.entitySignPool[num14].signType = (uint)r.ReadByte();
					this.entitySignPool[num14].iconType = (uint)r.ReadByte();
					this.entitySignPool[num14].iconId0 = (uint)r.ReadUInt16();
					this.entitySignPool[num14].x = r.ReadSingle();
					this.entitySignPool[num14].y = r.ReadSingle();
					this.entitySignPool[num14].z = r.ReadSingle();
					this.entitySignPool[num14].w = r.ReadSingle();
				}
			}
			else
			{
				for (int num15 = 1; num15 < this.entityCursor; num15++)
				{
					this.entitySignPool[num15].signType = r.ReadUInt32();
					this.entitySignPool[num15].iconType = r.ReadUInt32();
					this.entitySignPool[num15].iconId0 = r.ReadUInt32();
					this.entitySignPool[num15].iconId1 = r.ReadUInt32();
					this.entitySignPool[num15].iconId2 = r.ReadUInt32();
					this.entitySignPool[num15].iconId3 = r.ReadUInt32();
					this.entitySignPool[num15].count0 = r.ReadSingle();
					this.entitySignPool[num15].count1 = r.ReadSingle();
					this.entitySignPool[num15].count2 = r.ReadSingle();
					this.entitySignPool[num15].count3 = r.ReadSingle();
					this.entitySignPool[num15].x = r.ReadSingle();
					this.entitySignPool[num15].y = r.ReadSingle();
					this.entitySignPool[num15].z = r.ReadSingle();
					this.entitySignPool[num15].w = r.ReadSingle();
				}
			}
			int num16 = this.entityCursor * 16;
			for (int num17 = 16; num17 < num16; num17++)
			{
				this.entityConnPool[num17] = r.ReadInt32();
			}
			for (int num18 = 0; num18 < this.entityRecycleCursor; num18++)
			{
				this.entityRecycle[num18] = r.ReadInt32();
			}
			num2 = r.ReadInt32();
			this.SetPrebuildCapacity(num2);
			this.prebuildCursor = r.ReadInt32();
			this.prebuildRecycleCursor = r.ReadInt32();
			for (int num19 = 1; num19 < this.prebuildCursor; num19++)
			{
				this.prebuildPool[num19].Import(r);
			}
			int num20 = this.prebuildCursor * 16;
			for (int num21 = 16; num21 < num20; num21++)
			{
				this.prebuildConnPool[num21] = r.ReadInt32();
			}
			for (int num22 = 0; num22 < this.prebuildRecycleCursor; num22++)
			{
				this.prebuildRecycle[num22] = r.ReadInt32();
			}
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Entity);
		if (num >= 8)
		{
			num2 = r.ReadInt32();
			this.SetCraftCapacity(num2);
			this.craftCursor = r.ReadInt32();
			this.craftRecycleCursor = r.ReadInt32();
			for (int num23 = 1; num23 < this.craftCursor; num23++)
			{
				this.craftPool[num23].Import(r);
			}
			for (int num24 = 0; num24 < this.craftRecycleCursor; num24++)
			{
				this.craftRecycle[num24] = r.ReadInt32();
			}
			for (int num25 = 1; num25 < this.craftCursor; num25++)
			{
				this.craftAnimPool[num25].time = r.ReadSingle();
				this.craftAnimPool[num25].prepare_length = r.ReadSingle();
				this.craftAnimPool[num25].working_length = r.ReadSingle();
				this.craftAnimPool[num25].state = r.ReadUInt32();
				this.craftAnimPool[num25].power = r.ReadSingle();
			}
			if (this.gameData.patch < 10)
			{
				for (int num26 = 1; num26 < this.craftCursor; num26++)
				{
					ref CraftData ptr = ref this.craftPool[num26];
					if (ptr.fleetId > 0 && ptr.owner > 0 && ptr.port == 1)
					{
						ptr.port = 0;
					}
				}
			}
		}
		else
		{
			this.SetCraftCapacity(64);
			this.craftCursor = 1;
			this.craftRecycleCursor = 0;
		}
		PerformanceMonitor.BeginData(ESaveDataEntry.Combat);
		if (num >= 8)
		{
			num2 = r.ReadInt32();
			this.SetEnemyCapacity(num2);
			this.enemyCursor = r.ReadInt32();
			this.enemyRecycleCursor = r.ReadInt32();
			for (int num27 = 1; num27 < this.enemyCursor; num27++)
			{
				this.enemyPool[num27].Import(r);
			}
			for (int num28 = 0; num28 < this.enemyRecycleCursor; num28++)
			{
				this.enemyRecycle[num28] = r.ReadInt32();
			}
			for (int num29 = 1; num29 < this.enemyCursor; num29++)
			{
				this.enemyAnimPool[num29].time = r.ReadSingle();
				this.enemyAnimPool[num29].prepare_length = r.ReadSingle();
				this.enemyAnimPool[num29].working_length = r.ReadSingle();
				this.enemyAnimPool[num29].state = r.ReadUInt32();
				this.enemyAnimPool[num29].power = r.ReadSingle();
			}
		}
		else
		{
			this.SetEnemyCapacity(16);
			this.enemyCursor = 1;
			this.enemyRecycleCursor = 0;
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Combat);
		PerformanceMonitor.EndData(ESaveDataEntry.Factory);
		PerformanceMonitor.BeginData(ESaveDataEntry.Planet);
		num2 = r.ReadInt32();
		this.SetVegeCapacity(num2);
		this.vegeCursor = r.ReadInt32();
		this.vegeRecycleCursor = r.ReadInt32();
		for (int num30 = 1; num30 < this.vegeCursor; num30++)
		{
			this.vegePool[num30].Import(r);
		}
		for (int num31 = 0; num31 < this.vegeRecycleCursor; num31++)
		{
			this.vegeRecycle[num31] = r.ReadInt32();
		}
		num2 = r.ReadInt32();
		this.SetVeinCapacity(num2);
		this.veinCursor = r.ReadInt32();
		this.veinRecycleCursor = r.ReadInt32();
		int num32 = 0;
		for (int num33 = 1; num33 < this.veinCursor; num33++)
		{
			this.veinPool[num33].Import(r);
			if (num < 7)
			{
				VeinData[] array3 = this.veinPool;
				int num34 = num33;
				array3[num34].groupIndex = array3[num34].groupIndex + 1;
			}
			if (this.veinPool[num33].groupIndex < 0)
			{
				this.veinPool[num33].groupIndex = 0;
			}
			if ((int)this.veinPool[num33].groupIndex > num32)
			{
				num32 = (int)this.veinPool[num33].groupIndex;
			}
		}
		for (int num35 = 0; num35 < this.veinRecycleCursor; num35++)
		{
			this.veinRecycle[num35] = r.ReadInt32();
		}
		for (int num36 = 1; num36 < this.veinCursor; num36++)
		{
			this.veinAnimPool[num36].time = r.ReadSingle();
			this.veinAnimPool[num36].prepare_length = r.ReadSingle();
			this.veinAnimPool[num36].working_length = r.ReadSingle();
			this.veinAnimPool[num36].state = r.ReadUInt32();
			this.veinAnimPool[num36].power = r.ReadSingle();
		}
		this.InitVeinGroups(num32);
		this.RecalculateAllVeinGroups();
		PerformanceMonitor.EndData(ESaveDataEntry.Planet);
		PerformanceMonitor.BeginData(ESaveDataEntry.Factory);
		if (num < 8)
		{
			this.RefreshHashSystems();
		}
		PerformanceMonitor.BeginData(ESaveDataEntry.Ruin);
		if (num >= 8)
		{
			num2 = r.ReadInt32();
			this.SetRuinCapacity(num2);
			this.ruinCursor = r.ReadInt32();
			this.ruinRecycleCursor = r.ReadInt32();
			for (int num37 = 1; num37 < this.ruinCursor; num37++)
			{
				this.ruinPool[num37].Import(r);
			}
			for (int num38 = 0; num38 < this.ruinRecycleCursor; num38++)
			{
				this.ruinRecycle[num38] = r.ReadInt32();
			}
		}
		else
		{
			this.SetRuinCapacity(16);
			this.ruinCursor = 1;
			this.ruinRecycleCursor = 0;
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Ruin);
		PerformanceMonitor.BeginData(ESaveDataEntry.BeltAndCargo);
		this.cargoContainer = new CargoContainer(true);
		this.cargoContainer.Import(r);
		this.cargoTraffic = new CargoTraffic();
		this.cargoTraffic.planet = this.planet;
		this.cargoTraffic.factory = this;
		this.cargoTraffic.container = this.cargoContainer;
		this.cargoTraffic.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.BeltAndCargo);
		this.blockContainer = new MiniBlockContainer();
		PerformanceMonitor.BeginData(ESaveDataEntry.Storage);
		this.factoryStorage = new FactoryStorage(this.planet, true);
		this.factoryStorage.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.Storage);
		PerformanceMonitor.BeginData(ESaveDataEntry.PowerSystem);
		this.powerSystem = new PowerSystem(this.planet, true);
		this.powerSystem.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.PowerSystem);
		PerformanceMonitor.BeginData(ESaveDataEntry.Facility);
		this.factorySystem = new FactorySystem(this.planet, true);
		this.factorySystem.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.Facility);
		PerformanceMonitor.BeginData(ESaveDataEntry.Combat);
		if (num >= 8)
		{
			this.enemySystem = new EnemyDFGroundSystem(this.planet, true);
			this.enemySystem.Import(r);
			this.combatGroundSystem = new CombatGroundSystem(this.planet, true);
			this.combatGroundSystem.Import(r);
			PerformanceMonitor.BeginData(ESaveDataEntry.Defense);
			this.defenseSystem = new DefenseSystem(this.planet, true);
			this.defenseSystem.Import(r);
			this.planetATField = new PlanetATField(this.planet, true);
			this.planetATField.Import(r);
			this.planetATField.UpdatePhysicsShape(true);
			PerformanceMonitor.EndData(ESaveDataEntry.Defense);
			PerformanceMonitor.BeginData(ESaveDataEntry.Construction);
			this.constructionSystem = new ConstructionSystem(this.planet, true);
			this.constructionSystem.Import(r);
			PerformanceMonitor.EndData(ESaveDataEntry.Construction);
			if (this.gameData.patch < 12)
			{
				DFGBaseComponent[] buffer = this.enemySystem.bases.buffer;
				int cursor = this.enemySystem.bases.cursor;
				for (int num39 = 1; num39 < cursor; num39++)
				{
					if (buffer[num39] != null && buffer[num39].id == num39 && buffer[num39].ruinId > 0)
					{
						if (this.ruinPool[buffer[num39].ruinId].id != buffer[num39].ruinId)
						{
							buffer[num39].ruinId = 0;
						}
						else
						{
							Vector3 normalized = this.ruinPool[buffer[num39].ruinId].pos.normalized;
							Vector3 a = this.enemyPool[buffer[num39].enemyId].pos.normalized;
							if ((normalized * 200f - a * 200f).magnitude > 10f)
							{
								buffer[num39].ruinId = 0;
							}
						}
					}
				}
				PowerGeneratorComponent[] genPool = this.powerSystem.genPool;
				int genCursor = this.powerSystem.genCursor;
				for (int num40 = 1; num40 < genCursor; num40++)
				{
					ref PowerGeneratorComponent ptr2 = ref genPool[num40];
					if (ptr2.id == num40 && ptr2.geothermal && genPool[num40].baseRuinId > 0)
					{
						if (this.ruinPool[genPool[num40].baseRuinId].id != genPool[num40].baseRuinId)
						{
							genPool[num40].baseRuinId = 0;
						}
						else
						{
							Vector3 normalized2 = this.ruinPool[genPool[num40].baseRuinId].pos.normalized;
							Vector3 normalized3 = this.entityPool[genPool[num40].entityId].pos.normalized;
							if ((normalized2 * 200f - normalized3 * 200f).magnitude > 10f)
							{
								genPool[num40].baseRuinId = 0;
							}
						}
					}
				}
			}
			int patch = this.gameData.patch;
			DataPool<UnitComponent> units = this.combatGroundSystem.units;
			for (int num41 = 1; num41 < units.cursor; num41++)
			{
				ref UnitComponent ptr3 = ref units.buffer[num41];
				if (ptr3.id == num41)
				{
					ref CraftData ptr4 = ref this.craftPool[ptr3.craftId];
					if (ptr4.id == ptr3.craftId)
					{
						if (ptr4.owner <= 0)
						{
							this.RemoveCraftWithComponents(ptr4.id);
						}
						else if (this.craftPool[ptr4.owner].id != ptr4.owner)
						{
							this.RemoveCraftWithComponents(ptr4.id);
						}
					}
				}
			}
			DataPool<FleetComponent> fleets = this.combatGroundSystem.fleets;
			for (int num42 = 1; num42 < fleets.cursor; num42++)
			{
				ref FleetComponent ptr5 = ref fleets.buffer[num42];
				if (ptr5.id == num42)
				{
					ref CraftData ptr6 = ref this.craftPool[ptr5.craftId];
					if (ptr6.id == ptr5.craftId && !ptr5.CheckOwnerExist(ref ptr6, this, this.gameData.mainPlayer.mecha))
					{
						this.RemoveCraftWithComponents(ptr5.craftId);
					}
				}
			}
			ObjectPool<CombatModuleComponent> combatModules = this.combatGroundSystem.combatModules;
			for (int num43 = 1; num43 < combatModules.cursor; num43++)
			{
				ref CombatModuleComponent ptr7 = ref combatModules.buffer[num43];
				if (ptr7 != null && ptr7.id == num43 && this.entityPool[ptr7.entityId].id == ptr7.entityId)
				{
					ModuleFleet[] moduleFleets = ptr7.moduleFleets;
					int fleetCount = ptr7.fleetCount;
					for (int num44 = 0; num44 < fleetCount; num44++)
					{
						int fleetId = moduleFleets[num44].fleetId;
						if (fleetId != 0)
						{
							fleets = this.combatGroundSystem.fleets;
							ref FleetComponent ptr8 = ref fleets.buffer[fleetId];
							if (ptr8.owner != ptr7.entityId)
							{
								moduleFleets[num44].ClearFleetForeignKey();
							}
							else
							{
								ref CraftData ptr9 = ref this.craftPool[ptr8.craftId];
								if (ptr9.id != ptr8.craftId || ptr9.owner != ptr8.owner)
								{
									moduleFleets[num44].ClearFleetForeignKey();
								}
								else
								{
									ModuleFighter[] fighters = moduleFleets[num44].fighters;
									int num45 = fighters.Length;
									for (int num46 = 0; num46 < num45; num46++)
									{
										int craftId = fighters[num46].craftId;
										if (craftId != 0)
										{
											ref CraftData ptr10 = ref this.craftPool[craftId];
											if (ptr10.id != craftId || ptr10.unitId == 0)
											{
												fighters[num46].ClearFighterForeignKey();
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
		else
		{
			this.enemySystem = new EnemyDFGroundSystem(this.planet);
			this.combatGroundSystem = new CombatGroundSystem(this.planet);
			this.defenseSystem = new DefenseSystem(this.planet);
			this.planetATField = new PlanetATField(this.planet);
			this.constructionSystem = new ConstructionSystem(this.planet);
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Combat);
		PerformanceMonitor.BeginData(ESaveDataEntry.Transport);
		this.transport = new PlanetTransport(this.gameData, this.planet, true);
		this.transport.Init();
		this.transport.Import(r);
		PerformanceMonitor.EndData(ESaveDataEntry.Transport);
		if (num < 4)
		{
			r.ReadInt32();
			r.ReadInt32();
			r.ReadInt32();
			r.ReadInt32();
		}
		PerformanceMonitor.BeginData(ESaveDataEntry.Platform);
		if (num >= 1)
		{
			this.platformSystem = new PlatformSystem(this.planet, true);
			this.platformSystem.Import(r);
		}
		else
		{
			this.platformSystem = new PlatformSystem(this.planet);
		}
		this.enemySystem.RefreshPlanetReformState();
		PerformanceMonitor.EndData(ESaveDataEntry.Platform);
		PerformanceMonitor.BeginData(ESaveDataEntry.Digital);
		if (num >= 6)
		{
			this.digitalSystem = new DigitalSystem(this.planet, true);
			this.digitalSystem.Import(r);
		}
		else
		{
			this.digitalSystem = new DigitalSystem(this.planet);
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Digital);
		ProductionStatistics production = this.gameData.statistics.production;
		if (production.factoryStatPool[this.index] == null)
		{
			production.CreateFactoryStat(this.index);
		}
		if (this.entityCount > 0 || this.prebuildCount > 0 || this.veinRecycleCursor > 0 || this.vegeRecycleCursor > 0 || this.planet.id == _gameData.galaxy.birthPlanetId)
		{
			this.landed = true;
		}
		PerformanceMonitor.EndData(ESaveDataEntry.Factory);
	}

	// Token: 0x060018F0 RID: 6384 RVA: 0x001ADC84 File Offset: 0x001ABE84
	public void RefreshHashSystems()
	{
		this.hashSystemDynamic.Clear();
		this.hashSystemStatic.Clear();
		for (int i = 1; i < this.entityCursor; i++)
		{
			if (this.entityPool[i].id == i)
			{
				this.entityPool[i].hashAddress = this.hashSystemStatic.AddObjectToBucket(i, this.entityPool[i].pos, EObjectType.None);
			}
		}
		for (int j = 1; j < this.prebuildCursor; j++)
		{
			if (this.prebuildPool[j].id == j)
			{
				this.prebuildPool[j].hashAddress = this.hashSystemStatic.AddObjectToBucket(j, this.prebuildPool[j].pos, EObjectType.Prebuild);
			}
		}
		for (int k = 1; k < this.craftCursor; k++)
		{
			if (this.craftPool[k].id == k)
			{
				if (this.craftPool[k].dynamic)
				{
					this.craftPool[k].hashAddress = this.hashSystemDynamic.AddObjectToBucket(k, this.craftPool[k].pos, EObjectType.Craft);
				}
				else
				{
					this.craftPool[k].hashAddress = this.hashSystemStatic.AddObjectToBucket(k, this.craftPool[k].pos, EObjectType.Craft);
				}
			}
		}
		for (int l = 1; l < this.enemyCursor; l++)
		{
			if (this.enemyPool[l].id == l)
			{
				if (this.enemyPool[l].dynamic)
				{
					this.enemyPool[l].hashAddress = this.hashSystemDynamic.AddObjectToBucket(l, this.enemyPool[l].pos, EObjectType.Enemy);
				}
				else
				{
					this.enemyPool[l].hashAddress = this.hashSystemStatic.AddObjectToBucket(l, this.enemyPool[l].pos, EObjectType.Enemy);
				}
			}
		}
		for (int m = 1; m < this.vegeCursor; m++)
		{
			if (this.vegePool[m].id == m)
			{
				this.vegePool[m].hashAddress = this.hashSystemStatic.AddObjectToBucket(m, this.vegePool[m].pos, EObjectType.Vegetable);
			}
		}
		for (int n = 1; n < this.veinCursor; n++)
		{
			if (this.veinPool[n].id == n)
			{
				this.veinPool[n].hashAddress = this.hashSystemStatic.AddObjectToBucket(n, this.veinPool[n].pos, EObjectType.Vein);
			}
		}
		for (int num = 1; num < this.ruinCursor; num++)
		{
			if (this.ruinPool[num].id == num)
			{
				this.ruinPool[num].hashAddress = this.hashSystemStatic.AddObjectToBucket(num, this.ruinPool[num].pos, EObjectType.Ruin);
			}
		}
	}

	// Token: 0x060018F1 RID: 6385 RVA: 0x001ADFB6 File Offset: 0x001AC1B6
	public void ConstructionBeforeGameTick()
	{
		if (this.constructionSystem != null)
		{
			this.constructionSystem.BeforeGameTick();
		}
	}

	// Token: 0x060018F2 RID: 6386 RVA: 0x001ADFCB File Offset: 0x001AC1CB
	public void BeforeGameTick()
	{
		this.CheckDysonSphereConditionAfterConstruction();
		this._miningFlag = 0;
		this._veinMiningFlag = 0;
	}

	// Token: 0x060018F3 RID: 6387 RVA: 0x001ADFE1 File Offset: 0x001AC1E1
	private void CheckDysonSphereConditionAfterConstruction()
	{
		if (this.factorySystem.ejectorCount > 0)
		{
			this.CheckOrCreateDysonSphere();
		}
		if (this.factorySystem.siloCount > 0)
		{
			this.CheckOrCreateDysonSphere();
		}
	}

	// Token: 0x060018F4 RID: 6388 RVA: 0x001AE010 File Offset: 0x001AC210
	public void LocalizeEntities()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 1; i < this.entityCursor; i++)
		{
			ref EntityData ptr = ref this.entityPool[i];
			if (ptr.id == i && !ptr.localized)
			{
				num++;
				if (num2 == 0)
				{
					this.BeginFlattenTerrain();
				}
				num2++;
				ItemProto itemProto = LDB.items.Select((int)ptr.protoId);
				bool flag = this.planet.type == EPlanetType.Gas;
				if (itemProto != null && !flag)
				{
					this.FlattenTerrain(ptr.pos, ptr.rot, new Bounds(itemProto.prefabDesc.buildCollider.pos, itemProto.prefabDesc.buildCollider.ext * 2f), 6f, 1f, false, false, false, false, default(Bounds));
				}
				ptr.localized = true;
			}
		}
		if (num2 > 0)
		{
			this.EndFlattenTerrain();
		}
	}

	// Token: 0x060018F5 RID: 6389 RVA: 0x001AE104 File Offset: 0x001AC304
	public void LocalizeEnemies()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 1; i < this.enemyCursor; i++)
		{
			ref EnemyData ptr = ref this.enemyPool[i];
			if (ptr.id == i && !ptr.localized)
			{
				num++;
				if (!ptr.dynamic)
				{
					if (num2 == 0)
					{
						this.BeginFlattenTerrain();
					}
					num2++;
					int protoId = (int)ptr.protoId;
					PrefabDesc prefabDesc = PlanetFactory.PrefabDescByModelIndex[(int)ptr.modelIndex];
					if (prefabDesc != null)
					{
						ColliderData buildCollider = prefabDesc.buildCollider;
						if (protoId == 8120)
						{
							this.FlattenTerrain(ptr.pos, ptr.rot, new Bounds(buildCollider.pos, buildCollider.ext * 2.5f), 6f, 1f, false, true, false, false, default(Bounds));
							this.BuriedVeins(ptr.pos, 40f, 60f);
						}
						else
						{
							this.FlattenTerrain(ptr.pos, ptr.rot, new Bounds(buildCollider.pos, buildCollider.ext * 0.8f), 5f, 1f, false, true, false, false, default(Bounds));
						}
					}
				}
				ptr.localized = true;
			}
		}
		if (num2 > 0)
		{
			this.EndFlattenTerrain();
		}
	}

	// Token: 0x060018F6 RID: 6390 RVA: 0x001AE264 File Offset: 0x001AC464
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

	// Token: 0x060018F7 RID: 6391 RVA: 0x001AE2A8 File Offset: 0x001AC4A8
	public bool HasAnyEntityInArea(Vector3 center, float areaRadius, bool instant, ref int cursor)
	{
		center = center.normalized * this.planet.realRadius;
		if (this.planet.factoryLoaded)
		{
			return this.planet.physics.nearColliderLogic.HasAnyEntityInArea(center, areaRadius);
		}
		return this.HasAnyEntityInAreaOffline(center, areaRadius, instant, ref cursor);
	}

	// Token: 0x060018F8 RID: 6392 RVA: 0x001AE300 File Offset: 0x001AC500
	private bool HasAnyEntityInAreaOffline(Vector3 center, float areaRadius, bool instant, ref int cursor)
	{
		int area = (int)(areaRadius / 40f) + 1;
		uint mask = SimpleHash.GenerateHashMask(center.x, center.y, center.z, area);
		int num;
		if (instant)
		{
			cursor = 1;
			num = this.entityCursor;
		}
		else
		{
			num = cursor + this.AquireLoopLength();
		}
		if (cursor < 1)
		{
			cursor = 1;
		}
		if (num > this.entityCursor)
		{
			num = this.entityCursor;
		}
		for (int i = cursor; i < num; i++)
		{
			if (this.entityPool[i].id == i && this.entityPool[i].simpleHash.MaskPass(mask) && (this.entityPool[i].pos - center).sqrMagnitude <= areaRadius * areaRadius)
			{
				return true;
			}
		}
		cursor = num;
		return false;
	}

	// Token: 0x060018F9 RID: 6393 RVA: 0x001AE3D0 File Offset: 0x001AC5D0
	public int GetEntitiesInArea(Vector3 center, float areaRadius, ref int[] ids, ref int cursor, bool ignoreAltitude)
	{
		center = center.normalized * this.planet.realRadius;
		if (this.planet.factoryLoaded)
		{
			cursor = this.entityCursor;
			return this.planet.physics.nearColliderLogic.GetEntitiesInAreaNonAlloc(center, areaRadius, ref ids);
		}
		return this.GetEntitiesInAreaOffline(center, areaRadius, ref ids, ref cursor, ignoreAltitude);
	}

	// Token: 0x060018FA RID: 6394 RVA: 0x001AE434 File Offset: 0x001AC634
	private int GetEntitiesInAreaOffline(Vector3 center, float areaRadius, ref int[] ids, ref int cursor, bool ignoreAltitude)
	{
		if (ids == null)
		{
			return 0;
		}
		int area = (int)(areaRadius / 40f) + 1;
		uint mask = SimpleHash.GenerateHashMask(center.x, center.y, center.z, area);
		if (cursor < 1)
		{
			cursor = 1;
		}
		int num = cursor + this.AquireLoopLength();
		if (num > this.entityCursor)
		{
			num = this.entityCursor;
		}
		int num2 = 0;
		Array.Clear(ids, 0, ids.Length);
		for (int i = cursor; i < num; i++)
		{
			if (this.entityPool[i].id == i && this.entityPool[i].simpleHash.MaskPass(mask))
			{
				if (ignoreAltitude)
				{
					if ((this.entityPool[i].pos.normalized * center.magnitude - center).sqrMagnitude > areaRadius * areaRadius)
					{
						goto IL_117;
					}
				}
				else if ((this.entityPool[i].pos - center).sqrMagnitude > areaRadius * areaRadius)
				{
					goto IL_117;
				}
				ids[num2++] = i;
				if (num2 >= ids.Length - 1)
				{
					this.ExpandArrayCapacity(ref ids);
				}
			}
			IL_117:;
		}
		cursor = num;
		return num2;
	}

	// Token: 0x060018FB RID: 6395 RVA: 0x001AE56C File Offset: 0x001AC76C
	public void ExpandArrayCapacity(ref int[] _arr)
	{
		int num = (_arr == null) ? 1024 : _arr.Length;
		int num2 = num * 2;
		int[] array = _arr;
		_arr = new int[num2];
		if (array != null)
		{
			Array.Copy(array, _arr, (num2 > num) ? num : num2);
		}
	}

	// Token: 0x060018FC RID: 6396 RVA: 0x001AE5AB File Offset: 0x001AC7AB
	public int AquireLoopLength()
	{
		return 1024;
	}

	// Token: 0x060018FD RID: 6397 RVA: 0x001AE5B4 File Offset: 0x001AC7B4
	public int InsertInto(int entityId, int offset, int itemId, byte itemCount, byte itemInc, out byte remainInc)
	{
		remainInc = itemInc;
		int beltId = this.entityPool[entityId].beltId;
		if (beltId <= 0)
		{
			int[] array = this.entityNeeds[entityId];
			int assemblerId = this.entityPool[entityId].assemblerId;
			if (assemblerId > 0)
			{
				ref AssemblerComponent ptr = ref this.factorySystem.assemblerPool[assemblerId];
				if (array == null || ptr.recipeExecuteData == null)
				{
					return 0;
				}
				Mutex obj = this.entityMutexs[entityId];
				lock (obj)
				{
					int[] requires = ptr.recipeExecuteData.requires;
					int num = requires.Length;
					if (0 < num && requires[0] == itemId)
					{
						ptr.served[0] += (int)itemCount;
						ptr.incServed[0] += (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
					if (1 < num && requires[1] == itemId)
					{
						ptr.served[1] += (int)itemCount;
						ptr.incServed[1] += (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
					if (2 < num && requires[2] == itemId)
					{
						ptr.served[2] += (int)itemCount;
						ptr.incServed[2] += (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
					if (3 < num && requires[3] == itemId)
					{
						ptr.served[3] += (int)itemCount;
						ptr.incServed[3] += (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
					if (4 < num && requires[4] == itemId)
					{
						ptr.served[4] += (int)itemCount;
						ptr.incServed[4] += (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
					if (5 < num && requires[5] == itemId)
					{
						ptr.served[5] += (int)itemCount;
						ptr.incServed[5] += (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
				}
				return 0;
			}
			else
			{
				int ejectorId = this.entityPool[entityId].ejectorId;
				if (ejectorId > 0)
				{
					if (array == null)
					{
						return 0;
					}
					Mutex obj = this.entityMutexs[entityId];
					lock (obj)
					{
						if (array[0] == itemId && this.factorySystem.ejectorPool[ejectorId].bulletId == itemId)
						{
							EjectorComponent[] ejectorPool = this.factorySystem.ejectorPool;
							int num2 = ejectorId;
							ejectorPool[num2].bulletCount = ejectorPool[num2].bulletCount + (int)itemCount;
							EjectorComponent[] ejectorPool2 = this.factorySystem.ejectorPool;
							int num3 = ejectorId;
							ejectorPool2[num3].bulletInc = ejectorPool2[num3].bulletInc + (int)itemInc;
							remainInc = 0;
							return (int)itemCount;
						}
					}
					return 0;
				}
				else
				{
					int siloId = this.entityPool[entityId].siloId;
					if (siloId > 0)
					{
						if (array == null)
						{
							return 0;
						}
						Mutex obj = this.entityMutexs[entityId];
						lock (obj)
						{
							if (array[0] == itemId && this.factorySystem.siloPool[siloId].bulletId == itemId)
							{
								SiloComponent[] siloPool = this.factorySystem.siloPool;
								int num4 = siloId;
								siloPool[num4].bulletCount = siloPool[num4].bulletCount + (int)itemCount;
								SiloComponent[] siloPool2 = this.factorySystem.siloPool;
								int num5 = siloId;
								siloPool2[num5].bulletInc = siloPool2[num5].bulletInc + (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
						}
						return 0;
					}
					else
					{
						int labId = this.entityPool[entityId].labId;
						if (labId > 0)
						{
							if (array == null)
							{
								return 0;
							}
							Mutex obj = this.entityMutexs[entityId];
							lock (obj)
							{
								ref LabComponent ptr2 = ref this.factorySystem.labPool[labId];
								if (ptr2.researchMode)
								{
									int[] matrixServed = ptr2.matrixServed;
									int[] matrixIncServed = ptr2.matrixIncServed;
									if (matrixServed == null)
									{
										return 0;
									}
									int num6 = itemId - 6001;
									if (num6 >= 0 && num6 < 6)
									{
										matrixServed[num6] += 3600 * (int)itemCount;
										matrixIncServed[num6] += 3600 * (int)itemInc;
										remainInc = 0;
										return (int)itemCount;
									}
								}
								else
								{
									RecipeExecuteData recipeExecuteData = ptr2.recipeExecuteData;
									int[] array2 = (recipeExecuteData != null) ? recipeExecuteData.requires : null;
									int[] served = ptr2.served;
									int[] incServed = ptr2.incServed;
									if (array2 == null)
									{
										return 0;
									}
									int num7 = array2.Length;
									for (int i = 0; i < num7; i++)
									{
										if (array2[i] == itemId)
										{
											served[i] += (int)itemCount;
											incServed[i] += (int)itemInc;
											remainInc = 0;
											return (int)itemCount;
										}
									}
								}
							}
							return 0;
						}
						else
						{
							int storageId = this.entityPool[entityId].storageId;
							if (storageId > 0)
							{
								StorageComponent storageComponent = this.factoryStorage.storagePool[storageId];
								while (storageComponent != null)
								{
									Mutex obj = this.entityMutexs[storageComponent.entityId];
									lock (obj)
									{
										if (storageComponent.lastFullItem != itemId)
										{
											int num9;
											int num8;
											if (this.entityPool[storageComponent.entityId].battleBaseId == 0)
											{
												num8 = storageComponent.AddItem(itemId, (int)itemCount, (int)itemInc, out num9, true);
											}
											else
											{
												num8 = storageComponent.AddItemFilteredBanOnly(itemId, (int)itemCount, (int)itemInc, out num9);
											}
											remainInc = (byte)num9;
											if (num8 == (int)itemCount)
											{
												storageComponent.lastFullItem = -1;
											}
											else
											{
												storageComponent.lastFullItem = itemId;
											}
											if (num8 != 0 || storageComponent.nextStorage == null)
											{
												return num8;
											}
										}
										storageComponent = storageComponent.nextStorage;
									}
								}
								return 0;
							}
							int stationId = this.entityPool[entityId].stationId;
							if (stationId > 0)
							{
								if (array == null)
								{
									return 0;
								}
								StationComponent stationComponent = this.transport.stationPool[stationId];
								Mutex obj = this.entityMutexs[entityId];
								lock (obj)
								{
									if (itemId == 1210 && stationComponent.warperCount < stationComponent.warperMaxCount)
									{
										stationComponent.warperCount += (int)itemCount;
										remainInc = 0;
										return (int)itemCount;
									}
									StationStore[] storage = stationComponent.storage;
									int num10 = 0;
									while (num10 < array.Length && num10 < storage.Length)
									{
										if (array[num10] == itemId && storage[num10].itemId == itemId)
										{
											StationStore[] array3 = storage;
											int num11 = num10;
											array3[num11].count = array3[num11].count + (int)itemCount;
											StationStore[] array4 = storage;
											int num12 = num10;
											array4[num12].inc = array4[num12].inc + (int)itemInc;
											remainInc = 0;
											return (int)itemCount;
										}
										num10++;
									}
								}
								return 0;
							}
							else
							{
								int powerGenId = this.entityPool[entityId].powerGenId;
								if (powerGenId > 0)
								{
									PowerGeneratorComponent[] genPool = this.powerSystem.genPool;
									Mutex obj = this.entityMutexs[entityId];
									lock (obj)
									{
										if (itemId == (int)genPool[powerGenId].fuelId)
										{
											if (genPool[powerGenId].fuelCount < 10)
											{
												PowerGeneratorComponent[] array5 = genPool;
												int num13 = powerGenId;
												array5[num13].fuelCount = array5[num13].fuelCount + (short)itemCount;
												PowerGeneratorComponent[] array6 = genPool;
												int num14 = powerGenId;
												array6[num14].fuelInc = array6[num14].fuelInc + (short)itemInc;
												remainInc = 0;
												return (int)itemCount;
											}
											return 0;
										}
										else if (genPool[powerGenId].fuelId == 0)
										{
											array = ItemProto.fuelNeeds[(int)genPool[powerGenId].fuelMask];
											if (array == null || array.Length == 0)
											{
												return 0;
											}
											for (int j = 0; j < array.Length; j++)
											{
												if (array[j] == itemId)
												{
													genPool[powerGenId].SetNewFuel(itemId, (short)itemCount, (short)itemInc);
													remainInc = 0;
													return (int)itemCount;
												}
											}
											return 0;
										}
									}
									return 0;
								}
								int splitterId = this.entityPool[entityId].splitterId;
								if (splitterId > 0)
								{
									if (offset == 0)
									{
										if (this.cargoTraffic.TryInsertItem(this.cargoTraffic.splitterPool[splitterId].beltA, 0, itemId, itemCount, itemInc))
										{
											remainInc = 0;
											return (int)itemCount;
										}
									}
									else if (offset == 1)
									{
										if (this.cargoTraffic.TryInsertItem(this.cargoTraffic.splitterPool[splitterId].beltB, 0, itemId, itemCount, itemInc))
										{
											remainInc = 0;
											return (int)itemCount;
										}
									}
									else if (offset == 2)
									{
										if (this.cargoTraffic.TryInsertItem(this.cargoTraffic.splitterPool[splitterId].beltC, 0, itemId, itemCount, itemInc))
										{
											remainInc = 0;
											return (int)itemCount;
										}
									}
									else if (offset == 3 && this.cargoTraffic.TryInsertItem(this.cargoTraffic.splitterPool[splitterId].beltD, 0, itemId, itemCount, itemInc))
									{
										remainInc = 0;
										return (int)itemCount;
									}
									return 0;
								}
								return 0;
							}
						}
					}
				}
			}
			int result;
			return result;
		}
		if (this.cargoTraffic.TryInsertItem(beltId, offset, itemId, itemCount, itemInc))
		{
			remainInc = 0;
			return (int)itemCount;
		}
		return 0;
	}

	// Token: 0x060018FE RID: 6398 RVA: 0x001AEF40 File Offset: 0x001AD140
	public int InsertInto(uint ioTargetTypedId, int offset, int itemId, byte itemCount, byte itemInc, out byte remainInc)
	{
		remainInc = itemInc;
		int num = (int)(ioTargetTypedId & 16777215U);
		EFactoryIOTargetType efactoryIOTargetType = (EFactoryIOTargetType)(ioTargetTypedId & 4278190080U);
		if (efactoryIOTargetType <= EFactoryIOTargetType.Silo)
		{
			if (efactoryIOTargetType <= EFactoryIOTargetType.Assembler)
			{
				if (efactoryIOTargetType != EFactoryIOTargetType.Belt)
				{
					if (efactoryIOTargetType == EFactoryIOTargetType.Assembler)
					{
						ref AssemblerComponent ptr = ref this.factorySystem.assemblerPool[num];
						if (ptr.id <= 0)
						{
							return 0;
						}
						int entityId = ptr.entityId;
						RecipeExecuteData recipeExecuteData = ptr.recipeExecuteData;
						int[] array = (recipeExecuteData != null) ? recipeExecuteData.requires : null;
						if (array == null)
						{
							return 0;
						}
						int[] array2 = this.entityNeeds[entityId];
						Mutex obj = this.entityMutexs[entityId];
						lock (obj)
						{
							int num2 = array.Length;
							if (0 < num2 && array[0] == itemId)
							{
								ptr.served[0] += (int)itemCount;
								ptr.incServed[0] += (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
							if (1 < num2 && array[1] == itemId)
							{
								ptr.served[1] += (int)itemCount;
								ptr.incServed[1] += (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
							if (2 < num2 && array[2] == itemId)
							{
								ptr.served[2] += (int)itemCount;
								ptr.incServed[2] += (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
							if (3 < num2 && array[3] == itemId)
							{
								ptr.served[3] += (int)itemCount;
								ptr.incServed[3] += (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
							if (4 < num2 && array[4] == itemId)
							{
								ptr.served[4] += (int)itemCount;
								ptr.incServed[4] += (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
							if (5 < num2 && array[5] == itemId)
							{
								ptr.served[5] += (int)itemCount;
								ptr.incServed[5] += (int)itemInc;
								remainInc = 0;
								return (int)itemCount;
							}
						}
						return 0;
					}
				}
				else
				{
					if (this.cargoTraffic.beltPool[num].id <= 0)
					{
						return 0;
					}
					if (this.cargoTraffic.TryInsertItem(num, offset, itemId, itemCount, itemInc))
					{
						remainInc = 0;
						return (int)itemCount;
					}
					return 0;
				}
			}
			else if (efactoryIOTargetType != EFactoryIOTargetType.Ejector)
			{
				if (efactoryIOTargetType == EFactoryIOTargetType.Silo)
				{
					ref SiloComponent ptr2 = ref this.factorySystem.siloPool[num];
					if (ptr2.id <= 0)
					{
						return 0;
					}
					int entityId2 = ptr2.entityId;
					int[] array3 = this.entityNeeds[entityId2];
					if (array3 == null)
					{
						return 0;
					}
					Mutex obj = this.entityMutexs[entityId2];
					lock (obj)
					{
						if (array3[0] == itemId && this.factorySystem.siloPool[num].bulletId == itemId)
						{
							SiloComponent[] siloPool = this.factorySystem.siloPool;
							int num3 = num;
							siloPool[num3].bulletCount = siloPool[num3].bulletCount + (int)itemCount;
							SiloComponent[] siloPool2 = this.factorySystem.siloPool;
							int num4 = num;
							siloPool2[num4].bulletInc = siloPool2[num4].bulletInc + (int)itemInc;
							remainInc = 0;
							return (int)itemCount;
						}
					}
					return 0;
				}
			}
			else
			{
				ref EjectorComponent ptr3 = ref this.factorySystem.ejectorPool[num];
				if (ptr3.id <= 0)
				{
					return 0;
				}
				int entityId3 = ptr3.entityId;
				int[] array4 = this.entityNeeds[entityId3];
				if (array4 == null)
				{
					return 0;
				}
				Mutex obj = this.entityMutexs[entityId3];
				lock (obj)
				{
					if (array4[0] == itemId && this.factorySystem.ejectorPool[num].bulletId == itemId)
					{
						EjectorComponent[] ejectorPool = this.factorySystem.ejectorPool;
						int num5 = num;
						ejectorPool[num5].bulletCount = ejectorPool[num5].bulletCount + (int)itemCount;
						EjectorComponent[] ejectorPool2 = this.factorySystem.ejectorPool;
						int num6 = num;
						ejectorPool2[num6].bulletInc = ejectorPool2[num6].bulletInc + (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
				}
				return 0;
			}
		}
		else if (efactoryIOTargetType <= EFactoryIOTargetType.Station)
		{
			if (efactoryIOTargetType == EFactoryIOTargetType.Storage)
			{
				StorageComponent storageComponent = this.factoryStorage.storagePool[num];
				while (storageComponent != null)
				{
					Mutex obj = this.entityMutexs[storageComponent.entityId];
					lock (obj)
					{
						if (storageComponent.lastFullItem != itemId)
						{
							int num8;
							int num7;
							if (this.entityPool[storageComponent.entityId].battleBaseId == 0)
							{
								num7 = storageComponent.AddItem(itemId, (int)itemCount, (int)itemInc, out num8, true);
							}
							else
							{
								num7 = storageComponent.AddItemFilteredBanOnly(itemId, (int)itemCount, (int)itemInc, out num8);
							}
							remainInc = (byte)num8;
							if (num7 == (int)itemCount)
							{
								storageComponent.lastFullItem = -1;
							}
							else
							{
								storageComponent.lastFullItem = itemId;
							}
							if (num7 != 0 || storageComponent.nextStorage == null)
							{
								return num7;
							}
						}
						storageComponent = storageComponent.nextStorage;
					}
				}
				return 0;
			}
			if (efactoryIOTargetType == EFactoryIOTargetType.Station)
			{
				StationComponent stationComponent = this.transport.stationPool[num];
				if (stationComponent.id <= 0)
				{
					return 0;
				}
				int entityId4 = stationComponent.entityId;
				int[] array5 = this.entityNeeds[entityId4];
				Mutex obj = this.entityMutexs[entityId4];
				lock (obj)
				{
					if (itemId == 1210 && stationComponent.warperCount < stationComponent.warperMaxCount)
					{
						stationComponent.warperCount += (int)itemCount;
						remainInc = 0;
						return (int)itemCount;
					}
					StationStore[] storage = stationComponent.storage;
					int num9 = 0;
					while (num9 < array5.Length && num9 < storage.Length)
					{
						if (array5[num9] == itemId && storage[num9].itemId == itemId)
						{
							StationStore[] array6 = storage;
							int num10 = num9;
							array6[num10].count = array6[num10].count + (int)itemCount;
							StationStore[] array7 = storage;
							int num11 = num9;
							array7[num11].inc = array7[num11].inc + (int)itemInc;
							remainInc = 0;
							return (int)itemCount;
						}
						num9++;
					}
				}
				return 0;
			}
		}
		else if (efactoryIOTargetType != EFactoryIOTargetType.Lab)
		{
			if (efactoryIOTargetType == EFactoryIOTargetType.PowerGen)
			{
				ref PowerGeneratorComponent ptr4 = ref this.powerSystem.genPool[num];
				if (ptr4.id <= 0)
				{
					return 0;
				}
				int entityId5 = ptr4.entityId;
				int[] array8 = this.entityNeeds[entityId5];
				Mutex obj = this.entityMutexs[entityId5];
				lock (obj)
				{
					if (itemId == (int)ptr4.fuelId)
					{
						if (ptr4.fuelCount < 10)
						{
							ref PowerGeneratorComponent ptr5 = ref ptr4;
							ptr5.fuelCount += (short)itemCount;
							ref PowerGeneratorComponent ptr6 = ref ptr4;
							ptr6.fuelInc += (short)itemInc;
							remainInc = 0;
							return (int)itemCount;
						}
						return 0;
					}
					else if (ptr4.fuelId == 0)
					{
						array8 = ItemProto.fuelNeeds[(int)ptr4.fuelMask];
						if (array8 == null || array8.Length == 0)
						{
							return 0;
						}
						for (int i = 0; i < array8.Length; i++)
						{
							if (array8[i] == itemId)
							{
								ptr4.SetNewFuel(itemId, (short)itemCount, (short)itemInc);
								remainInc = 0;
								return (int)itemCount;
							}
						}
						return 0;
					}
				}
				return 0;
			}
		}
		else
		{
			ref LabComponent ptr7 = ref this.factorySystem.labPool[num];
			if (ptr7.id <= 0)
			{
				return 0;
			}
			int entityId6 = ptr7.entityId;
			if (this.entityNeeds[entityId6] == null)
			{
				return 0;
			}
			Mutex obj = this.entityMutexs[entityId6];
			lock (obj)
			{
				if (ptr7.researchMode)
				{
					int[] matrixServed = ptr7.matrixServed;
					int[] matrixIncServed = ptr7.matrixIncServed;
					if (matrixServed == null)
					{
						return 0;
					}
					int num12 = itemId - 6001;
					if (num12 >= 0 && num12 < 6)
					{
						matrixServed[num12] += 3600 * (int)itemCount;
						matrixIncServed[num12] += 3600 * (int)itemInc;
						remainInc = 0;
						return (int)itemCount;
					}
				}
				else
				{
					RecipeExecuteData recipeExecuteData2 = ptr7.recipeExecuteData;
					int[] array9 = (recipeExecuteData2 != null) ? recipeExecuteData2.requires : null;
					int[] served = ptr7.served;
					int[] incServed = ptr7.incServed;
					if (array9 == null)
					{
						return 0;
					}
					int num13 = array9.Length;
					for (int j = 0; j < num13; j++)
					{
						if (array9[j] == itemId)
						{
							served[j] += (int)itemCount;
							incServed[j] += (int)itemInc;
							remainInc = 0;
							return (int)itemCount;
						}
					}
				}
			}
			return 0;
		}
		return 0;
	}

	// Token: 0x060018FF RID: 6399 RVA: 0x001AF848 File Offset: 0x001ADA48
	public int InsertIntoStorage(int entityId, int itemId, int count, int inc, out int remainInc, bool useBan = true)
	{
		remainInc = inc;
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			StorageComponent storageComponent = this.factoryStorage.storagePool[storageId];
			int num = 0;
			while (storageComponent != null)
			{
				Mutex obj = this.entityMutexs[storageComponent.entityId];
				lock (obj)
				{
					if (!useBan || storageComponent.lastFullItem != itemId)
					{
						int num2;
						if (this.entityPool[storageComponent.entityId].battleBaseId > 0)
						{
							num2 = storageComponent.AddItemFilteredBanOnly(itemId, count, inc, out remainInc);
						}
						else if (useBan)
						{
							num2 = storageComponent.AddItem(itemId, count, inc, out remainInc, true);
						}
						else
						{
							num2 = storageComponent.AddItemBanGridFirst(itemId, count, inc, out remainInc);
						}
						inc = remainInc;
						num += num2;
						if (num2 == count)
						{
							storageComponent.lastFullItem = -1;
							return num;
						}
						storageComponent.lastFullItem = itemId;
						count -= num2;
						if (storageComponent.nextStorage == null)
						{
							return num;
						}
					}
					storageComponent = storageComponent.nextStorage;
				}
			}
			return num;
		}
		return 0;
	}

	// Token: 0x06001900 RID: 6400 RVA: 0x001AF968 File Offset: 0x001ADB68
	public bool InsertCargoIntoStorage(int entityId, int cargoId, bool useBan = true)
	{
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			StorageComponent storageComponent = this.factoryStorage.storagePool[storageId];
			this.cargoContainer.cargoContainer_sl.Enter();
			ref Cargo ptr = ref this.cargoContainer.cargoPool[cargoId];
			while (storageComponent != null)
			{
				Mutex obj = this.entityMutexs[storageComponent.entityId];
				lock (obj)
				{
					if (!useBan || storageComponent.lastFullItem != (int)ptr.item)
					{
						if (storageComponent.AddCargo(ref ptr, useBan))
						{
							this.cargoContainer.cargoContainer_sl.Exit();
							return true;
						}
						if (storageComponent.nextStorage == null)
						{
							this.cargoContainer.cargoContainer_sl.Exit();
							return false;
						}
					}
					storageComponent = storageComponent.nextStorage;
				}
			}
			this.cargoContainer.cargoContainer_sl.Exit();
		}
		return false;
	}

	// Token: 0x06001901 RID: 6401 RVA: 0x001AFA64 File Offset: 0x001ADC64
	public int PickFuelForPowerGenFrom(uint pickTargetTypedId, int insertPowerGenId, int offset, int filter, out byte stack, out byte inc, out bool fuelFull)
	{
		stack = 1;
		inc = 0;
		fuelFull = false;
		if (insertPowerGenId == 0 || this.powerSystem.genPool[insertPowerGenId].id != insertPowerGenId)
		{
			return 0;
		}
		int fuelId = (int)this.powerSystem.genPool[insertPowerGenId].fuelId;
		int fuelMask = (int)this.powerSystem.genPool[insertPowerGenId].fuelMask;
		if (fuelId > 0 && filter > 0 && fuelId != filter)
		{
			return 0;
		}
		if ((pickTargetTypedId & 4278190080U) != 16777216U)
		{
			if ((pickTargetTypedId & 4278190080U) == 33554432U)
			{
				int num = (int)(pickTargetTypedId & 16777215U);
				ref AssemblerComponent ptr = ref this.factorySystem.assemblerPool[num];
				RecipeExecuteData recipeExecuteData = ptr.recipeExecuteData;
				int[] array = (recipeExecuteData != null) ? recipeExecuteData.products : null;
				int[] produced = ptr.produced;
				if (array == null)
				{
					return 0;
				}
				Mutex obj = this.entityMutexs[ptr.entityId];
				lock (obj)
				{
					if (fuelId > 0)
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (produced[i] > 0 && array[i] > 0 && array[i] == fuelId)
							{
								produced[i]--;
								return array[i];
							}
						}
					}
					else
					{
						int[] array2 = ItemProto.fuelNeeds[fuelMask];
						for (int j = 0; j < array.Length; j++)
						{
							if (produced[j] > 0 && array[j] > 0 && (filter == 0 || filter == array[j]))
							{
								for (int k = 0; k < array2.Length; k++)
								{
									if (array2[k] == array[j])
									{
										produced[j]--;
										return array[j];
									}
								}
							}
						}
					}
				}
				return 0;
			}
			else
			{
				if ((pickTargetTypedId & 4278190080U) == 83886080U)
				{
					int num2 = (int)(pickTargetTypedId & 16777215U);
					StorageComponent storageComponent = this.factoryStorage.storagePool[num2];
					StorageComponent storageComponent2 = storageComponent;
					if (storageComponent != null)
					{
						storageComponent = storageComponent.topStorage;
						while (storageComponent != null)
						{
							Mutex obj = this.entityMutexs[storageComponent.entityId];
							lock (obj)
							{
								if (storageComponent.lastEmptyItem != 0 && storageComponent.lastEmptyItem != filter)
								{
									int result = (fuelId > 0) ? fuelId : filter;
									int num3 = 1;
									bool flag2;
									if (fuelId > 0)
									{
										int num4;
										storageComponent.TakeTailItems(ref result, ref num3, out num4, this.entityPool[storageComponent.entityId].battleBaseId > 0);
										inc = (byte)num4;
										flag2 = (num3 == 1);
									}
									else
									{
										int num4;
										bool flag3 = storageComponent.TakeTailFuel(ref result, ref num3, fuelMask, out num4, this.entityPool[storageComponent.entityId].battleBaseId > 0);
										inc = (byte)num4;
										flag2 = (num3 == 1 || flag3);
									}
									if (num3 == 1)
									{
										storageComponent.lastEmptyItem = -1;
										return result;
									}
									if (!flag2)
									{
										storageComponent.lastEmptyItem = filter;
									}
								}
								if (storageComponent == storageComponent2)
								{
									break;
								}
								storageComponent = storageComponent.previousStorage;
							}
						}
					}
					return 0;
				}
				if ((pickTargetTypedId & 4278190080U) == 134217728U)
				{
					int num5 = (int)(pickTargetTypedId & 16777215U);
					ref PowerGeneratorComponent ptr2 = ref this.powerSystem.genPool[num5];
					Mutex obj = this.entityMutexs[ptr2.entityId];
					lock (obj)
					{
						if (this.powerSystem.genPool[insertPowerGenId].fuelCount <= 8)
						{
							int num4;
							int result2;
							if (fuelId > 0)
							{
								result2 = ptr2.PickFuelFrom(fuelId, out num4);
							}
							else
							{
								int fuelMask2 = (int)ptr2.fuelMask;
								if ((fuelMask2 & fuelMask) == 0)
								{
									return 0;
								}
								if ((fuelMask2 & fuelMask) == fuelMask2)
								{
									result2 = ptr2.PickFuelFrom(filter, out num4);
								}
								else
								{
									result2 = ptr2.PickFuelFrom(filter, fuelMask, out num4);
								}
							}
							inc = (byte)num4;
							return result2;
						}
						fuelFull = true;
					}
				}
				return 0;
			}
			int result3;
			return result3;
		}
		int beltId = (int)(pickTargetTypedId & 16777215U);
		if (fuelId > 0)
		{
			return this.cargoTraffic.TryPickItem(beltId, offset, fuelId, out stack, out inc);
		}
		return this.cargoTraffic.TryPickFuel(beltId, offset, filter, fuelMask, out stack, out inc);
	}

	// Token: 0x06001902 RID: 6402 RVA: 0x001AFE98 File Offset: 0x001AE098
	public int PickFrom(int entityId, int offset, int filter, int[] needs, out byte stack, out byte inc)
	{
		stack = 1;
		inc = 0;
		int beltId = this.entityPool[entityId].beltId;
		if (beltId <= 0)
		{
			int assemblerId = this.entityPool[entityId].assemblerId;
			if (assemblerId > 0)
			{
				Mutex obj = this.entityMutexs[entityId];
				lock (obj)
				{
					RecipeExecuteData recipeExecuteData = this.factorySystem.assemblerPool[assemblerId].recipeExecuteData;
					int[] array = (recipeExecuteData != null) ? recipeExecuteData.products : null;
					int[] produced = this.factorySystem.assemblerPool[assemblerId].produced;
					if (array == null)
					{
						return 0;
					}
					int num = array.Length;
					if (num == 1)
					{
						if (produced[0] > 0 && array[0] > 0 && (filter == 0 || filter == array[0]) && (needs == null || needs[0] == array[0] || needs[1] == array[0] || needs[2] == array[0] || needs[3] == array[0] || needs[4] == array[0] || needs[5] == array[0]))
						{
							produced[0]--;
							return array[0];
						}
					}
					else if (num == 2)
					{
						if ((filter == array[0] || filter == 0) && produced[0] > 0 && array[0] > 0 && (needs == null || needs[0] == array[0] || needs[1] == array[0] || needs[2] == array[0] || needs[3] == array[0] || needs[4] == array[0] || needs[5] == array[0]))
						{
							produced[0]--;
							return array[0];
						}
						if ((filter == array[1] || filter == 0) && produced[1] > 0 && array[1] > 0 && (needs == null || needs[0] == array[1] || needs[1] == array[1] || needs[2] == array[1] || needs[3] == array[1] || needs[4] == array[1] || needs[5] == array[1]))
						{
							produced[1]--;
							return array[1];
						}
					}
					else
					{
						for (int i = 0; i < num; i++)
						{
							if ((filter == array[i] || filter == 0) && produced[i] > 0 && array[i] > 0 && (needs == null || needs[0] == array[i] || needs[1] == array[i] || needs[2] == array[i] || needs[3] == array[i] || needs[4] == array[i] || needs[5] == array[i]))
							{
								produced[i]--;
								return array[i];
							}
						}
					}
				}
				return 0;
			}
			int ejectorId = this.entityPool[entityId].ejectorId;
			if (ejectorId > 0)
			{
				Mutex obj = this.entityMutexs[entityId];
				lock (obj)
				{
					int bulletId = this.factorySystem.ejectorPool[ejectorId].bulletId;
					int bulletCount = this.factorySystem.ejectorPool[ejectorId].bulletCount;
					if (bulletId > 0 && bulletCount > 5 && (filter == 0 || filter == bulletId) && (needs == null || needs[0] == bulletId || needs[1] == bulletId || needs[2] == bulletId || needs[3] == bulletId || needs[4] == bulletId || needs[5] == bulletId))
					{
						this.factorySystem.ejectorPool[ejectorId].TakeOneBulletUnsafe(out inc);
						return bulletId;
					}
				}
				return 0;
			}
			int siloId = this.entityPool[entityId].siloId;
			if (siloId > 0)
			{
				Mutex obj = this.entityMutexs[entityId];
				lock (obj)
				{
					int bulletId2 = this.factorySystem.siloPool[siloId].bulletId;
					int bulletCount2 = this.factorySystem.siloPool[siloId].bulletCount;
					if (bulletId2 > 0 && bulletCount2 > 1 && (filter == 0 || filter == bulletId2) && (needs == null || needs[0] == bulletId2 || needs[1] == bulletId2 || needs[2] == bulletId2 || needs[3] == bulletId2 || needs[4] == bulletId2 || needs[5] == bulletId2))
					{
						this.factorySystem.siloPool[siloId].TakeOneBulletUnsafe(out inc);
						return bulletId2;
					}
				}
				return 0;
			}
			int storageId = this.entityPool[entityId].storageId;
			if (storageId > 0)
			{
				StorageComponent storageComponent = this.factoryStorage.storagePool[storageId];
				StorageComponent storageComponent2 = storageComponent;
				if (storageComponent != null)
				{
					storageComponent = storageComponent.topStorage;
					while (storageComponent != null)
					{
						Mutex obj = this.entityMutexs[storageComponent.entityId];
						lock (obj)
						{
							if (storageComponent.lastEmptyItem != 0 && storageComponent.lastEmptyItem != filter)
							{
								int result = filter;
								int num2 = 1;
								bool flag2;
								if (needs == null)
								{
									int num3;
									storageComponent.TakeTailItems(ref result, ref num2, out num3, this.entityPool[storageComponent.entityId].battleBaseId > 0);
									inc = (byte)num3;
									flag2 = (num2 == 1);
								}
								else
								{
									int num3;
									bool flag3 = storageComponent.TakeTailItems(ref result, ref num2, needs, out num3, this.entityPool[storageComponent.entityId].battleBaseId > 0);
									inc = (byte)num3;
									flag2 = (num2 == 1 || flag3);
								}
								if (num2 == 1)
								{
									storageComponent.lastEmptyItem = -1;
									return result;
								}
								if (!flag2)
								{
									storageComponent.lastEmptyItem = filter;
								}
							}
							if (storageComponent == storageComponent2)
							{
								break;
							}
							storageComponent = storageComponent.previousStorage;
						}
					}
				}
				return 0;
			}
			int stationId = this.entityPool[entityId].stationId;
			if (stationId > 0)
			{
				StationComponent stationComponent = this.transport.stationPool[stationId];
				if (stationComponent != null)
				{
					Mutex obj = this.entityMutexs[entityId];
					lock (obj)
					{
						int result2 = filter;
						int num4 = 1;
						if (needs == null)
						{
							int num3;
							stationComponent.TakeItem(ref result2, ref num4, out num3);
							inc = (byte)num3;
						}
						else
						{
							int num3;
							stationComponent.TakeItem(ref result2, ref num4, needs, out num3);
							inc = (byte)num3;
						}
						if (num4 == 1)
						{
							return result2;
						}
					}
				}
				return 0;
			}
			int labId = this.entityPool[entityId].labId;
			if (labId > 0)
			{
				Mutex obj = this.entityMutexs[entityId];
				lock (obj)
				{
					RecipeExecuteData recipeExecuteData2 = this.factorySystem.labPool[labId].recipeExecuteData;
					int[] array2 = (recipeExecuteData2 != null) ? recipeExecuteData2.products : null;
					int[] produced2 = this.factorySystem.labPool[labId].produced;
					if (array2 == null || produced2 == null)
					{
						return 0;
					}
					for (int j = 0; j < array2.Length; j++)
					{
						if (produced2[j] > 0 && array2[j] > 0 && (filter == 0 || filter == array2[j]) && (needs == null || needs[0] == array2[j] || needs[1] == array2[j] || needs[2] == array2[j] || needs[3] == array2[j] || needs[4] == array2[j] || needs[5] == array2[j]))
						{
							produced2[j]--;
							return array2[j];
						}
					}
				}
				return 0;
			}
			int powerGenId = this.entityPool[entityId].powerGenId;
			if (powerGenId > 0 && offset > 0 && this.powerSystem.genPool[offset].id == offset)
			{
				Mutex obj = this.entityMutexs[entityId];
				lock (obj)
				{
					if (this.powerSystem.genPool[offset].fuelCount <= 8)
					{
						int num3;
						int result3 = this.powerSystem.genPool[powerGenId].PickFuelFrom(filter, out num3);
						inc = (byte)num3;
						return result3;
					}
				}
			}
			return 0;
		}
		if (needs == null)
		{
			return this.cargoTraffic.TryPickItem(beltId, offset, filter, out stack, out inc);
		}
		return this.cargoTraffic.TryPickItem(beltId, offset, filter, needs, out stack, out inc);
	}

	// Token: 0x06001903 RID: 6403 RVA: 0x001B077C File Offset: 0x001AE97C
	public int PickFrom(uint ioTargetTypedId, int offset, int filter, int[] needs, out byte stack, out byte inc)
	{
		stack = 1;
		inc = 0;
		int num = (int)(ioTargetTypedId & 16777215U);
		EFactoryIOTargetType efactoryIOTargetType = (EFactoryIOTargetType)(ioTargetTypedId & 4278190080U);
		if (efactoryIOTargetType <= EFactoryIOTargetType.Silo)
		{
			if (efactoryIOTargetType <= EFactoryIOTargetType.Assembler)
			{
				if (efactoryIOTargetType != EFactoryIOTargetType.Belt)
				{
					if (efactoryIOTargetType == EFactoryIOTargetType.Assembler)
					{
						ref AssemblerComponent ptr = ref this.factorySystem.assemblerPool[num];
						if (ptr.id <= 0)
						{
							return 0;
						}
						int entityId = ptr.entityId;
						Mutex obj = this.entityMutexs[entityId];
						lock (obj)
						{
							RecipeExecuteData recipeExecuteData = this.factorySystem.assemblerPool[num].recipeExecuteData;
							int[] array = (recipeExecuteData != null) ? recipeExecuteData.products : null;
							int[] produced = this.factorySystem.assemblerPool[num].produced;
							if (array == null)
							{
								return 0;
							}
							int num2 = array.Length;
							if (num2 == 1)
							{
								if (produced[0] > 0 && array[0] > 0 && (filter == 0 || filter == array[0]) && (needs == null || needs[0] == array[0] || needs[1] == array[0] || needs[2] == array[0] || needs[3] == array[0] || needs[4] == array[0] || needs[5] == array[0]))
								{
									produced[0]--;
									return array[0];
								}
							}
							else if (num2 == 2)
							{
								if ((filter == array[0] || filter == 0) && produced[0] > 0 && array[0] > 0 && (needs == null || needs[0] == array[0] || needs[1] == array[0] || needs[2] == array[0] || needs[3] == array[0] || needs[4] == array[0] || needs[5] == array[0]))
								{
									produced[0]--;
									return array[0];
								}
								if ((filter == array[1] || filter == 0) && produced[1] > 0 && array[1] > 0 && (needs == null || needs[0] == array[1] || needs[1] == array[1] || needs[2] == array[1] || needs[3] == array[1] || needs[4] == array[1] || needs[5] == array[1]))
								{
									produced[1]--;
									return array[1];
								}
							}
							else
							{
								for (int i = 0; i < num2; i++)
								{
									if ((filter == array[i] || filter == 0) && produced[i] > 0 && array[i] > 0 && (needs == null || needs[0] == array[i] || needs[1] == array[i] || needs[2] == array[i] || needs[3] == array[i] || needs[4] == array[i] || needs[5] == array[i]))
									{
										produced[i]--;
										return array[i];
									}
								}
							}
						}
						return 0;
					}
				}
				else
				{
					if (this.cargoTraffic.beltPool[num].id <= 0)
					{
						return 0;
					}
					if (needs == null)
					{
						return this.cargoTraffic.TryPickItem(num, offset, filter, out stack, out inc);
					}
					return this.cargoTraffic.TryPickItem(num, offset, filter, needs, out stack, out inc);
				}
			}
			else if (efactoryIOTargetType != EFactoryIOTargetType.Ejector)
			{
				if (efactoryIOTargetType == EFactoryIOTargetType.Silo)
				{
					ref SiloComponent ptr2 = ref this.factorySystem.siloPool[num];
					if (ptr2.id <= 0)
					{
						return 0;
					}
					int entityId2 = ptr2.entityId;
					Mutex obj = this.entityMutexs[entityId2];
					lock (obj)
					{
						int bulletId = this.factorySystem.siloPool[num].bulletId;
						int bulletCount = this.factorySystem.siloPool[num].bulletCount;
						if (bulletId > 0 && bulletCount > 1 && (filter == 0 || filter == bulletId) && (needs == null || needs[0] == bulletId || needs[1] == bulletId || needs[2] == bulletId || needs[3] == bulletId || needs[4] == bulletId || needs[5] == bulletId))
						{
							this.factorySystem.siloPool[num].TakeOneBulletUnsafe(out inc);
							return bulletId;
						}
					}
					return 0;
				}
			}
			else
			{
				ref EjectorComponent ptr3 = ref this.factorySystem.ejectorPool[num];
				if (ptr3.id < 0)
				{
					return 0;
				}
				int entityId3 = ptr3.entityId;
				Mutex obj = this.entityMutexs[entityId3];
				lock (obj)
				{
					int bulletId2 = this.factorySystem.ejectorPool[num].bulletId;
					int bulletCount2 = this.factorySystem.ejectorPool[num].bulletCount;
					if (bulletId2 > 0 && bulletCount2 > 5 && (filter == 0 || filter == bulletId2) && (needs == null || needs[0] == bulletId2 || needs[1] == bulletId2 || needs[2] == bulletId2 || needs[3] == bulletId2 || needs[4] == bulletId2 || needs[5] == bulletId2))
					{
						this.factorySystem.ejectorPool[num].TakeOneBulletUnsafe(out inc);
						return bulletId2;
					}
				}
				return 0;
			}
		}
		else if (efactoryIOTargetType <= EFactoryIOTargetType.Station)
		{
			if (efactoryIOTargetType != EFactoryIOTargetType.Storage)
			{
				if (efactoryIOTargetType == EFactoryIOTargetType.Station)
				{
					if (num > 0)
					{
						StationComponent stationComponent = this.transport.stationPool[num];
						int entityId4 = stationComponent.entityId;
						if (stationComponent != null)
						{
							Mutex obj = this.entityMutexs[entityId4];
							lock (obj)
							{
								int result = filter;
								int num3 = 1;
								if (needs == null)
								{
									int num4;
									stationComponent.TakeItem(ref result, ref num3, out num4);
									inc = (byte)num4;
								}
								else
								{
									int num4;
									stationComponent.TakeItem(ref result, ref num3, needs, out num4);
									inc = (byte)num4;
								}
								if (num3 == 1)
								{
									return result;
								}
							}
						}
						return 0;
					}
					return 0;
				}
			}
			else
			{
				if (num > 0)
				{
					StorageComponent storageComponent = this.factoryStorage.storagePool[num];
					StorageComponent storageComponent2 = storageComponent;
					if (storageComponent != null)
					{
						storageComponent = storageComponent.topStorage;
						while (storageComponent != null)
						{
							Mutex obj = this.entityMutexs[storageComponent.entityId];
							lock (obj)
							{
								if (storageComponent.lastEmptyItem != 0 && storageComponent.lastEmptyItem != filter)
								{
									int result2 = filter;
									int num5 = 1;
									bool flag2;
									if (needs == null)
									{
										int num4;
										storageComponent.TakeTailItems(ref result2, ref num5, out num4, this.entityPool[storageComponent.entityId].battleBaseId > 0);
										inc = (byte)num4;
										flag2 = (num5 == 1);
									}
									else
									{
										int num4;
										bool flag3 = storageComponent.TakeTailItems(ref result2, ref num5, needs, out num4, this.entityPool[storageComponent.entityId].battleBaseId > 0);
										inc = (byte)num4;
										flag2 = (num5 == 1 || flag3);
									}
									if (num5 == 1)
									{
										storageComponent.lastEmptyItem = -1;
										return result2;
									}
									if (!flag2)
									{
										storageComponent.lastEmptyItem = filter;
									}
								}
								if (storageComponent == storageComponent2)
								{
									break;
								}
								storageComponent = storageComponent.previousStorage;
							}
						}
					}
					return 0;
				}
				return 0;
			}
		}
		else if (efactoryIOTargetType != EFactoryIOTargetType.Lab)
		{
			if (efactoryIOTargetType == EFactoryIOTargetType.PowerGen)
			{
				ref PowerGeneratorComponent ptr4 = ref this.powerSystem.genPool[num];
				if (ptr4.id < 0)
				{
					return 0;
				}
				int entityId5 = ptr4.entityId;
				if (offset > 0 && this.powerSystem.genPool[offset].id == offset)
				{
					Mutex obj = this.entityMutexs[entityId5];
					lock (obj)
					{
						if (this.powerSystem.genPool[offset].fuelCount <= 8)
						{
							int num4;
							int result3 = ptr4.PickFuelFrom(filter, out num4);
							inc = (byte)num4;
							return result3;
						}
					}
				}
				return 0;
			}
		}
		else
		{
			ref LabComponent ptr5 = ref this.factorySystem.labPool[num];
			if (ptr5.id <= 0)
			{
				return 0;
			}
			int entityId6 = ptr5.entityId;
			Mutex obj = this.entityMutexs[entityId6];
			lock (obj)
			{
				RecipeExecuteData recipeExecuteData2 = ptr5.recipeExecuteData;
				int[] array2 = (recipeExecuteData2 != null) ? recipeExecuteData2.products : null;
				int[] produced2 = ptr5.produced;
				if (array2 == null || produced2 == null)
				{
					return 0;
				}
				for (int j = 0; j < array2.Length; j++)
				{
					if (produced2[j] > 0 && array2[j] > 0 && (filter == 0 || filter == array2[j]) && (needs == null || needs[0] == array2[j] || needs[1] == array2[j] || needs[2] == array2[j] || needs[3] == array2[j] || needs[4] == array2[j] || needs[5] == array2[j]))
					{
						produced2[j]--;
						return array2[j];
					}
				}
			}
			return 0;
		}
		return 0;
	}

	// Token: 0x06001904 RID: 6404 RVA: 0x001B10F0 File Offset: 0x001AF2F0
	public int PickFromStorage(int entityId, int itemId, int count, out int inc)
	{
		inc = 0;
		int num = count;
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			StorageComponent storageComponent = this.factoryStorage.storagePool[storageId];
			StorageComponent storageComponent2 = storageComponent;
			if (storageComponent != null)
			{
				storageComponent = storageComponent.topStorage;
				while (storageComponent != null)
				{
					Mutex obj = this.entityMutexs[storageComponent.entityId];
					lock (obj)
					{
						if (storageComponent.lastEmptyItem != 0 && storageComponent.lastEmptyItem != itemId)
						{
							int num2 = itemId;
							int num3 = count;
							int num4;
							storageComponent.TakeTailItems(ref num2, ref num3, out num4, this.entityPool[storageComponent.entityId].battleBaseId > 0);
							count -= num3;
							inc += num4;
							if (num2 > 0)
							{
								itemId = num2;
							}
							if (count == 0)
							{
								storageComponent.lastEmptyItem = -1;
								return num;
							}
							storageComponent.lastEmptyItem = itemId;
						}
						if (storageComponent == storageComponent2)
						{
							break;
						}
						storageComponent = storageComponent.previousStorage;
					}
				}
			}
		}
		return num - count;
	}

	// Token: 0x06001905 RID: 6405 RVA: 0x001B11FC File Offset: 0x001AF3FC
	public int PickFromStorageFiltered(int entityId, ref int filter, int count, out int inc)
	{
		inc = 0;
		int num = count;
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			StorageComponent storageComponent = this.factoryStorage.storagePool[storageId];
			StorageComponent storageComponent2 = storageComponent;
			if (storageComponent != null)
			{
				storageComponent = storageComponent.topStorage;
				while (storageComponent != null)
				{
					Mutex obj = this.entityMutexs[storageComponent.entityId];
					lock (obj)
					{
						if (storageComponent.lastEmptyItem != 0 && storageComponent.lastEmptyItem != filter)
						{
							int num2 = filter;
							int num3 = count;
							int num4;
							storageComponent.TakeTailItemsFiltered(ref num2, ref num3, out num4, this.entityPool[storageComponent.entityId].battleBaseId > 0);
							count -= num3;
							inc += num4;
							if (num2 > 0)
							{
								filter = num2;
							}
							if (count == 0)
							{
								storageComponent.lastEmptyItem = -1;
								return num;
							}
							if (filter >= 0)
							{
								storageComponent.lastEmptyItem = filter;
							}
						}
						if (storageComponent == storageComponent2)
						{
							break;
						}
						storageComponent = storageComponent.previousStorage;
					}
				}
			}
		}
		return num - count;
	}

	// Token: 0x06001906 RID: 6406 RVA: 0x001B1310 File Offset: 0x001AF510
	public void ClearItemsInEntity(int entityId)
	{
		int beltId = this.entityPool[entityId].beltId;
		if (beltId > 0)
		{
			this.cargoTraffic.ClearItems_Belt(beltId);
		}
		int inserterId = this.entityPool[entityId].inserterId;
		if (inserterId > 0)
		{
			this.factorySystem.ClearItems_Inserter(inserterId);
		}
		int spraycoaterId = this.entityPool[entityId].spraycoaterId;
		if (spraycoaterId > 0)
		{
			this.cargoTraffic.ClearItems_Spraycoater(spraycoaterId);
		}
		int pilerId = this.entityPool[entityId].pilerId;
		if (pilerId > 0)
		{
			this.cargoTraffic.ClearItems_Piler(pilerId);
		}
		int minerId = this.entityPool[entityId].minerId;
		if (minerId > 0)
		{
			this.factorySystem.ClearItems_Miner(minerId);
		}
		int assemblerId = this.entityPool[entityId].assemblerId;
		if (assemblerId > 0)
		{
			this.factorySystem.ClearItems_Assembler(assemblerId);
		}
		int fractionatorId = this.entityPool[entityId].fractionatorId;
		if (fractionatorId > 0)
		{
			this.factorySystem.ClearItems_Fractionator(fractionatorId);
		}
		int ejectorId = this.entityPool[entityId].ejectorId;
		if (ejectorId > 0)
		{
			this.factorySystem.ClearItems_Ejector(ejectorId);
		}
		int siloId = this.entityPool[entityId].siloId;
		if (siloId > 0)
		{
			this.factorySystem.ClearItems_Silo(siloId);
		}
		int labId = this.entityPool[entityId].labId;
		if (labId > 0)
		{
			this.factorySystem.ClearItems_Lab(labId);
		}
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			this.factoryStorage.ClearItems_Storage(storageId);
		}
		int tankId = this.entityPool[entityId].tankId;
		if (tankId > 0)
		{
			this.factoryStorage.ClearItems_Tank(tankId);
		}
		int stationId = this.entityPool[entityId].stationId;
		if (stationId > 0)
		{
			this.transport.ClearItems_Station(stationId);
		}
		int dispenserId = this.entityPool[entityId].dispenserId;
		if (dispenserId > 0)
		{
			this.transport.ClearItems_Dispenser(dispenserId);
		}
		int turretId = this.entityPool[entityId].turretId;
		if (turretId > 0)
		{
			this.defenseSystem.ClearItems_Turret(turretId);
		}
		int battleBaseId = this.entityPool[entityId].battleBaseId;
		if (battleBaseId > 0)
		{
			this.defenseSystem.ClearItems_BattleBase(battleBaseId);
		}
		int powerGenId = this.entityPool[entityId].powerGenId;
		if (powerGenId > 0)
		{
			PowerGeneratorComponent powerGeneratorComponent = this.powerSystem.genPool[powerGenId];
			if (powerGeneratorComponent.fuelId > 0 && powerGeneratorComponent.fuelCount > 0)
			{
				this.powerSystem.genPool[powerGenId].fuelId = 0;
				this.powerSystem.genPool[powerGenId].fuelCount = 0;
				this.powerSystem.genPool[powerGenId].fuelInc = 0;
			}
			if (powerGeneratorComponent.gamma)
			{
				bool productId = powerGeneratorComponent.productId != 0;
				int num = (int)powerGeneratorComponent.productCount;
				if (productId && num > 0)
				{
					this.entitySignPool[powerGeneratorComponent.entityId].iconId0 = 0U;
					this.entitySignPool[powerGeneratorComponent.entityId].iconType = 0U;
					this.powerSystem.genPool[powerGenId].productCount = 0f;
				}
				bool catalystId = powerGeneratorComponent.catalystId != 0;
				int num2 = powerGeneratorComponent.catalystPoint / 3600;
				if (catalystId && num2 > 0)
				{
					this.powerSystem.genPool[powerGenId].catalystPoint = 0;
					this.powerSystem.genPool[powerGenId].catalystIncPoint = 0;
				}
			}
		}
		int powerExcId = this.entityPool[entityId].powerExcId;
		if (powerExcId > 0)
		{
			this.powerSystem.excPool[powerExcId].emptyCount = 0;
			this.powerSystem.excPool[powerExcId].fullCount = 0;
			this.powerSystem.excPool[powerExcId].emptyInc = 0;
			this.powerSystem.excPool[powerExcId].fullInc = 0;
		}
	}

	// Token: 0x06001907 RID: 6407 RVA: 0x001B1724 File Offset: 0x001AF924
	public void TakeBackItemsInEntity(Player player, int entityId)
	{
		if (player == null)
		{
			return;
		}
		int beltId = this.entityPool[entityId].beltId;
		if (beltId > 0)
		{
			this.cargoTraffic.PickupBeltItems(player, beltId, true);
		}
		int inserterId = this.entityPool[entityId].inserterId;
		if (inserterId > 0)
		{
			this.factorySystem.TakeBackItems_Inserter(player, inserterId);
		}
		int spraycoaterId = this.entityPool[entityId].spraycoaterId;
		if (spraycoaterId > 0)
		{
			this.cargoTraffic.TakeBackItems_Spraycoater(player, spraycoaterId);
		}
		int pilerId = this.entityPool[entityId].pilerId;
		if (pilerId > 0)
		{
			this.cargoTraffic.TakeBackItems_Piler(player, pilerId);
		}
		int minerId = this.entityPool[entityId].minerId;
		if (minerId > 0)
		{
			this.factorySystem.TakeBackItems_Miner(player, minerId);
		}
		int assemblerId = this.entityPool[entityId].assemblerId;
		if (assemblerId > 0)
		{
			this.factorySystem.TakeBackItems_Assembler(player, assemblerId);
		}
		int fractionatorId = this.entityPool[entityId].fractionatorId;
		if (fractionatorId > 0)
		{
			this.factorySystem.TakeBackItems_Fractionator(player, fractionatorId);
		}
		int ejectorId = this.entityPool[entityId].ejectorId;
		if (ejectorId > 0)
		{
			this.factorySystem.TakeBackItems_Ejector(player, ejectorId);
		}
		int siloId = this.entityPool[entityId].siloId;
		if (siloId > 0)
		{
			this.factorySystem.TakeBackItems_Silo(player, siloId);
		}
		int labId = this.entityPool[entityId].labId;
		if (labId > 0)
		{
			this.factorySystem.TakeBackItems_Lab(player, labId);
		}
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			this.factoryStorage.TakeBackItems_Storage(player, storageId);
			GC.Collect();
		}
		int tankId = this.entityPool[entityId].tankId;
		if (tankId > 0)
		{
			this.factoryStorage.TakeBackItems_Tank(player, tankId);
		}
		int stationId = this.entityPool[entityId].stationId;
		if (stationId > 0)
		{
			this.transport.TakeBackItems_Station(player, stationId);
			GC.Collect();
		}
		int dispenserId = this.entityPool[entityId].dispenserId;
		if (dispenserId > 0)
		{
			this.transport.TakeBackItems_Dispenser(player, dispenserId);
			GC.Collect();
		}
		int turretId = this.entityPool[entityId].turretId;
		if (turretId > 0)
		{
			this.defenseSystem.TakeBackItems_Turret(player, turretId);
		}
		int battleBaseId = this.entityPool[entityId].battleBaseId;
		if (battleBaseId > 0)
		{
			this.defenseSystem.TakeBackItems_BattleBase(player, battleBaseId);
		}
		int powerGenId = this.entityPool[entityId].powerGenId;
		if (powerGenId > 0)
		{
			PowerGeneratorComponent powerGeneratorComponent = this.powerSystem.genPool[powerGenId];
			if (powerGeneratorComponent.fuelId > 0 && powerGeneratorComponent.fuelCount > 0)
			{
				int upCount = player.TryAddItemToPackage((int)powerGeneratorComponent.fuelId, (int)powerGeneratorComponent.fuelCount, (int)powerGeneratorComponent.fuelInc, true, entityId, false);
				UIItemup.Up((int)powerGeneratorComponent.fuelId, upCount);
				this.powerSystem.genPool[powerGenId].fuelId = 0;
				this.powerSystem.genPool[powerGenId].fuelCount = 0;
				this.powerSystem.genPool[powerGenId].fuelInc = 0;
			}
			if (powerGeneratorComponent.gamma)
			{
				int productId = powerGeneratorComponent.productId;
				int num = (int)powerGeneratorComponent.productCount;
				if (productId != 0 && num > 0)
				{
					int upCount2 = player.TryAddItemToPackage(productId, num, 0, true, entityId, false);
					UIItemup.Up(productId, upCount2);
					this.entitySignPool[powerGeneratorComponent.entityId].iconId0 = 0U;
					this.entitySignPool[powerGeneratorComponent.entityId].iconType = 0U;
					this.powerSystem.genPool[powerGenId].productCount = 0f;
				}
				int catalystId = powerGeneratorComponent.catalystId;
				int catalystPoint = powerGeneratorComponent.catalystPoint;
				int catalystIncPoint = powerGeneratorComponent.catalystIncPoint;
				int num2 = powerGeneratorComponent.catalystPoint % 3600;
				int catalystIncPoint2 = this.split_inc(ref catalystPoint, ref catalystIncPoint, num2);
				int num3 = catalystPoint / 3600;
				int inc = catalystIncPoint / 3600;
				if (catalystId != 0 && num3 > 0)
				{
					this.powerSystem.genPool[powerGenId].catalystPoint = num2;
					this.powerSystem.genPool[powerGenId].catalystIncPoint = catalystIncPoint2;
					int upCount3 = player.TryAddItemToPackage(catalystId, num3, inc, true, entityId, false);
					UIItemup.Up(catalystId, upCount3);
				}
			}
		}
		int powerExcId = this.entityPool[entityId].powerExcId;
		if (powerExcId > 0)
		{
			PowerExchangerComponent powerExchangerComponent = this.powerSystem.excPool[powerExcId];
			if (powerExchangerComponent.emptyCount > 0)
			{
				int emptyCount = (int)powerExchangerComponent.emptyCount;
				int emptyInc = (int)powerExchangerComponent.emptyInc;
				int upCount4 = player.TryAddItemToPackage(powerExchangerComponent.emptyId, emptyCount, emptyInc, true, entityId, false);
				UIItemup.Up(powerExchangerComponent.emptyId, upCount4);
				this.powerSystem.excPool[powerExcId].emptyCount = 0;
			}
			if (powerExchangerComponent.fullCount > 0)
			{
				int fullCount = (int)powerExchangerComponent.fullCount;
				int fullInc = (int)powerExchangerComponent.fullInc;
				int upCount5 = player.TryAddItemToPackage(powerExchangerComponent.fullId, fullCount, fullInc, true, entityId, false);
				UIItemup.Up(powerExchangerComponent.fullId, upCount5);
				this.powerSystem.excPool[powerExcId].fullCount = 0;
			}
			this.powerSystem.excPool[powerExcId].emptyCount = 0;
			this.powerSystem.excPool[powerExcId].fullCount = 0;
			this.powerSystem.excPool[powerExcId].emptyInc = 0;
			this.powerSystem.excPool[powerExcId].fullInc = 0;
		}
	}

	// Token: 0x06001908 RID: 6408 RVA: 0x001B1CC4 File Offset: 0x001AFEC4
	public void ThrowItemsInEntity(int entityId)
	{
		float dropRate = LDB.items.Select((int)this.entityPool[entityId].protoId).DropRate;
		int assemblerId = this.entityPool[entityId].assemblerId;
		if (assemblerId > 0)
		{
			this.factorySystem.ThrowItems_Assembler(assemblerId, dropRate);
		}
		int labId = this.entityPool[entityId].labId;
		if (labId > 0)
		{
			this.factorySystem.ThrowItems_Lab(labId, dropRate);
		}
		int storageId = this.entityPool[entityId].storageId;
		if (storageId > 0)
		{
			this.factoryStorage.ThrowItems_Storage(storageId, dropRate);
		}
		int tankId = this.entityPool[entityId].tankId;
		if (tankId > 0)
		{
			this.factoryStorage.ThrowItems_Tank(tankId, dropRate);
		}
		int stationId = this.entityPool[entityId].stationId;
		if (stationId > 0)
		{
			this.transport.ThrowItems_Station(stationId, dropRate);
		}
		int battleBaseId = this.entityPool[entityId].battleBaseId;
		if (battleBaseId > 0)
		{
			this.defenseSystem.ThrowItems_BattleBase(battleBaseId, dropRate);
		}
		int beltId = this.entityPool[entityId].beltId;
		if (beltId > 0)
		{
			BeltComponent beltComponent = this.cargoTraffic.beltPool[beltId];
			int backInputId = beltComponent.backInputId;
			int rightInputId = beltComponent.rightInputId;
			int leftInputId = beltComponent.leftInputId;
			int outputId = beltComponent.outputId;
			this.cargoTraffic.ThrowItems_Belt(beltId, dropRate);
			if (backInputId > 0)
			{
				this.cargoTraffic.ThrowItems_Belt(backInputId, dropRate);
			}
			if (rightInputId > 0)
			{
				this.cargoTraffic.ThrowItems_Belt(rightInputId, dropRate);
			}
			if (leftInputId > 0)
			{
				this.cargoTraffic.ThrowItems_Belt(leftInputId, dropRate);
			}
			if (outputId > 0)
			{
				this.cargoTraffic.ThrowItems_Belt(outputId, dropRate);
			}
		}
	}

	// Token: 0x06001909 RID: 6409 RVA: 0x001B1E74 File Offset: 0x001B0074
	public void EntityFastTakeOut(int entityId, bool toPackage, out ItemBundle itemBundle, out bool full)
	{
		if (this._tmp_items == null)
		{
			this._tmp_items = new ItemBundle();
		}
		this._tmp_items.Clear();
		itemBundle = this._tmp_items;
		full = false;
		if (entityId == 0 || this.entityPool[entityId].id != entityId)
		{
			return;
		}
		Player mainPlayer = this.gameData.mainPlayer;
		int size = mainPlayer.package.size;
		if (this._tmp_package == null)
		{
			this._tmp_package = new StorageComponent(size);
		}
		if (size != this._tmp_package.size)
		{
			this._tmp_package.SetSize(size);
		}
		Array.Copy(mainPlayer.package.grids, this._tmp_package.grids, size);
		EntityData entityData = this.entityPool[entityId];
		if (entityData.beltId > 0)
		{
			this.BeltFastTakeOut(entityData.beltId, toPackage, false, 0, ref full);
		}
		if (entityData.splitterId > 0)
		{
			int splitterId = entityData.splitterId;
			SplitterComponent[] splitterPool = this.cargoTraffic.splitterPool;
			int beltA = splitterPool[splitterId].beltA;
			int beltB = splitterPool[splitterId].beltB;
			int beltC = splitterPool[splitterId].beltC;
			int beltD = splitterPool[splitterId].beltD;
			if (beltA > 0)
			{
				this.BeltFastTakeOut(beltA, toPackage, false, 0, ref full);
			}
			if (beltB > 0)
			{
				this.BeltFastTakeOut(beltB, toPackage, false, 0, ref full);
			}
			if (beltC > 0)
			{
				this.BeltFastTakeOut(beltC, toPackage, false, 0, ref full);
			}
			if (beltD > 0)
			{
				this.BeltFastTakeOut(beltD, toPackage, false, 0, ref full);
			}
		}
		if (entityData.monitorId > 0)
		{
			int monitorId = entityData.monitorId;
			int targetBeltId = this.cargoTraffic.monitorPool[monitorId].targetBeltId;
			if (targetBeltId > 0)
			{
				this.BeltFastTakeOut(targetBeltId, toPackage, true, 10, ref full);
			}
		}
		if (entityData.storageId > 0)
		{
			StorageComponent storageComponent = this.factoryStorage.storagePool[entityData.storageId];
			if (toPackage)
			{
				StorageComponent package = mainPlayer.package;
				storageComponent.TransferTo(package, this._tmp_items);
			}
			else
			{
				for (int i = storageComponent.grids.Length - 1; i >= 0; i--)
				{
					int inhandItemId = mainPlayer.inhandItemId;
					int num = 0;
					int addinc;
					storageComponent.TakeItemFromGrid(i, ref inhandItemId, ref num, out addinc);
					if (num > 0 && inhandItemId > 0)
					{
						mainPlayer.SetHandItemId_Unsafe(inhandItemId);
						mainPlayer.AddHandItemCount_Unsafe(num);
						mainPlayer.AddHandItemInc_Unsafe(addinc);
						this._tmp_items.Alter(inhandItemId, num);
					}
				}
			}
		}
		if (entityData.tankId > 0)
		{
			int tankId = entityData.tankId;
			TankComponent[] tankPool = this.factoryStorage.tankPool;
			if (toPackage)
			{
				int fluidId = tankPool[tankId].fluidId;
				int fluidCount = tankPool[tankId].fluidCount;
				int fluidInc = tankPool[tankId].fluidInc;
				int num3;
				int num2 = this._tmp_package.AddItemStacked(fluidId, 2, 0, out num3);
				int num4 = fluidCount;
				int num5 = fluidInc;
				int num6 = (tankPool[tankId].fluidCount >= num2) ? num2 : tankPool[tankId].fluidCount;
				int num7 = this.split_inc(ref num4, ref num5, num6);
				this._tmp_items.Alter(fluidId, num6);
				mainPlayer.TryAddItemToPackage(fluidId, num6, num7, true, entityId, false);
				TankComponent[] array = tankPool;
				int num8 = tankId;
				array[num8].fluidCount = array[num8].fluidCount - num6;
				TankComponent[] array2 = tankPool;
				int num9 = tankId;
				array2[num9].fluidInc = array2[num9].fluidInc - num7;
				if (tankPool[tankId].fluidCount == 0)
				{
					tankPool[tankId].fluidId = 0;
				}
			}
			else if ((mainPlayer.inhandItemId == 0 || mainPlayer.inhandItemId == tankPool[tankId].fluidId) && tankPool[tankId].fluidId > 0 && tankPool[tankId].fluidCount > 0)
			{
				int num10 = (tankPool[tankId].fluidCount > 1) ? 2 : 1;
				int fluidCount2 = tankPool[tankId].fluidCount;
				int fluidInc2 = tankPool[tankId].fluidInc;
				int num11 = num10;
				int num12 = this.split_inc(ref fluidCount2, ref fluidInc2, num11);
				this._tmp_items.Alter(mainPlayer.inhandItemId, num11);
				mainPlayer.SetHandItemId_Unsafe(tankPool[tankId].fluidId);
				mainPlayer.AddHandItemCount_Unsafe(num11);
				mainPlayer.SetHandItemInc_Unsafe(mainPlayer.inhandItemInc + num12);
				TankComponent[] array3 = tankPool;
				int num13 = tankId;
				array3[num13].fluidCount = array3[num13].fluidCount - num11;
				TankComponent[] array4 = tankPool;
				int num14 = tankId;
				array4[num14].fluidInc = array4[num14].fluidInc - num12;
				if (tankPool[tankId].fluidCount == 0)
				{
					tankPool[tankId].fluidId = 0;
				}
			}
		}
		if (entityData.minerId > 0)
		{
			int minerId = entityData.minerId;
			MinerComponent[] minerPool = this.factorySystem.minerPool;
			int productId = minerPool[minerId].productId;
			int productCount = minerPool[minerId].productCount;
			int num15 = 0;
			if (productId > 0 && productCount > 0)
			{
				mainPlayer.SendItemToPlayer(ref productId, ref productCount, ref num15, toPackage, this._tmp_items);
				minerPool[minerId].productId = productId;
				minerPool[minerId].productCount = productCount;
			}
			else if (minerPool[minerId].insertTarget > 0)
			{
				int beltId = this.entityPool[minerPool[minerId].insertTarget].beltId;
				if (beltId > 0)
				{
					this.BeltFastTakeOut(beltId, toPackage, false, 0, ref full);
				}
			}
		}
		if (entityData.inserterId > 0)
		{
			int inserterId = entityData.inserterId;
			InserterComponent[] inserterPool = this.factorySystem.inserterPool;
			int itemId = inserterPool[inserterId].itemId;
			int itemCount = (int)inserterPool[inserterId].itemCount;
			int itemInc = (int)inserterPool[inserterId].itemInc;
			mainPlayer.SendItemToPlayer(ref itemId, ref itemCount, ref itemInc, toPackage, this._tmp_items);
			inserterPool[inserterId].itemId = itemId;
			inserterPool[inserterId].itemCount = (short)itemCount;
			inserterPool[inserterId].itemInc = (short)itemInc;
			if (inserterPool[inserterId].stackCount > (int)inserterPool[inserterId].itemCount)
			{
				inserterPool[inserterId].stackCount = (int)inserterPool[inserterId].itemCount;
			}
		}
		if (entityData.assemblerId > 0)
		{
			int assemblerId = entityData.assemblerId;
			AssemblerComponent[] assemblerPool = this.factorySystem.assemblerPool;
			if (assemblerPool[assemblerId].recipeId > 0)
			{
				int[] products = assemblerPool[assemblerId].recipeExecuteData.products;
				int[] produced = assemblerPool[assemblerId].produced;
				for (int j = 0; j < produced.Length; j++)
				{
					int num16 = products[j];
					int num17 = produced[j];
					int num18 = 0;
					mainPlayer.SendItemToPlayer(ref num16, ref num17, ref num18, toPackage, this._tmp_items);
					produced[j] = num17;
				}
			}
		}
		if (entityData.fractionatorId > 0)
		{
			int fractionatorId = entityData.fractionatorId;
			FractionatorComponent[] fractionatorPool = this.factorySystem.fractionatorPool;
			int productId2 = fractionatorPool[fractionatorId].productId;
			int productOutputCount = fractionatorPool[fractionatorId].productOutputCount;
			int num19 = 0;
			mainPlayer.SendItemToPlayer(ref productId2, ref productOutputCount, ref num19, toPackage, this._tmp_items);
			fractionatorPool[fractionatorId].productOutputCount = productOutputCount;
			int belt = fractionatorPool[fractionatorId].belt0;
			if (belt > 0)
			{
				this.BeltFastTakeOut(belt, toPackage, true, 0, ref full);
			}
		}
		if (entityData.ejectorId > 0)
		{
			int ejectorId = entityData.ejectorId;
			EjectorComponent[] ejectorPool = this.factorySystem.ejectorPool;
			int bulletId = ejectorPool[ejectorId].bulletId;
			int bulletCount = ejectorPool[ejectorId].bulletCount;
			int bulletInc = ejectorPool[ejectorId].bulletInc;
			mainPlayer.SendItemToPlayer(ref bulletId, ref bulletCount, ref bulletInc, toPackage, this._tmp_items);
			ejectorPool[ejectorId].bulletCount = bulletCount;
			ejectorPool[ejectorId].bulletInc = bulletInc;
		}
		if (entityData.siloId > 0)
		{
			int siloId = entityData.siloId;
			SiloComponent[] siloPool = this.factorySystem.siloPool;
			int bulletId2 = siloPool[siloId].bulletId;
			int bulletCount2 = siloPool[siloId].bulletCount;
			int bulletInc2 = siloPool[siloId].bulletInc;
			mainPlayer.SendItemToPlayer(ref bulletId2, ref bulletCount2, ref bulletInc2, toPackage, this._tmp_items);
			siloPool[siloId].bulletCount = bulletCount2;
			siloPool[siloId].bulletInc = bulletInc2;
		}
		if (entityData.labId > 0)
		{
			int labId = entityData.labId;
			LabComponent[] labPool = this.factorySystem.labPool;
			if (labPool[labId].matrixMode)
			{
				int[] products2 = labPool[labId].recipeExecuteData.products;
				int[] produced2 = labPool[labId].produced;
				for (int k = 0; k < produced2.Length; k++)
				{
					int num20 = products2[k];
					int num21 = produced2[k];
					int num22 = 0;
					mainPlayer.SendItemToPlayer(ref num20, ref num21, ref num22, toPackage, this._tmp_items);
					produced2[k] = num21;
				}
			}
			else
			{
				bool researchMode = labPool[labId].researchMode;
			}
		}
		int stationId = entityData.stationId;
		int powerNodeId = entityData.powerNodeId;
		if (entityData.powerGenId > 0)
		{
			int powerGenId = entityData.powerGenId;
			PowerGeneratorComponent[] genPool = this.powerSystem.genPool;
			if (genPool[powerGenId].fuelId > 0)
			{
				int fuelId = (int)genPool[powerGenId].fuelId;
				int fuelCount = (int)genPool[powerGenId].fuelCount;
				int fuelInc = (int)genPool[powerGenId].fuelInc;
				mainPlayer.SendItemToPlayer(ref fuelId, ref fuelCount, ref fuelInc, toPackage, this._tmp_items);
				genPool[powerGenId].fuelId = (short)fuelId;
				genPool[powerGenId].fuelCount = (short)fuelCount;
				genPool[powerGenId].fuelInc = (short)fuelInc;
			}
			else if (genPool[powerGenId].gamma && genPool[powerGenId].productId > 0)
			{
				int productId3 = genPool[powerGenId].productId;
				int num23 = (int)genPool[powerGenId].productCount;
				float num24 = genPool[powerGenId].productCount - (float)num23;
				int num25 = 0;
				mainPlayer.SendItemToPlayer(ref productId3, ref num23, ref num25, toPackage, this._tmp_items);
				genPool[powerGenId].productCount = num24 + (float)num23;
			}
		}
		int powerConId = entityData.powerConId;
		int powerAccId = entityData.powerAccId;
		if (entityData.powerExcId > 0)
		{
			int powerExcId = entityData.powerExcId;
			PowerExchangerComponent[] excPool = this.powerSystem.excPool;
			if (excPool[powerExcId].state > 0f)
			{
				int fullId = excPool[powerExcId].fullId;
				int fullCount = (int)excPool[powerExcId].fullCount;
				int fullInc = (int)excPool[powerExcId].fullInc;
				mainPlayer.SendItemToPlayer(ref fullId, ref fullCount, ref fullInc, toPackage, this._tmp_items);
				excPool[powerExcId].fullCount = (short)fullCount;
				excPool[powerExcId].fullInc = (short)fullInc;
			}
			else if (excPool[powerExcId].state < 0f)
			{
				int emptyId = excPool[powerExcId].emptyId;
				int emptyCount = (int)excPool[powerExcId].emptyCount;
				int emptyInc = (int)excPool[powerExcId].emptyInc;
				mainPlayer.SendItemToPlayer(ref emptyId, ref emptyCount, ref emptyInc, toPackage, this._tmp_items);
				excPool[powerExcId].emptyCount = (short)emptyCount;
				excPool[powerExcId].emptyInc = (short)emptyInc;
			}
			else
			{
				int fullId2 = excPool[powerExcId].fullId;
				int fullCount2 = (int)excPool[powerExcId].fullCount;
				int fullInc2 = (int)excPool[powerExcId].fullInc;
				mainPlayer.SendItemToPlayer(ref fullId2, ref fullCount2, ref fullInc2, toPackage, this._tmp_items);
				excPool[powerExcId].fullCount = (short)fullCount2;
				excPool[powerExcId].fullInc = (short)fullInc2;
				int emptyId2 = excPool[powerExcId].emptyId;
				int emptyCount2 = (int)excPool[powerExcId].emptyCount;
				int emptyInc2 = (int)excPool[powerExcId].emptyInc;
				mainPlayer.SendItemToPlayer(ref emptyId2, ref emptyCount2, ref emptyInc2, toPackage, this._tmp_items);
				excPool[powerExcId].emptyCount = (short)emptyCount2;
				excPool[powerExcId].emptyInc = (short)emptyInc2;
			}
		}
		if (entityData.spraycoaterId > 0)
		{
			int spraycoaterId = entityData.spraycoaterId;
			int cargoBeltId = this.cargoTraffic.spraycoaterPool[spraycoaterId].cargoBeltId;
			if (cargoBeltId > 0)
			{
				this.BeltFastTakeOut(cargoBeltId, toPackage, true, 10, ref full);
			}
		}
		if (entityData.pilerId > 0)
		{
			int pilerId = entityData.pilerId;
			int inputBeltId = this.cargoTraffic.pilerPool[pilerId].inputBeltId;
			if (inputBeltId > 0)
			{
				this.BeltFastTakeOut(inputBeltId, toPackage, false, 0, ref full);
			}
		}
		if (entityData.dispenserId > 0)
		{
			int dispenserId = entityData.dispenserId;
			DispenserComponent dispenserComponent = this.transport.dispenserPool[dispenserId];
			if (dispenserComponent.holdupItemCount > 0)
			{
				for (int l = 0; l < dispenserComponent.holdupItemCount; l++)
				{
					int itemId2 = dispenserComponent.holdupPackage[l].itemId;
					int count = dispenserComponent.holdupPackage[l].count;
					int inc = dispenserComponent.holdupPackage[l].inc;
					mainPlayer.SendItemToPlayer(ref itemId2, ref count, ref inc, toPackage, this._tmp_items);
					dispenserComponent.holdupPackage[l].count = count;
					dispenserComponent.holdupPackage[l].inc = inc;
					if (count == 0)
					{
						dispenserComponent.RemoveHoldupItem(l);
						l--;
					}
				}
			}
		}
		if (entityData.turretId > 0)
		{
			this.defenseSystem.TakeBackItems_Turret(mainPlayer, entityData.turretId);
		}
	}

	// Token: 0x0600190A RID: 6410 RVA: 0x001B2BD8 File Offset: 0x001B0DD8
	private void BeltFastTakeOut(int beltId, bool toPackage, bool pivot, int offset, ref bool full)
	{
		if (beltId == 0)
		{
			return;
		}
		Player mainPlayer = this.gameData.mainPlayer;
		int size = mainPlayer.package.size;
		BeltComponent[] beltPool = this.cargoTraffic.beltPool;
		if (beltPool[beltId].id != 0 && beltPool[beltId].id == beltId)
		{
			int num = beltPool[beltId].segIndex + 4;
			int num2 = beltPool[beltId].segIndex + beltPool[beltId].segLength - 1 - 5;
			CargoPath cargoPath = this.cargoTraffic.GetCargoPath(beltPool[beltId].segPathId);
			if (pivot || num2 <= num)
			{
				num2 = (num = beltPool[beltId].segIndex + beltPool[beltId].segPivotOffset + offset);
				if (num >= cargoPath.pathLength)
				{
					num2 = (num = cargoPath.pathLength - 1);
				}
				else if (num < 0)
				{
					num2 = (num = 0);
				}
			}
			for (int i = num; i <= num2; i++)
			{
				byte b;
				byte b2;
				int num3 = cargoPath.QueryItemAtIndex(i, out b, out b2);
				if (num3 > 0 && b > 0)
				{
					if (toPackage)
					{
						Array.Copy(mainPlayer.package.grids, this._tmp_package.grids, size);
						int num5;
						int num4 = this._tmp_package.AddItemStacked(num3, (int)b, (int)b2, out num5);
						if (num4 == (int)b)
						{
							cargoPath.RemoveCargoAtIndex(i);
							mainPlayer.TryAddItemToPackage(num3, num4, (int)b2, true, beltPool[beltId].entityId, false);
							this._tmp_items.Alter(num3, num4);
						}
						else
						{
							full = true;
						}
					}
					else if (mainPlayer.inhandItemId == num3 || mainPlayer.inhandItemId == 0)
					{
						cargoPath.RemoveCargoAtIndex(i);
						mainPlayer.SetHandItemId_Unsafe(num3);
						mainPlayer.AddHandItemCount_Unsafe((int)b);
						mainPlayer.AddHandItemInc_Unsafe((int)b2);
						this._tmp_items.Alter(num3, (int)b);
					}
					else
					{
						full = true;
					}
				}
			}
		}
	}

	// Token: 0x0600190B RID: 6411 RVA: 0x001B2DB8 File Offset: 0x001B0FB8
	public void EntityFastFillIn(int entityId, bool fromPackage, out ItemBundle itemBundle)
	{
		if (this._tmp_items == null)
		{
			this._tmp_items = new ItemBundle();
		}
		this._tmp_items.Clear();
		itemBundle = this._tmp_items;
		if (this._tmp_itemIds == null)
		{
			this._tmp_itemIds = new ItemBundle();
		}
		this._tmp_itemIds.Clear();
		if (entityId == 0 || this.entityPool[entityId].id != entityId)
		{
			return;
		}
		Player mainPlayer = this.gameData.mainPlayer;
		int size = mainPlayer.package.size;
		if (this._tmp_package == null)
		{
			this._tmp_package = new StorageComponent(size);
		}
		if (size != this._tmp_package.size)
		{
			this._tmp_package.SetSize(size);
		}
		Array.Copy(mainPlayer.package.grids, this._tmp_package.grids, size);
		if (!fromPackage && (mainPlayer.inhandItemCount == 0 || mainPlayer.inhandItemId == 0))
		{
			return;
		}
		EntityData entityData = this.entityPool[entityId];
		if (entityData.beltId > 0)
		{
			this.BeltFastFillIn(entityData.beltId, 0, fromPackage, 0, 0);
		}
		if (entityData.splitterId > 0)
		{
			int splitterId = entityData.splitterId;
			SplitterComponent[] splitterPool = this.cargoTraffic.splitterPool;
			int outFilter = splitterPool[splitterId].outFilter;
			int output = splitterPool[splitterId].output0;
			int output2 = splitterPool[splitterId].output1;
			int output3 = splitterPool[splitterId].output2;
			int output4 = splitterPool[splitterId].output3;
			if (output > 0)
			{
				this.BeltFastFillIn(output, 0, fromPackage, outFilter, 0);
			}
			if (output2 > 0)
			{
				this.BeltFastFillIn(output2, 0, fromPackage, 0, outFilter);
			}
			if (output3 > 0)
			{
				this.BeltFastFillIn(output3, 0, fromPackage, 0, outFilter);
			}
			if (output4 > 0)
			{
				this.BeltFastFillIn(output4, 0, fromPackage, 0, outFilter);
			}
		}
		if (entityData.monitorId > 0)
		{
			int monitorId = entityData.monitorId;
			int targetBeltId = this.cargoTraffic.monitorPool[monitorId].targetBeltId;
			if (targetBeltId > 0)
			{
				this.BeltFastFillIn(targetBeltId, -10, fromPackage, this.cargoTraffic.monitorPool[monitorId].cargoFilter, 0);
			}
		}
		if (entityData.storageId > 0)
		{
			if (fromPackage)
			{
				StorageComponent package = mainPlayer.package;
				StorageComponent other = this.factoryStorage.storagePool[entityData.storageId];
				package.TransferTo(other, this._tmp_items);
			}
			else
			{
				StorageComponent storageComponent = this.factoryStorage.storagePool[entityData.storageId];
				int inhandItemId = mainPlayer.inhandItemId;
				int inhandItemCount = mainPlayer.inhandItemCount;
				int inhandItemInc = mainPlayer.inhandItemInc;
				int handItemInc_Unsafe = 0;
				int num = storageComponent.AddItemStacked(inhandItemId, inhandItemCount, inhandItemInc, out handItemInc_Unsafe);
				int num2 = inhandItemCount - num;
				this._tmp_items.Alter(inhandItemId, num);
				if (num2 > 0)
				{
					mainPlayer.SetHandItemId_Unsafe(inhandItemId);
					mainPlayer.SetHandItemCount_Unsafe(num2);
					mainPlayer.SetHandItemInc_Unsafe(handItemInc_Unsafe);
				}
				else
				{
					mainPlayer.SetHandItemId_Unsafe(0);
					mainPlayer.SetHandItemCount_Unsafe(0);
					mainPlayer.SetHandItemInc_Unsafe(0);
				}
			}
		}
		if (entityData.tankId > 0)
		{
			int tankId = entityData.tankId;
			TankComponent[] tankPool = this.factoryStorage.tankPool;
			if (fromPackage)
			{
				int num3 = tankPool[tankId].fluidId;
				if (num3 == 0)
				{
					int num4 = 60;
					bool flag;
					int num5;
					int num6;
					this.planet.factory.ReadObjectConn(entityId, 14, out flag, out num5, out num6);
					while (num5 != 0 && num4 > 0)
					{
						num4--;
						if (num5 > 0)
						{
							int tankId2 = this.entityPool[num5].tankId;
							num3 = tankPool[tankId2].fluidId;
						}
						if (num3 > 0)
						{
							break;
						}
						this.planet.factory.ReadObjectConn(num5, 14, out flag, out num5, out num6);
					}
				}
				if (num3 == 0)
				{
					int num7 = 60;
					bool flag2;
					int num8;
					int num9;
					this.planet.factory.ReadObjectConn(entityId, 15, out flag2, out num8, out num9);
					while (num8 != 0 && num7 > 0)
					{
						num7--;
						if (num8 > 0)
						{
							int tankId3 = this.entityPool[num8].tankId;
							num3 = tankPool[tankId3].fluidId;
						}
						if (num3 > 0)
						{
							break;
						}
						this.planet.factory.ReadObjectConn(num8, 15, out flag2, out num8, out num9);
					}
				}
				StorageComponent.GRID[] grids = mainPlayer.package.grids;
				int num10 = 2;
				for (int i = grids.Length - 1; i >= 0; i--)
				{
					if (grids[i].count != 0 && grids[i].itemId != 0 && ItemProto.isFluid(grids[i].itemId) && (grids[i].itemId == num3 || num3 == 0))
					{
						int num11 = num3;
						int num12 = (grids[i].count > num10) ? num10 : grids[i].count;
						int num13 = tankPool[tankId].fluidCapacity - tankPool[tankId].fluidCount;
						if (num13 == 0)
						{
							break;
						}
						if (num13 < num12)
						{
							num12 = num13;
						}
						if (num12 > 0)
						{
							int num14;
							mainPlayer.package.TakeItemFromGrid(i, ref num11, ref num12, out num14);
							if (num12 > 0 && num11 > 0)
							{
								if (tankPool[tankId].fluidId == 0)
								{
									tankPool[tankId].fluidId = num11;
								}
								TankComponent[] array = tankPool;
								int num15 = tankId;
								array[num15].fluidCount = array[num15].fluidCount + num12;
								TankComponent[] array2 = tankPool;
								int num16 = tankId;
								array2[num16].fluidInc = array2[num16].fluidInc + num14;
								this._tmp_items.Alter(num11, num12);
								num3 = num11;
							}
							num10 -= num12;
							if (num10 == 0)
							{
								break;
							}
						}
					}
				}
			}
			else if (mainPlayer.inhandItemId > 0 && ItemProto.isFluid(mainPlayer.inhandItemId) && mainPlayer.inhandItemCount > 0 && (mainPlayer.inhandItemId == tankPool[tankId].fluidId || tankPool[tankId].fluidId == 0 || tankPool[tankId].fluidCount == 0))
			{
				int inhandItemId2 = mainPlayer.inhandItemId;
				int num17 = (mainPlayer.inhandItemCount > 1) ? 2 : 1;
				int num18 = tankPool[tankId].fluidCapacity - tankPool[tankId].fluidCount;
				if (num18 < num17)
				{
					num17 = num18;
				}
				if (num17 > 0)
				{
					int num19;
					mainPlayer.UseHandItems(num17, out num19);
					if (tankPool[tankId].fluidId == 0)
					{
						tankPool[tankId].fluidId = inhandItemId2;
					}
					TankComponent[] array3 = tankPool;
					int num20 = tankId;
					array3[num20].fluidCount = array3[num20].fluidCount + num17;
					TankComponent[] array4 = tankPool;
					int num21 = tankId;
					array4[num21].fluidInc = array4[num21].fluidInc + num19;
					if (mainPlayer.inhandItemCount <= 0)
					{
						mainPlayer.SetHandItems(0, 0, 0);
					}
					this._tmp_items.Alter(tankPool[tankId].fluidId, num17);
				}
			}
		}
		if (entityData.minerId > 0)
		{
			MinerComponent minerComponent = this.factorySystem.minerPool[entityData.minerId];
			int productId = minerComponent.productId;
			if (productId == 0 && minerComponent.veinCount > 0)
			{
				productId = this.veinPool[minerComponent.veins[minerComponent.currentVeinIndex]].productId;
			}
			if (productId > 0 && minerComponent.insertTarget > 0)
			{
				int beltId = this.entityPool[minerComponent.insertTarget].beltId;
				if (beltId > 0)
				{
					this.BeltFastFillIn(beltId, 0, fromPackage, productId, 0);
				}
			}
		}
		int inserterId = entityData.inserterId;
		if (entityData.assemblerId > 0)
		{
			int assemblerId = entityData.assemblerId;
			AssemblerComponent[] assemblerPool = this.factorySystem.assemblerPool;
			if (assemblerPool[assemblerId].recipeId > 0)
			{
				int[] requires = assemblerPool[assemblerId].recipeExecuteData.requires;
				int[] requireCounts = assemblerPool[assemblerId].recipeExecuteData.requireCounts;
				int[] served = assemblerPool[assemblerId].served;
				int[] incServed = assemblerPool[assemblerId].incServed;
				for (int j = 0; j < requires.Length; j++)
				{
					int num22 = requires[j];
					int num23 = requireCounts[j] * ((assemblerPool[assemblerId].recipeType == ERecipeType.Smelt) ? 50 : 20) - served[j];
					int num24 = 0;
					if (num23 > 0)
					{
						mainPlayer.TakeItemFromPlayer(ref num22, ref num23, out num24, fromPackage, this._tmp_items);
					}
					if (num23 > 0)
					{
						served[j] += num23;
						incServed[j] += num24;
					}
				}
			}
		}
		if (entityData.fractionatorId > 0)
		{
			int fractionatorId = entityData.fractionatorId;
			FractionatorComponent fractionatorComponent = this.factorySystem.fractionatorPool[fractionatorId];
			int num25 = fractionatorComponent.fluidId;
			if (num25 == 0)
			{
				num25 = RecipeProto.fractionatorNeeds[0];
			}
			if (num25 > 0)
			{
				if (fractionatorComponent.belt1 > 0 && !fractionatorComponent.isOutput1)
				{
					this.BeltFastFillIn(fractionatorComponent.belt1, 0, fromPackage, num25, 0);
				}
				if (fractionatorComponent.belt2 > 0 && !fractionatorComponent.isOutput2)
				{
					this.BeltFastFillIn(fractionatorComponent.belt2, 0, fromPackage, num25, 0);
				}
			}
		}
		if (entityData.ejectorId > 0)
		{
			int ejectorId = entityData.ejectorId;
			EjectorComponent[] ejectorPool = this.factorySystem.ejectorPool;
			int bulletId = ejectorPool[ejectorId].bulletId;
			int num26 = 20 - ejectorPool[ejectorId].bulletCount;
			int num27 = 0;
			if (num26 > 0)
			{
				mainPlayer.TakeItemFromPlayer(ref bulletId, ref num26, out num27, fromPackage, this._tmp_items);
			}
			if (num26 > 0)
			{
				EjectorComponent[] array5 = ejectorPool;
				int num28 = ejectorId;
				array5[num28].bulletCount = array5[num28].bulletCount + num26;
				EjectorComponent[] array6 = ejectorPool;
				int num29 = ejectorId;
				array6[num29].bulletInc = array6[num29].bulletInc + num27;
			}
		}
		if (entityData.siloId > 0)
		{
			int siloId = entityData.siloId;
			SiloComponent[] siloPool = this.factorySystem.siloPool;
			int bulletId2 = siloPool[siloId].bulletId;
			int num30 = 20 - siloPool[siloId].bulletCount;
			int num31 = 0;
			if (num30 > 0)
			{
				mainPlayer.TakeItemFromPlayer(ref bulletId2, ref num30, out num31, fromPackage, this._tmp_items);
			}
			if (num30 > 0)
			{
				SiloComponent[] array7 = siloPool;
				int num32 = siloId;
				array7[num32].bulletCount = array7[num32].bulletCount + num30;
				SiloComponent[] array8 = siloPool;
				int num33 = siloId;
				array8[num33].bulletInc = array8[num33].bulletInc + num31;
			}
		}
		if (entityData.labId > 0)
		{
			int labId = entityData.labId;
			LabComponent[] labPool = this.factorySystem.labPool;
			if (labPool[labId].matrixMode)
			{
				int[] requires2 = labPool[labId].recipeExecuteData.requires;
				int[] served2 = labPool[labId].served;
				int[] incServed2 = labPool[labId].incServed;
				for (int k = 0; k < requires2.Length; k++)
				{
					int num34 = requires2[k];
					int num35 = 100 - served2[k];
					int num36 = 0;
					if (num35 > 0)
					{
						mainPlayer.TakeItemFromPlayer(ref num34, ref num35, out num36, fromPackage, this._tmp_items);
					}
					if (num35 > 0)
					{
						served2[k] += num35;
						incServed2[k] += num36;
					}
				}
			}
			else if (labPool[labId].researchMode)
			{
				int[] matrixIds = LabComponent.matrixIds;
				int[] matrixServed = labPool[labId].matrixServed;
				int[] matrixIncServed = labPool[labId].matrixIncServed;
				for (int l = 0; l < matrixIds.Length; l++)
				{
					int itemId = matrixIds[l];
					if (this.gameData.history.ItemUnlocked(itemId))
					{
						int num37 = 100 - matrixServed[l] / 3600;
						int num38 = 0;
						if (num37 > 0)
						{
							mainPlayer.TakeItemFromPlayer(ref itemId, ref num37, out num38, fromPackage, this._tmp_items);
						}
						if (num37 > 0)
						{
							matrixServed[l] += num37 * 3600;
							matrixIncServed[l] += num38 * 3600;
						}
					}
				}
			}
		}
		if (entityData.stationId > 0)
		{
			int stationId = entityData.stationId;
			StationComponent stationComponent = this.transport.stationPool[stationId];
			if (!fromPackage)
			{
				int num39 = LDB.models.Select((int)entityData.modelIndex).prefabDesc.stationMaxItemCount;
				int num40;
				if (stationComponent.isCollector)
				{
					num40 = this.gameData.history.localStationExtraStorage;
				}
				else if (stationComponent.isVeinCollector)
				{
					num40 = this.gameData.history.localStationExtraStorage;
				}
				else if (stationComponent.isStellar)
				{
					num40 = this.gameData.history.remoteStationExtraStorage;
				}
				else
				{
					num40 = this.gameData.history.localStationExtraStorage;
				}
				num39 += num40;
				int m = 0;
				while (m < stationComponent.storage.Length)
				{
					int itemId2 = stationComponent.storage[m].itemId;
					if (itemId2 > 0 && itemId2 == mainPlayer.inhandItemId)
					{
						int num41 = num39 - stationComponent.storage[m].count;
						int num42 = 0;
						if (num41 > 0)
						{
							mainPlayer.TakeItemFromPlayer(ref itemId2, ref num41, out num42, fromPackage, this._tmp_items);
						}
						if (itemId2 == stationComponent.storage[m].itemId && num41 > 0)
						{
							StationStore[] storage = stationComponent.storage;
							int num43 = m;
							storage[num43].count = storage[num43].count + num41;
							StationStore[] storage2 = stationComponent.storage;
							int num44 = m;
							storage2[num44].inc = storage2[num44].inc + num42;
							break;
						}
						break;
					}
					else
					{
						m++;
					}
				}
			}
			if (!stationComponent.isCollector && !stationComponent.isVeinCollector)
			{
				int num45 = 5001;
				int num46 = stationComponent.workDroneDatas.Length - stationComponent.workDroneCount - stationComponent.idleDroneCount;
				int num47 = 0;
				if (num46 > 0)
				{
					mainPlayer.TakeItemFromPlayer(ref num45, ref num46, out num47, fromPackage, this._tmp_items);
				}
				if (num46 > 0)
				{
					stationComponent.idleDroneCount += num46;
				}
			}
			if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector)
			{
				int num48 = 5002;
				int num49 = stationComponent.workShipDatas.Length - stationComponent.workShipCount - stationComponent.idleShipCount;
				int num50 = 0;
				if (num49 > 0)
				{
					mainPlayer.TakeItemFromPlayer(ref num48, ref num49, out num50, fromPackage, this._tmp_items);
				}
				if (num49 > 0)
				{
					stationComponent.idleShipCount += num49;
				}
			}
			if (stationComponent.isStellar && !stationComponent.isCollector && !stationComponent.isVeinCollector && this.gameData.history.logisticShipWarpDrive)
			{
				int num51 = 1210;
				int num52 = stationComponent.warperMaxCount - stationComponent.warperCount;
				int num53 = 0;
				if (num52 > 0)
				{
					mainPlayer.TakeItemFromPlayer(ref num51, ref num52, out num53, fromPackage, this._tmp_items);
				}
				if (num52 > 0)
				{
					stationComponent.warperCount += num52;
				}
			}
		}
		if (entityData.dispenserId > 0)
		{
			DispenserComponent dispenserComponent = this.transport.dispenserPool[entityData.dispenserId];
			int num54 = 5003;
			int num55 = dispenserComponent.workCourierDatas.Length - dispenserComponent.workCourierCount - dispenserComponent.idleCourierCount;
			int num56 = 0;
			if (num55 > 0)
			{
				mainPlayer.TakeItemFromPlayer(ref num54, ref num55, out num56, fromPackage, this._tmp_items);
			}
			if (num55 > 0)
			{
				dispenserComponent.idleCourierCount += num55;
			}
		}
		if (entityData.turretId > 0)
		{
			ref TurretComponent ptr = ref this.defenseSystem.turrets.buffer[entityData.turretId];
			int num57 = (int)ptr.itemId;
			int num58 = (int)(100 - ptr.itemCount);
			int num59 = 0;
			if (num57 == 0 && ptr.itemCount == 0 && ptr.bulletCount == 0)
			{
				int[] array9 = ItemProto.turretNeeds[(int)ptr.ammoType];
				if (array9 != null && array9.Length != 0)
				{
					if (fromPackage)
					{
						StorageComponent.GRID[] grids2 = mainPlayer.package.grids;
						for (int n = size - 1; n >= 0; n--)
						{
							int itemId3 = grids2[n].itemId;
							if (itemId3 > 0)
							{
								for (int num60 = 0; num60 < array9.Length; num60++)
								{
									if (array9[num60] == itemId3)
									{
										num57 = itemId3;
										break;
									}
								}
							}
							if (num57 > 0)
							{
								break;
							}
						}
					}
					else
					{
						int inhandItemId3 = mainPlayer.inhandItemId;
						if (inhandItemId3 > 0)
						{
							for (int num61 = 0; num61 < array9.Length; num61++)
							{
								if (array9[num61] == inhandItemId3)
								{
									num57 = inhandItemId3;
									break;
								}
							}
						}
					}
				}
			}
			if (num57 > 0 && num58 > 0)
			{
				if (!fromPackage)
				{
					num58 = 1;
				}
				else
				{
					num58 = ((num58 > 2) ? 2 : num58);
				}
				mainPlayer.TakeItemFromPlayer(ref num57, ref num58, out num59, fromPackage, this._tmp_items);
			}
			if (num57 > 0 && num58 > 0)
			{
				ptr.SetNewItem(num57, (short)((int)ptr.itemCount + num58), (short)((int)ptr.itemInc + num59));
			}
		}
		int powerNodeId = entityData.powerNodeId;
		if (entityData.powerGenId > 0)
		{
			int powerGenId = entityData.powerGenId;
			PowerGeneratorComponent[] genPool = this.powerSystem.genPool;
			if (genPool[powerGenId].fuelMask > 0)
			{
				int num62 = (int)genPool[powerGenId].fuelId;
				int num63 = (int)(100 - genPool[powerGenId].fuelCount);
				int num64 = 0;
				if (num62 == 0 && genPool[powerGenId].fuelCount == 0)
				{
					int[] array10 = ItemProto.fuelNeeds[(int)genPool[powerGenId].fuelMask];
					if (array10 != null && array10.Length != 0)
					{
						if (fromPackage)
						{
							StorageComponent.GRID[] grids3 = mainPlayer.package.grids;
							for (int num65 = size - 1; num65 >= 0; num65--)
							{
								int itemId4 = grids3[num65].itemId;
								if (itemId4 > 0)
								{
									for (int num66 = 0; num66 < array10.Length; num66++)
									{
										if (array10[num66] == itemId4)
										{
											num62 = itemId4;
											break;
										}
									}
								}
								if (num62 > 0)
								{
									break;
								}
							}
						}
						else
						{
							int inhandItemId4 = mainPlayer.inhandItemId;
							if (inhandItemId4 > 0)
							{
								for (int num67 = 0; num67 < array10.Length; num67++)
								{
									if (array10[num67] == inhandItemId4)
									{
										num62 = inhandItemId4;
										break;
									}
								}
							}
						}
					}
				}
				if (num62 > 0 && num63 > 0)
				{
					if (!fromPackage)
					{
						num63 = 1;
					}
					else
					{
						num63 = ((num63 > 2) ? 2 : num63);
					}
					mainPlayer.TakeItemFromPlayer(ref num62, ref num63, out num64, fromPackage, this._tmp_items);
				}
				if (num62 > 0 && num63 > 0)
				{
					if (genPool[powerGenId].fuelId == 0)
					{
						genPool[powerGenId].SetNewFuel(num62, (short)num63, (short)num64);
					}
					else
					{
						ItemProto itemProto = LDB.items.Select(num62);
						if (itemProto != null && itemProto.HeatValue > 0L)
						{
							genPool[powerGenId].fuelId = (short)num62;
							PowerGeneratorComponent[] array11 = genPool;
							int num68 = powerGenId;
							array11[num68].fuelCount = array11[num68].fuelCount + (short)num63;
							PowerGeneratorComponent[] array12 = genPool;
							int num69 = powerGenId;
							array12[num69].fuelInc = array12[num69].fuelInc + (short)num64;
							genPool[powerGenId].fuelHeat = itemProto.HeatValue;
						}
					}
				}
			}
			else if (genPool[powerGenId].gamma)
			{
				int catalystId = genPool[powerGenId].catalystId;
				ItemProto itemProto2 = LDB.items.Select(catalystId);
				if (itemProto2 != null && this.gameData.history.useIonLayer)
				{
					int num70 = itemProto2.StackSize - genPool[powerGenId].catalystPoint / 3600;
					int num71 = 0;
					if (catalystId > 0 && num70 > 0)
					{
						mainPlayer.TakeItemFromPlayer(ref catalystId, ref num70, out num71, fromPackage, this._tmp_items);
					}
					if (catalystId > 0 && num70 > 0)
					{
						PowerGeneratorComponent[] array13 = genPool;
						int num72 = powerGenId;
						array13[num72].catalystPoint = array13[num72].catalystPoint + num70 * 3600;
						PowerGeneratorComponent[] array14 = genPool;
						int num73 = powerGenId;
						array14[num73].catalystIncPoint = array14[num73].catalystIncPoint + num71 * 3600;
					}
				}
			}
		}
		int powerConId = entityData.powerConId;
		int powerAccId = entityData.powerAccId;
		if (entityData.powerExcId > 0)
		{
			int powerExcId = entityData.powerExcId;
			PowerExchangerComponent[] excPool = this.powerSystem.excPool;
			if (excPool[powerExcId].state > 0f)
			{
				int emptyId = excPool[powerExcId].emptyId;
				int num74 = (int)(20 - excPool[powerExcId].emptyCount);
				int num75 = 0;
				if (num74 > 0)
				{
					mainPlayer.TakeItemFromPlayer(ref emptyId, ref num74, out num75, fromPackage, this._tmp_items);
				}
				if (num74 > 0)
				{
					PowerExchangerComponent[] array15 = excPool;
					int num76 = powerExcId;
					array15[num76].emptyCount = array15[num76].emptyCount + (short)num74;
					PowerExchangerComponent[] array16 = excPool;
					int num77 = powerExcId;
					array16[num77].emptyInc = array16[num77].emptyInc + (short)num75;
				}
			}
			else if (excPool[powerExcId].state < 0f)
			{
				int fullId = excPool[powerExcId].fullId;
				int num78 = (int)(20 - excPool[powerExcId].fullCount);
				int num79 = 0;
				if (num78 > 0)
				{
					mainPlayer.TakeItemFromPlayer(ref fullId, ref num78, out num79, fromPackage, this._tmp_items);
				}
				if (num78 > 0)
				{
					PowerExchangerComponent[] array17 = excPool;
					int num80 = powerExcId;
					array17[num80].fullCount = array17[num80].fullCount + (short)num78;
					PowerExchangerComponent[] array18 = excPool;
					int num81 = powerExcId;
					array18[num81].fullInc = array18[num81].fullInc + (short)num79;
				}
			}
		}
		if (entityData.spraycoaterId > 0)
		{
			int spraycoaterId = entityData.spraycoaterId;
			SpraycoaterComponent[] spraycoaterPool = this.cargoTraffic.spraycoaterPool;
			int[] incItemId = LDB.models.Select((int)entityData.modelIndex).prefabDesc.incItemId;
			if (spraycoaterPool[spraycoaterId].incCount <= 0)
			{
				spraycoaterPool[spraycoaterId].incItemId = 0;
				spraycoaterPool[spraycoaterId].incCount = 0;
				spraycoaterPool[spraycoaterId].incAbility = 0;
				spraycoaterPool[spraycoaterId].extraIncCount = 0;
			}
			if (fromPackage)
			{
				StorageComponent.GRID[] grids4 = mainPlayer.package.grids;
				int num82 = spraycoaterPool[spraycoaterId].incItemId;
				if (num82 == 0)
				{
					for (int num83 = incItemId.Length - 1; num83 >= 0; num83--)
					{
						int num84 = incItemId[num83];
						if (num84 != 0)
						{
							for (int num85 = size - 1; num85 >= 0; num85--)
							{
								if (grids4[num85].itemId == num84)
								{
									num82 = num84;
									break;
								}
							}
							if (num82 > 0)
							{
								break;
							}
						}
					}
				}
				if (num82 > 0)
				{
					ItemProto itemProto3 = LDB.items.Select(num82);
					if (itemProto3 != null)
					{
						int ability = itemProto3.Ability;
						int hpMax = itemProto3.HpMax;
						int num86 = (spraycoaterPool[spraycoaterId].incCapacity - spraycoaterPool[spraycoaterId].incCount - spraycoaterPool[spraycoaterId].extraIncCount > 0) ? 1 : 0;
						int num87 = 0;
						if (num86 > 0)
						{
							mainPlayer.TakeItemFromPlayer(ref num82, ref num86, out num87, fromPackage, this._tmp_items);
						}
						if (num82 > 0 && num86 > 0)
						{
							num87 = ((num87 > 10) ? 10 : num87);
							spraycoaterPool[spraycoaterId].incItemId = num82;
							spraycoaterPool[spraycoaterId].incAbility = ability;
							spraycoaterPool[spraycoaterId].incSprayTimes = hpMax;
							SpraycoaterComponent[] array19 = spraycoaterPool;
							int num88 = spraycoaterId;
							array19[num88].incCount = array19[num88].incCount + num86 * hpMax;
							SpraycoaterComponent[] array20 = spraycoaterPool;
							int num89 = spraycoaterId;
							array20[num89].extraIncCount = array20[num89].extraIncCount + (int)((double)hpMax * ((double)Cargo.incTable[num87] * 0.001));
						}
					}
				}
			}
			else
			{
				bool flag3 = false;
				int num90 = spraycoaterPool[spraycoaterId].incItemId;
				int num91 = 0;
				int num92 = 0;
				if (num90 == 0)
				{
					for (int num93 = incItemId.Length - 1; num93 >= 0; num93--)
					{
						int num94 = incItemId[num93];
						if (num94 != 0)
						{
							if (mainPlayer.inhandItemId == num94)
							{
								num90 = num94;
								break;
							}
							if (num90 > 0)
							{
								break;
							}
						}
					}
				}
				if (num90 > 0)
				{
					num91 = ((spraycoaterPool[spraycoaterId].incCapacity - spraycoaterPool[spraycoaterId].incCount - spraycoaterPool[spraycoaterId].extraIncCount > 0) ? 1 : 0);
				}
				if (num91 > 0 && mainPlayer.inhandItemId == num90)
				{
					flag3 = true;
				}
				if (flag3)
				{
					ItemProto itemProto4 = LDB.items.Select(num90);
					if (itemProto4 != null)
					{
						int ability2 = itemProto4.Ability;
						int hpMax2 = itemProto4.HpMax;
						mainPlayer.TakeItemFromPlayer(ref num90, ref num91, out num92, fromPackage, this._tmp_items);
						if (num90 > 0 && num91 > 0)
						{
							num92 = ((num92 > 10) ? 10 : num92);
							spraycoaterPool[spraycoaterId].incItemId = num90;
							spraycoaterPool[spraycoaterId].incAbility = ability2;
							spraycoaterPool[spraycoaterId].incSprayTimes = hpMax2;
							SpraycoaterComponent[] array21 = spraycoaterPool;
							int num95 = spraycoaterId;
							array21[num95].incCount = array21[num95].incCount + num91 * hpMax2;
							SpraycoaterComponent[] array22 = spraycoaterPool;
							int num96 = spraycoaterId;
							array22[num96].extraIncCount = array22[num96].extraIncCount + (int)((double)hpMax2 * ((double)Cargo.incTable[num92] * 0.001));
						}
					}
				}
				else
				{
					BeltComponent[] beltPool = this.cargoTraffic.beltPool;
					int cargoBeltId = this.cargoTraffic.spraycoaterPool[spraycoaterId].cargoBeltId;
					if (cargoBeltId > 0)
					{
						CargoPath cargoPath = this.cargoTraffic.GetCargoPath(beltPool[cargoBeltId].segPathId);
						this._tmp_itemIds.Clear();
						for (int num97 = 0; num97 < cargoPath.pathLength; num97 += 10)
						{
							byte add_or_sub;
							byte b;
							int num98 = cargoPath.QueryItemAtIndex(num97, out add_or_sub, out b);
							if (num98 > 0)
							{
								this._tmp_itemIds.Alter(num98, (int)add_or_sub);
							}
						}
						if (this._tmp_itemIds.items.Count == 0)
						{
							for (int num99 = 0; num99 < cargoPath.belts.Count; num99++)
							{
								int entityId2 = beltPool[cargoPath.belts[num99]].entityId;
								if (this.entitySignPool[entityId2].iconId0 > 0U)
								{
									this._tmp_itemIds.Alter((int)this.entitySignPool[entityId2].iconId0, 1);
								}
							}
						}
						int inhandItemId5 = mainPlayer.inhandItemId;
						if (inhandItemId5 != 0 && mainPlayer.inhandItemCount > 0 && this._tmp_itemIds.items.ContainsKey(inhandItemId5))
						{
							num92 = this._test_take_player_inhand_inc(1);
							if (this.cargoTraffic.TryInsertItem(cargoBeltId, 0, inhandItemId5, 1, (byte)num92))
							{
								this._do_remove_player_inhand_item(1);
								this._tmp_items.Alter(inhandItemId5, 1);
							}
						}
					}
				}
			}
		}
		if (entityData.pilerId > 0)
		{
			fromPackage = false;
			int pilerId = entityData.pilerId;
			int inputBeltId = this.cargoTraffic.pilerPool[pilerId].inputBeltId;
			if (inputBeltId > 0)
			{
				this.BeltFastFillIn(inputBeltId, 0, fromPackage, 0, 0);
			}
		}
	}

	// Token: 0x0600190C RID: 6412 RVA: 0x001B4700 File Offset: 0x001B2900
	private void BeltFastFillIn(int beltId, int offset, bool fromPackage, int filterId, int blackId)
	{
		if (beltId == 0)
		{
			return;
		}
		Player mainPlayer = this.gameData.mainPlayer;
		StorageComponent package = mainPlayer.package;
		int size = package.size;
		BeltComponent[] beltPool = this.cargoTraffic.beltPool;
		if (beltPool[beltId].id != 0 && beltPool[beltId].id == beltId)
		{
			CargoPath cargoPath = this.cargoTraffic.GetCargoPath(beltPool[beltId].segPathId);
			int num = beltPool[beltId].segIndex + beltPool[beltId].segPivotOffset;
			if (num + offset < 0)
			{
				offset = 0 - num;
			}
			else if (num + offset > cargoPath.pathLength)
			{
				offset = cargoPath.pathLength - num - 1;
			}
			if (fromPackage)
			{
				this._tmp_itemIds.Clear();
				if (filterId > 0)
				{
					this._tmp_itemIds.Alter(filterId, 1);
				}
				else
				{
					for (int i = 0; i < cargoPath.pathLength; i += 10)
					{
						byte add_or_sub;
						byte b;
						int num2 = cargoPath.QueryItemAtIndex(i, out add_or_sub, out b);
						if (num2 > 0)
						{
							this._tmp_itemIds.Alter(num2, (int)add_or_sub);
						}
					}
					if (this._tmp_itemIds.items.Count == 0)
					{
						for (int j = 0; j < cargoPath.belts.Count; j++)
						{
							int entityId = beltPool[cargoPath.belts[j]].entityId;
							if (this.entitySignPool[entityId].iconId0 > 0U)
							{
								this._tmp_itemIds.Alter((int)this.entitySignPool[entityId].iconId0, 1);
							}
						}
					}
				}
				this._tmp_itemIds.items.Remove(blackId);
				if (this._tmp_itemIds.items.Count > 0)
				{
					Array.Copy(package.grids, this._tmp_package.grids, size);
					for (int k = size - 1; k >= 0; k--)
					{
						int num3 = 0;
						int num4 = 1;
						int num5 = 0;
						this._tmp_package.TakeItemFromGrid(k, ref num3, ref num4, out num5);
						if (num3 > 0 && num4 > 0 && this._tmp_itemIds.items.ContainsKey(num3) && this.cargoTraffic.TryInsertItem(beltId, offset, num3, (byte)num4, (byte)num5))
						{
							package.TakeItemFromGrid(k, ref num3, ref num4, out num5);
							this._tmp_items.Alter(num3, num4);
							return;
						}
					}
					return;
				}
			}
			else if (mainPlayer.inhandItemId > 0 && mainPlayer.inhandItemCount > 0)
			{
				int inhandItemId = mainPlayer.inhandItemId;
				if ((filterId == 0 || filterId == inhandItemId) && blackId != inhandItemId)
				{
					int num6 = this._test_take_player_inhand_inc(1);
					if (this.cargoTraffic.TryInsertItem(beltId, offset, inhandItemId, 1, (byte)num6))
					{
						this._do_remove_player_inhand_item(1);
						this._tmp_items.Alter(inhandItemId, 1);
					}
				}
			}
		}
	}

	// Token: 0x0600190D RID: 6413 RVA: 0x001B49B8 File Offset: 0x001B2BB8
	public void EntityAutoReplenishIfNeeded(int entityId, Vector2 tipUiPos, bool tipOnMouse = false)
	{
		if (this.gameData.guideRunning)
		{
			return;
		}
		Player mainPlayer = this.gameData.mainPlayer;
		string text = "";
		bool flag = false;
		ref EntityData ptr = ref this.entityPool[entityId];
		Vector3 vector = ptr.pos;
		if (ptr.dispenserId > 0)
		{
			DispenserComponent dispenserComponent = this.transport.dispenserPool[ptr.dispenserId];
			if (dispenserComponent == null || !dispenserComponent.courierAutoReplenish)
			{
				return;
			}
			vector += vector.normalized * 1f;
			int num = dispenserComponent.workCourierDatas.Length - (dispenserComponent.idleCourierCount + dispenserComponent.workCourierCount);
			if (num > 0)
			{
				int itemCount = mainPlayer.package.GetItemCount(5003);
				if (itemCount > 0)
				{
					int num2 = (itemCount < num) ? itemCount : num;
					int num3 = 5003;
					int num4 = num2;
					int num5 = 0;
					mainPlayer.package.TakeTailItems(ref num3, ref num4, out num5, false);
					if (num3 > 0 && num4 > 0)
					{
						dispenserComponent.idleCourierCount += num4;
						text += string.Format("已自动补充提示".Translate(), num4, LDB.items.Select(num3).name);
						if (num4 < num)
						{
							text += "自动补充物品不足0".Translate();
						}
						text += "\r\n";
						flag = true;
					}
				}
				else
				{
					text = text + string.Format("自动补充物品不足1".Translate(), LDB.items.Select(5003).name) + "\r\n";
				}
			}
		}
		if (ptr.stationId > 0)
		{
			StationComponent stationComponent = this.transport.stationPool[ptr.stationId];
			if (stationComponent == null || stationComponent.isCollector || stationComponent.isVeinCollector)
			{
				return;
			}
			vector += vector.normalized * 13f;
			if (stationComponent.droneAutoReplenish)
			{
				int num6 = stationComponent.workDroneDatas.Length - (stationComponent.idleDroneCount + stationComponent.workDroneCount);
				if (num6 > 0)
				{
					int itemCount2 = mainPlayer.package.GetItemCount(5001);
					if (itemCount2 > 0)
					{
						int num7 = (itemCount2 < num6) ? itemCount2 : num6;
						int num8 = 5001;
						int num9 = num7;
						int num10 = 0;
						mainPlayer.package.TakeTailItems(ref num8, ref num9, out num10, false);
						if (num8 > 0 && num9 > 0)
						{
							stationComponent.idleDroneCount += num9;
							text += string.Format("已自动补充提示".Translate(), num9, LDB.items.Select(num8).name);
							if (num9 < num6)
							{
								text += "自动补充物品不足0".Translate();
							}
							text += "\r\n";
							flag = true;
						}
					}
					else
					{
						text = text + string.Format("自动补充物品不足1".Translate(), LDB.items.Select(5001).name) + "\r\n";
					}
				}
			}
			if (stationComponent.isStellar && stationComponent.shipAutoReplenish)
			{
				int num11 = stationComponent.workShipDatas.Length - (stationComponent.idleShipCount + stationComponent.workShipCount);
				if (num11 > 0)
				{
					int itemCount3 = mainPlayer.package.GetItemCount(5002);
					if (itemCount3 > 0)
					{
						int num12 = (itemCount3 < num11) ? itemCount3 : num11;
						int num13 = 5002;
						int num14 = num12;
						int num15 = 0;
						mainPlayer.package.TakeTailItems(ref num13, ref num14, out num15, false);
						if (num13 > 0 && num14 > 0)
						{
							stationComponent.idleShipCount += num14;
							text += string.Format("已自动补充提示".Translate(), num14, LDB.items.Select(num13).name);
							if (num14 < num11)
							{
								text += "自动补充物品不足0".Translate();
							}
							text += "\r\n";
							flag = true;
						}
					}
					else
					{
						text = text + string.Format("自动补充物品不足1".Translate(), LDB.items.Select(5002).name) + "\r\n";
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (tipOnMouse)
			{
				UIRealtimeTip.Popup(text, false, 0);
			}
			else if (tipUiPos.sqrMagnitude < 0.1f)
			{
				UIRealtimeTip.Popup(text, vector, false, 0);
			}
			else
			{
				UIRealtimeTip.Popup(text, tipUiPos, false);
			}
			if (flag)
			{
				VFAudio.Create("equip-1", mainPlayer.transform, Vector3.zero, true, 4, -1, -1L);
			}
		}
	}

	// Token: 0x0600190E RID: 6414 RVA: 0x001B4E30 File Offset: 0x001B3030
	public void StationAutoReplenishIfNeeded(int entityId, Vector2 tipUiPos, bool isDrone, bool tipOnMouse = false)
	{
		if (this.gameData.guideRunning)
		{
			return;
		}
		Player mainPlayer = this.gameData.mainPlayer;
		string text = "";
		bool flag = false;
		ref EntityData ptr = ref this.entityPool[entityId];
		Vector3 vector = ptr.pos;
		if (ptr.stationId > 0)
		{
			StationComponent stationComponent = this.transport.stationPool[ptr.stationId];
			if (stationComponent == null || stationComponent.isCollector || stationComponent.isVeinCollector)
			{
				return;
			}
			vector += vector.normalized * 13f;
			if (isDrone && stationComponent.droneAutoReplenish)
			{
				int num = stationComponent.workDroneDatas.Length - (stationComponent.idleDroneCount + stationComponent.workDroneCount);
				if (num > 0)
				{
					int itemCount = mainPlayer.package.GetItemCount(5001);
					if (itemCount > 0)
					{
						int num2 = (itemCount < num) ? itemCount : num;
						int num3 = 5001;
						int num4 = num2;
						int num5 = 0;
						mainPlayer.package.TakeTailItems(ref num3, ref num4, out num5, false);
						if (num3 > 0 && num4 > 0)
						{
							stationComponent.idleDroneCount += num4;
							text += string.Format("已自动补充提示".Translate(), num4, LDB.items.Select(num3).name);
							if (num4 < num)
							{
								text += "自动补充物品不足0".Translate();
							}
							text += "\r\n";
							flag = true;
						}
					}
					else
					{
						text = text + string.Format("自动补充物品不足1".Translate(), LDB.items.Select(5001).name) + "\r\n";
					}
				}
			}
			if (!isDrone && stationComponent.isStellar && stationComponent.shipAutoReplenish)
			{
				int num6 = stationComponent.workShipDatas.Length - (stationComponent.idleShipCount + stationComponent.workShipCount);
				if (num6 > 0)
				{
					int itemCount2 = mainPlayer.package.GetItemCount(5002);
					if (itemCount2 > 0)
					{
						int num7 = (itemCount2 < num6) ? itemCount2 : num6;
						int num8 = 5002;
						int num9 = num7;
						int num10 = 0;
						mainPlayer.package.TakeTailItems(ref num8, ref num9, out num10, false);
						if (num8 > 0 && num9 > 0)
						{
							stationComponent.idleShipCount += num9;
							text += string.Format("已自动补充提示".Translate(), num9, LDB.items.Select(num8).name);
							if (num9 < num6)
							{
								text += "自动补充物品不足0".Translate();
							}
							text += "\r\n";
							flag = true;
						}
					}
					else
					{
						text = text + string.Format("自动补充物品不足1".Translate(), LDB.items.Select(5002).name) + "\r\n";
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (tipOnMouse)
			{
				UIRealtimeTip.Popup(text, false, 0);
			}
			else if (tipUiPos.sqrMagnitude < 0.1f)
			{
				UIRealtimeTip.Popup(text, vector, false, 0);
			}
			else
			{
				UIRealtimeTip.Popup(text, tipUiPos, false);
			}
			if (flag)
			{
				VFAudio.Create("equip-1", mainPlayer.transform, Vector3.zero, true, 4, -1, -1L);
			}
		}
	}

	// Token: 0x0600190F RID: 6415 RVA: 0x001B5164 File Offset: 0x001B3364
	private int _test_take_player_inhand_inc(int take_cnt)
	{
		Player mainPlayer = this.gameData.mainPlayer;
		int inhandItemCount = mainPlayer.inhandItemCount;
		int inhandItemInc = mainPlayer.inhandItemInc;
		if (inhandItemCount == 0)
		{
			return 0;
		}
		return this.split_inc(ref inhandItemCount, ref inhandItemInc, take_cnt);
	}

	// Token: 0x06001910 RID: 6416 RVA: 0x001B519C File Offset: 0x001B339C
	private void _do_remove_player_inhand_item(int take_cnt)
	{
		Player mainPlayer = this.gameData.mainPlayer;
		if (mainPlayer.inhandItemCount > take_cnt)
		{
			int inhandItemCount = mainPlayer.inhandItemCount;
			int inhandItemInc = mainPlayer.inhandItemInc;
			int num = this.split_inc(ref inhandItemCount, ref inhandItemInc, take_cnt);
			mainPlayer.AddHandItemCount_Unsafe(-take_cnt);
			mainPlayer.AddHandItemInc_Unsafe(-num);
			if (mainPlayer.inhandItemCount <= 0)
			{
				mainPlayer.SetHandItemId_Unsafe(0);
				mainPlayer.SetHandItemCount_Unsafe(0);
				mainPlayer.SetHandItemInc_Unsafe(0);
				return;
			}
		}
		else
		{
			mainPlayer.SetHandItemId_Unsafe(0);
			mainPlayer.SetHandItemCount_Unsafe(0);
			mainPlayer.SetHandItemInc_Unsafe(0);
		}
	}

	// Token: 0x04001DB6 RID: 7606
	public SpaceSector sector;

	// Token: 0x04001DB7 RID: 7607
	public SkillSystem skillSystem;

	// Token: 0x04001DB8 RID: 7608
	public bool landed;

	// Token: 0x04001DB9 RID: 7609
	public EntityData[] entityPool;

	// Token: 0x04001DBA RID: 7610
	public AnimData[] entityAnimPool;

	// Token: 0x04001DBB RID: 7611
	public SignData[] entitySignPool;

	// Token: 0x04001DBC RID: 7612
	public int[] entityConnPool;

	// Token: 0x04001DBD RID: 7613
	public Mutex[] entityMutexs;

	// Token: 0x04001DBE RID: 7614
	public int[][] entityNeeds;

	// Token: 0x04001DBF RID: 7615
	public int entityCursor = 1;

	// Token: 0x04001DC0 RID: 7616
	private int entityCapacity;

	// Token: 0x04001DC1 RID: 7617
	private int[] entityRecycle;

	// Token: 0x04001DC2 RID: 7618
	private int entityRecycleCursor;

	// Token: 0x04001DC3 RID: 7619
	public const int kMaxBuildingConn = 16;

	// Token: 0x04001DC4 RID: 7620
	public const int kAddonSlot = 13;

	// Token: 0x04001DC5 RID: 7621
	public const int kMultiLevelInputSlot = 14;

	// Token: 0x04001DC6 RID: 7622
	public const int kMultiLevelOutputSlot = 15;

	// Token: 0x04001DC7 RID: 7623
	public PrebuildData[] prebuildPool;

	// Token: 0x04001DC8 RID: 7624
	public int[] prebuildConnPool;

	// Token: 0x04001DC9 RID: 7625
	public int prebuildCursor = 1;

	// Token: 0x04001DCA RID: 7626
	private int prebuildCapacity;

	// Token: 0x04001DCB RID: 7627
	private int[] prebuildRecycle;

	// Token: 0x04001DCC RID: 7628
	private int prebuildRecycleCursor;

	// Token: 0x04001DCE RID: 7630
	public CraftData[] craftPool;

	// Token: 0x04001DCF RID: 7631
	public AnimData[] craftAnimPool;

	// Token: 0x04001DD0 RID: 7632
	public int craftCursor = 1;

	// Token: 0x04001DD1 RID: 7633
	private int craftCapacity;

	// Token: 0x04001DD2 RID: 7634
	private int[] craftRecycle;

	// Token: 0x04001DD3 RID: 7635
	private int craftRecycleCursor;

	// Token: 0x04001DD4 RID: 7636
	public EnemyData[] enemyPool;

	// Token: 0x04001DD5 RID: 7637
	public AnimData[] enemyAnimPool;

	// Token: 0x04001DD6 RID: 7638
	public int enemyCursor = 1;

	// Token: 0x04001DD7 RID: 7639
	private int enemyCapacity;

	// Token: 0x04001DD8 RID: 7640
	private int[] enemyRecycle;

	// Token: 0x04001DD9 RID: 7641
	private int enemyRecycleCursor;

	// Token: 0x04001DDA RID: 7642
	public VeinData[] veinPool;

	// Token: 0x04001DDB RID: 7643
	public AnimData[] veinAnimPool;

	// Token: 0x04001DDC RID: 7644
	public int veinCursor = 1;

	// Token: 0x04001DDD RID: 7645
	private int veinCapacity;

	// Token: 0x04001DDE RID: 7646
	private int[] veinRecycle;

	// Token: 0x04001DDF RID: 7647
	private int veinRecycleCursor;

	// Token: 0x04001DE0 RID: 7648
	public VeinGroup[] veinGroups;

	// Token: 0x04001DE1 RID: 7649
	private int _miningFlag;

	// Token: 0x04001DE2 RID: 7650
	private int _veinMiningFlag;

	// Token: 0x04001DE3 RID: 7651
	public VegeData[] vegePool;

	// Token: 0x04001DE4 RID: 7652
	public int vegeCursor = 1;

	// Token: 0x04001DE5 RID: 7653
	private int vegeCapacity;

	// Token: 0x04001DE6 RID: 7654
	private int[] vegeRecycle;

	// Token: 0x04001DE7 RID: 7655
	private int vegeRecycleCursor;

	// Token: 0x04001DE8 RID: 7656
	public RuinData[] ruinPool;

	// Token: 0x04001DE9 RID: 7657
	public int ruinCursor = 1;

	// Token: 0x04001DEA RID: 7658
	private int ruinCapacity;

	// Token: 0x04001DEB RID: 7659
	private int[] ruinRecycle;

	// Token: 0x04001DEC RID: 7660
	private int ruinRecycleCursor;

	// Token: 0x04001DED RID: 7661
	public HashSystem hashSystemDynamic;

	// Token: 0x04001DEE RID: 7662
	public HashSystem hashSystemStatic;

	// Token: 0x04001DEF RID: 7663
	public DFSDynamicHashSystem spaceHashSystemDynamic;

	// Token: 0x04001DF0 RID: 7664
	public CargoContainer cargoContainer;

	// Token: 0x04001DF1 RID: 7665
	public CargoTraffic cargoTraffic;

	// Token: 0x04001DF2 RID: 7666
	public MiniBlockContainer blockContainer;

	// Token: 0x04001DF3 RID: 7667
	public FactoryStorage factoryStorage;

	// Token: 0x04001DF4 RID: 7668
	public PowerSystem powerSystem;

	// Token: 0x04001DF5 RID: 7669
	public ConstructionSystem constructionSystem;

	// Token: 0x04001DF6 RID: 7670
	public FactorySystem factorySystem;

	// Token: 0x04001DF7 RID: 7671
	public EnemyDFGroundSystem enemySystem;

	// Token: 0x04001DF8 RID: 7672
	public CombatGroundSystem combatGroundSystem;

	// Token: 0x04001DF9 RID: 7673
	public DefenseSystem defenseSystem;

	// Token: 0x04001DFA RID: 7674
	public PlanetATField planetATField;

	// Token: 0x04001DFB RID: 7675
	public PlanetTransport transport;

	// Token: 0x04001DFC RID: 7676
	public PlatformSystem platformSystem;

	// Token: 0x04001DFD RID: 7677
	public DigitalSystem digitalSystem;

	// Token: 0x04001DFE RID: 7678
	public static PrefabDesc[] PrefabDescByModelIndex;

	// Token: 0x04001DFF RID: 7679
	private HashSet<int> _rmv_id_list;

	// Token: 0x04001E00 RID: 7680
	[TupleElementNames(new string[]
	{
		"prototype",
		"protoId",
		"ownerId",
		"port",
		"pos",
		"rot",
		"vel"
	})]
	private HashSet<ValueTuple<ECraftProto, int, int, int, Vector3, Quaternion, Vector3>> _craft_add_id_list_0;

	// Token: 0x04001E01 RID: 7681
	[TupleElementNames(new string[]
	{
		"modelIndex",
		"ownerId",
		"pos",
		"rot",
		"vel"
	})]
	private HashSet<ValueTuple<int, int, Vector3, Quaternion, Vector3>> _craft_add_id_list_1;

	// Token: 0x04001E03 RID: 7683
	private List<int> tmp_vein_group_merge;

	// Token: 0x04001E04 RID: 7684
	private bool[] tmp_vein_group_flags;

	// Token: 0x04001E05 RID: 7685
	private int[] tmp_vein_group_idx_mapping;

	// Token: 0x04001E17 RID: 7703
	public static bool batchBuild;

	// Token: 0x04001E18 RID: 7704
	private int[] Tmp_activeBuckets = new int[HashSystem.bucketCount];

	// Token: 0x04001E19 RID: 7705
	private Collider[] _tmp_cols = new Collider[512];

	// Token: 0x04001E1A RID: 7706
	private Dictionary<int, int> tmp_levelChanges;

	// Token: 0x04001E1B RID: 7707
	private int[] tmp_ids;

	// Token: 0x04001E1C RID: 7708
	private int[] tmp_entity_ids;

	// Token: 0x04001E1D RID: 7709
	public static HashSet<int> tmpRemovableDFGBaseRuinId;

	// Token: 0x04001E1E RID: 7710
	public const float VEIN_BURIED_OFS = 50f;

	// Token: 0x04001E1F RID: 7711
	private StorageComponent _tmp_package;

	// Token: 0x04001E20 RID: 7712
	private ItemBundle _tmp_items;

	// Token: 0x04001E21 RID: 7713
	private ItemBundle _tmp_itemIds;
}
