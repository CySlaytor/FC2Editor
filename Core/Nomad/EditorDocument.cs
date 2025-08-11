using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FC2Editor.Core.Nomad
{
    internal class EditorDocument
    {
        public delegate void LoadCompletedCallback(bool success);
        public delegate void SaveCompletedCallback(bool success);

        public enum BattlefieldSizes
        {
            Small,
            Medium,
            Large
        }

        public enum PlayerSizes
        {
            Small,
            Medium,
            Large,
            XLarge
        }

        private static LoadCompletedCallback m_loadCompletedCallback;
        private static SaveCompletedCallback m_saveCompletedCallback;

        public static string MapName
        {
            get { return Marshal.PtrToStringAnsi(FCE_Document_GetMapName()); }
            set { FCE_Document_SetMapName(value); }
        }

        public static string CreatorName
        {
            get { return Marshal.PtrToStringAnsi(FCE_Document_GetCreatorName()); }
            set { FCE_Document_SetCreatorName(value); }
        }

        public static string AuthorName
        {
            get { return Marshal.PtrToStringAnsi(FCE_Document_GetAuthorName()); }
            set { FCE_Document_SetAuthorName(value); }
        }

        public static BattlefieldSizes BattlefieldSize
        {
            get { return FCE_Document_GetBattlefieldSize(); }
            set { FCE_Document_SetBattlefieldSize(value); }
        }

        public static PlayerSizes PlayerSize
        {
            get { return FCE_Document_GetPlayerSize(); }
            set { FCE_Document_SetPlayerSize(value); }
        }

        public static bool IsSnapshotSet => FCE_Document_IsSnapshotSet();

        public static Vec3 SnapshotPos
        {
            get { FCE_Document_GetSnapshotPos(out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Document_SetSnapshotPos(value.X, value.Y, value.Z); }
        }

        public static Vec3 SnapshotAngle
        {
            get { FCE_Document_GetSnapshotAngle(out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Document_SetSnapshotAngle(value.X, value.Y, value.Z); }
        }

        public static void Reset()
        {
            FCE_Document_Reset();
            string author = Win32.GetUserNameHelper() ?? Localizer.Localize("PARAM_UNDEFINED") ?? "";
            AuthorName = author;
            string mapName = Localizer.Localize("EDITOR_UNTITLED");
            if (mapName != null)
            {
                MapName = mapName;
            }
        }

        public static bool Load(string fileName, LoadCompletedCallback callback)
        {
            string path = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar;
            string name = Path.GetFileName(fileName);
            byte[] pathBytes = Encoding.UTF8.GetBytes(path);
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            m_loadCompletedCallback = callback;
            return FCE_Document_Load(pathBytes, nameBytes);
        }

        public static void OnLoadCompleted(Editor.ResultCode resultCode)
        {
            bool success = resultCode == Editor.ResultCode.Succeeded;
            if (!success)
            {
                MessageBox.Show(Localizer.Localize("ERROR_LOAD_FAILED"), Localizer.Localize("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
                MainForm.Instance.ClearMapPath();
            }
            m_loadCompletedCallback?.Invoke(success);
        }

        public static void Save(string fileName, SaveCompletedCallback callback)
        {
            string path = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar;
            string name = Path.GetFileName(fileName);
            byte[] pathBytes = Encoding.UTF8.GetBytes(path);
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            m_saveCompletedCallback = callback;
            FCE_Document_Save(pathBytes, nameBytes);
        }

        public static void OnSaveCompleted(Editor.ResultCode resultCode)
        {
            bool success = resultCode == Editor.ResultCode.Succeeded;
            if (!success)
            {
                MessageBox.Show(Localizer.Localize("ERROR_SAVE_FAILED"), Localizer.Localize("ERROR"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            m_saveCompletedCallback?.Invoke(success);
        }

        public static bool Validate() => FCE_Document_Validate();
        public static void ClearSnapshot() => FCE_Document_ClearSnapshot();
        public static void TakeSnapshot(Snapshot snapshot) => FCE_Document_TakeSnapshot(snapshot.Pointer);
        public static void FinalizeMap() => FCE_Document_FinalizeMap();
        public static void Export(string mapFile, string exportPath, bool toConsole) => FCE_Document_Export(mapFile, exportPath, toConsole);

        #region P/Invoke
        [DllImport("Dunia.dll")] private static extern void FCE_Document_Reset();
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Document_Load(byte[] mapPath, byte[] mapName);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_Save(byte[] mapPath, byte[] mapName);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Document_Validate();
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Document_IsSnapshotSet();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Document_GetMapName();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetMapName(string name);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Document_GetCreatorName();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetCreatorName(string name);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Document_GetAuthorName();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetAuthorName(string name);
        [DllImport("Dunia.dll")] private static extern BattlefieldSizes FCE_Document_GetBattlefieldSize();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetBattlefieldSize(BattlefieldSizes size);
        [DllImport("Dunia.dll")] private static extern PlayerSizes FCE_Document_GetPlayerSize();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetPlayerSize(PlayerSizes size);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_ClearSnapshot();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_GetSnapshotPos(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetSnapshotPos(float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_GetSnapshotAngle(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_SetSnapshotAngle(float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_TakeSnapshot(IntPtr snapshot);
        [DllImport("Dunia.dll")] private static extern void FCE_Document_FinalizeMap();
        [DllImport("Dunia.dll")] private static extern void FCE_Document_Export(string mapFile, string exportPath, bool toConsole);
        #endregion
    }
}