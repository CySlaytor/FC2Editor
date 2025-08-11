using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;
using TD.SandBar;
using TD.SandDock;

namespace FC2Editor.UI
{
    internal class CodeEditor : Form
    {
        private IContainer components = null;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel lineStatusLabel;
        private ToolStripStatusLabel columnStatusLabel;
        private SandDockManager sandDockManager1;
        private ToolBarContainer leftSandBarDock;
        private SandBarManager sandBarManager1;
        private ToolBarContainer rightSandBarDock;
        private ToolBarContainer bottomSandBarDock;
        private ToolBarContainer topSandBarDock;
        private MenuBar menuBar1;
        private MenuBarItem menuBarItem1;
        private MenuButtonItem newFileMenuItem;
        private MenuBarItem menuBarItem2;
        private MenuBarItem menuBarItem3;
        private MenuBarItem menuBarItem4;
        private MenuBarItem menuBarItem5;
        private TD.SandBar.ToolBar toolBar1;
        private MenuButtonItem saveScriptMenuItem;
        private MenuButtonItem openScriptMenuItem;
        private MenuButtonItem cutMenuItem;
        private MenuButtonItem copyMenuItem;
        private ButtonItem buttonItem1;
        private ButtonItem buttonItem3;
        private ButtonItem buttonItem2;
        private ButtonItem buttonItem4;
        private MenuButtonItem pasteMenuItem;
        private ButtonItem buttonItem5;
        private ButtonItem buttonItem6;
        private MenuButtonItem runMenuItem;
        private ButtonItem buttonItem7;
        private CodeMapViewerDock codeMapViewerDock1;
        private MenuButtonItem undoMenuItem;
        private MenuButtonItem redoMenuItem;
        private ButtonItem buttonItem8;
        private ButtonItem buttonItem9;
        private MenuButtonItem exitMenuItem;
        private MenuButtonItem mapViewerMenuItem;
        private OpenFileDialog openFileDialog;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem cutToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem insertFunctionToolStripMenuItem;
        private ToolStripMenuItem insertTextureEntryIDToolStripMenuItem;
        private ToolStripMenuItem insertCollectionEntryIDToolStripMenuItem;

        private static CodeEditor s_instance;
        public static CodeEditor Instance => s_instance;

        public CodeEditor()
        {
            s_instance = this;
            InitializeComponent();
            base.Icon = (System.Drawing.Icon)Resources.ResourceManager.GetObject("appIcon");
            NomadCodeBox.InitFunctions();
            InitContextMenu();
        }

        private void DisposeInternal()
        {
            s_instance = null;
        }

        private CodeDocument CreateDocument()
        {
            CodeDocument codeDocument = new CodeDocument
            {
                Manager = sandDockManager1
            };
            codeDocument.Content.ContextMenuStrip = contextMenuStrip1;
            return codeDocument;
        }

        private void newFileMenuItem_Activate(object sender, EventArgs e)
        {
            CreateDocument().Open();
        }

        private void openScriptMenuItem_Activate(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                CodeDocument codeDocument = CreateDocument();
                codeDocument.LoadFile(openFileDialog.FileName);
                codeDocument.Open();
            }
        }

