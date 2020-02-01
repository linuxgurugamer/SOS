using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace SOS
{
     public partial class VesselInfo
    {
        static Dictionary<Guid, VesselInfo> vesselInfoList;

        internal static VesselInfo instance = null;
        internal static VesselInfo activeInstance = null;

        public static VesselInfo Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new VesselInfo();
                    activeInstance = instance;
                    if (vesselInfoList == null)
                        vesselInfoList = new Dictionary<Guid, VesselInfo>();
                }
                return instance;
            }
        }


        public enum SaveState { launch, prerevert };

        public SaveState saveState;
        public Guid id;
        internal bool vesselInfoSaved = false;

        public void SaveVesselInfo(Vessel v)
        {
            actionList = new SortedDictionary<string, ActionGroupInfo>();
            id = v.id;
            GetAllActionsFromPartsAndModules(v);
            GetAllAxesFromPartsAndModules(v);
            GetAllStaging(v);

            vesselInfoList.Remove(id); // No error if it doesn't exist, just returns false
            vesselInfoList.Add(id, this);
            vesselInfoSaved = true;
        }

        public void RemoveVessel(Vessel v)
        {
            if (vesselInfoList.ContainsKey(v.id))
                vesselInfoList.Remove(v.id);
        }



#if DEBUG
        void DumpAllActions(string fname)
        {
            if (actionList == null)
                return;

            var KSPActionGroupValues = Enum.GetValues(typeof(KSPActionGroup));

            string line = "";
            // Open the stream and write to it.
            using (FileStream fs = File.OpenWrite(fname))
            {

                foreach (var al in actionList)
                {
                    var a = al.Value;
                    if (a.actionGroup > KSPActionGroup.None)
                    {
                        line = al.Key + "::" + a.partName + ": ";
                        line += a.actionGroupName + ": ";
                        foreach (var v in KSPActionGroupValues)
                        {
                            if ((KSPActionGroup)v == KSPActionGroup.REPLACEWITHDEFAULT)
                                continue;
                            ulong v1 = Convert.ToUInt64(v);
                            if (v1 > 0)
                            {
                                ulong kag = Convert.ToUInt64(a.actionGroup);

                                if ((v1 & kag) == v1)
                                    line += v.ToString() + ", ";
                            }
                        }
                        line += "\r\n";
                        Byte[] array = Encoding.ASCII.GetBytes(line);
                        fs.Write(array, 0, line.Length);
                    }
                }
            }
        }
#endif


        public void RestoreFlightData(Vessel v)
        {
            if (vesselInfoList.ContainsKey(v.id))
            {
                VesselInfo i = vesselInfoList[v.id];

                if (SOS.fetch.saveActions)
                    i.RestoreActions(v);
                if (SOS.fetch.saveAxis)
                    i.RestoreAxes(v);
                if (SOS.fetch.saveStaging)
                    i.RestoreStaging(v);
            }
            else
            {
                // Should never get here

                SOS.Log.Error("RestoreData, vessel id not found");
                SOS.Log.Error("current v.id: " + v.id);
                foreach (var i in vesselInfoList.Keys)
                    SOS.Log.Error("vesselInfoList.Keys: " + i);
            }
        }

        public void RestoreEditorData()
        {
            if (SOS.fetch.saveActions && activeInstance.actionList != null)
                activeInstance.RestoreActions(EditorLogic.fetch.ship.parts);
            if (SOS.fetch.saveAxis && activeInstance.axesNodes != null)
                activeInstance.RestoreAxes(EditorLogic.fetch.ship.parts);
            if (SOS.fetch.saveStaging && activeInstance.stagingPartList != null)
                activeInstance.RestoreStaging(EditorLogic.fetch.ship.parts, EditorLogic.RootPart);

            InitActionLists();
            InitAxesConfigNodes();
            InitStagingLists();
        }
 
    }
}
