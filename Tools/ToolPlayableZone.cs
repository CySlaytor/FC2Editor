using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolPlayableZone : ToolSpline
    {
        private ParamButton m_actionReset = new ParamButton(Localizer.Localize("PARAM_RESET"), null);

        public ToolPlayableZone()
        {
            m_actionReset.Callback = action_Reset;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_PLAYABLE_ZONE");
        public override Image GetToolImage() => Resources.PlayableZone;

        public override IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramEditTool;
            yield return m_actionReset;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_PLAYABLEZONE") + "\r\n\r\n" + GetSplineHelp();

        private void action_Reset()
        {
            UndoManager.RecordUndo();
            SplineManager.GetPlayableZone().Reset();
            UndoManager.CommitUndo();
        }

        public override void Activate()
        {
            base.Activate();
            SetSpline(SplineManager.GetPlayableZone());
        }
    }
}