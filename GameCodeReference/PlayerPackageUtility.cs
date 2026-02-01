using System;

// Token: 0x020001E3 RID: 483
public class PlayerPackageUtility
{
	// Token: 0x06001406 RID: 5126 RVA: 0x00166DDC File Offset: 0x00164FDC
	public PlayerPackageUtility(Player _player)
	{
		this.player = _player;
		this.statistics = new PackageStatistics();
	}

	// Token: 0x06001407 RID: 5127 RVA: 0x00166DF6 File Offset: 0x00164FF6
	public void Free()
	{
		this.player = null;
		if (this.statistics != null)
		{
			this.statistics.Free();
			this.statistics = null;
		}
	}

	// Token: 0x06001408 RID: 5128 RVA: 0x00166E19 File Offset: 0x00165019
	public void Count()
	{
		DeepProfiler.BeginSample(DPEntry.Icarus, -1, 9L);
		this.statistics.Count(this.player.package);
		DeepProfiler.EndSample(-1, -2L);
	}

	// Token: 0x06001409 RID: 5129 RVA: 0x00166E45 File Offset: 0x00165045
	public int GetPackageItemCount(int itemId)
	{
		return this.statistics.GetItemCount(itemId);
	}

	// Token: 0x0600140A RID: 5130 RVA: 0x00166E53 File Offset: 0x00165053
	public int GetPackageItemCapacity(int itemId)
	{
		return this.statistics.GetItemCapacity(itemId);
	}

	// Token: 0x0600140B RID: 5131 RVA: 0x00166E64 File Offset: 0x00165064
	public int GetPackageItemCountIncludeHandItem(int itemId)
	{
		int num = this.GetPackageItemCount(itemId);
		if (this.player.inhandItemId == itemId)
		{
			num += this.player.inhandItemCount;
		}
		return num;
	}

	// Token: 0x0600140C RID: 5132 RVA: 0x00166E98 File Offset: 0x00165098
	public int AddItemToAllPackages(int itemId, int count, int gridIndex, int inc, out int remainInc, int priorityMode = 0)
	{
		remainInc = inc;
		if (itemId <= 0 || count <= 0 || itemId == 1099)
		{
			return 0;
		}
		int num = count;
		int num2 = num;
		if (priorityMode < 0)
		{
			int num3 = this.player.deliveryPackage.AddItem(gridIndex, itemId, count, inc, out remainInc);
			if (num3 > 0)
			{
				this.player.NotifyDeliveryPackageAddItem(itemId, num3, inc - remainInc);
			}
			num2 -= num3;
			count = num2;
			inc = remainInc;
			if (num2 == 0)
			{
				return num;
			}
			int num4 = this.player.package.AddItemStacked(itemId, count, inc, out remainInc);
			if (num4 > 0)
			{
				this.player.NotifyPackageAddItem(itemId, num4, inc - remainInc);
			}
			num2 -= num4;
		}
		else if (priorityMode > 0)
		{
			int num5 = this.player.package.AddItemStacked(itemId, count, inc, out remainInc);
			if (num5 > 0)
			{
				this.player.NotifyPackageAddItem(itemId, num5, inc - remainInc);
			}
			num2 -= num5;
			count = num2;
			inc = remainInc;
			if (num2 == 0)
			{
				return num;
			}
			int num6 = this.player.deliveryPackage.AddItem(gridIndex, itemId, count, inc, out remainInc);
			if (num6 > 0)
			{
				this.player.NotifyDeliveryPackageAddItem(itemId, num6, inc - remainInc);
			}
			num2 -= num6;
		}
		else
		{
			StorageComponent package = this.player.package;
			StorageComponent.GRID[] grids = package.grids;
			DeliveryPackage.GRID grid = this.player.deliveryPackage.grids[gridIndex];
			int num7 = 0;
			int num8 = 0;
			for (int i = 0; i < package.size; i++)
			{
				if (grids[i].itemId == itemId || grids[i].filter == itemId)
				{
					num7 += grids[i].count;
					num8 += StorageComponent.itemStackCount[itemId];
				}
			}
			int num9 = grid.clampedRequireCount - grid.stackSizeModified;
			if (num9 < 0)
			{
				num9 = 0;
			}
			if (num8 < num9)
			{
				num8 = num9;
			}
			int num10 = (this.player.inhandItemId == itemId) ? this.player.inhandItemCount : 0;
			int num11 = num8 - num7 - num10;
			if (num11 < 0)
			{
				num11 = 0;
			}
			if (num11 > 0)
			{
				int num12 = (count > num11) ? num11 : count;
				int num13 = this.split_inc(ref count, ref inc, num12);
				int num15;
				int num14 = this.player.package.AddItemStacked(itemId, num12, num13, out num15);
				if (num14 > 0)
				{
					this.player.NotifyPackageAddItem(itemId, num14, num13 - num15);
				}
				int num16 = num12 - num14;
				count += num16;
				inc += num15;
				num2 = count;
				remainInc = inc;
				if (num2 == 0)
				{
					return num;
				}
			}
			int num17 = this.player.deliveryPackage.AddItem(gridIndex, itemId, count, inc, out remainInc);
			if (num17 > 0)
			{
				this.player.NotifyDeliveryPackageAddItem(itemId, num17, inc - remainInc);
			}
			num2 -= num17;
			count = num2;
			inc = remainInc;
			if (num2 == 0)
			{
				return num;
			}
			int num18 = this.player.package.AddItemStacked(itemId, count, inc, out remainInc);
			if (num18 > 0)
			{
				this.player.NotifyPackageAddItem(itemId, num18, inc - remainInc);
			}
			num2 -= num18;
		}
		return num - num2;
	}

