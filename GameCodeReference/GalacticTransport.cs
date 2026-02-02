using System;
using System.Collections.Generic;
using System.IO;

// Token: 0x02000179 RID: 377
public class GalacticTransport
{
	// Token: 0x06000D41 RID: 3393 RVA: 0x000C7CEC File Offset: 0x000C5EEC
	public void Init(GameData _gameData)
	{
		this.gameData = _gameData;
		this.SetStationCapacity(2);
		this.station2stationRoutes = new HashSet<long>();
		this.astro2astroRoutes = new Dictionary<long, LogisticShipRoute>();
		this.astro2astroBans = new HashSet<long>();
		this.shipRenderer = new LogisticShipRenderer(this);
		this.shipUIRenderer = new LogisticShipUIRenderer(this);
	}

	// Token: 0x06000D42 RID: 3394 RVA: 0x000C7D40 File Offset: 0x000C5F40
	public void SetForNewGame()
	{
	}

	// Token: 0x06000D43 RID: 3395 RVA: 0x000C7D44 File Offset: 0x000C5F44
	public void Free()
	{
		this.stationPool = null;
		this.stationCursor = 1;
		this.stationCapacity = 0;
		this.stationRecycle = null;
		this.stationRecycleCursor = 0;
		if (this.shipRenderer != null)
		{
			this.shipRenderer.Destroy();
			this.shipRenderer = null;
		}
		if (this.shipUIRenderer != null)
		{
			this.shipUIRenderer.Destroy();
			this.shipUIRenderer = null;
		}
	}

	// Token: 0x170001A6 RID: 422
	// (get) Token: 0x06000D44 RID: 3396 RVA: 0x000C7DA8 File Offset: 0x000C5FA8
	// (set) Token: 0x06000D45 RID: 3397 RVA: 0x000C7DB0 File Offset: 0x000C5FB0
	public int remotePairCount { get; private set; }

	// Token: 0x06000D46 RID: 3398 RVA: 0x000C7DBC File Offset: 0x000C5FBC
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

	// Token: 0x06000D47 RID: 3399 RVA: 0x000C7E10 File Offset: 0x000C6010
	public int AddStationComponent(int planetId, StationComponent station)
	{
		if (station.gid > 0 && station.gid < 65536)
		{
			if (this.stationCapacity == 0)
			{
				this.SetStationCapacity(2);
			}
			while (this.stationCapacity <= station.gid)
			{
				this.SetStationCapacity(this.stationCapacity * 2);
			}
			this.stationPool[station.gid] = station;
			station.planetId = planetId;
			return station.gid;
		}
		Assert.True(station.gid < 65536);
		int num2;
		if (this.stationRecycleCursor > 0)
		{
			int[] array = this.stationRecycle;
			int num = this.stationRecycleCursor - 1;
			this.stationRecycleCursor = num;
			num2 = array[num];
		}
		else
		{
			int num = this.stationCursor;
			this.stationCursor = num + 1;
			num2 = num;
			if (num2 == this.stationCapacity)
			{
				this.SetStationCapacity(this.stationCapacity * 2);
			}
		}
		this.stationPool[num2] = station;
		station.gid = num2;
		station.planetId = planetId;
		return num2;
	}

	// Token: 0x06000D48 RID: 3400 RVA: 0x000C7EF8 File Offset: 0x000C60F8
	public void RemoveStationComponent(int gid)
	{
		if (this.stationPool[gid] != null)
		{
			this.stationPool[gid] = null;
			int[] array = this.stationRecycle;
			int num = this.stationRecycleCursor;
			this.stationRecycleCursor = num + 1;
			array[num] = gid;
		}
		this.RemoveStation2StationRoute(gid);
		this.RefreshTraffic(gid);
		if (this.OnStellarStationRemoved != null)
		{
			this.OnStellarStationRemoved();
		}
	}

