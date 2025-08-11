namespace FC2Editor.Core.Nomad
{
    internal struct AABB
    {
        public Vec3 min;
        public Vec3 max;

        public Vec3 Length => max - min;
        public Vec3 Center => (max + min) * 0.5f;

        public AABB(Vec3 min, Vec3 max)
        {
            this.min = min;
            this.max = max;
        }

        public static AABB operator -(AABB a, Vec3 b)
        {
            return new AABB(a.min - b, a.max - b);
        }

        public override string ToString()
        {
            Vec3 length = Length;
            return length.X.ToString("F1") + " x " + length.Y.ToString("F1") + " x " + length.Z.ToString("F1") + " m";
        }
    }
}