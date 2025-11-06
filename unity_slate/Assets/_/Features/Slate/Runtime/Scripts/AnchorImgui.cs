
using System;
using UnityEngine;
using Foundation.Runtime;
using ImGuiNET;
using Manager.Runtime;
using UImGui;

namespace Slate.Runtime
{
    public class AnchorImgui : FBehaviour
    {
        
        #region Unity API

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnLayout;
            UImGuiUtility.OnDeinitialize += OnDeinitialize;
            UImGuiUtility.OnInitialize += OnInitialize;
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
            UImGuiUtility.OnDeinitialize -= OnDeinitialize;
            UImGuiUtility.OnInitialize -= OnInitialize;
        }

        private void LateUpdate()
        {
            WindowPosManager.EndOfFrame();
        }

        #endregion

        #region Main Methods

        private void OnInitialize(UImGui.UImGui obj) { }
        private void OnDeinitialize(UImGui.UImGui obj) { }
        private void OnLayout(UImGui.UImGui obj)
        {
            // window style to avoid window clamping
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.zero);
            
            string windowName = "Anchored Window";

            WindowPosManager.RegisterWindow(windowName);

            if (ImGui.Begin(windowName, ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoResize))
            {
                // Initialize cache from actual window position the first frame it appears,
                // so that we can apply delta to it without snapping.
                if (ImGui.IsWindowAppearing())
                {
                    WindowPosManager.UpdateWindowCache(windowName);
                }
                
                // Apply pending delta (if any)
                WindowPosManager.SyncWindowPosition(windowName);
                ImGui.Text(windowName + " content");
            }
            ImGui.End();
            ImGui.PopStyleVar(4);
        }
   
        #endregion

        #region Private & Protected

        private UImGui.UImGui _UImGuiInstance;
        private Vector2 _anchor;

        #endregion
    }
}