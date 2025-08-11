using System;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.UI;

namespace FC2Editor.Parameters
{
    internal class ParamPickButton : Parameter
    {
        public delegate void PickDelegate(Vec2 normalizedMousePos);
        public delegate void StdDelegate();

        private Control m_captureControl;
        private bool m_keepCapture;
        private bool m_enabled = true;
        private PickDelegate m_pickCallback;
        private PickDelegate m_updateCallback;
        private StdDelegate m_cancelCallback;

        public bool KeepCapture
        {
            get { return m_keepCapture; }
            set { m_keepCapture = value; }
        }

        public bool Enabled
        {
            get { return m_enabled; }
            set
            {
                m_enabled = value;
                UpdateUIControls();
            }
        }

        public PickDelegate PickCallback
        {
            get { return m_pickCallback; }
            set { m_pickCallback = value; }
        }

        public PickDelegate UpdateCallback
        {
            get { return m_updateCallback; }
            set { m_updateCallback = value; }
        }

        public StdDelegate CancelCallback
        {
            get { return m_cancelCallback; }
            set { m_cancelCallback = value; }
        }

        public ParamPickButton(string display) : base(display) { }

        protected override Control CreateUIControl()
        {
            NomadCheckButton nomadCheckButton = new NomadCheckButton
            {
                AutoCheck = false,
                Text = base.DisplayName
            };
            nomadCheckButton.Click += button_Click;
            return nomadCheckButton;
        }

        protected override void UpdateUIControl(Control control)
        {
            NomadCheckButton nomadCheckButton = (NomadCheckButton)control;
            nomadCheckButton.Enabled = m_enabled;
            nomadCheckButton.Checked = m_captureControl != null && m_captureControl.Capture;
        }

        private void button_Click(object sender, EventArgs e)
        {
            InitCapture();
        }

        public void InitCapture()
        {
            m_captureControl = new Control
            {
                Capture = true,
                Parent = MainForm.Instance // NOTE: This will require MainForm to be implemented later.
            };
            m_captureControl.MouseCaptureChanged += captureControl_MouseCaptureChanged;
            m_captureControl.MouseMove += captureControl_MouseMove;
            m_captureControl.MouseUp += captureControl_MouseUp;
            m_captureControl.KeyUp += captureControl_KeyUp;
            m_captureControl.Focus();
            Cursor.Current = Cursors.Cross;
            UpdateUIControls();
        }

        public void FinalizeCapture()
        {
            m_cancelCallback?.Invoke();
            UpdateUIControls();
            Cursor.Current = Cursors.Default;
            m_captureControl.Dispose();
        }

        private bool GetCapturePos(out Vec2 normalizedMousePos)
        {
            Point position = Cursor.Position;
            Point pt = Editor.Viewport.PointToClient(position);
            Rectangle clientRectangle = Editor.Viewport.ClientRectangle;

            if (clientRectangle.Contains(pt))
            {
                normalizedMousePos = new Vec2((float)pt.X / clientRectangle.Width, (float)pt.Y / clientRectangle.Height);
                return true;
            }

            normalizedMousePos = default(Vec2);
            return false;
        }

        private void captureControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (GetCapturePos(out Vec2 normalizedMousePos) && m_updateCallback != null)
            {
                m_updateCallback(normalizedMousePos);
            }
        }

        private void captureControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (GetCapturePos(out Vec2 normalizedMousePos) && m_pickCallback != null)
            {
                m_pickCallback(normalizedMousePos);
            }
        }

        private void captureControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                KeepCapture = false;
                FinalizeCapture();
            }
        }

        private void captureControl_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (KeepCapture)
            {
                KeepCapture = false;
                m_captureControl.Capture = true;
            }
            else
            {
                FinalizeCapture();
            }
        }
    }
}