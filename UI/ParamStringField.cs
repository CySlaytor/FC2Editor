using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    public class ParamStringField : UserControl
    {
        private IContainer components = null;
        private Label parameterName;
        private TextBox parameterField;
        private string m_value;

        public string Value
        {
            get { return m_value; }
            set
            {
                if (m_value != value)
                {
                    m_value = value;
                    UpdateUI();
                }
            }
        }

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
        }

        public event EventHandler ValueChanged;

        public ParamStringField()
        {
            InitializeComponent();
        }

        private void UpdateText()
        {
            parameterField.Text = m_value;
        }

        public void UpdateUI()
        {
            UpdateText();
        }

        private void UpdateFromText()
        {
            OnValueChanged(parameterField.Text);
        }

        private void parameterField_Leave(object sender, EventArgs e)
        {
            UpdateFromText();
        }

        private void parameterField_KeyDown(object sender, KeyEventArgs e)
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

        private void OnValueChanged(string value)
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
            this.parameterField = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // parameterName
            // 
            this.parameterName.Dock = System.Windows.Forms.DockStyle.Top;
            this.parameterName.Location = new System.Drawing.Point(0, 0);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(285, 13);
            this.parameterName.TabIndex = 2;
            this.parameterName.Text = "Parameter name";
            // 
            // parameterField
            // 
            this.parameterField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parameterField.Location = new System.Drawing.Point(3, 16);
            this.parameterField.Name = "parameterField";
            this.parameterField.Size = new System.Drawing.Size(279, 20);
            this.parameterField.TabIndex = 3;
            this.parameterField.Leave += new System.EventHandler(this.parameterField_Leave);
            this.parameterField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.parameterField_KeyDown);
            // 
            // ParamStringField
            // 
            this.Controls.Add(this.parameterField);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamStringField";
            this.Size = new System.Drawing.Size(285, 41);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}