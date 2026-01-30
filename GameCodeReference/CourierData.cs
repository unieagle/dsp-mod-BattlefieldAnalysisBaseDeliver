using System;
using System.IO;
using UnityEngine;

// Token: 0x020000D1 RID: 209
[Serializable]
public struct CourierData
{
	// Token: 0x06000732 RID: 1842 RVA: 0x000412FC File Offset: 0x0003F4FC
	public void Export(BinaryWriter w)
	{
		w.Write(0);
		w.Write(this.begin.x);
		w.Write(this.begin.y);
		w.Write(this.begin.z);
		w.Write(this.end.x);
		w.Write(this.end.y);
		w.Write(this.end.z);
		w.Write(this.endId);
		w.Write(this.direction);
		w.Write(this.maxt);
		w.Write(this.t);
		w.Write(this.itemId);
		w.Write(this.itemCount);
		w.Write(this.inc);
		w.Write(this.gene);
	}

	// Token: 0x06000733 RID: 1843 RVA: 0x000413D8 File Offset: 0x0003F5D8
	public void Import(BinaryReader r)
	{
		r.ReadInt32();
		this.begin.x = r.ReadSingle();
		this.begin.y = r.ReadSingle();
		this.begin.z = r.ReadSingle();
		this.end.x = r.ReadSingle();
		this.end.y = r.ReadSingle();
		this.end.z = r.ReadSingle();
		this.endId = r.ReadInt32();
		this.direction = r.ReadSingle();
		this.maxt = r.ReadSingle();
		this.t = r.ReadSingle();
		this.itemId = r.ReadInt32();
		this.itemCount = r.ReadInt32();
		this.inc = r.ReadInt32();
		this.gene = r.ReadInt32();
	}

	// Token: 0x040005FB RID: 1531
	public Vector3 begin;

	// Token: 0x040005FC RID: 1532
	public Vector3 end;

	// Token: 0x040005FD RID: 1533
	public int endId;

	// Token: 0x040005FE RID: 1534
	public float direction;

	// Token: 0x040005FF RID: 1535
	public float maxt;

	// Token: 0x04000600 RID: 1536
	public float t;

	// Token: 0x04000601 RID: 1537
	public int itemId;

	// Token: 0x04000602 RID: 1538
	public int itemCount;

	// Token: 0x04000603 RID: 1539
	public int inc;

	// Token: 0x04000604 RID: 1540
	public int gene;

	// Token: 0x04000605 RID: 1541
	public const int dataLen = 56;
}
