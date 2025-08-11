using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FC2Editor.Core.Nomad
{
    internal static class Engine
    {
        public delegate void InvokeDelegate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MessagePumpCallbackDelegate(bool deferQuit, bool blockRenderer);

        private static List<InvokeDelegate> m_delayedCallbacks = new List<InvokeDelegate>();
        private static MessagePumpCallbackDelegate m_delegateMessagePumpCallback;
        private static bool m_initialized = false;

        public static string PersonalPath => Marshal.PtrToStringAnsi(FCE_Engine_GetPersonalPath());
        public static bool Initialized => m_initialized;
        public static bool ConsoleOpened => FCE_Engine_IsConsoleOpen();

        public static TimeSpan TimeOfDay
        {
            get { FCE_Engine_GetTimeOfDay(out int hour, out int minute, out int second); return new TimeSpan(hour, minute, second); }
            set { FCE_Engine_SetTimeOfDay(value.Hours, value.Minutes, value.Seconds); }
        }

        public static float StormFactor
        {
            get { return FCE_Engine_GetStormFactor(); }
            set { FCE_Engine_SetStormFactor(value); }
        }

        public static bool Init(Form mainWindow, Control viewport)
        {
            m_delegateMessagePumpCallback = MessagePumpCallback;
            string commandLine = "-editorpc -RenderProfile_Quality optimal -3dplatform d3d9";

            string mapArgument = Program.GetMapArgument();
            if (mapArgument != null)
            {
                // Enclose the map path in quotes to handle spaces
                commandLine = "\"" + mapArgument + "\" " + commandLine;
            }

            if (!InitDuniaEngine(Process.GetCurrentProcess().MainModule.BaseAddress, mainWindow.Handle, viewport.Handle, commandLine, true, true, m_delegateMessagePumpCallback))
            {
                return false;
            }
            FCE_Engine_AutoAcquireInput(true);
            Editor.Init();
            if (!Directory.Exists(PersonalPath))
            {
                Directory.CreateDirectory(PersonalPath);
            }
            m_initialized = true;
            return true;
        }

        public static void Run()
        {
            while (MainForm.Instance != null && !MainForm.Instance.IsDisposed)
            {
                bool isActive = Editor.IsActive;
                if (m_delayedCallbacks.Count > 0)
                {
                    isActive = true;
                    lock (m_delayedCallbacks)
                    {
                        foreach (InvokeDelegate delayedCallback in m_delayedCallbacks)
                        {
                            delayedCallback();
                        }
                        m_delayedCallbacks.Clear();
                    }
                }
                if (isActive)
                {
                    TickDuniaEngine();
                }
                else
                {
                    Thread.Sleep(50);
                }
                Application.DoEvents();
            }
        }

        public static void Invoke(InvokeDelegate callback)
        {
            lock (m_delayedCallbacks)
            {
                m_delayedCallbacks.Add(callback);
            }
        }

        private static void MessagePumpCallback(bool deferQuit, bool blockRenderer) { }

        public static void Close()
        {
            CloseDuniaEngine();
        }

        public static void UpdateResolution(Size size)
        {
            if (Initialized)
            {
                FCE_Engine_UpdateViewport(size.Width, size.Height);
            }
        }

        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool InitDuniaEngine(IntPtr hInstance, IntPtr focusWnd, IntPtr renderWnd, [MarshalAs(UnmanagedType.LPStr)] string cmdLine, [MarshalAs(UnmanagedType.U1)] bool launchGame, [MarshalAs(UnmanagedType.U1)] bool forceGpuSynchronization, MessagePumpCallbackDelegate messagePumpCallback);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] public static extern bool TickDuniaEngine();
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool CloseDuniaEngine();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Engine_GetPersonalPath();
        [DllImport("Dunia.dll")] private static extern void FCE_Engine_UpdateViewport(int sizeX, int sizeY);
        [DllImport("Dunia.dll")] private static extern void FCE_Engine_AutoAcquireInput([MarshalAs(UnmanagedType.U1)] bool autoAcquire);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Engine_IsConsoleOpen();
        [DllImport("Dunia.dll")] private static extern void FCE_Engine_GetTimeOfDay(out int hour, out int minute, out int second);
        [DllImport("Dunia.dll")] private static extern void FCE_Engine_SetTimeOfDay(int hour, int minute, int second);
        [DllImport("Dunia.dll")] private static extern float FCE_Engine_GetStormFactor();
        [DllImport("Dunia.dll")] private static extern void FCE_Engine_SetStormFactor(float stormFactor);
    }
}