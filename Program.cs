using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;

namespace FC2Editor
{
    internal static class Program
    {
        public static string programGuid = "9de9f6ee-6db7-41bf-a0b4-112e45dd3693";

        public static string GetMapArgument()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length >= 2 && !commandLineArgs[1].StartsWith("-"))
            {
                return commandLineArgs[1];
            }
            return null;
        }

        private static bool OpenExistingAppCallback(IntPtr hWnd, IntPtr lParam)
        {
            if (Win32.GetProp(hWnd, programGuid) != IntPtr.Zero)
            {
                // This function is not implemented in the provided decompiled source for Win32
                // Win32.SetForegroundWindow(hWnd); 

                IntPtr intPtr = IntPtr.Zero;
                int cbData = 0;
                string mapArgument = GetMapArgument();
                if (mapArgument != null)
                {
                    intPtr = Marshal.StringToCoTaskMemUni(mapArgument);
                    cbData = (mapArgument.Length + 1) * 2;
                }
                if (intPtr != IntPtr.Zero)
                {
                    Win32.COPYDATASTRUCT lParam2 = new Win32.COPYDATASTRUCT
                    {
                        dwData = IntPtr.Zero,
                        lpData = intPtr,
                        cbData = cbData
                    };
                    Win32.SendMessage(hWnd, Win32.WM_COPYDATA, 0, ref lParam2);
                }
                Marshal.FreeCoTaskMem(intPtr);
                return false;
            }
            return true;
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.CurrentCulture = CultureInfo.InvariantCulture;
            bool createdNew;
            using (new Mutex(true, programGuid, out createdNew))
            {
                if (!createdNew)
                {
                    Win32.EnumWindows(OpenExistingAppCallback, IntPtr.Zero);
                    return;
                }

                MainForm mainForm = new MainForm();
                SplashForm.Start();
                bool flag = Engine.Init(mainForm, mainForm.Viewport);
                SplashForm.Stop();
                if (flag)
                {
                    mainForm.Show();
                    mainForm.PostLoad();
                    Engine.Run();
                    mainForm.Dispose();
                }
            }
        }
    }
}