using System;
using System.IO;
using UnityEngine;

// Token: 0x02000215 RID: 533
public struct EntityData
{
	// Token: 0x06001618 RID: 5656 RVA: 0x001800AC File Offset: 0x0017E2AC
	public void SetNull()
	{
		this.id = 0;
		this.protoId = 0;
		this.modelIndex = 0;
		this.pos.x = 0f;
		this.pos.y = 0f;
		this.pos.z = 0f;
		this.rot.x = 0f;
		this.rot.y = 0f;
		this.rot.z = 0f;
		this.rot.w = 1f;
		this.tilt = 0f;
		this.alt = 0f;
		this.beltId = 0;
		this.splitterId = 0;
		this.monitorId = 0;
		this.storageId = 0;
		this.tankId = 0;
		this.minerId = 0;
		this.inserterId = 0;
		this.assemblerId = 0;
		this.fractionatorId = 0;
		this.ejectorId = 0;
		this.siloId = 0;
		this.labId = 0;
		this.stationId = 0;
		this.dispenserId = 0;
		this.turretId = 0;
		this.beaconId = 0;
		this.fieldGenId = 0;
		this.battleBaseId = 0;
		this.constructionModuleId = 0;
		this.combatModuleId = 0;
		this.powerNodeId = 0;
		this.powerGenId = 0;
		this.powerConId = 0;
		this.powerAccId = 0;
		this.powerExcId = 0;
		this.speakerId = 0;
		this.warningId = 0;
		this.combatStatId = 0;
		this.constructStatId = 0;
		this.spraycoaterId = 0;
		this.pilerId = 0;
		this.extraInfoId = 0;
		this.markerId = 0;
		this.simpleHash.SetEmpty();
		this.hashAddress = 0;
		this.modelId = 0;
		this.mmblockId = 0;
		this.colliderId = 0;
		this.audioId = 0;
	}

	// Token: 0x06001619 RID: 5657 RVA: 0x0018026C File Offset: 0x0017E46C
	private bool WriteCId(BinaryWriter w, int cid, ref int sum)
	{
		sum -= cid;
		if (cid > 0)
		{
			w.Write(1);
			w.Write((byte)(cid & 255));
			w.Write((byte)(cid >> 8 & 255));
			w.Write((byte)(cid >> 16 & 255));
		}
		else
		{
			w.Write(0);
		}
		return sum > 0;
	}

	// Token: 0x0600161A RID: 5658 RVA: 0x001802C8 File Offset: 0x0017E4C8
	private bool ReadCId(BinaryReader r, ref int cid, ref int sum)
	{
		cid = 0;
		if (r.ReadByte() == 0)
		{
			return true;
		}
		int num = (int)r.ReadByte();
		int num2 = (int)r.ReadByte();
		int num3 = (int)r.ReadByte();
		cid = (num | num2 << 8 | num3 << 16);
		sum -= cid;
		return sum > 0;
	}

	// Token: 0x0600161B RID: 5659 RVA: 0x00180310 File Offset: 0x0017E510
	public void Export(Stream s, BinaryWriter w)
	{
		w.Write(12);
		w.Write(this.id);
		if (this.id <= 0)
		{
			return;
		}
		UnsafeIO.Write<EntityData>(s, ref this, 4, 32);
		w.Write(this.tilt);
		w.Write(this.stateFlags);
		w.Write(this.simpleHash.bits);
		w.Write(this.hashAddress);
		int num = this.beltId + this.powerConId + this.inserterId + this.assemblerId + this.labId + this.powerNodeId + this.powerGenId + this.combatStatId + this.constructStatId + this.fractionatorId + this.storageId + this.tankId + this.splitterId + this.ejectorId + this.minerId + this.siloId + this.stationId + this.dispenserId + this.turretId + this.beaconId + this.fieldGenId + this.battleBaseId + this.constructionModuleId + this.combatModuleId + this.powerAccId + this.powerExcId + this.warningId + this.monitorId + this.speakerId + this.spraycoaterId + this.pilerId + this.extraInfoId + this.markerId;
		w.Write(num);
		if (num <= 0)
		{
			return;
		}
		if (!this.WriteCId(w, this.beltId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.powerConId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.inserterId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.assemblerId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.labId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.powerNodeId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.powerGenId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.combatStatId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.constructStatId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.fractionatorId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.storageId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.tankId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.splitterId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.ejectorId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.minerId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.siloId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.stationId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.dispenserId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.turretId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.beaconId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.fieldGenId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.battleBaseId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.constructionModuleId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.combatModuleId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.powerAccId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.powerExcId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.warningId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.monitorId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.speakerId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.spraycoaterId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.pilerId, ref num))
		{
			return;
		}
		if (!this.WriteCId(w, this.extraInfoId, ref num))
		{
			return;
		}
		this.WriteCId(w, this.markerId, ref num);
	}

