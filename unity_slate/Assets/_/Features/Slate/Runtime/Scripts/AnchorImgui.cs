
using ImGuiNET;

namespace Slate.Runtime
{
    public class AnchorImgui : WindowBaseBehaviour
    {
        
        #region Unity API

        private void Start()
        {
            WindowName = "Anchored Window Override";
        }

        #endregion

        #region Main Methods

        protected override void WindowLayout()
        {
            ImGui.Text(_windowName + " content");

            ImGui.Text($"Anchor: {ImGui.GetWindowPos()}");
            ImGui.Text($"Is being dragged: {ImGui.IsMouseDragging(ImGuiMouseButton.Left)}");
        }

        #endregion

        #region Private & Protected

        #endregion
    }
}