using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct EditorObject
    {
        public static EditorObject Null = new EditorObject(IntPtr.Zero);

        private IntPtr m_objPtr;

        public bool IsValid => Pointer != IntPtr.Zero;
        public IntPtr Pointer => m_objPtr;
        public bool IsLoaded => FCE_Object_IsLoaded(m_objPtr);
        public ObjectInventory.Entry Entry => new ObjectInventory.Entry(FCE_Object_GetEntry(m_objPtr));

        public Vec3 Position
        {
            get { FCE_Object_GetPos(m_objPtr, out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Object_SetPos(m_objPtr, value.X, value.Y, value.Z); }
        }

        public Vec3 Angles
        {
            get { FCE_Object_GetAngles(m_objPtr, out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Object_SetAngles(m_objPtr, value.X, value.Y, value.Z); }
        }

        public CoordinateSystem Axis => CoordinateSystem.FromAngles(Angles);

        public AABB LocalBounds
        {
            get { FCE_Object_GetBounds(m_objPtr, false, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2); return new AABB(new Vec3(x1, y1, z1), new Vec3(x2, y2, z2)); }
        }

        public AABB WorldBounds
        {
            get { FCE_Object_GetBounds(m_objPtr, true, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2); return new AABB(new Vec3(x1, y1, z1), new Vec3(x2, y2, z2)); }
        }

        public bool Visible
        {
            get { return FCE_Object_IsVisible(m_objPtr); }
            set { FCE_Object_SetVisible(m_objPtr, value); }
        }

        public bool HighlightState { set => FCE_Object_SetHighlight(m_objPtr, value); }
        public bool Frozen { set => FCE_Object_SetFreeze(m_objPtr, value); }

        public EditorObject(IntPtr objPtr)
        {
            m_objPtr = objPtr;
        }

        public static EditorObject CreateFromEntry(ObjectInventory.Entry entry, bool managed)
        {
            return new EditorObject(FCE_Object_Create_FromEntry(entry.Pointer, managed));
        }

        public void Acquire()
        {
            FCE_Object_AddRef(m_objPtr);
        }

        public void Release()
        {
            FCE_Object_Release(m_objPtr);
        }

        public void Destroy()
        {
            FCE_Object_Destroy(m_objPtr);
            m_objPtr = IntPtr.Zero;
        }

        public EditorObject Clone()
        {
            return new EditorObject(FCE_Object_Clone(m_objPtr));
        }

        public Vec3 GetPivotPoint(Pivot pivot)
        {
            AABB bounds = IsLoaded ? LocalBounds : default(AABB);
            return Axis.GetPivotPoint(Position, bounds, pivot);
        }

        public void DropToGround(bool physics)
        {
            FCE_Object_DropToGround(m_objPtr, physics);
        }

        public void ComputeAutoOrientation(ref Vec3 pos, out Vec3 angles, Vec3 normal)
        {
            angles = default(Vec3);
            FCE_Object_ComputeAutoOrientation(m_objPtr, ref pos.X, ref pos.Y, ref pos.Z, out angles.X, out angles.Y, out angles.Z, normal.X, normal.Y, normal.Z);
        }

        public bool GetPivot(int idx, out EditorObjectPivot pivot)
        {
            pivot = new EditorObjectPivot();
            return FCE_Object_GetPivot(m_objPtr, idx, out pivot.position.X, out pivot.position.Y, out pivot.position.Z, out pivot.normal.X, out pivot.normal.Y, out pivot.normal.Z, out pivot.normalUp.X, out pivot.normalUp.Y, out pivot.normalUp.Z);
        }

        public bool GetClosestPivot(Vec3 pos, out EditorObjectPivot pivot)
        {
            return GetClosestPivot(pos, out pivot, float.MaxValue);
        }

        public bool GetClosestPivot(Vec3 pos, out EditorObjectPivot pivot, float minDist)
        {
            pivot = new EditorObjectPivot();
            return FCE_Object_GetClosestPivot(m_objPtr, pos.X, pos.Y, pos.Z, out pivot.position.X, out pivot.position.Y, out pivot.position.Z, out pivot.normal.X, out pivot.normal.Y, out pivot.normal.Z, out pivot.normalUp.X, out pivot.normalUp.Y, out pivot.normalUp.Z, minDist);
        }

        public void SnapToClosestObject()
        {
            FCE_Object_SnapToClosestObject(m_objPtr);
        }

        public void GetPhysEntities(PhysEntityVector vector)
        {
            FCE_Object_GetPhysEntities(m_objPtr, vector.Pointer);
        }

        #region P/Invoke
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Object_Create_FromEntry(IntPtr entry, [MarshalAs(UnmanagedType.U1)] bool managed);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_Destroy(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_AddRef(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_Release(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Object_Clone(IntPtr obj);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Object_IsLoaded(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Object_GetEntry(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_GetPos(IntPtr obj, out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_SetPos(IntPtr obj, float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_GetAngles(IntPtr obj, out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_SetAngles(IntPtr obj, float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_GetBounds(IntPtr obj, [MarshalAs(UnmanagedType.U1)] bool world, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Object_IsVisible(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_SetVisible(IntPtr obj, [MarshalAs(UnmanagedType.U1)] bool visible);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_SetHighlight(IntPtr obj, [MarshalAs(UnmanagedType.U1)] bool highlight);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_SetFreeze(IntPtr obj, [MarshalAs(UnmanagedType.U1)] bool freeze);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_DropToGround(IntPtr obj, [MarshalAs(UnmanagedType.U1)] bool physics);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_ComputeAutoOrientation(IntPtr obj, ref float x, ref float y, ref float z, out float angleX, out float angleY, out float angleZ, float normX, float normY, float normZ);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Object_GetPivot(IntPtr obj, int idx, out float x, out float y, out float z, out float normX, out float normY, out float normZ, out float normUpX, out float normUpY, out float normUpZ);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Object_GetClosestPivot(IntPtr obj, float posX, float posY, float posZ, out float pivotX, out float pivotY, out float pivotZ, out float normX, out float normY, out float normZ, out float normUpX, out float normUpY, out float normUpZ, float minDist);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_SnapToClosestObject(IntPtr obj);
        [DllImport("Dunia.dll")] private static extern void FCE_Object_GetPhysEntities(IntPtr obj, IntPtr vector);
        #endregion
    }
}