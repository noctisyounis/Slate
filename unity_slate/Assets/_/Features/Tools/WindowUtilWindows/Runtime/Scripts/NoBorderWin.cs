using System;
using System.Runtime.InteropServices;
using Foundation.Runtime;
using UnityEngine;

namespace Slate.Runtime
{
    public class NoBorderWin : FBehaviour
    {
        #region public

            [Tooltip("Resize margin in logical pixels (DPI-aware). 12–16 is comfy.")]
            public int logicalResizeMargin = 14;

            [Tooltip("Enable corner sizing (diagonal).")]
            public bool enableCornerHitTest = true;

        #endregion
        
        #if UNITY_STANDALONE_WIN

        private void Start()
        {
            _hWnd = GetActiveWindow();
            
            var style = GetWindowLong(_hWnd, _gwlStyle);
            style &= ~(
                _wsBorder |
                _wsDlgframe |
                _wsCaption |
                _wsMinimize |
                _wsMaximize |
                _wsSysmenu
            );
            style |= _wsThickframe;
            SetWindowLong(_hWnd, _gwlStyle, (int)style);

            SetWindowPos(_hWnd, IntPtr.Zero, 0, 0, 0, 0,
                _swpNomove | _swpNosize | _swpNozorder | _swpFramechanged);

            _resizeMarginPhysical = LogicalToPhysical(logicalResizeMargin);

            _wndProcDelegate = CustomWndProc;
            _prevWndProc = SetWindowLongPtr(_hWnd, _gwlWndproc,
                Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
        }

        public void OnDestroy()
        {
            if (_hWnd == IntPtr.Zero || _prevWndProc == IntPtr.Zero) return;
            SetWindowLongPtr(_hWnd, _gwlWndproc, _prevWndProc);
            _prevWndProc = IntPtr.Zero;
        }
        
        private int LogicalToPhysical(int logicalPx)
        {
            try
            {
                var dpi = GetDpiForWindow(_hWnd);
                if (dpi == 0) dpi = 96;
                return Math.Max(1, (int)Math.Round(logicalPx * (dpi / 96f)));
            }
            catch
            {
                var hmon = MonitorFromWindow(_hWnd, _monitorDefaulttonearest);
                uint dpiX, dpiY;
                if (GetDpiForMonitor(hmon, _mdtEffectiveDPI, out dpiX, out dpiY) == 0 && dpiX != 0)
                    return Math.Max(1, (int)Math.Round(logicalPx * (dpiX / 96f)));
                return logicalPx;
            }
        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg != _wmNchittest) return CallWindowProc(_prevWndProc, hWnd, msg, wParam, lParam);
            int x = (short)((ulong)lParam & 0xFFFF);
            int y = (short)(((ulong)lParam >> 16) & 0xFFFF);

            if (!GetWindowRect(hWnd, out _rect rect)) return new IntPtr(_htclient);
            var margin = _resizeMarginPhysical;

            var onLeft   = x >= rect.left && x < rect.left + margin;
            var onRight  = x <= rect.right && x > rect.right - margin;
            var onTop    = y >= rect.top && y < rect.top + margin;
            var onBottom = y <= rect.bottom && y > rect.bottom - margin;

            if (enableCornerHitTest)
            {
                switch (onTop)
                {
                    case true when onLeft:
                        return new IntPtr(_httopleft);
                    case true when onRight:
                        return new IntPtr(_httopright);
                }

                switch (onBottom)
                {
                    case true when onLeft:
                        return new IntPtr(_htbottomleft);
                    case true when onRight:
                        return new IntPtr(_htbottomright);
                }
            }

            if (onLeft) return new IntPtr(_htleft);
            if (onRight) return new IntPtr(_htright);
            if (onTop) return new IntPtr(_httop);
            return onBottom ? new IntPtr(_htbottom) : new IntPtr(_htclient);
        }
                
        #endif
        
        #region private
        
            private const int _gwlStyle = -16;
            private const int _gwlWndproc = -4;
            
            private const uint _wsBorder = 0x00800000;
            private const uint _wsDlgframe = 0x00400000;
            private const uint _wsCaption = 0x00C00000;
            private const uint _wsMinimize = 0x00020000;
            private const uint _wsMaximize = 0x01000000;
            private const uint _wsSysmenu = 0x00080000;
            private const uint _wsThickframe = 0x00040000;

            private const uint _swpNosize = 0x0001;
            private const uint _swpNomove = 0x0002;
            private const uint _swpNozorder = 0x0004;
            private const uint _swpFramechanged = 0x0020;

            private const int _wmNchittest = 0x0084;

            private const int _htclient = 1;
            private const int _htleft   = 10;
            private const int _htright  = 11;
            private const int _httop    = 12;
            private const int _httopleft = 13;
            private const int _httopright = 14;
            private const int _htbottom = 15;
            private const int _htbottomleft = 16;
            private const int _htbottomright = 17;

            private const int _monitorDefaulttonearest = 2;
            private const int _mdtEffectiveDPI = 0;

            [StructLayout(LayoutKind.Sequential)]
            private struct _rect { public int left, top, right, bottom; }
            
            private delegate IntPtr WndProcDelegate(
                IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            private IntPtr _hWnd = IntPtr.Zero;
            private IntPtr _prevWndProc = IntPtr.Zero;
            private WndProcDelegate _wndProcDelegate;
            private int _resizeMarginPhysical = 12;
            
            private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc)
            {
                return IntPtr.Size == 8 ?
                    SetWindowLongPtr64(hWnd, nIndex, newProc)
                    : new IntPtr(SetWindowLong32(hWnd, nIndex, newProc.ToInt32()));
            }
        
        #endregion
        
        #region DllImport
        
            [DllImport("user32.dll")]
            private static extern IntPtr GetActiveWindow();

            [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
            private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
            private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
            [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
            private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
            private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool SetWindowPos(
                IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            private static extern IntPtr CallWindowProc(
                IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(IntPtr hWnd, out _rect lpRect);
            
            [DllImport("user32.dll")]
            private static extern int GetDpiForWindow(IntPtr hWnd);

            [DllImport("shcore.dll")]
            private static extern int GetDpiForMonitor(
                IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

            [DllImport("user32.dll")]
            private static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);
        
        #endregion
    }
}