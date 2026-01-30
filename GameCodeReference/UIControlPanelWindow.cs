using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000704 RID: 1796
public class UIControlPanelWindow : ManualBehaviour
{
	// Token: 0x1700074B RID: 1867
	// (get) Token: 0x060048B4 RID: 18612 RVA: 0x003AB0FD File Offset: 0x003A92FD
	// (set) Token: 0x060048B5 RID: 18613 RVA: 0x003AB105 File Offset: 0x003A9305
	public ControlPanelTarget selection
	{
		get
		{
			return this._selection;
		}
		set
		{
			if (this._selection != value)
			{
				this._selection = value;
				this.OnSelectionChange();
			}
		}
	}

	// Token: 0x060048B6 RID: 18614 RVA: 0x003AB124 File Offset: 0x003A9324
	protected override void _OnCreate()
	{
		this.filterPanel._Create();
		this.astroExpands = new HashSet<int>();
		this.sortedAstros = new List<ControlPanelAstroData>();
		this.results = new List<ControlPanelTarget>();
		this.resultPositions = new List<int>();
		this.resultEntries = new List<UIControlPanelObjectEntry>();
		this.objectEntryPool = new List<UIControlPanelObjectEntry>();
		int num = 7;
		this.objectEntryPrefabHeights = new int[num];
		for (int i = 0; i < num; i++)
		{
			this.objectEntryPrefabHeights[i] = (int)(this.objectEntryPrefabs[i].rectTransform.sizeDelta.y + 0.5f);
		}
		this.stationInspector._Create();
		this.dispenserInspector._Create();
		this.markerInspector._Create();
	}

	// Token: 0x060048B7 RID: 18615 RVA: 0x003AB1E0 File Offset: 0x003A93E0
	protected override void _OnDestroy()
	{
		this.astroExpands = null;
		this.sortedAstros = null;
		this.results = null;
		this.resultPositions = null;
		this.resultEntries = null;
		for (int i = 0; i < this.objectEntryPool.Count; i++)
		{
			if (this.objectEntryPool[i] != null)
			{
				this.objectEntryPool[i]._Destroy();
				this.objectEntryPool[i] = null;
			}
		}
		this.objectEntryPool = null;
		this.objectEntryPrefabHeights = null;
		this.stationInspector._Destroy();
		this.dispenserInspector._Destroy();
		this.markerInspector._Destroy();
		this.filterPanel._Destroy();
	}

	// Token: 0x060048B8 RID: 18616 RVA: 0x003AB294 File Offset: 0x003A9494
	protected override bool _OnInit()
	{
		this.gameData = (base.data as GameData);
		this.filterPanel._Init(this.gameData);
		this.ClearFilterResults();
		this.astroExpands.Clear();
		foreach (int item in this.gameData.preferences.uiControlPanelAstroExpands)
		{
			this.astroExpands.Add(item);
		}
		this.selection = ControlPanelTarget.none;
		this.stationInspector._Init(base.data);
		this.dispenserInspector._Init(base.data);
		this.markerInspector._Init(base.data);
		return true;
	}

	// Token: 0x060048B9 RID: 18617 RVA: 0x003AB36C File Offset: 0x003A956C
	protected override void _OnFree()
	{
		this.filterPanel._Free();
		this.filter = ControlPanelFilter.Default;
		this.ClearFilterResults();
		this.selection = ControlPanelTarget.none;
		this.gameData = null;
		this.stationInspector._Free();
		this.dispenserInspector._Free();
		this.markerInspector._Free();
	}

	// Token: 0x060048BA RID: 18618 RVA: 0x003AB3C8 File Offset: 0x003A95C8
	protected override void _OnRegEvent()
	{
	}

	// Token: 0x060048BB RID: 18619 RVA: 0x003AB3CA File Offset: 0x003A95CA
	protected override void _OnUnregEvent()
	{
	}

	// Token: 0x060048BC RID: 18620 RVA: 0x003AB3CC File Offset: 0x003A95CC
	protected override void _OnOpen()
	{
		this.filterPanel._Open();
		this.DetermineFilterResults();
		this.DetermineEntryVisible();
		this.DetermineSelectionInspector();
		this.UpdateAllObjectEntries();
		PlanetFactory.onFactoryBuildEntitySingly += this.OnEntityChange;
		PlanetFactory.beforeFactoryDismantleObject += this.OnEntityChange;
		PlanetFactory.beforeFactoryKillEntity += this.OnEntityChange;
		ConstructionSystem.onFactoryBatchBuild = (Action<PlanetFactory>)Delegate.Combine(ConstructionSystem.onFactoryBatchBuild, new Action<PlanetFactory>(this.OnBatchBuild));
		Localization.OnLanguageChange += this.OnLanguageChanged;
	}

	// Token: 0x060048BD RID: 18621 RVA: 0x003AB460 File Offset: 0x003A9660
	protected override void _OnClose()
	{
		this.filterPanel._Close();
		if (this.currentInspector)
		{
			this.currentInspector._Close();
		}
		this.ClearFilterResults();
		this.ResetObjectEntryPool();
		this.leftViewportBeginPrev = -1;
		PlanetFactory.onFactoryBuildEntitySingly -= this.OnEntityChange;
		PlanetFactory.beforeFactoryDismantleObject -= this.OnEntityChange;
		PlanetFactory.beforeFactoryKillEntity -= this.OnEntityChange;
		ConstructionSystem.onFactoryBatchBuild = (Action<PlanetFactory>)Delegate.Remove(ConstructionSystem.onFactoryBatchBuild, new Action<PlanetFactory>(this.OnBatchBuild));
		Localization.OnLanguageChange -= this.OnLanguageChanged;
	}

