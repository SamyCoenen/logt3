using System;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;


namespace Hook_Keyboard
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImportAttribute("User32.dll")]
        public static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpbKeyState,
        byte[] lpChar, int uFlags);
        [DllImportAttribute("User32.dll")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        string m_strLast = "";
        string m_strPrefix = "";
        LowLevelHook llhEngine = new LowLevelHook();      
        
        public Window1()
        {
            InitializeComponent();           
            llhEngine.KeyDownChar += new LowLevelHook.KeyDownCharEventHandler(llhEngine_KeyDownChar);
        }

        void llhEngine_KeyDownChar(uint vkCode, uint scancode)
        {
            IntPtr ipForegroundWindow = GetForegroundWindow();
            int nLength = GetWindowTextLength(ipForegroundWindow);
            StringBuilder sbTemp = new StringBuilder("", nLength + 1);
            m_strPrefix = "";
            GetWindowText(ipForegroundWindow, sbTemp, sbTemp.Capacity);
            if (m_strLast != sbTemp.ToString())
            {
                m_strLast = sbTemp.ToString();
                m_strPrefix = Environment.NewLine + m_strLast + Environment.NewLine;
            }

            txtText.Dispatcher.Invoke(new UpdateTextDel(UpdateText), m_strPrefix + ProcessInputKey(vkCode,scancode));
        }

        private string ProcessInputKey(uint vkCode, uint scancode)
        {
            char cTemp = GetAsciiCharacter(vkCode, scancode);
            if ( (int)cTemp != 0x0 && (int)cTemp != 0x8)
            {
                return cTemp.ToString();
            }
            else
            {
                return "[" + ((System.Windows.Forms.Keys)vkCode).ToString() + "]";
            }
        }

        public static char GetAsciiCharacter(uint uVirtKey, uint uScanCode)
        {
            byte[] lpKeyState = new byte[256];
            GetKeyboardState(lpKeyState);
            byte[] lpChar = new byte[2];
            if (ToAscii(uVirtKey, uScanCode, lpKeyState, lpChar, 0) == 1)
                return (char)lpChar[0];
            else
                return new char();
        }

        void vwWatcher_WindowsChanged(string strNewOwner)
        {           
            txtText.Dispatcher.Invoke(new UpdateTextDel(UpdateText),Environment.NewLine + strNewOwner + Environment.NewLine);            
        }

        private delegate void UpdateTextDel(string str);

        private void UpdateText(string str)
        {                                  
            txtText.Text += str;
        }  

        private void Window_Closed(object sender, EventArgs e)
        {
            llhEngine.Unhook();
        }

       
    }
}
