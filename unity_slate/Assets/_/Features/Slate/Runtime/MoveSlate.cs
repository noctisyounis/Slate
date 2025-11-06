using Foundation.Runtime;
using Inputs.Runtime;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Slate.Runtime
{
    public class MoveSlate : FBehaviour
    {

#if UNITY_STANDALONE_WIN

        #region Monobehaviour Methods
        private void Awake()
        {
            RegisterEvents(true);
        }

        private void Update()
        {
            WindowPositionManagement();
        }

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
                _inputsHandler.m_move += OnMovePressed;
                _inputsHandler.m_options += OnOptionsPressed;
            }
            else
            {
                _inputsHandler.m_move -= OnMovePressed;
                _inputsHandler.m_options -= OnOptionsPressed;
            }
        }
        private void OnOptionsPressed(bool optionsPressed)
            => _optionsPressed = optionsPressed;

        private void OnMovePressed(Vector2 move) => _movement = move;

        private void WindowPositionManagement()
        {
            if (!Application.isFocused)
                return;

            bool isMoving = Mathf.Abs(_movement.magnitude) >= 0.01f;

            if (!isMoving || !_optionsPressed)
                return;

            MoveWindowByOffset(_movement);
        }

        private int FloatToExtremeInt(float value)
        {
            return (value >= 0.0f) ?
                Mathf.CeilToInt(value) : Mathf.FloorToInt(value);
        }

        #region Windows (obscure) side
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public void MoveWindowByOffset(Vector2 movement)
        {
            IntPtr hWnd = FindWindow(null, Application.productName);
            if (hWnd == IntPtr.Zero)
            {
                Debug.LogWarning("Window handle not found. Ensure your window title matches Application.productName.");
                return;
            }

            // Get current position and size
            GetWindowRect(hWnd, out RECT rect);

            int currentX = rect.Left;
            int currentY = rect.Top;
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // OPTI Z : We should get mouse movement on Windows and then properly displace our build window
            // based on the actual mouse movement but unfortunately I ain't got time
            int newX = currentX + FloatToExtremeInt(Mathf.RoundToInt(movement.x) * _moveSpeedAppX);
            // Z : dk why I need to negate movementY but it's working
            int newY = currentY + FloatToExtremeInt(Mathf.RoundToInt(-movement.y) * _moveSpeedAppY);

            SetWindowPos(hWnd, IntPtr.Zero, newX, newY, width, height, SWP_SHOWWINDOW);
        }
        #endregion

        #endregion

        #region Private variables
        [SerializeField] private InputsHandler _inputsHandler; 
        [SerializeField] private float _moveSpeedAppX = 10.0f;
        [SerializeField] private float _moveSpeedAppY = 10.0f;

        private Vector2 _movement;
        private bool _optionsPressed = false;
        
        private const uint SWP_SHOWWINDOW = 0x0040;
        #endregion
#endif
    }
}
