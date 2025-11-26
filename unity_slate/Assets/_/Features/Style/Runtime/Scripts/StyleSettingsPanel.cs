using ImGuiNET;
using SharedData.Runtime;

namespace Style.Runtime
{
    public class StyleSettingsPanel
    {
        public void Draw()
        {
            var style = ImGui.GetStyle();
            
            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.55f);

            var scrollbarRounding = style.ScrollbarRounding;

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
            if (ImGui.SliderFloat("WindowBorderSize", ref windowBorderSize, 0f, 3f, "%.0f"))
                style.WindowBorderSize = windowBorderSize;

            var childBorderSize = style.ChildBorderSize;
            if (ImGui.SliderFloat("ChildBorderSize", ref childBorderSize, 0f, 3f, "%.0f"))
                style.ChildBorderSize = childBorderSize;

            var popupBorderSize = style.PopupBorderSize;
            if (ImGui.SliderFloat("PopupBorderSize", ref popupBorderSize, 0f, 3f, "%.0f"))
                style.PopupBorderSize = popupBorderSize;

            var frameBorderSize = style.FrameBorderSize;
            if (ImGui.SliderFloat("FrameBorderSize", ref frameBorderSize, 0f, 3f, "%.0f"))
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

            var scrollbarSize = style.ScrollbarSize;
            if (ImGui.SliderFloat("ScrollbarSize", ref scrollbarSize, 1f, 30f, "%.0f"))
                style.ScrollbarSize = scrollbarSize;

            if (ImGui.SliderFloat("ScrollbarRounding", ref scrollbarRounding, 0f, 12f, "%.0f"))
                style.ScrollbarRounding = scrollbarRounding;

            ImGui.SeparatorText("Tabs");

            var tabBorderSize = style.TabBorderSize;
            if (ImGui.SliderFloat("TabBorderSize", ref tabBorderSize, 0f, 3f, "%.0f"))
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

            ImGui.PopItemWidth();

            ImGui.Spacing();
            ImGui.Separator();

            if (ImGui.Button("Save sizes##StyleSizes"))
            {
                StyleRegistry.SaveFromImGui();
                PresetManagerPanel.ExportSizes();
            }
        }
    }
}