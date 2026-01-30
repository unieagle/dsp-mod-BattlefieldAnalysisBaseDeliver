using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020006E3 RID: 1763
public class UIControlPanelDispenserEntry : UIControlPanelObjectEntry
{
	// Token: 0x060046B5 RID: 18101 RVA: 0x00395110 File Offset: 0x00393310
	protected override void _OnCreate()
	{
		this.powerServedSB = new StringBuilder("         W", 12);
		this.sbw = new StringBuilder("         W", 16);
		this.sbw2 = new StringBuilder("         W", 16);
		this.droneCountSB = new StringBuilder();
		this.shipCountSB = new StringBuilder();
	}

	// Token: 0x060046B6 RID: 18102 RVA: 0x0039516C File Offset: 0x0039336C
	protected override void _OnDestroy()
	{
		this.powerServedSB.Clear();
		this.sbw.Clear();
		this.sbw2.Clear();
		this.droneCountSB.Clear();
		this.shipCountSB.Clear();
		this.powerServedSB = null;
		this.sbw = null;
		this.sbw2 = null;
		this.droneCountSB = null;
		this.shipCountSB = null;
	}

	// Token: 0x060046B7 RID: 18103 RVA: 0x003951D8 File Offset: 0x003933D8
	protected override bool _OnInit()
	{
		this.courierIconButton.tips.itemId = 5003;
		this.courierIconButton.tips.itemInc = 0;
		this.courierIconButton.tips.itemCount = 0;
		this.courierIconButton.tips.type = UIButton.ItemTipType.Other;
		return true;
	}

	// Token: 0x060046B8 RID: 18104 RVA: 0x0039522E File Offset: 0x0039342E
	protected override void _OnFree()
	{
	}

	// Token: 0x060046B9 RID: 18105 RVA: 0x00395230 File Offset: 0x00393430
	protected override void _OnRegEvent()
	{
		base._OnRegEvent();
		this.fillNecessaryButton.onClick += this.OnFillNecessaryButtonClick;
		this.viewToTargetButton.onClick += this.OnViewToTargetButtonClick;
		this.dispenserIconButton.onClick += base.OnSelectButtonClick;
		this.transitItemButton.onClick += base.OnSelectButtonClick;
		this.courierIconButton.onClick += base.OnSelectButtonClick;
	}

	// Token: 0x060046BA RID: 18106 RVA: 0x003952B8 File Offset: 0x003934B8
	protected override void _OnUnregEvent()
	{
		base._OnUnregEvent();
		this.fillNecessaryButton.onClick -= this.OnFillNecessaryButtonClick;
		this.viewToTargetButton.onClick -= this.OnViewToTargetButtonClick;
		this.dispenserIconButton.onClick -= base.OnSelectButtonClick;
		this.transitItemButton.onClick -= base.OnSelectButtonClick;
		this.courierIconButton.onClick -= base.OnSelectButtonClick;
	}

	// Token: 0x060046BB RID: 18107 RVA: 0x0039533E File Offset: 0x0039353E
	protected override void _OnOpen()
	{
		base._OnOpen();
	}

	// Token: 0x060046BC RID: 18108 RVA: 0x00395346 File Offset: 0x00393546
	protected override void _OnClose()
	{
		base._OnClose();
	}

