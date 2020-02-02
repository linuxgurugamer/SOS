using UnityEngine;
using KSP_Log;


namespace SOS
{
    public partial class SOS
    {
        public void onVesselChange(Vessel v)
        {
            vesselCanRevert = true;
        }

        public void SelectSavePanelActions()
        {
            // Save the data
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
                VesselInfo.Instance.SaveVesselInfo(FlightGlobals.ActiveVessel);
        }

        public void SelectExitPanelActions()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                VesselInfo.Instance.SaveVesselInfo(FlightGlobals.ActiveVessel);
            }
        }

        bool BLSG { get { return BetterLoadSaveGame.Main.fetch.Visible; } }
        bool gamePause = false;
        void onGamePause()
        {
            SOS.Log.Info("onGamePause");
            if ((BLSGAvailable && BLSG) || GameSettings.MODIFIER_KEY.GetKey(false))
            {
                return;
            }
            if (HighLogic.LoadedSceneIsFlight && FlightDriver.CanRevert && !gamePause /* && FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH */)
            {
                PauseMenu.Close();
                _windowRect = new Rect((Screen.width - WIDTH) / 2, (Screen.height - HEIGHT) / 2, WIDTH, HEIGHT);  //new Rect((float)(Screen.width / 2.0 - 125.0), (float)(Screen.height / 2.0 - 70.0), 250f, 130f);

                gamePause = true;
                FlightDriver.SetPause(true);

                axesChanged = VesselInfo.Instance.AxesChanged;
                actionsChanged = VesselInfo.Instance.ActionsChanged;
                stagingChanged = VesselInfo.Instance.StagingChanged;
            }
            else
                if (!FlightDriver.CanRevert)
                    Log.Info("Revert not possible");
        }

        void PauseMenuDismiss()
        {
            gamePause = false;
        }

        void onGameUnpause()
        {
            SOS.Log.Info("onGameUnpause");
            gamePause = false;
        }

        void onFlightReady()
        {
            simTermination = false;
            if (revertToLaunch)
            {
                RevertToLaunch();
                ScreenMessages.PostScreenMessage("Revert to Launch: ", 10.0f, ScreenMessageStyle.UPPER_CENTER);
            }
            if (revertToEditor)
            {
                RevertToEditor();
                ScreenMessages.PostScreenMessage("Revert to Editor: ", 10.0f, ScreenMessageStyle.UPPER_CENTER);
            }
            if (switchVessels)
            {
                ScreenMessages.PostScreenMessage("Switch Vessels: ", 10.0f, ScreenMessageStyle.UPPER_CENTER);
            }
            if (launch)
            {
                ScreenMessages.PostScreenMessage("Launch: ", 10.0f, ScreenMessageStyle.UPPER_CENTER);
            }

            revertToEditor = false;
            revertToLaunch = false;
            launch = false;
            switchVessels = false;

            postload = false;

            if (!VesselInfo.Instance.vesselInfoSaved)
            {
                //VesselInfo.Instance.actionList = new SortedDictionary<string, VesselInfo.ActionGroupInfo>();

                VesselInfo.Instance.InitActionLists();
                VesselInfo.Instance.InitAxesConfigNodes();
                VesselInfo.Instance.InitStagingLists();


                VesselInfo.Instance.SaveOriginalActionList(FlightGlobals.ActiveVessel);
                VesselInfo.Instance.SaveOriginalAxesList(FlightGlobals.ActiveVessel);
                VesselInfo.Instance.GetAllStaging(FlightGlobals.ActiveVessel);

                VesselInfo.Instance.SaveVesselInfo(FlightGlobals.ActiveVessel);

                axesChanged = VesselInfo.Instance.AxesChanged;
                actionsChanged = VesselInfo.Instance.ActionsChanged;
                stagingChanged = VesselInfo.Instance.StagingChanged;

            }

        }

        void onStageActivate(int i)
        {
#if false
            Log.Info("onStageActivate, FlightGlobals.ActiveVessel.Parts.Count: " + FlightGlobals.ActiveVessel.Parts.Count + ", VesselInfo.Instance.OriginalStagingPartCount: " + VesselInfo.Instance.OriginalStagingPartCount);
            if (FlightGlobals.ActiveVessel.Parts.Count == VesselInfo.Instance.OriginalStagingPartCount)
            {
                VesselInfo.Instance.SaveOriginalActionList(FlightGlobals.ActiveVessel, false);
                VesselInfo.Instance.GetAllAxesFromPartsAndModules(FlightGlobals.ActiveVessel, false);
                VesselInfo.Instance.GetAllStaging(FlightGlobals.ActiveVessel, false);
            }
#endif
        }

        void RevertToEditor()
        {
            VesselInfo.Instance.RestoreEditorData();
        }

        void RevertToLaunch()
        {

            VesselInfo.Instance.RestoreFlightData(FlightGlobals.ActiveVessel);
        }

        void onGameSceneSwitchRequested(GameEvents.FromToAction<GameScenes, GameScenes> fromtoaction)
        {
            if (postload)
            {
                postload = false;
                if (fromtoaction.from == GameScenes.FLIGHT && fromtoaction.to == GameScenes.FLIGHT)
                    revertToLaunch = true;
                if (fromtoaction.from == GameScenes.FLIGHT && fromtoaction.to == GameScenes.EDITOR)
                    revertToEditor = true;
            }
            else
            {
                if (fromtoaction.from == GameScenes.EDITOR && fromtoaction.to == GameScenes.FLIGHT)
                    launch = true;
                if (fromtoaction.from == GameScenes.FLIGHT && fromtoaction.to == GameScenes.FLIGHT)
                    switchVessels = true;
            }

        }

        void onGameStatePostLoad(ConfigNode c)
        {
            postload = true;
        }

        void onGameStateLoad(ConfigNode c)
        {
        }

        void onGameStateCreated(Game g)
        {
            postload = false;
        }

        void onGameStateSave(ConfigNode c)
        {
            postload = false;
        }
    }
}
