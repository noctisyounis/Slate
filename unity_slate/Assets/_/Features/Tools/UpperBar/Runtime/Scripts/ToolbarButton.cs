using UnityEngine;
using ImGuiNET;
using System;
using Foundation.Runtime;
using SharedData.Runtime;
using System.Runtime.InteropServices;
using Style.Runtime;

namespace UpperBar.Runtime
{
    [DisallowMultipleComponent]
    public class ToolbarButton : FBehaviour
    {
        #region Header

            [Header("State (SO)")]
            public ToolbarSharedState m_state;

        #endregion
        
        #region Rendering
        
            public void DrawRightAligned()
            {
                if (m_state == null) return;
                
                ImGui.PushFont(FontRegistry.m_notoFont);

                var rightGroupWidth =
                    GhostButtonWidth(m_state.m_btnMinLabel)
                    + 10f
                    + GhostButtonWidth(m_state.m_btnBorderlessLabel)
                    + 10f
                    + GhostButtonWidth(m_state.m_btnQuitLabel);

                var cur = ImGui.GetCursorPos();
                var avail = ImGui.GetContentRegionAvail().x;
                var startX = cur.x + Mathf.Max(0f, avail - rightGroupWidth);
                ImGui.SetCursorPos(new Vector2(startX, cur.y));

                DrawGhostButton(m_state.m_btnMinLabel, () => m_state.m_requestMinimize = true);
                ImGui.SameLine(0f, 10f);
                DrawGhostButton(m_state.m_btnBorderlessLabel, () => m_state.m_requestToggleBorderless  = true);
                ImGui.SameLine(0f, 10f);
                DrawGhostButton(m_state.m_btnQuitLabel, () => m_state.m_requestQuit = true);
                
                ImGui.PopFont();
            }
            
        #endregion
        
        #region Helpers
        
            public static float GhostButtonWidth(string label)
            {
                var style = ImGui.GetStyle();
                var text = ImGui.CalcTextSize(label).x;
                return text + style.FramePadding.x * 2f;
            }

            public static void DrawGhostButton(string label, Action onClick)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, ThemeRegistry.GhostButtonColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ThemeRegistry.GhostButtonHoverColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ThemeRegistry.GhostButtonActiveColor);
                
                if (ImGui.Button(label)) onClick?.Invoke();
                
                ImGui.PopStyleColor(3);
            }
            
        #endregion
    }
}