using UnityEngine;
using ImGuiNET;
using System;

namespace SharedData.Runtime
{
    [Serializable]
    public class StyleSizeState
    {
        #region Public

            public Vector2 windowPadding;
            public Vector2 framePadding;
            public Vector2 itemSpacing;
            public Vector2 itemInnerSpacing;
            public Vector2 touchExtraPadding;

            public float indentSpacing;
            public float grabMinSize;

            public float windowBorderSize;
            public float childBorderSize;
            public float popupBorderSize;
            public float frameBorderSize;

            public float windowRounding;
            public float childRounding;
            public float frameRounding;
            public float popupRounding;
            public float grabRounding;

            public float scrollbarSize;
            public float scrollbarRounding;

            public float tabBorderSize;
            public float tabRounding;

            public Vector2 displayWindowPadding;
            public Vector2 displaySafeAreaPadding;

        #endregion

        public void CaptureFrom(ImGuiStylePtr style)
        {
            windowPadding = style.WindowPadding;
            framePadding = style.FramePadding;
            itemSpacing = style.ItemSpacing;
            itemInnerSpacing = style.ItemInnerSpacing;
            touchExtraPadding = style.TouchExtraPadding;

            indentSpacing = style.IndentSpacing;
            grabMinSize = style.GrabMinSize;

            windowBorderSize = style.WindowBorderSize;
            childBorderSize = style.ChildBorderSize;
            popupBorderSize = style.PopupBorderSize;
            frameBorderSize = style.FrameBorderSize;

            windowRounding = style.WindowRounding;
            childRounding = style.ChildRounding;
            frameRounding = style.FrameRounding;
            popupRounding = style.PopupRounding;
            grabRounding = style.GrabRounding;

            scrollbarSize = style.ScrollbarSize;
            scrollbarRounding = style.ScrollbarRounding;

            tabBorderSize = style.TabBorderSize;
            tabRounding = style.TabRounding;

            displayWindowPadding = style.DisplayWindowPadding;
            displaySafeAreaPadding = style.DisplaySafeAreaPadding;
        }

        public void ApplyTo(ImGuiStylePtr style)
        {
            style.WindowPadding = windowPadding;
            style.FramePadding = framePadding;
            style.ItemSpacing = itemSpacing;
            style.ItemInnerSpacing = itemInnerSpacing;
            style.TouchExtraPadding = touchExtraPadding;

            style.IndentSpacing = indentSpacing;
            style.GrabMinSize = grabMinSize;

            style.WindowBorderSize = windowBorderSize;
            style.ChildBorderSize = childBorderSize;
            style.PopupBorderSize = popupBorderSize;
            style.FrameBorderSize = frameBorderSize;

            style.WindowRounding = windowRounding;
            style.ChildRounding = childRounding;
            style.FrameRounding = frameRounding;
            style.PopupRounding = popupRounding;
            style.GrabRounding = grabRounding;

            style.ScrollbarSize = scrollbarSize;
            style.ScrollbarRounding = scrollbarRounding;

            style.TabBorderSize = tabBorderSize;
            style.TabRounding = tabRounding;

            style.DisplayWindowPadding = displayWindowPadding;
            style.DisplaySafeAreaPadding = displaySafeAreaPadding;
        }
    }
    
    public class StyleRegistry
    {
        private const string Key = "imgui_style_sizes_v1";

        public static StyleSizeState m_state = new StyleSizeState();

        private static bool s_loaded;
        private static bool s_appliedOnce;

        private static void LoadIfNeeded()
        {
            if (s_loaded) return;
            s_loaded = true;

            var style = ImGui.GetStyle();

            if (PlayerPrefs.HasKey(Key))
            {
                try
                {
                    var json = PlayerPrefs.GetString(Key, "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, m_state);
                        m_state.ApplyTo(style);
                        s_appliedOnce = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[StyleSizeRegistry] Failed to load, using defaults: {ex.Message}");
                }
            }

            m_state.CaptureFrom(style);
            s_appliedOnce = true;
        }

        public static void ApplyToImGui()
        {
            LoadIfNeeded();
            
            if (s_appliedOnce) return;

            m_state.ApplyTo(ImGui.GetStyle());
            s_appliedOnce = true;
        }

        public static void SaveFromImGui()
        {
            var style = ImGui.GetStyle();
            m_state.CaptureFrom(style);

            var json = JsonUtility.ToJson(m_state);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
            
            s_appliedOnce = true;
        }
    }
}