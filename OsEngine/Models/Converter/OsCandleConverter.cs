using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OsEngine.Models.Entity;

namespace OsEngine.Models.OsConverter;

// FIX: Turn on Logging, move Tick Converter here
public class OsCandleConverter
{
    private static List<ValueSave> _valuesToFormula = [];

    /// <summary>
    /// save settings to file
    /// </summary>
    public static void Save(TimeFrame timeFrame, string sourceFilePath, string outputFilePath)
    {
        try
        {
            using StreamWriter writer = new("Engine\\CandleConverter.txt", false);
            writer.WriteLine(timeFrame);
            writer.WriteLine(sourceFilePath);
            writer.WriteLine(outputFilePath);

            writer.Close();
        }
        catch (Exception error)
        {
            // SendNewLogMessage(error.ToString(), LogMessageType.Error);
        }
    }

    public static void ConvertFile(TimeFrame timeFrame, string sourceFilePath, string outputFilePath)
    {
        try
        {
            double divider = timeFrame == TimeFrame.Min5 ? 5 : 1;

            List<Candle> candles = ReadSourceFile(sourceFilePath);
            List<Candle> mergedCandles = Merge(candles,
                    Convert.ToInt32(timeFrame.GetTotalMinutes() / divider));

            WriteExitFile(mergedCandles, outputFilePath);
            // SendNewLogMessage("The operation is complete", Logging.LogMessageType.System);
        }
        catch (Exception ex)
        {
            // SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
        }
    }

    public static List<Candle> ReadSourceFile(string sourceFilePath)
    {
        List<Candle> candles = [];

        if (sourceFilePath == null)
        {
            // SendNewLogMessage("There is no candles data file specified", LogMessageType.Error);
            return candles;
        }

        using (var fileStream = File.OpenRead(sourceFilePath))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
        {
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                Candle candle = new();
                candle.SetCandleFromString(line);
                candles.Add(candle);
            }
        }
        return candles;
    }

    public static void WriteExitFile(List<Candle> candles, string outputFilePath)
    {
        using StreamWriter outputFile = new(outputFilePath);
        foreach (Candle candle in candles)
            outputFile.WriteLine(candle.StringToSave);
    }


    /// <summary>
    /// dump candles
    /// </summary>
    /// <param name="candles">candles</param>
    /// <param name="countMerge">Number of folds for the initial TF</param>
    /// <returns></returns>
    public static List<Candle> Merge(List<Candle> candles, int countMerge)
    {
        if (countMerge <= 1)
        {
            return candles;
        }

        if (countMerge <= 1
                || candles == null
                || candles.Count == 0
                || candles.Count < countMerge)
        {
            return candles;
        }


        ValueSave saveVal = _valuesToFormula.Find(val => val.Name == candles[0].StringToSave + countMerge);

        List<Candle> mergeCandles = null;

        if (saveVal != null)
        {
            mergeCandles = saveVal.ValueCandles;
        }
        else
        {
            mergeCandles = [];
            saveVal = new ValueSave
            {
                ValueCandles = mergeCandles,
                Name = candles[0].StringToSave + countMerge
            };
            _valuesToFormula.Add(saveVal);
        }
        // we know the initial index.        
        // узнаём начальный индекс

        int firstIndex = 0;

        if (mergeCandles.Count != 0)
        {
            mergeCandles.RemoveAt(mergeCandles.Count - 1);
        }

        if (mergeCandles.Count != 0)
        {
            for (int i = candles.Count - 1; i > -1; i--)
            {
                if (mergeCandles[^1].Time == candles[i].Time)
                {
                    firstIndex = i + countMerge;

                    if (candles[i].Time.Hour == 10 && candles[i].Time.Minute == 1)
                    {
                        firstIndex -= 1;
                    }
                    break;
                }
            }
        }
        // " Gathering
        // собираем

        for (int i = firstIndex; i < candles.Count;)
        {
            int countReal = countMerge;

            if (countReal + i > candles.Count)
            {
                countReal = candles.Count - i;
            }
            else if (i + countMerge < candles.Count &&
                    candles[i].Time.Day != candles[i + countMerge].Time.Day)
            {
                countReal = 0;

                for (int i2 = i; i2 < candles.Count; i2++)
                {
                    if (candles[i].Time.Day == candles[i2].Time.Day)
                    {
                        countReal += 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (countReal == 0)
            {
                break;
            }

            if (candles[i].Time.Hour == 10
                    && candles[i].Time.Minute == 1
                    && countReal == countMerge)
            {
                countReal -= 1;
            }

            mergeCandles.Add(Concate(candles, i, countReal));
            i += countReal;

        }

        return mergeCandles;
    }

    /// <summary>
    /// candle connection
    /// </summary>
    /// <param name="candles">original candles</param>
    /// <param name="index">start index</param>
    /// <param name="count">candle count for connection</param>
    /// <returns></returns>
    private static Candle Concate(List<Candle> candles, int index, int count)
    {
        Candle candle = new()
        {
            Open = candles[index].Open,
            High = decimal.MinValue,
            Low = decimal.MaxValue,
            Time = candles[index].Time
        };

        for (int i = index; i < candles.Count && i < index + count; i++)
        {
            candle.Trades.AddRange(candles[i].Trades);

            candle.Volume += candles[i].Volume;

            if (candles[i].High > candle.High)
            {
                candle.High = candles[i].High;
            }

            if (candles[i].Low < candle.Low)
            {
                candle.Low = candles[i].Low;
            }

            candle.Close = candles[i].Close;
        }

        return candle;
    }
}
