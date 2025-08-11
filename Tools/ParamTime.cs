using System;
using System.Windows.Forms;
using FC2Editor.Parameters;
using FC2Editor.UI;

namespace FC2Editor.Tools
{
    internal class ParamTime : Parameter
    {
        private TimeSpan m_value;

        public TimeSpan Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                this.ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler ValueChanged;

        public ParamTime(string display) : base(display) { }

        protected override Control CreateUIControl()
        {
            ParamTimePicker picker = new ParamTimePicker
            {
                ParameterName = base.DisplayName,
                Value = m_value
            };
            picker.ValueChanged += delegate (object sender, EventArgs e)
            {
                Value = ((ParamTimePicker)sender).Value;
            };
            return picker;
        }
    }
}