	// Token: 0x060046BD RID: 18109 RVA: 0x00395350 File Offset: 0x00393550
	protected override void _OnUpdate()
	{
		base._OnUpdate();
		if (!this.isTargetDataValid)
		{
			return;
		}
		string format = "无名称编号".Translate();
		this.dispenserNameText.text = string.Format(format, this.id.ToString());
		if (this.dispenser.playerMode == EPlayerDeliveryMode.Recycle && this.dispenser.filter < 0)
		{
			this.transitItemGroup.alpha = 0f;
			this.transitItemGroup.blocksRaycasts = false;
			this.recycleAllText.color = this.masterWindow.recycleAllColor;
		}
		else
		{
			this.transitItemGroup.alpha = 1f;
			this.transitItemGroup.blocksRaycasts = true;
			this.transitItemText.color = this.masterWindow.transitItemTextColor;
			this.recycleAllText.color = Color.clear;
			ItemProto itemProto = LDB.items.Select(this.dispenser.filter);
			if (itemProto != null)
			{
				this.transitItemImage.sprite = itemProto.iconSprite;
				this.transitItemButton.tips.itemId = this.dispenser.filter;
				this.transitItemButton.tips.itemInc = 0;
				this.transitItemButton.tips.itemCount = 0;
				this.transitItemButton.tips.type = UIButton.ItemTipType.Other;
				int num;
				int num2;
				this.CalculateStorageTotalCount(out num, out num2);
				this.transitItemText.text = num.ToString();
			}
			else
			{
				this.transitItemGroup.alpha = 0f;
				this.transitItemGroup.blocksRaycasts = false;
				this.recycleAllText.color = Color.clear;
			}
		}
		this.SetPlayerDeliveryActiveModes(this.dispenser.playerMode, this.mechaDeliverySupply, this.mechaDeliveryDemand, this.mechaDeliverySupplyDemand);
		this.SetStorageDeliveryActiveModes(this.dispenser.storageMode, this.storageDeliverySupply, this.storageDeliveryDemand);
		this.courierCountText.text = this.dispenser.idleCourierCount.ToString() + "/" + (this.dispenser.idleCourierCount + this.dispenser.workCourierCount).ToString();
		this.powerSystem = this.factory.powerSystem;
		PowerConsumerComponent[] consumerPool = this.powerSystem.consumerPool;
		int pcId = this.dispenser.pcId;
		int networkId = consumerPool[pcId].networkId;
		PowerNetwork powerNetwork = this.powerSystem.netPool[networkId];
		float num3 = (powerNetwork != null && networkId > 0) ? ((float)powerNetwork.consumerRatio) : 0f;
		double num4 = (double)this.dispenser.energy / (double)this.dispenser.energyMax;
		long valuel = (long)((double)(consumerPool[pcId].requiredEnergy * 60L) * (double)num3 + 0.5);
		PowerConsumerComponent[] consumerPool2 = this.powerSystem.consumerPool;
		int pcId2 = this.dispenser.pcId;
		long workEnergyPerTick = this.factory.powerSystem.consumerPool[this.dispenser.pcId].workEnergyPerTick;
		float num5 = (float)powerNetwork.consumerRatio;
		int num6;
		if (num5 >= 1f)
		{
			this.powerCircleFg.fillAmount = 1f;
			num6 = 1;
		}
		else
		{
			this.powerCircleFg.fillAmount = num5;
			num6 = ((num5 >= 0.1f) ? 2 : 3);
		}
		StringBuilderUtility.WriteKMG1000(this.sbw, 8, valuel, true);
		StringBuilderUtility.WriteKMG1000(this.sbw2, 8, workEnergyPerTick * 60L, true);
		this.powerText.text = this.sbw.ToString();
		this.maxChargePowerValue.text = this.sbw2.ToString();
		this.powerRoundFg.fillAmount = (float)((double)this.dispenser.energy / (double)this.dispenser.energyMax);
		if (num6 == 1)
		{
			this.powerSignImage.color = this.masterWindow.powerSignColor1;
			this.powerCircleBg.color = this.masterWindow.powerCircleBgColor1;
			this.powerCircleFg.color = this.masterWindow.powerCircleFgColor1;
			this.powerRoundFg.color = this.masterWindow.powerRoundFgColor1;
			this.powerText.color = this.masterWindow.powerTextColor1;
			this.maxChargePowerValue.color = this.masterWindow.powerTextColor1;
		}
		else if (num6 == 2)
		{
			this.powerSignImage.color = this.masterWindow.powerSignColor2;
			this.powerCircleBg.color = this.masterWindow.powerCircleBgColor2;
			this.powerCircleFg.color = this.masterWindow.powerCircleFgColor2;
			this.powerRoundFg.color = this.masterWindow.powerRoundFgColor2;
			this.powerText.color = this.masterWindow.powerTextColor2;
			this.maxChargePowerValue.color = this.masterWindow.powerTextColor2;
		}
		else if (num6 == 3)
		{
			this.powerSignImage.color = this.masterWindow.powerSignColor3;
			this.powerCircleBg.color = this.masterWindow.powerCircleBgColor3;
			this.powerCircleFg.color = this.masterWindow.powerCircleFgColor3;
			this.powerRoundFg.color = this.masterWindow.powerRoundFgColor3;
			this.powerText.color = this.masterWindow.powerTextColor3;
			this.maxChargePowerValue.color = this.masterWindow.powerTextColor3;
		}
		else
		{
			this.powerSignImage.color = this.masterWindow.powerSignColor0;
			this.powerCircleBg.color = this.masterWindow.powerCircleBgColor0;
			this.powerCircleFg.color = this.masterWindow.powerCircleFgColor0;
			this.powerRoundFg.color = this.masterWindow.powerRoundFgColor0;
			this.powerText.color = this.masterWindow.powerTextColor0;
			this.maxChargePowerValue.color = this.masterWindow.powerTextColor0;
		}
		int num7 = 0;
		if (this.dispenser.playerMode == EPlayerDeliveryMode.None && this.dispenser.storageMode == EStorageDeliveryMode.None)
		{
			num7 = 1;
		}
		else if (this.dispenser.filter == 0)
		{
			num7 = 2;
		}
		else if (this.dispenser.idleCourierCount + this.dispenser.workCourierCount == 0)
		{
			num7 = 3;
		}
		else if (this.dispenser.holdupItemCount > 0)
		{
			num7 = 5;
		}
		else if (num3 < 0.0001f && this.dispenser.energy < 100000L)
		{
			num7 = 4;
		}
		this.warningItemCanvasGroup.alpha = (float)((num7 > 0) ? 1 : 0);
		this.viewToTargetButton.button.interactable = this.isLocal;
	}