	// Token: 0x06000D49 RID: 3401 RVA: 0x000C7F54 File Offset: 0x000C6154
	public void Arragement()
	{
		this.stationCapacity = this.stationPool.Length;
		if (this.stationCapacity == 0)
		{
			this.SetStationCapacity(2);
		}
		this.stationCursor = 1;
		this.stationPool[0] = null;
		this.stationRecycleCursor = 0;
		for (int i = 1; i < this.stationCapacity; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].gid != i)
			{
				this.stationPool[i] = null;
			}
			if (this.stationPool[i] != null)
			{
				this.stationCursor = i + 1;
			}
		}
		for (int j = 1; j < this.stationCursor; j++)
		{
			if (this.stationPool[j] == null)
			{
				int[] array = this.stationRecycle;
				int num = this.stationRecycleCursor;
				this.stationRecycleCursor = num + 1;
				array[num] = j;
			}
		}
	}

	// Token: 0x06000D4A RID: 3402 RVA: 0x000C8010 File Offset: 0x000C6210
	public void Export(BinaryWriter w)
	{
		w.Write(1);
		w.Write(this.station2stationRoutes.Count);
		foreach (long value in this.station2stationRoutes)
		{
			w.Write(value);
		}
		w.Write(this.astro2astroRoutes.Count);
		foreach (KeyValuePair<long, LogisticShipRoute> keyValuePair in this.astro2astroRoutes)
		{
			w.Write(keyValuePair.Key);
			keyValuePair.Value.Export(w);
		}
		w.Write(this.astro2astroBans.Count);
		foreach (long value2 in this.astro2astroBans)
		{
			w.Write(value2);
		}
	}

	// Token: 0x06000D4B RID: 3403 RVA: 0x000C8138 File Offset: 0x000C6338
	public void Import(BinaryReader r)
	{
		if (r.ReadInt32() >= 1)
		{
			int num = r.ReadInt32();
			for (int i = 0; i < num; i++)
			{
				long item = r.ReadInt64();
				this.station2stationRoutes.Add(item);
			}
			int num2 = r.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				long key = r.ReadInt64();
				LogisticShipRoute logisticShipRoute = new LogisticShipRoute();
				logisticShipRoute.Import(r);
				this.astro2astroRoutes.Add(key, logisticShipRoute);
			}
			int num3 = r.ReadInt32();
			for (int k = 0; k < num3; k++)
			{
				long item2 = r.ReadInt64();
				this.astro2astroBans.Add(item2);
			}
		}
	}

	// Token: 0x06000D4C RID: 3404 RVA: 0x000C81E4 File Offset: 0x000C63E4
	public void GameTick(long time)
	{
		GalaxyData galaxy = this.gameData.galaxy;
		GameHistoryData history = this.gameData.history;
		PlanetFactory[] factories = this.gameData.factories;
		FactoryProductionStat[] factoryStatPool = this.gameData.statistics.production.factoryStatPool;
		TrafficStatistics traffic = this.gameData.statistics.traffic;
		float logisticShipSailSpeedModified = history.logisticShipSailSpeedModified;
		float shipWarpSpeed = history.logisticShipWarpDrive ? history.logisticShipWarpSpeedModified : logisticShipSailSpeedModified;
		int logisticShipCarries = history.logisticShipCarries;
		for (int i = 1; i < 7; i++)
		{
			int num = i % 6;
			if (StationComponent.DetermineFramingDispatchTime(time, i))
			{
				for (int j = 1; j < this.stationCursor; j++)
				{
					if (this.stationPool[j] != null && this.stationPool[j].id > 0 && this.stationPool[j].gid == j)
					{
						StationComponent stationComponent = this.stationPool[j];
						if (num >= 1 && num <= 4 && (stationComponent.routePriority == ERemoteRoutePriority.Prioritize || stationComponent.routePriority == ERemoteRoutePriority.Only || stationComponent.routePriority == ERemoteRoutePriority.Designated))
						{
							stationComponent.DetermineDispatch(logisticShipSailSpeedModified, shipWarpSpeed, logisticShipCarries, num, this.stationPool, factoryStatPool, factories, galaxy, traffic);
						}
						else if (num == 5 && stationComponent.routePriority == ERemoteRoutePriority.Prioritize)
						{
							stationComponent.DetermineDispatch(logisticShipSailSpeedModified, shipWarpSpeed, logisticShipCarries, num, this.stationPool, factoryStatPool, factories, galaxy, traffic);
						}
						else if (num == 0 && stationComponent.routePriority == ERemoteRoutePriority.Ignore)
						{
							stationComponent.DetermineDispatch(logisticShipSailSpeedModified, shipWarpSpeed, logisticShipCarries, num, this.stationPool, factoryStatPool, factories, galaxy, traffic);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000D4D RID: 3405 RVA: 0x000C8380 File Offset: 0x000C6580
	public void RefreshTraffic(int keyStationGId = 0)
	{
		int logisticShipCarries = GameMain.history.logisticShipCarries;
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].gid == i)
			{
				this.stationPool[i].ClearRemotePairs();
			}
		}
		this.remotePairCount = 0;
		for (int j = 1; j < this.stationCursor; j++)
		{
			if (this.stationPool[j] != null && this.stationPool[j].gid == j)
			{
				this.stationPool[j].RematchRemotePairs(this, this.stationCursor, keyStationGId, logisticShipCarries);
				this.remotePairCount += this.stationPool[j].remotePairTotalCount;
			}
		}
		this.remotePairCount /= 2;
	}

	// Token: 0x06000D4E RID: 3406 RVA: 0x000C8440 File Offset: 0x000C6640
	public void AddStation2StationRoute(int gid0, int gid1)
	{
		if (gid0 == 0 || gid1 == 0 || gid0 == gid1)
		{
			return;
		}
		if (this.stationPool[gid0] == null || this.stationPool[gid0].id <= 0 || this.stationPool[gid1] == null || this.stationPool[gid1].id <= 0)
		{
			return;
		}
		long item = this.CalculateStation2StationKey(gid0, gid1);
		if (!this.station2stationRoutes.Contains(item))
		{
			this.station2stationRoutes.Add(item);
			this.RefreshTraffic(0);
		}
	}

	// Token: 0x06000D4F RID: 3407 RVA: 0x000C84B8 File Offset: 0x000C66B8
	public void RemoveStation2StationRoute(int gid0, int gid1)
	{
		if (gid0 == 0 || gid1 == 0 || gid0 == gid1)
		{
			return;
		}
		if (this.stationPool[gid0] == null || this.stationPool[gid0].id <= 0 || this.stationPool[gid1] == null || this.stationPool[gid1].id <= 0)
		{
			return;
		}
		long item = this.CalculateStation2StationKey(gid0, gid1);
		if (this.station2stationRoutes.Contains(item))
		{
			this.station2stationRoutes.Remove(item);
			this.RefreshTraffic(0);
		}
	}

	// Token: 0x06000D50 RID: 3408 RVA: 0x000C8530 File Offset: 0x000C6730
	private void RemoveStation2StationRoute(int gid)
	{
		if (gid == 0)
		{
			return;
		}
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].id > 0 && this.stationPool[i].gid == i)
			{
				long item = this.CalculateStation2StationKey(gid, i);
				if (this.station2stationRoutes.Contains(item))
				{
					this.station2stationRoutes.Remove(item);
				}
			}
		}
	}

	// Token: 0x06000D51 RID: 3409 RVA: 0x000C85A0 File Offset: 0x000C67A0
	public bool IsStation2StationRouteExist(int gid0, int gid1)
	{
		return this.station2stationRoutes.Contains(this.CalculateStation2StationKey(gid0, gid1));
	}

	// Token: 0x06000D52 RID: 3410 RVA: 0x000C85B8 File Offset: 0x000C67B8
	public void AddAstro2AstroRoute(int astroId0, int astroId1, int itemId)
	{
		if (astroId0 == 0 || astroId1 == 0 || astroId0 == astroId1)
		{
			return;
		}
		if (itemId == 0)
		{
			int[] itemIds = ItemProto.itemIds;
			int num = itemIds.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = itemIds[i];
				if (num2 != 11901 && num2 != 11902 && num2 != 11903)
				{
					long key = this.CalculateAstro2AstroKey(astroId0, astroId1, num2);
					if (!this.astro2astroRoutes.ContainsKey(key))
					{
						LogisticShipRoute logisticShipRoute = new LogisticShipRoute();
						logisticShipRoute.Init();
						this.astro2astroRoutes.Add(key, logisticShipRoute);
					}
				}
			}
			this.RefreshTraffic(0);
			return;
		}
		long key2 = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (!this.astro2astroRoutes.ContainsKey(key2))
		{
			LogisticShipRoute logisticShipRoute2 = new LogisticShipRoute();
			logisticShipRoute2.Init();
			this.astro2astroRoutes.Add(key2, logisticShipRoute2);
			this.RefreshTraffic(0);
		}
	}

	// Token: 0x06000D53 RID: 3411 RVA: 0x000C8684 File Offset: 0x000C6884
	public void RemoveAstro2AstroRoute(int astroId0, int astroId1, int itemId)
	{
		if (astroId0 == 0 || astroId1 == 0 || astroId0 == astroId1)
		{
			return;
		}
		if (itemId == 0)
		{
			int[] itemIds = ItemProto.itemIds;
			int num = itemIds.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = itemIds[i];
				if (num2 != 11901 && num2 != 11902 && num2 != 11903)
				{
					long key = this.CalculateAstro2AstroKey(astroId0, astroId1, num2);
					if (this.astro2astroRoutes.ContainsKey(key))
					{
						this.astro2astroRoutes[key].Free();
						this.astro2astroRoutes.Remove(key);
					}
				}
			}
			this.RefreshTraffic(0);
			return;
		}
		long key2 = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (this.astro2astroRoutes.ContainsKey(key2))
		{
			this.astro2astroRoutes[key2].Free();
			this.astro2astroRoutes.Remove(key2);
			this.RefreshTraffic(0);
		}
	}

	// Token: 0x06000D54 RID: 3412 RVA: 0x000C8754 File Offset: 0x000C6954
	public void AddAstro2AstroBan(int astroId0, int astroId1, int itemId)
	{
		if (astroId0 == 0 || astroId1 == 0 || astroId0 == astroId1)
		{
			return;
		}
		if (itemId == 0)
		{
			int[] itemIds = ItemProto.itemIds;
			int num = itemIds.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = itemIds[i];
				if (num2 != 11901 && num2 != 11902 && num2 != 11903)
				{
					long item = this.CalculateAstro2AstroKey(astroId0, astroId1, num2);
					if (!this.astro2astroBans.Contains(item))
					{
						this.astro2astroBans.Add(item);
					}
				}
			}
			this.RefreshTraffic(0);
			return;
		}
		long item2 = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (!this.astro2astroBans.Contains(item2))
		{
			this.astro2astroBans.Add(item2);
			this.RefreshTraffic(0);
		}
	}

	// Token: 0x06000D55 RID: 3413 RVA: 0x000C8800 File Offset: 0x000C6A00
	public void RemoveAstro2AstroBan(int astroId0, int astroId1, int itemId)
	{
		if (astroId0 == 0 || astroId1 == 0 || astroId0 == astroId1)
		{
			return;
		}
		if (itemId == 0)
		{
			int[] itemIds = ItemProto.itemIds;
			int num = itemIds.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = itemIds[i];
				if (num2 != 11901 && num2 != 11902 && num2 != 11903)
				{
					long item = this.CalculateAstro2AstroKey(astroId0, astroId1, num2);
					if (this.astro2astroBans.Contains(item))
					{
						this.astro2astroBans.Remove(item);
					}
				}
			}
			this.RefreshTraffic(0);
			return;
		}
		long item2 = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (this.astro2astroBans.Contains(item2))
		{
			this.astro2astroBans.Remove(item2);
			this.RefreshTraffic(0);
		}
	}

	// Token: 0x06000D56 RID: 3414 RVA: 0x000C88AC File Offset: 0x000C6AAC
	public bool IsAstro2AstroRouteExist(int astroId0, int astroId1, int itemId)
	{
		return this.astro2astroRoutes.ContainsKey(this.CalculateAstro2AstroKey(astroId0, astroId1, itemId));
	}

	// Token: 0x06000D57 RID: 3415 RVA: 0x000C88C2 File Offset: 0x000C6AC2
	public bool IsAstro2AstroBanExist(int astroId0, int astroId1, int itemId)
	{
		return this.astro2astroBans.Contains(this.CalculateAstro2AstroKey(astroId0, astroId1, itemId));
	}

	// Token: 0x06000D58 RID: 3416 RVA: 0x000C88D8 File Offset: 0x000C6AD8
	public bool IsAstro2AstroRouteEnable(int astroId0, int astroId1, int itemId)
	{
		long key = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		return this.astro2astroRoutes.ContainsKey(key) && this.astro2astroRoutes[key].enable;
	}

	// Token: 0x06000D59 RID: 3417 RVA: 0x000C8910 File Offset: 0x000C6B10
	public LogisticShipRoute GetAstro2AstroLogisticShipRoute(int astroId0, int astroId1, int itemId)
	{
		long key = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (this.astro2astroRoutes.ContainsKey(key))
		{
			return this.astro2astroRoutes[key];
		}
		return null;
	}

	// Token: 0x06000D5A RID: 3418 RVA: 0x000C8944 File Offset: 0x000C6B44
	public void SetAstro2AstroRouteEnable(int astroId0, int astroId1, int itemId, bool enable)
	{
		if (astroId0 == 0 || astroId1 == 0 || itemId == 0 || astroId0 == astroId1)
		{
			return;
		}
		long key = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (this.astro2astroRoutes.ContainsKey(key))
		{
			this.astro2astroRoutes[key].enable = enable;
		}
	}

	// Token: 0x06000D5B RID: 3419 RVA: 0x000C898C File Offset: 0x000C6B8C
	public void SetAstro2AstroRouteComment(int astroId0, int astroId1, int itemId, string comment)
	{
		if (astroId0 == 0 || astroId1 == 0 || itemId == 0 || astroId0 == astroId1)
		{
			return;
		}
		long key = this.CalculateAstro2AstroKey(astroId0, astroId1, itemId);
		if (this.astro2astroRoutes.ContainsKey(key))
		{
			this.astro2astroRoutes[key].comment = comment;
		}
	}

	// Token: 0x06000D5C RID: 3420 RVA: 0x000C89D4 File Offset: 0x000C6BD4
	public long CalculateStation2StationKey(int gid0, int gid1)
	{
		int num;
		int num2;
		if (gid0 < gid1)
		{
			num = gid0;
			num2 = gid1;
		}
		else
		{
			num = gid1;
			num2 = gid0;
		}
		return (long)num | (long)num2 << 32;
	}

	// Token: 0x06000D5D RID: 3421 RVA: 0x000C89F8 File Offset: 0x000C6BF8
	public long CalculateAstro2AstroKey(int astroId0, int astroId1, int itemId)
	{
		int num;
		int num2;
		if (astroId0 < astroId1)
		{
			num = astroId0;
			num2 = astroId1;
		}
		else
		{
			num = astroId1;
			num2 = astroId0;
		}
		return (long)num | (long)num2 << 22 | (long)itemId << 44;
	}

	// Token: 0x06000D5E RID: 3422 RVA: 0x000C8A24 File Offset: 0x000C6C24
	public void SetAllStationsMaxWarperCount(int count)
	{
		for (int i = 1; i < this.stationCursor; i++)
		{
			if (this.stationPool[i] != null && this.stationPool[i].gid == i)
			{
				this.stationPool[i].warperMaxCount = count;
			}
		}
	}

	// Token: 0x04000E7F RID: 3711
	public GameData gameData;

	// Token: 0x04000E80 RID: 3712
	public StationComponent[] stationPool;

	// Token: 0x04000E81 RID: 3713
	public int stationCursor = 1;

	// Token: 0x04000E82 RID: 3714
	public int stationCapacity;

	// Token: 0x04000E83 RID: 3715
	public int[] stationRecycle;

	// Token: 0x04000E84 RID: 3716
	public int stationRecycleCursor;

	// Token: 0x04000E85 RID: 3717
	public HashSet<long> station2stationRoutes;

	// Token: 0x04000E86 RID: 3718
	public Dictionary<long, LogisticShipRoute> astro2astroRoutes;

	// Token: 0x04000E87 RID: 3719
	public HashSet<long> astro2astroBans;

	// Token: 0x04000E88 RID: 3720
	public LogisticShipRenderer shipRenderer;

	// Token: 0x04000E89 RID: 3721
	public LogisticShipUIRenderer shipUIRenderer;

	// Token: 0x04000E8B RID: 3723
	public Action OnStellarStationRemoved;
}
