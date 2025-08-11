using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;

namespace FC2Editor.UI
{
    internal class CodeHelper : UserControl
    {
        private IContainer components = null;
        private Label prototypeLabel;
        private Label descriptionLabel;

        public CodeHelper()
        {
            InitializeComponent();
        }

        public void Setup(Wilderness.FunctionDef function)
        {
            prototypeLabel.Text = function.Prototype;
            descriptionLabel.Text = function.Description;
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
            this.prototypeLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // prototypeLabel
            // 
            this.prototypeLabel.AutoSize = true;
            this.prototypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.prototypeLabel.Location = new System.Drawing.Point(-1, 2);
            this.prototypeLabel.Name = "prototypeLabel";
            this.prototypeLabel.Size = new System.Drawing.Size(68, 13);
            this.prototypeLabel.TabIndex = 0;
            this.prototypeLabel.Text = "Function();";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(-1, 18);
            this.descriptionLabel.MaximumSize = new System.Drawing.Size(500, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(218, 13);
            this.descriptionLabel.TabIndex = 1;
            this.descriptionLabel.Text = "Some helper text just to see how it looks like.";
            // 
            // CodeHelper
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.prototypeLabel);
            this.Name = "CodeHelper";
            this.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.Size = new System.Drawing.Size(220, 33);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}