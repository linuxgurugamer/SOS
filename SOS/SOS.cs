using System.Collections;
using UnityEngine;
using KSP_Log;
using KSP.Localization;
using KSP.UI.Screens;
using KRASH;

namespace SOS
{
    [KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, true)]
    public partial class SOS : MonoBehaviour
    {
        static public SOS fetch = null;

        internal bool revertToEditor = false;
        internal bool revertToLaunch = false;
        internal bool launch = false;
        internal bool switchVessels = false;

        bool revertMenu = false;

        bool postload = false;

        public static KSP_Log.Log Log;

        // Do this in the Awake so it will be available in other Starts
        void Awake()
        {
            Log = new KSP_Log.Log("SOS");
        }

        bool BLSGAvailable = false;
        public void Start()
        {
            BLSGAvailable = HasMod("BetterLoadSaveGame");
            fetch = this;
            var i = VesselInfo.Instance; // this initializes the Instance

            GameEvents.onGameSceneSwitchRequested.Add(onGameSceneSwitchRequested);
            GameEvents.onGameStatePostLoad.Add(onGameStatePostLoad);
            GameEvents.onGameStateLoad.Add(onGameStateLoad);
            GameEvents.onGameStateCreated.Add(onGameStateCreated);
            GameEvents.onGameStateSave.Add(onGameStateSave);
            GameEvents.onFlightReady.Add(onFlightReady);
            GameEvents.onStageActivate.Add(onStageActivate);


            GameEvents.onGamePause.Add(onGamePause);
            GameEvents.onGameUnpause.Add(onGameUnpause);
            GameEvents.onVesselChange.Add(onVesselChange);
            //GameEvents.onVesselWasModified.Add(onVesselWasModified);

            GameEvents.StageManager.OnGUIStageSequenceModified.Add(OnGUIStageSequenceModified);

            DontDestroyOnLoad(this);

            saveStaging = HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().defaultSaveStaging;
            saveActions = HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().defaultSaveActions;
            saveAxis = HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().defaultSaveAxes;
        }

        void OnGUIStageSequenceModified()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                VesselInfo.Instance.SaveOriginalActionList(FlightGlobals.ActiveVessel, false);
                VesselInfo.Instance.GetAllAxesFromPartsAndModules(FlightGlobals.ActiveVessel, false);
                VesselInfo.Instance.GetAllStaging(FlightGlobals.ActiveVessel, false);
            }
        }
        void OnDestroy()
        {

            GameEvents.onGameSceneSwitchRequested.Remove(onGameSceneSwitchRequested);
            GameEvents.onGameStatePostLoad.Remove(onGameStatePostLoad);
            GameEvents.onGameStateLoad.Remove(onGameStateLoad);
            GameEvents.onGameStateCreated.Remove(onGameStateCreated);
            GameEvents.onGameStateSave.Remove(onGameStateSave);
            GameEvents.onFlightReady.Remove(onFlightReady);
            GameEvents.onStageActivate.Remove(onStageActivate);


            GameEvents.onGamePause.Remove(onGamePause);
            GameEvents.onGameUnpause.Remove(onGameUnpause);
            GameEvents.onVesselChange.Remove(onVesselChange);
            //GameEvents.onVesselWasModified.Remove(onVesselWasModified);
            GameEvents.StageManager.OnGUIStageSequenceModified.Remove(OnGUIStageSequenceModified);
        }

        /*******************************************************************************/
        public enum GoTo
        {
            None,
            TrackingStation,
            SpaceCenter,
            MissionControl,
            Administration,
            RnD,
            AstronautComplex,
            VAB,
            SPH,
            LastVessel,
            Recover,
            Revert,
            RevertToEditor,
            RevertToSpaceCenter,
            MainMenu,
            Settings,
            Configurations
        }

        bool vesselCanRevert = true;

        string SaveGame = "persistent";

        bool added = false;
        void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (!added)
            {
                if (ActionGroupsFlightController.Instance.exitButton != null)
                {
                    ActionGroupsFlightController.Instance.exitButton.onClick.AddListener(SelectExitPanelActions);
                    added = true;
                }
                if (ActionGroupsFlightController.Instance.saveButton != null)
                {
                    ActionGroupsFlightController.Instance.saveButton.onClick.AddListener(SelectSavePanelActions);
                    added = true;
                }
            }
            if (vesselCanRevert && !FlightDriver.CanRevert)
            {
                VesselInfo.Instance.RemoveVessel(FlightGlobals.ActiveVessel);
                vesselCanRevert = false;
            }
