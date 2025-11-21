using UnityEngine;
using ImGuiNET;
using System;
using System.IO;

namespace Style.Runtime
{
    public class FontInitializer : MonoBehaviour
    {
        #region Header
        
            [Header("Font settings")]
            [Tooltip("Chemin Unity vers la font (relative au projet)")]
            public string m_defaultRelativePath = "_/Content/Fonts/droidSans.ttf";
            public string m_notoRelativePath = "_/Content/Fonts/NotoSansSymbols2-Regular.ttf";
            public string m_dysRelativePath = "_/Content/Fonts/OpenDyslexic3-Regular.ttf";

            [Tooltip("Taille de la font ImGui")]
            public float m_fontSize = 22f;

        #endregion

        #region Unity
        
            public void AddToolbarFont(ImGuiIOPtr io)
            {
                var defaultFontPath = ResolveFontPath(m_defaultRelativePath, "Default");
                var notoPath = ResolveFontPath(m_notoRelativePath, "NotoSymbols2");
                var dysPath = ResolveFontPath(m_dysRelativePath, "OpenDyslexic");

                unsafe
                {
                    var ranges = new ushort[]
                    {
                        0x0020, 0x00FF,
                        0x2000, 0x206F,
                        0x25A0, 0x25FF,
                        0
                    };

                    fixed (ushort* pRanges = ranges)
                    {
                        var pixelSize = m_fontSize;
                        
                        if (!string.IsNullOrEmpty(defaultFontPath) && File.Exists(defaultFontPath))
                        {
                            var fDefault = io.Fonts.AddFontFromFileTTF(
                                defaultFontPath,
                                pixelSize,
                                null,
                                (IntPtr)pRanges
                            );
                            FontRegistry.m_defaultFont = fDefault;
                        }
                        
                        if (!string.IsNullOrEmpty(notoPath) && File.Exists(notoPath))
                        {
                            var fNoto = io.Fonts.AddFontFromFileTTF(
                                notoPath,
                                pixelSize,
                                null,
                                (IntPtr)pRanges
                            );
                            FontRegistry.m_notoFont = fNoto;
                        }

                        if (!string.IsNullOrEmpty(dysPath) && File.Exists(dysPath))
                        {
                            var fDys = io.Fonts.AddFontFromFileTTF(
                                dysPath,
                                pixelSize,
                                null,
                                (IntPtr)pRanges
                            );
                            FontRegistry.m_openDysFont = fDys;
                        }
                    }
                }
            }

        #endregion

        #region Helper

            private static string ResolveFontPath(string relativeUnityPath, string label)
            {
                if (string.IsNullOrEmpty(relativeUnityPath))
                {
                    Debug.LogWarning($"[FontInitializer] Relative path is empty for font '{label}'.");
                    return null;
                }

                var fileName = Path.GetFileName(relativeUnityPath);

                var fromData = Path.Combine(SettingsPath.FontsFolder, fileName);
                if (File.Exists(fromData))
                {
                    Debug.Log($"[FontInitializer] Using external font for '{label}' from '{fromData}'");
                    return fromData;
                }

                var fromAssets = Path.Combine(Application.dataPath, relativeUnityPath);
                if (File.Exists(fromAssets))
                {
                    Debug.Log($"[FontInitializer] Using project font for '{label}' from '{fromAssets}'");
                    return fromAssets;
                }

                Debug.LogWarning(
                    $"[FontInitializer] Font '{label}' not found.\n" +
                    $"  Tried: '{fromData}'\n" +
                    $"  Tried: '{fromAssets}'"
                );
                return null;
            }

        #endregion
    }
}