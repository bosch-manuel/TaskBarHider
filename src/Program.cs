using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;


namespace TaskbarHide
{

    public class Program
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr parentHwnd, IntPtr childAfterHwnd, IntPtr className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        private delegate bool EnumThreadProc(IntPtr hwnd, IntPtr lParam);

        static Boolean visible = false;
        static Timer timer;
        //we only want to have one instance of this app > copied from http://sanity-free.org/143/csharp_dotnet_single_instance_application.html
        static System.Threading.Mutex mutex = new System.Threading.Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        static void Main(string[] args)
        {   
            if(mutex.WaitOne(TimeSpan.Zero, true)) {
                timer = new Timer();  
                timer.Interval = 1000;
                timer.Tick += new EventHandler(OnTick);
                timer.Start();

                Console.WriteLine("Hiding taskbar");
                Console.WriteLine("Press ALT + T to toggle visiblilty or ALT + Q to exit ...");
                ShowTaskbar(visible);
                var kh = new KeyboardHook(true);
                kh.KeyDown += Kh_KeyDown;
                Application.Run();
                mutex.ReleaseMutex();
            }            
        }

        private static void OnTick(Object myObject, EventArgs myEventArgs){
            ShowTaskbar(visible);
        }

        private static void Kh_KeyDown(Keys key, bool Shift, bool Ctrl, bool Alt)
        {
             if(Alt) {
                switch (key)
                {
                    case Keys.Q:
                        ShowTaskbar(true);
                        Application.Exit();
                        break;
                    case Keys.T:
                        visible = !visible;
                        if(visible) {
                            timer.Stop();
                        }else {
                            timer.Start(); 
                        }
                        ShowTaskbar(visible);  
                    break;
                }
            }
        }

        private static void ShowTaskbar(bool show)
        {
            // get taskbar window
            IntPtr taskBarWnd = FindWindow("Shell_TrayWnd", null);

            // try it the WinXP way first...
            IntPtr startWnd = FindWindowEx(taskBarWnd, IntPtr.Zero, "Button", "Start");

            if (startWnd == IntPtr.Zero)
            {
                // try an alternate way, as mentioned on CodeProject by Earl Waylon Flinn
                startWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, "Start");
            }

            if (startWnd == IntPtr.Zero)
            {
                // ok, let's try the Vista easy way...
                startWnd = FindWindow("Button", null);
            }

            ShowWindow(taskBarWnd, show ? SW_SHOW : SW_HIDE);
            ShowWindow(startWnd, show ? SW_SHOW : SW_HIDE);
        }
    }
}