	// Token: 0x0600161C RID: 5660 RVA: 0x001806C0 File Offset: 0x0017E8C0
	public void Import(Stream s, BinaryReader r)
	{
		byte b = r.ReadByte();
		if (b == 0)
		{
			this.id = r.ReadInt32();
			this.protoId = r.ReadInt16();
			this.modelIndex = r.ReadInt16();
			this.pos.x = r.ReadSingle();
			this.pos.y = r.ReadSingle();
			this.pos.z = r.ReadSingle();
			this.rot.x = r.ReadSingle();
			this.rot.y = r.ReadSingle();
			this.rot.z = r.ReadSingle();
			this.rot.w = r.ReadSingle();
			this.alt = (float)Math.Sqrt((double)(this.pos.x * this.pos.x + this.pos.y * this.pos.y + this.pos.z * this.pos.z));
			this.tilt = 0f;
			this.localized = true;
			this.beltId = r.ReadInt32();
			this.splitterId = r.ReadInt32();
			this.storageId = r.ReadInt32();
			this.tankId = r.ReadInt32();
			this.minerId = r.ReadInt32();
			this.inserterId = r.ReadInt32();
			this.assemblerId = r.ReadInt32();
			this.fractionatorId = r.ReadInt32();
			this.ejectorId = r.ReadInt32();
			this.siloId = r.ReadInt32();
			this.labId = r.ReadInt32();
			this.stationId = r.ReadInt32();
			this.powerNodeId = r.ReadInt32();
			this.powerGenId = r.ReadInt32();
			this.powerConId = r.ReadInt32();
			this.powerAccId = r.ReadInt32();
			this.powerExcId = r.ReadInt32();
			r.ReadInt32();
			return;
		}
		this.id = r.ReadInt32();
		if (this.id <= 0)
		{
			return;
		}
		UnsafeIO.Read<EntityData>(s, ref this, 4, 32);
		this.alt = (float)Math.Sqrt((double)(this.pos.x * this.pos.x + this.pos.y * this.pos.y + this.pos.z * this.pos.z));
		if (b >= 10)
		{
			this.tilt = r.ReadSingle();
		}
		else
		{
			this.tilt = 0f;
		}
		if (b >= 11)
		{
			this.stateFlags = r.ReadByte();
		}
		else if (b >= 8)
		{
			this.localized = r.ReadBoolean();
		}
		else
		{
			this.localized = true;
		}
		if (b >= 7)
		{
			this.simpleHash.bits = r.ReadUInt32();
			this.hashAddress = r.ReadInt32();
		}
		int num = r.ReadInt32();
		if (num <= 0)
		{
			return;
		}
		if (!this.ReadCId(r, ref this.beltId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.powerConId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.inserterId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.assemblerId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.labId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.powerNodeId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.powerGenId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.combatStatId, ref num))
		{
			return;
		}
		if (b >= 9 && !this.ReadCId(r, ref this.constructStatId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.fractionatorId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.storageId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.tankId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.splitterId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.ejectorId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.minerId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.siloId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.stationId, ref num))
		{
			return;
		}
		if (b >= 6 && !this.ReadCId(r, ref this.dispenserId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.turretId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.beaconId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.fieldGenId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.battleBaseId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.constructionModuleId, ref num))
		{
			return;
		}
		if (b >= 7 && !this.ReadCId(r, ref this.combatModuleId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.powerAccId, ref num))
		{
			return;
		}
		if (!this.ReadCId(r, ref this.powerExcId, ref num))
		{
			return;
		}
		if (b >= 4 && !this.ReadCId(r, ref this.warningId, ref num))
		{
			return;
		}
		if (b >= 2 && !this.ReadCId(r, ref this.monitorId, ref num))
		{
			return;
		}
		if (b >= 3 && !this.ReadCId(r, ref this.speakerId, ref num))
		{
			return;
		}
		if (b >= 5)
		{
			if (!this.ReadCId(r, ref this.spraycoaterId, ref num))
			{
				return;
			}
			if (!this.ReadCId(r, ref this.pilerId, ref num))
			{
				return;
			}
		}
		if (b >= 7 && !this.ReadCId(r, ref this.extraInfoId, ref num))
		{
			return;
		}
		if (b >= 12)
		{
			this.ReadCId(r, ref this.markerId, ref num);
			return;
		}
	}

