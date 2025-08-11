using System;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    public class ParamEnumCombo : UserControl
    {
        private class Item
        {
            private string m_display;
            public object Value { get; }
            public Item(string display, object value) { m_display = display; Value = value; }
            public override string ToString() { return m_display; }
        }

        private System.ComponentModel.IContainer components = null;
        private Label parameterName;
        private ComboBox valueCombo;
        private object m_value;

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
        }

        public object Value
        {
            get { return m_value; }
            set { m_value = value; UpdateUI(); }
        }
        public event EventHandler ValueChanged;

        public ParamEnumCombo()
        {
            InitializeComponent();
            UpdateUI();
        }

        public void UpdateUI()
        {
            for (int i = 0; i < valueCombo.Items.Count; i++)
            {
                if (((Item)valueCombo.Items[i]).Value.Equals(Value))
                {
                    valueCombo.SelectedIndex = i;
                    break;
                }
            }
        }

        private void valueCombo_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnValueChanged(((Item)valueCombo.SelectedItem).Value);
        }

        public void Add(string display, object value)
        {
            valueCombo.Items.Add(new Item(display, value));
        }

        protected void OnValueChanged(object value)
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
            this.valueCombo = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // parameterName
            // 
            this.parameterName.Dock = System.Windows.Forms.DockStyle.Top;
            this.parameterName.Location = new System.Drawing.Point(0, 0);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(262, 13);
            this.parameterName.TabIndex = 0;
            // 
            // valueCombo
            // 
            this.valueCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.valueCombo.FormattingEnabled = true;
            this.valueCombo.Location = new System.Drawing.Point(3, 16);
            this.valueCombo.Name = "valueCombo";
            this.valueCombo.Size = new System.Drawing.Size(256, 21);
            this.valueCombo.TabIndex = 1;
            this.valueCombo.SelectionChangeCommitted += new System.EventHandler(this.valueCombo_SelectionChangeCommitted);
            // 
            // ParamEnumCombo
            // 
            this.Controls.Add(this.valueCombo);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamEnumCombo";
            this.Size = new System.Drawing.Size(262, 42);
            this.ResumeLayout(false);
        }
        #endregion
    }
}