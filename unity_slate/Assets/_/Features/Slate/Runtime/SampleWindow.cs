using ImGuiNET;
using UImGui;
using UnityEngine;

namespace Slate.Runtime
{
    public class SampleWindow : MonoBehaviour
    {
        public Camera m_camera;
        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;
        
        private void OnLayout(UImGui.UImGui uImGui)
        {
           
            // ImGui.ShowDemoWindow();
        }
        
        
    }
}
