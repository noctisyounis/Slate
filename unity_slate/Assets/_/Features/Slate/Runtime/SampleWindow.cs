using Foundation.Runtime;
using UImGui;

namespace Slate.Runtime
{
    public class SampleWindow : FBehaviour
    {
        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;

        private void OnLayout(UImGui.UImGui uImGui)
        {

        }
    }
}
