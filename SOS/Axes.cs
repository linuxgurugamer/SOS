using System.Collections.Generic;

namespace SOS
{

    public partial class VesselInfo
    {
        const string axisgroups = "AXISGROUPS";

        internal ConfigNode originalAxesNodes;
        ConfigNode axesNodes;

        string PartNodeStringValue(Part part) { return part.partInfo.name + ":" + part.persistentId.ToString(); }
        string ModuleNodeStringValue(PartModule module) { return module.moduleName + ":" + module.PersistentId.ToString(); }


        internal bool AxesChanged
        {
            get
            {
                string orig = "";
                if (originalAxesNodes != null)
                    orig = originalAxesNodes.ToString();
                string cur = "";
                if (axesNodes != null)
                    cur = axesNodes.ToString();

                return orig != cur;

            }
        }

        internal void SaveOriginalAxesList()
        {
            SaveOriginalAxesList(FlightGlobals.ActiveVessel);
        }

        internal void SaveOriginalAxesList(Vessel v)
        {
            originalAxesNodes = GetAllAxesFromPartsAndModules(v.Parts);            
            axesNodes = new ConfigNode();
        }

        internal void GetAllAxesFromPartsAndModules(Vessel v, bool original = true)
        {
            if (v.situation != Vessel.Situations.PRELAUNCH && original)
                return;

            axesNodes = GetAllAxesFromPartsAndModules(v.Parts);
        }

        ConfigNode GetAllAxesFromPartsAndModules(List<Part> partList)
        {
            ConfigNode localAxesNodes = new ConfigNode();
            for (int partNum = 0; partNum < partList.Count; partNum++)
            {
                Part part = partList[partNum];
                ConfigNode partNode = new ConfigNode(PartNodeStringValue(part));

                bool hasAxis = false;
                for (int moduleNum = 0; moduleNum < part.Modules.Count; moduleNum++)
                {
                    PartModule module = part.Modules[moduleNum];

                    ConfigNode axeNode = new ConfigNode(ModuleNodeStringValue(module));

                    AxisGroupsManager.SaveAxisFieldNodes(module, axeNode);
                    if (axeNode.HasNode(axisgroups))
                    {
                        hasAxis = true;
                        partNode.AddNode(axeNode);
                    }

                }
                if (hasAxis)
                    localAxesNodes.AddNode(partNode);
            }
            return localAxesNodes;
        }

        internal void InitAxesConfigNodes()
        {
            axesNodes = new ConfigNode();
            originalAxesNodes = new ConfigNode();
        }

        void RestoreAxes(Vessel v)
        {
            RestoreAxes(v.Parts);
        }
        void RestoreAxes(List<Part> partsList)
        {
            for (int partNum = 0; partNum < partsList.Count; partNum++)
            {
                Part part = partsList[partNum];
                if (axesNodes.HasNode(PartNodeStringValue(part)))
                {
                    var partNode = axesNodes.GetNode(PartNodeStringValue(part));
                    for (int moduleNum = 0; moduleNum < part.Modules.Count; moduleNum++)
                    {
                        PartModule module = part.Modules[moduleNum];
                        if (partNode.HasNode(ModuleNodeStringValue(module)))
                        {
                            var moduleNode = partNode.GetNode(ModuleNodeStringValue(module));

                            //moduleNode.Save("moduleNode-" + partNum + "-" + moduleNum);

                            AxisGroupsManager.BuildBaseAxisFields(module);
                            AxisGroupsManager.LoadAxisFieldNodes(module, moduleNode);
                        }
                    }
                }
            }
        }
    }
}
