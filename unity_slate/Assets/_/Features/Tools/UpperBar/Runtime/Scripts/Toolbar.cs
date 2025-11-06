using Foundation.Runtime;
using UnityEngine;
using ImGuiNET;
using System;
using UImGui;
using Slate.Runtime;
using System.Runtime.InteropServices;

namespace UpperBar.Runtime
{
    public class Toolbar : FBehaviour 
    {
        #region Publics
        
#if UNITY_STANDALONE_WIN
        const int m_sWMinimize = 6;
#endif
        
        #endregion
        
        #region DllImport

#if UNITY_STANDALONE_WIN
        
            [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
            [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

#endif
#if UNITY_STANDALONE_OSX

            [DllImport("no_border_mac")] private static extern void MakeWindowBorderless();
            [DllImport("no_border_mac")] private static extern void MakeWindowMovable();
            [DllImport("no_border_mac")] private static extern void MakeWindowNotMovable();
            [DllImport("no_border_mac")] private static extern void ResetWindowStyle();

#endif
        
        #endregion

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
                _prevMode = Screen.fullScreenMode;
                _prevW = Screen.width;
                _prevH = Screen.height;
                
                _isOpen = false; 
                _hiddenY = -Mathf.Max(1f, barHeight);
                _y = _hiddenY;
                
                _bootStartTime = Time.unscaledTime;
                _bootArmed = false;
                _mouseMovedSinceBoot = false;
                _lastMouse = Input.mousePosition;
                
                UImGuiUtility.Layout += OnLayout;
            }

            void OnDisable()
            {
                UImGuiUtility.Layout -= OnLayout;
            }

            void Update()
            {
                // --- Boot guard ---
                if (!_bootArmed)
                {
                    if ((Time.unscaledTime - _bootStartTime) >= _bootGuardSeconds)
                        _bootArmed = true;

                    if ((Input.mousePosition - _lastMouse).sqrMagnitude > 1f)
                    {
                        _mouseMovedSinceBoot = true;
                        _bootArmed = true;
                    }
                }
                _lastMouse = Input.mousePosition;

                // --- Inputs / géométrie ---
                var m = Input.mousePosition;
                var mouseY = Screen.height - m.y;

                _hiddenY  = -Mathf.Max(1f, barHeight) - 2f;        // cache = -barHeight - marge
                var visibleH = Mathf.Clamp(barHeight + _y, 0f, barHeight);

                var inActivationZone = _bootArmed && (mouseY <= topRevealZone);
                var overVisibleBar   = mouseY >= 0f && mouseY <= visibleH;

                // =================================================================
                // FSM : décision d'état (OUVERT / FERME) + temporisation de fermeture
                // =================================================================
                if (!_isOpen)
                {
                    // Fermé -> Ouvrir seulement si on touche la zone d'activation
                    if (inActivationZone)
                    {
                        _isOpen = true;
                        _closeArmTime = -1f;
                    }
                }
                else
                {
                    // Ouvert -> rester ouvert si on est dans la barre visible OU dans la zone d'activation
                    if (overVisibleBar || inActivationZone)
                    {
                        _closeArmTime = -1f; // on désarme la fermeture
                    }
                    else
                    {
                        // Curseur sorti -> armer le timer de fermeture
                        if (_closeArmTime < 0f)
                            _closeArmTime = Time.unscaledTime;

                        // Fermer après hideDelay
                        if (Time.unscaledTime - _closeArmTime >= hideDelay)
                        {
                            _isOpen = false;
                            _closeArmTime = -1f;

                            // Snap hors écran pour éviter tout “collage”
                            _vy = 0f;
                            _y  = _hiddenY;
                        }
                    }
                }

                // =====================
                // Animation (UN SEUL bloc)
                // =====================
                if (_isOpen)
                {
                    // Slide d'ouverture vers 0
                    var raw = Mathf.SmoothDamp(_y, 0f, ref _vy, smoothTime);
                    if (useEaseOut)
                    {
                        var t = Mathf.InverseLerp(_hiddenY, 0f, raw);
                        t = 1f - Mathf.Pow(1f - t, 3f);          // easeOutCubic
                        _y = Mathf.Lerp(_hiddenY, 0f, t);
                    }
                    else
                    {
                        _y = raw;
                    }
                }
                else
                {
                    // Fermé : pas d'anim (on plaque pour éviter le drift)
                    _vy = 0f;
                    _y  = _hiddenY;
                }
                
                // Si le mode ou la résolution a changé, on met à jour nos références
                if (_prevMode == Screen.fullScreenMode && _prevW == Screen.width && _prevH == Screen.height) return;
                _prevMode = Screen.fullScreenMode;
                _prevW = Screen.width;
                _prevH = Screen.height;

                // Si on vient de revenir en fenêtré : ré-appliquer le borderless après 1–2 frames
                if (Screen.fullScreenMode == FullScreenMode.Windowed)
                {
#if UNITY_STANDALONE_WIN
                    StartCoroutine(ReapplyBorderlessNextFrame());
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                    StartCoroutine(ReapplyBorderlessMacNextFrame());
#endif
                }
            }

            private void OnLayout(UImGui.UImGui ui)
            {
                if (!_isOpen && _y <= _hiddenY + 0.01f)
                    return;
                
                var visibleH = Mathf.Clamp(barHeight + _y, 0f, barHeight);
                
                var pos = new Vector2(0f, 0f); // on laisse à 0 pour éviter tout clamp
                var size = new Vector2(Screen.width, visibleH);

                ImGui.SetNextWindowPos(pos, ImGuiCond.Always);
                ImGui.SetNextWindowSize(size, ImGuiCond.Always);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);

                var k = (barHeight > 0f) ? (visibleH / barHeight) : 0f;
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(bgColor.r, bgColor.g, bgColor.b, bgAlpha * k));

