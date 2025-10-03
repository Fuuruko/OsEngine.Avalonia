/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Win32;

// using OsEngine.Alerts;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Utils;
using OsEngine.Views.Optimizer;
using OsEngine.Views.OsConverter;


// using OsEngine.Layout;
// using OsEngine.Market;
// using OsEngine.OsConverter;
using OsEngine.Views.OsData;
using OsEngine.Views.Terminal;


// using OsEngine.OsOptimizer;
// using OsEngine.OsTrader.Gui;
// using OsEngine.OsTrader.Gui.BlockInterface;
using OsEngine.Views.Utils;

namespace OsEngine
{

    /// <summary>
    /// Application start screen
    /// Стартовое окно приложения
    /// </summary>
    public partial class MainWindow : Window
    {

        private static MainWindow _window;

        public static readonly Dispatcher Dispatcher = Dispatcher.UIThread;

        public static bool DebuggerIsWork;

        /// <summary>
        ///  is application running
        /// работает ли приложение или закрывается
        /// </summary>
        public static bool ProccesIsWorked;

        public MainWindow()
        {
            // Process ps = Process.GetCurrentProcess();
            // // ps.PriorityClass = ProcessPriorityClass.RealTime;
            //
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            PrimeSettingsMaster.Load();
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // ImageAlor2.IsVisible = false;
            // ImageAlor.IsVisible = false;

            // Closing += MainWindow_Closing;

            // try
            // {
            //     int winVersion = Environment.OSVersion.Version.Major;
            //     if (winVersion < 6)
            //     {
            //         MessageBox.Show(OsLocalization.MainWindow.Message1);
            //         Close();
            //     }
            //     if (!CheckDotNetVersion())
            //     {
            //         Close();
            //     }
            //
            //     if (!CheckWorkWithDirectory())
            //     {
            //         MessageBox.Show(OsLocalization.MainWindow.Message2);
            //         Close();
            //     }
            //
            //     if (!CheckOutSomeLibrariesNearby())
            //     {
            //         MessageBox.Show(OsLocalization.MainWindow.Message6);
            //         Close();
            //     }
            //
            //     if (!CheckAlreadyWorkEngine())
            //     {
            //         MessageBox.Show(OsLocalization.MainWindow.Message7);
            //         Close();
            //     }
            //
            // }
            // catch (Exception)
            // {
            //     MessageBox.Show(OsLocalization.MainWindow.Message3);
            //     Close();
            // }

            // if (Debugger.IsAttached)
            // {
            //     DebuggerIsWork = true;
            // }

            ProccesIsWorked = true;
            _window = this;

            // ServerMaster.Activate();

            Thread.CurrentThread.CurrentCulture = OsLocalization.CurCulture;

            Task task = new(ThreadAreaGreeting);
            task.Start();

            ChangeText();
            OsLocalization.LocalizationTypeChangeEvent += ChangeText;

            CommandLineInterfaceProcess();

            Task.Run(ClearOptimizerWorkResults);

            Activate();
            Focus();

            // GlobalGUILayout.Listen(this, "mainWindow");

            ImageAlor.PointerEntered += ImageAlor_MouseEnter;
            ImageAlor2.PointerExited += ImageAlor_MouseLeave;
            ImageAlor2.PointerPressed += ImageAlor2_MouseDown;

            // if (BlockMaster.IsBlocked == true)
            // {
            //     BlockInterface();
            // }
            // else
            // {
            //     UnblockInterface();
            // }

            ChangeText();

            // ContentRendered += MainWindow_ContentRendered;
        }

        #region Block and Unblock interface

        private void BlockInterface()
        {
            // ImageData.Visibility = Visibility.Hidden;
            // ImageTests.Visibility = Visibility.Hidden;
            // ImageTrading.Visibility = Visibility.Hidden;

            // ImagePadlock.Visibility = Visibility.Visible;
            // ImagePadlock.PointerEntered += ImagePadlock_MouseEnter;
            // ImagePadlock.PointerExited += ImagePadlock_MouseLeave;
            // ImagePadlock.PointerPressed += ImagePadlock_MouseDown;
            ButtonSettings.IsEnabled = false;
            ButtonRobot.IsEnabled = false;
            ButtonTester.IsEnabled = false;
            ButtonData.IsEnabled = false;
            ButtonCandleConverter.IsEnabled = false;
            ButtonConverter.IsEnabled = false;
            ButtonOptimizer.IsEnabled = false;
            ButtonTesterLight.IsEnabled = false;
            ButtonRobotLight.IsEnabled = false;
        }

