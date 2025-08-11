using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct Gizmo : IDisposable
    {
        public static Gizmo Null = new Gizmo(IntPtr.Zero);
        private IntPtr m_gizmoPtr;

        public Vec3 Position
        {
            get { FCE_Gizmo_GetPos(m_gizmoPtr, out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Gizmo_SetPos(m_gizmoPtr, value.X, value.Y, value.Z); }
        }

        public CoordinateSystem Axis
        {
            get
            {
                CoordinateSystem result = default(CoordinateSystem);
                FCE_Gizmo_GetAxis(m_gizmoPtr, out result.axisX.X, out result.axisX.Y, out result.axisX.Z, out result.axisY.X, out result.axisY.Y, out result.axisY.Z, out result.axisZ.X, out result.axisZ.Y, out result.axisZ.Z);
                return result;
            }
            set { FCE_Gizmo_SetAxis(m_gizmoPtr, value.axisX.X, value.axisX.Y, value.axisX.Z, value.axisY.X, value.axisY.Y, value.axisY.Z, value.axisZ.X, value.axisZ.Y, value.axisZ.Z); }
        }

        public Axis Active
        {
            get { return FCE_Gizmo_GetActive(m_gizmoPtr); }
            set { FCE_Gizmo_SetActive(m_gizmoPtr, value); }
        }

        public bool IsValid => m_gizmoPtr != IntPtr.Zero;
        public IntPtr Pointer => m_gizmoPtr;

        public Gizmo(IntPtr ptr)
        {
            m_gizmoPtr = ptr;
        }

        public static Gizmo Create()
        {
            return new Gizmo(FCE_Gizmo_Create());
        }

        public void Dispose()
        {
            FCE_Gizmo_Destroy(m_gizmoPtr);
        }

        public void Redraw()
        {
            FCE_Gizmo_Redraw(m_gizmoPtr);
        }

        public void Hide()
        {
            FCE_Gizmo_Hide(m_gizmoPtr);
        }

        public Axis HitTest(Vec3 raySrc, Vec3 rayDir)
        {
            return FCE_Gizmo_HitTest(m_gizmoPtr, raySrc.X, raySrc.Y, raySrc.Z, rayDir.X, rayDir.Y, rayDir.Z);
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Gizmo_Create();
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_Destroy(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_GetPos(IntPtr ptr, out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_SetPos(IntPtr ptr, float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_GetAxis(IntPtr ptr, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2, out float x3, out float y3, out float z3);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_SetAxis(IntPtr ptr, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3);
        [DllImport("Dunia.dll")] private static extern Axis FCE_Gizmo_GetActive(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_SetActive(IntPtr ptr, Axis axis);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_Redraw(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_Gizmo_Hide(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern Axis FCE_Gizmo_HitTest(IntPtr ptr, float raySrcX, float raySrcY, float raySrcZ, float rayDirX, float rayDirY, float rayDirZ);
    }
}