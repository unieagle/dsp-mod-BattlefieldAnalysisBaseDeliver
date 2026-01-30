using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x02000163 RID: 355
public class StorageComponent
{
	// Token: 0x06000A9A RID: 2714 RVA: 0x0009DC4C File Offset: 0x0009BE4C
	public StorageComponent(int _size)
	{
		StorageComponent.LoadStatic();
		this.size = _size;
		this.grids = new StorageComponent.GRID[this.size];
		this.searchStart = 0;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
	}

	// Token: 0x06000A9B RID: 2715 RVA: 0x0009DCAC File Offset: 0x0009BEAC
	private static void LoadStatic()
	{
		if (!StorageComponent.staticLoaded)
		{
			StorageComponent.itemIsFuel = new bool[12000];
			StorageComponent.itemStackCount = new int[12000];
			StorageComponent.itemIsAmmo = new bool[12000];
			StorageComponent.itemIsBomb = new bool[12000];
			StorageComponent.itemIsFighter = new bool[12000];
			for (int i = 0; i < 12000; i++)
			{
				StorageComponent.itemStackCount[i] = 1000;
			}
			ItemProto[] dataArray = LDB.items.dataArray;
			for (int j = 0; j < dataArray.Length; j++)
			{
				StorageComponent.itemIsFuel[dataArray[j].ID] = (dataArray[j].HeatValue > 0L);
				StorageComponent.itemIsAmmo[dataArray[j].ID] = dataArray[j].isAmmo;
				StorageComponent.itemIsBomb[dataArray[j].ID] = dataArray[j].isBomb;
				StorageComponent.itemIsFighter[dataArray[j].ID] = dataArray[j].isFighter;
				StorageComponent.itemStackCount[dataArray[j].ID] = dataArray[j].StackSize;
			}
			StorageComponent.staticLoaded = true;
		}
	}

	// Token: 0x06000A9C RID: 2716 RVA: 0x0009DDBC File Offset: 0x0009BFBC
	public void Export(BinaryWriter w)
	{
		w.Write(2);
		w.Write(this.id);
		w.Write(this.entityId);
		w.Write(this.previous);
		w.Write(this.next);
		w.Write(this.bottom);
		w.Write(this.top);
		w.Write((int)this.type);
		w.Write(this.size);
		w.Write(this.bans);
		for (int i = 0; i < this.size; i++)
		{
			w.Write(this.grids[i].itemId);
			w.Write(this.grids[i].filter);
			w.Write(this.grids[i].count);
			w.Write(this.grids[i].stackSize);
			w.Write(this.grids[i].inc);
		}
	}

