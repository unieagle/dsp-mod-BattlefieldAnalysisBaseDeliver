using System;
using System.IO;
using UnityEngine;

// Token: 0x02000161 RID: 353
public class DeliveryPackage
{
	// Token: 0x06000A80 RID: 2688 RVA: 0x0009D468 File Offset: 0x0009B668
	public DeliveryPackage()
	{
		this._index2pos = new int[100];
		this._pos2index = new int[100];
		this.ResetIndexMaps();
	}

	// Token: 0x17000176 RID: 374
	// (get) Token: 0x06000A81 RID: 2689 RVA: 0x0009D490 File Offset: 0x0009B690
	// (set) Token: 0x06000A82 RID: 2690 RVA: 0x0009D4B0 File Offset: 0x0009B6B0
	public int stackSizeMultiplier
	{
		get
		{
			if (this.grids != null)
			{
				return this.grids[0].stackSizeMultiplier;
			}
			return 0;
		}
		set
		{
			if (this.grids != null)
			{
				for (int i = 0; i < 100; i++)
				{
					this.grids[i].stackSizeMultiplier = value;
				}
			}
		}
	}

	// Token: 0x17000177 RID: 375
	// (get) Token: 0x06000A83 RID: 2691 RVA: 0x0009D4E4 File Offset: 0x0009B6E4
	public int gridLength
	{
		get
		{
			return this.grids.Length;
		}
	}

	// Token: 0x17000178 RID: 376
	// (get) Token: 0x06000A84 RID: 2692 RVA: 0x0009D4EE File Offset: 0x0009B6EE
	public int activeCount
	{
		get
		{
			if (!this.unlocked)
			{
				return 0;
			}
			return this.rowCount * this.colCount;
		}
	}

	// Token: 0x17000179 RID: 377
	// (get) Token: 0x06000A85 RID: 2693 RVA: 0x0009D507 File Offset: 0x0009B707
	public bool unlockedAndEnabled
	{
		get
		{
			return this.unlocked && this.enable;
		}
	}

	// Token: 0x14000006 RID: 6
	// (add) Token: 0x06000A86 RID: 2694 RVA: 0x0009D51C File Offset: 0x0009B71C
	// (remove) Token: 0x06000A87 RID: 2695 RVA: 0x0009D554 File Offset: 0x0009B754
	public event Action onSizeChange;

	// Token: 0x06000A88 RID: 2696 RVA: 0x0009D58C File Offset: 0x0009B78C
	public void NotifySizeChange()
	{
		if (this.rowCount > 20)
		{
			this.rowCount = 20;
		}
		if (this.colCount > 5)
		{
			this.colCount = 5;
		}
		this.RecalcIndexMaps();
		if (this.onSizeChange != null)
		{
			try
			{
				this.onSizeChange();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x1700017A RID: 378
	// (get) Token: 0x06000A89 RID: 2697 RVA: 0x0009D5F0 File Offset: 0x0009B7F0
	// (set) Token: 0x06000A8A RID: 2698 RVA: 0x0009D5FB File Offset: 0x0009B7FB
	public int pairCount
	{
		get
		{
			return this.gridsPairOffsets[100];
		}
		set
		{
			this.gridsPairOffsets[100] = value;
		}
	}

	// Token: 0x06000A8B RID: 2699 RVA: 0x0009D607 File Offset: 0x0009B807
	public void Init()
	{
		this.grids = new DeliveryPackage.GRID[100];
		this.gridsPairOffsets = new int[101];
		this.gridsPairs = null;
		this.ResetIndexMaps();
	}

	// Token: 0x06000A8C RID: 2700 RVA: 0x0009D630 File Offset: 0x0009B830
	public void Free()
	{
		this.grids = null;
		this.gridsPairOffsets = null;
		this.gridsPairs = null;
		this.ResetIndexMaps();
	}

	// Token: 0x06000A8D RID: 2701 RVA: 0x0009D64D File Offset: 0x0009B84D
	public void SetForNewGame()
	{
		this.enable = true;
		this.unlocked = false;
		this.rowCount = 0;
		this.colCount = 0;
		this.stackSizeMultiplier = Configs.freeMode.deliveryPackageStackSizeMultiplier;
		this.ResetIndexMaps();
	}

	// Token: 0x06000A8E RID: 2702 RVA: 0x0009D684 File Offset: 0x0009B884
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(100);
		w.Write(this.stackSizeMultiplier);
		for (int i = 0; i < 100; i++)
		{
			if (this.grids[i].itemId > 0)
			{
				w.Write(this.grids[i].itemId);
				w.Write(this.grids[i].count);
				w.Write(this.grids[i].inc);
				w.Write(this.grids[i].ordered);
				w.Write(this.grids[i].stackSize);
				w.Write(this.grids[i].requireCount);
				w.Write(this.grids[i].recycleCount);
			}
			else
			{
				w.Write(0);
			}
		}
		w.Write(this.enable);
		w.Write(this.unlocked);
		w.Write(this.rowCount);
		w.Write(this.colCount);
	}

	// Token: 0x06000A8F RID: 2703 RVA: 0x0009D7B0 File Offset: 0x0009B9B0
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		int num = r.ReadInt32();
		int stackSizeMultiplier = r.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			DeliveryPackage.GRID grid = new DeliveryPackage.GRID
			{
				itemId = r.ReadInt32()
			};
			if (grid.itemId > 0)
			{
				grid.count = r.ReadInt32();
				grid.inc = r.ReadInt32();
				grid.ordered = r.ReadInt32();
				grid.stackSize = r.ReadInt32();
				if (StorageComponent.itemStackCount[grid.itemId] > grid.stackSize)
				{
					grid.stackSize = StorageComponent.itemStackCount[grid.itemId];
				}
				grid.requireCount = r.ReadInt32();
				grid.recycleCount = r.ReadInt32();
			}
			if (i < 100)
			{
				this.grids[i] = grid;
			}
		}
		this.stackSizeMultiplier = stackSizeMultiplier;
		this.enable = r.ReadBoolean();
		this.unlocked = r.ReadBoolean();
		this.rowCount = r.ReadInt32();
		this.colCount = r.ReadInt32();
		if (this.rowCount > 20)
		{
			this.rowCount = 20;
		}
		if (this.colCount > 5)
		{
			this.colCount = 5;
		}
		this.RecalcIndexMaps();
	}

