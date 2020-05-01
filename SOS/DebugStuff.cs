using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ToolbarControl_NS;
using KSP.UI.Screens;
using KSP.UI.Screens.Settings.Controls;
//using BetterLoadSaveGame;

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

    [KSPAddon(KSPAddon.Startup.FlightAndEditor, true)]
    public class DebugStuff : MonoBehaviour
    {
        internal static DebugStuff fetch = null;
        public void Start()
        {
            fetch = this;
            InitializeToolbar();
            DontDestroyOnLoad(this);
        }
        int cnt = 0;
        void LateUpdate()
        {
            if (scheduledEditorDump && HighLogic.LoadedSceneIsEditor)
            {
                if (cnt++ > 50)
                    StartCoroutine(DumpIn3());
            }
        }
        IEnumerator DumpIn3()
        {
            scheduledEditorDump = false;
            yield return new WaitForSeconds(3);
            EditorDump();
            PopupDialog.SpawnPopupDialog
            (
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                "SOS",
                "Debugging Editor Ship dump completed.",
                "Data dump completed\nPlease submit the Player.log for analysis",
                "OK",
                false,
                HighLogic.UISkin
            );

        }
        internal const string MODID = "SOS_NS";
        internal const string MODNAME = "Save Our Settings";
        internal ToolbarControl toolbarControl = null;

        internal void InitializeToolbar()
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().debugMode)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(DebugDump, DebugDump,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
                    MODID,
                    "SOSButton",
                    "SOS/PluginData/SOS-38",
                    "SOS/PluginData/SOS-24",
                    MODNAME
                );
            }
        }
        private void DebugDump()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                FlightDump();
            }
            else
            {
                EditorDump();
            }
        }

        public const string autoShipName = "Auto-Saved Ship";

        void FlightDump()
        {
            DumpParts("FlightDump", FlightGlobals.ActiveVessel.Parts, FlightGlobals.ActiveVessel.rootPart);

            string t = HighLogic.CurrentGame.Title;
            int
            i = t.IndexOf("(CAREER)");
            if (i != -1)
            {
                t = t.Remove(i, 8);
            }
            else
            {
                i = t.IndexOf("(SANDBOX)");
                if (i != -1)
                {
                    t = t.Remove(i, 9);
                }
                else
                {
                    i = t.IndexOf("(SCIENCE)");
                    if (i != -1)
                        t = t.Remove(i, 9);
                }
            }
            t = t.TrimEnd(' ');
            string e = "";
            EditorFacility shipType = ShipConstruction.ShipType;
            if (shipType != EditorFacility.VAB)
            {
                if (shipType == EditorFacility.SPH)
                {
                    e = "SPH";
                }
            }
            else
                e = "VAB";
            if (e != "")
            {
                string path = KSPUtil.ApplicationRootPath + "saves/" + t + "/Ships/" + e + "/" + autoShipName + ".craft";
                SOS.Log.Info("craft path: " + path);
                if (File.Exists(path))
                {
                    var sNode = ConfigNode.Load(path);
                    SOS.Log.Info("Ship: " + sNode.ToString());
                }
            }
            scheduledEditorDump = true;

            PopupDialog.SpawnPopupDialog
            (
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                "SOS",
                "Debugging Ship dump completed.",
                "Please revert to the editor,\nwait for a message,\nand then submit the Player.log for analysis",
                "OK",
                false,
                HighLogic.UISkin
            );
        }

        static bool scheduledEditorDump = false;
        void EditorDump()
        {
            DumpParts("EditorDump", EditorLogic.fetch.ship.parts, EditorLogic.RootPart);
            var sNode = EditorLogic.fetch.ship.SaveShip();
            SOS.Log.Info("Ship: " + sNode.ToString());
        }

        void DumpParts(string name, List<Part> partsList, Part rootPart)
        {
            StringBuilder craftInfo = new StringBuilder(name);
            craftInfo.Append("Root part: " + rootPart.partInfo.title);
            craftInfo.AppendLine();
            foreach (var p in partsList)
            {
                craftInfo.Append(name + ", part/stage: " + p.partInfo.partUrl + ":" + p.partInfo.name + ",  partInfo.title: " + p.partInfo.title + ", inverseStage: " + p.inverseStage + ", originalStage: " + p.originalStage);

                craftInfo.AppendLine();
            }

            SOS.Log.Info(craftInfo.ToString());
        }

        internal void OnDestroy()
        {
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
            toolbarControl = null;
        }
    }
}