using System.Drawing;

namespace FC2Editor.Utils
{
    internal class ColorUtils
    {
        public static void RGBToHSL(Color c, out float h, out float s, out float l)
        {
            float num = c.R / 255f;
            float num2 = c.G / 255f;
            float num3 = c.B / 255f;
            float num4 = num;
            float num5 = num;
            if (num2 < num4)
            {
                num4 = num2;
            }
            else if (num2 > num5)
            {
                num5 = num2;
            }
            if (num3 < num4)
            {
                num4 = num3;
            }
            else
            {
                num5 = num3;
            }
            if (num5 == num4)
            {
                h = 0f;
                s = 0f;
                l = num5;
                return;
            }
            if (num5 == num)
            {
                if (num2 >= num3)
                {
                    h = 0.1666f * (num2 - num3) / (num5 - num4);
                }
                else
                {
                    h = 0.1666f * (num2 - num3) / (num5 - num4) + 1f;
                }
            }
            else if (num5 == num2)
            {
                h = 0.1666f * (num3 - num) / (num5 - num4) + 0.3333f;
            }
            else
            {
                h = 0.1666f * (num - num2) / (num5 - num4) + 0.6666f;
            }
            l = (num5 + num4) * 0.5f;
            if (l <= 0.5f)
            {
                s = (num5 - num4) / (2f * l);
            }
            else
            {
                s = (num5 - num4) / (2f - 2f * l);
            }
        }

        public static Color HSLToRGB(float h, float s, float l)
        {
            float num = ((l < 0.5f) ? (l * (1f + s)) : (l + s - l * s));
            float num2 = 2f * l - num;
            float num3 = h + 0.3333f;
            float num4 = h;
            float num5 = h - 0.3333f;
            if (num3 > 1f)
            {
                num3 -= 1f;
            }
            if (num5 < 0f)
            {
                num5 += 1f;
            }
            num3 = ((num3 < 0.1666f) ? (num2 + (num - num2) * 6f * num3) : ((num3 < 0.5f) ? num : ((!(num3 < 0.6666f)) ? num2 : (num2 + (num - num2) * (0.6666f - num3) * 6f))));
            num4 = ((num4 < 0.1666f) ? (num2 + (num - num2) * 6f * num4) : ((num4 < 0.5f) ? num : ((!(num4 < 0.6666f)) ? num2 : (num2 + (num - num2) * (0.6666f - num4) * 6f))));
            num5 = ((num5 < 0.1666f) ? (num2 + (num - num2) * 6f * num5) : ((num5 < 0.5f) ? num : ((!(num5 < 0.6666f)) ? num2 : (num2 + (num - num2) * (0.6666f - num5) * 6f))));
            int red = (int)(num3 * 255f);
            int green = (int)(num4 * 255f);
            int blue = (int)(num5 * 255f);
            return Color.FromArgb(red, green, blue);
        }
    }
}