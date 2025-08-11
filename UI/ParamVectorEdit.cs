using System;
using System.ComponentModel;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Utils;

namespace FC2Editor.UI
{
    public enum ParamVectorEditValueType
    {
        Position,
        Angles
    }

    internal class ParamVectorEdit : UserControl
    {
        private IContainer components = null;
        private Label parameterName;
        private Label label1;
        private TextBox xBox;
        private TextBox yBox;
        private Label label2;
        private TextBox zBox;
        private Label label3;
        private Vec3 m_value;
        private ParamVectorEditValueType m_valueType;

        public Vec3 Value
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

        public ParamVectorEditValueType ValueType
        {
            get { return m_valueType; }
            set { m_valueType = value; UpdateUI(); }
        }

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
        }

        public event EventHandler ValueChanged;

        public ParamVectorEdit()
        {
            InitializeComponent();
        }

        private void UpdateText()
        {
            float x = Value.X;
            float y = Value.Y;
            float z = Value.Z;

            if (m_valueType == ParamVectorEditValueType.Angles)
            {
                x = MathUtils.Rad2Deg(x);
                y = MathUtils.Rad2Deg(y);
                z = MathUtils.Rad2Deg(z);
            }

            xBox.Text = x.ToString("F2");
            yBox.Text = y.ToString("F2");
            zBox.Text = z.ToString("F2");
        }

        public void UpdateUI()
        {
            UpdateText();
        }

        private void UpdateFromText()
        {
            if (!float.TryParse(xBox.Text, out float x) || !float.TryParse(yBox.Text, out float y) || !float.TryParse(zBox.Text, out float z))
            {
                UpdateText();
                return;
            }

            if (m_valueType == ParamVectorEditValueType.Angles)
            {
                x = MathUtils.Deg2Rad(x);
                y = MathUtils.Deg2Rad(y);
                z = MathUtils.Deg2Rad(z);
            }

            OnValueChanged(new Vec3(x, y, z));
        }

        private void valueBox_Leave(object sender, EventArgs e)
        {
            UpdateFromText();
        }

        private void value_KeyDown(object sender, KeyEventArgs e)
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

        private void OnValueChanged(Vec3 value)
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
            this.label1 = new System.Windows.Forms.Label();
            this.xBox = new System.Windows.Forms.TextBox();
            this.yBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.zBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // parameterName
            // 
            this.parameterName.Dock = System.Windows.Forms.DockStyle.Top;
            this.parameterName.Location = new System.Drawing.Point(0, 0);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(226, 13);
            this.parameterName.TabIndex = 0;
            this.parameterName.Text = "Name";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(1, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "X:";
            // 
            // xBox
            // 
            this.xBox.Location = new System.Drawing.Point(21, 17);
            this.xBox.Name = "xBox";
            this.xBox.Size = new System.Drawing.Size(51, 20);
            this.xBox.TabIndex = 2;
            this.xBox.Leave += new System.EventHandler(this.valueBox_Leave);
            this.xBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.value_KeyDown);
            // 
            // yBox
            // 
            this.yBox.Location = new System.Drawing.Point(93, 17);
            this.yBox.Name = "yBox";
            this.yBox.Size = new System.Drawing.Size(51, 20);
            this.yBox.TabIndex = 4;
            this.yBox.Leave += new System.EventHandler(this.valueBox_Leave);
            this.yBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.value_KeyDown);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(73, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Y:";
            // 
            // zBox
            // 
            this.zBox.Location = new System.Drawing.Point(167, 17);
            this.zBox.Name = "zBox";
            this.zBox.Size = new System.Drawing.Size(51, 20);
            this.zBox.TabIndex = 6;
            this.zBox.Leave += new System.EventHandler(this.valueBox_Leave);
            this.zBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.value_KeyDown);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(147, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(22, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Z:";
            // 
            // ParamVectorEdit
            // 
            this.Controls.Add(this.zBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.yBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.xBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamVectorEdit";
            this.Size = new System.Drawing.Size(226, 44);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}