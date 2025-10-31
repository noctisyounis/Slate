using System;
using ImGuiNET;
using UImGui;
using UnityEngine;

namespace Slate.Runtime
{
    public class SampleWindow : MonoBehaviour
    {
        public Camera m_camera;

        private void Awake()
        {
            _cameraPan = new CameraPan(m_camera);
        }

        private void Update()
        {
            // Mise à jour de la caméra via le controller
            _cameraPan.UpdatePan();
        }

        private void OnDestroy()
        {
            // Désactiver le input system
            _cameraPan.Disable();
        }

        private void OnEnable() => UImGuiUtility.Layout += OnLayout;
        private void OnDisable() => UImGuiUtility.Layout -= OnLayout;
        
        private void OnLayout(UImGui.UImGui uImGui)
        {
           
            // ImGui.ShowDemoWindow();
        }
        
        private CameraPan _cameraPan;
    }
}