                const ImGuiWindowFlags baseFlags =
                    ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse |
                    ImGuiWindowFlags.NoCollapse |
                    ImGuiWindowFlags.NoSavedSettings;

                ImGui.Begin("TopRevealToolbar", baseFlags);

                ImGui.SetCursorPosX(horizontalPadding);

                var winSize = ImGui.GetWindowSize();
                var totalButtonsWidth = ComputeButtonsWidth();
                var rightStartX = winSize.x - horizontalPadding - totalButtonsWidth;

                var cur = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new Vector2(rightStartX, cur.y));

                DrawGhostButton(labelOne, MinimizeWindow);
                ImGui.SameLine(0f, buttonSpacing);
                DrawGhostButton(labelTwo, ToggleBorderless);
                ImGui.SameLine(0f, buttonSpacing);
                DrawGhostButton(labelThree, QuitApp);
                
                ImGui.End();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar(2);
            }

            private static void DrawGhostButton(string label, Action onClick)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 1, 1, 0.08f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1, 1, 1, 0.12f));

                if (ImGui.Button(label))
                    onClick?.Invoke();

                ImGui.PopStyleColor(3);
            }

            private float ComputeButtonsWidth()
            {
                var w = 0f;
                w += GhostButtonWidth(labelOne);
                w += buttonSpacing;
                w += GhostButtonWidth(labelTwo);
                w += buttonSpacing;
                w += GhostButtonWidth(labelThree);
                w -= buttonSpacing;
                return w;
            }

            private static float GhostButtonWidth(string label)
            {
                var sz = ImGui.CalcTextSize(label);
                return sz.x + 12f;
            }
            
            // — Minimiser la fenêtre (Windows standalone uniquement) —
            private static void MinimizeWindow()
            {
#if UNITY_STANDALONE_WIN
                var hWnd = GetActiveWindow();
                if (hWnd != IntPtr.Zero) ShowWindow(hWnd, m_sWMinimize);
#endif
            }

            // — Toggle plein écran fenêtré (borderless) <-> fenêtré —
            private void ToggleBorderless()
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
                    var w = _savedWindowedRes ? _windowedW : 1280;
                    var h = _savedWindowedRes ? _windowedH : 720;
                    Screen.SetResolution(w, h, FullScreenMode.Windowed);

