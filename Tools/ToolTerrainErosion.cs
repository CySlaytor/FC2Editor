using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainErosion : ToolPaint
    {
        private ParamFloat m_density = new ParamFloat(Localizer.Localize("PARAM_DENSITY"), 0.5f, 0f, 1f, 0.01f);
        private ParamFloat m_deformation = new ParamFloat(Localizer.Localize("PARAM_DEFORMATION"), 0.5f, 0f, 1f, 0.01f);
        private ParamFloat m_channelDepth = new ParamFloat(Localizer.Localize("PARAM_CHANNEL_DEPTH"), 0.5f, 0f, 1f, 0.01f);
        private ParamFloat m_randomness = new ParamFloat("Randomness", 0f, 0f, 1f, 0.01f);

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_EROSION");
        public override Image GetToolImage() => Resources.TerrainEdit_Erosion;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_density;
            yield return m_deformation;
            yield return m_channelDepth;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_EROSION") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + GetShortcutContextHelp();

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            TerrainManipulator.Erosion(pos, m_radius.Value, m_density.Value, m_deformation.Value, m_channelDepth.Value, m_randomness.Value);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.Erosion_End();
        }
    }
}