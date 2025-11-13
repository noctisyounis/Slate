using UnityEngine;
using ImGuiNET;
using System;
using Foundation.Runtime;
using System.Runtime.InteropServices;

namespace UpperBar.Runtime
{
    [DisallowMultipleComponent]
    public class ToolbarButton : FBehaviour
    {
        #region Header

            [Header("Boutons")]
            public string labelOne = "_";
            public string labelTwo = "□";
            public string labelThree = "X";
            public float spacing = 10f;

        #endregion

        #region Public

            public Action OnToggleBorderless;
            public Action OnQuitApp;

        #endregion

        #region WinAPI

#if UNITY_STANDALONE_WIN
        const int m_sWMinimize = 6;
        
        [DllImport("user32.dll")] static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

        #endregion
        
        #region Rendering
        
            public void DrawRightAligned()
            {
                var rightGroupWidth =
                    GhostButtonWidth(labelOne)
                    + spacing
                    + GhostButtonWidth(labelTwo)
                    + spacing
                    + GhostButtonWidth(labelThree);

                var cur = ImGui.GetCursorPos();
                var avail = ImGui.GetContentRegionAvail().x;
                var startX = cur.x + Mathf.Max(0f, avail - rightGroupWidth);
                ImGui.SetCursorPos(new Vector2(startX, cur.y));

                DrawGhostButton(labelOne, MinimizeWindow);
                ImGui.SameLine(0f, spacing);
                DrawGhostButton(labelTwo, () => OnToggleBorderless?.Invoke());
                ImGui.SameLine(0f, spacing);
                DrawGhostButton(labelThree, () => OnQuitApp?.Invoke());
            }
            
        #endregion
        
        #region Helpers
        
            public static float GhostButtonWidth(string label)
            {
                var style = ImGui.GetStyle();
                var text = ImGui.CalcTextSize(label).x;
                return text + style.FramePadding.x * 2f;
            }

            static void DrawGhostButton(string label, Action onClick)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 1, 1, 0.08f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1, 1, 1, 0.12f));
                if (ImGui.Button(label)) onClick?.Invoke();
                ImGui.PopStyleColor(3);
            }

            static void MinimizeWindow()
            {
#if UNITY_STANDALONE_WIN
                var hWnd = GetActiveWindow();
                if (hWnd != IntPtr.Zero) ShowWindow(hWnd, m_sWMinimize);
#endif
            }
            
        #endregion
    }
}