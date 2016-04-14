using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace Hook_Keyboard
{
    public class LowLevelHook
    {
        #region ImportFunction
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowsHookExA")]
        static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc lpfn,
        IntPtr hMod, uint dwThreadId);        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);
        #endregion

        #region StructureType
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public UInt32 vkCode;
            public UInt32 scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public UInt32 time;
            public IntPtr dwExtraInfo;
        }

        [Flags()]
        public enum KBDLLHOOKSTRUCTFlags : int
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }
        #endregion

        #region Constant
        const int HC_ACTION = 0;
        const int WM_KEYDOWN = 0x100;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYUP = 0x105;
        #endregion

        #region Events      
        public delegate void KeyDownCharEventHandler(uint vkCode,uint scancode);    
        public event KeyDownCharEventHandler KeyDownChar;       
        #endregion

        #region MemberVariables
        private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
        private HookProc KeyboardHookDel;
        private IntPtr m_ipKeyHookResult = IntPtr.Zero;
        #endregion

        

        public LowLevelHook()
        {
            Process process = Process.GetCurrentProcess();
            ProcessModule module = process.MainModule;
            IntPtr hModule = GetModuleHandle(module.ModuleName);
            KeyboardHookDel = KeyboardHookProc;
            m_ipKeyHookResult = SetWindowsHookEx(HookType.WH_KEYBOARD_LL, KeyboardHookDel, hModule, 0);
        }

        public void Unhook()
        {
            UnhookWindowsHookEx(m_ipKeyHookResult);
        }

        private IntPtr KeyboardHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= HC_ACTION)
            {
                KBDLLHOOKSTRUCT kbhsStruct;
                switch (wParam.ToInt32())
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        kbhsStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, (new KBDLLHOOKSTRUCT()).GetType());                      
                        if (KeyDownChar != null) KeyDownChar(kbhsStruct.vkCode,kbhsStruct.scanCode);
                        break;
            
                }
            }
            return CallNextHookEx(m_ipKeyHookResult, code, wParam, lParam);
        }


    }
}
