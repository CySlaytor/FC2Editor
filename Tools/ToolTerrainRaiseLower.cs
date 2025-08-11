using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainRaiseLower : ToolPaint
    {
        private ParamFloat m_height = new ParamFloat(Localizer.Localize("PARAM_HEIGHT"), 5f, -32f, 32f, 0.01f);

        public ToolTerrainRaiseLower()
        {
            m_hardness.Value = 0.125f;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_RAISE_LOWER");
        public override Image GetToolImage() => Resources.TerrainEdit_RaiseLower;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_height;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_RAISELOWER") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + Localizer.Localize("HELP_TOOL_TERRAIN_ERASE") + "\r\n\r\n" + GetShortcutContextHelp();

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            float amount = m_height.Value;
            TerrainManipulator.RaiseLower(pos, (m_painting == PaintingMode.Plus) ? amount : -amount, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.RaiseLower_End();
        }
    }
}