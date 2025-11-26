using UnityEngine;
using System;
using System.IO;
using ImGuiNET;
using SharedData.Runtime;

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
        private static string _sizePath;
        private static string _colorPath;

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

        #region Font
        
            private void DrawFontSection()
            {
                ImGui.Text("Fonts");
                ImGui.SameLine();
                ImGui.TextDisabled("(font kind + scale)");

                if (ImGui.Button("Export fonts"))
                    ExportFonts();
                ImGui.SameLine();
                if (ImGui.Button("Import fonts"))
                    ImportFontsWithPicker();

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

            private void ImportFontsWithPicker()
            {
                try
                {
                    var path = PickJsonFile(_fontPath);
                    if (string.IsNullOrEmpty(path))
                        return;

                    _fontPath = path;
                    ImportFontsFromPath(path);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PresetManager] Failed to import fonts: {ex.Message}");
                }
            }

            private static void ImportFontsFromPath(string path)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[PresetManager] No font preset at {path}");
                    return;
                }

                var json = File.ReadAllText(path);
                var dto  = JsonUtility.FromJson<FontPresetDto>(json);
                if (dto == null)
                {
                    Debug.LogWarning("[PresetManager] Invalid font preset JSON.");
                    return;
                }

                FontRegistry.m_currentFont = (FontKind)dto.fontKind;
                FontRegistry.m_fontScale   = dto.scale;
                FontRegistry.SavePrefs();
                FontRegistry.ApplyAsDefault();
            }

        #endregion
        
        #region Sizes

            private void DrawSizeSection()
            {
                ImGui.Text("Style sizes");
                ImGui.SameLine();
                ImGui.TextDisabled("(padding, rounding, spacing, etc.)");

                if (ImGui.Button("Export sizes"))
                    ExportSizes();
                ImGui.SameLine();
                if (ImGui.Button("Import sizes"))
                    ImportSizesWithPicker();

                ImGui.TextDisabled(_sizePath);
            }

            public static void ExportSizes()
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

            private void ImportSizesWithPicker()
            {
                try
                {
                    var path = PickJsonFile(_sizePath);
                    if (string.IsNullOrEmpty(path))
                        return;

                    _sizePath = path;
                    ImportSizesFromPath(path);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PresetManager] Failed to import sizes: {ex.Message}");
                }
            }

            private static void ImportSizesFromPath(string path)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[PresetManager] No size preset at {path}");
                    return;
                }

                var json = File.ReadAllText(path);
                var dto  = JsonUtility.FromJson<SizePresetDto>(json);
                if (dto == null || dto.state == null)
                {
                    Debug.LogWarning("[PresetManager] Invalid size preset JSON.");
                    return;
                }

                StyleRegistry.m_state = dto.state;
                StyleRegistry.ApplyToImGui();
                StyleRegistry.SaveFromImGui();
            }

        #endregion
        
        #region Colors

            private void DrawColorSection()
            {
                ImGui.Text("Colors");
                ImGui.SameLine();
                ImGui.TextDisabled("(all ImGuiCol entries)");

                if (ImGui.Button("Export colors"))
                    ExportColors();
                ImGui.SameLine();
                if (ImGui.Button("Import colors"))
                    ImportColorsWithPicker();

                ImGui.TextDisabled(_colorPath);
            }

            public static void ExportColors()
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

            private void ImportColorsWithPicker()
            {
                try
                {
                    var path = PickJsonFile(_colorPath);
                    if (string.IsNullOrEmpty(path))
                        return;

                    _colorPath = path;
                    ImportColorsFromPath(path);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[PresetManager] Failed to import colors: {ex.Message}");
                }
            }

            private static void ImportColorsFromPath(string path)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[PresetManager] No color preset at {path}");
                    return;
                }

                var json = File.ReadAllText(path);
                var dto  = JsonUtility.FromJson<ColorPresetDto>(json);
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

        #endregion
        
        #region File picker helper

            private static string PickJsonFile(string currentPath)
            {
#if UNITY_EDITOR
                var dir = string.IsNullOrEmpty(currentPath) ? Application.dataPath : Path.GetDirectoryName(currentPath);
                var file = !string.IsNullOrEmpty(currentPath) ? Path.GetFileName(currentPath) : "";
                var chosen = UnityEditor.EditorUtility.OpenFilePanel("Import preset", dir, "json");
                return string.IsNullOrEmpty(chosen) ? currentPath : chosen;
#else
                Debug.Log("[PresetManager] File picker is only available in the Editor. Using default path.");
                return currentPath;
#endif
            }

        #endregion
    }
}