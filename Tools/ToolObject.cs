using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;
using FC2Editor.UI;
using FC2Editor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;

namespace FC2Editor.Tools
{
    internal class ToolObject : ITool, IInputSink, IParameterProviderDynamic, IContextHelpDynamic
    {
        #region Fields
        private SelectMode m_selectMode;
        private MoveMode m_moveMode;
        private RotateMode m_rotateMode;
        private SnapMode m_snapMode;
        private AddMode m_addMode;
        private EditPivotMode m_editPivotMode;
        private Mode m_mode;
        private ParamEnumBase<Mode> m_paramMode = new ParamEnumBase<Mode>(Localizer.Localize("PARAM_MODE"), null, ParamEnumUIType.Buttons);
        private ParamText m_textSelected = new ParamText("");
        private ParamBool m_paramGroupSelection = new ParamBool(Localizer.Localize("PARAM_OBJECT_GROUP_SELECTION"), false);
        private ParamBool m_paramMagicWand = new ParamBool(Localizer.Localize("PARAM_OBJECT_MAGIC_WAND"), false);
        private ParamEnum<AxisType> m_paramAxisType = new ParamEnum<AxisType>(Localizer.Localize("PARAM_AXIS_TYPE"), AxisType.Local, ParamEnumUIType.Buttons);
        private ParamButton m_actionDelete = new ParamButton(Localizer.Localize("PARAM_SELECTION_DELETE"), null);
        private ParamButton m_actionFreeze = new ParamButton(Localizer.Localize("PARAM_SELECTION_FREEZE"), null);
        private ParamButton m_actionUnfreeze = new ParamButton(Localizer.Localize("PARAM_SELECTION_UNFREEZE"), null);
        private EditorObjectSelection m_selection;
        private Gizmo m_gizmo;
        private bool m_gizmoActive;
        private EditorObject m_gizmoObject;
        private bool m_gizmoEnabled;

        public event EventHandler ContextHelpChanged;
        public event EventHandler ParamsChanged;
        #endregion

