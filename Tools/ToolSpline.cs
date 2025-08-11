using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal abstract class ToolSpline : ITool, IInputSink
    {
        public enum EditTool
        {
            Select,
            Paint,
            Add,
            Remove
        }

        protected enum State
        {
            None,
            Dragging,
            Moving,
            Drawing,
            Removing
        }

        protected const float penWidth = 0.005f;
        protected const float hitWidth = 0.015f;
        protected const int maxSplinePoints = 100;

        protected ParamEnum<EditTool> m_paramEditTool = new ParamEnum<EditTool>(Localizer.Localize("PARAM_SPLINE_MODE"), EditTool.Select, ParamEnumUIType.Buttons);
        protected bool m_forward;
        protected State m_state;
        protected Vec2 m_dragStart;
        protected SplineController.SelectMode m_dragMode;
        protected float m_drawLastUpdate;
        protected int m_hitPoint = -1;
        protected Vec2 m_hitPos2;
        protected Vec2 m_hitDelta;
        protected Spline m_spline;
        protected SplineController m_splineController;

        protected RectangleF DragRectangle
        {
            get
            {
                Vec2 p1 = m_dragStart;
                Vec2 p2 = Editor.Viewport.NormalizedMousePos;
                Vec2 topLeft = new Vec2(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
                Vec2 bottomRight = new Vec2(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
                return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
            }
        }

        public ToolSpline()
        {
            m_paramEditTool.Names = new string[] {
                Localizer.Localize("PARAM_SPLINE_MODE_SELECT") + " (1)",
                Localizer.Localize("PARAM_SPLINE_MODE_DRAW") + " (2)",
                Localizer.Localize("PARAM_SPLINE_MODE_ADD") + " (3)",
                Localizer.Localize("PARAM_SPLINE_MODE_REMOVE") + " (4)"
            };
            m_paramEditTool.Images = new Image[] { Resources.select, Resources.brush, Resources.add, Resources.remove };
        }

        public abstract string GetToolName();
        public abstract Image GetToolImage();
        public abstract IEnumerable<IParameter> GetParameters();
        public virtual IParameter GetMainParameter() => null;
        public abstract string GetContextHelp();
        public string GetSplineHelp() => Localizer.Localize("HELP_SPLINE");

        protected void SetSpline(Spline spline)
        {
            m_spline = spline;
            m_splineController.SetSpline(m_spline);
            if (m_spline.IsValid)
            {
                m_spline.UpdateSplineHeight();
            }
            m_paramEditTool.Enabled = m_spline.IsValid;
        }

        public virtual void Activate()
        {
            MainForm.Instance.CursorPhysics = false;
            m_splineController = SplineController.Create();
        }

        public void Deactivate()
        {
            EndOperation();
            MainForm.Instance.CursorPhysics = true;
            m_splineController.Dispose();
            m_spline = Spline.Null;
        }

        public void OnInputAcquire() { }
        public void OnInputRelease() { }

        protected bool TestPoints()
        {
            bool flag = m_spline.HitTestPoints(Editor.Viewport.NormalizedMousePos, penWidth, hitWidth, out m_hitPoint, out m_hitPos2);
            if (flag)
            {
                m_hitDelta = m_hitPos2 - Editor.Viewport.NormalizedMousePos;
            }
            return flag;
        }

        protected bool TestSegments()
        {
            if (!Editor.RayCastTerrainFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hitPos))
            {
                return false;
            }
            return m_spline.HitTestSegments(hitPos.XY, 4f, out m_hitPoint, out m_hitPos2);
        }

        protected void StartDrag(SplineController.SelectMode dragMode)
        {
            m_state = State.Dragging;
            m_dragStart = Editor.Viewport.NormalizedMousePos;
            m_dragMode = dragMode;
        }

        protected void MovePointsToMouse(bool add)
        {
            if (m_hitPoint < 0)
                return;

            if (!Editor.RayCastTerrainFromScreenPoint(Editor.Viewport.NormalizedMousePos + m_hitDelta, out Vec3 hitPos))
                return;

            if (add && m_spline.Count < maxSplinePoints)
            {
                if (m_forward)
                {
                    if ((hitPos.XY - m_spline[m_hitPoint - 1]).Length > 15f)
                    {
                        m_spline.InsertPoint(hitPos.XY, m_hitPoint);
                        m_hitPoint++;
                        if (m_hitPoint > 2 && m_spline.OptimizePoint(m_hitPoint - 2))
                        {
                            m_hitPoint--;
                        }
                    }
                }
                else // !m_forward
                {
                    if ((hitPos.XY - m_spline[m_hitPoint + 1]).Length > 15f)
                    {
                        m_spline.InsertPoint(hitPos.XY, m_hitPoint);
                        if (m_hitPoint + 2 < m_spline.Count - 1)
                        {
                            m_spline.OptimizePoint(m_hitPoint + 2);
                        }
                    }
                }
                m_splineController.ClearSelection();
                m_splineController.SetSelected(m_hitPoint, true);
            }
            m_splineController.MoveSelection(hitPos.XY - m_spline[m_hitPoint]);
            m_spline.UpdateSpline();
        }

        protected void RemovePointUnderMouse()
        {
            if (TestPoints())
            {
                m_splineController.ClearSelection();
                m_splineController.SetSelected(m_hitPoint, true);
                m_splineController.DeleteSelection();
                m_spline.RemoveSimilarPoints();
                m_spline.UpdateSpline();
            }
        }

        private bool EndOperation()
        {
            if (!m_spline.IsValid || m_state == State.None)
                return false;

            UndoManager.CommitUndo();
            switch (m_state)
            {
                case State.Dragging:
                    m_splineController.SelectFromScreenRect(DragRectangle, hitWidth, m_dragMode);
                    break;
                case State.Drawing:
                    bool optimized = false;
                    if (m_forward && m_hitPoint >= 1)
                        optimized = m_spline.OptimizePoint(m_hitPoint - 1);
                    else if (!m_forward && m_hitPoint < m_spline.Count - 1)
                        optimized = m_spline.OptimizePoint(m_hitPoint + 1);

                    if (optimized)
                        m_spline.UpdateSpline();
                    break;
            }

            if (m_spline.RemoveSimilarPoints())
            {
                m_spline.UpdateSpline();
                m_splineController.ClearSelection();
            }
            m_spline.FinalizeSpline();
            MainForm.Instance.EnableShortcuts = true;
            m_hitPoint = -1;
            m_state = State.None;
            return true;
        }

        public bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            if (!m_spline.IsValid) return false;

            switch (mouseEvent)
            {
                case Editor.MouseEvent.MouseDown:
                    UndoManager.RecordUndo();
                    switch (m_paramEditTool.Value)
                    {
                        case EditTool.Select:
                            if ((Control.ModifierKeys & Keys.Control) != Keys.None) StartDrag(SplineController.SelectMode.Toggle);
                            else if ((Control.ModifierKeys & Keys.Shift) != Keys.None) StartDrag(SplineController.SelectMode.Add);
                            else if (TestPoints())
                            {
                                if (!m_splineController.IsSelected(m_hitPoint))
                                {
                                    m_splineController.ClearSelection();
                                    m_splineController.SetSelected(m_hitPoint, true);
                                }
                                m_state = State.Moving;
                            }
                            else
                            {
                                StartDrag(SplineController.SelectMode.Replace);
                            }
                            break;

                        case EditTool.Paint:
                        case EditTool.Add:
                            m_hitPoint = -1;
                            m_hitDelta = new Vec2(0f, 0f);
                            if (m_spline.Count < maxSplinePoints && Editor.RayCastTerrainFromMouse(out Vec3 hitPos))
                            {
                                if (m_spline.Count <= 1)
                                {
                                    if (m_spline.Count < 1) m_spline.AddPoint(hitPos.XY);
                                    m_spline.AddPoint(hitPos.XY);
                                    m_hitPoint = 1;
                                }
                                else if (TestPoints())
                                {
                                    if (m_hitPoint == 0) m_spline.InsertPoint(hitPos.XY, 0);
                                    else if (m_hitPoint == m_spline.Count - 1)
                                    {
                                        m_spline.InsertPoint(hitPos.XY, m_hitPoint + 1);
                                        m_hitPoint++;
                                    }
                                }
                                else if (TestSegments())
                                {
                                    m_hitPoint++;
                                    m_spline.InsertPoint(hitPos.XY, m_hitPoint);
                                }

                                if (m_hitPoint != -1)
                                {
                                    m_splineController.ClearSelection();
                                    m_splineController.SetSelected(m_hitPoint, true);
                                    m_spline.UpdateSpline();
                                    if (m_paramEditTool.Value == EditTool.Paint)
                                    {
                                        m_state = State.Drawing;
                                        if (m_hitPoint == 0) m_forward = false;
                                        else if (m_hitPoint == m_spline.Count - 1) m_forward = true;
                                        else m_state = State.Moving;
                                    }
                                    else
                                    {
                                        m_state = State.Moving;
                                    }
                                }
                            }
                            break;

                        case EditTool.Remove:
                            RemovePointUnderMouse();
                            m_state = State.Removing;
                            break;
                    }

                    if (m_state != State.None)
                    {
                        MainForm.Instance.EnableShortcuts = false;
                    }
                    else
                    {
                        UndoManager.CommitUndo();
                    }
                    break;

                case Editor.MouseEvent.MouseMove:
                    switch (m_state)
                    {
                        case State.Moving: MovePointsToMouse(false); break;
                        case State.Drawing: MovePointsToMouse(true); break;
                        case State.Removing: RemovePointUnderMouse(); break;
                    }
                    break;

                case Editor.MouseEvent.MouseUp:
                    EndOperation();
                    break;
            }
            return false;
        }

        public bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
        {
            switch (keyEvent)
            {
                case Editor.KeyEvent.KeyDown:
                    switch (keyEventArgs.KeyCode)
                    {
                        case Keys.D1: m_paramEditTool.Value = EditTool.Select; break;
                        case Keys.D2: m_paramEditTool.Value = EditTool.Paint; break;
                        case Keys.D3: m_paramEditTool.Value = EditTool.Add; break;
                        case Keys.D4: m_paramEditTool.Value = EditTool.Remove; break;
                        case Keys.Escape:
                            if (EndOperation()) return true;
                            break;
                    }
                    break;
                case Editor.KeyEvent.KeyUp:
                    if (keyEventArgs.KeyCode == Keys.Delete)
                    {
                        DeleteSelection();
                        m_spline.RemoveSimilarPoints();
                        return true;
                    }
                    break;
            }
            return false;
        }

        public virtual void OnEditorEvent(uint eventType, IntPtr eventPtr) { }

        public void Update(float dt)
        {
            if (m_spline.IsValid)
            {
                m_spline.Draw(penWidth, m_splineController);
            }

            if (m_state == State.Dragging)
            {
                RectangleF dragRectangle = DragRectangle;
                if (IsDragRectangle(dragRectangle))
                {
                    Render.DrawScreenRectangleOutlined(dragRectangle, 1f, 0.00125f, Color.White);
                }
            }
        }

        protected void DeleteSelection()
        {
            UndoManager.RecordUndo();
            if (m_spline.IsValid)
            {
                m_splineController.DeleteSelection();
                m_spline.UpdateSpline();
            }
            UndoManager.CommitUndo();
        }

        protected bool IsDragRectangle(RectangleF rect)
        {
            return rect.Size.Width > 0.01f && rect.Size.Height > 0.01f;
        }
    }
}