using System.Collections;
using System.Reflection;

namespace SOS
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    // HighLogic.CurrentGame.Parameters.CustomParams<FES>().defaultSaveStaging
    public class S_O_S : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "SOS"; } }
        public override string DisplaySection { get { return "Save Our Settings"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Default to saving Staging when reverting",
            toolTip = "If enabled, will automatically enable the toggle to save Staging when reverting")]
        public bool defaultSaveStaging = true;

        [GameParameters.CustomParameterUI("Default to saving Actions when reverting",
            toolTip = "If enabled, will automatically enable the toggle to save Actions when reverting")]
        public bool defaultSaveActions = true;

        [GameParameters.CustomParameterUI("Default to saving Axes when reverting",
            toolTip = "If enabled, will automatically enable the toggle to save Axes when reverting")]
        public bool defaultSaveAxes = true;



        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {

        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {

            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }

}