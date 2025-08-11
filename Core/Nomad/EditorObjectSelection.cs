using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct EditorObjectSelection : IDisposable
    {
        public enum MoveMode
        {
            MoveNormal,
            MoveKeepHeight,
            MoveSnapToTerrain,
            MoveKeepAboveTerrain
        }

        public static EditorObject Null = new EditorObject(IntPtr.Zero);

        private IntPtr m_selPtr;

        public IntPtr Pointer => m_selPtr;
        public int Count => FCE_ObjectSelection_GetCount(m_selPtr);
        public EditorObject this[int index] => new EditorObject(FCE_ObjectSelection_Get(m_selPtr, index));

        public Vec3 Center
        {
            get { FCE_ObjectSelection_GetCenter(m_selPtr, out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_ObjectSelection_SetCenter(m_selPtr, value.X, value.Y, value.Z); }
        }

        public AABB WorldBounds
        {
            get { FCE_ObjectSelection_GetWorldBounds(m_selPtr, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2); return new AABB(new Vec3(x1, y1, z1), new Vec3(x2, y2, z2)); }
        }

        public EditorObjectSelection(IntPtr ptr)
        {
            m_selPtr = ptr;
        }

        public static EditorObjectSelection Create()
        {
            return new EditorObjectSelection(FCE_ObjectSelection_Create());
        }

        public void Dispose()
        {
            FCE_ObjectSelection_Destroy(m_selPtr);
        }

        public IEnumerable<EditorObject> GetObjects()
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
        }

        public void Clear()
        {
            FCE_ObjectSelection_Clear(m_selPtr);
        }

        public void AddObject(EditorObject obj)
        {
            FCE_ObjectSelection_Add(m_selPtr, obj.Pointer);
        }

        public void AddSelection(EditorObjectSelection selection)
        {
            FCE_ObjectSelection_AddSelection(m_selPtr, selection.Pointer);
        }

        public void GetValidObjects(EditorObjectSelection selection)
        {
            FCE_ObjectSelection_GetValidObjects(m_selPtr, selection.Pointer);
        }

        public void RemoveInvalidObjects()
        {
            FCE_ObjectSelection_RemoveInvalidObjects(m_selPtr);
        }

        public void Clone(EditorObjectSelection newSelection, bool cloneObjects)
        {
            FCE_ObjectSelection_Clone(m_selPtr, newSelection.Pointer, cloneObjects);
        }

        public void Delete()
        {
            FCE_ObjectSelection_Delete(m_selPtr);
        }

        public void ToggleObject(EditorObject obj)
        {
            FCE_ObjectSelection_ToggleObject(m_selPtr, obj.Pointer);
        }

        public void ToggleSelection(EditorObjectSelection selection)
        {
            FCE_ObjectSelection_ToggleSelection(m_selPtr, selection.Pointer);
        }

        public int IndexOf(EditorObject obj)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Pointer == obj.Pointer)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool Contains(EditorObject obj)
        {
            return IndexOf(obj) != -1;
        }

        public Vec3 GetComputeCenter()
        {
            FCE_ObjectSelection_GetComputeCenter(m_selPtr, out float x, out float y, out float z);
            return new Vec3(x, y, z);
        }

        public void ComputeCenter()
        {
            FCE_ObjectSelection_ComputeCenter(m_selPtr);
        }

        public void MoveTo(Vec3 pos, MoveMode mode)
        {
            FCE_ObjectSelection_MoveTo(m_selPtr, pos.X, pos.Y, pos.Z, mode);
        }

        public void Rotate(float angle, Vec3 axis, Vec3 pivot, bool affectCenter)
        {
            FCE_ObjectSelection_Rotate(m_selPtr, angle, axis.X, axis.Y, axis.Z, pivot.X, pivot.Y, pivot.Z, affectCenter);
        }

        public void Rotate(Vec3 angles, Vec3 axis, Vec3 pivot, bool affectCenter)
        {
            FCE_ObjectSelection_Rotate3(m_selPtr, angles.X, angles.Y, angles.Z, axis.X, axis.Y, axis.Z, pivot.X, pivot.Y, pivot.Z, affectCenter);
        }

        public void RotateCenter(float angle, Vec3 axis)
        {
            FCE_ObjectSelection_RotateCenter(m_selPtr, angle, axis.X, axis.Y, axis.Z);
        }

        public void RotateLocal(Vec3 angles)
        {
            FCE_ObjectSelection_RotateLocal3(m_selPtr, angles.X, angles.Y, angles.Z);
        }

        public void RotateGimbal(Vec3 angles)
        {
            FCE_ObjectSelection_RotateGimbal(m_selPtr, angles.X, angles.Y, angles.Z);
        }

        public void SetPos(Vec3 pos)
        {
            foreach (EditorObject @object in GetObjects())
            {
                EditorObject editorObject = @object;
                editorObject.Position = pos;
            }
        }

        public void SetAngles(Vec3 angles)
        {
            foreach (EditorObject @object in GetObjects())
            {
                EditorObject editorObject = @object;
                editorObject.Angles = angles;
            }
        }

        public void DropToGround(bool physics, bool group)
        {
            FCE_ObjectSelection_DropToGround(m_selPtr, physics, group);
        }

        public void SnapToPivot(EditorObjectPivot source, EditorObjectPivot target, bool preserveOrientation, float snapAngle)
        {
            FCE_ObjectSelection_SnapToPivot(m_selPtr, source.position.X, source.position.Y, source.position.Z, source.normal.X, source.normal.Y, source.normal.Z, source.normalUp.X, source.normalUp.Y, source.normalUp.Z, target.position.X, target.position.Y, target.position.Z, target.normal.X, target.normal.Y, target.normal.Z, target.normalUp.X, target.normalUp.Y, target.normalUp.Z, preserveOrientation, snapAngle);
        }

        public void SnapToClosestObjects()
        {
            FCE_ObjectSelection_SnapToClosestObjects(m_selPtr);
        }

        public void GetPhysEntities(PhysEntityVector vector)
        {
            FCE_ObjectSelection_GetPhysEntities(m_selPtr, vector.Pointer);
        }

        public void ClearState()
        {
            FCE_ObjectSelection_ClearState(m_selPtr);
        }

        public void LoadState()
        {
            FCE_ObjectSelection_LoadState(m_selPtr);
        }

        public void SaveState()
        {
            FCE_ObjectSelection_SaveState(m_selPtr);
        }

        #region P/Invoke
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ObjectSelection_Create();
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Destroy(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern int FCE_ObjectSelection_GetCount(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ObjectSelection_Get(IntPtr ptr, int index);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Clear(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Add(IntPtr ptr, IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_AddSelection(IntPtr ptr, IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Clone(IntPtr ptr, IntPtr selection, [MarshalAs(UnmanagedType.U1)] bool cloneObjects);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Delete(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_ToggleObject(IntPtr ptr, IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_ToggleSelection(IntPtr ptr, IntPtr selection);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_GetValidObjects(IntPtr ptr, IntPtr selection);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_RemoveInvalidObjects(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_GetCenter(IntPtr ptr, out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_SetCenter(IntPtr ptr, float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_GetComputeCenter(IntPtr ptr, out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_ComputeCenter(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_GetWorldBounds(IntPtr ptr, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_MoveTo(IntPtr ptr, float x, float y, float z, MoveMode mode);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Rotate(IntPtr ptr, float angle, float axisX, float axisY, float axisZ, float pivotX, float pivotY, float pivotZ, [MarshalAs(UnmanagedType.U1)] bool affectCenter);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_Rotate3(IntPtr ptr, float angleX, float angleY, float angleZ, float axisX, float axisY, float axisZ, float pivotX, float pivotY, float pivotZ, [MarshalAs(UnmanagedType.U1)] bool affectCenter);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_RotateCenter(IntPtr ptr, float angle, float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_RotateLocal3(IntPtr ptr, float angleX, float angleY, float angleZ);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_RotateGimbal(IntPtr ptr, float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_DropToGround(IntPtr ptr, [MarshalAs(UnmanagedType.U1)] bool physics, [MarshalAs(UnmanagedType.U1)] bool group);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_SnapToPivot(IntPtr ptr, float sourcePosX, float sourcePosY, float sourcePosZ, float sourceNormX, float sourceNormY, float sourceNormZ, float sourceNormUpX, float sourceNormUpY, float sourceNormUpZ, float targetPosX, float targetPosY, float targetPosZ, float targetNormX, float targetNormY, float targetNormZ, float targetNormUpX, float targetNormUpY, float targetNormUpZ, [MarshalAs(UnmanagedType.U1)] bool preserveOrientation, float snapAngle);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_SnapToClosestObjects(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_GetPhysEntities(IntPtr ptr, IntPtr vector);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_ClearState(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_LoadState(IntPtr ptr);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectSelection_SaveState(IntPtr ptr);
        #endregion
    }
}