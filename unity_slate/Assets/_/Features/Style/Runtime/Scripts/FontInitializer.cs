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
            public string m_notoRelativePath = "_/Content/Fonts/NotoSansSymbols2-Regular.ttf";
            public string m_dysRelativePath  = "_/Content/Fonts/OpenDyslexic3-Regular.ttf";

            [Tooltip("Taille de la font ImGui")]
            public float m_fontSize = 16f;

        #endregion

        #region Unity
        
            public void AddToolbarFont(ImGuiIOPtr io)
            {
                var notoPath = Path.Combine(Application.dataPath, m_notoRelativePath);
                var dysPath  = Path.Combine(Application.dataPath, m_dysRelativePath);

                unsafe
                {
                    if ((IntPtr)FontRegistry.m_defaultFont.NativePtr == IntPtr.Zero &&
                        io.Fonts.Fonts.Size > 0)
                    {
                        FontRegistry.m_defaultFont = io.Fonts.Fonts[0];
                    }
                    
                    var ranges = new ushort[]
                    {
                        0x0020, 0x00FF,
                        0x2000, 0x206F,
                        0x25A0, 0x25FF,
                        0
                    };

                    fixed (ushort* pRanges = ranges)
                    {
                        if (File.Exists(notoPath))
                        {
                            var fNoto = io.Fonts.AddFontFromFileTTF(
                                notoPath,
                                m_fontSize,
                                null,
                                (System.IntPtr)pRanges
                            );
                            
                            FontRegistry.m_notoFont = fNoto;
                        }

                        if (File.Exists(dysPath))
                        {
                            var fDys = io.Fonts.AddFontFromFileTTF(
                                dysPath,
                                m_fontSize,
                                null,
                                (System.IntPtr)pRanges
                            );
                            
                            FontRegistry.m_openDysFont = fDys;
                        }
                    }
                }
            }

        #endregion
    }
}