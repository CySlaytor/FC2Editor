using System;
using System.Windows.Forms;
using FC2Editor.Parameters;
using FC2Editor.UI;

namespace FC2Editor.Tools
{
    internal class ParamCollection : Parameter
    {
        protected int m_value = -1;

        public int Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                this.ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler ValueChanged;

        public ParamCollection(string display) : base(display) { }

        protected override Control CreateUIControl()
        {
            ParamCollectionList list = new ParamCollectionList
            {
                Value = m_value
            };
            list.ValueChanged += delegate (object sender, EventArgs e)
            {
                Value = ((ParamCollectionList)sender).Value;
            };
            return list;
        }

        protected override void UpdateUIControl(Control control)
        {
            ((ParamCollectionList)control).UpdateUI();
        }
    }
}