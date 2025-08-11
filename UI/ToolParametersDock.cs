using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Tools;
using FC2Editor.Utils;
using TD.SandDock;

namespace FC2Editor.UI
{
    internal class ToolParametersDock : UserDockableWindow
    {
        private ITool m_tool;
        private IContainer components = null;
        private Panel panel1;
        private Label toolName;
        private PictureBox toolImage;
        private GroupBox groupBox;
        private ParametersList toolParametersList;
        private Label label1;

        public ITool Tool
        {
            get { return m_tool; }
            set
            {
                m_tool = value;
                bool visible = m_tool != null;
                groupBox.Visible = visible;
                toolName.Visible = visible;
                toolImage.Visible = visible;
                label1.Visible = !visible;
                toolParametersList.Parameters = value;
                if (m_tool != null)
                {
                    toolName.Text = StringUtils.EscapeUIString(m_tool.GetToolName());
                    toolImage.Image = m_tool.GetToolImage();
                }
            }
        }

        public ToolParametersDock()
        {
            InitializeComponent();
            Tool = null;
            Text = Localizer.Localize(Text);
            label1.Text = Localizer.Localize(label1.Text);
            groupBox.Text = Localizer.Localize(groupBox.Text);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolName = new System.Windows.Forms.Label();
            this.toolImage = new System.Windows.Forms.PictureBox();
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.toolParametersList = new FC2Editor.UI.ParametersList();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toolImage)).BeginInit();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.toolName);
            this.panel1.Controls.Add(this.toolImage);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 45);
            this.panel1.TabIndex = 1;
            // 
            // toolName
            // 
            this.toolName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolName.Location = new System.Drawing.Point(46, 3);
            this.toolName.Name = "toolName";
            this.toolName.Size = new System.Drawing.Size(202, 41);
            this.toolName.TabIndex = 1;
            this.toolName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolImage
            // 
            this.toolImage.Location = new System.Drawing.Point(6, 6);
            this.toolImage.Name = "toolImage";
            this.toolImage.Size = new System.Drawing.Size(32, 32);
            this.toolImage.TabIndex = 0;
            this.toolImage.TabStop = false;
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.toolParametersList);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 45);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(250, 355);
            this.groupBox.TabIndex = 2;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "EDITOR_PARAMETERS";
            // 
            // toolParametersList
            // 
            this.toolParametersList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolParametersList.Location = new System.Drawing.Point(3, 16);
            this.toolParametersList.Name = "toolParametersList";
            this.toolParametersList.Parameters = null;
            this.toolParametersList.Size = new System.Drawing.Size(244, 336);
            this.toolParametersList.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(250, 400);
            this.label1.TabIndex = 1;
            this.label1.Text = "TOOL_NOT_SELECTED";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ToolParametersDock
            // 
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Name = "ToolParametersDock";
            this.Text = "DOCK_TOOL_PARAMETERS";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.toolImage)).EndInit();
            this.groupBox.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}