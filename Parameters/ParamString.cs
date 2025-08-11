using System;
using System.Windows.Forms;
using FC2Editor.UI;

namespace FC2Editor.Parameters
{
    internal class ParamString : Parameter
    {
        protected string m_value;

        public string Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                UpdateUIControls();
            }
        }

        public event EventHandler ValueChanged;

        public ParamString(string display, string value) : base(display)
        {
            Value = value;
        }

        protected override Control CreateUIControl()
        {
            ParamStringField control = new ParamStringField
            {
                ParameterName = base.DisplayName
            };
            control.ValueChanged += delegate { OnUIValueChanged(control.Value); };
            return control;
        }

        protected override void UpdateUIControl(Control control)
        {
            ((ParamStringField)control).Value = Value;
        }

        protected void OnUIValueChanged(string value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, null);
        }
    }
}