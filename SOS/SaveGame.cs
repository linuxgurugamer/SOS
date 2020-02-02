using System.IO;
using UnityEngine;
using KSP.Localization;

namespace SOS
{
    public partial class SOS
    {
        const string NAMEPREFIX = "quicksave #";
        string saveName;
        string GetSaveFileName()
        {
            int i = 1;
           
            while (File.Exists(KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/" + NAMEPREFIX + i.ToString() + ".sfs"))
                 i++;

            return NAMEPREFIX + i.ToString();
        }

        void ShowSaveGameDialog(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("#autoLOC_417232"), smallText);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            saveName = GUILayout.TextField(saveName, smallInputText);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (saveName == "")
                GUI.enabled = false;
            if (GUILayout.Button(Localizer.Format("#autoLOC_455877")))
            {
                GamePersistence.SaveGame(saveName, HighLogic.SaveFolder, SaveMode.OVERWRITE);
                showSaveGame = false;
                gamePause = false;
                FlightDriver.SetPause(false);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localizer.Format("autoLOC_455882")))
                showSaveGame = false;
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
    }
}
