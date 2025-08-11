using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FC2Editor.UI; // Note: Will be added later in this phase
using Microsoft.Win32;

namespace FC2Editor.Core.Nomad
{
    internal static class Editor
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UpdateCallbackDelegate(float dt);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallbackDelegate(uint eventType, IntPtr eventPtr);

        public enum ResultCode
        {
            None,
            Succeeded,
            Busy,
            CanceledByUser,
            Failed,
            MissingDLC,
            FileCorrupt
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void LoadCompletedCallbackDelegate(ResultCode resultCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SaveCompletedCallbackDelegate(ResultCode resultCode);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EnableUICallbackDelegate(bool enable);

        public enum MouseEvent
        {
            MouseDown,
            MouseUp,
            MouseMove,
            MouseMoveDelta,
            MouseWheel,
            MouseEnter,
            MouseLeave
        }

        public enum KeyEvent
        {
            KeyDown,
            KeyChar,
            KeyUp
        }

        private static UpdateCallbackDelegate m_delegateUpdateCallback;
        private static EventCallbackDelegate m_delegateEventCallback;
        private static LoadCompletedCallbackDelegate m_delegateLoadCompletedCallback;
        private static SaveCompletedCallbackDelegate m_delegateSaveCompletedCallback;
        private static EnableUICallbackDelegate m_delegateEnableUICallback;
        private static List<IInputSink> m_inputStack = new List<IInputSink>();

        public static bool IsActive => Win32.GetActiveWindow() != IntPtr.Zero && MainForm.Instance != null && MainForm.Instance.IsHandleCreated && !MainForm.Instance.IsDisposed && Win32.IsWindowEnabled(MainForm.Instance.Handle);

        public static bool IsLoadPending => FCE_Editor_IsLoadPending();
        public static float FrameTime => FCE_Editor_GetFrameTime();
        public static bool IsIngame => FCE_Editor_IsIngame();
        public static ViewportControl Viewport => MainForm.Instance.Viewport;

        public static void Init()
        {
            m_delegateUpdateCallback = UpdateCallback;
            FCE_Editor_Update_Callback(m_delegateUpdateCallback);
            m_delegateEventCallback = EventCallback;
            FCE_Editor_Event_Callback(m_delegateEventCallback);
            m_delegateLoadCompletedCallback = LoadCompletedCallback;
            FCE_Editor_LoadCompleted_Callback(m_delegateLoadCompletedCallback);
            m_delegateSaveCompletedCallback = SaveCompletedCallback;
            FCE_Editor_SaveCompleted_Callback(m_delegateSaveCompletedCallback);
            m_delegateEnableUICallback = EnableUICallback;
            FCE_Editor_EnableUI_Callback(m_delegateEnableUICallback);
            while (!FCE_Editor_IsInitialized())
            {
                Engine.TickDuniaEngine();
            }
        }

        private static void UpdateCallback(float dt)
        {
            OnUpdate(dt);
        }

        private static void EventCallback(uint eventType, IntPtr eventPtr)
        {
            OnEditorEvent(eventType, eventPtr);
        }

        private static void LoadCompletedCallback(ResultCode resultCode)
        {
            EditorDocument.OnLoadCompleted(resultCode);
        }

        private static void SaveCompletedCallback(ResultCode resultCode)
        {
            EditorDocument.OnSaveCompleted(resultCode);
        }

        private static void EnableUICallback(bool enable)
        {
            MainForm.Instance.EnableUI(enable);
        }

        public static bool GetScreenPointFromWorldPos(Vec3 worldPos, out Vec2 screenPoint)
        {
            return GetScreenPointFromWorldPos(worldPos, out screenPoint, false);
        }

        public static bool GetScreenPointFromWorldPos(Vec3 worldPos, out Vec2 screenPoint, bool clipped)
        {
            bool flag = FCE_Editor_GetScreenPointFromWorldPos(worldPos.X, worldPos.Y, worldPos.Z, out screenPoint.X, out screenPoint.Y);
            if (flag && clipped)
            {
                screenPoint.X = Math.Min(Math.Max(0f, screenPoint.X), 1f);
                screenPoint.Y = Math.Min(Math.Max(0f, screenPoint.Y), 1f);
            }
            return flag;
        }

        public static void GetWorldRayFromScreenPoint(Vec2 screenPoint, out Vec3 raySrc, out Vec3 rayDir)
        {
            FCE_Editor_GetWorldRayFromScreenPoint(screenPoint.X, screenPoint.Y, out raySrc.X, out raySrc.Y, out raySrc.Z, out rayDir.X, out rayDir.Y, out rayDir.Z);
        }

        public static bool RayCastTerrain(Vec3 raySrc, Vec3 rayDir, out Vec3 hitPos, out float hitDist)
        {
            return FCE_Editor_RayCastTerrain(raySrc.X, raySrc.Y, raySrc.Z, rayDir.X, rayDir.Y, rayDir.Z, out hitPos.X, out hitPos.Y, out hitPos.Z, out hitDist);
        }

        public static bool RayCastPhysics(Vec3 raySrc, Vec3 rayDir, EditorObject ignore, out Vec3 hitPos, out float hitDist)
        {
            return RayCastPhysics(raySrc, rayDir, ignore, out hitPos, out hitDist, out Vec3 _);
        }

        public static bool RayCastPhysics(Vec3 raySrc, Vec3 rayDir, EditorObject ignore, out Vec3 hitPos, out float hitDist, out Vec3 hitNormal)
        {
            return FCE_Editor_RayCastPhysics(raySrc.X, raySrc.Y, raySrc.Z, rayDir.X, rayDir.Y, rayDir.Z, ignore.Pointer, out hitPos.X, out hitPos.Y, out hitPos.Z, out hitDist, out hitNormal.X, out hitNormal.Y, out hitNormal.Z);
        }

        public static bool RayCastPhysics(Vec3 raySrc, Vec3 rayDir, EditorObjectSelection ignore, out Vec3 hitPos, out float hitDist)
        {
            return RayCastPhysics(raySrc, rayDir, ignore, out hitPos, out hitDist, out Vec3 _);
        }

        public static bool RayCastPhysics(Vec3 raySrc, Vec3 rayDir, EditorObjectSelection ignore, out Vec3 hitPos, out float hitDist, out Vec3 hitNormal)
        {
            return FCE_Editor_RayCastPhysics2(raySrc.X, raySrc.Y, raySrc.Z, rayDir.X, rayDir.Y, rayDir.Z, ignore.Pointer, out hitPos.X, out hitPos.Y, out hitPos.Z, out hitDist, out hitNormal.X, out hitNormal.Y, out hitNormal.Z);
        }

        public static void ToggleIngame()
        {
            if (!FCE_Editor_ValidateIngame())
            {
                MessageBox.Show(MainForm.Instance, Localizer.LocalizeCommon("MSG_DESC_INGAME_INVALID_OBJECTS"), Localizer.Localize("WARNING"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            FCE_Editor_ToggleIngame();
            MainForm.Instance.ToggleIngame();
        }

        public static bool RayCastTerrainFromScreenPoint(Vec2 screenPoint, out Vec3 hitPos)
        {
            GetWorldRayFromScreenPoint(screenPoint, out Vec3 raySrc, out Vec3 rayDir);
            return RayCastTerrain(raySrc, rayDir, out hitPos, out _);
        }

        public static bool RayCastTerrainFromMouse(out Vec3 hitPos)
        {
            return RayCastTerrainFromScreenPoint(Viewport.NormalizedMousePos, out hitPos);
        }

        public static bool RayCastPhysicsFromScreenPoint(Vec2 screenPoint, out Vec3 hitPos)
        {
            GetWorldRayFromScreenPoint(screenPoint, out Vec3 raySrc, out Vec3 rayDir);
            return RayCastPhysics(raySrc, rayDir, EditorObject.Null, out hitPos, out _);
        }

        public static bool RayCastPhysicsFromMouse(out Vec3 hitPos)
        {
            return RayCastPhysicsFromScreenPoint(Viewport.NormalizedMousePos, out hitPos);
        }

        public static void ApplyScreenDeltaToWorldPos(Vec2 screenDelta, ref Vec3 worldPos)
        {
            Vec3 vec = Camera.FrontVector;
            if (Math.Abs(vec.X) < 0.001 && Math.Abs(vec.Y) < 0.001)
            {
                vec = Camera.UpVector;
            }
            Vec2 vec2 = -vec.XY;
            vec2.Normalize();
            Vec2 vec3 = new Vec2(0f - vec2.Y, vec2.X);
            float num = (float)(Vec3.Dot(worldPos - Camera.Position, Camera.FrontVector) * Math.Tan(Camera.HalfFOV) * 2.0);
            worldPos.X += num * screenDelta.X * vec3.X + num * screenDelta.Y * vec2.X;
            worldPos.Y += num * screenDelta.X * vec3.Y + num * screenDelta.Y * vec2.Y;
        }

        public static void OnMouseEvent(MouseEvent mouseEvent, MouseEventArgs mouseEventArgs)
        {
            foreach (IInputSink input in GetInputs())
            {
                if (input.OnMouseEvent(mouseEvent, mouseEventArgs))
                {
                    break;
                }
            }
        }

        public static void OnKeyEvent(KeyEvent keyEvent, KeyEventArgs keyEventArgs)
        {
            if (IsIngame)
            {
                if (keyEvent == KeyEvent.KeyUp && keyEventArgs.KeyCode == Keys.Escape)
                {
                    ToggleIngame();
                }
                return;
            }
            foreach (IInputSink input in GetInputs())
            {
                if (input.OnKeyEvent(keyEvent, keyEventArgs))
                {
                    break;
                }
            }
        }

        public static void OnEditorEvent(uint eventType, IntPtr eventPtr)
        {
            foreach (IInputSink input in GetInputs())
            {
                input.OnEditorEvent(eventType, eventPtr);
            }
        }

        public static void OnUpdate(float dt)
        {
            foreach (IInputSink input in GetInputs())
            {
                input.Update(dt);
            }
        }

        public static void PushInput(IInputSink input)
        {
            Trace.Assert(!m_inputStack.Contains(input));
            if (m_inputStack.Count > 0)
            {
                m_inputStack[m_inputStack.Count - 1].OnInputRelease();
            }
            m_inputStack.Add(input);
            input.OnInputAcquire();
        }

        public static void PopInput(IInputSink input)
        {
            int num = m_inputStack.LastIndexOf(input);
            if (num != -1)
            {
                m_inputStack[m_inputStack.Count - 1].OnInputRelease();
                m_inputStack.RemoveRange(num, m_inputStack.Count - num);
                if (m_inputStack.Count > 0)
                {
                    m_inputStack[m_inputStack.Count - 1].OnInputAcquire();
                }
            }
        }

        private static IEnumerable<IInputSink> GetInputs()
        {
            for (int i = m_inputStack.Count - 1; i >= 0; i--)
            {
                yield return m_inputStack[i];
            }
        }

        public static RegistryKey GetRegistrySettings()
        {
            return Registry.CurrentUser.CreateSubKey("Software\\Ubisoft\\Far Cry 2\\Editor");
        }

        public static int GetRegistryInt(string name, int defaultValue)
        {
            using (RegistryKey key = GetRegistrySettings())
            {
                return GetRegistryInt(key, name, defaultValue);
            }
        }

        public static int GetRegistryInt(RegistryKey key, string name, int defaultValue)
        {
            object value = key.GetValue(name);
            if (value is int)
            {
                return (int)value;
            }
            return defaultValue;
        }

        public static string GetRegistryString(RegistryKey key, string name, string defaultValue)
        {
            object value = key.GetValue(name);
            if (value is string)
            {
                return (string)value;
            }
            return defaultValue;
        }

        public static void SetRegistryInt(string name, int value)
        {
            using (RegistryKey key = GetRegistrySettings())
            {
                SetRegistryInt(key, name, value);
            }
        }

        public static void SetRegistryInt(RegistryKey key, string name, int value)
        {
            key.SetValue(name, value);
        }

        #region P/Invoke
        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_IsInitialized();

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_Update_Callback(UpdateCallbackDelegate updateCallback);

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_Event_Callback(EventCallbackDelegate eventCallback);

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_LoadCompleted_Callback(LoadCompletedCallbackDelegate eventCallback);

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_SaveCompleted_Callback(SaveCompletedCallbackDelegate eventCallback);

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_EnableUI_Callback(EnableUICallbackDelegate eventCallback);

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_IsLoadPending();

        [DllImport("Dunia.dll")]
        private static extern float FCE_Editor_GetFrameTime();

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_GetScreenPointFromWorldPos(float worldX, float worldY, float worldZ, out float screenX, out float screenY);

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_GetWorldRayFromScreenPoint(float screenX, float screenY, out float raySrcX, out float raySrcY, out float raySrcZ, out float rayDirX, out float rayDirY, out float rayDirZ);

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_RayCastTerrain(float raySrcX, float raySrcY, float raySrcZ, float rayDirX, float rayDirY, float rayDirZ, out float hitX, out float hitY, out float hitZ, out float hitDist);

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_RayCastPhysics(float raySrcX, float raySrcY, float raySrcZ, float rayDirX, float rayDirY, float rayDirZ, IntPtr ignore, out float hitX, out float hitY, out float hitZ, out float hitDist, out float hitNormX, out float hitNormY, out float hitNormZ);

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_RayCastPhysics2(float raySrcX, float raySrcY, float raySrcZ, float rayDirX, float rayDirY, float rayDirZ, IntPtr ignore, out float hitX, out float hitY, out float hitZ, out float hitDist, out float hitNormX, out float hitNormY, out float hitNormZ);

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_ValidateIngame();

        [DllImport("Dunia.dll")]
        private static extern void FCE_Editor_ToggleIngame();

        [DllImport("Dunia.dll")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool FCE_Editor_IsIngame();
        #endregion
    }
}