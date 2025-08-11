using System;
using System.Windows.Forms;

namespace FC2Editor.Core.Nomad
{
    internal abstract class InputBase : IInputSink
    {
        public virtual void OnInputAcquire() { }
        public virtual void OnInputRelease() { }
        public virtual bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs) => false;
        public virtual bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs) => false;
        public virtual void OnEditorEvent(uint eventType, IntPtr eventPtr) { }
        public virtual void Update(float dt) { }

        protected void AcquireInput()
        {
            Editor.PushInput(this);
            MainForm.Instance.EnableShortcuts = false;
        }

        protected void ReleaseInput()
        {
            MainForm.Instance.EnableShortcuts = true;
            Editor.PopInput(this);
        }
    }
}