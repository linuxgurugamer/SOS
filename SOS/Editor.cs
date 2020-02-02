using System.Collections;
using UnityEngine;

namespace SOS
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class Editor : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(CheckForSavedData());
        }

        IEnumerator CheckForSavedData()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();
            VesselInfo.Instance.RestoreEditorData();

            SOS.fetch.saveStaging = HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().defaultSaveStaging;
            SOS.fetch.saveActions = HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().defaultSaveActions;
            SOS.fetch.saveAxis = HighLogic.CurrentGame.Parameters.CustomParams<S_O_S>().defaultSaveAxes;

            SOS.fetch.revertToEditor = false;
            SOS.fetch.revertToLaunch = false;
            SOS.fetch.launch = false;
            SOS.fetch.switchVessels = false;
            VesselInfo.Instance.vesselInfoSaved = false;

            yield break;
        }


#if false
        void ClearStaging(List<Part> partsList, Part rootPart)
        {
            foreach (var p in partsList)
                p.inverseStage =  
                    p.originalStage =
                        p.defaultInverseStage = 0;
            rootPart.inverseStage = rootPart.defaultInverseStage  = - 1;
            var SortIcons = typeof(KSP.UI.Screens.StageManager).GetMethod("SortIcons",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null,
                new Type[] { typeof(bool), typeof(Part), typeof(bool) }, null);


            //      This is ugly, but it works
            //      We call this private method for each part from the root, twice and it works.

            SetStages(rootPart, SortIcons);
            foreach (var p in partsList)
                p.inverseStage =
                    p.originalStage =
                        p.defaultInverseStage = 0;
            rootPart.inverseStage = rootPart.defaultInverseStage = -1;

            SetStages(rootPart, SortIcons);
            foreach (var p in partsList)
                SOS.Log.Info("ClearStaging, part/stage: " + p.partInfo.title + ", inverseStage: " + p.inverseStage + ", originalStage: " + p.originalStage);

        }
        internal void SetStages(Part part, System.Reflection.MethodInfo SortIcons)
        {
            var stageManager = KSP.UI.Screens.StageManager.Instance;

            if (part.stackIcon != null && part.stackIcon.StageIcon != null)
            {
                stageManager.HoldIcon(part.stackIcon.StageIcon);

                SortIcons.Invoke(stageManager, new object[] { true, part, false });
            }
            foreach (var child in part.children)
                SetStages(child, SortIcons);
        }
#endif
    }
}
