using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ImGuiNET;

namespace WindowCreator.Runtime
{
    /// <summary>
    /// LayoutType
    /// ----------
    /// Gère :
    /// - Les types utilisés dans les champs (int, float, enum, etc.)
    /// - Le rendu ImGui complet pour chaque type
    /// - Les sécurités (parsing, valeurs invalides, reset, etc.)
    /// - Les descriptions pour l’aide utilisateur
    ///
    /// 100% Générique → peut servir à tout type de jeu (RPG, TD, VN, etc.)
    /// </summary>
    public static class LayoutType
    {
        // =========================================================
        //  ENUM PRINCIPAL
        // =========================================================
        public enum LayoutValueType
        {
            Text,           // string simple
            Int,            // nombre entier
            Float,          // nombre flottant
            Bool,           // checkbox
            Slider,         // slider min/max
            TextArea,       // multi-ligne courte
            TextDocument,   // multi-ligne longue
            EnumCustom,     // enum définie par l’utilisateur (Window = Enums)
            Color,          // RGBA (Vector4)
            Vector2,
            Vector3,
            FilePath,       // chemin de fichier
            DatabaseRef     // référence interne à un record
        }

        // =========================================================
        //  AIDE / TOOLTIPS (SUPER UX)
        // =========================================================
        private static readonly Dictionary<LayoutValueType, string> _typeDescriptions =
            new Dictionary<LayoutValueType, string>
            {
                { LayoutValueType.Text, "Texte court (une seule ligne)." },
                { LayoutValueType.TextArea, "Texte multi-ligne simple (description courte)." },
                { LayoutValueType.TextDocument, "Texte long (dialogues, documentation, lore...)." },
                { LayoutValueType.Int, "Nombre entier (ex : 1, -5, 200)." },
                { LayoutValueType.Float, "Nombre décimal (ex : 0.5, 3.14)." },
                { LayoutValueType.Bool, "Valeur booléenne Vrai/Faux." },
                { LayoutValueType.Slider, "Curseur numérique entre Min et Max." },
                { LayoutValueType.EnumCustom, "Liste personnalisée créée dans une Window Enum." },
                { LayoutValueType.Color, "Sélecteur de couleur RGBA avec transparence." },
                { LayoutValueType.Vector2, "Coordonnées 2D (x,y)." },
                { LayoutValueType.Vector3, "Coordonnées 3D (x,y,z)." },
                { LayoutValueType.FilePath, "Chemin vers un fichier (image, JSON, prefab...)." },
                { LayoutValueType.DatabaseRef, "Référence vers un autre record de la database." }
            };

