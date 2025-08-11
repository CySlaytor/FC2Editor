using System.Windows.Forms;
using FC2Editor.UI;

namespace FC2Editor.Parameters
{
    internal class ParamButton : Parameter
    {
        public delegate void ButtonDelegate();

        private bool m_enabled = true;
        private ButtonDelegate m_callback;

        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                m_enabled = value;
                UpdateUIControls();
            }
        }

        public ButtonDelegate Callback
        {
            get { return m_callback; }
            set { m_callback = value; }
        }

        public ParamButton(string display, ButtonDelegate callback) : base(display)
        {
            m_callback = callback;
        }

        protected override Control CreateUIControl()
        {
            NomadButton nomadButton = new NomadButton
            {
                Text = base.DisplayName
            };
            nomadButton.Click += delegate { m_callback(); };
            return nomadButton;
        }

        protected override void UpdateUIControl(Control control)
        {
            ((NomadButton)control).Enabled = m_enabled;
        }
    }
}