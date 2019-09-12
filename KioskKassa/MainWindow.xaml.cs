using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace KioskKassa
{

    public partial class MainWindow : Window
    {
        private String code = String.Empty;
        private Timer exitTimer;
        private Timer monitorTimer;
        private DispatcherTimer dispatcherTimer;
        private String feedbackMessage = String.Empty;
        private Boolean coverScreen = false;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public MainWindow()
        {
            InitializeComponent();

            // Load the image from the resource.
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            //Stream myStream = myAssembly.GetManifestResourceStream("KioskKassa.logo_lubee.png");
            BitmapImage bmi = new BitmapImage(new Uri("pack://application:,,,/logo_lubee.png"));
            image.Source = bmi;

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
                //String secret = now.ToString("ddMM");
                String secret = now.ToString("HHdd");
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
            // Whether MplusKASSA is running.
            bool mPlusKassaIsRunning = false;

            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try
                {
                    // Console.WriteLine(process.ProcessName);
                    if (process.ProcessName == "MplusQ")
                    {
                        mPlusKassaIsRunning = true;

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

            // If MplusKASSA does not run, 
            // then set this application, KioskKassa, fullscreen.
            // The purpose of this is that it then prevents the user
            // from making setting and starting other apps.
            if (!mPlusKassaIsRunning)
            {
                coverScreen = true;
            }
        }

        private void feedbackStart ()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimerTick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }

        private void dispatcherTimerTick(Object obj, EventArgs e)
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
            if (coverScreen)
            {
                WindowState = WindowState.Maximized;
                Topmost = true;
                coverScreen = false;
            }
        }

        private void feedback(String message)
        {
            feedbackMessage = message;
        }


    }

}

