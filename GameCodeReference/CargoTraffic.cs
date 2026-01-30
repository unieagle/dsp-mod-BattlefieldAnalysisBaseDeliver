using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

// Token: 0x02000166 RID: 358
public class CargoTraffic
{
	// Token: 0x06000ADF RID: 2783 RVA: 0x000A2434 File Offset: 0x000A0634
	public CargoTraffic()
	{
	}

	// Token: 0x06000AE0 RID: 2784 RVA: 0x000A24F4 File Offset: 0x000A06F4
	public CargoTraffic(PlanetData _planet)
	{
		this.planet = _planet;
		this.factory = this.planet.factory;
		this.container = this.factory.cargoContainer;
		this.SetPathCapacity(16);
		this.SetBeltCapacity(16);
		this.SetSplitterCapacity(16);
		this.SetMonitorCapacity(16);
		this.SetSpraycoaterCapacity(16);
		this.SetPilerCapacity(16);
	}

	// Token: 0x1700017D RID: 381
	// (get) Token: 0x06000AE1 RID: 2785 RVA: 0x000A260D File Offset: 0x000A080D
	public int splitterCount
	{
		get
		{
			return this.splitterCursor - this.splitterRecycleCursor - 1;
		}
	}

	// Token: 0x1700017E RID: 382
	// (get) Token: 0x06000AE2 RID: 2786 RVA: 0x000A261E File Offset: 0x000A081E
	public int monitorCount
	{
		get
		{
			return this.monitorCursor - this.monitorRecycleCursor - 1;
		}
	}

	// Token: 0x1700017F RID: 383
	// (get) Token: 0x06000AE3 RID: 2787 RVA: 0x000A262F File Offset: 0x000A082F
	public int spraycoaterCount
	{
		get
		{
			return this.spraycoaterCursor - this.spraycoaterRecycleCursor - 1;
		}
	}

	// Token: 0x17000180 RID: 384
	// (get) Token: 0x06000AE4 RID: 2788 RVA: 0x000A2640 File Offset: 0x000A0840
	public int pilerCount
	{
		get
		{
			return this.pilerCursor - this.pilerRecycleCursor - 1;
		}
	}

	// Token: 0x17000181 RID: 385
	// (get) Token: 0x06000AE5 RID: 2789 RVA: 0x000A2651 File Offset: 0x000A0851
	public bool isLocal
	{
		get
		{
			return this.planet.factoryLoading || this.planet.factoryLoaded;
		}
	}

	// Token: 0x17000182 RID: 386
	// (get) Token: 0x06000AE6 RID: 2790 RVA: 0x000A266D File Offset: 0x000A086D
	public bool isLocalLoaded
	{
		get
		{
			return this.planet.factoryLoaded;
		}
	}

	// Token: 0x06000AE7 RID: 2791 RVA: 0x000A267C File Offset: 0x000A087C
	public void Free()
	{
		for (int i = 0; i < this.pathPool.Length; i++)
		{
			if (this.pathPool[i] != null)
			{
				this.pathPool[i].Free();
				this.pathPool[i] = null;
			}
		}
		this.pathPool = null;
		this.pathRecycle = null;
		this.beltPool = null;
		this.beltRecycle = null;
		this.splitterPool = null;
		this.splitterRecycle = null;
		this.monitorPool = null;
		this.monitorRecycle = null;
		this.spraycoaterPool = null;
		this.spraycoaterRecycle = null;
		this.pilerPool = null;
		this.pilerRecycle = null;
	}

	// Token: 0x06000AE8 RID: 2792 RVA: 0x000A2710 File Offset: 0x000A0910
	public unsafe void Export(BinaryWriter w)
	{
		w.Write(4);
		w.Write(this.beltCursor);
		w.Write(this.beltCapacity);
		w.Write(this.beltRecycleCursor);
		w.Write(this.splitterCursor);
		w.Write(this.splitterCapacity);
		w.Write(this.splitterRecycleCursor);
		w.Write(this.pathCursor);
		w.Write(this.pathCapacity);
		w.Write(this.pathRecycleCursor);
		Stream baseStream = w.BaseStream;
		byte[] array;
		byte* ptr;
		if ((array = UnsafeIO.buf) == null || array.Length == 0)
		{
			ptr = null;
		}
		else
		{
			ptr = &array[0];
		}
		BeltComponent[] array2;
		BeltComponent* ptr2;
		if ((array2 = this.beltPool) == null || array2.Length == 0)
		{
			ptr2 = null;
		}
		else
		{
			ptr2 = &array2[0];
		}
		for (int i = 1; i < this.beltCursor; i += 16384)
		{
			int num = i + 16384;
			if (num > this.beltCursor)
			{
				num = this.beltCursor;
			}
			int num2 = 0;
			for (int j = i; j < num; j++)
			{
				UnsafeUtility.MemCpy((void*)(ptr + num2), (void*)(ptr2 + j), 44L);
				num2 += 44;
			}
			baseStream.Write(UnsafeIO.buf, 0, num2);
		}
		array2 = null;
		array = null;
		UnsafeIO.WriteInt32Array(w.BaseStream, this.beltRecycle, this.beltRecycleCursor);
		for (int k = 1; k < this.splitterCursor; k++)
		{
			this.splitterPool[k].Export(w);
		}
		for (int l = 0; l < this.splitterRecycleCursor; l++)
		{
			w.Write(this.splitterRecycle[l]);
		}
		for (int m = 1; m < this.pathCursor; m++)
		{
			if (this.pathPool[m] != null && this.pathPool[m].id == m)
			{
				w.Write(m);
				this.pathPool[m].Export(w);
			}
			else
			{
				w.Write(0);
			}
		}
		for (int n = 0; n < this.pathRecycleCursor; n++)
		{
			w.Write(this.pathRecycle[n]);
		}
		w.Write(this.monitorCursor);
		w.Write(this.monitorCapacity);
		w.Write(this.monitorRecycleCursor);
		for (int num3 = 1; num3 < this.monitorCursor; num3++)
		{
			this.monitorPool[num3].Export(w);
		}
		for (int num4 = 0; num4 < this.monitorRecycleCursor; num4++)
		{
			w.Write(this.monitorRecycle[num4]);
		}
		w.Write(this.spraycoaterCursor);
		w.Write(this.spraycoaterCapacity);
		w.Write(this.spraycoaterRecycleCursor);
		for (int num5 = 1; num5 < this.spraycoaterCursor; num5++)
		{
			this.spraycoaterPool[num5].Export(w);
		}
		for (int num6 = 0; num6 < this.spraycoaterRecycleCursor; num6++)
		{
			w.Write(this.spraycoaterRecycle[num6]);
		}
		w.Write(this.pilerCursor);
		w.Write(this.pilerCapacity);
		w.Write(this.pilerRecycleCursor);
		for (int num7 = 1; num7 < this.pilerCursor; num7++)
		{
			this.pilerPool[num7].Export(w);
		}
		for (int num8 = 0; num8 < this.pilerRecycleCursor; num8++)
		{
			w.Write(this.pilerRecycle[num8]);
		}
	}

	// Token: 0x06000AE9 RID: 2793 RVA: 0x000A2A78 File Offset: 0x000A0C78
	public unsafe void Import(BinaryReader r)
	{
		int num = r.ReadInt32();
		this.beltCursor = r.ReadInt32();
		this.SetBeltCapacity(r.ReadInt32());
		this.beltRecycleCursor = r.ReadInt32();
		this.splitterCursor = r.ReadInt32();
		this.SetSplitterCapacity(r.ReadInt32());
		this.splitterRecycleCursor = r.ReadInt32();
		this.pathCursor = r.ReadInt32();
		this.SetPathCapacity(r.ReadInt32());
		this.pathRecycleCursor = r.ReadInt32();
		if (num >= 4)
		{
			Stream baseStream = r.BaseStream;
			byte[] array;
			byte* ptr;
			if ((array = UnsafeIO.buf) == null || array.Length == 0)
			{
				ptr = null;
			}
			else
			{
				ptr = &array[0];
			}
			BeltComponent[] array2;
			BeltComponent* ptr2;
			if ((array2 = this.beltPool) == null || array2.Length == 0)
			{
				ptr2 = null;
			}
			else
			{
				ptr2 = &array2[0];
			}
			for (int i = 1; i < this.beltCursor; i += 16384)
			{
				int num2 = i + 16384;
				if (num2 > this.beltCursor)
				{
					num2 = this.beltCursor;
				}
				int num3 = num2 - i;
				baseStream.Read(UnsafeIO.buf, 0, num3 * 44);
				int num4 = 0;
				for (int j = i; j < num2; j++)
				{
					UnsafeUtility.MemCpy((void*)(ptr2 + j), (void*)(ptr + num4), 44L);
					num4 += 44;
				}
			}
			array2 = null;
			array = null;
			UnsafeIO.ReadInt32Array(r.BaseStream, this.beltRecycle, this.beltRecycleCursor);
		}
		else
		{
			for (int k = 1; k < this.beltCursor; k++)
			{
				this.beltPool[k].Import(r);
			}
			for (int l = 0; l < this.beltRecycleCursor; l++)
			{
				this.beltRecycle[l] = r.ReadInt32();
			}
		}
		for (int m = 1; m < this.splitterCursor; m++)
		{
			this.splitterPool[m].Import(r);
		}
		for (int n = 0; n < this.splitterRecycleCursor; n++)
		{
			this.splitterRecycle[n] = r.ReadInt32();
		}
		for (int num5 = 1; num5 < this.pathCursor; num5++)
		{
			int num6 = r.ReadInt32();
			if (num6 != 0)
			{
				Assert.True(num6 == num5);
				this.pathPool[num5] = new CargoPath(this.container);
				this.pathPool[num5].Import(r);
			}
		}
		for (int num7 = 0; num7 < this.pathRecycleCursor; num7++)
		{
			this.pathRecycle[num7] = r.ReadInt32();
		}
		for (int num8 = 1; num8 < this.pathCursor; num8++)
		{
			if (this.pathPool[num8] != null && this.pathPool[num8].outputPathIdForImport > 0)
			{
				this.pathPool[num8].outputPath = this.pathPool[this.pathPool[num8].outputPathIdForImport];
			}
		}
		if (num >= 1)
		{
			this.monitorCursor = r.ReadInt32();
			this.SetMonitorCapacity(r.ReadInt32());
			this.monitorRecycleCursor = r.ReadInt32();
			for (int num9 = 1; num9 < this.monitorCursor; num9++)
			{
				this.monitorPool[num9].Import(r);
			}
			for (int num10 = 0; num10 < this.monitorRecycleCursor; num10++)
			{
				this.monitorRecycle[num10] = r.ReadInt32();
			}
		}
		else
		{
			this.SetMonitorCapacity(16);
		}
		if (num >= 2)
		{
			this.spraycoaterCursor = r.ReadInt32();
			this.SetSpraycoaterCapacity(r.ReadInt32());
			this.spraycoaterRecycleCursor = r.ReadInt32();
			for (int num11 = 1; num11 < this.spraycoaterCursor; num11++)
			{
				this.spraycoaterPool[num11].Import(r);
			}
			for (int num12 = 0; num12 < this.spraycoaterRecycleCursor; num12++)
			{
				this.spraycoaterRecycle[num12] = r.ReadInt32();
			}
		}
		else
		{
			this.SetSpraycoaterCapacity(16);
		}
		if (num >= 3)
		{
			this.pilerCursor = r.ReadInt32();
			this.SetPilerCapacity(r.ReadInt32());
			this.pilerRecycleCursor = r.ReadInt32();
			for (int num13 = 1; num13 < this.pilerCursor; num13++)
			{
				this.pilerPool[num13].Import(r);
			}
			for (int num14 = 0; num14 < this.pilerRecycleCursor; num14++)
			{
				this.pilerRecycle[num14] = r.ReadInt32();
			}
			return;
		}
		this.SetPilerCapacity(16);
	}