        private void ImagePadlock_MouseDown(object sender, RoutedEventArgs e)
        {
            // RobotsUiLightUnblock ui = new();
            //
            // ui.ShowDialog(this);
            //
            // if (ui.IsUnBlocked == true)
            // {
            //     UnblockInterface();
            // }
        }

        private void ImagePadlock_MouseLeave(object sender, RoutedEventArgs e)
        {
            Cursor = new(StandardCursorType.Arrow);
            // ImagePadlock.Cursor = StandardCursorType.Arrow;
        }

        private void ImagePadlock_MouseEnter(object sender, RoutedEventArgs e)
        {
            // ImagePadlock.Cursor = StandardCursorType.Hand;
            Cursor = new(StandardCursorType.Hand);
        }

        private void UnblockInterface()
        {
            // ImageData.Visibility = Visibility.Visible;
            // ImageTests.Visibility = Visibility.Visible;
            // ImageTrading.Visibility = Visibility.Visible;
            //
            // ImagePadlock.Visibility = Visibility.Hidden;
            ButtonSettings.IsEnabled = true;
            ButtonRobot.IsEnabled = true;
            ButtonTester.IsEnabled = true;
            ButtonData.IsEnabled = true;
            ButtonCandleConverter.IsEnabled = true;
            ButtonConverter.IsEnabled = true;
            ButtonOptimizer.IsEnabled = true;
            ButtonTesterLight.IsEnabled = true;
            ButtonRobotLight.IsEnabled = true;
        }

        #endregion

