using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FC2Editor.Core
{
    internal class Win32
    {
        public struct Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public struct Size
        {
            public int cx;
            public int cy;
            public Size(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public Rect(int left, int top, int width, int height)
            {
                this.left = left;
                this.top = top;
                right = left + width;
                bottom = top + height;
            }
        }

        public struct Message
        {
            public IntPtr hWnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public Point pt;
        }

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class ScrollInfo
        {
            public int cbSize = Marshal.SizeOf(typeof(ScrollInfo));
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage; // <<< MUST BE INT
            public int nPos;
            public int nTrackPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class DrawTextParams
        {
            public int cbSize = Marshal.SizeOf(typeof(DrawTextParams));
            public int iTabLength;
            public int iLeftMargin;
            public int iRightMargin;
            public uint uiLengthDrawn;
        }

        public struct ABC
        {
            public int A;
            public int B;
            public int C;
        }

        public struct TextMetric
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        public const int VK_LSHIFT = 160;
        public const int VK_RSHIFT = 161;

        public struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        public enum EXTENDED_NAME_FORMAT
        {
            NameUnknown = 0,
            NameFullyQualifiedDN = 1,
            NameSamCompatible = 2,
            NameDisplay = 3,
            NameUniqueId = 6,
            NameCanonical = 7,
            NameUserPrincipal = 8,
            NameCanonicalEx = 9,
            NameServicePrincipal = 10,
            NameDnsDomain = 12
        }

        public const int WM_SETREDRAW = 11;
        public const int WM_COPYDATA = 74;
        public const int WM_GETDLGCODE = 135;
        public const int WM_HSCROLL = 276;
        public const int WM_VSCROLL = 277;
        public const int LVM_SETEXTENDEDLISTVIEWSTYLE = 4150;
        public const int LVS_EX_BORDERSELECT = 32768;

        [DllImport("kernel32.dll")]
        public static extern void RtlMoveMemory(IntPtr dest, IntPtr src, int size);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern void GetPrivateProfileStringW(string lpAppName, string lpKeyName, string lpDefault, IntPtr lpReturnedString, int nSize, string lpFileName);

        public static void GetPrivateProfileStringW(string lpAppName, string lpKeyName, string lpDefault, out string lpReturnedString, string lpFileName)
        {
            IntPtr intPtr = Marshal.AllocHGlobal(514);
            GetPrivateProfileStringW(lpAppName, lpKeyName, lpDefault, intPtr, 256, lpFileName);
            lpReturnedString = Marshal.PtrToStringUni(intPtr);
            Marshal.FreeHGlobal(intPtr);
        }

        public static int LoWord(int dw)
        {
            return dw & 0xFFFF;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowEnabled(IntPtr hWnd);

        public static int MakeLong(int lw, int hw)
        {
            return (lw & 0xFFFF) | ((hw & 0xFFFF) << 16);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(int idThread);

        [DllImport("user32.dll")]
        public static extern int MapVirtualKeyEx(int uCode, int uMapType, IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern ushort GetKeyState(int nVirtKey);

        public static bool IsKeyDown(int nVirtKey)
        {
            return (GetKeyState(nVirtKey) & 0x8000) != 0;
        }
        [DllImport("user32.dll")] public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);
        [DllImport("user32.dll")] public static extern bool ShowCaret(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool HideCaret(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool DestroyCaret();
        [DllImport("user32.dll")] public static extern bool SetCaretPos(int x, int y);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref Rect lParam);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsProc ewp, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetProp(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr hData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr RemoveProp(IntPtr hWnd, string lpString);

        public static void SetRedraw(Control control, bool redraw)
        {
            SendMessage(control.Handle, WM_SETREDRAW, redraw ? 1 : 0, 0);
        }

        [DllImport("user32.dll")]
        public static extern int GetScrollInfo(IntPtr hWnd, int nBar, [In] ScrollInfo scrollInfo);

        [DllImport("user32.dll")]
        public static extern int SetScrollInfo(IntPtr hWnd, int nBar, [In] ScrollInfo scrollInfo, bool bRedraw);

        [DllImport("user32.dll")]
        public static extern int ScrollWindowEx(IntPtr hWnd, int dx, int dy, ref Rect prcScroll, ref Rect prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, int flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "DrawTextExW")]
        public static extern int DrawTextEx(IntPtr hdc, string lpchText, int cchText, ref Rect lprc, uint dwDTFormat, [In, Out] DrawTextParams lpDTParams);

        [DllImport("user32.dll")]
        public static extern int FillRect(IntPtr hDC, ref Rect lprc, IntPtr hbr);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(int crColor);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern int SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        public static extern int SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        public static extern bool GetTextMetrics(IntPtr hdc, out TextMetric lptm);

        [DllImport("user32.dll")]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, int crKey, ref BlendFunction pblend, int dwFlags);

        public static string GetUserNameHelper()
        {
            string text = GetUserNameEx(EXTENDED_NAME_FORMAT.NameDisplay);
            if (text == null)
            {
                text = GetUserName();
            }
            return text;
        }

        public static string GetUserName()
        {
            string result = null;
            IntPtr intPtr = Marshal.AllocHGlobal(512);
            uint nSize = 256u;
            if (GetUserNameW(intPtr, ref nSize) != 0)
            {
                result = Marshal.PtrToStringUni(intPtr);
            }
            Marshal.FreeHGlobal(intPtr);
            return result;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetUserNameW(IntPtr lpBuffer, ref uint nSize);

        public static string GetUserNameEx(EXTENDED_NAME_FORMAT NameFormat)
        {
            string result = null;
            IntPtr intPtr = Marshal.AllocHGlobal(512);
            uint nSize = 256u;
            if (GetUserNameExW(NameFormat, intPtr, ref nSize) != 0)
            {
                result = Marshal.PtrToStringUni(intPtr);
            }
            Marshal.FreeHGlobal(intPtr);
            return result;
        }

        [DllImport("secur32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetUserNameExW(EXTENDED_NAME_FORMAT NameFormat, IntPtr lpNameBuffer, ref uint nSize);
    }
}