	// Token: 0x060048BE RID: 18622 RVA: 0x003AB508 File Offset: 0x003A9708
	protected override void _OnUpdate()
	{
		this.filterPanel._Update();
		bool flag = this.leftViewportBegin - this.leftViewportBeginPrev != 0;
		this.leftViewportBeginPrev = this.leftViewportBegin;
		if (flag)
		{
			this.needDetermineEntryVisible = true;
		}
		if (this.needDetermineFilterResults)
		{
			this.DetermineFilterResults();
		}
		if (this.needDetermineEntryVisible)
		{
			this.DetermineEntryVisible();
		}
		if (this.needDetermineSelectionInspector)
		{
			this.DetermineSelectionInspector();
		}
		this.UpdateAllObjectEntries();
		if (this.currentInspector != null)
		{
			this.currentInspector._Update();
		}
	}

	// Token: 0x060048BF RID: 18623 RVA: 0x003AB58B File Offset: 0x003A978B
	protected override void _OnLateUpdate()
	{
		if (this.currentInspector != null)
		{
			this.currentInspector._LateUpdate();
		}
	}

	// Token: 0x1700074C RID: 1868
	// (get) Token: 0x060048C0 RID: 18624 RVA: 0x003AB5A6 File Offset: 0x003A97A6
	// (set) Token: 0x060048C1 RID: 18625 RVA: 0x003AB5BF File Offset: 0x003A97BF
	public int leftContentHeight
	{
		get
		{
			return (int)(this.leftScrollContentRect.sizeDelta.y + 0.5f);
		}
		private set
		{
			this.leftScrollContentRect.sizeDelta = new Vector2(this.leftScrollContentRect.sizeDelta.x, (float)value);
		}
	}

	// Token: 0x1700074D RID: 1869
	// (get) Token: 0x060048C2 RID: 18626 RVA: 0x003AB5E3 File Offset: 0x003A97E3
	// (set) Token: 0x060048C3 RID: 18627 RVA: 0x003AB5FC File Offset: 0x003A97FC
	public int leftViewportBegin
	{
		get
		{
			return (int)(this.leftScrollContentRect.anchoredPosition.y + 0.5f);
		}
		set
		{
			this.leftScrollContentRect.anchoredPosition = new Vector2(this.leftScrollContentRect.anchoredPosition.x, (float)value);
		}
	}

	// Token: 0x1700074E RID: 1870
	// (get) Token: 0x060048C4 RID: 18628 RVA: 0x003AB620 File Offset: 0x003A9820
	public int leftViewportEnd
	{
		get
		{
			return (int)(this.leftScrollContentRect.anchoredPosition.y + this.leftScrollViewportRect.rect.height + 0.5f);
		}
	}

	// Token: 0x060048C5 RID: 18629 RVA: 0x003AB658 File Offset: 0x003A9858
	private void UpdateLeftContentHeight()
	{
		int num;
		if (this.resultPositions.Count <= 1)
		{
			num = 0;
			this.leftEmptyTip.gameObject.SetActive(true);
		}
		else
		{
			this.leftEmptyTip.gameObject.SetActive(false);
			num = this.resultPositions[this.results.Count];
		}
		float num2 = this.leftScrollContentRect.anchoredPosition.y;
		float num3 = (float)num - this.leftScrollViewportRect.rect.height;
		float num4 = (float)num;
		if (num2 > num4)
		{
			num2 = num3;
			this.leftScrollContentRect.anchoredPosition = new Vector2(this.leftScrollContentRect.anchoredPosition.x, num2);
		}
		if (num3 > 0f && num2 > num3)
		{
			num = (int)(num2 + this.leftScrollViewportRect.rect.height + 0.5f);
		}
		this.leftContentHeight = num;
	}

	// Token: 0x060048C6 RID: 18630 RVA: 0x003AB738 File Offset: 0x003A9938
	public void UpdateAllObjectEntries()
	{
		for (int i = 0; i < this.objectEntryPool.Count; i++)
		{
			if (this.objectEntryPool[i] != null && this.objectEntryPool[i].inUse)
			{
				this.objectEntryPool[i]._Update();
			}
		}
	}

