using System.Collections.Generic;
using System.Windows.Forms;

namespace FC2Editor.Parameters
{
    internal abstract class Parameter : IParameter
    {
        protected string m_displayName = "Unassigned";
        private string m_tooltip;
        protected Dictionary<Control, object> m_uiControls = new Dictionary<Control, object>();

        public string DisplayName
        {
            get { return m_displayName; }
            set
            {
                m_displayName = value;
                OnDisplayChanged();
            }
        }

        public string ToolTip
        {
            get { return m_tooltip; }
            set { m_tooltip = value; }
        }

        public Parameter(string display)
        {
            m_displayName = display;
        }

        protected abstract Control CreateUIControl();

        public Control AcquireUIControl()
        {
            Control control = CreateUIControl();
            UpdateUIControl(control);
            m_uiControls.Add(control, null);
            return control;
        }

        public virtual void ReleaseUIControl(Control control)
        {
            control.Dispose();
            m_uiControls.Remove(control);
        }

        protected virtual void UpdateUIControl(Control control) { }

        public void UpdateUIControls()
        {
            foreach (Control key in m_uiControls.Keys)
            {
                UpdateUIControl(key);
            }
        }

        protected virtual void OnDisplayChanged() { }
    }
}