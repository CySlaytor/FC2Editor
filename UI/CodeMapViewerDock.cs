using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using TD.SandDock;

namespace FC2Editor.UI
{
    internal class CodeMapViewerDock : UserDockableWindow
    {
        private IContainer components = null;
        private PictureBox pictureBox1;
        private Label statsLabel;
        private ImageMap m_imageMap;

        public ImageMap Image
        {
            get { return m_imageMap; }
            set
            {
                m_imageMap = value;
                UpdateUI();
            }
        }

        public CodeMapViewerDock()
        {
            InitializeComponent();
        }

        private void UpdateUI()
        {
            if (m_imageMap == null)
            {
                pictureBox1.Image = null;
                statsLabel.Text = "";
            }
            else
            {
                pictureBox1.Image = m_imageMap.Image;
                statsLabel.Text = "Minimum: " + m_imageMap.Minimum.ToString("F3") + "  Maximum: " + m_imageMap.Maximum.ToString("F3");
            }
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.statsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(250, 232);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // statsLabel
            // 
            this.statsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statsLabel.Location = new System.Drawing.Point(0, 232);
            this.statsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.statsLabel.Name = "statsLabel";
            this.statsLabel.Size = new System.Drawing.Size(250, 23);
            this.statsLabel.TabIndex = 1;
            this.statsLabel.Text = "Minimum: 0  Maximum: 255";
            this.statsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CodeMapViewerDock
            // 
            this.Controls.Add(this.statsLabel);
            this.Controls.Add(this.pictureBox1);
            this.Name = "CodeMapViewerDock";
            this.Size = new System.Drawing.Size(250, 255);
            this.Text = "Map Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
        }
    }
}