	// Token: 0x060046BE RID: 18110 RVA: 0x003959EC File Offset: 0x00393BEC
	public override void OnSetTarget()
	{
		this.factory = GameMain.data.galaxy.PlanetById(this.target.astroId).factory;
		this.dispenser = this.factory.transport.dispenserPool[this.factory.entityPool[this.target.objId].dispenserId];
		this.id = this.dispenser.id;
		EntityData entityData = this.factory.entityPool[this.dispenser.entityId];
		ItemProto itemProto = LDB.items.Select((int)entityData.protoId);
		this.dispenserIcon.sprite = ((itemProto == null) ? null : itemProto.iconSprite);
		this.dispenserIdText.text = "#" + this.id.ToString();
		this.dispenserIconButton.tips.itemId = (int)entityData.protoId;
		this.dispenserIconButton.tips.itemInc = 0;
		this.dispenserIconButton.tips.itemCount = 0;
		this.dispenserIconButton.tips.type = UIButton.ItemTipType.Other;
	}

	// Token: 0x060046BF RID: 18111 RVA: 0x00395B13 File Offset: 0x00393D13
	public override void OnUnsetTarget()
	{
		this.dispenser = null;
		this.factory = null;
	}

	// Token: 0x17000726 RID: 1830
	// (get) Token: 0x060046C0 RID: 18112 RVA: 0x00395B23 File Offset: 0x00393D23
	public override bool isTargetDataValid
	{
		get
		{
			return this.dispenser != null && this.dispenser.id > 0;
		}
	}

	// Token: 0x17000727 RID: 1831
	// (get) Token: 0x060046C1 RID: 18113 RVA: 0x00395B3D File Offset: 0x00393D3D
	public bool isLocal
	{
		get
		{
			return GameMain.localPlanet != null && this.factory.planetId == GameMain.localPlanet.id;
		}
	}

	// Token: 0x060046C2 RID: 18114 RVA: 0x00395B60 File Offset: 0x00393D60
	public void CalculateStorageTotalCount(out int count, out int inc)
	{
		count = 0;
		inc = 0;
		if (!this.isTargetDataValid)
		{
			return;
		}
		if (this.dispenser.storage != null && this.dispenser.filter > 0)
		{
			StorageComponent storageComponent = this.dispenser.storage;
			do
			{
				int num;
				count += storageComponent.GetItemCount(this.dispenser.filter, out num);
				inc += num;
				storageComponent = storageComponent.previousStorage;
			}
			while (storageComponent != null);
		}
	}

