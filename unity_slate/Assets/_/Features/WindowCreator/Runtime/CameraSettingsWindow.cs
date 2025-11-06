using ImGuiNET;
using UnityEngine;

namespace WindowCreator.Runtime
{
    public class CameraSettingsWindow : IRuntimeWindow
    {
        private CameraPanSettings _settings;

        public CameraSettingsWindow(CameraPanSettings settings)
        {
            _settings = settings;
        }

        public void Draw()
        {
            if (_settings == null)
            {
                ImGui.Text("Aucun CameraPanSettings assigné.");
                return;
            }
            
            ImGui.Text("=== Camera Pan Settings ===");
            
            // Section Pan
            ImGui.Text("Pan Settings");
            ImGui.SliderFloat("Pan Speed min", ref _settings.m_panSpeedmin, 0, 50);
            ImGui.SliderFloat("Pan Speed max", ref _settings.m_panSpeedmax, 0, 50);
            
            // Section Mouse Pan
            ImGui.Text("Mouse Pan Settings");
            ImGui.SliderFloat("Mouse Pan Speed min", ref _settings.m_mousePanSpeedmin, 0, 50);
            ImGui.SliderFloat("Mouse Pan Speed max", ref _settings.m_mousePanSpeedmax, 0, 50);
            
            // Section Zoom
            ImGui.Text("Zoom Settings");
            ImGui.SliderFloat("Zoom Speed", ref _settings.m_zoomSpeed, 1, 200);
            
            // Section Zoom Limits
            ImGui.Text("Zoom Limits");
            ImGui.SliderFloat("Min Ortho Zoom", ref _settings.m_minOrthoZoom, 0.1f, 20f);
            ImGui.SliderFloat("Max Ortho Zoom", ref _settings.m_maxOrthoZoom, 10f, 100f);
            ImGui.SliderFloat("Correction", ref _settings.m_correcZoom, 0f, 1f);
            
            // On marque l'objet comme modifié pour l'inspector Unity
#if  UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_settings);
            
#endif
        }
    }

    public interface IRuntimeWindow
    {
    }
}
