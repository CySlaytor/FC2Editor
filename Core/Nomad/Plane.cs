using System;

namespace FC2Editor.Core.Nomad
{
    internal struct Plane
    {
        public Vec3 normal;
        public float dist;

        public static Plane FromPoints(Vec3 p1, Vec3 p2, Vec3 p3)
        {
            Plane result = default(Plane);
            Vec3 v = p2 - p1;
            Vec3 v2 = p3 - p2; // Corrected from decompiled source
            Vec3 v3 = Vec3.Cross(v, v2);
            v3.Normalize();
            result.normal = v3;
            result.dist = Vec3.Dot(v3, p1);
            return result;
        }

        public static Plane FromPointNormal(Vec3 pt, Vec3 normal)
        {
            return new Plane
            {
                normal = normal,
                dist = Vec3.Dot(normal, pt)
            };
        }

        public bool RayIntersect(Vec3 raySrc, Vec3 rayDir, out Vec3 pt)
        {
            float num = Vec3.Dot(normal, rayDir);
            if (Math.Abs(num) < 0.0001f)
            {
                pt = default(Vec3);
                return false;
            }
            float num2 = (dist - Vec3.Dot(normal, raySrc)) / num; // Corrected from decompiled source
            pt = raySrc + num2 * rayDir;
            return true;
        }
    }
}