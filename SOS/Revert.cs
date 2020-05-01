using UnityEngine;
using KSP.Localization;

namespace SOS
{
    public partial class SOS
    {

        public void Revert()
        {
            Log.Info("Revert, setting revertMenu true");
            revertMenu = true;
            _revertRect = new Rect((Screen.width - REVERTWIDTH) / 2, (Screen.height - REVERTHEIGHT) / 2, REVERTWIDTH, REVERTHEIGHT); // initted here to properly position the window
        }

        private void drawRevert(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(Localizer.Format("#SOS_020"));

            if (FlightDriver.CanRevertToPostInit && HighLogic.CurrentGame.Parameters.Flight.CanRestart)
            {
                if (GUILayout.Button(Localizer.Format("#autoLOC_418666", KSPUtil.PrintTime(Planetarium.GetUniversalTime() - FlightDriver.PostInitState.UniversalTime, 3, false))))
                {
                    revertMenu = false;
                    CloseWindow();
                    FlightDriver.RevertToLaunch();
                }
            }
            if (HighLogic.CurrentGame.Parameters.Flight.CanLeaveToEditor && FlightDriver.CanRevertToPrelaunch && ShipConstruction.ShipConfig != null)
            {
                EditorFacility shipType = ShipConstruction.ShipType;
                if (shipType != EditorFacility.VAB)
                {
                    if (shipType == EditorFacility.SPH)
                    {
                        if (GUILayout.Button(Localizer.Format("#autoLOC_418687", KSPUtil.PrintTime(Planetarium.GetUniversalTime() - FlightDriver.PostInitState.UniversalTime, 3, false))))
                        {
                            revertMenu = false;
                            FlightDriver.RevertToPrelaunch(EditorFacility.SPH);
                        };
                    }
                }
                else
                {
                    if (GUILayout.Button(Localizer.Format("#autoLOC_418682", KSPUtil.PrintTime(Planetarium.GetUniversalTime() - FlightDriver.PostInitState.UniversalTime, 3, false))))
                    {
                        revertMenu = false;
                        FlightDriver.RevertToPrelaunch(EditorFacility.VAB);
                    }
                }
            }
            if (GUILayout.Button(Localizer.Format("#autoLOC_455882")))
            {
                revertMenu = false;
            }


            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
