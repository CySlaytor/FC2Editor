using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class SplineRoad : Spline
    {
        public new static SplineRoad Null = new SplineRoad(IntPtr.Zero);

        public SplineInventory.Entry Entry
        {
            get { return new SplineInventory.Entry(FCE_SplineRoad_GetEntry(m_splinePtr)); }
            set { FCE_SplineRoad_SetEntry(m_splinePtr, value.Pointer); }
        }

        public float Width
        {
            get { return FCE_SplineRoad_GetWidth(m_splinePtr); }
            set { FCE_SplineRoad_SetWidth(m_splinePtr, value); }
        }

        public SplineRoad(IntPtr ptr) : base(ptr) { }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_SplineRoad_GetEntry(IntPtr spline);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineRoad_SetEntry(IntPtr spline, IntPtr entry);
        [DllImport("Dunia.dll")] private static extern float FCE_SplineRoad_GetWidth(IntPtr spline);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineRoad_SetWidth(IntPtr spline, float width);
    }
}