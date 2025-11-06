using Foundation.Runtime;
using UnityEngine;
using ImGuiNET;
using System;
using UImGui;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace UpperBar.Runtime
{
    public class Toolbar : FBehaviour 
    {
        #region Publics
        
#if UNITY_STANDALONE_WIN
        const int _sWMinimize = 6;
#endif
        
        #endregion
        
#if UNITY_STANDALONE_WIN

        #region DllImport

            [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
            [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion

#endif

        #region Header

            [Header("Layout")]
            [SerializeField] float barHeight = 30f;
            [SerializeField] float horizontalPadding = 12f;
            [SerializeField] float buttonSpacing = 10f;
            [SerializeField] float topRevealZone = 30f;

            [Header("Animation")]
            [SerializeField] float smoothTime = 0.12f;
            [SerializeField] float hideDelay = 0.15f;
            [SerializeField] bool useEaseOut = true;

            [Header("Style")]
            [SerializeField] float bgAlpha = 0.65f;
            [SerializeField] Color bgColor = new Color(0f, 0f, 0f, 1f);

            [Header("Boutons droite (texte seulement)")]
            [SerializeField] string labelOne = "_";
            [SerializeField] string labelTwo = "□";
            [SerializeField] string labelThree = "X";

        #endregion

        #region Unity API
        
            void OnEnable()
            {
                _hiddenY = -Mathf.Max(1f, barHeight);
                _y = _hiddenY;
                UImGuiUtility.Layout += OnLayout;
            }

            void OnDisable()
            {
                UImGuiUtility.Layout -= OnLayout;
            }

            void Update()
            {
                var m = Input.mousePosition;
                float mouseY = Screen.height - m.y;

                float hiddenY  = -Mathf.Max(1f, barHeight) - 2f;
                float visibleH = Mathf.Clamp(barHeight + _y, 0f, barHeight);

                // Géométrie visible stricte
                float barTop    = 0f;

                bool inActivationZone = mouseY <= topRevealZone;
                bool overVisibleBar = mouseY >= 0f && mouseY <= visibleH;

                // ======== FSM (avec logs d'événements) ========
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
                        _closeArmTime = -1f;
                    else
                    {
                        if (Time.unscaledTime - _closeArmTime >= hideDelay)
                        {
                            _isOpen = false;
                            _closeArmTime = -1f;

                            _y = hiddenY;
                            _vy = 0f;
                        }
                    }
                }

                // ======== Animation ========
                float targetY = _isOpen ? 0f : hiddenY;
                float raw = Mathf.SmoothDamp(_y, targetY, ref _vy, smoothTime);

                if (useEaseOut)
                {
                    float t = Mathf.InverseLerp(hiddenY, 0f, raw);
                    t = 1f - Mathf.Pow(1f - t, 3f);
                    _y = Mathf.Lerp(hiddenY, 0f, t);
                }
                else
                {
                    _y = raw;
                }
            }

            void OnLayout(UImGui.UImGui ui)
            {
                Vector2 pos = new Vector2(0f, _y);
                Vector2 size = new Vector2(Screen.width, barHeight);

                ImGui.SetNextWindowPos(pos, ImGuiCond.Always);
                ImGui.SetNextWindowSize(size, ImGuiCond.Always);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(bgColor.r, bgColor.g, bgColor.b, bgAlpha));

                var flags = ImGuiWindowFlags.NoTitleBar |
                            ImGuiWindowFlags.NoResize |
                            ImGuiWindowFlags.NoMove |
                            ImGuiWindowFlags.NoScrollbar |
                            ImGuiWindowFlags.NoScrollWithMouse |
                            ImGuiWindowFlags.NoCollapse |
                            ImGuiWindowFlags.NoSavedSettings;

                ImGui.Begin("TopRevealToolbar", flags);

                // _hoveredThisFrame = ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);

                ImGui.SetCursorPosX(horizontalPadding);

                float totalButtonsWidth = ComputeButtonsWidth();
                float rightStartX = size.x - horizontalPadding - totalButtonsWidth;

                var cur = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new Vector2(rightStartX, cur.y));

                DrawGhostButton(labelOne, MinimizeWindow);
                ImGui.SameLine(0f, buttonSpacing);
                DrawGhostButton(labelTwo, ToggleBorderless);
                ImGui.SameLine(0f, buttonSpacing);
                DrawGhostButton(labelThree, QuitApp);
                
                if (debugHUD)
                {
                    // Un petit overlay non interactif pour debug
                    ImGui.SetNextWindowPos(new Vector2(10, barHeight + 10), ImGuiCond.Always);
                    ImGui.SetNextWindowBgAlpha(0.6f);
                    ImGui.Begin("ToolbarDebug##overlay",
                        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize |
                        ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings |
                        ImGuiWindowFlags.NoInputs);

                    float hiddenY  = -Mathf.Max(1f, barHeight);
                    float visibleH = Mathf.Clamp(barHeight + _y, 0f, barHeight);
                    float mouseY   = Screen.height - Input.mousePosition.y;

                    ImGui.Text($"open:      {_isOpen}");
                    ImGui.Text($"y:         {_y:F2}  (hiddenY={hiddenY:F2})");
                    ImGui.Text($"visibleH:  {visibleH:F1} / {barHeight:F1}");
                    ImGui.Text($"mouseYTop: {mouseY:F1}");
                    ImGui.Text($"zone:      {(mouseY <= topRevealZone ? "YES" : "no")}");
                    ImGui.Text($"armed:     {(_closeArmTime >= 0f ? (Time.unscaledTime - _closeArmTime).ToString("F2")+"s" : "-")}");
                    ImGui.End();
                }

                ImGui.End();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar(2);
            }
            
            static void DrawGhostButton(string label, Action onClick)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 1, 1, 0.08f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1, 1, 1, 0.12f));

                if (ImGui.Button(label))
                    onClick?.Invoke();

                ImGui.PopStyleColor(3);
            }

            float ComputeButtonsWidth()
            {
                float w = 0f;
                w += GhostButtonWidth(labelOne);
                w += buttonSpacing;
                w += GhostButtonWidth(labelTwo);
                w += buttonSpacing;
                w += GhostButtonWidth(labelThree);
                w -= buttonSpacing;
                return w;
            }

            static float GhostButtonWidth(string label)
            {
                var sz = ImGui.CalcTextSize(label);
                return sz.x + 12f;
            }
            
            // — Minimiser la fenêtre (Windows standalone uniquement) —
            public void MinimizeWindow()
            {
#if UNITY_STANDALONE_WIN
                var hWnd = GetActiveWindow();
                if (hWnd != IntPtr.Zero) ShowWindow(hWnd, _sWMinimize);
#else
            Debug.LogWarning("MinimizeWindow: non supporté nativement hors Windows standalone.");
#endif
            }

            // — Toggle plein écran fenêtré (borderless) <-> fenêtré —
            public void ToggleBorderless()
            {
                // Sauvegarde la résolution fenêtrée la première fois
                if (!_savedWindowedRes && Screen.fullScreenMode == FullScreenMode.Windowed)
                {
                    _windowedW = Screen.width;
                    _windowedH = Screen.height;
                    _savedWindowedRes = true;
                }

                if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                {
                    // Revenir en fenêtré à la résolution sauvegardée (fallback = 1280x720)
                    int w = _savedWindowedRes ? _windowedW : 1280;
                    int h = _savedWindowedRes ? _windowedH : 720;
                    Screen.SetResolution(w, h, FullScreenMode.Windowed);
                    Debug.Log($"[Toolbar] Windowed {w}x{h}");
                }
                else
                {
                    // Passer en borderless sur la résolution du display principal
                    int w = Display.main.systemWidth;
                    int h = Display.main.systemHeight;
                    Screen.SetResolution(w, h, FullScreenMode.FullScreenWindow);
                    Debug.Log($"[Toolbar] FullScreenWindow {w}x{h}");
                }
            }

            // — Quitter l’application (Editor-safe) —
            public void QuitApp()
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        
        #endregion

        #region Main Methods

   
        #endregion

        #region Utils

   
        #endregion
        
        #region Privates & Protected
        
            float _y;
            float _vy;
            float _hiddenY;
            float _mouseLeftAt = -1f;
            bool _hoveredThisFrame;
            bool _wantOpen;
            bool _isOpen;
            float _closeArmTime = -1f;
            int _windowedW, _windowedH;
            bool _savedWindowedRes;
            [SerializeField] float closeTolerance = 0f;
            [SerializeField] bool debugLogs = true;
            [SerializeField] bool debugHUD  = true;
        
        #endregion
    }
}