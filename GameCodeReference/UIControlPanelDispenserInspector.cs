using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020006E4 RID: 1764
public class UIControlPanelDispenserInspector : UIControlPanelInspector
{
	// Token: 0x17000728 RID: 1832
	// (get) Token: 0x060046EB RID: 18155 RVA: 0x00395CB4 File Offset: 0x00393EB4
	// (set) Token: 0x060046EC RID: 18156 RVA: 0x00395CBC File Offset: 0x00393EBC
	public int dispenserId
	{
		get
		{
			return this._dispenserId;
		}
		set
		{
			if (this._dispenserId != value)
			{
				this._dispenserId = value;
				this.OnDispenserIdChange();
			}
		}
	}

	// Token: 0x17000729 RID: 1833
	// (get) Token: 0x060046ED RID: 18157 RVA: 0x00395CD4 File Offset: 0x00393ED4
	public bool isLocal
	{
		get
		{
			return this.gameData.localPlanet != null && this.factory.planetId == this.gameData.localPlanet.id;
		}
	}

	// Token: 0x060046EE RID: 18158 RVA: 0x00395D04 File Offset: 0x00393F04
	protected override void _OnCreate()
	{
		this.deliveryRectOriginHeight = (int)(this.deliveryRectTransform.sizeDelta.y + 0.5f);
		this.warningRectOriginHeight = 0;
		this.uiRouteViewPanel._Create();
		this.planetNameTextMaxWidth = this.planetNameText.rectTransform.rect.width;
	}

	// Token: 0x060046EF RID: 18159 RVA: 0x00395D5E File Offset: 0x00393F5E
	protected override void _OnDestroy()
	{
		this.uiRouteViewPanel._Destroy();
	}

	// Token: 0x060046F0 RID: 18160 RVA: 0x00395D6C File Offset: 0x00393F6C
	protected override bool _OnInit()
	{
		this.gameData = (base.data as GameData);
		this.powerServedSB = new StringBuilder("         W", 12);
		this.powerAccumulatedSB = new StringBuilder("         J", 12);
		this.courierIconButton.tips.itemId = 5003;
		this.courierIconButton.tips.itemInc = 0;
		this.courierIconButton.tips.itemCount = 0;
		this.courierIconButton.tips.type = UIButton.ItemTipType.Other;
		this.itemButton.tips.type = UIButton.ItemTipType.Item;
		this.currentTabPanel = EUIControlPanelDispenserPanel.Info;
		this.uiRouteViewPanel._Init(base.data);
		return true;
	}

	// Token: 0x060046F1 RID: 18161 RVA: 0x00395E20 File Offset: 0x00394020
	protected override void _OnFree()
	{
		this.uiRouteViewPanel._Free();
	}

