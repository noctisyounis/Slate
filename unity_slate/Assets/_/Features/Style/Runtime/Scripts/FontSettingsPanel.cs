using System;
using ImGuiNET;
using UnityEngine;

namespace Style.Runtime
{
    public class FontSettingsPanel
    {

        #region GUI

            public void Draw()
            {
                InitOnce();

                var currentLabel = _previewFont.ToString();
                if (ImGui.BeginCombo("Font", currentLabel))
                {
                    foreach (FontKind kind in Enum.GetValues(typeof(FontKind)))
                    {
                        var selected = (kind == _previewFont);
                        if (ImGui.Selectable(kind.ToString(), selected))
                            _previewFont = kind;

                        if (selected)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndCombo();
                }

                ImGui.Spacing();
                ImGui.Text("Size");
                ImGui.SameLine();
                ImGui.SliderFloat("##FontScale",
                    ref _previewScale,
                    MinScale,
                    MaxScale,
                    $"x{_previewScale:0.00}");

                ImGui.Separator();
                ImGui.Text("Preview :");

                var previewFont = GetFontPtr(_previewFont);
                ImGui.PushFont(previewFont);
                ImGui.SetWindowFontScale(_previewScale);

                ImGui.Text("The quick brown fox jumps over the lazy dog.");
                ImGui.Text("0123456789  □ ■ ▢  …");

                ImGui.SetWindowFontScale(1f);
                ImGui.PopFont();

                ImGui.Spacing();
                ImGui.Separator();

                ImGui.Text($"Current : {FontRegistry.m_currentFont})");

                if (!ImGui.Button("Apply##Font")) return;
                FontRegistry.m_currentFont = _previewFont;
                FontRegistry.m_fontScale = Mathf.Clamp(_previewScale, MinScale, MaxScale);
                FontRegistry.SavePrefs();
                Debug.Log($"[Settings.Font] Apply font={FontRegistry.m_currentFont}, scale={_previewScale:0.00}");
            }

            private void InitOnce()
            {
                if (_initialized) return;
                _initialized = true;
                var _ = FontRegistry.m_currentFont;
                _previewFont = FontRegistry.m_currentFont;
                _previewScale = Mathf.Clamp(FontRegistry.m_fontScale, MinScale, MaxScale);
            }

            private ImFontPtr GetFontPtr(FontKind kind)
            {
                return kind switch
                {
                    FontKind.UImGUIDefault => FontRegistry.m_defaultFont,
                    FontKind.OpenDyslexic => FontRegistry.m_openDysFont,
                    _ => FontRegistry.m_notoFont
                };
            }

        #endregion

        #region Private

            private bool _initialized;
            private FontKind _previewFont;
            private float _previewScale;

            private const float MinScale = 0.5f;
            private const float MaxScale = 10.0f;

        #endregion
    }
}