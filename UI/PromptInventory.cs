using FC2Editor.Core.Nomad;
using FC2Editor.Properties;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    internal class PromptInventory : Form
    {
        private IContainer components = null;
        private Label label1;
        private InventoryTree inventoryTree;
        private NomadButton buttonCancel;
        private NomadButton buttonOK;

        public Inventory.Entry Root
        {
            get { return inventoryTree.Root; }
            set { inventoryTree.Root = value; }
        }

        public Inventory.Entry Value
        {
            get { return inventoryTree.Value; }
            set { inventoryTree.Value = value; }
        }

        public PromptInventory()
        {
            InitializeComponent();
            base.Icon = new Icon(new System.IO.MemoryStream(Resources.appIcon));
            Text = Localizer.Localize(Text);
            label1.Text = Localizer.Localize(label1.Text);
            buttonOK.Text = Localizer.Localize(buttonOK.Text);
            buttonCancel.Text = Localizer.Localize(buttonCancel.Text);
        }

        private void inventoryTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (Value != null && Value.Count == 0)
            {
                base.DialogResult = DialogResult.OK;
            }
        }

        private void inventoryTree_ValueChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = Value != null && Value.Count == 0;
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
            this.label1 = new System.Windows.Forms.Label();
            this.buttonCancel = new FC2Editor.UI.NomadButton();
            this.buttonOK = new FC2Editor.UI.NomadButton();
            this.inventoryTree = new FC2Editor.UI.InventoryTree();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "PROMPT_INVENTORY_TEXT";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(209, 273);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "PROMPT_CANCEL";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(128, 273);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "PROMPT_OK";
            // 
            // inventoryTree
            // 
            this.inventoryTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inventoryTree.HideSelection = false;
            this.inventoryTree.ImageIndex = 0;
            this.inventoryTree.Location = new System.Drawing.Point(15, 25);
            this.inventoryTree.Name = "inventoryTree";
            this.inventoryTree.Root = null;
            this.inventoryTree.SelectedImageIndex = 0;
            this.inventoryTree.Size = new System.Drawing.Size(269, 244);
            this.inventoryTree.TabIndex = 1;
            this.inventoryTree.Value = null;
            this.inventoryTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.inventoryTree_NodeMouseDoubleClick);
            this.inventoryTree.ValueChanged += new System.EventHandler(this.inventoryTree_ValueChanged);
            // 
            // PromptInventory
            // 
            this.AcceptButton = this.buttonOK;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(296, 300);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.inventoryTree);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PromptInventory";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PROMPT_INVENTORY_TITLE";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}