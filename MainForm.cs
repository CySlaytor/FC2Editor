using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;
using FC2Editor.Tools;
using FC2Editor.UI;
using FC2Editor.Utils;
using Microsoft.Win32;
using TD.SandBar;
using TD.SandDock;

namespace FC2Editor
{
    internal class MainForm : Form, IInputSink
    {
        private List<ButtonItem> m_toolItemList = new List<ButtonItem>();
        private Dictionary<Keys, ButtonItem> m_toolShortcuts = new Dictionary<Keys, ButtonItem>();
        private string m_helpString;
        private static ITool m_currentTool;
        private bool m_closeSaveConfirmed;
        private float m_lastUpdate;
        private bool m_isLoading;
        private float m_currentMemoryUsage;
        private float m_currentObjectUsage;
        private Vec3 m_currentCameraPos;
        private bool m_currentCursorValid;
        private Vec3 m_currentCursorPos;
        private bool m_cursorPhysics = true;
        private float m_currentFps;
        private string m_documentPath;
        private static MainForm s_instance;
        private IContainer components;
        private SandBarManager sandBarManager;
        private ToolBarContainer leftSandBarDock;
        private ToolBarContainer rightSandBarDock;
        private ToolBarContainer bottomSandBarDock;
        private ToolBarContainer topSandBarDock;
        private MenuBar menuBar;
        private MenuBarItem menuBarItem1;
        private TD.SandBar.ToolBar toolBarMain;
        private MenuButtonItem menuItem_newMap;
        private MenuButtonItem menuItem_loadMap;
        private MenuButtonItem menuItem_saveMap;
        private MenuButtonItem menuItem_exit;
        private ButtonItem buttonItem1;
        private ButtonItem buttonItem2;
        private ButtonItem buttonItem3;
        internal ViewportControl viewport;
        private SandDockManager sandDockManager;
        private ToolParametersDock toolParametersDock;
        private ToolTip toolTip;
        private MenuBarItem menuBarItem2;
        private MenuButtonItem menuItem_viewToolParameters;
        private MenuButtonItem menuItem_viewEditorSettings;
        private OpenFileDialog openMapDialog;
        private SaveFileDialog saveMapDialog;
        private MenuButtonItem menuItem_saveMapAs;
        private MenuBarItem menuBarItem3;
        private MenuButtonItem menuItem_Undo;
        private MenuButtonItem menuItem_Redo;
        private ButtonItem buttonItem4;
        private ButtonItem buttonItem5;
        private Timer timerUIUpdate;
        private MenuBarItem menuBarItem4;
        private MenuButtonItem menuItem_TestIngame;
        private EditorSettingsDock editorSettingsDock;
        private ContextHelpDock contextHelpDock;
        private MenuButtonItem menuItem_viewContextHelp;
        private StatusStrip statusBar;
        private ToolStripStatusLabel statusCaption;
        private ToolStripStatusLabel statusBarFpsItem;
        private ToolStripStatusLabel statusBarCameraPos;
        private ToolStripStatusLabel statusBarCursorPos;
        private ToolStripDropDownButton statusBarCameraSpeed;
        private ContextMenuStrip cameraSpeedStrip;
        private MenuBarItem menuBarItem5;
        private MenuButtonItem menuItem_OpenCodeEditor;
        private MenuButtonItem menuItem_FlushCache;
        private MenuButtonItem menuItem_ExportToConsole;
        private FolderBrowserDialog folderBrowserDialog;
        private ToolStripStatusLabel statusBarObjectUsage;
        private ToolStripStatusLabel statusBarMemoryUsage;
        private MenuButtonItem menuItem_ResetLayout;
        private ContextMenuStrip statusBarContextMenu;
        private ToolStripMenuItem whatsThisToolStripMenuItem;
        private MenuButtonItem menuItem_newWilderness;
        private MenuBarItem menuBarItem6;
        private MenuButtonItem menuItem_visitWebsite;
        private MenuButtonItem menuItem_about;
        private MenuButtonItem menuItem_ExportToPC;

        public bool EnableShortcuts
        {
            get { return this.menuBar.ShortcutListener.Listening; }
            set
            {
                this.menuBar.ShortcutListener.Listening = value;
                if (value)
                {
                    this.viewport.BlockNextKeyRepeats = true;
                }
            }
        }

        public ITool CurrentTool
        {
            get { return m_currentTool; }
            set
            {
                if (m_currentTool != null)
                {
                    m_currentTool.Deactivate();
                    if (m_currentTool is IInputSink sink)
                        Editor.PopInput(sink);
                }
                m_currentTool = value;
                if (m_currentTool != null)
                {
                    if (m_currentTool is IInputSink sink)
                        Editor.PushInput(sink);
                    m_currentTool.Activate();
                }
                this.UpdateCurrentTool();
                if (m_currentTool != null)
                {
                    this.toolParametersDock.Open();
                }
            }
        }

        public bool CursorPhysics
        {
            get { return this.m_cursorPhysics; }
            set { this.m_cursorPhysics = value; }
        }

        public string DocumentPath
        {
            get { return this.m_documentPath; }
            set { this.m_documentPath = value; }
        }

        public ViewportControl Viewport { get { return this.viewport; } }
        public ToolTip ToolTip { get { return this.toolTip; } }
        public static bool IsActive
        {
            get
            {
                IntPtr activeWindow = Win32.GetActiveWindow();
                while (activeWindow != IntPtr.Zero)
                {
                    if (activeWindow == MainForm.Instance.Handle)
                        return true;
                    activeWindow = Win32.GetParent(activeWindow);
                }
                return false;
            }
        }
        public static MainForm Instance { get { return s_instance; } }

        public MainForm()
        {
            MainForm.s_instance = this;
            this.InitializeComponent();
            byte[] iconBytes = (byte[])Resources.ResourceManager.GetObject("appIcon");
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(iconBytes))
            {
                base.Icon = new System.Drawing.Icon(ms);
            }
            Editor.PushInput((IInputSink)this);
            base.HandleCreated += new EventHandler(this.MainForm_HandleCreated);
            base.HandleDestroyed += new EventHandler(this.MainForm_HandleDestroyed);
            bool flag = false;
            foreach (string str in Environment.GetCommandLineArgs())
            {
                if (str == "-wilderness")
                    flag = true;
            }
            this.menuItem_OpenCodeEditor.Visible = flag;
            this.menuItem_ExportToPC.Visible = false;
            this.menuItem_ExportToConsole.Visible = false;
        }

        private void DisposeInternal()
        {
            Editor.PopInput((IInputSink)this);
        }

        public void InitializeDocks()
        {
            this.toolParametersDock = new ToolParametersDock();
            this.toolParametersDock.Guid = new Guid("3b44d7d6-f472-4373-9bac-a5d4cc471425");
            this.toolParametersDock.Manager = this.sandDockManager;
            this.contextHelpDock = new ContextHelpDock();
            this.contextHelpDock.Guid = new Guid("4a3bcfd3-d4b0-44a0-bac0-bfd030fbc69b");
            this.contextHelpDock.Manager = this.sandDockManager;
            this.editorSettingsDock = new EditorSettingsDock();
            this.editorSettingsDock.Guid = new Guid("ad8a4d52-f3ba-4463-9ce3-4cbed143ec05");
            this.editorSettingsDock.Manager = this.sandDockManager;
        }

