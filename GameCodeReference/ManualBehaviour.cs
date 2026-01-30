using System;
using UnityEngine;

// Token: 0x020005E0 RID: 1504
public abstract class ManualBehaviour : MonoBehaviour
{
	// Token: 0x17000646 RID: 1606
	// (get) Token: 0x060038CD RID: 14541 RVA: 0x00304FF2 File Offset: 0x003031F2
	// (set) Token: 0x060038CE RID: 14542 RVA: 0x00304FFA File Offset: 0x003031FA
	public bool created { get; private set; }

	// Token: 0x17000647 RID: 1607
	// (get) Token: 0x060038CF RID: 14543 RVA: 0x00305003 File Offset: 0x00303203
	// (set) Token: 0x060038D0 RID: 14544 RVA: 0x0030500B File Offset: 0x0030320B
	public bool destroyed { get; private set; }

	// Token: 0x17000648 RID: 1608
	// (get) Token: 0x060038D1 RID: 14545 RVA: 0x00305014 File Offset: 0x00303214
	// (set) Token: 0x060038D2 RID: 14546 RVA: 0x0030501C File Offset: 0x0030321C
	public bool inited { get; private set; }

	// Token: 0x17000649 RID: 1609
	// (get) Token: 0x060038D3 RID: 14547 RVA: 0x00305025 File Offset: 0x00303225
	// (set) Token: 0x060038D4 RID: 14548 RVA: 0x0030502D File Offset: 0x0030322D
	public bool active { get; private set; }

	// Token: 0x1700064A RID: 1610
	// (get) Token: 0x060038D5 RID: 14549 RVA: 0x00305036 File Offset: 0x00303236
	// (set) Token: 0x060038D6 RID: 14550 RVA: 0x0030503E File Offset: 0x0030323E
	public object data { get; private set; }

	// Token: 0x060038D7 RID: 14551 RVA: 0x00305048 File Offset: 0x00303248
	public void _Create()
	{
		if (!this.created && !this.destroyed)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
			this.created = true;
			try
			{
				this._OnCreate();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x060038D8 RID: 14552 RVA: 0x003050A4 File Offset: 0x003032A4
	public void _Destroy()
	{
		if (this.created)
		{
			this._Free();
			this.created = false;
			this.destroyed = true;
			try
			{
				this._OnDestroy();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x060038D9 RID: 14553 RVA: 0x003050F8 File Offset: 0x003032F8
	public void _Init(object _data)
	{
		if (this.created && !this.inited)
		{
			this.data = _data;
			bool flag = false;
			try
			{
				flag = this._OnInit();
			}
			catch (Exception message)
			{
				flag = false;
				Debug.LogError(message);
			}
			if (flag)
			{
				this.inited = true;
				try
				{
					this._OnRegEvent();
					return;
				}
				catch (Exception message2)
				{
					Debug.LogError(message2);
					return;
				}
			}
			this.inited = false;
			this.data = null;
		}
	}

	// Token: 0x060038DA RID: 14554 RVA: 0x00305174 File Offset: 0x00303374
	public void _Free()
	{
		if (this.inited)
		{
			this._Close();
			try
			{
				this._OnUnregEvent();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			try
			{
				this._OnFree();
			}
			catch (Exception message2)
			{
				Debug.LogError(message2);
			}
			this.inited = false;
			this.data = null;
		}
	}

	// Token: 0x060038DB RID: 14555 RVA: 0x003051D8 File Offset: 0x003033D8
	public void _Open()
	{
		if (this.inited && !this.active)
		{
			this.active = true;
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			try
			{
				this._OnOpen();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x060038DC RID: 14556 RVA: 0x00305238 File Offset: 0x00303438
	public void _Close()
	{
		if (this.active)
		{
			this.active = false;
			try
			{
				this._OnClose();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}
	}

	// Token: 0x060038DD RID: 14557 RVA: 0x0030528C File Offset: 0x0030348C
	public void _Update()
	{
		if (this.active)
		{
			if (!this.unsafeGameObjectState && !base.gameObject.activeInHierarchy)
			{
				return;
			}
			try
			{
				this._OnUpdate();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x060038DE RID: 14558 RVA: 0x003052D8 File Offset: 0x003034D8
	public void _LateUpdate()
	{
		if (this.active)
		{
			if (!this.unsafeGameObjectState && !base.gameObject.activeInHierarchy)
			{
				return;
			}
			try
			{
				this._OnLateUpdate();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
	}

	// Token: 0x060038DF RID: 14559 RVA: 0x00305324 File Offset: 0x00303524
	protected virtual void _OnCreate()
	{
	}

	// Token: 0x060038E0 RID: 14560 RVA: 0x00305326 File Offset: 0x00303526
	protected virtual void _OnDestroy()
	{
	}

	// Token: 0x060038E1 RID: 14561 RVA: 0x00305328 File Offset: 0x00303528
	protected virtual bool _OnInit()
	{
		return true;
	}

	// Token: 0x060038E2 RID: 14562 RVA: 0x0030532B File Offset: 0x0030352B
	protected virtual void _OnFree()
	{
	}

	// Token: 0x060038E3 RID: 14563 RVA: 0x0030532D File Offset: 0x0030352D
	protected virtual void _OnRegEvent()
	{
	}

	// Token: 0x060038E4 RID: 14564 RVA: 0x0030532F File Offset: 0x0030352F
	protected virtual void _OnUnregEvent()
	{
	}

	// Token: 0x060038E5 RID: 14565 RVA: 0x00305331 File Offset: 0x00303531
	protected virtual void _OnOpen()
	{
	}

	// Token: 0x060038E6 RID: 14566 RVA: 0x00305333 File Offset: 0x00303533
	protected virtual void _OnClose()
	{
	}

	// Token: 0x060038E7 RID: 14567 RVA: 0x00305335 File Offset: 0x00303535
	protected virtual void _OnUpdate()
	{
	}

	// Token: 0x060038E8 RID: 14568 RVA: 0x00305337 File Offset: 0x00303537
	protected virtual void _OnLateUpdate()
	{
	}

	// Token: 0x0400416A RID: 16746
	public string updateProfiler = "";

	// Token: 0x0400416B RID: 16747
	public string lateUpdateProfiler = "";

	// Token: 0x0400416C RID: 16748
	[NonSerialized]
	public bool unsafeGameObjectState;
}
