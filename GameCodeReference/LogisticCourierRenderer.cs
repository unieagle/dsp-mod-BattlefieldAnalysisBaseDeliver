using System;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000391 RID: 913
public class LogisticCourierRenderer
{
	// Token: 0x0600258F RID: 9615 RVA: 0x0025DE2C File Offset: 0x0025C02C
	public LogisticCourierRenderer(PlanetTransport _transport)
	{
		PrefabDesc prefabDesc = LDB.items.Select(5003).prefabDesc;
		this.transport = _transport;
		this.courierMesh = prefabDesc.lodMeshes[0];
		Material[] array = prefabDesc.lodMaterials[0];
		this.courierMats = new Material[array.Length];
		int num = 0;
		while (num < this.courierMesh.subMeshCount && num < array.Length)
		{
			this.courierMats[num] = Object.Instantiate<Material>(array[num]);
			num++;
		}
		this.argBuffer = new ComputeBuffer(25, 4, ComputeBufferType.DrawIndirect);
		this.courierCount = 0;
	}

	// Token: 0x06002590 RID: 9616 RVA: 0x0025DED4 File Offset: 0x0025C0D4
	public void Destroy()
	{
		this.couriersArr = null;
		this.courierCount = 0;
		if (this.couriersBuffer != null)
		{
			this.couriersBuffer.Release();
			this.couriersBuffer = null;
		}
		if (this.argBuffer != null)
		{
			this.argBuffer.Release();
			this.argBuffer = null;
		}
		if (this.courierMats != null)
		{
			for (int i = 0; i < this.courierMats.Length; i++)
			{
				Object.Destroy(this.courierMats[i]);
			}
			this.courierMats = null;
		}
	}

	// Token: 0x1700055D RID: 1373
	// (get) Token: 0x06002591 RID: 9617 RVA: 0x0025DF52 File Offset: 0x0025C152
	public int capacity
	{
		get
		{
			if (this.couriersArr != null)
			{
				return this.couriersArr.Length;
			}
			return 0;
		}
	}

	// Token: 0x06002592 RID: 9618 RVA: 0x0025DF68 File Offset: 0x0025C168
	public void SetCapacity(int newCap)
	{
		if (newCap < 512)
		{
			newCap = 512;
		}
		if (this.couriersArr == null)
		{
			this.couriersArr = new CourierData[newCap];
			this.couriersBuffer = new ComputeBuffer(newCap, 56, ComputeBufferType.Default);
			return;
		}
		int num = this.couriersArr.Length;
		if (newCap <= num)
		{
			return;
		}
		Array sourceArray = this.couriersArr;
		this.couriersArr = new CourierData[newCap];
		if (this.couriersBuffer != null)
		{
			this.couriersBuffer.Release();
		}
		this.couriersBuffer = new ComputeBuffer(newCap, 56, ComputeBufferType.Default);
		Array.Copy(sourceArray, this.couriersArr, num);
	}

	// Token: 0x06002593 RID: 9619 RVA: 0x0025DFF6 File Offset: 0x0025C1F6
	public void Expand2x()
	{
		this.SetCapacity(this.capacity * 2);
	}

	// Token: 0x06002594 RID: 9620 RVA: 0x0025E008 File Offset: 0x0025C208
	public void Update()
	{
		this.courierCount = 0;
		if (this.transport == null)
		{
			return;
		}
		for (int i = 1; i < this.transport.dispenserCursor; i++)
		{
			DispenserComponent dispenserComponent = this.transport.dispenserPool[i];
			if (dispenserComponent != null && dispenserComponent.id == i)
			{
				int num = this.courierCount + dispenserComponent.workCourierCount;
				if (num > 0)
				{
					while (this.capacity < num)
					{
						this.Expand2x();
					}
					Array.Copy(dispenserComponent.workCourierDatas, 0, this.couriersArr, this.courierCount, dispenserComponent.workCourierCount);
					this.courierCount = num;
				}
			}
		}
		if (this.couriersBuffer != null)
		{
			this.couriersBuffer.SetData(this.couriersArr, 0, 0, this.courierCount);
		}
	}

	// Token: 0x06002595 RID: 9621 RVA: 0x0025E0C0 File Offset: 0x0025C2C0
	public void Draw()
	{
		if (this.courierCount <= 0)
		{
			return;
		}
		float logisticCourierSpeedModified = GameMain.history.logisticCourierSpeedModified;
		for (int i = 0; i < this.courierMats.Length; i++)
		{
			this.argArr[i * 5] = this.courierMesh.GetIndexCount(i);
			this.argArr[1 + i * 5] = (uint)this.courierCount;
			this.argArr[2 + i * 5] = this.courierMesh.GetIndexStart(i);
			this.argArr[3 + i * 5] = this.courierMesh.GetBaseVertex(i);
			this.argArr[4 + i * 5] = 0U;
		}
		this.argBuffer.SetData(this.argArr);
		for (int j = 0; j < this.courierMats.Length; j++)
		{
			this.courierMats[j].SetBuffer("_CourierBuffer", this.couriersBuffer);
			this.courierMats[j].SetFloat("_CourierSpeed", logisticCourierSpeedModified);
			Graphics.DrawMeshInstancedIndirect(this.courierMesh, j, this.courierMats[j], new Bounds(Vector3.zero, new Vector3(10000f, 10000f, 10000f)), this.argBuffer, j * 5 * 4, null, (j == 0) ? ShadowCastingMode.On : ShadowCastingMode.Off, j == 0, 0, null, LightProbeUsage.BlendProbes);
		}
		this.GpuAnalysis();
	}

	// Token: 0x06002596 RID: 9622 RVA: 0x0025E200 File Offset: 0x0025C400
	private void GpuAnalysis()
	{
		if (PerformanceMonitor.GpuProfilerOn)
		{
			int num = this.courierCount;
			int vertexCount = this.courierMesh.vertexCount;
			PerformanceMonitor.RecordGpuWork(EGpuWorkEntry.LogisticCourier, num, num * vertexCount);
		}
	}

	// Token: 0x04002E2C RID: 11820
	public PlanetTransport transport;

	// Token: 0x04002E2D RID: 11821
	private CourierData[] couriersArr;

	// Token: 0x04002E2E RID: 11822
	private ComputeBuffer couriersBuffer;

	// Token: 0x04002E2F RID: 11823
	private Material[] courierMats;

	// Token: 0x04002E30 RID: 11824
	private Mesh courierMesh;

	// Token: 0x04002E31 RID: 11825
	private int courierCount;

	// Token: 0x04002E32 RID: 11826
	private ComputeBuffer argBuffer;

	// Token: 0x04002E33 RID: 11827
	private uint[] argArr = new uint[25];
}