	// Token: 0x06000A90 RID: 2704 RVA: 0x0009D8E8 File Offset: 0x0009BAE8
	public void SetGridPairCapacity(int newCap)
	{
		if (this.gridsPairs == null)
		{
			this.gridsPairs = new SupplyDemandPair[newCap];
			return;
		}
		if (newCap <= this.gridsPairs.Length)
		{
			return;
		}
		SupplyDemandPair[] array = this.gridsPairs;
		this.gridsPairs = new SupplyDemandPair[newCap];
		Array.Copy(array, this.gridsPairs, array.Length);
	}

	// Token: 0x06000A91 RID: 2705 RVA: 0x0009D938 File Offset: 0x0009BB38
	public void AddGridPair(int sId, int sIdx, int dId, int dIdx)
	{
		if (this.gridsPairs == null)
		{
			this.SetGridPairCapacity(128);
		}
		if (this.pairCount == this.gridsPairs.Length)
		{
			this.SetGridPairCapacity(this.gridsPairs.Length * 2);
		}
		this.gridsPairs[this.pairCount].supplyId = sId;
		this.gridsPairs[this.pairCount].supplyIndex = sIdx;
		this.gridsPairs[this.pairCount].demandId = dId;
		this.gridsPairs[this.pairCount].demandIndex = dIdx;
		int pairCount = this.pairCount;
		this.pairCount = pairCount + 1;
	}

	// Token: 0x06000A92 RID: 2706 RVA: 0x0009D9E5 File Offset: 0x0009BBE5
	public void ClearPairs()
	{
		if (this.gridsPairOffsets != null)
		{
			Array.Clear(this.gridsPairOffsets, 0, 101);
		}
		if (this.gridsPairs != null)
		{
			Array.Clear(this.gridsPairs, 0, this.gridsPairs.Length);
		}
	}

	// Token: 0x06000A93 RID: 2707 RVA: 0x0009DA19 File Offset: 0x0009BC19
	public IntVector2 GetXYByIndex(int gridIndex)
	{
		return new IntVector2(4 - gridIndex % 5, gridIndex / 5);
	}

	// Token: 0x06000A94 RID: 2708 RVA: 0x0009DA28 File Offset: 0x0009BC28
	public int GetIndexByXY(int x, int y)
	{
		return 4 - x + y * 5;
	}

	// Token: 0x06000A95 RID: 2709 RVA: 0x0009DA34 File Offset: 0x0009BC34
	public bool IsGridActive(int gridIndex)
	{
		if (!this.unlocked)
		{
			return false;
		}
		if (gridIndex >= 100)
		{
			return false;
		}
		int num = 4 - gridIndex % 5;
		int num2 = gridIndex / 5;
		return num < this.colCount && num2 < this.rowCount;
	}

	// Token: 0x06000A96 RID: 2710 RVA: 0x0009DA70 File Offset: 0x0009BC70
	public void RecalcIndexMaps()
	{
		this.ResetIndexMaps();
		if (this.unlocked && this.colCount * this.rowCount > 0)
		{
			for (int i = 0; i < this.rowCount; i++)
			{
				for (int j = 0; j < this.colCount; j++)
				{
					int num = i * this.colCount + j;
					int num2 = (i + 1) * 5 - (this.colCount - j);
					this._pos2index[num] = num2;
					this._index2pos[num2] = num;
				}
			}
		}
	}

