using System;
using UnityEngine;
using ImGuiNET;
using Foundation.Runtime;

namespace UpperBar.Runtime
{
    [DisallowMultipleComponent]
    public class ToolbarMenu : FBehaviour
    {
        #region Header

            [Header("Menus")]
            public string m_menuOneLabel = "Fichier";
            public string m_menuTwoLabel = "Affichage";

            [Header("UI / Menus")]
            public float m_menuWidth = 110f;
            public float m_popupMaxWidth = 320f;
            public float m_popupMaxHeight = 280f;
            public float m_itemSpacing = 10f;

        #endregion

        #region Public

            public bool  m_isAnyMenuOpen { get; private set; }
            public float m_totalMenusWidth { get; private set; }

        #endregion

        #region Unity

            private void Awake()
            {
                _menuOneItems ??= new (string, Action)[]
                {
                    ("Nouveau", () => Debug.Log("[Toolbar] Fichier/Nouveau")),
                    ("Ouvrir…",  () => Debug.Log("[Toolbar] Fichier/Ouvrir")),
                    ("Enregistrer", () => Debug.Log("[Toolbar] Fichier/Enregistrer")),
                    ("—", null),
                    ("Quitter", () => Application.Quit()),
                };
                _menuTwoItems ??= new (string, Action)[]
                {
                    ("Zoom 100%", () => Debug.Log("[Toolbar] Affichage/Zoom 100%")),
                    ("Zoom +",    () => Debug.Log("[Toolbar] Affichage/Zoom +")),
                    ("Zoom −",    () => Debug.Log("[Toolbar] Affichage/Zoom −")),
                };
            }

        #endregion
        
        #region Rendering
        
            public void DrawMenusLeft()
            {
                m_isAnyMenuOpen = false;
                m_totalMenusWidth = 0f;

                var first = true;
                if (_menuOneItems is { Length: > 0 })
                {
                    var opened = DrawDropdownFixed(m_menuOneLabel, _menuOneItems, m_menuWidth, m_popupMaxWidth, m_popupMaxHeight);
                    m_isAnyMenuOpen |= opened;
                    m_totalMenusWidth += m_menuWidth;
                    first = false;
                }
                if (_menuTwoItems is { Length: > 0 })
                {
                    if (!first) ImGui.SameLine(0f, m_itemSpacing);
                    var opened = DrawDropdownFixed(m_menuTwoLabel, _menuTwoItems, m_menuWidth, m_popupMaxWidth, m_popupMaxHeight);
                    m_isAnyMenuOpen |= opened;
                    m_totalMenusWidth += m_menuWidth;
                }

                var count = 0;
                if (_menuOneItems is { Length: > 0 }) count++;
                if (_menuTwoItems is { Length: > 0 }) count++;
                if (count > 1) m_totalMenusWidth += (count - 1) * m_itemSpacing;
            }
            
        #endregion

        #region Helpers

            private static string EllipseToFit(string s, float maxPx)
            {
                var style = ImGui.GetStyle();
                var arrow = ImGui.GetFrameHeight();
                var padding = style.FramePadding.x * 2f;
                var budget = Mathf.Max(0f, maxPx - (padding + arrow));
                if (ImGui.CalcTextSize(s).x <= budget) return s;
                const string ell = "…";
                int lo = 0, hi = s.Length;
                while (lo < hi)
                {
                    var mid = (lo + hi + 1) >> 1;
                    var probe = s.Substring(0, mid) + ell;
                    if (ImGui.CalcTextSize(probe).x <= budget) lo = mid; else hi = mid - 1;
                }
                return (lo <= 0) ? ell : s.Substring(0, lo) + ell;
            }

            private static bool DrawDropdownFixed(string label, (string, Action)[] items,
                                          float fixedWidth, float popupMaxW, float popupMaxH)
            {
                var id = "##" + label;
                var preview = EllipseToFit(label, fixedWidth);

                ImGui.SetNextItemWidth(fixedWidth);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(1, 1, 1, 0.08f));
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(1, 1, 1, 0.12f));

                ImGui.SetNextWindowSizeConstraints(new Vector2(100f, 0f), new Vector2(popupMaxW, popupMaxH));

                var opened = false;
                if (ImGui.BeginCombo(id, preview, ImGuiComboFlags.None))
                {
                    opened = true;
                    foreach (var it in items)
                    {
                        if (it.Item1 is "—" or "---" or "-") { ImGui.Separator(); continue; }
                        if (ImGui.Selectable(it.Item1)) it.Item2?.Invoke();
                    }
                    ImGui.EndCombo();
                }

                ImGui.PopStyleColor(3);
                return opened;
            }
            
        #endregion
        
        

        #region Private

            private (string, Action)[] _menuOneItems;
            private (string, Action)[] _menuTwoItems;

        #endregion
    }
}