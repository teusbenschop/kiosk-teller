using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace KioskKassa
{

    public partial class MainWindow : Window
    {
        private String code = String.Empty;
        private Timer exitTimer;
        private Timer restartTimer;
        private Timer monitorTimer;
        private Boolean windowIsActive = false;

        private Process tellerProcess;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public MainWindow()
        {
            InitializeComponent();
            feedback("KioskKassa start");
            autostart();
            startTeller();
            startMonitor();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            String label = button.Content.ToString();
            code += label;
            feedback(code);
            if (code.Length >= 4)
            {
                DateTime now = DateTime.Now;
                String secret = now.ToString("ddMM");
                if (code == secret)
                {
                    exitTimer = new System.Timers.Timer(1000);
                    exitTimer.Elapsed += exit;
                    exitTimer.Start();
                    this.feedback("Schakelt uit");
                }
                code = String.Empty;
            }
        }

        private void feedback (String message)
        {
            feedbackUI(message);
        }

        private void feedbackUI (String message)
        {
            try
            {
                if (Label3.Content.ToString().Length > 4)
                {
                    Label1.Content = Label2.Content;
                    Label2.Content = Label3.Content;
                }
                Label3.Content = message;
            }
            catch (Exception)
            {
            }
        }

        private void exit (Object obj, ElapsedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            feedback("Programma sluit niet");
            e.Cancel = true;
        }

        private void autostart()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                string registryValue = registryKey.GetValue("KioskKassa").ToString();
                if (registryValue == Assembly.GetExecutingAssembly().Location)
                {
                    feedback("Autostart is on");
                }
                else
                {
                    feedback("Setting autostart on");
                    registryKey.SetValue("KioskKassa", Assembly.GetExecutingAssembly().Location);
                }
            }
            catch (Exception exception)
            {
                feedback(exception.ToString());
            }
        }

        private void startTeller()
        {
            try
            {
                tellerProcess = new Process();
                tellerProcess.StartInfo.WorkingDirectory = @"C:\Program Files (x86)\Mplus Software\MplusKASSA\bin";
                tellerProcess.StartInfo.FileName = "MplusQ.exe";
                tellerProcess.StartInfo.Arguments = "";
                tellerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                tellerProcess.EnableRaisingEvents = true;
                tellerProcess.Exited += new EventHandler(tellerExited);
                tellerProcess.Start();
            }
            catch (Exception exception)
            {
                feedback(exception.ToString());
            }
        }

        private void tellerExited(object sender, System.EventArgs e)
        {
            feedback("Kassa is gestopt");
            // Delay shortly, to avoid endless loops that slow down the system.
            restartTimer = new System.Timers.Timer(1000);
            restartTimer.AutoReset = false;
            restartTimer.Elapsed += restartTimeout;
            restartTimer.Start();
        }


        private void restartTimeout(Object obj, ElapsedEventArgs e)
        {
            if (windowIsActive)
            {
                // If the KioskKassa is active, postpone restarting the Teller app.
                restartTimer = new System.Timers.Timer(1000);
                restartTimer.AutoReset = false;
                restartTimer.Elapsed += restartTimeout;
                restartTimer.Start();
            }
            else
            {
                // When the teller process exits or crashes, restart it again.
                tellerProcess.Start();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            windowIsActive = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            windowIsActive = false;
        }

        private void startMonitor ()
        {
            monitorTimer = new Timer(1000);
            monitorTimer.Elapsed += monitorRun;
            monitorTimer.AutoReset = true;
            monitorTimer.Start();
        }

        private void monitorRun(Object obj, ElapsedEventArgs e)
        {
            tellerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

            try
            {
                IntPtr windowHandle = tellerProcess.MainWindowHandle;

                const int SW_MAXIMIZE = 3;
                ShowWindow(windowHandle, SW_MAXIMIZE);

                IntPtr HWND_TOPMOST = new IntPtr(-1);
                const UInt32 SWP_NOSIZE = 0x0001;
                //const UInt32 SWP_NOMOVE = 0x0002;
                const UInt32 SWP_SHOWWINDOW = 0x0040;
                SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);

                Process[] processes = Process.GetProcessesByName("iexplore");
                foreach (Process process in processes)
                {
                    feedback("Terminating " + process.ProcessName);
                    process.Kill();
                }
                processes = Process.GetProcessesByName("MicrosoftEdge");
                foreach (Process process in processes)
                {
                    feedback("Terminating " + process.ProcessName);
                    process.Kill();
                }
            }
            catch (Exception exception)
            {
                feedback(exception.Message);
            }

        }

    }

}

