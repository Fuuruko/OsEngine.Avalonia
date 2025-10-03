/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.IO;

namespace OsEngine.Models.Utils;

public class PrimeSettingsMaster
{
    public static string LabelInHeaderBotStation
    {
        get; set { field = value; Save(); }
    } = "";

    public static bool ErrorLogMessageBoxIsActive
    {
        get; set { field = value; Save(); }
    } = true;

    public static bool ErrorLogBeepIsActive
    {
        get; set { field = value; Save(); }
    } = false;

    public static bool TransactionBeepIsActive
    {
        get; set { field = value; Save(); }
    } = false;

    public static bool RebootTradeUiLight
    {
        get; set { field = value; Save(); }
    } = false;

    public static bool ReportCriticalErrors
    {
        get;
        set
        {
            if (field == value) { return; }
            field = value;
            Save();
        }
    } = true;

    public static MemoryCleanerRegime MemoryCleanerRegime
    {
        get;
        set
        {
            if (field == value) { return; }
            field = value;
            Save();
        }
    } = MemoryCleanerRegime.Disable;

    public static void Save()
    {
        try
        {
            using StreamWriter writer = new(@"Engine\PrimeSettings.txt", false);
            writer.WriteLine(TransactionBeepIsActive);
            writer.WriteLine(ErrorLogBeepIsActive);
            writer.WriteLine(ErrorLogMessageBoxIsActive);
            writer.WriteLine(LabelInHeaderBotStation);
            writer.WriteLine(RebootTradeUiLight);
            writer.WriteLine(ReportCriticalErrors);
            writer.WriteLine(MemoryCleanerRegime);

            writer.Close();
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private static bool _isLoad = false;

    public static void Load()
    {
        if (_isLoad) { return; }
        _isLoad = true;
        if (!File.Exists(@"Engine\PrimeSettings.txt")) { return; }

        try
        {
            using StreamReader reader = new(@"Engine\PrimeSettings.txt");
            TransactionBeepIsActive = Convert.ToBoolean(reader.ReadLine());
            ErrorLogBeepIsActive = Convert.ToBoolean(reader.ReadLine());
            ErrorLogMessageBoxIsActive = Convert.ToBoolean(reader.ReadLine());

            LabelInHeaderBotStation = reader.ReadLine();

            if (LabelInHeaderBotStation == "True"
                || LabelInHeaderBotStation == "False")
            {
                LabelInHeaderBotStation = "";
            }

            RebootTradeUiLight = Convert.ToBoolean(reader.ReadLine());
            ReportCriticalErrors = Convert.ToBoolean(reader.ReadLine());

            Enum.TryParse(reader.ReadLine(), out MemoryCleanerRegime regime);
            MemoryCleanerRegime = regime;

            reader.Close();
        }
        catch (Exception)
        {
            ReportCriticalErrors = true;
            // ignore
        }
    }

}

public enum MemoryCleanerRegime
{
    Disable,
    At5Minutes,
    At30Minutes
}
