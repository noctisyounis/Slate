using UnityEngine;
using ImGuiNET;
using Slate.Runtime;
using SharedData.Runtime;

namespace Style.Runtime
{
    [SlateWindow(categoryName = "Tools", entry = "ImGUI Settings")]
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

            protected override void WindowLayout()
            {
                DrawTitlebarCloseButton();
                if (!ImGui.BeginTabBar("SettingsTabs")) return;
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
                
                ImGui.EndChild();
                return;

                void DrawTitlebarCloseButton()
                {
                    var frameH = ImGui.GetFrameHeight();

                    ImGui.BeginGroup();

                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.12f, 0.20f, 0.35f, 1f));
                    ImGui.BeginChild("TitleBar", new Vector2(0, frameH + 4));

                    ImGui.PushFont(ImGui.GetFont());
                    ImGui.Text("ImGUI Settings");
                    ImGui.PopFont();

                    ImGui.SameLine();

                    var fullWidth = ImGui.GetContentRegionAvail().x - frameH;
                    ImGui.Dummy(new Vector2(fullWidth, 1));
                    ImGui.SameLine();

                    if (ImGui.Button("X", new Vector2(frameH, frameH)))
                        Destroy(gameObject);

                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                    ImGui.EndGroup();
                }
            }

        #endregion

        #region Private

            private FontSettingsPanel _fontPanel;
            private StyleSettingsPanel _stylePanel;
            private ColorSettingsPanel _colorPanel;
            private PresetManagerPanel _presetPanel;
            private bool _sizeInitialized;

        #endregion
    }
}