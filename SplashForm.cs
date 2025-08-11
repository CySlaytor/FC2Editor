using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor
{
    public class SplashForm : Form
    {
        private bool m_aboutMode;
        private static SplashForm s_form;
        private static bool s_isDone;
        private IContainer components = null;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x80000; // WS_EX_LAYERED
                return createParams;
            }
        }

        public bool AboutMode
        {
            get { return m_aboutMode; }
            set { m_aboutMode = value; }
        }

        public bool IsDoneLoading
        {
            get { return s_isDone; }
            set { s_isDone = value; }
        }

        public SplashForm()
        {
            InitializeComponent();
            base.Icon = new Icon(new System.IO.MemoryStream(Resources.appIcon));
        }

        private void SplashForm_Load(object sender, EventArgs e)
        {
            Text = AboutMode ? Localizer.Localize("EDITOR_ABOUT") : "Loading Far Cry® 2 Map Editor...";

            Bitmap splash = Resources.splash;
            base.Width = splash.Width;
            base.Height = splash.Height;

            string loadingText = "Loading...";
            string editorName = "Far Cry® 2 Map Editor";
            string legalText = "© 2008 Ubisoft Entertainment. All Rights Reserved. Far Cry®, Ubisoft and the Ubisoft logo are trademarks of Ubisoft Entertainment in the US and/or other countries. Based on Crytek's original Far Cry® directed by Cevat Yerli.";
            string language = "english";

            try
            {
                string iniPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "FC2Init.ini");
                Win32.GetPrivateProfileStringW("FC2_INIT", "language", "english", out language, iniPath);
                language = language.ToLower();

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(Resources.SplashLocalization); // NOTE: This resource is empty, so this will fail. We'll proceed assuming default text.
                XmlElement documentElement = xmlDocument.DocumentElement;
                XmlNodeList elementsByTagName = documentElement.GetElementsByTagName(language);
                foreach (XmlNode item in elementsByTagName)
                {
                    if (item.Name != language) continue;

                    XmlElement xmlElement = (XmlElement)item;
                    foreach (XmlNode childNode in xmlElement.ChildNodes)
                    {
                        string key = childNode.Attributes["enum"].Value;
                        string value = childNode.Attributes["value"].Value;
                        switch (key)
                        {
                            case "LOADING_GAMEFILE": loadingText = value; break;
                            case "EDITOR_NAME": editorName = value; break;
                            case "TEXT_LEGAL": legalText = value; break;
                            case "LOADING_TITLE": if (!AboutMode) Text = value; break;
                        }
                    }
                }
            }
            catch
            {
                // Fallback to default English text if XML parsing fails
            }

            using (Graphics graphics = Graphics.FromImage(splash))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (Font titleFont = new Font("Tahoma", 20f, FontStyle.Bold))
                using (Font statusFont = new Font("Tahoma", 10f, FontStyle.Bold))
                using (Font legalFont = new Font("Tahoma", 8f))
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    graphics.DrawString(editorName, titleFont, brush, new RectangleF(128f, 338f, 448f, 50f));
                    graphics.DrawString(legalText, legalFont, brush, new RectangleF(128f, 378f, 448f, 50f));
                    if (!AboutMode)
                    {
                        graphics.DrawString(loadingText, statusFont, brush, new RectangleF(128f, 428f, 448f, 20f));
                    }
                }
            }

            // Code to display a layered window (alpha-blended)
            IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
            IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = splash.GetHbitmap(Color.FromArgb(0));
                oldBitmap = Win32.SelectObject(memDc, hBitmap);
                Win32.Size size = new Win32.Size(splash.Width, splash.Height);
                Win32.Point pointSource = new Win32.Point(0, 0);
                Win32.Point topPos = new Win32.Point(Left, Top);
                Win32.BlendFunction blend = new Win32.BlendFunction
                {
                    BlendOp = 0, // AC_SRC_OVER
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = 1 // AC_SRC_ALPHA
                };
                Win32.UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, 2); // ULW_ALPHA
            }
            finally
            {
                Win32.ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDc, oldBitmap);
                    Win32.DeleteObject(hBitmap);
                }
                Win32.DeleteDC(memDc);
            }
        }

        private void SplashForm_Click(object sender, EventArgs e)
        {
            if (AboutMode) Close();
        }

        private void SplashForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (AboutMode) Close();
        }

        public static void Start()
        {
            s_form = new SplashForm();
            s_form.Show();
        }

        public static void Stop()
        {
            if (s_form != null)
            {
                s_form.Dispose();
                s_form = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SplashForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SplashForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Loading...";
            this.Click += new System.EventHandler(this.SplashForm_Click);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SplashForm_KeyUp);
            this.Load += new System.EventHandler(this.SplashForm_Load);
            this.ResumeLayout(false);
        }
    }
}