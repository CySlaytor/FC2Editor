using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;

namespace FC2Editor.Tools
{
    internal abstract class ToolPaint : ITool, IToolBase, IParameterProvider, IInputSink
    {
        public enum PaintingMode
        {
            None,
            Plus,
            Minus,
            Shortcut
        }

        protected ParamBool m_square = new ParamBool(Localizer.Localize("PARAM_SQUARE_BRUSH"), false);
        protected ParamFloat m_radius = new ParamFloat(Localizer.Localize("PARAM_RADIUS"), 8f, 1f, 128f, 0.5f);
        protected ParamFloat m_hardness = new ParamFloat(Localizer.Localize("PARAM_HARDNESS"), 0.4f, 0f, 1f, 0.01f);
        protected ParamFloat m_opacity = new ParamFloat(Localizer.Localize("PARAM_STRENGTH"), 1f, 0f, 1f, 0.01f);
        protected ParamFloat m_distortion = new ParamFloat(Localizer.Localize("PARAM_DISTORTION"), 0f, 0f, 1f, 0.01f);
        protected ParamBool m_grabMode = new ParamBool(Localizer.Localize("PARAM_GRAB_MODE"), false);

        protected PaintingMode m_painting;
        protected Vec3 m_cursorPos;
        protected PaintBrush m_brush;

        public abstract string GetToolName();
        public abstract Image GetToolImage();
        public abstract string GetContextHelp();

        protected IEnumerable<IParameter> _GetParameters()
        {
            yield return m_square;
            yield return m_radius;
            yield return m_hardness;
            yield return m_distortion;
        }

        public virtual IEnumerable<IParameter> GetParameters()
        {
            return _GetParameters();
        }

        public virtual IParameter GetMainParameter()
        {
            return null;
        }

        protected string GetPaintContextHelp() => Localizer.Localize("HELP_PAINT");
        protected string GetShortcutContextHelp() => Localizer.Localize("HELP_SHORTCUT");

        public virtual void Activate()
        {
            MainForm.Instance.CursorPhysics = false;
        }

        public virtual void Deactivate()
        {
            EndPaint();
            MainForm.Instance.CursorPhysics = true;
        }

        public virtual void OnInputAcquire() { }
        public virtual void OnInputRelease() { }

        private bool EndPaint()
        {
            if (m_painting == PaintingMode.None)
                return false;

            switch (m_painting)
            {
                case PaintingMode.Plus:
                case PaintingMode.Minus:
                    OnEndPaint();
                    break;
            }

            m_cursorPos.Z = TerrainManager.GetHeightAt(m_cursorPos.XY);
            if (Editor.GetScreenPointFromWorldPos(m_cursorPos, out Vec2 screenPoint, true))
            {
                Editor.Viewport.CaptureMousePos = screenPoint;
            }

            Editor.Viewport.CaptureMouse = false;
            Editor.Viewport.CameraEnabled = true;
            m_painting = PaintingMode.None;
            return true;
        }

        public virtual bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            switch (mouseEvent)
            {
                case Editor.MouseEvent.MouseDown:
                    if ((Control.ModifierKeys & Keys.Shift) == 0)
                    {
                        if (Editor.RayCastTerrainFromMouse(out Vec3 hitPos))
                        {
                            m_cursorPos = hitPos;
                            OnBeginPaint();
                        }
                    }
                    else
                    {
                        m_painting = PaintingMode.Shortcut;
                    }

                    if (m_painting != PaintingMode.None)
                    {
                        Editor.Viewport.CaptureMouse = true;
                        Editor.Viewport.CameraEnabled = false;
                    }
                    break;

                case Editor.MouseEvent.MouseUp:
                    EndPaint();
                    break;

                case Editor.MouseEvent.MouseMove:
                    switch (m_painting)
                    {
                        case PaintingMode.None:
                        case PaintingMode.Plus:
                        case PaintingMode.Minus:
                            Editor.RayCastTerrainFromMouse(out m_cursorPos);
                            break;
                    }
                    break;

                case Editor.MouseEvent.MouseMoveDelta:
                    switch (m_painting)
                    {
                        case PaintingMode.Plus:
                        case PaintingMode.Minus:
                            if (!m_grabMode.Value)
                            {
                                Editor.ApplyScreenDeltaToWorldPos(new Vec2((float)mouseEventArgs.X / Editor.Viewport.Width, (float)mouseEventArgs.Y / Editor.Viewport.Height), ref m_cursorPos);
                                m_cursorPos.Z = TerrainManager.GetHeightAt(m_cursorPos.XY);
                            }
                            else
                            {
                                OnPaintGrab(mouseEventArgs.X, mouseEventArgs.Y);
                            }
                            break;
                        case PaintingMode.Shortcut:
                            float delta = (Math.Abs(mouseEventArgs.X) <= Math.Abs(mouseEventArgs.Y)) ? -mouseEventArgs.Y : (float)mouseEventArgs.X;
                            OnShortcutDelta(delta);
                            break;
                    }
                    break;
            }
            return false;
        }

        public bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
        {
            if (keyEvent == Editor.KeyEvent.KeyDown)
            {
                if (keyEventArgs.KeyCode == Keys.Escape && EndPaint())
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void OnEditorEvent(uint eventType, IntPtr eventPtr) { }

        protected virtual void OnBeginPaint()
        {
            MainForm.Instance.EnableShortcuts = false;
            m_painting = ((Control.ModifierKeys & Keys.Control) == 0) ? PaintingMode.Plus : PaintingMode.Minus;
            UndoManager.RecordUndo();
            CreateBrush();
        }

        protected virtual void OnPaint(float dt, Vec2 pos) { }
        protected virtual void OnPaintGrab(float x, float y) { }

        protected virtual void OnEndPaint()
        {
            DestroyBrush();
            UndoManager.CommitUndo();
            MainForm.Instance.EnableShortcuts = true;
        }

        protected virtual void OnShortcutDelta(float delta)
        {
            m_radius.Value += delta * 0.5f;
        }

        public virtual void Update(float dt)
        {
            if (!m_grabMode.Value && (m_painting == PaintingMode.Plus || m_painting == PaintingMode.Minus))
            {
                OnPaint(dt, m_cursorPos.XY);
            }

            float length = (Camera.Position - m_cursorPos).Length;
            if (m_square.Value)
            {
                Render.DrawTerrainSquare(m_cursorPos.XY, m_radius.Value, length * 0.01f, Color.White, 0f);
                Render.DrawTerrainSquare(m_cursorPos.XY, m_radius.Value * m_hardness.Value, length * 0.01f, Color.Yellow, 0.001f);
            }
            else
            {
                Render.DrawTerrainCircle(m_cursorPos.XY, m_radius.Value, length * 0.01f, Color.White, 0f);
                Render.DrawTerrainCircle(m_cursorPos.XY, m_radius.Value * m_hardness.Value, length * 0.01f, Color.Yellow, 0.001f);
            }
            Render.DrawTerrainCircle(m_cursorPos.XY, length * 0.00375f, length * 0.0075f, Color.White, 0f);
        }

        protected void CreateBrush()
        {
            if (m_brush.IsValid)
            {
                DestroyBrush();
            }
            m_brush = PaintBrush.Create(!m_square.Value, m_radius.Value, m_hardness.Value, m_opacity.Value, m_distortion.Value * m_radius.Value * 0.7f);
        }

        protected void DestroyBrush()
        {
            m_brush.Destroy();
        }
    }
}