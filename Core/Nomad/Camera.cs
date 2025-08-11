using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class Camera
    {
        public static float ForwardInput { set => FCE_Camera_Input_Forward(value); }
        public static float LateralInput { set => FCE_Camera_Input_Lateral(value); }

        public static Vec3 Position
        {
            get { FCE_Camera_GetPos(out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Camera_SetPos(value.X, value.Y, value.Z); }
        }

        public static Vec3 Angles
        {
            get { FCE_Camera_GetAngles(out float x, out float y, out float z); return new Vec3(x, y, z); }
            set { FCE_Camera_SetAngles(value.X, value.Y, value.Z); }
        }

        public static Vec3 FrontVector
        {
            get { FCE_Camera_GetFrontVector(out float x, out float y, out float z); return new Vec3(x, y, z); }
        }

        public static Vec3 RightVector
        {
            get { FCE_Camera_GetRightVector(out float x, out float y, out float z); return new Vec3(x, y, z); }
        }

        public static Vec3 UpVector
        {
            get { FCE_Camera_GetUpVector(out float x, out float y, out float z); return new Vec3(x, y, z); }
        }

        public static CoordinateSystem Axis => new CoordinateSystem(RightVector, FrontVector, UpVector);

        public static float Speed
        {
            get { return FCE_Camera_GetSpeed(); }
            set { FCE_Camera_SetSpeed(value); }
        }

        public static float SpeedFactor { set => FCE_Camera_SetSpeedFactor(value); }

        public static float FOV => FCE_Camera_GetFOV();
        public static float HalfFOV => FOV * 0.5f;

        public static void Rotate(float pitch, float roll, float yaw)
        {
            FCE_Camera_Rotate(pitch, roll, yaw);
        }

        public static void Focus(EditorObject obj)
        {
            if (obj.IsValid)
            {
                AABB worldBounds = obj.WorldBounds;
                Vec3 center = worldBounds.Center;
                Vec3 vec = (worldBounds - center).Length * 0.5f;
                Vec3 vec2 = -FrontVector;
                Position = center + vec2 * (vec2 * vec).Length * 4f;
            }
        }

        [DllImport("Dunia.dll")] private static extern void FCE_Camera_Input_Forward(float input);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_Input_Lateral(float input);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_GetPos(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_SetPos(float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_GetAngles(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_SetAngles(float x, float y, float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_Rotate(float pitch, float roll, float yaw);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_GetFrontVector(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_GetRightVector(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_GetUpVector(out float x, out float y, out float z);
        [DllImport("Dunia.dll")] private static extern float FCE_Camera_GetSpeed();
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_SetSpeed(float speed);
        [DllImport("Dunia.dll")] private static extern void FCE_Camera_SetSpeedFactor(float input);
        [DllImport("Dunia.dll")] private static extern float FCE_Camera_GetFOV();
    }
}