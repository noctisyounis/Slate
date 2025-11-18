using UnityEngine;
using System;
using ImGuiNET;

namespace Style.Runtime
{
    [Serializable]
    public class StyleColorState
    {
        public Color[] colors;

        public void EnsureSize()
        {
            var n = (int)ImGuiCol.COUNT;
            if (colors == null || colors.Length != n)
                colors = new Color[n];
        }

        public void CaptureFrom(ImGuiStylePtr style)
        {
            EnsureSize();
            var n = (int)ImGuiCol.COUNT;

            for (var i = 0; i < n; i++)
            {
                var v = style.Colors[i];
                colors[i] = new Color(v.x, v.y, v.z, v.w);
            }
        }

        public void ApplyTo(ImGuiStylePtr style)
        {
            if (colors == null) return;

            var n = Math.Min(colors.Length, (int)ImGuiCol.COUNT);
            for (var i = 0; i < n; i++)
            {
                var c = colors[i];
                var v = style.Colors[i];

                v.x = c.r;
                v.y = c.g;
                v.z = c.b;
                v.w = c.a;

                style.Colors[i] = v;
            }
        }
    }
    public class ColorRegistry
    {
        private const string Key = "imgui_style_colors_v1";

        public static StyleColorState m_state = new StyleColorState();

        private static bool s_loaded;
        private static bool s_appliedOnce;

        private static void LoadIfNeeded()
        {
            if (s_loaded) return;
            s_loaded = true;

            var style = ImGui.GetStyle();
            m_state.EnsureSize();

            if (PlayerPrefs.HasKey(Key))
            {
                try
                {
                    var json = PlayerPrefs.GetString(Key, "");
                    if (!string.IsNullOrEmpty(json))
                    {
                        JsonUtility.FromJsonOverwrite(json, m_state);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ColorRegistry] Load error, using live style: {ex.Message}");
                }
            }

            m_state.CaptureFrom(style);
        }

        public static void ApplyOnce()
        {
            LoadIfNeeded();
            if (s_appliedOnce) return;

            m_state.ApplyTo(ImGui.GetStyle());
            s_appliedOnce = true;
        }

        public static void SaveFromImGui()
        {
            m_state.CaptureFrom(ImGui.GetStyle());

            var json = JsonUtility.ToJson(m_state);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();

            s_appliedOnce = true;
        }
    }
}