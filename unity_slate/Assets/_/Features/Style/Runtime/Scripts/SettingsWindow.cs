using UnityEngine;
using ImGuiNET;
using Foundation.Runtime;
using Slate.Runtime;

namespace Style.Runtime
{
    [SlateWindow(categoryName = "Tools", entry = "Settings")]
    public class SettingsWindow : WindowBaseBehaviour
    {
        #region Unity

            private void Awake()
            {
                _fontPanel ??= new FontSettingsPanel();
                _stylePanel ??= new StyleSettingsPanel();
                _colorPanel ??= new ColorSettingsPanel();
                _presetPanel ??= new PresetManagerPanel();
            }

            protected override void OnEnable()
            {
                base.OnEnable();
            }

            protected override void OnDisable()
            {
                base.OnDisable();
            }
            
        #endregion

        #region GUI
        
            public void ToggleVisible()
            {
                _visible = !_visible;
            }

            // protected override bool ShouldDrawWindow()
            // {
            //     return _visible;
            // }

            protected override void WindowLayout()
            {
                if (!_visible)
                    return;
                
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
                        _presetPanel.Draw();
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

                ImGui.End();
            }

        #endregion

        #region Private

            private FontSettingsPanel _fontPanel;
            private StyleSettingsPanel _stylePanel;
            private ColorSettingsPanel _colorPanel;
            private PresetManagerPanel _presetPanel;
            private bool _visible = false;

        #endregion
    }
}