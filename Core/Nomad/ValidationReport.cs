using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct ValidationReport
    {
        public static ValidationReport Null = new ValidationReport(IntPtr.Zero);
        private IntPtr m_pointer;

        public int Count => FCE_ValidationReport_GetCount(m_pointer);
        public ValidationRecord this[int index] => new ValidationRecord(FCE_ValidationReport_GetRecord(m_pointer, index));
        public bool IsValid => m_pointer != IntPtr.Zero;

        public ValidationReport(IntPtr ptr)
        {
            m_pointer = ptr;
        }

        public void Destroy()
        {
            if (IsValid)
            {
                FCE_ValidationReport_Destroy(m_pointer);
                m_pointer = IntPtr.Zero;
            }
        }

        [DllImport("Dunia.dll")] private static extern void FCE_ValidationReport_Destroy(IntPtr report);
        [DllImport("Dunia.dll")] private static extern int FCE_ValidationReport_GetCount(IntPtr report);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ValidationReport_GetRecord(IntPtr report, int index);
    }
}