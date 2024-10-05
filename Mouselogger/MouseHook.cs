using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Mouselogger
{
    public class MouseHook
    {
        [DllImport("User32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("User32.dll")]
        public static extern int CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        readonly IntPtr mouseHandle;
        readonly IntPtr moduleHandle;

        public event MouseEventHandler OnMouseActivity;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        public delegate void MouseEventHandler(object sender, MouseEventArgs e);

        readonly HookProc mouseHookProc;
        MouseButtons buttonStates;

        public MouseHook()
        {
            moduleHandle = GetMainModuleHandle();
            mouseHookProc = MouseHookProcedure;
            mouseHandle = SetWindowsHookEx(HookType.WH_MOUSE_LL, mouseHookProc, moduleHandle, 0);

            buttonStates = MouseButtons.None;
            if (mouseHandle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        ~MouseHook()
        {
            Stop();
        }

        public void Stop()
        {
            if (mouseHandle != IntPtr.Zero && !UnhookWindowsHookEx(mouseHandle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private int MouseHookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                var mouseHookStruct = (MouseLowLevelHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLowLevelHookStruct));

                short mouseDelta = 0;
                var msg = (WindowMessages)wParam.ToInt32();

                switch (msg)
                {
                    case WindowMessages.WM_LBUTTONDOWN:
                        buttonStates |= MouseButtons.Left;
                        break;
                    case WindowMessages.WM_RBUTTONDOWN:
                        buttonStates |= MouseButtons.Right;
                        break;
                    case WindowMessages.WM_MBUTTONDOWN:
                        buttonStates |= MouseButtons.Middle;
                        break;
                    case WindowMessages.WM_LBUTTONUP:
                        buttonStates -= MouseButtons.Left;
                        break;
                    case WindowMessages.WM_RBUTTONUP:
                        buttonStates -= MouseButtons.Right;
                        break;
                    case WindowMessages.WM_MBUTTONUP:
                        buttonStates -= MouseButtons.Middle;
                        break;
                    case WindowMessages.WM_MOUSEWHEEL:
                        mouseDelta = (short)((mouseHookStruct.mouseData >> 16) & 0xFFFF);
                        break;
                }

                var clicks = 0;
                if (buttonStates != MouseButtons.None)
                {
                    clicks = (msg == WindowMessages.WM_LBUTTONDBLCLK) || (msg == WindowMessages.WM_RBUTTONDBLCLK) || (msg == WindowMessages.WM_MBUTTONDBLCLK) ? 2 : 1;
                }

                var e = new MouseEventArgs(buttonStates, clicks, mouseHookStruct.pt.X, mouseHookStruct.pt.Y, mouseDelta);
                OnMouseActivity(this, e);
            }
            return CallNextHookEx(mouseHandle, nCode, wParam, lParam);
        }

        public static IntPtr GetMainModuleHandle()
        {
            using (var process = Process.GetCurrentProcess())
            {
                using (var module = process.MainModule)
                {
                    return GetModuleHandle(module.ModuleName);
                }
            }            
        }
    }
}
