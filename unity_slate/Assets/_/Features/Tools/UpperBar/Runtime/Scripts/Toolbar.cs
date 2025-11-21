using Foundation.Runtime;
using UnityEngine;
using System;
using System.Linq;
using Slate.Runtime;
using SharedData.Runtime;
using System.Runtime.InteropServices;
using Style.Runtime;

namespace UpperBar.Runtime
{
    public class Toolbar : FBehaviour 
    {
        #region Header

            [Header("Layout")]
            [SerializeField] public float m_barHeight = 30f;
            [SerializeField] public float m_horizontalPadding = 12f;
            [SerializeField] public float m_topRevealZone = 30f;

            [Header("Animation")]
            [SerializeField] public float m_smoothTime = 0.12f;
            [SerializeField] public float m_hideDelay  = 0.15f;
            [SerializeField] public bool  m_useEaseOut = true;

            [Header("State (SO)")]
            public ToolbarSharedState m_state;
            
            [Header("Windows")]
            public SettingsWindow m_SettingsWindow;
            private SettingsWindow _settingsInstance;

            [Header("Boot Guard")]
            [SerializeField] public float m_bootGuardSeconds = 0.35f;

        #endregion

        #region DllImport

#if UNITY_STANDALONE_WIN
            const int SW_MINIMIZE = 6;
            [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
            [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    #endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            [DllImport("no_border_mac")] static extern void MakeWindowBorderless();
            [DllImport("no_border_mac")] static extern void MakeWindowNotMovable();
#endif

        #endregion

        #region Public

            public float m_y, m_vy, m_hiddenY;
            public bool m_isOpen;
            public float m_closeArmTime = -1f;

            public bool m_bootArmed;
            public float m_bootStartTime;
            public Vector3 m_lastMouse;
            public bool m_mouseMovedSinceBoot;

            public bool m_reapplyScheduled;
            public float m_reapplyAt;
            public FullScreenMode m_prevMode;

            public bool m_savedWindowedRes;
            public int m_windowedW, m_windowedH;

        #endregion

        #region Unity
        
            private void Awake()
            {
                if (m_SettingsWindow != null)
                    WindowRegistry.RegisterPrefab(m_SettingsWindow);
            }

            public void OnEnable()
            {
                m_prevMode = Screen.fullScreenMode;

                m_hiddenY = -Mathf.Max(1f, m_barHeight);
                m_y = m_hiddenY;
                m_isOpen = false;

                m_bootArmed = false;
                m_bootStartTime = Time.unscaledTime;
                m_mouseMovedSinceBoot = false;
                m_lastMouse = Input.mousePosition;

                if (m_state == null) return;
                m_state.m_isToolbarDisplayed = m_isOpen;
                m_state.m_y = m_y;
            }

            public void Update()
            {
                if (m_state == null) return;

                if (!m_bootArmed)
                {
                    if ((Time.unscaledTime - m_bootStartTime) >= m_bootGuardSeconds) m_bootArmed = true;
                    if ((Input.mousePosition - m_lastMouse).sqrMagnitude > 1f) m_bootArmed = true;
                }
                m_lastMouse = Input.mousePosition;

                var m = Input.mousePosition;
                var mouseY = Screen.height - m.y;

                m_hiddenY = -Mathf.Max(1f, m_barHeight) - 2f;
                var visibleH = Mathf.Clamp(m_barHeight + m_y, 0f, m_barHeight);

                var menuOpen = m_state.m_isAnyMenuOpen;
                var inMenusX = false;
                if (m_state.m_menusTotalWidth > 0f)
                {
                    var menusStartX = m_horizontalPadding;
                    var menusEndX   = m_horizontalPadding + m_state.m_menusTotalWidth;
                    var mouseX      = m.x;
                    inMenusX = (mouseX >= menusStartX && mouseX <= menusEndX);
                }

                var dynZone = m_topRevealZone;
                if (menuOpen && inMenusX)
                    dynZone = Mathf.Max(dynZone, m_barHeight + m_state.m_popupMaxHeight + 8f);

                var inActivationZone = m_bootArmed && (mouseY <= dynZone || (menuOpen && inMenusX));
                var overVisibleBar   = mouseY >= 0f && mouseY <= visibleH;

                if (menuOpen && inMenusX) { m_isOpen = true; m_closeArmTime = -1f; }

                if (!m_isOpen)
                {
                    if (inActivationZone) { m_isOpen = true; m_closeArmTime = -1f; }
                }
                else
                {
                    if (overVisibleBar || inActivationZone) m_closeArmTime = -1f;
                    else
                    {
                        if (m_closeArmTime < 0f) m_closeArmTime = Time.unscaledTime;
                        if (Time.unscaledTime - m_closeArmTime >= m_hideDelay)
                        {
                            m_isOpen = false; m_closeArmTime = -1f; m_vy = 0f; m_y = m_hiddenY;
                        }
                    }
                }

                if (m_isOpen)
                {
                    var raw = Mathf.SmoothDamp(m_y, 0f, ref m_vy, m_smoothTime);
                    if (m_useEaseOut)
                    {
                        var t = Mathf.InverseLerp(m_hiddenY, 0f, raw);
                        t = 1f - Mathf.Pow(1f - t, 3f);
                        m_y = Mathf.Lerp(m_hiddenY, 0f, t);
                    }
                    else m_y = raw;
                }
                else { m_vy = 0f; m_y = m_hiddenY; }

                m_state.m_isPointerInToolbar = (mouseY <= Mathf.Max(m_topRevealZone, m_barHeight));
                
                if (m_state.m_requestMinimize)
                {
                    m_state.m_requestMinimize = false;
                    MinimizeWindow();
                }
                if (m_state.m_requestToggleBorderless)
                {
                    m_state.m_requestToggleBorderless = false;
                    ToggleBorderless();
                }
                if (m_state.m_requestQuit)
                {
                    m_state.m_requestQuit = false;
                    QuitApp();
                }

                m_state.m_isToolbarDisplayed = m_isOpen;
                m_state.m_y = m_y;
                
                HandleMenuCommands();
            }

        #endregion

        #region API
        
            private void HandleMenuCommands()
            {
                if (m_state == null) return;
                if (!m_state.m_menuCommandPending) return;

                var cmd = m_state.m_menuCommandId;
                m_state.m_menuCommandPending = false;
                m_state.m_menuCommandId = null;

                switch (cmd)
                {
                    case "settings":
                        var info = WindowRegistry.Windows
                            .FirstOrDefault(w => w.Type == typeof(SettingsWindow));

                        if (info != null)
                            WindowRegistry.ToggleWindow(info);
                        break;
                }
        }

            public void MinimizeWindow()
            {
#if UNITY_STANDALONE_WIN
                var hWnd = GetActiveWindow();
                if (hWnd != IntPtr.Zero) ShowWindow(hWnd, SW_MINIMIZE);
#endif
            }

            public void ToggleBorderless()
            {
                if (!m_savedWindowedRes && Screen.fullScreenMode == FullScreenMode.Windowed)
                {
                    m_windowedW = Screen.width;
                    m_windowedH = Screen.height;
                    m_savedWindowedRes = true;
                }

                if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                {
                    var w = m_savedWindowedRes ? m_windowedW : 1280;
                    var h = m_savedWindowedRes ? m_windowedH : 720;
                    Screen.SetResolution(w, h, FullScreenMode.Windowed);
                    
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                    StartCoroutine(ReapplyNoBorderWinNextFrame());
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                    StartCoroutine(ReapplyBorderlessMacNextFrame());
#endif
                }
                else
                {
                    var w = Display.main.systemWidth;
                    var h = Display.main.systemHeight;
                    Screen.SetResolution(w, h, FullScreenMode.FullScreenWindow);
                }
            }

            public static void QuitApp()
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

        #endregion

        #region Coroutine

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            private System.Collections.IEnumerator ReapplyNoBorderWinNextFrame()
            {
                yield return null;
                yield return null;

                var go = GameObject.Find("BorderRemover");
                if (go == null)
                    go = new GameObject("BorderRemover");

                var old = go.GetComponent<NoBorderWin>();
                if (old != null)
                {
                    UnityEngine.Object.Destroy(old);
                    yield return null;
                }

                go.AddComponent<NoBorderWin>();
            }
#endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            private System.Collections.IEnumerator ReapplyBorderlessMacNextFrame()
            {
                yield return null;
                yield return null;
                try { MakeWindowBorderless(); MakeWindowNotMovable(); } catch {}
            }
#endif

        #endregion
    }
}