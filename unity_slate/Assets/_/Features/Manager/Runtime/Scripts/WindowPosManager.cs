using System.Collections.Generic;
using ImGuiNET;
using UnityEngine;

namespace Manager.Runtime
{
    public static class WindowPosManager
    {
        #region Public

   
        #endregion

        #region Main Methods

        public static void RegisterWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (_registeredWindows.Contains(windowName)) return;
            
            _registeredWindows.Add(windowName);

            _windowDatas[windowName] = new WindowData(windowName)
            {
                InitialPos = null,
                Offset = Vector2.zero,
                Size = null,
                Visible = true,
                IsBeingDragged = false,
                IsSizeInitialized = false,  // Track if size has been captured
                VisibilityOverride = true,

            };
        }
        public static void RegisterWindow(string windowName, Vector2 initialPos)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (_registeredWindows.Contains(windowName)) return;
            
            _registeredWindows.Add(windowName);

            _windowDatas[windowName] = new WindowData(windowName)
            {
                InitialPos = initialPos,
                Offset = Vector2.zero,
                Size = null,
                Visible = true,
                IsBeingDragged = false,
                IsSizeInitialized = false,  // Track if size has been captured
                VisibilityOverride = true,

            };
        }

        public static void UnregisterWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            
            _registeredWindows.Remove(windowName);
            _windowDatas.Remove(windowName);
        }

        /// <summary>
        /// Call this once per frame when camera/world pans. Applies a screen-space delta to all cached window positions.
        /// Does NOT directly call SetWindowPos; that is done per-window in SyncWindowPosition when it is current.
        /// </summary>
        public static void MoveWindows(Vector2 screenDelta)
        {
            if (screenDelta == Vector2.zero) return;

            _hasPendingDeltaThisFrame = true;
            
            for (int i = 0; i < _registeredWindows.Count; i++)
            {
                string currentWindow = _registeredWindows[i];
            
                Vector2 oldPosOffset;
                
                WindowData data = _windowDatas[currentWindow];
                data.HasPendingDelta = true;
                oldPosOffset = data.Offset ?? Vector2.zero;
                
                Vector2 newPosOffset = oldPosOffset + screenDelta;
                data.Offset = newPosOffset;
                
            }
        }

        public static void ResizeWindows(float scaleFactor)
        {
            
            _isProgrammaticResize = true;
            
            for (int i = 0; i < _registeredWindows.Count; i++)
            {
                string currentWindow = _registeredWindows[i];
                InitializeWindowCaches(currentWindow);
                
                WindowData data = _windowDatas[currentWindow];// ONLY use the stored base size if it has been initialized
                // If not initialized yet, skip this window
                if (!data.IsSizeInitialized || data.Size == null)
                {
                    Debug.LogWarning($"[RESIZE] Window {currentWindow} size not initialized yet, skipping resize");
                    continue;
                }

                Vector2 oldSize = (Vector2)data.Size;
                var newSize = oldSize * scaleFactor;
                
                // Debug.Log($"New window size: {size}->{newSize} | ScaleFactor: {scaleFactor}");
                
                ImGui.SetWindowSize(data.WindowName, newSize, ImGuiCond.Always);
            }
        }

        /// <summary>
        /// Call this inside the window's Begin(...) block each frame to:
        /// - Initialize cache from the actual ImGui position once (first time we see the window)
        /// - Apply pending delta (SetWindowPos with ImGuiCond.Always) only during pan frames
        /// - Skip rendering when the window would be outside the camera view after offset
        /// </summary>
        public static void SyncWindowPosition(string windowName)
        {
            if (!CheckRegisteredWindow(windowName)) return;
            
            bool isWindowHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows);
            bool mouseDragging = ImGui.IsMouseDragging(ImGuiMouseButton.Left);
            
            bool isMouseDown = ImGui.IsMouseDown(ImGuiMouseButton.Left);
            bool isMouseReleased = ImGui.IsMouseReleased(ImGuiMouseButton.Left);

            bool draggingThisWindow = isWindowHovered && mouseDragging;

            InitializeWindowCaches(windowName);

            WindowData data = _windowDatas[windowName];
            
            // Capture the initial size ONLY ONCE on first frame (before any scaling)
            if (!data.IsSizeInitialized)
            {
                var size = ImGui.GetWindowSize();
                data.Size = size;
                data.IsSizeInitialized = true;
                // Debug.Log($"[INIT] Captured base size for {windowName}: {data.Size}");
            }
            
            bool wasDraggedLast = data.IsBeingDragged ?? false;
            
            Vector2 currentSize = ImGui.GetWindowSize();
            Vector2 lastSize = (Vector2)data.Size;
            
            bool wasResized = isMouseDown && (currentSize != lastSize);

            if (wasResized)
            {
                _isResizing = true;
            }
            
            if (!draggingThisWindow && wasDraggedLast)
            {
                UpdateWindowCache(windowName);
            }

            if (_isResizing && isMouseReleased)
            {
                data.Size = currentSize;
            }
            data.IsBeingDragged = draggingThisWindow;
            
            // if the window needs to move
            if (data.HasPendingDelta)
            {
                var offset = data.Offset ?? Vector2.zero;
                var initialPos = data.InitialPos ?? Vector2.zero;
                var targetPos = initialPos + offset;
                
                ImGui.SetWindowPos(windowName, targetPos, ImGuiCond.Always);
                
                data.HasPendingDelta = false;
            }
            
        }

        /// <summary>
        /// Optional: call inside the window after Begin, when it appears for the first time,
        /// to ensure the cache matches the actual position if user moved it or it was set by SetNextWindowPos.
        /// </summary>
        public static void UpdateWindowCache(string windowName)
        {
            if (!CheckRegisteredWindow(windowName)) return;
           
            WindowData currentData = _windowDatas[windowName];
            var pos = ImGui.GetWindowPos();
            currentData.InitialPos = pos;
            currentData.Offset = Vector2.zero;
            
            // Add offset if dragged position is outside of screen bounds
            if (!ShouldDraw(windowName) || !currentData.VisibilityOverride)
            {
                var size = new Vector2(ImGui.GetWindowSize().x, ImGui.GetWindowSize().y);
                var sizeBounds = new Vector2(size.x - (size.x * .75f), size.y - (size.y * .75f));
                var newPosX = Mathf.Clamp(pos.x, sizeBounds.x, ImGui.GetIO().DisplaySize.x - sizeBounds.x);
                var newPosY = Mathf.Clamp(pos.y, sizeBounds.y, ImGui.GetIO().DisplaySize.y - sizeBounds.y);
                currentData.InitialPos = new Vector2(newPosX, newPosY);
            }
        }

        /// <summary>
        /// Should be called once per frame after all windows are drawn to clear the pan flag.
        /// Call it at the end of your UImGui layout pass.
        /// </summary>
        public static void EndOfFrame()
        {
            // _hasPendingDeltaThisFrame = false;
            // _isResizing = false;
            _isProgrammaticResize = false;
        }

        public static void SetVisibilityOverride(string windowName, bool visible)
        {
            if (!CheckRegisteredWindow(windowName)) return;
            
            WindowData currentData = _windowDatas[windowName];
            currentData.VisibilityOverride = visible;
        }

        public static bool GetVisibilityOverride(string windowName)
        {
            if (!CheckRegisteredWindow(windowName)) return false;
            
            WindowData currentData = _windowDatas[windowName];
            return currentData.VisibilityOverride;
        }

        public static void FocusWindow(string windowName)
        {
            if (!CheckRegisteredWindow(windowName)) return;
            
            WindowData currentWindow = _windowDatas[windowName];
            
            // Current screen position of focused window
            var currentWindowOffset = currentWindow.Offset ?? Vector2.zero;
            var initialPos = currentWindow.InitialPos ?? Vector2.zero;
            var currentScreenPos = initialPos + currentWindowOffset;
            
            // currentData.Offset = Vector2.zero;
            // currentData.HasPendingDelta = true;

            // Calculate screen center
            var screenCenter = ImGui.GetIO().DisplaySize * .5f;
            var windowSize = currentWindow.Size ?? Vector2.zero;
            var targetPos = screenCenter - (windowSize * .5f);
            
            // Calculate delta to move window to center
            var delta = targetPos - currentScreenPos;
            
            for (int i = 0; i < _registeredWindows.Count; i++)
            {
                var currentWindowName = _registeredWindows[i];
                // if (currentWindowName == windowName) continue;
                
                WindowData windowData = _windowDatas[currentWindowName];
                // otherData.Offset += currentWindowOffset;
                
                // This should be the same for all windows, so just use the first one
                // Todo: remove condition and apply += delta to all windows when button is implemented
                
                if (currentWindowName == windowName)
                {
                    windowData.Offset += delta;
                }
                else
                {
                    windowData.Offset -= delta;
                }
                windowData.HasPendingDelta = true;
                
                _windowDatas[currentWindowName] = windowData;
            }
            
            // _windowDatas[windowName] = currentData;
        }
        #endregion

        #region Utils
        
        private static bool CheckRegisteredWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return false;
            return _registeredWindows.Contains(windowName) && _windowDatas.ContainsKey(windowName);
        }
        
        private static void InitializeWindowCaches(string windowName)
        {
            WindowData currentData = _windowDatas[windowName];
            
            currentData.IsBeingDragged ??= false;
            currentData.Offset ??= Vector2.zero;
            currentData.InitialPos ??= ImGui.GetWindowSize();
        }

        /// <summary>
        /// Call inside the window's layout site BEFORE calling ImGui.Begin(windowName).
        /// Returns true if the window should be drawn this frame. If false, skip ImGui.Begin/End entirely.
        /// This avoids any rendering and input for windows that are fully outside the camera view.
        /// </summary>
        public static bool ShouldDraw(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return false;
            // if not registered, always draw
            if (!CheckRegisteredWindow(windowName)) return true;
            
            WindowData currentData = _windowDatas[windowName];
            
            currentData.InitialPos ??= Vector2.zero;
            
            InitializeWindowCaches(windowName);
            
            currentData = _windowDatas[windowName];

            Vector2 offset = currentData.Offset ?? Vector2.zero;
            Vector2 initialPos = (Vector2)currentData.InitialPos;
            Vector2 targetPos = initialPos + offset;
            Vector2 size = currentData.Size ?? new Vector2(1, 1);

            var io = ImGui.GetIO();
            // screen bounds (top-left origin for ImGui)
            float screenW = io.DisplaySize.x;
            float screenH = io.DisplaySize.y;
         
            
            bool outside = IsRectOutsideScreen(targetPos, size, screenW, screenH);
            
            currentData.Visible = !outside;
            return (bool)currentData.Visible;
        }
        
        private static bool IsRectOutsideScreen(Vector2 pos, Vector2 size, float screenW, float screenH)
        {
            var xSize = size.x - (size.x * .75f);
            var ySize = size.y - (size.y * .75f);
            
            // window rect
            float left = pos.x;
            float right = pos.x + size.x;
            float top = pos.y;
            float bottom = pos.y + ySize;

            //Debug.Log($"Window size: {size.x}:{size.y} | Rect data:  l:{left}, r:{right}, t:{top}, b:{bottom} | Screen size: w:{screenW}, h:{screenH}");
            return (right <= xSize || left + xSize >= screenW || bottom <= 0f || top >= screenH);
        }

        #endregion

        #region Private & Protected

        private static readonly List<string> _registeredWindows = new List<string>();
        
        private static readonly Dictionary<string, WindowData> _windowDatas = new Dictionary<string, WindowData>();

        private static bool _hasPendingDeltaThisFrame;
        private static CameraPanSettings _panSettings;

        private static bool _isProgrammaticResize;

        private static bool _isResizing;

        #endregion

        #region External

        private class WindowData
        {
            public WindowData(string windowName = null)
            {
                WindowName = windowName;
            }
            public string WindowName;
            public Vector2? InitialPos;
            public Vector2? Offset;
            public Vector2? Size;
            public bool Visible;
            public bool? IsBeingDragged;
            public bool IsSizeInitialized;
            public bool HasPendingDelta;
            public bool VisibilityOverride;
        }

        #endregion
    }
}