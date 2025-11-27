using ImGuiNET;
using System;
using UnityEngine;

namespace SharedData.Runtime
{
    #region Enumerations

        public enum FontKind
        {
            UImGUIDefault,
            NotoSymbols2,
            OpenDyslexic
        }

    #endregion
    
    public class FontRegistry
    {
        #region Public
        
            public static ImFontPtr m_defaultFont; 
            public static ImFontPtr m_notoFont;
            public static ImFontPtr m_openDysFont;

            public static FontKind m_currentFont;
            
            public static float m_fontScale = 1f;

        #endregion

        #region Unity
        
            private static void LoadPrefsIfNeeded()
            {
                if (s_loaded) return;
                s_loaded = true;

                if (PlayerPrefs.HasKey(KeyKind))
                {
                    var kindInt = PlayerPrefs.GetInt(KeyKind, (int)FontKind.NotoSymbols2);
                    if (Enum.IsDefined(typeof(FontKind), kindInt))
                        m_currentFont = (FontKind)kindInt;
                    else
                        m_currentFont = FontKind.NotoSymbols2;
                }

                if (!PlayerPrefs.HasKey(KeyScale)) return;
                m_fontScale = PlayerPrefs.GetFloat(KeyScale, 1f);
                if (m_fontScale <= 0f) m_fontScale = 1.0f;
            }

            public static void SavePrefs()
            {
                PlayerPrefs.SetInt(KeyKind,  (int)m_currentFont);
                PlayerPrefs.SetFloat(KeyScale, m_fontScale);
                PlayerPrefs.Save();
            }
        
            public static ImFontPtr GetCurrentFont()
            {
                LoadPrefsIfNeeded();
                
                switch (m_currentFont)
                {
                    case FontKind.NotoSymbols2:
                        return m_notoFont;

                    case FontKind.OpenDyslexic:
                        return m_openDysFont;

                    case FontKind.UImGUIDefault:
                    default:
                        return m_defaultFont;
                }
            }

            public static unsafe void ApplyAsDefault()
            {
                LoadPrefsIfNeeded();
                
                var io = ImGui.GetIO();
                
                io.FontGlobalScale = m_fontScale;
                
                var font = GetCurrentFont();
                if (font.NativePtr == null)
                    return;

                io.NativePtr->FontDefault = font.NativePtr;
            }
            
        #endregion

        #region Private

            private const string KeyKind  = "imgui_font_kind";
            private const string KeyScale = "imgui_font_scale";
            private static bool  s_loaded;

        #endregion
    }
}