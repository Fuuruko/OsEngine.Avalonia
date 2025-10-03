/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Terminal
{
    public class PositionStatisticGenerator
    {
        public static List<string> GetStatisticNew(List<Position> positions)
        {
            if (positions == null)
            {
                return null;
            }

            List<Position> positionsNew = positions.FindAll(
                position => position.State != PositionStateType.OpeningFail);

            if (positionsNew.Count == 0)
            {
                return null;
            }

            Position[] deals = [.. positionsNew];

            List<string> report =
            [
                /*
                 Чистый П\У
                 Чистый П\У %
                 Количество сделок
                 Среднее время удержания
                 Шарп

                 Сред. П\У по движению
                 Сред. П\У % по движению
                 Сред. П\У на капитал
                 Сред. П\У % на капитал

                 Прибыльных  сделок
                 Прибыльных %
                 Сред. П\У по сделке
                 Сред. П\У % по сделке
                 Сред. П\У на капитал
                 Сред. П\У % на капитал
                 Максимум подряд

                 Убыточных сделок
                 Убыточных  %
                 Сред. П\У по сделке
                 Сред. П\У % по сделке
                 Сред. П\У на капитал
                 Сред. П\У % на капитал
                 Максимум подряд

                 Макс просадка %
                 Размер комиссии
            */
                // Convert.ToDouble(GetAllProfitInAbsolute(deals)).ToString(new CultureInfo("ru-RU")), //Net profit
                // Math.Round(GetAllProfitPercent(deals), 6).ToString(new CultureInfo("ru-RU")),//Net profti %
                // deals.Length.ToString(new CultureInfo("ru-RU")),// Number of transactions
                // GetAverageTimeOnPoses(deals),
                // GetSharpRatio(deals, 7).ToString(),
                // Math.Round(GetProfitFactor(deals), 6).ToString(new CultureInfo("ru-RU")),   //Profit Factor
                // Math.Round(GetRecovery(deals), 6).ToString(new CultureInfo("ru-RU")),   // Recovery
                // "",
                // Convert.ToDouble(GetMiddleProfitInAbsolute(deals)).ToString(new CultureInfo("ru-RU")), //average profit in 1 contract
                // Math.Round(GetMiddleProfitInPercentOneContract(deals), 6).ToString(new CultureInfo("ru-RU")), //average profit in % 1 contract
                // Convert.ToDouble(GetMiddleProfitInAbsoluteToDeposit(deals)).ToString(new CultureInfo("ru-RU")), //average profit
                // Math.Round(GetMiddleProfitInPercentToDeposit(deals), 6).ToString(new CultureInfo("ru-RU")), //average profit in %
                // "", // 11
                // GetProfitDial(deals).ToString(new CultureInfo("ru-RU")), //wining trades/выигрышных сделок
                // Math.Round(GetProfitDialPercent(deals), 6).ToString(new CultureInfo("ru-RU")),//wining trade in %/выигрышных сделок в %
                // //report += Convert.ToDouble(GetAllProfitInProfitInPunkt(deals)).ToString(new CultureInfo("ru-RU")) + "\r\n"; //total profit margins/общий профит выигрышных сделок
                // Convert.ToDouble(GetAllMiddleProfitInProfitInAbsolute(deals)).ToString(new CultureInfo("ru-RU")), //Average profit in winning trades/средний профит в выигрышных сделках
                // Math.Round(GetAllMiddleProfitInProfitInPercent(deals), 6).ToString(new CultureInfo("ru-RU")), //Average profit as a percentage of winning trades/средний профит в процентах в выигрышных сделках
                // Convert.ToDouble(GetAllMiddleProfitInProfitInAbsoluteOnDeposit(deals)).ToString(new CultureInfo("ru-RU")), //Average profit in winning trades/средний профит в выигрышных сделках
                // Math.Round(GetAllMiddleProfitInProfitInPercentOnDeposit(deals), 6).ToString(new CultureInfo("ru-RU")),//Average profit as a percentage of winning trades/средний профит в процентах в выигрышных сделках
                // GetMaxProfitSeries(deals).ToString(new CultureInfo("ru-RU")), //maximum series of winning trades/максимальная серия выигрышных сделок
                // "",
                // GetLossDial(deals).ToString(new CultureInfo("ru-RU")), //losing trades/проигрышных сделок
                // Math.Round(GetLossDialPercent(deals), 6).ToString(new CultureInfo("ru-RU")), //losing deals in/проигрышных сделок в %
                // //report += Convert.ToDouble(GetAllLossInLossInPunkt(deals)).ToString(new CultureInfo("ru-RU")) + "\r\n"; //loss-making total profit/общий профит проигрышных сделок
                // Convert.ToDouble(GetAllMiddleLossInLossInAbsolute(deals)).ToString(new CultureInfo("ru-RU")),//average profit in losing trades/средний профит в проигрышных сделках
                // Math.Round(GetAllMiddleLossInLossInPercent(deals), 6).ToString(new CultureInfo("ru-RU")),//Average profit as a percentage in losing trades/средний профит в процентах в проигрышных сделках
                // Convert.ToDouble(GetAllMiddleLossInLossInAbsoluteOnDeposit(deals)).ToString(new CultureInfo("ru-RU")), //Average profit in winning trades/средний профит в выигрышных сделках
                // Math.Round(GetAllMiddleLossInLossInPercentOnDeposit(deals), 6).ToString(new CultureInfo("ru-RU")),//Average profit as a percentage of winning trades/средний профит в процентах в выигрышных сделках
                // GetMaxLossSeries(deals).ToString(new CultureInfo("ru-RU")),//maximum series of winning trades/максимальная серия выигрышных сделок
                // "",
                // Math.Round(GetMaxDownPercent(deals), 6).ToString(new CultureInfo("ru-RU")), //maximum drawdown in percent/максимальная просадка в процентах
                // Math.Round(GetCommissionAmount(deals), 6).ToString(new CultureInfo("ru-RU")), //maximum drawdown in percent/максимальная просадка в процентах
            ];

            /*report += Math.Round(GetSharp(), 2).ToString(new CultureInfo("ru-RU"));
            */
            return report;
        }

        #region Profit

        public static decimal GetAllProfitInAbsolute(Position[] deals, decimal multiplier)
        {
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                profit += deals[i].ProfitPortfolioAbs;
            }
            profit *= multiplier / 100;

            return Round(profit);
        }

        public static decimal GetAllProfitPercent(Position[] deals, decimal multiplier)
        {
            if (deals == null || deals.Length == 0)
            {
                return 0;
            }

            decimal start = 0;
            int i = 0;
            while (start == 0)
            {
                start = deals[i].PortfolioValueOnOpenPosition;
                i++;
                if (i >= deals.Length)
                    break;
            }

            if (start == 0)
            {
                return 0;
            }

            decimal end = start + GetAllProfitInAbsolute(deals, multiplier);

            decimal profit = end / start * 100 - 100;
            return profit;
        }

        public static decimal GetMiddleProfitInPercentOneContract(Position[] deals)
        {
            if (deals.Length == 0)
            {
                return 0;
            }

            decimal profit = 0;

            int divider = deals.Length;

            for (int i = 0; i < deals.Length; i++)
            {
                decimal enter = deals[i].EntryPrice;
                decimal exit = deals[i].ClosePrice;

                if (exit == 0)
                {
                    divider--;
                    continue;
                }

                if (enter == 0) continue;

                decimal value = exit / enter * 100 - 100;

                if (deals[i].Direction == Side.Buy)
                {
                    profit += value;
                }
                else if (deals[i].Direction == Side.Sell)
                {
                    profit -= value;
                }
            }

            if (divider <= 0)
            {
                return 0;
            }

            return Round(profit / divider);
        }

        public static decimal GetMiddleProfitInAbsolute(Position[] deals, decimal multiplier)
        {
            if (deals.Length == 0)
            {
                return 0;
            }
            decimal profit = 0;
            decimal someProfit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                decimal curProfit = deals[i].ProfitOperationAbs;

                profit += curProfit;

                if (curProfit != 0)
                {
                    someProfit = curProfit;
                }
            }
            profit *= multiplier / 100;
            someProfit *= multiplier / 100;

            if (Convert.ToInt32(someProfit) == someProfit)
            {
                return Math.Round(profit / deals.Length, 6);
            }

            try
            {
                return Math.Round(profit / deals.Length, 6);
            }
            catch (Exception)
            {
                return Math.Round(profit, 6);
            }
        }

        private static decimal GetMiddleProfitInAbsoluteToDeposit(Position[] deals, decimal multiplier)
        {
            if (deals.Length == 0)
            {
                return 0;
            }
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                profit += deals[i].ProfitPortfolioAbs;
            }
            profit *= multiplier / 100;

            return Math.Round(profit / deals.Length, 6);
        }

        private static decimal GetMiddleProfitInPercentToDeposit(Position[] deals, decimal multiplier)
        {
            if (deals.Length == 0)
            {
                return 0;
            }
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                profit += deals[i].ProfitPortfolioPercent;
            }
            profit *= multiplier / 100;

            return Math.Round(profit / deals.Length, 6);
        }

        // helper method to calculate the average return from individual trades
        private static decimal GetAverageReturn(Position[] deals, decimal multiplier)
        {
            if (deals == null || deals.Length == 0)
                return 0;

            decimal totalReturn = 0;

            foreach (Position deal in deals)
            {
                // 1. Get the portfolio return percentage for the trade
                decimal tradeReturnPercent = deal.ProfitOperationPercent;

                decimal days = (decimal)(deal.TimeClose - deal.TimeOpen).TotalDays;

                if (days == 0)
                {
                    days = 1;
                }

                decimal annualizedReturn = tradeReturnPercent * (365.0m / days);

                totalReturn += annualizedReturn;
            }
            // 2. Apply scaling from MultToJournal (e.g., 50% → 0.5)
            totalReturn *= multiplier / 100;

            // 4. Calculate arithmetic mean  and scaling from percent to decimal
            return totalReturn / deals.Length;
        }

        public static decimal GetSharpRatio(Position[] deals, decimal riskFreeProfitInYear, decimal multiplier)
        {
            /*

            Sharpe Ratio = (AHPR - RFR) / SD

            AHPR - усреднённая прибыль в % к портфелю со всех сделок за всё время (Average Holding Period Return (AHPR) as the arithmetic mean of individual trade returns)
            RFR - безрисковая ставка, рассчитанная за всё время которое мог получить инвестор от открытия первой сделки до открытия последней
            SD - стандартное отклонение массива прибылей всех сделок в отдельности

            */

            if (deals == null ||
                deals.Length == 0)
            {
                return 0;
            }

            // 1 берём AHRP - усредненная прибыль одной сделки в % к портфелю со всех сделок за всё время 

            decimal ahpr = GetAverageReturn(deals, multiplier);

            if (ahpr == 0)
            {
                return 0;
            }

            // RFR - no-risk rate 

            DateTime timeFirstDeal = DateTime.MaxValue;
            DateTime timeEndDeal = DateTime.MinValue;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].TimeOpen < timeFirstDeal)
                {
                    timeFirstDeal = deals[i].TimeOpen;
                }

                if (deals[i].TimeClose > timeEndDeal)
                {
                    timeEndDeal = deals[i].TimeClose;
                }
            }

            decimal rfr = 0;
            if (timeFirstDeal != DateTime.MaxValue && timeEndDeal != DateTime.MinValue && riskFreeProfitInYear != 0)
            {
                int daysCountInPoses = (int)(timeEndDeal - timeFirstDeal).TotalDays;
                decimal riskFreeProfitInYearDecimal = riskFreeProfitInYear; // Convert to decimal
                rfr = (decimal)daysCountInPoses / 365 * riskFreeProfitInYearDecimal; // average risk-free return from holding time 
            }

            // 3. Calculate standard deviation
            List<decimal> portfolioReturns = [];
            foreach (Position deal in deals)
            {
                decimal scaledReturn = deal.ProfitOperationPercent * multiplier / 100m;
                decimal days = (decimal)(deal.TimeClose - deal.TimeOpen).TotalDays;

                if (days == 0)
                {
                    days = 1;
                }

                decimal annualizedReturn = scaledReturn * (365.0m / days);

                portfolioReturns.Add(annualizedReturn);
            }

            decimal sd = GetValueStandardDeviation(portfolioReturns);

            // 4. Compute Sharpe Ratio
            if (sd == 0)
                return 0;

            decimal sharp = (ahpr - rfr) / sd;
            return Math.Round(sharp, 4);
        }

        private static decimal GetValueStandardDeviation(List<decimal> candles)
        {
            int length = candles.Count;
            if (length < 1) return 0;

            decimal sum = candles.Sum();
            decimal m = sum / length;  // Correct mean
            decimal sd = 0;

            foreach (decimal value in candles)
            {
                decimal x = value - m;
                sd += x * x;  // Avoid double conversion
            }

            decimal variance = sd / length;
            sd = (decimal)Math.Sqrt((double)variance);  // Population SD
            return Math.Round(sd, 5);
        }

        private static int GetProfitDial(Position[] deals)
        {
            int profitDeal = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationPercent > 0)
                {
                    profitDeal++;
                }
            }

            return profitDeal;
        }

        private static decimal GetProfitDialPercent(Position[] deals)
        {
            decimal profitDeal = GetProfitDial(deals);

            if (profitDeal == 0)
            {
                return profitDeal;
            }

            return profitDeal / deals.Length * 100;

        }

        private static decimal GetAllMiddleProfitInProfitInAbsolute(Position[] deals, decimal multiplier)
        {
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationAbs > 0)
                {
                    profit += deals[i].ProfitOperationAbs;
                }
            }

            if (profit == 0)
            {
                return profit;
            }

            var profitDeals = GetProfitDial(deals);
            if (profitDeals == 0)
            {
                return 0;
            }

            profit *= multiplier / 100;

            return Math.Round(profit / profitDeals, 6);
        }

        private static decimal GetAllMiddleProfitInProfitInPercent(Position[] deals, decimal multiplier)
        {
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationPercent > 0)
                {
                    profit += deals[i].ProfitOperationPercent;
                }
            }
            if (profit == 0)
            {
                return profit;
            }

            var profitDeals = GetProfitDial(deals);
            if (profitDeals == 0)
            {
                return 0;
            }

            profit *= multiplier / 100;

            return profit / GetProfitDial(deals);
        }

        private static decimal GetAllMiddleProfitInProfitInAbsoluteOnDeposit(Position[] deals, decimal multiplier)
        {
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitPortfolioAbs > 0)
                {
                    profit += deals[i].ProfitPortfolioAbs;
                }
            }

            if (profit == 0)
            {
                return profit;
            }

            if (GetProfitDial(deals) == 0)
            {
                return 0;
            }

            profit *= multiplier / 100;

            return Math.Round(profit / GetProfitDial(deals), 6);
        }

        private static decimal GetAllMiddleProfitInProfitInPercentOnDeposit(Position[] deals, decimal multiplier)
        {
            decimal profit = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitPortfolioPercent > 0)
                {
                    profit += deals[i].ProfitPortfolioPercent;
                }
            }
            if (profit == 0)
            {
                return profit;
            }

            if (GetProfitDial(deals) == 0)
            {
                return 0;
            }
            profit *= multiplier / 100;

            return profit / GetProfitDial(deals);
        }

        private static int GetMaxProfitSeries(Position[] deals)
        {
            int maxSeries = 0;

            int nowSeries = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationPercent > 0)
                {
                    nowSeries++;

                    if (nowSeries > maxSeries)
                    {
                        maxSeries = nowSeries;
                    }
                }
                else
                {
                    nowSeries = 0;
                }
            }

            return maxSeries;
        }

        #endregion

        #region Loss

        private static int GetLossDial(Position[] deals)
        {
            int lossDeal = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationPercent <= 0)
                {
                    lossDeal++;
                }
            }

            return lossDeal;
        }

        private static decimal GetLossDialPercent(Position[] deals)
        {
            decimal lossDeal = GetLossDial(deals);

            if (lossDeal == 0)
            {
                return lossDeal;
            }

            return lossDeal / deals.Length * 100;

        }

        private static decimal GetAllMiddleLossInLossInAbsolute(Position[] deals, decimal multiplier)
        {
            decimal loss = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationAbs <= 0)
                {
                    loss += deals[i].ProfitOperationAbs;
                }
            }
            if (loss == 0)
            {
                return loss;
            }
            loss *= multiplier / 100;
            return Math.Round(loss / GetLossDial(deals), 6);
        }

        private static decimal GetAllMiddleLossInLossInPercent(Position[] deals, decimal multiplier)
        {
            decimal loss = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationPercent <= 0)
                {
                    loss += deals[i].ProfitOperationPercent;
                }
            }
            if (loss == 0)
            {
                return loss;
            }
            loss *= multiplier / 100;
            return loss / GetLossDial(deals);
        }

        private static decimal GetAllMiddleLossInLossInAbsoluteOnDeposit(Position[] deals, decimal multiplier)
        {
            decimal loss = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitPortfolioAbs <= 0)
                {
                    loss += deals[i].ProfitPortfolioAbs;
                }
            }
            if (loss == 0)
            {
                return loss;
            }

            decimal lossDeals = GetLossDial(deals);
            if (lossDeals == 0)
            {
                return lossDeals;
            }
            loss *= multiplier / 100;

            return Math.Round(loss / lossDeals, 6);
        }

        private static decimal GetAllMiddleLossInLossInPercentOnDeposit(Position[] deals, decimal multiplier)
        {
            decimal loss = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitPortfolioPercent <= 0)
                {
                    loss += deals[i].ProfitPortfolioPercent;
                }
            }
            if (loss == 0)
            {
                return loss;
            }

            int lossDeals = GetLossDial(deals);

            if (lossDeals == 0)
            {
                return 0;
            }
            loss *= multiplier / 100;

            return loss / lossDeals;
        }

        private static int GetMaxLossSeries(Position[] deals)
        {
            int maxSeries = 0;

            int nowSeries = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationPercent <= 0)
                {
                    nowSeries++;

                    if (nowSeries > maxSeries)
                    {
                        maxSeries = nowSeries;
                    }
                }
                else
                {
                    nowSeries = 0;
                }
            }

            return maxSeries;
        }

        #endregion

        #region Common

        public static string GetAverageTimeOnPoses(Position[] deals)
        {
            TimeSpan allTime = new();
            int dealsCount = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                DateTime openTime = deals[i].TimeOpen;
                DateTime closeTime = deals[i].TimeClose;

                if (closeTime == DateTime.MinValue)
                {
                    continue;
                }

                dealsCount++;

                allTime += closeTime - openTime;
            }

            if (dealsCount == 0)
            {
                return "";
            }
            else
            {
                allTime /= dealsCount;
                return string.Format("{0:D2}:{1:D2}:{2:D2}",
                        (int)allTime.TotalHours, allTime.Minutes, allTime.Seconds);
            }
        }

        public static decimal GetMaxDownPercent(Position[] deals, decimal multiplier)
        {
            decimal maxDownAbs = decimal.MaxValue;
            decimal maxDownPercent = decimal.MaxValue;

            if (GetProfitDial(deals) == 0)
            {
                return 0;
            }
            decimal firsValue = deals[0].PortfolioValueOnOpenPosition;

            for (int i = 0; i < deals.Length; i++)
            {
                if (firsValue != 0)
                {
                    break;
                }
                firsValue = deals[i].PortfolioValueOnOpenPosition;
            }

            if (firsValue == 0)
            {
                firsValue = 1;
            }

            decimal thisSum = firsValue;
            decimal thisPik = firsValue;

            for (int i = 0; i < deals.Length; i++)
            {
                thisSum += deals[i].ProfitPortfolioAbs * multiplier / 100;

                decimal thisDown;

                if (thisSum > thisPik)
                {
                    thisPik = thisSum;
                }
                else
                {
                    if (thisSum < 0)
                    {
                        // уже ушли ниже нулевой отметки по счёту

                        thisDown = -thisPik + thisSum;

                        if (maxDownAbs > thisDown)
                        {
                            maxDownAbs = thisDown;
                            decimal curDownPersent = maxDownAbs / (thisPik / 100);

                            if (maxDownPercent > curDownPersent)
                            {
                                maxDownPercent = curDownPersent;
                            }
                        }
                    }
                    else if (thisSum > 0)
                    {
                        // выше нулевой отметки по счёту
                        thisDown = -(thisPik - thisSum);

                        if (maxDownAbs > thisDown)
                        {
                            maxDownAbs = thisDown;
                            decimal curDownPersent = maxDownAbs / (thisPik / 100);

                            if (maxDownPercent > curDownPersent)
                            {
                                maxDownPercent = curDownPersent;
                            }
                        }

                    }
                }
            }

            if (maxDownPercent == decimal.MaxValue)
            {
                return 0;
            }

            return Round(maxDownPercent);
        }

        public static decimal GetCommissionAmount(Position[] deals, decimal multiplier)
        {
            if (deals.Length == 0)
            {
                return 0;
            }

            decimal commissionTotal = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                commissionTotal += deals[i].CommissionTotal();
            }
            commissionTotal *= multiplier / 100;

            return Round(commissionTotal);
        }

        public static decimal GetProfitFactor(Position[] deals, decimal multiplier)
        {
            decimal commonProfitPunkt = 0m;
            decimal commonLossPunkt = 0m;
            decimal profitFactor = 0m;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationAbs < 0)
                {
                    commonLossPunkt += deals[i].ProfitOperationAbs;
                }
                else
                {
                    commonProfitPunkt += deals[i].ProfitOperationAbs;
                }
            }

            if (commonLossPunkt != 0 && commonProfitPunkt != 0)
            {
                profitFactor = Math.Abs(commonProfitPunkt / commonLossPunkt);
            }
            profitFactor *= multiplier / 100;
            return Round(profitFactor);
        }

        public static decimal GetPayOffRatio(Position[] deals, decimal multiplier)
        {
            decimal allProfit = 0;
            decimal allLoss = 0;

            int profitPos = 0;
            int lossPos = 0;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationAbs > 0)
                {
                    allProfit += deals[i].ProfitOperationAbs;
                    profitPos++;
                }
                else
                {
                    allLoss += deals[i].ProfitOperationAbs;
                    lossPos++;
                }
            }

            if (profitPos == 0
                || lossPos == 0)
            {
                return 0;
            }

            // средняя прибыль разделить на средний убыток)
            if (allLoss != 0)
            {
                allProfit *= multiplier / 100;
                allLoss *= multiplier / 100;
                return Math.Abs(allProfit / profitPos) / Math.Abs(allLoss / lossPos);
            }

            return 0;
        }

        public static decimal GetRecovery(Position[] deals, decimal multiplier)
        {
            decimal recovery = 0m;
            decimal maxLossPunkt = 0m;

            for (int i = 0; i < deals.Length; i++)
            {
                if (deals[i].ProfitOperationAbs <= maxLossPunkt)   // ProfitOperationPercent
                {
                    maxLossPunkt = deals[i].ProfitOperationAbs;
                }
            }
            decimal profit = GetAllProfitInAbsolute(deals, multiplier);
            if (profit != 0 && maxLossPunkt != 0)
            {
                maxLossPunkt *= multiplier / 100;
                recovery = Math.Abs(profit / maxLossPunkt);
            }

            return Round(recovery);
        }

        public static List<Position> SortByTime(List<Position> positionsAll)
        {
            List<Position> newPositionsAll = [];

            for (int i = 0; i < positionsAll.Count; i++)
            {
                if (newPositionsAll.Count == 0 ||
                    newPositionsAll[^1].TimeCreate < positionsAll[i].TimeCreate)
                {
                    newPositionsAll.Add(positionsAll[i]);
                }
                else if (newPositionsAll[0].TimeCreate >= positionsAll[i].TimeCreate)
                {
                    newPositionsAll.Insert(0, positionsAll[i]);
                }
                else
                {
                    for (int i2 = 0; i2 < newPositionsAll.Count - 1; i2++)
                    {
                        if (newPositionsAll[i2].TimeCreate <= positionsAll[i].TimeCreate &&
                            newPositionsAll[i2 + 1].TimeCreate >= positionsAll[i].TimeCreate)
                        {
                            newPositionsAll.Insert(i2 + 1, positionsAll[i]);
                            break;
                        }
                    }
                }
            }

            return newPositionsAll;
        }

        private static decimal Round(decimal number)
        {
            return decimal.Round(number, 6);
        }

        #endregion
    }
}
