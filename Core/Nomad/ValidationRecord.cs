using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class ValidationRecord
    {
        public enum Severities
        {
            Error = 1,
            Warning = 2,
            Comment = 4,
            Success = 8,
            All = 15,
            NoSuccess = 7
        }

        public enum Flags
        {
            None = 0,
            Validation = 0x20
        }

        private IntPtr m_pointer;

        public Severities Severity => FCE_ValidationRecord_GetSeverity(m_pointer);
        public Flags Flag => FCE_ValidationRecord_GetFlags(m_pointer);
        public string Message => Marshal.PtrToStringUni(FCE_ValidationRecord_GetMessage(m_pointer));
        public EditorObject Object => new EditorObject(FCE_ValidationRecord_GetObject(m_pointer));

        public ValidationRecord(IntPtr ptr)
        {
            m_pointer = ptr;
        }

        [DllImport("Dunia.dll")] private static extern Severities FCE_ValidationRecord_GetSeverity(IntPtr record);
        [DllImport("Dunia.dll")] private static extern Flags FCE_ValidationRecord_GetFlags(IntPtr record);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ValidationRecord_GetMessage(IntPtr record);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ValidationRecord_GetObject(IntPtr record);
    }
}