#if UNITY_STANDALONE_WIN
                    StartCoroutine(ReapplyBorderlessNextFrame());
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
                    StartCoroutine(ReapplyBorderlessMacNextFrame());
#endif
                }
                else
                {
                    // Passer en borderless sur la résolution du display principal
                    var w = Display.main.systemWidth;
                    var h = Display.main.systemHeight;
                    Screen.SetResolution(w, h, FullScreenMode.FullScreenWindow);
                }
            }

            // — Quitter l’application (Editor-safe) —
            private void QuitApp()
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
            
#if UNITY_STANDALONE_WIN
            private System.Collections.IEnumerator ReapplyBorderlessNextFrame()
            {
                // Laisse Unity finir le switch de mode/résolution
                yield return null; // N+1
                yield return null; // N+2 (drivers lents)

                // Trouve/Crée le GO porteur
                var go = GameObject.Find("BorderRemover");
                if (go == null) go = new GameObject("BorderRemover");

                // S’il y a déjà un NoBorderWin, on le détruit proprement pour relancer Start()
                var old = go.GetComponent<NoBorderWin>();
                if (old != null)
                {
                    // OnDestroy() rétablit bien l'ancien WndProc chez toi — safe
                    Destroy(old);
                    yield return null; // laisse OnDestroy s’exécuter
                }

                // Recrée le composant → Start() est rappelé → styles borderless ré-appliqués
                go.AddComponent<NoBorderWin>();
            }
#endif
        
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            private System.Collections.IEnumerator ReapplyBorderlessMacNextFrame()
            {
                // Laisse Unity terminer le switch en Windowed
                yield return null; // N+1
                yield return null; // N+2

                // Re-applique le borderless et verrouille le non-déplacement par défaut
                try
                {
                    MakeWindowBorderless();
                    MakeWindowNotMovable(); // ou MakeWindowMovable() si tu préfères
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[Toolbar/macOS] Reapply borderless failed: {e.Message}");
                }
            }
#endif
        
#if UNITY_STANDALONE_WIN
            private static Component EnsureBorderRemoverGO()
            {
                // 1) Trouver / créer le GameObject
                var go = GameObject.Find("BorderRemover");
                if (go == null) go = new GameObject("BorderRemover");

                // 2) Retrouver le type "BorderRemover" quel que soit son namespace
                Type type = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = asm.GetTypes(); } catch { continue; }
                    foreach (var t in types)
                    {
                        if (t.Name == "BorderRemover") { type = t; break; }
                    }
                    if (type != null) break;
                }

                if (type == null)
                {
                    Debug.LogWarning("[Toolbar] Type 'BorderRemover' introuvable.");
                    return null;
                }

                // 3) Ajouter le composant si absent
                var comp = go.GetComponent(type);
                if (comp == null) comp = go.AddComponent(type);

                // 4) Appeler Apply() si dispo (public ou non)
                var mi = type.GetMethod("Apply",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (mi != null)
                {
                    try { mi.Invoke(comp, null); }
                    catch (Exception e) { Debug.LogWarning($"[Toolbar] BorderRemover.Apply() a levé: {e.Message}"); }
                }

                return comp;
            }
#endif
        
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
            float _closeArmTime = -1f;
            bool _hoveredThisFrame;
            bool _wantOpen;
            bool _isOpen;
            bool _bootArmed;
            bool _savedWindowedRes;
            bool _mouseMovedSinceBoot;
            float _bootStartTime;
            int _windowedW, _windowedH;
            Vector3 _lastMouse;
            [SerializeField] float _closeTolerance = 0f;
            [SerializeField] float _bootGuardSeconds = 0.35f;
#if UNITY_STANDALONE_WIN
            FullScreenMode _prevMode;
            int _prevW, _prevH;
#endif
        
        #endregion
    }
}