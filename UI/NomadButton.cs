using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    internal class NomadButton : Button
    {
        private bool mouseOver;
        private bool pushed;

        public static Color disableText = Color.FromArgb(180, 180, 180);
        public static ColorMatrix disableMatrix = new ColorMatrix(new float[][]
        {
            new float[] { 0.3f, 0.3f, 0.3f, 0f, 0f },
            new float[] { 0.59f, 0.59f, 0.59f, 0f, 0f },
            new float[] { 0.11f, 0.11f, 0.11f, 0f, 0f },
            new float[] { 0f, 0f, 0f, 0.33f, 0f },
            new float[] { 0f, 0f, 0f, 0f, 1f }
        });

        public NomadButton()
        {
            ButtonShader.InitShaders();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouseOver = true;
            Refresh();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouseOver = false;
            Refresh();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            pushed = true;
            Refresh();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            pushed = false;
            Refresh();
        }

        protected override void OnKeyDown(KeyEventArgs kevent)
        {
            base.OnKeyDown(kevent);
            if (kevent.KeyCode == Keys.Space)
            {
                pushed = true;
                Refresh();
            }
        }

        protected override void OnKeyUp(KeyEventArgs kevent)
        {
            base.OnKeyUp(kevent);
            if (kevent.KeyCode == Keys.Space)
            {
                pushed = false;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics graphics = pevent.Graphics;
            ButtonShader buttonShader = ButtonShader.normalShader;
            if (mouseOver)
            {
                buttonShader = ButtonShader.hoverShader;
            }
            if (pushed)
            {
                buttonShader = ButtonShader.pushShader;
            }
            if (!base.Enabled)
            {
                buttonShader = ButtonShader.disableShader;
            }

            Rectangle clientRectangle = base.ClientRectangle;
            using (Pen pen = new Pen(BackColor))
            {
                graphics.DrawRectangle(pen, clientRectangle.X, clientRectangle.Y, clientRectangle.Width - 1, clientRectangle.Height - 1);
            }
            clientRectangle.Inflate(-1, -1);
            buttonShader.DrawButton(graphics, clientRectangle, BackColor);

            if (Focused)
            {
                Rectangle rectangle = clientRectangle;
                rectangle.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(graphics, rectangle);
            }

            if (base.Image != null)
            {
                ImageAttributes imageAttributes = new ImageAttributes();
                if (!base.Enabled)
                {
                    imageAttributes.SetColorMatrix(disableMatrix);
                }
                graphics.DrawImage(base.Image,
                    new Rectangle(clientRectangle.X + (clientRectangle.Width - base.Image.Width) / 2, clientRectangle.Y + (clientRectangle.Height - base.Image.Height) / 2, base.Image.Width, base.Image.Height),
                    0, 0, base.Image.Width, base.Image.Height, GraphicsUnit.Pixel, imageAttributes);
                return;
            }

            Rectangle rect = clientRectangle;
            if (pushed)
            {
                rect.Offset(1, 1);
            }

            Color controlText = SystemColors.ControlText;
            if (!base.Enabled)
            {
                controlText = disableText;
            }
            buttonShader.DrawText(graphics, rect, Text, Font, controlText);
        }
    }
}