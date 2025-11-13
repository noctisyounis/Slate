using Foundation.Runtime;
using UnityEngine;
using System;
using Slate.Runtime;
using System.Runtime.InteropServices;

namespace UpperBar.Runtime
{
    public class Toolbar : FBehaviour 
    {
        #region Header

            [Header("Layout")]
            [SerializeField] public float barHeight = 30f;
            [SerializeField] public float horizontalPadding = 12f;
            [SerializeField] public float topRevealZone = 30f;

            [Header("Animation")]
            [SerializeField] public float smoothTime = 0.12f;
            [SerializeField] public float hideDelay  = 0.15f;
            [SerializeField] public bool  useEaseOut = true;

            [Header("Refs")]
            [SerializeField] public ToolbarView m_view;
            [SerializeField] public ToolbarMenu m_menus;

            [Header("Boot Guard")]
            [SerializeField] public float bootGuardSeconds = 0.35f;

        #endregion

        #region DllImport

#if UNITY_STANDALONE_WIN
            public const int SW_MINIMIZE = 6;
            [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
            [DllImport("user32.dll")] static extern bool   ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            [DllImport("no_border_mac")] static extern void MakeWindowBorderless();
            [DllImport("no_border_mac")] static extern void MakeWindowNotMovable();
#endif

        #endregion

        #region Public

            public float _y, _vy, _hiddenY;
            public bool _isOpen;
            public float _closeArmTime = -1f;

            public bool _bootArmed;
            public float _bootStartTime;
            public Vector3 _lastMouse;
            public bool _mouseMovedSinceBoot;

            public bool _reapplyScheduled;
            public float _reapplyAt;
            public FullScreenMode _prevMode;

            public bool _savedWindowedRes;
            public int _windowedW, _windowedH;

        #endregion

        #region Unity

            public void OnEnable()
            {
                _prevMode = Screen.fullScreenMode;

                _hiddenY = -Mathf.Max(1f, barHeight);
                _y       = _hiddenY;
                _isOpen  = false;

                _bootArmed = false;
                _bootStartTime = Time.unscaledTime;
                _mouseMovedSinceBoot = false;
                _lastMouse = Input.mousePosition;

                if (m_view == null) return;
                m_view.m_isOpen = _isOpen; m_view.m_y = _y;
            }

            public void Update()
            {
                if (!_bootArmed)
                {
                    if ((Time.unscaledTime - _bootStartTime) >= bootGuardSeconds)
                        _bootArmed = true;

                    if ((Input.mousePosition - _lastMouse).sqrMagnitude > 1f)
                    {
                        _mouseMovedSinceBoot = true;
                        _bootArmed = true;
                    }
                }
                _lastMouse = Input.mousePosition;

                var m = Input.mousePosition;
                var mouseY = Screen.height - m.y;

                _hiddenY = -Mathf.Max(1f, barHeight) - 2f;
                var visibleH = Mathf.Clamp(barHeight + _y, 0f, barHeight);

                var menuOpen = m_menus != null && m_menus.m_isAnyMenuOpen;

                var inMenusX = false;
                if (m_menus != null && m_menus.m_totalMenusWidth > 0f)
                {
                    var menusStartX = horizontalPadding;
                    var menusEndX   = horizontalPadding + m_menus.m_totalMenusWidth;
                    var mouseX      = m.x;
                    inMenusX = (mouseX >= menusStartX && mouseX <= menusEndX);
                }

                var dynZone = topRevealZone;
                if (menuOpen && inMenusX)
                {
                    var popupH = (m_menus != null ? m_menus.m_popupMaxHeight : 280f);
                    dynZone = Mathf.Max(dynZone, barHeight + popupH + 8f);
                }

                var inActivationZone = _bootArmed && (mouseY <= dynZone || (menuOpen && inMenusX));
                var overVisibleBar   = mouseY >= 0f && mouseY <= visibleH;

                if (menuOpen && inMenusX)
                {
                    _isOpen = true;
                    _closeArmTime = -1f;
                }

                if (!_isOpen)
                {
                    if (inActivationZone)
                    {
                        _isOpen = true;
                        _closeArmTime = -1f;
                    }
                }
                else
                {
                    if (overVisibleBar || inActivationZone)
                    {
                        _closeArmTime = -1f;
                    }
                    else
                    {
                        if (_closeArmTime < 0f) _closeArmTime = Time.unscaledTime;
                        if (Time.unscaledTime - _closeArmTime >= hideDelay)
                        {
                            _isOpen = false;
                            _closeArmTime = -1f;
                            _vy = 0f;
                            _y  = _hiddenY;
                        }
                    }
                }

                if (_isOpen)
                {
                    var raw = Mathf.SmoothDamp(_y, 0f, ref _vy, smoothTime);
                    if (useEaseOut)
                    {
                        var t = Mathf.InverseLerp(_hiddenY, 0f, raw);
                        t = 1f - Mathf.Pow(1f - t, 3f);
                        _y = Mathf.Lerp(_hiddenY, 0f, t);
                    }
                    else _y = raw;
                }
                else
                {
                    _vy = 0f;
                    _y  = _hiddenY;
                }

                if (_prevMode != Screen.fullScreenMode)
                {
                    _prevMode = Screen.fullScreenMode;
                    if (Screen.fullScreenMode == FullScreenMode.Windowed)
                    {
                        _reapplyScheduled = true;
                        _reapplyAt = Time.unscaledTime + 0.35f;
                    }
                }

                if (_reapplyScheduled && Time.unscaledTime >= _reapplyAt)
                {
                    _reapplyScheduled = false;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                    StartCoroutine(ReapplyNoBorderWinNextFrame());
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                    StartCoroutine(ReapplyBorderlessMacNextFrame());
#endif
                }

                if (m_view == null) return;
                m_view.m_isOpen = _isOpen; m_view.m_y = _y;
            }

        #endregion

        #region API

            public void ToggleBorderless()
            {
                if (!_savedWindowedRes && Screen.fullScreenMode == FullScreenMode.Windowed)
                {
                    _windowedW = Screen.width;
                    _windowedH = Screen.height;
                    _savedWindowedRes = true;
                }

                if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                {
                    var w = _savedWindowedRes ? _windowedW : 1280;
                    var h = _savedWindowedRes ? _windowedH : 720;
                    Screen.SetResolution(w, h, FullScreenMode.Windowed);

                    _reapplyScheduled = true;
                    _reapplyAt = Time.unscaledTime + 0.35f;
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
                yield return null; // N+1
                yield return null; // N+2

                var go = GameObject.Find("BorderRemover");
                if (go == null) go = new GameObject("BorderRemover");

                var nb = go.GetComponent<NoBorderWin>();
                if (nb == null) nb = go.AddComponent<NoBorderWin>();

                var mi = typeof(NoBorderWin).GetMethod("Apply",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (mi != null) { try { mi.Invoke(nb, null); } catch { /* ignore */ } }
            }
#endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            private System.Collections.IEnumerator ReapplyBorderlessMacNextFrame()
            {
                yield return null; // N+1
                yield return null; // N+2
                try
                {
                    MakeWindowBorderless();
                    MakeWindowNotMovable();
                }
                catch { /* ignore */ }
            }
#endif

        #endregion
    }
}