        private void ImageAlor2_MouseDown(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("https://www.alorbroker.ru/open?pr=L0745") { UseShellExecute = true });
            }
            catch
            {
                // ignore
            }
        }

        private void ImageAlor_MouseLeave(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OsLocalization.CurLocalization == OsLocalization.OsLocalType.Ru)
                {
                    ImageAlor2.IsVisible = false;
                    ImageAlor.IsVisible = true;
                }
            }
            catch
            {
                // ignore
            }
        }

        private void ImageAlor_MouseEnter(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OsLocalization.CurLocalization == OsLocalization.OsLocalType.Ru)
                {
                    ImageAlor2.IsVisible = true;
                    ImageAlor.IsVisible = false;
                }
            }
            catch
            {
                // ignore
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // GlobalGUILayout.IsClosed = true;

            if (ProccesIsWorked == true)
            {
                ProccesIsWorked = false;

                if (IsVisible == false)
                {
                    // _awaitUiBotsInfoLoading = new AwaitObject(OsLocalization.Trader.Label391, 100, 0, true);
                    // AwaitUi ui = new(_awaitUiBotsInfoLoading);

                    Thread worker = new(Await7Seconds);
                    worker.Start();

                    // ui.ShowDialog(this);
                }
            }

            Thread.Sleep(500);

            Process.GetCurrentProcess().Kill();
        }

        // AwaitObject _awaitUiBotsInfoLoading;

        private void Await7Seconds()
        {
            // Это нужно чтобы потоки сохраняющие данные в файловую систему штатно завершили свою работу
            // This is necessary for threads saving data to the file system to complete their work properly
            Thread.Sleep(7000);
            // _awaitUiBotsInfoLoading.Dispose();
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                try
                {
                    ChangeText();
                }
                catch
                {
                    // ignore
                }
            });
        }

        private void ChangeText()
        {

            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(ChangeText));
                return;
            }

            Title = OsLocalization.MainWindow.Title;
            BlockDataLabel.Content = OsLocalization.MainWindow.BlockDataLabel;
            BlockTestingLabel.Content = OsLocalization.MainWindow.BlockTestingLabel;
            BlockTradingLabel.Content = OsLocalization.MainWindow.BlockTradingLabel;
            ButtonData.Content = OsLocalization.MainWindow.OsDataName;
            ButtonConverter.Content = OsLocalization.MainWindow.OsConverter;
            ButtonTester.Content = OsLocalization.MainWindow.OsTesterName;
            ButtonOptimizer.Content = OsLocalization.MainWindow.OsOptimizerName;

            ButtonRobot.Content = OsLocalization.MainWindow.OsBotStationName;
            ButtonCandleConverter.Content = OsLocalization.MainWindow.OsCandleConverter;

            ButtonTesterLight.Content = OsLocalization.MainWindow.OsTesterLightName;
            ButtonRobotLight.Content = OsLocalization.MainWindow.OsBotStationLightName;

            if (OsLocalization.CurLocalization == OsLocalization.OsLocalType.Ru)
            {
                Height = 415;
                ImageAlor.IsVisible = true;
            }
            else
            {
                Height = 315;
                ImageAlor.IsVisible = false;
                ImageAlor2.IsVisible = false;
            }
        }

        /// <summary>
        /// check the version of dotnet
        /// проверить версию дотНет
        /// </summary>
        private bool CheckDotNetVersion()
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey == null)
                {
                    return false;
                }
                int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));

                if (releaseKey >= 393295)
                {
                    //"4.6 or later";
                    return true;
                }
                if ((releaseKey >= 379893))
                {
                    //"4.5.2 or later";
                    return true;
                }
                if ((releaseKey >= 378675))
                {
                    //"4.5.1 or later";
                    return true;
                }
                if ((releaseKey >= 378389))
                {
                    MessageBox.Show(OsLocalization.MainWindow.Message4);
                    return false;
                }

                MessageBox.Show(OsLocalization.MainWindow.Message4);

                return false;
            }
        }

        private bool CheckOutSomeLibrariesNearby()
        {
            // проверяем чтобы пользователь не запустился с рабочего стола, но не ярлыком, а экзешником

            if (File.Exists("QuikSharp.dll") == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// check the permission of the program to create files in the directory
        /// проверяем разрешение программы создавать файлы в директории
        /// </summary>
        private bool CheckWorkWithDirectory()
        {
            try
            {

                if (!Directory.Exists("Engine"))
                {
                    Directory.CreateDirectory("Engine");
                }

                if (File.Exists("Engine\\checkFile.txt"))
                {
                    File.Delete("Engine\\checkFile.txt");
                }

                File.Create("Engine\\checkFile.txt");

                if (File.Exists("Engine\\checkFile.txt") == false)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }


            return true;
        }

        private bool CheckAlreadyWorkEngine()
        {
            try
            {
                string myDirectory = Directory.GetCurrentDirectory();

                Process[] ps1 = Process.GetProcesses();

                List<Process> process = [];

                for (int i = 0; i < ps1.Length; i++)
                {
                    Process p = ps1[i];

                    try
                    {
                        string mainStr = p.MainWindowHandle.ToString();

                        if (mainStr == "0")
                        {
                            continue;
                        }

                        if (p.MainModule.FileName != ""
                            && p.Modules != null)
                        {
                            process.Add(p);
                        }
                    }
                    catch
                    {

                    }
                }

                int osEngineCount = 0;

                string myProgramPath = myDirectory + "\\OsEngine.exe";

                for (int i = 0; i < process.Count; i++)
                {
                    Process p = process[i];

                    for (int j = 0; p.Modules != null && j < p.Modules.Count; j++)
                    {
                        if (p.Modules[j].FileName == null)
                        {
                            continue;
                        }

                        if (p.Modules[j].FileName.EndsWith(myProgramPath))
                        {
                            osEngineCount++;
                        }
                    }
                }

                if (osEngineCount > 0)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // string message = OsLocalization.MainWindow.Message5 + " THREAD " + e.ExceptionObject;
            //
            // message = _startProgram + "  " + message;
            //
            // message = System.Reflection.Assembly.GetExecutingAssembly() + "\n" + message;
            //
            // _messageToCrashServer = "Crash% " + message;
            // Thread worker = new(SendMessageInCrashServer);
            // worker.Start();
            //
            // if (PrimeSettingsMaster.RebootTradeUiLight == true &&
            //     RobotUiLight.IsRobotUiLightStart)
            // {
            //     Reboot(message);
            // }
            // else
            // {
            //     MessageBox.Show(message);
            // }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            string message = OsLocalization.MainWindow.Message5 + " TASK " + e.Exception.ToString();

            message = _startProgram + "  " + message;

            message = System.Reflection.Assembly.GetExecutingAssembly() + "\n" + message;

            _messageToCrashServer = "Crash% " + message;
            Thread worker = new(SendMessageInCrashServer);
            worker.Start();

            if (PrimeSettingsMaster.RebootTradeUiLight == true
                    // && RobotUiLight.IsRobotUiLightStart
                    )
            {
                Reboot(message);
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        private StartProgram _startProgram;

        private void Reboot(string message)
        {

            if (!CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    Reboot(message);
                });
                return;
            }

            App.app.Shutdown();
            Process process = new();
            process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\OsEngine.exe";
            process.StartInfo.Arguments = " -error " + message;
            process.Start();

            Process.GetCurrentProcess().Kill();
        }

        private void ButtonTesterCandleOne_Click(object sender, RoutedEventArgs e)
        {
            // try
            // {
            //     _startProgram = StartProgram.IsTester;
            //     Hide();
            //     TesterUi candleOneUi = new();
            //     candleOneUi.ShowDialog(this);
            //     Close();
            //     ProccesIsWorked = false;
            //     Thread.Sleep(5000);
            // }
            // catch (Exception error)
            // {
            //     MessageBox.Show(error.ToString());
            // }
            // Process.GetCurrentProcess().Kill();
        }

        private void ButtonTesterLight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _startProgram = StartProgram.IsTester;
                Hide();
                // TesterUiLight candleOneUi = new();
                TerminalLight terminalLight = new();
                terminalLight.Closed += delegate
                {
                    Show();
                };
                terminalLight.Show();

                // candleOneUi.ShowDialog(this);
                // Close();
                // ProccesIsWorked = false;
                // Thread.Sleep(5000);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
            // Process.GetCurrentProcess().Kill();
        }

        private void ButtonRobotCandleOne_Click(object sender, RoutedEventArgs e)
        {
            // try
            // {
            //     _startProgram = StartProgram.IsOsTrader;
            //     Hide();
            //     RobotUi candleOneUi = new();
            //     candleOneUi.ShowDialog(this);
            //     Close();
            //     ProccesIsWorked = false;
            //     Thread.Sleep(5000);
            // }
            // catch (Exception error)
            // {
            //     MessageBox.Show(error.ToString());
            // }
            // Process.GetCurrentProcess().Kill();
        }

        private void ButtonRobotLight_Click(object sender, RoutedEventArgs e)
        {
            // try
            // {
            //     _startProgram = StartProgram.IsOsTrader;
            //     Hide();
            //     RobotUiLight candleOneUi = new();
            //     candleOneUi.ShowDialog(this);
            //     Close();
            //     ProccesIsWorked = false;
            //     Thread.Sleep(5000);
            // }
            // catch (Exception error)
            // {
            //     MessageBox.Show(error.ToString());
            // }
            // Process.GetCurrentProcess().Kill();
        }

        private void ButtonData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _startProgram = StartProgram.IsOsData;
                Hide();
                OsDataView ui = new();
                ui.Closed += (s, e) =>
                {
                    Show();
                    // Thread.Sleep(5000);
                    // Close();
                    // ProccesIsWorked = false;
                    // Process.GetCurrentProcess().Kill();
                };
                ui.Show();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
            // Process.GetCurrentProcess().Kill();
        }

        private void ButtonConverter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _startProgram = StartProgram.IsOsConverter;
                Hide();
                OsConverterView ui = new();
                ui.Closed += (s, e) =>
                {
                    Show();
                    // Thread.Sleep(5000);
                    // Close();
                    // ProccesIsWorked = false;
                    // Process.GetCurrentProcess().Kill();
                };
                ui.Show();
                // ui.ShowDialog(this);
                // Close();
                // ProccesIsWorked = false;
                // Thread.Sleep(10000);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
            // Process.GetCurrentProcess().Kill();
        }

        private void ButtonOptimizer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _startProgram = StartProgram.IsOsOptimizer;
                Hide();
                OptimizerWindow ui = new();
                ui.Closed += (s, e) =>
                {
                    Show();
                };
                ui.Show();
                // Close();
                // ProccesIsWorked = false;
                // Thread.Sleep(10000);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
            // Process.GetCurrentProcess().Kill();
        }

        private async void ThreadAreaGreeting()
        {
            await Task.Delay(1000);
            double angle = 5;

            for (int i = 0; i < 7; i++)
            {
                RotatePic(angle);
                await Task.Delay(50);
                angle += 10;
            }

            for (int i = 0; i < 7; i++)
            {
                RotatePic(angle);
                await Task.Delay(100);
                angle += 10;
            }

            await Task.Delay(100);
            RotatePic(angle);

        }

        private void RotatePic(double angle)
        {
            // if (ImageGear.Dispatcher.CheckAccess() == false)
            // {
            //     ImageGear.Dispatcher.Invoke(new Action<double>(RotatePic), angle);
            //     return;
            // }
            //
            // ImageGear.RenderTransform = new RotateTransform(angle, 12, 12);

        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsUi == null)
            {
                _settingsUi = new SettingsWindow();
                _settingsUi.Show();
                _settingsUi.Closing += delegate { _settingsUi = null; };
            }
            else
            {
                _settingsUi.Activate();
            }
        }

        private SettingsWindow _settingsUi;

        private void CandleConverter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                OsCandleConverterView ui = new();
                ui.Closed += (s, e) =>
                {
                    Show();
                    // Thread.Sleep(10000);
                    // Close();
                    // ProccesIsWorked = false;
                    // Process.GetCurrentProcess().Kill();
                };
                ui.Show();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void CommandLineInterfaceProcess()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.Exists(args, a => a.Equals("-robots")))
            {
                ButtonRobotCandleOne_Click(this, default);
            }
            else if (Array.Exists(args, a => a.Equals("-tester")))
            {
                ButtonTesterCandleOne_Click(this, default);
            }
            else if (Array.Exists(args, a => a.Equals("-robotslight")))
            {
                ButtonRobotLight_Click(this, default);
            }
            else if (Array.Exists(args, a => a.Equals("-error"))
                    // && PrimeSettingsMaster.RebootTradeUiLight
                    )
            {

                CriticalErrorHandler.ErrorInStartUp = true;

                Array.ForEach(args, (a) => { CriticalErrorHandler.ErrorMessage += a; });

                new Task(() =>
                {
                    string messageError = string.Empty;

                    for (int i = 0; i < args.Length; i++)
                    {
                        messageError += args[i];
                    }

                    MessageBox.Show(messageError);

                }).Start();

                ButtonRobotLight_Click(this, default);
            }
        }

        private void ClearOptimizerWorkResults()
        {
            try
            {
                if (Directory.Exists("Engine") == false)
                {
                    return;
                }

                string[] files = Directory.GetFiles("Engine");

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        if (files[i].Contains(" OpT "))
                        {
                            File.Delete(files[i]);
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                }
            }
            catch
            {
                // ignore
            }
        }

        string _messageToCrashServer;

        private void SendMessageInCrashServer()
        {
            try
            {
                // if (PrimeSettingsMaster.ReportCriticalErrors == false)
                // {
                //     return;
                // }

                return;
                TcpClient newClient = new();
                newClient.Connect("195.133.196.183", 11000);
                NetworkStream tcpStream = newClient.GetStream();
                byte[] sendBytes = Encoding.UTF8.GetBytes(_messageToCrashServer);
                tcpStream.Write(sendBytes, 0, sendBytes.Length);
                newClient.Close();
            }
            catch
            {
                // ignore
            }
        }
    }

    public static class CriticalErrorHandler
    {
        public static string ErrorMessage = string.Empty;

        public static bool ErrorInStartUp = false;
    }

}