	// Token: 0x060046C3 RID: 18115 RVA: 0x00395BCC File Offset: 0x00393DCC
	private void SetPlayerDeliveryActiveModes(EPlayerDeliveryMode mode, Image supply, Image demand, Image supplyDemand)
	{
		if (mode == EPlayerDeliveryMode.Supply)
		{
			supply.color = this.masterWindow.supplyArrowColor;
			demand.color = Color.clear;
			supplyDemand.color = Color.clear;
			this.mechaDeliveryImage.color = this.masterWindow.deliveryActiveColor;
			return;
		}
		if (mode == EPlayerDeliveryMode.Recycle)
		{
			supply.color = Color.clear;
			demand.color = this.masterWindow.demandArrowColor;
			supplyDemand.color = Color.clear;
			this.mechaDeliveryImage.color = this.masterWindow.deliveryActiveColor;
			return;
		}
		if (mode == EPlayerDeliveryMode.Both)
		{
			supply.color = Color.clear;
			demand.color = Color.clear;
			supplyDemand.color = this.masterWindow.supplyDemandArrowColor;
			this.mechaDeliveryImage.color = this.masterWindow.deliveryActiveColor;
			return;
		}
		supply.color = Color.clear;
		demand.color = Color.clear;
		supplyDemand.color = Color.clear;
		this.mechaDeliveryImage.color = this.masterWindow.deliveryDeactiveColor;
	}

	// Token: 0x060046C4 RID: 18116 RVA: 0x00395CDC File Offset: 0x00393EDC
	private void SetStorageDeliveryActiveModes(EStorageDeliveryMode mode, Image supply, Image demand)
	{
		if (!this.isTargetDataValid)
		{
			return;
		}
		if (this.dispenser.filter < 0)
		{
			supply.color = Color.clear;
			demand.color = Color.clear;
			this.storageDeliveryImage.color = this.masterWindow.deliveryDeactiveColor;
			return;
		}
		if (mode == EStorageDeliveryMode.Supply)
		{
			supply.color = this.masterWindow.supplyArrowColor;
			demand.color = Color.clear;
			this.storageDeliveryImage.color = this.masterWindow.deliveryActiveColor;
			return;
		}
		if (mode == EStorageDeliveryMode.Demand)
		{
			supply.color = Color.clear;
			demand.color = this.masterWindow.demandArrowColor;
			this.storageDeliveryImage.color = this.masterWindow.deliveryActiveColor;
			return;
		}
		supply.color = Color.clear;
		demand.color = Color.clear;
		this.storageDeliveryImage.color = this.masterWindow.deliveryDeactiveColor;
	}

	// Token: 0x060046C5 RID: 18117 RVA: 0x00395DC8 File Offset: 0x00393FC8
	private void OnFillNecessaryButtonClick(int obj)
	{
		if (!this.isTargetDataValid)
		{
			return;
		}
		string text = "";
		int shortage = this.dispenser.workCourierDatas.Length - (this.dispenser.idleCourierCount + this.dispenser.workCourierCount);
		UIControlPanelObjectEntry.ReplenishItems(5003, shortage, ref this.dispenser.idleCourierCount, ref text);
		if (!string.IsNullOrEmpty(text))
		{
			UIRealtimeTip.Popup(text, false, 0);
			VFAudio.Create("equip-1", GameMain.mainPlayer.transform, Vector3.zero, true, 4, -1, -1L);
		}
	}

	// Token: 0x060046C6 RID: 18118 RVA: 0x00395E54 File Offset: 0x00394054
	private void OnViewToTargetButtonClick(int obj)
	{
		if (!this.isTargetDataValid)
		{
			return;
		}
		if (this.factory == null)
		{
			return;
		}
		if (this.isLocal)
		{
			EntityData entityData = this.factory.entityPool[this.dispenser.entityId];
			if (entityData.dispenserId == this.dispenser.id)
			{
				UIRoot.instance.uiGame.globemap.MoveToViewTargetTwoStep(entityData.pos, entityData.pos.magnitude + SkillSystem.RoughHeightByModelIndex[(int)entityData.modelIndex] * 2f - this.factory.planet.realRadius);
			}
		}
	}

