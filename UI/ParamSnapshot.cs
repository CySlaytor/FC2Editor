using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.UI
{
    public class ParamSnapshot : UserControl
    {
        private IContainer components = null;
        private Label label1;
        private PictureBox screenshotPicture;
        private TableLayoutPanel tableLayoutPanel1;
        private NomadButton buttonGotoCamera;
        private NomadButton buttonSetCamera;

        public ParamSnapshot()
        {
            InitializeComponent();
            label1.Text = Localizer.Localize(label1.Text);
            buttonSetCamera.Text = Localizer.Localize(buttonSetCamera.Text);
            buttonGotoCamera.Text = Localizer.Localize(buttonGotoCamera.Text);
            UpdateSnapshot();
        }

        private void UpdateSnapshot()
        {
            if (EditorDocument.IsSnapshotSet)
            {
                Snapshot snapshot = Snapshot.Create(160, 128);
                EditorDocument.TakeSnapshot(snapshot);
                screenshotPicture.Image = snapshot.GetImage();
                buttonGotoCamera.Enabled = true;
            }
            else
            {
                screenshotPicture.Image = Resources.emptySnapshot;
                buttonGotoCamera.Enabled = false;
            }
        }

        private void buttonSetCamera_Click(object sender, EventArgs e)
        {
            EditorDocument.SnapshotPos = Camera.Position;
            EditorDocument.SnapshotAngle = Camera.Angles;
            UpdateSnapshot();
        }

        private void buttonGotoCamera_Click(object sender, EventArgs e)
        {
            if (EditorDocument.IsSnapshotSet)
            {
                Camera.Position = EditorDocument.SnapshotPos;
                Camera.Angles = EditorDocument.SnapshotAngle;
            }
        }

        #region Component Designer generated code
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
            this.label1 = new System.Windows.Forms.Label();
            this.screenshotPicture = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonGotoCamera = new FC2Editor.UI.NomadButton();
            this.buttonSetCamera = new FC2Editor.UI.NomadButton();
            ((System.ComponentModel.ISupportInitialize)(this.screenshotPicture)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "PARAM_SNAPSHOT";
            // 
            // screenshotPicture
            // 
            this.screenshotPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenshotPicture.Location = new System.Drawing.Point(0, 15);
            this.screenshotPicture.Name = "screenshotPicture";
            this.screenshotPicture.Size = new System.Drawing.Size(212, 156);
            this.screenshotPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.screenshotPicture.TabIndex = 3;
            this.screenshotPicture.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.buttonGotoCamera, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonSetCamera, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 171);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(212, 29);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // buttonGotoCamera
            // 
            this.buttonGotoCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonGotoCamera.Location = new System.Drawing.Point(109, 3);
            this.buttonGotoCamera.Name = "buttonGotoCamera";
            this.buttonGotoCamera.Size = new System.Drawing.Size(100, 23);
            this.buttonGotoCamera.TabIndex = 4;
            this.buttonGotoCamera.Text = "PARAM_SNAPSHOT_GOTO";
            this.buttonGotoCamera.Click += new System.EventHandler(this.buttonGotoCamera_Click);
            // 
            // buttonSetCamera
            // 
            this.buttonSetCamera.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonSetCamera.Location = new System.Drawing.Point(3, 3);
            this.buttonSetCamera.Name = "buttonSetCamera";
            this.buttonSetCamera.Size = new System.Drawing.Size(100, 23);
            this.buttonSetCamera.TabIndex = 3;
            this.buttonSetCamera.Text = "PARAM_SNAPSHOT_SET";
            this.buttonSetCamera.Click += new System.EventHandler(this.buttonSetCamera_Click);
            // 
            // ParamSnapshot
            // 
            this.Controls.Add(this.screenshotPicture);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Name = "ParamSnapshot";
            this.Size = new System.Drawing.Size(212, 200);
            ((System.ComponentModel.ISupportInitialize)(this.screenshotPicture)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion
    }
}