	// Token: 0x060046F2 RID: 18162 RVA: 0x00395E30 File Offset: 0x00394030
	protected override void _OnRegEvent()
	{
		this.courierIconButton.onClick += this.OnCourierIconClick;
		this.courierAutoReplenishButton.onClick += this.OnCourierAutoReplenishButtonClick;
		for (int i = 0; i < this.holdupItemBtns.Length; i++)
		{
			this.holdupItemBtns[i].onClick += this.OnHoldupItemClick;
		}
		this.maxChargePowerSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnMaxChargePowerSliderValueChange));
		this.playerModeSwitch.onModeChange += this.OnModeSwitchClicked;
		this.storageModeSwitch.onToggle += this.OnModeToggleClicked;
		this.guessFilterButton.onClick += this.OnGuessFilterButtonClick;
		this.infoTabButton.onClick += this.OnInfoTabButtonClick;
		this.routeTabButton.onClick += this.OnRouteTabButtonClick;
	}

	// Token: 0x060046F3 RID: 18163 RVA: 0x00395F28 File Offset: 0x00394128
	protected override void _OnUnregEvent()
	{
		this.courierIconButton.onClick -= this.OnCourierIconClick;
		this.courierAutoReplenishButton.onClick -= this.OnCourierAutoReplenishButtonClick;
		for (int i = 0; i < this.holdupItemBtns.Length; i++)
		{
			this.holdupItemBtns[i].onClick -= this.OnHoldupItemClick;
		}
		this.maxChargePowerSlider.onValueChanged.RemoveAllListeners();
		this.playerModeSwitch.onModeChange -= this.OnModeSwitchClicked;
		this.storageModeSwitch.onToggle -= this.OnModeToggleClicked;
		this.guessFilterButton.onClick -= this.OnGuessFilterButtonClick;
		this.infoTabButton.onClick -= this.OnInfoTabButtonClick;
		this.routeTabButton.onClick -= this.OnRouteTabButtonClick;
	}

	// Token: 0x060046F4 RID: 18164 RVA: 0x00396014 File Offset: 0x00394214
	protected override void _OnOpen()
	{
		if (base.active)
		{
			this.selectItemButton.onClick += this.OnSelectItemButtonClick;
			this.takeBackButton.onClick += this.OnTakeBackButtonClick;
			this.player.onIntendToTransferItems += this.OnPlayerIntendToTransferItems;
		}
		this.OnDispenserIdChange();
		if (this.dispenserId == 0)
		{
			base._Close();
			return;
		}
		UIItemPicker.showAll = GameMain.sandboxToolsEnabled;
		this.pointerInIcon = false;
		this.UpdateTitle();
	}

	// Token: 0x060046F5 RID: 18165 RVA: 0x0039609C File Offset: 0x0039429C
	protected override void _OnClose()
	{
		if (this.player != null)
		{
			this.player.onIntendToTransferItems -= this.OnPlayerIntendToTransferItems;
		}
		this.selectItemButton.onClick -= this.OnSelectItemButtonClick;
		this.takeBackButton.onClick -= this.OnTakeBackButtonClick;
		this.insplit = false;
		if (UIRoot.instance != null)
		{
			UIRoot.instance.uiGame.CloseGridSplit();
		}
		this._dispenserId = 0;
		this.factory = null;
		this.transport = null;
		this.player = null;
		this.uiRouteViewPanel._Close();
	}

	// Token: 0x060046F6 RID: 18166 RVA: 0x00396144 File Offset: 0x00394344
	protected override void _OnUpdate()
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			base._Close();
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			base._Close();
			return;
		}
		this.UpdateTitle();
		int num = 0;
		EUIControlPanelDispenserPanel euicontrolPanelDispenserPanel = this.currentTabPanel;
		if (euicontrolPanelDispenserPanel != EUIControlPanelDispenserPanel.Info)
		{
			if (euicontrolPanelDispenserPanel == EUIControlPanelDispenserPanel.Route)
			{
				this.uiRouteViewPanel._Update();
				num = (int)((double)this.uiRouteViewPanel.rectTransform.sizeDelta.y + 0.5);
			}
		}
		else
		{
			this.UpdatePower();
			this.UpdateDelivery();
		}
		this.scrollContentRect.sizeDelta = new Vector3(this.scrollContentRect.sizeDelta.x, (float)num);
		if (UIRoot.instance.uiGame.gridSplit.active && this.insplit && (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)))
		{
			this.OnItemIconMouseUp(null);
		}
	}

	// Token: 0x060046F7 RID: 18167 RVA: 0x00396248 File Offset: 0x00394448
	public void UpdatePower()
	{
		PowerConsumerComponent[] consumerPool = this.powerSystem.consumerPool;
		int pcId = this.dispenser.pcId;
		int networkId = consumerPool[pcId].networkId;
		PowerNetwork powerNetwork = this.powerSystem.netPool[networkId];
		float num = (powerNetwork != null && networkId > 0) ? ((float)powerNetwork.consumerRatio) : 0f;
		float num2 = (float)((double)this.dispenser.energy / (double)this.dispenser.energyMax);
		double num3 = (double)(consumerPool[pcId].requiredEnergy * 60L);
		long valuel = (long)(num3 * (double)num + 0.5);
		if (num > 0f)
		{
			StringBuilderUtility.WriteKMG(this.powerServedSB, 8, valuel, false, '\u2009', ' ');
			this.powerText.text = this.powerServedSB.ToString();
			if (num2 == 1f)
			{
				this.stateText.text = "已充满".Translate();
				this.stateText.color = this.idleColor;
			}
			else
			{
				this.stateText.text = ((num3 > 0.0) ? "充电中".Translate() : "待机中".Translate());
				this.stateText.color = ((num3 > 0.0) ? this.chargeColor : this.idleColor);
			}
		}
		else
		{
			this.powerText.text = "断电".Translate();
			this.stateText.text = "无法充电".Translate();
			this.stateText.color = this.powerOffColor;
		}
		StringBuilderUtility.WriteKMG(this.powerAccumulatedSB, 8, this.dispenser.energy, false, '\u2009', ' ');
		this.energyText.text = this.powerAccumulatedSB.ToString().TrimStart();
		this.powerIcon.color = ((num > 0f || num2 > 0.12f) ? this.powerNormalIconColor : this.powerOffIconColor);
		this.powerText.color = this.powerNormalColor;
		this.energyBar.fillAmount = num2;
		this.powerText.color = ((num > 0f) ? this.powerNormalColor : this.powerOffColor);
		float width = this.energyBar.rectTransform.rect.width;
		if (num2 > 0.7f)
		{
			this.energyText.rectTransform.anchoredPosition = new Vector2(Mathf.Round(width * num2 - 30f), 0f);
			this.energyText.color = this.energyTextColor1;
			this.energyBar.color = this.energyBarColor0;
			return;
		}
		this.energyText.rectTransform.anchoredPosition = new Vector2(Mathf.Round(width * num2 + 30f), 0f);
		this.energyText.color = ((num2 < 0.12f) ? this.energyTextColor2 : this.energyTextColor0);
		this.energyBar.color = ((num2 < 0.12f) ? this.energyBarColor1 : this.energyBarColor0);
	}

	// Token: 0x060046F8 RID: 18168 RVA: 0x0039655C File Offset: 0x0039475C
	public void UpdateDelivery()
	{
		this.takeBackButton.gameObject.SetActive(this.pointerInIcon);
		this.courierCountText.text = this.dispenser.idleCourierCount.ToString() + "/" + (this.dispenser.idleCourierCount + this.dispenser.workCourierCount).ToString();
		this.courierAutoReplenishButton.highlighted = this.dispenser.courierAutoReplenish;
		this.ValueToUI(this.dispenser);
		int num = this.holdupItemBtns.Length;
		DispenserStore[] holdupPackage = this.dispenser.holdupPackage;
		for (int i = 0; i < num; i++)
		{
			int itemId = holdupPackage[i].itemId;
			ItemProto itemProto = LDB.items.Select(itemId);
			if (itemProto == null)
			{
				this.holdupItemBtns[i].gameObject.SetActive(false);
			}
			else
			{
				this.holdupItemBtns[i].gameObject.SetActive(true);
				this.holdupItemBtns[i].tips.itemId = itemId;
				this.holdupItemBtns[i].tips.itemCount = holdupPackage[i].count;
				this.holdupItemBtns[i].tips.itemInc = holdupPackage[i].inc;
				this.holdupItemIcons[i].sprite = itemProto.iconSprite;
				this.holdupItemCountTexts[i].text = holdupPackage[i].count.ToString();
				int num2 = (holdupPackage[i].count == 0) ? holdupPackage[i].inc : (holdupPackage[i].inc / holdupPackage[i].count);
				int num3 = (int)Cargo.fastIncArrowTable[(num2 > 10) ? 10 : num2];
				float fillAmount = 0f;
				if (num3 == 1)
				{
					fillAmount = 0.35f;
				}
				else if (num3 == 2)
				{
					fillAmount = 0.65f;
				}
				else if (num3 == 3)
				{
					fillAmount = 1f;
				}
				this.holdupItemIncImages[i].fillAmount = fillAmount;
			}
		}
	}

	// Token: 0x060046F9 RID: 18169 RVA: 0x00396764 File Offset: 0x00394964
	public void UpdateTitle()
	{
		if (this.planet != null)
		{
			if (this.isLocal)
			{
				this.planetDistance.text = "(" + "当前星球".Translate() + ")";
				this.planetDistance.font = this.masterWindow.FONT_SAIRASB;
			}
			else
			{
				VectorLF3 uPosition = this.planet.uPosition;
				double magnitude = (this.gameData.mainPlayer.uPosition - uPosition).magnitude;
				double num = magnitude / 2400000.0;
				if (num < 0.10000000149011612)
				{
					double num2 = magnitude / 40000.0;
					this.planetDistance.text = "(" + num2.ToString("F1") + " AU)";
				}
				else
				{
					this.planetDistance.text = "(" + num.ToString("F1") + " ly)";
				}
				this.planetDistance.font = this.masterWindow.FONT_DIN;
			}
			int num3 = (int)(this.planetDistance.preferredWidth + 0.5f);
			this.planetDistance.rectTransform.sizeDelta = new Vector2((float)num3, this.planetDistance.rectTransform.sizeDelta.y);
			string displayName = this.planet.displayName;
			this.planetNameText.text = displayName;
			float preferredWidth = this.planetNameText.preferredWidth;
			if (preferredWidth > this.planetNameTextMaxWidth)
			{
				preferredWidth = this.planetNameTextMaxWidth;
			}
			int num4 = (int)(preferredWidth + 0.5f);
			this.planetNameText.rectTransform.sizeDelta = new Vector2((float)num4, this.planetNameText.rectTransform.sizeDelta.y);
		}
	}

	// Token: 0x060046FA RID: 18170 RVA: 0x00396928 File Offset: 0x00394B28
	public void SetData(int planetId, int entityId)
	{
		this.planet = this.gameData.galaxy.PlanetById(planetId);
		this.factory = this.planet.factory;
		this.transport = this.factory.transport;
		this.powerSystem = this.factory.powerSystem;
		this.player = this.gameData.mainPlayer;
		EntityData entityData = this.factory.entityPool[entityId];
		this.dispenser = this.transport.dispenserPool[entityData.dispenserId];
		this.dispenserId = ((this.dispenser == null) ? 0 : entityData.dispenserId);
	}

	// Token: 0x060046FB RID: 18171 RVA: 0x003969D4 File Offset: 0x00394BD4
	private void OnDispenserIdChange()
	{
		if (base.active)
		{
			if (this.dispenserId == 0 || this.factory == null)
			{
				base._Close();
				return;
			}
			DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
			if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
			{
				base._Close();
				return;
			}
			ItemProto itemProto = LDB.items.Select((int)this.factory.entityPool[dispenserComponent.entityId].protoId);
			if (itemProto == null)
			{
				base._Close();
				return;
			}
			this.ValueToUI(dispenserComponent);
			this.titleText.text = itemProto.name;
			this.planetNameText.text = this.planet.displayName;
			long workEnergyPerTick = itemProto.prefabDesc.workEnergyPerTick;
			long num = workEnergyPerTick * 5L;
			long num2 = workEnergyPerTick / 2L;
			long workEnergyPerTick2 = this.powerSystem.consumerPool[dispenserComponent.pcId].workEnergyPerTick;
			this.maxChargePowerSlider.maxValue = (float)(num / 5000L);
			this.maxChargePowerSlider.minValue = (float)(num2 / 5000L);
			this.maxChargePowerSlider.value = (float)(workEnergyPerTick2 / 5000L);
			StringBuilderUtility.WriteKMG(this.powerServedSB, 8, workEnergyPerTick2 * 60L, true, '\u2009', ' ');
			this.maxChargePowerValue.text = this.powerServedSB.ToString();
			this.courierAutoReplenishButton.highlighted = dispenserComponent.courierAutoReplenish;
			this.ReplenishItemsIfNeeded(dispenserComponent.entityId);
			this.RefreshTabPanelUI();
		}
	}

	// Token: 0x060046FC RID: 18172 RVA: 0x00396B50 File Offset: 0x00394D50
	public void OnModeSwitchClicked(int value)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		this.UIToValue(dispenserComponent);
	}

	// Token: 0x060046FD RID: 18173 RVA: 0x00396B9C File Offset: 0x00394D9C
	public void OnModeToggleClicked(bool toggle)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		this.UIToValue(dispenserComponent);
	}

	// Token: 0x060046FE RID: 18174 RVA: 0x00396BE8 File Offset: 0x00394DE8
	public void UIToValue(DispenserComponent dispenser)
	{
		EPlayerDeliveryMode eplayerDeliveryMode = dispenser.playerMode;
		EStorageDeliveryMode estorageDeliveryMode = dispenser.storageMode;
		int num = dispenser.filter;
		if (this.playerModeToggle.isOn && this.storageModeToggle.isOn)
		{
			eplayerDeliveryMode = this.playerModeSwitch.currMode + EPlayerDeliveryMode.Recycle;
			estorageDeliveryMode = (this.storageModeSwitch.isOn ? EStorageDeliveryMode.Supply : EStorageDeliveryMode.Demand);
		}
		else if (this.playerModeToggle.isOn && !this.storageModeToggle.isOn)
		{
			eplayerDeliveryMode = this.playerModeSwitch.currMode + EPlayerDeliveryMode.Recycle;
			estorageDeliveryMode = EStorageDeliveryMode.None;
		}
		else if (!this.playerModeToggle.isOn && this.storageModeToggle.isOn)
		{
			eplayerDeliveryMode = EPlayerDeliveryMode.None;
			estorageDeliveryMode = (this.storageModeSwitch.isOn ? EStorageDeliveryMode.Supply : EStorageDeliveryMode.Demand);
		}
		else
		{
			eplayerDeliveryMode = EPlayerDeliveryMode.None;
			estorageDeliveryMode = EStorageDeliveryMode.None;
		}
		if (eplayerDeliveryMode == EPlayerDeliveryMode.Recycle && this.recycleAllToggle.isOn)
		{
			if (num > 0)
			{
				num = -num;
			}
			else if (num == 0)
			{
				num = -1;
			}
			estorageDeliveryMode = EStorageDeliveryMode.None;
		}
		else if (num == -1)
		{
			num = 0;
		}
		else if (num < 0)
		{
			num = -num;
		}
		if (eplayerDeliveryMode == EPlayerDeliveryMode.None || dispenser.playerMode == EPlayerDeliveryMode.None)
		{
			this.playerModeSwitch.SetModeNoEvent(1);
		}
		if (estorageDeliveryMode == EStorageDeliveryMode.None || dispenser.storageMode == EStorageDeliveryMode.None)
		{
			this.storageModeSwitch.SetToggleNoEvent(true);
		}
		this.transport.SetDispenserFilter(this.dispenserId, num);
		this.transport.SetDispenserPlayerDeliveryMode(this.dispenserId, eplayerDeliveryMode);
		this.transport.SetDispenserStorageDeliveryMode(this.dispenserId, estorageDeliveryMode);
	}

	// Token: 0x060046FF RID: 18175 RVA: 0x00396D40 File Offset: 0x00394F40
	private void ValueToUI(DispenserComponent dispenser)
	{
		EPlayerDeliveryMode playerMode = dispenser.playerMode;
		EStorageDeliveryMode storageMode = dispenser.storageMode;
		bool flag = playerMode > EPlayerDeliveryMode.None;
		bool flag2 = storageMode > EStorageDeliveryMode.None;
		Vector2 sizeDelta = (base.transform as RectTransform).sizeDelta;
		bool flag3 = playerMode == EPlayerDeliveryMode.Recycle && dispenser.filter < 0;
		this.holdupItemGo.SetActive(dispenser.holdupItemCount > 0);
		if (dispenser.filter > 0)
		{
			this.itemButton.tips.itemId = dispenser.filter;
			ItemProto itemProto = LDB.items.Select(dispenser.filter);
			if (itemProto != null)
			{
				this.itemImage.sprite = itemProto.iconSprite;
			}
			else
			{
				this.itemImage.sprite = null;
			}
			this.selectItemButton.gameObject.SetActive(false);
			this.itemButton.gameObject.SetActive(true);
			int num;
			int num2;
			this.CalculateStorageTotalCount(dispenser, out num, out num2);
			this.itemButton.tips.itemCount = num;
			this.itemButton.tips.itemInc = num2;
			this.currentCountText.text = num.ToString();
			int num3 = (num <= 0 || num2 <= 0) ? 0 : (num2 / num);
			int num4 = (int)Cargo.fastIncArrowTable[(num3 > 10) ? 10 : num3];
			this.itemIncs[0].enabled = (num4 == 1);
			this.itemIncs[1].enabled = (num4 == 2);
			this.itemIncs[2].enabled = (num4 == 3);
			int num5 = dispenser.playerOrdered + dispenser.storageOrdered;
			if (num5 == 0)
			{
				this.orderedCountText.text = "";
				if (this.orderedCountLabel != null)
				{
					this.orderedCountLabel.color = this.orderedNormalColor;
				}
				if (this.orderedCountText != null)
				{
					this.orderedCountText.color = this.orderedNormalColor;
				}
			}
			else if (num5 < 0)
			{
				this.orderedCountText.text = num5.ToString();
				if (this.orderedCountLabel != null)
				{
					this.orderedCountLabel.color = this.orderedNagativeColor;
				}
				if (this.orderedCountText != null)
				{
					this.orderedCountText.color = this.orderedNagativeColor;
				}
			}
			else
			{
				this.orderedCountText.text = "+" + num5.ToString();
				if (this.orderedCountLabel != null)
				{
					this.orderedCountLabel.color = this.orderedPositiveColor;
				}
				if (this.orderedCountText != null)
				{
					this.orderedCountText.color = this.orderedPositiveColor;
				}
			}
		}
		else
		{
			this.itemImage.sprite = null;
			this.selectItemButton.gameObject.SetActive(true);
			this.itemButton.gameObject.SetActive(false);
		}
		this.playerModeToggle.isOn = flag;
		this.storageModeToggle.isOn = flag2;
		this.playerModeSwitch.gameObject.SetActive(flag);
		this.storageModeSwitch.gameObject.SetActive(flag2);
		this.playerModeSwitch.currMode = ((playerMode == EPlayerDeliveryMode.None) ? EPlayerDeliveryMode.Both : playerMode) - EPlayerDeliveryMode.Recycle;
		this.storageModeSwitch.isOn = (storageMode == EStorageDeliveryMode.Supply || storageMode == EStorageDeliveryMode.None);
		this.recycleAllToggle.gameObject.SetActive(playerMode == EPlayerDeliveryMode.Recycle);
		this.recycleAllToggle.isOn = flag3;
		this.selectItemButton.button.interactable = !flag3;
		this.selectItemButton.highlighted = flag3;
		this.storageModeIcon.color = (flag3 ? this.inactiveColor : this.activeColor);
		ColorBlock colors = this.storageModeToggle.toggle.colors;
		colors.fadeDuration = (flag3 ? 0f : 0.1f);
		this.storageModeToggle.toggle.colors = colors;
		this.storageModeToggle.toggle.interactable = !flag3;
		this.storageModeButton.highlighted = flag3;
		if (playerMode == EPlayerDeliveryMode.None)
		{
			this.playerModeText.text = "向玩家配送模式0".Translate();
		}
		else if (playerMode == EPlayerDeliveryMode.Supply)
		{
			this.playerModeText.text = "向玩家配送模式1".Translate();
		}
		else if (playerMode == EPlayerDeliveryMode.Recycle)
		{
			this.playerModeText.text = "向玩家配送模式2".Translate();
		}
		else if (playerMode == EPlayerDeliveryMode.Both)
		{
			this.playerModeText.text = "向玩家配送模式3".Translate();
		}
		if (storageMode == EStorageDeliveryMode.None)
		{
			this.storageModeText.text = "向箱子配送模式0".Translate();
		}
		else if (storageMode == EStorageDeliveryMode.Supply)
		{
			this.storageModeText.text = "向箱子配送模式1".Translate();
		}
		else if (storageMode == EStorageDeliveryMode.Demand)
		{
			this.storageModeText.text = "向箱子配送模式2".Translate();
		}
		if (flag2)
		{
			double num6 = Math.Cos((double)this.gameData.history.dispenserDeliveryMaxAngle * 3.141592653589793 / 180.0);
			if (num6 < -0.999)
			{
				num6 = -1.0;
			}
			int num7 = dispenser.pairCount - dispenser.playerPairCount;
			int num8 = 0;
			for (int i = dispenser.playerPairCount; i < dispenser.pairCount; i++)
			{
				SupplyDemandPair supplyDemandPair = dispenser.pairs[i];
				int num9 = (supplyDemandPair.supplyId == this.dispenserId) ? supplyDemandPair.demandId : supplyDemandPair.supplyId;
				DispenserComponent dispenserComponent = this.transport.dispenserPool[num9];
				double num10;
				if (dispenserComponent == null || dispenserComponent.id != num9)
				{
					num8++;
				}
				else if (!dispenser.CheckDeliveryRange(this.factory.entityPool[dispenser.entityId].pos, this.factory.entityPool[dispenserComponent.entityId].pos, num6, out num10))
				{
					num8++;
				}
			}
			if (num8 == 0)
			{
				this.pairTipText.text = string.Format("配送线路数量提示0".Translate(), num7);
			}
			else
			{
				this.pairTipText.text = string.Format("配送线路数量提示1".Translate(), num7, num8);
			}
		}
		else
		{
			this.pairTipText.text = "";
		}
		this.pairTipText.gameObject.SetActive(flag2);
		int num11 = 0;
		if (dispenser.playerMode == EPlayerDeliveryMode.None && dispenser.storageMode == EStorageDeliveryMode.None)
		{
			num11 = 1;
		}
		else if (dispenser.filter == 0)
		{
			num11 = 2;
		}
		else if (dispenser.idleCourierCount + dispenser.workCourierCount == 0)
		{
			num11 = 3;
		}
		else if (dispenser.holdupItemCount > 0)
		{
			num11 = 5;
		}
		else
		{
			PowerConsumerComponent[] consumerPool = this.powerSystem.consumerPool;
			int pcId = dispenser.pcId;
			int networkId = consumerPool[pcId].networkId;
			PowerNetwork powerNetwork = this.powerSystem.netPool[networkId];
			if (((powerNetwork != null && networkId > 0) ? ((float)powerNetwork.consumerRatio) : 0f) < 0.0001f && dispenser.energy < 100000L)
			{
				num11 = 4;
			}
		}
		this.SetWarningTipVisible(num11);
		int num12 = 0;
		int num13 = 0;
		if (dispenser.holdupItemCount > 0)
		{
			num13 += 86;
		}
		if (playerMode == EPlayerDeliveryMode.Recycle)
		{
			num12 += 20;
		}
		if (num11 > 0)
		{
			num13 += 48;
		}
		if (storageMode != EStorageDeliveryMode.None)
		{
			num12 += 16;
		}
		this.storageModeBox.anchoredPosition = new Vector2(this.storageModeBox.anchoredPosition.x, (float)((playerMode == EPlayerDeliveryMode.Recycle) ? -62 : -42));
		int num14 = this.deliveryRectOriginHeight + num12;
		int num15 = this.warningRectOriginHeight + num13;
		this.deliveryRectTransform.sizeDelta = new Vector2(this.deliveryRectTransform.sizeDelta.x, (float)num14);
		int num16 = (int)((double)(this.deliveryRectTransform.anchoredPosition.y - this.deliveryRectTransform.sizeDelta.y) - 0.5);
		this.warningRectTransform.anchoredPosition = new Vector2(this.warningRectTransform.anchoredPosition.x, (float)num16);
		this.warningRectTransform.sizeDelta = new Vector2(this.warningRectTransform.sizeDelta.x, (float)num15);
		int num17 = (int)((double)(this.warningRectTransform.anchoredPosition.y - this.warningRectTransform.sizeDelta.y) - 0.5);
		this.settingRectTransform.anchoredPosition = new Vector2(this.settingRectTransform.anchoredPosition.x, (float)num17);
	}

	// Token: 0x06004700 RID: 18176 RVA: 0x00397584 File Offset: 0x00395784
	private void OnSelectItemButtonClick(int obj)
	{
		if (UIItemPicker.isOpened)
		{
			UIItemPicker.Close();
			return;
		}
		UIItemPicker.Popup((base.transform as RectTransform).anchoredPosition + new Vector2(20f, 266f), new Action<ItemProto>(this.OnItemPickerReturn));
	}

	// Token: 0x06004701 RID: 18177 RVA: 0x003975D4 File Offset: 0x003957D4
	private void OnItemPickerReturn(ItemProto itemProto)
	{
		if (itemProto == null)
		{
			return;
		}
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		this.transport.SetDispenserFilter(this.dispenserId, itemProto.ID);
		if (dispenserComponent.filter > 0)
		{
			this.itemButton.tips.itemId = dispenserComponent.filter;
			this.itemImage.sprite = itemProto.iconSprite;
			this.selectItemButton.gameObject.SetActive(false);
			this.itemButton.gameObject.SetActive(true);
		}
	}

	// Token: 0x06004702 RID: 18178 RVA: 0x00397684 File Offset: 0x00395884
	private void OnTakeBackButtonClick(int obj)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		this.transport.SetDispenserFilter(this.dispenserId, 0);
		this.itemImage.sprite = null;
		this.selectItemButton.gameObject.SetActive(true);
		this.itemButton.gameObject.SetActive(false);
		this.pointerInIcon = false;
	}

	// Token: 0x06004703 RID: 18179 RVA: 0x0039770E File Offset: 0x0039590E
	public void OnIconEnter()
	{
		this.pointerInIcon = true;
	}

	// Token: 0x06004704 RID: 18180 RVA: 0x00397717 File Offset: 0x00395917
	public void OnIconExit()
	{
		this.pointerInIcon = false;
	}

	// Token: 0x06004705 RID: 18181 RVA: 0x00397720 File Offset: 0x00395920
	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			this.insplit = false;
			if (UIRoot.instance != null)
			{
				UIRoot.instance.uiGame.CloseGridSplit();
			}
		}
	}

	// Token: 0x06004706 RID: 18182 RVA: 0x0039774C File Offset: 0x0039594C
	public void OnItemIconMouseDown(BaseEventData evt)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		PointerEventData pointerEventData = evt as PointerEventData;
		if (pointerEventData == null)
		{
			return;
		}
		if (this.player.inhandItemId == 0)
		{
			if (pointerEventData.button == PointerEventData.InputButton.Right)
			{
				if (!this.isLocal)
				{
					UIRealtimeTip.Popup("非本地星球拿取提示".Translate(), true, 0);
					return;
				}
				int num;
				int num2;
				this.CalculateStorageTotalCount(dispenserComponent, out num, out num2);
				if (num > 0)
				{
					UIRoot.instance.uiGame.OpenGridSplit(dispenserComponent.filter, num, Input.mousePosition);
					this.insplit = true;
					return;
				}
			}
		}
		else if (this.player.inhandItemId == dispenserComponent.filter && dispenserComponent.filter > 0)
		{
			if (!this.isLocal)
			{
				UIRealtimeTip.Popup("非本地星球放置提示".Translate(), true, 0);
				return;
			}
			if (pointerEventData.button == PointerEventData.InputButton.Left && dispenserComponent.storage != null)
			{
				int handItemInc_Unsafe;
				int num3 = this.factory.InsertIntoStorage(dispenserComponent.storage.bottomStorage.entityId, this.player.inhandItemId, this.player.inhandItemCount, this.player.inhandItemInc, out handItemInc_Unsafe, false);
				this.player.AddHandItemCount_Unsafe(-num3);
				this.player.SetHandItemInc_Unsafe(handItemInc_Unsafe);
				if (this.player.inhandItemCount <= 0)
				{
					this.player.SetHandItems(0, 0, 0);
				}
			}
		}
	}

	// Token: 0x06004707 RID: 18183 RVA: 0x003978D0 File Offset: 0x00395AD0
	public void OnItemIconMouseUp(BaseEventData evt)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		if (this.insplit)
		{
			if (dispenserComponent.storage != null)
			{
				int count = UIRoot.instance.uiGame.CloseGridSplit();
				if (this.player.inhandItemId == 0 && this.player.inhandItemCount == 0 && dispenserComponent.filter > 0)
				{
					int handItemInc_Unsafe;
					int handItemCount_Unsafe = this.factory.PickFromStorage(dispenserComponent.storage.bottomStorage.entityId, dispenserComponent.filter, count, out handItemInc_Unsafe);
					this.player.SetHandItemId_Unsafe(dispenserComponent.filter);
					this.player.SetHandItemCount_Unsafe(handItemCount_Unsafe);
					this.player.SetHandItemInc_Unsafe(handItemInc_Unsafe);
				}
			}
			this.insplit = false;
		}
	}

	// Token: 0x06004708 RID: 18184 RVA: 0x003979B4 File Offset: 0x00395BB4
	private void OnHoldupItemClick(int data)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		if (this.player.inhandItemId > 0 && this.player.inhandItemCount == 0)
		{
			this.player.SetHandItems(0, 0, 0);
			return;
		}
		if (this.player.inhandItemId == 0 && this.player.inhandItemCount == 0)
		{
			if (!this.isLocal)
			{
				UIRealtimeTip.Popup("非本地星球拿取提示".Translate(), true, 0);
				return;
			}
			DispenserStore[] holdupPackage = dispenserComponent.holdupPackage;
			int count = holdupPackage[data].count;
			if (count <= 0)
			{
				return;
			}
			int itemId = holdupPackage[data].itemId;
			int inc = holdupPackage[data].inc;
			if (VFInput.shift || VFInput.control)
			{
				int upCount = this.player.TryAddItemToPackage(itemId, count, inc, false, 0, false);
				UIItemup.Up(itemId, upCount);
			}
			else
			{
				this.player.SetHandItemId_Unsafe(itemId);
				this.player.SetHandItemCount_Unsafe(count);
				this.player.SetHandItemInc_Unsafe(inc);
			}
			dispenserComponent.RemoveHoldupItem(data);
		}
	}

	// Token: 0x06004709 RID: 18185 RVA: 0x00397AE8 File Offset: 0x00395CE8
	private void OnCourierIconClick(int obj)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		if (this.player.inhandItemId > 0 && this.player.inhandItemCount == 0)
		{
			this.player.SetHandItems(0, 0, 0);
			return;
		}
		if (this.player.inhandItemId > 0 && this.player.inhandItemCount > 0)
		{
			int num = 5003;
			ItemProto itemProto = LDB.items.Select(num);
			if (this.player.inhandItemId != num)
			{
				UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name, true, 0);
				return;
			}
			ItemProto itemProto2 = LDB.items.Select((int)this.factory.entityPool[dispenserComponent.entityId].protoId);
			int num2 = (itemProto2 != null) ? itemProto2.prefabDesc.dispenserMaxCourierCount : 10;
			int num3 = dispenserComponent.idleCourierCount + dispenserComponent.workCourierCount;
			int num4 = num2 - num3;
			if (num4 < 0)
			{
				num4 = 0;
			}
			int num5 = (this.player.inhandItemCount < num4) ? this.player.inhandItemCount : num4;
			if (num5 <= 0)
			{
				UIRealtimeTip.Popup("栏位已满".Translate(), true, 0);
				return;
			}
			int inhandItemCount = this.player.inhandItemCount;
			int inhandItemInc = this.player.inhandItemInc;
			int num6 = num5;
			int num7 = this.split_inc(ref inhandItemCount, ref inhandItemInc, num6);
			dispenserComponent.idleCourierCount += num6;
			this.player.AddHandItemCount_Unsafe(-num6);
			this.player.SetHandItemInc_Unsafe(this.player.inhandItemInc - num7);
			if (this.player.inhandItemCount <= 0)
			{
				this.player.SetHandItemId_Unsafe(0);
				this.player.SetHandItemCount_Unsafe(0);
				this.player.SetHandItemInc_Unsafe(0);
				return;
			}
		}
		else if (this.player.inhandItemId == 0 && this.player.inhandItemCount == 0)
		{
			if (!this.isLocal)
			{
				UIRealtimeTip.Popup("非本地星球拿取提示".Translate(), true, 0);
				return;
			}
			int idleCourierCount = dispenserComponent.idleCourierCount;
			if (idleCourierCount <= 0)
			{
				return;
			}
			if (VFInput.shift || VFInput.control)
			{
				int upCount = this.player.TryAddItemToPackage(5003, idleCourierCount, 0, false, 0, false);
				UIItemup.Up(5003, upCount);
			}
			else
			{
				this.player.SetHandItemId_Unsafe(5003);
				this.player.SetHandItemCount_Unsafe(idleCourierCount);
				this.player.SetHandItemInc_Unsafe(0);
			}
			dispenserComponent.idleCourierCount = 0;
		}
	}

	// Token: 0x0600470A RID: 18186 RVA: 0x00397D84 File Offset: 0x00395F84
	private void OnCourierAutoReplenishButtonClick(int obj)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		dispenserComponent.courierAutoReplenish = !dispenserComponent.courierAutoReplenish;
		this.courierAutoReplenishButton.highlighted = dispenserComponent.courierAutoReplenish;
		this.ReplenishItemsIfNeeded(dispenserComponent.entityId);
	}

	// Token: 0x0600470B RID: 18187 RVA: 0x00397DF4 File Offset: 0x00395FF4
	private void OnPlayerIntendToTransferItems(int _itemId, int _itemCount, int _itemInc)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		if (_itemId == 5003 && this.courierIconButton.button.interactable)
		{
			this.OnCourierIconClick(_itemId);
		}
	}

	// Token: 0x0600470C RID: 18188 RVA: 0x00397E58 File Offset: 0x00396058
	public void OnMaxChargePowerSliderValueChange(float value)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		this.powerSystem.consumerPool[dispenserComponent.pcId].workEnergyPerTick = (long)(5000.0 * (double)value + 0.5);
		StringBuilderUtility.WriteKMG(this.powerServedSB, 8, (long)(300000.0 * (double)value + 0.5), false, '\u2009', ' ');
		this.maxChargePowerValue.text = this.powerServedSB.ToString();
	}

	// Token: 0x0600470D RID: 18189 RVA: 0x00397F0E File Offset: 0x0039610E
	public void OnInfoTabButtonClick(int index)
	{
		this.currentTabPanel = EUIControlPanelDispenserPanel.Info;
		this.RefreshTabPanelUI();
	}

	// Token: 0x0600470E RID: 18190 RVA: 0x00397F1D File Offset: 0x0039611D
	public void OnRouteTabButtonClick(int index)
	{
		this.currentTabPanel = EUIControlPanelDispenserPanel.Route;
		this.RefreshTabPanelUI();
	}

	// Token: 0x0600470F RID: 18191 RVA: 0x00397F2C File Offset: 0x0039612C
	private void ReplenishItemsIfNeeded(int entityId)
	{
		RectTransform tipPanelRect = UIRoot.instance.uiGame.generalTips.tipPanelRect;
		Vector3 vector = this.courierIconImage.rectTransform.TransformPoint(new Vector3(0f, 15f, 0f));
		vector = tipPanelRect.InverseTransformPoint(vector) + tipPanelRect.rect.size * 0.5f;
		this.factory.EntityAutoReplenishIfNeeded(entityId, vector, false);
	}

	// Token: 0x06004710 RID: 18192 RVA: 0x00397FB0 File Offset: 0x003961B0
	private void SetWarningTipVisible(int tipId)
	{
		string text = ("配送器设置提示" + tipId.ToString()).Translate();
		this.warningRectTransform.gameObject.SetActive(tipId > 0);
		this.warningTipBox.gameObject.SetActive(tipId > 0);
		this.warningTipText.text = text;
		this.guessFilterButton.gameObject.SetActive(tipId == 2);
	}

	// Token: 0x06004711 RID: 18193 RVA: 0x0039801C File Offset: 0x0039621C
	private void OnGuessFilterButtonClick(int obj)
	{
		if (this.dispenserId == 0 || this.factory == null)
		{
			return;
		}
		DispenserComponent dispenserComponent = this.transport.dispenserPool[this.dispenserId];
		if (dispenserComponent == null || dispenserComponent.id != this.dispenserId)
		{
			return;
		}
		if (dispenserComponent.filter == 0)
		{
			dispenserComponent.GuessFilter(this.factory);
		}
	}

	// Token: 0x06004712 RID: 18194 RVA: 0x00398074 File Offset: 0x00396274
	public void CalculateStorageTotalCount(DispenserComponent dispenser, out int count, out int inc)
	{
		count = 0;
		inc = 0;
		if (dispenser.storage != null && dispenser.filter > 0)
		{
			StorageComponent storageComponent = dispenser.storage;
			do
			{
				int num;
				count += storageComponent.GetItemCount(dispenser.filter, out num);
				inc += num;
				storageComponent = storageComponent.previousStorage;
			}
			while (storageComponent != null);
		}
	}

	// Token: 0x06004713 RID: 18195 RVA: 0x003980C4 File Offset: 0x003962C4
	private int split_inc(ref int n, ref int m, int p)
	{
		if (n == 0)
		{
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

	// Token: 0x06004714 RID: 18196 RVA: 0x00398108 File Offset: 0x00396308
	public void RefreshTabPanelUI()
	{
		this.routeTabText.text = "配对页签标题".Translate();
		this.infoTabText.text = "信息页签标题".Translate();
		int num = Mathf.Max(new int[]
		{
			60,
			(int)(this.infoTabText.preferredWidth + 10.5f),
			(int)(this.routeTabText.preferredWidth + 10.5f)
		});
		int num2 = 12;
		this.routeTabTrans.anchoredPosition = new Vector2((float)(-(float)num2), 8f);
		this.routeTabTrans.sizeDelta = new Vector2((float)num, 30f);
		num2 += num;
		this.infoTabTrans.anchoredPosition = new Vector2((float)(-(float)num2), 8f);
		this.infoTabTrans.sizeDelta = new Vector2((float)num, 30f);
		this.infoTabButton.highlighted = (this.currentTabPanel == EUIControlPanelDispenserPanel.Info);
		this.routeTabButton.highlighted = (this.currentTabPanel == EUIControlPanelDispenserPanel.Route);
		this.infoRectTransform.gameObject.SetActive(this.currentTabPanel == EUIControlPanelDispenserPanel.Info);
		this.uiRouteViewPanel.gameObject.SetActive(this.currentTabPanel == EUIControlPanelDispenserPanel.Route);
		if (this.currentTabPanel == EUIControlPanelDispenserPanel.Route)
		{
			this.uiRouteViewPanel.SetData(this.dispenser, this.factory);
			this.uiRouteViewPanel._Open();
			return;
		}
		this.uiRouteViewPanel._Close();
	}

	// Token: 0x040054DD RID: 21725
	[SerializeField]
	public UIControlPanelWindow masterWindow;

	// Token: 0x040054DE RID: 21726
	[Header("Top")]
	[SerializeField]
	public Text titleText;

	// Token: 0x040054DF RID: 21727
	[SerializeField]
	public Text planetNameText;

	// Token: 0x040054E0 RID: 21728
	[SerializeField]
	public Text planetDistance;

	// Token: 0x040054E1 RID: 21729
	[Header("Delivery")]
	[SerializeField]
	public RectTransform deliveryRectTransform;

	// Token: 0x040054E2 RID: 21730
	[SerializeField]
	public UIButton selectItemButton;

	// Token: 0x040054E3 RID: 21731
	[SerializeField]
	public UIButton itemButton;

	// Token: 0x040054E4 RID: 21732
	[SerializeField]
	public UIButton takeBackButton;

	// Token: 0x040054E5 RID: 21733
	[SerializeField]
	public Image itemImage;

	// Token: 0x040054E6 RID: 21734
	[NonSerialized]
	public Text currentCountLabel;

	// Token: 0x040054E7 RID: 21735
	[SerializeField]
	public Text currentCountText;

	// Token: 0x040054E8 RID: 21736
	[NonSerialized]
	public Text orderedCountLabel;

	// Token: 0x040054E9 RID: 21737
	[SerializeField]
	public Text orderedCountText;

	// Token: 0x040054EA RID: 21738
	[SerializeField]
	public RectTransform storageModeBox;

	// Token: 0x040054EB RID: 21739
	[SerializeField]
	public UIToggle playerModeToggle;

	// Token: 0x040054EC RID: 21740
	[SerializeField]
	public UIToggle storageModeToggle;

	// Token: 0x040054ED RID: 21741
	[SerializeField]
	public Text playerModeText;

	// Token: 0x040054EE RID: 21742
	[SerializeField]
	public Text storageModeText;

	// Token: 0x040054EF RID: 21743
	[SerializeField]
	public UIButton storageModeButton;

	// Token: 0x040054F0 RID: 21744
	[SerializeField]
	public UIMultipleModesSwitch playerModeSwitch;

	// Token: 0x040054F1 RID: 21745
	[SerializeField]
	public UISwitch storageModeSwitch;

	// Token: 0x040054F2 RID: 21746
	[SerializeField]
	public UIToggle recycleAllToggle;

	// Token: 0x040054F3 RID: 21747
	[SerializeField]
	public Image storageModeIcon;

	// Token: 0x040054F4 RID: 21748
	[SerializeField]
	public Text pairTipText;

	// Token: 0x040054F5 RID: 21749
	[Header("Warning")]
	[SerializeField]
	public RectTransform warningRectTransform;

	// Token: 0x040054F6 RID: 21750
	[SerializeField]
	public Text warningTipText;

	// Token: 0x040054F7 RID: 21751
	[SerializeField]
	public RectTransform warningTipBox;

	// Token: 0x040054F8 RID: 21752
	[SerializeField]
	public UIButton guessFilterButton;

	// Token: 0x040054F9 RID: 21753
	[SerializeField]
	public GameObject holdupItemGo;

	// Token: 0x040054FA RID: 21754
	[SerializeField]
	public UIButton[] holdupItemBtns;

	// Token: 0x040054FB RID: 21755
	[SerializeField]
	public Image[] holdupItemIcons;

	// Token: 0x040054FC RID: 21756
	[SerializeField]
	public Image[] holdupItemIncImages;

	// Token: 0x040054FD RID: 21757
	[SerializeField]
	public Text[] holdupItemCountTexts;

	// Token: 0x040054FE RID: 21758
	[Header("Setting")]
	[SerializeField]
	public RectTransform settingRectTransform;

	// Token: 0x040054FF RID: 21759
	[SerializeField]
	public Image powerIcon;

	// Token: 0x04005500 RID: 21760
	[SerializeField]
	public Text powerText;

	// Token: 0x04005501 RID: 21761
	[SerializeField]
	public Text stateText;

	// Token: 0x04005502 RID: 21762
	[SerializeField]
	public Text energyText;

	// Token: 0x04005503 RID: 21763
	[SerializeField]
	public Image energyBar;

	// Token: 0x04005504 RID: 21764
	[SerializeField]
	public UIButton courierIconButton;

	// Token: 0x04005505 RID: 21765
	[SerializeField]
	public Image courierIconImage;

	// Token: 0x04005506 RID: 21766
	[SerializeField]
	public Text courierCountText;

	// Token: 0x04005507 RID: 21767
	[SerializeField]
	public UIButton courierAutoReplenishButton;

	// Token: 0x04005508 RID: 21768
	[SerializeField]
	public Image[] itemIncs;

	// Token: 0x04005509 RID: 21769
	[SerializeField]
	public Slider maxChargePowerSlider;

	// Token: 0x0400550A RID: 21770
	[SerializeField]
	public Text maxChargePowerValue;

	// Token: 0x0400550B RID: 21771
	[Header("Buttom")]
	[SerializeField]
	public UIButton infoTabButton;

	// Token: 0x0400550C RID: 21772
	[SerializeField]
	public UIButton routeTabButton;

	// Token: 0x0400550D RID: 21773
	[SerializeField]
	public Text infoTabText;

	// Token: 0x0400550E RID: 21774
	[SerializeField]
	public Text routeTabText;

	// Token: 0x0400550F RID: 21775
	[SerializeField]
	public RectTransform infoTabTrans;

	// Token: 0x04005510 RID: 21776
	[SerializeField]
	public RectTransform routeTabTrans;

	// Token: 0x04005511 RID: 21777
	[SerializeField]
	public RectTransform infoRectTransform;

	// Token: 0x04005512 RID: 21778
	[SerializeField]
	public UIControlPanelDispenserRouteViewPanel uiRouteViewPanel;

	// Token: 0x04005513 RID: 21779
	[SerializeField]
	public RectTransform scrollContentRect;

	// Token: 0x04005514 RID: 21780
	[Header("Colors & Settings")]
	[SerializeField]
	public Color orderedNormalColor;

	// Token: 0x04005515 RID: 21781
	[SerializeField]
	public Color orderedPositiveColor;

	// Token: 0x04005516 RID: 21782
	[SerializeField]
	public Color orderedNagativeColor;

	// Token: 0x04005517 RID: 21783
	[SerializeField]
	public Color powerNormalColor;

	// Token: 0x04005518 RID: 21784
	[SerializeField]
	public Color powerNormalIconColor;

	// Token: 0x04005519 RID: 21785
	[SerializeField]
	public Color powerOffColor;

	// Token: 0x0400551A RID: 21786
	[SerializeField]
	public Color powerOffIconColor;

	// Token: 0x0400551B RID: 21787
	[SerializeField]
	public Color energyBarColor0;

	// Token: 0x0400551C RID: 21788
	[SerializeField]
	public Color energyBarColor1;

	// Token: 0x0400551D RID: 21789
	[SerializeField]
	public Color energyTextColor0;

	// Token: 0x0400551E RID: 21790
	[SerializeField]
	public Color energyTextColor1;

	// Token: 0x0400551F RID: 21791
	[SerializeField]
	public Color energyTextColor2;

	// Token: 0x04005520 RID: 21792
	[SerializeField]
	public Color idleColor;

	// Token: 0x04005521 RID: 21793
	[SerializeField]
	public Color chargeColor;

	// Token: 0x04005522 RID: 21794
	[SerializeField]
	public Color activeColor;

	// Token: 0x04005523 RID: 21795
	[SerializeField]
	public Color inactiveColor;

	// Token: 0x04005524 RID: 21796
	[Header("Pair Panel Colors")]
	[SerializeField]
	public Color unnamedColor;

	// Token: 0x04005525 RID: 21797
	[SerializeField]
	public Color renamedColor;

	// Token: 0x04005526 RID: 21798
	[SerializeField]
	public Color storagePairColor;

	// Token: 0x04005527 RID: 21799
	[SerializeField]
	public Color storageNotPairColor;

	// Token: 0x04005528 RID: 21800
	public PlanetFactory factory;

	// Token: 0x04005529 RID: 21801
	public PlanetTransport transport;

	// Token: 0x0400552A RID: 21802
	public PowerSystem powerSystem;

	// Token: 0x0400552B RID: 21803
	public Player player;

	// Token: 0x0400552C RID: 21804
	public DispenserComponent dispenser;

	// Token: 0x0400552D RID: 21805
	public PlanetData planet;

	// Token: 0x0400552E RID: 21806
	private GameData gameData;

	// Token: 0x0400552F RID: 21807
	private int _dispenserId;

	// Token: 0x04005530 RID: 21808
	private StringBuilder powerServedSB;

	// Token: 0x04005531 RID: 21809
	private StringBuilder powerAccumulatedSB;

	// Token: 0x04005532 RID: 21810
	private int deliveryRectOriginHeight;

	// Token: 0x04005533 RID: 21811
	private int warningRectOriginHeight;

	// Token: 0x04005534 RID: 21812
	private float planetNameTextMaxWidth;

	// Token: 0x04005535 RID: 21813
	public EUIControlPanelDispenserPanel currentTabPanel;

	// Token: 0x04005536 RID: 21814
	private bool pointerInIcon;

	// Token: 0x04005537 RID: 21815
	private bool insplit;
}
