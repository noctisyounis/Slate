using System.Collections.Generic;
using UnityEngine;
using ImGuiNET;
using Foundation.Runtime;
using SharedData.Runtime;
using System.Linq;

namespace UpperBar.Runtime
{
    [DisallowMultipleComponent]
    public class ToolbarMenu : FBehaviour
    {
        #region Header

            [Header("State (SO)")]
            public ToolbarSharedState m_state;

        #endregion

        #region Unity

            private void Awake()
            {
                if (m_state != null && m_state.m_popupMaxHeight <= 0f)
                    m_state.m_popupMaxHeight = 280f;
            }

        #endregion
        
        #region Rendering
        
            public void DrawMenusLeft()
            {
                if (m_state == null) return;

                m_state.m_isAnyMenuOpen = false;
                m_state.m_menusTotalWidth = 0f;

                var first = true;
                var groups = WindowRegistry.GroupedByCategory().ToList();
                
                foreach (var group in groups)
                {
                    if (!first) ImGui.SameLine(0f, m_state.m_menuItemSpacing);
                    first = false;

                    var opened = DrawDynamicWindowMenu(
                        group.Key,
                        group.ToList(),
                        m_state.m_menuPreviewWidth,
                        m_state.m_menuPopupMaxWidth,
                        m_state.m_popupMaxHeight,
                        out var usedWidth
                    );

                    m_state.m_isAnyMenuOpen |= opened;
                    m_state.m_menusTotalWidth += usedWidth;
                }

                var count = groups.Count;
                if (count > 1)
                    m_state.m_menusTotalWidth += (count - 1) * m_state.m_menuItemSpacing;
            }
            
        #endregion

        #region Helpers

            private static string EllipseToFit(string s, float maxPx)
            {
                if (maxPx <= 0f)
                    return s;

                var style = ImGui.GetStyle();
                var padding = style.FramePadding.x * 2f;
                var fullW = ImGui.CalcTextSize(s).x + padding;
                
                if (fullW <= maxPx)
                    return s;

                var budget = Mathf.Max(0f, maxPx - padding);
                const string ell = "...";

                if (ImGui.CalcTextSize(ell).x > budget)
                    return ell;

                int lo = 0, hi = s.Length;
                while (lo < hi)
                {
                    var mid = (lo + hi + 1) >> 1;
                    var probe = s[..mid] + ell;
                    if (ImGui.CalcTextSize(probe).x <= budget) lo = mid;
                    else hi = mid - 1;
                }

                return (lo <= 0) ? ell : s.Substring(0, lo) + ell;
            }
            
            private static float ComputeUsedWidth(string preview, float fixedWidth)
            {
                var style = ImGui.GetStyle();
                var padding = style.FramePadding.x * 2f;

                if (fixedWidth > 0f)
                    return fixedWidth;

                var textW = ImGui.CalcTextSize(preview).x;
                return textW + padding;
            }
            
            private static bool DrawDynamicWindowMenu(
                string category,
                List<WindowRegistry.WindowInfo> windows,
                float fixedWidth,
                float popupMaxW,
                float popupMaxH,
                out float usedWidth)
            {
                var id = "##dynmenu_" + category;
                var preview = EllipseToFit(category + "…", fixedWidth);
                
                usedWidth = ComputeUsedWidth(preview, fixedWidth);

                if (fixedWidth > 0f)
                    ImGui.SetNextItemWidth(fixedWidth);

                ImGui.PushStyleColor(ImGuiCol.Button, 0);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0);

                var opened = false;
                
                if (ImGui.BeginCombo(id, preview, ImGuiComboFlags.NoArrowButton))
                {
                    opened = true;
                    
                    ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + popupMaxW);

                    foreach (var win in windows)
                    {
                        ImGui.PushID(win.Entry);

                        var hasInstance = WindowRegistry.HasInstance(win);

                        var marker = hasInstance ? "x" : "+";
                        var label  = $"{win.Entry} {marker}";

                        if (ImGui.Selectable(label, hasInstance))
                        {
                            if (!hasInstance)
                            {
                                WindowRegistry.SpawnInstance(win);
                                WindowRegistry.ToggleWindow(win);
                            }
                            else
                            {
                                WindowRegistry.KillInstance(win);
                            }
                        }

                        ImGui.PopID();
                    }

                    ImGui.PopTextWrapPos();
                    ImGui.EndCombo();
                }

                ImGui.PopStyleColor(3);
                return opened;
            }
            
        #endregion
    }
}