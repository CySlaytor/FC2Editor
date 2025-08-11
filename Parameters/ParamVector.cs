using System;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.UI;

namespace FC2Editor.Parameters
{
    internal enum ParamVectorUIType
    {
        Position,
        Angles
    }

    internal class ParamVector : Parameter
    {
        protected Vec3 m_value;
        private readonly ParamVectorUIType m_uiType;

        public Vec3 Value
        {
            get { return m_value; }
            set
            {
                if (m_value == value)
                {
                    return;
                }
                m_value = value;
                foreach (Control key in m_uiControls.Keys)
                {
                    if (key is ParamVectorEdit paramVectorEdit)
                    {
                        paramVectorEdit.Value = m_value;
                    }
                }
            }
        }

        public event EventHandler ValueChanged;

        public ParamVector(string display, Vec3 value, ParamVectorUIType uiType) : base(display)
        {
            Value = value;
            m_uiType = uiType;
        }

        protected override Control CreateUIControl()
        {
            ParamVectorEdit paramVectorEdit = new ParamVectorEdit
            {
                Value = Value,
                ParameterName = base.DisplayName
            };
            paramVectorEdit.ValueChanged += delegate (object sender, EventArgs e)
            {
                OnValueChanged(((ParamVectorEdit)sender).Value);
            };
            if (m_uiType == ParamVectorUIType.Angles)
            {
                paramVectorEdit.ValueType = ParamVectorEditValueType.Angles;
            }
            paramVectorEdit.UpdateUI();
            return paramVectorEdit;
        }

        private void OnValueChanged(Vec3 value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}