	// Token: 0x17000325 RID: 805
	// (get) Token: 0x0600161D RID: 5661 RVA: 0x00180C29 File Offset: 0x0017EE29
	// (set) Token: 0x0600161E RID: 5662 RVA: 0x00180C3A File Offset: 0x0017EE3A
	public bool localized
	{
		get
		{
			return (this.stateFlags & 128) > 0;
		}
		set
		{
			this.stateFlags = (value ? ((this.stateFlags & 127) | 128) : (this.stateFlags & 127));
		}
	}

	// Token: 0x17000326 RID: 806
	// (get) Token: 0x0600161F RID: 5663 RVA: 0x00180C61 File Offset: 0x0017EE61
	// (set) Token: 0x06001620 RID: 5664 RVA: 0x00180C6F File Offset: 0x0017EE6F
	public bool ignoreRepairWarning
	{
		get
		{
			return (this.stateFlags & 64) > 0;
		}
		set
		{
			this.stateFlags = (value ? ((this.stateFlags & 191) | 64) : (this.stateFlags & 191));
		}
	}

	// Token: 0x04001AB0 RID: 6832
	public int id;

	// Token: 0x04001AB1 RID: 6833
	public short protoId;

	// Token: 0x04001AB2 RID: 6834
	public short modelIndex;

	// Token: 0x04001AB3 RID: 6835
	public Vector3 pos;

	// Token: 0x04001AB4 RID: 6836
	public Quaternion rot;

	// Token: 0x04001AB5 RID: 6837
	public float tilt;

	// Token: 0x04001AB6 RID: 6838
	public float alt;

	// Token: 0x04001AB7 RID: 6839
	public byte stateFlags;

	// Token: 0x04001AB8 RID: 6840
	public SimpleHash simpleHash;

	// Token: 0x04001AB9 RID: 6841
	public int hashAddress;

	// Token: 0x04001ABA RID: 6842
	public int beltId;

	// Token: 0x04001ABB RID: 6843
	public int splitterId;

	// Token: 0x04001ABC RID: 6844
	public int monitorId;

	// Token: 0x04001ABD RID: 6845
	public int storageId;

	// Token: 0x04001ABE RID: 6846
	public int tankId;

	// Token: 0x04001ABF RID: 6847
	public int minerId;

	// Token: 0x04001AC0 RID: 6848
	public int inserterId;

	// Token: 0x04001AC1 RID: 6849
	public int assemblerId;

	// Token: 0x04001AC2 RID: 6850
	public int fractionatorId;

	// Token: 0x04001AC3 RID: 6851
	public int ejectorId;

	// Token: 0x04001AC4 RID: 6852
	public int siloId;

	// Token: 0x04001AC5 RID: 6853
	public int labId;

	// Token: 0x04001AC6 RID: 6854
	public int stationId;

	// Token: 0x04001AC7 RID: 6855
	public int dispenserId;

	// Token: 0x04001AC8 RID: 6856
	public int turretId;

	// Token: 0x04001AC9 RID: 6857
	public int beaconId;

	// Token: 0x04001ACA RID: 6858
	public int fieldGenId;

	// Token: 0x04001ACB RID: 6859
	public int battleBaseId;

	// Token: 0x04001ACC RID: 6860
	public int constructionModuleId;

	// Token: 0x04001ACD RID: 6861
	public int combatModuleId;

	// Token: 0x04001ACE RID: 6862
	public int powerNodeId;

	// Token: 0x04001ACF RID: 6863
	public int powerGenId;

	// Token: 0x04001AD0 RID: 6864
	public int powerConId;

	// Token: 0x04001AD1 RID: 6865
	public int powerAccId;

	// Token: 0x04001AD2 RID: 6866
	public int powerExcId;

	// Token: 0x04001AD3 RID: 6867
	public int speakerId;

	// Token: 0x04001AD4 RID: 6868
	public int warningId;

	// Token: 0x04001AD5 RID: 6869
	public int combatStatId;

	// Token: 0x04001AD6 RID: 6870
	public int constructStatId;

	// Token: 0x04001AD7 RID: 6871
	public int spraycoaterId;

	// Token: 0x04001AD8 RID: 6872
	public int pilerId;

	// Token: 0x04001AD9 RID: 6873
	public int extraInfoId;

	// Token: 0x04001ADA RID: 6874
	public int markerId;

	// Token: 0x04001ADB RID: 6875
	public int modelId;

	// Token: 0x04001ADC RID: 6876
	public int mmblockId;

	// Token: 0x04001ADD RID: 6877
	public int colliderId;

	// Token: 0x04001ADE RID: 6878
	public int audioId;
}
