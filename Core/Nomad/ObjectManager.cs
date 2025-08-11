using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal static class ObjectManager
    {
        public static EditorObject GetObjectFromScreenPoint(Vec2 pt, out Vec3 hitPos)
        {
            return GetObjectFromScreenPoint(pt, out hitPos, false, EditorObject.Null);
        }

        public static EditorObject GetObjectFromScreenPoint(Vec2 pt, out Vec3 hitPos, bool includeFrozen)
        {
            return GetObjectFromScreenPoint(pt, out hitPos, includeFrozen, EditorObject.Null);
        }

        public static EditorObject GetObjectFromScreenPoint(Vec2 pt, out Vec3 hitPos, bool includeFrozen, EditorObject ignore)
        {
            PhysEntityVector vector = PhysEntityVector.Null;
            if (ignore.IsValid)
            {
                vector = PhysEntityVector.Create();
                ignore.GetPhysEntities(vector);
            }

            EditorObject result = new EditorObject(FCE_ObjectManager_GetObjectFromScreenPoint(pt.X, pt.Y, out hitPos.X, out hitPos.Y, out hitPos.Z, includeFrozen, vector.Pointer));
            if (vector.IsValid)
            {
                vector.Dispose();
            }
            return result;
        }

        public static EditorObject GetObjectFromScreenPoint(Vec2 pt, out Vec3 hitPos, bool includeFrozen, EditorObjectSelection ignore)
        {
            using (PhysEntityVector vector = PhysEntityVector.Create())
            {
                ignore.GetPhysEntities(vector);
                return new EditorObject(FCE_ObjectManager_GetObjectFromScreenPoint(pt.X, pt.Y, out hitPos.X, out hitPos.Y, out hitPos.Z, includeFrozen, vector.Pointer));
            }
        }

        public static void GetObjectsFromScreenRect(EditorObjectSelection selection, RectangleF rect)
        {
            GetObjectsFromScreenRect(selection, rect, false);
        }

        public static void GetObjectsFromScreenRect(EditorObjectSelection selection, RectangleF rect, bool includeFrozen)
        {
            FCE_ObjectManager_GetObjectsFromScreenRect(selection.Pointer, rect.Left, rect.Top, rect.Right, rect.Bottom, includeFrozen);
        }

        public static void GetObjectsFromMagicWand(EditorObjectSelection selection, EditorObject obj)
        {
            FCE_ObjectManager_GetObjectsFromMagicWand(selection.Pointer, obj.Pointer);
        }

        public static void UnfreezeObjects()
        {
            FCE_ObjectManager_UnfreezeObjects();
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ObjectManager_GetObjectFromScreenPoint(float x, float y, out float hitX, out float hitY, out float hitZ, [MarshalAs(UnmanagedType.U1)] bool includeFrozen, IntPtr ignore);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectManager_GetObjectsFromScreenRect(IntPtr selection, float x1, float y1, float x2, float y2, [MarshalAs(UnmanagedType.U1)] bool includeFrozen);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectManager_GetObjectsFromMagicWand(IntPtr selection, IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectManager_UnfreezeObjects();
    }
}