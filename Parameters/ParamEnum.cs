using System;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.UI;

namespace FC2Editor.Parameters
{
    public enum ParamEnumUIType
    {
        ComboBox,
        Buttons
    }

    internal class ParamEnum<T> : ParamEnumBase<T>
    {
        public ParamEnum(string display, T value, ParamEnumUIType uiType)
            : base(display, value, uiType)
        {
            base.Names = Enum.GetNames(typeof(T));
            base.Values = (T[])Enum.GetValues(typeof(T));
        }
    }

    internal class ParamEnumAngles : ParamEnumBase<float>
    {
        private static readonly string[] s_names = { "5", "10", "20", "45", "90" };
        private static readonly float[] s_values = { 5f, 10f, 20f, 45f, 90f };

        public ParamEnumAngles(string display, float value, ParamEnumUIType uiType)
            : base(display, value, uiType)
        {
            base.Names = s_names;
            base.Values = s_values;
        }
    }

    internal class ParamEnumBase<T> : Parameter
    {
        protected T m_value;
        private bool m_enabled = true;
        private readonly ParamEnumUIType m_uiType;
        protected string[] m_names;
        protected Image[] m_images = new Image[0];
        protected T[] m_values = new T[0];

        public T Value
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

        public string[] Names
        {
            get { return m_names; }
            set { m_names = value ?? new string[0]; }
        }

        public Image[] Images
        {
            get { return m_images; }
            set { m_images = value ?? new Image[0]; }
        }

        public T[] Values
        {
            get { return m_values; }
            set { m_values = value ?? new T[0]; }
        }

        public event EventHandler ValueChanged;

        public ParamEnumBase(string display, T value, ParamEnumUIType uiType) : base(display)
        {
            Value = value;
            m_uiType = uiType;
        }

        protected override Control CreateUIControl()
        {
            switch (m_uiType)
            {
                case ParamEnumUIType.ComboBox:
                    {
                        ParamEnumCombo paramEnumCombo = new ParamEnumCombo();
                        for (int i = 0; i < m_names.Length; i++)
                        {
                            paramEnumCombo.Add(m_names[i], m_values.GetValue(i));
                        }
                        paramEnumCombo.UpdateUI();
                        paramEnumCombo.ParameterName = base.DisplayName;
                        paramEnumCombo.ValueChanged += delegate (object sender, EventArgs e)
                        {
                            OnValueChanged((T)((ParamEnumCombo)sender).Value);
                        };
                        return paramEnumCombo;
                    }
                case ParamEnumUIType.Buttons:
                    {
                        ParamEnumButtons paramEnumButtons = new ParamEnumButtons();
                        for (int i = 0; i < m_names.Length; i++)
                        {
                            paramEnumButtons.Add(m_names[i], (i < m_images.Length) ? m_images[i] : null, m_values.GetValue(i));
                        }
                        paramEnumButtons.UpdateUI();
                        paramEnumButtons.ParameterName = base.DisplayName;
                        paramEnumButtons.ValueChanged += delegate (object sender, EventArgs e)
                        {
                            OnValueChanged((T)((ParamEnumButtons)sender).Value);
                        };
                        return paramEnumButtons;
                    }
                default:
                    return null;
            }
        }

        protected override void UpdateUIControl(Control control)
        {
            control.Enabled = m_enabled;
            switch (m_uiType)
            {
                case ParamEnumUIType.ComboBox:
                    ((ParamEnumCombo)control).Value = m_value;
                    break;
                case ParamEnumUIType.Buttons:
                    ((ParamEnumButtons)control).Value = m_value;
                    break;
            }
        }

        protected void OnValueChanged(T value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, new EventArgs());
        }
    }
}