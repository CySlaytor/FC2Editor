using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    public class ParamTimePicker : UserControl
    {
        private IContainer components = null;
        private Label parameterName;
        private DateTimePicker dateTimePicker;
        private NomadSlider timeSlider;

        private bool updatePicker;
        private TimeSpan m_value = default(TimeSpan);
        private readonly TimeSpan MaxValue = new TimeSpan(23, 59, 59);

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
        }

        public TimeSpan Value
        {
            get { return m_value; }
            set { m_value = value; UpdateUI(); }
        }

        public event EventHandler ValueChanged;

        public ParamTimePicker()
        {
            InitializeComponent();
        }

        private void UpdateSlider()
        {
            timeSlider.Value = (int)(Value.Ticks * timeSlider.Maximum / MaxValue.Ticks);
        }

        private void UpdatePicker()
        {
            updatePicker = true;
            DateTime now = DateTime.Now;
            dateTimePicker.Value = new DateTime(now.Year, now.Month, now.Day, Value.Hours, Value.Minutes, Value.Seconds, Value.Milliseconds);
            updatePicker = false;
        }

        private void UpdateUI()
        {
            UpdateSlider();
            UpdatePicker();
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (!updatePicker)
            {
                OnValueChanged(dateTimePicker.Value.TimeOfDay);
                UpdateSlider();
            }
        }

        private void timeSlider_Scroll(object sender, EventArgs e)
        {
            OnValueChanged(new TimeSpan(timeSlider.Value * MaxValue.Ticks / timeSlider.Maximum));
            UpdatePicker();
        }

        private void OnValueChanged(TimeSpan value)
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
            this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.timeSlider = new FC2Editor.UI.NomadSlider();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // parameterName
            // 
            this.parameterName.AutoSize = true;
            this.parameterName.Location = new System.Drawing.Point(3, 0);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(30, 13);
            this.parameterName.TabIndex = 0;
            this.parameterName.Text = "Time";
            // 
            // dateTimePicker
            // 
            this.dateTimePicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePicker.Location = new System.Drawing.Point(138, 14);
            this.dateTimePicker.Name = "dateTimePicker";
            this.dateTimePicker.ShowUpDown = true;
            this.dateTimePicker.Size = new System.Drawing.Size(107, 20);
            this.dateTimePicker.TabIndex = 1;
            this.dateTimePicker.ValueChanged += new System.EventHandler(this.dateTimePicker_ValueChanged);
            // 
            // timeSlider
            // 
            this.timeSlider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeSlider.AutoSize = false;
            this.timeSlider.Location = new System.Drawing.Point(0, 16);
            this.timeSlider.Maximum = 1000;
            this.timeSlider.Name = "timeSlider";
            this.timeSlider.Size = new System.Drawing.Size(137, 20);
            this.timeSlider.TabIndex = 2;
            this.timeSlider.TickFrequency = 50;
            this.timeSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.timeSlider.Scroll += new System.EventHandler(this.timeSlider_Scroll);
            // 
            // ParamTimePicker
            // 
            this.Controls.Add(this.timeSlider);
            this.Controls.Add(this.dateTimePicker);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamTimePicker";
            this.Size = new System.Drawing.Size(248, 43);
            ((System.ComponentModel.ISupportInitialize)(this.timeSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}