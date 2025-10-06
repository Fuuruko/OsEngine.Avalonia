/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using Avalonia.Controls;
using OsEngine.Language;
using OsEngine.Models.Utils;

namespace OsEngine.Views.Logging
{
    /// <summary>
    /// Interaction logic for LogErrorUi.xaml
    /// Логика взаимодействия для LogErrorUi.xaml
    /// </summary>
    public partial class LogErrorUi : Window
    {
        private static DateTime LastShow = DateTime.MinValue;
        private static LogErrorUi _instance { get; set; }
        // public LogErrorUi(DataGridView gridErrorLog)
        public LogErrorUi()
        {
            InitializeComponent();
            // HostLog.Child = gridErrorLog;
            Title = OsLocalization.Logging.TitleExtraLog;
            // Title = Title + " " + OsEngine.PrimeSettings.PrimeSettingsMaster.LabelInHeaderBotStation;

            Activate();

            ButtonClear.Content = OsLocalization.Logging.ButtonClearExtraLog;
        }

        public static void ShowErrorLog()
        {
            if (PrimeSettingsMaster.ErrorLogBeepIsActive) { Console.Beep(); }

            if (!PrimeSettingsMaster.ErrorLogMessageBoxIsActive) { return; }

            if (LastShow.AddSeconds(1) < DateTime.Now)
            {
                LastShow = DateTime.Now;
                if (_instance != null)
                {
                    _instance.Activate();
                    return;
                }

                _instance = new();
                _instance.Closing += delegate { _instance = null; };
                _instance.Show();
            }
        }
    }
}
