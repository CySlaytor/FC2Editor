using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct CoordinateSystem
    {
        public static CoordinateSystem Standard = new CoordinateSystem(new Vec3(1f, 0f, 0f), new Vec3(0f, 1f, 0f), new Vec3(0f, 0f, 1f));

        public Vec3 axisX;
        public Vec3 axisY;
        public Vec3 axisZ;

        public CoordinateSystem(Vec3 x, Vec3 y, Vec3 z)
        {
            axisX = x;
            axisY = y;
            axisZ = z;
        }

        public static CoordinateSystem FromAngles(Vec3 angles)
        {
            CoordinateSystem result = default(CoordinateSystem);
            FCE_Core_GetAxisFromAngles(angles.X, angles.Y, angles.Z, out result.axisX.X, out result.axisX.Y, out result.axisX.Z, out result.axisY.X, out result.axisY.Y, out result.axisY.Z, out result.axisZ.X, out result.axisZ.Y, out result.axisZ.Z);
            return result;
        }

        public Vec3 ToAngles()
        {
            Vec3 result = default(Vec3);
            FCE_Core_GetAnglesFromAxis(out result.X, out result.Y, out result.Z, axisX.X, axisX.Y, axisX.Z, axisY.X, axisY.Y, axisY.Z, axisZ.X, axisZ.Y, axisZ.Z);
            return result;
        }

        public Vec3 ConvertFromWorld(Vec3 pos)
        {
            return new Vec3(Vec3.Dot(pos, axisX), Vec3.Dot(pos, axisY), Vec3.Dot(pos, axisZ));
        }

        public Vec3 ConvertToWorld(Vec3 pos)
        {
            return pos.X * axisX + pos.Y * axisY + pos.Z * axisZ;
        }

        public Vec3 ConvertFromSystem(Vec3 pos, CoordinateSystem coords)
        {
            Vec3 pos2 = coords.ConvertToWorld(pos);
            return ConvertFromWorld(pos2);
        }

        public Vec3 ConvertToSystem(Vec3 pos, CoordinateSystem coords)
        {
            Vec3 pos2 = ConvertToWorld(pos);
            return coords.ConvertFromWorld(pos2);
        }

        public Vec3 GetPivotPoint(Vec3 center, AABB bounds, Pivot pivot)
        {
            Vec3 result = center;
            switch (pivot)
            {
                case Pivot.Left: result += axisX * bounds.min.X; break;
                case Pivot.Right: result += axisX * bounds.max.X; break;
                case Pivot.Down: result += axisY * bounds.min.Y; break;
                case Pivot.Up: result += axisY * bounds.max.Y; break;
            }
            return result;
        }

        [DllImport("Dunia.dll")] private static extern void FCE_Core_GetAxisFromAngles(float angleX, float angleY, float angleZ, out float x1, out float y1, out float z1, out float x2, out float y2, out float z2, out float x3, out float y3, out float z3);
        [DllImport("Dunia.dll")] private static extern void FCE_Core_GetAnglesFromAxis(out float angleX, out float angleY, out float angleZ, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3);
    }
}