using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Slate.Runtime
{
    public class RescalableWin : MonoBehaviour
    {
        #region Header
        
            // === Paramètres tweakables ===
            [Header("Layout")]
            [Tooltip("Epaisseur (px) sur les bords pour attraper le redimensionnement.")]
            public int resizeBorder = 8;

            [Tooltip("Taille minimum de la fenêtre.")]
            public Vector2Int minSize = new Vector2Int(640, 360);

            [Tooltip("Centrer la fenêtre au lancement.")]
            public bool centerOnStart = true;

            [Tooltip("Autoriser le déplacement de la fenêtre en cliquant n'importe où.")]
            public bool dragAnywhere = true;

        #endregion

        #if UNITY_STANDALONE_WIN

        public void InitializeBorderlessWindow()
        {
            _hWnd = GetActiveWindow();
            ApplyBorderlessStyle();
            if (centerOnStart) CenterWindow();
            HookWndProc();
        }

        private void ApplyBorderlessStyle()
        {
            var style = GetWindowLong(_hWnd, _gwlStyle);
            style &= ~(
                _wsCaption |
                _wsSysmenu |
                _wsMinimizebox |
                _wsMaximizebox
            );
            style |= _wsThickframe;
            SetWindowLong(_hWnd, _gwlStyle, (int)style);

            var exStyle = GetWindowLong(_hWnd, _gwlExstyle);
            exStyle |= _wsExAppwindow;
            exStyle &= ~_wsExWindowedge;
            SetWindowLong(_hWnd, _gwlExstyle, (int)exStyle);

            SetWindowPos(
                _hWnd,
                IntPtr.Zero,
                0, 0, 0, 0,
                _swpNomove |
                _swpNosize |
                _swpNozorder |
                _swpFramechanged |
                _swpShowwindow
            );
        }

        private void CenterWindow()
        {
            if (!GetWindowRect(_hWnd, out var wr)) return;
            var hMon = MonitorFromWindow(_hWnd, _monitorDefaulttonearest);
            var mi = new MONITORINFO();
            if (!GetMonitorInfo(hMon, mi)) return;

            var winW = wr.right - wr.left;
            var winH = wr.bottom - wr.top;
            var workW = mi.rcWork.right - mi.rcWork.left;
            var workH = mi.rcWork.bottom - mi.rcWork.top;

            var x = mi.rcWork.left + (workW - winW) / 2;
            var y = mi.rcWork.top + (workH - winH) / 2;

            SetWindowPos(_hWnd, 
                IntPtr.Zero,
                x, y, 0, 0, 
                _swpNosize | _swpNozorder | _swpShowwindow
            );
        }

        private void HookWndProc()
        {
            if (_prevWndProc != IntPtr.Zero) return;

            _wndProcDelegate = CustomWndProc;
            _prevWndProc = SetWindowLongPtr(_hWnd, -4, _wndProcDelegate);
        }
        
        void OnDestroy()
        {
            if (_hWnd == IntPtr.Zero || _prevWndProc == IntPtr.Zero) return;
            SetWindowLongPtr(_hWnd,
                -4,
                Marshal.GetDelegateForFunctionPointer<WndProcDelegate>(_prevWndProc)
            );
            _prevWndProc = IntPtr.Zero;
        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case _wmNchittest:
                {
                    int x = (short)((ulong)lParam & 0xFFFF);
                    int y = (short)(((ulong)lParam >> 16) & 0xFFFF);

                    var p = new POINT { x = x, y = y };

                    if (!ScreenToClient(hWnd, ref p))
                        break;

                    if (!GetClientRect(hWnd, out var rc))
                        break;

                    var onLeft = p.x <= resizeBorder;
                    var onRight = p.x >= (rc.right - resizeBorder);
                    var onTop = p.y <= resizeBorder;
                    var onBottom = p.y >= (rc.bottom - resizeBorder);

                    if (onLeft) return (IntPtr)_htLeft;
                    if (onRight) return (IntPtr)_htRight;
                    if (onTop) return (IntPtr)_htTop;
                    if (onBottom) return (IntPtr)_htBottom;

                    if (dragAnywhere)
                        return (IntPtr)_htCaption;

                    return (IntPtr)_htClient;
                }

                case _wmGetminmaxinfo:
                {
                    var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    mmi.ptMinTrackSize.x = Mathf.Max(1, minSize.x);
                    mmi.ptMinTrackSize.y = Mathf.Max(1, minSize.y);
                    Marshal.StructureToPtr(mmi, lParam, true);
                    return IntPtr.Zero;
                }
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        #endif

        #region Private Variable
        
            // === Win32 ===
            private const int _gwlStyle = -16;
            private const int _gwlExstyle = -20;

            // Styles
            private const uint _wsCaption      = 0x00C00000;
            private const uint _wsSysmenu      = 0x00080000;
            private const uint _wsThickframe   = 0x00040000;  // garde la capacité de resize côté Win32
            private const uint _wsMinimizebox  = 0x00020000;
            private const uint _wsMaximizebox  = 0x00010000;

            private const uint _wsExAppwindow = 0x00040000;
            private const uint _wsExWindowedge = 0x00000100;

            // Messages
            private const int _wmNchittest = 0x0084;
            private const int _wmGetminmaxinfo = 0x0024;

            // HitTest codes
            private const int _htClient = 1;
            private const int _htCaption = 2;
            private const int _htLeft = 10;
            private const int _htRight = 11;
            private const int _htTop = 12;
            private const int _htBottom = 15;

            // SetWindowPos flags
            private const uint _swpNosize = 0x0001;
            private const uint _swpNomove = 0x0002;
            private const uint _swpNozorder = 0x0004;
            private const uint _swpFramechanged = 0x0020;
            private const uint _swpShowwindow = 0x0040;

            // Monitor flags
            private const uint _monitorDefaulttonearest = 0x00000002;

            private static WndProcDelegate _wndProcDelegate; // garder une ref statique pour GC
            private static IntPtr _prevWndProc = IntPtr.Zero;
            private IntPtr _hWnd = IntPtr.Zero;

            private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
            
        #endregion

        #region Private Structure
        
            [StructLayout(LayoutKind.Sequential)]
            private struct POINT { public int x, y; }

            [StructLayout(LayoutKind.Sequential)]
            private struct RECT { public int left, top, right, bottom; }

            [StructLayout(LayoutKind.Sequential)]
            private struct MINMAXINFO
            {
                public POINT ptReserved;
                public POINT ptMaxSize;
                public POINT ptMaxPosition;
                public POINT ptMinTrackSize;
                public POINT ptMaxTrackSize;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private class MONITORINFO
            {
                public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                public RECT rcMonitor = new RECT();
                public RECT rcWork = new RECT();
                public int dwFlags = 0;
            }

        #endregion
        
        #region DllImport
        
            [DllImport("user32.dll")]
            private static extern IntPtr GetActiveWindow();
            [DllImport("user32.dll")]
            private static extern IntPtr SetWindowLongPtr(
                IntPtr hWnd,
                int nIndex,
                WndProcDelegate newProc);
            [DllImport("user32.dll")]
            private static extern IntPtr GetWindowLongPtr(
                IntPtr hWnd,
                int nIndex);
            [DllImport("user32.dll")]
            private static extern uint GetWindowLong(
                IntPtr hWnd,
                int nIndex);
            [DllImport("user32.dll")]
            private static extern int SetWindowLong(
                IntPtr hWnd,
                int nIndex,
                int dwNewLong
                );
            [DllImport("user32.dll")]
            private static extern bool SetWindowPos(
                IntPtr hWnd,
                IntPtr hWndInsertAfter,
                int x, int y, int cx, int cy,
                uint uFlags);
            [DllImport("user32.dll")]
            private static extern bool GetClientRect(
                IntPtr hWnd,
                out RECT rect);
            [DllImport("user32.dll")]
            private static extern bool GetWindowRect(
                IntPtr hWnd,
                out RECT rect);
            [DllImport("user32.dll")]
            private static extern bool ScreenToClient(
                IntPtr hWnd,
                ref POINT lpPoint);
            [DllImport("user32.dll")]
            private static extern IntPtr DefWindowProc(
                IntPtr hWnd,
                uint msg,
                IntPtr wParam,
                IntPtr lParam);
            [DllImport("user32.dll")]
            private static extern IntPtr MonitorFromWindow(
                IntPtr hwnd,
                uint dwFlags);
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern bool GetMonitorInfo(
                IntPtr hMonitor,
                MONITORINFO lpmi);

        #endregion
    }
}