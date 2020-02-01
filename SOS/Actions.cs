using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using KSP_Log;
using KSP.Localization;
using KSP.UI.Screens;


namespace SOS
{
    public partial class VesselInfo
    {
        public class ActionGroupInfo : IEquatable<ActionGroupInfo>
        {
            public string partName;
            public uint persistentId;
            public string partModuleName;
            public uint partModulePeristentId;
            public int partModuleNum;
            public KSPActionGroup baseActionActionGroup;
            public string actionGroupName;
            public int actionNum;
            public KSPActionGroup actionGroup;

            public bool Equals(ActionGroupInfo other)
            {
                if (partName != other.partName || persistentId != other.persistentId)
                    return false;
                if (partModuleName != other.partModuleName ||
                    partModuleNum != other.partModuleNum ||
                    partModulePeristentId != other.partModulePeristentId)
                    return false;
                if (baseActionActionGroup != other.baseActionActionGroup)
                    return false;
                if (actionNum != other.actionNum)
                    return false;
                if (actionGroup != other.actionGroup)
                    return false;
                return true;
            }

            public ActionGroupInfo(Part part, BaseAction baseAction, int actionNum, KSPActionGroup actionGroup)
            {
                this.partName = part.partInfo.name;
                this.persistentId = part.persistentId;
                partModuleName = null;
                partModuleNum = 0;
                partModulePeristentId = 0;
                this.baseActionActionGroup = baseAction.actionGroup;
                this.actionGroupName = baseAction.name;
                this.actionNum = actionNum;
                this.actionGroup = actionGroup;

            }
            public ActionGroupInfo(Part part, BaseAction baseAction, int actionNum, PartModule partModule, int partModuleNum, KSPActionGroup actionGroup)
            {
                this.partName = part.partInfo.name;
                this.persistentId = part.persistentId;
                this.baseActionActionGroup = baseAction.actionGroup;
                this.actionGroupName = baseAction.name;
                this.partModuleName = partModule.moduleName;
                this.partModulePeristentId = partModule.PersistentId;
                this.partModuleNum = partModuleNum;
                this.actionGroup = actionGroup;
                this.actionNum = actionNum;
            }

            public string Key { get { return KeyFrom(partName, persistentId, partModuleName, partModulePeristentId, actionNum); } }

            public static string KeyFrom(Part part, PartModule partModule, int partModuleNum, int actionNum)
            {
                return KeyFrom(part, partModule, actionNum);
            }

            public static string KeyFrom(Part part, PartModule partModule, int actionNum)
            {
                if (partModule != null)
                    return KeyFrom(part.partInfo.name, part.persistentId, partModule.moduleName, partModule.PersistentId, actionNum);
                else
                    return KeyFrom(part.partInfo.name, part.persistentId, null, 0, actionNum);
            }
            public static string KeyFrom(string partName, uint partPersistentId, string partModuleName, uint partModulePersistentId, int actionNum)
            {
                string key = partName + ":" + partPersistentId.ToString();
                if (partModuleName != null)
                    key += ":" + partModuleName + ":" + partModulePersistentId.ToString();
                key += ":" + actionNum.ToString();
                return key;
            }


            public override string ToString()
            {
                string str =
                    partName + ":" +
                    persistentId.ToString() + ":" +
                    partModuleName + ":" +
                    partModulePeristentId.ToString() + ":" +
                    partModuleNum.ToString() + ":" +
                    baseActionActionGroup.ToString() + ":" +
                    actionGroupName.ToString() + ":" +
                    actionNum.ToString() + ":" +
                    actionGroup.ToString();


                return str;
            }
        }

        internal SortedDictionary<string, ActionGroupInfo> originalActionList;
        internal SortedDictionary<string, ActionGroupInfo> actionList;

        internal bool ActionsChanged
        {
            get
            {
                if (originalActionList == null)
                {
                    return false;
                }
                if (actionList == null)
                {
                    return false;
                }
                if (actionList.Count != originalActionList.Count)
                {
                    return true;
                }
                foreach (var o in originalActionList)
                {
                    if (!actionList.ContainsKey(o.Key))
                    {
                        return true;
                    }
                    if (!o.Value.Equals(actionList[o.Key]))
                    {
                        return true;
                    }
                }
                return false;
            }
        }



