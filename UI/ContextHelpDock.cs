using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Tools;
using TD.SandDock;

namespace FC2Editor.UI
{
    internal class ContextHelpDock : UserDockableWindow
    {
        private IContainer components = null;
        private Label contextHelpText;
        private ITool m_tool;
        private EventHandler m_contextHelpDynamicHandler;

        public string ContextHelp
        {
            get { return contextHelpText.Text; }
            set
            {
                if (value == null) value = "No context help defined.";

                Win32.SetRedraw(this, false);
                base.AutoScrollPosition = default(Point);
                contextHelpText.Text = value;
                UpdateTextSize();
                Win32.SetRedraw(this, true);
                Refresh();
            }
        }

        public ITool Tool
        {
            get { return m_tool; }
            set
            {
                if (m_tool is IContextHelpDynamic dynamicTool)
                {
                    dynamicTool.ContextHelpChanged -= m_contextHelpDynamicHandler;
                }
                m_tool = value;
                if (m_tool is IContextHelpDynamic newDynamicTool)
                {
                    newDynamicTool.ContextHelpChanged += m_contextHelpDynamicHandler;
                }
                UpdateContextHelp();
            }
        }

        public ContextHelpDock()
        {
            InitializeComponent();
            Text = Localizer.Localize(Text);
            m_contextHelpDynamicHandler = _ContextHelpChanged;
        }

        private void UpdateTextSize()
        {
            Size clientSize = base.ClientSize;
            clientSize.Width -= SystemInformation.VerticalScrollBarWidth;
            Size preferredSize = contextHelpText.GetPreferredSize(clientSize);
            contextHelpText.Size = preferredSize;
        }

        private void ContextHelpDock_SizeChanged(object sender, EventArgs e)
        {
            UpdateTextSize();
        }

        private void UpdateContextHelp()
        {
            if (Tool == null)
            {
                ContextHelp = Localizer.Localize("HELP_WELCOME");
            }
            else
            {
                ContextHelp = Tool.GetContextHelp();
            }
        }

        private void _ContextHelpChanged(object sender, EventArgs e)
        {
            UpdateContextHelp();
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
            this.contextHelpText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // contextHelpText
            // 
            this.contextHelpText.BackColor = System.Drawing.SystemColors.Info;
            this.contextHelpText.ForeColor = System.Drawing.SystemColors.InfoText;
            this.contextHelpText.Location = new System.Drawing.Point(0, 0);
            this.contextHelpText.Name = "contextHelpText";
            this.contextHelpText.Padding = new System.Windows.Forms.Padding(3);
            this.contextHelpText.Size = new System.Drawing.Size(165, 16);
            this.contextHelpText.TabIndex = 0;
            this.contextHelpText.Text = "Placeholder help text.";
            // 
            // ContextHelpDock
            // 
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.Controls.Add(this.contextHelpText);
            this.Name = "ContextHelpDock";
            this.Padding = new System.Windows.Forms.Padding(2);
            this.Size = new System.Drawing.Size(167, 146);
            this.Text = "DOCK_CONTEXT_HELP";
            this.SizeChanged += new System.EventHandler(this.ContextHelpDock_SizeChanged);
            this.ResumeLayout(false);
        }
    }
}