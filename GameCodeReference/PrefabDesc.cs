using System;
using UnityEngine;

// Token: 0x0200023F RID: 575
public class PrefabDesc
{
	// Token: 0x06001953 RID: 6483 RVA: 0x001BB2FC File Offset: 0x001B94FC
	public PrefabDesc()
	{
		this.hasObject = false;
		this.modelIndex = 0;
		this.independentCollider = false;
	}

	// Token: 0x06001954 RID: 6484 RVA: 0x001BB374 File Offset: 0x001B9574
	public PrefabDesc(int _modelIndex, GameObject _prefab)
	{
		this.hasObject = false;
		this.modelIndex = _modelIndex;
		this.independentCollider = false;
		this.ReadPrefab(_prefab, _prefab);
	}

	// Token: 0x06001955 RID: 6485 RVA: 0x001BB3F4 File Offset: 0x001B95F4
	public PrefabDesc(int _modelIndex, GameObject _prefab, GameObject _colliderPrefab)
	{
		this.hasObject = false;
		this.modelIndex = _modelIndex;
		this.independentCollider = true;
		this.ReadPrefab(_prefab, _colliderPrefab);
	}

	// Token: 0x06001956 RID: 6486 RVA: 0x001BB474 File Offset: 0x001B9674
	public void Free()
	{
		int num = 0;
		while (this.lodMeshes != null && num < this.lodMeshes.Length)
		{
			if (this.lodMeshes[num] != null)
			{
				Object.Destroy(this.lodMeshes[num]);
			}
			num++;
		}
		int num2 = 0;
		while (this.lodVertas != null && num2 < this.lodVertas.Length)
		{
			if (this.lodVertas[num2] != null)
			{
				this.lodVertas[num2].Free();
			}
			num2++;
		}
		if (this.starmapVerta != null)
		{
			this.starmapVerta.Free();
		}
	}

