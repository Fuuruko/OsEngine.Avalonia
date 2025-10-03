/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Media;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OsEngine.Language;
using OsEngine.Models.Logging;
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
            Focus();

            ButtonClear.Content = OsLocalization.Logging.ButtonClearExtraLog;
        }

        public async static void ShowErrorLog()
        {
            if (PrimeSettingsMaster.ErrorLogBeepIsActive) { Console.Beep(); }

            if (!PrimeSettingsMaster.ErrorLogMessageBoxIsActive) { return; }

            if (LastShow.AddSeconds(1) < DateTime.Now)
            {
                LastShow = DateTime.Now;
                if (_instance == null)
                {
                    _instance = new();
                    _instance.Closing += delegate { _instance = null; };
                    _instance.Show();
                }
                // _instance.Show();
                // TODO: Ask community.
                // Without it may not focus on ErrorLog
                await Task.Delay(20);
                _instance.Activate();
                _instance.Focus();
                Console.WriteLine(_instance.IsActive);
            }
        }
    }
}
