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
                _inputsHandler.m_moveRaw += OnMovePressed;
                _inputsHandler.m_options += OnOptionsPressed;
            }
            else
            {
                _inputsHandler.m_moveRaw -= OnMovePressed;
                _inputsHandler.m_options -= OnOptionsPressed;
            }
        }
        private void OnOptionsPressed(bool optionsPressed)
            => _optionsPressed = optionsPressed;

        private void OnMovePressed(Vector2 move) => _rawMovement = move;

        private void WindowPositionManagement()
        {
            if (!Application.isFocused)
                return;

            bool isMoving = Mathf.Abs(_rawMovement.normalized.magnitude) >= 0.01f;

            if (!isMoving || !_optionsPressed)
                return;

            MoveWindowByOffset(_rawMovement);
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

            // Z : The fact I'm casting to an int probably causes the small displacement when moving the mouse with it but that seems acceptable
            int newX = currentX + FloatToExtremeInt(Mathf.RoundToInt(movement.x));
            // Z : dk why I need to negate movementY but it's working
            int newY = currentY + FloatToExtremeInt(Mathf.RoundToInt(-movement.y));

            SetWindowPos(hWnd, IntPtr.Zero, newX, newY, width, height, SWP_SHOWWINDOW);
        }
        #endregion

        #endregion

        #region Private variables
        [SerializeField] private InputsHandler _inputsHandler; 

        private Vector2 _rawMovement;
        private bool _optionsPressed = false;

        private const uint SWP_SHOWWINDOW = 0x0040;
        #endregion
#endif
    }
}
