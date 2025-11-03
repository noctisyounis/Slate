using System;
using System.Runtime.InteropServices;
using Foundation.Runtime;
using UnityEngine;

namespace Slate.Runtime
{
    public class NoBorderWin : FBehaviour
    {
        #if UNITY_STANDALONE_WIN
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        private void Start()
        {
            var handle = GetActiveWindow();
            var style = GetWindowLong(handle, _gwlStyle);
            
            style &= ~(_wsBorder |
                       _wsDlgframe |
                       _wsCaption |
                       _wsThickframe |
                       _wsMinimize |
                       _wsMaximize |
                       _wsSysmenu
                       );
            
            SetWindowLong(handle, _gwlStyle, style);
            
            Screen.SetResolution(854, 480, false);
        }
        #endif

        #region private
        
        private const int _gwlStyle = -16;
        private const uint _wsBorder = 0x00800000;
        private const uint _wsDlgframe = 0x00400000;
        private const uint _wsCaption = 0x00C00000;
        private const uint _wsThickframe = 0x00040000;
        private const uint _wsMinimize = 0x00020000;
        private const uint _wsMaximize = 0x01000000;
        private const uint _wsSysmenu = 0x00080000;

        #endregion
    }
}