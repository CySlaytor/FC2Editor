using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.UI
{
    internal class ParamReport : UserControl
    {
        private IContainer components = null;
        private ListView listView;
        private Label parameterName;
        private ImageList imageList;
        private ColumnHeader columnHeader1;

        public string ParameterName
        {
            get { return parameterName.Text; }
            set { parameterName.Text = value; }
        }

        public ParamReport()
        {
            InitializeComponent();
            imageList.Images.AddRange(new Image[] { Resources.valid16, Resources.error16 });
        }

        public void UpdateUI(ValidationReport report)
        {
            listView.BeginUpdate();
            listView.Items.Clear();
            if (report.IsValid)
            {
                for (int i = 0; i < report.Count; i++)
                {
                    ValidationRecord validationRecord = report[i];
                    ListViewItem listViewItem = new ListViewItem(validationRecord.Message, (validationRecord.Severity != ValidationRecord.Severities.Success) ? 1 : 0);
                    listViewItem.Tag = validationRecord;
                    listView.Items.Add(listViewItem);
                }
            }
            listView.EndUpdate();
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
                return;

            if (listView.SelectedItems[0].Tag is ValidationRecord validationRecord && validationRecord.Object.IsValid)
            {
                Camera.Focus(validationRecord.Object);
            }
        }

        private void listView_Click(object sender, EventArgs e)
        {
            listView_SelectedIndexChanged(sender, e);
        }

        private void listView_Layout(object sender, LayoutEventArgs e)
        {
            listView.Columns[0].Width = listView.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 5;
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
            this.components = new System.ComponentModel.Container();
            this.listView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.parameterName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.columnHeader1 });
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView.Location = new System.Drawing.Point(0, 18);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(150, 199);
            this.listView.SmallImageList = this.imageList;
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            this.listView.Click += new System.EventHandler(this.listView_Click);
            this.listView.Layout += new System.Windows.Forms.LayoutEventHandler(this.listView_Layout);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = -2;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // parameterName
            // 
            this.parameterName.Dock = System.Windows.Forms.DockStyle.Top;
            this.parameterName.Location = new System.Drawing.Point(0, 0);
            this.parameterName.Name = "parameterName";
            this.parameterName.Size = new System.Drawing.Size(150, 18);
            this.parameterName.TabIndex = 1;
            this.parameterName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ParamReport
            // 
            this.Controls.Add(this.listView);
            this.Controls.Add(this.parameterName);
            this.Name = "ParamReport";
            this.Size = new System.Drawing.Size(150, 217);
            this.ResumeLayout(false);
        }
        #endregion
    }
}