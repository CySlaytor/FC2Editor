using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.UI
{
    // Note: The original decompiled code did not have this helper class, 
    // but it's good practice to create it for type safety with the tree nodes.
    internal class CollectionTreeItem
    {
        public Inventory.Entry Entry { get; }
        public int Id { get; }

        public CollectionTreeItem(int id, Inventory.Entry entry)
        {
            Id = id;
            Entry = entry;
        }
    }

    internal class ParamObjectInventoryTree : UserControl, ObjectRenderer.IListener
    {
        private const int smallSize = 64;
        private const int mediumSize = 96;
        private const int largeSize = 128;

        private IContainer components = null;
        private Label label1;
        private IndentedComboBox categoryComboBox;
        private ListView objectList;
        private ImageList imageList;
        private ContextMenuStrip contextMenu;
        private ToolStripMenuItem largeImagesToolStripMenuItem;
        private ToolStripMenuItem mediumImagesToolStripMenuItem;
        private ToolStripMenuItem smallImagesToolStripMenuItem;
        private NomadButton parentButton;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label3;
        private Label label2;
        private Label selObjNameLabel;
        private Label selObjSizeLabel;
        private Timer flushTimer;

        private Dictionary<ObjectInventory.Entry, ListViewItem> m_listItems = new Dictionary<ObjectInventory.Entry, ListViewItem>();
        private List<ObjectInventory.Entry> m_cachedEntries = new List<ObjectInventory.Entry>();
        private List<Image> m_cachedImages = new List<Image>();
        private AABB m_objectSize;

        public ObjectInventory.Entry Value
        {
            get
            {
                if (objectList.SelectedItems.Count == 0)
                    return categoryComboBox.SelectedItem?.Tag as ObjectInventory.Entry;

                return objectList.SelectedItems[0]?.Tag as ObjectInventory.Entry;
            }
            set
            {
                if (value == null)
                {
                    categoryComboBox.SelectedItem = categoryComboBox.FirstItem;
                    return;
                }

                ObjectInventory.Entry parent = (ObjectInventory.Entry)value.Parent;
                ObjectInventory.Entry category = (value.Count > 0) ? value : ((parent != null && parent.Pointer != IntPtr.Zero) ? parent : null);
                categoryComboBox.SelectedItem = FindCategory(category);
                objectList.SelectedItems.Clear();

                foreach (ListViewItem item in objectList.Items)
                {
                    if (item.Tag is ObjectInventory.Entry entry && entry.Pointer == value.Pointer)
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                        break;
                    }
                }
            }
        }

        public AABB ObjectSize
        {
            get { return m_objectSize; }
            set
            {
                m_objectSize = value;
                selObjSizeLabel.Text = m_objectSize.ToString();
            }
        }

        public event EventHandler ValueChanged;

        public ParamObjectInventoryTree()
        {
            InitializeComponent();
            label1.Text = Localizer.Localize(label1.Text);
            label2.Text = Localizer.Localize(label2.Text);
            label3.Text = Localizer.Localize(label3.Text);
            smallImagesToolStripMenuItem.Text = Localizer.Localize(smallImagesToolStripMenuItem.Text);
            mediumImagesToolStripMenuItem.Text = Localizer.Localize(mediumImagesToolStripMenuItem.Text);
            largeImagesToolStripMenuItem.Text = Localizer.Localize(largeImagesToolStripMenuItem.Text);
            Win32.SendMessage(objectList.Handle, Win32.LVM_SETEXTENDEDLISTVIEWSTYLE, Win32.LVS_EX_BORDERSELECT, Win32.LVS_EX_BORDERSELECT);
            UpdateImageSize(Editor.GetRegistryInt("ObjectThumbnailSize", 96));
            ObjectRenderer.RegisterListener(this);
            FillCategories((ObjectInventory.Entry)ObjectInventory.Instance.Root, categoryComboBox.Root);
            categoryComboBox.UpdateItems();
        }

        private void DoDispose()
        {
            ObjectRenderer.UnregisterListener(this);
        }

        private void FillCategories(ObjectInventory.Entry entry, IndentedComboBox.Item item)
        {
            if (entry.Count != 0)
            {
                IndentedComboBox.Item item2 = new IndentedComboBox.Item
                {
                    Tag = entry,
                    Text = entry.DisplayName,
                    Image = entry.Icon
                };
                item.Add(item2);
                foreach (Inventory.Entry child in entry.Children)
                {
                    FillCategories((ObjectInventory.Entry)child, item2);
                }
            }
        }

        private IndentedComboBox.Item FindCategory(ObjectInventory.Entry entry)
        {
            foreach (IndentedComboBox.Item item in categoryComboBox.GetItems())
            {
                if (item.Tag is ObjectInventory.Entry entry2 && entry2.Pointer == entry.Pointer)
                {
                    return item;
                }
            }
            return null;
        }

        private int AddCustomIcon(Image icon)
        {
            Size imageSize = imageList.ImageSize;
            using (Bitmap bitmap = new Bitmap(imageSize.Width, imageSize.Height))
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawImageUnscaled(icon, (imageSize.Width - icon.Width) / 2, (imageSize.Height - icon.Height) / 2);
                }
                imageList.Images.Add(bitmap);
            }
            return imageList.Images.Count - 1;
        }

        private void RefreshList()
        {
            ObjectRenderer.Clear();
            objectList.Items.Clear();
            imageList.Images.Clear();
            m_listItems.Clear();
            m_cachedEntries.Clear();
            m_cachedImages.Clear();
            AddCustomIcon(Resources.thumbwait);
            int folderIconIndex = -1;
            objectList.BeginUpdate();
            IndentedComboBox.Item selectedItem = categoryComboBox.SelectedItem;
            if (selectedItem != null)
            {
                ObjectInventory.Entry entry = (ObjectInventory.Entry)selectedItem.Tag;
                foreach (Inventory.Entry child in entry.Children)
                {
                    ObjectInventory.Entry entry2 = (ObjectInventory.Entry)child;
                    ListViewItem listViewItem = new ListViewItem(entry2.DisplayName);
                    m_listItems.Add(entry2, listViewItem);
                    listViewItem.Tag = entry2;
                    listViewItem.ImageIndex = 0;
                    objectList.Items.Add(listViewItem);
                    if (entry2.Count > 0)
                    {
                        if (folderIconIndex == -1)
                        {
                            folderIconIndex = AddCustomIcon(Resources.folder_big);
                        }
                        listViewItem.ImageIndex = folderIconIndex;
                    }
                    else
                    {
                        ObjectRenderer.RequestObjectImage(entry2);
                    }
                }
                parentButton.Enabled = entry.Parent.IsValid;
            }
            else
            {
                parentButton.Enabled = false;
            }
            objectList.EndUpdate();
        }

        public void ProcessObject(ObjectInventory.Entry entry, Image img)
        {
            int width = imageList.ImageSize.Width;
            int height = imageList.ImageSize.Height;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                using (SolidBrush brush = new SolidBrush(objectList.BackColor))
                {
                    graphics.FillRectangle(brush, rect);
                }
                graphics.DrawImage(img, rect);
            }
            m_cachedEntries.Add(entry);
            m_cachedImages.Add(bitmap);
        }

        private void FlushImages()
        {
            if (m_cachedEntries.Count == 0)
                return;

            int baseIndex = imageList.Images.Count;
            imageList.Images.AddRange(m_cachedImages.ToArray());
            for (int i = 0; i < m_cachedEntries.Count; i++)
            {
                if (m_listItems.TryGetValue(m_cachedEntries[i], out ListViewItem value))
                {
                    value.ImageIndex = baseIndex + i;
                }
                m_cachedImages[i].Dispose();
            }
            m_cachedEntries.Clear();
            m_cachedImages.Clear();
        }

        private void flushTimer_Tick(object sender, EventArgs e)
        {
            FlushImages();
        }

        private void OnValueChanged()
        {
            this.ValueChanged?.Invoke(this, new EventArgs());

            if (Value == null || Value.Count > 0)
            {
                selObjNameLabel.Text = Localizer.Localize("PARAM_OBJECT_BROWSER_NONE");
            }
            else
            {
                selObjNameLabel.Text = Value.DisplayName;
            }
            selObjSizeLabel.Text = Localizer.Localize("PARAM_NA");
        }

        private void indentedComboBox1_SelectedItemChanged(object sender, IndentedComboboxItemEventArgs e)
        {
            RefreshList();
            objectList.Focus();
            OnValueChanged();
        }

        private void parentButton_Click(object sender, EventArgs e)
        {
            if (categoryComboBox.SelectedItem?.Tag is ObjectInventory.Entry entry && entry.IsValid)
            {
                categoryComboBox.SelectedItem = FindCategory((ObjectInventory.Entry)entry.Parent);
            }
        }

        private void objectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnValueChanged();
        }

        private void objectList_DoubleClick(object sender, EventArgs e)
        {
            if (objectList.SelectedItems.Count != 0 && objectList.SelectedItems[0].Tag is ObjectInventory.Entry entry && entry.Count > 0)
            {
                categoryComboBox.SelectedItem = FindCategory(entry);
            }
        }

        private void UpdateImageSize(int size)
        {
            Editor.SetRegistryInt("ObjectThumbnailSize", size);
            imageList.ImageSize = new Size(size, size);
            Win32.SendMessage(objectList.Handle, 4149, 0, -1); // LVM_SETICONSPACING
            Win32.SendMessage(objectList.Handle, 4149, 0, Win32.MakeLong(size + 8, size + 24));
            RefreshList();
        }

        private void contextMenu_Opening(object sender, CancelEventArgs e)
        {
            smallImagesToolStripMenuItem.Checked = (imageList.ImageSize.Width == smallSize);
            mediumImagesToolStripMenuItem.Checked = (imageList.ImageSize.Width == mediumSize);
            largeImagesToolStripMenuItem.Checked = (imageList.ImageSize.Width == largeSize);
        }

        private void largeImagesToolStripMenuItem_Click(object sender, EventArgs e) => UpdateImageSize(largeSize);
        private void mediumImagesToolStripMenuItem_Click(object sender, EventArgs e) => UpdateImageSize(mediumSize);
        private void smallImagesToolStripMenuItem_Click(object sender, EventArgs e) => UpdateImageSize(smallSize);

        #region Component Designer generated code
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            DoDispose();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.objectList = new System.Windows.Forms.ListView();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.smallImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mediumImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.selObjNameLabel = new System.Windows.Forms.Label();
            this.selObjSizeLabel = new System.Windows.Forms.Label();
            this.parentButton = new FC2Editor.UI.NomadButton();
            this.categoryComboBox = new FC2Editor.UI.IndentedComboBox();
            this.flushTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenu.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "PARAM_OBJECT_BROWSER";
            // 
            // objectList
            // 
            this.objectList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectList.ContextMenuStrip = this.contextMenu;
            this.objectList.HideSelection = false;
            this.objectList.LargeImageList = this.imageList;
            this.objectList.Location = new System.Drawing.Point(3, 43);
            this.objectList.MultiSelect = false;
            this.objectList.Name = "objectList";
            this.objectList.Size = new System.Drawing.Size(203, 304);
            this.objectList.TabIndex = 7;
            this.objectList.UseCompatibleStateImageBehavior = false;
            this.objectList.DoubleClick += new System.EventHandler(this.objectList_DoubleClick);
            this.objectList.SelectedIndexChanged += new System.EventHandler(this.objectList_SelectedIndexChanged);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.smallImagesToolStripMenuItem,
            this.mediumImagesToolStripMenuItem,
            this.largeImagesToolStripMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(236, 70);
            this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Opening);
            // 
            // smallImagesToolStripMenuItem
            // 
            this.smallImagesToolStripMenuItem.Name = "smallImagesToolStripMenuItem";
            this.smallImagesToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.smallImagesToolStripMenuItem.Text = "MENUITEM_THUMBNAIL_SMALL";
            this.smallImagesToolStripMenuItem.Click += new System.EventHandler(this.smallImagesToolStripMenuItem_Click);
            // 
            // mediumImagesToolStripMenuItem
            // 
            this.mediumImagesToolStripMenuItem.Name = "mediumImagesToolStripMenuItem";
            this.mediumImagesToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.mediumImagesToolStripMenuItem.Text = "MENUITEM_THUMBNAIL_MEDIUM";
            this.mediumImagesToolStripMenuItem.Click += new System.EventHandler(this.mediumImagesToolStripMenuItem_Click);
            // 
            // largeImagesToolStripMenuItem
            // 
            this.largeImagesToolStripMenuItem.Name = "largeImagesToolStripMenuItem";
            this.largeImagesToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.largeImagesToolStripMenuItem.Text = "MENUITEM_THUMBNAIL_LARGE";
            this.largeImagesToolStripMenuItem.Click += new System.EventHandler(this.largeImagesToolStripMenuItem_Click);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(96, 96);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.selObjNameLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.selObjSizeLabel, 1, 1);
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 351);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(203, 32);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(0, 16);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "PARAM_OBJECT_BROWSER_SIZE";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "PARAM_OBJECT_BROWSER_SELECTED";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // selObjNameLabel
            // 
            this.selObjNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.selObjNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selObjNameLabel.Location = new System.Drawing.Point(101, 0);
            this.selObjNameLabel.Margin = new System.Windows.Forms.Padding(0);
            this.selObjNameLabel.Name = "selObjNameLabel";
            this.selObjNameLabel.Size = new System.Drawing.Size(102, 16);
            this.selObjNameLabel.TabIndex = 5;
            this.selObjNameLabel.Text = "None";
            this.selObjNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // selObjSizeLabel
            // 
            this.selObjSizeLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.selObjSizeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selObjSizeLabel.Location = new System.Drawing.Point(101, 16);
            this.selObjSizeLabel.Margin = new System.Windows.Forms.Padding(0);
            this.selObjSizeLabel.Name = "selObjSizeLabel";
            this.selObjSizeLabel.Size = new System.Drawing.Size(102, 16);
            this.selObjSizeLabel.TabIndex = 7;
            this.selObjSizeLabel.Text = "None";
            this.selObjSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // parentButton
            // 
            this.parentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.parentButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.parentButton.Image = global::FC2Editor.Properties.Resources.up;
            this.parentButton.Location = new System.Drawing.Point(182, 17);
            this.parentButton.Name = "parentButton";
            this.parentButton.Size = new System.Drawing.Size(26, 26);
            this.parentButton.TabIndex = 8;
            this.parentButton.Click += new System.EventHandler(this.parentButton_Click);
            // 
            // categoryComboBox
            // 
            this.categoryComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryComboBox.Location = new System.Drawing.Point(3, 18);
            this.categoryComboBox.MaximumSize = new System.Drawing.Size(9999, 24);
            this.categoryComboBox.MinimumSize = new System.Drawing.Size(0, 24);
            this.categoryComboBox.Name = "categoryComboBox";
            this.categoryComboBox.SelectedItem = null;
            this.categoryComboBox.Size = new System.Drawing.Size(176, 24);
            this.categoryComboBox.TabIndex = 6;
            this.categoryComboBox.SelectedItemChanged += new System.EventHandler<FC2Editor.UI.IndentedComboboxItemEventArgs>(this.indentedComboBox1_SelectedItemChanged);
            // 
            // flushTimer
            // 
            this.flushTimer.Enabled = true;
            this.flushTimer.Interval = 250;
            this.flushTimer.Tick += new System.EventHandler(this.flushTimer_Tick);
            // 
            // ParamObjectInventoryTree
            // 
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.parentButton);
            this.Controls.Add(this.objectList);
            this.Controls.Add(this.categoryComboBox);
            this.Controls.Add(this.label1);
            this.Name = "ParamObjectInventoryTree";
            this.Size = new System.Drawing.Size(209, 388);
            this.contextMenu.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}