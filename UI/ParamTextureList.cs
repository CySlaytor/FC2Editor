using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.UI
{
    internal class TextureTreeItem
    {
        public Inventory.Entry Entry { get; }
        public int Id { get; }

        public TextureTreeItem(int id, Inventory.Entry entry)
        {
            Id = id;
            Entry = entry;
        }
    }

    public class ParamTextureList : UserControl
    {
        private IContainer components = null;
        private Label label1;
        private TableLayoutPanel tableLayoutPanel1;
        private NomadButton buttonClear;
        private NomadButton buttonAssign;
        private TreeView treeView;
        private ImageList imageList;
        private int m_value;

        public int Value
        {
            get { return m_value; }
            set { m_value = value; UpdateSelection(); }
        }

        public event EventHandler ValueChanged;

        public ParamTextureList()
        {
            InitializeComponent();
            label1.Text = Localizer.Localize(label1.Text);
            buttonAssign.Text = Localizer.Localize(buttonAssign.Text);
            buttonClear.Text = Localizer.Localize(buttonClear.Text);
        }

        private void ParamTextureList_Load(object sender, EventArgs e) => UpdateList();
        public void UpdateUI() => UpdateList();

        private void UpdateList()
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            imageList.Images.Clear();
            for (int i = 0; i < 4; i++)
            {
                TextureInventory.Entry entry = TerrainManager.GetTextureEntryFromId(i);
                string iconKey;
                string text;
                if (entry.IsValid)
                {
                    iconKey = entry.IconName;
                    if (!imageList.Images.ContainsKey(iconKey))
                    {
                        imageList.Images.Add(iconKey, entry.Icon);
                    }
                    text = entry.DisplayName;
                }
                else
                {
                    iconKey = "empty16";
                    if (!imageList.Images.ContainsKey(iconKey))
                    {
                        imageList.Images.Add(iconKey, Resources.empty16);
                    }
                    text = Localizer.Localize("PARAM_EMPTY");
                }
                TextureTreeItem tag = new TextureTreeItem(i, entry);
                TreeNode node = treeView.Nodes.Add(text);
                node.ImageKey = iconKey;
                node.SelectedImageKey = iconKey;
                node.Tag = tag;
            }
            UpdateSelection();
            treeView.EndUpdate();
        }

        private void UpdateSelection()
        {
            if (m_value >= 0 && m_value < treeView.Nodes.Count)
            {
                treeView.SelectedNode = treeView.Nodes[m_value];
            }
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            TextureTreeItem item = GetCurrentItem();
            if (item != null)
            {
                buttonAssign.Enabled = true;
                buttonClear.Enabled = item.Entry.IsValid && item.Id > 0;
            }
            else
            {
                buttonAssign.Enabled = false;
                buttonClear.Enabled = false;
            }
        }

        private TextureTreeItem GetCurrentItem()
        {
            if (treeView.SelectedNode == null)
            {
                return null;
            }
            return (TextureTreeItem)treeView.SelectedNode.Tag;
        }

        private void buttonAssign_Click(object sender, EventArgs e) => AssignToSelected();
        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => AssignToSelected();

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TextureTreeItem currentItem = GetCurrentItem();
            OnValueChanged(currentItem?.Id ?? -1);
            UpdateButtons();
        }

        private void AssignToSelected()
        {
            TextureTreeItem currentItem = GetCurrentItem();
            if (currentItem == null)
                return;

            using (PromptInventory promptInventory = new PromptInventory())
            {
                promptInventory.Root = TextureInventory.Instance.Root;
                promptInventory.Value = currentItem.Entry;
                if (promptInventory.ShowDialog(this) != DialogResult.Cancel)
                {
                    AssignTextureId(currentItem.Id, (TextureInventory.Entry)promptInventory.Value);
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            TextureTreeItem currentItem = GetCurrentItem();
            if (currentItem != null && currentItem.Id != 0)
            {
                AssignTextureId(currentItem.Id, TextureInventory.Entry.Null);
            }
        }

        private void AssignTextureId(int id, TextureInventory.Entry entry)
        {
            Win32.SetRedraw(this, false);
            UndoManager.RecordUndo();
            if (!entry.IsValid)
            {
                TerrainManager.ClearTextureId(id);
            }
            TerrainManager.AssignTextureId(id, entry);
            UndoManager.CommitUndo();
            UpdateList();
            Win32.SetRedraw(this, true);
            Refresh();
        }

        private void OnValueChanged(int value)
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonClear = new FC2Editor.UI.NomadButton();
            this.buttonAssign = new FC2Editor.UI.NomadButton();
            this.treeView = new System.Windows.Forms.TreeView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "PARAM_TEXTURES";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.buttonClear, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAssign, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 102);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(212, 29);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // buttonClear
            // 
            this.buttonClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonClear.Location = new System.Drawing.Point(109, 3);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(100, 23);
            this.buttonClear.TabIndex = 4;
            this.buttonClear.Text = "PARAM_CLEAR";
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonAssign
            // 
            this.buttonAssign.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonAssign.Location = new System.Drawing.Point(3, 3);
            this.buttonAssign.Name = "buttonAssign";
            this.buttonAssign.Size = new System.Drawing.Size(100, 23);
            this.buttonAssign.TabIndex = 3;
            this.buttonAssign.Text = "PARAM_ASSIGN";
            this.buttonAssign.Click += new System.EventHandler(this.buttonAssign_Click);
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.imageList;
            this.treeView.Location = new System.Drawing.Point(6, 16);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.ShowLines = false;
            this.treeView.ShowPlusMinus = false;
            this.treeView.ShowRootLines = false;
            this.treeView.Size = new System.Drawing.Size(206, 83);
            this.treeView.TabIndex = 5;
            this.treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseDoubleClick);
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ParamTextureList
            // 
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Name = "ParamTextureList";
            this.Size = new System.Drawing.Size(218, 134);
            this.Load += new System.EventHandler(this.ParamTextureList_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}