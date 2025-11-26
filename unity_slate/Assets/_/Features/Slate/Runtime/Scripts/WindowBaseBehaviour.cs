using System;
using UnityEngine;
using Foundation.Runtime;
using ImGuiNET;
using Manager.Runtime;
using UImGui;

namespace Slate.Runtime
{
    public abstract class WindowBaseBehaviour : FBehaviour
    {
        #region Public

        public  string WindowName {get => _windowName; set => _windowName = value;}

        #endregion

        #region Unity API

        protected void Start()
        {
            if (string.IsNullOrEmpty(_windowName))
            {
                Warning($"Window name is not set! Setting it to {gameObject.name}");
                _windowName = gameObject.name;
            }
        }

        protected virtual void OnEnable()
        {
            UImGuiUtility.OnInitialize += OnInitialize;
            UImGuiUtility.OnDeinitialize += OnDeinitialize;
            UImGuiUtility.Layout += OnLayout;
        }
        protected virtual void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
            UImGuiUtility.OnDeinitialize -= OnDeinitialize;
            UImGuiUtility.OnInitialize -= OnInitialize;
        }

        protected virtual void LateUpdate()
        {
            WindowPosManager.EndOfFrame();
        }
        
        protected virtual void OnDestroy()
        {
            WindowPosManager.UnregisterWindow(_windowName);
        }

        #endregion

        #region Main Methods

        protected virtual void OnInitialize(UImGui.UImGui imgui) { }
        protected virtual void OnDeinitialize(UImGui.UImGui imgui) { }

        private void OnLayout(UImGui.UImGui imgui)
        {
            if (string.IsNullOrEmpty(_windowName))
            {
                Error("Window name is not set! Give window a name in inspector.");
                return;
            }
            WindowPosManager.RegisterWindow(_windowName);
            
            if (_focusWindow)
            {
                FocusWindow();
            }

            // visibility test before Begin
            // if false, fully skip window rendering
            if (!WindowPosManager.ShouldDraw(_windowName) || !WindowPosManager.GetVisibilityOverride(_windowName))
                return;
            
            if (ImGui.Begin(_windowName,_windowFlags))
            {
                // Initialize cache from actual window position the first frame it appears,
                // so that we can apply delta to it without snapping.
                if (ImGui.IsWindowAppearing())
                {
                    WindowPosManager.UpdateWindowCache(_windowName);
                }
                
                // Apply pending delta (if any)
                WindowPosManager.SyncWindowPosition(_windowName);
                
                WindowLayout();
                
                Info($"New position: {ImGui.GetWindowPos()} || Camera pos: {Camera.main.transform.position}");
            }
            ImGui.End();
        }
   
        #endregion

        #region Utils

        protected abstract void WindowLayout();

        protected void DrawWindow(bool shouldDraw)
        {
            if (string.IsNullOrEmpty(_windowName) || _windowName is null)
            {
                throw new Exception("Window name is not set!");
            }
            WindowPosManager.SetVisibilityOverride(_windowName, shouldDraw);
        }

        public void FocusWindow()
        {
            WindowPosManager.FocusWindow(_windowName);
            _focusWindow = false;
        }

        #endregion

        #region Private & Protected

        [SerializeField]
        protected string _windowName;
        [SerializeField]
        protected ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.None;

        [SerializeField] protected bool _focusWindow = false;

        private bool _wasDraggedLastFrame;

        #endregion
    }
}