        public void EnableUI(bool enable)
        {
            foreach (Control control in base.Controls)
            {
                control.Enabled = enable;
            }
        }

        private TD.SandBar.ToolBar CreateToolbar(string id, string text, int row, int column)
        {
            TD.SandBar.ToolBar toolBar = new TD.SandBar.ToolBar();
            toolBar.Guid = new Guid(id.GetHashCode(), (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
            toolBar.DockLine = row;
            toolBar.DockOffset = column;
            this.sandBarManager.AddToolbar(toolBar);
            toolBar.Redock(this.sandBarManager.FindSuitableContainer(DockStyle.Top));
            toolBar.Text = text;
            toolBar.EnterMenuLoop += new EventHandler(this.menuBar_EnterMenuLoop);
            toolBar.ExitMenuLoop += new EventHandler(this.menuBar_ExitMenuLoop);
            return toolBar;
        }

        private void AddTool(TD.SandBar.ToolBar toolbar, IToolBase tool, Keys shortcut)
        {
            ButtonItem buttonItem = new ButtonItem();
            buttonItem.Image = tool.GetToolImage();
            buttonItem.ToolTipText = StringUtils.EscapeUIString(tool.GetToolName()) + (shortcut != Keys.None ? " (" + shortcut.ToString() + ")" : "");
            buttonItem.Tag = (object)tool;
            if (tool is ITool)
                buttonItem.Activate += new EventHandler(this.tool_Activate);
            else if (tool is IToolAction)
                buttonItem.Activate += new EventHandler(this.toolBase_Activate);
            toolbar.Items.Add((ToolbarItemBase)buttonItem);
            this.m_toolItemList.Add(buttonItem);
            if (shortcut == Keys.None)
                return;
            this.m_toolShortcuts.Add(shortcut, buttonItem);
        }

        public void InitializeTools()
        {
            TD.SandBar.ToolBar toolbar1 = this.CreateToolbar("Terrain", Localizer.Localize("TOOLBAR_TERRAIN"), 1, 1);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainBump(), Keys.F1);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainRaiseLower(), Keys.F2);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainSetHeight(), Keys.F3);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainSmooth(), Keys.F4);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainRamp(), Keys.F5);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainNoise(), Keys.F6);
            this.AddTool(toolbar1, (IToolBase)new ToolTerrainErosion(), Keys.F7);
            this.AddTool(toolbar1, (IToolBase)new ToolTexture(), Keys.F8);
            TD.SandBar.ToolBar toolbar2 = this.CreateToolbar("Objects", Localizer.Localize("TOOLBAR_OBJECTS"), 1, 2);
            this.AddTool(toolbar2, (IToolBase)new ToolObject(), Keys.F9);
            this.AddTool(toolbar2, (IToolBase)new ToolCollection(), Keys.F10);
            this.AddTool(toolbar2, (IToolBase)new ToolRoad(), Keys.F11);
            this.AddTool(toolbar2, (IToolBase)new ToolPlayableZone(), Keys.F12);
            TD.SandBar.ToolBar toolbar3 = this.CreateToolbar("Miscellaneous", Localizer.Localize("TOOLBAR_MISCELLANEOUS"), 1, 3);
            this.AddTool(toolbar3, (IToolBase)new ToolEnvironment(), Keys.None);
            this.AddTool(toolbar3, (IToolBase)new ToolValidation(), Keys.None);
            this.AddTool(toolbar3, (IToolBase)new ToolProperties(), Keys.None);
            this.AddTool(toolbar3, (IToolBase)new ToolFinalize(), Keys.None);
        }

        private void toolBase_Activate(object sender, EventArgs e)
        {
            ((IToolAction)((ButtonItem)sender).Tag).Fire();
        }

        private void tool_Activate(object sender, EventArgs e)
        {
            ITool tag = (ITool)((ButtonItem)sender).Tag;
            this.CurrentTool = this.CurrentTool != tag ? tag : (ITool)null;
        }

        private void UpdateToolbar()
        {
            foreach (ButtonItem toolItem in this.m_toolItemList)
            {
                if (toolItem.Tag == this.CurrentTool)
                {
                    if (!toolItem.Checked)
                        toolItem.Checked = true;
                }
                else if (toolItem.Checked)
                    toolItem.Checked = false;
            }
        }

        public void ClearMapPath()
        {
            this.m_documentPath = (string)null;
            this.UpdateTitleBar();
        }

        private bool PromptSave(EditorDocument.SaveCompletedCallback callback)
        {
            switch (MessageBox.Show((IWin32Window)MainForm.Instance, Localizer.Localize("EDITOR_CHANGE_MAP_PROMPT"), Localizer.Localize("EDITOR_CONFIRMATION"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
            {
                case DialogResult.Yes:
                    this.SaveMap(false, true, callback);
                    return false;
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
                default:
                    return false;
            }
        }

        private void NewMap()
        {
            if (!this.PromptSave((EditorDocument.SaveCompletedCallback)(success =>
            {
                if (!success)
                    return;
                this.NewMapInternal();
            })))
                return;
            this.NewMapInternal();
        }

        private void NewMapInternal()
        {
            this.CurrentTool = (ITool)null;
            EditorDocument.Reset();
            this.m_documentPath = (string)null;
            this.UpdateTitleBar();
        }

        private void LoadMap(string fileName, EditorDocument.LoadCompletedCallback callback)
        {
            if (!this.PromptSave((EditorDocument.SaveCompletedCallback)(success =>
            {
                if (!success)
                    return;
                this.LoadMapInternal((string)null, callback);
            })))
                return;
            this.LoadMapInternal(fileName, callback);
        }

        private void LoadMapInternal(string fileName, EditorDocument.LoadCompletedCallback callback)
        {
            if (fileName == null)
            {
                if (this.openMapDialog.ShowDialog((IWin32Window)this) != DialogResult.OK)
                    return;
                fileName = this.openMapDialog.FileName;
            }
            this.m_documentPath = fileName;
            this.CurrentTool = (ITool)null;
            EditorDocument.Load(this.m_documentPath, callback);
            this.UpdateTitleBar();
        }

        private bool SaveMap(bool saveAs, bool silent, EditorDocument.SaveCompletedCallback callback)
        {
            if (!EditorDocument.Validate() && MessageBox.Show(Localizer.Localize("ERROR_VALIDATION_FAILED"), Localizer.Localize("ERROR"), MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.No)
                return false;
            string fileName = this.m_documentPath;
            if (saveAs || this.m_documentPath == null)
            {
                this.saveMapDialog.FileName = fileName;
                if (this.saveMapDialog.ShowDialog((IWin32Window)this) != DialogResult.OK)
                    return false;
                fileName = this.saveMapDialog.FileName;
            }
            string str = (string)null;
            if (string.IsNullOrEmpty(EditorDocument.CreatorName))
            {
                using (PromptForm promptForm = new PromptForm(Localizer.Localize("PROMPT_CREATOR_TEXT")))
                {
                    promptForm.Text = Localizer.Localize("PROMPT_CREATOR_TITLE");
                    promptForm.Input = EditorDocument.AuthorName;
                    if (promptForm.ShowDialog((IWin32Window)this) != DialogResult.OK)
                        return false;
                    str = promptForm.Input;
                }
            }
            if (str != null)
                EditorDocument.CreatorName = str;
            this.m_documentPath = fileName;
            EditorDocument.Save(this.m_documentPath, callback);
            this.UpdateTitleBar();
            return true;
        }

        private void ExportMap(bool toConsole)
        {
            if (this.openMapDialog.ShowDialog((IWin32Window)this) != DialogResult.OK || this.folderBrowserDialog.ShowDialog((IWin32Window)this) != DialogResult.OK)
                return;
            EditorDocument.Export(this.openMapDialog.FileName, this.folderBrowserDialog.SelectedPath, toConsole);
        }

        public void ToggleIngame()
        {
            bool isIngame = Editor.IsIngame;
            if (isIngame)
            {
                this.CurrentTool = (ITool)null;
                this.statusCaption.Text = Localizer.Localize("EDITOR_STATUS_INGAME");
            }
            else
                this.statusCaption.Text = Localizer.Localize("EDITOR_STATUS_READY");
            this.Viewport.CaptureMouse = isIngame;
            foreach (Control control in base.Controls)
            {
                if (control != this.Viewport && control != this.statusBar)
                {
                    control.Enabled = !isIngame;
                    control.Visible = !isIngame;
                }
            }
            this.Viewport.Focus();
        }

        private void menuItem_newMap_Activate(object sender, EventArgs e)
        {
            this.NewMap();
        }

        private void menuItem_newWilderness_Activate(object sender, EventArgs e)
        {
            this.NewMap();
            using (PromptInventory promptInventory = new PromptInventory())
            {
                promptInventory.Root = WildernessInventory.Instance.Root;
                if (promptInventory.ShowDialog((IWin32Window)this) == DialogResult.OK)
                    Wilderness.RunScriptEntry((WildernessInventory.Entry)promptInventory.Value);
            }
        }

        private void menuItem_loadMap_Activate(object sender, EventArgs e)
        {
            this.LoadMap((string)null, (EditorDocument.LoadCompletedCallback)null);
        }

        private void menuItem_saveMap_Activate(object sender, EventArgs e)
        {
            this.SaveMap(false, false, (EditorDocument.SaveCompletedCallback)null);
        }

        private void menuItem_saveMapAs_Activate(object sender, EventArgs e)
        {
            this.SaveMap(true, false, (EditorDocument.SaveCompletedCallback)null);
        }

        private void menuItem_exit_Activate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuItem_Undo_Activate(object sender, EventArgs e)
        {
            UndoManager.Undo();
        }

        private void menuItem_Redo_Activate(object sender, EventArgs e)
        {
            UndoManager.Redo();
        }

        private void menuItem_viewToolParameters_Activate(object sender, EventArgs e)
        {
            this.toolParametersDock.Open();
        }

        private void menuItem_viewEditorSettings_Activate(object sender, EventArgs e)
        {
            this.editorSettingsDock.Open();
        }

        private void menuItem_viewContextHelp_Activate(object sender, EventArgs e)
        {
            this.contextHelpDock.Open();
        }

        private void menuItem_TestIngame_Activate(object sender, EventArgs e)
        {
            Editor.ToggleIngame();
        }

        private void menuItem_OpenCodeEditor_Activate(object sender, EventArgs e)
        {
            if (CodeEditor.Instance == null)
                new CodeEditor();
            CodeEditor.Instance.Show();
        }

        private void menuItem_FlushCache_Activate(object sender, EventArgs e)
        {
            ObjectRenderer.ClearCache();
        }

        private void menuItem_ResetLayout_Activate(object sender, EventArgs e)
        {
            try
            {
                this.sandDockManager.SetLayout(Resources.DefaultSandDockLayout);
            }
            catch (Exception ex)
            {
            }
            try
            {
                this.sandBarManager.SetLayout(Resources.DefaultSandBarLayout);
            }
            catch (Exception ex)
            {
            }
        }

        private void menuItem_ExportToPC_Activate(object sender, EventArgs e)
        {
            this.ExportMap(false);
        }

        private void menuItem_ExportToConsole_Activate(object sender, EventArgs e)
        {
            this.ExportMap(true);
        }

        private void menuItem_visitWebsite_Activate(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://www.farcrygame.com");
            }
            catch (Exception ex)
            {
            }
        }

        private void menuItem_about_Activate(object sender, EventArgs e)
        {
            using (SplashForm splashForm = new SplashForm())
            {
                splashForm.AboutMode = true;
                int num = (int)splashForm.ShowDialog((IWin32Window)this);
            }
        }

        private void statusBarContextMenu_Opening(object sender, CancelEventArgs e)
        {
            Point position = Cursor.Position;
            this.m_helpString = (string)null;
            if (this.statusBar.RectangleToScreen(this.statusBarFpsItem.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_PERFORMANCE");
            else if (this.statusBar.RectangleToScreen(this.statusBarObjectUsage.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_OBJECT_COST");
            else if (this.statusBar.RectangleToScreen(this.statusBarMemoryUsage.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_MEMORY_USAGE");
            else if (this.statusBar.RectangleToScreen(this.statusBarCursorPos.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_CURSOR_POS");
            else if (this.statusBar.RectangleToScreen(this.statusBarCameraPos.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_CAMERA_POS");
            else if (this.statusBar.RectangleToScreen(this.statusBarCameraSpeed.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_CAMERA_SPEED");
            else if (this.statusBar.RectangleToScreen(this.statusCaption.Bounds).Contains(position))
                this.m_helpString = Localizer.Localize("HELP_STATUSBAR_STATE");
        }

        private void whatsThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.m_helpString != null)
            {
                int num1 = (int)MessageBox.Show((IWin32Window)this, this.m_helpString, Localizer.Localize("HELP_WHATS_THIS"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                int num2 = (int)MessageBox.Show((IWin32Window)this, "Help text not defined.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void LoadSettings()
        {
            using (RegistryKey registrySettings = Editor.GetRegistrySettings())
            {
                Rectangle rectangle = new Rectangle()
                {
                    X = Editor.GetRegistryInt(registrySettings, "MainFormX", this.Bounds.X),
                    Y = Editor.GetRegistryInt(registrySettings, "MainFormY", this.Bounds.Y),
                    Width = Editor.GetRegistryInt(registrySettings, "MainFormW", this.Bounds.Width),
                    Height = Editor.GetRegistryInt(registrySettings, "MainFormH", this.Bounds.Height)
                };
                foreach (Screen allScreen in Screen.AllScreens)
                {
                    if (allScreen.Bounds.IntersectsWith(rectangle))
                    {
                        this.Bounds = rectangle;
                        this.StartPosition = FormStartPosition.Manual;
                        break;
                    }
                }
                try
                {
                    this.sandDockManager.SetLayout(Editor.GetRegistryString(registrySettings, "SandDock", Resources.DefaultSandDockLayout));
                }
                catch (Exception ex)
                {
                }
                try
                {
                    this.sandBarManager.SetLayout(Editor.GetRegistryString(registrySettings, "SandBar", Resources.DefaultSandBarLayout));
                }
                catch (Exception ex)
                {
                }
                foreach (TD.SandBar.ToolBar toolBar in this.sandBarManager.GetToolBars())
                {
                    if (toolBar.Parent != null)
                        toolBar.Parent.Visible = true;
                }
                if (this.menuBar.Visible)
                    return;
                foreach (TD.SandBar.ToolBar toolBar in this.sandBarManager.GetToolBars())
                    toolBar.Visible = true;
            }
        }

        private void SaveSettings()
        {
            using (RegistryKey registrySettings = Editor.GetRegistrySettings())
            {
                registrySettings.SetValue("MainFormX", (object)this.Bounds.X);
                registrySettings.SetValue("MainFormY", (object)this.Bounds.Y);
                registrySettings.SetValue("MainFormW", (object)this.Bounds.Width);
                registrySettings.SetValue("MainFormH", (object)this.Bounds.Height);
                registrySettings.SetValue("SandDock", (object)this.sandDockManager.GetLayout());
                registrySettings.SetValue("SandBar", (object)this.sandBarManager.GetLayout());
            }
        }

        private void UpdateCurrentTool()
        {
            this.toolParametersDock.Tool = this.CurrentTool;
            this.contextHelpDock.Tool = this.CurrentTool;
            this.UpdateToolbar();
        }

        private void UpdateTitleBar()
        {
            this.Text = (this.m_documentPath != null ? Path.GetFileNameWithoutExtension(this.m_documentPath) : Localizer.Localize("EDITOR_UNTITLED")) + " - " + Localizer.Localize("EDITOR_NAME");
        }

        private void Localize()
        {
            Localizer.Localize(this.menuBar);
            this.toolBarMain.Text = Localizer.Localize(this.toolBarMain.Text);
            this.whatsThisToolStripMenuItem.Text = Localizer.Localize(this.whatsThisToolStripMenuItem.Text);
            SandBarLanguage.AddRemoveButtonsText = Localizer.Localize("SANDBAR_ADDREMOVEBUTTONSTEXT");
            SandBarLanguage.CloseMenuText = Localizer.Localize("SANDBAR_CLOSEMENUTEXT");
            SandBarLanguage.CloseWindowText = Localizer.Localize("SANDBAR_CLOSEWINDOWTEXT");
            SandBarLanguage.MaximizeMenuText = Localizer.Localize("SANDBAR_MAXIMIZEMENUTEXT");
            SandBarLanguage.MinimizeMenuText = Localizer.Localize("SANDBAR_MINIMIZEMENUTEXT");
            SandBarLanguage.MinimizeWindowText = Localizer.Localize("SANDBAR_MINIMIZEWINDOWTEXT");
            SandBarLanguage.MoveMenuText = Localizer.Localize("SANDBAR_MOVEMENUTEXT");
            SandBarLanguage.RestoreMenuText = Localizer.Localize("SANDBAR_RESTOREMENUTEXT");
            SandBarLanguage.RestoreWindowText = Localizer.Localize("SANDBAR_RESTOREWINDOWTEXT");
            SandBarLanguage.SizeMenuText = Localizer.Localize("SANDBAR_SIZEMENUTEXT");
            SandBarLanguage.ToolbarOptionsText = Localizer.Localize("SANDBAR_TOOLBAROPTIONSTEXT");
            SandDockLanguage.AutoHideText = Localizer.Localize("SANDDOCK_AUTOHIDETEXT");
            SandDockLanguage.CloseText = Localizer.Localize("SANDDOCK_CLOSETEXT");
            SandDockLanguage.ScrollLeftText = Localizer.Localize("SANDDOCK_SCROLLLEFTTEXT");
            SandDockLanguage.ScrollRightText = Localizer.Localize("SANDDOCK_SCROLLRIGHTTEXT");
            SandDockLanguage.WindowPositionText = Localizer.Localize("SANDDOCK_WINDOWPOSITIONTEXT");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Localize();
            this.InitCameraSpeed();
            this.InitializeDocks();
            this.InitializeTools();
            this.LoadSettings();
            this.UpdateCurrentTool();
            this.UpdateTitleBar();
            this.openMapDialog.InitialDirectory = Engine.PersonalPath;
            this.saveMapDialog.InitialDirectory = Engine.PersonalPath;
            EditorDocument.Reset();
        }

        public void PostLoad()
        {
            if (Program.GetMapArgument() == null)
                return;
            this.LoadMapInternal(Program.GetMapArgument(), (EditorDocument.LoadCompletedCallback)null);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Editor.IsIngame)
                Editor.ToggleIngame();
            if (this.m_closeSaveConfirmed || this.PromptSave((EditorDocument.SaveCompletedCallback)(success =>
            {
                if (!success)
                    return;
                this.m_closeSaveConfirmed = true;
                this.Close();
            })))
                this.SaveSettings();
            else
                e.Cancel = true;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Engine.Close();
        }

        private void MainForm_HandleCreated(object sender, EventArgs e)
        {
            Win32.SetProp(this.Handle, Program.programGuid, this.Handle);
        }

        private void MainForm_HandleDestroyed(object sender, EventArgs e)
        {
            Win32.RemoveProp(this.Handle, Program.programGuid);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            this.Viewport.UpdateFocus();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            this.Viewport.UpdateFocus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F10 && Editor.Viewport.Focused)
            {
                this.OnKeyEvent(Editor.KeyEvent.KeyDown, new KeyEventArgs(Keys.F10));
                this.OnKeyEvent(Editor.KeyEvent.KeyUp, new KeyEventArgs(Keys.F10));
                return true;
            }
            return Editor.IsIngame || base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 74 && Engine.Initialized)
            {
                string stringUni = Marshal.PtrToStringUni(((Win32.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(Win32.COPYDATASTRUCT))).lpData);
                this.LoadMap(stringUni, (EditorDocument.LoadCompletedCallback)null);
            }
            base.WndProc(ref m);
        }

        private void menuBar_EnterMenuLoop(object sender, EventArgs e)
        {
            this.viewport.ForceRefresh = true;
        }

        private void menuBar_ExitMenuLoop(object sender, EventArgs e)
        {
            this.viewport.ForceRefresh = false;
        }

        private void timerUIUpdate_Tick(object sender, EventArgs e)
        {
            if (!Engine.Initialized)
                return;
            bool flag1 = UndoManager.UndoCount > 0;
            bool flag2 = UndoManager.RedoCount > 0;
            if (this.menuItem_Undo.Enabled != flag1)
                this.menuItem_Undo.Enabled = flag1;
            if (this.menuItem_Redo.Enabled == flag2)
                return;
            this.menuItem_Redo.Enabled = flag2;
        }

        private void InitCameraSpeed()
        {
            Camera.Speed = 16f;
            float num1 = 2f;
            for (int index = 0; index < 6; ++index)
            {
                ToolStripItem toolStripItem = this.cameraSpeedStrip.Items.Add(num1.ToString());
                toolStripItem.Tag = (object)num1;
                toolStripItem.Click += new EventHandler(this.cameraSpeedItem_Click);
                num1 *= 2f;
            }
            this.cameraSpeedStrip.Items.Add(Localizer.Localize("EDITOR_CAMERA_SPEED_CUSTOM")).Click += new EventHandler(this.cameraSpeedItem_Click);
            this.UpdateCameraSpeed();
        }

        private void UpdateCameraSpeed()
        {
            this.statusBarCameraSpeed.Text = Camera.Speed.ToString();
        }

        private void cameraSpeedItem_Click(object sender, EventArgs e)
        {
            object tag = ((ToolStripItem)sender).Tag;
            float num;
            if (tag != null)
            {
                num = (float)tag;
            }
            else
            {
                using (PromptForm promptForm = new PromptForm(Localizer.Localize("EDITOR_CAMERA_SPEED_PROMPT"), Localizer.Localize("EDITOR_CAMERA_SPEED_PROMPT_TITLE")))
                {
                    promptForm.Validation = PromptForm.GetFloatValidation(1f / 1000f, 50f);
                    if (promptForm.ShowDialog((IWin32Window)this) != DialogResult.OK)
                        return;
                    num = float.Parse(promptForm.Input);
                }
            }
            Camera.Speed = num;
            this.UpdateCameraSpeed();
        }

        public void OnInputAcquire()
        {
        }

        public void OnInputRelease()
        {
        }

        public bool OnMouseEvent(Editor.MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            return false;
        }

        public bool OnKeyEvent(Editor.KeyEvent keyEvent, KeyEventArgs keyEventArgs)
        {
            if (keyEvent == Editor.KeyEvent.KeyDown)
            {
                if (keyEventArgs.KeyCode == Keys.Escape)
                {
                    this.CurrentTool = (ITool)null;
                    return true;
                }
                ButtonItem buttonItem;
                if (this.m_toolShortcuts.TryGetValue(keyEventArgs.KeyCode, out buttonItem))
                {
                    this.CurrentTool = (ITool)buttonItem.Tag;
                    return true;
                }
            }
            return false;
        }

        public void OnEditorEvent(uint eventType, IntPtr eventPtr)
        {
        }

        public void Update(float dt)
        {
            this.m_lastUpdate += dt;
            if ((double)this.m_lastUpdate >= 0.1)
            {
                this.m_lastUpdate = 0.0f;
                bool isLoadPending = Editor.IsLoadPending;
                if (this.m_isLoading != isLoadPending)
                {
                    this.m_isLoading = isLoadPending;
                    if (isLoadPending)
                    {
                        this.statusCaption.Image = (Image)Resources.hourglass;
                        this.statusCaption.Text = Localizer.Localize("EDITOR_STATUS_LOADING");
                    }
                    else
                    {
                        this.statusCaption.Image = (Image)null;
                        this.statusCaption.Text = Localizer.Localize("EDITOR_STATUS_READY");
                    }
                }
                float num1 = (float)BudgetManager.MemoryUsage / 1048576f;
                if ((double)Math.Abs(this.m_currentMemoryUsage - num1) >= 0.1)
                {
                    this.m_currentMemoryUsage = num1;
                    this.statusBarMemoryUsage.Text = num1.ToString("F1");
                }
                float objectUsage = BudgetManager.ObjectUsage;
                if ((double)Math.Abs(this.m_currentObjectUsage - objectUsage) >= 0.1)
                {
                    this.m_currentObjectUsage = objectUsage;
                    this.statusBarObjectUsage.Text = objectUsage.ToString("F0");
                }
                float num2 = 1f / Editor.FrameTime;
                if ((double)num2 != (double)this.m_currentFps)
                {
                    this.m_currentFps = num2;
                    this.statusBarFpsItem.Text = num2.ToString("F1") + " fps";
                }
                Vec3 position = Camera.Position;
                if (position != this.m_currentCameraPos)
                {
                    this.m_currentCameraPos = position;
                    this.statusBarCameraPos.Text = position.ToString("F2");
                }
                Vec3 hitPos;
                bool flag = this.m_cursorPhysics ? Editor.RayCastPhysicsFromMouse(out hitPos) : Editor.RayCastTerrainFromMouse(out hitPos);
                if (this.m_currentCursorValid != flag || hitPos != this.m_currentCursorPos)
                {
                    this.m_currentCursorPos = hitPos;
                    this.m_currentCursorValid = flag;
                    if (flag)
                        this.statusBarCursorPos.Text = hitPos.ToString("F2");
                    else
                        this.statusBarCursorPos.Text = Localizer.Localize("PARAM_NA");
                }
            }
            ObjectRenderer.Update(dt);
        }

        protected override void Dispose(bool disposing)
        {
            this.DisposeInternal();
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.sandDockManager = new TD.SandDock.SandDockManager();
            this.sandBarManager = new TD.SandBar.SandBarManager(this.components);
            this.leftSandBarDock = new TD.SandBar.ToolBarContainer();
            this.rightSandBarDock = new TD.SandBar.ToolBarContainer();
            this.bottomSandBarDock = new TD.SandBar.ToolBarContainer();
            this.topSandBarDock = new TD.SandBar.ToolBarContainer();
            this.menuBar = new TD.SandBar.MenuBar();
            this.menuBarItem1 = new TD.SandBar.MenuBarItem();
            this.menuItem_newMap = new TD.SandBar.MenuButtonItem();
            this.menuItem_newWilderness = new TD.SandBar.MenuButtonItem();
            this.menuItem_loadMap = new TD.SandBar.MenuButtonItem();
            this.menuItem_saveMap = new TD.SandBar.MenuButtonItem();
            this.menuItem_saveMapAs = new TD.SandBar.MenuButtonItem();
            this.menuItem_exit = new TD.SandBar.MenuButtonItem();
            this.menuBarItem3 = new TD.SandBar.MenuBarItem();
            this.menuItem_Undo = new TD.SandBar.MenuButtonItem();
            this.menuItem_Redo = new TD.SandBar.MenuButtonItem();
            this.menuBarItem2 = new TD.SandBar.MenuBarItem();
            this.menuItem_viewToolParameters = new TD.SandBar.MenuButtonItem();
            this.menuItem_viewEditorSettings = new TD.SandBar.MenuButtonItem();
            this.menuItem_viewContextHelp = new TD.SandBar.MenuButtonItem();
            this.menuBarItem4 = new TD.SandBar.MenuBarItem();
            this.menuItem_TestIngame = new TD.SandBar.MenuButtonItem();
            this.menuBarItem5 = new TD.SandBar.MenuBarItem();
            this.menuItem_OpenCodeEditor = new TD.SandBar.MenuButtonItem();
            this.menuItem_ExportToPC = new TD.SandBar.MenuButtonItem();
            this.menuItem_ExportToConsole = new TD.SandBar.MenuButtonItem();
            this.menuItem_FlushCache = new TD.SandBar.MenuButtonItem();
            this.menuItem_ResetLayout = new TD.SandBar.MenuButtonItem();
            this.menuBarItem6 = new TD.SandBar.MenuBarItem();
            this.menuItem_visitWebsite = new TD.SandBar.MenuButtonItem();
            this.menuItem_about = new TD.SandBar.MenuButtonItem();
            this.toolBarMain = new TD.SandBar.ToolBar();
            this.buttonItem1 = new TD.SandBar.ButtonItem();
            this.buttonItem2 = new TD.SandBar.ButtonItem();
            this.buttonItem3 = new TD.SandBar.ButtonItem();
            this.buttonItem4 = new TD.SandBar.ButtonItem();
            this.buttonItem5 = new TD.SandBar.ButtonItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.openMapDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveMapDialog = new System.Windows.Forms.SaveFileDialog();
            this.timerUIUpdate = new System.Windows.Forms.Timer(this.components);
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusBarContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.whatsThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarCameraSpeed = new System.Windows.Forms.ToolStripDropDownButton();
            this.cameraSpeedStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.statusBarCameraPos = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarCursorPos = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarMemoryUsage = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarObjectUsage = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBarFpsItem = new System.Windows.Forms.ToolStripStatusLabel();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.viewport = new FC2Editor.UI.ViewportControl();
            this.topSandBarDock.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.statusBarContextMenu.SuspendLayout();
            this.SuspendLayout();
            this.sandDockManager.DockSystemContainer = (System.Windows.Forms.Control)this;
            this.sandDockManager.OwnerForm = this;
            this.sandBarManager.OwnerForm = this;
            this.sandBarManager.Renderer = new TD.SandBar.WhidbeyRenderer();
            this.leftSandBarDock.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftSandBarDock.Guid = new System.Guid("5af42533-b4bf-4ff9-b113-19c06ff822ad");
            this.leftSandBarDock.Location = new System.Drawing.Point(0, 49);
            this.leftSandBarDock.Manager = this.sandBarManager;
            this.leftSandBarDock.Name = "leftSandBarDock";
            this.leftSandBarDock.Size = new System.Drawing.Size(0, 592);
            this.leftSandBarDock.TabIndex = 0;
            this.rightSandBarDock.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightSandBarDock.Guid = new System.Guid("dd1d865e-52de-4df1-b60b-8d95778c6a99");
            this.rightSandBarDock.Location = new System.Drawing.Point(1007, 49);
            this.rightSandBarDock.Manager = this.sandBarManager;
            this.rightSandBarDock.Name = "rightSandBarDock";
            this.rightSandBarDock.Size = new System.Drawing.Size(0, 592);
            this.rightSandBarDock.TabIndex = 1;
            this.bottomSandBarDock.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomSandBarDock.Guid = new System.Guid("241b4d86-e08e-4fbd-9187-85417e2a6762");
            this.bottomSandBarDock.Location = new System.Drawing.Point(0, 641);
            this.bottomSandBarDock.Manager = this.sandBarManager;
            this.bottomSandBarDock.Name = "bottomSandBarDock";
            this.bottomSandBarDock.Size = new System.Drawing.Size(1007, 0);
            this.bottomSandBarDock.TabIndex = 2;
            this.topSandBarDock.Controls.Add((System.Windows.Forms.Control)this.menuBar);
            this.topSandBarDock.Controls.Add((System.Windows.Forms.Control)this.toolBarMain);
            this.topSandBarDock.Dock = System.Windows.Forms.DockStyle.Top;
            this.topSandBarDock.Guid = new System.Guid("9d4d899c-2f1c-48a5-b6b3-c2965fee85b0");
            this.topSandBarDock.Location = new System.Drawing.Point(0, 0);
            this.topSandBarDock.Manager = this.sandBarManager;
            this.topSandBarDock.Name = "topSandBarDock";
            this.topSandBarDock.Size = new System.Drawing.Size(1007, 49);
            this.topSandBarDock.TabIndex = 3;
            this.menuBar.Guid = new System.Guid("67d4f929-8914-4b23-9c2d-b9539a54dd9b");
            this.menuBar.Items.AddRange(new TD.SandBar.ToolbarItemBase[6]
            {
                (TD.SandBar.ToolbarItemBase) this.menuBarItem1,
                (TD.SandBar.ToolbarItemBase) this.menuBarItem3,
                (TD.SandBar.ToolbarItemBase) this.menuBarItem2,
                (TD.SandBar.ToolbarItemBase) this.menuBarItem4,
                (TD.SandBar.ToolbarItemBase) this.menuBarItem5,
                (TD.SandBar.ToolbarItemBase) this.menuBarItem6
            });
            this.menuBar.Location = new System.Drawing.Point(2, 0);
            this.menuBar.Movable = false;
            this.menuBar.Name = "menuBar";
            this.menuBar.OwnerForm = this;
            this.menuBar.Size = new System.Drawing.Size(1005, 23);
            this.menuBar.TabIndex = 0;
            this.menuBar.Text = "menuBar1";
            this.menuBar.EnterMenuLoop += new System.EventHandler(this.menuBar_EnterMenuLoop);
            this.menuBar.ExitMenuLoop += new System.EventHandler(this.menuBar_ExitMenuLoop);
            this.menuBarItem1.Items.AddRange(new TD.SandBar.ToolbarItemBase[6]
            {
                (TD.SandBar.ToolbarItemBase) this.menuItem_newMap,
                (TD.SandBar.ToolbarItemBase) this.menuItem_newWilderness,
                (TD.SandBar.ToolbarItemBase) this.menuItem_loadMap,
                (TD.SandBar.ToolbarItemBase) this.menuItem_saveMap,
                (TD.SandBar.ToolbarItemBase) this.menuItem_saveMapAs,
                (TD.SandBar.ToolbarItemBase) this.menuItem_exit
            });
            this.menuBarItem1.Text = "MENU_FILE";
            this.menuItem_newMap.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.menuItem_newMap.Text = "MENUITEM_FILE_NEW_MAP";
            this.menuItem_newMap.Activate += new System.EventHandler(this.menuItem_newMap_Activate);
            this.menuItem_newWilderness.Text = "MENUITEM_FILE_NEW_WILDERNESS";
            this.menuItem_newWilderness.Activate += new System.EventHandler(this.menuItem_newWilderness_Activate);
            this.menuItem_loadMap.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuItem_loadMap.Text = "MENUITEM_FILE_LOAD_MAP";
            this.menuItem_loadMap.Activate += new System.EventHandler(this.menuItem_loadMap_Activate);
            this.menuItem_saveMap.BeginGroup = true;
            this.menuItem_saveMap.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuItem_saveMap.Text = "MENUITEM_FILE_SAVE_MAP";
            this.menuItem_saveMap.Activate += new System.EventHandler(this.menuItem_saveMap_Activate);
            this.menuItem_saveMapAs.Text = "MENUITEM_FILE_SAVE_MAP_AS";
            this.menuItem_saveMapAs.Activate += new System.EventHandler(this.menuItem_saveMapAs_Activate);
            this.menuItem_exit.BeginGroup = true;
            this.menuItem_exit.Text = "MENUITEM_FILE_EXIT";
            this.menuItem_exit.Activate += new System.EventHandler(this.menuItem_exit_Activate);
            this.menuBarItem3.Items.AddRange(new TD.SandBar.ToolbarItemBase[2]
            {
                (TD.SandBar.ToolbarItemBase) this.menuItem_Undo,
                (TD.SandBar.ToolbarItemBase) this.menuItem_Redo
            });
            this.menuBarItem3.Text = "MENU_EDIT";
            this.menuItem_Undo.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
            this.menuItem_Undo.Text = "MENUITEM_EDIT_UNDO";
            this.menuItem_Undo.Activate += new System.EventHandler(this.menuItem_Undo_Activate);
            this.menuItem_Redo.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftZ;
            this.menuItem_Redo.Text = "MENUITEM_EDIT_REDO";
            this.menuItem_Redo.Activate += new System.EventHandler(this.menuItem_Redo_Activate);
            this.menuBarItem2.Items.AddRange(new TD.SandBar.ToolbarItemBase[3]
            {
                (TD.SandBar.ToolbarItemBase) this.menuItem_viewToolParameters,
                (TD.SandBar.ToolbarItemBase) this.menuItem_viewEditorSettings,
                (TD.SandBar.ToolbarItemBase) this.menuItem_viewContextHelp
            });
            this.menuBarItem2.Text = "MENU_VIEW";
            this.menuItem_viewToolParameters.Text = "DOCK_TOOL_PARAMETERS";
            this.menuItem_viewToolParameters.Activate += new System.EventHandler(this.menuItem_viewToolParameters_Activate);
            this.menuItem_viewEditorSettings.Text = "DOCK_EDITOR_SETTINGS";
            this.menuItem_viewEditorSettings.Activate += new System.EventHandler(this.menuItem_viewEditorSettings_Activate);
            this.menuItem_viewContextHelp.Text = "DOCK_CONTEXT_HELP";
            this.menuItem_viewContextHelp.Activate += new System.EventHandler(this.menuItem_viewContextHelp_Activate);
            this.menuBarItem4.Items.AddRange(new TD.SandBar.ToolbarItemBase[1]
            {
                (TD.SandBar.ToolbarItemBase) this.menuItem_TestIngame
            });
            this.menuBarItem4.Text = "MENU_GAME";
            this.menuItem_TestIngame.Shortcut = System.Windows.Forms.Shortcut.CtrlG;
            this.menuItem_TestIngame.Text = "MENUITEM_GAME_TEST_INGAME";
            this.menuItem_TestIngame.Activate += new System.EventHandler(this.menuItem_TestIngame_Activate);
            this.menuBarItem5.Items.AddRange(new TD.SandBar.ToolbarItemBase[5]
            {
                (TD.SandBar.ToolbarItemBase) this.menuItem_OpenCodeEditor,
                (TD.SandBar.ToolbarItemBase) this.menuItem_ExportToPC,
                (TD.SandBar.ToolbarItemBase) this.menuItem_ExportToConsole,
                (TD.SandBar.ToolbarItemBase) this.menuItem_FlushCache,
                (TD.SandBar.ToolbarItemBase) this.menuItem_ResetLayout
            });
            this.menuBarItem5.Text = "MENU_ADVANCED";
            this.menuItem_OpenCodeEditor.Text = "MENUITEM_ADVANCED_CODE_EDITOR";
            this.menuItem_OpenCodeEditor.Activate += new System.EventHandler(this.menuItem_OpenCodeEditor_Activate);
            this.menuItem_ExportToPC.BeginGroup = true;
            this.menuItem_ExportToPC.Text = "MENUITEM_ADVANCED_EXPORT_TO_PC";
            this.menuItem_ExportToPC.Activate += new System.EventHandler(this.menuItem_ExportToPC_Activate);
            this.menuItem_ExportToConsole.Text = "MENUITEM_ADVANCED_EXPORT_TO_CONSOLE";
            this.menuItem_ExportToConsole.Activate += new System.EventHandler(this.menuItem_ExportToConsole_Activate);
            this.menuItem_FlushCache.BeginGroup = true;
            this.menuItem_FlushCache.Text = "MENUITEM_ADVANCED_FLUSH_CACHE";
            this.menuItem_FlushCache.Activate += new System.EventHandler(this.menuItem_FlushCache_Activate);
            this.menuItem_ResetLayout.Text = "MENUITEM_ADVANCED_RESET_LAYOUT";
            this.menuItem_ResetLayout.Activate += new System.EventHandler(this.menuItem_ResetLayout_Activate);
            this.menuBarItem6.Items.AddRange(new TD.SandBar.ToolbarItemBase[2]
            {
                (TD.SandBar.ToolbarItemBase) this.menuItem_visitWebsite,
                (TD.SandBar.ToolbarItemBase) this.menuItem_about
            });
            this.menuBarItem6.Text = "MENU_HELP";
            this.menuItem_visitWebsite.Text = "MENUITEM_HELP_VISIT_WEBSITE";
            this.menuItem_visitWebsite.Activate += new System.EventHandler(this.menuItem_visitWebsite_Activate);
            this.menuItem_about.BeginGroup = true;
            this.menuItem_about.Text = "MENUITEM_HELP_ABOUT";
            this.menuItem_about.Activate += new System.EventHandler(this.menuItem_about_Activate);
            this.toolBarMain.DockLine = 1;
            this.toolBarMain.Guid = new System.Guid("472238c1-4ade-4ff2-ba4f-4429ed30f50f");
            this.toolBarMain.Items.AddRange(new TD.SandBar.ToolbarItemBase[5]
            {
                (TD.SandBar.ToolbarItemBase) this.buttonItem1,
                (TD.SandBar.ToolbarItemBase) this.buttonItem2,
                (TD.SandBar.ToolbarItemBase) this.buttonItem3,
                (TD.SandBar.ToolbarItemBase) this.buttonItem4,
                (TD.SandBar.ToolbarItemBase) this.buttonItem5
            });
            this.toolBarMain.Location = new System.Drawing.Point(2, 23);
            this.toolBarMain.Name = "toolBarMain";
            this.toolBarMain.Size = new System.Drawing.Size(146, 26);
            this.toolBarMain.TabIndex = 1;
            this.toolBarMain.Text = "TOOLBAR_MAIN";
            this.toolBarMain.EnterMenuLoop += new System.EventHandler(this.menuBar_EnterMenuLoop);
            this.toolBarMain.ExitMenuLoop += new System.EventHandler(this.menuBar_ExitMenuLoop);
            this.buttonItem1.BuddyMenu = (TD.SandBar.MenuButtonItem)this.menuItem_newMap;
            this.buttonItem1.Image = FC2Editor.Properties.Resources.newMap;
            this.buttonItem2.BuddyMenu = (TD.SandBar.MenuButtonItem)this.menuItem_loadMap;
            this.buttonItem2.Image = FC2Editor.Properties.Resources.openMap;
            this.buttonItem3.BuddyMenu = (TD.SandBar.MenuButtonItem)this.menuItem_saveMap;
            this.buttonItem3.Image = FC2Editor.Properties.Resources.save;
            this.buttonItem4.BeginGroup = true;
            this.buttonItem4.BuddyMenu = (TD.SandBar.MenuButtonItem)this.menuItem_Undo;
            this.buttonItem4.Image = FC2Editor.Properties.Resources.undo;
            this.buttonItem5.BuddyMenu = (TD.SandBar.MenuButtonItem)this.menuItem_Redo;
            this.buttonItem5.Image = FC2Editor.Properties.Resources.redo;
            this.openMapDialog.DefaultExt = "fc2map";
            this.openMapDialog.Filter = "Far Cry 2 maps (*.fc2map)|*.fc2map|All files (*.*)|*.*";
            this.openMapDialog.Title = "Open Far Cry 2 map";
            this.saveMapDialog.DefaultExt = "fc2map";
            this.saveMapDialog.Filter = "Far Cry 2 maps (*.fc2map)|*.fc2map|All files (*.*)|*.*";
            this.saveMapDialog.Title = "Save Far Cry 2 map";
            this.timerUIUpdate.Enabled = true;
            this.timerUIUpdate.Tick += new System.EventHandler(this.timerUIUpdate_Tick);
            this.statusBar.ContextMenuStrip = this.statusBarContextMenu;
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[7]
            {
                (System.Windows.Forms.ToolStripItem) this.statusCaption,
                (System.Windows.Forms.ToolStripItem) this.statusBarCameraSpeed,
                (System.Windows.Forms.ToolStripItem) this.statusBarCameraPos,
                (System.Windows.Forms.ToolStripItem) this.statusBarCursorPos,
                (System.Windows.Forms.ToolStripItem) this.statusBarMemoryUsage,
                (System.Windows.Forms.ToolStripItem) this.statusBarObjectUsage,
                (System.Windows.Forms.ToolStripItem) this.statusBarFpsItem
            });
            this.statusBar.Location = new System.Drawing.Point(0, 641);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(1007, 22);
            this.statusBar.TabIndex = 7;
            this.statusBar.Text = "statusStrip1";
            this.statusBarContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[1]
            {
                (System.Windows.Forms.ToolStripItem) this.whatsThisToolStripMenuItem
            });
            this.statusBarContextMenu.Name = "statusBarContextMenu";
            this.statusBarContextMenu.Size = new System.Drawing.Size(170, 26);
            this.statusBarContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.statusBarContextMenu_Opening);
            this.whatsThisToolStripMenuItem.Name = "whatsThisToolStripMenuItem";
            this.whatsThisToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.whatsThisToolStripMenuItem.Text = "HELP_WHATS_THIS";
            this.whatsThisToolStripMenuItem.Click += new System.EventHandler(this.whatsThisToolStripMenuItem_Click);
            this.statusCaption.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusCaption.Name = "statusCaption";
            this.statusCaption.Size = new System.Drawing.Size(442, 17);
            this.statusCaption.Spring = true;
            this.statusCaption.Text = "Ready";
            this.statusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarCameraSpeed.AutoSize = false;
            this.statusBarCameraSpeed.DropDown = this.cameraSpeedStrip;
            this.statusBarCameraSpeed.Image = (System.Drawing.Image)FC2Editor.Properties.Resources.speed;
            this.statusBarCameraSpeed.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarCameraSpeed.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.statusBarCameraSpeed.Name = "statusBarCameraSpeed";
            this.statusBarCameraSpeed.Size = new System.Drawing.Size(55, 20);
            this.statusBarCameraSpeed.Text = "0";
            this.statusBarCameraSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cameraSpeedStrip.Name = "cameraSpeedStrip";
            this.cameraSpeedStrip.OwnerItem = (System.Windows.Forms.ToolStripItem)this.statusBarCameraSpeed;
            this.cameraSpeedStrip.ShowImageMargin = false;
            this.cameraSpeedStrip.Size = new System.Drawing.Size(36, 4);
            this.statusBarCameraPos.AutoSize = false;
            this.statusBarCameraPos.Image = (System.Drawing.Image)FC2Editor.Properties.Resources.camera;
            this.statusBarCameraPos.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarCameraPos.Name = "statusBarCameraPos";
            this.statusBarCameraPos.Size = new System.Drawing.Size(150, 17);
            this.statusBarCameraPos.Text = "(0, 0, 0)";
            this.statusBarCameraPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarCursorPos.AutoSize = false;
            this.statusBarCursorPos.Image = (System.Drawing.Image)FC2Editor.Properties.Resources.cursor;
            this.statusBarCursorPos.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarCursorPos.Name = "statusBarCursorPos";
            this.statusBarCursorPos.Size = new System.Drawing.Size(150, 17);
            this.statusBarCursorPos.Text = "(0, 0, 0)";
            this.statusBarCursorPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarMemoryUsage.AutoSize = false;
            this.statusBarMemoryUsage.Image = (System.Drawing.Image)FC2Editor.Properties.Resources.MemoryUsage;
            this.statusBarMemoryUsage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarMemoryUsage.Name = "statusBarMemoryUsage";
            this.statusBarMemoryUsage.Size = new System.Drawing.Size(60, 17);
            this.statusBarMemoryUsage.Text = "0";
            this.statusBarMemoryUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarObjectUsage.AutoSize = false;
            this.statusBarObjectUsage.Image = (System.Drawing.Image)FC2Editor.Properties.Resources.ObjectUsage;
            this.statusBarObjectUsage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarObjectUsage.Name = "statusBarObjectUsage";
            this.statusBarObjectUsage.Size = new System.Drawing.Size(60, 17);
            this.statusBarObjectUsage.Text = "0";
            this.statusBarObjectUsage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarFpsItem.AutoSize = false;
            this.statusBarFpsItem.Image = (System.Drawing.Image)FC2Editor.Properties.Resources.frametime;
            this.statusBarFpsItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statusBarFpsItem.Name = "statusBarFpsItem";
            this.statusBarFpsItem.Size = new System.Drawing.Size(75, 17);
            this.statusBarFpsItem.Text = "0.0 fps";
            this.statusBarFpsItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.folderBrowserDialog.Description = "Select export folder";
            this.viewport.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.viewport.BlockNextKeyRepeats = false;
            this.viewport.CameraEnabled = true;
            this.viewport.CaptureMouse = false;
            this.viewport.CaptureWheel = false;
            this.viewport.Cursor = System.Windows.Forms.Cursors.Default;
            this.viewport.DefaultCursor = System.Windows.Forms.Cursors.Default;
            this.viewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewport.Location = new System.Drawing.Point(0, 49);
            this.viewport.Name = "viewport";
            this.viewport.Size = new System.Drawing.Size(1007, 592);
            this.viewport.TabIndex = 5;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1007, 663);
            this.Controls.Add((System.Windows.Forms.Control)this.viewport);
            this.Controls.Add((System.Windows.Forms.Control)this.leftSandBarDock);
            this.Controls.Add((System.Windows.Forms.Control)this.rightSandBarDock);
            this.Controls.Add((System.Windows.Forms.Control)this.bottomSandBarDock);
            this.Controls.Add((System.Windows.Forms.Control)this.topSandBarDock);
            this.Controls.Add((System.Windows.Forms.Control)this.statusBar);
            this.Name = "MainForm";
            this.Text = "Far Cry 2 Editor";
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.topSandBarDock.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.statusBarContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}