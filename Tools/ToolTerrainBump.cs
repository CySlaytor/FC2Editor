using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainBump : ToolPaint
    {
        private ParamFloat m_strength = new ParamFloat(Localizer.Localize("PARAM_STRENGTH"), 0.5f, 0f, 1f, 0.01f);

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_BUMP");
        public override Image GetToolImage() => Resources.TerrainEdit_Bump;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_strength;
            yield return m_grabMode;
        }

        public override string GetContextHelp()
        {
            return Localizer.LocalizeCommon("HELP_TOOL_BUMP") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + Localizer.Localize("HELP_TOOL_TERRAIN_ERASE") + "\r\n\r\n" + GetShortcutContextHelp();
        }

        protected override void OnPaintGrab(float x, float y)
        {
            base.OnPaintGrab(x, y);
            float amount = -y * m_strength.Value * 0.3f;
            TerrainManipulator.Bump(m_cursorPos.XY, amount, m_brush);
        }

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            float amount = m_strength.Value * 32f * dt;
            TerrainManipulator.Bump(pos, (m_painting == PaintingMode.Plus) ? amount : -amount, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.Bump_End();
        }
    }
}