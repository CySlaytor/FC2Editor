using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainTerrace : ToolPaint
    {
        private ParamFloat m_strength = new ParamFloat(Localizer.Localize("PARAM_STRENGTH"), 0.5f, 0f, 1f, 0.01f);
        private ParamFloat m_height = new ParamFloat(Localizer.Localize("PARAM_HEIGHT"), 2f, 0f, 32f, 0.01f);

        public ToolTerrainTerrace()
        {
            m_hardness.Value = 0.125f;
        }

        public override string GetToolName() => "Terrain Terrace";
        public override Image GetToolImage() => Resources.TerrainEdit_Terrace;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_strength;
            yield return m_height;
        }

        public override string GetContextHelp() => "This tool levels out the terrain in multiple steps of equal height.\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + GetShortcutContextHelp();

        protected override void OnBeginPaint()
        {
            base.OnBeginPaint();
            m_opacity.Value = m_strength.Value * 0.04f;
        }

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            TerrainManipulator.Terrace(pos, m_height.Value, 1.5f, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.Terrace_End();
        }
    }
}