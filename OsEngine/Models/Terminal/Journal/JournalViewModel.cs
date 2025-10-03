using System;
using System.Collections.ObjectModel;
using OsEngine.Models.Terminal;

namespace OsEngine.ViewModels.Terminal;

public class JournalViewModel : BaseViewModel
{
    public ObservableCollection<Journal> Journals { get; set; } = [];

    public decimal BotMultiplier { get; set; } = 100m;

    public void OpenPosition()
    {
        // try
        // {
        //     if (UserSelectActionEvent != null)
        //     {
        //         UserSelectActionEvent(null, SignalType.OpenNew);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SendNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }

    private void CloseAllPositions(object sender, EventArgs e)
    {
        // try
        // {
        //     if (OpenPositions == null)
        //     {
        //         return;
        //     }
        //
        //     AcceptDialogUi ui = new AcceptDialogUi(OsLocalization.Journal.Message5);
        //     ui.ShowDialog();
        //
        //     if (ui.UserAcceptAction == false)
        //     {
        //         return;
        //     }
        //
        //     if (UserSelectActionEvent != null)
        //     {
        //         UserSelectActionEvent(null, SignalType.CloseAll);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SendNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }

    private void PositionCloseForNumber_Click(object sender, EventArgs e)
    {
        // try
        // {
        //     int number;
        //     try
        //     {
        //         if (_gridOpenDeal.Rows == null ||
        //                 _gridOpenDeal.Rows.Count == 0 ||
        //                 _gridOpenDeal.CurrentCell == null)
        //         {
        //             return;
        //         }
        //         number = Convert.ToInt32(_gridOpenDeal.Rows[_gridOpenDeal.CurrentCell.RowIndex].Cells[0].Value);
        //     }
        //     catch (Exception)
        //     {
        //         return;
        //     }
        //
        //
        //     if (UserSelectActionEvent != null)
        //     {
        //         UserSelectActionEvent(GetPositionForNumber(number), SignalType.CloseOne);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SendNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }

    private void PositionNewStop_Click(object sender, EventArgs e)
    {
        // try
        // {
        //     int number;
        //     try
        //     {
        //         if (_gridOpenDeal.Rows.Count == 0)
        //         {
        //             return;
        //         }
        //
        //         number = Convert.ToInt32(_gridOpenDeal.Rows[_gridOpenDeal.CurrentCell.RowIndex].Cells[0].Value);
        //     }
        //     catch (Exception)
        //     {
        //         return;
        //     }
        //
        //     if (UserSelectActionEvent != null)
        //     {
        //         UserSelectActionEvent(GetPositionForNumber(number), SignalType.ReloadStop);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SendNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }

    private void PositionNewProfit_Click(object sender, EventArgs e)
    {
        // try
        // {
        //     int number;
        //     try
        //     {
        //         if (_gridOpenDeal.Rows.Count == 0)
        //         {
        //             return;
        //         }
        //
        //         number = Convert.ToInt32(_gridOpenDeal.Rows[_gridOpenDeal.CurrentCell.RowIndex].Cells[0].Value);
        //     }
        //     catch (Exception)
        //     {
        //         return;
        //     }
        //
        //     if (UserSelectActionEvent != null)
        //     {
        //         UserSelectActionEvent(GetPositionForNumber(number), SignalType.ReloadProfit);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SendNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }

    private void PositionClearDelete_Click(object sender, EventArgs e)
    {
        // try
        // {
        //     AcceptDialogUi ui = new AcceptDialogUi(OsLocalization.Journal.Message3);
        //     ui.ShowDialog();
        //
        //     if (ui.UserAcceptAction == false)
        //     {
        //         return;
        //     }
        //
        //     int number;
        //     try
        //     {
        //         if (_gridOpenDeal.Rows.Count == 0)
        //         {
        //             return;
        //         }
        //
        //         number = Convert.ToInt32(_gridOpenDeal.Rows[_gridOpenDeal.CurrentCell.RowIndex].Cells[0].Value);
        //     }
        //     catch (Exception)
        //     {
        //         return;
        //     }
        //
        //     if (UserSelectActionEvent != null)
        //     {
        //         UserSelectActionEvent(GetPositionForNumber(number), SignalType.DeletePos);
        //     }
        // }
        // catch (Exception error)
        // {
        //     SendNewLogMessage(error.ToString(), LogMessageType.Error);
        // }
    }
}
