using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.UI
{
    internal class SplineTreeItem
    {
        public int Id { get; }
        public SplineRoad Spline { get; }

        public SplineTreeItem(int id, SplineRoad spline)
        {
            Id = id;
            Spline = spline;
        }
    }

    public class ParamSplineList : UserControl
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

        public ParamSplineList()
        {
            InitializeComponent();
            label1.Text = Localizer.Localize(label1.Text);
            buttonAssign.Text = Localizer.Localize(buttonAssign.Text);
            buttonClear.Text = Localizer.Localize(buttonClear.Text);
        }

        private void ParamSplineList_Load(object sender, EventArgs e) => UpdateList();
        public void UpdateUI() => UpdateList();

        private void UpdateList()
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            imageList.Images.Clear();
            for (int i = 0; i < SplineManager.MaxRoads; i++)
            {
                SplineRoad road = SplineManager.GetRoadFromId(i);
                bool hasEntry = false;
                string iconKey = null;
                string text = null;

                if (road.IsValid)
                {
                    SplineInventory.Entry entry = road.Entry;
                    if (entry.IsValid)
                    {
                        iconKey = entry.IconName;
                        if (!imageList.Images.ContainsKey(iconKey))
                        {
                            imageList.Images.Add(iconKey, entry.Icon);
                        }
                        text = entry.DisplayName;
                        hasEntry = true;
                    }
                }

                if (!hasEntry)
                {
                    iconKey = "empty16";
                    if (!imageList.Images.ContainsKey(iconKey))
                    {
                        imageList.Images.Add(iconKey, Resources.empty16);
                    }
                    text = Localizer.Localize("PARAM_EMPTY");
                }

                SplineTreeItem tag = new SplineTreeItem(i, road);
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
            if (m_value >= 0 && m_value < treeView.Nodes.Count && treeView.SelectedNode != treeView.Nodes[m_value])
            {
                treeView.SelectedNode = treeView.Nodes[m_value];
            }
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            SplineTreeItem item = GetCurrentItem();
            if (item != null)
            {
                buttonAssign.Enabled = true;
                buttonClear.Enabled = item.Spline.IsValid && item.Spline.Entry.IsValid;
            }
            else
            {
                buttonAssign.Enabled = false;
                buttonClear.Enabled = false;
            }
        }

        private SplineTreeItem GetCurrentItem()
        {
            if (treeView.SelectedNode == null)
            {
                return null;
            }
            return (SplineTreeItem)treeView.SelectedNode.Tag;
        }

        private void buttonAssign_Click(object sender, EventArgs e)
        {
            AssignToSelected();
        }

        private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            AssignToSelected();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SplineTreeItem currentItem = GetCurrentItem();
            OnValueChanged(currentItem?.Id ?? -1);
            UpdateButtons();
        }

        private void AssignToSelected()
        {
            SplineTreeItem currentItem = GetCurrentItem();
            if (currentItem == null)
                return;

            using (PromptInventory promptInventory = new PromptInventory())
            {
                promptInventory.Root = SplineInventory.Instance.Root;
                promptInventory.Value = (currentItem.Spline.IsValid ? currentItem.Spline.Entry : SplineInventory.Entry.Null);
                if (promptInventory.ShowDialog(this) != DialogResult.Cancel)
                {
                    AssignSplineId(currentItem.Id, (SplineInventory.Entry)promptInventory.Value);
                }
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            SplineTreeItem currentItem = GetCurrentItem();
            if (currentItem != null)
            {
                AssignSplineId(currentItem.Id, SplineInventory.Entry.Null);
            }
        }

        private void AssignSplineId(int id, SplineInventory.Entry entry)
        {
            Win32.SetRedraw(this, false);
            UndoManager.RecordUndo();
            if (!entry.IsValid)
            {
                SplineManager.DestroyRoad(id);
            }
            else
            {
                SplineRoad road = SplineManager.GetRoadFromId(id);
                if (!road.IsValid)
                {
                    road = SplineManager.CreateRoad(id);
                }
                road.Entry = entry;
                road.UpdateSpline();
            }
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
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "PARAM_ROADS";
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
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 156);
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
            this.treeView.Size = new System.Drawing.Size(212, 137);
            this.treeView.TabIndex = 5;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
            this.treeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeView_MouseDoubleClick);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ParamSplineList
            // 
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label1);
            this.Name = "ParamSplineList";
            this.Size = new System.Drawing.Size(218, 188);
            this.Load += new System.EventHandler(this.ParamSplineList_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}