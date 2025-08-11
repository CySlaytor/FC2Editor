using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using FC2Editor.Utils;

namespace FC2Editor.UI
{
    internal class ButtonShader
    {
        private Color[] colorStart;
        private Color[] colorEnd;
        private ColorBlend colorMiddle;
        private Color colorEdge;
        private int border;

        public static bool shadersInit;
        public static ButtonShader normalShader;
        public static ButtonShader hoverShader;
        public static ButtonShader pushShader;
        public static ButtonShader disableShader;
        public static ButtonShader checkShader;

        public ButtonShader(float hue, float saturation, float start, float start2, float end2, float end, float edge, int border)
        {
            ComputeColors(hue, saturation, start, start2, end2, end, edge, border);
        }

        public void ComputeColors(float hue, float saturation, float start, float start2, float end2, float end, float edge, int border)
        {
            this.border = border;
            colorStart = new Color[border];
            colorEnd = new Color[border];
            float num = (start2 - start) / (float)border;
            float num2 = (end2 - end) / (float)border;
            float num3 = start;
            float num4 = end;
            for (int i = 0; i < border; i++)
            {
                colorStart[i] = ColorUtils.HSLToRGB(hue, saturation, num3);
                colorEnd[i] = ColorUtils.HSLToRGB(hue, saturation, num4);
                num3 += num;
                num4 += num2;
            }
            colorEdge = ColorUtils.HSLToRGB(hue, saturation, edge);
            float num5 = (end2 - start2) / 9f;
            float num6 = start2;
            colorMiddle = new ColorBlend(10);
            for (int j = 0; j < 10; j++)
            {
                colorMiddle.Positions[j] = (float)j / 9f;
                colorMiddle.Colors[j] = ColorUtils.HSLToRGB(hue, saturation, num6);
                num6 += num5;
            }
        }

        public void DrawButton(Graphics g, Rectangle rect, Color backColor)
        {
            Rectangle rect2 = rect;
            using (Pen pen = new Pen(colorEdge))
            {
                g.DrawRectangle(pen, rect2.X, rect2.Y, rect2.Width - 1, rect2.Height - 1);
                rect2.Inflate(-1, -1);
                for (int i = 0; i < border; i++)
                {
                    using (Pen pen2 = new Pen(colorStart[i]))
                    {
                        g.DrawLine(pen2, rect2.X + i, rect2.Y + i, rect2.Right - 1 - i, rect2.Y + i);
                        g.DrawLine(pen2, rect2.X + i, rect2.Y + i, rect2.X + i, rect2.Bottom - 1 - i);
                    }
                    using (Pen pen3 = new Pen(colorEnd[i]))
                    {
                        g.DrawLine(pen3, rect2.Right - 1 - i, rect2.Y + i, rect2.Right - 1 - i, rect2.Bottom - 1 - i);
                        g.DrawLine(pen3, rect2.X + i, rect2.Bottom - 1 - i, rect2.Right - 1 - i, rect2.Bottom - 1 - i);
                    }
                }
            }
            rect2.Inflate(-border, -border);
            using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect2, Color.Black, Color.White, 90f))
            {
                linearGradientBrush.InterpolationColors = colorMiddle;
                g.FillRectangle(linearGradientBrush, rect2);
            }
        }

        public void DrawText(Graphics g, Rectangle rect, string text, Font font, Color color)
        {
            using (SolidBrush brush = new SolidBrush(color))
            {
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    HotkeyPrefix = HotkeyPrefix.Show
                };
                Rectangle rectangle = rect;
                rectangle.Inflate(-border - 2, -border - 2);
                g.DrawString(text, font, brush, rectangle, stringFormat);
            }
        }

        public static void InitShaders()
        {
            if (!shadersInit)
            {
                ColorUtils.RGBToHSL(SystemColors.ControlLight, out float h, out float s, out _);
                normalShader = new ButtonShader(h, s, 1f, 1f, 0.72f, 0.63f, 0.33f, 1);
                hoverShader = new ButtonShader(h, s, 1f, 1f, 0.87f, 0.72f, 0.33f, 1);
                pushShader = new ButtonShader(h, s, 0.5f, 0.65f, 0.95f, 1f, 0.33f, 0);
                disableShader = new ButtonShader(h, s, 0.97f, 0.93f, 0.9f, 0.78f, 0.83f, 0);
                ColorUtils.RGBToHSL(SystemColors.Highlight, out h, out s, out _);
                checkShader = new ButtonShader(h, 0.75f, 0.8f, 0.7f, 0.95f, 1f, 0.24f, 0);
                shadersInit = true;
            }
        }
    }
}