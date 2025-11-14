using System.Globalization;
using ImGuiNET;
using UnityEngine;

namespace WindowCreator.Runtime
{
    public static class FieldRenderer
    {
        public static void DrawField(DataModels.LayoutZone zone, string idSuffix)
        {
            if (zone == null) return;

            switch (zone.TypeEnum)
            {
                case LayoutType.LayoutValueType.Bool:
                    DrawBool(zone, idSuffix);
                    break;
                case LayoutType.LayoutValueType.Int:
                    DrawInt(zone, idSuffix);
                    break;
                case LayoutType.LayoutValueType.Float:
                    DrawFloat(zone, idSuffix);
                    break;
                case LayoutType.LayoutValueType.Slider:
                    DrawSlider(zone, idSuffix);
                    break;
                case LayoutType.LayoutValueType.String:
                default:
                    DrawString(zone, idSuffix);
                    break;
                    
            }
        }

        private static void DrawBool(DataModels.LayoutZone zone, string id)
        {
            bool v = false;
            TryParseBool(zone.Value, out v);
            if (ImGui.Checkbox($"{zone.Key}##{id}", ref v))
                zone.Value = v ? "true" : "false";
        }

        private static void DrawInt(DataModels.LayoutZone zone, string id)
        {
            int v = 0;
            int.TryParse(zone.Value, out v);
            if (ImGui.InputInt($"{zone.Key}##{id}", ref v))
                zone.Value = v.ToString();
        }

        private static void DrawFloat(DataModels.LayoutZone zone, string id)
        {
            float v = 0;
            float.TryParse(zone.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
            if (ImGui.InputFloat($"{zone.Key}##{id}", ref v))
                zone.Value = v.ToString(CultureInfo.InvariantCulture);
        }

        private static void DrawString(DataModels.LayoutZone zone, string id)
        {
            string tmp = zone.Value ?? string.Empty;
            if (ImGui.InputText($"{zone.Key}##{id}", ref tmp, 512))
                zone.Value = tmp;
        }

        private static bool TryParseBool(string s, out bool result)
        {
            if (bool.TryParse(s, out result)) return true;
            if (int.TryParse(s, out var i))
            {
                result = i != 0; return true;
            }
            result = false;
            return false;
        }

        private static void DrawSlider(DataModels.LayoutZone zone, string id)
        {
            float value = 0f;
            float.TryParse(zone.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

            // Afficher et éditer Min et Max
            ImGui.PushID(id + "_slider_range");
            float min = zone.SliderMin;
            float max = zone.SliderMax;

            ImGui.Text("Min:");
            ImGui.SameLine();
            if (ImGui.InputFloat("##min", ref min, 0.1f, 1f, "%.2f"))
                zone.SliderMin = min;

            ImGui.Text("Max:");
            ImGui.SameLine();
            if (ImGui.InputFloat("##max", ref max, 0.1f, 1f, "%.2f"))
                zone.SliderMax = max;

            ImGui.PopID();

            // Clamp value pour rester entre min et max
            value = Mathf.Clamp(value, zone.SliderMin, zone.SliderMax);

            // Slider principal
            if (ImGui.SliderFloat($"{zone.Key}##{id}", ref value, zone.SliderMin, zone.SliderMax))
                zone.Value = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
