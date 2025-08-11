using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct Vec3
    {
        public float X;
        public float Y;
        public float Z;

        public Vec2 XY
        {
            get { return new Vec2(X, Y); }
            set { X = value.X; Y = value.Y; }
        }

        public Vec2 XZ
        {
            get { return new Vec2(X, Z); }
            set { X = value.X; Z = value.Y; }
        }

        public Vec2 YZ
        {
            get { return new Vec2(Y, Z); }
            set { Y = value.X; Z = value.Y; }
        }

        public float LengthSquare => X * X + Y * Y + Z * Z;
        public float Length => (float)Math.Sqrt(LengthSquare);

        public bool IsZero => Math.Abs(X) < 0.001f && Math.Abs(Y) < 0.001f && Math.Abs(Z) < 0.001f;

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vec3 operator +(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vec3 operator -(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vec3 operator -(Vec3 v)
        {
            return new Vec3(0f - v.X, 0f - v.Y, 0f - v.Z);
        }

        public static Vec3 operator *(float s, Vec3 v)
        {
            return new Vec3(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vec3 operator *(Vec3 v, float s)
        {
            return new Vec3(v.X * s, v.Y * s, v.Z * s);
        }

        public static Vec3 operator *(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static Vec3 operator /(Vec3 v, float s)
        {
            return new Vec3(v.X / s, v.Y / s, v.Z / s);
        }

        public static bool operator ==(Vec3 v1, Vec3 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
        }

        public static bool operator !=(Vec3 v1, Vec3 v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vec3))
            {
                return false;
            }
            return this == (Vec3)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static float Dot(Vec3 v1, Vec3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static Vec3 Cross(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        }

        public float Normalize()
        {
            float length = Length;
            if (length > 0)
            {
                this /= length;
            }
            return length;
        }

        public void Snap(float resolution)
        {
            X -= (float)Math.IEEERemainder(X, resolution);
            Y -= (float)Math.IEEERemainder(Y, resolution);
            Z -= (float)Math.IEEERemainder(Z, resolution);
        }

        public void Snap(Vec3 resolutionVector)
        {
            X -= (float)Math.IEEERemainder(X, resolutionVector.X);
            Y -= (float)Math.IEEERemainder(Y, resolutionVector.Y);
            Z -= (float)Math.IEEERemainder(Z, resolutionVector.Z);
        }

        public Vec3 ToAngles()
        {
            Vec3 result = default(Vec3);
            FCE_Core_GetAnglesFromDir(out result.X, out result.Y, out result.Z, X, Y, Z);
            return result;
        }

        public string ToString(string format)
        {
            return "(" + X.ToString(format) + ", " + Y.ToString(format) + ", " + Z.ToString(format) + ")";
        }

        public override string ToString()
        {
            return ToString("F4");
        }

        [DllImport("Dunia.dll")]
        private static extern void FCE_Core_GetAnglesFromDir(out float angleX, out float angleY, out float angleZ, float x, float y, float z);
    }
}