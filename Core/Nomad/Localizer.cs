using System;
using System.Runtime.InteropServices;
using TD.SandBar;

namespace FC2Editor.Core.Nomad
{
    internal static class Localizer
    {
        private static string LocalizeInternal(string section, string key)
        {
            return Marshal.PtrToStringUni(LocalizeText(section, key));
        }

        public static string Localize(string section, string key)
        {
            if (!Engine.Initialized)
            {
                return "!DLL_NOT_LOADED";
            }
            return LocalizeInternal(section, key);
        }

        public static string LocalizeCommon(string key)
        {
            return LocalizeInternal("InGameEditor", key);
        }

        public static string Localize(string key)
        {
            return LocalizeInternal("InGameEditor_PC", key);
        }

        public static void Localize(MenuButtonItem item)
        {
            item.Text = Localize(item.Text);
            foreach (MenuButtonItem subItem in item.Items)
            {
                Localize(subItem);
            }
        }

        public static void Localize(MenuBarItem item)
        {
            item.Text = Localize(item.Text);
            foreach (MenuButtonItem subItem in item.Items)
            {
                Localize(subItem);
            }
        }

        public static void Localize(MenuBar menuBar)
        {
            foreach (MenuBarItem item in menuBar.Items)
            {
                Localize(item);
            }
        }

        [DllImport("Dunia.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr LocalizeText(string section, string text);
    }
}