#if false
            if (FlightDriver.CanRevert)
            {
                if (VesselInfo.Instance.originalActionList == null ||
                    VesselInfo.Instance.originalAxesNodes == null ||
                    VesselInfo.Instance.originalStagingPartList == null)
                    SaveVesselInfoInFlight();
            }
#endif
        }

#if false
        void SaveVesselInfoInFlight()
        {
            return;
            SOS.Log.Info("SaveVesselInfoInFlight");
            VesselInfo.Instance.actionList = new SortedDictionary<string, VesselInfo.ActionGroupInfo>();
            VesselInfo.Instance.SaveOriginalActionList(FlightGlobals.ActiveVessel);
            VesselInfo.Instance.SaveOriginalAxesList(FlightGlobals.ActiveVessel);
            VesselInfo.Instance.GetAllStaging(FlightGlobals.ActiveVessel);
        }
#endif
        private const float SPACER = 5.0f;
        const float WIDTH = 350f;
        const float REVERTWIDTH = 450;
        const float REVERTHEIGHT = 100;
        const float HEIGHT = 200f;

        const float SAVEGAMEWIDTH = 300f;
        const float SAVEGAMEHEIGHT = 150f;

        private Rect _windowRect = new Rect((Screen.width - WIDTH) / 2, (Screen.height - HEIGHT) / 2, WIDTH, HEIGHT);
        private Rect _revertRect;
        private Rect _saveGameRect = new Rect((Screen.width - SAVEGAMEWIDTH) / 2, (Screen.height - SAVEGAMEHEIGHT) / 2, SAVEGAMEWIDTH, SAVEGAMEHEIGHT);
        internal bool saveStaging = false, saveActions = false, saveAxis = false;
        bool axesChanged = false, actionsChanged = false, stagingChanged = false;
        bool simTermination = false;
        bool showSaveGame = false;
        void OnGUI()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null /*&& FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH */ && !simTermination)
            {
                GUI.skin = HighLogic.Skin;

                if (showSaveGame)
                {
                    _saveGameRect = GUILayout.Window(this.GetInstanceID() + 1, _saveGameRect, new GUI.WindowFunction(ShowSaveGameDialog), "Quicksave As...", new GUILayoutOption[0]);

                }
                else
                {
                    if (revertMenu)
                    {
                        _revertRect = GUILayout.Window(this.GetInstanceID() + 1, _revertRect, new GUI.WindowFunction(drawRevert), "Reverting Flight", new GUILayoutOption[0]);

                    }
                    else
                    {
                        if (gamePause && vesselCanRevert)
                        {
                           // if (firstPause)
                            {
                                axesChanged = VesselInfo.Instance.AxesChanged;
                                actionsChanged = VesselInfo.Instance.ActionsChanged;
                                stagingChanged = VesselInfo.Instance.StagingChanged;
                            }
                            // Checking the time here protects against keybounce and the same key being read 2x

                            _windowRect = GUILayout.Window(this.GetInstanceID() + 1, _windowRect, new GUI.WindowFunction(DisplayPauseMenu), "Game Paused", new GUILayoutOption[0]);
                        }
                    }
                }
            }
        }

        void Update()
        {
#if false
            if (GameSettings.MODIFIER_KEY.GetKey(false))
            {
                gamePause = false;
                firstPause = true;
                return;
            }
#endif
            if (GameSettings.PAUSE.GetKeyUp(true))
            {
                if (gamePause && !delayedCloseWindowActive)
                    StartCoroutine(DelayedCloseWindow());
                else
                    onGamePause();
            }
        }

        bool delayedCloseWindowActive = false;
        IEnumerator DelayedCloseWindow()
        {
            delayedCloseWindowActive = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            CloseWindow(false);
            delayedCloseWindowActive = false;
            FlightDriver.SetPause(false);
            yield return null;
        }

        void CloseWindow(bool unpause = false)
        {
            gamePause = false;
            if (unpause)
            {
                FlightDriver.SetPause(false);
            }
        }

        // Needed for KRASH support
        void TerminateSim()
        {
            simTermination = true;
            try
            {
                APIManager.ApiInstance.simAPI.TerminateSimNoDialog();
            }
            catch
            {
                Log.Error("KRASH needs updating");
                ScreenMessages.PostScreenMessage("KRASH needs to be updated for this to work ", 30.0f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        bool guistyleInitted = false;
        GUIStyle orangeButton;
        GUIStyle buttonSkin;
        GUIStyle textField;

        private void DisplayPauseMenu(int id)
        {
            if (!guistyleInitted)
            {
                InitGUI();
            }
            if (PauseMenu.isOpen)
            {
                PauseMenu.Close();
                FlightDriver.SetPause(true);
            }
            GUILayout.TextField(HighLogic.CurrentGame.Title, textField);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (KRASHWrapper.KRASHAvailable && KRASHWrapper.simulationActive())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Localizer.Format("SOS_001"), buttonSkin))
                {
                    CloseWindow(true);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Localizer.Format("#SOS_002"), orangeButton))
                {
                    TerminateSim();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Localizer.Format("#autoLOC_360624"), buttonSkin))
                {
                    GoToSettings();
                    CloseWindow();
                }

                GUILayout.FlexibleSpace();
            }
            else
            {
                if (GUILayout.Button(Localizer.Format("#autoLOC_360539"), buttonSkin))
                {
                    CloseWindow(true);
                }

                if (GUILayout.Button(Localizer.Format("#autoLOC_360586"), orangeButton))
                {
                    if (CanSpaceCenter)
                    {
                        gotoSpaceCenter();
                        CloseWindow();

                        Log.Info(GetText(GoTo.SpaceCenter));
                        return;
                    }
                    screenMSG(GoTo.SpaceCenter);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(SPACER);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(Localizer.Format("#autoLOC_360545"), buttonSkin))
                {
                    Revert();
                }
                if (GUILayout.Button(Localizer.Format("#autoLOC_360600"), orangeButton))
                {
                    ClearSpaceCenter();
                    GamePersistence.SaveGame(SaveGame, HighLogic.SaveFolder, SaveMode.OVERWRITE);
                    InputLockManager.ClearControlLocks();
                    Log.Info(GetText(GoTo.TrackingStation));
                    loadScene(GameScenes.TRACKSTATION);
                    CloseWindow();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(SPACER);
                GUILayout.BeginHorizontal();

                if (BLSGAvailable)
                {
                    if (GUILayout.Button(Localizer.Format("#autoLOC_417146"), buttonSkin))
                    {
                        LoadSave();
                        CloseWindow();
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button(Localizer.Format("#SOS_003"), buttonSkin);
                    GUI.enabled = true;
                }

                if (GUILayout.Button(Localizer.Format("#autoLOC_900734"), buttonSkin))
                {
                    GoToSettings();
                    CloseWindow();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(SPACER);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(Localizer.Format("#autoLOC_360553"), buttonSkin))
                {
                    showSaveGame = true;
                    saveName = GetSaveFileName();
                }

                if (!KRASHWrapper.KRASHAvailable || !KRASHWrapper.simulationActive())
                {
                    if (GUILayout.Button(Localizer.Format("#autoLOC_360644"), orangeButton))
                    {
                        GoToMainMenu();
                        CloseWindow();
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);


            string text;
            Vector2 size;

            text = Localizer.Format("#SOS_010");
            GUIContent content = new GUIContent(text);
            size = GUI.skin.textField.CalcSize(content);
            var leftSpace = (_windowRect.width - size.x + 24) / 2;

            if (stagingChanged)
            {

                GUILayout.BeginHorizontal();
                GUILayout.Space(leftSpace);
                saveStaging = GUILayout.Toggle(saveStaging, text, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (actionsChanged)
            {
                text = Localizer.Format("#SOS_011");

                GUILayout.BeginHorizontal();
                GUILayout.Space(leftSpace);
                saveActions = GUILayout.Toggle(saveActions, text, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }
            if (axesChanged)
            {
                text = Localizer.Format("#SOS_012");

                GUILayout.BeginHorizontal();
                GUILayout.Space(leftSpace);
                saveAxis = GUILayout.Toggle(saveAxis, text, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }

            GUI.DragWindow();
        }
        GUIStyle smallText;
        GUIStyle smallInputText;
        void InitGUI()
        {
            smallText = new GUIStyle(GUI.skin.label);
            smallText.fontSize -= 2;
            smallInputText = new GUIStyle(GUI.skin.textField);
            smallInputText.fontSize -= 2;

            buttonSkin = new GUIStyle(GUI.skin.button);
            buttonSkin.fixedWidth = 165;

            textField = new GUIStyle(GUI.skin.textField);
            textField.alignment = TextAnchor.MiddleCenter;



            orangeButton = new GUIStyle(buttonSkin);
            orangeButton.fixedWidth = 165;
            orangeButton.normal.textColor =
                orangeButton.hover.textColor =
                orangeButton.focused.textColor =
                orangeButton.active.textColor =
                orangeButton.normal.textColor = new Color(1, 128f / 255f, 0);
            guistyleInitted = true;
        }


        void screenMSG(GoTo scene)
        {
            Log.Warning("You can't " + GetText(scene));
            ScreenMessages.PostScreenMessage(Localizer.Format("quickgoto_cant", GetText(scene)), 10, ScreenMessageStyle.UPPER_RIGHT);
        }

        public string GetText(GoTo goTo, bool force = false)
        {
            switch (goTo)
            {
                case GoTo.TrackingStation:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_ts");
                case GoTo.SpaceCenter:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_sc");
                case GoTo.MissionControl:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_mc");
                case GoTo.Administration:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_admin");
                case GoTo.RnD:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_rnd");
                case GoTo.AstronautComplex:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_ac");
                case GoTo.VAB:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_vab");
                case GoTo.SPH:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_sph");
                case GoTo.Recover:
                    return Localizer.Format("quickgoto_recover");
                case GoTo.Revert:
                    return Localizer.Format("quickgoto_revert") + " " + Localizer.Format("quickgoto_launch");
                case GoTo.RevertToEditor:
                    return Localizer.Format("quickgoto_revert") + " " + Localizer.Format("quickgoto_editor");
                case GoTo.RevertToSpaceCenter:
                    return Localizer.Format("quickgoto_revert") + " " + Localizer.Format("quickgoto_sc");
                case GoTo.MainMenu:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_mainMenu");
                case GoTo.Settings:
                    return Localizer.Format("quickgoto_goto") + " " + Localizer.Format("quickgoto_toSettings");

            }
            return string.Empty;
        }

        void loadScene(GameScenes scenes, EditorFacility facility = EditorFacility.None)
        {
            if (scenes != GameScenes.EDITOR)
            {
                HighLogic.LoadScene(scenes);
            }
            else if (facility != EditorFacility.None)
            {
                EditorFacility editorFacility = EditorFacility.None;
                if (ShipConstruction.ShipConfig != null)
                {
                    editorFacility = ShipConstruction.ShipType;
                }
                if (HighLogic.LoadedSceneIsFlight && FlightGlobals.fetch)
                {
                    FlightGlobals.PersistentVesselIds.Clear();
                    FlightGlobals.PersistentLoadedPartIds.Clear();
                    FlightGlobals.PersistentUnloadedPartIds.Clear();
                }
                EditorDriver.StartupBehaviour = (editorFacility == facility ? EditorDriver.StartupBehaviours.LOAD_FROM_CACHE : EditorDriver.StartupBehaviours.START_CLEAN);
                EditorDriver.StartEditor(facility);
            }
            InputLockManager.ClearControlLocks();
        }

        void gotoSpaceCenter(GameBackup gameBackup = null)
        {
            if (gameBackup == null)
            {
                GamePersistence.SaveGame(SaveGame, HighLogic.SaveFolder, SaveMode.OVERWRITE);
            }
            else
            {
                GamePersistence.SaveGame(gameBackup, SaveGame, HighLogic.SaveFolder, SaveMode.OVERWRITE);
            }
            loadScene(GameScenes.SPACECENTER);
        }

        void ClearSpaceCenter()
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
            {
                return;
            }
            if (isLaunchScreen)
            {
                GameEvents.onGUILaunchScreenDespawn.Fire();
            }
            if (isMissionControl)
            {
                GameEvents.onGUIMissionControlDespawn.Fire();
            }
            if (isAdministration)
            {
                GameEvents.onGUIAdministrationFacilityDespawn.Fire();
            }

            InputLockManager.ClearControlLocks();
        }

        public bool isLaunchScreen
        {
            get
            {
                return VesselSpawnDialog.Instance != null;
            }
        }
        public bool isMissionControl
        {
            get
            {
                return MissionControl.Instance != null;
            }
        }

        public bool isAdministration
        {
            get
            {
                return Administration.Instance != null;
            }
        }

        public bool CanSpaceCenter
        {
            get
            {
                if (HighLogic.LoadedSceneIsGame)
                {
                    if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                    {
                        if (!HighLogic.LoadedSceneIsFlight)
                        {
                            return true;
                        }
                        if (FlightGlobals.ready && FlightGlobals.ActiveVessel != null && HighLogic.CurrentGame.Parameters.Flight.CanLeaveToSpaceCenter)
                        {
                            return FlightGlobals.ActiveVessel.IsClearToSave() == ClearToSaveStatus.CLEAR;
                        }
                    }
                }
                return false;
            }

        }
        public bool CanMainMenu
        {
            get
            {
                if (!HighLogic.LoadedSceneIsFlight)
                {
                    return true;
                }
                if (FlightGlobals.ready && FlightGlobals.ActiveVessel != null && HighLogic.CurrentGame.Parameters.Flight.CanLeaveToMainMenu)
                {
                    return FlightGlobals.ClearToSave() == ClearToSaveStatus.CLEAR;
                }
                return false;
            }
        }
        MiniSettings miniSettings;
        public void GoToSettings()
        {
            this.miniSettings = MiniSettings.Create(new Callback(this.onMiniSettingsFinished));
            CloseWindow();

            screenMSG(GoTo.Settings);
        }
        private void onMiniSettingsFinished()
        {
            this.miniSettings = null;
            gamePause = true;
            FlightDriver.SetPause(true, false);
        }


        public void GoToMainMenu()
        {
            if (CanMainMenu)
            {
                ClearSpaceCenter();
                GamePersistence.SaveGame(SaveGame, HighLogic.SaveFolder, SaveMode.OVERWRITE);
                Log.Info(GetText(GoTo.MainMenu));
                //                StartCoroutine(loadScene(GameScenes.MAINMENU));
                loadScene(GameScenes.MAINMENU);
                return;
            }
            screenMSG(GoTo.MainMenu);
        }

        public void LoadSave()
        {
            BetterLoadSaveGame.Main.fetch.EnableDialog();
        }

        private static bool HasMod(string modIdent)
        {
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                if (modIdent == a.name)
                    return true;
            }
            return false;
        }

    }
}
