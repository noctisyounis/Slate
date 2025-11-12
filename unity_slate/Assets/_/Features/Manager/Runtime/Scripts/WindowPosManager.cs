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
        }
        public static void RegisterWindow(string windowName, Vector2 initialPos)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (_registeredWindows.Contains(windowName)) return;
            
            _windowOffsetCache[windowName] = initialPos;
            _windowInitialPosCache[windowName] = initialPos;
            _registeredWindows.Add(windowName);
        }

        public static void UnregisterWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (!_registeredWindows.Contains(windowName)) return;
            
            _registeredWindows.Remove(windowName);
            _windowOffsetCache.Remove(windowName);
            _windowInitialPosCache.Remove(windowName);
            _windowSizeCache.Remove(windowName);
            _windowVisibility.Remove(windowName);
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
                if (_windowOffsetCache.TryGetValue(currentWindow, out oldPosOffset))
                {
                    Vector2 newPosOffset = oldPosOffset + screenDelta;
                    // add new window pos to cache
                    _windowOffsetCache[currentWindow] = newPosOffset;
                }
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
            if (string.IsNullOrEmpty(windowName)) return;
            if (!_registeredWindows.Contains(windowName)) return;
            
            // If not in cache, grab true pos
            if (!_windowOffsetCache.ContainsKey(windowName))
            {
                // Vector2 current = ImGui.GetWindowPos();
                // _windowOffsetCache[windowName] = current;
                _windowOffsetCache[windowName] = Vector2.zero;
            }
            if (!_windowInitialPosCache.ContainsKey(windowName))
                _windowInitialPosCache[windowName] = ImGui.GetWindowPos();
            
            if (!_windowSizeCache.ContainsKey(windowName))
                _windowSizeCache[windowName] = ImGui.GetWindowSize();
            
            
            // then if the window needs to move
            if (_hasPendingDeltaThisFrame)
            {
                var offset = _windowOffsetCache[windowName];
                // var pos = ImGui.GetWindowPos() + offset;
                var targetPos = _windowInitialPosCache[windowName] + offset;
                ImGui.SetWindowPos(windowName, targetPos, ImGuiCond.Always);
            }
            
        }

        /// <summary>
        /// Optional: call inside the window after Begin, when it appears for the first time,
        /// to ensure the cache matches the actual position if user moved it or it was set by SetNextWindowPos.
        /// </summary>
        public static void UpdateWindowCache(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (!_registeredWindows.Contains(windowName)) return;
            
            var pos = ImGui.GetWindowPos();
            // _windowOffsetCache[windowName] = pos;
            _windowInitialPosCache[windowName] = pos;
            _windowOffsetCache[windowName] = Vector2.zero;
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

        /// <summary>
        /// Call inside the window's layout site BEFORE calling ImGui.Begin(windowName).
        /// Returns true if the window should be drawn this frame. If false, skip ImGui.Begin/End entirely.
        /// This avoids any rendering and input for windows that are fully outside the camera view.
        /// </summary>
        public static bool ShouldDraw(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return false;
            // if not registered, always draw
            if (!_registeredWindows.Contains(windowName)) return true;
            
            // Ensure caches
            if (!_windowOffsetCache.ContainsKey(windowName))
                _windowOffsetCache[windowName] = Vector2.zero;

            if (!_windowInitialPosCache.ContainsKey(windowName))
            {
                // We don't know initial position yet (first frame before Begin).
                // Assume current imgui cursor pos to 0,0
                // real initial pos will be set in UpdateWindowCache right after Begin();
                _windowInitialPosCache[windowName] = Vector2.zero;
            }
            
            // target rect (position, size)
            Vector2 offset = _windowOffsetCache[windowName];
            Vector2 initialPos = _windowInitialPosCache[windowName];
            // Vector2 currentPos = ImGui.GetWindowPos();
            Vector2 targetPos = initialPos + offset;
            Vector2 size = _windowSizeCache.TryGetValue(windowName, out var lastSize) ? lastSize : new Vector2(1, 1);

            var io = ImGui.GetIO();
            // screen bounds (top-left origin for ImGui)
            float screenW = io.DisplaySize.x;
            float screenH = io.DisplaySize.y;
         
            // Check if the window would be outside the screen
            // if (IsRectOutsideScreen(targetPos, size, screenW, screenH))
            // {
            //     ImGui.SetWindowCollapsed(true, ImGuiCond.Always);
            //     return;
            // }
            //
            // ImGui.SetWindowCollapsed(false, ImGuiCond.Always);
            
            bool outside = IsRectOutsideScreen(targetPos, size, screenW, screenH);
            return !outside;
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

            //Debug.Log($"Window size: {size.x}:{size.y} | Rect data:  {left}, {right}, {top}, {bottom} | Screen size: {screenW}, {screenH}");
            return (right <= xSize || left + xSize >= screenW || bottom <= 0f || top >= screenH);
        }

        #endregion

        #region Private & Protected

        private static readonly List<string> _registeredWindows = new List<string>();
        // offset from baseline (camera/world pan)
        private static readonly Dictionary<string, Vector2> _windowOffsetCache = new Dictionary<string, Vector2>();
        // absolute position captured on first appearance
        private static readonly Dictionary<string, Vector2> _windowInitialPosCache = new Dictionary<string, Vector2>();
        // last known window size
        private static readonly Dictionary<string, Vector2> _windowSizeCache = new Dictionary<string, Vector2>();
        // stocked from last ShouldDraw call
        private static readonly Dictionary<string, bool> _windowVisibility = new Dictionary<string, bool>();

        private static bool _hasPendingDeltaThisFrame;

        #endregion
    }
}