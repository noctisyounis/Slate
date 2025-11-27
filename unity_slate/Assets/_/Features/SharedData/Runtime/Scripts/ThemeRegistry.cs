using UnityEngine;
using ImGuiNET;

namespace SharedData.Runtime
{
    public class ThemeRegistry
    {
        public static ImGUIThemeAsset CurrentTheme { get; private set; }

        private static Vector4[] _savedColors;
        private static bool _hasSavedColors;

        public static void SetCurrentTheme(ImGUIThemeAsset theme)
        {
            CurrentTheme = theme;
        }
        
        public static void ApplyCurrentToImGui()
        {
            if (CurrentTheme == null) return;
        
            var style = ImGui.GetStyle();
        
            style.Colors[(int)ImGuiCol.Text] = ToVec4(CurrentTheme.text);
            style.Colors[(int)ImGuiCol.WindowBg] = ToVec4(CurrentTheme.windowBg);
            style.Colors[(int)ImGuiCol.ChildBg] = ToVec4(CurrentTheme.childBg);
            style.Colors[(int)ImGuiCol.PopupBg] = ToVec4(CurrentTheme.popupBg);
            style.Colors[(int)ImGuiCol.Border] = ToVec4(CurrentTheme.border);
            style.Colors[(int)ImGuiCol.BorderShadow] = ToVec4(CurrentTheme.borderShadow);
        
            style.Colors[(int)ImGuiCol.FrameBg] = ToVec4(CurrentTheme.frameBg);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = ToVec4(CurrentTheme.frameBgHovered);
            style.Colors[(int)ImGuiCol.FrameBgActive] = ToVec4(CurrentTheme.frameBgActive);
        
            style.Colors[(int)ImGuiCol.Button] = ToVec4(CurrentTheme.button);
            style.Colors[(int)ImGuiCol.ButtonHovered] = ToVec4(CurrentTheme.buttonHovered);
            style.Colors[(int)ImGuiCol.ButtonActive] = ToVec4(CurrentTheme.buttonActive);
        
            style.Colors[(int)ImGuiCol.Header] = ToVec4(CurrentTheme.header);
            style.Colors[(int)ImGuiCol.HeaderHovered] = ToVec4(CurrentTheme.headerHovered);
            style.Colors[(int)ImGuiCol.HeaderActive] = ToVec4(CurrentTheme.headerActive);
        
            style.Colors[(int)ImGuiCol.MenuBarBg] = ToVec4(CurrentTheme.menuBarBg);
            style.Colors[(int)ImGuiCol.TitleBg] = ToVec4(CurrentTheme.titleBg);
            style.Colors[(int)ImGuiCol.TitleBgActive] = ToVec4(CurrentTheme.titleBgActive);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = ToVec4(CurrentTheme.titleBgCollapsed);
        }
        
        public static void PushTheme(ImGUIThemeAsset theme)
        {
            if (theme == null) return;
            
            var style = ImGui.GetStyle();
            
            if (!_hasSavedColors)
            {
                _savedColors = new Vector4[(int)ImGuiCol.COUNT];
                for (var i = 0; i < (int)ImGuiCol.COUNT; ++i)
                    _savedColors[i] = style.Colors[i];

                _hasSavedColors = true;
            }
            
            SetCurrentTheme(theme);
            ApplyCurrentToImGui();
        }
        
        public static void PopTheme()
        {
            if (!_hasSavedColors) return;

            var style = ImGui.GetStyle();
            for (var i = 0; i < (int)ImGuiCol.COUNT; ++i)
                style.Colors[i] = _savedColors[i];

            _hasSavedColors = false;

            if (CurrentTheme != null)
                ApplyCurrentToImGui();
        }
        
        public static Vector4 GhostButtonColor => CurrentTheme != null ? ToVec4(CurrentTheme.ghostButton) : new Vector4(0, 0, 0, 0);
        public static Vector4 GhostButtonHoverColor => CurrentTheme != null ? ToVec4(CurrentTheme.ghostHover) : new Vector4(1, 1, 1, 0.08f);
        public static Vector4 GhostButtonActiveColor => CurrentTheme != null ? ToVec4(CurrentTheme.ghostActive) : new Vector4(1, 1, 1, 0.12f);

        private static Vector4 ToVec4(Color c) => new Vector4(c.r, c.g, c.b, c.a);
    }
}