using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    public class ParamEnumButtons : UserControl
    {
        private class Item
        {
            public string Name { get; }
            public Image Image { get; }
            public object Value { get; }

            public Item(string name, Image img, object value)
            {
                Name = name;
                Image = img;
                Value = value;
            }

            public override string ToString() => Name;
        }

        private System.ComponentModel.IContainer components = null;
        private Label parameterName;
        private FlowLayoutPanel flowLayoutPanel;
        private List<Item> m_itemList = new List<Item>();
        private Dictionary<Item, NomadRadioButton> m_buttonList = new Dictionary<Item, NomadRadioButton>();
        private object m_value;

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
        }

        public object Value
        {
            get { return m_value; }
            set
            {
                if (value.Equals(m_value))
                    return;

                m_value = value;
                for (int i = 0; i < m_itemList.Count; i++)
                {
                    if (m_itemList[i].Value.Equals(value) && m_buttonList.TryGetValue(m_itemList[i], out var button))
                    {
                        button.Checked = true;
                        break;
                    }
                }
            }
        }

        public event EventHandler ValueChanged;

        public ParamEnumButtons()
        {
            InitializeComponent();
        }

        public void Add(string name, Image img, object value)
        {
            m_itemList.Add(new Item(name, img, value));
        }

        private void ClearUI()
        {
            foreach (NomadRadioButton button in m_buttonList.Values)
            {
                // Assuming MainForm exists and has a ToolTip component.
                // This will be resolved later.
                // MainForm.Instance.ToolTip.SetToolTip(button, null); 
                button.Dispose();
            }
            flowLayoutPanel.Controls.Clear();
            m_buttonList.Clear();
        }

        public void UpdateUI()
        {
            SuspendLayout();
            ClearUI();
            foreach (Item item in m_itemList)
            {
                NomadRadioButton button = new NomadRadioButton
                {
                    Appearance = Appearance.Button,
                    AutoSize = true,
                    Margin = new Padding(1),
                    Checked = item.Value.Equals(Value),
                    Image = item.Image,
                    Text = item.Image == null ? item.Name : "",
                    Tag = item
                };

                button.CheckedChanged += button_CheckedChanged;

                // We will add tooltip support when MainForm is available
                // MainForm.Instance.ToolTip.SetToolTip(button, item.Name);

                flowLayoutPanel.Controls.Add(button);
                m_buttonList.Add(item, button);
            }
            ResumeLayout();
        }

        private void button_CheckedChanged(object sender, EventArgs e)
        {
            NomadRadioButton button = (NomadRadioButton)sender;
            object value = ((Item)button.Tag).Value;
            if (button.Checked && !m_value.Equals(value))
            {
                OnValueChanged(value);
            }
        }

        protected void OnValueChanged(object value)
        {
            m_value = value;
            this.ValueChanged?.Invoke(this, new EventArgs());
        }

        protected override void Dispose(bool disposing)
        {
            ClearUI();
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
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // parameterName
            // 
            this.parameterName.Dock = System.Windows.Forms.DockStyle.Top;
            this.parameterName.Location = new System.Drawing.Point(0, 0);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(150, 13);
            this.parameterName.TabIndex = 2;
            this.parameterName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 15);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(0, 0);
            this.flowLayoutPanel.TabIndex = 3;
            // 
            // ParamEnumButtons
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamEnumButtons";
            this.Size = new System.Drawing.Size(150, 18);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}