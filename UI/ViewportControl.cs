using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.UI
{
    internal class ViewportControl : UserControl
    {
        private enum CameraModes
        {
            None,
            Lookaround,
            Panning
        }

        private const float kSpeedBoost = 5f;

        private bool m_blockNextKeyRepeats;
        private bool m_forceRefresh;
        private Vec2 m_normalizedMousePos;
        private bool m_captureMouse;
        private Point m_captureMousePos;
        private bool m_captureWheel;
        private Cursor m_invisibleCursor;
        private CameraModes m_cameraMode;
        private bool m_cameraEnabled = true;
        private bool m_mouseOver;
        private Cursor m_defaultCursor = Cursors.Default;
        private IContainer components = null;

        public bool BlockNextKeyRepeats
        {
            get { return m_blockNextKeyRepeats; }
            set { m_blockNextKeyRepeats = value; }
        }

        public bool ForceRefresh
        {
            get { return m_forceRefresh; }
            set { m_forceRefresh = value; }
        }

        public Vec2 NormalizedMousePos
        {
            get { return m_normalizedMousePos; }
            set { Cursor.Position = PointToScreen(new Point((int)(value.X * base.ClientSize.Width), (int)(value.Y * base.ClientSize.Height))); }
        }

        public bool CaptureMouse
        {
            get { return m_captureMouse; }
            set { if (m_captureMouse != value) { m_captureMouse = value; UpdateCaptureMouse(); } }
        }

        public Vec2 CaptureMousePos
        {
            set { m_captureMousePos = PointToScreen(new Point((int)(value.X * base.ClientSize.Width), (int)(value.Y * base.ClientSize.Height))); }
        }

        public bool CaptureWheel
        {
            get { return m_captureWheel; }
            set { m_captureWheel = value; }
        }

        private CameraModes CameraMode
        {
            get { return m_cameraMode; }
            set { m_cameraMode = value; UpdateCameraMode(); }
        }

        public bool CameraEnabled
        {
            get { return m_cameraEnabled; }
            set { m_cameraEnabled = value; }
        }

        public bool MouseOver => m_mouseOver;

        public new Cursor DefaultCursor
        {
            get { return m_defaultCursor; }
            set
            {
                if (Cursor == m_defaultCursor)
                {
                    Cursor = value;
                }
                m_defaultCursor = value;
            }
        }

        public ViewportControl()
        {
            InitializeComponent();
            BackColor = SystemColors.AppWorkspace;
            base.MouseWheel += ViewportControl_MouseWheel;
            m_invisibleCursor = new Cursor(new MemoryStream(Resources.invisible_cursor));
        }

        protected override bool IsInputKey(Keys keyData) => true;

        public override bool PreProcessMessage(ref Message msg)
        {
            bool isKeyDown = msg.Msg == 256 || msg.Msg == 260; // WM_KEYDOWN, WM_SYSKEYDOWN
            bool isRepeat = (msg.LParam.ToInt32() & 0x40000000) != 0;

            if (isKeyDown)
            {
                if (!isRepeat)
                {
                    BlockNextKeyRepeats = false;
                }
                else if (BlockNextKeyRepeats)
                {
                    return true;
                }
            }
            return base.PreProcessMessage(ref msg);
        }

        protected override bool ProcessKeyMessage(ref Message msg)
        {
            bool isKeyDown = msg.Msg == 256 || msg.Msg == 260; // WM_KEYDOWN, WM_SYSKEYDOWN
            bool isKeyUp = msg.Msg == 257 || msg.Msg == 261; // WM_KEYUP, WM_SYSKEYUP
            bool isRepeat = (msg.LParam.ToInt32() & 0x40000000) != 0;

            Keys keyData = (Keys)(msg.WParam.ToInt32() | (int)Control.ModifierKeys);
            KeyEventArgs keyEventArgs = new KeyEventArgs(keyData);

            if (!Editor.IsIngame)
            {
                UpdateCameraState();
            }

            if (!Engine.ConsoleOpened)
            {
                if (isKeyDown)
                {
                    if (!isRepeat)
                    {
                        Editor.OnKeyEvent(Editor.KeyEvent.KeyDown, keyEventArgs);
                    }
                    Editor.OnKeyEvent(Editor.KeyEvent.KeyChar, keyEventArgs);
                }
                else if (isKeyUp)
                {
                    Editor.OnKeyEvent(Editor.KeyEvent.KeyUp, keyEventArgs);
                }
            }
            return base.ProcessKeyMessage(ref msg);
        }

        private void ViewportControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (CameraMode != CameraModes.None)
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    Editor.OnMouseEvent(Editor.MouseEvent.MouseDown, e);
                    break;
                case MouseButtons.Middle:
                    if (!Editor.IsIngame && CameraEnabled)
                        CameraMode = CameraModes.Panning;
                    break;
                case MouseButtons.Right:
                    if (!Editor.IsIngame && CameraEnabled)
                        CameraMode = CameraModes.Lookaround;
                    break;
            }
        }

        private void ViewportControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (CameraMode == CameraModes.None)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Editor.OnMouseEvent(Editor.MouseEvent.MouseUp, e);
                }
            }
            else if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                CameraMode = CameraModes.None;
            }
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (CaptureMouse)
            {
                if (!MainForm.IsActive)
                    return;

                Point centerScreen = PointToScreen(new Point(base.Width / 2, base.Height / 2));
                int dx = Cursor.Position.X - centerScreen.X;
                int dy = Cursor.Position.Y - centerScreen.Y;

                if (dx != 0 || dy != 0)
                {
                    switch (CameraMode)
                    {
                        case CameraModes.Lookaround:
                            Camera.Rotate((EditorSettings.InvertMouseView ? dy : -dy) * 0.005f, 0f, -dx * 0.005f);
                            break;
                        case CameraModes.Panning:
                            Camera.Position += Camera.RightVector * dx * 0.125f + Camera.UpVector * (EditorSettings.InvertMousePan ? dy : -dy) * 0.125f;
                            break;
                        default:
                            Editor.OnMouseEvent(Editor.MouseEvent.MouseMoveDelta, new MouseEventArgs(e.Button, e.Clicks, dx, dy, e.Delta));
                            break;
                    }
                    Cursor.Position = centerScreen;
                }
            }
            else
            {
                m_normalizedMousePos = new Vec2((float)e.X / base.ClientSize.Width, (float)e.Y / base.ClientSize.Height);
                Editor.OnMouseEvent(Editor.MouseEvent.MouseMove, e);
            }
        }

        private void ViewportControl_MouseEnter(object sender, EventArgs e)
        {
            if (MainForm.IsActive)
            {
                Focus();
            }
            m_mouseOver = true;
            Editor.OnMouseEvent(Editor.MouseEvent.MouseEnter, null);
        }

        private void ViewportControl_Paint(object sender, PaintEventArgs e) { }

        protected override void WndProc(ref Message m)
        {
            if (!ForceRefresh && Editor.IsActive && m.Msg == 20) // WM_ERASEBKGND
            {
                m.Result = new IntPtr(1);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        public void UpdateFocus()
        {
            if (MainForm.IsActive)
            {
                if (CaptureMouse)
                {
                    Point position = PointToScreen(new Point(base.Width / 2, base.Height / 2));
                    Cursor.Position = position;
                    Cursor = m_invisibleCursor;
                }
            }
            else if (CaptureMouse)
            {
                Cursor = m_defaultCursor;
            }
        }

        private void ViewportControl_MouseLeave(object sender, EventArgs e)
        {
            if (CameraMode != CameraModes.None)
            {
                CameraMode = CameraModes.None;
            }
            m_mouseOver = false;
            Editor.OnMouseEvent(Editor.MouseEvent.MouseLeave, null);
        }

        private void ViewportControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!m_captureWheel)
            {
                if (!Editor.IsIngame)
                {
                    Camera.Position += Camera.FrontVector * e.Delta * 0.0625f;
                }
            }
            else
            {
                Editor.OnMouseEvent(Editor.MouseEvent.MouseWheel, e);
            }
        }

        private void Viewport_Leave(object sender, EventArgs e)
        {
            if (CameraMode != CameraModes.None)
            {
                CameraMode = CameraModes.None;
            }
            ResetCameraState();
        }

        public void UpdateSize()
        {
            if (base.ParentForm != null && base.ParentForm.WindowState != FormWindowState.Minimized)
            {
                Size clientSize = base.ClientSize;
                clientSize.Width = (int)(clientSize.Width * EditorSettings.ViewportQuality);
                clientSize.Height = (int)(clientSize.Height * EditorSettings.ViewportQuality);
                if (clientSize.Width < 16) clientSize.Width = 16;
                if (clientSize.Height < 16) clientSize.Height = 16;
                Engine.UpdateResolution(clientSize);
            }
        }

        private void ViewportControl_Resize(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void UpdateCaptureMouse()
        {
            if (CaptureMouse)
            {
                Cursor = m_invisibleCursor;
                m_captureMousePos = Cursor.Position;
                Cursor.Position = PointToScreen(new Point(base.Width / 2, base.Height / 2));
            }
            else
            {
                Cursor.Position = m_captureMousePos;
                Cursor = m_defaultCursor;
            }
        }

        private void ResetCameraState()
        {
            Camera.ForwardInput = 0f;
            Camera.LateralInput = 0f;
            Camera.SpeedFactor = 1f;
        }

        private void UpdateCameraState()
        {
            if (!Engine.Initialized)
                return;

            if (Engine.ConsoleOpened || !Focused)
            {
                ResetCameraState();
                return;
            }

            IntPtr keyboardLayout = Win32.GetKeyboardLayout(0);
            int keyW = Win32.MapVirtualKeyEx(17, 1, keyboardLayout);
            int keyS = Win32.MapVirtualKeyEx(31, 1, keyboardLayout);
            int keyA = Win32.MapVirtualKeyEx(30, 1, keyboardLayout);
            int keyD = Win32.MapVirtualKeyEx(32, 1, keyboardLayout);

            if (Win32.IsKeyDown(keyW))
                Camera.ForwardInput = 1f;
            else if (Win32.IsKeyDown(keyS))
                Camera.ForwardInput = -1f;
            else
                Camera.ForwardInput = 0f;

            if (Win32.IsKeyDown(keyA))
                Camera.LateralInput = -1f;
            else if (Win32.IsKeyDown(keyD))
                Camera.LateralInput = 1f;
            else
                Camera.LateralInput = 0f;

            if (Win32.IsKeyDown(160) || Win32.IsKeyDown(161)) // LSHIFT, RSHIFT
                Camera.SpeedFactor = 5f;
            else
                Camera.SpeedFactor = 1f;
        }

        private void UpdateCameraMode()
        {
            if (CameraMode != CameraModes.None)
            {
                CaptureMouse = true;
                UpdateCameraState();
            }
            else
            {
                CaptureMouse = false;
                Camera.ForwardInput = 0f;
                Camera.LateralInput = 0f;
            }
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
            this.SuspendLayout();
            this.Name = "ViewportControl";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ViewportControl_MouseDown);
            this.MouseEnter += new System.EventHandler(this.ViewportControl_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ViewportControl_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Viewport_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ViewportControl_MouseUp);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ViewportControl_Paint);
            this.Leave += new System.EventHandler(this.Viewport_Leave);
            this.Resize += new System.EventHandler(this.ViewportControl_Resize);
            this.ResumeLayout(false);
        }
    }
}