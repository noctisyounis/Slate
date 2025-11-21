using UnityEngine;
using ImGuiNET;

namespace Style.Runtime
{
    public class ColorSettingsPanel
    {
        private static readonly string[] kPresetDefaults =
        {
            "Current (live)",
            "Preset 1",
            "Preset 2",
            "Preset 3"
        };
        
        private static readonly string[] kBuiltinLabels =
        {
            "ImGui Dark",
            "ImGui Light",
            "ImGui Classic"
        };

        private const string NamesKey = "imgui_color_preset_names_v1";

        private string[] _presetNames = (string[])kPresetDefaults.Clone();
        private bool _namesLoaded;

        private int _selectedPresetIndex = 0;
        private int _selectedBuiltinIndex = 0;

        private const int ColorCount = (int)ImGuiCol.COUNT;
        private readonly bool[]  _previewActive = new bool[ColorCount];
        private readonly float[] _previewEndTime = new float[ColorCount];
        private readonly Color[] _previewOrig = new Color[ColorCount];
        
        private readonly (string label, ImGuiCol[] colors)[] _groups =
        {
            ("Text & Headers", new[]
            {
                ImGuiCol.Text,
                ImGuiCol.TextDisabled,
                ImGuiCol.Header,
                ImGuiCol.HeaderHovered,
                ImGuiCol.HeaderActive,
                ImGuiCol.TitleBg,
                ImGuiCol.TitleBgActive,
                ImGuiCol.TitleBgCollapsed
            }),

            ("Windows / Backgrounds", new[]
            {
                ImGuiCol.WindowBg,
                ImGuiCol.ChildBg,
                ImGuiCol.PopupBg,
                ImGuiCol.MenuBarBg
            }),

            ("Widgets (Frame / Button / Inputs)", new[]
            {
                ImGuiCol.FrameBg,
                ImGuiCol.FrameBgHovered,
                ImGuiCol.FrameBgActive,
                ImGuiCol.Button,
                ImGuiCol.ButtonHovered,
                ImGuiCol.ButtonActive,
                ImGuiCol.CheckMark,
                ImGuiCol.SliderGrab,
                ImGuiCol.SliderGrabActive
            }),

            ("Tabs / Menus", new[]
            {
                ImGuiCol.Tab,
                ImGuiCol.TabHovered,
                ImGuiCol.TabActive,
                ImGuiCol.TabUnfocused,
                ImGuiCol.TabUnfocusedActive
            }),

            ("Scrollbars & Separators", new[]
            {
                ImGuiCol.ScrollbarBg,
                ImGuiCol.ScrollbarGrab,
                ImGuiCol.ScrollbarGrabHovered,
                ImGuiCol.ScrollbarGrabActive,
                ImGuiCol.Separator,
                ImGuiCol.SeparatorHovered,
                ImGuiCol.SeparatorActive
            }),

            ("Tables & Plots", new[]
            {
                ImGuiCol.TableHeaderBg,
                ImGuiCol.TableBorderStrong,
                ImGuiCol.TableBorderLight,
                ImGuiCol.TableRowBg,
                ImGuiCol.TableRowBgAlt,
                ImGuiCol.PlotLines,
                ImGuiCol.PlotLinesHovered,
                ImGuiCol.PlotHistogram,
                ImGuiCol.PlotHistogramHovered
            }),

            ("Navigation & Modals", new[]
            {
                ImGuiCol.NavHighlight,
                ImGuiCol.NavWindowingHighlight,
                ImGuiCol.NavWindowingDimBg,
                ImGuiCol.ModalWindowDimBg
            }),

            ("Misc", new[]
            {
                ImGuiCol.Border,
                ImGuiCol.BorderShadow,
                ImGuiCol.ResizeGrip,
                ImGuiCol.ResizeGripHovered,
                ImGuiCol.ResizeGripActive,
                ImGuiCol.DragDropTarget
            }),
        };
        
        #region Names persistence

            [System.Serializable]
            private class NamesDto
            {
                public string[] names;
            }

            private void EnsureNamesLoaded()
            {
                if (_namesLoaded)
                    return;

                _namesLoaded = true;

                if (!PlayerPrefs.HasKey(NamesKey))
                {
                    _presetNames = (string[])kPresetDefaults.Clone();
                    return;
                }

                try
                {
                    var json = PlayerPrefs.GetString(NamesKey, "");
                    if (string.IsNullOrEmpty(json))
                    {
                        _presetNames = (string[])kPresetDefaults.Clone();
                        return;
                    }

                    var dto = JsonUtility.FromJson<NamesDto>(json);
                    if (dto?.names == null || dto.names.Length != kPresetDefaults.Length)
                    {
                        _presetNames = (string[])kPresetDefaults.Clone();
                        return;
                    }

                    _presetNames = dto.names;
                }
                catch
                {
                    _presetNames = (string[])kPresetDefaults.Clone();
                }
            }

