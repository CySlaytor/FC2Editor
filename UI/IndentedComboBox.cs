using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    public class IndentedComboBox : UserControl
    {
        public class Item
        {
            public IndentedComboBox Owner { get; set; }
            public Item Parent { get; set; }
            public Image Image { get; set; }
            public string Text { get; set; }
            public object Tag { get; set; }
            public int Depth { get; protected set; }
            private List<Item> m_childList = new List<Item>();

            public void Clear()
            {
                m_childList.Clear();
            }

            public void Add(Item item)
            {
                m_childList.Add(item);
                item.Depth = Depth + 1;
                item.Owner = Owner;
            }

            public IEnumerable<Item> GetChildren()
            {
                foreach (Item item in m_childList)
                {
                    yield return item;
                }
            }
        }

        private IContainer components = null;
        private ComboBox comboBox;
        private Item m_root;

        public Item Root => m_root;

        public Item SelectedItem
        {
            get { return comboBox.SelectedItem as Item; }
            set { comboBox.SelectedItem = value; }
        }

        public Item FirstItem
        {
            get
            {
                if (comboBox.Items.Count <= 0)
                {
                    return null;
                }
                return (Item)comboBox.Items[0];
            }
        }

        public event EventHandler<IndentedComboboxItemEventArgs> SelectedItemChanged;

        public IndentedComboBox()
        {
            InitializeComponent();
            m_root = new Item();
            m_root.Owner = this;
        }

        private void UpdateItem(Item item, Item selectedItem, ref int index)
        {
            foreach (Item child in item.GetChildren())
            {
                if (child == selectedItem)
                {
                    index = comboBox.Items.Count;
                }
                comboBox.Items.Add(child);
                UpdateItem(child, selectedItem, ref index);
            }
        }

        public void UpdateItems()
        {
            comboBox.BeginUpdate();
            Item selectedItem = comboBox.SelectedItem as Item;
            comboBox.Items.Clear();
            int index = -1;
            UpdateItem(m_root, selectedItem, ref index);
            if (index == -1 && comboBox.Items.Count > 0)
            {
                index = 0;
            }
            comboBox.SelectedIndex = index;
            comboBox.EndUpdate();
        }

        private void comboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                RectangleF bounds = e.Bounds;
                Item item = comboBox.Items[e.Index] as Item;
                SizeF sizeF = e.Graphics.MeasureString(item.Text, e.Font);

                bool isEdit = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;

                e.DrawBackground();

                if (!isEdit)
                {
                    int indent = 4 + (item.Depth - 1) * 16;
                    bounds.X += indent;
                    bounds.Width -= indent;
                }

                if (item.Image != null)
                {
                    e.Graphics.DrawImage(item.Image, new RectangleF(bounds.Location, new SizeF(16f, 16f)));
                    bounds.X += 18; // Give a bit of space
                    bounds.Width -= 18;
                }

                using (Brush brush = new SolidBrush(e.ForeColor))
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };
                    e.Graphics.DrawString(item.Text, e.Font, brush, bounds, stringFormat);
                }
                e.DrawFocusRectangle();
            }
        }

        public IEnumerable<Item> GetItems()
        {
            foreach (object obj in comboBox.Items)
            {
                yield return (Item)obj;
            }
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedItemChanged?.Invoke(this, new IndentedComboboxItemEventArgs(SelectedItem));
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
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBox
            // 
            this.comboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.ItemHeight = 18;
            this.comboBox.Location = new System.Drawing.Point(0, 0);
            this.comboBox.Margin = new System.Windows.Forms.Padding(0);
            this.comboBox.MaxDropDownItems = 16;
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(245, 24);
            this.comboBox.TabIndex = 0;
            this.comboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox_DrawItem);
            this.comboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // IndentedComboBox
            // 
            this.Controls.Add(this.comboBox);
            this.MaximumSize = new System.Drawing.Size(9999, 24);
            this.MinimumSize = new System.Drawing.Size(0, 24);
            this.Name = "IndentedComboBox";
            this.Size = new System.Drawing.Size(245, 24);
            this.ResumeLayout(false);
        }
    }

    public class IndentedComboboxItemEventArgs : EventArgs
    {
        public IndentedComboBox.Item Item { get; }

        public IndentedComboboxItemEventArgs(IndentedComboBox.Item item)
        {
            Item = item;
        }
    }
}