	// Token: 0x06000AEA RID: 2794 RVA: 0x000A2EC0 File Offset: 0x000A10C0
	private void SetPathCapacity(int newCapacity)
	{
		CargoPath[] array = this.pathPool;
		this.pathPool = new CargoPath[newCapacity];
		this.pathRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.pathPool, (newCapacity > this.pathCapacity) ? this.pathCapacity : newCapacity);
		}
		this.pathCapacity = newCapacity;
	}

	// Token: 0x06000AEB RID: 2795 RVA: 0x000A2F14 File Offset: 0x000A1114
	public CargoPath NewCargoPath()
	{
		int num;
		if (this.pathRecycleCursor > 0)
		{
			int[] array = this.pathRecycle;
			num = this.pathRecycleCursor - 1;
			this.pathRecycleCursor = num;
			int num2 = array[num];
			CargoPath cargoPath = this.pathPool[num2];
			if (cargoPath == null)
			{
				cargoPath = new CargoPath(this.container);
				this.pathPool[num2] = cargoPath;
			}
			cargoPath.id = num2;
			return cargoPath;
		}
		num = this.pathCursor;
		this.pathCursor = num + 1;
		int num3 = num;
		if (num3 == this.pathCapacity)
		{
			this.SetPathCapacity(this.pathCapacity * 2);
		}
		CargoPath cargoPath2 = this.pathPool[num3];
		if (cargoPath2 == null)
		{
			cargoPath2 = new CargoPath(this.container);
			this.pathPool[num3] = cargoPath2;
		}
		cargoPath2.id = num3;
		return cargoPath2;
	}

	// Token: 0x06000AEC RID: 2796 RVA: 0x000A2FC8 File Offset: 0x000A11C8
	public void RemoveCargoPath(int id)
	{
		if (this.pathPool[id] != null && this.pathPool[id].id != 0)
		{
			this.pathPool[id].RemoveCargosInSegment(0, 0);
			this.RemovePathRenderer(id);
			this.pathPool[id].id = 0;
			this.pathPool[id].Clear();
			int[] array = this.pathRecycle;
			int num = this.pathRecycleCursor;
			this.pathRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x06000AED RID: 2797 RVA: 0x000A3038 File Offset: 0x000A1238
	public CargoPath GetCargoPath(int id)
	{
		if (id <= 0 && id >= this.pathCursor)
		{
			return null;
		}
		CargoPath cargoPath = this.pathPool[id];
		if (cargoPath != null && cargoPath.id == id)
		{
			return cargoPath;
		}
		return null;
	}

	// Token: 0x06000AEE RID: 2798 RVA: 0x000A306C File Offset: 0x000A126C
	private void SetBeltCapacity(int newCapacity)
	{
		BeltComponent[] array = this.beltPool;
		this.beltPool = new BeltComponent[newCapacity];
		this.beltRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.beltPool, (newCapacity > this.beltCapacity) ? this.beltCapacity : newCapacity);
		}
		this.beltCapacity = newCapacity;
	}

	// Token: 0x06000AEF RID: 2799 RVA: 0x000A30C0 File Offset: 0x000A12C0
	public int NewBeltComponent(int entityId, int speed)
	{
		if (speed < 1)
		{
			speed = 1;
		}
		int num;
		if (this.beltRecycleCursor > 0)
		{
			int[] array = this.beltRecycle;
			num = this.beltRecycleCursor - 1;
			this.beltRecycleCursor = num;
			int num2 = array[num];
			this.beltPool[num2].id = num2;
			this.beltPool[num2].entityId = entityId;
			this.beltPool[num2].speed = speed;
			Assert.True(this.factory.entityPool[entityId].id == entityId);
			this.factory.entityPool[entityId].beltId = num2;
			return num2;
		}
		num = this.beltCursor;
		this.beltCursor = num + 1;
		int num3 = num;
		if (num3 == this.beltCapacity)
		{
			this.SetBeltCapacity(this.beltCapacity * 2);
		}
		this.beltPool[num3].id = num3;
		this.beltPool[num3].entityId = entityId;
		this.beltPool[num3].speed = speed;
		Assert.True(this.factory.entityPool[entityId].id == entityId);
		this.factory.entityPool[entityId].beltId = num3;
		return num3;
	}

	// Token: 0x06000AF0 RID: 2800 RVA: 0x000A31FC File Offset: 0x000A13FC
	public void UpgradeBeltComponent(int beltId, int speed)
	{
		this.beltPool[beltId].speed = speed;
		this.AlterBeltRenderer(beltId, this.factory.entityPool, (this.planet.physics == null) ? null : this.planet.physics.colChunks, false);
		this.planet.physics.isPlanetPhysicsColliderDirty = true;
		CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
		bool flag = this.beltPool[beltId].segIndex < 10;
		bool flag2 = this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength > cargoPath.pathLength - 10;
		bool flag3 = !cargoPath.closed && (flag || (flag2 && cargoPath.outputPath == null));
		if (cargoPath.belts[cargoPath.belts.Count - 1] == beltId)
		{
			cargoPath.InsertChunk(this.beltPool[beltId].segIndex, cargoPath.pathLength - this.beltPool[beltId].segIndex, speed);
		}
		else
		{
			cargoPath.InsertChunk(this.beltPool[beltId].segIndex, this.beltPool[beltId].segLength, speed);
		}
		if (cargoPath.closed && this.beltPool[beltId].segIndex == 0)
		{
			cargoPath.SyncBuckleSpeed();
		}
		this.RefreshBeltBatchesBuffers();
		if (flag3)
		{
			this.AlterPathRenderer(this.beltPool[beltId].segPathId, false);
			this.RefreshPathBatchesBuffers();
		}
	}

	// Token: 0x06000AF1 RID: 2801 RVA: 0x000A339C File Offset: 0x000A159C
	public void RemoveBeltComponent(int id)
	{
		if (this.beltPool[id].id != 0)
		{
			this.AlterBeltConnections(id, 0, 0, 0, 0, false);
			this.RemoveBeltRenderer(id);
			this.RemoveCargoPath(this.beltPool[id].segPathId);
			this.beltPool[id].SetEmpty();
			int[] array = this.beltRecycle;
			int num = this.beltRecycleCursor;
			this.beltRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x06000AF2 RID: 2802 RVA: 0x000A3410 File Offset: 0x000A1610
	public void PickupBeltItems(Player player, int beltId, bool all)
	{
		if (this.beltPool[beltId].id != 0 && this.beltPool[beltId].id == beltId)
		{
			int num = all ? this.beltPool[beltId].segIndex : (this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset);
			int num2 = all ? (this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength - 1) : (this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset);
			CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
			for (int i = num; i <= num2; i++)
			{
				byte count;
				byte inc;
				int itemId = cargoPath.TryPickItem(i - 4 - 1, 12, out count, out inc);
				int num3 = player.TryAddItemToPackage(itemId, (int)count, (int)inc, true, this.beltPool[beltId].entityId, false);
				if (num3 > 0)
				{
					UIItemup.Up(itemId, num3);
				}
			}
		}
	}

	// Token: 0x06000AF3 RID: 2803 RVA: 0x000A3538 File Offset: 0x000A1738
	public void RemoveBeltItems(int beltId, bool all)
	{
		if (this.beltPool[beltId].id != 0 && this.beltPool[beltId].id == beltId)
		{
			int num = all ? this.beltPool[beltId].segIndex : (this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset);
			int num2 = all ? (this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength - 1) : (this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset);
			CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
			for (int i = num; i <= num2; i++)
			{
				byte b;
				byte b2;
				cargoPath.TryPickItem(i - 4 - 1, 12, out b, out b2);
			}
		}
	}

	// Token: 0x06000AF4 RID: 2804 RVA: 0x000A3630 File Offset: 0x000A1830
	public bool PutItemOnBelt(int beltId, int itemId, byte itemInc)
	{
		if (this.beltPool[beltId].id != 0 && this.beltPool[beltId].id == beltId)
		{
			int index = this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset;
			return this.GetCargoPath(this.beltPool[beltId].segPathId).TryInsertItem(index, itemId, 1, itemInc);
		}
		return false;
	}

	// Token: 0x06000AF5 RID: 2805 RVA: 0x000A36AC File Offset: 0x000A18AC
	private void SetSplitterCapacity(int newCapacity)
	{
		SplitterComponent[] array = this.splitterPool;
		this.splitterPool = new SplitterComponent[newCapacity];
		this.splitterRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.splitterPool, (newCapacity > this.splitterCapacity) ? this.splitterCapacity : newCapacity);
		}
		this.splitterCapacity = newCapacity;
	}

	// Token: 0x06000AF6 RID: 2806 RVA: 0x000A3700 File Offset: 0x000A1900
	public int NewSplitterComponent(int entityId)
	{
		int num;
		if (this.splitterRecycleCursor > 0)
		{
			int[] array = this.splitterRecycle;
			num = this.splitterRecycleCursor - 1;
			this.splitterRecycleCursor = num;
			int num2 = array[num];
			this.splitterPool[num2].id = num2;
			this.splitterPool[num2].entityId = entityId;
			Assert.True(this.factory.entityPool[entityId].id == entityId);
			this.factory.entityPool[entityId].splitterId = num2;
			return num2;
		}
		num = this.splitterCursor;
		this.splitterCursor = num + 1;
		int num3 = num;
		if (num3 == this.splitterCapacity)
		{
			this.SetSplitterCapacity(this.splitterCapacity * 2);
		}
		this.splitterPool[num3].id = num3;
		this.splitterPool[num3].entityId = entityId;
		Assert.True(this.factory.entityPool[entityId].id == entityId);
		this.factory.entityPool[entityId].splitterId = num3;
		return num3;
	}

	// Token: 0x06000AF7 RID: 2807 RVA: 0x000A380D File Offset: 0x000A1A0D
	public void ConnectToSplitter(int splitterId, int targetBeltId, int slot, bool connectIn)
	{
		if (splitterId == 0)
		{
			return;
		}
		if (this.splitterPool[splitterId].id != splitterId)
		{
			return;
		}
		if (slot < 0 || slot > 3)
		{
			return;
		}
		this.splitterPool[splitterId].AlterBelt(slot, targetBeltId, connectIn);
	}

	// Token: 0x06000AF8 RID: 2808 RVA: 0x000A3848 File Offset: 0x000A1A48
	public int DisconnectToSplitter(int splitterId, int removingBeltId)
	{
		if (splitterId == 0)
		{
			return -1;
		}
		if (this.splitterPool[splitterId].id != splitterId)
		{
			return -1;
		}
		if (removingBeltId == 0)
		{
			return -1;
		}
		int result = -1;
		if (this.splitterPool[splitterId].beltA == removingBeltId)
		{
			this.splitterPool[splitterId].AlterBelt(0, 0, false);
			result = 0;
		}
		if (this.splitterPool[splitterId].beltB == removingBeltId)
		{
			this.splitterPool[splitterId].AlterBelt(1, 0, false);
			result = 1;
		}
		if (this.splitterPool[splitterId].beltC == removingBeltId)
		{
			this.splitterPool[splitterId].AlterBelt(2, 0, false);
			result = 2;
		}
		if (this.splitterPool[splitterId].beltD == removingBeltId)
		{
			this.splitterPool[splitterId].AlterBelt(3, 0, false);
			result = 3;
		}
		return result;
	}

	// Token: 0x06000AF9 RID: 2809 RVA: 0x000A3920 File Offset: 0x000A1B20
	public int GetBeltConnectedToSplitter(int splitterId, int slot)
	{
		if (splitterId <= 0 || this.splitterPool[splitterId].id != splitterId)
		{
			return 0;
		}
		int slotBelt = this.splitterPool[splitterId].GetSlotBelt(slot);
		if (slotBelt <= 0 || this.beltPool[slotBelt].id != slotBelt)
		{
			return 0;
		}
		int num = 0;
		if (this.beltPool[slotBelt].outputId > 0)
		{
			num = this.beltPool[slotBelt].outputId;
		}
		if (this.beltPool[slotBelt].mainInputId > 0)
		{
			Assert.Zero(num);
			num = this.beltPool[slotBelt].mainInputId;
		}
		if (num <= 0 || this.beltPool[num].id != num)
		{
			return 0;
		}
		return num;
	}

	// Token: 0x06000AFA RID: 2810 RVA: 0x000A39EC File Offset: 0x000A1BEC
	public void RemoveSplitterComponent(int id)
	{
		if (this.splitterPool[id].id != 0)
		{
			this.splitterPool[id].SetEmpty();
			int[] array = this.splitterRecycle;
			int num = this.splitterRecycleCursor;
			this.splitterRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x06000AFB RID: 2811 RVA: 0x000A3A38 File Offset: 0x000A1C38
	private void SetMonitorCapacity(int newCapacity)
	{
		MonitorComponent[] array = this.monitorPool;
		this.monitorPool = new MonitorComponent[newCapacity];
		this.monitorRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.monitorPool, (newCapacity > this.monitorCapacity) ? this.monitorCapacity : newCapacity);
		}
		this.monitorCapacity = newCapacity;
	}

	// Token: 0x06000AFC RID: 2812 RVA: 0x000A3A8C File Offset: 0x000A1C8C
	public int NewMonitorComponent(int entityId)
	{
		if (this.monitorCapacity < 16)
		{
			this.SetMonitorCapacity(16);
		}
		int num;
		if (this.monitorRecycleCursor > 0)
		{
			int[] array = this.monitorRecycle;
			num = this.monitorRecycleCursor - 1;
			this.monitorRecycleCursor = num;
			int num2 = array[num];
			this.monitorPool[num2].SetEmpty();
			this.monitorPool[num2].id = num2;
			this.monitorPool[num2].entityId = entityId;
			Assert.True(this.factory.entityPool[entityId].id == entityId);
			this.factory.entityPool[entityId].monitorId = num2;
			return num2;
		}
		num = this.monitorCursor;
		this.monitorCursor = num + 1;
		int num3 = num;
		if (num3 == this.monitorCapacity)
		{
			this.SetMonitorCapacity(this.monitorCapacity * 2);
		}
		this.monitorPool[num3].SetEmpty();
		this.monitorPool[num3].id = num3;
		this.monitorPool[num3].entityId = entityId;
		Assert.True(this.factory.entityPool[entityId].id == entityId);
		this.factory.entityPool[entityId].monitorId = num3;
		return num3;
	}

	// Token: 0x06000AFD RID: 2813 RVA: 0x000A3BD0 File Offset: 0x000A1DD0
	public void ConnectToMonitor(int monitorId, int targetBeltId, int offset)
	{
		if (monitorId == 0)
		{
			return;
		}
		if (this.monitorPool[monitorId].id != monitorId)
		{
			return;
		}
		if (offset > 10 || offset < -10)
		{
			return;
		}
		this.monitorPool[monitorId].SetTargetBelt(targetBeltId, offset);
	}

	// Token: 0x06000AFE RID: 2814 RVA: 0x000A3C0C File Offset: 0x000A1E0C
	public void DisconnectToMonitor(int monitorId, int removingBeltId)
	{
		if (monitorId == 0)
		{
			return;
		}
		if (this.monitorPool[monitorId].id != monitorId)
		{
			return;
		}
		if (removingBeltId == 0)
		{
			return;
		}
		if (this.monitorPool[monitorId].targetBeltId == removingBeltId)
		{
			this.monitorPool[monitorId].SetTargetBelt(0, 0);
		}
	}

	// Token: 0x06000AFF RID: 2815 RVA: 0x000A3C60 File Offset: 0x000A1E60
	public int GetBeltConnectedToMonitor(int monitorId)
	{
		if (monitorId <= 0 || this.monitorPool[monitorId].id != monitorId)
		{
			return 0;
		}
		int targetBeltId = this.monitorPool[monitorId].targetBeltId;
		if (targetBeltId <= 0 || this.beltPool[targetBeltId].id != targetBeltId)
		{
			return 0;
		}
		return targetBeltId;
	}

	// Token: 0x06000B00 RID: 2816 RVA: 0x000A3CB4 File Offset: 0x000A1EB4
	public void RemoveMonitorComponent(int id)
	{
		if (this.monitorPool[id].id != 0)
		{
			this.monitorPool[id].SetEmpty();
			int[] array = this.monitorRecycle;
			int num = this.monitorRecycleCursor;
			this.monitorRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x06000B01 RID: 2817 RVA: 0x000A3D00 File Offset: 0x000A1F00
	private void SetSpraycoaterCapacity(int newCapacity)
	{
		SpraycoaterComponent[] array = this.spraycoaterPool;
		this.spraycoaterPool = new SpraycoaterComponent[newCapacity];
		this.spraycoaterRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.spraycoaterPool, (newCapacity > this.spraycoaterCapacity) ? this.spraycoaterCapacity : newCapacity);
		}
		this.spraycoaterCapacity = newCapacity;
	}

	// Token: 0x06000B02 RID: 2818 RVA: 0x000A3D54 File Offset: 0x000A1F54
	public int NewSpraycoaterComponent(int entityId, int incCapacity)
	{
		if (this.spraycoaterCapacity < 16)
		{
			this.SetSpraycoaterCapacity(16);
		}
		int num;
		if (this.spraycoaterRecycleCursor > 0)
		{
			int[] array = this.spraycoaterRecycle;
			num = this.spraycoaterRecycleCursor - 1;
			this.spraycoaterRecycleCursor = num;
			int num2 = array[num];
			this.spraycoaterPool[num2].SetEmpty();
			this.spraycoaterPool[num2].id = num2;
			this.spraycoaterPool[num2].entityId = entityId;
			this.spraycoaterPool[num2].incCapacity = incCapacity;
			Assert.True(this.factory.entityPool[entityId].id == entityId);
			this.factory.entityPool[entityId].spraycoaterId = num2;
			return num2;
		}
		num = this.spraycoaterCursor;
		this.spraycoaterCursor = num + 1;
		int num3 = num;
		if (num3 == this.spraycoaterCapacity)
		{
			this.SetSpraycoaterCapacity(this.spraycoaterCapacity * 2);
		}
		this.spraycoaterPool[num3].SetEmpty();
		this.spraycoaterPool[num3].id = num3;
		this.spraycoaterPool[num3].entityId = entityId;
		this.spraycoaterPool[num3].incCapacity = incCapacity;
		Assert.True(this.factory.entityPool[entityId].id == entityId);
		this.factory.entityPool[entityId].spraycoaterId = num3;
		return num3;
	}

	// Token: 0x06000B03 RID: 2819 RVA: 0x000A3EBC File Offset: 0x000A20BC
	public void ConnectToSpraycoater(int spraycoaterId, int cargoBeltId, int incBeltId)
	{
		if (spraycoaterId == 0)
		{
			return;
		}
		if (this.spraycoaterPool[spraycoaterId].id != spraycoaterId)
		{
			return;
		}
		this.spraycoaterPool[spraycoaterId].SetCargoBeltId(this.factory, this.factory.entityAnimPool, cargoBeltId);
		this.spraycoaterPool[spraycoaterId].SetIncBeltId(this.factory, this.factory.entityAnimPool, incBeltId);
	}

	// Token: 0x06000B04 RID: 2820 RVA: 0x000A3F28 File Offset: 0x000A2128
	public void DisconnectToSpraycoater(int spraycoaterId, int removingBeltId)
	{
		if (spraycoaterId == 0)
		{
			return;
		}
		if (this.spraycoaterPool[spraycoaterId].id != spraycoaterId)
		{
			return;
		}
		if (removingBeltId == 0)
		{
			return;
		}
		if (this.spraycoaterPool[spraycoaterId].cargoBeltId == removingBeltId)
		{
			this.spraycoaterPool[spraycoaterId].SetCargoBeltId(this.factory, this.factory.entityAnimPool, 0);
		}
		if (this.spraycoaterPool[spraycoaterId].incBeltId == removingBeltId)
		{
			this.spraycoaterPool[spraycoaterId].SetIncBeltId(this.factory, this.factory.entityAnimPool, 0);
		}
	}

	// Token: 0x06000B05 RID: 2821 RVA: 0x000A3FC0 File Offset: 0x000A21C0
	public void RemoveSpraycoaterComponent(int id)
	{
		if (this.spraycoaterPool[id].id != 0)
		{
			this.spraycoaterPool[id].SetEmpty();
			int[] array = this.spraycoaterRecycle;
			int num = this.spraycoaterRecycleCursor;
			this.spraycoaterRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x06000B06 RID: 2822 RVA: 0x000A400C File Offset: 0x000A220C
	private void SetPilerCapacity(int newCapacity)
	{
		PilerComponent[] array = this.pilerPool;
		this.pilerPool = new PilerComponent[newCapacity];
		this.pilerRecycle = new int[newCapacity];
		if (array != null)
		{
			Array.Copy(array, this.pilerPool, (newCapacity > this.pilerCapacity) ? this.pilerCapacity : newCapacity);
		}
		this.pilerCapacity = newCapacity;
	}

	// Token: 0x06000B07 RID: 2823 RVA: 0x000A4060 File Offset: 0x000A2260
	public int NewPilerComponent(int entityId)
	{
		if (this.pilerCapacity < 16)
		{
			this.SetPilerCapacity(16);
		}
		int num;
		if (this.pilerRecycleCursor > 0)
		{
			int[] array = this.pilerRecycle;
			num = this.pilerRecycleCursor - 1;
			this.pilerRecycleCursor = num;
			int num2 = array[num];
			this.pilerPool[num2].SetEmpty();
			this.pilerPool[num2].id = num2;
			this.pilerPool[num2].entityId = entityId;
			Assert.True(this.factory.entityPool[entityId].id == entityId);
			this.factory.entityPool[entityId].pilerId = num2;
			return num2;
		}
		num = this.pilerCursor;
		this.pilerCursor = num + 1;
		int num3 = num;
		if (num3 == this.pilerCapacity)
		{
			this.SetPilerCapacity(this.pilerCapacity * 2);
		}
		this.pilerPool[num3].SetEmpty();
		this.pilerPool[num3].id = num3;
		this.pilerPool[num3].entityId = entityId;
		Assert.True(this.factory.entityPool[entityId].id == entityId);
		this.factory.entityPool[entityId].pilerId = num3;
		return num3;
	}

	// Token: 0x06000B08 RID: 2824 RVA: 0x000A41A4 File Offset: 0x000A23A4
	public void RematchPilerConnection(int pilerId)
	{
		if (pilerId == 0)
		{
			return;
		}
		if (this.pilerPool[pilerId].id != pilerId)
		{
			return;
		}
		bool flag;
		int num;
		int num2;
		this.factory.ReadObjectConn(this.pilerPool[pilerId].entityId, 0, out flag, out num, out num2);
		bool flag2;
		int num3;
		int num4;
		this.factory.ReadObjectConn(this.pilerPool[pilerId].entityId, 1, out flag2, out num3, out num4);
		int num5 = 0;
		int num6 = 0;
		if (num > 0)
		{
			num5 = this.factory.entityPool[num].beltId;
		}
		if (num3 > 0)
		{
			num6 = this.factory.entityPool[num3].beltId;
		}
		if (num5 <= 0 || num6 <= 0 || flag == flag2)
		{
			this.pilerPool[pilerId].pilerState = PilerState.None;
			return;
		}
		if (flag)
		{
			this.pilerPool[pilerId].outputBeltId = num5;
			this.pilerPool[pilerId].inputBeltId = num6;
			this.pilerPool[pilerId].pilerState = PilerState.Pile;
			return;
		}
		this.pilerPool[pilerId].outputBeltId = num6;
		this.pilerPool[pilerId].inputBeltId = num5;
		this.pilerPool[pilerId].pilerState = PilerState.Split;
	}

	// Token: 0x06000B09 RID: 2825 RVA: 0x000A42E8 File Offset: 0x000A24E8
	public void DisconnectToPiler(int pilerId, int removingBeltId)
	{
		if (pilerId == 0)
		{
			return;
		}
		if (this.pilerPool[pilerId].id != pilerId)
		{
			return;
		}
		if (this.pilerPool[pilerId].inputBeltId == removingBeltId)
		{
			this.pilerPool[pilerId].inputBeltId = 0;
			this.pilerPool[pilerId].pilerState = PilerState.None;
		}
		if (this.pilerPool[pilerId].outputBeltId == removingBeltId)
		{
			this.pilerPool[pilerId].outputBeltId = 0;
			this.pilerPool[pilerId].pilerState = PilerState.None;
		}
	}

	// Token: 0x06000B0A RID: 2826 RVA: 0x000A4380 File Offset: 0x000A2580
	public void RemovePilerComponent(int id)
	{
		if (this.pilerPool[id].id != 0)
		{
			this.pilerPool[id].SetEmpty();
			int[] array = this.pilerRecycle;
			int num = this.pilerRecycleCursor;
			this.pilerRecycleCursor = num + 1;
			array[num] = id;
		}
	}

	// Token: 0x06000B0B RID: 2827 RVA: 0x000A43CC File Offset: 0x000A25CC
	public int PilerConnCount(int id)
	{
		int num = 0;
		if (this.pilerPool[id].id != 0)
		{
			if (this.pilerPool[id].inputBeltId != 0)
			{
				num++;
			}
			if (this.pilerPool[id].outputBeltId != 0)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000B0C RID: 2828 RVA: 0x000A4420 File Offset: 0x000A2620
	public void CargoPathsGameTickSync()
	{
		for (int i = 1; i < this.pathCursor; i++)
		{
			if (this.pathPool[i] != null && this.pathPool[i].id == i)
			{
				this.pathPool[i].Update();
			}
		}
	}

	// Token: 0x06000B0D RID: 2829 RVA: 0x000A4468 File Offset: 0x000A2668
	public void SplitterGameTick(long time)
	{
		for (int i = 1; i < this.splitterCursor; i++)
		{
			if (this.splitterPool[i].id == i)
			{
				this.UpdateSplitter(ref this.splitterPool[i]);
			}
		}
	}

	// Token: 0x06000B0E RID: 2830 RVA: 0x000A44AC File Offset: 0x000A26AC
	public void MonitorGameTick()
	{
		AnimData[] entityAnimPool = this.factory.entityAnimPool;
		SpeakerComponent[] speakerPool = this.factory.digitalSystem.speakerPool;
		EntityData[] entityPool = this.factory.entityPool;
		PowerConsumerComponent[] consumerPool = this.factory.powerSystem.consumerPool;
		bool sandboxToolsEnabled = GameMain.sandboxToolsEnabled;
		for (int i = 1; i < this.monitorCursor; i++)
		{
			if (this.monitorPool[i].id == i)
			{
				this.monitorPool[i].InternalUpdate(this, sandboxToolsEnabled, entityPool, speakerPool, entityAnimPool);
				this.monitorPool[i].SetPCState(consumerPool);
			}
		}
	}

	// Token: 0x06000B0F RID: 2831 RVA: 0x000A4550 File Offset: 0x000A2750
	public void SpraycoaterGameTick()
	{
		AnimData[] entityAnimPool = this.factory.entityAnimPool;
		int[] consumeRegister = GameMain.statistics.production.factoryStatPool[this.factory.index].consumeRegister;
		PowerConsumerComponent[] consumerPool = this.factory.powerSystem.consumerPool;
		for (int i = 1; i < this.spraycoaterCursor; i++)
		{
			if (this.spraycoaterPool[i].id == i)
			{
				this.spraycoaterPool[i].InternalUpdate(this, entityAnimPool, consumeRegister);
				this.spraycoaterPool[i].SetPCState(consumerPool);
			}
		}
	}

	// Token: 0x06000B10 RID: 2832 RVA: 0x000A45E8 File Offset: 0x000A27E8
	public void PilerGameTick()
	{
		AnimData[] entityAnimPool = this.factory.entityAnimPool;
		PowerConsumerComponent[] consumerPool = this.factory.powerSystem.consumerPool;
		for (int i = 1; i < this.pilerCursor; i++)
		{
			if (this.pilerPool[i].id == i)
			{
				this.pilerPool[i].InternalUpdate(this, entityAnimPool);
				this.pilerPool[i].SetPCState(consumerPool);
			}
		}
	}

	// Token: 0x06000B11 RID: 2833 RVA: 0x000A465C File Offset: 0x000A285C
	public void PresentCargoPathsSync()
	{
		for (int i = 0; i < this.pathCursor; i++)
		{
			if (this.pathPool[i] != null && this.pathPool[i].id > 0)
			{
				this.pathPool[i].PresentCargos();
			}
		}
	}

	// Token: 0x06000B12 RID: 2834 RVA: 0x000A46A4 File Offset: 0x000A28A4
	public void UpdateSplitter(ref SplitterComponent sp)
	{
		sp.CheckPriorityPreset();
		if (sp.topId == 0)
		{
			if (sp.input0 == 0 || sp.output0 == 0)
			{
				return;
			}
		}
		else if (sp.input0 == 0 && sp.output0 == 0)
		{
			return;
		}
		this.us_tmp_inputPath = null;
		this.us_tmp_inputPath0 = null;
		this.us_tmp_inputPath1 = null;
		this.us_tmp_inputPath2 = null;
		this.us_tmp_inputPath3 = null;
		this.us_tmp_inputCargo = -1;
		this.us_tmp_inputCargo0 = -1;
		this.us_tmp_inputCargo1 = -1;
		this.us_tmp_inputCargo2 = -1;
		this.us_tmp_inputCargo3 = -1;
		this.us_tmp_inputIndex0 = -1;
		this.us_tmp_inputIndex1 = -1;
		this.us_tmp_inputIndex2 = -1;
		this.us_tmp_inputIndex3 = -1;
		if (sp.input0 != 0)
		{
			this.us_tmp_inputPath = this.GetCargoPath(this.beltPool[sp.input0].segPathId);
			this.us_tmp_inputCargo = this.us_tmp_inputPath.GetCargoIdAtRear();
			if (this.us_tmp_inputCargo != -1)
			{
				this.us_tmp_inputCargo0 = this.us_tmp_inputCargo;
				this.us_tmp_inputPath0 = this.us_tmp_inputPath;
				this.us_tmp_inputIndex0 = 0;
			}
			if (sp.input1 != 0)
			{
				this.us_tmp_inputPath = this.GetCargoPath(this.beltPool[sp.input1].segPathId);
				this.us_tmp_inputCargo = this.us_tmp_inputPath.GetCargoIdAtRear();
				if (this.us_tmp_inputCargo != -1)
				{
					if (this.us_tmp_inputPath0 == null)
					{
						this.us_tmp_inputCargo0 = this.us_tmp_inputCargo;
						this.us_tmp_inputPath0 = this.us_tmp_inputPath;
						this.us_tmp_inputIndex0 = 1;
					}
					else
					{
						this.us_tmp_inputCargo1 = this.us_tmp_inputCargo;
						this.us_tmp_inputPath1 = this.us_tmp_inputPath;
						this.us_tmp_inputIndex1 = 1;
					}
				}
				if (sp.input2 != 0)
				{
					this.us_tmp_inputPath = this.GetCargoPath(this.beltPool[sp.input2].segPathId);
					this.us_tmp_inputCargo = this.us_tmp_inputPath.GetCargoIdAtRear();
					if (this.us_tmp_inputCargo != -1)
					{
						if (this.us_tmp_inputPath0 == null)
						{
							this.us_tmp_inputCargo0 = this.us_tmp_inputCargo;
							this.us_tmp_inputPath0 = this.us_tmp_inputPath;
							this.us_tmp_inputIndex0 = 2;
						}
						else if (this.us_tmp_inputPath1 == null)
						{
							this.us_tmp_inputCargo1 = this.us_tmp_inputCargo;
							this.us_tmp_inputPath1 = this.us_tmp_inputPath;
							this.us_tmp_inputIndex1 = 2;
						}
						else
						{
							this.us_tmp_inputCargo2 = this.us_tmp_inputCargo;
							this.us_tmp_inputPath2 = this.us_tmp_inputPath;
							this.us_tmp_inputIndex2 = 2;
						}
					}
					if (sp.input3 != 0)
					{
						this.us_tmp_inputPath = this.GetCargoPath(this.beltPool[sp.input3].segPathId);
						this.us_tmp_inputCargo = this.us_tmp_inputPath.GetCargoIdAtRear();
						if (this.us_tmp_inputCargo != -1)
						{
							if (this.us_tmp_inputPath0 == null)
							{
								this.us_tmp_inputCargo0 = this.us_tmp_inputCargo;
								this.us_tmp_inputPath0 = this.us_tmp_inputPath;
								this.us_tmp_inputIndex0 = 3;
							}
							else if (this.us_tmp_inputPath1 == null)
							{
								this.us_tmp_inputCargo1 = this.us_tmp_inputCargo;
								this.us_tmp_inputPath1 = this.us_tmp_inputPath;
								this.us_tmp_inputIndex1 = 3;
							}
							else if (this.us_tmp_inputPath2 == null)
							{
								this.us_tmp_inputCargo2 = this.us_tmp_inputCargo;
								this.us_tmp_inputPath2 = this.us_tmp_inputPath;
								this.us_tmp_inputIndex2 = 3;
							}
							else
							{
								this.us_tmp_inputCargo3 = this.us_tmp_inputCargo;
								this.us_tmp_inputPath3 = this.us_tmp_inputPath;
								this.us_tmp_inputIndex3 = 3;
							}
						}
					}
				}
			}
		}
		while (this.us_tmp_inputPath0 != null)
		{
			bool flag = true;
			if (sp.outFilter != 0)
			{
				flag = ((int)this.container.cargoPool[this.us_tmp_inputCargo0].item == sp.outFilter);
			}
			this.us_tmp_outputPath = null;
			this.us_tmp_outputPath0 = null;
			this.us_tmp_outputIdx = 0;
			int num = -1;
			if (!flag && sp.outFilter != 0)
			{
				goto IL_3E5;
			}
			if (sp.output0 != 0)
			{
				this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output0].segPathId);
				num = this.us_tmp_outputPath.TestBlankAtHead();
				if (this.us_tmp_outputPath.pathLength <= 10 || num < 0)
				{
					goto IL_3E5;
				}
				this.us_tmp_outputPath0 = this.us_tmp_outputPath;
				this.us_tmp_outputIdx = 0;
			}
			IL_514:
			if (this.us_tmp_outputPath0 != null)
			{
				int num2 = this.us_tmp_inputPath0.TryPickCargoAtEnd();
				Assert.True(num2 >= 0);
				this.us_tmp_outputPath0.InsertCargoAtHeadDirect(num2, num);
				sp.InputAlternate(this.us_tmp_inputIndex0);
				sp.OutputAlternate(this.us_tmp_outputIdx);
			}
			else if (sp.topId != 0 && (flag || sp.outFilter == 0) && this.factory.InsertCargoIntoStorage(sp.topId, this.us_tmp_inputCargo0, true))
			{
				int num3 = this.us_tmp_inputPath0.TryPickCargoAtEnd();
				Assert.True(num3 >= 0);
				this.container.RemoveCargo(num3);
				sp.InputAlternate(this.us_tmp_inputIndex0);
			}
			this.us_tmp_inputPath0 = this.us_tmp_inputPath1;
			this.us_tmp_inputCargo0 = this.us_tmp_inputCargo1;
			this.us_tmp_inputIndex0 = this.us_tmp_inputIndex1;
			this.us_tmp_inputPath1 = this.us_tmp_inputPath2;
			this.us_tmp_inputCargo1 = this.us_tmp_inputCargo2;
			this.us_tmp_inputIndex1 = this.us_tmp_inputIndex2;
			this.us_tmp_inputPath2 = this.us_tmp_inputPath3;
			this.us_tmp_inputCargo2 = this.us_tmp_inputCargo3;
			this.us_tmp_inputIndex2 = this.us_tmp_inputIndex3;
			this.us_tmp_inputPath3 = null;
			this.us_tmp_inputCargo3 = -1;
			this.us_tmp_inputIndex3 = -1;
			continue;
			IL_3E5:
			if ((flag && sp.outFilter != 0) || sp.output1 == 0)
			{
				goto IL_514;
			}
			this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output1].segPathId);
			num = this.us_tmp_outputPath.TestBlankAtHead();
			if (this.us_tmp_outputPath.pathLength > 10 && num >= 0)
			{
				this.us_tmp_outputPath0 = this.us_tmp_outputPath;
				this.us_tmp_outputIdx = 1;
				goto IL_514;
			}
			if (sp.output2 == 0)
			{
				goto IL_514;
			}
			this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output2].segPathId);
			num = this.us_tmp_outputPath.TestBlankAtHead();
			if (this.us_tmp_outputPath.pathLength > 10 && num >= 0)
			{
				this.us_tmp_outputPath0 = this.us_tmp_outputPath;
				this.us_tmp_outputIdx = 2;
				goto IL_514;
			}
			if (sp.output3 == 0)
			{
				goto IL_514;
			}
			this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output3].segPathId);
			num = this.us_tmp_outputPath.TestBlankAtHead();
			if (this.us_tmp_outputPath.pathLength > 10 && num >= 0)
			{
				this.us_tmp_outputPath0 = this.us_tmp_outputPath;
				this.us_tmp_outputIdx = 3;
				goto IL_514;
			}
			goto IL_514;
		}
		if (sp.topId != 0)
		{
			if (sp.outFilter == 0)
			{
				int num4 = 4;
				while (num4-- > 0)
				{
					this.us_tmp_outputPath = null;
					this.us_tmp_outputPath0 = null;
					this.us_tmp_outputIdx = 0;
					int num5 = -1;
					if (sp.output0 != 0)
					{
						this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output0].segPathId);
						num5 = this.us_tmp_outputPath.TestBlankAtHead();
						if (this.us_tmp_outputPath.pathLength > 10 && num5 >= 0)
						{
							this.us_tmp_outputPath0 = this.us_tmp_outputPath;
							this.us_tmp_outputIdx = 0;
						}
						else if (sp.output1 != 0)
						{
							this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output1].segPathId);
							num5 = this.us_tmp_outputPath.TestBlankAtHead();
							if (this.us_tmp_outputPath.pathLength > 10 && num5 >= 0)
							{
								this.us_tmp_outputPath0 = this.us_tmp_outputPath;
								this.us_tmp_outputIdx = 1;
							}
							else if (sp.output2 != 0)
							{
								this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output2].segPathId);
								num5 = this.us_tmp_outputPath.TestBlankAtHead();
								if (this.us_tmp_outputPath.pathLength > 10 && num5 >= 0)
								{
									this.us_tmp_outputPath0 = this.us_tmp_outputPath;
									this.us_tmp_outputIdx = 2;
								}
								else if (sp.output3 != 0)
								{
									this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output3].segPathId);
									num5 = this.us_tmp_outputPath.TestBlankAtHead();
									if (this.us_tmp_outputPath.pathLength > 10 && num5 >= 0)
									{
										this.us_tmp_outputPath0 = this.us_tmp_outputPath;
										this.us_tmp_outputIdx = 3;
									}
								}
							}
						}
					}
					if (this.us_tmp_outputPath0 == null)
					{
						return;
					}
					int num6 = (this.us_tmp_outputIdx == 0) ? sp.outFilter : (-sp.outFilter);
					int num8;
					int num7 = this.factory.PickFromStorageFiltered(sp.topId, ref num6, 1, out num8);
					if (num6 <= 0 || num7 <= 0)
					{
						return;
					}
					int cargoId = this.container.AddCargo((short)num6, (byte)num7, (byte)num8);
					this.us_tmp_outputPath0.InsertCargoAtHeadDirect(cargoId, num5);
					sp.OutputAlternate(this.us_tmp_outputIdx);
				}
				return;
			}
			this.us_tmp_outputPath = null;
			this.us_tmp_outputPath0 = null;
			this.us_tmp_outputIdx = 0;
			int num9 = -1;
			if (sp.output0 != 0)
			{
				this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output0].segPathId);
				num9 = this.us_tmp_outputPath.TestBlankAtHead();
				if (this.us_tmp_outputPath.pathLength > 10 && num9 >= 0)
				{
					this.us_tmp_outputPath0 = this.us_tmp_outputPath;
					this.us_tmp_outputIdx = 0;
				}
			}
			if (this.us_tmp_outputPath0 != null)
			{
				int outFilter = sp.outFilter;
				int num11;
				int num10 = this.factory.PickFromStorageFiltered(sp.topId, ref outFilter, 1, out num11);
				if (outFilter > 0 && num10 > 0)
				{
					int cargoId2 = this.container.AddCargo((short)outFilter, (byte)num10, (byte)num11);
					this.us_tmp_outputPath0.InsertCargoAtHeadDirect(cargoId2, num9);
					sp.OutputAlternate(this.us_tmp_outputIdx);
				}
			}
			int num12 = 3;
			while (num12-- > 0)
			{
				this.us_tmp_outputPath = null;
				this.us_tmp_outputPath0 = null;
				this.us_tmp_outputIdx = 0;
				int num13 = -1;
				if (sp.output1 != 0)
				{
					this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output1].segPathId);
					num13 = this.us_tmp_outputPath.TestBlankAtHead();
					if (this.us_tmp_outputPath.pathLength > 10 && num13 >= 0)
					{
						this.us_tmp_outputPath0 = this.us_tmp_outputPath;
						this.us_tmp_outputIdx = 1;
					}
					else if (sp.output2 != 0)
					{
						this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output2].segPathId);
						num13 = this.us_tmp_outputPath.TestBlankAtHead();
						if (this.us_tmp_outputPath.pathLength > 10 && num13 >= 0)
						{
							this.us_tmp_outputPath0 = this.us_tmp_outputPath;
							this.us_tmp_outputIdx = 2;
						}
						else if (sp.output3 != 0)
						{
							this.us_tmp_outputPath = this.GetCargoPath(this.beltPool[sp.output3].segPathId);
							num13 = this.us_tmp_outputPath.TestBlankAtHead();
							if (this.us_tmp_outputPath.pathLength > 10 && num13 >= 0)
							{
								this.us_tmp_outputPath0 = this.us_tmp_outputPath;
								this.us_tmp_outputIdx = 3;
							}
						}
					}
				}
				if (this.us_tmp_outputPath0 == null)
				{
					break;
				}
				int num14 = -sp.outFilter;
				int num16;
				int num15 = this.factory.PickFromStorageFiltered(sp.topId, ref num14, 1, out num16);
				if (num14 <= 0 || num15 <= 0)
				{
					break;
				}
				int cargoId3 = this.container.AddCargo((short)num14, (byte)num15, (byte)num16);
				this.us_tmp_outputPath0.InsertCargoAtHeadDirect(cargoId3, num13);
				sp.OutputAlternate(this.us_tmp_outputIdx);
			}
		}
	}

	// Token: 0x06000B13 RID: 2835 RVA: 0x000A51D4 File Offset: 0x000A33D4
	public void UpdateSplitterAsync(ref SplitterComponent sp)
	{
		sp.CheckPriorityPreset();
		if (sp.topId == 0)
		{
			if (sp.input0 == 0 || sp.output0 == 0)
			{
				return;
			}
		}
		else if (sp.input0 == 0 && sp.output0 == 0)
		{
			return;
		}
		CargoPath cargoPath = null;
		CargoPath cargoPath2 = null;
		CargoPath cargoPath3 = null;
		CargoPath cargoPath4 = null;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int index = -1;
		int num5 = -1;
		int num6 = -1;
		int num7 = -1;
		if (sp.input0 != 0)
		{
			CargoPath cargoPath5 = this.GetCargoPath(this.beltPool[sp.input0].segPathId);
			int cargoIdAtRear = cargoPath5.GetCargoIdAtRear();
			if (cargoIdAtRear != -1)
			{
				num = cargoIdAtRear;
				cargoPath = cargoPath5;
				index = 0;
			}
			if (sp.input1 != 0)
			{
				cargoPath5 = this.GetCargoPath(this.beltPool[sp.input1].segPathId);
				cargoIdAtRear = cargoPath5.GetCargoIdAtRear();
				if (cargoIdAtRear != -1)
				{
					if (cargoPath == null)
					{
						num = cargoIdAtRear;
						cargoPath = cargoPath5;
						index = 1;
					}
					else
					{
						num2 = cargoIdAtRear;
						cargoPath2 = cargoPath5;
						num5 = 1;
					}
				}
				if (sp.input2 != 0)
				{
					cargoPath5 = this.GetCargoPath(this.beltPool[sp.input2].segPathId);
					cargoIdAtRear = cargoPath5.GetCargoIdAtRear();
					if (cargoIdAtRear != -1)
					{
						if (cargoPath == null)
						{
							num = cargoIdAtRear;
							cargoPath = cargoPath5;
							index = 2;
						}
						else if (cargoPath2 == null)
						{
							num2 = cargoIdAtRear;
							cargoPath2 = cargoPath5;
							num5 = 2;
						}
						else
						{
							num3 = cargoIdAtRear;
							cargoPath3 = cargoPath5;
							num6 = 2;
						}
					}
					if (sp.input3 != 0)
					{
						cargoPath5 = this.GetCargoPath(this.beltPool[sp.input3].segPathId);
						cargoIdAtRear = cargoPath5.GetCargoIdAtRear();
						if (cargoIdAtRear != -1)
						{
							if (cargoPath == null)
							{
								num = cargoIdAtRear;
								cargoPath = cargoPath5;
								index = 3;
							}
							else if (cargoPath2 == null)
							{
								num2 = cargoIdAtRear;
								cargoPath2 = cargoPath5;
								num5 = 3;
							}
							else if (cargoPath3 == null)
							{
								num3 = cargoIdAtRear;
								cargoPath3 = cargoPath5;
								num6 = 3;
							}
							else
							{
								num4 = cargoIdAtRear;
								cargoPath4 = cargoPath5;
								num7 = 3;
							}
						}
					}
				}
			}
		}
		while (cargoPath != null)
		{
			bool flag = true;
			if (sp.outFilter != 0)
			{
				flag = ((int)this.container.cargoPool[num].item == sp.outFilter);
			}
			CargoPath cargoPath6 = null;
			int num8 = 0;
			int num9 = -1;
			if (!flag && sp.outFilter != 0)
			{
				goto IL_24F;
			}
			CargoPath cargoPath7;
			if (sp.output0 != 0)
			{
				cargoPath7 = this.GetCargoPath(this.beltPool[sp.output0].segPathId);
				num9 = cargoPath7.TestBlankAtHead();
				if (cargoPath7.pathLength <= 10 || num9 < 0)
				{
					goto IL_24F;
				}
				cargoPath6 = cargoPath7;
				num8 = 0;
			}
			IL_33D:
			if (cargoPath6 != null)
			{
				int num10 = cargoPath.TryPickCargoAtEnd();
				Assert.True(num10 >= 0);
				cargoPath6.InsertCargoAtHeadDirect(num10, num9);
				sp.InputAlternate(index);
				sp.OutputAlternate(num8);
			}
			else if (sp.topId != 0 && (flag || sp.outFilter == 0) && this.factory.InsertCargoIntoStorage(sp.topId, num, true))
			{
				int num11 = cargoPath.TryPickCargoAtEnd();
				Assert.True(num11 >= 0);
				this.container.RemoveCargo(num11);
				sp.InputAlternate(index);
			}
			cargoPath = cargoPath2;
			num = num2;
			index = num5;
			cargoPath2 = cargoPath3;
			num2 = num3;
			num5 = num6;
			cargoPath3 = cargoPath4;
			num3 = num4;
			num6 = num7;
			cargoPath4 = null;
			num4 = -1;
			num7 = -1;
			continue;
			IL_24F:
			if ((flag && sp.outFilter != 0) || sp.output1 == 0)
			{
				goto IL_33D;
			}
			cargoPath7 = this.GetCargoPath(this.beltPool[sp.output1].segPathId);
			num9 = cargoPath7.TestBlankAtHead();
			if (cargoPath7.pathLength > 10 && num9 >= 0)
			{
				cargoPath6 = cargoPath7;
				num8 = 1;
				goto IL_33D;
			}
			if (sp.output2 == 0)
			{
				goto IL_33D;
			}
			cargoPath7 = this.GetCargoPath(this.beltPool[sp.output2].segPathId);
			num9 = cargoPath7.TestBlankAtHead();
			if (cargoPath7.pathLength > 10 && num9 >= 0)
			{
				cargoPath6 = cargoPath7;
				num8 = 2;
				goto IL_33D;
			}
			if (sp.output3 == 0)
			{
				goto IL_33D;
			}
			cargoPath7 = this.GetCargoPath(this.beltPool[sp.output3].segPathId);
			num9 = cargoPath7.TestBlankAtHead();
			if (cargoPath7.pathLength > 10 && num9 >= 0)
			{
				cargoPath6 = cargoPath7;
				num8 = 3;
				goto IL_33D;
			}
			goto IL_33D;
		}
		if (sp.topId != 0)
		{
			CargoPath cargoPath6;
			int num8;
			if (sp.outFilter == 0)
			{
				int num12 = 4;
				while (num12-- > 0)
				{
					cargoPath6 = null;
					num8 = 0;
					int num13 = -1;
					if (sp.output0 != 0)
					{
						CargoPath cargoPath7 = this.GetCargoPath(this.beltPool[sp.output0].segPathId);
						num13 = cargoPath7.TestBlankAtHead();
						if (cargoPath7.pathLength > 10 && num13 >= 0)
						{
							cargoPath6 = cargoPath7;
							num8 = 0;
						}
						else if (sp.output1 != 0)
						{
							cargoPath7 = this.GetCargoPath(this.beltPool[sp.output1].segPathId);
							num13 = cargoPath7.TestBlankAtHead();
							if (cargoPath7.pathLength > 10 && num13 >= 0)
							{
								cargoPath6 = cargoPath7;
								num8 = 1;
							}
							else if (sp.output2 != 0)
							{
								cargoPath7 = this.GetCargoPath(this.beltPool[sp.output2].segPathId);
								num13 = cargoPath7.TestBlankAtHead();
								if (cargoPath7.pathLength > 10 && num13 >= 0)
								{
									cargoPath6 = cargoPath7;
									num8 = 2;
								}
								else if (sp.output3 != 0)
								{
									cargoPath7 = this.GetCargoPath(this.beltPool[sp.output3].segPathId);
									num13 = cargoPath7.TestBlankAtHead();
									if (cargoPath7.pathLength > 10 && num13 >= 0)
									{
										cargoPath6 = cargoPath7;
										num8 = 3;
									}
								}
							}
						}
					}
					if (cargoPath6 == null)
					{
						return;
					}
					int num14 = (num8 == 0) ? sp.outFilter : (-sp.outFilter);
					int num16;
					int num15 = this.factory.PickFromStorageFiltered(sp.topId, ref num14, 1, out num16);
					if (num14 <= 0 || num15 <= 0)
					{
						return;
					}
					int cargoId = this.container.AddCargo((short)num14, (byte)num15, (byte)num16);
					cargoPath6.InsertCargoAtHeadDirect(cargoId, num13);
					sp.OutputAlternate(num8);
				}
				return;
			}
			cargoPath6 = null;
			num8 = 0;
			int num17 = -1;
			if (sp.output0 != 0)
			{
				CargoPath cargoPath7 = this.GetCargoPath(this.beltPool[sp.output0].segPathId);
				num17 = cargoPath7.TestBlankAtHead();
				if (cargoPath7.pathLength > 10 && num17 >= 0)
				{
					cargoPath6 = cargoPath7;
					num8 = 0;
				}
			}
			if (cargoPath6 != null)
			{
				int outFilter = sp.outFilter;
				int num19;
				int num18 = this.factory.PickFromStorageFiltered(sp.topId, ref outFilter, 1, out num19);
				if (outFilter > 0 && num18 > 0)
				{
					int cargoId2 = this.container.AddCargo((short)outFilter, (byte)num18, (byte)num19);
					cargoPath6.InsertCargoAtHeadDirect(cargoId2, num17);
					sp.OutputAlternate(num8);
				}
			}
			int num20 = 3;
			while (num20-- > 0)
			{
				cargoPath6 = null;
				num8 = 0;
				int num21 = -1;
				if (sp.output1 != 0)
				{
					CargoPath cargoPath7 = this.GetCargoPath(this.beltPool[sp.output1].segPathId);
					num21 = cargoPath7.TestBlankAtHead();
					if (cargoPath7.pathLength > 10 && num21 >= 0)
					{
						cargoPath6 = cargoPath7;
						num8 = 1;
					}
					else if (sp.output2 != 0)
					{
						cargoPath7 = this.GetCargoPath(this.beltPool[sp.output2].segPathId);
						num21 = cargoPath7.TestBlankAtHead();
						if (cargoPath7.pathLength > 10 && num21 >= 0)
						{
							cargoPath6 = cargoPath7;
							num8 = 2;
						}
						else if (sp.output3 != 0)
						{
							cargoPath7 = this.GetCargoPath(this.beltPool[sp.output3].segPathId);
							num21 = cargoPath7.TestBlankAtHead();
							if (cargoPath7.pathLength > 10 && num21 >= 0)
							{
								cargoPath6 = cargoPath7;
								num8 = 3;
							}
						}
					}
				}
				if (cargoPath6 == null)
				{
					break;
				}
				int num22 = -sp.outFilter;
				int num24;
				int num23 = this.factory.PickFromStorageFiltered(sp.topId, ref num22, 1, out num24);
				if (num22 <= 0 || num23 <= 0)
				{
					break;
				}
				int cargoId3 = this.container.AddCargo((short)num22, (byte)num23, (byte)num24);
				cargoPath6.InsertCargoAtHeadDirect(cargoId3, num21);
				sp.OutputAlternate(num8);
			}
		}
	}

	// Token: 0x06000B14 RID: 2836 RVA: 0x000A59A9 File Offset: 0x000A3BA9
	public bool TryInsertItem(int beltId, int offset, int itemId, byte itemCount, byte itemInc)
	{
		return this.pathPool[this.beltPool[beltId].segPathId].TryInsertItem(this.beltPool[beltId].pivotOnPath + offset, itemId, itemCount, itemInc);
	}

	// Token: 0x06000B15 RID: 2837 RVA: 0x000A59E0 File Offset: 0x000A3BE0
	public void TryInsertItemToBeltWithStackIncreasement(int beltId, int offset, int itemId, int maxStack, ref int itemCount, ref int itemInc)
	{
		if (this.beltPool[beltId].id <= 0)
		{
			return;
		}
		this.pathPool[this.beltPool[beltId].segPathId].TryInsertItemWithStackIncreasement(this.beltPool[beltId].pivotOnPath + offset, itemId, maxStack, ref itemCount, ref itemInc);
	}

	// Token: 0x06000B16 RID: 2838 RVA: 0x000A5A39 File Offset: 0x000A3C39
	public bool TryInsertItemAtHead(int beltId, int itemId, byte stack, byte inc)
	{
		return this.pathPool[this.beltPool[beltId].segPathId].TryInsertItemAtHeadAndFillBlank(itemId, stack, inc);
	}

	// Token: 0x06000B17 RID: 2839 RVA: 0x000A5A5C File Offset: 0x000A3C5C
	public int GetItemIdAtRear(int beltId)
	{
		CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
		Assert.NotNull(cargoPath);
		int cargoIdAtRear = cargoPath.GetCargoIdAtRear();
		if (cargoIdAtRear == -1)
		{
			return 0;
		}
		return (int)this.container.cargoPool[cargoIdAtRear].item;
	}

	// Token: 0x06000B18 RID: 2840 RVA: 0x000A5AA8 File Offset: 0x000A3CA8
	public bool HasCargoAtRear(int beltId)
	{
		CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
		Assert.NotNull(cargoPath);
		return cargoPath.HasCargoAtRear();
	}

	// Token: 0x06000B19 RID: 2841 RVA: 0x000A5ACC File Offset: 0x000A3CCC
	public int TryPickItemAtRear(int beltId, int filter, int[] needs, out byte stack, out byte inc)
	{
		stack = 1;
		inc = 0;
		CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
		Assert.NotNull(cargoPath);
		int cargoIdAtRear = cargoPath.GetCargoIdAtRear();
		if (cargoIdAtRear == -1)
		{
			return 0;
		}
		int item = (int)this.container.cargoPool[cargoIdAtRear].item;
		stack = this.container.cargoPool[cargoIdAtRear].stack;
		inc = this.container.cargoPool[cargoIdAtRear].inc;
		if (filter != 0)
		{
			if (item == filter)
			{
				cargoPath.TryRemoveItemAtRear(cargoIdAtRear);
				return item;
			}
		}
		else
		{
			if (needs == null)
			{
				cargoPath.TryRemoveItemAtRear(cargoIdAtRear);
				return item;
			}
			for (int i = 0; i < needs.Length; i++)
			{
				if (needs[i] == item)
				{
					cargoPath.TryRemoveItemAtRear(cargoIdAtRear);
					return item;
				}
			}
		}
		stack = 1;
		inc = 0;
		return 0;
	}

	// Token: 0x06000B1A RID: 2842 RVA: 0x000A5B97 File Offset: 0x000A3D97
	public int TryPickFuel(int beltId, int offset, int filter, int fuelMask, out byte stack, out byte inc)
	{
		return this.GetCargoPath(this.beltPool[beltId].segPathId).TryPickFuel(this.beltPool[beltId].pivotOnPath + offset - 2, 5, filter, fuelMask, out stack, out inc);
	}

	// Token: 0x06000B1B RID: 2843 RVA: 0x000A5BD4 File Offset: 0x000A3DD4
	public int TryPickItem(int beltId, int offset, int filter, out byte stack, out byte inc)
	{
		if (filter != 0)
		{
			return this.GetCargoPath(this.beltPool[beltId].segPathId).TryPickItem(this.beltPool[beltId].pivotOnPath + offset - 2, 5, filter, out stack, out inc);
		}
		return this.GetCargoPath(this.beltPool[beltId].segPathId).TryPickItem(this.beltPool[beltId].pivotOnPath + offset - 2, 5, out stack, out inc);
	}

	// Token: 0x06000B1C RID: 2844 RVA: 0x000A5C52 File Offset: 0x000A3E52
	public int TryPickItem(int beltId, int offset, int filter, int[] needs, out byte stack, out byte inc)
	{
		return this.GetCargoPath(this.beltPool[beltId].segPathId).TryPickItem(this.beltPool[beltId].pivotOnPath + offset - 2, 5, filter, needs, out stack, out inc);
	}

	// Token: 0x06000B1D RID: 2845 RVA: 0x000A5C90 File Offset: 0x000A3E90
	public void ThrowItems_Belt(int beltId, float dropRate)
	{
		if (beltId == 0)
		{
			return;
		}
		TrashSystem trashSystem = GameMain.data.trashSystem;
		if (this.beltPool[beltId].id == beltId)
		{
			int segIndex = this.beltPool[beltId].segIndex;
			int num = this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength - 1;
			CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
			for (int i = segIndex; i < num; i++)
			{
				byte count;
				byte inc;
				int itemId = cargoPath.TryPickItem(i - 4 - 1, 12, out count, out inc);
				trashSystem.AddTrashOnPlanet(itemId, (int)count, (int)inc, this.beltPool[beltId].entityId, this.planet);
			}
		}
	}

	// Token: 0x06000B1E RID: 2846 RVA: 0x000A5D54 File Offset: 0x000A3F54
	public void AlterBeltConnections(int beltId, int outputId, int input0Id, int input1Id, int input2Id, bool bridgeChanged = false)
	{
		if (beltId == 0 || this.beltPool[beltId].id != beltId)
		{
			return;
		}
		if (outputId == beltId)
		{
			outputId = 0;
		}
		if (input0Id == beltId)
		{
			input0Id = 0;
		}
		if (input1Id == beltId)
		{
			input1Id = 0;
		}
		if (input2Id == beltId)
		{
			input2Id = 0;
		}
		if (outputId != 0 && this.beltPool[outputId].rightInputId > 0 && this.beltPool[outputId].rightInputId != beltId && this.beltPool[outputId].backInputId > 0 && this.beltPool[outputId].backInputId != beltId && this.beltPool[outputId].leftInputId > 0 && this.beltPool[outputId].leftInputId != beltId)
		{
			Debug.Log("传送带输入接口已满");
			outputId = 0;
		}
		if (this.beltPool[outputId].id != outputId)
		{
			outputId = 0;
		}
		if (this.beltPool[input0Id].id != input0Id)
		{
			input0Id = 0;
		}
		if (this.beltPool[input1Id].id != input1Id)
		{
			input1Id = 0;
		}
		if (this.beltPool[input2Id].id != input2Id)
		{
			input2Id = 0;
		}
		if (input0Id == outputId)
		{
			outputId = 0;
		}
		if (input1Id == outputId)
		{
			outputId = 0;
		}
		if (input2Id == outputId)
		{
			outputId = 0;
		}
		this._arrInputs(ref input0Id, ref input1Id, ref input2Id);
		EntityData[] entityPool = this.factory.entityPool;
		ColliderContainer[] colChunks = (this.planet.physics == null) ? null : this.planet.physics.colChunks;
		Vector3 pos = entityPool[this.beltPool[beltId].entityId].pos;
		Vector3 pos2 = entityPool[this.beltPool[outputId].entityId].pos;
		Vector3 pos3 = entityPool[this.beltPool[input0Id].entityId].pos;
		Vector3 pos4 = entityPool[this.beltPool[input1Id].entityId].pos;
		Vector3 pos5 = entityPool[this.beltPool[input2Id].entityId].pos;
		Vector3 normalized = pos.normalized;
		Vector3 zero = Vector3.zero;
		int num = 0;
		Vector3 zero2 = Vector3.zero;
		int num2 = 0;
		Vector3 zero3 = Vector3.zero;
		int num3 = 0;
		if (!bridgeChanged)
		{
			if (outputId > 0)
			{
				Vector3 vector = pos2 - pos;
				vector = (vector - 0.8f * Vector3.Dot(vector, normalized) * normalized).normalized;
				Vector3 lhs = Vector3.Cross(normalized, vector);
				if (input0Id == 0)
				{
					num2 = (num = (num3 = 0));
				}
				else if (input1Id == 0)
				{
					Vector3 vector2 = pos3 - pos;
					vector2 = (vector2 - 0.8f * Vector3.Dot(vector2, normalized) * normalized).normalized;
					float num4 = Vector3.Dot(lhs, vector2);
					if (Vector3.Dot(vector, vector2) <= -0.70710677f)
					{
						num = input0Id;
					}
					else if (num4 > 0f)
					{
						num3 = input0Id;
					}
					else
					{
						num2 = input0Id;
					}
				}
				else if (input2Id == 0)
				{
					Vector3 vector3 = pos3 - pos;
					vector3 = (vector3 - 0.8f * Vector3.Dot(vector3, normalized) * normalized).normalized;
					float num5 = Vector3.Dot(lhs, vector3);
					float num6 = Vector3.Dot(vector, vector3);
					Vector3 vector4 = pos4 - pos;
					vector4 = (vector4 - 0.8f * Vector3.Dot(vector4, normalized) * normalized).normalized;
					float num7 = Vector3.Dot(lhs, vector4);
					float num8 = Vector3.Dot(vector, vector4);
					float num9 = (num6 < num8) ? num6 : num8;
					if (num5 * num7 <= -0.001f && num9 > -0.5f)
					{
						if (num5 > 0f)
						{
							num3 = input0Id;
							num2 = input1Id;
						}
						else
						{
							num3 = input1Id;
							num2 = input0Id;
						}
					}
					else if (num6 < num8)
					{
						num = input0Id;
						if (num7 > 0f)
						{
							num3 = input1Id;
						}
						else
						{
							num2 = input1Id;
						}
					}
					else
					{
						num = input1Id;
						if (num5 > 0f)
						{
							num3 = input0Id;
						}
						else
						{
							num2 = input0Id;
						}
					}
				}
				else
				{
					Vector3 vector5 = pos3 - pos;
					vector5 = (vector5 - 0.8f * Vector3.Dot(vector5, normalized) * normalized).normalized;
					float y = Vector3.Dot(lhs, vector5);
					float x = Vector3.Dot(vector, vector5);
					float num10 = Mathf.Atan2(y, x);
					if (num10 < 0f)
					{
						num10 += 6.2831855f;
					}
					Vector3 vector6 = pos4 - pos;
					vector6 = (vector6 - 0.8f * Vector3.Dot(vector6, normalized) * normalized).normalized;
					float y2 = Vector3.Dot(lhs, vector6);
					float x2 = Vector3.Dot(vector, vector6);
					float num11 = Mathf.Atan2(y2, x2);
					if (num11 < 0f)
					{
						num11 += 6.2831855f;
					}
					Vector3 vector7 = pos5 - pos;
					vector7 = (vector7 - 0.8f * Vector3.Dot(vector7, normalized) * normalized).normalized;
					float y3 = Vector3.Dot(lhs, vector7);
					float x3 = Vector3.Dot(vector, vector7);
					float num12 = Mathf.Atan2(y3, x3);
					if (num12 < 0f)
					{
						num12 += 6.2831855f;
					}
					if (num10 <= num11 && num10 <= num12)
					{
						num3 = input0Id;
						if (num11 > num12)
						{
							num2 = input1Id;
							num = input2Id;
						}
						else
						{
							num2 = input2Id;
							num = input1Id;
						}
					}
					else if (num11 <= num10 && num11 <= num12)
					{
						num3 = input1Id;
						if (num10 > num12)
						{
							num2 = input0Id;
							num = input2Id;
						}
						else
						{
							num2 = input2Id;
							num = input0Id;
						}
					}
					else
					{
						num3 = input2Id;
						if (num10 > num11)
						{
							num2 = input0Id;
							num = input1Id;
						}
						else
						{
							num2 = input1Id;
							num = input0Id;
						}
					}
				}
			}
			else if (input0Id == 0)
			{
				num2 = (num = (num3 = 0));
			}
			else if (input1Id == 0)
			{
				num = input0Id;
			}
			else if (input2Id == 0)
			{
				Vector3 normalized2 = (pos3 - pos).normalized;
				Vector3 normalized3 = (pos4 - pos).normalized;
				if (Vector3.Angle(normalized2, normalized3) < 120f)
				{
					Vector3 rhs = -normalized2;
					if (Vector3.Dot(Vector3.Cross(normalized, rhs), normalized3) > 0f)
					{
						if (Mathf.Abs(entityPool[this.beltPool[input0Id].entityId].tilt - entityPool[this.beltPool[beltId].entityId].tilt) < Mathf.Abs(entityPool[this.beltPool[input1Id].entityId].tilt - entityPool[this.beltPool[beltId].entityId].tilt))
						{
							num = input0Id;
							num3 = input1Id;
						}
						else
						{
							num2 = input0Id;
							num = input1Id;
						}
					}
					else if (Mathf.Abs(entityPool[this.beltPool[input0Id].entityId].tilt - entityPool[this.beltPool[beltId].entityId].tilt) < Mathf.Abs(entityPool[this.beltPool[input1Id].entityId].tilt - entityPool[this.beltPool[beltId].entityId].tilt))
					{
						num = input0Id;
						num2 = input1Id;
					}
					else
					{
						num3 = input0Id;
						num = input1Id;
					}
				}
				else
				{
					num3 = input0Id;
					num2 = input1Id;
				}
			}
			else
			{
				Vector3 normalized4 = (pos3 - pos).normalized;
				Vector3 normalized5 = (pos4 - pos).normalized;
				Vector3 normalized6 = (pos5 - pos).normalized;
				float num13 = Vector3.Angle(normalized4, normalized5);
				float num14 = Vector3.Angle(normalized5, normalized6);
				float num15 = Vector3.Angle(normalized4, normalized6);
				if (num13 >= num15 && num13 >= num14)
				{
					num = input2Id;
					Vector3 rhs2 = -normalized6.normalized;
					Vector3 lhs2 = Vector3.Cross(normalized, rhs2);
					float num16 = Vector3.Dot(lhs2, normalized4);
					float num17 = Vector3.Dot(lhs2, normalized5);
					if (num16 < num17)
					{
						num2 = input0Id;
						num3 = input1Id;
					}
					else
					{
						num2 = input1Id;
						num3 = input0Id;
					}
				}
				else if (num14 >= num15 && num14 >= num13)
				{
					num = input0Id;
					Vector3 rhs3 = -normalized4.normalized;
					Vector3 lhs3 = Vector3.Cross(normalized, rhs3);
					float num18 = Vector3.Dot(lhs3, normalized5);
					float num19 = Vector3.Dot(lhs3, normalized6);
					if (num18 < num19)
					{
						num2 = input1Id;
						num3 = input2Id;
					}
					else
					{
						num2 = input2Id;
						num3 = input1Id;
					}
				}
				else
				{
					num = input1Id;
					Vector3 rhs4 = -normalized5.normalized;
					Vector3 lhs4 = Vector3.Cross(normalized, rhs4);
					float num20 = Vector3.Dot(lhs4, normalized4);
					float num21 = Vector3.Dot(lhs4, normalized6);
					if (num20 < num21)
					{
						num2 = input0Id;
						num3 = input2Id;
					}
					else
					{
						num2 = input2Id;
						num3 = input0Id;
					}
				}
			}
		}
		if (bridgeChanged)
		{
			outputId = this.beltPool[beltId].outputId;
			num3 = this.beltPool[beltId].rightInputId;
			num = this.beltPool[beltId].backInputId;
			num2 = this.beltPool[beltId].leftInputId;
		}
		int outputId2 = this.beltPool[beltId].outputId;
		this.beltPool[beltId].outputId = outputId;
		int rightInputId = this.beltPool[beltId].rightInputId;
		int backInputId = this.beltPool[beltId].backInputId;
		int leftInputId = this.beltPool[beltId].leftInputId;
		int mainInputId = this.beltPool[beltId].mainInputId;
		this.beltPool[beltId].rightInputId = num3;
		this.beltPool[beltId].backInputId = num;
		this.beltPool[beltId].leftInputId = num2;
		int mainInputId2 = this.beltPool[beltId].mainInputId;
		bool flag = outputId2 != outputId;
		bool flag2 = mainInputId != mainInputId2;
		if (flag)
		{
			if (outputId2 != 0)
			{
				BeltComponent beltComponent = this.beltPool[outputId2];
				int rightInputId2 = beltComponent.rightInputId;
				int backInputId2 = beltComponent.backInputId;
				int leftInputId2 = beltComponent.leftInputId;
				if (this._delInput(ref rightInputId2, ref backInputId2, ref leftInputId2, beltId))
				{
					this.AlterBeltConnections(beltComponent.id, beltComponent.outputId, rightInputId2, backInputId2, leftInputId2, false);
				}
			}
			if (outputId != 0)
			{
				BeltComponent beltComponent2 = this.beltPool[outputId];
				int rightInputId3 = beltComponent2.rightInputId;
				int backInputId3 = beltComponent2.backInputId;
				int leftInputId3 = beltComponent2.leftInputId;
				bool flag3 = false;
				bool flag4 = this._addInput(ref rightInputId3, ref backInputId3, ref leftInputId3, beltId, out flag3);
				Assert.False(flag3, "传送带输入接口已满");
				if (flag3)
				{
					this.beltPool[beltId].outputId = 0;
				}
				if (flag4)
				{
					this.AlterBeltConnections(outputId, (beltId == beltComponent2.outputId) ? 0 : beltComponent2.outputId, rightInputId3, backInputId3, leftInputId3, false);
				}
			}
		}
		if (flag || flag2 || this.beltPool[beltId].segPathId == 0 || bridgeChanged)
		{
			CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
			CargoContainer cargoContainer = this.factory.cargoContainer;
			if (cargoPath != null)
			{
				cargoContainer.BeginTemp();
				if (!cargoPath.closed)
				{
					this._TrafficChangeWithPathSplit(cargoPath, this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength);
				}
				else
				{
					this._TrafficChangeWithPathOpen(cargoPath, this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength);
				}
				this._TrafficChangeWithPathTrunc(cargoPath, this.beltPool[beltId].segIndex, true, true, true);
				if (cargoPath.pathLength == 0)
				{
					this.RemoveCargoPath(cargoPath.id);
				}
			}
			bool flag5 = outputId != 0 && this.beltPool[outputId].mainInputId != beltId;
			bool flag6 = outputId != 0 && this.beltPool[outputId].mainInputId == beltId;
			int newSegPivotOffset;
			int num22 = this.GeneratePathGeometry(beltId, mainInputId2, outputId, flag5, out newSegPivotOffset);
			Assert.Positive(num22);
			if (mainInputId2 != 0 && !flag2)
			{
				cargoPath = this.GetCargoPath(this.beltPool[mainInputId2].segPathId);
				Assert.NotNull(cargoPath);
				this.beltPool[beltId].SetSegment(cargoPath.id, cargoPath.pathLength, newSegPivotOffset, num22);
				cargoPath.AddRelatedBelt(beltId);
				cargoPath.AddBuffer(num22, this.beltPool[beltId].speed, this.posTmp, this.rotTmp, Vector3.zero);
			}
			else
			{
				cargoPath = this.NewCargoPath();
				this.beltPool[beltId].SetSegment(cargoPath.id, 0, newSegPivotOffset, num22);
				cargoPath.AddRelatedBelt(beltId);
				cargoPath.AddBuffer(num22, this.beltPool[beltId].speed, this.posTmp, this.rotTmp, Vector3.zero);
			}
			if (cargoContainer.tmpEnabled && cargoContainer.tmpCargoCount > 0)
			{
				cargoContainer.EndTemp();
				int num23 = this.beltPool[beltId].segIndex - 10;
				while (num23 < this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength && cargoContainer.tmpCargoCount != 0)
				{
					if (num23 < 4)
					{
						num23 = 4;
					}
					if (num23 >= cargoPath.pathLength - 5)
					{
						break;
					}
					ItemPackage itemPackage = cargoContainer.tmpCargos[cargoContainer.tmpCargoCount - 1];
					int num24 = cargoContainer.AddCargo(itemPackage.item, itemPackage.stack, (byte)itemPackage.inc, Vector3.zero, Quaternion.identity);
					if (cargoPath.TryInsertCargo(num23, num24))
					{
						cargoContainer.tmpCargoCount--;
						num23 += 9;
					}
					else
					{
						cargoContainer.RemoveCargo(num24);
					}
					num23++;
				}
				if (this._return_items)
				{
					for (int i = 0; i < cargoContainer.tmpCargoCount; i++)
					{
						ItemPackage itemPackage2 = cargoContainer.tmpCargos[i];
						if (itemPackage2.item > 0)
						{
							int upCount = GameMain.mainPlayer.TryAddItemToPackage((int)itemPackage2.item, (int)itemPackage2.stack, itemPackage2.inc, true, this.beltPool[beltId].entityId, false);
							UIItemup.Up((int)itemPackage2.item, upCount);
						}
					}
				}
			}
			cargoContainer.EndTemp();
			cargoContainer.ClearTempCargos();
			if (flag6)
			{
				CargoPath cargoPath2 = this.GetCargoPath(this.beltPool[outputId].segPathId);
				Assert.NotNull(cargoPath2);
				if (cargoPath2 != null)
				{
					if (cargoPath2 != cargoPath)
					{
						this._TrafficChangeWithPathConcat(cargoPath, cargoPath2);
					}
					else
					{
						cargoPath.PathClose();
					}
				}
			}
			else if (flag5)
			{
				CargoPath cargoPath3 = this.GetCargoPath(this.beltPool[outputId].segPathId);
				Assert.NotNull(cargoPath3);
				if (cargoPath3 != null)
				{
					int outputIndex = this.beltPool[outputId].segIndex + this.beltPool[outputId].segPivotOffset;
					cargoPath.SetOutput(cargoPath3, outputIndex);
					cargoPath3.AddRelatedInputPath(cargoPath.id);
				}
			}
			if (bridgeChanged && this.isLocalLoaded)
			{
				this.AlterBeltRenderer(beltId, entityPool, colChunks, false);
				this.AlterBeltRenderer(this.beltPool[beltId].mainInputId, entityPool, colChunks, false);
				this.AlterBeltRenderer(this.beltPool[beltId].outputId, entityPool, colChunks, false);
				this.AlterPathRenderer(this.beltPool[beltId].segPathId, false);
				this.RefreshPathUV(this.beltPool[beltId].segPathId);
				this.planet.physics.isPlanetPhysicsColliderDirty = true;
				return;
			}
		}
		if (num != 0)
		{
			BeltComponent beltComponent3 = this.beltPool[num];
			if (beltComponent3.outputId != beltId)
			{
				this.AlterBeltConnections(beltComponent3.id, beltId, beltComponent3.rightInputId, beltComponent3.backInputId, beltComponent3.leftInputId, false);
			}
		}
		if (num2 != 0)
		{
			BeltComponent beltComponent3 = this.beltPool[num2];
			if (beltComponent3.outputId != beltId)
			{
				this.AlterBeltConnections(beltComponent3.id, beltId, beltComponent3.rightInputId, beltComponent3.backInputId, beltComponent3.leftInputId, false);
			}
		}
		if (num3 != 0)
		{
			BeltComponent beltComponent3 = this.beltPool[num3];
			if (beltComponent3.outputId != beltId)
			{
				this.AlterBeltConnections(beltComponent3.id, beltId, beltComponent3.rightInputId, beltComponent3.backInputId, beltComponent3.leftInputId, false);
			}
		}
		if (rightInputId != 0 && rightInputId != num && rightInputId != num2 && rightInputId != num3)
		{
			BeltComponent beltComponent4 = this.beltPool[rightInputId];
			if (beltComponent4.outputId == beltId)
			{
				this.AlterBeltConnections(rightInputId, 0, beltComponent4.rightInputId, beltComponent4.backInputId, beltComponent4.leftInputId, false);
			}
		}
		if (backInputId != 0 && backInputId != num && backInputId != num2 && backInputId != num3)
		{
			BeltComponent beltComponent4 = this.beltPool[backInputId];
			if (beltComponent4.outputId == beltId)
			{
				this.AlterBeltConnections(backInputId, 0, beltComponent4.rightInputId, beltComponent4.backInputId, beltComponent4.leftInputId, false);
			}
		}
		if (leftInputId != 0 && leftInputId != num && leftInputId != num2 && leftInputId != num3)
		{
			BeltComponent beltComponent4 = this.beltPool[leftInputId];
			if (beltComponent4.outputId == beltId)
			{
				this.AlterBeltConnections(leftInputId, 0, beltComponent4.rightInputId, beltComponent4.backInputId, beltComponent4.leftInputId, false);
			}
		}
		if (flag2)
		{
			bool flag7 = mainInputId != 0 && (mainInputId == num || mainInputId == num3 || mainInputId == num2);
			bool flag8 = mainInputId2 != 0 && (mainInputId2 == rightInputId || mainInputId2 == backInputId || mainInputId2 == leftInputId);
			if (flag7)
			{
				Assert.True(beltId == this.beltPool[mainInputId].outputId);
				this.AlterBeltConnections(mainInputId, beltId, this.beltPool[mainInputId].rightInputId, this.beltPool[mainInputId].backInputId, this.beltPool[mainInputId].leftInputId, true);
				this.AlterBeltConnections(mainInputId, beltId, this.beltPool[mainInputId].rightInputId, this.beltPool[mainInputId].backInputId, this.beltPool[mainInputId].leftInputId, false);
			}
			if (flag8)
			{
				Assert.True(beltId == this.beltPool[mainInputId2].outputId);
				this.AlterBeltConnections(mainInputId2, beltId, this.beltPool[mainInputId2].rightInputId, this.beltPool[mainInputId2].backInputId, this.beltPool[mainInputId2].leftInputId, true);
				this.AlterBeltConnections(mainInputId2, beltId, this.beltPool[mainInputId2].rightInputId, this.beltPool[mainInputId2].backInputId, this.beltPool[mainInputId2].leftInputId, false);
			}
		}
		int outputIndex2 = this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset;
		if (num3 != 0 && num3 != mainInputId2)
		{
			CargoPath cargoPath4 = this.GetCargoPath(this.beltPool[num3].segPathId);
			CargoPath cargoPath5 = this.GetCargoPath(this.beltPool[beltId].segPathId);
			Assert.NotNull(cargoPath5);
			if (cargoPath4 != null && cargoPath4.outputPath == null)
			{
				cargoPath4.SetOutput(cargoPath5, outputIndex2);
				cargoPath5.AddRelatedInputPath(cargoPath4.id);
			}
		}
		if (num2 != 0 && num2 != mainInputId2)
		{
			CargoPath cargoPath6 = this.GetCargoPath(this.beltPool[num2].segPathId);
			CargoPath cargoPath7 = this.GetCargoPath(this.beltPool[beltId].segPathId);
			Assert.NotNull(cargoPath7);
			if (cargoPath6 != null && cargoPath6.outputPath == null)
			{
				cargoPath6.SetOutput(cargoPath7, outputIndex2);
				cargoPath7.AddRelatedInputPath(cargoPath6.id);
			}
		}
		if (this.isLocalLoaded)
		{
			this.AlterBeltRenderer(beltId, entityPool, colChunks, false);
			this.AlterBeltRenderer(this.beltPool[beltId].mainInputId, entityPool, colChunks, false);
			this.AlterBeltRenderer(this.beltPool[beltId].outputId, entityPool, colChunks, false);
			this.AlterPathRenderer(this.beltPool[beltId].segPathId, false);
			this.RefreshPathUV(this.beltPool[beltId].segPathId);
			this.planet.physics.isPlanetPhysicsColliderDirty = true;
		}
	}

	// Token: 0x06000B1F RID: 2847 RVA: 0x000A71A8 File Offset: 0x000A53A8
	private void _vecProj(ref Vector3 dir, Vector3 norm)
	{
		float num = dir.x * norm.x + dir.y * norm.y + dir.z * norm.z;
		dir.x -= num * norm.x;
		dir.y -= num * norm.y;
		dir.z -= num * norm.z;
	}

	// Token: 0x06000B20 RID: 2848 RVA: 0x000A7218 File Offset: 0x000A5418
	private void _vecProjN(ref Vector3 dir, Vector3 norm)
	{
		float num = dir.x * norm.x + dir.y * norm.y + dir.z * norm.z;
		dir.x -= num * norm.x;
		dir.y -= num * norm.y;
		dir.z -= num * norm.z;
		dir.Normalize();
	}

	// Token: 0x06000B21 RID: 2849 RVA: 0x000A728C File Offset: 0x000A548C
	private void _arrInputs(ref int i0, ref int i1, ref int i2)
	{
		if (i1 == i0)
		{
			i1 = 0;
		}
		if (i2 == i0)
		{
			i2 = 0;
		}
		if (i1 == i2)
		{
			i2 = 0;
		}
		if (i0 == 0)
		{
			if (i1 != 0)
			{
				i0 = i1;
				i1 = 0;
			}
			else if (i2 != 0)
			{
				i0 = i2;
				i2 = 0;
			}
		}
		if (i1 == 0)
		{
			i1 = i2;
			i2 = 0;
		}
	}

	// Token: 0x06000B22 RID: 2850 RVA: 0x000A72DC File Offset: 0x000A54DC
	private bool _delInput(ref int i0, ref int i1, ref int i2, int delId)
	{
		bool result = true;
		if (i0 == delId)
		{
			i0 = 0;
		}
		else if (i1 == delId)
		{
			i1 = 0;
		}
		else if (i2 == delId)
		{
			i2 = 0;
		}
		else
		{
			result = false;
		}
		return result;
	}

	// Token: 0x06000B23 RID: 2851 RVA: 0x000A7310 File Offset: 0x000A5510
	private bool _addInput(ref int i0, ref int i1, ref int i2, int addId, out bool full)
	{
		full = false;
		if (i0 == addId || i1 == addId || i2 == addId)
		{
			return false;
		}
		if (i0 == 0)
		{
			i0 = addId;
		}
		else if (i1 == 0)
		{
			i1 = addId;
		}
		else if (i2 == 0)
		{
			i2 = addId;
		}
		else
		{
			full = true;
		}
		return !full;
	}

	// Token: 0x06000B24 RID: 2852 RVA: 0x000A7360 File Offset: 0x000A5560
	private void _TrafficChangeWithPathTrunc(CargoPath path, int truncLen, bool manageBeltRelate, bool manageInputRelate, bool manageOutputRelate)
	{
		if (truncLen >= path.pathLength)
		{
			return;
		}
		if (manageBeltRelate)
		{
			for (int i = 0; i < path.belts.Count; i++)
			{
				int num = path.belts[i];
				Assert.Positive(this.beltPool[num].id);
				bool flag;
				this.beltPool[num].ManagePathTrunc(path.id, truncLen, out flag);
				if (!flag)
				{
					path.belts.RemoveAt(i);
					i--;
				}
			}
		}
		if (manageInputRelate)
		{
			for (int j = 0; j < path.inputPaths.Count; j++)
			{
				int id = path.inputPaths[j];
				CargoPath cargoPath = this.GetCargoPath(id);
				Assert.NotNull(cargoPath);
				if (cargoPath != null)
				{
					Assert.True(cargoPath.outputPath == path);
					if (cargoPath.outputPath == path)
					{
						if (cargoPath.outputIndex >= truncLen)
						{
							cargoPath.UnsetOutput();
							path.inputPaths.RemoveAt(j);
							j--;
						}
					}
					else
					{
						path.inputPaths.RemoveAt(j);
						j--;
					}
				}
				else
				{
					path.inputPaths.RemoveAt(j);
					j--;
				}
			}
		}
		if (manageOutputRelate && path.outputPath != null)
		{
			path.outputPath.RemoveRelatedInputPath(path.id);
		}
		path.TruncBuffer(truncLen);
	}

	// Token: 0x06000B25 RID: 2853 RVA: 0x000A74A8 File Offset: 0x000A56A8
	private CargoPath _TrafficChangeWithPathSplit(CargoPath oldPath, int splitPosition)
	{
		if (splitPosition < oldPath.pathLength)
		{
			CargoPath cargoPath = this.NewCargoPath();
			cargoPath.PathCopy(oldPath, splitPosition);
			if (cargoPath.outputPath != null)
			{
				cargoPath.outputPath.AddRelatedInputPath(cargoPath.id);
			}
			for (int i = 0; i < oldPath.belts.Count; i++)
			{
				int num = oldPath.belts[i];
				Assert.Positive(this.beltPool[num].id);
				bool flag;
				bool flag2;
				this.beltPool[num].ManagePathSplit(oldPath.id, cargoPath.id, splitPosition, out flag, out flag2);
				if (!flag)
				{
					oldPath.belts.RemoveAt(i);
					i--;
				}
				if (flag2)
				{
					cargoPath.AddRelatedBelt(num);
				}
			}
			for (int j = 0; j < oldPath.inputPaths.Count; j++)
			{
				int num2 = oldPath.inputPaths[j];
				CargoPath cargoPath2 = this.GetCargoPath(num2);
				Assert.NotNull(cargoPath2);
				if (cargoPath2 != null)
				{
					Assert.True(cargoPath2.outputPath == oldPath);
					if (cargoPath2.outputPath == oldPath)
					{
						if (cargoPath2.outputIndex >= splitPosition)
						{
							cargoPath2.outputPath = cargoPath;
							cargoPath2.outputIndex -= splitPosition;
							cargoPath.AddRelatedInputPath(num2);
							oldPath.inputPaths.RemoveAt(j);
							j--;
						}
					}
					else
					{
						oldPath.inputPaths.RemoveAt(j);
						j--;
					}
				}
				else
				{
					oldPath.inputPaths.RemoveAt(j);
					j--;
				}
			}
			this._TrafficChangeWithPathTrunc(oldPath, splitPosition, false, false, true);
			if (oldPath.pathLength == 0)
			{
				this.RemoveCargoPath(oldPath.id);
			}
			return cargoPath;
		}
		return null;
	}

	// Token: 0x06000B26 RID: 2854 RVA: 0x000A7648 File Offset: 0x000A5848
	private void _TrafficChangeWithPathConcat(CargoPath mainPath, CargoPath concatPath)
	{
		if (concatPath.pathLength > 0)
		{
			for (int i = 0; i < concatPath.belts.Count; i++)
			{
				int num = concatPath.belts[i];
				Assert.Positive(this.beltPool[num].id);
				bool flag;
				this.beltPool[num].ManagePathConcat(mainPath.id, concatPath.id, mainPath.pathLength, out flag);
				Assert.True(flag);
				if (flag)
				{
					mainPath.AddRelatedBelt(num);
				}
			}
			for (int j = 0; j < concatPath.inputPaths.Count; j++)
			{
				int num2 = concatPath.inputPaths[j];
				CargoPath cargoPath = this.GetCargoPath(num2);
				Assert.NotNull(cargoPath);
				if (cargoPath != null)
				{
					Assert.True(cargoPath.outputPath == concatPath);
					if (cargoPath.outputPath == concatPath)
					{
						cargoPath.outputPath = mainPath;
						cargoPath.outputIndex += mainPath.pathLength;
						mainPath.AddRelatedInputPath(num2);
					}
				}
			}
			if (concatPath.outputPath != null)
			{
				concatPath.outputPath.RemoveRelatedInputPath(concatPath.id);
			}
			if (mainPath.outputPath != null)
			{
				mainPath.outputPath.RemoveRelatedInputPath(mainPath.id);
			}
			mainPath.PathConcat(concatPath);
			if (mainPath.outputPath != null)
			{
				mainPath.outputPath.AddRelatedInputPath(mainPath.id);
			}
			this.RemoveCargoPath(concatPath.id);
		}
	}

	// Token: 0x06000B27 RID: 2855 RVA: 0x000A77A4 File Offset: 0x000A59A4
	private void _TrafficChangeWithPathOpen(CargoPath path, int openIdx)
	{
		path.PathOpen(openIdx);
		int pathLength = path.pathLength;
		for (int i = 0; i < path.belts.Count; i++)
		{
			int num = path.belts[i];
			Assert.Positive(this.beltPool[num].id);
			this.beltPool[num].ManagePathOpen(path.id, pathLength, openIdx);
		}
		for (int j = 0; j < path.inputPaths.Count; j++)
		{
			int id = path.inputPaths[j];
			CargoPath cargoPath = this.GetCargoPath(id);
			Assert.NotNull(cargoPath);
			if (cargoPath != null)
			{
				Assert.True(cargoPath.outputPath == path);
				if (cargoPath.outputPath == path)
				{
					cargoPath.outputIndex -= openIdx;
					if (cargoPath.outputIndex < 0)
					{
						cargoPath.outputIndex += pathLength;
					}
					Assert.False(cargoPath.outputIndex < 4);
					Assert.False(cargoPath.outputIndex >= pathLength - 5);
				}
				else
				{
					path.inputPaths.RemoveAt(j);
					j--;
				}
			}
			else
			{
				path.inputPaths.RemoveAt(j);
				j--;
			}
		}
	}

	// Token: 0x06000B28 RID: 2856 RVA: 0x000A78D8 File Offset: 0x000A5AD8
	private int GeneratePathGeometry(int beltId, int prevId, int nextId, bool ext, out int pivot)
	{
		pivot = 0;
		Array.Clear(this.posTmp, 0, this.posTmp.Length);
		Array.Clear(this.rollTmp, 0, this.rollTmp.Length);
		Array.Clear(this.rotTmp, 0, this.rotTmp.Length);
		Assert.True(beltId == this.beltPool[beltId].id);
		Assert.True(prevId == this.beltPool[prevId].id);
		Assert.True(nextId == this.beltPool[nextId].id);
		if (beltId == 0 || beltId != this.beltPool[beltId].id)
		{
			return 0;
		}
		if (prevId != this.beltPool[prevId].id)
		{
			prevId = 0;
		}
		if (nextId != this.beltPool[nextId].id)
		{
			nextId = 0;
		}
		Vector3 ext2 = Vector3.zero;
		Quaternion extrot = Quaternion.identity;
		bool extdot = false;
		if (nextId != 0)
		{
			CargoPath cargoPath = this.GetCargoPath(this.beltPool[nextId].segPathId);
			if (cargoPath != null)
			{
				ext2 = cargoPath.pointPos[this.beltPool[nextId].segIndex + this.beltPool[nextId].segPivotOffset];
				extrot = cargoPath.pointRot[this.beltPool[nextId].segIndex + this.beltPool[nextId].segPivotOffset];
				extdot = (cargoPath.belts.Count <= 1);
			}
			else
			{
				ext2 = this.factory.entityPool[this.beltPool[nextId].entityId].pos;
				extrot = this.factory.entityPool[this.beltPool[nextId].entityId].rot;
			}
		}
		return this.GetBezierArc(this.factory.entityPool[this.beltPool[beltId].entityId].pos, this.factory.entityPool[this.beltPool[prevId].entityId].pos, this.factory.entityPool[this.beltPool[nextId].entityId].pos, this.factory.entityPool[this.beltPool[beltId].entityId].tilt, this.factory.entityPool[this.beltPool[prevId].entityId].tilt, this.factory.entityPool[this.beltPool[nextId].entityId].tilt, ext2, extrot, extdot, prevId != 0, nextId != 0, ext, out pivot);
	}

	// Token: 0x06000B29 RID: 2857 RVA: 0x000A7BA0 File Offset: 0x000A5DA0
	public int GetBezierArc(Vector3 center, Vector3 prev, Vector3 next, float tiltCenter, float tiltPrev, float tiltNext, Vector3 ext, Quaternion extrot, bool extdot, bool hasPrev, bool hasNext, bool hasExt, out int pivot)
	{
		if (!hasPrev)
		{
			prev = center;
			tiltPrev = tiltCenter;
		}
		if (!hasNext)
		{
			next = center;
			tiltNext = tiltCenter;
		}
		if (hasExt)
		{
			tiltNext = tiltCenter;
		}
		float num = tiltPrev - tiltCenter;
		if (num > 180f)
		{
			tiltPrev -= 360f;
		}
		else if (num < -180f)
		{
			tiltPrev += 360f;
		}
		float num2 = tiltNext - tiltCenter;
		if (num2 > 180f)
		{
			tiltNext -= 360f;
		}
		else if (num2 < -180f)
		{
			tiltNext += 360f;
		}
		if (!hasPrev)
		{
			Vector3 vector = center - next;
			Vector3 b = Vector3.Dot(center.normalized, vector) * center.normalized;
			vector -= b;
			prev = center + vector;
		}
		if (!hasNext)
		{
			Vector3 vector2 = center - prev;
			Vector3 b2 = Vector3.Dot(center.normalized, vector2) * center.normalized;
			vector2 -= b2;
			next = center + vector2;
		}
		Vector3 a = center - prev;
		Vector3 to = next - center;
		next - prev;
		float num3 = (tiltCenter + tiltPrev) / 2f;
		float num4 = (tiltCenter + tiltNext) / 2f;
		float num5 = Vector3.Angle(-a, to);
		float num6 = Mathf.Lerp(0.55192f, 0.66667f, (num5 - 90f) / 90f);
		Vector3 vector3 = (prev + center) * 0.5f;
		Vector3 vector4 = (next + center) * 0.5f;
		Vector3 vector5 = vector3 * (1f - num6) + center * num6;
		Vector3 vector6 = vector4 * (1f - num6) + center * num6;
		Vector3 b3 = new Vector3(0.125f * vector3.x + 0.375f * vector5.x + 0.375f * vector6.x + 0.125f * vector4.x, 0.125f * vector3.y + 0.375f * vector5.y + 0.375f * vector6.y + 0.125f * vector4.y, 0.125f * vector3.z + 0.375f * vector5.z + 0.375f * vector6.z + 0.125f * vector4.z);
		int num7 = 4;
		int num8 = 5;
		if (hasPrev)
		{
			num7 = Mathf.RoundToInt(Vector3.Distance(vector3, b3) / 0.06f - 0.3f);
		}
		if (hasNext)
		{
			num8 = Mathf.RoundToInt(Vector3.Distance(vector4, b3) / 0.06f - 0.3f);
		}
		if (num7 > 170)
		{
			num7 = 170;
		}
		if (num8 > 170)
		{
			num8 = 170;
		}
		int num9 = num7;
		this.posTmp[num9].x = 0.125f * vector3.x + 0.375f * vector5.x + 0.375f * vector6.x + 0.125f * vector4.x;
		this.posTmp[num9].y = 0.125f * vector3.y + 0.375f * vector5.y + 0.375f * vector6.y + 0.125f * vector4.y;
		this.posTmp[num9].z = 0.125f * vector3.z + 0.375f * vector5.z + 0.375f * vector6.z + 0.125f * vector4.z;
		this.rollTmp[num9] = tiltCenter;
		pivot = num9;
		for (int i = 1; i <= num7; i++)
		{
			float num10 = 0.5f - (float)i * 0.5f / ((float)num7 + 0.5f);
			float num11 = 1f - num10;
			this.posTmp[num9 - i].x = num11 * num11 * num11 * vector3.x + 3f * num11 * num11 * num10 * vector5.x + 3f * num11 * num10 * num10 * vector6.x + num10 * num10 * num10 * vector4.x;
			this.posTmp[num9 - i].y = num11 * num11 * num11 * vector3.y + 3f * num11 * num11 * num10 * vector5.y + 3f * num11 * num10 * num10 * vector6.y + num10 * num10 * num10 * vector4.y;
			this.posTmp[num9 - i].z = num11 * num11 * num11 * vector3.z + 3f * num11 * num11 * num10 * vector5.z + 3f * num11 * num10 * num10 * vector6.z + num10 * num10 * num10 * vector4.z;
			float num12 = 1f - (float)i / ((float)num7 + 0.5f);
			this.rollTmp[num9 - i] = tiltCenter * num12 + num3 * (1f - num12);
		}
		for (int j = 1; j <= num8; j++)
		{
			float num13 = 0.5f + (float)j * 0.5f / ((float)num8 + 0.5f);
			float num14 = 1f - num13;
			this.posTmp[num9 + j].x = num14 * num14 * num14 * vector3.x + 3f * num14 * num14 * num13 * vector5.x + 3f * num14 * num13 * num13 * vector6.x + num13 * num13 * num13 * vector4.x;
			this.posTmp[num9 + j].y = num14 * num14 * num14 * vector3.y + 3f * num14 * num14 * num13 * vector5.y + 3f * num14 * num13 * num13 * vector6.y + num13 * num13 * num13 * vector4.y;
			this.posTmp[num9 + j].z = num14 * num14 * num14 * vector3.z + 3f * num14 * num14 * num13 * vector5.z + 3f * num14 * num13 * num13 * vector6.z + num13 * num13 * num13 * vector4.z;
			float num15 = 1f - (float)j / ((float)num8 + 0.5f);
			this.rollTmp[num9 + j] = tiltCenter * num15 + num4 * (1f - num15);
		}
		int num16 = num7 + 1 + num8;
		if (hasExt && hasNext)
		{
			ext -= ext.normalized * 0.15f;
			float magnitude = (ext - vector4).magnitude;
			Vector3 normalized = (ext - vector4).normalized;
			int num17 = Mathf.RoundToInt(magnitude / 0.06f);
			if (num17 > 170)
			{
				num17 = 170;
			}
			if (num17 > 0)
			{
				float num18 = magnitude / (float)num17;
				int num19 = num17 - 5;
				if (num19 < 1)
				{
					num19 = 1;
				}
				Vector3 vector7 = vector4;
				Vector3 vector8 = vector7 + (float)num19 * num18 * normalized;
				float d = Vector3.Distance(vector7, vector8) / 3f;
				Vector3 vector9 = extrot.Right();
				vector9 *= Mathf.Sign(Vector3.Dot(vector7 - vector8, vector9));
				Vector3 vector10 = vector7 + (vector4 - center).normalized * d;
				Vector3 vector11 = extdot ? (vector10 + (vector4 - center).normalized * d) : (vector8 + vector9 * d);
				Vector3 vector12 = Vector3.Cross(Vector3.Cross(normalized, vector7), normalized).normalized;
				vector12 = Quaternion.AngleAxis(tiltCenter, normalized) * vector12;
				float num20 = 1f - Mathf.Abs(Vector3.Dot(vector12, vector7.normalized));
				vector11 = (1f - num20) * vector11 + num20 * (vector7 + Vector3.Dot((vector8 - vector7).normalized, vector11 - vector7) * (vector8 - vector7).normalized);
				for (int k = 0; k < num19; k++)
				{
					float num21 = (float)(k + 1) / (float)num19;
					float num22 = 1f - num21;
					this.posTmp[num16].x = num22 * num22 * num22 * vector7.x + 3f * num22 * num22 * num21 * vector10.x + 3f * num22 * num21 * num21 * vector11.x + num21 * num21 * num21 * vector8.x;
					this.posTmp[num16].y = num22 * num22 * num22 * vector7.y + 3f * num22 * num22 * num21 * vector10.y + 3f * num22 * num21 * num21 * vector11.y + num21 * num21 * num21 * vector8.y;
					this.posTmp[num16].z = num22 * num22 * num22 * vector7.z + 3f * num22 * num22 * num21 * vector10.z + 3f * num22 * num21 * num21 * vector11.z + num21 * num21 * num21 * vector8.z;
					this.rollTmp[num16] = num4;
					num16++;
				}
				for (int l = 0; l < 5; l++)
				{
					this.posTmp[num16] = this.posTmp[num16 - 1];
					this.rollTmp[num16] = this.rollTmp[num16 - 1];
					num16++;
				}
			}
		}
		for (int m = 0; m < num16; m++)
		{
			Vector3 b4 = (m == 0) ? vector3 : this.posTmp[m - 1];
			Vector3 normalized2 = (((m == num16 - 1) ? ((hasExt && hasNext) ? ext : vector4) : this.posTmp[m + 1]) - b4).normalized;
			Vector3 normalized3 = this.posTmp[m].normalized;
			if (normalized2.sqrMagnitude < 0.5f)
			{
				this.rotTmp[m] = Quaternion.FromToRotation(Vector3.up, normalized3) * Quaternion.Euler(0f, 0f, this.rollTmp[m]);
			}
			else
			{
				this.rotTmp[m] = Quaternion.LookRotation(normalized2, normalized3) * Quaternion.Euler(0f, 0f, this.rollTmp[m]);
			}
		}
		for (int n = 0; n < num16; n++)
		{
			Vector3 normalized4 = this.posTmp[n].normalized;
			Vector3[] array = this.posTmp;
			int num23 = n;
			array[num23].x = array[num23].x + normalized4.x * 0.15f;
			Vector3[] array2 = this.posTmp;
			int num24 = n;
			array2[num24].y = array2[num24].y + normalized4.y * 0.15f;
			Vector3[] array3 = this.posTmp;
			int num25 = n;
			array3[num25].z = array3[num25].z + normalized4.z * 0.15f;
		}
		return num16;
	}

	// Token: 0x06000B2A RID: 2858 RVA: 0x000A87A8 File Offset: 0x000A69A8
	private float calc_roll(Vector3 pos, Quaternion rot, Vector3 dir)
	{
		if ((double)dir.sqrMagnitude < 0.0001)
		{
			dir = Vector3.Cross(dir, Vector3.up).normalized;
		}
		if ((double)dir.sqrMagnitude < 0.0001)
		{
			dir = Vector3.right * Mathf.Sign(pos.y);
		}
		dir.Normalize();
		pos.Normalize();
		Vector3 normalized = Vector3.Cross(pos, dir).normalized;
		Vector3 normalized2 = Vector3.Cross(rot.Up(), dir).normalized;
		return Vector3.SignedAngle(normalized, normalized2, dir);
	}

	// Token: 0x06000B2B RID: 2859 RVA: 0x000A8840 File Offset: 0x000A6A40
	public void DebugGUI()
	{
		DGUI.Pool("Belt Components", this.beltPool, this.beltCapacity, this.beltCursor, this.beltRecycle, 0, this.beltRecycleCursor);
		DGUI.Pool("Cargo Paths", this.pathPool, this.pathCapacity, this.pathCursor, this.pathRecycle, 0, this.pathRecycleCursor);
	}

	// Token: 0x06000B2C RID: 2860 RVA: 0x000A88A0 File Offset: 0x000A6AA0
	public void DebugPathCurves()
	{
		for (int i = 1; i < this.pathCursor; i++)
		{
			CargoPath cargoPath = this.GetCargoPath(i);
			if (cargoPath != null)
			{
				cargoPath.DrawDebugLine();
			}
		}
	}

	// Token: 0x06000B2D RID: 2861 RVA: 0x000A88D0 File Offset: 0x000A6AD0
	public void DebugPathCurve(int pathId)
	{
		CargoPath cargoPath = this.GetCargoPath(pathId);
		if (cargoPath != null)
		{
			cargoPath.DrawDebugLine();
		}
	}

	// Token: 0x06000B2E RID: 2862 RVA: 0x000A88F0 File Offset: 0x000A6AF0
	public void CreateRenderingBatches()
	{
		this._batch_buffer_no_refresh = true;
		if (this.beltRenderingBatch != null || this.pathRenderingBatch != null)
		{
			this.DestroyRenderingBatches();
		}
		this.beltRenderingBatch = new BeltRenderingBatch[12];
		for (int i = 0; i < 12; i++)
		{
			int num = i % 4;
			num = Mathf.RoundToInt(Mathf.Pow(2f, (float)num) + 1f);
			this.beltRenderingBatch[i] = new BeltRenderingBatch(i, num);
		}
		EntityData[] entityPool = this.factory.entityPool;
		ColliderContainer[] colChunks = (this.planet.physics == null) ? null : this.planet.physics.colChunks;
		for (int j = 1; j < this.beltCursor; j++)
		{
			if (this.beltPool[j].id == j)
			{
				this.AlterBeltRenderer(j, entityPool, colChunks, true);
			}
		}
		this.RefreshBeltBatchesBuffers();
		this.pathRenderingBatch = new PathRenderingBatch[2];
		for (int k = 0; k < 2; k++)
		{
			int num2 = k % 4;
			num2 = Mathf.RoundToInt(Mathf.Pow(2f, (float)num2) + 1f);
			this.pathRenderingBatch[k] = new PathRenderingBatch(k);
		}
		for (int l = 1; l < this.pathCursor; l++)
		{
			if (this.pathPool[l] != null && this.pathPool[l].id == l)
			{
				this.AlterPathRenderer(l, true);
			}
		}
		this.RefreshPathBatchesBuffers();
		this.planet.physics.isPlanetPhysicsColliderDirty = true;
		this._batch_buffer_no_refresh = false;
	}

	// Token: 0x06000B2F RID: 2863 RVA: 0x000A8A70 File Offset: 0x000A6C70
	public void DestroyRenderingBatches()
	{
		if (this.beltRenderingBatch != null)
		{
			for (int i = 0; i < 12; i++)
			{
				this.beltRenderingBatch[i].Free();
				this.beltRenderingBatch[i] = null;
			}
			this.beltRenderingBatch = null;
		}
		for (int j = 1; j < this.beltCursor; j++)
		{
			this.beltPool[j].modelBatchIndex = 0;
			this.beltPool[j].modelIndex = 0;
		}
		if (this.pathRenderingBatch != null)
		{
			for (int k = 0; k < 2; k++)
			{
				this.pathRenderingBatch[k].Free();
				this.pathRenderingBatch[k] = null;
			}
			this.pathRenderingBatch = null;
		}
		for (int l = 1; l < this.pathCursor; l++)
		{
			if (this.pathPool[l] != null)
			{
				this.pathPool[l].modelBatchIndex = 0;
				this.pathPool[l].modelIndex = 0;
			}
		}
	}

	// Token: 0x06000B30 RID: 2864 RVA: 0x000A8B4C File Offset: 0x000A6D4C
	public void AlterBeltRenderer(int beltId, EntityData[] entityPool, ColliderContainer[] colChunks, bool isFacting = false)
	{
		if (!this.planet.factoryLoaded && !isFacting && (!this.planet.factoryLoading || this.planet.factingCompletedStage < 8))
		{
			return;
		}
		if (beltId == 0)
		{
			return;
		}
		this.RemoveBeltRenderer(beltId);
		CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
		if (cargoPath == null)
		{
			return;
		}
		BeltComponent beltComponent = this.beltPool[beltId];
		double num = (double)((float)beltComponent.segIndex - 0.5f);
		double num2 = (double)((float)(beltComponent.segIndex + beltComponent.segLength) - 0.5f);
		if (cargoPath.closed)
		{
			if (num < 4.0)
			{
				num = 4.0;
			}
			if (num2 + 9.0 + 1.0 >= (double)cargoPath.pathLength)
			{
				num2 = (double)(cargoPath.pathLength - 5);
			}
		}
		else
		{
			if (num < 4.0)
			{
				num = 4.0;
			}
			if (num2 + 5.0 >= (double)cargoPath.pathLength)
			{
				num2 = (double)(cargoPath.pathLength - 5 - 1);
			}
		}
		int num3 = 1;
		int num4 = 0;
		if (beltComponent.mainInputId > 0 && beltComponent.outputId > 0)
		{
			ref EntityData ptr = ref entityPool[this.beltPool[beltComponent.mainInputId].entityId];
			int entityId = beltComponent.entityId;
			ref EntityData ptr2 = ref entityPool[this.beltPool[beltComponent.outputId].entityId];
			Vector3 pos = ptr.pos;
			Vector3 pos2 = entityPool[entityId].pos;
			Vector3 pos3 = ptr2.pos;
			float num5 = entityPool[entityId].tilt - ptr.tilt;
			float num6 = entityPool[entityId].tilt - ptr2.tilt;
			if (num5 < 0f)
			{
				num5 = -num5;
			}
			if (num6 < 0f)
			{
				num6 = -num6;
			}
			if (num5 > 180f)
			{
				num5 = 360f - num5;
			}
			if (num6 > 180f)
			{
				num6 = 360f - num6;
			}
			float num7 = Vector3.Angle(pos - pos2, pos3 - pos2);
			num7 -= (num5 + num6) * 1.7f;
			if (num7 > 170f)
			{
				num3 = 1;
				num4 = 0;
			}
			else if (num7 > 150f)
			{
				num3 = 2;
				num4 = 1;
			}
			else if (num7 > 120f)
			{
				num3 = 4;
				num4 = 2;
			}
			else
			{
				num3 = 8;
				num4 = 3;
			}
			if (beltComponent.segIndex + beltComponent.segLength == cargoPath.pathLength && cargoPath.outputPath != null)
			{
				num3 = 8;
				num4 = 3;
			}
		}
		else if (beltComponent.mainInputId > 0)
		{
			ref EntityData ptr3 = ref entityPool[this.beltPool[beltComponent.mainInputId].entityId];
			float num8 = entityPool[beltComponent.entityId].tilt - ptr3.tilt;
			if (num8 < 0f)
			{
				num8 = -num8;
			}
			if (num8 > 180f)
			{
				num8 = 360f - num8;
			}
			float num9 = num8 * 2.1f;
			if (num9 < 10f)
			{
				num3 = 1;
				num4 = 0;
			}
			else if (num9 < 30f)
			{
				num3 = 2;
				num4 = 1;
			}
			else if (num9 < 60f)
			{
				num3 = 4;
				num4 = 2;
			}
			else
			{
				num3 = 8;
				num4 = 3;
			}
		}
		else if (beltComponent.outputId > 0)
		{
			int entityId2 = beltComponent.entityId;
			ref EntityData ptr4 = ref entityPool[this.beltPool[beltComponent.outputId].entityId];
			float num10 = entityPool[entityId2].tilt - ptr4.tilt;
			if (num10 < 0f)
			{
				num10 = -num10;
			}
			if (num10 > 180f)
			{
				num10 = 360f - num10;
			}
			float num11 = num10 * 2.1f;
			if (num11 < 10f)
			{
				num3 = 1;
				num4 = 0;
			}
			else if (num11 < 30f)
			{
				num3 = 2;
				num4 = 1;
			}
			else if (num11 < 60f)
			{
				num3 = 4;
				num4 = 2;
			}
			else
			{
				num3 = 8;
				num4 = 3;
			}
		}
		if (beltComponent.speed <= 1)
		{
			num4 = num4;
		}
		else if (beltComponent.speed == 2)
		{
			num4 += 4;
		}
		else
		{
			num4 += 8;
		}
		Array.Clear(this.tmpBeltAnchors, 0, this.tmpBeltAnchors.Length);
		for (int i = 0; i <= num3; i++)
		{
			double num12 = num + (num2 - num) / (double)num3 * (double)i;
			int num13 = (int)(Math.Floor(num12) + 1E-06);
			double num14 = num12 - (double)num13;
			int num15 = (num14 < 1E-05) ? num13 : (num13 + 1);
			this.tmpBeltAnchors[i].t = (float)num12;
			if (num15 == num13)
			{
				this.tmpBeltAnchors[i].pos = cargoPath.pointPos[num13];
				this.tmpBeltAnchors[i].rot = cargoPath.pointRot[num13];
			}
			else
			{
				this.tmpBeltAnchors[i].pos = Vector3.Lerp(cargoPath.pointPos[num13], cargoPath.pointPos[num15], (float)num14);
				this.tmpBeltAnchors[i].rot = Quaternion.Slerp(cargoPath.pointRot[num13], cargoPath.pointRot[num15], (float)num14);
			}
		}
		this.beltPool[beltId].modelBatchIndex = num4 + 1;
		if (this._batch_buffer_no_refresh)
		{
			this.beltPool[beltId].modelIndex = this.beltRenderingBatch[num4].AddNodeNoRefresh(this.tmpBeltAnchors);
		}
		else
		{
			this.beltPool[beltId].modelIndex = this.beltRenderingBatch[num4].AddNode(this.tmpBeltAnchors);
		}
		int segIndex = beltComponent.segIndex;
		int num16 = beltComponent.segIndex + beltComponent.segLength - 1;
		if (colChunks != null)
		{
			int num17 = entityPool[beltComponent.entityId].colliderId;
			if (num17 > 0)
			{
				Vector3 vector;
				vector.x = (cargoPath.pointPos[segIndex].x + cargoPath.pointPos[num16].x) * 0.50016f;
				vector.y = (cargoPath.pointPos[segIndex].y + cargoPath.pointPos[num16].y) * 0.50016f;
				vector.z = (cargoPath.pointPos[segIndex].z + cargoPath.pointPos[num16].z) * 0.50016f;
				Vector3 vector2;
				vector2.x = cargoPath.pointPos[num16].x - cargoPath.pointPos[segIndex].x;
				vector2.y = cargoPath.pointPos[num16].y - cargoPath.pointPos[segIndex].y;
				vector2.z = cargoPath.pointPos[num16].z - cargoPath.pointPos[segIndex].z;
				float num18 = Mathf.Sqrt(vector2.x * vector2.x + vector2.y * vector2.y + vector2.z * vector2.z);
				int num19 = num17 >> 20;
				num17 &= 1048575;
				ColliderData[] colliderPool = colChunks[num19].colliderPool;
				colliderPool[num17].pos = vector;
				if (num18 > 0.6f)
				{
					colliderPool[num17].q = (entityPool[beltComponent.entityId].rot = Quaternion.LookRotation(vector2, vector));
					colliderPool[num17].ext.z = num18 * 0.5f;
					return;
				}
				colliderPool[num17].q = (entityPool[beltComponent.entityId].rot = Quaternion.LookRotation(vector) * Quaternion.Euler(90f, 0f, 0f));
				colliderPool[num17].ext.z = 0.3f;
			}
		}
	}

	// Token: 0x06000B31 RID: 2865 RVA: 0x000A9358 File Offset: 0x000A7558
	public void RemoveBeltRenderer(int beltId)
	{
		if (!this.isLocal)
		{
			return;
		}
		if (beltId == 0)
		{
			return;
		}
		if (this.beltPool[beltId].modelBatchIndex > 0)
		{
			this.beltRenderingBatch[this.beltPool[beltId].modelBatchIndex - 1].RemoveNode(this.beltPool[beltId].modelIndex);
		}
		this.beltPool[beltId].modelBatchIndex = 0;
		this.beltPool[beltId].modelIndex = 0;
	}

	// Token: 0x06000B32 RID: 2866 RVA: 0x000A93DC File Offset: 0x000A75DC
	public void RefreshPathUV(int pathId)
	{
		if (!this.isLocal)
		{
			return;
		}
		CargoPath cargoPath = this.GetCargoPath(pathId);
		if (cargoPath != null)
		{
			for (int i = 0; i < cargoPath.belts.Count; i++)
			{
				int num = cargoPath.belts[i];
				if (num != 0)
				{
					BeltComponent beltComponent = this.beltPool[num];
					if (beltComponent.id == num && beltComponent.modelBatchIndex != 0)
					{
						double num2 = (double)((float)beltComponent.segIndex - 0.5f);
						double num3 = (double)((float)(beltComponent.segIndex + beltComponent.segLength) - 0.5f);
						if (cargoPath.closed)
						{
							if (num2 < 4.0)
							{
								num2 = 4.0;
							}
							if (num3 + 9.0 + 1.0 >= (double)cargoPath.pathLength)
							{
								num3 = (double)(cargoPath.pathLength - 5);
							}
						}
						else
						{
							if (num2 < 4.0)
							{
								num2 = 4.0;
							}
							if (num3 + 5.0 >= (double)cargoPath.pathLength)
							{
								num3 = (double)(cargoPath.pathLength - 5 - 1);
							}
						}
						int num4 = this.beltRenderingBatch[beltComponent.modelBatchIndex - 1].nodeWidth - 1;
						for (int j = 0; j <= num4; j++)
						{
							double num5 = num2 + (num3 - num2) / (double)num4 * (double)j;
							this.beltRenderingBatch[beltComponent.modelBatchIndex - 1].nodePool[beltComponent.modelIndex + j].t = (float)num5;
						}
					}
				}
			}
			this.RefreshBeltBatchesBuffers();
		}
	}

	// Token: 0x06000B33 RID: 2867 RVA: 0x000A956C File Offset: 0x000A776C
	public void RefreshBeltBatchesBuffers()
	{
		for (int i = 0; i < this.beltRenderingBatch.Length; i++)
		{
			if (this.beltRenderingBatch[i] != null)
			{
				this.beltRenderingBatch[i].RefreshData();
			}
		}
	}

	// Token: 0x06000B34 RID: 2868 RVA: 0x000A95A4 File Offset: 0x000A77A4
	public void RefreshPathBatchesBuffers()
	{
		for (int i = 0; i < this.pathRenderingBatch.Length; i++)
		{
			if (this.pathRenderingBatch[i] != null)
			{
				this.pathRenderingBatch[i].RefreshData();
			}
		}
	}

	// Token: 0x06000B35 RID: 2869 RVA: 0x000A95DC File Offset: 0x000A77DC
	public void AlterPathRenderer(int pathId, bool isFacting = false)
	{
		if (!this.planet.factoryLoaded && !isFacting && (!this.planet.factoryLoading || this.planet.factingCompletedStage < 8))
		{
			return;
		}
		CargoPath cargoPath = this.GetCargoPath(pathId);
		if (cargoPath == null)
		{
			return;
		}
		this.RemovePathRenderer(pathId);
		int num = -1;
		Vector3 pos = Vector3.zero;
		Quaternion rot = Quaternion.identity;
		int num2 = 0;
		int num3 = 4;
		int num4 = cargoPath.pathLength - 5 - 1;
		if (!cargoPath.closed)
		{
			num = ((cargoPath.pathLength <= 10) ? 0 : 1);
			int next = 0;
			pos = cargoPath.pointPos[num3];
			rot = cargoPath.pointRot[num3] * Quaternion.Euler(0f, 180f, 0f);
			if (this._batch_buffer_no_refresh)
			{
				num2 = this.pathRenderingBatch[num].AddNodeNoRefresh(pathId, next, pos, rot, cargoPath.headSpeed);
			}
			else
			{
				num2 = this.pathRenderingBatch[num].AddNode(pathId, next, pos, rot, cargoPath.headSpeed);
			}
			if (cargoPath.outputPath == null)
			{
				num = ((cargoPath.pathLength <= 10) ? 0 : 1);
				next = (num + 1) * 10000000 + num2;
				pos = cargoPath.pointPos[num4];
				rot = cargoPath.pointRot[num4];
				if (this._batch_buffer_no_refresh)
				{
					num2 = this.pathRenderingBatch[num].AddNodeNoRefresh(pathId, next, pos, rot, cargoPath.rearSpeed);
				}
				else
				{
					num2 = this.pathRenderingBatch[num].AddNode(pathId, next, pos, rot, cargoPath.rearSpeed);
				}
			}
		}
		cargoPath.modelBatchIndex = num + 1;
		cargoPath.modelIndex = num2;
	}

	// Token: 0x06000B36 RID: 2870 RVA: 0x000A976C File Offset: 0x000A796C
	public void RemovePathRenderer(int pathId)
	{
		if (!this.isLocal)
		{
			return;
		}
		CargoPath cargoPath = this.GetCargoPath(pathId);
		if (cargoPath == null)
		{
			return;
		}
		int num = cargoPath.modelBatchIndex - 1;
		int num2 = cargoPath.modelIndex;
		int num3 = 0;
		while (num >= 0 && num2 >= 0 && num3++ < 100000)
		{
			int next = this.pathRenderingBatch[num].nodePool[num2].next;
			this.pathRenderingBatch[num].RemoveNode(num2);
			num = next / 10000000 - 1;
			num2 = next % 10000000;
		}
		cargoPath.modelBatchIndex = 0;
		cargoPath.modelIndex = 0;
	}

	// Token: 0x06000B37 RID: 2871 RVA: 0x000A97FC File Offset: 0x000A79FC
	public void ClearStates()
	{
		for (int i = 0; i < 12; i++)
		{
			this.beltRenderingBatch[i].ClearStates();
		}
		for (int j = 0; j < 2; j++)
		{
			this.pathRenderingBatch[j].ClearStates();
		}
	}

	// Token: 0x06000B38 RID: 2872 RVA: 0x000A983C File Offset: 0x000A7A3C
	public void SetBeltSelected(int beltId)
	{
		this.SetBeltState(beltId, this.beltPool[beltId].speed);
	}

	// Token: 0x06000B39 RID: 2873 RVA: 0x000A9858 File Offset: 0x000A7A58
	public void SetBeltState(int beltId, int state)
	{
		if (this.beltPool[beltId].id != 0 && this.beltPool[beltId].id == beltId)
		{
			BeltRenderingBatch beltRenderingBatch = this.beltRenderingBatch[this.beltPool[beltId].modelBatchIndex - 1];
			int num = beltRenderingBatch.nodeWidth - 1;
			for (int i = 0; i <= num; i++)
			{
				beltRenderingBatch.statePool[this.beltPool[beltId].modelIndex + i] = (float)state;
			}
			CargoPath cargoPath = this.GetCargoPath(this.beltPool[beltId].segPathId);
			if (cargoPath == null || cargoPath.closed)
			{
				return;
			}
			bool flag = this.beltPool[beltId].segIndex == 0;
			bool flag2 = this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength == cargoPath.pathLength;
			int num2 = this.beltPool[beltId].segIndex + this.beltPool[beltId].segPivotOffset;
			if (!flag && !flag2)
			{
				return;
			}
			int num3 = cargoPath.modelBatchIndex - 1;
			int num4 = cargoPath.modelIndex;
			int num5 = 0;
			while (num3 >= 0 && num4 >= 0 && num5++ < 100000)
			{
				if ((this.pathRenderingBatch[num3].nodePool[num4].pos - cargoPath.pointPos[num2]).magnitude < 0.5f)
				{
					this.pathRenderingBatch[num3].statePool[num4] = (float)state;
				}
				int next = this.pathRenderingBatch[num3].nodePool[num4].next;
				num3 = next / 10000000 - 1;
				num4 = next % 10000000;
			}
		}
	}

	// Token: 0x06000B3A RID: 2874 RVA: 0x000A9A20 File Offset: 0x000A7C20
	public void SetBeltSignalIcon(int entityId, int signalId)
	{
		bool flag = false;
		bool flag2;
		int objId;
		int num;
		this.factory.ReadObjectConn(entityId, 0, out flag2, out objId, out num);
		PrefabDesc prefabDesc = this.GetPrefabDesc(objId);
		if (prefabDesc != null && (prefabDesc.isStation || prefabDesc.gammaRayReceiver || prefabDesc.isPowerExchanger))
		{
			flag = true;
		}
		this.factory.ReadObjectConn(entityId, 1, out flag2, out objId, out num);
		prefabDesc = this.GetPrefabDesc(objId);
		if (prefabDesc != null && (prefabDesc.isStation || prefabDesc.gammaRayReceiver || prefabDesc.isPowerExchanger))
		{
			flag = true;
		}
		float num2 = flag ? 1.2f : 0.5f;
		Vector3 vector = new Vector3(this.factory.entitySignPool[entityId].x, this.factory.entitySignPool[entityId].y, this.factory.entitySignPool[entityId].z);
		vector.Normalize();
		vector *= this.factory.entityPool[entityId].pos.magnitude + num2;
		this.factory.entitySignPool[entityId].x = vector.x;
		this.factory.entitySignPool[entityId].y = vector.y;
		this.factory.entitySignPool[entityId].z = vector.z;
		if (signalId == 0)
		{
			this.factory.entitySignPool[entityId].iconType = 0U;
			this.factory.entitySignPool[entityId].iconId0 = 0U;
			this.factory.entitySignPool[entityId].count0 = 0f;
			return;
		}
		this.factory.entitySignPool[entityId].iconType = 4U;
		this.factory.entitySignPool[entityId].iconId0 = (uint)signalId;
	}

	// Token: 0x06000B3B RID: 2875 RVA: 0x000A9C08 File Offset: 0x000A7E08
	public void SetBeltSignalNumber(int entityId, float number)
	{
		if (this.factory.entitySignPool[entityId].iconType != 0U && this.factory.entitySignPool[entityId].iconId0 != 0U)
		{
			this.factory.entitySignPool[entityId].count0 = number;
			return;
		}
		this.factory.entitySignPool[entityId].count0 = 0f;
	}

	// Token: 0x06000B3C RID: 2876 RVA: 0x000A9C78 File Offset: 0x000A7E78
	private PrefabDesc GetPrefabDesc(int objId)
	{
		if (objId == 0)
		{
			return null;
		}
		if (objId > 0)
		{
			ModelProto modelProto = LDB.models.Select((int)this.factory.entityPool[objId].modelIndex);
			if (modelProto == null)
			{
				return null;
			}
			return modelProto.prefabDesc;
		}
		else
		{
			ModelProto modelProto2 = LDB.models.Select((int)this.factory.prebuildPool[-objId].modelIndex);
			if (modelProto2 == null)
			{
				return null;
			}
			return modelProto2.prefabDesc;
		}
	}

	// Token: 0x06000B3D RID: 2877 RVA: 0x000A9CE8 File Offset: 0x000A7EE8
	public void Draw(Camera _cam)
	{
		for (int i = 0; i < 12; i++)
		{
			this.beltRenderingBatch[i].Draw(_cam);
		}
		for (int j = 0; j < 2; j++)
		{
			this.pathRenderingBatch[j].Draw(_cam);
		}
	}

	// Token: 0x06000B3E RID: 2878 RVA: 0x000A9D2C File Offset: 0x000A7F2C
	public void TakeBackItems_Spraycoater(Player player, int spraycoaterId)
	{
		if (spraycoaterId == 0)
		{
			return;
		}
		if (this.spraycoaterPool[spraycoaterId].id == spraycoaterId)
		{
			SpraycoaterComponent spraycoaterComponent = this.spraycoaterPool[spraycoaterId];
			if (spraycoaterComponent.incItemId == 0 || spraycoaterComponent.incCount == 0)
			{
				return;
			}
			ItemProto itemProto = LDB.items.Select(spraycoaterComponent.incItemId);
			int num = spraycoaterComponent.incCount / itemProto.HpMax;
			if (num == 0)
			{
				return;
			}
			int incItemId = spraycoaterComponent.incItemId;
			int count = num;
			int inc = 0;
			int upCount = player.TryAddItemToPackage(incItemId, count, inc, true, spraycoaterComponent.entityId, false);
			UIItemup.Up(incItemId, upCount);
			this.spraycoaterPool[spraycoaterId].incItemId = 0;
			this.spraycoaterPool[spraycoaterId].incAbility = 0;
			this.spraycoaterPool[spraycoaterId].incCount = 0;
			this.spraycoaterPool[spraycoaterId].extraIncCount = 0;
		}
	}

	// Token: 0x06000B3F RID: 2879 RVA: 0x000A9E08 File Offset: 0x000A8008
	public void TakeBackItems_Piler(Player player, int pilerId)
	{
		if (pilerId == 0)
		{
			return;
		}
		if (this.pilerPool[pilerId].id == pilerId)
		{
			PilerComponent pilerComponent = this.pilerPool[pilerId];
			if (pilerComponent.cacheItemId1 != 0 && pilerComponent.cacheCargoStack1 != 0)
			{
				int cacheItemId = (int)pilerComponent.cacheItemId1;
				int cacheCargoStack = (int)pilerComponent.cacheCargoStack1;
				int cacheCargoInc = (int)pilerComponent.cacheCargoInc1;
				int upCount = player.TryAddItemToPackage(cacheItemId, cacheCargoStack, cacheCargoInc, true, pilerComponent.entityId, false);
				UIItemup.Up(cacheItemId, upCount);
				this.pilerPool[pilerId].cacheItemId1 = 0;
				this.pilerPool[pilerId].cacheCargoStack1 = 0;
				this.pilerPool[pilerId].cacheCargoInc1 = 0;
			}
			if (pilerComponent.cacheItemId2 != 0 && pilerComponent.cacheCargoStack2 != 0)
			{
				int cacheItemId2 = (int)pilerComponent.cacheItemId2;
				int cacheCargoStack2 = (int)pilerComponent.cacheCargoStack2;
				int cacheCargoInc2 = (int)pilerComponent.cacheCargoInc2;
				int upCount2 = player.TryAddItemToPackage(cacheItemId2, cacheCargoStack2, cacheCargoInc2, true, pilerComponent.entityId, false);
				UIItemup.Up(cacheItemId2, upCount2);
				this.pilerPool[pilerId].cacheItemId2 = 0;
				this.pilerPool[pilerId].cacheCargoStack2 = 0;
				this.pilerPool[pilerId].cacheCargoInc2 = 0;
			}
		}
	}

	// Token: 0x06000B40 RID: 2880 RVA: 0x000A9F30 File Offset: 0x000A8130
	public void ClearItems_Belt(int beltId)
	{
		if (this.beltPool[beltId].id != 0 && this.beltPool[beltId].id == beltId)
		{
			int segIndex = this.beltPool[beltId].segIndex;
			int endIndex = this.beltPool[beltId].segIndex + this.beltPool[beltId].segLength - 1;
			this.GetCargoPath(this.beltPool[beltId].segPathId).ClearItemDirectly(segIndex, endIndex);
		}
	}

	// Token: 0x06000B41 RID: 2881 RVA: 0x000A9FBC File Offset: 0x000A81BC
	public void ClearItems_Spraycoater(int spraycoaterId)
	{
		if (spraycoaterId == 0)
		{
			return;
		}
		if (this.spraycoaterPool[spraycoaterId].id == spraycoaterId)
		{
			this.spraycoaterPool[spraycoaterId].incItemId = 0;
			this.spraycoaterPool[spraycoaterId].incAbility = 0;
			this.spraycoaterPool[spraycoaterId].incCount = 0;
			this.spraycoaterPool[spraycoaterId].extraIncCount = 0;
		}
	}

	// Token: 0x06000B42 RID: 2882 RVA: 0x000AA02C File Offset: 0x000A822C
	public void ClearItems_Piler(int pilerId)
	{
		if (pilerId == 0)
		{
			return;
		}
		if (this.pilerPool[pilerId].id == pilerId)
		{
			this.pilerPool[pilerId].cacheItemId1 = 0;
			this.pilerPool[pilerId].cacheCargoStack1 = 0;
			this.pilerPool[pilerId].cacheCargoInc1 = 0;
			this.pilerPool[pilerId].cacheItemId2 = 0;
			this.pilerPool[pilerId].cacheCargoStack2 = 0;
			this.pilerPool[pilerId].cacheCargoInc2 = 0;
		}
	}

	// Token: 0x04000CE2 RID: 3298
	public PlanetData planet;

	// Token: 0x04000CE3 RID: 3299
	public PlanetFactory factory;

	// Token: 0x04000CE4 RID: 3300
	public CargoContainer container;

	// Token: 0x04000CE5 RID: 3301
	public CargoPath[] pathPool;

	// Token: 0x04000CE6 RID: 3302
	public int pathCursor = 1;

	// Token: 0x04000CE7 RID: 3303
	private int pathCapacity;

	// Token: 0x04000CE8 RID: 3304
	private int[] pathRecycle;

	// Token: 0x04000CE9 RID: 3305
	private int pathRecycleCursor;

	// Token: 0x04000CEA RID: 3306
	public BeltComponent[] beltPool;

	// Token: 0x04000CEB RID: 3307
	public int beltCursor = 1;

	// Token: 0x04000CEC RID: 3308
	private int beltCapacity;

	// Token: 0x04000CED RID: 3309
	private int[] beltRecycle;

	// Token: 0x04000CEE RID: 3310
	private int beltRecycleCursor;

	// Token: 0x04000CEF RID: 3311
	public SplitterComponent[] splitterPool;

	// Token: 0x04000CF0 RID: 3312
	public int splitterCursor = 1;

	// Token: 0x04000CF1 RID: 3313
	private int splitterCapacity;

	// Token: 0x04000CF2 RID: 3314
	private int[] splitterRecycle;

	// Token: 0x04000CF3 RID: 3315
	private int splitterRecycleCursor;

	// Token: 0x04000CF4 RID: 3316
	public MonitorComponent[] monitorPool;

	// Token: 0x04000CF5 RID: 3317
	public int monitorCursor = 1;

	// Token: 0x04000CF6 RID: 3318
	private int monitorCapacity;

	// Token: 0x04000CF7 RID: 3319
	private int[] monitorRecycle;

	// Token: 0x04000CF8 RID: 3320
	private int monitorRecycleCursor;

	// Token: 0x04000CF9 RID: 3321
	public SpraycoaterComponent[] spraycoaterPool;

	// Token: 0x04000CFA RID: 3322
	public int spraycoaterCursor = 1;

	// Token: 0x04000CFB RID: 3323
	private int spraycoaterCapacity;

	// Token: 0x04000CFC RID: 3324
	private int[] spraycoaterRecycle;

	// Token: 0x04000CFD RID: 3325
	private int spraycoaterRecycleCursor;

	// Token: 0x04000CFE RID: 3326
	public PilerComponent[] pilerPool;

	// Token: 0x04000CFF RID: 3327
	public int pilerCursor = 1;

	// Token: 0x04000D00 RID: 3328
	private int pilerCapacity;

	// Token: 0x04000D01 RID: 3329
	private int[] pilerRecycle;

	// Token: 0x04000D02 RID: 3330
	private int pilerRecycleCursor;

	// Token: 0x04000D03 RID: 3331
	public const float kBeltThick = 0.15f;

	// Token: 0x04000D04 RID: 3332
	private CargoPath us_tmp_inputPath;

	// Token: 0x04000D05 RID: 3333
	private CargoPath us_tmp_inputPath0;

	// Token: 0x04000D06 RID: 3334
	private CargoPath us_tmp_inputPath1;

	// Token: 0x04000D07 RID: 3335
	private CargoPath us_tmp_inputPath2;

	// Token: 0x04000D08 RID: 3336
	private CargoPath us_tmp_inputPath3;

	// Token: 0x04000D09 RID: 3337
	private int us_tmp_inputCargo = -1;

	// Token: 0x04000D0A RID: 3338
	private int us_tmp_inputCargo0 = -1;

	// Token: 0x04000D0B RID: 3339
	private int us_tmp_inputCargo1 = -1;

	// Token: 0x04000D0C RID: 3340
	private int us_tmp_inputCargo2 = -1;

	// Token: 0x04000D0D RID: 3341
	private int us_tmp_inputCargo3 = -1;

	// Token: 0x04000D0E RID: 3342
	private int us_tmp_inputIndex0 = -1;

	// Token: 0x04000D0F RID: 3343
	private int us_tmp_inputIndex1 = -1;

	// Token: 0x04000D10 RID: 3344
	private int us_tmp_inputIndex2 = -1;

	// Token: 0x04000D11 RID: 3345
	private int us_tmp_inputIndex3 = -1;

	// Token: 0x04000D12 RID: 3346
	private CargoPath us_tmp_outputPath;

	// Token: 0x04000D13 RID: 3347
	private CargoPath us_tmp_outputPath0;

	// Token: 0x04000D14 RID: 3348
	private int us_tmp_outputIdx;

	// Token: 0x04000D15 RID: 3349
	private const float kHalfSqrt2 = 0.70710677f;

	// Token: 0x04000D16 RID: 3350
	private const float kHalf = 0.5f;

	// Token: 0x04000D17 RID: 3351
	private const float kVerticalWeaken = 0.8f;

	// Token: 0x04000D18 RID: 3352
	public bool _return_items = true;

	// Token: 0x04000D19 RID: 3353
	public Vector3[] posTmp = new Vector3[512];

	// Token: 0x04000D1A RID: 3354
	public float[] rollTmp = new float[512];

	// Token: 0x04000D1B RID: 3355
	public Quaternion[] rotTmp = new Quaternion[512];

	// Token: 0x04000D1C RID: 3356
	private const float kInterval = 0.06f;

	// Token: 0x04000D1D RID: 3357
	private BeltRenderingBatch[] beltRenderingBatch;

	// Token: 0x04000D1E RID: 3358
	private const int kBeltBatchCount = 12;

	// Token: 0x04000D1F RID: 3359
	private PathRenderingBatch[] pathRenderingBatch;

	// Token: 0x04000D20 RID: 3360
	private const int kPathBatchCount = 2;

	// Token: 0x04000D21 RID: 3361
	private bool _batch_buffer_no_refresh;

	// Token: 0x04000D22 RID: 3362
	private BeltAnchor[] tmpBeltAnchors = new BeltAnchor[9];
}
