using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Terminal;

public partial class Journal
{
    private static readonly Task task = Task.Run(WatcherHome);

    public static async void WatcherHome()
    {

        while (true)
        {
            try
            {
                foreach (Journal journal in _journals)
                {

                    // controller.SavePositions();
                    // controller.TryPaintPositions();
                    // controller.TrySaveStopLimits();
                }

                // if (!MainWindow.ProccesIsWorked)
                // {
                //     return;
                // }
            }
            catch
            {
                // ignore
            }
            await Task.Delay(1000);
        }
    }

    public void Save() => _needToSave = true;

    private void Load()
    {
        if (_startProgram == StartProgram.IsOsOptimizer
                || _startProgram == StartProgram.IsTester)
        {
            return;
        }

        string path = $@"Engine\{Name}DealController.txt";
        if (!File.Exists(path)) { return; }

        try
        {
            // 1 count the number of transactions in the file
            //1 считаем кол-во сделок в файле

            List<string> deals = [];

            using (StreamReader reader = new(path))
            {
                try
                {
                    Enum.TryParse(reader.ReadLine(), out CommissionType commissionType);
                    Commission = new()
                    {
                        Value = reader.ReadLine().ToDecimal(),
                        Type = commissionType,
                    };
                }
                catch
                {
                    // ignore
                }

                while (!reader.EndOfStream)
                {
                    deals.Add(reader.ReadLine());
                }
            }

            if (deals.Count == 0
                    || _startProgram == StartProgram.IsTester)
            {
                return;
            }

            List<Position> positions = [];

            int i = 0;
            foreach (string deal in deals)
            {
                try
                {
                    positions.Add(new Position());
                    positions[i].SetDealFromString(deal);
                    UpdateOpenPositionArray(positions[i]);
                }
                catch (Exception error)
                {
                    SendNewLogMessage("ERROR on loading position " + error.ToString(), LogMessageType.Error);
                    positions.Remove(positions[i]);
                    i--;
                }

                i++;
            }

            _deals = new ObservableCollection<Position>(positions);
            OpenPositions = new ObservableCollection<Position>(_deals.Where(p =>
                    p.State != PositionStateType.Done
                    && p.State != PositionStateType.OpeningFail));
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    public void Delete()
    {
        try
        {
            if (_startProgram == StartProgram.IsOsOptimizer 
                    || _startProgram == StartProgram.IsTester)
            {
                return;
            }

            _needToSave = false;
            string dealControllerPath = $@"Engine\{Name}DealController.txt";

            if (File.Exists(dealControllerPath))
            {
                try
                {
                    File.Delete(dealControllerPath);
                }
                catch (Exception error)
                {
                    SendNewLogMessage(error.ToString(), LogMessageType.System);
                }
            }

            string dealControllerStopLimitsPath = $@"Engine\{Name}DealControllerStopLimits.txt";

            if (File.Exists(dealControllerStopLimitsPath))
            {
                try
                {
                    File.Delete(dealControllerStopLimitsPath);
                }
                catch (Exception error)
                {
                    SendNewLogMessage(error.ToString(), LogMessageType.System);
                }
            }

            if (_startProgram != StartProgram.IsOsOptimizer)
            {
                _journals.Remove(this);
            }
        }
        catch (Exception error)
        {
            SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }

        PositionStateChanged -= OnPositionStateChanged;
        PositionNetVolumeChanged -= OnPositionNetVolumeChanged;
        UserSelectedAction -= OnUserSelectedAction;
        LogMessageEvent -= SendNewLogMessage;
    }

    public void Clear()
    {
        AllPositions.Clear();
        OpenPositions.Clear();
        OpenLongPositions.Clear();
        OpenShortPositions.Clear();
        // _openLongChanged = true;
        // _openShortChanged = true;
        // _closePositionChanged = true;
        // _closeShortChanged = true;
        // _closeLongChanged = true;
        _needToSave = true;
    }
}
