using UImGui;
using UnityEngine;

namespace Slate.Runtime
{
    public class SampleWindow : MonoBehaviour
    {
        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;

        private void OnLayout(UImGui.UImGui uImGui)
        {
            
        }
    }
}
