using UnityEngine;
using ImGuiNET;
using Foundation.Runtime;
using UImGui;
using SharedData.Runtime;
using Style.Runtime;

namespace UpperBar.Runtime
{
    [DisallowMultipleComponent]
    public class ToolbarView : FBehaviour
    {
        #region Header

            [Header("Layout")]
            public float m_barHeight = 30f;
            public float m_horizontalPadding = 12f;

            [Header("Style")]
            public float m_bgAlpha = 0.65f;
            public Color m_bgColor = new Color(0f, 0f, 0f, 1f);

            [Header("State (SO)")]
            public ToolbarSharedState m_state;

            [Header("Refs")]
            public ToolbarMenu m_menus;
            public ToolbarButton m_buttons;

        #endregion
        
        #region Unity

            public void OnEnable()  => UImGuiUtility.Layout += OnLayout;
            public void OnDisable() => UImGuiUtility.Layout -= OnLayout;
            
        #endregion

        #region Rendering

            private void OnLayout(UImGui.UImGui ui)
            {
                if (m_state == null) return;
                
                var font = FontRegistry.GetCurrentFont();
                ImGui.PushFont(font);

                var isOpen = m_state.m_isToolbarDisplayed;
                var y = m_state.m_y;

                if (!isOpen && y <= -Mathf.Max(1f, m_barHeight) + 0.01f) return;

                var visibleH = Mathf.Clamp(m_barHeight + y, 0f, m_barHeight);

                ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);
                ImGui.SetNextWindowSize(new Vector2(Screen.width, visibleH), ImGuiCond.Always);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
                var k = m_barHeight > 0 ? (visibleH / m_barHeight) : 0f;
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(m_bgColor.r, m_bgColor.g, m_bgColor.b, m_bgAlpha * k));

                const ImGuiWindowFlags flags =
                    ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse |
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;

                ImGui.Begin("TopRevealToolbar", flags);
                ImGui.SetCursorPosX(m_horizontalPadding);
                var lineY = ImGui.GetCursorPosY();

                if (ImGui.BeginTable("TopBarTbl", 3,
                        ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.PadOuterX))
                {
                    ImGui.TableSetupColumn("L", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("S", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("R", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    m_menus?.DrawMenusLeft();

                    ImGui.TableSetColumnIndex(2);
                    m_buttons?.DrawRightAligned();

                    ImGui.EndTable();
                }

                ImGui.SetCursorPosY(lineY + ImGui.GetFrameHeight());
                ImGui.End();
                
                ImGui.PopFont();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar(2);
            }

        #endregion
    }
}