using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;
using FC2Editor.UI;

namespace FC2Editor.Tools
{
    // Note: ParamTexture needs to be defined
    internal class ParamTexture : Parameter
    {
        protected int m_value = -1;

        public int Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                this.ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler ValueChanged;

        public ParamTexture(string display) : base(display) { }

        protected override Control CreateUIControl()
        {
            ParamTextureList list = new ParamTextureList
            {
                Value = m_value
            };
            list.ValueChanged += delegate (object sender, EventArgs e)
            {
                Value = ((ParamTextureList)sender).Value;
            };
            return list;
        }

        protected override void UpdateUIControl(Control control)
        {
            ((ParamTextureList)control).UpdateUI();
        }
    }

    internal class ToolTexture : ToolPaint
    {
        private ParamFloat m_paramStrength = new ParamFloat(Localizer.Localize("PARAM_STRENGTH"), 0.5f, 0f, 1f, 0.01f);
        private ParamTexture m_paramTexture = new ParamTexture("");
        private ParamBool m_paramConstraints = new ParamBool(Localizer.Localize("PARAM_CONSTRAINTS"), false);
        private ParamFloat m_paramMinHeight = new ParamFloat(Localizer.Localize("PARAM_ALTITUDE_MIN"), 0f, 0f, 255f, 0.01f);
        private ParamFloat m_paramMaxHeight = new ParamFloat(Localizer.Localize("PARAM_ALTITUDE_MAX"), 255f, 0f, 255f, 0.01f);
        private ParamFloat m_paramHeightFuzziness = new ParamFloat(Localizer.Localize("PARAM_ALTITUDE_FUZZINESS"), 0f, 0f, 32f, 0.01f);
        private ParamFloat m_paramMinSlope = new ParamFloat(Localizer.Localize("PARAM_SLOPE_MIN"), 0f, 0f, 90f, 0.01f);
        private ParamFloat m_paramMaxSlope = new ParamFloat(Localizer.Localize("PARAM_SLOPE_MAX"), 90f, 0f, 90f, 0.01f);

        public ToolTexture()
        {
            m_paramMinHeight.Enabled = false;
            m_paramMaxHeight.Enabled = false;
            m_paramHeightFuzziness.Enabled = false;
            m_paramMinSlope.Enabled = false;
            m_paramMaxSlope.Enabled = false;
            m_paramConstraints.ValueChanged += constraints_ValueChanged;
            m_hardness.Value = 0.85f;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_TEXTURE");
        public override Image GetToolImage() => Resources.Texture;
        public override IEnumerable<IParameter> GetParameters()
        {
            foreach (IParameter parameter in base._GetParameters())
            {
                yield return parameter;
            }
            yield return m_paramStrength;
            yield return m_paramTexture;
            yield return m_paramConstraints;
            yield return m_paramMinHeight;
            yield return m_paramMaxHeight;
            yield return m_paramHeightFuzziness;
            yield return m_paramMinSlope;
            yield return m_paramMaxSlope;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_TEXTURE") + "\r\n\r\n" + GetPaintContextHelp() + "\r\n\r\n" + GetShortcutContextHelp();

        private void constraints_ValueChanged(object sender, EventArgs e)
        {
            m_paramMinHeight.Enabled = m_paramConstraints.Value;
            m_paramMaxHeight.Enabled = m_paramConstraints.Value;
            m_paramHeightFuzziness.Enabled = m_paramConstraints.Value;
            m_paramMinSlope.Enabled = m_paramConstraints.Value;
            m_paramMaxSlope.Enabled = m_paramConstraints.Value;
        }

        protected override void OnBeginPaint()
        {
            base.OnBeginPaint();
            if (m_paramConstraints.Value)
            {
                TextureManipulator.PaintConstraints_Begin(m_paramMinHeight.Value, m_paramMaxHeight.Value, m_paramHeightFuzziness.Value, m_paramMinSlope.Value, m_paramMaxSlope.Value);
            }
        }

        protected override void OnPaint(float dt, Vec2 pos)
        {
            base.OnPaint(dt, pos);
            int textureId = ((Control.ModifierKeys & Keys.Control) != Keys.None) ? 0 : m_paramTexture.Value;
            if (textureId == -1)
                return;

            TextureInventory.Entry entry = TerrainManager.GetTextureEntryFromId(textureId);
            if (entry.IsValid)
            {
                if (!m_paramConstraints.Value)
                {
                    TextureManipulator.Paint(pos, m_paramStrength.Value * 512f * dt, textureId, m_brush);
                }
                else
                {
                    TextureManipulator.PaintConstraints(pos, m_paramStrength.Value * 512f * dt, textureId, m_brush);
                }
            }
        }

        protected override void OnEndPaint()
        {
            base.OnEndPaint();
            if (!m_paramConstraints.Value)
            {
                TextureManipulator.Paint_End();
            }
            else
            {
                TextureManipulator.PaintConstraints_End();
            }
        }

        public override void OnEditorEvent(uint eventType, IntPtr eventPtr)
        {
            if (eventType == EditorEventUndo.TypeId)
            {
                m_paramTexture.UpdateUIControls();
            }
        }
    }
}