using System;
using System.Collections.Generic;

namespace SOS
{

    public partial class VesselInfo
    {
        public class StagingInfo
        {
            public Part part;


            public int originalStage;
            public int inverseStage;

            public int defaultInverseStage;


            public StagingInfo(Part p)
            {
                this.part = p;

                this.originalStage = p.originalStage;                
                this.inverseStage = p.inverseStage;
                this.defaultInverseStage = p.defaultInverseStage;

            }
        }

        internal Dictionary<string, StagingInfo> originalStagingPartList;
        internal Dictionary<string, StagingInfo> stagingPartList;

        internal bool StagingChanged
        {
            get
            {
                if (originalStagingPartList.Count != stagingPartList.Count)
                {
                    return true;
                }
                foreach (var k in originalStagingPartList.Keys)
                {
                    if (!stagingPartList.ContainsKey(k))
                    {
                        return true;
                    }
                    var cur = stagingPartList[k];
                    if (originalStagingPartList[k].inverseStage != cur.inverseStage ||
                        originalStagingPartList[k].originalStage != cur.originalStage ||
                        originalStagingPartList[k].defaultInverseStage != cur.defaultInverseStage)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        internal int OriginalStagingPartCount { get { return originalStagingPartList.Count; } }
        internal void GetAllStaging(Vessel v, bool original = true)
        {
            if (original)
                originalStagingPartList = GetAllStaging(v.Parts);
            stagingPartList = GetAllStaging(v.Parts);            
        }

        void GetAllStaging(Vessel v)
        {
            if (v.situation != Vessel.Situations.PRELAUNCH)
                return;
            stagingPartList = GetAllStaging(v.Parts);
        }

        Dictionary<string, StagingInfo>  GetAllStaging(List<Part> partsList)
        {
            SOS.Log.Info("GetAllStaging");
            Dictionary<string, StagingInfo> stagingList = new Dictionary<string, StagingInfo>();
            foreach (var p in partsList)
            {
                if (!stagingList.ContainsKey(p.persistentId.ToString()))
                {
                    StagingInfo si = new StagingInfo(p);
                    stagingList.Add(p.persistentId.ToString(), si);
                    SOS.Log.Info("GetAllStaging, part/stage: " + p.partInfo.title + ", inverseStage: " + si.inverseStage + ", originalStage: " + si.originalStage);
                }
            }
            return stagingList;
        }

        internal class Stages
        {
            internal int inverseStage;
            internal List<Part> partList;

            internal Stages(int invStage, Part p)
            {
                inverseStage = invStage;
                partList = new List<Part>();
                partList.Add(p);
            }
        }

        Dictionary<int, Stages> sd;
        int minStage, maxStage;


        internal void InitStagingLists()
        {
            stagingPartList = new Dictionary<string, StagingInfo>();
            originalStagingPartList = new Dictionary<string, StagingInfo>();
        }

        void RestoreStaging(Vessel v)
        {
            SOS.Log.Info("RestoreStaging");
            RestoreStaging(v.Parts, v.rootPart);
        }

        void RestoreStaging(List<Part> partsList, Part rootPart)
        {
            if (rootPart == null) // On initial entry into editor
            {
                InitStagingLists();
                return;
            }
            sd = new Dictionary<int, Stages>();
            minStage = 999;
            maxStage = -1;

            Stages stages;

            foreach (var p in partsList)
            {
                if (stagingPartList.ContainsKey(p.persistentId.ToString()))
                {
                    StagingInfo si = stagingPartList[p.persistentId.ToString()];

                    if (sd.TryGetValue(si.inverseStage, out stages))
                    {
                        stages.partList.Add(p);
                    }
                    else
                    {
                        stages = new Stages(si.inverseStage, p);
                        sd.Add(si.inverseStage, stages);
                    }
                    minStage = Math.Min(minStage, si.inverseStage);
                    maxStage = Math.Max(maxStage, si.inverseStage);

                    SOS.Log.Info("RestoreStaging, part/stage: " + p.partInfo.title + ", inverseStage: " + si.inverseStage + ", originalStage: " + si.originalStage);

                }
                else
                {
                    // Should never get here
                    SOS.Log.Error("Part not found in stagingPartList");
                }
            }

            // The following code has been adapted from SmartStage

            var SortIcons = typeof(KSP.UI.Screens.StageManager).GetMethod("SortIcons",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null,
                new Type[] { typeof(bool), typeof(Part), typeof(bool) }, null);


            //      This is ugly, but it works
            //      We call this private method for each part from the root, twice and it works.

            SetStages(rootPart, SortIcons);
            SetStages(rootPart, SortIcons);
        }

        internal void SetStages(Part part, System.Reflection.MethodInfo SortIcons)
        {
            int adj = 0;
            var stageManager = KSP.UI.Screens.StageManager.Instance;
#if false
            if (sd.ContainsKey(-1))
                foreach (var p in sd[-1].partList)
                    p.inverseStage = -1;
#endif

            for (int i = minStage; i <= maxStage; i++)
            {
                if (sd.ContainsKey(i))
                {
                    foreach (var p in sd[i].partList)
                    {
                        p.inverseStage = i - adj;
                    }
                }
                else
                    adj++;
            }

            if (part.stackIcon != null && part.stackIcon.StageIcon != null)
            {
                stageManager.HoldIcon(part.stackIcon.StageIcon);

                SortIcons.Invoke(stageManager, new object[] { true, part, false });
            }
            foreach (var child in part.children)
                SetStages(child, SortIcons);
        }
    }
}
