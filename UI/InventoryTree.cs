using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;

namespace FC2Editor.UI
{
    internal class InventoryTree : TreeView
    {
        private Inventory.Entry m_root;
        private Dictionary<Inventory.Entry, TreeNode> m_nodes = new Dictionary<Inventory.Entry, TreeNode>();
        private ImageList imageList;

        public Inventory.Entry Root
        {
            get { return m_root; }
            set { m_root = value; UpdateTree(); }
        }

        public Inventory.Entry Value
        {
            get
            {
                if (base.SelectedNode == null)
                    return null;
                return (Inventory.Entry)base.SelectedNode.Tag;
            }
            set
            {
                if (value != null && m_nodes.TryGetValue(value, out TreeNode node))
                {
                    base.SelectedNode = node;
                    node.EnsureVisible();
                }
            }
        }

        public event EventHandler ValueChanged;

        public InventoryTree()
        {
            imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16)
            };
            base.ImageList = imageList;
            base.HideSelection = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            imageList.Dispose();
        }

        private void FillNodes(Inventory.Entry root, TreeNodeCollection nodes)
        {
            Inventory.Entry[] children = root.Children;
            foreach (Inventory.Entry entry in children)
            {
                string iconName = entry.IconName;
                if (!imageList.Images.ContainsKey(iconName))
                {
                    imageList.Images.Add(iconName, entry.Icon);
                }

                TreeNode treeNode = nodes.Add(entry.DisplayName);
                treeNode.ImageKey = iconName;
                treeNode.SelectedImageKey = iconName;
                treeNode.Tag = entry;
                m_nodes.Add(entry, treeNode);
                FillNodes(entry, treeNode.Nodes);
            }
        }

        private void UpdateTree()
        {
            BeginUpdate();
            imageList.Images.Clear();
            base.Nodes.Clear();
            m_nodes.Clear();
            if (m_root != null)
            {
                FillNodes(m_root, base.Nodes);
            }
            EndUpdate();
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);
            this.ValueChanged?.Invoke(this, e);
        }
    }
}