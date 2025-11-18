using UnityEngine;
using UImGui;
using ImGuiNET;

namespace Style.Runtime
{
    [DisallowMultipleComponent]
    public class SettingsWindow : MonoBehaviour
    {
        #region Header
        
            [Header("Title")]
            public string m_windowTitle = "Settings";

            [Header("State")]
            public bool m_visible = false;

        #endregion
        
        #region Unity

            public void OnEnable()  => UImGuiUtility.Layout += OnLayout;
            public void OnDisable() => UImGuiUtility.Layout -= OnLayout;
            
        #endregion

        #region GUI
        
            public void ToggleVisible()
            {
                m_visible = !m_visible;
            }

            private void OnLayout(UImGui.UImGui ui)
            {
                if (!m_visible)
                    return;
                
                if (!ImGui.Begin(m_windowTitle))
                {
                    ImGui.End();
                    return;
                }

                if (ImGui.BeginTabBar("SettingsTabs"))
                {
                    if (ImGui.BeginTabItem("Fonts"))
                    {
                        _fontPanel.Draw();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Styles"))
                    {
                        _stylePanel.Draw();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Colors"))
                    {
                        _colorPanel.Draw();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Preset Manager"))
                    {
                        _pmPanel.Draw();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

                ImGui.End();
            }

        #endregion

        #region Private

            private readonly FontSettingsPanel  _fontPanel  = new FontSettingsPanel();
            private readonly StyleSettingsPanel _stylePanel = new StyleSettingsPanel();
            private readonly ColorSettingsPanel _colorPanel = new ColorSettingsPanel();
            private readonly PresetManagerPanel _pmPanel = new PresetManagerPanel();

        #endregion
    }
}