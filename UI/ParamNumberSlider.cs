using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    public class ParamNumberSlider : UserControl
    {
        private IContainer components = null;
        private Label parameterName;
        private NomadSlider valueSlider;
        private TextBox valueText;

        private float m_minValue;
        private float m_maxValue = 1f;
        private float m_resolution = 0.1f;
        private int m_numTicks = 10;
        private float m_value;

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
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
            set
            {
                m_resolution = value;
                m_numTicks = (int)Math.Round((MaxValue - MinValue) / m_resolution);
            }
        }

        public float Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                if (m_value < MinValue) m_value = MinValue;
                if (m_value > MaxValue) m_value = MaxValue;
                UpdateUI();
            }
        }

        public event EventHandler ValueChanged;

        public ParamNumberSlider()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void UpdateSlider()
        {
            valueSlider.Value = (int)((Value - MinValue) / (MaxValue - MinValue) * valueSlider.Maximum);
        }

        private void UpdateText()
        {
            valueText.Text = Value.ToString("F2");
        }

        private void UpdateFromText()
        {
            if (float.TryParse(valueText.Text, out float result) && result >= MinValue && result <= MaxValue)
            {
                OnValueChanged(result);
                UpdateSlider();
            }
            else
            {
                UpdateText();
            }
        }

        public void UpdateUI()
        {
            if (valueSlider.Maximum != m_numTicks)
            {
                valueSlider.Maximum = m_numTicks;
            }
            UpdateSlider();
            UpdateText();
        }

        private void valueSlider_Scroll(object sender, EventArgs e)
        {
            OnValueChanged((float)valueSlider.Value / valueSlider.Maximum * (MaxValue - MinValue) + MinValue);
            UpdateText();
        }

        private void valueText_Leave(object sender, EventArgs e)
        {
            UpdateFromText();
        }

        private void valueText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                UpdateText();
            }
            else if (e.KeyCode == Keys.Return)
            {
                UpdateFromText();
            }
        }

        public void OnValueChanged(float value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, new EventArgs());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.parameterName = new System.Windows.Forms.Label();
            this.valueSlider = new FC2Editor.UI.NomadSlider();
            this.valueText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.valueSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // parameterName
            // 
            this.parameterName.Location = new System.Drawing.Point(0, 1);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(262, 13);
            this.parameterName.TabIndex = 0;
            this.parameterName.Text = "Parameter name";
            // 
            // valueSlider
            // 
            this.valueSlider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueSlider.AutoSize = false;
            this.valueSlider.Location = new System.Drawing.Point(3, 16);
            this.valueSlider.Maximum = 1000;
            this.valueSlider.Name = "valueSlider";
            this.valueSlider.Size = new System.Drawing.Size(209, 20);
            this.valueSlider.TabIndex = 1;
            this.valueSlider.TickFrequency = 50;
            this.valueSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.valueSlider.Scroll += new System.EventHandler(this.valueSlider_Scroll);
            // 
            // valueText
            // 
            this.valueText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.valueText.Location = new System.Drawing.Point(214, 14);
            this.valueText.Name = "valueText";
            this.valueText.Size = new System.Drawing.Size(45, 20);
            this.valueText.TabIndex = 2;
            this.valueText.Leave += new System.EventHandler(this.valueText_Leave);
            this.valueText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.valueText_KeyDown);
            // 
            // ParamNumberSlider
            // 
            this.Controls.Add(this.valueText);
            this.Controls.Add(this.valueSlider);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamNumberSlider";
            this.Size = new System.Drawing.Size(262, 37);
            ((System.ComponentModel.ISupportInitialize)(this.valueSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}