	// Token: 0x06000A97 RID: 2711 RVA: 0x0009DAEC File Offset: 0x0009BCEC
	private void ResetIndexMaps()
	{
		for (int i = 0; i < 100; i++)
		{
			this._index2pos[i] = -1;
			this._pos2index[i] = -1;
		}
	}

	// Token: 0x06000A98 RID: 2712 RVA: 0x0009DB18 File Offset: 0x0009BD18
	public int AddItem(int itemId, int count, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count <= 0)
		{
			return 0;
		}
		int num = -1;
		for (int i = 0; i < this.gridLength; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			return 0;
		}
		int num2 = count;
		if (this.grids[num].itemId == itemId)
		{
			int stackSizeModified = this.grids[num].stackSizeModified;
			int num3 = stackSizeModified - this.grids[num].count;
			if (count <= num3)
			{
				DeliveryPackage.GRID[] array = this.grids;
				int num4 = num;
				array[num4].count = array[num4].count + count;
				DeliveryPackage.GRID[] array2 = this.grids;
				int num5 = num;
				array2[num5].inc = array2[num5].inc + inc;
				inc = 0;
				remainInc = 0;
				count = 0;
			}
			else
			{
				this.grids[num].count = stackSizeModified;
				DeliveryPackage.GRID[] array3 = this.grids;
				int num6 = num;
				array3[num6].inc = array3[num6].inc + this.split_inc(ref count, ref inc, num3);
				remainInc = inc;
			}
		}
		return num2 - count;
	}

	// Token: 0x06000A99 RID: 2713 RVA: 0x0009DC18 File Offset: 0x0009BE18
	public int AddItem(int gridIndex, int itemId, int count, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count <= 0)
		{
			return 0;
		}
		int num = count;
		if (this.grids[gridIndex].itemId == itemId)
		{
			int stackSizeModified = this.grids[gridIndex].stackSizeModified;
			int num2 = stackSizeModified - this.grids[gridIndex].count;
			if (count <= num2)
			{
				DeliveryPackage.GRID[] array = this.grids;
				array[gridIndex].count = array[gridIndex].count + count;
				DeliveryPackage.GRID[] array2 = this.grids;
				array2[gridIndex].inc = array2[gridIndex].inc + inc;
				inc = 0;
				remainInc = 0;
				count = 0;
			}
			else
			{
				this.grids[gridIndex].count = stackSizeModified;
				DeliveryPackage.GRID[] array3 = this.grids;
				array3[gridIndex].inc = array3[gridIndex].inc + this.split_inc(ref count, ref inc, num2);
				remainInc = inc;
			}
		}
		return num - count;
	}

	// Token: 0x06000A9A RID: 2714 RVA: 0x0009DCEC File Offset: 0x0009BEEC
	public void TakeItemFromGrid(int gridIndex, ref int itemId, ref int count, out int inc)
	{
		inc = 0;
		if (this.grids[gridIndex].itemId == 0 || (itemId != 0 && this.grids[gridIndex].itemId != itemId))
		{
			itemId = 0;
			count = 0;
			return;
		}
		itemId = this.grids[gridIndex].itemId;
		if (count == 0)
		{
			count = this.grids[gridIndex].count;
			inc = this.grids[gridIndex].inc;
			this.grids[gridIndex].count = 0;
			this.grids[gridIndex].inc = 0;
		}
		else if (this.grids[gridIndex].count > count)
		{
			inc += this.split_inc(ref this.grids[gridIndex].count, ref this.grids[gridIndex].inc, count);
		}
		else
		{
			count = this.grids[gridIndex].count;
			inc = this.grids[gridIndex].inc;
			this.grids[gridIndex].count = 0;
			this.grids[gridIndex].inc = 0;
		}
		if (count == 0)
		{
			itemId = 0;
		}
	}

	// Token: 0x06000A9B RID: 2715 RVA: 0x0009DE34 File Offset: 0x0009C034
	public void TakeItems(ref int itemId, ref int count, out int inc)
	{
		inc = 0;
		if (itemId == 0)
		{
			return;
		}
		for (int i = 99; i >= 0; i--)
		{
			if (this.grids[i].itemId == itemId)
			{
				this.TakeItemFromGrid(i, ref itemId, ref count, out inc);
				return;
			}
		}
		itemId = 0;
		count = 0;
	}

	// Token: 0x06000A9C RID: 2716 RVA: 0x0009DE7C File Offset: 0x0009C07C
	public int GetItemCount(int itemId)
	{
		int num = 0;
		for (int i = 0; i < 100; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				num += this.grids[i].count;
			}
		}
		return num;
	}

	// Token: 0x06000A9D RID: 2717 RVA: 0x0009DEC4 File Offset: 0x0009C0C4
	public void SetDeliveryItem(int gridIndex, int itemId)
	{
		if (itemId != this.grids[gridIndex].itemId)
		{
			this.grids[gridIndex].ordered = 0;
			this.grids[gridIndex].count = 0;
			this.grids[gridIndex].inc = 0;
		}
		this.grids[gridIndex].itemId = itemId;
		this.grids[gridIndex].stackSize = ((itemId == 0) ? 0 : StorageComponent.itemStackCount[itemId]);
		this.grids[gridIndex].requireCount = 0;
		this.grids[gridIndex].recycleCount = int.MaxValue;
	}

	// Token: 0x06000A9E RID: 2718 RVA: 0x0009DF74 File Offset: 0x0009C174
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

	// Token: 0x04000CA2 RID: 3234
	public const int MAX_COLCOUNT = 5;

	// Token: 0x04000CA3 RID: 3235
	public const int MAX_ROWCOUNT = 20;

	// Token: 0x04000CA4 RID: 3236
	public const int MAX_COUNT = 100;

	// Token: 0x04000CA5 RID: 3237
	public DeliveryPackage.GRID[] grids;

	// Token: 0x04000CA6 RID: 3238
	public bool enable;

	// Token: 0x04000CA7 RID: 3239
	public bool unlocked;

	// Token: 0x04000CA8 RID: 3240
	public int rowCount;

	// Token: 0x04000CA9 RID: 3241
	public int colCount;

	// Token: 0x04000CAA RID: 3242
	public const int REQUIRE_COUNT_MAX_MULTIPLIER = 30;

	// Token: 0x04000CAC RID: 3244
	public SupplyDemandPair[] gridsPairs;

	// Token: 0x04000CAD RID: 3245
	public int[] gridsPairOffsets;

	// Token: 0x04000CAE RID: 3246
	public int[] _index2pos;

	// Token: 0x04000CAF RID: 3247
	public int[] _pos2index;

	// Token: 0x02000C03 RID: 3075
	public struct GRID
	{
		// Token: 0x170008D0 RID: 2256
		// (get) Token: 0x06007D8D RID: 32141 RVA: 0x0046EAF6 File Offset: 0x0046CCF6
		public int stackSizeModified
		{
			get
			{
				return this.stackSize * this.stackSizeMultiplier;
			}
		}

		// Token: 0x170008D1 RID: 2257
		// (get) Token: 0x06007D8E RID: 32142 RVA: 0x0046EB05 File Offset: 0x0046CD05
		public int modifiedCount
		{
			get
			{
				return this.count + this.ordered;
			}
		}

		// Token: 0x170008D2 RID: 2258
		// (get) Token: 0x06007D8F RID: 32143 RVA: 0x0046EB14 File Offset: 0x0046CD14
		public int clampedRequireCount
		{
			get
			{
				int num = this.stackSize * 30;
				if (this.requireCount > num)
				{
					return num;
				}
				return this.requireCount;
			}
		}

		// Token: 0x06007D90 RID: 32144 RVA: 0x0046EB3C File Offset: 0x0046CD3C
		public override string ToString()
		{
			if (this.itemId > 0)
			{
				if (this.ordered > 0)
				{
					return string.Format("[{0}] {1} / {2} (+{3}) / {4}", new object[]
					{
						this.itemId,
						this.requireCount,
						this.count,
						this.ordered,
						this.recycleCount
					});
				}
				if (this.ordered < 0)
				{
					return string.Format("[{0}] {1} / {2} ({3}) / {4}", new object[]
					{
						this.itemId,
						this.requireCount,
						this.count,
						this.ordered,
						this.recycleCount
					});
				}
				return string.Format("[{0}] {1} / {2} / {3}", new object[]
				{
					this.itemId,
					this.requireCount,
					this.count,
					this.recycleCount
				});
			}
			else
			{
				if (this.count != 0 || this.ordered != 0)
				{
					return string.Format("[{0}] {1} ({2}) ???", this.itemId, this.count, this.ordered);
				}
				return string.Format("[{0}]", this.itemId);
			}
		}

		// Token: 0x04007C5C RID: 31836
		public int itemId;

		// Token: 0x04007C5D RID: 31837
		public int count;

		// Token: 0x04007C5E RID: 31838
		public int inc;

		// Token: 0x04007C5F RID: 31839
		public int ordered;

		// Token: 0x04007C60 RID: 31840
		public int stackSize;

		// Token: 0x04007C61 RID: 31841
		public int stackSizeMultiplier;

		// Token: 0x04007C62 RID: 31842
		public int requireCount;

		// Token: 0x04007C63 RID: 31843
		public int recycleCount;
	}
}
