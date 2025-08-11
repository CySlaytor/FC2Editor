using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class Wilderness
    {
        public delegate void MapCallback(int line, IntPtr map);
        public delegate void ErrorCallback(int line, IntPtr errorMessage);

        public struct FunctionDef
        {
            private IntPtr m_pointer;

            public string Name => Marshal.PtrToStringAnsi(FCE_ScriptFunction_GetName(m_pointer));
            public string Prototype => Marshal.PtrToStringAnsi(FCE_ScriptFunction_GetPrototype(m_pointer));
            public string Description => Marshal.PtrToStringAnsi(FCE_ScriptFunction_GetDescription(m_pointer));

            public FunctionDef(IntPtr ptr) { m_pointer = ptr; }

            [DllImport("Dunia.dll")] private static extern IntPtr FCE_ScriptFunction_GetName(IntPtr ptr);
            [DllImport("Dunia.dll")] private static extern IntPtr FCE_ScriptFunction_GetPrototype(IntPtr ptr);
            [DllImport("Dunia.dll")] private static extern IntPtr FCE_ScriptFunction_GetDescription(IntPtr ptr);
        }

        public static int NumFunctions => FCE_Script_GetNumFunctions();
        public static void GenerateDesert(float gradientWidth, float gradientHeight, float distorsion, float noiseAdd, float blurRadius) => FCE_Wilderness_Desert(gradientWidth, gradientHeight, distorsion, noiseAdd, blurRadius);
        public static void RunScript(string scriptName) => FCE_Wilderness_Script(scriptName);
        public static void RunScriptBuffer(string buffer, MapCallback mapCallback, ErrorCallback errorCallback) => FCE_Wilderness_ScriptBuffer(buffer, buffer.Length, mapCallback, errorCallback);
        public static void RunScriptEntry(WildernessInventory.Entry entry) => FCE_Wilderness_ScriptEntry(entry.Pointer);
        public static FunctionDef GetFunction(int index) => new FunctionDef(FCE_Script_GetFunction(index));

        [DllImport("Dunia.dll")] private static extern void FCE_Wilderness_Desert(float gradientWidth, float gradientHeight, float distorsion, float noiseAdd, float blurRadius);
        [DllImport("Dunia.dll")] private static extern void FCE_Wilderness_Script(string scriptName);
        [DllImport("Dunia.dll")] private static extern void FCE_Wilderness_ScriptBuffer(string buffer, int size, MapCallback mapCallback, ErrorCallback errorCallback);
        [DllImport("Dunia.dll")] private static extern void FCE_Wilderness_ScriptEntry(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern int FCE_Script_GetNumFunctions();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Script_GetFunction(int index);
    }
}