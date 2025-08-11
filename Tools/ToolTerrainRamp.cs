using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainRamp : ToolPaint
    {
        private Vec3 m_rampStart;
        private bool m_rampStarted;

        public ToolTerrainRamp()
        {
            m_square.Enabled = false;
            m_distortion.Enabled = false;
            m_hardness.Value = 0.25f;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_RAMP");
        public override Image GetToolImage() => Resources.TerrainEdit_Ramp;
        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_RAMP") + "\r\n\r\n" + GetShortcutContextHelp();

        public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            if (mouseEvent == Editor.MouseEvent.MouseUp && m_painting == PaintingMode.None)
            {
                if (!m_rampStarted)
                {
                    m_rampStarted = Editor.RayCastTerrainFromMouse(out m_rampStart);
                }
                else if (Editor.RayCastTerrainFromMouse(out Vec3 hitPos))
                {
                    UndoManager.RecordUndo();
                    TerrainManipulator.Ramp(m_rampStart.XY, hitPos.XY, m_radius.Value, m_hardness.Value);
                    UndoManager.CommitUndo();
                    m_rampStarted = false;
                }
            }
            return base.OnMouseEvent(mouseEvent, mouseEventArgs);
        }

        protected override void OnBeginPaint() { }

        public override void Update(float dt)
        {
            if (m_rampStarted)
            {
                float length = (Camera.Position - m_rampStart).Length;
                Render.DrawTerrainCircle(m_rampStart.XY, m_radius.Value, length * 0.01f, Color.OrangeRed, -0.001f);
                Render.DrawTerrainCircle(m_rampStart.XY, length * 0.00375f, length * 0.0075f, Color.OrangeRed, -0.001f);
            }
            base.Update(dt);
        }
    }
}