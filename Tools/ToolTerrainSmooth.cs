using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainSmooth : ToolPaint
    {
        public ToolTerrainSmooth()
        {
            m_hardness.Value = 0.125f;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_SMOOTH");
        public override Image GetToolImage() => Resources.TerrainEdit_Smooth;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_opacity;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_SMOOTH") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + GetShortcutContextHelp();

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            TerrainManipulator.Smooth(pos, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.Smooth_End();
        }
    }
}