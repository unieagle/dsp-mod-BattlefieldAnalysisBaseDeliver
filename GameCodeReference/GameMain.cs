using System;
using UnityEngine;

// Token: 0x02000561 RID: 1377
public class GameMain : MonoBehaviour
{
	// Token: 0x17000586 RID: 1414
	// (get) Token: 0x06002FF1 RID: 12273 RVA: 0x002ACE49 File Offset: 0x002AB049
	// (set) Token: 0x06002FF2 RID: 12274 RVA: 0x002ACE50 File Offset: 0x002AB050
	public static GameMain instance { get; private set; }

	// Token: 0x17000587 RID: 1415
	// (get) Token: 0x06002FF3 RID: 12275 RVA: 0x002ACE58 File Offset: 0x002AB058
	public static bool notNull
	{
		get
		{
			return GameMain.instance == null;
		}
	}

	// Token: 0x17000588 RID: 1416
	// (get) Token: 0x06002FF4 RID: 12276 RVA: 0x002ACE65 File Offset: 0x002AB065
	public static bool isNull
	{
		get
		{
			return GameMain.instance == null;
		}
	}

	// Token: 0x17000589 RID: 1417
	// (get) Token: 0x06002FF5 RID: 12277 RVA: 0x002ACE72 File Offset: 0x002AB072
	// (set) Token: 0x06002FF6 RID: 12278 RVA: 0x002ACE8B File Offset: 0x002AB08B
	public static string gameName
	{
		get
		{
			if (GameMain.data == null)
			{
				return "";
			}
			return GameMain.data.gameName;
		}
		set
		{
			if (GameMain.data != null)
			{
				GameMain.data.gameName = value;
			}
		}
	}

