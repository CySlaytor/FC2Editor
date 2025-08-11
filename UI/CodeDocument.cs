using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;
using TD.SandDock;

namespace FC2Editor.UI
{
    internal class CodeDocument : UserTabbedDocument
    {
        private class ImageIcon : NomadCodeBox.IIcon, IDisposable
        {
            public ImageMap Image { get; }

            public ImageIcon(ImageMap image)
            {
                Image = image;
            }

            public void Draw(Graphics g, Rectangle rect)
            {
                g.DrawImage(Resources.InsertPictureHS, rect);
            }

            public void Dispose()
            {
                Image.Dispose();
            }
        }

        private IContainer components = null;
        private NomadCodeBox textBox1;
        private SaveFileDialog saveFileDialog;
        private string m_fileName;

        public string FileName
        {
            get { return m_fileName; }
            set
            {
                m_fileName = value;
                Text = Path.GetFileName(value);
            }
        }

        public NomadCodeBox Content => textBox1;

        public CodeDocument()
        {
            InitializeComponent();
        }

        private void DisposeInternal()
        {
            ClearImages();
        }

        public void LoadFile(string fileName)
        {
            FileName = fileName;
            Content.Text = File.ReadAllText(fileName);
        }

        public void SaveFile()
        {
            if (FileName == null)
            {
                if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                FileName = saveFileDialog.FileName;
            }
            File.WriteAllText(FileName, textBox1.Text);
        }

        public void Run()
        {
            ClearImages();
            Wilderness.RunScriptBuffer(Content.Text, mapCallback, errorCallback);
        }

        private void mapCallback(int line, IntPtr pMap)
        {
            ImageMap image = new ImageMapEngine(pMap).GetImage();
            Content.SetIcon(line - 1, new ImageIcon(image));
        }

        private void errorCallback(int line, IntPtr errorMessagePtr)
        {
            string text = Marshal.PtrToStringAnsi(errorMessagePtr);
            MessageBox.Show(this, "An error has occurred while executing the wilderness script.\n\n" + text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            if (line > 0)
            {
                textBox1.CaretPosition = new NomadTextBox.Position(line - 1, 0); // Corrected to be 0-indexed
            }
        }

        public ImageMap GetLineImage(int line)
        {
            return (Content.GetIcon(line) as ImageIcon)?.Image;
        }

        private void ClearImages()
        {
            Content.ClearIcons();
        }

        private void textBox1_CaretPositionChanged(object sender, EventArgs e)
        {
            CodeEditor.Instance.UpdateCaretPosition(this);
        }

        protected override void Dispose(bool disposing)
        {
            DisposeInternal();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.textBox1 = new FC2Editor.UI.NomadCodeBox();
            this.SuspendLayout();
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "lua";
            this.saveFileDialog.Filter = "Far Cry 2 script files (*.lua)|*.lua|All files (*.*)|*.*";
            this.saveFileDialog.Title = "Save script file";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Lucida Console", 10F);
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(550, 400);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "\r\n";
            this.textBox1.CaretPositionChanged += new System.EventHandler(this.textBox1_CaretPositionChanged);
            // 
            // CodeDocument
            // 
            this.Controls.Add(this.textBox1);
            this.Name = "CodeDocument";
            this.Text = "Untitled";
            this.ResumeLayout(false);
        }
    }
}