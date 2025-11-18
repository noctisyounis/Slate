using UnityEngine;
using ImGuiNET;
using Foundation.Runtime;
using SharedData.Runtime;

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
                if (m_state.m_menuOne is { Length: > 0 })
                {
                    if (!first) ImGui.SameLine(0f, m_state.m_menuItemSpacing);

                    var labelOne = string.IsNullOrEmpty(m_state.m_menuOneLabel)
                        ? "File"
                        : m_state.m_menuOneLabel;

                    var opened = DrawMenuPopupFixed(
                        labelOne,
                        m_state.m_menuOne,
                        m_state.m_menuPreviewWidth,
                        m_state.m_menuPopupMaxWidth,
                        m_state.m_popupMaxHeight,
                        m_state
                    );

                    m_state.m_isAnyMenuOpen   |= opened;
                    m_state.m_menusTotalWidth += m_state.m_menuPreviewWidth;
                    first = false;
                }
                if (m_state.m_menuTwo is { Length: > 0 })
                {
                    if (!first) ImGui.SameLine(0f, m_state.m_menuItemSpacing);

                    var labelTwo = string.IsNullOrEmpty(m_state.m_menuTwoLabel)
                        ? "View"
                        : m_state.m_menuTwoLabel;

                    var opened = DrawMenuPopupFixed(
                        labelTwo,
                        m_state.m_menuTwo,
                        m_state.m_menuPreviewWidth,
                        m_state.m_menuPopupMaxWidth,
                        m_state.m_popupMaxHeight,
                        m_state
                    );

                    m_state.m_isAnyMenuOpen   |= opened;
                    m_state.m_menusTotalWidth += m_state.m_menuPreviewWidth;
                }

                var count = 0;
                if (m_state.m_menuOne is { Length: > 0 }) count++;
                if (m_state.m_menuTwo is { Length: > 0 }) count++;
                if (count > 1) m_state.m_menusTotalWidth += (count - 1) * m_state.m_menuItemSpacing;
            }
            
        #endregion

        #region Helpers

            private static string EllipseToFit(string s, float maxPx)
            {
                var style = ImGui.GetStyle();
                var padding = style.FramePadding.x * 2f;
                var budget = Mathf.Max(0f, maxPx - padding);
                if (ImGui.CalcTextSize(s).x <= budget) return s;
                const string ell = "...";
                int lo = 0, hi = s.Length;
                while (lo < hi)
                {
                    var mid = (lo + hi + 1) >> 1;
                    var probe = s.Substring(0, mid) + ell;
                    if (ImGui.CalcTextSize(probe).x <= budget) lo = mid; else hi = mid - 1;
                }
                return (lo <= 0) ? ell : s.Substring(0, lo) + ell;
            }

            private static bool DrawMenuPopupFixed(string label, ToolbarSharedState.MenuNode[] items,
                float fixedWidth, float popupMaxW, float popupMaxH, ToolbarSharedState state)
            {
                if (state == null) return false;
                var id = "##menu_" + label;
                var preview = EllipseToFit(label + "…", fixedWidth);
                
                var style = ImGui.GetStyle();
                var textW = ImGui.CalcTextSize(preview).x;
                var fullW = textW + style.FramePadding.x * 2f;

                var boxW = Mathf.Min(fixedWidth, fullW);

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0,0,0,0));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1,1,1,0.08f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1,1,1,0.12f));

                var frameH = ImGui.GetFrameHeight();
                var curPos = ImGui.GetCursorPos();

                ImGui.InvisibleButton(id, new Vector2(boxW, frameH));

                var hovered = ImGui.IsItemHovered();
                var active  = ImGui.IsItemActive();
                var clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
                var btnMin = ImGui.GetItemRectMin();
                
                var rectMin = ImGui.GetItemRectMin();
                var rectMax = new Vector2(btnMin.x + boxW, btnMin.y + frameH);
                
                var bg = new Vector4(0, 0, 0, 0);
                
                if (hovered)
                    bg = new Vector4(1, 1, 1, 0.08f);
                
                if (active)
                    bg = new Vector4(1, 1, 1, 0.12f);
                
                var dl = ImGui.GetWindowDrawList();
                dl.AddRectFilled(rectMin, rectMax, ImGui.ColorConvertFloat4ToU32(bg), 0f);

                ImGui.SetCursorPos(new Vector2(curPos.x + ImGui.GetStyle().FramePadding.x, curPos.y));
                ImGui.TextUnformatted(preview);

                if (clicked)
                    ImGui.OpenPopup(id);

                ImGui.SetNextWindowPos(new Vector2(btnMin.x, btnMin.y + frameH));
                ImGui.SetNextWindowSizeConstraints(new Vector2(140f, 0f), new Vector2(popupMaxW, popupMaxH));

                var opened = false;
                if (ImGui.BeginPopup(id, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings))
                {
                    opened = true;
                    DrawNodes(items, state);
                    ImGui.EndPopup();
                }

                ImGui.PopStyleColor(3);
                return opened;
            }
            
            private static void DrawNodes(ToolbarSharedState.MenuNode[] nodes, ToolbarSharedState state)
            {
                if (nodes == null || state == null) return;
                
                ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0,0,0,0));
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(1,1,1,0.08f));
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(1,1,1,0.12f));
                
                foreach (var n in nodes)
                {
                    if (n == null) continue;

                    if (n.m_separator)
                    {
                        ImGui.Separator();
                        continue;
                    }

                    if (!ImGui.Selectable(n.m_label ?? "…", false, ImGuiSelectableFlags.SpanAllColumns))
                        continue;
                    
                    n.m_onClick?.Invoke();

                    if (string.IsNullOrEmpty(n.m_commandId)) continue;
                    state.m_menuCommandPending = true;
                    state.m_menuCommandId = n.m_commandId;
                }
            }
            
        #endregion
    }
}