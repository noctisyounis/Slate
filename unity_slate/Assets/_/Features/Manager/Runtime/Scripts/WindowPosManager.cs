using System.Collections.Generic;
using ImGuiNET;
using JetBrains.Annotations;
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
            // _windowIsBeingDraggedCache[windowName] = false;

            _windowDatas[windowName] = new WindowData()
            {
                WindowName = windowName,
                InitialPos = null,
                Offset = Vector2.zero,
                Size = null,
                Visible = true,
                IsBeingDragged = false
            };
        }
        public static void RegisterWindow(string windowName, Vector2 initialPos)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (_registeredWindows.Contains(windowName)) return;
            //
            // _windowOffsetCache[windowName] = initialPos;
            // _windowInitialPosCache[windowName] = initialPos;
            _registeredWindows.Add(windowName);
            // _windowIsBeingDraggedCache[windowName] = false;

            _windowDatas[windowName] = new WindowData()
            {
                WindowName = windowName,
                InitialPos = initialPos,
                Offset = Vector2.zero,
                Size = null,
                Visible = true,
                IsBeingDragged = false
            };
        }

        public static void UnregisterWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            // if (!_registeredWindows.Contains(windowName)) return;
            //
            _registeredWindows.Remove(windowName);
            // _windowOffsetCache.Remove(windowName);
            // _windowInitialPosCache.Remove(windowName);
            // _windowSizeCache.Remove(windowName);
            // _windowVisibility.Remove(windowName);
            // _windowIsBeingDraggedCache.Remove(windowName);

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
                // if (_windowOffsetCache.TryGetValue(currentWindow, out oldPosOffset))
                // {
                //     Vector2 newPosOffset = oldPosOffset + screenDelta;
                //     // add new window pos to cache
                //     _windowOffsetCache[currentWindow] = newPosOffset;
                // }
                
                WindowData data = _windowDatas[currentWindow];
                oldPosOffset = data.Offset ?? Vector2.zero;
                
                Vector2 newPosOffset = oldPosOffset + screenDelta;
                data.Offset = newPosOffset;
                
                _windowDatas[currentWindow] = data;
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

            bool draggingThisWindow = isWindowHovered && mouseDragging;

            InitializeWindowCaches(windowName);

            WindowData data = _windowDatas[windowName];
            
            // bool wasDraggedLast = _windowIsBeingDraggedCache.GetValueOrDefault(windowName, false);
            bool wasDraggedLast = data.IsBeingDragged ?? false;

            if (!draggingThisWindow && wasDraggedLast)
            {
                UpdateWindowCache(windowName);
                data = _windowDatas[windowName];
            }
            // _windowIsBeingDraggedCache[windowName] = draggingThisWindow;
            data.IsBeingDragged = draggingThisWindow;
            _windowDatas[windowName] = data;
            
            // Use mouse release as a flag to update cache when dragging window
            // if(!mouseDragging && _wasDraggedLastFrame)
            // {
            //     UpdateWindowCache(windowName);
            // }
            // _wasDraggedLastFrame = mouseDragging;
            
            // then if the window needs to move
            if (_hasPendingDeltaThisFrame)
            {
                // var offset = _windowOffsetCache[windowName];
                var offset = data.Offset ?? Vector2.zero;
                // var pos = ImGui.GetWindowPos() + offset;
                // var targetPos = _windowInitialPosCache[windowName] + offset;
                var initialPos = data.InitialPos ?? Vector2.zero;
                var targetPos = initialPos + offset;
                ImGui.SetWindowPos(windowName, targetPos, ImGuiCond.Always);
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
            // _windowOffsetCache[windowName] = pos;
            // _windowInitialPosCache[windowName] = pos;
            currentData.InitialPos = pos;
            // _windowOffsetCache[windowName] = Vector2.zero;
            currentData.Offset = Vector2.zero;
            
            // Add offset if dragged position is outside of screen bounds
            if (!ShouldDraw(windowName))
            {
                var size = new Vector2(ImGui.GetWindowSize().x, ImGui.GetWindowSize().y);
                var sizeBounds = new Vector2(size.x - (size.x * .75f), size.y - (size.y * .75f));
                var newPosX = Mathf.Clamp(pos.x, sizeBounds.x, ImGui.GetIO().DisplaySize.x - sizeBounds.x);
                var newPosY = Mathf.Clamp(pos.y, sizeBounds.y, ImGui.GetIO().DisplaySize.y - sizeBounds.y);
                // _windowInitialPosCache[windowName] = new Vector2(newPosX, newPosY);
                currentData.InitialPos = new Vector2(newPosX, newPosY);
            }
            
            _windowDatas[windowName] = currentData;
        }

        /// <summary>
        /// Should be called once per frame after all windows are drawn to clear the pan flag.
        /// Call it at the end of your UImGui layout pass.
        /// </summary>
        public static void EndOfFrame()
        {
            _hasPendingDeltaThisFrame = false;
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
            // if (!_windowIsBeingDraggedCache.ContainsKey(windowName))
            // {
            //     _windowIsBeingDraggedCache[windowName] = false;
            // }
            //
            // if (!_windowOffsetCache.ContainsKey(windowName))
            // {
            //     _windowOffsetCache[windowName] = Vector2.zero;
            // }
            // if (!_windowInitialPosCache.ContainsKey(windowName))
            //     _windowInitialPosCache[windowName] = ImGui.GetWindowPos();
            //
            // if (!_windowSizeCache.ContainsKey(windowName))
            //     _windowSizeCache[windowName] = ImGui.GetWindowSize();
            
            WindowData currentData = _windowDatas[windowName];
            
            currentData.IsBeingDragged ??= false;
            currentData.Offset ??= Vector2.zero;
            currentData.InitialPos ??= ImGui.GetWindowSize();
            currentData.Size ??= ImGui.GetWindowSize();
            
            _windowDatas[windowName] = currentData;
            
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
            
            // // Ensure caches
            // if (!_windowOffsetCache.ContainsKey(windowName))
            //     _windowOffsetCache[windowName] = Vector2.zero;

            // if (!_windowInitialPosCache.ContainsKey(windowName))
            // {
            //     // We don't know the initial position yet (first frame before Begin).
            //     // Assume current imgui cursor pos to 0,0
            //     // real initial pos will be set in UpdateWindowCache right after Begin();
            //     _windowInitialPosCache[windowName] = Vector2.zero;
            // }
            
            currentData.InitialPos ??= Vector2.zero;
            _windowDatas[windowName] = currentData;
            
            InitializeWindowCaches(windowName);
            
            currentData = _windowDatas[windowName];
            // target rect (position, size)
            // Vector2 offset = _windowOffsetCache[windowName];
            // Vector2 initialPos = _windowInitialPosCache[windowName];

            Vector2 offset = currentData.Offset ?? Vector2.zero;
            Vector2 initialPos = (Vector2)currentData.InitialPos;
            
            // Vector2 currentPos = ImGui.GetWindowPos();
            Vector2 targetPos = initialPos + offset;
            // Vector2 size = _windowSizeCache.TryGetValue(windowName, out var lastSize) ? lastSize : new Vector2(1, 1);
            Vector2 size = currentData.Size ?? new Vector2(1, 1);

            var io = ImGui.GetIO();
            // screen bounds (top-left origin for ImGui)
            float screenW = io.DisplaySize.x;
            float screenH = io.DisplaySize.y;
         
            
            bool outside = IsRectOutsideScreen(targetPos, size, screenW, screenH);
            // _windowVisibility[windowName] = !outside;
            
            currentData.Visible = !outside;
            _windowDatas[windowName] = currentData;
            // return _windowVisibility[windowName];
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
        // // offset from baseline (camera/world pan)
        // private static readonly Dictionary<string, Vector2> _windowOffsetCache = new Dictionary<string, Vector2>();
        // // absolute position captured on first appearance
        // private static readonly Dictionary<string, Vector2> _windowInitialPosCache = new Dictionary<string, Vector2>();
        // // last known window size
        // private static readonly Dictionary<string, Vector2> _windowSizeCache = new Dictionary<string, Vector2>();
        // // stocked from last ShouldDraw call
        // private static readonly Dictionary<string, bool> _windowVisibility = new Dictionary<string, bool>();
        // private static readonly Dictionary<string, bool> _windowIsBeingDraggedCache = new Dictionary<string, bool>();
        
        private static readonly Dictionary<string, WindowData> _windowDatas = new Dictionary<string, WindowData>();

        private static bool _hasPendingDeltaThisFrame;
        // private static bool _wasDraggedLastFrame;

        #endregion

        #region External

        private struct WindowData
        {
            [CanBeNull] public string WindowName;
            public Vector2? InitialPos;
            public Vector2? Offset;
            public Vector2? Size;
            public bool? Visible;
            public bool? IsBeingDragged;
        }

        #endregion
    }
}