        #region Nested Classes (Modes and Actions)
        private abstract class Mode : ITool, IInputSink
        {
            protected ToolObject m_context;
            public Mode(ToolObject context) { m_context = context; }
            public virtual string GetToolName() => null;
            public virtual Image GetToolImage() => null;
            public virtual string GetContextHelp() => null;
            public virtual IEnumerable<IParameter> GetParameters() { yield break; }
            public virtual IParameter GetMainParameter() => null;
            public virtual void UpdateParams() { }
            public virtual void Activate() { }
            public virtual void Deactivate() { }
            public virtual void OnInputAcquire() { }
            public virtual void OnInputRelease() { }
            public virtual bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs) => false;
            public virtual bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs) => false;
            public virtual void OnEditorEvent(uint eventType, IntPtr eventPtr) { }
            public virtual void Update(float dt) { }
        }

        private class SelectAction : InputBase
        {
            private ToolObject m_context;
            private Vec2 m_dragStart;

            private RectangleF DragRectangle
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

            public SelectAction(ToolObject context)
            {
                m_context = context;
            }

            public bool Start()
            {
                m_dragStart = Editor.Viewport.NormalizedMousePos;
                AcquireInput();
                return true;
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                if (mouseEvent == Editor.MouseEvent.MouseUp)
                {
                    bool isCtrl = (Control.ModifierKeys & Keys.Control) != Keys.None;
                    bool isShift = (Control.ModifierKeys & Keys.Shift) != Keys.None;

                    EditorObjectSelection newSelection = EditorObjectSelection.Create();
                    if (isCtrl || isShift)
                    {
                        m_context.m_selection.Clone(newSelection, false);
                    }

                    EditorObject gizmoObject = EditorObject.Null;
                    RectangleF dragRectangle = DragRectangle;
                    if (IsDragRectangle(dragRectangle))
                    {
                        EditorObjectSelection selectionInRect = isCtrl ? EditorObjectSelection.Create() : newSelection;
                        ObjectManager.GetObjectsFromScreenRect(selectionInRect, DragRectangle);
                        if (isCtrl)
                        {
                            newSelection.ToggleSelection(selectionInRect);
                            selectionInRect.Dispose();
                        }
                    }
                    else
                    {
                        EditorObject obj = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out _);
                        if (obj.IsValid)
                        {
                            m_context.SelectObject(newSelection, obj);
                            gizmoObject = obj;
                        }
                    }
                    m_context.SetSelection(newSelection, gizmoObject);
                    ReleaseInput();
                }
                return false;
            }

            public override void Update(float dt)
            {
                RectangleF dragRectangle = DragRectangle;
                if (IsDragRectangle(dragRectangle))
                {
                    Render.DrawScreenRectangleOutlined(dragRectangle, 1f, 0.00125f, Color.White);
                }
            }

            private bool IsDragRectangle(RectangleF rect)
            {
                return rect.Width > 0.01f || rect.Height > 0.01f;
            }
        }

        private class MoveAction : InputBase
        {
            private ToolObject m_context;
            private Vec3 m_startPosition;
            private Vec3 m_virtualStart;
            private Vec3 m_pivot;
            private Gizmo m_refGizmo;
            private GizmoHelper m_gizmoHelper = new GizmoHelper();
            private bool m_snap;
            private float m_snapSize;
            private EditorObject m_snapObject;

            public MoveAction(ToolObject context)
            {
                m_context = context;
            }

            public void SetSnap(float snapSize)
            {
                m_snap = true;
                m_snapSize = snapSize;
                m_snapObject = EditorObject.Null;
            }

            public void SetSnap(EditorObject snapObject)
            {
                m_snap = true;
                m_snapObject = snapObject;
            }

            public bool Start(Vec3 pivot)
            {
                m_refGizmo = m_context.m_gizmo;
                m_gizmoHelper.InitVirtualPlane(m_refGizmo.Position, m_refGizmo.Axis, m_refGizmo.Active);
                if (!m_gizmoHelper.GetVirtualPos(out m_virtualStart))
                {
                    return false;
                }
                m_pivot = pivot;
                m_startPosition = pivot;
                AcquireInput();
                m_context.m_selection.SaveState();
                return true;
            }

            public override void OnInputAcquire()
            {
                m_context.m_selection.Center = m_pivot;
                m_context.UpdateSelection();
                UndoManager.RecordUndo();
            }

            public override void OnInputRelease()
            {
                UndoManager.CommitUndo();
                m_context.UpdateSelection(true);
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                switch (mouseEvent)
                {
                    case Editor.MouseEvent.MouseMove:
                        if (m_gizmoHelper.GetVirtualPos(out Vec3 pos))
                        {
                            Vec3 delta = pos - m_virtualStart;
                            if (m_snap)
                            {
                                delta = m_refGizmo.Axis.ConvertFromWorld(delta);
                                if (!m_snapObject.IsValid)
                                {
                                    delta.Snap(m_snapSize);
                                }
                                else if (m_snapObject.IsLoaded)
                                {
                                    delta.Snap(m_snapObject.LocalBounds.Length);
                                }
                                else
                                {
                                    delta = new Vec3(0f, 0f, 0f);
                                }
                                delta = m_refGizmo.Axis.ConvertToWorld(delta);
                            }
                            Vec3 newPosition = m_startPosition + delta;
                            m_context.m_selection.LoadState();
                            m_context.m_selection.MoveTo(newPosition, EditorObjectSelection.MoveMode.MoveNormal);
                            m_context.m_selection.SnapToClosestObjects();
                            m_pivot = newPosition;
                            m_context.UpdateSelection();
                        }
                        break;

                    case Editor.MouseEvent.MouseUp:
                        ReleaseInput();
                        break;
                }
                return false;
            }
        }

        private class MovePhysicsAction : InputBase
        {
            protected ToolObject m_context;
            protected bool m_delayedMove;
            protected Point m_delayedMoveStart;
            protected bool m_localRotate;
            protected Vec3 m_pivot;

            public MovePhysicsAction(ToolObject context)
            {
                m_context = context;
            }

            public virtual bool Start(Vec3 pivot)
            {
                m_pivot = pivot;
                m_delayedMove = true;
                m_delayedMoveStart = Cursor.Position;
                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                {
                    m_localRotate = true;
                    Editor.Viewport.CaptureMouse = true;
                }
                AcquireInput();
                m_context.m_selection.SaveState();
                return true;
            }

            public override void OnInputAcquire()
            {
                Editor.Viewport.CaptureWheel = true;
                m_context.m_selection.Center = m_pivot;
                m_context.UpdateSelection();
                UndoManager.RecordUndo();
            }

            public override void OnInputRelease()
            {
                Editor.Viewport.CaptureWheel = false;
                SetLocalRotate(false);
                UndoManager.CommitUndo();
                m_context.UpdateSelection(true);
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                switch (mouseEvent)
                {
                    case Editor.MouseEvent.MouseMove:
                        if (m_delayedMove)
                        {
                            if (Math.Abs(m_delayedMoveStart.X - Cursor.Position.X) < 2 && Math.Abs(m_delayedMoveStart.Y - Cursor.Position.Y) < 2)
                            {
                                break;
                            }
                            if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                            {
                                EditorObjectSelection newSelection = EditorObjectSelection.Create();
                                m_context.m_selection.Clone(newSelection, true);
                                int gizmoIndex = m_context.m_selection.IndexOf(m_context.m_gizmoObject);
                                m_context.SetSelection(newSelection, (gizmoIndex != -1) ? newSelection[gizmoIndex] : EditorObject.Null);
                            }
                            if (Editor.GetScreenPointFromWorldPos(m_pivot, out Vec2 screenPoint))
                            {
                                Editor.Viewport.NormalizedMousePos = screenPoint;
                            }
                            m_delayedMove = false;
                        }

                        Editor.GetWorldRayFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 raySrc, out Vec3 rayDir);
                        if (Editor.RayCastPhysics(raySrc, rayDir, m_context.m_selection, out Vec3 hitPos, out float _, out Vec3 hitNormal))
                        {
                            m_context.m_selection.Center = m_pivot;
                            m_context.m_selection.LoadState();
                            if (m_context.m_selection.Count == 1)
                            {
                                EditorObject obj = m_context.m_selection[0];
                                if (obj.Entry.AutoOrientation)
                                {
                                    obj.ComputeAutoOrientation(ref hitPos, out Vec3 angles, hitNormal);
                                    obj.Angles = angles;
                                }
                            }
                            m_context.m_selection.MoveTo(hitPos, EditorObjectSelection.MoveMode.MoveNormal);
                            m_context.m_selection.SnapToClosestObjects();
                            m_pivot = hitPos;
                            m_context.UpdateSelection();
                        }
                        break;

                    case Editor.MouseEvent.MouseMoveDelta:
                        m_context.m_selection.LoadState();
                        m_context.m_selection.RotateCenter(0.025f * mouseEventArgs.X, new Vec3(0f, 0f, 1f));
                        m_context.m_selection.SaveState();
                        m_context.m_selection.SnapToClosestObjects();
                        break;

                    case Editor.MouseEvent.MouseWheel:
                        m_context.m_selection.LoadState();
                        Vec3 center = m_context.m_selection.Center;
                        center.Z += ((0.3f * mouseEventArgs.Delta > 0f) ? 1 : -1);
                        m_context.m_selection.MoveTo(center, EditorObjectSelection.MoveMode.MoveNormal);
                        m_context.m_selection.SaveState();
                        break;

                    case Editor.MouseEvent.MouseUp:
                        ReleaseInput();
                        break;
                }
                return false;
            }

            public override bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
            {
                switch (keyEvent)
                {
                    case Editor.KeyEvent.KeyDown:
                        if (keyEventArgs.KeyCode == Keys.ControlKey)
                        {
                            SetLocalRotate(true);
                            return true;
                        }
                        break;
                    case Editor.KeyEvent.KeyUp:
                        if (keyEventArgs.KeyCode == Keys.ControlKey)
                        {
                            SetLocalRotate(false);
                            return true;
                        }
                        break;
                }
                return false;
            }

            private void SetLocalRotate(bool localRotate)
            {
                if (m_localRotate != localRotate)
                {
                    if (localRotate)
                    {
                        m_context.m_selection.LoadState();
                        m_localRotate = true;
                        Editor.Viewport.CaptureMouse = true;
                    }
                    else
                    {
                        m_context.m_selection.SaveState();
                        m_localRotate = false;
                        Editor.Viewport.CaptureMouse = false;
                    }
                }
            }
        }

        private class RotateAction : InputBase
        {
            private ToolObject m_context;
            private Vec3 m_rotationPivot;
            private Vec3 m_rotationAxis;
            private float m_rotationDelta;
            private bool m_snap;
            private float m_snapSize;

            public RotateAction(ToolObject context)
            {
                m_context = context;
            }

            public void SetSnap(float snapSize)
            {
                m_snap = true;
                m_snapSize = snapSize;
            }

            public bool Start(Vec3 rotationPivot, Vec3 rotationAxis)
            {
                m_rotationPivot = rotationPivot;
                m_rotationAxis = rotationAxis;
                AcquireInput();
                m_context.m_selection.SaveState();
                return true;
            }

            public override void OnInputAcquire()
            {
                UndoManager.RecordUndo();
                Editor.Viewport.CaptureMouse = true;
            }

            public override void OnInputRelease()
            {
                Editor.Viewport.CaptureMouse = false;
                UndoManager.CommitUndo();
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                switch (mouseEvent)
                {
                    case Editor.MouseEvent.MouseMoveDelta:
                        Vec3 rotationAxis = m_rotationAxis;
                        Vec2 xz = Camera.Axis.ConvertFromWorld(rotationAxis).XZ;
                        xz.Normalize();
                        xz.Rotate90CW();

                        float num = Vec2.Dot(new Vec2(mouseEventArgs.X, -mouseEventArgs.Y), xz);
                        float angle;

                        if (!m_snap)
                        {
                            angle = num * 0.025f;
                        }
                        else
                        {
                            m_rotationDelta += num;
                            float remainder = (float)Math.IEEERemainder(m_rotationDelta, 25.0);
                            angle = (m_rotationDelta - remainder) / 25f * MathUtils.Deg2Rad(m_snapSize);
                            m_rotationDelta = remainder;
                        }

                        m_context.m_selection.LoadState();
                        switch (m_context.m_paramAxisType.Value)
                        {
                            case AxisType.World:
                                if (m_context.m_selection.Count > 1)
                                    m_context.m_selection.RotateCenter(angle, rotationAxis);
                                else
                                    m_context.m_selection.Rotate(angle, rotationAxis, m_rotationPivot, false);
                                break;
                            case AxisType.Local:
                                m_context.m_selection.Rotate(angle, rotationAxis, m_rotationPivot, false);
                                break;
                        }
                        m_context.m_selection.SaveState();
                        m_context.m_selection.SnapToClosestObjects();
                        m_context.UpdateSelection();
                        break;

                    case Editor.MouseEvent.MouseUp:
                        ReleaseInput();
                        break;
                }
                return false;
            }
        }

        private class SnapAction : InputBase
        {
            private ToolObject m_context;
            private EditorObject m_source;
            private EditorObjectPivot m_sourcePivot;
            private EditorObject m_target;
            private EditorObjectPivot m_targetPivot;

            public bool PreserveOrientation { get; set; }
            public float AngleSnap { get; set; }

            public SnapAction(ToolObject context)
            {
                m_context = context;
            }

            public bool Start()
            {
                m_source = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hitPos);
                if (!m_source.IsValid || !m_source.GetClosestPivot(hitPos, out m_sourcePivot))
                {
                    return false;
                }

                if (!m_context.m_selection.Contains(m_source))
                {
                    EditorObjectSelection newSelection = EditorObjectSelection.Create();
                    if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                    {
                        m_context.m_selection.Clone(newSelection, false);
                    }
                    m_context.SelectObject(newSelection, m_source);
                    m_context.SetSelection(newSelection, m_source);
                }

                AcquireInput();
                return true;
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                switch (mouseEvent)
                {
                    case Editor.MouseEvent.MouseMove:
                        m_target = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hitPos, false, m_source);
                        if (m_target.IsValid)
                        {
                            m_target.GetClosestPivot(hitPos, out m_targetPivot);
                        }
                        break;
                    case Editor.MouseEvent.MouseUp:
                        EditorObject targetObj = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hit, false, m_source);
                        if (targetObj.IsValid && targetObj.GetClosestPivot(hit, out m_targetPivot))
                        {
                            UndoManager.RecordUndo();
                            m_context.m_selection.Center = m_sourcePivot.position;
                            m_context.m_selection.SnapToPivot(m_sourcePivot, m_targetPivot, PreserveOrientation, AngleSnap);
                            UndoManager.CommitUndo();
                        }
                        ReleaseInput();
                        break;
                }
                return false;
            }

            public override void Update(float dt)
            {
                if (m_source.IsValid && Editor.GetScreenPointFromWorldPos(m_sourcePivot.position, out Vec2 sourceScreenPos))
                {
                    Render.DrawScreenCircleOutlined(sourceScreenPos, 0f, 0.005f, 0.002f, Color.Red);
                }
                if (m_target.IsValid && m_targetPivot != null && Editor.GetScreenPointFromWorldPos(m_targetPivot.position, out Vec2 targetScreenPos))
                {
                    Render.DrawScreenCircleOutlined(targetScreenPos, 0f, 0.005f, 0.002f, Color.Yellow);
                }
            }
        }

        private class SelectMode : Mode
        {
            public SelectMode(ToolObject context) : base(context) { }
            public override string GetToolName() => "Select objects";
            public override Image GetToolImage() => Resources.Tool_Select;
            public override string GetContextHelp() => Localizer.Localize("HELP_TOOL_SELECTOBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_DELETE_OBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_GROUP_SELECTION") + "\r\n\r\n" + Localizer.Localize("HELP_NEIGHBORHOOD_SELECTION");
            public override IEnumerable<IParameter> GetParameters()
            {
                yield return m_context.m_textSelected;
                yield return m_context.m_paramGroupSelection;
                yield return m_context.m_paramMagicWand;
                yield return m_context.m_actionDelete;
                yield return m_context.m_actionFreeze;
                yield return m_context.m_actionUnfreeze;
            }
            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                if (mouseEvent == Editor.MouseEvent.MouseDown)
                {
                    new SelectAction(m_context).Start();
                }
                return false;
            }
        }

        private class MoveMode : Mode
        {
            private ParamVector m_paramPosition = new ParamVector(Localizer.Localize("PARAM_POSITION"), default(Vec3), ParamVectorUIType.Position);
            private ParamBool m_paramSnap = new ParamBool(Localizer.Localize("PARAM_USE_SNAP_GRID"), false);
            private ParamFloat m_paramSnapSize = new ParamFloat(Localizer.Localize("PARAM_SNAP_GRID_SIZE"), 1f, 1f, 16f, 0.25f);
            private ParamBool m_paramSnapObjectSize = new ParamBool(Localizer.Localize("PARAM_SNAP_OBJECT_SIZE"), false);
            private ParamBool m_paramUseGizmos = new ParamBool(Localizer.Localize("PARAM_USE_GIZMO"), true);
            private ParamBool m_paramGrabAnchor = new ParamBool(Localizer.Localize("PARAM_GRAB_ANCHOR"), false);
            private ParamButton m_actionDropToPhysics = new ParamButton(Localizer.Localize("PARAM_SELECTION_DROP"), null);
            private Keys m_keyStart;
            private bool m_keyMoving;

            public MoveMode(ToolObject context) : base(context)
            {
                m_paramSnapSize.Enabled = false;
                m_paramSnapObjectSize.Enabled = false;
                m_paramPosition.ValueChanged += position_ValueChanged;
                m_paramSnap.ValueChanged += snap_ValueChanged;
                m_paramSnapObjectSize.ValueChanged += snapObjectSize_ValueChanged;
                m_paramUseGizmos.ValueChanged += useGizmos_ValueChanged;
                m_actionDropToPhysics.Callback = action_DropToPhysics;
            }

            public override string GetToolName() => "Move objects";
            public override Image GetToolImage() => Resources.Tool_Move;
            public override string GetContextHelp() => Localizer.Localize("HELP_TOOL_MOVEOBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_DELETE_OBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_GROUP_SELECTION") + "\r\n\r\n" + Localizer.Localize("HELP_NEIGHBORHOOD_SELECTION") + "\r\n\r\n" + Localizer.Localize("HELP_AXIS_TYPE");

            public override IEnumerable<IParameter> GetParameters()
            {
                yield return m_context.m_textSelected;
                yield return m_context.m_paramGroupSelection;
                yield return m_context.m_paramMagicWand;
                yield return m_context.m_paramAxisType;
                yield return m_paramPosition;
                yield return m_paramSnap;
                yield return m_paramSnapSize;
                yield return m_paramSnapObjectSize;
                yield return m_paramUseGizmos;
                yield return m_paramGrabAnchor;
                yield return m_actionDropToPhysics;
                yield return m_context.m_actionDelete;
            }

            public override void UpdateParams()
            {
                if (m_context.m_paramAxisType.Value == AxisType.World && m_context.m_selection.Count > 0)
                {
                    m_paramPosition.Value = m_context.m_selection.Center;
                }
                else
                {
                    m_paramPosition.Value = default(Vec3);
                }
                m_actionDropToPhysics.Enabled = m_context.m_selection.Count > 0;
            }

            private void position_ValueChanged(object sender, EventArgs e)
            {
                if (m_context.m_selection.Count == 0) return;

                UndoManager.RecordUndo();
                m_context.m_selection.ComputeCenter();
                switch (m_context.m_paramAxisType.Value)
                {
                    case AxisType.World:
                        m_context.m_selection.MoveTo(m_paramPosition.Value, EditorObjectSelection.MoveMode.MoveNormal);
                        break;
                    case AxisType.Local:
                        if (m_context.m_selection.Count == 1)
                        {
                            m_context.m_selection.MoveTo(m_context.m_selection.Center + CoordinateSystem.FromAngles(m_context.m_selection[0].Angles).ConvertToWorld(m_paramPosition.Value), EditorObjectSelection.MoveMode.MoveNormal);
                        }
                        else
                        {
                            m_context.m_selection.MoveTo(m_context.m_selection.Center + m_paramPosition.Value, EditorObjectSelection.MoveMode.MoveNormal);
                        }
                        m_paramPosition.Value = default(Vec3);
                        break;
                }
                UndoManager.CommitUndo();
                m_context.UpdateSelection();
            }

            private void snap_ValueChanged(object sender, EventArgs e)
            {
                m_paramSnapSize.Enabled = m_paramSnap.Value && !m_paramSnapObjectSize.Value;
                m_paramSnapObjectSize.Enabled = m_paramSnap.Value;
            }

            private void snapObjectSize_ValueChanged(object sender, EventArgs e)
            {
                m_paramSnapSize.Enabled = m_paramSnap.Value && !m_paramSnapObjectSize.Value;
            }

            private void useGizmos_ValueChanged(object sender, EventArgs e)
            {
                m_context.EnableGizmo(m_paramUseGizmos.Value);
            }

            private void action_DropToPhysics()
            {
                UndoManager.RecordUndo();
                m_context.m_selection.DropToGround(true, m_context.m_paramGroupSelection.Value);
                UndoManager.CommitUndo();
                m_context.UpdateSelection();
            }

            public override void Activate() => m_context.EnableGizmo(m_paramUseGizmos.Value);
            public override void Deactivate() => m_context.EnableGizmo(false);

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                if (mouseEvent == Editor.MouseEvent.MouseDown)
                {
                    if (m_context.m_gizmoActive)
                    {
                        UndoManager.RecordUndo();
                        if ((Control.ModifierKeys & Keys.Control) == 0)
                        {
                            if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
                            {
                                EditorObjectSelection newSelection = EditorObjectSelection.Create();
                                m_context.m_selection.Clone(newSelection, true);
                                int gizmoIndex = m_context.m_selection.IndexOf(m_context.m_gizmoObject);
                                m_context.SetSelection(newSelection, (gizmoIndex != -1) ? newSelection[gizmoIndex] : EditorObject.Null);
                            }
                            MoveAction moveAction = new MoveAction(m_context);
                            if (m_paramSnap.Value)
                            {
                                if (m_paramSnapObjectSize.Value && m_context.m_gizmoObject.IsValid)
                                    moveAction.SetSnap(m_context.m_gizmoObject);
                                else
                                    moveAction.SetSnap(m_paramSnapSize.Value);
                            }
                            moveAction.Start(m_context.m_gizmo.Position);
                        }
                        else
                        {
                            RotateAction rotateAction = new RotateAction(m_context);
                            Vec3 rotationAxis = default(Vec3);
                            switch (m_context.m_gizmo.Active)
                            {
                                case Axis.X: rotationAxis = m_context.m_gizmo.Axis.axisX; break;
                                case Axis.Y: rotationAxis = m_context.m_gizmo.Axis.axisY; break;
                                case Axis.Z: rotationAxis = m_context.m_gizmo.Axis.axisZ; break;
                            }
                            rotateAction.Start(m_context.m_gizmo.Position, rotationAxis);
                        }
                    }
                    else
                    {
                        UndoManager.RecordUndo();
                        bool handled = false;
                        EditorObject obj = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hitPos);
                        if (obj.IsValid)
                        {
                            if (!m_context.m_selection.Contains(obj))
                            {
                                EditorObjectSelection newSelection = EditorObjectSelection.Create();
                                if ((Control.ModifierKeys & Keys.Control) != Keys.None || (Control.ModifierKeys & Keys.Shift) != Keys.None)
                                {
                                    m_context.m_selection.Clone(newSelection, false);
                                }
                                m_context.SelectObject(newSelection, obj);
                                m_context.SetSelection(newSelection, obj);
                            }
                            else
                            {
                                m_context.SetupGizmo(obj);
                            }

                            Vec3 pivot = obj.Position;
                            if (m_paramGrabAnchor.Value && obj.GetClosestPivot(hitPos, out EditorObjectPivot objPivot, (obj.Position - hitPos).Length * 1.1f))
                            {
                                pivot = objPivot.position;
                            }
                            new MovePhysicsAction(m_context).Start(pivot);
                            handled = true;
                        }
                        if (!handled)
                        {
                            new SelectAction(m_context).Start();
                        }
                    }
                }
                return false;
            }

            public override bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
            {
                switch (keyEvent)
                {
                    case Editor.KeyEvent.KeyDown:
                        switch (keyEventArgs.KeyCode)
                        {
                            case Keys.Left:
                            case Keys.Up:
                            case Keys.Right:
                            case Keys.Down:
                                m_keyStart = keyEventArgs.KeyCode;
                                if (!m_keyMoving)
                                {
                                    m_keyMoving = true;
                                    UndoManager.RecordUndo();
                                }
                                break;
                        }
                        break;

                    case Editor.KeyEvent.KeyChar:
                        if (m_keyMoving && keyEventArgs.KeyCode == m_keyStart && m_context.m_gizmo.IsValid)
                        {
                            Vec3 moveDelta = default(Vec3);
                            Vec3 angleDelta = default(Vec3);

                            switch (m_keyStart)
                            {
                                case Keys.Left: if (!keyEventArgs.Control) moveDelta.X = -1f; else angleDelta.Z = -1f; break;
                                case Keys.Right: if (!keyEventArgs.Control) moveDelta.X = 1f; else angleDelta.Z = 1f; break;
                                case Keys.Up: if (!keyEventArgs.Control) moveDelta.Y = 1f; else moveDelta.Z = 1f; break;
                                case Keys.Down: if (!keyEventArgs.Control) moveDelta.Y = -1f; else moveDelta.Z = -1f; break;
                            }

                            CoordinateSystem camAxis = Camera.Axis;
                            CoordinateSystem gizmoAxis = m_context.m_gizmo.Axis;
                            float dotX = Vec3.Dot(gizmoAxis.axisX, camAxis.axisX);
                            float dotY = Vec3.Dot(gizmoAxis.axisY, camAxis.axisX);
                            Vec3 finalDelta = (Math.Abs(dotX) > Math.Abs(dotY)) ?
                                (gizmoAxis.axisX * moveDelta.X * Math.Sign(dotX) + gizmoAxis.axisY * moveDelta.Y * Math.Sign(Vec3.Dot(gizmoAxis.axisY, camAxis.axisZ))) :
                                (gizmoAxis.axisY * moveDelta.X * Math.Sign(dotY) + gizmoAxis.axisX * moveDelta.Y * Math.Sign(Vec3.Dot(gizmoAxis.axisX, camAxis.axisZ)));

                            finalDelta += gizmoAxis.axisZ * moveDelta.Z;

                            if (keyEventArgs.Shift)
                            {
                                finalDelta *= 0.0025f;
                                angleDelta *= MathUtils.Deg2Rad(0.25f);
                            }
                            else
                            {
                                finalDelta *= 0.01f;
                                angleDelta *= MathUtils.Deg2Rad(1f);
                            }

                            m_context.m_selection.Center = m_context.m_gizmo.Position;
                            m_context.m_selection.MoveTo(m_context.m_gizmo.Position + finalDelta, EditorObjectSelection.MoveMode.MoveNormal);
                            m_context.m_selection.Rotate(angleDelta, gizmoAxis.ToAngles(), m_context.m_gizmo.Position, false);
                        }
                        break;
                    case Editor.KeyEvent.KeyUp:
                        if (m_keyMoving && keyEventArgs.KeyCode == m_keyStart)
                        {
                            UndoManager.CommitUndo();
                            m_keyMoving = false;
                        }
                        break;
                }
                return false;
            }
        }

        private class RotateMode : Mode
        {
            private ParamVector m_paramRotation = new ParamVector(Localizer.Localize("PARAM_ROTATION"), default(Vec3), ParamVectorUIType.Angles);
            private ParamBool m_paramSnap = new ParamBool(Localizer.Localize("PARAM_USE_SNAP_ANGLES"), false);
            private ParamEnumAngles m_paramSnapSize = new ParamEnumAngles(Localizer.Localize("PARAM_SNAP_ANGLE"), 90f, ParamEnumUIType.Buttons);
            private ParamButton m_actionResetAngles = new ParamButton(Localizer.Localize("PARAM_RESET_TILT"), null);
            private Keys m_keyStart;
            private bool m_keyMoving;

            public RotateMode(ToolObject context) : base(context)
            {
                m_paramRotation.ValueChanged += rotation_ValueChanged;
                m_paramSnap.ValueChanged += snap_ValueChanged;
                m_actionResetAngles.Callback = action_ResetAngles;
                m_paramSnapSize.Enabled = false;
            }

            public override string GetToolName() => "Rotate objects";
            public override Image GetToolImage() => Resources.Tool_Rotate;
            public override string GetContextHelp() => Localizer.Localize("HELP_TOOL_ROTATEOBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_DELETE_OBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_GROUP_SELECTION") + "\r\n\r\n" + Localizer.Localize("HELP_NEIGHBORHOOD_SELECTION") + "\r\n\r\n" + Localizer.Localize("HELP_AXIS_TYPE");

            public override IEnumerable<IParameter> GetParameters()
            {
                yield return m_context.m_textSelected;
                yield return m_context.m_paramGroupSelection;
                yield return m_context.m_paramMagicWand;
                yield return m_context.m_paramAxisType;
                yield return m_paramRotation;
                yield return m_paramSnap;
                yield return m_paramSnapSize;
                yield return m_actionResetAngles;
                yield return m_context.m_actionDelete;
            }

            public override void UpdateParams()
            {
                if (m_context.m_paramAxisType.Value == AxisType.World && m_context.m_selection.Count == 1)
                {
                    m_paramRotation.Value = m_context.m_selection[0].Angles;
                }
                else
                {
                    m_paramRotation.Value = default(Vec3);
                }
                m_actionResetAngles.Enabled = m_context.m_selection.Count > 0;
            }

            private void snap_ValueChanged(object sender, EventArgs e)
            {
                m_paramSnapSize.Enabled = m_paramSnap.Value;
            }

            private void rotation_ValueChanged(object sender, EventArgs e)
            {
                if (m_context.m_selection.Count == 0) return;

                UndoManager.RecordUndo();
                if (!m_context.m_paramGroupSelection.Value)
                {
                    switch (m_context.m_paramAxisType.Value)
                    {
                        case AxisType.World:
                            m_context.m_selection.SetAngles(m_paramRotation.Value);
                            break;
                        case AxisType.Local:
                            if (m_context.m_selection.Count == 1)
                            {
                                m_context.m_selection.Rotate(m_paramRotation.Value, m_context.m_selection[0].Angles, m_context.m_selection[0].Position, false);
                            }
                            else
                            {
                                m_context.m_selection.RotateLocal(m_paramRotation.Value);
                            }
                            m_paramRotation.Value = default(Vec3);
                            break;
                    }
                }
                else
                {
                    m_context.m_selection.ComputeCenter();
                    m_context.m_selection.Rotate(m_paramRotation.Value, new Vec3(0f, 0f, 0f), m_context.m_selection.Center, false);
                }
                UndoManager.CommitUndo();
                m_context.UpdateSelection();
            }

            private void action_ResetAngles()
            {
                for (int i = 0; i < m_context.m_selection.Count; i++)
                {
                    EditorObject editorObject = m_context.m_selection[i];
                    Vec3 angles = editorObject.Angles;
                    angles.X = 0f;
                    angles.Y = 0f;
                    editorObject.Angles = angles;
                }
                m_context.UpdateSelection();
            }


            public override void Activate() => m_context.EnableGizmo(true);
            public override void Deactivate() => m_context.EnableGizmo(false);

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                if (mouseEvent == Editor.MouseEvent.MouseDown)
                {
                    if (m_context.m_gizmoActive)
                    {
                        Vec3 pivot = m_context.m_gizmo.Position;
                        Vec3 rotationAxis = default(Vec3);
                        switch (m_context.m_gizmo.Active)
                        {
                            case Axis.X: rotationAxis = m_context.m_gizmo.Axis.axisX; break;
                            case Axis.Y: rotationAxis = m_context.m_gizmo.Axis.axisY; break;
                            case Axis.Z: rotationAxis = m_context.m_gizmo.Axis.axisZ; break;
                        }
                        RotateAction rotateAction = new RotateAction(m_context);
                        if (m_paramSnap.Value)
                        {
                            rotateAction.SetSnap(m_paramSnapSize.Value);
                        }
                        rotateAction.Start(pivot, rotationAxis);
                    }
                    else
                    {
                        EditorObject obj = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hitPos);
                        if (obj.IsValid)
                        {
                            if (!m_context.m_selection.Contains(obj))
                            {
                                EditorObjectSelection newSelection = EditorObjectSelection.Create();
                                if ((Control.ModifierKeys & Keys.Control) != Keys.None)
                                {
                                    m_context.m_selection.Clone(newSelection, false);
                                }
                                m_context.SelectObject(newSelection, obj);
                                m_context.SetSelection(newSelection, obj);
                            }
                            else
                            {
                                m_context.SetupGizmo(obj);
                            }
                            Vec3 pivot = obj.Position;
                            if (obj.GetClosestPivot(hitPos, out EditorObjectPivot objPivot))
                            {
                                pivot = objPivot.position;
                            }
                            Vec3 rotationAxis = new Vec3(0f, 0f, 1f);
                            RotateAction rotateAction = new RotateAction(m_context);
                            if (m_paramSnap.Value)
                            {
                                rotateAction.SetSnap(m_paramSnapSize.Value);
                            }
                            rotateAction.Start(pivot, rotationAxis);
                        }
                        else
                        {
                            new SelectAction(m_context).Start();
                        }
                    }
                }
                return false;
            }

            public override bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
            {
                switch (keyEvent)
                {
                    case Editor.KeyEvent.KeyDown:
                        switch (keyEventArgs.KeyCode)
                        {
                            case Keys.Left:
                            case Keys.Up:
                            case Keys.Right:
                            case Keys.Down:
                                m_keyStart = keyEventArgs.KeyCode;
                                if (!m_keyMoving)
                                {
                                    m_keyMoving = true;
                                    UndoManager.RecordUndo();
                                }
                                break;
                        }
                        break;

                    case Editor.KeyEvent.KeyChar:
                        if (m_keyMoving && keyEventArgs.KeyCode == m_keyStart && m_context.m_gizmo.IsValid)
                        {
                            Vec3 angles = default(Vec3);
                            switch (m_keyStart)
                            {
                                case Keys.Left: if (!keyEventArgs.Control) angles.Z = -1f; else angles.Y = -1f; break;
                                case Keys.Right: if (!keyEventArgs.Control) angles.Z = 1f; else angles.Y = 1f; break;
                                case Keys.Up: if (!keyEventArgs.Control) angles.Z = -1f; else angles.X = -1f; break;
                                case Keys.Down: if (!keyEventArgs.Control) angles.Z = 1f; else angles.X = 1f; break;
                            }

                            if (keyEventArgs.Shift)
                                angles *= MathUtils.Deg2Rad(0.25f);
                            else
                                angles *= MathUtils.Deg2Rad(1f);

                            m_context.m_selection.Rotate(angles, m_context.m_gizmo.Axis.ToAngles(), m_context.m_gizmo.Position, false);
                        }
                        break;
                    case Editor.KeyEvent.KeyUp:
                        if (m_keyMoving && keyEventArgs.KeyCode == m_keyStart)
                        {
                            UndoManager.CommitUndo();
                            m_keyMoving = false;
                        }
                        break;
                }
                return false;
            }
        }

        private class SnapMode : Mode
        {
            private ParamBool m_paramUseSnapAngle = new ParamBool(Localizer.Localize("PARAM_USE_SNAP_ANGLES"), false);
            private ParamEnumAngles m_paramSnapAngle = new ParamEnumAngles(Localizer.Localize("PARAM_SNAP_ANGLE"), 90f, ParamEnumUIType.Buttons);
            private ParamEnum<RotationDirection> m_paramAngleDir = new ParamEnum<RotationDirection>(Localizer.Localize("PARAM_ANGLE_DIRECTION"), RotationDirection.CCW, ParamEnumUIType.Buttons);
            private ParamBool m_paramPreserveOrientation = new ParamBool(Localizer.Localize("PARAM_PRESERVE_ORIENTATION"), false);

            public SnapMode(ToolObject context) : base(context)
            {
                m_paramUseSnapAngle.ValueChanged += paramUseSnapAngle_ValueChanged;
                m_paramPreserveOrientation.ValueChanged += paramPreserveOrientation_ValueChanged;
                paramPreserveOrientation_ValueChanged(null, null); // Initial setup
            }

            public override string GetToolName() => "Snap objects";
            public override Image GetToolImage() => Resources.Tool_Link;
            public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_SNAPOBJECT") + "\r\n\r\n" + Localizer.Localize("HELP_TOOL_SNAPOBJECT");

            public override IEnumerable<IParameter> GetParameters()
            {
                yield return m_context.m_textSelected;
                yield return m_paramUseSnapAngle;
                yield return m_paramSnapAngle;
                yield return m_paramAngleDir;
                yield return m_paramPreserveOrientation;
                yield return m_context.m_actionDelete;
            }

            private void paramPreserveOrientation_ValueChanged(object sender, EventArgs e)
            {
                m_paramUseSnapAngle.Enabled = !m_paramPreserveOrientation.Value;
                paramUseSnapAngle_ValueChanged(sender, e);
            }

            private void paramUseSnapAngle_ValueChanged(object sender, EventArgs e)
            {
                bool enabled = !m_paramPreserveOrientation.Value && m_paramUseSnapAngle.Value;
                m_paramSnapAngle.Enabled = enabled;
                m_paramAngleDir.Enabled = enabled;
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                if (mouseEvent == Editor.MouseEvent.MouseDown)
                {
                    SnapAction snapAction = new SnapAction(m_context)
                    {
                        PreserveOrientation = m_paramPreserveOrientation.Value
                    };
                    if (m_paramUseSnapAngle.Value)
                    {
                        snapAction.AngleSnap = MathUtils.Deg2Rad((m_paramAngleDir.Value == RotationDirection.CCW) ? m_paramSnapAngle.Value : -m_paramSnapAngle.Value);
                    }
                    if (!snapAction.Start())
                    {
                        new SelectAction(m_context).Start();
                    }
                }
                return false;
            }
        }

        private class AddMode : Mode
        {
            private class ParamInventoryObject : Parameter
            {
                public ObjectInventory.Entry Value { get; set; }
                public event EventHandler ValueChanged;

                public ParamInventoryObject(string display) : base(display) { }

                public void UpdateSize(AABB size)
                {
                    foreach (Control key in m_uiControls.Keys)
                    {
                        if (key is ParamObjectInventoryTree tree)
                        {
                            tree.ObjectSize = size;
                        }
                    }
                }

                protected override Control CreateUIControl()
                {
                    ParamObjectInventoryTree tree = new ParamObjectInventoryTree();
                    tree.ValueChanged += delegate (object sender, EventArgs e)
                    {
                        OnValueChanged(((ParamObjectInventoryTree)sender).Value);
                    };
                    UpdateUIControl(tree);
                    return tree;
                }

                protected override void UpdateUIControl(Control control)
                {
                    ((ParamObjectInventoryTree)control).Value = Value;
                }

                protected void OnValueChanged(ObjectInventory.Entry value)
                {
                    Value = value;
                    this.ValueChanged?.Invoke(this, new EventArgs());
                }
            }

            private bool m_wasInOcclusionFolder;
            private ParamInventoryObject m_paramObject = new ParamInventoryObject(null);
            private ParamBool m_paramBatchAdd = new ParamBool(Localizer.Localize("PARAM_BATCH_ADD"), false);
            private ParamBool m_paramPreview = new ParamBool(Localizer.Localize("PARAM_OBJECT_PREVIEW"), false);
            private bool m_localRotate;
            private EditorObject m_newObject;
            private bool m_newObjectPending;
            private Vec3 m_prevAngles;

            private bool IsInOcclusionFolder
            {
                get
                {
                    ObjectInventory.Entry value = m_paramObject.Value;
                    if (value == null || !value.IsValid)
                        return false;

                    ObjectInventory.Entry parent = (ObjectInventory.Entry)value.Parent;
                    if (value.Id == 0xC3D5E5CC) // Occlusion
                        return true;

                    return parent != null && parent.IsValid && parent.Id == 0xC3D5E5CC;
                }
            }

            public AddMode(ToolObject context) : base(context)
            {
                m_paramObject.ValueChanged += paramObject_ValueChanged;
                m_paramPreview.ValueChanged += paramPreview_ValueChanged;
            }

            public override string GetToolName() => "Add object";
            public override Image GetToolImage() => Resources.Object_Add;
            public override string GetContextHelp() => (IsInOcclusionFolder ? (Localizer.LocalizeCommon("HELP_OBJECT_OCCLUSION") + "\r\n\r\n") : "") + Localizer.Localize("HELP_TOOL_ADDOBJECT");
            public override IEnumerable<IParameter> GetParameters()
            {
                yield return m_paramObject;
                yield return m_paramBatchAdd;
                yield return m_paramPreview;
            }
            public override IParameter GetMainParameter() => m_paramObject;
            private void paramObject_ValueChanged(object sender, EventArgs e) => SetNewObject();
            private void paramPreview_ValueChanged(object sender, EventArgs e) => UpdatePreview();

            public override void Activate()
            {
                m_prevAngles = default(Vec3);
                ClearObjectParam();
                SetNewObject();
                UpdatePreview();
            }

            public override void Deactivate()
            {
                ClearPreview();
                ClearNewObject();
                ObjectRenderer.Clear();
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                switch (mouseEvent)
                {
                    case Editor.MouseEvent.MouseDown:
                        if (!m_paramPreview.Value && (Control.ModifierKeys & Keys.Control) != Keys.None)
                        {
                            m_localRotate = true;
                            Editor.Viewport.CaptureMouse = true;
                        }
                        break;
                    case Editor.MouseEvent.MouseMove:
                        if (!m_paramPreview.Value && m_newObject.IsValid)
                        {
                            Editor.GetWorldRayFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 raySrc, out Vec3 rayDir);
                            if (Editor.RayCastPhysics(raySrc, rayDir, m_newObject, out Vec3 hitPos, out float _, out Vec3 hitNormal))
                            {
                                m_context.m_selection.LoadState();
                                if (m_newObject.Entry.AutoOrientation)
                                {
                                    m_newObject.ComputeAutoOrientation(ref hitPos, out Vec3 angles, hitNormal);
                                    m_newObject.Angles = angles;
                                }
                                m_newObject.Position = hitPos;
                                m_context.m_selection.SaveState();
                                m_context.m_selection.SnapToClosestObjects();
                                m_newObject.Visible = true;
                            }
                            else
                            {
                                m_newObject.Visible = false;
                            }
                        }
                        break;
                    case Editor.MouseEvent.MouseMoveDelta:
                        if (!m_paramPreview.Value && m_newObject.IsValid)
                        {
                            m_context.m_selection.LoadState();
                            m_context.m_selection.Rotate(0.025f * mouseEventArgs.X, new Vec3(0f, 0f, 1f), m_newObject.Position, false);
                            m_context.m_selection.SaveState();
                            m_context.m_selection.SnapToClosestObjects();
                        }
                        break;
                    case Editor.MouseEvent.MouseUp:
                        if (m_paramPreview.Value) break;

                        if (m_localRotate)
                        {
                            m_localRotate = false;
                            Editor.Viewport.CaptureMouse = false;
                        }
                        else if (m_newObject.Visible)
                        {
                            UndoManager.RecordUndo();
                            EditorObjectSelection.Create().AddObject(m_newObject.Clone());
                            UndoManager.CommitUndo();
                            if (!m_paramBatchAdd.Value)
                            {
                                ClearObjectParam();
                            }
                        }
                        break;
                }
                return false;
            }

            public override bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
            {
                if (keyEvent == Editor.KeyEvent.KeyUp && keyEventArgs.KeyCode == Keys.Escape && m_newObject.IsValid)
                {
                    ClearObjectParam();
                    return true;
                }
                return false;
            }

            public override void Update(float dt)
            {
                if (m_newObjectPending && m_newObject.IsLoaded)
                {
                    m_paramObject.UpdateSize(m_newObject.LocalBounds);
                    m_newObjectPending = false;
                }
                UpdateNewObject();
            }

            private void ClearNewObject()
            {
                if (m_newObject.IsValid)
                {
                    m_context.ClearSelection();
                    ObjectViewer.Object = EditorObject.Null;
                    if (!m_newObject.Entry.AutoOrientation)
                    {
                        m_prevAngles = m_newObject.Angles;
                    }
                    m_newObject.Release();
                    m_newObject.Destroy();
                    m_newObjectPending = false;
                }
            }

            private void SetNewObject()
            {
                bool inOcclusion = IsInOcclusionFolder;
                if (inOcclusion != m_wasInOcclusionFolder)
                {
                    m_context.UpdateContextHelp();
                    m_wasInOcclusionFolder = inOcclusion;
                }
                ClearNewObject();
                if (m_paramObject.Value == null || m_paramObject.Value.IsDirectory)
                {
                    return;
                }

                m_newObject = EditorObject.CreateFromEntry(m_paramObject.Value, false);
                if (m_newObject.IsValid)
                {
                    m_context.ClearSelection();
                    m_newObject.Acquire();
                    m_newObjectPending = true;
                    m_newObject.Position = new Vec3(1f, 1f, 1f);
                    m_newObject.Visible = false;
                    m_newObject.Angles = m_prevAngles;
                    EditorObjectSelection selection = EditorObjectSelection.Create();
                    selection.AddObject(m_newObject);
                    m_context.SetSelection(selection, m_newObject);
                    m_context.m_selection.SaveState();
                    UpdateNewObject();
                    if (ObjectViewer.Active)
                    {
                        UpdatePreview();
                    }
                }
            }

            private void UpdateNewObject()
            {
                if (!m_newObject.IsValid) return;

                if (m_paramPreview.Value)
                {
                    m_newObject.Visible = true;
                    m_newObject.HighlightState = false;
                }
                else
                {
                    m_newObject.Visible = Editor.Viewport.MouseOver;
                    m_newObject.HighlightState = true;
                }
            }

            private void ClearObjectParam()
            {
                if (m_paramObject.Value != null && !m_paramObject.Value.IsDirectory)
                {
                    m_paramObject.Value = (ObjectInventory.Entry)m_paramObject.Value.Parent;
                    ClearNewObject();
                }
            }

            private void UpdatePreview()
            {
                if (m_paramPreview.Value)
                {
                    SetPreview();
                }
                else
                {
                    ClearPreview();
                }
            }
            private void ClearPreview()
            {
                ObjectViewer.Active = false;
                ObjectViewer.Object = EditorObject.Null;
                UpdateNewObject();
            }
            private void SetPreview()
            {
                ObjectViewer.Active = true;
                ObjectViewer.Object = m_newObject;
                UpdateNewObject();
            }
        }

        private class EditPivotMode : Mode
        {
            private ParamButton m_actionSaveDatabase = new ParamButton("Save object database", null);
            private ParamButton m_actionSetHeight = new ParamButton("Set height", null);
            private ParamButton m_actionResetHeight = new ParamButton("Reset height", null);
            private ParamButton m_actionClearAnchors = new ParamButton("Clear anchors", null);
            private ParamButton m_actionSetAutoAnchors = new ParamButton("Set auto anchors", null);
            private ParamFloat m_paramMinX = new ParamFloat("-X pos", 1f, 0f, 1f, 0.01f);
            private ParamFloat m_paramMaxX = new ParamFloat("+X pos", 1f, 0f, 1f, 0.01f);
            private ParamFloat m_paramMinY = new ParamFloat("-Y pos", 1f, 0f, 1f, 0.01f);
            private ParamFloat m_paramMaxY = new ParamFloat("+Y pos", 1f, 0f, 1f, 0.01f);
            private ParamButton m_actionSetAnchors = new ParamButton("Set anchors", null);
            private ParamButton m_actionMergeAnchors = new ParamButton("Merge anchors", null);
            private bool m_copying;
            private EditorObject m_sourceObject;
            private EditorObjectPivot m_sourcePivot;

            public EditPivotMode(ToolObject context) : base(context)
            {
                m_actionSaveDatabase.Callback = action_SaveDatabase;
                m_actionSetHeight.Callback = action_SetHeight;
                m_actionResetHeight.Callback = action_ResetHeight;
                m_actionClearAnchors.Callback = action_ClearAnchors;
                m_actionSetAutoAnchors.Callback = action_SetAutoAnchors;
                m_actionSetAnchors.Callback = action_SetAnchors;
                m_actionMergeAnchors.Callback = action_MergeAnchors;
            }

            public override string GetToolName() => "*DEV ONLY* Edit anchors";
            public override Image GetToolImage() => Resources.Anchor;
            public override string GetContextHelp() => "This tool is used only for development purposes! It will not be included in retail, so do not make tests on it.";

            public override IEnumerable<IParameter> GetParameters()
            {
                yield return m_actionSaveDatabase;
                yield return m_actionSetHeight;
                yield return m_actionResetHeight;
                yield return m_paramMinX;
                yield return m_paramMaxX;
                yield return m_paramMinY;
                yield return m_paramMaxY;
                yield return m_actionSetAnchors;
                yield return m_actionMergeAnchors;
                yield return m_actionClearAnchors;
                yield return m_actionSetAutoAnchors;
            }

            private bool CheckSelection(int count)
            {
                if (m_context.m_selection.Count != count)
                {
                    MessageBox.Show(count == 1 ? "Please select one object for this operation." : "Please select " + count + " objects for this operation.");
                    return false;
                }
                return true;
            }

            private void action_SaveDatabase() => ObjectInventory.Instance.SavePivots();
            private void action_SetHeight()
            {
                if (!CheckSelection(1)) return;
                foreach (EditorObject obj in m_context.m_selection.GetObjects())
                {
                    float offset = obj.Position.Z - TerrainManager.GetHeightAt(obj.Position.XY);
                    if (Math.Abs(offset) > 0.001f)
                    {
                        obj.Entry.ZOffset += offset;
                    }
                }
            }
            private void action_ResetHeight()
            {
                if (CheckSelection(1))
                {
                    foreach (EditorObject obj in m_context.m_selection.GetObjects())
                    {
                        obj.Entry.ZOffset = 0f;
                    }
                }
            }
            private void action_ClearAnchors()
            {
                if (CheckSelection(1)) m_context.m_selection[0].Entry.ClearPivots();
            }
            private void action_SetAutoAnchors()
            {
                if (CheckSelection(1)) m_context.m_selection[0].Entry.AutoPivot = true;
            }
            private void action_SetAnchors()
            {
                if (CheckSelection(1))
                {
                    m_context.m_selection[0].Entry.SetPivots(m_paramMinX.Value, m_paramMaxX.Value, m_paramMinY.Value, m_paramMaxY.Value);
                    m_context.m_selection.SnapToClosestObjects();
                }
            }
            private void action_MergeAnchors()
            {
                if (!CheckSelection(2)) return;

                EditorObject obj1 = m_context.m_selection[0];
                EditorObject obj2 = m_context.m_selection[1];
                int pivotCount1 = obj1.Entry.PivotCount;
                int pivotCount2 = obj2.Entry.PivotCount;
                float minDistSq = float.MaxValue;
                EditorObjectPivot bestPivot1 = new EditorObjectPivot();
                EditorObjectPivot bestPivot2 = new EditorObjectPivot();
                int bestIndex1 = -1;
                int bestIndex2 = -1;

                for (int i = 0; i < pivotCount1; i++)
                {
                    if (!obj1.GetPivot(i, out EditorObjectPivot pivot1)) continue;
                    for (int j = 0; j < pivotCount2; j++)
                    {
                        if (obj2.GetPivot(j, out EditorObjectPivot pivot2))
                        {
                            float distSq = (pivot2.position - pivot1.position).LengthSquare;
                            if (distSq < minDistSq)
                            {
                                minDistSq = distSq;
                                bestPivot1 = pivot1;
                                bestIndex1 = i;
                                bestPivot2 = pivot2;
                                bestIndex2 = j;
                            }
                        }
                    }
                }

                if (minDistSq == float.MaxValue)
                {
                    MessageBox.Show("Could not find any anchors to merge.");
                    return;
                }

                Vec3 newPosition = (bestPivot1.position + bestPivot2.position) * 0.5f;
                bestPivot1.position = newPosition;
                bestPivot1.Unapply(obj1);
                obj1.Entry.SetPivot(bestIndex1, bestPivot1);
                bestPivot2.position = newPosition;
                bestPivot2.Unapply(obj2);
                obj2.Entry.SetPivot(bestIndex2, bestPivot2);
            }

            public override void Update(float dt)
            {
                foreach (EditorObject obj in m_context.m_selection.GetObjects())
                {
                    bool autoPivot = obj.Entry.AutoPivot;
                    for (int j = 0; j < obj.Entry.PivotCount; j++)
                    {
                        if (obj.GetPivot(j, out EditorObjectPivot pivot) && Editor.GetScreenPointFromWorldPos(pivot.position, out Vec2 screenPoint))
                        {
                            Render.DrawScreenCircleOutlined(screenPoint, 0f, 0.002f, 0.002f, autoPivot ? Color.Yellow : Color.Red);
                        }
                    }
                }

                if (m_copying && Editor.GetScreenPointFromWorldPos(m_sourcePivot.position, out Vec2 sourceScreenPoint))
                {
                    Render.DrawScreenCircleOutlined(sourceScreenPoint, 0f, 0.002f, 0.002f, Color.LimeGreen);
                }
            }

            public override bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
            {
                switch (mouseEvent)
                {
                    case Editor.MouseEvent.MouseDown:
                        m_sourceObject = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 hitPos);
                        if ((Control.ModifierKeys & Keys.Control) != Keys.None && m_sourceObject.IsValid)
                        {
                            if (m_sourceObject.GetClosestPivot(hitPos, out m_sourcePivot))
                            {
                                m_copying = true;
                            }
                        }
                        else
                        {
                            new SelectAction(m_context).Start();
                        }
                        break;

                    case Editor.MouseEvent.MouseMove:
                        if (m_copying)
                        {
                            EditorObject targetObj = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out _);
                            if (targetObj.IsValid)
                            {
                                if (m_context.m_selection.Count != 1 || !m_context.m_selection.Contains(targetObj))
                                {
                                    EditorObjectSelection selection = EditorObjectSelection.Create();
                                    selection.AddObject(targetObj);
                                    m_context.SetSelection(selection, targetObj);
                                }
                            }
                            else if (m_context.m_selection.Count != 0)
                            {
                                m_context.ClearSelection();
                            }
                        }
                        break;

                    case Editor.MouseEvent.MouseUp:
                        if (m_copying)
                        {
                            EditorObject targetObj = ObjectManager.GetObjectFromScreenPoint(Editor.Viewport.NormalizedMousePos, out _);
                            if (targetObj.IsValid && targetObj.Pointer != m_sourceObject.Pointer)
                            {
                                m_sourcePivot.Unapply(targetObj);
                                targetObj.Entry.AddPivot(m_sourcePivot);
                            }
                            m_copying = false;
                        }
                        break;
                }
                return false;
            }
        }

        public ToolObject()
        {
            m_selectMode = new SelectMode(this);
            m_moveMode = new MoveMode(this);
            m_rotateMode = new RotateMode(this);
            m_snapMode = new SnapMode(this);
            m_addMode = new AddMode(this);
            if (Environment.CommandLine.Contains("-editobjectdb"))
            {
                m_editPivotMode = new EditPivotMode(this);
                m_paramMode.Names = new string[] {
                    Localizer.Localize("TOOL_OBJECT_MODE_SELECT") + " (1)", Localizer.Localize("TOOL_OBJECT_MODE_MOVE") + " (2)",
                    Localizer.Localize("TOOL_OBJECT_MODE_ROTATE") + " (3)", Localizer.Localize("TOOL_OBJECT_MODE_SNAP") + " (4)",
                    Localizer.Localize("TOOL_OBJECT_MODE_ADD") + " (5)", "Edit pivots (6)"
                };
                m_paramMode.Values = new Mode[] { m_selectMode, m_moveMode, m_rotateMode, m_snapMode, m_addMode, m_editPivotMode };
                m_paramMode.Images = new Image[] {
                    m_selectMode.GetToolImage(), m_moveMode.GetToolImage(), m_rotateMode.GetToolImage(),
                    m_snapMode.GetToolImage(), m_addMode.GetToolImage(), m_editPivotMode.GetToolImage()
                };
            }
            else
            {
                m_paramMode.Names = new string[] {
                    Localizer.Localize("TOOL_OBJECT_MODE_SELECT") + " (1)", Localizer.Localize("TOOL_OBJECT_MODE_MOVE") + " (2)",
                    Localizer.Localize("TOOL_OBJECT_MODE_ROTATE") + " (3)", Localizer.Localize("TOOL_OBJECT_MODE_SNAP") + " (4)",
                    Localizer.Localize("TOOL_OBJECT_MODE_ADD") + " (5)"
                };
                m_paramMode.Values = new Mode[] { m_selectMode, m_moveMode, m_rotateMode, m_snapMode, m_addMode };
                m_paramMode.Images = new Image[] {
                    m_selectMode.GetToolImage(), m_moveMode.GetToolImage(), m_rotateMode.GetToolImage(),
                    m_snapMode.GetToolImage(), m_addMode.GetToolImage()
                };
            }
            m_paramMode.ValueChanged += editTool_ValueChanged;
            m_paramMode.Value = m_addMode;
            m_paramAxisType.Names = new string[] { Localizer.Localize("PARAM_AXIS_LOCAL"), Localizer.Localize("PARAM_AXIS_WORLD") };
            m_paramAxisType.ValueChanged += axisType_ValueChanged;
            m_actionDelete.Callback = action_Delete;
            m_actionFreeze.Callback = action_Freeze;
            m_actionUnfreeze.Callback = action_Unfreeze;
        }

        public string GetToolName() => Localizer.Localize("TOOL_OBJECT");
        public Image GetToolImage() => Resources.Object_Edit;
        public IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramMode;

            if (m_paramMode.Value != null)
            {
                foreach (IParameter parameter in m_paramMode.Value.GetParameters())
                {
                    yield return parameter;
                }
            }
        }

        public IParameter GetMainParameter() => m_paramMode.Value?.GetMainParameter();
        public string GetContextHelp() => m_paramMode.Value.GetContextHelp();
        public void UpdateContextHelp() => this.ContextHelpChanged?.Invoke(this, null);

        private void UpdateParams()
        {
            bool enabled = m_selection.Count > 0;
            m_actionDelete.Enabled = enabled;
            m_actionFreeze.Enabled = enabled;
            m_textSelected.DisplayName = m_selection.Count + " " + (m_selection.Count > 1 ? Localizer.Localize("PARAM_OBJECTS_SELECTED_COUNT") : Localizer.Localize("PARAM_OBJECT_SELECTED_COUNT"));
            m_paramMode.Value?.UpdateParams();
        }

        private void ClearMode(Mode mode)
        {
            mode.Deactivate();
            Editor.PopInput(mode);
        }

        private void SetMode(Mode mode)
        {
            m_mode = mode;
            Editor.PushInput(mode);
            mode.Activate();
            this.ParamsChanged?.Invoke(this, null);
            UpdateContextHelp();
        }

        private void SwitchMode(Mode prevMode, Mode mode)
        {
            ClearMode(prevMode);
            m_paramMode.Value = mode;
            SetMode(mode);
        }

        private void SwitchMode(Mode mode) => SwitchMode(m_paramMode.Value, mode);
        private void editTool_ValueChanged(object sender, EventArgs e) => SwitchMode(m_mode, m_paramMode.Value);
        private void axisType_ValueChanged(object sender, EventArgs e) => UpdateSelection();
        private void action_Delete() => DeleteSelection();
        private void action_Freeze()
        {
            for (int i = 0; i < m_selection.Count; i++)
            {
                EditorObject editorObject = m_selection[i];
                editorObject.Frozen = true;
            }
            ClearSelection();
        }
        private void action_Unfreeze() => ObjectManager.UnfreezeObjects();

        public void Activate()
        {
            m_selection = EditorObjectSelection.Create();
            SetMode(m_paramMode.Value);
        }

        public void Deactivate()
        {
            ClearMode(m_paramMode.Value);
            ClearSelection();
            m_selection.Dispose();
        }

        public void OnInputAcquire() { }
        public void OnInputRelease() { }

        public bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            if (mouseEvent == Editor.MouseEvent.MouseMove)
            {
                TestGizmo();
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
                        case Keys.D1: SwitchMode(m_selectMode); return true;
                        case Keys.D2: SwitchMode(m_moveMode); return true;
                        case Keys.D3: SwitchMode(m_rotateMode); return true;
                        case Keys.D4: SwitchMode(m_snapMode); return true;
                        case Keys.D5: SwitchMode(m_addMode); break;
                        case Keys.D6: if (m_editPivotMode != null) SwitchMode(m_editPivotMode); break;
                    }
                    break;
                case Editor.KeyEvent.KeyUp:
                    if (keyEventArgs.KeyCode == Keys.Delete)
                    {
                        DeleteSelection();
                        return true;
                    }
                    break;
            }
            return false;
        }

        public void OnEditorEvent(uint eventType, IntPtr eventPtr)
        {
            if (eventType == EditorEventUndo.TypeId)
            {
                ClearSelectionState();
                m_selection.RemoveInvalidObjects();
                m_selection.ComputeCenter();
                UpdateSelectionState();
                UpdateSelection();
            }
        }

        public void Update(float dt)
        {
            if (m_gizmo.IsValid)
            {
                UpdateGizmo();
                if (m_gizmoEnabled)
                    m_gizmo.Redraw();
                else
                    m_gizmo.Hide();
            }
            if (m_paramGroupSelection.Value)
            {
                AABB worldBounds = m_selection.WorldBounds;
                Vec3 length = worldBounds.Length;
                Vec3 pos = worldBounds.min + length * 0.5f;
                pos.Z = worldBounds.min.Z;
                Render.DrawWireBoxFromBottomZ(pos, length, 0.005f);
            }
        }

        private void ClearSelectionState()
        {
            for (int i = 0; i < m_selection.Count; i++)
            {
                EditorObject editorObject = m_selection[i];
                editorObject.HighlightState = false;
            }
        }

        private void UpdateSelectionState()
        {
            for (int i = 0; i < m_selection.Count; i++)
            {
                EditorObject editorObject = m_selection[i];
                editorObject.HighlightState = true;
            }
        }

        private void ClearSelection()
        {
            ClearSelectionState();
            ClearGizmo();
            m_selection.Clear();
            UpdateSelection();
        }

        private void SetSelection(EditorObjectSelection selection, EditorObject gizmoObject)
        {
            ClearSelectionState();
            m_selection.Dispose();
            m_selection = selection;
            m_selection.ComputeCenter();

            if (!m_selection.Contains(gizmoObject))
                gizmoObject = EditorObject.Null;

            if (!gizmoObject.IsValid && m_selection.Count > 0)
                gizmoObject = m_selection[0];

            if (gizmoObject.IsValid)
                SetupGizmo(gizmoObject);
            else
                ClearGizmo();

            UpdateSelectionState();
            UpdateSelection();
        }

        private void UpdateSelection() => UpdateSelection(false);
        private void UpdateSelection(bool updateCenter)
        {
            if (updateCenter)
                m_selection.ComputeCenter();

            UpdateGizmo();
            UpdateParams();
        }

        private void DeleteSelection()
        {
            if (m_selection.Count != 0)
            {
                UndoManager.RecordUndo();
                m_selection.Delete();
                UndoManager.CommitUndo();
                UpdateSelection();
            }
        }

        private void SelectObject(EditorObjectSelection selection, EditorObject obj)
        {
            bool isCtrl = (Control.ModifierKeys & Keys.Control) != Keys.None;
            if (!m_paramMagicWand.Value)
            {
                if (!isCtrl)
                    selection.AddObject(obj);
                else
                    selection.ToggleObject(obj);
            }
            else
            {
                using (EditorObjectSelection wandSelection = EditorObjectSelection.Create())
                {
                    ObjectManager.GetObjectsFromMagicWand(wandSelection, obj);
                    if (!isCtrl)
                        selection.AddSelection(wandSelection);
                    else
                        selection.ToggleSelection(wandSelection);
                }
            }
        }

        private void ClearGizmo()
        {
            if (m_gizmo.IsValid)
            {
                m_gizmo.Dispose();
                m_gizmo = Gizmo.Null;
            }
            m_gizmoObject = EditorObject.Null;
            m_gizmoActive = false;
        }

        private void SetupGizmo(EditorObject gizmoObject)
        {
            ClearGizmo();
            m_gizmo = Gizmo.Create();
            m_gizmoObject = gizmoObject;
            UpdateGizmo();
            TestGizmo();
        }

        private void UpdateGizmo()
        {
            if (m_gizmo.IsValid)
            {
                if (m_selection.Count == 0)
                {
                    ClearGizmo();
                }
                else if (!m_paramGroupSelection.Value)
                {
                    CoordinateSystem axis = CoordinateSystem.FromAngles(m_gizmoObject.Angles);
                    m_gizmo.Axis = (m_paramAxisType.Value == AxisType.World) ? CoordinateSystem.Standard : axis;
                    m_gizmo.Position = m_gizmoObject.Position;
                }
                else
                {
                    m_gizmo.Axis = CoordinateSystem.Standard;
                    m_gizmo.Position = m_selection.GetComputeCenter();
                }
            }
        }

        private void TestGizmo()
        {
            if (m_gizmo.IsValid && m_gizmoEnabled)
            {
                Editor.GetWorldRayFromScreenPoint(Editor.Viewport.NormalizedMousePos, out Vec3 raySrc, out Vec3 rayDir);
                Axis axis = m_gizmo.HitTest(raySrc, rayDir);
                if (m_gizmo.Active != axis)
                {
                    m_gizmo.Active = axis;
                }
                m_gizmoActive = axis != Axis.None;
            }
            else
            {
                m_gizmoActive = false;
            }
        }

        private void EnableGizmo(bool enable)
        {
            m_gizmoEnabled = enable;
        }
    }
    #endregion
}