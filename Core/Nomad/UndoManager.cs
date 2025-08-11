using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class UndoManager
    {
        public static int UndoCount => FCE_UndoManager_GetUndoCount();
        public static int RedoCount => FCE_UndoManager_GetRedoCount();

        public static void Undo() => FCE_UndoManager_Undo();
        public static void Redo() => FCE_UndoManager_Redo();
        public static void RecordUndo() => FCE_UndoManager_RecordUndo();
        public static void CommitUndo() => FCE_UndoManager_CommitUndo();

        [DllImport("Dunia.dll")] private static extern int FCE_UndoManager_GetUndoCount();
        [DllImport("Dunia.dll")] private static extern int FCE_UndoManager_GetRedoCount();
        [DllImport("Dunia.dll")] private static extern void FCE_UndoManager_Undo();
        [DllImport("Dunia.dll")] private static extern void FCE_UndoManager_Redo();
        [DllImport("Dunia.dll")] private static extern void FCE_UndoManager_RecordUndo();
        [DllImport("Dunia.dll")] private static extern void FCE_UndoManager_CommitUndo();
    }
}