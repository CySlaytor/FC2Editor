using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class SplineZone : Spline
    {
        public new static SplineZone Null = new SplineZone(IntPtr.Zero);

        public SplineZone(IntPtr ptr) : base(ptr) { }

        public void Reset() => FCE_SplineZone_Reset(m_splinePtr);

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_SplineZone_Reset(IntPtr spline);
    }
}