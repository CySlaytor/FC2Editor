namespace FC2Editor.Core.Nomad
{
    internal class EditorObjectPivot
    {
        public Vec3 position;
        public Vec3 normal;
        public Vec3 normalUp;

        public void Unapply(EditorObject obj)
        {
            CoordinateSystem coordinateSystem = CoordinateSystem.FromAngles(obj.Angles);
            AABB localBounds = obj.LocalBounds;
            Vec3 vec = (localBounds.max + localBounds.min) * 0.5f;
            Vec3 vec2 = localBounds.Length * 0.5f;

            position -= obj.Position + vec.X * coordinateSystem.axisX + vec.Y * coordinateSystem.axisY;
            position = coordinateSystem.ConvertFromWorld(position);
            normal = coordinateSystem.ConvertFromWorld(normal);
            normalUp = coordinateSystem.ConvertFromWorld(normalUp);

            if (vec2.X != 0) position.X /= vec2.X;
            if (vec2.Y != 0) position.Y /= vec2.Y;
            if (vec2.Z != 0) position.Z /= vec2.Z;

            if (position.X > 1f) position.X = 1f;
            else if (position.X < -1f) position.X = -1f;

            if (position.Y > 1f) position.Y = 1f;
            else if (position.Y < -1f) position.Y = -1f;

            if (position.Z > 1f) position.Z = 1f;
            else if (position.Z < -1f) position.Z = -1f;

            normal.Z = 0f;
            normalUp = new Vec3(0f, 0f, 1f);
        }
    }
}