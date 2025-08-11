using System;
using System.Windows.Forms;

namespace FC2Editor.Parameters
{
    internal class ParamBool : Parameter
    {
        protected bool m_value;
        private bool m_enabled = true;

        public bool Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                UpdateUIControls();
            }
        }

        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                m_enabled = value;
                UpdateUIControls();
            }
        }

        public event EventHandler ValueChanged;

        public ParamBool(string display, bool value) : base(display)
        {
            Value = value;
        }

        protected override Control CreateUIControl()
        {
            CheckBox control = new CheckBox
            {
                Text = base.DisplayName,
                AutoSize = true,
                Padding = new Padding(2, 2, 0, 2)
            };
            control.Click += delegate { OnUIValueChanged(control.Checked); };
            return control;
        }

        protected override void UpdateUIControl(Control control)
        {
            control.Enabled = m_enabled;
            ((CheckBox)control).Checked = Value;
        }

        protected void OnUIValueChanged(bool value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, null);
        }
    }
}