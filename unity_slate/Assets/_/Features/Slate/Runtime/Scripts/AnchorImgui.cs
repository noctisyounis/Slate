
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
            string windowName = "Anchored Window";

            WindowPosManager.RegisterWindow(windowName);
            
            // visibility test before Begin
            if (!WindowPosManager.ShouldDraw(windowName))
            {
                ImGui.PopStyleVar(4);
                return;
                // fully skip window rendering
            }

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
                
                Info($"New position: {ImGui.GetWindowPos()} || Camera pos: {Camera.main.transform.position}");
            }
            ImGui.End();
        }
   
        #endregion

        #region Private & Protected

        private UImGui.UImGui _UImGuiInstance;
        private Vector2 _anchor;

        #endregion
    }
}