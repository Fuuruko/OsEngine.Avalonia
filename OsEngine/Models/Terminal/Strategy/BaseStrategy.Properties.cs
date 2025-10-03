using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Terminal;

public abstract partial class BaseStrategy
{
    private List<IBot> _botTabs;

    public List<Journal> GetJournals()
    {
        List<Journal> journals = [];

        for (int i = 0; _botTabs != null && i < _botTabs.Count; i++)
        {
            // FIX:
            // if (_botTabs[i].TabType == BotTabType.Simple)
            // {
            //     journals.Add(((BotTabSimple)_botTabs[i]).GetJournal());
            // }
            // else if (_botTabs[i].TabType == BotTabType.Screener)
            // {
            //     List<Journal> journalsOnTab = ((BotTabScreener)_botTabs[i]).GetJournals();
            //
            //     if (journalsOnTab == null ||
            //             journalsOnTab.Count == 0)
            //     {
            //         continue;
            //     }
            //
            //     journals.AddRange(journalsOnTab);
            // }
            // else if (_botTabs[i].TabType == BotTabType.Pair)
            // {
            //     List<Journal> journalsOnTab = ((BotTabPair)_botTabs[i]).GetJournals();
            //
            //     if (journalsOnTab == null ||
            //             journalsOnTab.Count == 0)
            //     {
            //         continue;
            //     }
            //
            //     journals.AddRange(journalsOnTab);
            // }
            // else if (_botTabs[i].TabType == BotTabType.Polygon)
            // {
            //     List<Journal> journalsOnTab = ((BotTabPolygon)_botTabs[i]).GetJournals();
            //
            //     if (journalsOnTab == null ||
            //             journalsOnTab.Count == 0)
            //     {
            //         continue;
            //     }
            //
            //     journals.AddRange(journalsOnTab);
            // }
        }

        return journals;
    }

    /// <summary>
    /// total profit
    /// </summary>
    public decimal TotalProfitInPercent
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            decimal result = 0;

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i].AllPosition == null ||
                        journals[i].AllPosition.Count == 0)
                {
                    continue;
                }

                // List<Position> positions = journals[i].AllPosition.FindAll(
                //             position => position.State != PositionStateType.OpeningFail
                //             && position.EntryPrice != 0 && position.ClosePrice != 0);
                //
                // result += PositionStatisticGenerator.GetAllProfitPercent(positions.ToArray(), journals[i].PositionMultiplier);
            }
            return result;
        }
    }

    /// <summary>
    /// total profit absolute
    /// </summary>
    public decimal TotalProfitAbs
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            decimal result = 0;

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i].AllPosition == null ||
                        journals[i].AllPosition.Count == 0)
                {
                    continue;
                }

                // List<Position> positions = journals[i].AllPosition.FindAll(
                //             position => position.State != PositionStateType.OpeningFail
                //             && position.EntryPrice != 0 && position.ClosePrice != 0);
                //
                // result += PositionStatisticGenerator.GetAllProfitInAbsolute(positions.ToArray(), journals[i].PositionMultiplier);
            }
            return result;
        }
    }

    /// <summary>
    /// average profit from the transaction
    /// </summary>
    public decimal MiddleProfitInPercent
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            decimal result = 0;

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i].AllPosition == null ||
                        journals[i].AllPosition.Count == 0)
                {
                    continue;
                }

                // List<Position> positions = journals[i].AllPosition.FindAll(
                //             position => position.State != PositionStateType.OpeningFail
                //             && position.EntryPrice != 0 && position.ClosePrice != 0);
                //
                // result += PositionStatisticGenerator.GetMiddleProfitInPercentOneContract(positions.ToArray());
            }
            return result;
        }
    }

    /// <summary>
    /// profit factor
    /// </summary>
    public decimal ProfitFactor
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            decimal result = 0;

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i].AllPosition == null ||
                        journals[i].AllPosition.Count == 0)
                {
                    continue;
                }
                // result += PositionStatisticGenerator.GetProfitFactor(journals[i].AllPosition.ToArray(), journals[i].PositionMultiplier);
            }
            return result;
        }
    }

    /// <summary>
    /// maximum drawdown
    /// </summary>
    public decimal MaxDrawDown
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            decimal result = 0;

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i].AllPosition == null ||
                        journals[i].AllPosition.Count == 0)
                {
                    continue;
                }
                // result += PositionStatisticGenerator.GetMaxDownPercent(journals[i].AllPosition.ToArray());
            }
            return result;
        }
    }

    /// <summary>
    /// profit position count
    /// </summary>
    public decimal WinPositionPercent
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            decimal winPoses = 0;

            decimal allPoses = 0;

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i].AllPosition == null ||
                        journals[i].AllPosition.Count == 0)
                {
                    continue;
                }

                allPoses += journals[i].AllPosition.Count;
                // List<Position> winPositions = journals[i].AllPosition.FindAll(pos => pos.ProfitOperationAbs > 0);
                // winPoses += winPositions.Count;
            }
            return winPoses / allPoses;
        }
    }

    /// <summary>
    /// the number of positions at the tabs of the robot
    /// </summary>
    public int PositionsCount
    {
        get
        {

            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return 0;
            }

            List<Position> pos = [];

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i] == null)
                {
                    continue;
                }
                if (journals[i].OpenPositions == null ||
                        journals[i].OpenPositions.Count == 0)
                {
                    continue;
                }
                pos.AddRange(journals[i].OpenPositions);
            }
            return pos.Count;
        }
    }

    /// <summary>
    /// the number of all positions at the tabs of the robot
    /// </summary>
    public int AllPositionsCount
    {
        get
        {
            List<Journal> journals = GetJournals();

            if (journals == null || journals.Count == 0)
            {
                return 0;
            }

            List<Position> pos = [];

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i] == null)
                {
                    continue;
                }
                if (journals[i].AllPosition == null || journals[i].AllPosition.Count == 0)
                {
                    continue;
                }

                List<Position> allPositionOpen = [];

                for(int i2 = 0;i2 < journals[i].AllPosition.Count;i2++)
                {
                    if (journals[i].AllPosition[i2].State == PositionStateType.OpeningFail)
                    {
                        continue;
                    }
                    allPositionOpen.Add(journals[i].AllPosition[i2]);
                }

                if (allPositionOpen == null || allPositionOpen.Count == 0)
                {
                    continue;
                }

                pos.AddRange(allPositionOpen);
            }
            return pos.Count;
        }
    }

    /// <summary>
    /// open positions on robot sources
    /// </summary>
    public List<Position> OpenPositions
    {
        get
        {
            List<Position> result = [];

            List<Journal> journals = GetJournals();

            if (journals == null ||
                    journals.Count == 0)
            {
                return result;
            }

            for (int i = 0; i < journals.Count; i++)
            {
                if (journals[i] == null)
                {
                    continue;
                }
                if (journals[i].OpenPositions == null ||
                        journals[i].OpenPositions.Count == 0)
                {
                    continue;
                }
                result.AddRange(journals[i].OpenPositions);
            }
            return result;
        }
    }
}