            private void SaveNames()
            {
                var dto = new NamesDto { names = _presetNames };
                var json = JsonUtility.ToJson(dto);
                PlayerPrefs.SetString(NamesKey, json);
                PlayerPrefs.Save();
            }

        #endregion
        
        public void Draw()
        {
            EnsureNamesLoaded();
            
            var style = ImGui.GetStyle();
            
            UpdatePreviews(style);
            
            ImGui.Text("Base color scheme");
            var baseLabel = kBuiltinLabels[_selectedBuiltinIndex];
            if (ImGui.BeginCombo("Base scheme", baseLabel))
            {
                for (var i = 0; i < kBuiltinLabels.Length; i++)
                {
                    var selected = (i == _selectedBuiltinIndex);
                    if (ImGui.Selectable(kBuiltinLabels[i], selected))
                        _selectedBuiltinIndex = i;

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            if (_selectedBuiltinIndex < 3)
            {
                if (ImGui.Button("Apply base scheme"))
                {
                    switch (_selectedBuiltinIndex)
                    {
                        case 0:
                            ImGui.StyleColorsDark();
                            break;
                        case 1:
                            ImGui.StyleColorsLight();
                            break;
                        case 2:
                            ImGui.StyleColorsClassic();
                            break;
                    }
                    
                    var st = ImGui.GetStyle();
                    for (var i = 0; i < (int)ImGuiCol.COUNT; i++)
                    {
                        var v = st.Colors[i];
                        v.w = ColorRegistry.ClampAlpha(v.w);
                        st.Colors[i] = v;
                    }
                    
                    ColorRegistry.SaveFromImGui();
                }
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("Apply base scheme");
                ImGui.EndDisabled();
            }

            ImGui.Spacing();
            ImGui.Separator();

            ImGui.Text("Color presets");
            ImGui.Spacing();

            var currentLabel = _presetNames[_selectedPresetIndex];
            if (ImGui.BeginCombo("Preset", currentLabel))
            {
                for (var i = 0; i < _presetNames.Length; i++)
                {
                    var selected = (i == _selectedPresetIndex);
                    if (ImGui.Selectable(_presetNames[i], selected))
                        _selectedPresetIndex = i;

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
            
            if (_selectedPresetIndex > 0)
            {
                var name = _presetNames[_selectedPresetIndex];
                if (ImGui.InputText("Preset name", ref name, 64))
                {
                    if (string.IsNullOrWhiteSpace(name))
                        name = $"Preset {_selectedPresetIndex}";

                    _presetNames[_selectedPresetIndex] = name;
                    SaveNames();
                }
            }
            else
            {
                ImGui.BeginDisabled();
                var dummy = _presetNames[0];
                ImGui.InputText("Preset name", ref dummy, 64);
                ImGui.EndDisabled();
            }

            if (_selectedPresetIndex > 0)
            {
                if (ImGui.Button("Load preset"))
                    LoadPreset(_selectedPresetIndex, style);

                ImGui.SameLine();

                if (ImGui.Button("Save to preset"))
                    SavePreset(_selectedPresetIndex, style);
            }
            else
            {
                ImGui.BeginDisabled();
                ImGui.Button("Load preset");
                ImGui.SameLine();
                ImGui.Button("Save to preset");
                ImGui.EndDisabled();
            }

            ImGui.SameLine();
            if (ImGui.Button("Apply low-alpha preset"))
                ApplyLowAlphaPreset(style);

            ImGui.Spacing();
            ImGui.Separator();

            
            ImGui.TextDisabled("Hover the (?) button to see what each color controls.");
            ImGui.TextDisabled("Click (?) to temporarily highlight it for short time.");
            ImGui.Spacing();

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.45f);

            foreach (var group in _groups)
            {
                if (!ImGui.CollapsingHeader(group.label, ImGuiTreeNodeFlags.DefaultOpen))
                    continue;

                ImGui.Indent();

                foreach (var col in group.colors)
                {
                    var idx = (int)col;
                    if (idx is < 0 or >= ColorCount)
                        continue;

                    DrawColorLine(style, col);
                }

                ImGui.Unindent();
                ImGui.Separator();
            }

            ImGui.PopItemWidth();

            ImGui.Separator();
            if (ImGui.Button("Save colors##StyleColors"))
                ColorRegistry.SaveFromImGui();
        }
        
        private void DrawColorLine(ImGuiStylePtr style, ImGuiCol colorId)
        {
            var idx = (int)colorId;
            ImGui.PushID(idx);

            if (ImGui.SmallButton("?"))
            {
                StartPreview(style, idx);
            }

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort))
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
                ImGui.TextUnformatted(GetColorDescription(colorId));
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }

            ImGui.SameLine();

            if (ImGui.ColorEdit4(
                    "##Color",
                    ref style.Colors[idx],
                    ImGuiColorEditFlags.AlphaBar |
                    ImGuiColorEditFlags.AlphaPreviewHalf |
                    ImGuiColorEditFlags.DisplayRGB |
                    ImGuiColorEditFlags.InputRGB))
            {
                var v = style.Colors[idx];
                v.w = ColorRegistry.ClampAlpha(v.w);
                style.Colors[idx] = v;
            }

            ImGui.SameLine();
            ImGui.TextUnformatted(ImGui.GetStyleColorName(colorId));
            
            ImGui.SameLine();
            if (ImGui.SmallButton("Reset"))
                ColorRegistry.RevertColor(colorId);

            ImGui.PopID();
        }
        
