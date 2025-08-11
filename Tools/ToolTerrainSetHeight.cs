using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolTerrainSetHeight : ToolPaint
    {
        private bool m_picking;
        private ParamFloat m_height = new ParamFloat(Localizer.Localize("PARAM_HEIGHT"), 32f, 0f, 256f, 0.01f);
        private ParamFloat m_strength = new ParamFloat(Localizer.Localize("PARAM_STRENGTH"), 0.75f, 0f, 1f, 0.01f);

        public ToolTerrainSetHeight()
        {
            m_hardness.Value = 0.25f;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_TERRAIN_SET_HEIGHT");
        public override Image GetToolImage() => Resources.TerrainEdit_SetHeight;

        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_strength;
            yield return m_height;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_SETHEIGHT") + "\r\n\r\n" + Localizer.Localize("HELP_PICK_HEIGHT") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + GetShortcutContextHelp();

        public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            if ((Control.ModifierKeys & Keys.Control) == 0)
            {
                m_picking = false;
                return base.OnMouseEvent(mouseEvent, mouseEventArgs);
            }

            switch (mouseEvent)
            {
                case Editor.MouseEvent.MouseDown:
                    if (!m_picking)
                    {
                        m_picking = true;
                        UpdatePicking();
                    }
                    break;
                case Editor.MouseEvent.MouseMove:
                    if (m_picking)
                    {
                        UpdatePicking();
                    }
                    break;
                case Editor.MouseEvent.MouseUp:
                    m_picking = false;
                    break;
            }
            return false;
        }

        protected override void OnBeginPaint()
        {
            base.OnBeginPaint();
            m_opacity.Value = m_strength.Value;
        }

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            TerrainManipulator.SetHeight(pos, m_height.Value, m_brush);
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            TerrainManipulator.SetHeight_End();
        }

        public override void Update(float dt)
        {
            if ((Control.ModifierKeys & Keys.Control) == 0)
            {
                base.Update(dt);
            }
        }

        private void UpdatePicking()
        {
            m_height.Value = m_cursorPos.Z;
        }
    }
}