	// Token: 0x04005466 RID: 21606
	public DispenserComponent dispenser;

	// Token: 0x04005467 RID: 21607
	public PlanetFactory factory;

	// Token: 0x04005468 RID: 21608
	private PowerSystem powerSystem;

	// Token: 0x04005469 RID: 21609
	public int id;

	// Token: 0x0400546A RID: 21610
	[SerializeField]
	public Image dispenserIcon;

	// Token: 0x0400546B RID: 21611
	[SerializeField]
	public UIButton dispenserIconButton;

	// Token: 0x0400546C RID: 21612
	[Header("Info Group")]
	[SerializeField]
	public Text dispenserNameText;

	// Token: 0x0400546D RID: 21613
	[SerializeField]
	public Text dispenserIdText;

	// Token: 0x0400546E RID: 21614
	[SerializeField]
	public RectTransform dispenserIdRectTransform;

	// Token: 0x0400546F RID: 21615
	[Header("Transit group")]
	[SerializeField]
	public CanvasGroup transitItemGroup;

	// Token: 0x04005470 RID: 21616
	[SerializeField]
	public UIButton transitItemButton;

	// Token: 0x04005471 RID: 21617
	[SerializeField]
	public Image transitItemImage;

	// Token: 0x04005472 RID: 21618
	[SerializeField]
	public Text transitItemText;

	// Token: 0x04005473 RID: 21619
	[SerializeField]
	public Text recycleAllText;

	// Token: 0x04005474 RID: 21620
	[Header("Delivery State")]
	[SerializeField]
	public Image mechaDeliveryImage;

	// Token: 0x04005475 RID: 21621
	[SerializeField]
	public Image mechaDeliverySupply;

	// Token: 0x04005476 RID: 21622
	[SerializeField]
	public Image mechaDeliveryDemand;

	// Token: 0x04005477 RID: 21623
	[SerializeField]
	public Image mechaDeliverySupplyDemand;

	// Token: 0x04005478 RID: 21624
	[SerializeField]
	public Image storageDeliveryImage;

	// Token: 0x04005479 RID: 21625
	[SerializeField]
	public Image storageDeliverySupply;

	// Token: 0x0400547A RID: 21626
	[SerializeField]
	public Image storageDeliveryDemand;

	// Token: 0x0400547B RID: 21627
	[Header("Warning group")]
	[SerializeField]
	public CanvasGroup warningItemCanvasGroup;

	// Token: 0x0400547C RID: 21628
	[Header("Courier group")]
	[SerializeField]
	public UIButton courierIconButton;

	// Token: 0x0400547D RID: 21629
	[SerializeField]
	public Image courierIconImage;

	// Token: 0x0400547E RID: 21630
	[SerializeField]
	public Text courierCountText;

	// Token: 0x0400547F RID: 21631
	[Header("Button group")]
	[SerializeField]
	public UIButton fillNecessaryButton;

	// Token: 0x04005480 RID: 21632
	[SerializeField]
	public UIButton viewToTargetButton;

	// Token: 0x04005481 RID: 21633
	[Header("Color group")]
	[SerializeField]
	public Image powerSignImage;

	// Token: 0x04005482 RID: 21634
	[SerializeField]
	public Image powerCircleBg;

	// Token: 0x04005483 RID: 21635
	[SerializeField]
	public Image powerCircleFg;

	// Token: 0x04005484 RID: 21636
	[SerializeField]
	public Image powerRoundFg;

	// Token: 0x04005485 RID: 21637
	[SerializeField]
	public Text powerText;

	// Token: 0x04005486 RID: 21638
	[SerializeField]
	public Text maxChargePowerValue;

	// Token: 0x04005487 RID: 21639
	private StringBuilder powerServedSB;

	// Token: 0x04005488 RID: 21640
	private StringBuilder sbw;

	// Token: 0x04005489 RID: 21641
	private StringBuilder sbw2;

	// Token: 0x0400548A RID: 21642
	private StringBuilder droneCountSB;

	// Token: 0x0400548B RID: 21643
	private StringBuilder shipCountSB;
}
