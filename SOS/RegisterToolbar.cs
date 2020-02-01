using UnityEngine;
using ToolbarControl_NS;
using KSP.UI.Screens;

namespace SOS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(DebugStuff.MODID, DebugStuff.MODNAME);
        }
    }

    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class DebugStuff:MonoBehaviour
    {
        public void Start()
        {
            InitializeToolbar();
        }

        internal const string MODID = "SOS_NS";
        internal const string MODNAME = "Save Our Settings";
        ToolbarControl toolbarControl;
        private void InitializeToolbar()
        {
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(DebugDump, DebugDump,
                ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                MODID,
                "SOSButton",
                "SOS/PluginData/SOS-38",
                "SOS/PluginData/SOS-24",
                MODNAME
            );
        }
        private void DebugDump()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                FlightDump();
            } else
            {

            }
        }

        void FlightDump()
        {
            foreach (var p in FlightGlobals.ActiveVessel.Parts)
                SOS.Log.Info("DebugDump, part/stage: " + p.partInfo.title + ", inverseStage: " + p.inverseStage + ", originalStage: " + p.originalStage);

        }
    }
}