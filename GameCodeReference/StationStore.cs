using System;
using System.IO;

// Token: 0x02000158 RID: 344
[Serializable]
public struct StationStore
{
	// Token: 0x1700016E RID: 366
	// (get) Token: 0x06000A5D RID: 2653 RVA: 0x0009C448 File Offset: 0x0009A648
	public int totalOrdered
	{
		get
		{
			return this.localOrder + this.remoteOrder;
		}
	}

	// Token: 0x1700016F RID: 367
	// (get) Token: 0x06000A5E RID: 2654 RVA: 0x0009C457 File Offset: 0x0009A657
	public int localSupplyCount
	{
		get
		{
			return this.count + this.localOrder;
		}
	}

	// Token: 0x17000170 RID: 368
	// (get) Token: 0x06000A5F RID: 2655 RVA: 0x0009C466 File Offset: 0x0009A666
	public int localDemandCount
	{
		get
		{
			return this.max - (this.count + this.localOrder);
		}
	}

	// Token: 0x17000171 RID: 369
	// (get) Token: 0x06000A60 RID: 2656 RVA: 0x0009C47C File Offset: 0x0009A67C
	public int remoteSupplyCount
	{
		get
		{
			return this.count + this.remoteOrder;
		}
	}

	// Token: 0x17000172 RID: 370
	// (get) Token: 0x06000A61 RID: 2657 RVA: 0x0009C48B File Offset: 0x0009A68B
	public int remoteDemandCount
	{
		get
		{
			return this.max - (this.count + this.remoteOrder);
		}
	}

	// Token: 0x17000173 RID: 371
	// (get) Token: 0x06000A62 RID: 2658 RVA: 0x0009C4A1 File Offset: 0x0009A6A1
	public int totalSupplyCount
	{
		get
		{
			return this.count + this.localOrder + this.remoteOrder;
		}
	}

	// Token: 0x17000174 RID: 372
	// (get) Token: 0x06000A63 RID: 2659 RVA: 0x0009C4B7 File Offset: 0x0009A6B7
	public int totalDemandCount
	{
		get
		{
			return this.max - (this.count + this.localOrder + this.remoteOrder);
		}
	}

	// Token: 0x06000A64 RID: 2660 RVA: 0x0009C4D4 File Offset: 0x0009A6D4
	public StationStore(int _id, int _count, int _inc, int _localOrder, int _remoteOrder, int _max, ELogisticStorage _localLogic, ELogisticStorage _remoteLogic)
	{
		this.itemId = _id;
		this.count = _count;
		this.inc = _inc;
		this.localOrder = _localOrder;
		this.remoteOrder = _remoteOrder;
		this.max = _max;
		this.keepMode = 0;
		this.keepIncRatio = 0f;
		this.localLogic = _localLogic;
		this.remoteLogic = _remoteLogic;
	}

	// Token: 0x06000A65 RID: 2661 RVA: 0x0009C530 File Offset: 0x0009A730
	public void Export(BinaryWriter w)
	{
		w.Write(2);
		w.Write(this.itemId);
		w.Write(this.count);
		w.Write(this.inc);
		w.Write(this.localOrder);
		w.Write(this.remoteOrder);
		w.Write(this.max + ((this.keepMode > 0) ? (this.keepMode * 100000000) : 0));
		w.Write(this.keepIncRatio);
		w.Write((int)this.localLogic);
		w.Write((int)this.remoteLogic);
	}

	// Token: 0x06000A66 RID: 2662 RVA: 0x0009C5CC File Offset: 0x0009A7CC
	public void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.itemId = r.ReadInt32();
		this.count = r.ReadInt32();
		if (num >= 1)
		{
			this.inc = r.ReadInt32();
		}
		else
		{
			this.inc = 0;
		}
		this.localOrder = r.ReadInt32();
		this.remoteOrder = r.ReadInt32();
		this.max = r.ReadInt32();
		if (this.max < 0)
		{
			this.max = 0;
		}
		else if (this.max >= 100000000)
		{
			this.keepMode = this.max / 100000000;
			this.max %= 100000000;
		}
		this.keepIncRatio = ((num >= 2) ? r.ReadSingle() : 0f);
		this.localLogic = (ELogisticStorage)r.ReadInt32();
		this.remoteLogic = (ELogisticStorage)r.ReadInt32();
	}

	// Token: 0x04000C4B RID: 3147
	public int itemId;

	// Token: 0x04000C4C RID: 3148
	public int count;

	// Token: 0x04000C4D RID: 3149
	public int inc;

	// Token: 0x04000C4E RID: 3150
	public int localOrder;

	// Token: 0x04000C4F RID: 3151
	public int remoteOrder;

	// Token: 0x04000C50 RID: 3152
	public int max;

	// Token: 0x04000C51 RID: 3153
	public int keepMode;

	// Token: 0x04000C52 RID: 3154
	public float keepIncRatio;

	// Token: 0x04000C53 RID: 3155
	public ELogisticStorage localLogic;

	// Token: 0x04000C54 RID: 3156
	public ELogisticStorage remoteLogic;
}