	// Token: 0x060048C7 RID: 18631 RVA: 0x003AB794 File Offset: 0x003A9994
	public void DetermineEntryVisible()
	{
		this.UpdateLeftContentHeight();
		int leftViewportBegin = this.leftViewportBegin;
		int leftViewportEnd = this.leftViewportEnd;
		this.leftViewportBeginPrev = leftViewportBegin;
		int count = this.objectEntryPool.Count;
		for (int i = 0; i < count; i++)
		{
			UIControlPanelObjectEntry uicontrolPanelObjectEntry = this.objectEntryPool[i];
			if (uicontrolPanelObjectEntry.inUse)
			{
				bool flag = false;
				if (uicontrolPanelObjectEntry.selected)
				{
					int num = this.resultPositions[uicontrolPanelObjectEntry.index];
					if (this.resultPositions[uicontrolPanelObjectEntry.index + 1] <= num)
					{
						flag = true;
					}
					else
					{
						this.resultEntries[uicontrolPanelObjectEntry.index].position = num;
					}
				}
				else
				{
					int num2 = this.resultPositions[uicontrolPanelObjectEntry.index];
					int num3 = this.resultPositions[uicontrolPanelObjectEntry.index + 1];
					if (num3 <= leftViewportBegin || num2 >= leftViewportEnd || num3 <= num2)
					{
						flag = true;
					}
				}
				if (flag)
				{
					this.resultEntries[uicontrolPanelObjectEntry.index] = null;
					this.PutObjectEntryIntoPool(uicontrolPanelObjectEntry);
				}
			}
		}
		int j = 0;
		int num4 = this.results.Count;
		int num5 = (j + num4) / 2;
		int num6 = j;
		int num7 = 16;
		while (j < num4)
		{
			if (num7-- <= 0)
			{
				break;
			}
			num6 = j;
			int num8 = this.resultPositions[num5 + 1];
			int num9 = this.resultPositions[num5];
			if (leftViewportBegin >= num8)
			{
				j = num5 + 1;
				num5 = (j + num4) / 2;
			}
			else
			{
				if (leftViewportBegin >= num9)
				{
					num6 = num5;
					break;
				}
				num4 = num5 - 1;
				num5 = (j + num4) / 2;
			}
		}
		while (this.resultPositions[num6] < leftViewportEnd && num6 < this.results.Count)
		{
			int num10 = this.resultPositions[num6];
			int num11 = this.resultPositions[num6 + 1];
			if (num11 > leftViewportBegin && num11 > num10)
			{
				if (this.resultEntries[num6] != null)
				{
					this.resultEntries[num6].position = num10;
				}
				else
				{
					UIControlPanelObjectEntry uicontrolPanelObjectEntry2 = this.TakeObjectEntryFromPool(num6, this.results[num6]);
					uicontrolPanelObjectEntry2.generation = this.resultGeneration;
					uicontrolPanelObjectEntry2.position = num10;
					this.resultEntries[num6] = uicontrolPanelObjectEntry2;
				}
			}
			num6++;
		}
		this.needDetermineEntryVisible = false;
	}