	// Token: 0x06000A9D RID: 2717 RVA: 0x0009DEC0 File Offset: 0x0009C0C0
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.id = r.ReadInt32();
		this.entityId = r.ReadInt32();
		if (num >= 1)
		{
			this.previous = r.ReadInt32();
			this.next = r.ReadInt32();
			this.bottom = r.ReadInt32();
			this.top = r.ReadInt32();
		}
		this.type = (EStorageType)r.ReadInt32();
		int num2 = r.ReadInt32();
		if (num >= 1)
		{
			this.bans = r.ReadInt32();
		}
		this.SetSize(num2);
		for (int i = 0; i < this.size; i++)
		{
			this.grids[i].itemId = r.ReadInt32();
			this.grids[i].filter = r.ReadInt32();
			this.grids[i].count = r.ReadInt32();
			this.grids[i].stackSize = r.ReadInt32();
			if (num >= 2)
			{
				this.grids[i].inc = r.ReadInt32();
			}
			else
			{
				this.grids[i].inc = 0;
			}
			if (this.entityId > 0 && this.grids[i].itemId > 0)
			{
				ItemProto itemProto = LDB.items.Select(this.grids[i].itemId);
				if (itemProto != null)
				{
					this.grids[i].stackSize = itemProto.StackSize;
				}
			}
		}
	}

	// Token: 0x06000A9E RID: 2718 RVA: 0x0009E040 File Offset: 0x0009C240
	public void FindConnRefs(StorageComponent[] pool)
	{
		if (this.entityId > 0 && this.id != 0)
		{
			this.previousStorage = pool[this.previous];
			if (this.previousStorage == null || this.previousStorage.id != this.previous)
			{
				this.previousStorage = null;
				this.previous = 0;
			}
			this.nextStorage = pool[this.next];
			if (this.nextStorage == null || this.nextStorage.id != this.next)
			{
				this.nextStorage = null;
				this.next = 0;
			}
			this.bottomStorage = pool[this.bottom];
			if (this.bottomStorage == null || this.bottomStorage.id != this.bottom)
			{
				this.bottomStorage = null;
				this.bottom = 0;
			}
			this.topStorage = pool[this.top];
			if (this.topStorage == null || this.topStorage.id != this.top)
			{
				this.topStorage = null;
				this.top = 0;
				return;
			}
		}
		else
		{
			this.previousStorage = null;
			this.previous = 0;
			this.nextStorage = null;
			this.next = 0;
			this.bottomStorage = null;
			this.bottom = 0;
			this.topStorage = null;
			this.top = 0;
		}
	}

	// Token: 0x06000A9F RID: 2719 RVA: 0x0009E179 File Offset: 0x0009C379
	public void InitConn()
	{
		if (this.bottom == 0)
		{
			this.bottom = this.id;
			this.bottomStorage = this;
		}
		if (this.top == 0)
		{
			this.top = this.id;
			this.topStorage = this;
		}
	}

	// Token: 0x06000AA0 RID: 2720 RVA: 0x0009E1B4 File Offset: 0x0009C3B4
	public void CutNext()
	{
		if (this.bottom == 0)
		{
			this.bottom = this.id;
			this.bottomStorage = this;
		}
		if (this.top == 0)
		{
			this.top = this.id;
			this.topStorage = this;
		}
		this.next = 0;
		this.nextStorage = null;
		int num = 0;
		StorageComponent storageComponent = this;
		while (storageComponent != null)
		{
			storageComponent.top = this.id;
			storageComponent.topStorage = this;
			storageComponent = storageComponent.previousStorage;
			if (num++ > 256)
			{
				Assert.CannotBeReached();
				return;
			}
		}
	}

	// Token: 0x06000AA1 RID: 2721 RVA: 0x0009E23C File Offset: 0x0009C43C
	public void SetBans(int _bans)
	{
		this.bans = _bans;
		if (this.bans > this.size)
		{
			this.bans = this.size;
		}
		else if (this.bans < 0)
		{
			this.bans = 0;
		}
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
	}

	// Token: 0x06000AA2 RID: 2722 RVA: 0x0009E28A File Offset: 0x0009C48A
	public void ResetOptimizationFlags()
	{
		this.searchStart = 0;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
	}

	// Token: 0x14000007 RID: 7
	// (add) Token: 0x06000AA3 RID: 2723 RVA: 0x0009E2A4 File Offset: 0x0009C4A4
	// (remove) Token: 0x06000AA4 RID: 2724 RVA: 0x0009E2DC File Offset: 0x0009C4DC
	public event Action onStorageChange;

	// Token: 0x06000AA5 RID: 2725 RVA: 0x0009E314 File Offset: 0x0009C514
	public void NotifyStorageChange()
	{
		this.changed = true;
		if (this.onStorageChange != null)
		{
			try
			{
				this.onStorageChange();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x14000008 RID: 8
	// (add) Token: 0x06000AA6 RID: 2726 RVA: 0x0009E354 File Offset: 0x0009C554
	// (remove) Token: 0x06000AA7 RID: 2727 RVA: 0x0009E38C File Offset: 0x0009C58C
	public event Action onStorageSizeChange;

	// Token: 0x06000AA8 RID: 2728 RVA: 0x0009E3C4 File Offset: 0x0009C5C4
	public void NotifyStorageSizeChange()
	{
		if (this.onStorageSizeChange != null)
		{
			try
			{
				this.onStorageSizeChange();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x06000AA9 RID: 2729 RVA: 0x0009E400 File Offset: 0x0009C600
	public void SetSize(int newsize)
	{
		if (this.size == newsize)
		{
			return;
		}
		StorageComponent.GRID[] destinationArray = new StorageComponent.GRID[newsize];
		Array.Copy(this.grids, destinationArray, (newsize < this.size) ? newsize : this.size);
		this.grids = null;
		this.grids = destinationArray;
		this.size = newsize;
		this.searchStart = 0;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
		this.NotifyStorageChange();
		this.NotifyStorageSizeChange();
	}

	// Token: 0x06000AAA RID: 2730 RVA: 0x0009E474 File Offset: 0x0009C674
	public void SetSize(int newsize, int oldcol, int newcol)
	{
		if (this.size == newsize)
		{
			return;
		}
		StorageComponent.GRID[] array = new StorageComponent.GRID[newsize];
		for (int i = 0; i < this.grids.Length; i++)
		{
			int num = i / oldcol;
			int num2 = i % oldcol;
			int num3 = num * newcol + num2;
			array[num3] = this.grids[i];
		}
		this.grids = null;
		this.grids = array;
		this.size = newsize;
		this.searchStart = 0;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
		this.NotifyStorageChange();
		this.NotifyStorageSizeChange();
	}

	// Token: 0x06000AAB RID: 2731 RVA: 0x0009E4FC File Offset: 0x0009C6FC
	public void SetFilterDirect(int gridIndex, int filterId, int stackSize)
	{
		if (this.type > EStorageType.Default)
		{
			if (this.grids[gridIndex].itemId > 0 && this.grids[gridIndex].count > 0 && this.grids[gridIndex].itemId != filterId)
			{
				Debug.LogError("直接设置过滤器非法操作导致物品更改");
			}
			this.grids[gridIndex].filter = filterId;
			this.grids[gridIndex].itemId = filterId;
			this.grids[gridIndex].stackSize = stackSize;
			if (this.id > 0 && this.entityId > 0)
			{
				this.ResetOptimizationFlags();
			}
		}
	}

	// Token: 0x06000AAC RID: 2732 RVA: 0x0009E5AC File Offset: 0x0009C7AC
	public void SetFilter(int gridIndex, int filterId)
	{
		if (filterId < 0)
		{
			filterId = 0;
		}
		if (this.type > EStorageType.Default || filterId == 0)
		{
			if (filterId > 0)
			{
				if (this.grids[gridIndex].itemId == 0 || this.grids[gridIndex].count == 0 || this.grids[gridIndex].itemId == filterId)
				{
					this.grids[gridIndex].filter = filterId;
					this.grids[gridIndex].itemId = filterId;
					this.grids[gridIndex].stackSize = StorageComponent.itemStackCount[filterId];
				}
				else
				{
					Debug.LogWarning("非法设置过滤器，设计上应避免");
				}
			}
			else
			{
				this.grids[gridIndex].filter = 0;
				if (this.grids[gridIndex].count == 0)
				{
					this.grids[gridIndex].itemId = 0;
					this.grids[gridIndex].inc = 0;
					this.grids[gridIndex].stackSize = 0;
				}
			}
			if (this.id > 0 && this.entityId > 0)
			{
				this.ResetOptimizationFlags();
			}
		}
	}

	// Token: 0x06000AAD RID: 2733 RVA: 0x0009E6D0 File Offset: 0x0009C8D0
	public bool AddCargo(ref Cargo cargo, bool useBan = false)
	{
		if (cargo.item <= 0 || cargo.stack == 0 || cargo.item >= 12000)
		{
			return false;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[(int)cargo.item])
			{
				return false;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[(int)cargo.item] || StorageComponent.itemIsBomb[(int)cargo.item]))
			{
				return false;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[(int)cargo.item])
			{
				return false;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[(int)cargo.item])
			{
				return false;
			}
		}
		bool result = false;
		bool flag2 = false;
		int num = 0;
		int num2 = useBan ? (this.size - this.bans) : this.size;
		ref byte ptr = ref cargo.stack;
		int i = 0;
		while (i < num2)
		{
			if (this.grids[i].itemId != 0)
			{
				goto IL_173;
			}
			if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[i].filter <= 0) || (int)cargo.item == this.grids[i].filter)
			{
				if (num == 0)
				{
					num = StorageComponent.itemStackCount[(int)cargo.item];
				}
				this.grids[i].itemId = (int)cargo.item;
				if (this.grids[i].filter == 0)
				{
					this.grids[i].stackSize = num;
					goto IL_173;
				}
				goto IL_173;
			}
			IL_23A:
			i++;
			continue;
			IL_173:
			if (this.grids[i].itemId != (int)cargo.item)
			{
				goto IL_23A;
			}
			if (num == 0)
			{
				num = this.grids[i].stackSize;
			}
			int num3 = num - this.grids[i].count;
			if ((int)ptr <= num3)
			{
				StorageComponent.GRID[] array = this.grids;
				int num4 = i;
				array[num4].count = array[num4].count + (int)ptr;
				StorageComponent.GRID[] array2 = this.grids;
				int num5 = i;
				array2[num5].inc = array2[num5].inc + (int)cargo.inc;
				result = true;
				flag2 = true;
				break;
			}
			this.grids[i].count = num;
			StorageComponent.GRID[] array3 = this.grids;
			int num6 = i;
			array3[num6].inc = array3[num6].inc + this.split_inc(ref ptr, ref cargo.inc, (byte)num3);
			flag2 = true;
			goto IL_23A;
		}
		if (flag2)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return result;
	}

	// Token: 0x06000AAE RID: 2734 RVA: 0x0009E940 File Offset: 0x0009CB40
	public int AddItem(int itemId, int count, int inc, out int remainInc, bool useBan = false)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		if (itemId >= 12000)
		{
			return count;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = count;
		int num2 = 0;
		int num3 = useBan ? (this.size - this.bans) : this.size;
		int i = 0;
		while (i < num3)
		{
			if (this.grids[i].itemId != 0)
			{
				goto IL_135;
			}
			if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[i].filter <= 0) || itemId == this.grids[i].filter)
			{
				if (num2 == 0)
				{
					num2 = StorageComponent.itemStackCount[itemId];
				}
				this.grids[i].itemId = itemId;
				if (this.grids[i].filter == 0)
				{
					this.grids[i].stackSize = num2;
					goto IL_135;
				}
				goto IL_135;
			}
			IL_1FA:
			i++;
			continue;
			IL_135:
			if (this.grids[i].itemId != itemId)
			{
				goto IL_1FA;
			}
			if (num2 == 0)
			{
				num2 = this.grids[i].stackSize;
			}
			int num4 = num2 - this.grids[i].count;
			if (count <= num4)
			{
				StorageComponent.GRID[] array = this.grids;
				int num5 = i;
				array[num5].count = array[num5].count + count;
				StorageComponent.GRID[] array2 = this.grids;
				int num6 = i;
				array2[num6].inc = array2[num6].inc + inc;
				inc = 0;
				remainInc = 0;
				count = 0;
				break;
			}
			this.grids[i].count = num2;
			StorageComponent.GRID[] array3 = this.grids;
			int num7 = i;
			array3[num7].inc = array3[num7].inc + this.split_inc(ref count, ref inc, num4);
			remainInc = inc;
			if (this.type != EStorageType.DeliveryFiltered)
			{
				goto IL_1FA;
			}
			break;
		}
		int num8 = num - count;
		if (num8 > 0)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return num8;
	}

	// Token: 0x06000AAF RID: 2735 RVA: 0x0009EB70 File Offset: 0x0009CD70
	public int AddItemStacked(int itemId, int count, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = count;
		int num2 = 0;
		for (int i = 0; i < this.size; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				if (num2 == 0)
				{
					num2 = this.grids[i].stackSize;
				}
				int num3 = num2 - this.grids[i].count;
				if (count <= num3)
				{
					StorageComponent.GRID[] array = this.grids;
					int num4 = i;
					array[num4].count = array[num4].count + count;
					StorageComponent.GRID[] array2 = this.grids;
					int num5 = i;
					array2[num5].inc = array2[num5].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[i].count = num2;
				StorageComponent.GRID[] array3 = this.grids;
				int num6 = i;
				array3[num6].inc = array3[num6].inc + this.split_inc(ref count, ref inc, num3);
				remainInc = inc;
				if (this.type == EStorageType.DeliveryFiltered)
				{
					break;
				}
			}
		}
		if (count > 0)
		{
			int j = 0;
			while (j < this.size)
			{
				if (this.grids[j].itemId != 0)
				{
					goto IL_1ED;
				}
				if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[j].filter <= 0) || itemId == this.grids[j].filter)
				{
					if (num2 == 0)
					{
						num2 = StorageComponent.itemStackCount[itemId];
					}
					this.grids[j].itemId = itemId;
					if (this.grids[j].filter == 0)
					{
						this.grids[j].stackSize = num2;
						goto IL_1ED;
					}
					goto IL_1ED;
				}
				IL_2B2:
				j++;
				continue;
				IL_1ED:
				if (this.grids[j].itemId != itemId)
				{
					goto IL_2B2;
				}
				if (num2 == 0)
				{
					num2 = this.grids[j].stackSize;
				}
				int num7 = num2 - this.grids[j].count;
				if (count <= num7)
				{
					StorageComponent.GRID[] array4 = this.grids;
					int num8 = j;
					array4[num8].count = array4[num8].count + count;
					StorageComponent.GRID[] array5 = this.grids;
					int num9 = j;
					array5[num9].inc = array5[num9].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[j].count = num2;
				StorageComponent.GRID[] array6 = this.grids;
				int num10 = j;
				array6[num10].inc = array6[num10].inc + this.split_inc(ref count, ref inc, num7);
				remainInc = inc;
				if (this.type != EStorageType.DeliveryFiltered)
				{
					goto IL_2B2;
				}
				break;
			}
		}
		int num11 = num - count;
		if (num11 > 0)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return num11;
	}

	// Token: 0x06000AB0 RID: 2736 RVA: 0x0009EE60 File Offset: 0x0009D060
	public int AddItemFiltered(int itemId, int count, int inc, out int remainInc, bool useBan)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = count;
		int num2 = 0;
		int num3 = useBan ? (this.size - this.bans) : this.size;
		for (int i = 0; i < num3; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				if (num2 == 0)
				{
					num2 = this.grids[i].stackSize;
				}
				int num4 = num2 - this.grids[i].count;
				if (count <= num4)
				{
					StorageComponent.GRID[] array = this.grids;
					int num5 = i;
					array[num5].count = array[num5].count + count;
					StorageComponent.GRID[] array2 = this.grids;
					int num6 = i;
					array2[num6].inc = array2[num6].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[i].count = num2;
				StorageComponent.GRID[] array3 = this.grids;
				int num7 = i;
				array3[num7].inc = array3[num7].inc + this.split_inc(ref count, ref inc, num4);
				remainInc = inc;
				if (this.type == EStorageType.DeliveryFiltered)
				{
					break;
				}
			}
		}
		if (count > 0)
		{
			int j = 0;
			while (j < num3)
			{
				if (this.grids[j].itemId != 0)
				{
					goto IL_20D;
				}
				if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[j].filter <= 0) || itemId == this.grids[j].filter)
				{
					if (num2 == 0)
					{
						num2 = StorageComponent.itemStackCount[itemId];
					}
					this.grids[j].itemId = itemId;
					if (this.grids[j].filter == 0)
					{
						this.grids[j].stackSize = num2;
						goto IL_20D;
					}
					goto IL_20D;
				}
				IL_2D2:
				j++;
				continue;
				IL_20D:
				if (this.grids[j].itemId != itemId)
				{
					goto IL_2D2;
				}
				if (num2 == 0)
				{
					num2 = this.grids[j].stackSize;
				}
				int num8 = num2 - this.grids[j].count;
				if (count <= num8)
				{
					StorageComponent.GRID[] array4 = this.grids;
					int num9 = j;
					array4[num9].count = array4[num9].count + count;
					StorageComponent.GRID[] array5 = this.grids;
					int num10 = j;
					array5[num10].inc = array5[num10].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[j].count = num2;
				StorageComponent.GRID[] array6 = this.grids;
				int num11 = j;
				array6[num11].inc = array6[num11].inc + this.split_inc(ref count, ref inc, num8);
				remainInc = inc;
				if (this.type != EStorageType.DeliveryFiltered)
				{
					goto IL_2D2;
				}
				break;
			}
		}
		int num12 = num - count;
		if (num12 > 0)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return num12;
	}

	// Token: 0x06000AB1 RID: 2737 RVA: 0x0009F168 File Offset: 0x0009D368
	public int AddItemFilteredBanOnly(int itemId, int count, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = count;
		int num2 = 0;
		int num3 = (this.bans > 0) ? (this.size - this.bans) : 0;
		for (int i = num3; i < this.size; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				if (num2 == 0)
				{
					num2 = this.grids[i].stackSize;
				}
				int num4 = num2 - this.grids[i].count;
				if (count <= num4)
				{
					StorageComponent.GRID[] array = this.grids;
					int num5 = i;
					array[num5].count = array[num5].count + count;
					StorageComponent.GRID[] array2 = this.grids;
					int num6 = i;
					array2[num6].inc = array2[num6].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[i].count = num2;
				StorageComponent.GRID[] array3 = this.grids;
				int num7 = i;
				array3[num7].inc = array3[num7].inc + this.split_inc(ref count, ref inc, num4);
				remainInc = inc;
				if (this.type == EStorageType.DeliveryFiltered)
				{
					break;
				}
			}
		}
		if (count > 0)
		{
			int j = num3;
			while (j < this.size)
			{
				if (this.grids[j].itemId != 0)
				{
					goto IL_212;
				}
				if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[j].filter <= 0) || itemId == this.grids[j].filter)
				{
					if (num2 == 0)
					{
						num2 = StorageComponent.itemStackCount[itemId];
					}
					this.grids[j].itemId = itemId;
					if (this.grids[j].filter == 0)
					{
						this.grids[j].stackSize = num2;
						goto IL_212;
					}
					goto IL_212;
				}
				IL_2D7:
				j++;
				continue;
				IL_212:
				if (this.grids[j].itemId != itemId)
				{
					goto IL_2D7;
				}
				if (num2 == 0)
				{
					num2 = this.grids[j].stackSize;
				}
				int num8 = num2 - this.grids[j].count;
				if (count <= num8)
				{
					StorageComponent.GRID[] array4 = this.grids;
					int num9 = j;
					array4[num9].count = array4[num9].count + count;
					StorageComponent.GRID[] array5 = this.grids;
					int num10 = j;
					array5[num10].inc = array5[num10].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[j].count = num2;
				StorageComponent.GRID[] array6 = this.grids;
				int num11 = j;
				array6[num11].inc = array6[num11].inc + this.split_inc(ref count, ref inc, num8);
				remainInc = inc;
				if (this.type != EStorageType.DeliveryFiltered)
				{
					goto IL_2D7;
				}
				break;
			}
		}
		int num12 = num - count;
		if (num12 > 0)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return num12;
	}

	// Token: 0x06000AB2 RID: 2738 RVA: 0x0009F47C File Offset: 0x0009D67C
	private int AddItemForSort(int itemId, int count, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = count;
		int num2 = 0;
		for (int i = 0; i < this.size; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				if (num2 == 0)
				{
					num2 = this.grids[i].stackSize;
				}
				int num3 = num2 - this.grids[i].count;
				if (count <= num3)
				{
					StorageComponent.GRID[] array = this.grids;
					int num4 = i;
					array[num4].count = array[num4].count + count;
					StorageComponent.GRID[] array2 = this.grids;
					int num5 = i;
					array2[num5].inc = array2[num5].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[i].count = num2;
				StorageComponent.GRID[] array3 = this.grids;
				int num6 = i;
				array3[num6].inc = array3[num6].inc + this.split_inc(ref count, ref inc, num3);
				remainInc = inc;
				if (this.type == EStorageType.DeliveryFiltered)
				{
					break;
				}
			}
		}
		if (count > 0)
		{
			int j = 0;
			while (j < this.size)
			{
				if (this.grids[j].itemId != 0)
				{
					goto IL_1ED;
				}
				if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[j].filter <= 0) || itemId == this.grids[j].filter)
				{
					if (num2 == 0)
					{
						num2 = StorageComponent.itemStackCount[itemId];
					}
					this.grids[j].itemId = itemId;
					if (this.grids[j].filter == 0)
					{
						this.grids[j].stackSize = num2;
						goto IL_1ED;
					}
					goto IL_1ED;
				}
				IL_2B2:
				j++;
				continue;
				IL_1ED:
				if (this.grids[j].itemId != itemId)
				{
					goto IL_2B2;
				}
				if (num2 == 0)
				{
					num2 = this.grids[j].stackSize;
				}
				int num7 = num2 - this.grids[j].count;
				if (count <= num7)
				{
					StorageComponent.GRID[] array4 = this.grids;
					int num8 = j;
					array4[num8].count = array4[num8].count + count;
					StorageComponent.GRID[] array5 = this.grids;
					int num9 = j;
					array5[num9].inc = array5[num9].inc + inc;
					inc = 0;
					remainInc = 0;
					count = 0;
					break;
				}
				this.grids[j].count = num2;
				StorageComponent.GRID[] array6 = this.grids;
				int num10 = j;
				array6[num10].inc = array6[num10].inc + this.split_inc(ref count, ref inc, num7);
				remainInc = inc;
				if (this.type != EStorageType.DeliveryFiltered)
				{
					goto IL_2B2;
				}
				break;
			}
		}
		return num - count;
	}

	// Token: 0x06000AB3 RID: 2739 RVA: 0x0009F754 File Offset: 0x0009D954
	public int AddItem(int itemId, int count, int startIndex, int length, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = count;
		int num2 = 0;
		if (this.type == EStorageType.DeliveryFiltered)
		{
			length = 1;
		}
		int i = startIndex;
		while (i < startIndex + length)
		{
			int num3 = i % this.size;
			if (this.grids[num3].itemId != 0)
			{
				goto IL_127;
			}
			if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[num3].filter <= 0) || itemId == this.grids[num3].filter)
			{
				if (num2 == 0)
				{
					num2 = StorageComponent.itemStackCount[itemId];
				}
				this.grids[num3].itemId = itemId;
				if (this.grids[num3].filter == 0)
				{
					this.grids[num3].stackSize = num2;
					goto IL_127;
				}
				goto IL_127;
			}
			IL_1EE:
			i++;
			continue;
			IL_127:
			if (this.grids[num3].itemId != itemId)
			{
				goto IL_1EE;
			}
			if (num2 == 0)
			{
				num2 = this.grids[num3].stackSize;
			}
			int num4 = num2 - this.grids[num3].count;
			if (count <= num4)
			{
				StorageComponent.GRID[] array = this.grids;
				int num5 = num3;
				array[num5].count = array[num5].count + count;
				StorageComponent.GRID[] array2 = this.grids;
				int num6 = num3;
				array2[num6].inc = array2[num6].inc + inc;
				inc = 0;
				remainInc = 0;
				count = 0;
				break;
			}
			this.grids[num3].count = num2;
			StorageComponent.GRID[] array3 = this.grids;
			int num7 = num3;
			array3[num7].inc = array3[num7].inc + this.split_inc(ref count, ref inc, num4);
			remainInc = inc;
			if (this.type != EStorageType.DeliveryFiltered)
			{
				goto IL_1EE;
			}
			break;
		}
		int num8 = num - count;
		if (num8 > 0)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return num8;
	}

	// Token: 0x06000AB4 RID: 2740 RVA: 0x0009F978 File Offset: 0x0009DB78
	public int AddItemBanGridFirst(int itemId, int count, int inc, out int remainInc)
	{
		remainInc = inc;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		bool flag = this.type > EStorageType.Default;
		if (flag)
		{
			if (this.type == EStorageType.Fuel && !StorageComponent.itemIsFuel[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Ammo && (!StorageComponent.itemIsAmmo[itemId] || StorageComponent.itemIsBomb[itemId]))
			{
				return 0;
			}
			if (this.type == EStorageType.Bomb && !StorageComponent.itemIsBomb[itemId])
			{
				return 0;
			}
			if (this.type == EStorageType.Fighter && !StorageComponent.itemIsFighter[itemId])
			{
				return 0;
			}
		}
		int num = (this.bans == 0) ? 0 : (this.size - this.bans);
		int num2 = count;
		int num3 = 0;
		int i = num;
		while (i < num + this.size)
		{
			int num4 = i % this.size;
			if (this.grids[num4].itemId != 0)
			{
				goto IL_135;
			}
			if (!flag || (this.type != EStorageType.DeliveryFiltered && this.grids[num4].filter <= 0) || itemId == this.grids[num4].filter)
			{
				if (num3 == 0)
				{
					num3 = StorageComponent.itemStackCount[itemId];
				}
				this.grids[num4].itemId = itemId;
				if (this.grids[num4].filter == 0)
				{
					this.grids[num4].stackSize = num3;
					goto IL_135;
				}
				goto IL_135;
			}
			IL_1FA:
			i++;
			continue;
			IL_135:
			if (this.grids[num4].itemId != itemId)
			{
				goto IL_1FA;
			}
			if (num3 == 0)
			{
				num3 = this.grids[num4].stackSize;
			}
			int num5 = num3 - this.grids[num4].count;
			if (count <= num5)
			{
				StorageComponent.GRID[] array = this.grids;
				int num6 = num4;
				array[num6].count = array[num6].count + count;
				StorageComponent.GRID[] array2 = this.grids;
				int num7 = num4;
				array2[num7].inc = array2[num7].inc + inc;
				inc = 0;
				remainInc = 0;
				count = 0;
				break;
			}
			this.grids[num4].count = num3;
			StorageComponent.GRID[] array3 = this.grids;
			int num8 = num4;
			array3[num8].inc = array3[num8].inc + this.split_inc(ref count, ref inc, num5);
			remainInc = inc;
			if (this.type != EStorageType.DeliveryFiltered)
			{
				goto IL_1FA;
			}
			break;
		}
		int num9 = num2 - count;
		if (num9 > 0)
		{
			this.searchStart = 0;
			this.lastEmptyItem = -1;
			this.NotifyStorageChange();
		}
		return num9;
	}

	// Token: 0x06000AB5 RID: 2741 RVA: 0x0009FBB0 File Offset: 0x0009DDB0
	public int TakeItem(int itemId, int count, out int inc)
	{
		inc = 0;
		if (itemId <= 0 || count == 0)
		{
			return 0;
		}
		int num = count;
		for (int i = 0; i < this.size; i++)
		{
			if (this.grids[i].itemId == itemId && this.grids[i].count > 0)
			{
				if (this.grids[i].count >= count)
				{
					inc += this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, count);
					count = 0;
				}
				else
				{
					count -= this.grids[i].count;
					this.grids[i].count = 0;
					inc += this.grids[i].inc;
					this.grids[i].inc = 0;
				}
				if (this.grids[i].count == 0)
				{
					this.grids[i].itemId = this.grids[i].filter;
					this.grids[i].inc = 0;
					if (this.grids[i].filter == 0)
					{
						this.grids[i].stackSize = 0;
					}
				}
				if (count == 0)
				{
					break;
				}
			}
		}
		if (num > count)
		{
			this.lastFullItem = -1;
		}
		this.NotifyStorageChange();
		return num - count;
	}

	// Token: 0x06000AB6 RID: 2742 RVA: 0x0009FD2C File Offset: 0x0009DF2C
	public void TakeItemFromGrid(int gridIndex, ref int itemId, ref int count, out int inc)
	{
		inc = 0;
		if (this.grids[gridIndex].itemId == 0 || this.grids[gridIndex].count == 0 || (itemId != 0 && this.grids[gridIndex].itemId != itemId))
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
			this.grids[gridIndex].itemId = this.grids[gridIndex].filter;
			this.grids[gridIndex].count = 0;
			this.grids[gridIndex].inc = 0;
			if (this.grids[gridIndex].filter == 0)
			{
				this.grids[gridIndex].stackSize = 0;
			}
		}
		else if (this.grids[gridIndex].count > count)
		{
			inc += this.split_inc(ref this.grids[gridIndex].count, ref this.grids[gridIndex].inc, count);
		}
		else
		{
			count = this.grids[gridIndex].count;
			inc = this.grids[gridIndex].inc;
			this.grids[gridIndex].itemId = this.grids[gridIndex].filter;
			this.grids[gridIndex].count = 0;
			this.grids[gridIndex].inc = 0;
			if (this.grids[gridIndex].filter == 0)
			{
				this.grids[gridIndex].stackSize = 0;
			}
		}
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		this.lastFullItem = -1;
		this.NotifyStorageChange();
	}

	// Token: 0x06000AB7 RID: 2743 RVA: 0x0009FF2C File Offset: 0x0009E12C
	public void TakeHeadItems(ref int itemId, ref int count, out int inc)
	{
		inc = 0;
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		int num = count;
		count = 0;
		for (int i = 0; i < this.size; i++)
		{
			if (this.grids[i].itemId != 0 && this.grids[i].count != 0 && (itemId == 0 || this.grids[i].itemId == itemId))
			{
				itemId = this.grids[i].itemId;
				if (this.grids[i].count > num)
				{
					inc += this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, num);
					count += num;
					break;
				}
				inc += this.grids[i].inc;
				count += this.grids[i].count;
				num -= this.grids[i].count;
				this.grids[i].itemId = this.grids[i].filter;
				this.grids[i].count = 0;
				this.grids[i].inc = 0;
				if (this.grids[i].filter == 0)
				{
					this.grids[i].stackSize = 0;
				}
			}
		}
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		this.lastFullItem = -1;
		this.NotifyStorageChange();
	}

	// Token: 0x06000AB8 RID: 2744 RVA: 0x000A00D4 File Offset: 0x0009E2D4
	public void TakeTailItems(ref int itemId, ref int count, out int inc, bool useBan = false)
	{
		inc = 0;
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		int num = count;
		count = 0;
		for (int i = useBan ? (this.size - this.bans - 1) : (this.size - 1); i >= 0; i--)
		{
			if (this.grids[i].itemId != 0 && this.grids[i].count != 0 && (itemId == 0 || this.grids[i].itemId == itemId))
			{
				itemId = this.grids[i].itemId;
				if (this.grids[i].count > num)
				{
					inc += this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, num);
					count += num;
					break;
				}
				inc += this.grids[i].inc;
				count += this.grids[i].count;
				num -= this.grids[i].count;
				this.grids[i].itemId = this.grids[i].filter;
				this.grids[i].count = 0;
				this.grids[i].inc = 0;
				if (this.grids[i].filter == 0)
				{
					this.grids[i].stackSize = 0;
				}
			}
		}
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		this.lastFullItem = -1;
		this.NotifyStorageChange();
	}

	// Token: 0x06000AB9 RID: 2745 RVA: 0x000A0290 File Offset: 0x0009E490
	public bool TakeTailFuel(ref int itemId, ref int count, int fuelMask, out int inc, bool useBan = false)
	{
		inc = 0;
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return false;
		}
		bool result = false;
		int num = count;
		count = 0;
		int num2 = useBan ? (this.size - this.bans - 1) : (this.size - 1);
		int[] array = ItemProto.fuelNeeds[fuelMask];
		for (int i = num2; i >= 0; i--)
		{
			if (this.grids[i].itemId != 0 && this.grids[i].count != 0 && (itemId == 0 || this.grids[i].itemId == itemId))
			{
				result = true;
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] == this.grids[i].itemId)
					{
						itemId = this.grids[i].itemId;
						if (this.grids[i].count > num)
						{
							inc += this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, num);
							count += num;
							num = 0;
							break;
						}
						inc += this.grids[i].inc;
						count += this.grids[i].count;
						num -= this.grids[i].count;
						this.grids[i].itemId = this.grids[i].filter;
						this.grids[i].count = 0;
						this.grids[i].inc = 0;
						if (this.grids[i].filter == 0)
						{
							this.grids[i].stackSize = 0;
						}
					}
				}
			}
		}
		if (count == 0)
		{
			itemId = 0;
			count = 0;
		}
		else
		{
			this.lastFullItem = -1;
			this.NotifyStorageChange();
		}
		return result;
	}

	// Token: 0x06000ABA RID: 2746 RVA: 0x000A0494 File Offset: 0x0009E694
	public bool TakeTailItems(ref int itemId, ref int count, int[] needs, out int inc, bool useBan = false)
	{
		inc = 0;
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return false;
		}
		bool result = false;
		int num = count;
		count = 0;
		for (int i = useBan ? (this.size - this.bans - 1) : (this.size - 1); i >= 0; i--)
		{
			if (this.grids[i].itemId != 0 && this.grids[i].count != 0 && (itemId == 0 || this.grids[i].itemId == itemId))
			{
				result = true;
				if (this.grids[i].itemId == needs[0] || this.grids[i].itemId == needs[1] || this.grids[i].itemId == needs[2] || this.grids[i].itemId == needs[3] || this.grids[i].itemId == needs[4] || this.grids[i].itemId == needs[5])
				{
					itemId = this.grids[i].itemId;
					if (this.grids[i].count > num)
					{
						inc += this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, num);
						count += num;
						break;
					}
					inc += this.grids[i].inc;
					count += this.grids[i].count;
					num -= this.grids[i].count;
					this.grids[i].itemId = this.grids[i].filter;
					this.grids[i].count = 0;
					this.grids[i].inc = 0;
					if (this.grids[i].filter == 0)
					{
						this.grids[i].stackSize = 0;
					}
				}
			}
		}
		if (count == 0)
		{
			itemId = 0;
			count = 0;
		}
		else
		{
			this.lastFullItem = -1;
			this.NotifyStorageChange();
		}
		return result;
	}

	// Token: 0x06000ABB RID: 2747 RVA: 0x000A06E4 File Offset: 0x0009E8E4
	public void TakeTailItemsFiltered(ref int filter, ref int count, out int inc, bool useBan = false)
	{
		inc = 0;
		if (count == 0)
		{
			filter = 0;
			count = 0;
			return;
		}
		int num = count;
		count = 0;
		for (int i = useBan ? (this.size - this.bans - 1) : (this.size - 1); i >= 0; i--)
		{
			if (this.grids[i].itemId != 0 && this.grids[i].count != 0 && (filter <= 0 || this.grids[i].itemId == filter) && (filter >= 0 || this.grids[i].itemId != -filter))
			{
				filter = this.grids[i].itemId;
				if (this.grids[i].count > num)
				{
					inc += this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, num);
					count += num;
					break;
				}
				inc += this.grids[i].inc;
				count += this.grids[i].count;
				num -= this.grids[i].count;
				this.grids[i].itemId = this.grids[i].filter;
				this.grids[i].count = 0;
				this.grids[i].inc = 0;
				if (this.grids[i].filter == 0)
				{
					this.grids[i].stackSize = 0;
				}
			}
		}
		if (count == 0)
		{
			filter = 0;
			count = 0;
			return;
		}
		this.lastFullItem = -1;
		this.NotifyStorageChange();
	}

	// Token: 0x06000ABC RID: 2748 RVA: 0x000A08C0 File Offset: 0x0009EAC0
	public void TakeTailItemsWithIncTable(int itemId, ref int count, out int inc, ref int[] incTable, bool useBan = false)
	{
		inc = 0;
		if (incTable == null || incTable.Length <= 10)
		{
			incTable = new int[11];
		}
		else
		{
			Array.Clear(incTable, 0, incTable.Length);
		}
		if (count == 0)
		{
			return;
		}
		int num = count;
		count = 0;
		for (int i = useBan ? (this.size - this.bans - 1) : (this.size - 1); i >= 0; i--)
		{
			if (this.grids[i].itemId != 0 && this.grids[i].count != 0 && (itemId == 0 || this.grids[i].itemId == itemId))
			{
				itemId = this.grids[i].itemId;
				if (this.grids[i].count > num)
				{
					int num2 = this.split_inc(ref this.grids[i].count, ref this.grids[i].inc, num);
					inc += num2;
					count += num;
					int num3 = num2 / num;
					int num4 = num2 - num * num3;
					int num5 = num - num4;
					incTable[(num3 > 10) ? 10 : num3] += num5;
					num3++;
					incTable[(num3 > 10) ? 10 : num3] += num4;
					break;
				}
				int num6 = this.grids[i].inc / this.grids[i].count;
				int num7 = this.grids[i].inc - this.grids[i].count * num6;
				int num8 = this.grids[i].count - num7;
				incTable[(num6 > 10) ? 10 : num6] += num8;
				num6++;
				incTable[(num6 > 10) ? 10 : num6] += num7;
				inc += this.grids[i].inc;
				count += this.grids[i].count;
				num -= this.grids[i].count;
				this.grids[i].itemId = this.grids[i].filter;
				this.grids[i].count = 0;
				this.grids[i].inc = 0;
				if (this.grids[i].filter == 0)
				{
					this.grids[i].stackSize = 0;
				}
				if (num == 0)
				{
					break;
				}
			}
		}
		if (count == 0)
		{
			itemId = 0;
			count = 0;
			return;
		}
		this.lastFullItem = -1;
		this.NotifyStorageChange();
	}

	// Token: 0x06000ABD RID: 2749 RVA: 0x000A0B84 File Offset: 0x0009ED84
	public bool TakeTailItemsByIncTable(int itemId, out int count, ref int[] incTable, bool useBan = false)
	{
		count = 0;
		if (itemId == 0 || incTable == null)
		{
			return false;
		}
		for (int i = useBan ? (this.size - this.bans - 1) : (this.size - 1); i >= 0; i--)
		{
			if (this.grids[i].itemId == itemId && this.grids[i].count != 0)
			{
				int num = this.grids[i].inc / this.grids[i].count;
				int num2 = this.grids[i].inc - this.grids[i].count * num;
				int num3 = this.grids[i].count - num2;
				if (num > 10)
				{
					num = 10;
				}
				if (incTable[num] > num3)
				{
					incTable[num] -= num3;
					count += num3;
					int num4 = num3 * num;
					StorageComponent.GRID[] array = this.grids;
					int num5 = i;
					array[num5].inc = array[num5].inc - num4;
					StorageComponent.GRID[] array2 = this.grids;
					int num6 = i;
					array2[num6].count = array2[num6].count - num3;
				}
				else if (incTable[num] > 0)
				{
					count += incTable[num];
					int num7 = incTable[num] * num;
					StorageComponent.GRID[] array3 = this.grids;
					int num8 = i;
					array3[num8].inc = array3[num8].inc - num7;
					StorageComponent.GRID[] array4 = this.grids;
					int num9 = i;
					array4[num9].count = array4[num9].count - incTable[num];
					incTable[num] = 0;
				}
				num = ((++num > 10) ? 10 : num);
				if (incTable[num] > num2)
				{
					incTable[num] -= num2;
					count += num2;
					int num10 = num2 * num;
					StorageComponent.GRID[] array5 = this.grids;
					int num11 = i;
					array5[num11].inc = array5[num11].inc - num10;
					this.grids[i].count = num2;
				}
				else if (incTable[num] > 0)
				{
					count += incTable[num];
					int num12 = incTable[num] * num;
					StorageComponent.GRID[] array6 = this.grids;
					int num13 = i;
					array6[num13].inc = array6[num13].inc - num12;
					StorageComponent.GRID[] array7 = this.grids;
					int num14 = i;
					array7[num14].count = array7[num14].count - incTable[num];
				}
				if (this.grids[i].count == 0)
				{
					this.grids[i].itemId = this.grids[i].filter;
				}
			}
		}
		if (count != 0)
		{
			this.lastFullItem = -1;
			this.NotifyStorageChange();
		}
		for (int j = 0; j < incTable.Length; j++)
		{
			if (incTable[j] > 0)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000ABE RID: 2750 RVA: 0x000A0E00 File Offset: 0x0009F000
	public void TransferTo(StorageComponent other)
	{
		for (int i = this.grids.Length - 1; i >= 0; i--)
		{
			int num = 0;
			int num2 = 0;
			int inc;
			this.TakeItemFromGrid(i, ref num, ref num2, out inc);
			if (num2 > 0 && num > 0)
			{
				int inc2;
				int num3 = other.AddItemStacked(num, num2, inc, out inc2);
				if (num3 < num2)
				{
					int num4;
					this.AddItem(num, num2 - num3, i, 1, inc2, out num4);
				}
			}
		}
	}

	// Token: 0x06000ABF RID: 2751 RVA: 0x000A0E60 File Offset: 0x0009F060
	public void TransferTo(StorageComponent other, ItemBundle change)
	{
		for (int i = this.grids.Length - 1; i >= 0; i--)
		{
			int num = 0;
			int num2 = 0;
			int inc;
			this.TakeItemFromGrid(i, ref num, ref num2, out inc);
			if (num2 > 0 && num > 0)
			{
				int inc2;
				int num3 = other.AddItemStacked(num, num2, inc, out inc2);
				if (num3 < num2)
				{
					int num4;
					this.AddItem(num, num2 - num3, i, 1, inc2, out num4);
				}
				change.Alter(num, num3);
			}
		}
	}

	// Token: 0x06000AC0 RID: 2752 RVA: 0x000A0EC8 File Offset: 0x0009F0C8
	public void Sort(bool raiseNotify = true)
	{
		if (this.type != EStorageType.Default && this.type != EStorageType.Filtered)
		{
			return;
		}
		ItemProtoSet items = LDB.items;
		if (StorageComponent.s_dict4sort == null)
		{
			StorageComponent.s_dict4sort = new SortedDictionary<int, IDCNTINC>();
		}
		StorageComponent.s_dict4sort.Clear();
		for (int i = 0; i < this.grids.Length; i++)
		{
			ItemProto itemProto = items.Select(this.grids[i].itemId);
			int key = 10000 + this.grids[i].itemId;
			if (itemProto != null)
			{
				key = itemProto.index;
			}
			if (!StorageComponent.s_dict4sort.ContainsKey(key))
			{
				StorageComponent.s_dict4sort[key] = new IDCNTINC(this.grids[i].itemId, 0, 0);
			}
			IDCNTINC idcntinc = StorageComponent.s_dict4sort[key];
			idcntinc.count += this.grids[i].count;
			idcntinc.inc += this.grids[i].inc;
			StorageComponent.s_dict4sort[key] = idcntinc;
			this.grids[i].itemId = this.grids[i].filter;
			this.grids[i].count = 0;
			this.grids[i].inc = 0;
		}
		if (this.type == EStorageType.Default)
		{
			this.Clear();
			int num = 0;
			using (SortedDictionary<int, IDCNTINC>.Enumerator enumerator = StorageComponent.s_dict4sort.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, IDCNTINC> keyValuePair = enumerator.Current;
					int num2 = keyValuePair.Value.count;
					int inc = keyValuePair.Value.inc;
					int num3 = StorageComponent.itemStackCount[keyValuePair.Value.id];
					while (num2 > 0 && num < this.grids.Length)
					{
						this.grids[num].itemId = keyValuePair.Value.id;
						this.grids[num].stackSize = num3;
						if (num2 <= num3)
						{
							this.grids[num].count = num2;
							this.grids[num].inc = inc;
							num2 = 0;
							inc = 0;
						}
						else
						{
							this.grids[num].inc = this.split_inc(ref num2, ref inc, num3);
							this.grids[num].count = num3;
						}
						num++;
					}
					if (num >= this.grids.Length)
					{
						break;
					}
				}
				goto IL_324;
			}
		}
		if (this.type == EStorageType.Filtered)
		{
			foreach (KeyValuePair<int, IDCNTINC> keyValuePair2 in StorageComponent.s_dict4sort)
			{
				int itemId = keyValuePair2.Value.id;
				int count = keyValuePair2.Value.count;
				int inc2 = keyValuePair2.Value.inc;
				int number;
				int num4 = this.AddItemForSort(itemId, count, inc2, out number);
				Assert.True(count == num4);
				Assert.Zero(number);
			}
		}
		IL_324:
		StorageComponent.s_dict4sort.Clear();
		if (raiseNotify)
		{
			this.ResetOptimizationFlags();
			this.NotifyStorageChange();
		}
	}

	// Token: 0x06000AC1 RID: 2753 RVA: 0x000A1248 File Offset: 0x0009F448
	public void Clear()
	{
		this.searchStart = -1;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
		Array.Clear(this.grids, 0, this.grids.Length);
	}

	// Token: 0x06000AC2 RID: 2754 RVA: 0x000A1274 File Offset: 0x0009F474
	public void SetEmpty()
	{
		this.id = 0;
		this.entityId = 0;
		this.previous = 0;
		this.next = 0;
		this.bottom = 0;
		this.top = 0;
		this.previousStorage = null;
		this.nextStorage = null;
		this.bottomStorage = null;
		this.topStorage = null;
		this.type = EStorageType.Default;
		this.bans = 0;
		Array.Clear(this.grids, 0, this.grids.Length);
		this.searchStart = 0;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
	}

	// Token: 0x06000AC3 RID: 2755 RVA: 0x000A1300 File Offset: 0x0009F500
	public void Free()
	{
		this.id = 0;
		this.entityId = 0;
		this.grids = null;
		this.size = 0;
		this.searchStart = 0;
		this.lastFullItem = -1;
		this.lastEmptyItem = -1;
		this.previous = 0;
		this.next = 0;
		this.bottom = 0;
		this.top = 0;
		this.previousStorage = null;
		this.nextStorage = null;
		this.bottomStorage = null;
		this.topStorage = null;
		this.type = EStorageType.Default;
		this.bans = 0;
	}

	// Token: 0x06000AC4 RID: 2756 RVA: 0x000A1384 File Offset: 0x0009F584
	public int GetItemCount(int itemId)
	{
		int num = 0;
		for (int i = 0; i < this.size; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				num += this.grids[i].count;
			}
		}
		return num;
	}

	// Token: 0x06000AC5 RID: 2757 RVA: 0x000A13D0 File Offset: 0x0009F5D0
	public int GetItemCount(int itemId, out int inc)
	{
		int num = 0;
		inc = 0;
		for (int i = 0; i < this.size; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				num += this.grids[i].count;
				inc += this.grids[i].inc;
			}
		}
		return num;
	}

	// Token: 0x06000AC6 RID: 2758 RVA: 0x000A1434 File Offset: 0x0009F634
	public int GetNotBannedItemCount(int itemId)
	{
		int num = 0;
		int num2 = this.size - this.bans;
		for (int i = 0; i < num2; i++)
		{
			if (this.grids[i].itemId == itemId)
			{
				num += this.grids[i].count;
			}
		}
		return num;
	}

	// Token: 0x06000AC7 RID: 2759 RVA: 0x000A1488 File Offset: 0x0009F688
	public void SetSkimSign(SignData[] signPool)
	{
		if (this.searchStart >= 0)
		{
			int num = (this.grids[this.searchStart].count > 0) ? this.grids[this.searchStart].itemId : 0;
			if (num == 0)
			{
				for (int i = this.searchStart + 1; i < this.size; i++)
				{
					num = ((this.grids[i].count > 0) ? this.grids[i].itemId : 0);
					if (num > 0)
					{
						this.searchStart = i;
						break;
					}
				}
				if (num == 0)
				{
					this.searchStart = -1;
				}
			}
			signPool[this.entityId].iconId0 = (uint)num;
			signPool[this.entityId].iconType = ((num == 0) ? 0U : 1U);
			return;
		}
		signPool[this.entityId].iconId0 = 0U;
		signPool[this.entityId].iconType = 0U;
	}

	// Token: 0x1700017A RID: 378
	// (get) Token: 0x06000AC8 RID: 2760 RVA: 0x000A1580 File Offset: 0x0009F780
	public bool isEmpty
	{
		get
		{
			for (int i = 0; i < this.size; i++)
			{
				if (this.grids[i].count != 0)
				{
					return false;
				}
			}
			return true;
		}
	}

	// Token: 0x1700017B RID: 379
	// (get) Token: 0x06000AC9 RID: 2761 RVA: 0x000A15B4 File Offset: 0x0009F7B4
	public bool isFull
	{
		get
		{
			for (int i = 0; i < this.size; i++)
			{
				if (this.grids[i].itemId == 0 || this.grids[i].count < this.grids[i].stackSize)
				{
					return false;
				}
			}
			return true;
		}
	}

	// Token: 0x06000ACA RID: 2762 RVA: 0x000A160C File Offset: 0x0009F80C
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

	// Token: 0x06000ACB RID: 2763 RVA: 0x000A1658 File Offset: 0x0009F858
	private int split_inc(ref byte n, ref byte m, byte p)
	{
		if (n <= 0)
		{
			n = (m = 0);
			return 0;
		}
		int num = (int)(m / n);
		int num2 = (int)m - num * (int)n;
		n -= p;
		num2 -= (int)n;
		num = ((num2 > 0) ? (num * (int)p + num2) : (num * (int)p));
		m -= (byte)num;
		return num;
	}

	// Token: 0x04000CB4 RID: 3252
	public int id;

	// Token: 0x04000CB5 RID: 3253
	public int entityId;

	// Token: 0x04000CB6 RID: 3254
	public int previous;

	// Token: 0x04000CB7 RID: 3255
	public int next;

	// Token: 0x04000CB8 RID: 3256
	public int bottom;

	// Token: 0x04000CB9 RID: 3257
	public int top;

	// Token: 0x04000CBA RID: 3258
	public StorageComponent previousStorage;

	// Token: 0x04000CBB RID: 3259
	public StorageComponent nextStorage;

	// Token: 0x04000CBC RID: 3260
	public StorageComponent bottomStorage;

	// Token: 0x04000CBD RID: 3261
	public StorageComponent topStorage;

	// Token: 0x04000CBE RID: 3262
	public EStorageType type;

	// Token: 0x04000CBF RID: 3263
	public bool isPlayerInventory;

	// Token: 0x04000CC0 RID: 3264
	public int size;

	// Token: 0x04000CC1 RID: 3265
	public int bans;

	// Token: 0x04000CC2 RID: 3266
	public StorageComponent.GRID[] grids;

	// Token: 0x04000CC3 RID: 3267
	public int searchStart;

	// Token: 0x04000CC4 RID: 3268
	public int lastFullItem = -1;

	// Token: 0x04000CC5 RID: 3269
	public int lastEmptyItem = -1;

	// Token: 0x04000CC6 RID: 3270
	public bool changed;

	// Token: 0x04000CC7 RID: 3271
	public Mutex stg_mx = new Mutex();

	// Token: 0x04000CC8 RID: 3272
	private const int kMaxItemId = 12000;

	// Token: 0x04000CC9 RID: 3273
	public static bool staticLoaded;

	// Token: 0x04000CCA RID: 3274
	public static bool[] itemIsFuel;

	// Token: 0x04000CCB RID: 3275
	public static bool[] itemIsAmmo;

	// Token: 0x04000CCC RID: 3276
	public static bool[] itemIsBomb;

	// Token: 0x04000CCD RID: 3277
	public static bool[] itemIsFighter;

	// Token: 0x04000CCE RID: 3278
	public static int[] itemStackCount;

	// Token: 0x04000CD1 RID: 3281
	private static SortedDictionary<int, IDCNTINC> s_dict4sort;

	// Token: 0x02000C01 RID: 3073
	public struct GRID
	{
		// Token: 0x04007C1D RID: 31773
		public int itemId;

		// Token: 0x04007C1E RID: 31774
		public int filter;

		// Token: 0x04007C1F RID: 31775
		public int count;

		// Token: 0x04007C20 RID: 31776
		public int stackSize;

		// Token: 0x04007C21 RID: 31777
		public int inc;
	}
}
