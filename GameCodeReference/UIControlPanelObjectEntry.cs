using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020006EE RID: 1774
public abstract class UIControlPanelObjectEntry : ManualBehaviour
{
	// Token: 0x17000734 RID: 1844
	// (get) Token: 0x06004779 RID: 18297 RVA: 0x0039BEEF File Offset: 0x0039A0EF
	// (set) Token: 0x0600477A RID: 18298 RVA: 0x0039BF09 File Offset: 0x0039A109
	public int position
	{
		get
		{
			return (int)(-this.rectTransform.anchoredPosition.y - 0.5f);
		}
		set
		{
			if (this.rectTransform.anchoredPosition.y != (float)(-(float)value))
			{
				this.rectTransform.anchoredPosition = new Vector2(this.rectTransform.anchoredPosition.x, (float)(-(float)value));
			}
		}
	}

	// Token: 0x0600477B RID: 18299 RVA: 0x0039BF43 File Offset: 0x0039A143
	public void InitFromPool(int _index, ControlPanelTarget _target)
	{
		if (!this.inUse)
		{
			this.index = _index;
			this.target = _target;
			this.OnSetTarget();
			this.inUse = true;
			base._Init(null);
			base._Open();
		}
	}

	// Token: 0x0600477C RID: 18300 RVA: 0x0039BF75 File Offset: 0x0039A175
	public void FreeIntoPool()
	{
		if (this.inUse)
		{
			base._Close();
			base._Free();
			this.inUse = false;
			this.OnUnsetTarget();
			this.target = ControlPanelTarget.none;
			this.index = -1;
		}
	}

	// Token: 0x0600477D RID: 18301
	public abstract void OnSetTarget();

	// Token: 0x0600477E RID: 18302
	public abstract void OnUnsetTarget();

	// Token: 0x17000735 RID: 1845
	// (get) Token: 0x0600477F RID: 18303
	public abstract bool isTargetDataValid { get; }

	// Token: 0x06004780 RID: 18304 RVA: 0x0039BFAA File Offset: 0x0039A1AA
	protected override void _OnRegEvent()
	{
		if (this.selectButton != null)
		{
			this.selectButton.onClick += this.OnSelectButtonClick;
		}
	}

	// Token: 0x06004781 RID: 18305 RVA: 0x0039BFD1 File Offset: 0x0039A1D1
	protected override void _OnUnregEvent()
	{
		if (this.selectButton != null)
		{
			this.selectButton.onClick -= this.OnSelectButtonClick;
		}
	}

	// Token: 0x06004782 RID: 18306 RVA: 0x0039BFF8 File Offset: 0x0039A1F8
	protected override void _OnOpen()
	{
	}

	// Token: 0x06004783 RID: 18307 RVA: 0x0039BFFC File Offset: 0x0039A1FC
	protected override void _OnClose()
	{
		if (this.selectButton != null)
		{
			this.selectButton.highlighted = false;
		}
		if (this.focusImage != null)
		{
			this.focusImage.gameObject.SetActive(false);
		}
		this.isFocus = false;
		this.focusTime = 0f;
	}

	// Token: 0x06004784 RID: 18308 RVA: 0x0039C054 File Offset: 0x0039A254
	protected override void _OnUpdate()
	{
		float deltaTime = Time.deltaTime;
		this.selectButton.highlighted = this.selected;
		if (this.focusImage != null)
		{
			if (this.isFocus && this.focusTime < 1f)
			{
				this.focusImage.gameObject.SetActive(true);
				float num = -((2f * this.focusTime - 1f) * (2f * this.focusTime - 1f)) + 1f;
				float num2;
				float num3;
				float num4;
				Color.RGBToHSV(this.masterWindow.clearColor, out num2, out num3, out num4);
				float num5;
				float num6;
				float num7;
				Color.RGBToHSV(this.masterWindow.focusColor, out num5, out num6, out num7);
				float h = num5 * num + num2 * (1f - num);
				float s = num6 * num + num3 * (1f - num);
				float v = num7 * num + num4 * (1f - num);
				float a = this.masterWindow.focusColor.a * num + this.masterWindow.clearColor.a * (1f - num);
				Color color = Color.HSVToRGB(h, s, v);
				color.a = a;
				this.focusImage.color = color;
				this.focusTime += deltaTime;
				return;
			}
			this.isFocus = false;
			this.focusImage.gameObject.SetActive(false);
		}
	}

