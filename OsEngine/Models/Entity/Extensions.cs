/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
// using OsEngine.Entity;

namespace OsEngine.Models.Entity;

public static class Extensions
{
    private static CultureInfo _culture = CultureInfo.GetCultureInfo("ru-RU");

    /// <summary>
    /// remove dangerous characters from the name of the security
    /// </summary>
    public static string RemoveExcessFromSecurityName(this string value)
    {
        if (value == null)
        {
            return null;
        }
        
        // это для того чтобы из названия бумаги удалять кавычки (правка @cibermax).
        // К примеру ПАО ЛУКОЙЛ, АДР tiker LKOD@GS не получалось создать папку выдавало исключение

        // value = value
        //     .Replace("/", "")
        //     .Replace("\\", "")
        //     .Replace("*", "")
        //     .Replace(":", "")
        //     .Replace("@", "")
        //     .Replace(";", "")
        //     .Replace("\"", "");// это для того чтобы из названия бумаги удалять кавычки (правка @cibermax).;
        return Regex.Replace(value, "[/\\*:@;\"]", "");
    }

    /// <summary>
    /// whether the string includes dangerous symbols.
    /// </summary>
    public static bool HaveExcessInString(this string value)
    {
        if (value == null)
        {
            return false;
        }

        int len = value.Length;

        char x = '"';

        value = value
            .Replace("*", "")
            .Replace("@", "")
            .Replace(x.ToString(), "");


        if(len != value.Length)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// culture-neutral conversion of string to Decimal type
    /// </summary>
    public static decimal ToDecimal(this string value)
    {
        if(value == null)
        {
            return 0;
        }
        if (value.Contains('E'))
        {
            return Convert.ToDecimal(value.ToDouble());
        }
        try
        {
            return Convert.ToDecimal(value.Replace(",",
                    CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator),
                CultureInfo.InvariantCulture);
        }
        catch
        {
            return Convert.ToDecimal(value.ToDouble());
        }
    }

    /// <summary>
    /// culture-neutral conversion of string to Double type
    /// </summary>
    public static double ToDouble(this string value)
    {
        return Convert.ToDouble(value.Replace(",",
                CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator),
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// remove zeros from the decimal value at the end
    /// </summary>
    public static string ToStringWithNoEndZero(this decimal value)
    {
        string result = value.ToString(_culture);

        if(result.Contains(','))
        {
            result = result.TrimEnd('0');

            if(result.EndsWith(","))
            {
                result = result.TrimEnd(',');
            }
        }

        return result;
    }

    public static string TrimZeros(this decimal value)
    {
        string result = value.ToString(_culture);

        // TODO: Check how it work
        // return result.TrimEnd(['0', ','])
        if(result.Contains(','))
        {
            result = result.TrimEnd('0');

            if(result.EndsWith(','))
            {
                result = result.TrimEnd(',');
            }
        }

        return result;
    }

    /// <summary>
    /// remove zeros from the double value at the end
    /// </summary>
    public static string ToStringWithNoEndZero(this double value)
    {
        string result = value.ToString(_culture);

        if (result.Contains(','))
        {
            result = result.TrimEnd('0');

            if (result.EndsWith(","))
            {
                result = result.TrimEnd(',');
            }
        }

        return result;
    }

    /// <summary>
    /// get decimal point from double ro decimal values
    /// </summary>
    public static int DecimalsCount(this string value)
    {
        if (value.Contains('E'))
        {
            value = value.ToDecimal().ToString();
        }

        value = value.Replace(",", ".");

        while (value.Length > 0 &&
               value.EndsWith("0"))
        {
            value = value[..^1];
        }

        if (value.Split('.').Length == 1)
        {
            return 0;
        }

        return value.Split('.')[1].Length;
    }

    // private static Dictionary<int, decimal> IDK = Enumerable.Range(0, 16)
    //     .ToDictionary(i => i, i => (decimal)(1 / Math.Pow(10, i)));

    /// <summary>
    /// get scale accuracy based on the number of decimal places / 
    /// получить точность шкалы на основании количества знаков после запятой
    /// </summary>
    /// <param name="value">decimal point / количество знаков после запятой</param>
    public static decimal GetValueByDecimals(this int value)
    {
        return value switch
        {
            0 => 1,
            1 => 0.1m,
            2 => 0.01m,
            3 => 0.001m,
            4 => 0.0001m,
            5 => 0.00001m,
            6 => 0.000001m,
            7 => 0.0000001m,
            8 => 0.00000001m,
            9 => 0.000000001m,
            10 => 0.0000000001m,
            11 => 0.00000000001m,
            12 => 0.000000000001m,
            13 => 0.0000000000001m,
            14 => 0.00000000000001m,
            15 => 0.000000000000001m,
            _ => 0,
        };
    }

    /// <summary>
    /// merge two candlestick data arrays
    /// </summary>
    public static List<Candle> Merge(this List<Candle> oldCandles, List<Candle> candlesToMerge)
    {
        if (candlesToMerge == null ||
            candlesToMerge.Count == 0)
        {
            return oldCandles;
        }

        // костыль от наличия null свечек в массиве

        for(int i = 0;i < candlesToMerge.Count;i++)
        {
            if (candlesToMerge[i] == null)
            {
                candlesToMerge.RemoveAt(i);
                i--;
            }
        }

        if(candlesToMerge.Count == 0)
        {
            return oldCandles;
        }

        if (oldCandles.Count == 0)
        {
            oldCandles.AddRange(candlesToMerge);
            return oldCandles;
        }

        if (candlesToMerge[0].Time < oldCandles[0].Time &&
            candlesToMerge[^1].Time >= oldCandles[^1].Time)
        {
            // начало массива в новых свечках раньше. Конец позже. Перезаписываем полностью 
            oldCandles.Clear();
            oldCandles.AddRange(candlesToMerge);
            return oldCandles;
        }

        // смотрим более ранние свечи в новой серии

        List<Candle> newCandles = [];

        int indexLastInsertCandle = 0;

        for (int i = 0; i < candlesToMerge.Count; i++)
        {
            if (candlesToMerge[i].Time < oldCandles[0].Time)
            {
                newCandles.Add(candlesToMerge[i]);
            }
            else
            {
                indexLastInsertCandle = i;
                break;
            }
        }

        newCandles.AddRange(oldCandles);

        // обновляем последнюю свечку в старых данных

        if (newCandles.Count != 0)
        {
            Candle lastCandle = null;

            for(int i = 0;i < candlesToMerge.Count;i++)
            {
                if (candlesToMerge[i].Time == newCandles[^1].Time)
                {
                    lastCandle = candlesToMerge[i]; 
                    break;
                }
            }

            if (lastCandle != null)
            {
                newCandles[^1] = lastCandle;
            }
        }

        // вставляем новые свечи в середину объединённого массива. Смотрим последние 500 свечек, не более

        int indxStart = newCandles.Count - 500;

        if(indxStart < 0)
        {
            indxStart = 0;
        }

        for (int i = indexLastInsertCandle; i < candlesToMerge.Count; i++)
        {
            Candle candle = candlesToMerge[i];

            bool candleInsertInOldArray = false;

            for (int i2 = indxStart; i2 < newCandles.Count - 2; i2++)
            {
                if (candle.Time > newCandles[i2].Time &&
                    candle.Time < newCandles[i2 + 1].Time)
                {
                    newCandles.Insert(i2 + 1, candle);
                    candleInsertInOldArray = true;
                    break;
                }
            }

            if(candleInsertInOldArray == false)
            {
                i += 10;
            }
        }

        // вставляем новые свечи в конец объединённого массива

        for (int i = 0; i < candlesToMerge.Count; i++)
        {
            Candle candle = candlesToMerge[i];

            if (candle.Time > newCandles[^1].Time)
            {
                newCandles.Add(candle);
            }
        }

        return newCandles;
    }

    /// <summary>
    /// merge two trades data arrays
    /// </summary>
    // TODO: Create function in OsDataSet insead of here
    public static List<Trade> Merge(this List<Trade> oldTrades, List<Trade> tradesToMerge)
    {
        if (tradesToMerge == null ||
            tradesToMerge.Count == 0)
        {
            return oldTrades;
        }

        if (oldTrades.Count == 0)
        {
            oldTrades.AddRange(tradesToMerge);
            return oldTrades;
        }

        if (tradesToMerge[0].Time < oldTrades[0].Time &&
            tradesToMerge[^1].Time >= oldTrades[^1].Time)
        {
            // начало массива в новых свечках раньше. Конец позже. Перезаписываем полностью 
            oldTrades.Clear();
            oldTrades.AddRange(tradesToMerge);
            return oldTrades;
        }

        if (oldTrades[^1].Time < tradesToMerge[0].Time)
        {
            oldTrades.AddRange(tradesToMerge);
            return oldTrades;
        }

        // смотрим более ранние свечи в новой серии

        List<Trade> newTrades = [];

        int indexLastInsertCandle = 0;

        for (int i = 0; i < tradesToMerge.Count; i++)
        {
            if (tradesToMerge[i].Time < oldTrades[0].Time)
            {
                newTrades.Add(tradesToMerge[i]);
            }
            else
            {
                indexLastInsertCandle = i;
                break;
            }
        }

        newTrades.AddRange(oldTrades);

        // обновляем последнюю свечку в старых данных

        if (newTrades.Count != 0)
        {
            Trade lastTrade = tradesToMerge.Find(c => c.Time == newTrades[^1].Time);

            if (lastTrade != null)
            {
                newTrades[^1] = lastTrade;
            }
        }

        // вставляем новые свечи в середину объединённого массива. Смотрим последние 500 свечек, не более

        int indxStart = newTrades.Count - 500;

        if (indxStart < 0)
        {
            indxStart = 0;
        }

        for (int i = indexLastInsertCandle; i < tradesToMerge.Count; i++)
        {
            Trade trade = tradesToMerge[i];

            bool tradesInsertInOldArray = false;

            for (int i2 = indxStart; i2 < newTrades.Count - 2; i2++)
            {
                if (trade.Time > newTrades[i2].Time &&
                    trade.Time < newTrades[i2 + 1].Time)
                {
                    newTrades.Insert(i2 + 1, trade);
                    tradesInsertInOldArray = true;
                    break;
                }
            }

            if (tradesInsertInOldArray == false)
            {
                i += 10;
            }
        }

        // вставляем новые свечи в конец объединённого массива

        for (int i = 0; i < tradesToMerge.Count; i++)
        {
            Trade tradeNew = tradesToMerge[i];

            if (tradeNew.Time >= newTrades[^1].Time)
            {
                newTrades.Add(tradeNew);
            }
        }

        return newTrades;
    }

    /// <summary>
    /// convert a row in a table to a string representation
    /// </summary>
    // FIX: ?

    // public static string ToFormatString(this DataGridViewRow row)
    // {
    //     string result = "";
    //
    //     for(int i = 0; row.Cells != null && i < row.Cells.Count;i++)
    //     {
    //         if(row.Cells[i].Value == null)
    //         {
    //             result +=  ";";
    //             continue;
    //         }
    //         result += row.Cells[i].Value.ToString().Replace("\n"," ").Replace("\r"," ").Replace(",",".") + ";";
    //     }
    //
    //     return result;
    // }

}

public static class DateTimeParseHelper
{
    /// <summary>
    /// Converts date-time from two strings, a date string and a time string.
    /// </summary>
    /// <param name="date">Date string in the format "YYYYMMDD".</param>
    /// <param name="time">Time string in the format  "HHmmSS".</param>
    public static DateTime ParseFromTwoStrings(string date, string time)
    {
        ParseDateOrTimeString(date, out int year, out int month, out int day);
        ParseDateOrTimeString(time, out int hour, out int minute, out int second);
        return new DateTime(year, month, day, hour, minute, second);
    }

    /// <summary>
    /// Converts a date or time string to the output variables year-month-day
    /// (if a date string) or hour-minute-second (if a time string).
    /// </summary>
    public static void ParseDateOrTimeString(string date_time, out int year_hour, out int month_minute, out int day_second)
    {
        int dateOrTimeInt = Convert.ToInt32(date_time);
        year_hour = dateOrTimeInt / 10000;
        month_minute = dateOrTimeInt / 100 % 100;
        day_second = dateOrTimeInt % 100;
    }
}

public static class FileExtensions
{
    public static void TryDelete(this string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.");
        }

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                MessageBox.Show($"Error deleting file:\n{ex.Message}");
            }
        }
    }
}
