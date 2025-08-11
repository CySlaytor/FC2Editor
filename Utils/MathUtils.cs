using System;

namespace FC2Editor.Utils
{
    internal static class MathUtils
    {
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }

        public static float Deg2Rad(float angleDeg)
        {
            return angleDeg * 2f * (float)Math.PI / 360f;
        }

        public static float Rad2Deg(float angleRad)
        {
            return angleRad * 360f / ((float)Math.PI * 2f);
        }
    }
}