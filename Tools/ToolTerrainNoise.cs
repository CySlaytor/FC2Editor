using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainNoise : ToolPaint
    {
        private ParamFloat m_strength = new ParamFloat(Localizer.Localize("PARAM_STRENGTH"), 0.5f, 0f, 1f, 0.01f);
        private ParamFloat m_roughness = new ParamFloat(Localizer.Localize("PARAM_ROUGHNESS"), 0.5f, 0f, 1f, 0.01f);
        private ParamEnum<TerrainManipulator.NoiseType> m_noiseType = new ParamEnum<TerrainManipulator.NoiseType>(Localizer.Localize("PARAM_NOISE_TYPE"), TerrainManipulator.NoiseType.Absolute, ParamEnumUIType.ComboBox);

        public ToolTerrainNoise()
        {
            m_noiseType.Names = new string[] { Localizer.Localize("PARAM_NOISE_RAISE"), Localizer.Localize("PARAM_NOISE_LOWER"), Localizer.Localize("PARAM_NOISE_RAISE_LOWER") };
            m_noiseType.Values = new TerrainManipulator.NoiseType[] { TerrainManipulator.NoiseType.Absolute, TerrainManipulator.NoiseType.InverseAbsolute, TerrainManipulator.NoiseType.Normal };
            m_hardness.Value = 0.3f;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_NOISE");
        public override Image GetToolImage() => Resources.TerrainEdit_Noise;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_strength;
            yield return m_roughness;
            yield return m_noiseType;
            yield return m_grabMode;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_NOISE") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + GetShortcutContextHelp();

        protected override void OnBeginPaint()
        {
            base.OnBeginPaint();
            TerrainManipulator.Noise_Begin(8, 128f, m_roughness.Value, m_noiseType.Value);
        }

        protected override void OnPaintGrab(float x, float y)
        {
            base.OnPaintGrab(x, y);
            float amount = -y * m_strength.Value * 0.3f;
            TerrainManipulator.Noise(m_cursorPos.XY, amount, m_brush);
        }

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            float amount = m_strength.Value * 40f * dt;
            TerrainManipulator.Noise(pos, (m_painting == PaintingMode.Plus) ? amount : -amount, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.Noise_End();
        }
    }
}