        private static string GetPresetKey(int slot)
        {
            return $"imgui_color_preset_{slot}";
        }

        private static void SavePreset(int slot, ImGuiStylePtr style)
        {
            var state = new StyleColorState();
            state.CaptureFrom(style);

            var json = JsonUtility.ToJson(state);
            var key  = GetPresetKey(slot);

            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();

            Debug.Log($"[ColorSettings] Saved preset slot {slot}.");
        }

        private void LoadPreset(int slot, ImGuiStylePtr style)
        {
            var key = GetPresetKey(slot);
            if (!PlayerPrefs.HasKey(key))
            {
                Debug.LogWarning($"[ColorSettings] No preset saved for slot {slot}.");
                return;
            }

            var json = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[ColorSettings] Empty preset data for slot {slot}.");
                return;
            }

            var state = new StyleColorState();
            try
            {
                JsonUtility.FromJsonOverwrite(json, state);
                state.EnsureSize();
                state.ApplyTo(style);

                ColorRegistry.m_state.EnsureSize();
                for (var i = 0; i < state.colors.Length; i++)
                    ColorRegistry.m_state.colors[i] = state.colors[i];

                ColorRegistry.SaveFromImGui();
                
                _presetNames[0] = _presetNames[slot];
                SaveNames();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ColorSettings] Failed to load preset slot {slot}: {ex.Message}");
            }
        }
        
        private void StartPreview(ImGuiStylePtr style, int idx)
        {
            var v = style.Colors[idx];
            _previewOrig[idx] = new Color(v.x, v.y, v.z, v.w);

            _previewActive[idx]  = true;
            _previewEndTime[idx] = Time.unscaledTime + 1f;
        }

        private void UpdatePreviews(ImGuiStylePtr style)
        {
            var now = Time.unscaledTime;

            for (var i = 0; i < ColorCount; i++)
            {
                if (!_previewActive[i])
                    continue;

                if (now >= _previewEndTime[i])
                {
                    var orig = _previewOrig[i];
                    var v    = style.Colors[i];
                    v.x = orig.r;
                    v.y = orig.g;
                    v.z = orig.b;
                    v.w = orig.a;
                    style.Colors[i] = v;

                    _previewActive[i] = false;
                    continue;
                }

                var t = now * 0.8f;
                var h = Mathf.Repeat(t + i * 0.11f, 1f);
                var rgb = Color.HSVToRGB(h, 0.8f, 1f);

                var a = _previewOrig[i].a;
                rgb.a   = a;

                var vv = style.Colors[i];
                vv.x = rgb.r;
                vv.y = rgb.g;
                vv.z = rgb.b;
                vv.w = rgb.a;

                style.Colors[i] = vv;
            }
        }

        private void ApplyLowAlphaPreset(ImGuiStylePtr style)
        {
            for (var i = 0; i < (int)ImGuiCol.COUNT; i++)
            {
                var v = style.Colors[i];
                v.w = ColorRegistry.MinAlpha;
                style.Colors[i] = v;
            }

            ColorRegistry.SaveFromImGui();
        }
        
        private static string GetColorDescription(ImGuiCol colorId)
        {
            switch (colorId)
            {
                case ImGuiCol.Text:
                    return "Main text color.";

                case ImGuiCol.TextDisabled:
                    return "Text color for disabled elements.";

                case ImGuiCol.WindowBg:
                    return "Background of standard windows.";
                case ImGuiCol.ChildBg:
                    return "Background of child windows.";
                case ImGuiCol.PopupBg:
                    return "Background of popups and menus.";

                case ImGuiCol.Border:
                    return "Border color of most widgets.";
                case ImGuiCol.BorderShadow:
                    return "Shadow used for some borders.";

                case ImGuiCol.FrameBg:
                    return "Background of sliders, inputs, etc.";
                case ImGuiCol.FrameBgHovered:
                    return "Hovered background of interactive widgets.";
                case ImGuiCol.FrameBgActive:
                    return "Active background of widgets during drag.";

                case ImGuiCol.TitleBg:
                    return "Title bar (inactive).";
                case ImGuiCol.TitleBgActive:
                    return "Title bar (active window).";
                case ImGuiCol.TitleBgCollapsed:
                    return "Title bar of collapsed windows.";

                case ImGuiCol.MenuBarBg:
                    return "Menu bar background.";

                case ImGuiCol.ScrollbarBg:
                    return "Scrollbar background.";
                case ImGuiCol.ScrollbarGrab:
                    return "Scrollbar grab (normal).";
                case ImGuiCol.ScrollbarGrabHovered:
                    return "Scrollbar grab (hovered).";
                case ImGuiCol.ScrollbarGrabActive:
                    return "Scrollbar grab (active).";

                case ImGuiCol.CheckMark:
                    return "Checkbox tick mark.";

                case ImGuiCol.SliderGrab:
                    return "Slider grab handle.";
                case ImGuiCol.SliderGrabActive:
                    return "Slider grab (active).";

                case ImGuiCol.Button:
                    return "Button (normal).";
                case ImGuiCol.ButtonHovered:
                    return "Button (hovered).";
                case ImGuiCol.ButtonActive:
                    return "Button (active).";

                case ImGuiCol.Header:
                    return "Header background.";
                case ImGuiCol.HeaderHovered:
                    return "Header (hovered).";
                case ImGuiCol.HeaderActive:
                    return "Header (active).";

                case ImGuiCol.Separator:
                    return "Separator lines.";
                case ImGuiCol.SeparatorHovered:
                    return "Separator (hovered).";
                case ImGuiCol.SeparatorActive:
                    return "Separator (active).";

                case ImGuiCol.ResizeGrip:
                    return "Window resize grip.";
                case ImGuiCol.ResizeGripHovered:
                    return "Resize grip (hovered).";
                case ImGuiCol.ResizeGripActive:
                    return "Resize grip (active).";

                case ImGuiCol.Tab:
                    return "Tab (normal).";
                case ImGuiCol.TabHovered:
                    return "Tab (hovered).";
                case ImGuiCol.TabActive:
                    return "Tab (active).";
                case ImGuiCol.TabUnfocused:
                    return "Tab (in unfocused window).";
                case ImGuiCol.TabUnfocusedActive:
                    return "Active tab in unfocused window.";

                case ImGuiCol.DockingPreview:
                    return "Docking preview overlay.";
                case ImGuiCol.DockingEmptyBg:
                    return "Background of empty docking spaces.";

                case ImGuiCol.NavHighlight:
                    return "Keyboard/gamepad navigation highlight.";
                case ImGuiCol.NavWindowingHighlight:
                    return "Highlighted target window (e.g. Alt+Tab).";
                case ImGuiCol.NavWindowingDimBg:
                    return "Dim background during windowing.";
                case ImGuiCol.ModalWindowDimBg:
                    return "Dim background behind modal windows.";

                case ImGuiCol.PlotLines:
                case ImGuiCol.PlotLinesHovered:
                case ImGuiCol.PlotHistogram:
                case ImGuiCol.PlotHistogramHovered:
                case ImGuiCol.TableHeaderBg:
                case ImGuiCol.TableBorderStrong:
                case ImGuiCol.TableBorderLight:
                case ImGuiCol.TableRowBg:
                case ImGuiCol.TableRowBgAlt:
                case ImGuiCol.TextSelectedBg:
                case ImGuiCol.DragDropTarget:
                case ImGuiCol.COUNT:
                default:
                    return "Internal ImGui color (rarely used directly).";
            }
        }
    }
}