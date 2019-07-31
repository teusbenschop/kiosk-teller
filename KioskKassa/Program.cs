using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KioskKassa
{
    class Program
    {
        private static Process tellerProcess;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            Console.WriteLine("Kiosk Kassa 1.0");

            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (registryKey.GetValue("KioskKassa") == null)
                {
                    Console.WriteLine("Setting autostart on");
                    registryKey.SetValue("KioskKassa", Assembly.GetExecutingAssembly().Location);
                }
                else
                {
                    Console.WriteLine("Autostart is on");
                }
            }
            catch (Exception exception)
            {
                Console.Write(exception.ToString());
            }

            tellerProcess = new Process();
            tellerProcess.StartInfo.WorkingDirectory = @"C:\Windows\System32";
            tellerProcess.StartInfo.FileName = "notepad.exe";
            tellerProcess.StartInfo.Arguments = "";
            tellerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            tellerProcess.EnableRaisingEvents = true;
            tellerProcess.Exited += new EventHandler(ProcessExited);
            tellerProcess.Start();

            while (true)
            {
                //Console.WriteLine(DateTime.Now.ToString());
                Thread.Sleep(1000);
                tellerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

                try
                {
                    IntPtr windowHandle = tellerProcess.MainWindowHandle;

                    const int SW_MAXIMIZE = 3;
                    ShowWindow(windowHandle, SW_MAXIMIZE);

                    IntPtr HWND_TOPMOST = new IntPtr(-1);
                    const UInt32 SWP_NOSIZE = 0x0001;
                    const UInt32 SWP_NOMOVE = 0x0002;
                    const UInt32 SWP_SHOWWINDOW = 0x0040;
                    SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                    /*
                    Process[] processes = Process.GetProcessesByName("Microsoft Edge");
                    foreach (Process process in processes)
                    {
                        process.Kill();
                    }
                    */
                }
                catch (Exception)
                {
                }
            }

        }

        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit");
        }

        private static void ProcessExited(object sender, System.EventArgs e)
        {
            // When the teller process exits or crashes, restart it straightaway.
            tellerProcess.Start();
        }
    }
}
