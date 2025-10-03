using Avalonia.Controls;
using Avalonia.Interactivity;
using OsEngine.Language;

namespace OsEngine.Views.Utils
{
    public partial class AcceptDialogView : Window
    {
        public string Msg { set { Message.Text = value; } }

        /// <summary>
        /// window designer
        /// </summary>
        /// <param name="text">text that will be displayed as the main message to the user. What he will have to approve/текст который будет выведен в качестве основного сообщения пользователю. То что он должен будет одобрить</param>
        public AcceptDialogView(string text)
        {
            InitializeComponent();
            Message.Text = text;
            ButtonCancel.Content = OsLocalization.Entity.ButtonCancel1;

            Title = OsLocalization.Entity.TitleAcceptDialog;
            ButtonAccept.Content = OsLocalization.Entity.ButtonAccept;

            Activate();
            Focus();
        }

        public AcceptDialogView() : this("Default Text") {  }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e){
            Close(false);
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e){
            Close(true);}
    }
}
