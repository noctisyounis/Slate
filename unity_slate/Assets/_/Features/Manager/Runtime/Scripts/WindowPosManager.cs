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
            
            _windowPosCache[windowName] = initialPos;
            _registeredWindows.Add(windowName);
        }

        public static void UnregisterWindow(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (!_registeredWindows.Contains(windowName)) return;
            
            _registeredWindows.Remove(windowName);
        }
        public static void UnregisterWindow(string windowName, Vector2 initialPos)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (!_registeredWindows.Contains(windowName)) return;
            
            _windowPosCache.Remove(windowName);
            _registeredWindows.Remove(windowName);
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

                Vector2 oldPos;
                if (_windowPosCache.TryGetValue(currentWindow, out oldPos))
                {
                    Vector2 newPos = oldPos + screenDelta;
                    // add new window pos to cache
                    _windowPosCache[currentWindow] = newPos;
                }
            }
        }

        /// <summary>
        /// Call this inside the window's Begin(...) block each frame to:
        /// - Initialize cache from the actual ImGui position once (first time we see the window)
        /// - Apply pending delta (SetWindowPos with ImGuiCond.Always) only during pan frames
        /// </summary>
        public static void SyncWindowPosition(string windowName)
        {
            if (string.IsNullOrEmpty(windowName)) return;
            if (!_registeredWindows.Contains(windowName)) return;
            
            // If not in cache, grab true pos
            if (!_windowPosCache.ContainsKey(windowName))
            {
                Vector2 current = ImGui.GetWindowPos();
                _windowPosCache[windowName] = current;
            }
            
            // then if the window needs to move
            if (_hasPendingDeltaThisFrame)
            {
                var pos = _windowPosCache[windowName];
                ImGui.SetWindowPos(windowName, pos, ImGuiCond.Always);
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
            _windowPosCache[windowName] = pos;
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

        #region Private & Protected

        private static readonly List<string> _registeredWindows = new List<string>();
        private static readonly Dictionary<string, Vector2> _windowPosCache = new Dictionary<string, Vector2>();

        private static bool _hasPendingDeltaThisFrame;

        #endregion
    }
}