        internal void SaveOriginalActionList( Vessel v, bool original = true)
        {
            if (original)
                originalActionList = GetAllActionsFromPartsAndModules(v.Parts);
            actionList = GetAllActionsFromPartsAndModules(v.Parts);
        }

        void GetAllActionsFromPartsAndModules(Vessel v)
        {
            if (v.situation != Vessel.Situations.PRELAUNCH)
                return;

            actionList = GetAllActionsFromPartsAndModules(v.Parts);
        }

        SortedDictionary<string, ActionGroupInfo> GetAllActionsFromPartsAndModules(List<Part> Parts)
        {
            SortedDictionary<string, ActionGroupInfo> localActionList = new SortedDictionary<string, ActionGroupInfo>();

            for (int partNum = 0; partNum < Parts.Count; partNum++)
            {
                Part part = Parts[partNum];

                if (part != null)
                {
                    // Add BaseActions in the part
                    for (int actionNum = 0; actionNum < part.Actions.Count; actionNum++)
                    {
                        var action = part.Actions[actionNum];
                        if (action.actionGroup >= KSPActionGroup.Stage)
                        {
                            ActionGroupInfo agi = new ActionGroupInfo(part, action, actionNum, action.actionGroup);
                            localActionList.Add(agi.Key, agi);
                        }
                    }
                    // Add BaseActions in the part modules.
                    for (int moduleNum = 0; moduleNum < part.Modules.Count; moduleNum++)
                    {
                        PartModule module = part.Modules[moduleNum];

                        for (int actionNum = 0; actionNum < module.Actions.Count; actionNum++)
                        {
                            BaseAction action = module.Actions[actionNum];
                            if (action.actionGroup >= KSPActionGroup.Stage)
                            {
                                ActionGroupInfo agi = new ActionGroupInfo(part, action, actionNum, module, moduleNum, action.actionGroup);

                                localActionList.Add(agi.Key, agi); ;
                            }

                        }
                    }
                }
            }
#if DEBUG
            DumpAllActions("actions.config");
#endif
            return localActionList;
        }

        void RestoreActions(Vessel v)
        {
            RestoreActions(v.Parts);
#if DEBUG
            DumpAllActions("restored.config");
#endif
        }

        internal void InitActionLists()
        {
            actionList = new SortedDictionary<string, ActionGroupInfo>();
            originalActionList = new SortedDictionary<string, ActionGroupInfo>();
        }

        void RestoreActions(List<Part> partsList)
        {

            for (int partNum = 0; partNum < partsList.Count; partNum++)
            {
                Part part = partsList[partNum];

                if (part != null)
                {
                    for (int cnt = 0; cnt < part.Actions.Count; cnt++)
                    {
                        var action = part.Actions[cnt];
                        action.actionGroup = KSPActionGroup.None;
                        if (actionList.ContainsKey(ActionGroupInfo.KeyFrom(part, null, 0, cnt)))
                        {
                            action.actionGroup = actionList[ActionGroupInfo.KeyFrom(part, null, 0, cnt)].baseActionActionGroup;                            
                        }
                    }

                    // Add BaseActions in the part modules.
                    for (int moduleNum = 0; moduleNum < part.Modules.Count; moduleNum++)
                    {
                        PartModule module = part.Modules[moduleNum];

                        for (int actionNum = 0; actionNum < module.Actions.Count; actionNum++)
                        {
                            BaseAction action = module.Actions[actionNum];
                            action.actionGroup = KSPActionGroup.None;
                            if (actionList.ContainsKey(ActionGroupInfo.KeyFrom(part, module, moduleNum, actionNum)))
                            {
                                action.actionGroup = actionList[ActionGroupInfo.KeyFrom(part, module, moduleNum, actionNum)].baseActionActionGroup;
                            }
                        }
                    }
                }
            }
        }

    }
}
