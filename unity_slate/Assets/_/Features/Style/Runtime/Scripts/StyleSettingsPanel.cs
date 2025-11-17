using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using UnityEngine;

namespace Style.Runtime
{
    [Serializable]
    public class StylePresetData
    {
        public string name;

        public Color text = Color.white;
        public Color windowBg = new Color(0.06f, 0.06f, 0.06f, 1f);
        public Color frameBg = new Color(0.20f, 0.25f, 0.30f, 1f);
        public Color button = new Color(0.26f, 0.59f, 0.98f, 0.40f);
        public Color buttonHover = new Color(0.26f, 0.59f, 0.98f, 1f);
        public Color buttonActive = new Color(0.06f, 0.53f, 0.98f, 1f);
    }
    
    public class StyleSettingsPanel
    {
        public void Draw()
        {
            InitOnce();
            
            var style = ImGui.GetStyle();

            if (_presets.Count == 0)
            {
                ImGui.Text("No style presets found.");
                ImGui.TextDisabled($"Folder: {_presetsFolder}");
                if (!ImGui.Button("Create default preset")) return;
                CreateDefaultPresetOnDisk();
                ReloadPresets();
                return;
            }

            var current = _presets[Mathf.Clamp(_selectedIndex, 0, _presets.Count - 1)];
            var currentLabel = string.IsNullOrEmpty(current.name)
                ? $"Preset {_selectedIndex}"
                : current.name;

            if (ImGui.BeginCombo("Style preset", currentLabel))
            {
                for (var i = 0; i < _presets.Count; i++)
                {
                    var selected = (i == _selectedIndex);
                    var label  = string.IsNullOrEmpty(_presets[i].name)
                        ? $"Preset {i}"
                        : _presets[i].name;

                    if (ImGui.Selectable(label, selected))
                        _selectedIndex = i;

                    if (selected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }

            ImGui.Separator();
            ImGui.Text("Preview :");

            PushPresetColors(current);
            if (ImGui.Button("Button preview"))
            {
                // no-op, juste pour visuel
            }
            ImGui.SameLine();
            ImGui.Button("Another", new Vector2(120, 0));
            PopPresetColors();
            
            ImGui.Spacing();
            ImGui.Text("Dropdown preview :");
            PushPresetColors(current);
            
            ImGui.BeginChild(
                "DropdownPreview",
                new Vector2(0, 80),
                ImGuiChildFlags.Border,
                ImGuiWindowFlags.None
            );

            if (ImGui.BeginMenu("File..."))
            {
                ImGui.MenuItem("New");
                ImGui.MenuItem("Open");
                ImGui.MenuItem("Save");
                ImGui.MenuItem("Exit");
                ImGui.EndMenu();
            }

            ImGui.EndChild();
            PopPresetColors();

            ImGui.Spacing();
            ImGui.Separator();
            
            ImGui.Text("Edit preset colors :");

            EditColor("Text", ref current.text);
            EditColor("Window Bg", ref current.windowBg);
            EditColor("Frame Bg", ref current.frameBg);
            EditColor("Button", ref current.button);
            EditColor("Button Hovered", ref current.buttonHover);
            EditColor("Button Active", ref current.buttonActive);

            ImGui.Spacing();
            ImGui.Separator();

            if (ImGui.Button("Apply##Style"))
            {
                ApplyPresetToImGui(current);
                Debug.Log($"[Settings.Style] Apply preset '{current.name}'");
            }

            ImGui.SameLine();
            ImGui.TextDisabled("(colors only, geometry below reste live)");

            ImGui.Spacing();
            ImGui.Separator();

            ImGui.Text("Create new style from current ImGui style:");
            ImGui.InputText("Name", ref _newPresetName, 64);

            if (ImGui.Button("Save current as preset"))
            {
                SaveCurrentImGuiStyleAsPreset(_newPresetName);
                ReloadPresets();
            }

            ImGui.SameLine();
            ImGui.TextDisabled($"Folder: {_presetsFolder}");
            
            ImGui.Text("ImGui style (live, façon ShowStyleEditor) :");

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);

            if (ImGui.BeginTabBar("StyleDetailsTabs"))
            {
                var scrollbarRounding = style.ScrollbarRounding;
                if (ImGui.BeginTabItem("Sizes"))
                {
                    ImGui.SeparatorText("Main");

                    var windowPadding = style.WindowPadding;
                    if (ImGui.SliderFloat2("WindowPadding", ref windowPadding, 0f, 20f, "%.0f"))
                        style.WindowPadding = windowPadding;

                    var framePadding = style.FramePadding;
                    if (ImGui.SliderFloat2("FramePadding", ref framePadding, 0f, 20f, "%.0f"))
                        style.FramePadding = framePadding;

                    var itemSpacing = style.ItemSpacing;
                    if (ImGui.SliderFloat2("ItemSpacing", ref itemSpacing, 0f, 20f, "%.0f"))
                        style.ItemSpacing = itemSpacing;

                    var itemInnerSpacing = style.ItemInnerSpacing;
                    if (ImGui.SliderFloat2("ItemInnerSpacing", ref itemInnerSpacing, 0f, 20f, "%.0f"))
                        style.ItemInnerSpacing = itemInnerSpacing;

                    var touchExtraPadding = style.TouchExtraPadding;
                    if (ImGui.SliderFloat2("TouchExtraPadding", ref touchExtraPadding, 0f, 10f, "%.0f"))
                        style.TouchExtraPadding = touchExtraPadding;

                    var indentSpacing = style.IndentSpacing;
                    if (ImGui.SliderFloat("IndentSpacing", ref indentSpacing, 0f, 30f, "%.0f"))
                        style.IndentSpacing = indentSpacing;

                    var grabMinSize = style.GrabMinSize;
                    if (ImGui.SliderFloat("GrabMinSize", ref grabMinSize, 1f, 20f, "%.0f"))
                        style.GrabMinSize = grabMinSize;

                    ImGui.SeparatorText("Borders");

                    var windowBorderSize = style.WindowBorderSize;
                    if (ImGui.SliderFloat("WindowBorderSize", ref windowBorderSize, 0f, 1f, "%.0f"))
                        style.WindowBorderSize = windowBorderSize;

                    var childBorderSize = style.ChildBorderSize;
                    if (ImGui.SliderFloat("ChildBorderSize", ref childBorderSize, 0f, 1f, "%.0f"))
                        style.ChildBorderSize = childBorderSize;

                    var popupBorderSize = style.PopupBorderSize;
                    if (ImGui.SliderFloat("PopupBorderSize", ref popupBorderSize, 0f, 1f, "%.0f"))
                        style.PopupBorderSize = popupBorderSize;

                    var frameBorderSize = style.FrameBorderSize;
                    if (ImGui.SliderFloat("FrameBorderSize", ref frameBorderSize, 0f, 1f, "%.0f"))
                        style.FrameBorderSize = frameBorderSize;

                    ImGui.SeparatorText("Rounding");

                    var windowRounding = style.WindowRounding;
                    if (ImGui.SliderFloat("WindowRounding", ref windowRounding, 0f, 12f, "%.0f"))
                        style.WindowRounding = windowRounding;

                    var childRounding = style.ChildRounding;
                    if (ImGui.SliderFloat("ChildRounding", ref childRounding, 0f, 12f, "%.0f"))
                        style.ChildRounding = childRounding;

                    var frameRounding = style.FrameRounding;
                    if (ImGui.SliderFloat("FrameRounding", ref frameRounding, 0f, 12f, "%.0f"))
                        style.FrameRounding = frameRounding;

                    var popupRounding = style.PopupRounding;
                    if (ImGui.SliderFloat("PopupRounding", ref popupRounding, 0f, 12f, "%.0f"))
                        style.PopupRounding = popupRounding;

                    var grabRounding = style.GrabRounding;
                    if (ImGui.SliderFloat("GrabRounding", ref grabRounding, 0f, 12f, "%.0f"))
                        style.GrabRounding = grabRounding;

                    ImGui.SeparatorText("Scrollbar");

                    float scrollbarSize = style.ScrollbarSize;
                    if (ImGui.SliderFloat("ScrollbarSize", ref scrollbarSize, 1f, 20f, "%.0f"))
                        style.ScrollbarSize = scrollbarSize;

                    if (ImGui.SliderFloat("ScrollbarRounding", ref scrollbarRounding, 0f, 12f, "%.0f"))
                        style.ScrollbarRounding = scrollbarRounding;

                    ImGui.SeparatorText("Tabs");

                    var tabBorderSize = style.TabBorderSize;
                    if (ImGui.SliderFloat("TabBorderSize", ref tabBorderSize, 0f, 1f, "%.0f"))
                        style.TabBorderSize = tabBorderSize;

                    var tabRounding = style.TabRounding;
                    if (ImGui.SliderFloat("TabRounding", ref tabRounding, 0f, 12f, "%.0f"))
                        style.TabRounding = tabRounding;

                    ImGui.SeparatorText("Display");

                    var displayWindowPadding = style.DisplayWindowPadding;
                    if (ImGui.SliderFloat2("DisplayWindowPadding", ref displayWindowPadding, 0f, 30f, "%.0f"))
                        style.DisplayWindowPadding = displayWindowPadding;

                    var displaySafeAreaPadding = style.DisplaySafeAreaPadding;
                    if (ImGui.SliderFloat2("DisplaySafeAreaPadding", ref displaySafeAreaPadding, 0f, 30f, "%.0f"))
                        style.DisplaySafeAreaPadding = displaySafeAreaPadding;

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Rendering"))
                {
                    var aaLines = style.AntiAliasedLines;
                    if (ImGui.Checkbox("Anti-aliased lines", ref aaLines))
                        style.AntiAliasedLines = aaLines;

                    var aaFill = style.AntiAliasedFill;
                    if (ImGui.Checkbox("Anti-aliased fill", ref aaFill))
                        style.AntiAliasedFill = aaFill;

                    ImGui.Spacing();

                    var curveTol = style.CurveTessellationTol;
                    if (ImGui.DragFloat("Curve Tessellation Tol", ref curveTol, 0.02f, 0.1f, 10f, "%.2f"))
                        style.CurveTessellationTol = Mathf.Max(0.1f, curveTol);

                    var circleErr = style.CircleTessellationMaxError;
                    if (ImGui.DragFloat("Circle Tessellation Max Error", ref circleErr, 0.005f, 0.1f, 5f, "%.2f"))
                        style.CircleTessellationMaxError = circleErr;

                    ImGui.Spacing();

                    var alpha = style.Alpha;
                    if (ImGui.DragFloat("Global Alpha", ref alpha, 0.005f, 0.2f, 1f, "%.2f"))
                        style.Alpha = Mathf.Clamp(alpha, 0.2f, 1f);

                    var disabledAlpha = style.DisabledAlpha;
                    if (ImGui.DragFloat("Disabled Alpha", ref disabledAlpha, 0.005f, 0f, 1f, "%.2f"))
                        style.DisabledAlpha = Mathf.Clamp01(disabledAlpha);

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.PopItemWidth();
        }

        #region Init / IO

        private void InitOnce()
        {
            if (_initialized) return;
            _initialized = true;
            
            _presetsFolder = Path.Combine(Application.persistentDataPath, "imgui_styles");

            Directory.CreateDirectory(_presetsFolder);
            ReloadPresets();
        }

        private void ReloadPresets()
        {
            _presets.Clear();
            _presetFiles.Clear();

            var files = Directory.GetFiles(_presetsFolder, "*.json");
            foreach (var f in files)
            {
                try
                {
                    var json = File.ReadAllText(f);
                    var data = JsonUtility.FromJson<StylePresetData>(json);
                    if (data == null) continue;
                    if (string.IsNullOrEmpty(data.name))
                        data.name = Path.GetFileNameWithoutExtension(f);

                    _presets.Add(data);
                    _presetFiles.Add(f);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[StyleSettings] Failed to load preset '{f}': {ex.Message}");
                }
            }

            if (_presets.Count == 0)
            {
                CreateDefaultPresetOnDisk();
                ReloadPresets();
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _presets.Count - 1);
        }

        private void CreateDefaultPresetOnDisk()
        {
            var preset = CaptureCurrentImGuiStyle();
            preset.name = "Default";

            var path = Path.Combine(_presetsFolder, "Default.json");
            var json = JsonUtility.ToJson(preset, true);
            File.WriteAllText(path, json);
            Debug.Log($"[StyleSettings] Default preset saved at {path}");
        }

        #endregion

        #region Capture / Apply

        private StylePresetData CaptureCurrentImGuiStyle()
        {
            var style = ImGui.GetStyle();
            return new StylePresetData
            {
                text = ToColor(style.Colors[(int)ImGuiCol.Text]),
                windowBg = ToColor(style.Colors[(int)ImGuiCol.WindowBg]),
                frameBg = ToColor(style.Colors[(int)ImGuiCol.FrameBg]),
                button = ToColor(style.Colors[(int)ImGuiCol.Button]),
                buttonHover = ToColor(style.Colors[(int)ImGuiCol.ButtonHovered]),
                buttonActive = ToColor(style.Colors[(int)ImGuiCol.ButtonActive]),
            };
        }

        private void SaveCurrentImGuiStyleAsPreset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = "Preset";

            var data = CaptureCurrentImGuiStyle();
            data.name = name;

            var safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));
            var path = Path.Combine(_presetsFolder, safeName + ".json");

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log($"[StyleSettings] Saved preset '{name}' at {path}");
        }

        private void ApplyPresetToImGui(StylePresetData preset)
        {
            var style = ImGui.GetStyle();
            style.Colors[(int)ImGuiCol.Text] = ToVec4(preset.text);
            style.Colors[(int)ImGuiCol.WindowBg] = ToVec4(preset.windowBg);
            style.Colors[(int)ImGuiCol.FrameBg] = ToVec4(preset.frameBg);
            style.Colors[(int)ImGuiCol.Button] = ToVec4(preset.button);
            style.Colors[(int)ImGuiCol.ButtonHovered] = ToVec4(preset.buttonHover);
            style.Colors[(int)ImGuiCol.ButtonActive] = ToVec4(preset.buttonActive);
        }

        #endregion

        #region Preview helpers
        
            private void PushPresetColors(StylePresetData preset)
            {
                var style = ImGui.GetStyle();

                if (!_hasSaved)
                {
                    _savedButton = style.Colors[(int)ImGuiCol.Button];
                    _savedButtonHovered = style.Colors[(int)ImGuiCol.ButtonHovered];
                    _savedButtonActive = style.Colors[(int)ImGuiCol.ButtonActive];
                    _hasSaved = true;
                }

                style.Colors[(int)ImGuiCol.Button] = ToVec4(preset.button);
                style.Colors[(int)ImGuiCol.ButtonHovered] = ToVec4(preset.buttonHover);
                style.Colors[(int)ImGuiCol.ButtonActive] = ToVec4(preset.buttonActive);
            }

            private void PopPresetColors()
            {
                if (!_hasSaved) return;

                var style = ImGui.GetStyle();
                style.Colors[(int)ImGuiCol.Button] = _savedButton;
                style.Colors[(int)ImGuiCol.ButtonHovered] = _savedButtonHovered;
                style.Colors[(int)ImGuiCol.ButtonActive] = _savedButtonActive;
                _hasSaved = false;
            }

        #endregion

        #region Conversion helpers

            private static Vector4 ToVec4(Color c) =>
                new Vector4(c.r, c.g, c.b, c.a);

            private static Color ToColor(Vector4 v) =>
                new Color(v.x, v.y, v.z, v.w);
            
            private static void EditColor(string label, ref Color color)
            {
                var v = new Vector4(color.r, color.g, color.b, color.a);

                if (ImGui.ColorEdit4(label, ref v))
                {
                    color = new Color(v.x, v.y, v.z, v.w);
                }
            }

        #endregion

        #region Private

        private string _presetsFolder;

            private readonly List<StylePresetData> _presets = new();
            private readonly List<string> _presetFiles = new();

            private int _selectedIndex;
            private bool _initialized;
            private string _newPresetName = "MyStyle";
            
            private Vector4 _savedButton;
            private Vector4 _savedButtonHovered;
            private Vector4 _savedButtonActive;
            private bool _hasSaved;

        #endregion
    }
}