        // =========================================================
        //  STYLE COMBO (lisibilité améliorée)
        // =========================================================
        private static void PushComboStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new Vector4(0.18f, 0.18f, 0.26f, 1f));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.25f, 0.25f, 0.33f, 1f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.32f, 0.32f, 0.40f, 1f));
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.36f, 0.36f, 0.44f, 1f));
        }

        private static void PopComboStyle()
        {
            ImGui.PopStyleColor(4);
        }

        // =========================================================
        //  RENDU COMPLET D’UN CHAMP (type + valeur)
        // =========================================================
        public static void DrawField(DataModels.LayoutZone field)
        {
            field.m_key ??= "";
            field.m_type ??= LayoutValueType.Text.ToString();
            field.m_value ??= "";

            // ----- Sélecteur de type -----
            string[] typeNames = Enum.GetNames(typeof(LayoutValueType));
            int typeIndex = Array.IndexOf(typeNames, field.m_type);
            if (typeIndex < 0) typeIndex = 0;

            ImGui.SetNextItemWidth(160);
            PushComboStyle();
            if (ImGui.Combo("##FieldType", ref typeIndex, typeNames, typeNames.Length))
                field.m_type = typeNames[typeIndex];
            PopComboStyle();

            LayoutValueType typeEnum;
            if (!Enum.TryParse(field.m_type, out typeEnum))
                typeEnum = LayoutValueType.Text;

            // Tooltip automatique
            if (_typeDescriptions.TryGetValue(typeEnum, out string desc))
            {
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(desc);
            }

            // ----- Rendu du champ selon le type -----
            switch (typeEnum)
            {
                case LayoutValueType.Text:         DrawText(field); break;
                case LayoutValueType.Int:          DrawInt(field); break;
                case LayoutValueType.Float:        DrawFloat(field); break;
                case LayoutValueType.Bool:         DrawBool(field); break;
                case LayoutValueType.Slider:       DrawSlider(field); break;
                case LayoutValueType.TextArea:     DrawTextArea(field); break;
                case LayoutValueType.TextDocument: DrawTextDocument(field); break;
                case LayoutValueType.EnumCustom:   DrawEnumCustom(field); break;
                case LayoutValueType.Color:        DrawColor(field); break;
                case LayoutValueType.Vector2:      DrawVector2(field); break;
                case LayoutValueType.Vector3:      DrawVector3(field); break;
                case LayoutValueType.FilePath:     DrawFilePath(field); break;
                case LayoutValueType.DatabaseRef:  DrawDatabaseRef(field); break;
            }
        }

        // =========================================================
        //  TYPES SIMPLES
        // =========================================================
        private static void DrawText(DataModels.LayoutZone field)
        {
            ImGui.SetNextItemWidth(260);
            ImGui.InputText("##Str", ref field.m_value, 256);
        }

        private static void DrawInt(DataModels.LayoutZone field)
        {
            int v = SafeInt(field.m_value);
            ImGui.SetNextItemWidth(120);
            ImGui.InputInt("##Int", ref v);
            field.m_value = v.ToString();
        }

        private static void DrawFloat(DataModels.LayoutZone field)
        {
            float f = SafeFloat(field.m_value);
            ImGui.SetNextItemWidth(120);
            ImGui.InputFloat("##Float", ref f);
            field.m_value = f.ToString();
        }

        private static void DrawBool(DataModels.LayoutZone field)
        {
            bool v = SafeBool(field.m_value);
            ImGui.Checkbox("##Bool", ref v);
            field.m_value = v ? "true" : "false";
        }

        // =========================================================
        //  SLIDER (avec sécurités)
        // =========================================================
        private static void DrawSlider(DataModels.LayoutZone field)
        {
            float val = SafeFloat(field.m_value);
            float min = field.m_sliderMin;
            float max = field.m_sliderMax;

            ImGui.Text("Min");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputFloat("##SMin", ref min);

            ImGui.Text("Max");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputFloat("##SMax", ref max);

            // sécurité min < max
            if (max <= min)
                max = min + 0.001f;

            field.m_sliderMin = min;
            field.m_sliderMax = max;

            ImGui.SetNextItemWidth(220);
            ImGui.SliderFloat("##SVal", ref val, min, max);

            field.m_value = val.ToString();
        }

        // =========================================================
        //  TEXT MULTILIGNE
        // =========================================================
        private static void DrawTextArea(DataModels.LayoutZone field)
        {
            ImGui.InputTextMultiline("##TA", ref field.m_value, 2000, new Vector2(300, 80));
        }

        private static void DrawTextDocument(DataModels.LayoutZone field)
        {
            ImGui.InputTextMultiline("##TD", ref field.m_value, 8000, new Vector2(300, 140));
        }

        // =========================================================
        //  ENUM CUSTOM (Window/Record)
        // =========================================================
        private static void DrawEnumCustom(DataModels.LayoutZone field)
        {
            field.m_key ??= "/";
            field.m_value ??= "";

            // m_key = "Window/Record"
            var split = field.m_key.Split('/');
            string enumWindow = split.Length > 0 ? split[0] : "";
            string enumRecord = split.Length > 1 ? split[1] : "";

            var manager = ImGuiLayoutManager.Instance();
            if (manager == null)
            {
                ImGui.TextDisabled("No LayoutManager.");
                return;
            }

            // ---- 1. Windows Enum ----
            var windows = manager.GetEnumWindowTitles();
            if (windows == null || windows.Count == 0)
            {
                ImGui.TextDisabled("No Enum Window.");
                return;
            }

            int winIndex = Math.Max(0, windows.IndexOf(enumWindow));
            ImGui.Text("Enum Window:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            PushComboStyle();
            if (ImGui.Combo("##ECW", ref winIndex, windows.ToArray(), windows.Count))
            {
                enumWindow = windows[winIndex];
                enumRecord = "";
                field.m_value = "";
            }
            PopComboStyle();

            // ---- 2. Records ----
            var records = manager.GetEnumRecords(enumWindow);
            if (records == null || records.Count == 0)
            {
                ImGui.TextDisabled("No records in this window.");
                return;
            }

            var recNames = records.Select(r => r.m_name).ToList();
            int recIndex = Math.Max(0, recNames.IndexOf(enumRecord));

            ImGui.Text("Enum Name:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            PushComboStyle();
            if (ImGui.Combo("##ECR", ref recIndex, recNames.ToArray(), recNames.Count))
            {
                enumRecord = recNames[recIndex];
                field.m_value = "";
            }
            PopComboStyle();

            field.m_key = $"{enumWindow}/{enumRecord}";

            // ---- 3. Options ----
            var options = manager.GetEnumOptions(enumWindow, enumRecord);
            if (options == null || options.Count == 0)
            {
                ImGui.TextDisabled("No options.");
                return;
            }

            int valIndex = Math.Max(0, options.IndexOf(field.m_value));

            ImGui.Text("Value:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(220);
            PushComboStyle();
            if (ImGui.Combo("##ECV", ref valIndex, options.ToArray(), options.Count))
                field.m_value = options[valIndex];
            PopComboStyle();
        }

        // =========================================================
        //  AUTRES TYPES
        // =========================================================
        private static void DrawColor(DataModels.LayoutZone field)
        {
            Vector4 color = SafeColor(field.m_value);
            if (ImGui.ColorEdit4("##Col", ref color))
                field.m_value = $"{color.x},{color.y},{color.z},{color.w}";
        }

        private static void DrawVector2(DataModels.LayoutZone field)
        {
            Vector2 v = SafeVector2(field.m_value);
            if (ImGui.InputFloat2("##V2", ref v))
                field.m_value = $"{v.x},{v.y}";
        }

        private static void DrawVector3(DataModels.LayoutZone field)
        {
            Vector3 v = SafeVector3(field.m_value);
            if (ImGui.InputFloat3("##V3", ref v))
                field.m_value = $"{v.x},{v.y},{v.z}";
        }
        
        private static void DrawFilePath(DataModels.LayoutZone field)
        {
            ImGui.SetNextItemWidth(260);
            ImGui.InputText("##FP", ref field.m_value, 256);
        }

        private static void DrawDatabaseRef(DataModels.LayoutZone field)
        {
            ImGui.SetNextItemWidth(220);
            ImGui.InputText("##DB", ref field.m_value, 128);
        }

        // =========================================================
        //  PARSING SÉCURISÉ (ANTI-CRASH)
        // =========================================================
        private static int SafeInt(string s) => int.TryParse(s, out int r) ? r : 0;
        private static float SafeFloat(string s) => float.TryParse(s, out float r) ? r : 0f;

        private static bool SafeBool(string s) => s == "true";

        private static Vector2 SafeVector2(string s)
        {
            var sp = s.Split(',');
            if (sp.Length != 2) return Vector2.zero;
            return new Vector2(SafeFloat(sp[0]), SafeFloat(sp[1]));
        }

        private static Vector3 SafeVector3(string s)
        {
            var sp = s.Split(',');
            if (sp.Length != 3) return Vector3.zero;
            return new Vector3(SafeFloat(sp[0]), SafeFloat(sp[1]), SafeFloat(sp[2]));
        }

        private static Vector4 SafeColor(string s)
        {
            var sp = s.Split(',');
            if (sp.Length != 4) return new Vector4(1, 1, 1, 1);
            return new Vector4(SafeFloat(sp[0]), SafeFloat(sp[1]), SafeFloat(sp[2]), SafeFloat(sp[3]));
        }
    }
}
