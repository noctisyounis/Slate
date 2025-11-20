using UnityEngine;
using System;
using System.IO;
using ImGuiNET;

namespace Style.Runtime
{
    public class PresetManagerPanel
    {
        [Serializable]
        private class FontPresetDto
        {
            public int fontKind;
            public float scale;
        }

        [Serializable]
        private class SizePresetDto
        {
            public StyleSizeState state;
        }

        [Serializable]
        private class ColorPresetDto
        {
            public StyleColorState state;
        }

        private bool _initialized;
        private string _fontPath;
        private string _sizePath;
        private string _colorPath;

        private void InitOnce()
        {
            if (_initialized)
                return;

            _fontPath = SettingsPath.FontsPresetPath;
            _sizePath = SettingsPath.SizesPresetPath;
            _colorPath = SettingsPath.ColorsPresetPath;

            _initialized = true;
        }

        public void Draw()
        {
            InitOnce();

            ImGui.Text("Presets import / export");
            ImGui.Separator();

            DrawFontSection();
            ImGui.Separator();

            DrawSizeSection();
            ImGui.Separator();

            DrawColorSection();
        }

        private void DrawFontSection()
        {
            ImGui.Text("Fonts");
            ImGui.SameLine();
            ImGui.TextDisabled("(font kind + scale)");

            if (ImGui.Button("Export fonts"))
            {
                ExportFonts();
            }
            ImGui.SameLine();
            if (ImGui.Button("Import fonts"))
            {
                ImportFonts();
            }

            ImGui.TextDisabled(_fontPath);
        }

        private void ExportFonts()
        {
            try
            {
                var dto = new FontPresetDto
                {
                    fontKind = (int)FontRegistry.m_currentFont,
                    scale = FontRegistry.m_fontScale
                };

                var json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(_fontPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetManager] Failed to export fonts: {ex.Message}");
            }
        }

        private void ImportFonts()
        {
            try
            {
                if (!File.Exists(_fontPath))
                {
                    Debug.LogWarning($"[PresetManager] No font preset at {_fontPath}");
                    return;
                }

                var json = File.ReadAllText(_fontPath);
                var dto = JsonUtility.FromJson<FontPresetDto>(json);
                if (dto == null)
                {
                    Debug.LogWarning("[PresetManager] Invalid font preset JSON.");
                    return;
                }

                FontRegistry.m_currentFont = (FontKind)dto.fontKind;
                FontRegistry.m_fontScale = dto.scale;
                FontRegistry.SavePrefs();
                FontRegistry.ApplyAsDefault();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetManager] Failed to import fonts: {ex.Message}");
            }
        }

        private void DrawSizeSection()
        {
            ImGui.Text("Style sizes");
            ImGui.SameLine();
            ImGui.TextDisabled("(padding, rounding, spacing, etc.)");

            if (ImGui.Button("Export sizes"))
            {
                ExportSizes();
            }
            ImGui.SameLine();
            if (ImGui.Button("Import sizes"))
            {
                ImportSizes();
            }

            ImGui.TextDisabled(_sizePath);
        }

        private void ExportSizes()
        {
            try
            {
                StyleRegistry.m_state.CaptureFrom(ImGui.GetStyle());

                var dto = new SizePresetDto
                {
                    state = StyleRegistry.m_state
                };

                var json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(_sizePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetManager] Failed to export sizes: {ex.Message}");
            }
        }

        private void ImportSizes()
        {
            try
            {
                if (!File.Exists(_sizePath))
                {
                    Debug.LogWarning($"[PresetManager] No size preset at {_sizePath}");
                    return;
                }

                var json = File.ReadAllText(_sizePath);
                var dto = JsonUtility.FromJson<SizePresetDto>(json);
                if (dto == null || dto.state == null)
                {
                    Debug.LogWarning("[PresetManager] Invalid size preset JSON.");
                    return;
                }

                StyleRegistry.m_state = dto.state;
                StyleRegistry.ApplyToImGui();
                StyleRegistry.SaveFromImGui();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetManager] Failed to import sizes: {ex.Message}");
            }
        }
        
        private void DrawColorSection()
        {
            ImGui.Text("Colors");
            ImGui.SameLine();
            ImGui.TextDisabled("(all ImGuiCol entries)");

            if (ImGui.Button("Export colors"))
            {
                ExportColors();
            }
            ImGui.SameLine();
            if (ImGui.Button("Import colors"))
            {
                ImportColors();
            }

            ImGui.TextDisabled(_colorPath);
        }

        private void ExportColors()
        {
            try
            {
                ColorRegistry.SaveFromImGui();

                var dto = new ColorPresetDto
                {
                    state = ColorRegistry.m_state
                };

                var json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(_colorPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetManager] Failed to export colors: {ex.Message}");
            }
        }

        private void ImportColors()
        {
            try
            {
                if (!File.Exists(_colorPath))
                {
                    Debug.LogWarning($"[PresetManager] No color preset at {_colorPath}");
                    return;
                }

                var json = File.ReadAllText(_colorPath);
                var dto = JsonUtility.FromJson<ColorPresetDto>(json);
                if (dto?.state == null)
                {
                    Debug.LogWarning("[PresetManager] Invalid color preset JSON.");
                    return;
                }

                ColorRegistry.m_state = dto.state;
                ColorRegistry.m_state.EnsureSize();
                ColorRegistry.m_state.ApplyTo(ImGui.GetStyle());
                ColorRegistry.SaveFromImGui();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetManager] Failed to import colors: {ex.Message}");
            }
        }
    }
}