	// Token: 0x0600140D RID: 5133 RVA: 0x0016718C File Offset: 0x0016538C
	public void TakeItemFromAllPackages(int gridIndex, ref int itemId, ref int count, out int inc, bool deliveryFirst)
	{
		inc = 0;
		if (itemId <= 0 || count <= 0 || itemId == 1099)
		{
			itemId = 0;
			count = 0;
			return;
		}
		int num = itemId;
		int num2 = count;
		int num3 = num2;
		if (deliveryFirst)
		{
			int num4;
			this.player.deliveryPackage.TakeItemFromGrid(gridIndex, ref itemId, ref num3, out num4);
			inc = num4;
			if (num3 == count)
			{
				return;
			}
			if (itemId == 0 || num3 == 0)
			{
				itemId = num;
			}
			count = num3;
			num3 = num2 - num3;
			this.player.package.TakeTailItems(ref itemId, ref num3, out num4, false);
			count += num3;
			inc += num4;
			return;
		}
		else
		{
			int num5;
			this.player.package.TakeTailItems(ref itemId, ref num3, out num5, false);
			inc = num5;
			if (num3 == count)
			{
				return;
			}
			if (itemId == 0 || num3 == 0)
			{
				itemId = num;
			}
			count = num3;
			num3 = num2 - num3;
			this.player.deliveryPackage.TakeItemFromGrid(gridIndex, ref itemId, ref num3, out num5);
			count += num3;
			inc += num5;
			return;
		}
	}

	// Token: 0x0600140E RID: 5134 RVA: 0x00167270 File Offset: 0x00165470
	public void TryTakeItemFromAllPackages(ref int itemId, ref int count, out int inc, bool deliveryFirst = false)
	{
		inc = 0;
		if (itemId <= 0 || count <= 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		int num = itemId;
		int num2 = count;
		int num3 = num2;
		if (deliveryFirst)
		{
			int num4;
			this.player.deliveryPackage.TakeItems(ref itemId, ref num3, out num4);
			inc = num4;
			if (num3 == count)
			{
				return;
			}
			if (itemId == 0 || num3 == 0)
			{
				itemId = num;
			}
			count = num3;
			num3 = num2 - num3;
			this.player.package.TakeTailItems(ref itemId, ref num3, out num4, false);
			count += num3;
			inc += num4;
			return;
		}
		else
		{
			int num5;
			this.player.package.TakeTailItems(ref itemId, ref num3, out num5, false);
			inc = num5;
			if (num3 == count)
			{
				return;
			}
			if (itemId == 0 || num3 == 0)
			{
				itemId = num;
			}
			count = num3;
			num3 = num2 - num3;
			this.player.deliveryPackage.TakeItems(ref itemId, ref num3, out num5);
			count += num3;
			inc += num5;
			return;
		}
	}

	// Token: 0x0600140F RID: 5135 RVA: 0x00167344 File Offset: 0x00165544
	public void ThrowAllItemsInAllPackage()
	{
		for (int i = 0; i < this.player.package.size; i++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.package.TakeItemFromGrid(i, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
		for (int j = 0; j < 100; j++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.deliveryPackage.TakeItemFromGrid(j, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
		for (int k = 0; k < this.player.mecha.reactorStorage.size; k++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.mecha.reactorStorage.TakeItemFromGrid(k, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
		for (int l = 0; l < this.player.mecha.warpStorage.size; l++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.mecha.warpStorage.TakeItemFromGrid(l, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
		for (int m = 0; m < this.player.mecha.ammoStorage.size; m++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.mecha.ammoStorage.TakeItemFromGrid(m, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
		for (int n = 0; n < this.player.mecha.bombStorage.size; n++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.mecha.bombStorage.TakeItemFromGrid(n, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
		for (int num3 = 0; num3 < this.player.mecha.fighterStorage.size; num3++)
		{
			int inc;
			int num2;
			int num = num2 = (inc = 0);
			this.player.mecha.fighterStorage.TakeItemFromGrid(num3, ref num2, ref num, out inc);
			if (num2 > 0 && num > 0)
			{
				this.player.ThrowTrash(num2, num, inc, 0, 0);
			}
		}
	}

	// Token: 0x06001410 RID: 5136 RVA: 0x001675C4 File Offset: 0x001657C4
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

	// Token: 0x04001833 RID: 6195
	public Player player;

	// Token: 0x04001834 RID: 6196
	public PackageStatistics statistics;
}
