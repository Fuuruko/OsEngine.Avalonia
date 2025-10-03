using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using OsEngine.Language;
using OsEngine.Models.Logging;
using OsEngine.Models.Optimizer;

namespace OsEngine.ViewModels.Optimizer;

public partial class OptimizerViewModel : BaseViewModel
{
    public CrossValidationViewModel WalkForwardPeriodsView;
    private DateTime startTime = DateTime.Now;
    private DateTime endTime = DateTime.Now;
    private bool lastInSample;
    private int iterationCount;
    private decimal percentOnFiltration;

    public DateTime StartTime
    {
        get => startTime;
        set
        {
            SetProperty(ref startTime, value);
            UpdateBotsNumberCommand();
            CreatePhazesCommand();
            // if (DateTimeStartEndChange != null)
            // {
            //     DateTimeStartEndChange();
            // }
        }
    }
    public DateTime EndTime
    {
        get => endTime;
        set
        {
            SetProperty(ref endTime, value);
            UpdateBotsNumberCommand();
            CreatePhazesCommand();
            // if (DateTimeStartEndChange != null)
            // {
            //     DateTimeStartEndChange();
            // }
        }
    }

    public bool LastInSample
    {
        get => lastInSample;
        set
        {
            SetProperty(ref lastInSample, value);
            UpdateBotsNumberCommand();
            CreatePhazesCommand();
        }
    }

    public int IterationCount
    {
        get => iterationCount;
        set
        {
            SetProperty(ref iterationCount, value);
            UpdateBotsNumberCommand();
            CreatePhazesCommand();
        }
    }

    public decimal PercentOnFiltration
    {
        get => percentOnFiltration;
        set
        {
            SetProperty(ref percentOnFiltration, value);
            CreatePhazesCommand();
        }
    }

    public ObservableCollection<OptimizerPhaze> Phazes { get; } = [];


    public ISeries[] Series { get; set; } = [

        // new RowSeries<int>
        // {
        //     Values = [8, -3, 4],
        //     Stroke = null,
        //     DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
        //     DataLabelsSize = 14,
        //     DataLabelsPosition = DataLabelsPosition.End
        // },
        // new RowSeries<int>
        // {
        //     Values = [4, -6, 5],
        //     Stroke = null,
        //     DataLabelsPaint = new SolidColorPaint(new SKColor(250, 250, 250)),
        //     DataLabelsSize = 14,
        //     DataLabelsPosition = DataLabelsPosition.Middle
        // },
        // new RowSeries<int>
        // {
        //     Values = [6, -9, 3],
        //     Stroke = null,
        //     DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
        //     DataLabelsSize = 14,
        //     DataLabelsPosition = DataLabelsPosition.Start
        // }
    ];

    public void CreatePhazesCommand()
    {
        ReloadPhazes();

        if (Phazes.Count == 0) { return; }

        // WalkForwardPeriodsPainter.PaintForwards(HostWalkForwardPeriods, Fazes);
        //
        // PaintCountBotsInOptimization();
    }

    private void ReloadPhazes()
    {
        int phazeCount = IterationCount;

        if (phazeCount < 1)
        {
            phazeCount = 1;
        }

        if (EndTime == DateTime.MinValue ||
                StartTime == DateTime.MinValue)
        {
            SendLogMessage(OsLocalization.Optimizer.Message12, LogMessageType.System);
            return;
        }

        int dayAll = Convert.ToInt32((EndTime - StartTime).TotalDays);

        if (dayAll < 2)
        {
            SendLogMessage(OsLocalization.Optimizer.Message12, LogMessageType.System);
            return;
        }

        (int daysOnInSample, int daysOnForward) = GetInSample(phazeCount, dayAll);

        Phazes.Clear();

        DateTime time = StartTime;

        for (int i = 0; i < phazeCount; i++)
        {
            OptimizerPhaze newFaze = new()
            {
                Num = 2 * i + 1,
                Type = OptimizerPhazeType.InSample,
                StartTime = time,
                EndTime = time.AddDays(daysOnInSample),
                Days = daysOnInSample
            };
            time = time.AddDays(daysOnForward);
            Phazes.Add(newFaze);

            if (LastInSample && i + 1 == phazeCount)
            {
                newFaze.Days = daysOnInSample;
                break;
            }

            OptimizerPhaze newFazeOut = new()
            {
                Num = 2 * i + 2,
                Type = OptimizerPhazeType.OutOfSample,
                StartTime = newFaze.StartTime.AddDays(daysOnInSample)
            };
            newFazeOut.EndTime = newFazeOut.StartTime.AddDays(daysOnForward);
            newFazeOut.StartTime = newFazeOut.StartTime.AddDays(1);
            newFazeOut.Days = daysOnForward;
            Phazes.Add(newFazeOut);
        }

        foreach (OptimizerPhaze phaze in Phazes)
        {
            if (phaze.Days <= 0)
            {
                SendLogMessage(OsLocalization.Optimizer.Label50, LogMessageType.Error);
                Phazes.Clear();
                return;
            }
        }

        /*while (DaysInFazes(Fazes) != dayAll)
          {
          int daysGone = DaysInFazes(Fazes) - dayAll;

          for (int i = 0; i < Fazes.Count && daysGone != 0; i++)
          {

          if (daysGone > 0)
          {
          Fazes[i].Days--;
          if (Fazes[i].TypeFaze == OptimizerFazeType.InSample &&
          i + 1 != Fazes.Count)
          {
          Fazes[i + 1].TimeStart = Fazes[i + 1].TimeStart.AddDays(-1);
          }
          else
          {
          Fazes[i].TimeStart = Fazes[i].TimeStart.AddDays(-1);
          }
          daysGone--;
          }
          else if (daysGone < 0)
          {
          Fazes[i].Days++;
          if (Fazes[i].TypeFaze == OptimizerFazeType.InSample && 
          i + 1 != Fazes.Count)
          {
          Fazes[i + 1].TimeStart = Fazes[i + 1].TimeStart.AddDays(+1);
          }
          else
          {
          Fazes[i].TimeStart = Fazes[i].TimeStart.AddDays(+1);
          }
          daysGone++;
          }
          }
          }*/
    }

    private (int, int) GetInSample(int fazeCount, int allDays)
    {
        // х = Y + Y/P * С;
        // x - общая длинна в днях. Уже известна
        // Y - длинна InSample
        // P - процент OutOfSample от InSample
        // C - количество отрезков

        if (LastInSample) { fazeCount--; }

        decimal smth = 1 + fazeCount * PercentOnFiltration / 100;

        decimal curLengthInSample = Math
            .Round(allDays / smth, MidpointRounding.AwayFromZero);

        decimal outDays = Math.Round(curLengthInSample * PercentOnFiltration / 100);

        if (curLengthInSample + fazeCount * outDays > allDays)
        {
            outDays--;
        }
        return ((int)curLengthInSample, (int)outDays);
    }
}
