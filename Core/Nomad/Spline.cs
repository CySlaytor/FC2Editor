using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class Spline : IDisposable
    {
        public static Spline Null = new Spline(IntPtr.Zero);
        protected IntPtr m_splinePtr;

        public int Count => FCE_Spline_GetNumPoints(m_splinePtr);

        public Vec2 this[int index]
        {
            get { FCE_Spline_GetPoint(m_splinePtr, index, out float x, out float y); return new Vec2(x, y); }
            set { FCE_Spline_SetPoint(m_splinePtr, index, value.X, value.Y); }
        }

        public bool IsValid => Pointer != IntPtr.Zero;
        public IntPtr Pointer => m_splinePtr;

        public Spline(IntPtr ptr)
        {
            m_splinePtr = ptr;
        }

        public static Spline Create()
        {
            return new Spline(FCE_Spline_Create());
        }

        public void Dispose()
        {
            FCE_Spline_Destroy(m_splinePtr);
        }

        public void Clear() => FCE_Spline_Clear(m_splinePtr);
        public void AddPoint(Vec2 point) => FCE_Spline_AddPoint(m_splinePtr, point.X, point.Y);
        public void InsertPoint(Vec2 point, int index) => FCE_Spline_InsertPoint(m_splinePtr, point.X, point.Y, index);
        public void RemovePoint(int index) => FCE_Spline_RemovePoint(m_splinePtr, index);
        public bool RemoveSimilarPoints() => FCE_Spline_RemoveSimilarPoints(m_splinePtr);
        public bool OptimizePoint(int index) => FCE_Spline_OptimizePoint(m_splinePtr, index);
        public void UpdateSpline() => FCE_Spline_UpdateSpline(m_splinePtr);
        public void UpdateSplineHeight() => FCE_Spline_UpdateSplineHeight(m_splinePtr);
        public void FinalizeSpline() => FCE_Spline_FinalizeSpline(m_splinePtr);
        public void Draw(float penWidth, SplineController controller) => FCE_Spline_Draw(m_splinePtr, penWidth, controller.Pointer);

        public bool HitTestPoints(Vec2 point, float penWidth, float hitWidth, out int hitIndex, out Vec2 hitPos)
        {
            return FCE_Spline_HitTestPoints(m_splinePtr, point.X, point.Y, penWidth, hitWidth, out hitIndex, out hitPos.X, out hitPos.Y);
        }

        public bool HitTestSegments(Vec2 center, float radius, out int hitIndex, out Vec2 hitPos)
        {
            return FCE_Spline_HitTestSegments(m_splinePtr, center.X, center.Y, radius, out hitIndex, out hitPos.X, out hitPos.Y);
        }

        [DllImport("Dunia.dll")] protected static extern IntPtr FCE_Spline_Create();
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_Destroy(IntPtr spline);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_Clear(IntPtr spline);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_AddPoint(IntPtr spline, float x, float y);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_InsertPoint(IntPtr spline, float x, float y, int index);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_RemovePoint(IntPtr spline, int index);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] protected static extern bool FCE_Spline_RemoveSimilarPoints(IntPtr spline);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] protected static extern bool FCE_Spline_OptimizePoint(IntPtr spline, int index);
        [DllImport("Dunia.dll")] protected static extern int FCE_Spline_GetNumPoints(IntPtr spline);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_GetPoint(IntPtr spline, int i, out float x, out float y);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_SetPoint(IntPtr spline, int i, float x, float y);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_UpdateSpline(IntPtr spline);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_UpdateSplineHeight(IntPtr spline);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_FinalizeSpline(IntPtr spline);
        [DllImport("Dunia.dll")] protected static extern void FCE_Spline_Draw(IntPtr spline, float penWidth, IntPtr controller);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] protected static extern bool FCE_Spline_HitTestPoints(IntPtr spline, float x, float y, float penWidth, float hitWidth, out int hitIndex, out float hitX, out float hitY);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] protected static extern bool FCE_Spline_HitTestSegments(IntPtr spline, float centerX, float centerY, float radius, out int hitIndex, out float hitX, out float hitY);
    }
}