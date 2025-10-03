using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace OsEngine
{
    public class MessageBox
    {
        public static void Show(string str)
        {
            MessageBoxManager
                .GetMessageBoxStandard("Message", str)
                .ShowAsync();
        }

        public static async Task<bool> ConfirmDialog(string str, Window win, string title = "Confirmation window")
        {
            var result = await MessageBoxManager
                .GetMessageBoxStandard(title, str, ButtonEnum.YesNo)
                .ShowWindowDialogAsync(win);
            return result.Equals(ButtonResult.Yes);
        }
    }
}

