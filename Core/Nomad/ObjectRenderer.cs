using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class ObjectRenderer
    {
        public interface IListener
        {
            void ProcessObject(ObjectInventory.Entry entry, Image img);
        }

        private static List<IListener> m_listeners = new List<IListener>();
        private static bool IsSnapshotReady => FCE_ObjectRenderer_IsSnapshotReady();

        public static void RegisterListener(IListener listener)
        {
            if (!m_listeners.Contains(listener))
            {
                m_listeners.Add(listener);
            }
        }

        public static void UnregisterListener(IListener listener)
        {
            m_listeners.Remove(listener);
        }

        private static void TriggerListeners(ObjectInventory.Entry entry, Image img)
        {
            IListener[] array = m_listeners.ToArray();
            foreach (IListener listener in array)
            {
                listener.ProcessObject(entry, img);
            }
            img.Dispose();
        }

        public static void RequestObjectImage(ObjectInventory.Entry entry)
        {
            string cachePath = GetCacheDirectory() + entry.Id + ".png";
            if (File.Exists(cachePath))
            {
                try
                {
                    TriggerListeners(entry, Image.FromFile(cachePath));
                }
                catch
                {
                    // File might be corrupted, re-render
                    RenderObject(entry);
                }
            }
            else
            {
                RenderObject(entry);
            }
        }

        public static void Update(float dt)
        {
            if (IsSnapshotReady)
            {
                GetSnapshot(out Image img, out ObjectInventory.Entry entry);
                ClearSnapshot();
                TriggerListeners(entry, img);
            }
        }

        public static void Clear() => FCE_ObjectRenderer_Clear();
        private static bool ThumbnailDummy() => false;

        private static void GetSnapshot(out Image img, out ObjectInventory.Entry entry)
        {
            using (Image image = new Snapshot(FCE_ObjectRenderer_GetSnapshot()).GetImage())
            {
                img = image.GetThumbnailImage(128, 128, ThumbnailDummy, IntPtr.Zero);
            }

            entry = new ObjectInventory.Entry(FCE_ObjectRenderer_GetSnapshotEntry());
            string cacheDirectory = GetCacheDirectory();
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                img.Save(cacheDirectory + entry.Id + ".png");
            }
            catch (Exception)
            {
                // Ignore save errors
            }
        }

        private static void ClearSnapshot() => FCE_ObjectRenderer_ClearSnapshot();
        private static void RenderObject(ObjectInventory.Entry entry) => FCE_ObjectRenderer_RenderObject(entry.Pointer);
        private static string GetCacheDirectory() => Path.GetTempPath() + "\\FarCry2\\Editor\\";

        public static void ClearCache()
        {
            string cacheDirectory = GetCacheDirectory();
            if (Directory.Exists(cacheDirectory))
            {
                try
                {
                    Directory.Delete(cacheDirectory, true);
                }
                catch
                {
                    // Ignore errors if cache is in use
                }
            }
        }

        [DllImport("Dunia.dll")] private static extern void FCE_ObjectRenderer_Clear();
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_ObjectRenderer_IsSnapshotReady();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ObjectRenderer_GetSnapshot();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ObjectRenderer_GetSnapshotEntry();
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectRenderer_ClearSnapshot();
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectRenderer_RenderObject(IntPtr entry);
    }
}