	// Token: 0x06001957 RID: 6487 RVA: 0x001BB500 File Offset: 0x001B9700
	private void ReadPrefab(GameObject _prefab, GameObject _colliderPrefab)
	{
		ItemProto[] dataArray = LDB.items.dataArray;
		for (int i = 0; i < dataArray.Length; i++)
		{
			int num = dataArray[i].ModelIndex;
			if (num != 0)
			{
				int num2 = dataArray[i].ModelCount;
				if (num2 < 1)
				{
					num2 = 1;
				}
				if (num <= this.modelIndex && this.modelIndex < num + num2)
				{
					this.subId = dataArray[i].SubID;
					break;
				}
			}
		}
		this.prefab = _prefab;
		this.colliderPrefab = _colliderPrefab;
		this.hasObject = (this.prefab != null);
		MeshFilter component = this.prefab.GetComponent<MeshFilter>();
		if (component != null)
		{
			this.mesh = component.sharedMesh;
		}
		Renderer componentInChildren = this.prefab.GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			this.materials = componentInChildren.sharedMaterials;
		}
		MeshFilter[] componentsInChildren = this.prefab.GetComponentsInChildren<MeshFilter>(true);
		this.meshes = new Mesh[componentsInChildren.Length];
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			this.meshes[j] = componentsInChildren[j].sharedMesh;
		}
		if (this.meshes.Length != 0 && this.mesh == null)
		{
			this.mesh = this.meshes[0];
		}
		this.roughRadius = 0.7f;
		this.roughHeight = 0.7f;
		this.roughWidth = 1.4f;
		EnemyDesc componentInChildren2 = this.prefab.GetComponentInChildren<EnemyDesc>(true);
		if (componentInChildren2 != null)
		{
			this.roughRadius = componentInChildren2.roughRadius;
			this.roughHeight = componentInChildren2.roughRadius;
			this.roughWidth = componentInChildren2.roughRadius * 2f;
		}
		CraftDesc componentInChildren3 = this.prefab.GetComponentInChildren<CraftDesc>(true);
		if (componentInChildren3 != null)
		{
			this.roughRadius = componentInChildren3.roughRadius;
			this.roughHeight = componentInChildren3.roughRadius;
			this.roughWidth = componentInChildren3.roughRadius * 2f;
		}
		Collider[] componentsInChildren2 = this.colliderPrefab.GetComponentsInChildren<Collider>(true);
		this.colliders = new ColliderData[componentsInChildren2.Length];
		this.hasBuildCollider = false;
		this.buildCollider = default(ColliderData);
		int num3 = 0;
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			this.colliders[k].InitFromCollider(componentsInChildren2[k], this.independentCollider);
			if (this.colliders[k].usage == EColliderUsage.Build && this.colliders[k].shape == EColliderShape.Box)
			{
				this.buildCollider = this.colliders[k];
				num3++;
				this.hasBuildCollider = true;
			}
			else if (this.colliders[k].usage != EColliderUsage.Build)
			{
				Vector3 pos = this.colliders[k].pos;
				if (pos.x < 0f)
				{
					pos.x = -pos.x;
				}
				if (pos.y < 0f)
				{
					pos.y = -pos.y;
				}
				if (pos.z < 0f)
				{
					pos.z = -pos.z;
				}
				float num4 = 0f;
				float num5 = pos.y;
				float num6 = Mathf.Sqrt(pos.x * pos.x + pos.z * pos.z);
				if (this.colliders[k].shape == EColliderShape.Sphere)
				{
					num4 = pos.magnitude + this.colliders[k].radius;
					num5 += this.colliders[k].radius;
					num6 += this.colliders[k].radius * 2f;
				}
				else if (this.colliders[k].shape == EColliderShape.Capsule)
				{
					num4 = pos.magnitude + this.colliders[k].ext.magnitude + this.colliders[k].radius;
					num5 += this.colliders[k].ext.y + this.colliders[k].radius;
					num6 += Mathf.Sqrt(this.colliders[k].ext.x * this.colliders[k].ext.x + this.colliders[k].ext.z * this.colliders[k].ext.z) * 2f + this.colliders[k].radius * 2f;
				}
				else if (this.colliders[k].shape == EColliderShape.Box)
				{
					num4 = (pos + this.colliders[k].ext).magnitude;
					num5 += this.colliders[k].ext.y;
					num6 += Mathf.Sqrt(this.colliders[k].ext.x * this.colliders[k].ext.x + this.colliders[k].ext.z * this.colliders[k].ext.z) * 2f * 0.7f;
				}
				this.roughRadius = ((num4 > this.roughRadius) ? num4 : this.roughRadius);
				this.roughHeight = ((num5 > this.roughHeight) ? num5 : this.roughHeight);
				this.roughWidth = ((num6 > this.roughWidth) ? num6 : this.roughWidth);
			}
		}
		if (this.colliders.Length == 0)
		{
			this.colliderComplexity = 0;
		}
		else if (this.colliders.Length == 1 && this.colliders[0].shape == EColliderShape.Sphere)
		{
			this.colliderComplexity = 1;
		}
		else
		{
			this.colliderComplexity = Mathf.Max(2, this.colliders.Length);
		}
		if (num3 > 2)
		{
			for (int l = componentsInChildren2.Length - 1; l >= 0; l--)
			{
				if (this.colliders[l].usage == EColliderUsage.Build && this.colliders[l].shape == EColliderShape.Box)
				{
					this.colliders[l].ext = new Vector3(0.1f, 0.1f, 0.1f);
					break;
				}
			}
		}
		if (num3 > 0)
		{
			if (num3 == 1)
			{
				this.buildColliders = new ColliderData[1];
			}
			else if (num3 == 2)
			{
				this.buildColliders = new ColliderData[2];
			}
			else
			{
				this.buildColliders = new ColliderData[num3 - 1];
			}
			int num7 = 0;
			for (int m = 0; m < componentsInChildren2.Length; m++)
			{
				if (this.colliders[m].usage == EColliderUsage.Build && this.colliders[m].shape == EColliderShape.Box)
				{
					this.buildColliders[num7] = this.colliders[m];
					num7++;
					if (num7 == this.buildColliders.Length)
					{
						break;
					}
				}
			}
			float num8 = Mathf.Sqrt(this.buildCollider.ext.x * this.buildCollider.ext.x + this.buildCollider.ext.y * this.buildCollider.ext.y * 4f + this.buildCollider.ext.z * this.buildCollider.ext.z);
			this.roughRadius = ((num8 > this.roughRadius) ? num8 : this.roughRadius);
		}
		this.roughRadius = this.roughRadius * 1.03f + 0.2f;
		this.roughHeight = this.roughHeight * 1.03f + 0.2f;
		this.roughWidth = this.roughWidth * 1.03f + 0.2f;
		if (componentInChildren2 != null && componentInChildren2.colliderComplexityOverride >= 0)
		{
			this.colliderComplexity = componentInChildren2.colliderComplexityOverride;
			if (this.colliderComplexity == 0)
			{
				this.roughRadius = componentInChildren2.roughRadius;
			}
		}
		if (componentInChildren3 != null && componentInChildren3.colliderComplexityOverride >= 0)
		{
			this.colliderComplexity = componentInChildren3.colliderComplexityOverride;
			if (this.colliderComplexity == 0)
			{
				this.roughRadius = componentInChildren3.roughRadius;
			}
		}
		LODModelDesc componentInChildren4 = this.prefab.GetComponentInChildren<LODModelDesc>(true);
		if (componentInChildren4 != null && componentInChildren4.lodMeshDatas != null && componentInChildren4.lodDistances != null && componentInChildren4.lodVertaPaths != null && componentInChildren4.lodMeshDatas.Length != 0 && componentInChildren4.lodDistances.Length != 0 && componentInChildren4.lodVertaPaths.Length != 0)
		{
			this.lodCount = componentInChildren4.lodMeshDatas.Length;
			if (componentInChildren4.lodDistances.Length < this.lodCount)
			{
				this.lodCount = componentInChildren4.lodDistances.Length;
			}
			if (componentInChildren4.lodVertaPaths.Length < this.lodCount)
			{
				this.lodCount = componentInChildren4.lodVertaPaths.Length;
			}
			if (4 < this.lodCount)
			{
				this.lodCount = 4;
			}
			this.lodMeshes = new Mesh[4];
			this.lodDistances = new float[4];
			this.lodMaterials = new Material[4][];
			this.lodBlueprintMaterials = new Material[4][];
			this.lodSubmeshIgnores = new bool[4][];
			this.lodVertas = new VertaBuffer[4];
			for (int n = 0; n < this.lodCount; n++)
			{
				try
				{
					this.lodMeshes[n] = new Mesh();
					MeshData meshData = new MeshData();
					meshData.Load(componentInChildren4.lodMeshDatas[n]);
					meshData.FillMesh(this.lodMeshes[n]);
					meshData.Free();
					this.lodDistances[n] = componentInChildren4.lodDistances[n];
					this.lodMaterials[n] = new Material[componentInChildren4.lodMeshDatas[n].materials.Length];
					Array.Copy(componentInChildren4.lodMeshDatas[n].materials, this.lodMaterials[n], componentInChildren4.lodMeshDatas[n].materials.Length);
					this.lodBlueprintMaterials[n] = new Material[componentInChildren4.lodMeshDatas[n].materials.Length];
					if (componentInChildren4.lodMeshDatas[n].blueprintMaterials != null)
					{
						Array.Copy(componentInChildren4.lodMeshDatas[n].blueprintMaterials, this.lodBlueprintMaterials[n], Mathf.Min(componentInChildren4.lodMeshDatas[n].blueprintMaterials.Length, this.lodBlueprintMaterials[n].Length));
					}
					this.lodSubmeshIgnores[n] = new bool[componentInChildren4.lodMeshDatas[n].materials.Length];
					if (componentInChildren4.lodMeshDatas[n].blueprintSubmeshIgnores != null)
					{
						Array.Copy(componentInChildren4.lodMeshDatas[n].blueprintSubmeshIgnores, this.lodSubmeshIgnores[n], Mathf.Min(componentInChildren4.lodMeshDatas[n].blueprintSubmeshIgnores.Length, this.lodSubmeshIgnores[n].Length));
					}
					if (!string.IsNullOrEmpty(componentInChildren4.lodVertaPaths[n]))
					{
						VertaBuffer vertaBuffer = new VertaBuffer();
						string str = "Verta/";
						if (vertaBuffer.LoadFromFile(str + componentInChildren4.lodVertaPaths[n] + ".verta"))
						{
							this.lodVertas[n] = vertaBuffer;
						}
						else
						{
							Debug.LogError("verta file load error @ " + this.prefab.name + " " + componentInChildren4.lodVertaPaths[n]);
						}
					}
					this.meshCollider = componentInChildren4.meshCollider;
					this.physCollider = componentInChildren4.physCollider;
					this.startInstCapacity = componentInChildren4.startInstCapacity;
					this.batchCapacity = componentInChildren4.batchCapacity;
					this.cullingHeight = componentInChildren4.cullingHeight;
					this.castShadow = ((componentInChildren4.castShadowSubCount < 0) ? 99 : componentInChildren4.castShadowSubCount);
					this.recvShadow = ((componentInChildren4.recvShadowSubCount < 0) ? 99 : componentInChildren4.recvShadowSubCount);
					if (this.startInstCapacity < 2)
					{
						this.startInstCapacity = 2;
					}
					if (this.batchCapacity < 8)
					{
						this.batchCapacity = 8;
					}
					if (this.cullingHeight < 0.1f)
					{
						this.cullingHeight = 0.1f;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Concat(new string[]
					{
						"lod data load error @ ",
						this.prefab.name,
						" lod ",
						n.ToString(),
						"\r\n",
						ex.ToString()
					}));
				}
			}
			if (componentInChildren4.starmapMesh != null)
			{
				this.starmapMesh = componentInChildren4.starmapMesh;
				this.starmapMaterial = ((componentInChildren4.starmapMaterial != null) ? componentInChildren4.starmapMaterial : Configs.builtin.starmapInstancingDefaultMat);
				this.disableStarmapCulling = componentInChildren4.disableCulling;
				if (!string.IsNullOrEmpty(componentInChildren4.starmapVertaPath))
				{
					VertaBuffer vertaBuffer2 = new VertaBuffer();
					string str2 = "Verta/";
					if (vertaBuffer2.LoadFromFile(str2 + componentInChildren4.starmapVertaPath + ".verta"))
					{
						this.starmapVerta = vertaBuffer2;
					}
					else
					{
						Debug.LogError("starmap verta file load error @ " + this.prefab.name + " " + componentInChildren4.starmapVertaPath);
					}
				}
			}
		}
		else
		{
			this.lodCount = 0;
			this.lodMeshes = new Mesh[0];
			this.lodDistances = new float[0];
			this.lodMaterials = new Material[0][];
			this.lodBlueprintMaterials = new Material[0][];
			this.lodSubmeshIgnores = new bool[0][];
			this.lodVertas = new VertaBuffer[0];
		}
		BuildConditionConfig componentInChildren5 = this.prefab.GetComponentInChildren<BuildConditionConfig>(true);
		if (componentInChildren5 != null)
		{
			if (componentInChildren5.landPoints != null)
			{
				this.landPoints = new Vector3[componentInChildren5.landPoints.Length];
				Array.Copy(componentInChildren5.landPoints, this.landPoints, this.landPoints.Length);
				this.landOffset = componentInChildren5.landOffset;
			}
			else
			{
				this.landPoints = new Vector3[0];
			}
			this.allowBuildInWater = componentInChildren5.allowBuildInWater;
			this.needBuildInWaterTech = (this.allowBuildInWater && componentInChildren5.needBuildInWaterTech);
			if (componentInChildren5.waterPoints != null)
			{
				this.waterPoints = new Vector3[componentInChildren5.waterPoints.Length];
				Array.Copy(componentInChildren5.waterPoints, this.waterPoints, this.waterPoints.Length);
			}
			else
			{
				this.waterPoints = new Vector3[0];
			}
			if (componentInChildren5.waterTypes != null)
			{
				this.waterTypes = new int[componentInChildren5.waterTypes.Length];
				Array.Copy(componentInChildren5.waterTypes, this.waterTypes, this.waterTypes.Length);
			}
			else
			{
				this.waterTypes = new int[0];
			}
			this.multiLevel = componentInChildren5.multiLevel;
			this.lapJoint = componentInChildren5.lapJoint;
			this.multiLevelAllowPortsOrSlots = componentInChildren5.multiLevelAllowInserter;
			this.multiLevelAllowRotate = componentInChildren5.multiLevelAllowRotate;
			this.multiLevelAlternativeIds = new int[componentInChildren5.multiLevelAlternativeIds.Length];
			Array.Copy(componentInChildren5.multiLevelAlternativeIds, this.multiLevelAlternativeIds, this.multiLevelAlternativeIds.Length);
			this.multiLevelAlternativeYawTransposes = new bool[componentInChildren5.multiLevelAlternativeIds.Length];
			if (componentInChildren5.multiLevelAlternativeYawTransposes != null && componentInChildren5.multiLevelAlternativeYawTransposes.Length != 0)
			{
				Array.Copy(componentInChildren5.multiLevelAlternativeYawTransposes, this.multiLevelAlternativeYawTransposes, componentInChildren5.multiLevelAlternativeYawTransposes.Length);
			}
			this.addonType = componentInChildren5.addonType;
			this.veinMiner = componentInChildren5.veinMiner;
			this.oilMiner = componentInChildren5.oilMiner;
			this.dragBuild = componentInChildren5.dragBuild;
			this.dragBuildByGrid = componentInChildren5.dragBuildByGrid;
			this.dragBuildByRect = componentInChildren5.dragBuildByRect;
			this.dragBuildDist = new Vector2(this.buildCollider.ext.x * 2f, this.buildCollider.ext.z * 2f);
			if (componentInChildren5.dragBuildDistOverride.x >= 0.25f)
			{
				this.dragBuildDist.x = componentInChildren5.dragBuildDistOverride.x;
			}
			if (componentInChildren5.dragBuildDistOverride.y >= 0.25f)
			{
				this.dragBuildDist.y = componentInChildren5.dragBuildDistOverride.y;
			}
			if (this.dragBuildDist.x < 0.25f || this.dragBuildDist.y < 0.25f)
			{
				this.dragBuild = false;
			}
			if (this.isBelt || this.isInserter)
			{
				this.dragBuild = false;
			}
			this.dragBuildGridDistOverride = componentInChildren5.dragBuildGridDistOverride;
			if (this.dragBuildGridDistOverride.x <= 0 || this.dragBuildGridDistOverride.y <= 0)
			{
				this.dragBuildByGrid = false;
			}
			this.blueprintBoxSize = new Vector2(this.buildCollider.ext.x * 2f, this.buildCollider.ext.z * 2f);
			if (componentInChildren5.blueprintBoxSizeOverride.x >= 0.25f)
			{
				this.blueprintBoxSize.x = componentInChildren5.blueprintBoxSizeOverride.x;
			}
			if (componentInChildren5.blueprintBoxSizeOverride.y >= 0.25f)
			{
				this.blueprintBoxSize.y = componentInChildren5.blueprintBoxSizeOverride.y;
			}
			this.shiftSnapGridInterval = componentInChildren5.shiftSnapGridInterval;
		}
		else
		{
			this.landPoints = new Vector3[0];
			this.waterPoints = new Vector3[0];
			this.waterTypes = new int[0];
		}
		this.barHeight = this.roughHeight;
		this.barWidth = this.roughWidth;
		SlotConfig componentInChildren6 = this.prefab.GetComponentInChildren<SlotConfig>(true);
		if (componentInChildren6 != null)
		{
			if (componentInChildren6.slotPoses != null)
			{
				this.portPoses = new Pose[componentInChildren6.slotPoses.Length];
				for (int num9 = 0; num9 < this.portPoses.Length; num9++)
				{
					this.portPoses[num9] = new Pose(componentInChildren6.slotPoses[num9].position, componentInChildren6.slotPoses[num9].rotation);
				}
			}
			if (componentInChildren6.insertPoses != null)
			{
				this.slotPoses = new Pose[componentInChildren6.insertPoses.Length];
				for (int num10 = 0; num10 < this.slotPoses.Length; num10++)
				{
					this.slotPoses[num10] = new Pose(componentInChildren6.insertPoses[num10].position, componentInChildren6.insertPoses[num10].rotation);
				}
			}
			if (componentInChildren6.addonAreaCol != null)
			{
				this.addonAreaPoses = new Pose[componentInChildren6.addonAreaCol.Length];
				this.addonAreaColPoses = new Pose[componentInChildren6.addonAreaCol.Length];
				this.addonAreaSize = new Vector3[componentInChildren6.addonAreaCol.Length];
				for (int num11 = 0; num11 < componentInChildren6.addonAreaCol.Length; num11++)
				{
					this.addonAreaPoses[num11] = new Pose(componentInChildren6.addonAreaCenter[num11], componentInChildren6.addonAreaCol[num11].rotation);
					this.addonAreaColPoses[num11] = new Pose(componentInChildren6.addonAreaCol[num11].position, componentInChildren6.addonAreaCol[num11].rotation);
					this.addonAreaSize[num11] = componentInChildren6.addonAreaCol[num11].localScale * 0.5f;
				}
			}
			this.selectCenter = componentInChildren6.selectCenter;
			this.selectSize = componentInChildren6.selectSize;
			this.selectAlpha = componentInChildren6.selectAlpha;
			this.selectDistance = componentInChildren6.selectDistance;
			this.signHeight = componentInChildren6.signHeight;
			this.signSize = componentInChildren6.signSize;
			if (componentInChildren6.overrideBarWidth > 0f)
			{
				this.barWidth = componentInChildren6.overrideBarWidth;
				if (this.barWidth < 0.011f)
				{
					this.barWidth = 0f;
				}
			}
			if (componentInChildren6.overrideBarHeight != 0f)
			{
				this.barHeight = componentInChildren6.overrideBarHeight;
				if (this.barHeight < 0.011f && this.barHeight > -0.011f)
				{
					this.barHeight = 0f;
				}
			}
		}
		else
		{
			this.portPoses = new Pose[0];
			this.slotPoses = new Pose[0];
			this.addonAreaPoses = new Pose[0];
			this.addonAreaColPoses = new Pose[0];
			this.addonAreaSize = new Vector3[0];
		}
		BeltDesc componentInChildren7 = this.prefab.GetComponentInChildren<BeltDesc>(true);
		SplitterDesc componentInChildren8 = this.prefab.GetComponentInChildren<SplitterDesc>(true);
		StorageDesc componentInChildren9 = this.prefab.GetComponentInChildren<StorageDesc>(true);
		TankDesc componentInChildren10 = this.prefab.GetComponentInChildren<TankDesc>(true);
		MinerDesc componentInChildren11 = this.prefab.GetComponentInChildren<MinerDesc>(true);
		InserterDesc componentInChildren12 = this.prefab.GetComponentInChildren<InserterDesc>(true);
		AssemblerDesc componentInChildren13 = this.prefab.GetComponentInChildren<AssemblerDesc>(true);
		FractionatorDesc componentInChildren14 = this.prefab.GetComponentInChildren<FractionatorDesc>(true);
		LabDesc componentInChildren15 = this.prefab.GetComponentInChildren<LabDesc>(true);
		StationDesc componentInChildren16 = this.prefab.GetComponentInChildren<StationDesc>(true);
		DispenserDesc componentInChildren17 = this.prefab.GetComponentInChildren<DispenserDesc>(true);
		MarkerDesc componentInChildren18 = this.prefab.GetComponentInChildren<MarkerDesc>(true);
		EjectorDesc componentInChildren19 = this.prefab.GetComponentInChildren<EjectorDesc>(true);
		SiloDesc componentInChildren20 = this.prefab.GetComponentInChildren<SiloDesc>(true);
		AnimDesc componentInChildren21 = this.prefab.GetComponentInChildren<AnimDesc>(true);
		MinimapConfig componentInChildren22 = this.prefab.GetComponentInChildren<MinimapConfig>(true);
		PowerDesc componentInChildren23 = this.prefab.GetComponentInChildren<PowerDesc>(true);
		AudioDesc componentInChildren24 = this.prefab.GetComponentInChildren<AudioDesc>(true);
		MonitorDesc componentInChildren25 = this.prefab.GetComponentInChildren<MonitorDesc>(true);
		SpeakerDesc componentInChildren26 = this.prefab.GetComponentInChildren<SpeakerDesc>(true);
		SpraycoaterDesc componentInChildren27 = this.prefab.GetComponentInChildren<SpraycoaterDesc>(true);
		Object componentInChildren28 = this.prefab.GetComponentInChildren<PilerDesc>(true);
		if (componentInChildren7 != null)
		{
			this.beltPrototype = componentInChildren7.beltPrototype;
			this.beltSpeed = componentInChildren7.speed;
			this.isBelt = (this.beltSpeed > 0);
		}
		else if (componentInChildren8 != null)
		{
			this.isSplitter = true;
		}
		else if (componentInChildren25 != null)
		{
			this.isMonitor = true;
			this.monitorOffset = componentInChildren25.offset;
			this.monitorTargetCargoBytes = componentInChildren25.targetCargoBytes;
			this.monitorPeriodTickCount = componentInChildren25.periodTickCount;
			this.monitorPassOperator = componentInChildren25.passOperator;
			this.monitorPassColorId = componentInChildren25.passColorId;
			this.monitorFailColorId = componentInChildren25.failColorId;
			this.monitorSystemWarningMode = componentInChildren25.systemWarningMode;
			this.monitorMode = componentInChildren25.monitorMode;
			this.monitorCargoFilter = componentInChildren25.cargoFilter;
			this.monitorSignalId = componentInChildren25.signalId;
			this.monitorSpawnOperator = componentInChildren25.spawnOperator;
		}
		if (componentInChildren26 != null)
		{
			this.isSpeaker = true;
			this.speakerTone = componentInChildren26.tone;
			this.speakerVolume = componentInChildren26.volume;
			this.speakerPitch = componentInChildren26.pitch;
			this.speakerLength = componentInChildren26.length;
			this.speakerRepeat = componentInChildren26.repeat;
		}
		if (componentInChildren27 != null)
		{
			this.incCapacity = componentInChildren27.incCapacity;
			this.incItemId = new int[componentInChildren27.incItemId.Length];
			Array.Copy(componentInChildren27.incItemId, this.incItemId, componentInChildren27.incItemId.Length);
			this.isSpraycoster = true;
		}
		else
		{
			this.incItemId = new int[0];
		}
		if (componentInChildren28 != null)
		{
			this.isPiler = true;
		}
		if (componentInChildren9 != null)
		{
			this.storageCol = componentInChildren9.colCount;
			this.storageRow = componentInChildren9.rowCount;
			this.isStorage = (this.storageCol * this.storageRow > 0);
		}
		if (componentInChildren10 != null)
		{
			this.isTank = true;
			this.fluidStorageCount = componentInChildren10.fluidStorageCount;
		}
		if (componentInChildren11 != null)
		{
			this.minerType = componentInChildren11.minerType;
			this.minerPeriod = Mathf.RoundToInt(componentInChildren11.periodf * 600000f);
		}
		if (componentInChildren12 != null)
		{
			this.inserterGrade = componentInChildren12.grade;
			this.inserterSTT = Mathf.RoundToInt(componentInChildren12.sttf * 600000f);
			this.inserterDelay = Mathf.RoundToInt(componentInChildren12.delayf * 600000f);
			this.inserterCanStack = componentInChildren12.canStack;
			this.inserterStackSize = componentInChildren12.stackSize;
			this.isInserter = (this.inserterSTT > 0);
		}
		if (componentInChildren13 != null)
		{
			this.assemblerSpeed = Mathf.RoundToInt(componentInChildren13.speedf * 10000f);
			this.assemblerRecipeType = componentInChildren13.recipeType;
			this.isAssembler = (this.assemblerRecipeType > ERecipeType.None);
		}
		if (componentInChildren14 != null)
		{
			this.isFractionator = true;
			this.fracFluidInputMax = componentInChildren14.fluidInputMax;
			this.fracProductOutputMax = componentInChildren14.productOutputMax;
			this.fracFluidOutputMax = componentInChildren14.fluidOutputMax;
			this.assemblerRecipeType = componentInChildren14.recipeType;
		}
		if (componentInChildren15 != null)
		{
			this.isLab = true;
			this.labAssembleSpeed = Mathf.RoundToInt(componentInChildren15.assembleSpeed * 10000f);
			this.labResearchSpeed = componentInChildren15.researchSpeed;
		}
		if (componentInChildren16 != null)
		{
			this.isStation = true;
			this.isStellarStation = componentInChildren16.isStellar;
			this.stationMaxItemCount = componentInChildren16.maxItemCount;
			this.stationMaxItemKinds = componentInChildren16.maxItemKinds;
			this.stationMaxDroneCount = componentInChildren16.maxDroneCount;
			this.stationMaxShipCount = componentInChildren16.maxShipCount;
			this.stationMaxEnergyAcc = componentInChildren16.maxEnergyAcc;
			this.stationDronePos = componentInChildren16.dronePoint;
			this.stationShipPos = componentInChildren16.shipPoint;
			this.isCollectStation = componentInChildren16.isCollector;
			this.isVeinCollector = componentInChildren16.isVeinCollector;
			this.stationCollectSpeed = componentInChildren16.collectSpeed;
		}
		if (componentInChildren17 != null)
		{
			this.isDispenser = true;
			this.dispenserMaxCourierCount = componentInChildren17.maxCourierCount;
			this.dispenserMaxEnergyAcc = componentInChildren17.maxEnergyAcc;
		}
		if (componentInChildren18 != null)
		{
			this.isMarker = true;
			this.markerPointDefaultHeight = componentInChildren18.markerPointDefaultHeight;
			this.markerPointDefaultRadius = componentInChildren18.markerPointDefaultRadius;
		}
		if (componentInChildren19 != null)
		{
			this.isEjector = true;
			this.ejectorPivotY = componentInChildren19.pivotY;
			this.ejectorMuzzleY = componentInChildren19.muzzleY;
			this.ejectorChargeFrame = componentInChildren19.chargeFrame;
			this.ejectorColdFrame = componentInChildren19.coldFrame;
			this.ejectorBulletId = componentInChildren19.bulletProtoId;
		}
		if (componentInChildren20 != null)
		{
			this.isSilo = true;
			this.siloChargeFrame = componentInChildren20.chargeFrame;
			this.siloColdFrame = componentInChildren20.coldFrame;
			this.siloBulletId = componentInChildren20.bulletProtoId;
		}
		if (componentInChildren21 != null)
		{
			this.anim_prepare_length = componentInChildren21.prepare_length;
			this.anim_working_length = componentInChildren21.working_length;
		}
		else
		{
			this.anim_prepare_length = 0f;
			this.anim_working_length = 1f;
		}
		if (componentInChildren23 != null)
		{
			this.isPowerNode = componentInChildren23.node;
			this.powerConnectDistance = componentInChildren23.connectDistance;
			this.powerCoverRadius = componentInChildren23.coverRadius;
			this.powerPoint = componentInChildren23.powerPoint;
			this.isPowerGen = componentInChildren23.generator;
			this.photovoltaic = componentInChildren23.photovoltaic;
			this.windForcedPower = componentInChildren23.wind;
			this.gammaRayReceiver = componentInChildren23.gamma;
			this.geothermal = componentInChildren23.geothermal;
			this.genEnergyPerTick = componentInChildren23.genEnergyPerTick;
			this.useFuelPerTick = componentInChildren23.useFuelPerTick;
			this.fuelMask = componentInChildren23.fuelMask;
			this.powerCatalystId = componentInChildren23.catalystId;
			this.powerProductId = componentInChildren23.productId;
			this.powerProductHeat = componentInChildren23.productHeat;
			this.isAccumulator = componentInChildren23.accumulator;
			this.inputEnergyPerTick = componentInChildren23.inputEnergyPerTick;
			this.outputEnergyPerTick = componentInChildren23.outputEnergyPerTick;
			this.maxAcuEnergy = componentInChildren23.maxAcuEnergy;
			this.isPowerConsumer = componentInChildren23.consumer;
			this.isPowerCharger = componentInChildren23.charger;
			this.workEnergyPerTick = componentInChildren23.workEnergyPerTick;
			this.idleEnergyPerTick = componentInChildren23.idleEnergyPerTick;
			this.isPowerExchanger = componentInChildren23.exchanger;
			this.exchangeEnergyPerTick = componentInChildren23.exchangeEnergyPerTick;
			this.emptyId = componentInChildren23.emptyId;
			this.fullId = componentInChildren23.fullId;
			ItemProto itemProto = LDB.items.Select(this.fullId);
			if (itemProto != null)
			{
				this.maxExcEnergy = itemProto.HeatValue;
			}
		}
		if (componentInChildren22 != null)
		{
			this.minimapType = componentInChildren22.type;
			this.minimapScl.x = this.selectSize.x;
			this.minimapScl.y = this.selectSize.y;
			this.minimapScl.z = this.selectSize.z;
			this.minimapScl.w = componentInChildren22.height;
			if (componentInChildren22.overrideSize.x > 0f)
			{
				this.minimapScl.x = componentInChildren22.overrideSize.x;
			}
			if (componentInChildren22.overrideSize.y > 0f)
			{
				this.minimapScl.y = componentInChildren22.overrideSize.y;
			}
			if (componentInChildren22.overrideSize.z > 0f)
			{
				this.minimapScl.z = componentInChildren22.overrideSize.z;
			}
		}
		else
		{
			this.minimapType = 0;
			this.minimapScl = Vector4.zero;
		}
		if (componentInChildren24 != null)
		{
			this.hasAudio = (componentInChildren24.logic > 0);
			AudioProto audioProto = LDB.audios[componentInChildren24.audio0];
			this.audioProtoId0 = ((audioProto == null) ? 0 : audioProto.ID);
			AudioProto audioProto2 = LDB.audios[componentInChildren24.audio1];
			this.audioProtoId1 = ((audioProto2 == null) ? 0 : audioProto2.ID);
			AudioProto audioProto3 = LDB.audios[componentInChildren24.audio2];
			this.audioProtoId2 = ((audioProto3 == null) ? 0 : audioProto3.ID);
			this.audioLogic = componentInChildren24.logic;
			this.audioRadius0 = componentInChildren24.radius0;
			this.audioRadius1 = componentInChildren24.radius1;
			this.audioFalloff = componentInChildren24.falloff;
			this.audioVolume = componentInChildren24.volume;
			this.audioPitch = componentInChildren24.pitch;
			this.audioDoppler = componentInChildren24.doppler;
		}
		else
		{
			this.hasAudio = false;
		}
		EnemyDesc componentInChildren29 = this.prefab.GetComponentInChildren<EnemyDesc>(true);
		if (componentInChildren29 != null)
		{
			this.isEnemy = true;
			this.enemySelectCircleRadius = componentInChildren29.selectCircleRadius;
			this.enemySandCount = componentInChildren29.sandCount;
			EnemyBuilderDesc componentInChildren30 = this.prefab.GetComponentInChildren<EnemyBuilderDesc>(true);
			if (componentInChildren30 != null)
			{
				this.isEnemyBuilder = true;
				this.enemyMinMatter = componentInChildren30.minMatter;
				this.enemyMinEnergy = componentInChildren30.minEnergy;
				this.enemyMaxMatter = componentInChildren30.maxMatter;
				this.enemyMaxEnergy = componentInChildren30.maxEnergy;
				this.enemySpMatter = componentInChildren30.spMatter;
				this.enemySpEnergy = componentInChildren30.spEnergy;
				this.enemySpMax = componentInChildren30.spMax;
				this.enemyIdleEnergy = componentInChildren30.idleEnergy;
				this.enemyWorkEnergy = componentInChildren30.workEnergy;
				this.enemyGenMatter = componentInChildren30.genMatter;
				this.enemyGenEnergy = componentInChildren30.genEnergy;
			}
			else
			{
				this.enemyMinMatter = 0;
				this.enemyMinEnergy = 0;
				this.enemyMaxMatter = 0;
				this.enemyMaxEnergy = 0;
				this.enemySpMatter = 0;
				this.enemySpEnergy = 0;
				this.enemySpMax = 1;
				this.enemyIdleEnergy = 0;
				this.enemyWorkEnergy = 0;
				this.enemyGenMatter = 0;
				this.enemyGenEnergy = 0;
			}
			if (componentInChildren29.overrideBarWidth > 0f)
			{
				this.barWidth = componentInChildren29.overrideBarWidth;
				if (this.barWidth < 0.011f)
				{
					this.barWidth = 0f;
				}
			}
			if (componentInChildren29.overrideBarHeight != 0f)
			{
				this.barHeight = componentInChildren29.overrideBarHeight;
				if (this.barHeight < 0.011f && this.barHeight > -0.011f)
				{
					this.barHeight = 0f;
				}
			}
		}
		Object componentInChildren31 = this.prefab.GetComponentInChildren<SpacecraftDesc>(true);
		CreationPartDesc componentInChildren32 = this.prefab.GetComponentInChildren<CreationPartDesc>(true);
		if (componentInChildren31 != null || componentInChildren32 != null)
		{
			this.isSpacecraft = true;
		}
		if (this.isSpacecraft)
		{
			int num12 = 0;
			int num13 = 0;
			for (int num14 = 0; num14 < componentsInChildren2.Length; num14++)
			{
				if (this.colliders[num14].usage == EColliderUsage.Physics)
				{
					num12++;
				}
				else if (this.colliders[num14].usage == EColliderUsage.Build)
				{
					num13++;
				}
			}
			this.spacePhysicsColliders = new ColliderDataLF[num12];
			this.spaceBuildColliders = new ColliderDataLF[num13];
			num12 = 0;
			num13 = 0;
			for (int num15 = 0; num15 < componentsInChildren2.Length; num15++)
			{
				if (this.colliders[num15].usage == EColliderUsage.Physics)
				{
					this.spacePhysicsColliders[num12].InitFromCollider(componentsInChildren2[num15], this.independentCollider);
					num12++;
				}
				else if (this.colliders[num15].usage == EColliderUsage.Build)
				{
					this.spaceBuildColliders[num13].InitFromCollider(componentsInChildren2[num15], this.independentCollider);
					num13++;
				}
			}
		}
		if (this.prefab.GetComponentInChildren<DFGBaseDesc>(true) != null)
		{
			this.isDFGroundBase = true;
		}
		if (this.prefab.GetComponentInChildren<DFGConnectorDesc>(true) != null)
		{
			this.isDFGroundConnector = true;
		}
		DFGReplicatorDesc componentInChildren33 = this.prefab.GetComponentInChildren<DFGReplicatorDesc>(true);
		if (componentInChildren33 != null)
		{
			this.isDFGroundReplicator = true;
			this.dfReplicatorProductId = componentInChildren33.productId;
			this.dfReplicatorProductSpMatter = componentInChildren33.productSpMatter;
			this.dfReplicatorProductSpEnergy = componentInChildren33.productSpEnergy;
			this.dfReplicatorProductSpMax = componentInChildren33.productSpMax;
			this.dfReplicatorTurboSpeed = componentInChildren33.turboSpeed;
			this.dfReplicatorUnitSupply = componentInChildren33.unitSupply;
			this.dfReplicatorProductInitialPos = componentInChildren33.productInitialPose.localPosition;
			this.dfReplicatorProductInitialRot = componentInChildren33.productInitialPose.localRotation;
			this.dfReplicatorProductInitialVel = componentInChildren33.productInitialVel.forward * componentInChildren33.productInitialVel.localScale.z;
			this.dfReplicatorProductInitialTick = componentInChildren33.productInitialTick;
		}
		DFGTurretDesc componentInChildren34 = this.prefab.GetComponentInChildren<DFGTurretDesc>(true);
		if (componentInChildren34 != null)
		{
			this.isDFGroundTurret = true;
			this.dfTurretType = componentInChildren34.type;
			this.dfTurretAttackRange = componentInChildren34.attackRange;
			this.dfTurretSensorRange = componentInChildren34.sensorRange;
			this.dfTurretRangeInc = componentInChildren34.rangeInc;
			this.dfTurretAttackEnergy = componentInChildren34.attackEnergy;
			this.dfTurretAttackInterval = componentInChildren34.attackInterval;
			this.dfTurretAttackHeat = componentInChildren34.attackHeat;
			this.dfTurretAttackDamage = componentInChildren34.attackDamage;
			this.dfTurretAttackDamageInc = componentInChildren34.attackDamageInc;
			this.dfTurretColdSpeed = componentInChildren34.coldSpeed;
			this.dfTurretColdSpeedInc = componentInChildren34.coldSpeedInc;
			this.dfTurretMuzzleY = componentInChildren34.muzzleY;
		}
		if (this.prefab.GetComponentInChildren<DFGShieldDesc>(true) != null)
		{
			this.isDFGroundShield = true;
		}
		DFSCoreDesc componentInChildren35 = this.prefab.GetComponentInChildren<DFSCoreDesc>(true);
		if (componentInChildren35 != null)
		{
			this.isDFSpaceCore = true;
			this.dfsCoreBuildRelaySpMatter = componentInChildren35.relayBuilderDesc.spMatter;
			this.dfsCoreBuildRelaySpEnergy = componentInChildren35.relayBuilderDesc.spEnergy;
			this.dfsCoreBuildRelaySpMax = componentInChildren35.relayBuilderDesc.spMax;
			this.dfsCoreBuildTinderSpMatter = componentInChildren35.tinderBuilderDesc.spMatter;
			this.dfsCoreBuildTinderSpEnergy = componentInChildren35.tinderBuilderDesc.spEnergy;
			this.dfsCoreBuildTinderSpMax = componentInChildren35.tinderBuilderDesc.spMax;
			this.dfsCoreBuildTinderTriggerMinTick = componentInChildren35.tinderTriggerMinTick;
			this.dfsCoreBuildTinderTriggerKeyTick = componentInChildren35.tinderTriggerKeyTick;
			this.dfsCoreBuildTinderTriggerProbability = componentInChildren35.tinderTriggerProbability;
		}
		if (this.prefab.GetComponentInChildren<DFSNodeDesc>(true) != null)
		{
			this.isDFSpaceNode = true;
		}
		DFSConnectorDesc componentInChildren36 = this.prefab.GetComponentInChildren<DFSConnectorDesc>(true);
		if (componentInChildren36 != null)
		{
			this.isDFSpaceConnector = true;
			this.isDFSpaceConnectorVertical = componentInChildren36.isVertical;
		}
		DFSReplicatorDesc componentInChildren37 = this.prefab.GetComponentInChildren<DFSReplicatorDesc>(true);
		if (componentInChildren37 != null)
		{
			this.isDFSpaceReplicator = true;
			this.dfReplicatorProductId = componentInChildren37.productId;
			this.dfReplicatorProductSpMatter = componentInChildren37.productSpMatter;
			this.dfReplicatorProductSpEnergy = componentInChildren37.productSpEnergy;
			this.dfReplicatorProductSpMax = componentInChildren37.productSpMax;
			this.dfReplicatorTurboSpeed = componentInChildren37.turboSpeed;
			this.dfReplicatorUnitSupply = componentInChildren37.unitSupply;
			this.dfReplicatorProductInitialPos = componentInChildren37.productInitialPose.localPosition;
			this.dfReplicatorProductInitialRot = componentInChildren37.productInitialPose.localRotation;
			this.dfReplicatorProductInitialVel = componentInChildren37.productInitialVel.forward * componentInChildren37.productInitialVel.localScale.z;
			this.dfReplicatorProductInitialTick = componentInChildren37.productInitialTick;
		}
		if (this.prefab.GetComponentInChildren<DFSGammaDesc>(true) != null)
		{
			this.isDFSpaceGammaReceiver = true;
		}
		DFSTurretDesc componentInChildren38 = this.prefab.GetComponentInChildren<DFSTurretDesc>(true);
		if (componentInChildren38 != null)
		{
			this.isDFSpaceTurret = true;
			this.dfTurretType = componentInChildren38.type;
			this.dfTurretAttackRange = componentInChildren38.attackRange;
			this.dfTurretSensorRange = componentInChildren38.sensorRange;
			this.dfTurretRangeInc = componentInChildren38.rangeInc;
			this.dfTurretAttackEnergy = componentInChildren38.attackEnergy;
			this.dfTurretAttackInterval = componentInChildren38.attackInterval;
			this.dfTurretAttackHeat = componentInChildren38.attackHeat;
			this.dfTurretAttackDamage = componentInChildren38.attackDamage;
			this.dfTurretAttackDamageInc = componentInChildren38.attackDamageInc;
			this.dfTurretColdSpeed = componentInChildren38.coldSpeed;
			this.dfTurretColdSpeedInc = componentInChildren38.coldSpeedInc;
			this.dfTurretMuzzleY = componentInChildren38.muzzleY;
		}
		if (this.prefab.GetComponentInChildren<DFRelayDesc>(true) != null)
		{
			this.isDFRelay = true;
		}
		if (this.prefab.GetComponentInChildren<DFTinderDesc>(true) != null)
		{
			this.isDFTinder = true;
		}
		EnemyUnitDesc componentInChildren39 = this.prefab.GetComponentInChildren<EnemyUnitDesc>(true);
		if (componentInChildren39 != null)
		{
			this.isEnemyUnit = true;
			this.unitMaxMovementSpeed = componentInChildren39.maxMovementSpeed;
			this.unitMaxMovementAcceleration = componentInChildren39.maxMovementAcceleration;
			this.unitMarchMovementSpeed = componentInChildren39.marchMovementSpeed;
			this.unitAssaultArriveRange = componentInChildren39.assaultArriveRange;
			this.unitEngageArriveRange = componentInChildren39.engageArriveRange;
			this.unitSensorRange = componentInChildren39.sensorRange;
			this.unitAttackRange0 = componentInChildren39.attackRange0;
			this.unitAttackInterval0 = componentInChildren39.attackInterval0;
			this.unitAttackHeat0 = componentInChildren39.attackHeat0;
			this.unitAttackDamage0 = componentInChildren39.attackDamage0;
			this.unitAttackDamageInc0 = componentInChildren39.attackDamageInc0;
			this.unitAttackRange1 = componentInChildren39.attackRange1;
			this.unitAttackInterval1 = componentInChildren39.attackInterval1;
			this.unitAttackHeat1 = componentInChildren39.attackHeat1;
			this.unitAttackDamage1 = componentInChildren39.attackDamage1;
			this.unitAttackDamageInc1 = componentInChildren39.attackDamageInc1;
			this.unitAttackRange2 = componentInChildren39.attackRange2;
			this.unitAttackInterval2 = componentInChildren39.attackInterval2;
			this.unitAttackHeat2 = componentInChildren39.attackHeat2;
			this.unitAttackDamage2 = componentInChildren39.attackDamage2;
			this.unitAttackDamageInc2 = componentInChildren39.attackDamageInc2;
			this.unitColdSpeed = componentInChildren39.coldSpeed;
			this.unitColdSpeedInc = componentInChildren39.coldSpeedInc;
		}
		FleetDesc componentInChildren40 = this.prefab.GetComponentInChildren<FleetDesc>(true);
		if (componentInChildren40 != null)
		{
			this.isFleet = true;
			this.fleetInitializeUnitSpeedScale = componentInChildren40.initializeUnitSpeedScale;
			this.fleetSensorRange = componentInChildren40.sensorRange;
			this.fleetMaxActiveArea = componentInChildren40.maxActiveArea;
			this.fleetMaxMovementSpeed = componentInChildren40.maxMovementSpeed;
			this.fleetMaxMovementAcceleration = componentInChildren40.maxMovementAcceleration;
			this.fleetMaxRotateAcceleration = componentInChildren40.maxRotateAcceleration;
			this.fleetMaxAssembleCount = componentInChildren40.maxAssembleCount;
			this.fleetMaxChargingCount = componentInChildren40.maxChargingCount;
			int num16 = componentInChildren40.ports.Length;
			this.fleetPorts = new FleetPortDesc[num16];
			for (int num17 = 0; num17 < num16; num17++)
			{
				this.fleetPorts[num17] = componentInChildren40.ports[num17];
				this.fleetPorts[num17].pose.position = componentInChildren40.portTrans[num17].localPosition;
				this.fleetPorts[num17].pose.rotation = componentInChildren40.portTrans[num17].localRotation;
			}
		}
		UnitDesc componentInChildren41 = this.prefab.GetComponentInChildren<UnitDesc>(true);
		if (componentInChildren41 != null)
		{
			this.isCraftUnit = true;
			this.craftUnitSize = componentInChildren41.craftSize;
			this.craftUnitInitializeSpeed = componentInChildren41.initializeSpeed;
			this.craftUnitMaxMovementSpeed = componentInChildren41.maxMovementSpeed;
			this.craftUnitMaxMovementAcceleration = componentInChildren41.maxMovementAcceleration;
			this.craftUnitMaxRotateAcceleration = componentInChildren41.maxRotateAcceleration;
			this.craftUnitAttackRange0 = componentInChildren41.attackRange0;
			this.craftUnitAttackRange1 = componentInChildren41.attackRange1;
			this.craftUnitSensorRange = componentInChildren41.sensorRange;
			this.craftUnitROF0 = componentInChildren41.ROF0;
			this.craftUnitROF1 = componentInChildren41.ROF1;
			this.craftUnitRoundInterval0 = componentInChildren41.roundInterval0;
			this.craftUnitRoundInterval1 = componentInChildren41.roundInterval1;
			this.craftUnitMuzzleInterval0 = componentInChildren41.muzzleInterval0;
			this.craftUnitMuzzleInterval1 = componentInChildren41.muzzleInterval1;
			this.craftUnitMuzzleCount0 = componentInChildren41.muzzleCount0;
			this.craftUnitMuzzleCount1 = componentInChildren41.muzzleCount1;
			this.craftUnitAttackDamage0 = componentInChildren41.attackDamage0;
			this.craftUnitAttackDamage1 = componentInChildren41.attackDamage1;
			this.craftUnitEnergyPerTick = componentInChildren41.energyPerTick;
			this.craftUnitFireEnergy0 = componentInChildren41.fireEnergy0;
			this.craftUnitFireEnergy1 = componentInChildren41.fireEnergy1;
			this.craftUnitRepairEnergyPerHP = componentInChildren41.repairEnergyPerHP;
			this.craftUnitRepairHPPerTick = componentInChildren41.repairHPPerTick;
			this.craftUnitAddEnemyExppBase = componentInChildren41.addEnemyExppBase;
			this.craftUnitAddEnemyThreatBase = componentInChildren41.addEnemyThreatBase;
			this.craftUnitAddEnemyHatredBase = componentInChildren41.addEnemyHatredBase;
			this.craftUnitAddEnemyExppCoef = componentInChildren41.addEnemyExppCoef;
			this.craftUnitAddEnemyThreatCoef = componentInChildren41.addEnemyThreatCoef;
			this.craftUnitAddEnemyHatredCoef = componentInChildren41.addEnemyHatredCoef;
		}
		TurretDesc componentInChildren42 = this.prefab.GetComponentInChildren<TurretDesc>(true);
		if (componentInChildren42 != null)
		{
			this.isTurret = true;
			this.turretType = componentInChildren42.type;
			this.turretAmmoType = componentInChildren42.ammoType;
			this.turretVSCaps = componentInChildren42.vsCaps;
			this.turretDefaultDir = componentInChildren42.defaultDir;
			this.turretROF = componentInChildren42.ROF;
			this.turretMuzzleCount = componentInChildren42.muzzleCount;
			this.turretRoundInterval = componentInChildren42.roundInterval;
			this.turretMuzzleInterval = componentInChildren42.muzzleInterval;
			this.turretMinAttackRange = componentInChildren42.minAttackRange;
			this.turretMaxAttackRange = componentInChildren42.maxAttackRange;
			this.turretSpaceAttackRange = componentInChildren42.spaceAttackRange;
			this.turretPitchUpMax = componentInChildren42.pitchUpMax;
			this.turretPitchDownMax = componentInChildren42.pitchDownMax;
			this.turretDamageScale = componentInChildren42.damageScale;
			this.turretMuzzleY = componentInChildren42.muzzleY;
			this.turretAimSpeed = componentInChildren42.aimSpeed;
			this.turretUniformAngleSpeed = componentInChildren42.uniformAngleSpeed;
			this.turretAngleAcc = componentInChildren42.angleAcc;
			this.turretAddEnemyExppBase = componentInChildren42.addEnemyExppBase;
			this.turretAddEnemyThreatBase = componentInChildren42.addEnemyThreatBase;
			this.turretAddEnemyHatredBase = componentInChildren42.addEnemyHatredBase;
			this.turretAddEnemyExppCoef = componentInChildren42.addEnemyExppCoef;
			this.turretAddEnemyThreatCoef = componentInChildren42.addEnemyThreatCoef;
			this.turretAddEnemyHatredCoef = componentInChildren42.addEnemyHatredCoef;
		}
		AmmoDesc componentInChildren43 = this.prefab.GetComponentInChildren<AmmoDesc>(true);
		if (componentInChildren43 != null)
		{
			this.isAmmo = true;
			this.AmmoBlastRadius0 = componentInChildren43.blastRadius0;
			this.AmmoBlastRadius1 = componentInChildren43.blastRadius1;
			this.AmmoBlastFalloff = componentInChildren43.blastFalloff;
			this.AmmoMoveAcc = componentInChildren43.moveAcc;
			this.AmmoTurnAcc = componentInChildren43.turnAcc;
			this.AmmoHitIndex = componentInChildren43.hitIndex;
			this.AmmoParameter0 = componentInChildren43.parameter0;
		}
		BeaconDesc componentInChildren44 = this.prefab.GetComponentInChildren<BeaconDesc>(true);
		if (componentInChildren44 != null)
		{
			this.isBeacon = true;
			this.beaconSignalRadius = componentInChildren44.signalRadius;
			this.beaconROF = componentInChildren44.ROF;
			this.beaconSpaceSignalRange = componentInChildren44.spaceSignalRange;
			this.beaconPitchUpMax = componentInChildren44.pitchUpMax;
			this.beaconPitchDownMax = componentInChildren44.pitchDownMax;
		}
		FieldGeneratorDesc componentInChildren45 = this.prefab.GetComponentInChildren<FieldGeneratorDesc>(true);
		if (componentInChildren45 != null)
		{
			this.isFieldGenerator = true;
			this.fieldGenEnergyCapacity = componentInChildren45.energyCapacity;
			this.fieldGenEnergyRequire0 = componentInChildren45.energyRequire0;
			this.fieldGenEnergyRequire1 = componentInChildren45.energyRequire1;
		}
		BattleBaseDesc componentInChildren46 = this.prefab.GetComponentInChildren<BattleBaseDesc>(true);
		if (componentInChildren46 != null)
		{
			this.isBattleBase = true;
			this.battleBaseMaxEnergyAcc = componentInChildren46.maxEnergyAcc;
			this.battleBasePickRange = componentInChildren46.pickRange;
		}
		ConstructionModuleDesc componentInChildren47 = this.prefab.GetComponentInChildren<ConstructionModuleDesc>(true);
		if (componentInChildren47 != null)
		{
			this.isConstructionModule = true;
			this.constructionDroneCount = componentInChildren47.droneCount;
			this.constructionRange = componentInChildren47.buildRange;
			this.constructionDroneEjectPos = componentInChildren47.droneEjectPos;
		}
		CombatModuleDesc componentInChildren48 = this.prefab.GetComponentInChildren<CombatModuleDesc>(true);
		if (componentInChildren48 != null)
		{
			this.isCombatModule = true;
			this.combatModuleFleetProtoId = componentInChildren48.fleetProtoId;
			this.combatModuleFleetTrans = componentInChildren48.fleetTrans;
		}
		if (this.prefab.GetComponentInChildren<DroneDesc>(true) != null)
		{
			this.isConstructionDrone = true;
		}
		CreationPartDesc componentInChildren49 = this.prefab.GetComponentInChildren<CreationPartDesc>(true);
		if (componentInChildren49 != null)
		{
			this.creationBlock = new CreationPartPropertyBlock();
			this.creationBlock.mass = componentInChildren49.mass;
			if (this.creationBlock.mass < 0.1f)
			{
				this.creationBlock.mass = 0.1f;
				Debug.LogWarning("[" + this.prefab.name + "] 部件质量太小 CreationPartDesc.mass");
			}
			if (this.prefab.GetComponentInChildren<ConstructAnchorDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.ConstructAnchor;
			}
			if (this.prefab.GetComponentInChildren<VStructureDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VStructure;
			}
			if (this.prefab.GetComponentInChildren<VDCockpitDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDCockpit;
			}
			if (this.prefab.GetComponentInChildren<VDGyroscopeDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDGyroscope;
			}
			if (this.prefab.GetComponentInChildren<VDTyreDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDTyre;
			}
			if (this.prefab.GetComponentInChildren<VDSuspensionDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDSuspension;
			}
			if (this.prefab.GetComponentInChildren<VDEngineDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDEngine;
			}
			if (this.prefab.GetComponentInChildren<VDWarpDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDWarp;
			}
			if (this.prefab.GetComponentInChildren<VDBatteryDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDBattery;
			}
			if (this.prefab.GetComponentInChildren<VDStorageDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDStorage;
			}
			if (this.prefab.GetComponentInChildren<VDFuelStorageDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDFuelStorage;
			}
			if (this.prefab.GetComponentInChildren<VDTankDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDTank;
			}
			if (this.prefab.GetComponentInChildren<VDConnectorDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VDConnector;
			}
			if (this.prefab.GetComponentInChildren<VWGaussDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWGauss;
			}
			if (this.prefab.GetComponentInChildren<VWLaserDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWLaser;
			}
			if (this.prefab.GetComponentInChildren<VWCannonDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWCannon;
			}
			if (this.prefab.GetComponentInChildren<VWMissileDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWMissile;
			}
			if (this.prefab.GetComponentInChildren<VWPlasmaDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWPlasma;
			}
			if (this.prefab.GetComponentInChildren<VWDisturbDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWDisturb;
			}
			if (this.prefab.GetComponentInChildren<VWThrowDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWThrow;
			}
			if (this.prefab.GetComponentInChildren<VWShieldDesc>(true) != null)
			{
				this.creationBlock.type = ECreationPart.VWShield;
				return;
			}
		}
		else
		{
			this.creationBlock = null;
		}
	}

	// Token: 0x04001E71 RID: 7793
	public static PrefabDesc none = new PrefabDesc();

	// Token: 0x04001E72 RID: 7794
	public int modelIndex;

	// Token: 0x04001E73 RID: 7795
	public int subId;

	// Token: 0x04001E74 RID: 7796
	public bool hasObject;

	// Token: 0x04001E75 RID: 7797
	public GameObject prefab;

	// Token: 0x04001E76 RID: 7798
	public GameObject colliderPrefab;

	// Token: 0x04001E77 RID: 7799
	public bool independentCollider;

	// Token: 0x04001E78 RID: 7800
	public Mesh mesh;

	// Token: 0x04001E79 RID: 7801
	public Mesh[] meshes;

	// Token: 0x04001E7A RID: 7802
	public Material[] materials;

	// Token: 0x04001E7B RID: 7803
	public int lodCount;

	// Token: 0x04001E7C RID: 7804
	public Mesh[] lodMeshes;

	// Token: 0x04001E7D RID: 7805
	public float[] lodDistances;

	// Token: 0x04001E7E RID: 7806
	public Material[][] lodMaterials;

	// Token: 0x04001E7F RID: 7807
	public Material[][] lodBlueprintMaterials;

	// Token: 0x04001E80 RID: 7808
	public bool[][] lodSubmeshIgnores;

	// Token: 0x04001E81 RID: 7809
	public VertaBuffer[] lodVertas;

	// Token: 0x04001E82 RID: 7810
	public Mesh starmapMesh;

	// Token: 0x04001E83 RID: 7811
	public VertaBuffer starmapVerta;

	// Token: 0x04001E84 RID: 7812
	public Material starmapMaterial;

	// Token: 0x04001E85 RID: 7813
	public bool disableStarmapCulling;

	// Token: 0x04001E86 RID: 7814
	public Mesh physCollider;

	// Token: 0x04001E87 RID: 7815
	public Mesh meshCollider;

	// Token: 0x04001E88 RID: 7816
	public int startInstCapacity = 256;

	// Token: 0x04001E89 RID: 7817
	public int batchCapacity = 2048;

	// Token: 0x04001E8A RID: 7818
	public float cullingHeight;

	// Token: 0x04001E8B RID: 7819
	public int castShadow;

	// Token: 0x04001E8C RID: 7820
	public int recvShadow;

	// Token: 0x04001E8D RID: 7821
	public ColliderData[] colliders;

	// Token: 0x04001E8E RID: 7822
	public bool hasBuildCollider;

	// Token: 0x04001E8F RID: 7823
	public ColliderData buildCollider;

	// Token: 0x04001E90 RID: 7824
	public ColliderData[] buildColliders;

	// Token: 0x04001E91 RID: 7825
	public ColliderDataLF[] spacePhysicsColliders;

	// Token: 0x04001E92 RID: 7826
	public ColliderDataLF[] spaceBuildColliders;

	// Token: 0x04001E93 RID: 7827
	public float roughRadius;

	// Token: 0x04001E94 RID: 7828
	public float roughHeight;

	// Token: 0x04001E95 RID: 7829
	public float roughWidth;

	// Token: 0x04001E96 RID: 7830
	public int colliderComplexity;

	// Token: 0x04001E97 RID: 7831
	public float barWidth;

	// Token: 0x04001E98 RID: 7832
	public float barHeight;

	// Token: 0x04001E99 RID: 7833
	public Vector3[] landPoints;

	// Token: 0x04001E9A RID: 7834
	public float landOffset;

	// Token: 0x04001E9B RID: 7835
	public bool allowBuildInWater;

	// Token: 0x04001E9C RID: 7836
	public bool needBuildInWaterTech;

	// Token: 0x04001E9D RID: 7837
	public Vector3[] waterPoints;

	// Token: 0x04001E9E RID: 7838
	public int[] waterTypes;

	// Token: 0x04001E9F RID: 7839
	public bool multiLevel;

	// Token: 0x04001EA0 RID: 7840
	public Vector3 lapJoint;

	// Token: 0x04001EA1 RID: 7841
	public bool multiLevelAllowPortsOrSlots;

	// Token: 0x04001EA2 RID: 7842
	public bool multiLevelAllowRotate;

	// Token: 0x04001EA3 RID: 7843
	public int[] multiLevelAlternativeIds;

	// Token: 0x04001EA4 RID: 7844
	public bool[] multiLevelAlternativeYawTransposes;

	// Token: 0x04001EA5 RID: 7845
	public EAddonType addonType;

	// Token: 0x04001EA6 RID: 7846
	public bool veinMiner;

	// Token: 0x04001EA7 RID: 7847
	public bool oilMiner;

	// Token: 0x04001EA8 RID: 7848
	public bool dragBuild;

	// Token: 0x04001EA9 RID: 7849
	public bool dragBuildByGrid;

	// Token: 0x04001EAA RID: 7850
	public bool dragBuildByRect;

	// Token: 0x04001EAB RID: 7851
	public Vector2 dragBuildDist;

	// Token: 0x04001EAC RID: 7852
	public IntVector2 dragBuildGridDistOverride;

	// Token: 0x04001EAD RID: 7853
	public Vector2 blueprintBoxSize;

	// Token: 0x04001EAE RID: 7854
	public int shiftSnapGridInterval;

	// Token: 0x04001EAF RID: 7855
	public bool isBelt;

	// Token: 0x04001EB0 RID: 7856
	public int beltSpeed;

	// Token: 0x04001EB1 RID: 7857
	public int beltPrototype;

	// Token: 0x04001EB2 RID: 7858
	public bool isSplitter;

	// Token: 0x04001EB3 RID: 7859
	public bool isMonitor;

	// Token: 0x04001EB4 RID: 7860
	public bool isSpeaker;

	// Token: 0x04001EB5 RID: 7861
	public bool isSpraycoster;

	// Token: 0x04001EB6 RID: 7862
	public bool isPiler;

	// Token: 0x04001EB7 RID: 7863
	public bool isStorage;

	// Token: 0x04001EB8 RID: 7864
	public int storageCol;

	// Token: 0x04001EB9 RID: 7865
	public int storageRow;

	// Token: 0x04001EBA RID: 7866
	public EMinerType minerType;

	// Token: 0x04001EBB RID: 7867
	public int minerPeriod;

	// Token: 0x04001EBC RID: 7868
	public bool isInserter;

	// Token: 0x04001EBD RID: 7869
	public int inserterGrade;

	// Token: 0x04001EBE RID: 7870
	public int inserterSTT;

	// Token: 0x04001EBF RID: 7871
	public int inserterDelay;

	// Token: 0x04001EC0 RID: 7872
	public bool inserterCanStack;

	// Token: 0x04001EC1 RID: 7873
	public int inserterStackSize;

	// Token: 0x04001EC2 RID: 7874
	public bool isAssembler;

	// Token: 0x04001EC3 RID: 7875
	public int assemblerSpeed;

	// Token: 0x04001EC4 RID: 7876
	public ERecipeType assemblerRecipeType;

	// Token: 0x04001EC5 RID: 7877
	public bool isFractionator;

	// Token: 0x04001EC6 RID: 7878
	public int fracFluidInputMax;

	// Token: 0x04001EC7 RID: 7879
	public int fracProductOutputMax;

	// Token: 0x04001EC8 RID: 7880
	public int fracFluidOutputMax;

	// Token: 0x04001EC9 RID: 7881
	public bool isLab;

	// Token: 0x04001ECA RID: 7882
	public int labAssembleSpeed;

	// Token: 0x04001ECB RID: 7883
	public float labResearchSpeed;

	// Token: 0x04001ECC RID: 7884
	public bool isTank;

	// Token: 0x04001ECD RID: 7885
	public int fluidStorageCount;

	// Token: 0x04001ECE RID: 7886
	public float anim_prepare_length;

	// Token: 0x04001ECF RID: 7887
	public float anim_working_length = 1f;

	// Token: 0x04001ED0 RID: 7888
	public bool isPowerNode;

	// Token: 0x04001ED1 RID: 7889
	public float powerConnectDistance;

	// Token: 0x04001ED2 RID: 7890
	public float powerCoverRadius;

	// Token: 0x04001ED3 RID: 7891
	public Vector3 powerPoint;

	// Token: 0x04001ED4 RID: 7892
	public bool isPowerGen;

	// Token: 0x04001ED5 RID: 7893
	public bool photovoltaic;

	// Token: 0x04001ED6 RID: 7894
	public bool windForcedPower;

	// Token: 0x04001ED7 RID: 7895
	public bool gammaRayReceiver;

	// Token: 0x04001ED8 RID: 7896
	public bool geothermal;

	// Token: 0x04001ED9 RID: 7897
	public long genEnergyPerTick;

	// Token: 0x04001EDA RID: 7898
	public long useFuelPerTick;

	// Token: 0x04001EDB RID: 7899
	public int fuelMask;

	// Token: 0x04001EDC RID: 7900
	public int powerCatalystId;

	// Token: 0x04001EDD RID: 7901
	public int powerProductId;

	// Token: 0x04001EDE RID: 7902
	public long powerProductHeat;

	// Token: 0x04001EDF RID: 7903
	public bool isAccumulator;

	// Token: 0x04001EE0 RID: 7904
	public long inputEnergyPerTick;

	// Token: 0x04001EE1 RID: 7905
	public long outputEnergyPerTick;

	// Token: 0x04001EE2 RID: 7906
	public long maxAcuEnergy;

	// Token: 0x04001EE3 RID: 7907
	public bool isPowerConsumer;

	// Token: 0x04001EE4 RID: 7908
	public bool isPowerCharger;

	// Token: 0x04001EE5 RID: 7909
	public long workEnergyPerTick;

	// Token: 0x04001EE6 RID: 7910
	public long idleEnergyPerTick;

	// Token: 0x04001EE7 RID: 7911
	public bool isPowerExchanger;

	// Token: 0x04001EE8 RID: 7912
	public long exchangeEnergyPerTick;

	// Token: 0x04001EE9 RID: 7913
	public int emptyId;

	// Token: 0x04001EEA RID: 7914
	public int fullId;

	// Token: 0x04001EEB RID: 7915
	public long maxExcEnergy;

	// Token: 0x04001EEC RID: 7916
	public bool isStation;

	// Token: 0x04001EED RID: 7917
	public bool isStellarStation;

	// Token: 0x04001EEE RID: 7918
	public int stationMaxItemCount;

	// Token: 0x04001EEF RID: 7919
	public int stationMaxItemKinds;

	// Token: 0x04001EF0 RID: 7920
	public int stationMaxDroneCount;

	// Token: 0x04001EF1 RID: 7921
	public int stationMaxShipCount;

	// Token: 0x04001EF2 RID: 7922
	public long stationMaxEnergyAcc;

	// Token: 0x04001EF3 RID: 7923
	public Vector3 stationDronePos;

	// Token: 0x04001EF4 RID: 7924
	public Vector3 stationShipPos;

	// Token: 0x04001EF5 RID: 7925
	public bool isCollectStation;

	// Token: 0x04001EF6 RID: 7926
	public int stationCollectSpeed;

	// Token: 0x04001EF7 RID: 7927
	public int[] stationCollectIds;

	// Token: 0x04001EF8 RID: 7928
	public bool isVeinCollector;

	// Token: 0x04001EF9 RID: 7929
	public bool isDispenser;

	// Token: 0x04001EFA RID: 7930
	public int dispenserMaxCourierCount;

	// Token: 0x04001EFB RID: 7931
	public long dispenserMaxEnergyAcc;

	// Token: 0x04001EFC RID: 7932
	public bool isEjector;

	// Token: 0x04001EFD RID: 7933
	public float ejectorPivotY;

	// Token: 0x04001EFE RID: 7934
	public float ejectorMuzzleY;

	// Token: 0x04001EFF RID: 7935
	public int ejectorChargeFrame;

	// Token: 0x04001F00 RID: 7936
	public int ejectorColdFrame;

	// Token: 0x04001F01 RID: 7937
	public int ejectorBulletId;

	// Token: 0x04001F02 RID: 7938
	public bool isSilo;

	// Token: 0x04001F03 RID: 7939
	public int siloChargeFrame;

	// Token: 0x04001F04 RID: 7940
	public int siloColdFrame;

	// Token: 0x04001F05 RID: 7941
	public int siloBulletId;

	// Token: 0x04001F06 RID: 7942
	public int minimapType;

	// Token: 0x04001F07 RID: 7943
	public Vector4 minimapScl = Vector4.zero;

	// Token: 0x04001F08 RID: 7944
	public Pose[] portPoses;

	// Token: 0x04001F09 RID: 7945
	public Pose[] slotPoses;

	// Token: 0x04001F0A RID: 7946
	public Vector3 selectCenter = Vector3.zero;

	// Token: 0x04001F0B RID: 7947
	public Vector3 selectSize = Vector3.one;

	// Token: 0x04001F0C RID: 7948
	public float selectAlpha;

	// Token: 0x04001F0D RID: 7949
	public float selectDistance = 35f;

	// Token: 0x04001F0E RID: 7950
	public float signHeight;

	// Token: 0x04001F0F RID: 7951
	public float signSize;

	// Token: 0x04001F10 RID: 7952
	public Pose[] addonAreaPoses;

	// Token: 0x04001F11 RID: 7953
	public Pose[] addonAreaColPoses;

	// Token: 0x04001F12 RID: 7954
	public Vector3[] addonAreaSize;

	// Token: 0x04001F13 RID: 7955
	public bool hasAudio;

	// Token: 0x04001F14 RID: 7956
	public int audioProtoId0;

	// Token: 0x04001F15 RID: 7957
	public int audioProtoId1;

	// Token: 0x04001F16 RID: 7958
	public int audioProtoId2;

	// Token: 0x04001F17 RID: 7959
	public int audioLogic;

	// Token: 0x04001F18 RID: 7960
	public float audioRadius0;

	// Token: 0x04001F19 RID: 7961
	public float audioRadius1;

	// Token: 0x04001F1A RID: 7962
	public float audioFalloff;

	// Token: 0x04001F1B RID: 7963
	public float audioVolume;

	// Token: 0x04001F1C RID: 7964
	public float audioPitch;

	// Token: 0x04001F1D RID: 7965
	public float audioDoppler;

	// Token: 0x04001F1E RID: 7966
	public int monitorOffset;

	// Token: 0x04001F1F RID: 7967
	public int monitorTargetCargoBytes;

	// Token: 0x04001F20 RID: 7968
	public int monitorPeriodTickCount;

	// Token: 0x04001F21 RID: 7969
	public int monitorPassOperator;

	// Token: 0x04001F22 RID: 7970
	public int monitorPassColorId;

	// Token: 0x04001F23 RID: 7971
	public int monitorFailColorId;

	// Token: 0x04001F24 RID: 7972
	public int monitorSystemWarningMode;

	// Token: 0x04001F25 RID: 7973
	public int monitorMode;

	// Token: 0x04001F26 RID: 7974
	public int monitorCargoFilter;

	// Token: 0x04001F27 RID: 7975
	public int monitorSignalId;

	// Token: 0x04001F28 RID: 7976
	public int monitorSpawnOperator;

	// Token: 0x04001F29 RID: 7977
	public int speakerTone;

	// Token: 0x04001F2A RID: 7978
	public int speakerVolume;

	// Token: 0x04001F2B RID: 7979
	public int speakerPitch;

	// Token: 0x04001F2C RID: 7980
	public float speakerLength;

	// Token: 0x04001F2D RID: 7981
	public bool speakerRepeat;

	// Token: 0x04001F2E RID: 7982
	public int incCapacity;

	// Token: 0x04001F2F RID: 7983
	public int[] incItemId;

	// Token: 0x04001F30 RID: 7984
	public bool isRuin;

	// Token: 0x04001F31 RID: 7985
	public int ruinOriginModelId;

	// Token: 0x04001F32 RID: 7986
	public WreckageHandler wreckagePrefab;

	// Token: 0x04001F33 RID: 7987
	public WreckageHandler[] wreckagePrefabs;

	// Token: 0x04001F34 RID: 7988
	public WreckageFragment[][] wreckageFragments;

	// Token: 0x04001F35 RID: 7989
	public bool isSpacecraft;

	// Token: 0x04001F36 RID: 7990
	public bool isEnemy;

	// Token: 0x04001F37 RID: 7991
	public int enemyProtoId;

	// Token: 0x04001F38 RID: 7992
	public float enemySelectCircleRadius;

	// Token: 0x04001F39 RID: 7993
	public int enemySandCount;

	// Token: 0x04001F3A RID: 7994
	public bool isEnemyBuilder;

	// Token: 0x04001F3B RID: 7995
	public int enemyMinMatter;

	// Token: 0x04001F3C RID: 7996
	public int enemyMinEnergy;

	// Token: 0x04001F3D RID: 7997
	public int enemyMaxMatter;

	// Token: 0x04001F3E RID: 7998
	public int enemyMaxEnergy;

	// Token: 0x04001F3F RID: 7999
	public int enemySpMatter;

	// Token: 0x04001F40 RID: 8000
	public int enemySpEnergy;

	// Token: 0x04001F41 RID: 8001
	public int enemySpMax;

	// Token: 0x04001F42 RID: 8002
	public int enemyIdleEnergy;

	// Token: 0x04001F43 RID: 8003
	public int enemyWorkEnergy;

	// Token: 0x04001F44 RID: 8004
	public int enemyGenMatter;

	// Token: 0x04001F45 RID: 8005
	public int enemyGenEnergy;

	// Token: 0x04001F46 RID: 8006
	public bool isDFGroundBase;

	// Token: 0x04001F47 RID: 8007
	public bool isDFGroundConnector;

	// Token: 0x04001F48 RID: 8008
	public bool isDFGroundReplicator;

	// Token: 0x04001F49 RID: 8009
	public int dfReplicatorProductId;

	// Token: 0x04001F4A RID: 8010
	public int dfReplicatorProductSpMatter;

	// Token: 0x04001F4B RID: 8011
	public int dfReplicatorProductSpEnergy;

	// Token: 0x04001F4C RID: 8012
	public int dfReplicatorProductSpMax;

	// Token: 0x04001F4D RID: 8013
	public int dfReplicatorTurboSpeed;

	// Token: 0x04001F4E RID: 8014
	public int dfReplicatorUnitSupply;

	// Token: 0x04001F4F RID: 8015
	public Vector3 dfReplicatorProductInitialPos;

	// Token: 0x04001F50 RID: 8016
	public Quaternion dfReplicatorProductInitialRot;

	// Token: 0x04001F51 RID: 8017
	public Vector3 dfReplicatorProductInitialVel;

	// Token: 0x04001F52 RID: 8018
	public int dfReplicatorProductInitialTick;

	// Token: 0x04001F53 RID: 8019
	public bool isDFGroundTurret;

	// Token: 0x04001F54 RID: 8020
	public EDFTurretType dfTurretType;

	// Token: 0x04001F55 RID: 8021
	public float dfTurretAttackRange;

	// Token: 0x04001F56 RID: 8022
	public float dfTurretSensorRange;

	// Token: 0x04001F57 RID: 8023
	public float dfTurretRangeInc;

	// Token: 0x04001F58 RID: 8024
	public int dfTurretAttackEnergy;

	// Token: 0x04001F59 RID: 8025
	public int dfTurretAttackInterval;

	// Token: 0x04001F5A RID: 8026
	public int dfTurretAttackHeat;

	// Token: 0x04001F5B RID: 8027
	public int dfTurretAttackDamage;

	// Token: 0x04001F5C RID: 8028
	public int dfTurretAttackDamageInc;

	// Token: 0x04001F5D RID: 8029
	public int dfTurretColdSpeed;

	// Token: 0x04001F5E RID: 8030
	public int dfTurretColdSpeedInc;

	// Token: 0x04001F5F RID: 8031
	public float dfTurretMuzzleY;

	// Token: 0x04001F60 RID: 8032
	public bool isDFGroundShield;

	// Token: 0x04001F61 RID: 8033
	public bool isDFSpaceCore;

	// Token: 0x04001F62 RID: 8034
	public int dfsCoreBuildRelaySpMatter;

	// Token: 0x04001F63 RID: 8035
	public int dfsCoreBuildRelaySpEnergy;

	// Token: 0x04001F64 RID: 8036
	public int dfsCoreBuildRelaySpMax;

	// Token: 0x04001F65 RID: 8037
	public int dfsCoreBuildTinderSpMatter;

	// Token: 0x04001F66 RID: 8038
	public int dfsCoreBuildTinderSpEnergy;

	// Token: 0x04001F67 RID: 8039
	public int dfsCoreBuildTinderSpMax;

	// Token: 0x04001F68 RID: 8040
	public int dfsCoreBuildTinderTriggerMinTick;

	// Token: 0x04001F69 RID: 8041
	public int dfsCoreBuildTinderTriggerKeyTick;

	// Token: 0x04001F6A RID: 8042
	public float dfsCoreBuildTinderTriggerProbability;

	// Token: 0x04001F6B RID: 8043
	public bool isDFSpaceNode;

	// Token: 0x04001F6C RID: 8044
	public bool isDFSpaceConnector;

	// Token: 0x04001F6D RID: 8045
	public bool isDFSpaceConnectorVertical;

	// Token: 0x04001F6E RID: 8046
	public bool isDFSpaceReplicator;

	// Token: 0x04001F6F RID: 8047
	public bool isDFSpaceGammaReceiver;

	// Token: 0x04001F70 RID: 8048
	public bool isDFSpaceTurret;

	// Token: 0x04001F71 RID: 8049
	public bool isDFRelay;

	// Token: 0x04001F72 RID: 8050
	public bool isDFTinder;

	// Token: 0x04001F73 RID: 8051
	public bool isEnemyUnit;

	// Token: 0x04001F74 RID: 8052
	public float unitMaxMovementSpeed;

	// Token: 0x04001F75 RID: 8053
	public float unitMaxMovementAcceleration;

	// Token: 0x04001F76 RID: 8054
	public float unitMarchMovementSpeed;

	// Token: 0x04001F77 RID: 8055
	public float unitAssaultArriveRange;

	// Token: 0x04001F78 RID: 8056
	public float unitEngageArriveRange;

	// Token: 0x04001F79 RID: 8057
	public float unitSensorRange;

	// Token: 0x04001F7A RID: 8058
	public float unitAttackRange0;

	// Token: 0x04001F7B RID: 8059
	public int unitAttackInterval0;

	// Token: 0x04001F7C RID: 8060
	public int unitAttackHeat0;

	// Token: 0x04001F7D RID: 8061
	public int unitAttackDamage0;

	// Token: 0x04001F7E RID: 8062
	public int unitAttackDamageInc0;

	// Token: 0x04001F7F RID: 8063
	public float unitAttackRange1;

	// Token: 0x04001F80 RID: 8064
	public int unitAttackInterval1;

	// Token: 0x04001F81 RID: 8065
	public int unitAttackHeat1;

	// Token: 0x04001F82 RID: 8066
	public int unitAttackDamage1;

	// Token: 0x04001F83 RID: 8067
	public int unitAttackDamageInc1;

	// Token: 0x04001F84 RID: 8068
	public float unitAttackRange2;

	// Token: 0x04001F85 RID: 8069
	public int unitAttackInterval2;

	// Token: 0x04001F86 RID: 8070
	public int unitAttackHeat2;

	// Token: 0x04001F87 RID: 8071
	public int unitAttackDamage2;

	// Token: 0x04001F88 RID: 8072
	public int unitAttackDamageInc2;

	// Token: 0x04001F89 RID: 8073
	public int unitColdSpeed;

	// Token: 0x04001F8A RID: 8074
	public int unitColdSpeedInc;

	// Token: 0x04001F8B RID: 8075
	public bool isFleet;

	// Token: 0x04001F8C RID: 8076
	public float fleetInitializeUnitSpeedScale;

	// Token: 0x04001F8D RID: 8077
	public float fleetSensorRange;

	// Token: 0x04001F8E RID: 8078
	public float fleetMaxActiveArea;

	// Token: 0x04001F8F RID: 8079
	public float fleetMaxMovementSpeed;

	// Token: 0x04001F90 RID: 8080
	public float fleetMaxMovementAcceleration;

	// Token: 0x04001F91 RID: 8081
	public float fleetMaxRotateAcceleration;

	// Token: 0x04001F92 RID: 8082
	public int fleetMaxAssembleCount;

	// Token: 0x04001F93 RID: 8083
	public int fleetMaxChargingCount;

	// Token: 0x04001F94 RID: 8084
	public FleetPortDesc[] fleetPorts;

	// Token: 0x04001F95 RID: 8085
	public bool isCraftUnit;

	// Token: 0x04001F96 RID: 8086
	public ECraftSize craftUnitSize;

	// Token: 0x04001F97 RID: 8087
	public int craftUnitInitializeSpeed;

	// Token: 0x04001F98 RID: 8088
	public float craftUnitMaxMovementSpeed;

	// Token: 0x04001F99 RID: 8089
	public float craftUnitMaxMovementAcceleration;

	// Token: 0x04001F9A RID: 8090
	public float craftUnitMaxRotateAcceleration;

	// Token: 0x04001F9B RID: 8091
	public float craftUnitAttackRange0;

	// Token: 0x04001F9C RID: 8092
	public float craftUnitAttackRange1;

	// Token: 0x04001F9D RID: 8093
	public float craftUnitSensorRange;

	// Token: 0x04001F9E RID: 8094
	public int craftUnitROF0;

	// Token: 0x04001F9F RID: 8095
	public int craftUnitROF1;

	// Token: 0x04001FA0 RID: 8096
	public int craftUnitRoundInterval0;

	// Token: 0x04001FA1 RID: 8097
	public int craftUnitRoundInterval1;

	// Token: 0x04001FA2 RID: 8098
	public int craftUnitMuzzleInterval0;

	// Token: 0x04001FA3 RID: 8099
	public int craftUnitMuzzleInterval1;

	// Token: 0x04001FA4 RID: 8100
	public int craftUnitMuzzleCount0;

	// Token: 0x04001FA5 RID: 8101
	public int craftUnitMuzzleCount1;

	// Token: 0x04001FA6 RID: 8102
	public int craftUnitAttackDamage0;

	// Token: 0x04001FA7 RID: 8103
	public int craftUnitAttackDamage1;

	// Token: 0x04001FA8 RID: 8104
	public int craftUnitEnergyPerTick;

	// Token: 0x04001FA9 RID: 8105
	public int craftUnitFireEnergy0;

	// Token: 0x04001FAA RID: 8106
	public int craftUnitFireEnergy1;

	// Token: 0x04001FAB RID: 8107
	public int craftUnitRepairEnergyPerHP;

	// Token: 0x04001FAC RID: 8108
	public int craftUnitRepairHPPerTick;

	// Token: 0x04001FAD RID: 8109
	public int craftUnitAddEnemyExppBase;

	// Token: 0x04001FAE RID: 8110
	public int craftUnitAddEnemyThreatBase;

	// Token: 0x04001FAF RID: 8111
	public int craftUnitAddEnemyHatredBase;

	// Token: 0x04001FB0 RID: 8112
	public float craftUnitAddEnemyExppCoef;

	// Token: 0x04001FB1 RID: 8113
	public float craftUnitAddEnemyThreatCoef;

	// Token: 0x04001FB2 RID: 8114
	public float craftUnitAddEnemyHatredCoef;

	// Token: 0x04001FB3 RID: 8115
	public bool isTurret;

	// Token: 0x04001FB4 RID: 8116
	public ETurretType turretType;

	// Token: 0x04001FB5 RID: 8117
	public EAmmoType turretAmmoType;

	// Token: 0x04001FB6 RID: 8118
	public VSLayerMask turretVSCaps;

	// Token: 0x04001FB7 RID: 8119
	public Vector3 turretDefaultDir;

	// Token: 0x04001FB8 RID: 8120
	public int turretROF;

	// Token: 0x04001FB9 RID: 8121
	public int turretRoundInterval;

	// Token: 0x04001FBA RID: 8122
	public int turretMuzzleInterval;

	// Token: 0x04001FBB RID: 8123
	public byte turretMuzzleCount;

	// Token: 0x04001FBC RID: 8124
	public float turretMinAttackRange;

	// Token: 0x04001FBD RID: 8125
	public float turretMaxAttackRange;

	// Token: 0x04001FBE RID: 8126
	public float turretSpaceAttackRange;

	// Token: 0x04001FBF RID: 8127
	public float turretPitchUpMax;

	// Token: 0x04001FC0 RID: 8128
	public float turretPitchDownMax;

	// Token: 0x04001FC1 RID: 8129
	public float turretDamageScale;

	// Token: 0x04001FC2 RID: 8130
	public float turretMuzzleY;

	// Token: 0x04001FC3 RID: 8131
	public float turretAimSpeed;

	// Token: 0x04001FC4 RID: 8132
	public float turretUniformAngleSpeed;

	// Token: 0x04001FC5 RID: 8133
	public float turretAngleAcc;

	// Token: 0x04001FC6 RID: 8134
	public int turretAddEnemyExppBase;

	// Token: 0x04001FC7 RID: 8135
	public int turretAddEnemyThreatBase;

	// Token: 0x04001FC8 RID: 8136
	public int turretAddEnemyHatredBase;

	// Token: 0x04001FC9 RID: 8137
	public float turretAddEnemyExppCoef;

	// Token: 0x04001FCA RID: 8138
	public float turretAddEnemyThreatCoef;

	// Token: 0x04001FCB RID: 8139
	public float turretAddEnemyHatredCoef;

	// Token: 0x04001FCC RID: 8140
	public bool isBeacon;

	// Token: 0x04001FCD RID: 8141
	public float beaconSignalRadius;

	// Token: 0x04001FCE RID: 8142
	public int beaconROF;

	// Token: 0x04001FCF RID: 8143
	public float beaconSpaceSignalRange;

	// Token: 0x04001FD0 RID: 8144
	public float beaconPitchUpMax;

	// Token: 0x04001FD1 RID: 8145
	public float beaconPitchDownMax;

	// Token: 0x04001FD2 RID: 8146
	public bool isFieldGenerator;

	// Token: 0x04001FD3 RID: 8147
	public long fieldGenEnergyCapacity;

	// Token: 0x04001FD4 RID: 8148
	public long fieldGenEnergyRequire0;

	// Token: 0x04001FD5 RID: 8149
	public long fieldGenEnergyRequire1;

	// Token: 0x04001FD6 RID: 8150
	public bool isBattleBase;

	// Token: 0x04001FD7 RID: 8151
	public long battleBaseMaxEnergyAcc;

	// Token: 0x04001FD8 RID: 8152
	public float battleBasePickRange;

	// Token: 0x04001FD9 RID: 8153
	public bool isAmmo;

	// Token: 0x04001FDA RID: 8154
	public float AmmoBlastRadius0;

	// Token: 0x04001FDB RID: 8155
	public float AmmoBlastRadius1;

	// Token: 0x04001FDC RID: 8156
	public float AmmoBlastFalloff;

	// Token: 0x04001FDD RID: 8157
	public float AmmoMoveAcc;

	// Token: 0x04001FDE RID: 8158
	public float AmmoTurnAcc;

	// Token: 0x04001FDF RID: 8159
	public int AmmoHitIndex;

	// Token: 0x04001FE0 RID: 8160
	public int AmmoParameter0;

	// Token: 0x04001FE1 RID: 8161
	public bool isConstructionModule;

	// Token: 0x04001FE2 RID: 8162
	public int constructionDroneCount;

	// Token: 0x04001FE3 RID: 8163
	public float constructionRange;

	// Token: 0x04001FE4 RID: 8164
	public Vector3 constructionDroneEjectPos;

	// Token: 0x04001FE5 RID: 8165
	public bool isConstructionDrone;

	// Token: 0x04001FE6 RID: 8166
	public bool isCombatModule;

	// Token: 0x04001FE7 RID: 8167
	public int combatModuleFleetProtoId;

	// Token: 0x04001FE8 RID: 8168
	public Transform[] combatModuleFleetTrans;

	// Token: 0x04001FE9 RID: 8169
	public CreationPartPropertyBlock creationBlock;

	// Token: 0x04001FEA RID: 8170
	public bool isMarker;

	// Token: 0x04001FEB RID: 8171
	public float markerPointDefaultHeight;

	// Token: 0x04001FEC RID: 8172
	public float markerPointDefaultRadius;
}
