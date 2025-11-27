using UnityEngine;
using System;
using Foundation.Runtime;
using SharedData.Runtime;

using Inputs.Runtime;
using System.Runtime.InteropServices;

namespace Slate.Runtime
{
    /// <summary>
    /// Allows displacement of Slate Window at the top based on edge detection + drag & drop.
    /// Z : This script should ideally be enclosed within a #if !UNITY_EDITOR as it should only be ran in builds.
    /// Otherwise, this script technically allows users to move Unity window when holding the top bar within the editor.
    /// I've been told to move on as it wasn't high prio, sorry my friends, HF.
    /// </summary>
    public class MoveSlateWindow : FBehaviour
    {
#if UNITY_STANDALONE_WIN 

        #region Monobehaviour Methods
        private void Awake()
        {
            RegisterEvents(true);
            _window = FindWindow(null, Application.productName);
        }

        private void LateUpdate()
        {
            WindowPositionManagement();
            SaveMousePos();
        }

        private void SaveMousePos()
            => _mousePosLastFrame = _currentMousePos;


        private void OnDestroy()
        {
            RegisterEvents(false);
        }
        #endregion

        #region Custom Methods
        private void RegisterEvents(bool value)
        {
            if (value)
            {
                _inputsHandler.m_moveRaw += OnMovePressed;
                _inputsHandler.m_select += OnSelectPressed;
            }
            else
            {
                _inputsHandler.m_moveRaw -= OnMovePressed;
                _inputsHandler.m_select -= OnSelectPressed;
            }
        }
        private void OnSelectPressed(bool selectPressed)
        {
            _selectPressed = selectPressed;

            if (!selectPressed)
            {
                _isBeingMoved = false;
                _holdingWindow = false;
            }
            else if (_toolbarSO.m_isPointerInToolbar)
                _holdingWindow = true;
        }

        private void OnMovePressed(Vector2 move) => _rawMovement = move;

        /// <summary>
        /// In LateUpdate to prevent small compute issues while syncing with user inputs
        /// </summary>
        private void WindowPositionManagement()
        {
            if (!Application.isFocused || !_holdingWindow)
                return;

            bool isMoving = Mathf.Abs(_rawMovement.normalized.magnitude) >= 0.01f;
            if (!isMoving)
                return;

            if (!_isBeingMoved)
            {
                _isBeingMoved = true;
                OnWindowDragged();
            }
            MoveWindowBasedOnCursor();
        }

        private void OnWindowDragged()
        {
            _window = GetActiveWindow();

            bool getCursor = GetCursorPos(out _currentMousePos);
            bool getWindowRect = GetWindowRect(_window, out RECT rect);

            _mousePosLastFrame = _currentMousePos;
            if (!getWindowRect || !getCursor)
                return;
        }

        public void MoveWindowBasedOnCursor()
        {
            if (_window == IntPtr.Zero)
            {
                Debug.Log("Window handle not found. Ensure your window title matches Application.productName.");
                return;
            }

            GetCursorPos(out _currentMousePos); // update cursor

            // Get current position and size
            GetWindowRect(_window, out RECT rect);
            int currentX = rect.m_left; // x value of left top corner
            int currentY = rect.m_top;  // y value of left top corner
            // int width = rect.m_right - rect.m_left; 
            // int height = rect.m_bottom - rect.m_top;


            GetCursorPos(out POINT currentMousePos);
            Vector2Int mouseDeltaPos = new(currentMousePos.m_x - _mousePosLastFrame.m_x, currentMousePos.m_y - _mousePosLastFrame.m_y);

            // Z : The fact I'm casting to an int probably causes the small displacement when moving the mouse with it but that seems acceptable 
            int x = currentX + mouseDeltaPos.x;
            int y = currentY + mouseDeltaPos.y;

            SetWindowPos(_window, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        #region Windows (obscure) side
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Get current position and size
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// Windows function that allows you to move and resize a window
        /// </summary>
        /// <param name="hWnd">Window to apply operation on</param>
        /// <param name="hWndInsertAfter">Window (if you want to change the z-order). IntPtr.Zero if you don't want to change it.</param>
        /// <param name="X">x position</param>
        /// <param name="Y">y position</param>
        /// <param name="cx">Width alteration (based on flags)</param>
        /// <param name="cy">Height alteration (based on flags)</param>
        /// <param name="uFlags">Various compiler tags</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);
        
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT pos);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetActiveWindow();


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int m_left;
            public int m_top;
            public int m_right;
            public int m_bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int m_x;
            public int m_y;
        }
        #endregion

        #endregion

        #region Private variables
        [SerializeField] private InputsHandler _inputsHandler;
        [SerializeField] private ToolbarSharedState _toolbarSO;

        private IntPtr _window;
        private Vector2 _rawMovement;
        private bool _selectPressed = false;

        private bool _holdingWindow = false;
        private bool _isBeingMoved = false;
        private POINT _mousePosLastFrame; // POINT seems to be required by Windows for some reason
        private POINT _currentMousePos;

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_SHOWWINDOW = 0x0040;
        #endregion
#endif
    }
}