	// Token: 0x060048C8 RID: 18632 RVA: 0x003AB9FC File Offset: 0x003A9BFC
	public void DetermineFilterResults()
	{
		this.ResetObjectEntryPool();
		this.ClearFilterResults();
		int factoryCount = this.gameData.factoryCount;
		Transform parent = this.objectEntryPrefabs[0].transform.parent;
		int num = (this.gameData.localPlanet != null) ? this.gameData.localPlanet.astroId : 0;
		int num2 = (this.gameData.localStar != null) ? this.gameData.localStar.astroId : 0;
		bool hasStationType = this.filter.hasStationType;
		bool flag = (this.filter.typeFilter & ControlPanelFilter.EEntryFilter.InterstellarStation) > ControlPanelFilter.EEntryFilter.None;
		bool flag2 = (this.filter.typeFilter & ControlPanelFilter.EEntryFilter.OrbitCollector) > ControlPanelFilter.EEntryFilter.None;
		bool flag3 = (this.filter.typeFilter & ControlPanelFilter.EEntryFilter.LocalStation) > ControlPanelFilter.EEntryFilter.None;
		bool flag4 = (this.filter.typeFilter & ControlPanelFilter.EEntryFilter.VeinCollector) > ControlPanelFilter.EEntryFilter.None;
		bool flag5 = (this.filter.typeFilter & (ControlPanelFilter.EEntryFilter.LocalStation | ControlPanelFilter.EEntryFilter.VeinCollector)) > ControlPanelFilter.EEntryFilter.None;
		bool flag6 = !flag5;
		bool flag7 = (this.filter.typeFilter & ControlPanelFilter.EEntryFilter.Dispenser) > ControlPanelFilter.EEntryFilter.None;
		bool flag8 = (this.filter.typeFilter & ControlPanelFilter.EEntryFilter.Marker) > ControlPanelFilter.EEntryFilter.None;
		bool flag9 = !string.IsNullOrWhiteSpace(this.filter.tagFilter);
		bool flag10 = !string.IsNullOrWhiteSpace(this.filter.searchFilter);
		int num3 = 0;
		bool flag11 = flag10 && int.TryParse(this.filter.searchFilter, out num3);
		bool hasItemFilter = this.filter.hasItemFilter;
		bool flag12 = this.filter.stateFilter != -1;
		ref VectorLF3 ptr = ref this.gameData.mainPlayer.uPosition;
		this.sortedAstros.Clear();
		PlanetFactory[] factories = this.gameData.factories;
		AstroData[] galaxyAstros = this.gameData.spaceSector.galaxyAstros;
		for (int i = 0; i < factoryCount; i++)
		{
			int astroId = factories[i].planet.astroId;
			if (this.filter.sortMethod == ControlPanelFilter.ESortMethod.AstroDistance)
			{
				double sqrMagnitude = (galaxyAstros[astroId].uPos - ptr).sqrMagnitude;
				int num4 = this.sortedAstros.Count - 1;
				while (num4 >= 0 && sqrMagnitude < this.sortedAstros[num4].sqrDistToPlayer)
				{
					num4--;
				}
				this.sortedAstros.Insert(num4 + 1, new ControlPanelAstroData(astroId, sqrMagnitude));
			}
			else
			{
				this.sortedAstros.Add(new ControlPanelAstroData(astroId));
			}
		}
		PlanetFactory[] astrosFactory = this.gameData.galaxy.astrosFactory;
		for (int j = 0; j < this.sortedAstros.Count; j++)
		{
			PlanetFactory planetFactory = astrosFactory[this.sortedAstros[j].astroId];
			if (planetFactory != null)
			{
				PlanetData planet = planetFactory.planet;
				if (planet != null && (this.filter.astroFilter == -1 || (this.filter.astroFilter == 0 && (planet.astroId == num || (num == 0 && planet.star.astroId == num2))) || this.filter.astroFilter == planet.astroId || this.filter.astroFilter == planet.star.astroId))
				{
					ControlPanelTarget controlPanelTarget = new ControlPanelTarget(EObjectType.None, 0, planet.astroId, EControlPanelEntryType.None);
					this.AddFilterResult(ref controlPanelTarget, true);
					int count = this.results.Count;
					bool visible = (num == planet.astroId) ? (!this.astroExpands.Contains(0)) : this.astroExpands.Contains(planet.astroId);
					if (hasStationType)
					{
						StationComponent[] stationPool = planetFactory.transport.stationPool;
						int stationCursor = planetFactory.transport.stationCursor;
						EntityData[] entityPool = planetFactory.entityPool;
						ExtraInfoComponent[] buffer = planetFactory.digitalSystem.extraInfoes.buffer;
						for (int k = 1; k < stationCursor; k++)
						{
							StationComponent stationComponent = stationPool[k];
							if (stationComponent != null && stationComponent.id == k)
							{
								EControlPanelEntryType econtrolPanelEntryType = stationComponent.isStellar ? (stationComponent.isCollector ? EControlPanelEntryType.OrbitCollector : EControlPanelEntryType.InterstellarStation) : (stationComponent.isVeinCollector ? EControlPanelEntryType.VeinCollector : EControlPanelEntryType.LocalStation);
								if ((flag && econtrolPanelEntryType == EControlPanelEntryType.InterstellarStation) || (flag2 && econtrolPanelEntryType == EControlPanelEntryType.OrbitCollector) || (flag3 && econtrolPanelEntryType == EControlPanelEntryType.LocalStation) || (flag4 && econtrolPanelEntryType == EControlPanelEntryType.VeinCollector))
								{
									bool flag13 = true;
									if (flag10)
									{
										flag13 = (flag11 && stationComponent.id == num3);
										if (!flag13)
										{
											int extraInfoId = entityPool[stationComponent.entityId].extraInfoId;
											if (extraInfoId > 0)
											{
												string info = buffer[extraInfoId].info;
												if (!string.IsNullOrEmpty(info) && info.IndexOf(this.filter.searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
												{
													flag13 = true;
												}
											}
										}
									}
									if (flag13)
									{
										bool flag14 = true;
										if (hasItemFilter || flag12)
										{
											StationStore[] storage = stationComponent.storage;
											flag14 = false;
											for (int l = 0; l < storage.Length; l++)
											{
												if ((!hasItemFilter || this.filter.ItemFilterPass(storage[l].itemId)) && (!flag12 || (storage[l].itemId > 0 && ((flag6 && storage[l].remoteLogic == (ELogisticStorage)this.filter.stateFilter) || (flag5 && storage[l].localLogic == (ELogisticStorage)this.filter.stateFilter)))))
												{
													flag14 = true;
													break;
												}
											}
										}
										if (flag14)
										{
											ControlPanelTarget controlPanelTarget2 = new ControlPanelTarget(EObjectType.None, stationComponent.entityId, planet.astroId, econtrolPanelEntryType);
											this.AddFilterResult(ref controlPanelTarget2, visible);
										}
									}
								}
							}
						}
					}
					if (flag7)
					{
						DispenserComponent[] dispenserPool = planetFactory.transport.dispenserPool;
						int dispenserCursor = planetFactory.transport.dispenserCursor;
						for (int m = 1; m < dispenserCursor; m++)
						{
							DispenserComponent dispenserComponent = dispenserPool[m];
							if (dispenserComponent != null && dispenserComponent.id == m)
							{
								bool flag15 = true;
								if (flag10 && flag11)
								{
									flag15 = (dispenserComponent.id == num3);
								}
								if (flag15)
								{
									bool flag16 = true;
									if (hasItemFilter || flag12)
									{
										flag16 = false;
										if ((!hasItemFilter || this.filter.ItemFilterPass(dispenserComponent.filter) || (dispenserComponent.filter < 0 && dispenserComponent.playerMode == EPlayerDeliveryMode.Recycle)) && (!flag12 || (dispenserComponent.storageMode > EStorageDeliveryMode.None && (this.filter.stateFilter == 0 || this.filter.stateFilter == (int)dispenserComponent.storageMode)) || (dispenserComponent.playerMode > EPlayerDeliveryMode.None && this.filter.stateFilter - (int)dispenserComponent.playerMode - 9 <= 2)))
										{
											flag16 = true;
										}
									}
									if (flag16)
									{
										ControlPanelTarget controlPanelTarget3 = new ControlPanelTarget(EObjectType.None, dispenserComponent.entityId, planet.astroId, EControlPanelEntryType.Dispenser);
										this.AddFilterResult(ref controlPanelTarget3, visible);
									}
								}
							}
						}
					}
					if (flag8)
					{
						ObjectPool<MarkerComponent> markers = planetFactory.digitalSystem.markers;
						int cursor = planetFactory.digitalSystem.markers.cursor;
						for (int n = 1; n < cursor; n++)
						{
							MarkerComponent markerComponent = markers[n];
							if (markerComponent != null && markerComponent.id == n)
							{
								bool flag17 = true;
								if (flag9)
								{
									flag17 = false;
									string tags = markerComponent.tags;
									if (!string.IsNullOrEmpty(tags))
									{
										string[] array = tags.Split(';', StringSplitOptions.None);
										foreach (string b in this.filter.tagFilter.Split(';', StringSplitOptions.None))
										{
											string[] array3 = array;
											for (int num6 = 0; num6 < array3.Length; num6++)
											{
												if (array3[num6] == b)
												{
													flag17 = true;
													break;
												}
											}
											if (flag17)
											{
												break;
											}
										}
									}
								}
								bool flag18 = true;
								if (flag10)
								{
									flag18 = (flag11 && markerComponent.id == num3);
									if (!flag18)
									{
										string name = markerComponent.name;
										string text = planetFactory.ReadMarkerComment(markerComponent.id);
										if ((!string.IsNullOrEmpty(name) && name.IndexOf(this.filter.searchFilter, StringComparison.OrdinalIgnoreCase) >= 0) || (!string.IsNullOrEmpty(text) && text.IndexOf(this.filter.searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
										{
											flag18 = true;
										}
									}
								}
								if (flag17 && flag18)
								{
									ControlPanelTarget controlPanelTarget4 = new ControlPanelTarget(EObjectType.None, markerComponent.entityId, planet.astroId, EControlPanelEntryType.Marker);
									this.AddFilterResult(ref controlPanelTarget4, visible);
								}
							}
						}
					}
					if (this.results.Count == count)
					{
						this.RemoveLastFilterResult();
					}
				}
			}
		}
		this.resultGeneration++;
		this.needDetermineEntryVisible = true;
		this.needDetermineFilterResults = false;
		this.ReconnSelectionOnDetermineResults();
	}

	// Token: 0x060048C9 RID: 18633 RVA: 0x003AC272 File Offset: 0x003AA472
	public void OnSelectionChange()
	{
		this.needDetermineSelectionInspector = true;
	}

	// Token: 0x060048CA RID: 18634 RVA: 0x003AC27C File Offset: 0x003AA47C
	public void DetermineSelectionInspector()
	{
		this.needDetermineSelectionInspector = false;
		switch (this.selection.entryType)
		{
		case EControlPanelEntryType.InterstellarStation:
		case EControlPanelEntryType.OrbitCollector:
		case EControlPanelEntryType.LocalStation:
		case EControlPanelEntryType.VeinCollector:
			this.stationInspector.SetData(this.selection.astroId, this.selection.objId);
			if (this.currentInspector != this.stationInspector)
			{
				if (this.currentInspector != null)
				{
					this.currentInspector._Close();
				}
				this.currentInspector = this.stationInspector;
				goto IL_174;
			}
			goto IL_174;
		case EControlPanelEntryType.Dispenser:
			this.dispenserInspector.SetData(this.selection.astroId, this.selection.objId);
			if (this.currentInspector != this.dispenserInspector)
			{
				if (this.currentInspector != null)
				{
					this.currentInspector._Close();
				}
				this.currentInspector = this.dispenserInspector;
				goto IL_174;
			}
			goto IL_174;
		case EControlPanelEntryType.Marker:
			this.markerInspector.SetData(this.selection.astroId, this.selection.objId);
			if (this.currentInspector != this.markerInspector)
			{
				if (this.currentInspector != null)
				{
					this.currentInspector._Close();
				}
				this.currentInspector = this.markerInspector;
				goto IL_174;
			}
			goto IL_174;
		}
		if (this.currentInspector != null)
		{
			this.currentInspector._Close();
			this.currentInspector = null;
		}
		IL_174:
		if (this.currentInspector != null)
		{
			this.currentInspector._Open();
		}
	}

	// Token: 0x060048CB RID: 18635 RVA: 0x003AC418 File Offset: 0x003AA618
	public void FocusToEntryByObjId(int _astroId, int _objId, EObjectType _objType, bool isHighlight = true)
	{
		int num = -1;
		int count = this.results.Count;
		for (int i = 0; i < count; i++)
		{
			ControlPanelTarget controlPanelTarget = this.results[i];
			if (controlPanelTarget.astroId == _astroId && controlPanelTarget.objId == _objId && controlPanelTarget.objType == _objType)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			this.filterPanel.ResetAllFilterExceptMainType();
			this.DetermineEntryVisible();
			count = this.results.Count;
			for (int j = 0; j < count; j++)
			{
				ControlPanelTarget controlPanelTarget2 = this.results[j];
				if (controlPanelTarget2.astroId == _astroId && controlPanelTarget2.objId == _objId && controlPanelTarget2.objType == _objType)
				{
					num = j;
					break;
				}
			}
		}
		if (num == -1)
		{
			return;
		}
		ControlPanelTarget controlPanelTarget3 = this.results[num];
		bool flag = this.gameData.localPlanet != null && controlPanelTarget3.astroId == this.gameData.localPlanet.astroId;
		if (!(flag ? (!this.astroExpands.Contains(0)) : this.astroExpands.Contains(controlPanelTarget3.astroId)))
		{
			int num2 = num - 1;
			while (num2 > 0 && this.results[num2].entryType != EControlPanelEntryType.None)
			{
				num2--;
			}
			if (flag)
			{
				this.astroExpands.Remove(0);
			}
			else
			{
				this.astroExpands.Add(controlPanelTarget3.astroId);
			}
			this.SetPlanetEntryExpand(num2, true);
			this.needDetermineEntryVisible = false;
		}
		this.UpdateLeftContentHeight();
		int num3 = this.resultPositions[num];
		int num4 = this.resultPositions[num + 1] - this.resultPositions[num];
		int num5 = num3 - (int)(this.leftScrollViewportRect.rect.height * 0.5f + 0.5f) + (int)((float)num4 * 0.5f + 0.5f);
		float y = 1f - Mathf.Clamp01((float)num5 / (this.leftScrollContentRect.sizeDelta.y - this.leftScrollViewportRect.rect.height));
		this.leftScrollRect.normalizedPosition = new Vector2(0f, y);
		this.DetermineEntryVisible();
		if (isHighlight)
		{
			this.resultEntries[num].StartFocus();
		}
	}

	// Token: 0x060048CC RID: 18636 RVA: 0x003AC660 File Offset: 0x003AA860
	private void AddFilterResult(ref ControlPanelTarget target, bool visible)
	{
		this.results.Add(target);
		this.resultPositions.Add(this.resultPositions[this.resultPositions.Count - 1] + (visible ? this.objectEntryPrefabHeights[(int)target.entryType] : 0));
		this.resultEntries.Add(null);
	}

	// Token: 0x060048CD RID: 18637 RVA: 0x003AC6C4 File Offset: 0x003AA8C4
	private void RemoveLastFilterResult()
	{
		this.results.RemoveAt(this.results.Count - 1);
		this.resultPositions.RemoveAt(this.resultPositions.Count - 1);
		this.resultEntries.RemoveAt(this.resultEntries.Count - 1);
	}

	// Token: 0x060048CE RID: 18638 RVA: 0x003AC71C File Offset: 0x003AA91C
	public void SetPlanetEntryExpand(int index, bool expand)
	{
		if (this.results[index].entryType == EControlPanelEntryType.None)
		{
			index++;
			int count = this.results.Count;
			int num = this.resultPositions[index];
			while (index < count && this.results[index].entryType != EControlPanelEntryType.None)
			{
				this.resultPositions[index] = num;
				if (expand)
				{
					num += this.objectEntryPrefabHeights[(int)this.results[index].entryType];
				}
				index++;
			}
			int num2 = num - this.resultPositions[index];
			if (num2 != 0)
			{
				for (int i = index; i <= count; i++)
				{
					List<int> list = this.resultPositions;
					int index2 = i;
					list[index2] += num2;
				}
			}
		}
		this.needDetermineEntryVisible = true;
	}

	// Token: 0x060048CF RID: 18639 RVA: 0x003AC7EA File Offset: 0x003AA9EA
	private void ClearFilterResults()
	{
		this.results.Clear();
		this.resultPositions.Clear();
		this.resultEntries.Clear();
		this.resultPositions.Add(-4);
	}

	// Token: 0x060048D0 RID: 18640 RVA: 0x003AC81C File Offset: 0x003AAA1C
	private UIControlPanelObjectEntry TakeObjectEntryFromPool(int _index, ControlPanelTarget _target)
	{
		int count = this.objectEntryPool.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.objectEntryPool[i] != null && this.objectEntryPool[i].entryType == _target.entryType && !this.objectEntryPool[i].inUse)
			{
				this.objectEntryPool[i].InitFromPool(_index, _target);
				return this.objectEntryPool[i];
			}
		}
		UIControlPanelObjectEntry uicontrolPanelObjectEntry = Object.Instantiate<UIControlPanelObjectEntry>(this.objectEntryPrefabs[(int)_target.entryType], this.leftScrollContentRect);
		Assert.True(uicontrolPanelObjectEntry.entryType == _target.entryType);
		uicontrolPanelObjectEntry._Create();
		uicontrolPanelObjectEntry.InitFromPool(_index, _target);
		this.objectEntryPool.Add(uicontrolPanelObjectEntry);
		return uicontrolPanelObjectEntry;
	}

	// Token: 0x060048D1 RID: 18641 RVA: 0x003AC8E8 File Offset: 0x003AAAE8
	private void PutObjectEntryIntoPool(UIControlPanelObjectEntry entry)
	{
		entry.FreeIntoPool();
	}

	// Token: 0x060048D2 RID: 18642 RVA: 0x003AC8F0 File Offset: 0x003AAAF0
	public void ResetObjectEntryPool()
	{
		int count = this.objectEntryPool.Count;
		for (int i = 0; i < count; i++)
		{
			this.objectEntryPool[i].FreeIntoPool();
		}
	}

	// Token: 0x060048D3 RID: 18643 RVA: 0x003AC928 File Offset: 0x003AAB28
	public void ReconnSelectionOnDetermineResults()
	{
		if (this.selection == ControlPanelTarget.none)
		{
			return;
		}
		ControlPanelTarget selection = this.selection;
		ControlPanelTarget selection2 = ControlPanelTarget.none;
		int count = this.results.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.results[i] == selection)
			{
				selection2 = this.results[i];
				break;
			}
		}
		this.selection = selection2;
	}

	// Token: 0x060048D4 RID: 18644 RVA: 0x003AC998 File Offset: 0x003AAB98
	private void OnEntityChange(PlanetFactory factory, int entityId)
	{
		if (entityId <= 0)
		{
			return;
		}
		if (factory == null)
		{
			return;
		}
		if (this.filter.hasStationType && factory.entityPool[entityId].stationId > 0)
		{
			this.needDetermineFilterResults = true;
			return;
		}
		if (this.filter.hasDispenserType && factory.entityPool[entityId].dispenserId > 0)
		{
			this.needDetermineFilterResults = true;
			return;
		}
		if (this.filter.hasMarkerType && factory.entityPool[entityId].markerId > 0)
		{
			this.needDetermineFilterResults = true;
		}
	}

	// Token: 0x060048D5 RID: 18645 RVA: 0x003ACA28 File Offset: 0x003AAC28
	private void OnEntityChange(PlanetFactory factory, int entityId, int oldPrebuildId)
	{
		if (entityId <= 0)
		{
			return;
		}
		if (factory == null)
		{
			return;
		}
		if (this.filter.hasStationType && factory.entityPool[entityId].stationId > 0)
		{
			this.needDetermineFilterResults = true;
			return;
		}
		if (this.filter.hasDispenserType && factory.entityPool[entityId].dispenserId > 0)
		{
			this.needDetermineFilterResults = true;
			return;
		}
		if (this.filter.hasMarkerType && factory.entityPool[entityId].markerId > 0)
		{
			this.needDetermineFilterResults = true;
		}
	}

	// Token: 0x060048D6 RID: 18646 RVA: 0x003ACAB8 File Offset: 0x003AACB8
	private void OnBatchBuild(PlanetFactory factory)
	{
		if (factory == null)
		{
			return;
		}
		this.needDetermineFilterResults = true;
	}

	// Token: 0x060048D7 RID: 18647 RVA: 0x003ACAC5 File Offset: 0x003AACC5
	private void OnLanguageChanged()
	{
		this.DetermineFilterResults();
		this.DetermineEntryVisible();
		this.DetermineSelectionInspector();
		this.UpdateAllObjectEntries();
	}

	// Token: 0x04005751 RID: 22353
	[SerializeField]
	public UIControlPanelFilterPanel filterPanel;

	// Token: 0x04005752 RID: 22354
	[SerializeField]
	public UIControlPanelObjectEntry[] objectEntryPrefabs;

	// Token: 0x04005753 RID: 22355
	[SerializeField]
	public ScrollRect leftScrollRect;

	// Token: 0x04005754 RID: 22356
	[SerializeField]
	public RectTransform leftScrollContentRect;

	// Token: 0x04005755 RID: 22357
	[SerializeField]
	public RectTransform leftScrollViewportRect;

	// Token: 0x04005756 RID: 22358
	[SerializeField]
	public Text leftEmptyTip;

	// Token: 0x04005757 RID: 22359
	[SerializeField]
	public UIControlPanelStationInspector stationInspector;

	// Token: 0x04005758 RID: 22360
	[SerializeField]
	public UIControlPanelDispenserInspector dispenserInspector;

	// Token: 0x04005759 RID: 22361
	[SerializeField]
	public UIControlPanelMarkerInspector markerInspector;

	// Token: 0x0400575A RID: 22362
	[NonSerialized]
	public UIControlPanelInspector currentInspector;

	// Token: 0x0400575B RID: 22363
	[Header("DeliveryColor")]
	[SerializeField]
	public Color deliveryIconColor;

	// Token: 0x0400575C RID: 22364
	[SerializeField]
	public Color deliveryTextColor;

	// Token: 0x0400575D RID: 22365
	[SerializeField]
	public Color unnamedColor;

	// Token: 0x0400575E RID: 22366
	[SerializeField]
	public Color renamedColor;

	// Token: 0x0400575F RID: 22367
	[Header("DispenserEntryColor")]
	[SerializeField]
	public Color deliveryActiveColor;

	// Token: 0x04005760 RID: 22368
	[SerializeField]
	public Color deliveryDeactiveColor;

	// Token: 0x04005761 RID: 22369
	[SerializeField]
	public Color recycleAllColor;

	// Token: 0x04005762 RID: 22370
	[SerializeField]
	public Color transitItemTextColor;

	// Token: 0x04005763 RID: 22371
	[SerializeField]
	public Color supplyArrowColor;

	// Token: 0x04005764 RID: 22372
	[SerializeField]
	public Color demandArrowColor;

	// Token: 0x04005765 RID: 22373
	[SerializeField]
	public Color supplyDemandArrowColor;

	// Token: 0x04005766 RID: 22374
	[Header("EntryPowerColor")]
	[SerializeField]
	public Color powerSignColor0;

	// Token: 0x04005767 RID: 22375
	[SerializeField]
	public Color powerSignColor1;

	// Token: 0x04005768 RID: 22376
	[SerializeField]
	public Color powerSignColor2;

	// Token: 0x04005769 RID: 22377
	[SerializeField]
	public Color powerSignColor3;

	// Token: 0x0400576A RID: 22378
	[SerializeField]
	public Color powerCircleBgColor0;

	// Token: 0x0400576B RID: 22379
	[SerializeField]
	public Color powerCircleBgColor1;

	// Token: 0x0400576C RID: 22380
	[SerializeField]
	public Color powerCircleBgColor2;

	// Token: 0x0400576D RID: 22381
	[SerializeField]
	public Color powerCircleBgColor3;

	// Token: 0x0400576E RID: 22382
	[SerializeField]
	public Color powerCircleFgColor0;

	// Token: 0x0400576F RID: 22383
	[SerializeField]
	public Color powerCircleFgColor1;

	// Token: 0x04005770 RID: 22384
	[SerializeField]
	public Color powerCircleFgColor2;

	// Token: 0x04005771 RID: 22385
	[SerializeField]
	public Color powerCircleFgColor3;

	// Token: 0x04005772 RID: 22386
	[SerializeField]
	public Color powerRoundFgColor0;

	// Token: 0x04005773 RID: 22387
	[SerializeField]
	public Color powerRoundFgColor1;

	// Token: 0x04005774 RID: 22388
	[SerializeField]
	public Color powerRoundFgColor2;

	// Token: 0x04005775 RID: 22389
	[SerializeField]
	public Color powerRoundFgColor3;

	// Token: 0x04005776 RID: 22390
	[SerializeField]
	public Color powerTextColor0;

	// Token: 0x04005777 RID: 22391
	[SerializeField]
	public Color powerTextColor1;

	// Token: 0x04005778 RID: 22392
	[SerializeField]
	public Color powerTextColor2;

	// Token: 0x04005779 RID: 22393
	[SerializeField]
	public Color powerTextColor3;

	// Token: 0x0400577A RID: 22394
	[Header("StorageItemColor")]
	[SerializeField]
	public Color supplyBarColor;

	// Token: 0x0400577B RID: 22395
	[SerializeField]
	public Color demandBarColor;

	// Token: 0x0400577C RID: 22396
	[SerializeField]
	public Color supplyTextColor;

	// Token: 0x0400577D RID: 22397
	[SerializeField]
	public Color demandTextColor;

	// Token: 0x0400577E RID: 22398
	[SerializeField]
	public Color emptyItemColor;

	// Token: 0x0400577F RID: 22399
	[SerializeField]
	public Font FONT_DIN;

	// Token: 0x04005780 RID: 22400
	[SerializeField]
	public Font FONT_SAIRASB;

	// Token: 0x04005781 RID: 22401
	[Header("FocusColor")]
	[SerializeField]
	public Color focusColor;

	// Token: 0x04005782 RID: 22402
	[SerializeField]
	public Color clearColor;

	// Token: 0x04005783 RID: 22403
	[Header("MarkerEntry")]
	[SerializeField]
	public int tagXPadding;

	// Token: 0x04005784 RID: 22404
	[SerializeField]
	public int tagYPadding;

	// Token: 0x04005785 RID: 22405
	[SerializeField]
	public int tagTextWidthLimit;

	// Token: 0x04005786 RID: 22406
	[SerializeField]
	public int tagWidthOffset;

	// Token: 0x04005787 RID: 22407
	[SerializeField]
	public Sprite powerSprit;

	// Token: 0x04005788 RID: 22408
	[SerializeField]
	public Sprite powerOffSprit;

	// Token: 0x04005789 RID: 22409
	[SerializeField]
	public Sprite[] levelStateSprits;

	// Token: 0x0400578A RID: 22410
	[SerializeField]
	public Color thumbnailIconNormalColor;

	// Token: 0x0400578B RID: 22411
	private GameData gameData;

	// Token: 0x0400578C RID: 22412
	public ControlPanelFilter filter = ControlPanelFilter.Default;

	// Token: 0x0400578D RID: 22413
	public HashSet<int> astroExpands;

	// Token: 0x0400578E RID: 22414
	public List<ControlPanelAstroData> sortedAstros;

	// Token: 0x0400578F RID: 22415
	private List<ControlPanelTarget> results;

	// Token: 0x04005790 RID: 22416
	private List<int> resultPositions;

	// Token: 0x04005791 RID: 22417
	private List<UIControlPanelObjectEntry> resultEntries;

	// Token: 0x04005792 RID: 22418
	private int resultGeneration;

	// Token: 0x04005793 RID: 22419
	private List<UIControlPanelObjectEntry> objectEntryPool;

	// Token: 0x04005794 RID: 22420
	private int[] objectEntryPrefabHeights;

	// Token: 0x04005795 RID: 22421
	private ControlPanelTarget _selection;

	// Token: 0x04005796 RID: 22422
	private const int PLANET_GROUP_SPACING = 4;

	// Token: 0x04005797 RID: 22423
	private int leftViewportBeginPrev = -1;

	// Token: 0x04005798 RID: 22424
	private bool needDetermineEntryVisible;

	// Token: 0x04005799 RID: 22425
	public bool needDetermineFilterResults;

	// Token: 0x0400579A RID: 22426
	private bool needDetermineSelectionInspector;
}
