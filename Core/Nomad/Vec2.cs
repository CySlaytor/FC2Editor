using System;

namespace FC2Editor.Core.Nomad
{
    internal struct Vec2
    {
        public static Vec2 Zero = new Vec2(0f, 0f);

        public float X;
        public float Y;

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 operator +(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vec2 operator -(Vec2 v)
        {
            return new Vec2(0f - v.X, 0f - v.Y);
        }

        public static Vec2 operator -(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vec2 operator *(float s, Vec2 v)
        {
            return new Vec2(v.X * s, v.Y * s);
        }

        public static Vec2 operator *(Vec2 v, float s)
        {
            return new Vec2(v.X * s, v.Y * s);
        }

        public static Vec2 operator *(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static Vec2 operator /(Vec2 v, float s)
        {
            return new Vec2(v.X / s, v.Y / s);
        }

        public static Vec2 operator /(Vec2 v1, Vec2 v2)
        {
            return new Vec2(v1.X / v2.X, v1.Y / v2.Y);
        }

        public static bool operator ==(Vec2 v1, Vec2 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static bool operator !=(Vec2 v1, Vec2 v2)
        {
            return !(v1 == v2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vec2))
            {
                return false;
            }
            return this == (Vec2)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static float Dot(Vec2 v1, Vec2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
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

        public void Rotate90CCW()
        {
            float x = X;
            X = 0f - Y;
            Y = x;
        }

        public void Rotate90CW()
        {
            float x = X;
            X = Y;
            Y = 0f - x;
        }

        public override string ToString()
        {
            return "(" + X.ToString("F4") + ", " + Y.ToString("F4") + ")";
        }
    }
}