        private void saveScriptMenuItem_Activate(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.SaveFile();
            }
        }

        private void exitMenuItem_Activate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void undoMenuItem_Activate(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.Content.Undo();
            }
        }

        private void redoMenuItem_Activate(object sender, EventArgs e)
        {
            // Note: The original decompiled source did not contain a Redo implementation for the textbox.
            // We can leave this as is, or implement it later.
        }

        private void cutMenuItem_Activate(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.Content.Cut();
            }
        }

        private void copyMenuItem_Activate(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.Content.Copy();
            }
        }

        private void pasteMenuItem_Activate(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.Content.Paste();
            }
        }

        private void runMenuItem_Activate(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.Run();
                UpdateCaretPosition();
            }
        }

        private void mapViewerMenuItem_Activate(object sender, EventArgs e)
        {
            codeMapViewerDock1.Open();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e) => cutMenuItem_Activate(sender, e);
        private void copyToolStripMenuItem_Click(object sender, EventArgs e) => copyMenuItem_Activate(sender, e);
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e) => pasteMenuItem_Activate(sender, e);

        private void functionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                codeDocument.Content.Paste(((ToolStripMenuItem)sender).Text + "(");
                codeDocument.Content.ShowCodeHelper();
            }
        }

        private void InitContextMenuFunctions()
        {
            insertFunctionToolStripMenuItem.DropDownItems.Clear();
            SortedList<string, Wilderness.FunctionDef> sortedList = new SortedList<string, Wilderness.FunctionDef>();
            for (int i = 0; i < Wilderness.NumFunctions; i++)
            {
                Wilderness.FunctionDef function = Wilderness.GetFunction(i);
                sortedList.Add(function.Name, function);
            }
            foreach (string key in sortedList.Keys)
            {
                ToolStripMenuItem value = new ToolStripMenuItem(key, null, functionToolStripMenuItem_Click);
                insertFunctionToolStripMenuItem.DropDownItems.Add(value);
            }
        }

        private void InitContextMenu()
        {
            InitContextMenuFunctions();
        }

        public void UpdateCaretPosition()
        {
            if (sandDockManager1.ActiveTabbedDocument is CodeDocument codeDocument)
            {
                NomadTextBox.Position caretPosition = codeDocument.Content.CaretPosition;
                lineStatusLabel.Text = "Line " + (caretPosition.line + 1);
                columnStatusLabel.Text = "Col " + (caretPosition.column + 1);
                codeMapViewerDock1.Image = codeDocument.GetLineImage(codeDocument.Content.CaretPosition.line);
            }
        }

        public void UpdateCaretPosition(CodeDocument document)
        {
            if (sandDockManager1.ActiveTabbedDocument == document)
            {
                UpdateCaretPosition();
            }
        }

        private void sandDockManager1_ActiveTabbedDocumentChanged(object sender, EventArgs e)
        {
            UpdateCaretPosition();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            DisposeInternal();
        }

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lineStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.columnStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.sandDockManager1 = new TD.SandDock.SandDockManager();
            this.leftSandBarDock = new TD.SandBar.ToolBarContainer();
            this.sandBarManager1 = new TD.SandBar.SandBarManager(this.components);
            this.rightSandBarDock = new TD.SandBar.ToolBarContainer();
            this.bottomSandBarDock = new TD.SandBar.ToolBarContainer();
            this.topSandBarDock = new TD.SandBar.ToolBarContainer();
            this.menuBar1 = new TD.SandBar.MenuBar();
            this.menuBarItem1 = new TD.SandBar.MenuBarItem();
            this.newFileMenuItem = new TD.SandBar.MenuButtonItem();
            this.openScriptMenuItem = new TD.SandBar.MenuButtonItem();
            this.saveScriptMenuItem = new TD.SandBar.MenuButtonItem();
            this.exitMenuItem = new TD.SandBar.MenuButtonItem();
            this.menuBarItem2 = new TD.SandBar.MenuBarItem();
            this.undoMenuItem = new TD.SandBar.MenuButtonItem();
            this.redoMenuItem = new TD.SandBar.MenuButtonItem();
            this.cutMenuItem = new TD.SandBar.MenuButtonItem();
            this.copyMenuItem = new TD.SandBar.MenuButtonItem();
            this.pasteMenuItem = new TD.SandBar.MenuButtonItem();
            this.menuBarItem3 = new TD.SandBar.MenuBarItem();
            this.mapViewerMenuItem = new TD.SandBar.MenuButtonItem();
            this.menuBarItem4 = new TD.SandBar.MenuBarItem();
            this.runMenuItem = new TD.SandBar.MenuButtonItem();
            this.menuBarItem5 = new TD.SandBar.MenuBarItem();
            this.toolBar1 = new TD.SandBar.ToolBar();
            this.buttonItem1 = new TD.SandBar.ButtonItem();
            this.buttonItem3 = new TD.SandBar.ButtonItem();
            this.buttonItem2 = new TD.SandBar.ButtonItem();
            this.buttonItem4 = new TD.SandBar.ButtonItem();
            this.buttonItem5 = new TD.SandBar.ButtonItem();
            this.buttonItem6 = new TD.SandBar.ButtonItem();
            this.buttonItem8 = new TD.SandBar.ButtonItem();
            this.buttonItem9 = new TD.SandBar.ButtonItem();
            this.buttonItem7 = new TD.SandBar.ButtonItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.insertFunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertTextureEntryIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertCollectionEntryIDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.codeMapViewerDock1 = new FC2Editor.UI.CodeMapViewerDock();
            TD.SandDock.DockContainer dockContainer = new TD.SandDock.DockContainer();
            dockContainer.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.topSandBarDock.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lineStatusLabel,
            this.columnStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 361);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(528, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(447, 17);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // lineStatusLabel
            // 
            this.lineStatusLabel.Name = "lineStatusLabel";
            this.lineStatusLabel.Size = new System.Drawing.Size(35, 17);
            this.lineStatusLabel.Text = "Line 0";
            // 
            // columnStatusLabel
            // 
            this.columnStatusLabel.Name = "columnStatusLabel";
            this.columnStatusLabel.Size = new System.Drawing.Size(31, 17);
            this.columnStatusLabel.Text = "Col 0";
            // 
            // sandDockManager1
            // 
            this.sandDockManager1.DockSystemContainer = this;
            this.sandDockManager1.OwnerForm = this;
            this.sandDockManager1.ActiveTabbedDocumentChanged += new System.EventHandler(this.sandDockManager1_ActiveTabbedDocumentChanged);
            // 
            // leftSandBarDock
            // 
            this.leftSandBarDock.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftSandBarDock.Guid = new System.Guid("f51f9d3c-d12d-4b04-b3c1-dba150a785e6");
            this.leftSandBarDock.Location = new System.Drawing.Point(0, 49);
            this.leftSandBarDock.Manager = this.sandBarManager1;
            this.leftSandBarDock.Name = "leftSandBarDock";
            this.leftSandBarDock.Size = new System.Drawing.Size(0, 312);
            this.leftSandBarDock.TabIndex = 2;
            // 
            // sandBarManager1
            // 
            this.sandBarManager1.OwnerForm = this;
            this.sandBarManager1.Renderer = new TD.SandBar.Office2003Renderer();
            // 
            // rightSandBarDock
            // 
            this.rightSandBarDock.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightSandBarDock.Guid = new System.Guid("c3e7b6e1-de74-4788-aa84-badf07d4feb3");
            this.rightSandBarDock.Location = new System.Drawing.Point(528, 49);
            this.rightSandBarDock.Manager = this.sandBarManager1;
            this.rightSandBarDock.Name = "rightSandBarDock";
            this.rightSandBarDock.Size = new System.Drawing.Size(0, 312);
            this.rightSandBarDock.TabIndex = 3;
            // 
            // bottomSandBarDock
            // 
            this.bottomSandBarDock.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomSandBarDock.Guid = new System.Guid("214365d9-c066-4c57-b2e9-f4e9b8b58fd5");
            this.bottomSandBarDock.Location = new System.Drawing.Point(0, 361);
            this.bottomSandBarDock.Manager = this.sandBarManager1;
            this.bottomSandBarDock.Name = "bottomSandBarDock";
            this.bottomSandBarDock.Size = new System.Drawing.Size(528, 0);
            this.bottomSandBarDock.TabIndex = 4;
            // 
            // topSandBarDock
            // 
            this.topSandBarDock.Controls.Add(this.menuBar1);
            this.topSandBarDock.Controls.Add(this.toolBar1);
            this.topSandBarDock.Dock = System.Windows.Forms.DockStyle.Top;
            this.topSandBarDock.Guid = new System.Guid("31bfec4a-f321-4810-8e02-c4eccad1d7f9");
            this.topSandBarDock.Location = new System.Drawing.Point(0, 0);
            this.topSandBarDock.Manager = this.sandBarManager1;
            this.topSandBarDock.Name = "topSandBarDock";
            this.topSandBarDock.Size = new System.Drawing.Size(528, 49);
            this.topSandBarDock.TabIndex = 5;
            // 
            // menuBar1
            // 
            this.menuBar1.Guid = new System.Guid("0188290c-f307-4042-a99f-b3a2a1517a38");
            this.menuBar1.Items.AddRange(new TD.SandBar.ToolbarItemBase[] {
            this.menuBarItem1,
            this.menuBarItem2,
            this.menuBarItem3,
            this.menuBarItem4,
            this.menuBarItem5});
            this.menuBar1.Location = new System.Drawing.Point(2, 0);
            this.menuBar1.Name = "menuBar1";
            this.menuBar1.OwnerForm = this;
            this.menuBar1.Size = new System.Drawing.Size(526, 23);
            this.menuBar1.TabIndex = 0;
            this.menuBar1.Text = "menuBar1";
            // 
            // menuBarItem1
            // 
            this.menuBarItem1.Items.AddRange(new TD.SandBar.ToolbarItemBase[] {
            this.newFileMenuItem,
            this.openScriptMenuItem,
            this.saveScriptMenuItem,
            this.exitMenuItem});
            this.menuBarItem1.Text = "&File";
            // 
            // newFileMenuItem
            // 
            this.newFileMenuItem.Image = Resources.newMap;
            this.newFileMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.newFileMenuItem.Text = "New script";
            this.newFileMenuItem.Activate += new System.EventHandler(this.newFileMenuItem_Activate);
            // 
            // openScriptMenuItem
            // 
            this.openScriptMenuItem.Image = Resources.openMap;
            this.openScriptMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.openScriptMenuItem.Text = "Open script";
            this.openScriptMenuItem.Activate += new System.EventHandler(this.openScriptMenuItem_Activate);
            // 
            // saveScriptMenuItem
            // 
            this.saveScriptMenuItem.BeginGroup = true;
            this.saveScriptMenuItem.Image = Resources.save;
            this.saveScriptMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.saveScriptMenuItem.Text = "Save script";
            this.saveScriptMenuItem.Activate += new System.EventHandler(this.saveScriptMenuItem_Activate);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.BeginGroup = true;
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Activate += new System.EventHandler(this.exitMenuItem_Activate);
            // 
            // menuBarItem2
            // 
            this.menuBarItem2.Items.AddRange(new TD.SandBar.ToolbarItemBase[] {
            this.undoMenuItem,
            this.redoMenuItem,
            this.cutMenuItem,
            this.copyMenuItem,
            this.pasteMenuItem});
            this.menuBarItem2.Text = "&Edit";
            // 
            // undoMenuItem
            // 
            this.undoMenuItem.Image = Resources.undo;
            this.undoMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
            this.undoMenuItem.Text = "&Undo";
            this.undoMenuItem.Activate += new System.EventHandler(this.undoMenuItem_Activate);
            // 
            // redoMenuItem
            // 
            this.redoMenuItem.Image = Resources.redo;
            this.redoMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftZ;
            this.redoMenuItem.Text = "&Redo";
            this.redoMenuItem.Activate += new System.EventHandler(this.redoMenuItem_Activate);
            // 
            // cutMenuItem
            // 
            this.cutMenuItem.BeginGroup = true;
            this.cutMenuItem.Image = Resources.cut;
            this.cutMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
            this.cutMenuItem.Text = "C&ut";
            this.cutMenuItem.Activate += new System.EventHandler(this.cutMenuItem_Activate);
            // 
            // copyMenuItem
            // 
            this.copyMenuItem.Image = Resources.copy;
            this.copyMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.copyMenuItem.Text = "&Copy";
            this.copyMenuItem.Activate += new System.EventHandler(this.copyMenuItem_Activate);
            // 
            // pasteMenuItem
            // 
            this.pasteMenuItem.Image = Resources.paste;
            this.pasteMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlV;
            this.pasteMenuItem.Text = "&Paste";
            this.pasteMenuItem.Activate += new System.EventHandler(this.pasteMenuItem_Activate);
            // 
            // menuBarItem3
            // 
            this.menuBarItem3.Items.AddRange(new TD.SandBar.ToolbarItemBase[] { this.mapViewerMenuItem });
            this.menuBarItem3.Text = "&View";
            // 
            // mapViewerMenuItem
            // 
            this.mapViewerMenuItem.Text = "Map Viewer";
            this.mapViewerMenuItem.Activate += new System.EventHandler(this.mapViewerMenuItem_Activate);
            // 
            // menuBarItem4
            // 
            this.menuBarItem4.Items.AddRange(new TD.SandBar.ToolbarItemBase[] { this.runMenuItem });
            this.menuBarItem4.MdiWindowList = true;
            this.menuBarItem4.Text = "&Script";
            // 
            // runMenuItem
            // 
            this.runMenuItem.Image = Resources.PlayHS;
            this.runMenuItem.Shortcut = System.Windows.Forms.Shortcut.F5;
            this.runMenuItem.Text = "&Run";
            this.runMenuItem.Activate += new System.EventHandler(this.runMenuItem_Activate);
            // 
            // menuBarItem5
            // 
            this.menuBarItem5.Text = "&Help";
            // 
            // toolBar1
            // 
            this.toolBar1.DockLine = 1;
            this.toolBar1.Guid = new System.Guid("46756475-373b-4e41-8b89-f8ab1b41c370");
            this.toolBar1.Items.AddRange(new TD.SandBar.ToolbarItemBase[] {
            this.buttonItem1,
            this.buttonItem3,
            this.buttonItem2,
            this.buttonItem4,
            this.buttonItem5,
            this.buttonItem6,
            this.buttonItem8,
            this.buttonItem9,
            this.buttonItem7});
            this.toolBar1.Location = new System.Drawing.Point(2, 23);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.Size = new System.Drawing.Size(252, 26);
            this.toolBar1.TabIndex = 1;
            this.toolBar1.Text = "toolBar1";
            // 
            // buttonItem1
            // 
            this.buttonItem1.BuddyMenu = this.newFileMenuItem;
            this.buttonItem1.Image = Resources.newMap;
            // 
            // buttonItem3
            // 
            this.buttonItem3.BuddyMenu = this.openScriptMenuItem;
            this.buttonItem3.Image = Resources.openMap;
            // 
            // buttonItem2
            // 
            this.buttonItem2.BuddyMenu = this.saveScriptMenuItem;
            this.buttonItem2.Image = Resources.save;
            // 
            // buttonItem4
            // 
            this.buttonItem4.BeginGroup = true;
            this.buttonItem4.BuddyMenu = this.cutMenuItem;
            this.buttonItem4.Image = Resources.cut;
            // 
            // buttonItem5
            // 
            this.buttonItem5.BuddyMenu = this.copyMenuItem;
            this.buttonItem5.Image = Resources.copy;
            // 
            // buttonItem6
            // 
            this.buttonItem6.BuddyMenu = this.pasteMenuItem;
            this.buttonItem6.Image = Resources.paste;
            // 
            // buttonItem8
            // 
            this.buttonItem8.BeginGroup = true;
            this.buttonItem8.BuddyMenu = this.undoMenuItem;
            this.buttonItem8.Image = Resources.undo;
            // 
            // buttonItem9
            // 
            this.buttonItem9.BuddyMenu = this.redoMenuItem;
            this.buttonItem9.Image = Resources.redo;
            // 
            // buttonItem7
            // 
            this.buttonItem7.BeginGroup = true;
            this.buttonItem7.BuddyMenu = this.runMenuItem;
            this.buttonItem7.Image = Resources.PlayHS;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "lua";
            this.openFileDialog.Filter = "Far Cry 2 script files (*.lua)|*.lua|All files (*.*)|*.*";
            this.openFileDialog.Title = "Open script";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator1,
            this.insertFunctionToolStripMenuItem,
            this.insertTextureEntryIDToolStripMenuItem,
            this.insertCollectionEntryIDToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(211, 142);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // insertFunctionToolStripMenuItem
            // 
            this.insertFunctionToolStripMenuItem.Name = "insertFunctionToolStripMenuItem";
            this.insertFunctionToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.insertFunctionToolStripMenuItem.Text = "Insert function";
            // 
            // insertTextureEntryIDToolStripMenuItem
            // 
            this.insertTextureEntryIDToolStripMenuItem.Name = "insertTextureEntryIDToolStripMenuItem";
            this.insertTextureEntryIDToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.insertTextureEntryIDToolStripMenuItem.Text = "Insert texture entry ID";
            // 
            // insertCollectionEntryIDToolStripMenuItem
            // 
            this.insertCollectionEntryIDToolStripMenuItem.Name = "insertCollectionEntryIDToolStripMenuItem";
            this.insertCollectionEntryIDToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            this.insertCollectionEntryIDToolStripMenuItem.Text = "Insert collection entry ID";
            // 
            // codeMapViewerDock1
            // 
            this.codeMapViewerDock1.Guid = new System.Guid("40b77176-f5ac-44ce-a28c-d2f296197e1d");
            this.codeMapViewerDock1.Location = new System.Drawing.Point(4, 18);
            this.codeMapViewerDock1.Name = "codeMapViewerDock1";
            this.codeMapViewerDock1.Size = new System.Drawing.Size(250, 270);
            this.codeMapViewerDock1.TabIndex = 0;
            this.codeMapViewerDock1.Text = "Map Viewer";
            //
            // dockContainer
            //
            dockContainer.Controls.Add(this.codeMapViewerDock1);
            dockContainer.Dock = System.Windows.Forms.DockStyle.Right;
            dockContainer.LayoutSystem = new TD.SandDock.SplitLayoutSystem(250, 312, System.Windows.Forms.Orientation.Horizontal, new TD.SandDock.LayoutSystemBase[] { new TD.SandDock.ControlLayoutSystem(250, 312, new TD.SandDock.DockControl[] { this.codeMapViewerDock1 }, this.codeMapViewerDock1) });
            dockContainer.Location = new System.Drawing.Point(274, 49);
            dockContainer.Manager = this.sandDockManager1;
            dockContainer.Name = "dockContainer1";
            dockContainer.Size = new System.Drawing.Size(254, 312);
            dockContainer.TabIndex = 6;
            // 
            // CodeEditor
            // 
            this.ClientSize = new System.Drawing.Size(528, 383);
            this.Controls.Add(dockContainer);
            this.Controls.Add(this.leftSandBarDock);
            this.Controls.Add(this.rightSandBarDock);
            this.Controls.Add(this.bottomSandBarDock);
            this.Controls.Add(this.topSandBarDock);
            this.Controls.Add(this.statusStrip1);
            this.Name = "CodeEditor";
            this.Text = "Far Cry 2 Script Editor";
            dockContainer.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.topSandBarDock.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}