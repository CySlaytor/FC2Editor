using FC2Editor.Core.Nomad; // Will be added later
using FC2Editor.Properties; // Exists
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FC2Editor.UI
{
    internal class PromptForm : Form
    {
        public delegate bool ValidationDelegate(string input, out string message);

        private IContainer components = null;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private NomadButton okButton;
        private NomadButton cancelButton;
        private TextBox promptTextBox;
        private Label promptLabel;
        private ValidationDelegate m_validation;

        public string Prompt
        {
            get { return promptLabel.Text; }
            set { promptLabel.Text = value; }
        }

        public string Input
        {
            get { return promptTextBox.Text; }
            set { promptTextBox.Text = value; }
        }

        public ValidationDelegate Validation
        {
            get { return m_validation; }
            set { m_validation = value; }
        }

        public PromptForm()
        {
            InitializeComponent();
            base.Icon = new Icon(new System.IO.MemoryStream(Resources.appIcon));
        }

        public PromptForm(string prompt) : this()
        {
            Prompt = prompt;
        }

        public PromptForm(string prompt, string title) : this()
        {
            Prompt = prompt;
            Text = title;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (m_validation != null && !m_validation(promptTextBox.Text, out string message))
            {
                MessageBox.Show(this, message);
                base.DialogResult = DialogResult.None;
            }
            else
            {
                base.DialogResult = DialogResult.OK;
            }
        }

        public static ValidationDelegate GetIntegerValidation(int min, int max)
        {
            return delegate (string input, out string message)
            {
                message = null;
                if (!int.TryParse(input, out int result))
                {
                    message = Localizer.Localize("PROMPT_NOT_A_NUMBER");
                    return false;
                }
                if (result < min || result > max)
                {
                    message = string.Format(Localizer.Localize("PROMPT_NUMBER_NOT_IN_RANGE"), min, max);
                    return false;
                }
                return true;
            };
        }

        public static ValidationDelegate GetFloatValidation(float min, float max)
        {
            return delegate (string input, out string message)
            {
                message = null;
                if (!float.TryParse(input, out float result))
                {
                    message = Localizer.Localize("PROMPT_NOT_A_NUMBER");
                    return false;
                }
                if (result < min || result > max)
                {
                    message = string.Format(Localizer.Localize("PROMPT_NUMBER_NOT_IN_RANGE"), min, max);
                    return false;
                }
                return true;
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.okButton = new FC2Editor.UI.NomadButton();
            this.cancelButton = new FC2Editor.UI.NomadButton();
            this.promptTextBox = new System.Windows.Forms.TextBox();
            this.promptLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.promptTextBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.promptLabel, 0, 0);
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 2);
            this.tableLayoutPanel1.MaximumSize = new System.Drawing.Size(375, 0);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(375, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(375, 80);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.okButton);
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 48);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel1.Size = new System.Drawing.Size(369, 29);
            this.panel1.TabIndex = 3;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.okButton.Location = new System.Drawing.Point(219, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.cancelButton.Location = new System.Drawing.Point(294, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "&Cancel";
            // 
            // promptTextBox
            // 
            this.promptTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.promptTextBox.Location = new System.Drawing.Point(3, 22);
            this.promptTextBox.Name = "promptTextBox";
            this.promptTextBox.Size = new System.Drawing.Size(369, 20);
            this.promptTextBox.TabIndex = 2;
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(3, 0);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.promptLabel.Size = new System.Drawing.Size(40, 19);
            this.promptLabel.TabIndex = 1;
            this.promptLabel.Text = "Prompt";
            // 
            // PromptForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(378, 84);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PromptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Prompt";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
    }
}