	// Token: 0x06004785 RID: 18309 RVA: 0x0039C1B2 File Offset: 0x0039A3B2
	public void StartFocus()
	{
		this.focusTime = (this.isFocus ? this.focusTime : 0f);
		this.isFocus = true;
	}

	// Token: 0x17000736 RID: 1846
	// (get) Token: 0x06004786 RID: 18310 RVA: 0x0039C1D6 File Offset: 0x0039A3D6
	public bool selected
	{
		get
		{
			return this.inUse && this.masterWindow.selection == this.target;
		}
	}

	// Token: 0x06004787 RID: 18311 RVA: 0x0039C1F8 File Offset: 0x0039A3F8
	public void OnSelectButtonClick(int obj)
	{
		if (!(this.masterWindow.selection != this.target))
		{
			this.masterWindow.selection = ControlPanelTarget.none;
			return;
		}
		if (!this.isTargetDataValid)
		{
			return;
		}
		this.masterWindow.selection = this.target;
	}

	// Token: 0x06004788 RID: 18312 RVA: 0x0039C248 File Offset: 0x0039A448
	public static void ReplenishItems(int itemProtId, int shortage, ref int currentCount, ref string tips)
	{
		if (shortage == 0)
		{
			return;
		}
		Player mainPlayer = GameMain.mainPlayer;
		if (GameMain.data.gameDesc.isSandboxMode)
		{
			int num = itemProtId;
			int num2 = shortage;
			int num3;
			mainPlayer.package.TakeTailItems(ref num, ref num2, out num3, false);
			currentCount += shortage;
			tips += string.Format("已自动补充提示".Translate(), shortage, LDB.items.Select(itemProtId).name);
			tips += "\r\n";
			return;
		}
		int itemCount = mainPlayer.package.GetItemCount(itemProtId);
		if (itemCount > 0)
		{
			int num4 = (itemCount < shortage) ? itemCount : shortage;
			int num5 = itemProtId;
			int num6 = num4;
			int num7;
			mainPlayer.package.TakeTailItems(ref num5, ref num6, out num7, false);
			if (num5 > 0 && num6 > 0)
			{
				currentCount += num6;
				tips += string.Format("已自动补充提示".Translate(), num6, LDB.items.Select(num5).name);
				if (num6 < num4)
				{
					tips += "自动补充物品不足0".Translate();
				}
				tips += "\r\n";
				return;
			}
		}
		else
		{
			tips = tips + string.Format("自动补充物品不足1".Translate(), LDB.items.Select(itemProtId).name) + "\r\n";
		}
	}

	// Token: 0x0400555B RID: 21851
	[SerializeField]
	public UIControlPanelWindow masterWindow;

	// Token: 0x0400555C RID: 21852
	[SerializeField]
	public RectTransform rectTransform;

	// Token: 0x0400555D RID: 21853
	[SerializeField]
	public UIButton selectButton;

	// Token: 0x0400555E RID: 21854
	[SerializeField]
	public Image focusImage;

	// Token: 0x0400555F RID: 21855
	[SerializeField]
	public EControlPanelEntryType entryType;

	// Token: 0x04005560 RID: 21856
	public int generation;

	// Token: 0x04005561 RID: 21857
	public int index = -1;

	// Token: 0x04005562 RID: 21858
	public ControlPanelTarget target;

	// Token: 0x04005563 RID: 21859
	public bool inUse;

	// Token: 0x04005564 RID: 21860
	private bool isFocus;

	// Token: 0x04005565 RID: 21861
	private float focusTime;
}