	// Token: 0x1700058A RID: 1418
	// (get) Token: 0x06002FF7 RID: 12279 RVA: 0x002ACE9F File Offset: 0x002AB09F
	public static bool isLoading
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._loading;
		}
	}

	// Token: 0x1700058B RID: 1419
	// (get) Token: 0x06002FF8 RID: 12280 RVA: 0x002ACEBA File Offset: 0x002AB0BA
	public static bool isEnded
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._ended;
		}
	}

	// Token: 0x1700058C RID: 1420
	// (get) Token: 0x06002FF9 RID: 12281 RVA: 0x002ACED5 File Offset: 0x002AB0D5
	public static bool loadErrored
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._loadErrored;
		}
	}

	// Token: 0x1700058D RID: 1421
	// (get) Token: 0x06002FFA RID: 12282 RVA: 0x002ACEF0 File Offset: 0x002AB0F0
	public static bool isRunning
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._running;
		}
	}

	// Token: 0x1700058E RID: 1422
	// (get) Token: 0x06002FFB RID: 12283 RVA: 0x002ACF0B File Offset: 0x002AB10B
	public static bool isPaused
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._paused;
		}
	}

	// Token: 0x1700058F RID: 1423
	// (get) Token: 0x06002FFC RID: 12284 RVA: 0x002ACF26 File Offset: 0x002AB126
	// (set) Token: 0x06002FFD RID: 12285 RVA: 0x002ACF41 File Offset: 0x002AB141
	public static bool isFullscreenPaused
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._fullscreenPaused;
		}
		set
		{
			if (GameMain.instance != null)
			{
				GameMain.instance._fullscreenPaused = value;
			}
		}
	}

	// Token: 0x17000590 RID: 1424
	// (get) Token: 0x06002FFE RID: 12286 RVA: 0x002ACF5B File Offset: 0x002AB15B
	// (set) Token: 0x06002FFF RID: 12287 RVA: 0x002ACF77 File Offset: 0x002AB177
	public static long gameTick
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return 0L;
			}
			return GameMain.instance.timei;
		}
		set
		{
			if (GameMain.instance != null)
			{
				GameMain.instance.timei = value;
			}
		}
	}

	// Token: 0x17000591 RID: 1425
	// (get) Token: 0x06003000 RID: 12288 RVA: 0x002ACF94 File Offset: 0x002AB194
	public static double gameTime
	{
		get
		{
			if (GameMain.instance != null)
			{
				GameMain.instance.timef = (double)GameMain.instance.timei * 0.016666666666666666;
				return GameMain.instance.timef;
			}
			return 0.0;
		}
	}

	// Token: 0x17000592 RID: 1426
	// (get) Token: 0x06003001 RID: 12289 RVA: 0x002ACFE1 File Offset: 0x002AB1E1
	// (set) Token: 0x06003002 RID: 12290 RVA: 0x002ACFFD File Offset: 0x002AB1FD
	public static long onceGameTick
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return 0L;
			}
			return GameMain.instance.timei_once;
		}
		set
		{
			if (GameMain.instance != null)
			{
				GameMain.instance.timei_once = value;
			}
		}
	}

	// Token: 0x17000593 RID: 1427
	// (get) Token: 0x06003003 RID: 12291 RVA: 0x002AD018 File Offset: 0x002AB218
	public static double onceGameTime
	{
		get
		{
			if (GameMain.instance != null)
			{
				GameMain.instance.timef_once = (double)GameMain.instance.timei_once * 0.016666666666666666;
				return GameMain.instance.timef_once;
			}
			return 0.0;
		}
	}

	// Token: 0x17000594 RID: 1428
	// (get) Token: 0x06003004 RID: 12292 RVA: 0x002AD065 File Offset: 0x002AB265
	// (set) Token: 0x06003005 RID: 12293 RVA: 0x002AD080 File Offset: 0x002AB280
	public static bool sandboxToolsEnabled
	{
		get
		{
			return GameMain.instance != null && GameMain.instance._sandboxToolsEnabled;
		}
		set
		{
			if (GameMain.instance != null)
			{
				GameMain.instance._sandboxToolsEnabled = value;
			}
		}
	}

	// Token: 0x17000595 RID: 1429
	// (get) Token: 0x06003006 RID: 12294 RVA: 0x002AD09A File Offset: 0x002AB29A
	public static DateTime creationTime
	{
		get
		{
			return GameMain.data.gameDesc.creationTime;
		}
	}

	// Token: 0x17000596 RID: 1430
	// (get) Token: 0x06003007 RID: 12295 RVA: 0x002AD0AB File Offset: 0x002AB2AB
	// (set) Token: 0x06003008 RID: 12296 RVA: 0x002AD0B2 File Offset: 0x002AB2B2
	public static bool inOtherScene { get; set; }

	// Token: 0x17000597 RID: 1431
	// (get) Token: 0x06003009 RID: 12297 RVA: 0x002AD0BA File Offset: 0x002AB2BA
	public bool running
	{
		get
		{
			return this._running;
		}
	}

	// Token: 0x14000087 RID: 135
	// (add) Token: 0x0600300A RID: 12298 RVA: 0x002AD0C4 File Offset: 0x002AB2C4
	// (remove) Token: 0x0600300B RID: 12299 RVA: 0x002AD0F8 File Offset: 0x002AB2F8
	public static event Action onGameEnded;

	// Token: 0x17000598 RID: 1432
	// (get) Token: 0x0600300C RID: 12300 RVA: 0x002AD12B File Offset: 0x002AB32B
	public static GameHistoryData history
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.history;
		}
	}

	// Token: 0x17000599 RID: 1433
	// (get) Token: 0x0600300D RID: 12301 RVA: 0x002AD140 File Offset: 0x002AB340
	public static GamePrefsData preferences
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.preferences;
		}
	}

	// Token: 0x1700059A RID: 1434
	// (get) Token: 0x0600300E RID: 12302 RVA: 0x002AD155 File Offset: 0x002AB355
	public static GameStatData statistics
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.statistics;
		}
	}

	// Token: 0x1700059B RID: 1435
	// (get) Token: 0x0600300F RID: 12303 RVA: 0x002AD16A File Offset: 0x002AB36A
	public static GalaxyData galaxy
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.galaxy;
		}
	}

	// Token: 0x1700059C RID: 1436
	// (get) Token: 0x06003010 RID: 12304 RVA: 0x002AD17F File Offset: 0x002AB37F
	public static StarData localStar
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.localStar;
		}
	}

	// Token: 0x1700059D RID: 1437
	// (get) Token: 0x06003011 RID: 12305 RVA: 0x002AD194 File Offset: 0x002AB394
	public static PlanetData localPlanet
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.localPlanet;
		}
	}

	// Token: 0x1700059E RID: 1438
	// (get) Token: 0x06003012 RID: 12306 RVA: 0x002AD1A9 File Offset: 0x002AB3A9
	public static Player mainPlayer
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.mainPlayer;
		}
	}

	// Token: 0x1700059F RID: 1439
	// (get) Token: 0x06003013 RID: 12307 RVA: 0x002AD1BE File Offset: 0x002AB3BE
	// (set) Token: 0x06003014 RID: 12308 RVA: 0x002AD1C5 File Offset: 0x002AB3C5
	public static UniverseSimulator universeSimulator { get; private set; }

	// Token: 0x170005A0 RID: 1440
	// (get) Token: 0x06003015 RID: 12309 RVA: 0x002AD1CD File Offset: 0x002AB3CD
	public static SpaceSector spaceSector
	{
		get
		{
			if (GameMain.data == null)
			{
				return null;
			}
			return GameMain.data.spaceSector;
		}
	}

	// Token: 0x170005A1 RID: 1441
	// (get) Token: 0x06003016 RID: 12310 RVA: 0x002AD1E2 File Offset: 0x002AB3E2
	// (set) Token: 0x06003017 RID: 12311 RVA: 0x002AD1E9 File Offset: 0x002AB3E9
	public static SectorModel sectorModel { get; private set; }

	// Token: 0x170005A2 RID: 1442
	// (get) Token: 0x06003018 RID: 12312 RVA: 0x002AD1F1 File Offset: 0x002AB3F1
	public static GPUInstancingManager gpuiManager
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return null;
			}
			return GameMain.instance._gpuiManager;
		}
	}

	// Token: 0x170005A3 RID: 1443
	// (get) Token: 0x06003019 RID: 12313 RVA: 0x002AD20C File Offset: 0x002AB40C
	public static BPGPUInstancingManager bpgpuiManager
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return null;
			}
			return GameMain.instance._bpgpuiManager;
		}
	}

	// Token: 0x170005A4 RID: 1444
	// (get) Token: 0x0600301A RID: 12314 RVA: 0x002AD227 File Offset: 0x002AB427
	public static SSGPUInstancingManager ssgpuiManager
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return null;
			}
			return GameMain.instance._ssgpuiManager;
		}
	}

	// Token: 0x170005A5 RID: 1445
	// (get) Token: 0x0600301B RID: 12315 RVA: 0x002AD242 File Offset: 0x002AB442
	public static StarmapGPUInstancingManager starmapgpuiManager
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return null;
			}
			return GameMain.instance._starmapgpuiManager;
		}
	}

	// Token: 0x170005A6 RID: 1446
	// (get) Token: 0x0600301C RID: 12316 RVA: 0x002AD25D File Offset: 0x002AB45D
	public static IntelliAssist assist
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return null;
			}
			return GameMain.instance._intelliAssist;
		}
	}

	// Token: 0x170005A7 RID: 1447
	// (get) Token: 0x0600301D RID: 12317 RVA: 0x002AD278 File Offset: 0x002AB478
	public static IconSet iconSet
	{
		get
		{
			if (!(GameMain.instance != null))
			{
				return null;
			}
			return GameMain.instance._iconSet;
		}
	}

	// Token: 0x0600301E RID: 12318 RVA: 0x002AD294 File Offset: 0x002AB494
	public static void Begin()
	{
		if (GameMain.instance != null)
		{
			Debug.Log("GameMain.Begin()");
			if (!GameMain.instance.isMenuDemo && GameMain.creationTime > new DateTime(2021, 7, 24))
			{
				Debug.Log("Game Creation Time: UTC-" + GameMain.creationTime.ToString("yyyy-MM-dd HH:mm:ss"));
			}
			GameMain.instance._loading = false;
			GameMain.instance._running = true;
			GameMain.instance.CreateLogic();
			DSPGame.achievementSystem.OnGameBegin();
			DSPGame.propertySystem.OnGameBegin();
			UIRoot.instance.OnGameBegin();
			GameMain.universeSimulator.OnGameBegin();
			if (!GameMain.instance.isMenuDemo)
			{
				if (GameMain.gameScenario != null)
				{
					GameMain.gameScenario.Free();
					GameMain.gameScenario = null;
				}
				GameMain.gameScenario = new GameScenarioLogic(GameMain.data);
				GameMain.gameScenario.Init();
			}
			MainCamera.onPostRender += GameMain.instance.OnPostDraw;
			if (GameMain.gameTick == 0L && GameMain.data.guideComplete)
			{
				GameMain.gameScenario.NotifyOnGameStart();
			}
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.NotifyOnGameBegin();
			}
			GameMain.instance._intelliAssist = new IntelliAssist();
			GameMain.instance._intelliAssist.Init(GameMain.data);
			GameMain.instance.disableLogisticShip = false;
			GameMain.instance.disableTrash = false;
			PerformanceMonitor.ResetStatistics();
			PerformanceMonitor.ResetCpuStatAverageWindow();
			FPSController.ResetStatistics();
			FPSController.SetFixUPS(0.0);
			GameMain.instance.InitLogic();
		}
	}

	// Token: 0x0600301F RID: 12319 RVA: 0x002AD428 File Offset: 0x002AB628
	public static void End()
	{
		if (GameMain.instance != null)
		{
			if (GameMain.gameTime > 0.0 && GameMain.data.guideComplete)
			{
				if (GameMain.errored)
				{
					if (GameMain.isRunning && !GameSave.dontSaveToLastExit && !DSPGame.IsMenuDemo)
					{
						Debug.LogWarning("Auto saving to <auto save errored>.");
						GameSave.AutoSaveAfterErrored();
					}
				}
				else if (GameMain.isRunning && !GameSave.dontSaveToLastExit && !DSPGame.IsMenuDemo)
				{
					Debug.Log("Auto saving to <last exit>");
					GameSave.SaveAsLastExit();
				}
			}
			Debug.Log("GameMain.End()");
			UniverseGen.End();
			if (GameMain.gameScenario != null)
			{
				GameMain.gameScenario.Free();
				GameMain.gameScenario = null;
			}
			if (GameMain.instance._intelliAssist != null)
			{
				GameMain.instance._intelliAssist.Free();
				GameMain.instance._intelliAssist = null;
			}
			GameMain.instance._running = false;
			GameMain.instance._ended = true;
			UIRoot.instance.OnGameEnd();
			GameMain.universeSimulator.OnGameEnd();
			DSPGame.achievementSystem.OnGameEnd();
			MainCamera.onPostRender -= GameMain.instance.OnPostDraw;
			if (GameMain.onGameEnded != null)
			{
				GameMain.onGameEnded();
			}
			PerformanceMonitor.ResetStatistics();
			PerformanceMonitor.ResetDataStats();
			FPSController.ResetStatistics();
			PowerSystemRenderer.RevertStaticSettings();
			DefenseSystemRenderer.RevertStaticSettings();
			EntitySignRenderer.RevertStaticSettings();
			GameMain.instance.EndLogic();
		}
	}

	// Token: 0x06003020 RID: 12320 RVA: 0x002AD582 File Offset: 0x002AB782
	public static void Pause()
	{
		if (GameMain.instance != null && !GameMain.isPaused)
		{
			Debug.Log("GameMain.Pause()");
			GameMain.instance._paused = true;
		}
	}

	// Token: 0x06003021 RID: 12321 RVA: 0x002AD5AD File Offset: 0x002AB7AD
	public static void Resume()
	{
		if (GameMain.instance != null && GameMain.isPaused)
		{
			Debug.Log("GameMain.Resume()");
			GameMain.instance._paused = false;
		}
	}

	// Token: 0x06003022 RID: 12322 RVA: 0x002AD5D8 File Offset: 0x002AB7D8
	public static void PauseToOtherScene()
	{
		GameMain.Pause();
		GameMain.inOtherScene = true;
	}

	// Token: 0x06003023 RID: 12323 RVA: 0x002AD5E5 File Offset: 0x002AB7E5
	public static void ResumeFromOtherScene()
	{
		GameMain.Resume();
		GameMain.inOtherScene = false;
	}

	// Token: 0x06003024 RID: 12324 RVA: 0x002AD5F2 File Offset: 0x002AB7F2
	public static void HandleApplicationQuit()
	{
		if (GameMain.instance != null)
		{
			GameMain.End();
		}
	}

	// Token: 0x06003025 RID: 12325 RVA: 0x002AD606 File Offset: 0x002AB806
	public static void UnlockFullscreenPauseOneFrame()
	{
		if (GameMain.instance != null)
		{
			GameMain.instance._fullscreenPausedUnlockOneFrame = true;
		}
	}

	// Token: 0x06003026 RID: 12326 RVA: 0x002AD620 File Offset: 0x002AB820
	public static void UnlockPlayerKilledPauseOneFrame()
	{
		if (GameMain.instance != null)
		{
			GameMain.instance._playerKilledPausedUnlockOneFrame = true;
		}
	}

	// Token: 0x06003027 RID: 12327 RVA: 0x002AD63C File Offset: 0x002AB83C
	private void Awake()
	{
		GameMain.instance = this;
		this._loading = true;
		this._running = false;
		GameMain.errored = false;
		this._ended = false;
		SpaceSector.InitPrefabDescArray();
		PlanetFactory.InitPrefabDescArray();
		this.CreateGPUInstancing();
		this.CreateSSGPUInstancing();
		this.CreateBPGPUInstancing();
		this.CreateStarmapGPUInstancing();
	}

	// Token: 0x06003028 RID: 12328 RVA: 0x002AD68C File Offset: 0x002AB88C
	private void Start()
	{
		UniverseGen.Start();
		ColliderPoolLF.Create();
		AudioPoolLF.Create();
		ColliderPool.Create();
		AudioPool.Create();
		if (GameMain.data == null)
		{
			GameMain.data = new GameData();
			bool flag = false;
			string loadFile = DSPGame.LoadFile;
			if (DSPGame.GameDesc != null)
			{
				flag = GameMain.data.NewGame(DSPGame.GameDesc);
			}
			else if (!string.IsNullOrEmpty(DSPGame.LoadFile))
			{
				flag = GameSave.LoadCurrentGame(DSPGame.LoadFile);
			}
			else if (DSPGame.LoadDemoIndex > 0 || DSPGame.IsMenuDemo)
			{
				flag = GameSave.LoadCurrentGameInResource(DSPGame.LoadDemoIndex);
			}
			if (flag)
			{
				this._sandboxToolsEnabled = DSPGame.WillEnableSandboxTools;
				GameMain.data.gameDesc.goalLevel = DSPGame.LoadWithGoalLevel;
				GameMain.errored = false;
				if (!DSPGame.IsMenuDemo)
				{
					GameMain.data.galaxy.StartAutoScanning();
				}
				this.CreateUniverseSimulator();
				this.CreateIconSet();
				if (!(UIRoot.instance != null) || DSPGame.IsMenuDemo)
				{
					return;
				}
				try
				{
					UIRoot.instance.OnGameMainObjectCreated();
					GameMain.data.preferences.Restore();
					return;
				}
				catch (Exception message)
				{
					Debug.LogError(message);
					return;
				}
			}
			GameMain.errored = true;
			this._loadErrored = true;
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06003029 RID: 12329 RVA: 0x002AD7C4 File Offset: 0x002AB9C4
	private void OnDestroy()
	{
		Debug.Log("GameMain.OnDestroy()");
		if (GameMain.gameScenario != null)
		{
			GameMain.gameScenario.Free();
			GameMain.gameScenario = null;
		}
		this._running = false;
		this._ended = true;
		if (UIRoot.instance != null)
		{
			try
			{
				UIRoot.instance.OnGameEnd();
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		this.DestroyIconSet();
		ColliderPool.Destroy();
		AudioPool.Destroy();
		ColliderPoolLF.Destroy();
		AudioPoolLF.Destroy();
		this.DestroyGPUInstancing();
		this.DestroyBPGPUInstancing();
		this.DestroySSGPUInstancing();
		this.DestroyStarmapGUPInstancing();
		this.DestroyUniverseSimulator();
		if (GameMain.data != null)
		{
			GameMain.data.Destroy();
			GameMain.data = null;
		}
		UICursor.ResetCursor();
		PlanetFactory.ClearStaticEvents();
		GameMain.instance = null;
	}

	// Token: 0x0600302A RID: 12330 RVA: 0x002AD890 File Offset: 0x002ABA90
	private void CreateLogic()
	{
		if (GameMain.logic != null)
		{
			GameMain.logic.Free();
		}
		GameMain.logic = new GameLogic();
	}

	// Token: 0x0600302B RID: 12331 RVA: 0x002AD8AD File Offset: 0x002ABAAD
	private void InitLogic()
	{
		GameMain.logic.Init(GameMain.data);
	}

	// Token: 0x0600302C RID: 12332 RVA: 0x002AD8BE File Offset: 0x002ABABE
	private void EndLogic()
	{
		if (GameMain.logic != null)
		{
			GameMain.logic.Free();
			GameMain.logic = null;
		}
	}

	// Token: 0x0600302D RID: 12333 RVA: 0x002AD8D8 File Offset: 0x002ABAD8
	public int DetermineGameTickRate()
	{
		if (GameMain.data.mainPlayer.isAlive)
		{
			if (this._fullscreenPaused && !this._fullscreenPausedUnlockOneFrame)
			{
				return 0;
			}
			this._fullscreenPausedUnlockOneFrame = false;
			return 1;
		}
		else
		{
			if (this._playerKilledPausedUnlockOneFrame)
			{
				this._playerKilledPausedUnlockOneFrame = false;
				return 1;
			}
			int timeSinceKilled = GameMain.data.mainPlayer.timeSinceKilled;
			if (timeSinceKilled < 90 || GameMain.data.mainPlayer.respawning)
			{
				return 1;
			}
			if (timeSinceKilled >= 200)
			{
				return 0;
			}
			double num = (double)(timeSinceKilled - 90) / 110.0;
			double num2 = num / 0.016666666666666666;
			double num3 = 2.0 * Math.Abs(num2 - Math.Floor(num2 + 0.5));
			if (Math.Exp(-3.0 * num) <= num3)
			{
				return 0;
			}
			return 1;
		}
	}

	// Token: 0x0600302E RID: 12334 RVA: 0x002AD9A8 File Offset: 0x002ABBA8
	private void FixedUpdate()
	{
		if (!this._running)
		{
			return;
		}
		GameMain.data.mainPlayer.ApplyGamePauseState(this._paused || (this._fullscreenPaused && !this._fullscreenPausedUnlockOneFrame));
		if (!this._paused)
		{
			int num = this.DetermineGameTickRate();
			for (int i = 0; i < num; i++)
			{
				if (GameMain.data.guideComplete)
				{
					this.timei += 1L;
					this.timei_once += 1L;
				}
				this.timef = (double)this.timei * 0.016666666666666666;
				this.timef_once = (double)this.timei_once * 0.016666666666666666;
				DeepProfiler.BeginMajorSample(DPEntry.LogicTick, -1, this.timei);
				GameMain.logic.LogicFrame();
				DeepProfiler.EndMajorSample(-1);
			}
		}
	}

	// Token: 0x0600302F RID: 12335 RVA: 0x002ADA7F File Offset: 0x002ABC7F
	private void Update()
	{
		DeepProfiler.BeginSample(DPEntry.PlanetLoading, -1, -1L);
		UniverseGen.Update();
		DeepProfiler.EndSample(-1, -2L);
		if (!this._running)
		{
			return;
		}
		if (GameMain.logic != null)
		{
			GameMain.logic.Update();
		}
	}

	// Token: 0x06003030 RID: 12336 RVA: 0x002ADAB3 File Offset: 0x002ABCB3
	private void LateUpdate()
	{
		if (!this._running)
		{
			return;
		}
		if (GameMain.logic != null)
		{
			GameMain.logic.LateUpdate();
		}
		this.OnDraw();
		this.PausingInputLogic();
	}

	// Token: 0x06003031 RID: 12337 RVA: 0x002ADADC File Offset: 0x002ABCDC
	private void OnDraw()
	{
		GameMain.iconSet.SetShaderVars();
		if (!GameMain.inOtherScene && GameMain.logic != null)
		{
			DeepProfiler.BeginSample(DPEntry.Drawing, -1, -1L);
			GameMain.logic.Draw(this.disableLogisticShip, this.disableTrash);
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x06003032 RID: 12338 RVA: 0x002ADB2A File Offset: 0x002ABD2A
	private void OnPostDraw(Camera cam)
	{
		if (!GameMain.isPaused && GameMain.logic != null)
		{
			DeepProfiler.BeginSample(DPEntry.Drawing, -1, -1L);
			GameMain.logic.DrawPost();
			DeepProfiler.EndSample(-1, -2L);
		}
	}

	// Token: 0x06003033 RID: 12339 RVA: 0x002ADB58 File Offset: 0x002ABD58
	private void PausingInputLogic()
	{
		if (!this.isMenuDemo && this._running && VFInput.delayedEscape && !GameMain.inOtherScene && !GameMain.mainPlayer.respawning)
		{
			VFInput.UseEscape();
			if (this._paused)
			{
				GameMain.Resume();
				return;
			}
			GameMain.Pause();
		}
	}

	// Token: 0x06003034 RID: 12340 RVA: 0x002ADBA7 File Offset: 0x002ABDA7
	private void CreateGPUInstancing()
	{
		if (this._gpuiManager != null)
		{
			this._gpuiManager.Free();
		}
		this._gpuiManager = new GPUInstancingManager();
		this._gpuiManager.Init();
	}

	// Token: 0x06003035 RID: 12341 RVA: 0x002ADBD2 File Offset: 0x002ABDD2
	private void DestroyGPUInstancing()
	{
		if (this._gpuiManager != null)
		{
			this._gpuiManager.Free();
			this._gpuiManager = null;
		}
	}

	// Token: 0x06003036 RID: 12342 RVA: 0x002ADBEE File Offset: 0x002ABDEE
	private void CreateBPGPUInstancing()
	{
		if (this._bpgpuiManager != null)
		{
			this._bpgpuiManager.Free();
		}
		this._bpgpuiManager = new BPGPUInstancingManager();
		this._bpgpuiManager.Init();
	}

	// Token: 0x06003037 RID: 12343 RVA: 0x002ADC19 File Offset: 0x002ABE19
	private void DestroyBPGPUInstancing()
	{
		if (this._bpgpuiManager != null)
		{
			this._bpgpuiManager.Free();
			this._bpgpuiManager = null;
		}
	}

	// Token: 0x06003038 RID: 12344 RVA: 0x002ADC35 File Offset: 0x002ABE35
	private void CreateSSGPUInstancing()
	{
		if (this._ssgpuiManager != null)
		{
			this._ssgpuiManager.Free();
		}
		this._ssgpuiManager = new SSGPUInstancingManager();
		this._ssgpuiManager.Init();
	}

	// Token: 0x06003039 RID: 12345 RVA: 0x002ADC60 File Offset: 0x002ABE60
	private void DestroySSGPUInstancing()
	{
		if (this._ssgpuiManager != null)
		{
			this._ssgpuiManager.Free();
			this._ssgpuiManager = null;
		}
	}

	// Token: 0x0600303A RID: 12346 RVA: 0x002ADC7C File Offset: 0x002ABE7C
	private void CreateStarmapGPUInstancing()
	{
		if (this._starmapgpuiManager != null)
		{
			this._starmapgpuiManager.Free();
		}
		this._starmapgpuiManager = new StarmapGPUInstancingManager();
		this._starmapgpuiManager.Init();
	}

	// Token: 0x0600303B RID: 12347 RVA: 0x002ADCA7 File Offset: 0x002ABEA7
	private void DestroyStarmapGUPInstancing()
	{
		if (this._starmapgpuiManager != null)
		{
			this._starmapgpuiManager.Free();
			this._starmapgpuiManager = null;
		}
	}

	// Token: 0x0600303C RID: 12348 RVA: 0x002ADCC3 File Offset: 0x002ABEC3
	private void CreateIconSet()
	{
		if (this._iconSet != null)
		{
			this._iconSet.Destroy();
		}
		this._iconSet = new IconSet();
		this._iconSet.Create();
	}

	// Token: 0x0600303D RID: 12349 RVA: 0x002ADCEE File Offset: 0x002ABEEE
	private void DestroyIconSet()
	{
		if (this._iconSet != null)
		{
			this._iconSet.Destroy();
			this._iconSet = null;
		}
	}

	// Token: 0x0600303E RID: 12350 RVA: 0x002ADD0C File Offset: 0x002ABF0C
	private void CreateUniverseSimulator()
	{
		if (GameMain.universeSimulator != null)
		{
			Object.Destroy(GameMain.universeSimulator.gameObject);
		}
		GameMain.universeSimulator = Object.Instantiate<UniverseSimulator>(Configs.builtin.universeSimulatorPrefab);
		GameMain.universeSimulator.gameObject.name = "Universe";
		GameMain.universeSimulator.galaxyData = GameMain.galaxy;
		GameMain.sectorModel = GameMain.universeSimulator.gameObject.AddComponent<SectorModel>();
		GameMain.sectorModel.Init(GameMain.data);
		GameMain.ssgpuiManager.sectorModel = GameMain.sectorModel;
		GameMain.starmapgpuiManager.sectorModel = GameMain.sectorModel;
		GameMain.starmapgpuiManager.gameData = GameMain.data;
		GameMain.universeSimulator.OnGameLoaded();
	}

	// Token: 0x0600303F RID: 12351 RVA: 0x002ADDC6 File Offset: 0x002ABFC6
	private void DestroyUniverseSimulator()
	{
		if (GameMain.universeSimulator != null)
		{
			GameMain.universeSimulator.OnGameShut();
			Object.Destroy(GameMain.universeSimulator.gameObject);
			GameMain.universeSimulator = null;
			GameMain.sectorModel = null;
		}
	}

	// Token: 0x040036D6 RID: 14038
	private bool _sandboxToolsEnabled;

	// Token: 0x040036D8 RID: 14040
	public const int tickPerSecI = 60;

	// Token: 0x040036D9 RID: 14041
	public const double tickPerSec = 60.0;

	// Token: 0x040036DA RID: 14042
	public const double tickDeltaTime = 0.016666666666666666;

	// Token: 0x040036DB RID: 14043
	public long timei;

	// Token: 0x040036DC RID: 14044
	[NonSerialized]
	public double timef;

	// Token: 0x040036DD RID: 14045
	public long timei_once;

	// Token: 0x040036DE RID: 14046
	[NonSerialized]
	public double timef_once;

	// Token: 0x040036DF RID: 14047
	private bool _running;

	// Token: 0x040036E0 RID: 14048
	private bool _loading;

	// Token: 0x040036E1 RID: 14049
	private bool _loadErrored;

	// Token: 0x040036E2 RID: 14050
	private bool _paused;

	// Token: 0x040036E3 RID: 14051
	private bool _fullscreenPaused;

	// Token: 0x040036E4 RID: 14052
	private bool _fullscreenPausedUnlockOneFrame;

	// Token: 0x040036E5 RID: 14053
	private bool _playerKilledPausedUnlockOneFrame;

	// Token: 0x040036E6 RID: 14054
	private bool _ended;

	// Token: 0x040036E7 RID: 14055
	public bool isMenuDemo;

	// Token: 0x040036E9 RID: 14057
	public static GameData data;

	// Token: 0x040036EA RID: 14058
	public static GameLogic logic;

	// Token: 0x040036ED RID: 14061
	private GPUInstancingManager _gpuiManager;

	// Token: 0x040036EE RID: 14062
	private BPGPUInstancingManager _bpgpuiManager;

	// Token: 0x040036EF RID: 14063
	private SSGPUInstancingManager _ssgpuiManager;

	// Token: 0x040036F0 RID: 14064
	private StarmapGPUInstancingManager _starmapgpuiManager;

	// Token: 0x040036F1 RID: 14065
	private IntelliAssist _intelliAssist;

	// Token: 0x040036F2 RID: 14066
	private IconSet _iconSet;

	// Token: 0x040036F3 RID: 14067
	public static GameScenarioLogic gameScenario;

	// Token: 0x040036F4 RID: 14068
	public static bool errored;

	// Token: 0x040036F5 RID: 14069
	public bool disableLogisticShip;

	// Token: 0x040036F6 RID: 14070
	public bool disableTrash;
}
