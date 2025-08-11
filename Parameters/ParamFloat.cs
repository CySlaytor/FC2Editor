using System;
using System.Windows.Forms;
using FC2Editor.UI;

namespace FC2Editor.Parameters
{
    internal class ParamFloat : Parameter
    {
        protected float m_value;
        protected float m_minValue = float.MinValue;
        protected float m_maxValue = float.MaxValue;
        protected float m_resolution = 1f;
        private bool m_enabled = true;

        public float Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                if (m_value < MinValue) m_value = MinValue;
                if (m_value > MaxValue) m_value = MaxValue;

                foreach (Control key in m_uiControls.Keys)
                {
                    if (key is ParamNumberSlider paramNumberSlider)
                    {
                        paramNumberSlider.Value = m_value;
                    }
                }
            }
        }

        public float MinValue
        {
            get { return m_minValue; }
            set { m_minValue = value; }
        }

        public float MaxValue
        {
            get { return m_maxValue; }
            set { m_maxValue = value; }
        }

        public float Resolution
        {
            get { return m_resolution; }
            set { m_resolution = value; }
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

        public ParamFloat(string display, float value, float min, float max, float resolution) : base(display)
        {
            MinValue = min;
            MaxValue = max;
            Resolution = resolution;
            Value = value;
        }

        protected override Control CreateUIControl()
        {
            ParamNumberSlider paramNumberSlider = new ParamNumberSlider
            {
                ParameterName = base.DisplayName
            };
            paramNumberSlider.ValueChanged += delegate (object sender, EventArgs e)
            {
                OnValueChanged(((ParamNumberSlider)sender).Value);
            };
            return paramNumberSlider;
        }

        protected override void UpdateUIControl(Control control)
        {
            ParamNumberSlider paramNumberSlider = (ParamNumberSlider)control;
            paramNumberSlider.Enabled = m_enabled;
            paramNumberSlider.MinValue = MinValue;
            paramNumberSlider.MaxValue = MaxValue;
            paramNumberSlider.Resolution = Resolution;
            paramNumberSlider.Value = Value;
            paramNumberSlider.UpdateUI();
        }

        protected void OnValueChanged(float value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}