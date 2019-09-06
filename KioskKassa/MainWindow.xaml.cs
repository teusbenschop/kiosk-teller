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
        private Timer monitorTimer;
        private Timer feedbackTimer;
        private String feedbackMessage = String.Empty;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public MainWindow()
        {
            InitializeComponent();
            feedback("KioskKassa start");
            autostart();
            startMonitor();
            feedbackStart();
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


        private void startMonitor ()
        {
            monitorTimer = new Timer(1000);
            monitorTimer.Elapsed += monitorRun;
            monitorTimer.AutoReset = true;
            monitorTimer.Start();
        }


        private void monitorRun(Object obj, ElapsedEventArgs e)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    // Console.WriteLine(process.ProcessName);
                    if (process.ProcessName == "MplusQ")
                    {
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;

                        IntPtr windowHandle = process.MainWindowHandle;

                        const int SW_MAXIMIZE = 3;
                        ShowWindow(windowHandle, SW_MAXIMIZE);

                        IntPtr HWND_TOPMOST = new IntPtr(-1);
                        const UInt32 SWP_NOSIZE = 0x0001;
                        //const UInt32 SWP_NOMOVE = 0x0002;
                        const UInt32 SWP_SHOWWINDOW = 0x0040;
                        SetWindowPos(windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);
                    }

                    if (process.ProcessName == "iexplore")
                    {
                        feedback("Terminating " + process.ProcessName);
                        process.Kill();
                    }

                    if (process.ProcessName == "MicrosoftEdge")
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

        private void feedbackStart ()
        {
            feedbackTimer = new Timer(100);
            feedbackTimer.Elapsed += feedbackRun;
            feedbackTimer.AutoReset = true;
            feedbackTimer.Start();
        }

        private void feedbackRun(Object obj, ElapsedEventArgs e)
        {
            if (!String.IsNullOrEmpty(feedbackMessage))
            {
                try
                {
                    if (Label3.Content.ToString().Length > 4)
                    {
                        Label1.Content = Label2.Content;
                        Label2.Content = Label3.Content;
                    }
                    Label3.Content = feedbackMessage;
                }
                catch (Exception)
                {
                }
                feedbackMessage = String.Empty;
            }
        }

        private void feedback(String message)
        {
            feedbackMessage = message;
            try
            {
                if (Label3.Content.ToString().Length > 4)
                {
                    Label1.Content = Label2.Content;
                    Label2.Content = Label3.Content;
                }
                Label3.Content = feedbackMessage;
            }
            catch (Exception)
            {
            }
        }


    }

}

