using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct SplineController
    {
        public enum SelectMode
        {
            Replace,
            Add,
            Toggle
        }

        public static SplineController Null = new SplineController(IntPtr.Zero);
        private IntPtr m_controllerPtr;

        public IntPtr Pointer => m_controllerPtr;

        public SplineController(IntPtr ptr)
        {
            m_controllerPtr = ptr;
        }

        public static SplineController Create() => new SplineController(FCE_SplineController_Create());
        public void Dispose() => FCE_SplineController_Destroy(m_controllerPtr);
        public void SetSpline(Spline spline) => FCE_SplineController_SetSpline(m_controllerPtr, spline.Pointer);
        public void ClearSelection() => FCE_SplineController_ClearSelection(m_controllerPtr);
        public bool IsSelected(int index) => FCE_SplineController_IsSelected(m_controllerPtr, index);
        public void SetSelected(int index, bool selected) => FCE_SplineController_SetSelected(m_controllerPtr, index, selected);
        public void SelectFromScreenRect(RectangleF rect, float penWidth, SelectMode selectMode) => FCE_SplineController_SelectFromScreenRect(m_controllerPtr, rect.X, rect.Y, rect.Right, rect.Bottom, penWidth, selectMode);
        public void MoveSelection(Vec2 delta) => FCE_SplineController_MoveSelection(m_controllerPtr, delta.X, delta.Y);
        public void DeleteSelection() => FCE_SplineController_DeleteSelection(m_controllerPtr);

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_SplineController_Create();
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_Destroy(IntPtr controller);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_SetSpline(IntPtr controller, IntPtr spline);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_ClearSelection(IntPtr controller);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_SplineController_IsSelected(IntPtr controller, int index);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_SetSelected(IntPtr controller, int index, [MarshalAs(UnmanagedType.U1)] bool selected);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_SelectFromScreenRect(IntPtr controller, float x1, float y1, float x2, float y2, float penWidth, SelectMode selectMode);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_MoveSelection(IntPtr controller, float x, float y);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineController_DeleteSelection(IntPtr controller);
    }
}