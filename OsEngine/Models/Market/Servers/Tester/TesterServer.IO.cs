using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;

namespace OsEngine.Models.Market.Servers.Tester;

public partial class TesterServer
{
    private void Load()
    {
        if (!File.Exists(@"Engine\" + @"TestServer.txt"))
        {
            return;
        }

        try
        {
            using StreamReader reader = new(@"Engine\" + @"TestServer.txt");
            ActiveSet = reader.ReadLine();
            _slippageToSimpleOrder = Convert.ToInt32(reader.ReadLine());
            Enum.TryParse(reader.ReadLine(), out _typeTesterData);
            Enum.TryParse(reader.ReadLine(), out _sourceDataType);
            _pathToFolder = reader.ReadLine();
            _slippageToStopOrder = Convert.ToInt32(reader.ReadLine());
            Enum.TryParse(reader.ReadLine(), out _orderExecutionType);
            _profitMarketIsOn = Convert.ToBoolean(reader.ReadLine());
            // _guiIsOpenFullSettings = Convert.ToBoolean(reader.ReadLine());
            _removeTradesFromMemory = Convert.ToBoolean(reader.ReadLine());

            reader.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void Save()
    {
        try
        {
            using StreamWriter writer = new(@"Engine\" + @"TestServer.txt", false);
            writer.WriteLine(ActiveSet);
            writer.WriteLine(_slippageToSimpleOrder);
            writer.WriteLine(_typeTesterData);
            writer.WriteLine(_sourceDataType);
            writer.WriteLine(_pathToFolder);
            writer.WriteLine(_slippageToStopOrder);
            writer.WriteLine(_orderExecutionType);
            writer.WriteLine(_profitMarketIsOn);
            // writer.WriteLine(_guiIsOpenFullSettings);
            writer.WriteLine(_removeTradesFromMemory);
            writer.Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void SaveSecurityTestSettings()
    {
        try
        {
            using StreamWriter writer = new(GetSecurityTestSettingsPath(), false);
            writer.WriteLine(_timeStart.ToString(InvariantCulture));
            writer.WriteLine(_timeEnd.ToString(InvariantCulture));
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void LoadSecurityTestSettings()
    {
        try
        {
            string pathToSettings = GetSecurityTestSettingsPath();
            if (!File.Exists(pathToSettings))
            {
                return;
            }

            using StreamReader reader = new(pathToSettings);
            string timeStart = reader.ReadLine();
            if (timeStart != null)
            {
                _timeStart = Convert.ToDateTime(timeStart, InvariantCulture);
            }
            string timeEnd = reader.ReadLine();
            if (timeEnd != null)
            {
                _timeEnd = Convert.ToDateTime(timeEnd, InvariantCulture);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private string GetSecurityTestSettingsPath()
    {
        string pathToSettings;

        if (SourceDataType == TesterSourceDataType.Set)
        {
            if (string.IsNullOrWhiteSpace(ActiveSet))
            {
                return "";
            }
            pathToSettings = ActiveSet + "\\SecurityTestSettings.txt";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_pathToFolder))
            {
                return "";
            }
            pathToSettings = _pathToFolder + "\\SecurityTestSettings.txt";
        }

        return pathToSettings;
    }

    private string GetSecuritiesSettingsPath()
    {
        string pathToSettings;

        if (SourceDataType == TesterSourceDataType.Set)
        {
            if (string.IsNullOrWhiteSpace(ActiveSet))
            {
                return "";
            }
            pathToSettings = ActiveSet + "\\SecuritiesSettings.txt";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_pathToFolder))
            {
                return "";
            }
            pathToSettings = _pathToFolder + "\\SecuritiesSettings.txt";
        }

        return pathToSettings;
    }











    private void LoadSecurities()
    {
        if ((_sourceDataType == TesterSourceDataType.Set && (string.IsNullOrWhiteSpace(ActiveSet) || !Directory.Exists(ActiveSet))) ||
                (_sourceDataType == TesterSourceDataType.Folder && (string.IsNullOrWhiteSpace(_pathToFolder) || !Directory.Exists(_pathToFolder))))
        {
            return;
        }

        _timeMax = DateTime.MinValue;
        _timeEnd = DateTime.MaxValue;
        _timeMin = DateTime.MaxValue;
        _timeStart = DateTime.MinValue;
        _timeNow = DateTime.MinValue;

        if (_sourceDataType == TesterSourceDataType.Set)
        { // Hercules data sets/сеты данных Геркулеса
            string[] directories = Directory.GetDirectories(ActiveSet);

            if (directories.Length == 0)
            {
                SendLogMessage(OsLocalization.Market.Message28, LogMessageType.System);
                return;
            }

            for (int i = 0; i < directories.Length; i++)
            {
                LoadSecurity(directories[i]);
            }

            _dataIsReady = true;
        }
        else // if (_sourceDataType == TesterSourceDataType.Folder)
        { // simple files from folder/простые файлы из папки

            string[] files = Directory.GetFiles(_pathToFolder);

            if (files.Length == 0)
            {
                SendLogMessage(OsLocalization.Market.Message49, LogMessageType.Error);
            }

            LoadCandleFromFolder(_pathToFolder);
            LoadTickFromFolder(_pathToFolder);
            LoadMarketDepthFromFolder(_pathToFolder);
            _dataIsReady = true;
        }

        LoadSetSecuritiesTimeFrameSettings();
    }

    private void LoadSecurity(string path)
    {
        string[] directories = Directory.GetDirectories(path);

        if (directories.Length == 0)
        {
            return;
        }

        for (int i = 0; i < directories.Length; i++)
        {
            string name = directories[i].Split('\\')[3];

            if (name == "MarketDepth")
            {
                LoadMarketDepthFromFolder(directories[i]);
            }
            else if (name == "Tick")
            {
                LoadTickFromFolder(directories[i]);
            }
            else
            {
                LoadCandleFromFolder(directories[i]);
            }
        }
    }

    private void LoadCandleFromFolder(string folderName)
    {

        string[] files = Directory.GetFiles(folderName);

        if (files.Length == 0)
        {
            return;
        }

        List<SecurityTester> security = [];

        for (int i = 0; i < files.Length; i++)
        {
            StreamReader reader = new(files[i]);

            // candles / свечи: 20110111,100000,19577.00000,19655.00000,19533.00000,19585.00000,2752
            // ticks ver.1 / тики 1 вар: 20150401,100000,86160.000000000,2
            // ticks ver.2 / тики 2 вар: 20151006,040529,3010,5,Buy/Sell/Unknown

            string firstRowInFile = reader.ReadLine();

            if (string.IsNullOrEmpty(firstRowInFile))
            {
                // security.Remove(security[^1]);
                reader.Close();
                continue;
            }

            // timeframe / тф
            // price step / шаг цены
            // begin / начало
            // end / конец

            try
            {
                // check whether candles are in the file / смотрим свечи ли в файле
                Candle candle = Candle.Parse(firstRowInFile);
                // candles are in the file. We look at which ones / в файле свечи. Смотрим какие именно

                Candle.Parse(reader.ReadLine());


                var timeFrameSpan = GetTimeSpan(reader);

                // step price / шаг цены

                decimal minPriceStep = decimal.MaxValue;
                int countFive = 0;

                CultureInfo culture = InvariantCulture;

                for (int j = 0; j < 20; j++)
                {
                    if (reader.EndOfStream == true)
                    {
                        reader.Close();
                        // NOTE: Doesnt it open it from the start
                        // so it will be read the same candles?
                        reader = new StreamReader(files[i]);

                        // if (reader.EndOfStream == true) { break; }
                        j--;
                        continue;
                    }

                    Candle candleN = Candle.Parse(reader.ReadLine());

                    decimal openD = candleN.Open;
                    decimal highD = candleN.High;
                    decimal lowD = candleN.Low;
                    decimal closeD = candleN.Close;

                    string[] prices = [
                        openD.ToString(InvariantCulture),
                        highD.ToString(InvariantCulture),
                        lowD.ToString(InvariantCulture),
                        closeD.ToString(InvariantCulture),
                    ];
                    string open = openD.ToString(InvariantCulture);
                    string high = highD.ToString(InvariantCulture);
                    string low = lowD.ToString(InvariantCulture);
                    string close = closeD.ToString(InvariantCulture);

                    int maxFractionLength = prices.Select(p =>
                            p.Contains('.') ? p.Split('.')[1].TrimEnd('0').Length : 0).Max();

                    if (maxFractionLength > 0)
                    {
                        minPriceStep = maxFractionLength switch
                        {
                            1 when minPriceStep > 0.1m => 0.1m,
                            2 when minPriceStep > 0.01m => 0.01m,
                            3 when minPriceStep > 0.001m => 0.001m,
                            4 when minPriceStep > 0.0001m => 0.0001m,
                            5 when minPriceStep > 0.00001m => 0.00001m,
                            6 when minPriceStep > 0.000001m => 0.000001m,
                            7 when minPriceStep > 0.0000001m => 0.0000001m,
                            8 when minPriceStep > 0.00000001m => 0.00000001m,
                            9 when minPriceStep > 0.000000001m => 0.000000001m,
                            _ => minPriceStep
                        };
                    }
                    else
                    {
                        int trailingZeros = prices.Select(p => p.TrimEnd('0').Length).Min();
                        minPriceStep = Math.Min(minPriceStep, trailingZeros);

                        if (minPriceStep == 1
                                && openD % 5 == 0
                                && highD % 5 == 0
                                && closeD % 5 == 0
                                && lowD % 5 == 0)
                        {
                            countFive++;
                        }
                    }

                    // string[] openParts = open.Split('.');
                    // string[] highParts = high.Split('.');
                    // string[] lowParts = low.Split('.');
                    // string[] closeParts = close.Split('.');

                    if (open.Split('.').Length > 1 ||
                            high.Split('.').Length > 1 ||
                            low.Split('.').Length > 1 ||
                            close.Split('.').Length > 1)
                    {
                        // if the real part takes place / если имеет место вещественная часть
                        int length = 1;

                        if (open.Split('.').Length > 1 &&
                                open.Split('.')[1].Length > length)
                        {
                            length = open.Split('.')[1].Length;
                        }

                        if (high.Split('.').Length > 1 &&
                                high.Split('.')[1].Length > length)
                        {
                            length = high.Split('.')[1].Length;
                        }

                        if (low.Split('.').Length > 1 &&
                                low.Split('.')[1].Length > length)
                        {
                            length = low.Split('.')[1].Length;
                        }

                        if (close.Split('.').Length > 1 &&
                                close.Split('.')[1].Length > length)
                        {
                            length = close.Split('.')[1].Length;
                        }

                        if (length == 1 && minPriceStep > 0.1m)
                        {
                            minPriceStep = 0.1m;
                        }
                        if (length == 2 && minPriceStep > 0.01m)
                        {
                            minPriceStep = 0.01m;
                        }
                        if (length == 3 && minPriceStep > 0.001m)
                        {
                            minPriceStep = 0.001m;
                        }
                        if (length == 4 && minPriceStep > 0.0001m)
                        {
                            minPriceStep = 0.0001m;
                        }
                        if (length == 5 && minPriceStep > 0.00001m)
                        {
                            minPriceStep = 0.00001m;
                        }
                        if (length == 6 && minPriceStep > 0.000001m)
                        {
                            minPriceStep = 0.000001m;
                        }
                        if (length == 7 && minPriceStep > 0.0000001m)
                        {
                            minPriceStep = 0.0000001m;
                        }
                        if (length == 8 && minPriceStep > 0.00000001m)
                        {
                            minPriceStep = 0.00000001m;
                        }
                        if (length == 9 && minPriceStep > 0.000000001m)
                        {
                            minPriceStep = 0.000000001m;
                        }
                    }
                    else
                    {
                        // if the real part doesn't take place / если вещественной части нет
                        int length = 1;

                        for (int i3 = open.Length - 1; open.ToString(culture)[i3] == '0'; i3--)
                        {
                            length *= 10;
                        }

                        int lengthLow = 1;

                        for (int i3 = low.Length - 1; low[i3] == '0'; i3--)
                        {
                            lengthLow *= 10;

                            if (length > lengthLow)
                            {
                                length = lengthLow;
                            }
                        }

                        int lengthHigh = 1;

                        for (int i3 = high.Length - 1; high[i3] == '0'; i3--)
                        {
                            lengthHigh *= 10;

                            if (length > lengthHigh)
                            {
                                length = lengthHigh;
                            }
                        }

                        int lengthClose = 1;

                        for (int i3 = close.Length - 1; close[i3] == '0'; i3--)
                        {
                            lengthClose *= 10;

                            if (length > lengthClose)
                            {
                                length = lengthClose;
                            }
                        }
                        if (minPriceStep > length)
                        {
                            minPriceStep = length;
                        }

                        if (minPriceStep == 1 &&
                                openD % 5 == 0 && highD % 5 == 0 &&
                                closeD % 5 == 0 && lowD % 5 == 0)
                        {
                            countFive++;
                        }
                    }
                }


                if (minPriceStep == 1 &&
                        countFive == 20)
                {
                    minPriceStep = 5;
                }


                // last data / последняя дата
                string lastString = firstRowInFile;

                while (!reader.EndOfStream)
                {
                    string curStr = reader.ReadLine();

                    if (string.IsNullOrEmpty(curStr)) { continue; }

                    lastString = curStr;
                }

                Candle lastCandle = Candle.Parse(lastString);


                string name = files[i].Split('\\')[^1];
                var timeFrame = GetTimeFrame(timeFrameSpan);

                SecurityTester securityTester = new()
                {
                    FileAddress = files[i],
                    DataType = SecurityTesterDataType.Candle,

                    Security = new Security
                    {
                        Name = name,
                        Lot = 1,
                        NameClass = "TestClass",
                        Go = 1,
                        PriceStepCost = minPriceStep,
                        PriceStep = minPriceStep
                    },

                    TimeFrameSpan = timeFrameSpan,
                    TimeFrame = timeFrame,

                    TimeStart = candle.TimeStart,
                    TimeEnd = lastCandle.TimeStart
                };

                securityTester.NewCandleEvent += TesterServer_NewCandleEvent;
                securityTester.NewTradesEvent += TesterServer_NewTradesEvent;
                securityTester.NewMarketDepthEvent += TesterServer_NewMarketDepthEvent;
                securityTester.LogMessageEvent += OnLogRecieved;
                securityTester.NeedToCheckOrders += CheckOrders;

                security.Add(securityTester);
            }
            catch (Exception)
            {
                security.Remove(security[^1]);
            }
            finally
            {
                reader.Close();
            }
        }

        // save securities 
        // сохраняем бумаги

        if (security == null ||
                security.Count == 0)
        {
            return;
        }

        _securities ??= [];

        SecuritiesTester ??= [];

        for (int i = 0; i < security.Count; i++)
        {
            if (_securities.Find(security1 => security1.Name == security[i].Security.Name) == null)
            {
                _securities.Add(security[i].Security);
            }

            SecuritiesTester.Add(security[i]);
        }

        // count the time
        // считаем время

        if (SecuritiesTester.Count != 0)
        {
            for (int i = 0; i < SecuritiesTester.Count; i++)
            {
                if ((_timeMin == DateTime.MinValue && SecuritiesTester[i].TimeStart != DateTime.MinValue) ||
                        (SecuritiesTester[i].TimeStart != DateTime.MinValue && SecuritiesTester[i].TimeStart < _timeMin))
                {
                    _timeMin = SecuritiesTester[i].TimeStart;
                    _timeStart = SecuritiesTester[i].TimeStart;
                    _timeNow = SecuritiesTester[i].TimeStart;
                }
                if (SecuritiesTester[i].TimeEnd != DateTime.MinValue &&
                        SecuritiesTester[i].TimeEnd > _timeMax)
                {
                    _timeMax = SecuritiesTester[i].TimeEnd;
                    _timeEnd = SecuritiesTester[i].TimeEnd;
                }
            }
        }

        // check in tester file data on the presence of multipliers and GO for securities
        // проверяем в файле тестера данные о наличии мультипликаторов и ГО для бумаг

        SetToSecuritiesDopSettings();
        LoadSecurityTestSettings();

        TestingNewSecurityEvent?.Invoke();
    }

    private void LoadTickFromFolder(string folderName)
    {
        string[] files = Directory.GetFiles(folderName);

        if (files.Length == 0)
        {
            return;
        }

        List<SecurityTester> security = [];

        for (int i = 0; i < files.Length; i++)
        {
            security.Add(new SecurityTester());
            security[^1].FileAddress = files[i];
            security[^1].NewCandleEvent += TesterServer_NewCandleEvent;
            security[^1].NewTradesEvent += TesterServer_NewTradesEvent;
            security[^1].NewMarketDepthEvent += TesterServer_NewMarketDepthEvent;
            security[^1].LogMessageEvent += OnLogRecieved;
            security[^1].NeedToCheckOrders += CheckOrders;

            string name = files[i].Split('\\')[^1];

            security[^1].Security = new Security
            {
                Name = name,
                Lot = 1,
                NameClass = "TestClass",
                Go = 1,
                PriceStepCost = 1,
                PriceStep = 1
            };
            // timeframe / тф
            // price step / шаг цены
            // begin / начало
            // end / конец

            StreamReader reader = new(files[i]);

            // candles / свечи: 20110111,100000,19577.00000,19655.00000,19533.00000,19585.00000,2752
            // ticks ver.1 / тики 1 вар: 20150401,100000,86160.000000000,2
            // ticks ver.2 / тики 2 вар: 20151006,040529,3010,5,Buy/Sell/Unknown

            string firstRowInFile = reader.ReadLine();

            if (string.IsNullOrEmpty(firstRowInFile))
            {
                security.Remove(security[^1]);
                reader.Close();
                continue;
            }

            try
            {
                // check whether ticks are in the file / смотрим тики ли в файле
                Trade trade = new();
                trade.SetTradeFromString(firstRowInFile);
                // ticks are in the file / в файле тики

                security[^1].TimeStart = trade.Time;
                security[^1].DataType = SecurityTesterDataType.Tick;

                // price step / шаг цены

                decimal minPriceStep = decimal.MaxValue;
                int countFive = 0;

                CultureInfo culture = InvariantCulture;

                for (int i2 = 0; i2 < 100; i2++)
                {
                    Trade tradeN = new();
                    tradeN.SetTradeFromString(reader.ReadLine());

                    decimal open = (decimal)Convert.ToDouble(tradeN.Price);


                    if (open.ToString(culture).Split('.').Length > 1)
                    {
                        // if the real part takes place / если имеет место вещественная часть
                        int length = 1;

                        if (open.ToString(culture).Split('.').Length > 1 &&
                                open.ToString(culture).Split('.')[1].Length > length)
                        {
                            length = open.ToString(culture).Split('.')[1].Length;
                        }


                        if (length == 1 && minPriceStep > 0.1m)
                        {
                            minPriceStep = 0.1m;
                        }
                        if (length == 2 && minPriceStep > 0.01m)
                        {
                            minPriceStep = 0.01m;
                        }
                        if (length == 3 && minPriceStep > 0.001m)
                        {
                            minPriceStep = 0.001m;
                        }
                        if (length == 4 && minPriceStep > 0.0001m)
                        {
                            minPriceStep = 0.0001m;
                        }
                        if (length == 5 && minPriceStep > 0.00001m)
                        {
                            minPriceStep = 0.00001m;
                        }
                        if (length == 6 && minPriceStep > 0.000001m)
                        {
                            minPriceStep = 0.000001m;
                        }
                        if (length == 7 && minPriceStep > 0.0000001m)
                        {
                            minPriceStep = 0.0000001m;
                        }
                        if (length == 8 && minPriceStep > 0.00000001m)
                        {
                            minPriceStep = 0.00000001m;
                        }
                        if (length == 9 && minPriceStep > 0.000000001m)
                        {
                            minPriceStep = 0.000000001m;
                        }
                    }
                    else
                    {
                        // if the real part doesn't take place / если вещественной части нет
                        int length = 1;

                        for (int i3 = open.ToString(culture).Length - 1; open.ToString(culture)[i3] == '0'; i3--)
                        {
                            length *= 10;
                        }

                        if (minPriceStep > length)
                        {
                            minPriceStep = length;
                        }

                        if (length == 1 &&
                                open % 5 == 0)
                        {
                            countFive++;
                        }
                    }
                }


                if (minPriceStep == 1 &&
                        countFive == 20)
                {
                    minPriceStep = 5;
                }


                security[^1].Security.PriceStep = minPriceStep;
                security[^1].Security.PriceStepCost = minPriceStep;

                // last data / последняя дата
                string lastString2 = firstRowInFile;

                while (!reader.EndOfStream)
                {
                    string curRow = reader.ReadLine();

                    if (string.IsNullOrEmpty(curRow))
                    {
                        continue;
                    }

                    lastString2 = curRow;
                }

                Trade trade2 = new();
                trade2.SetTradeFromString(lastString2);
                security[^1].TimeEnd = trade2.Time;
            }
            catch (Exception)
            {
                security.Remove(security[^1]);
            }

            reader.Close();
        }

        // save securities / сохраняем бумаги

        if (security.Count == 0)
        {
            return;
        }

        _securities ??= [];

        SecuritiesTester ??= [];

        for (int i = 0; i < security.Count; i++)
        {
            if (_securities.Find(security1 => security1.Name == security[i].Security.Name) == null)
            {
                _securities.Add(security[i].Security);
            }
            SecuritiesTester.Add(security[i]);
        }

        // count the time / считаем время 

        if (SecuritiesTester.Count != 0)
        {
            for (int i = 0; i < SecuritiesTester.Count; i++)
            {
                if ((_timeMin == DateTime.MinValue && SecuritiesTester[i].TimeStart != DateTime.MinValue) ||
                        (SecuritiesTester[i].TimeStart != DateTime.MinValue && SecuritiesTester[i].TimeStart < _timeMin))
                {
                    _timeMin = SecuritiesTester[i].TimeStart;
                    _timeStart = SecuritiesTester[i].TimeStart;
                    _timeNow = SecuritiesTester[i].TimeStart;
                }
                if (SecuritiesTester[i].TimeEnd != DateTime.MinValue &&
                        SecuritiesTester[i].TimeEnd > _timeMax)
                {
                    _timeMax = SecuritiesTester[i].TimeEnd;
                    _timeEnd = SecuritiesTester[i].TimeEnd;
                }
            }
        }

        // check in the tester file data on the presence of multipliers and GO for securities
        // проверяем в файле тестера данные о наличии мультипликаторов и ГО для бумаг

        SetToSecuritiesDopSettings();

        TestingNewSecurityEvent?.Invoke();
    }

    private void LoadMarketDepthFromFolder(string folderName)
    {
        string[] files = Directory.GetFiles(folderName);

        if (files.Length == 0)
        {
            return;
        }

        List<SecurityTester> security = [];

        for (int i = 0; i < files.Length; i++)
        {
            string name = files[i].Split('\\')[^1];
            SecurityTester security_ = new()
            {
                FileAddress = files[i],
                Security = new()
                {
                    Name = name,
                    Lot = 1,
                    NameClass = "TestClass",
                    Go = 1,
                    PriceStepCost = 1,
                    PriceStep = 1
                }
            };
            security_.NewCandleEvent += TesterServer_NewCandleEvent;
            security_.NewTradesEvent += TesterServer_NewTradesEvent;
            security_.LogMessageEvent += OnLogRecieved;
            security_.NewMarketDepthEvent += TesterServer_NewMarketDepthEvent;
            security_.NeedToCheckOrders += CheckOrders;

            security.Add(security_);

            // timeframe / тф
            // price step / шаг цены
            // begin / начало
            // end / конец

            StreamReader reader = new(files[i]);

            // NameSecurity_Time_Bids_Asks
            // Bids: level*level*level
            // level: Bid&Ask&Price

            string firstRowInFile = reader.ReadLine();

            // NOTE: check before create security?
            if (string.IsNullOrEmpty(firstRowInFile))
            {
                security.Remove(security[^1]);
                reader.Close();
                continue;
            }

            try
            {
                // check whether depth is in the file / смотрим стакан ли в файле

                MarketDepth trade = new();
                trade.SetMarketDepthFromString(firstRowInFile);

                // depth is in the file / в файле стаканы

                security[^1].TimeStart = trade.Time;
                security[^1].DataType = SecurityTesterDataType.MarketDepth;

                // price step / шаг цены

                decimal minPriceStep = decimal.MaxValue;
                int countFive = 0;

                CultureInfo culture = InvariantCulture;

                for (int i2 = 0; i2 < 20; i2++)
                {
                    MarketDepth tradeN = new();
                    string lastStr = reader.ReadLine();
                    try
                    {
                        tradeN.SetMarketDepthFromString(lastStr);
                    }
                    catch (Exception error)
                    {
                        Thread.Sleep(2000);
                        SendLogMessage(error.ToString(), LogMessageType.Error);
                        continue;
                    }

                    decimal open = (decimal)Convert.ToDouble(tradeN.Bids[0].Price);

                    if (open == 0)
                    {
                        open = (decimal)Convert.ToDouble(tradeN.Asks[0].Price);
                    }

                    if (open.ToString(culture).Split('.').Length > 1)
                    {
                        // if the real part takes place / если имеет место вещественная часть
                        int length = 1;

                        if (open.ToString(culture).Split('.').Length > 1 &&
                                open.ToString(culture).Split('.')[1].Length > length)
                        {
                            length = open.ToString(culture).Split('.')[1].Length;
                        }


                        if (length == 1 && minPriceStep > 0.1m)
                        {
                            minPriceStep = 0.1m;
                        }
                        if (length == 2 && minPriceStep > 0.01m)
                        {
                            minPriceStep = 0.01m;
                        }
                        if (length == 3 && minPriceStep > 0.001m)
                        {
                            minPriceStep = 0.001m;
                        }
                        if (length == 4 && minPriceStep > 0.0001m)
                        {
                            minPriceStep = 0.0001m;
                        }
                        if (length == 5 && minPriceStep > 0.00001m)
                        {
                            minPriceStep = 0.00001m;
                        }
                        if (length == 6 && minPriceStep > 0.000001m)
                        {
                            minPriceStep = 0.000001m;
                        }
                        if (length == 7 && minPriceStep > 0.0000001m)
                        {
                            minPriceStep = 0.0000001m;
                        }
                        if (length == 8 && minPriceStep > 0.00000001m)
                        {
                            minPriceStep = 0.00000001m;
                        }
                        if (length == 9 && minPriceStep > 0.000000001m)
                        {
                            minPriceStep = 0.000000001m;
                        }
                    }
                    else
                    {
                        // if the real part doesn't take place / если вещественной части нет
                        int length = 1;

                        for (int i3 = open.ToString(culture).Length - 1; open.ToString(culture)[i3] == '0'; i3--)
                        {
                            length *= 10;
                        }

                        if (minPriceStep > length)
                        {
                            minPriceStep = length;
                        }

                        if (length == 1 &&
                                open % 5 == 0)
                        {
                            countFive++;
                        }
                    }
                }


                if (minPriceStep == 1 &&
                        countFive == 20)
                {
                    minPriceStep = 5;
                }


                security[^1].Security.PriceStep = minPriceStep;
                security[^1].Security.PriceStepCost = minPriceStep;

                // last data / последняя дата
                string lastString2 = firstRowInFile;

                while (!reader.EndOfStream)
                {
                    string curRow = reader.ReadLine();

                    if (string.IsNullOrEmpty(curRow))
                    {
                        continue;
                    }

                    lastString2 = curRow;
                }

                MarketDepth trade2 = new();
                trade2.SetMarketDepthFromString(lastString2);
                security[^1].TimeEnd = trade2.Time;
            }
            catch (Exception error)
            {
                security.Remove(security[^1]);
            }

            reader.Close();
        }

        // save securities
        // сохраняем бумаги

        if (security == null ||
                security.Count == 0)
        {
            return;
        }

        _securities ??= [];

        SecuritiesTester ??= [];

        for (int i = 0; i < security.Count; i++)
        {
            if (_securities.Find(security1 => security1.Name == security[i].Security.Name) == null)
            {
                _securities.Add(security[i].Security);
            }
            SecuritiesTester.Add(security[i]);
        }

        // count the time
        // считаем время 

        if (SecuritiesTester.Count != 0)
        {
            for (int i = 0; i < SecuritiesTester.Count; i++)
            {
                if ((_timeMin == DateTime.MinValue && SecuritiesTester[i].TimeStart != DateTime.MinValue) ||
                        (SecuritiesTester[i].TimeStart != DateTime.MinValue && SecuritiesTester[i].TimeStart < _timeMin))
                {
                    _timeMin = SecuritiesTester[i].TimeStart;
                    _timeStart = SecuritiesTester[i].TimeStart;
                    _timeNow = SecuritiesTester[i].TimeStart;
                }
                if (SecuritiesTester[i].TimeEnd != DateTime.MinValue &&
                        SecuritiesTester[i].TimeEnd > _timeMax)
                {
                    _timeMax = SecuritiesTester[i].TimeEnd;
                    _timeEnd = SecuritiesTester[i].TimeEnd;
                }
            }
        }

        // check in the tester file data on the presence of multipliers and GO for securities
        // проверяем в файле тестера данные о наличии мультипликаторов и ГО для бумаг

        SetToSecuritiesDopSettings();

        TestingNewSecurityEvent?.Invoke();
    }

    private static TimeSpan GetTimeSpan(StreamReader reader)
    {
        Candle lastCandle = null;

        TimeSpan lastTimeSpan = TimeSpan.MaxValue;

        int counter = 0;

        if (!reader.EndOfStream)
        {
            lastCandle = Candle.Parse(reader.ReadLine());
        }

        while (!reader.EndOfStream)
        {
            var currentCandle = Candle.Parse(reader.ReadLine());

            var currentTimeSpan = currentCandle.TimeStart - lastCandle.TimeStart;

            lastCandle = currentCandle;

            if (currentTimeSpan < lastTimeSpan)
            {
                lastTimeSpan = currentTimeSpan;
                continue;
            }

            if (currentTimeSpan == lastTimeSpan)
            {
                counter++;
            }

            // NOTE: Rise counter to 200 mb?
            if (counter >= 100)
            {
                return lastTimeSpan;
            }
        }
        return lastTimeSpan != TimeSpan.MaxValue ? lastTimeSpan : TimeSpan.Zero;
    }



    public void SaveClearingInfo()
    {
        try
        {
            using (StreamWriter writer = new(@"Engine\" + @"TestServerClearings.txt", false))
            {
                for (int i = 0; i < ClearingTimes.Count; i++)
                {
                    writer.WriteLine(ClearingTimes[i].GetSaveString());
                }

                writer.Close();
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void LoadClearingInfo()
    {
        if (!File.Exists(@"Engine\" + @"TestServerClearings.txt"))
        {
            return;
        }

        try
        {
            using (StreamReader reader = new(@"Engine\" + @"TestServerClearings.txt"))
            {
                while (reader.EndOfStream == false)
                {
                    string str = reader.ReadLine();

                    if (str != "")
                    {
                        OrderClearing clearings = new();
                        clearings.SetFromString(str);
                        ClearingTimes.Add(clearings);
                    }
                }

                reader.Close();
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }



    public void SaveNonTradePeriods()
    {
        try
        {
            using (StreamWriter writer = new(@"Engine\" + @"TestServerNonTradePeriods.txt", false))
            {
                for (int i = 0; i < NonTradePeriods.Count; i++)
                {
                    writer.WriteLine(NonTradePeriods[i].GetSaveString());
                }

                writer.Close();
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void LoadNonTradePeriods()
    {
        if (!File.Exists(@"Engine\" + @"TestServerNonTradePeriods.txt"))
        {
            return;
        }

        try
        {
            using (StreamReader reader = new(@"Engine\" + @"TestServerNonTradePeriods.txt"))
            {
                while (reader.EndOfStream == false)
                {
                    string str = reader.ReadLine();

                    if (str != "")
                    {
                        NonTradePeriod period = new();
                        period.SetFromString(str);
                        NonTradePeriods.Add(period);
                    }
                }

                reader.Close();
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }


    #region Storage of additional security data: GO, Multipliers, Lots

    private void SetToSecuritiesDopSettings()
    {
        string pathToSecuritySettings = GetSecuritiesSettingsPath();
        List<string[]> array = LoadSecurityDopSettings(pathToSecuritySettings);

        for (int i = 0; array != null && i < array.Count; i++)
        {
            List<Security> secuAll = Securities.FindAll(s => s.Name == array[i][0]);

            if (secuAll != null && secuAll.Count != 0)
            {
                for (int i2 = 0; i2 < secuAll.Count; i2++)
                {
                    Security secu = secuAll[i2];

                    decimal lot = array[i][1].ToDecimal();
                    decimal go = array[i][2].ToDecimal();
                    decimal priceStepCost = array[i][3].ToDecimal();
                    decimal priceStep = array[i][4].ToDecimal();

                    int volDecimals = 0;

                    if (array[i].Length > 5)
                    {
                        volDecimals = Convert.ToInt32(array[i][5]);
                    }

                    if (lot != 0)
                        secu.Lot = lot;
                    if (go != 0)
                        secu.Go = go;
                    if (priceStepCost != 0)
                        secu.PriceStepCost = priceStepCost;
                    if (priceStep != 0)
                        secu.PriceStep = priceStep;
                    secu.DecimalsVolume = volDecimals;
                }
            }
        }

        for (int i = 0; i < Securities.Count; i++)
        {
            Security etalonSecurity = Securities[i];

            for (int j = 0; j < SecuritiesTester.Count; j++)
            {
                Security currentSecurity = SecuritiesTester[j].Security;
                if (currentSecurity.Name == etalonSecurity.Name
                        && currentSecurity.NameClass == etalonSecurity.NameClass)
                {
                    currentSecurity.LoadFromString(etalonSecurity.GetSaveStr());
                }
            }
        }
    }

    private List<string[]> LoadSecurityDopSettings(string path)
    {
        if (SecuritiesTester.Count == 0)
        {
            return null;
        }

        if (!File.Exists(path))
        {
            return null;
        }
        try
        {
            using (StreamReader reader = new(path))
            {
                List<string[]> array = [];

                while (!reader.EndOfStream)
                {
                    string[] set = reader.ReadLine().Split('$');
                    array.Add(set);
                }

                reader.Close();
                return array;
            }
        }
        catch (Exception)
        {
            // send to the log / отправить в лог
        }
        return null;
    }

    public void SaveSecurityDopSettings(Security securityToSave)
    {
        if (SecuritiesTester.Count == 0)
        {
            return;
        }

        for (int i = 0; i < Securities.Count; i++)
        {
            if (Securities[i].Name == securityToSave.Name)
            {
                Securities[i].LoadFromString(securityToSave.GetSaveStr());
            }
        }

        for (int i = 0; i < SecuritiesTester.Count; i++)
        {
            if (SecuritiesTester[i].Security.Name == securityToSave.Name)
            {
                SecuritiesTester[i].Security.LoadFromString(securityToSave.GetSaveStr());
            }
        }

        string pathToSettings = GetSecuritiesSettingsPath();

        List<string[]> saves = LoadSecurityDopSettings(pathToSettings);

        saves ??= [];

        CultureInfo culture = InvariantCulture;

        for (int i = 0; i < saves.Count; i++)
        { // delete the same / удаляем совпадающие

            if (saves[i][0] == securityToSave.Name)
            {
                saves.RemoveAt(i);
                i--;
            }
        }

        if (saves.Count == 0)
        {
            saves.Add(
                    [
                    securityToSave.Name,
                    securityToSave.Lot.ToString(culture),
                    securityToSave.Go.ToString(culture),
                    securityToSave.PriceStepCost.ToString(culture),
                    securityToSave.PriceStep.ToString(culture),
                    securityToSave.DecimalsVolume.ToString(culture)
                    ]);
        }

        bool isInArray = false;

        for (int i = 0; i < saves.Count; i++)
        {
            if (saves[i][0] == securityToSave.Name)
            {
                isInArray = true;
            }
        }

        if (isInArray == false)
        {
            saves.Add(
                    [
                    securityToSave.Name,
                    securityToSave.Lot.ToString(culture),
                    securityToSave.Go.ToString(culture),
                    securityToSave.PriceStepCost.ToString(culture),
                    securityToSave.PriceStep.ToString(culture),
                    securityToSave.DecimalsVolume.ToString(culture)
                    ]);
        }

        try
        {
            using (StreamWriter writer = new(pathToSettings, false))
            {
                // name, lot, GO, price step, cost of price step / Имя, Лот, ГО, Цена шага, стоимость цены шага
                for (int i = 0; i < saves.Count; i++)
                {
                    writer.WriteLine(
                            saves[i][0] + "$" +
                            saves[i][1] + "$" +
                            saves[i][2] + "$" +
                            saves[i][3] + "$" +
                            saves[i][4] + "$" +
                            saves[i][5]
                            );
                }

                writer.Close();
            }
        }
        catch (Exception)
        {
            // send to the log / отправить в лог
        }

        NeedToReconnectEvent?.Invoke();
    }

    #endregion



    public void SaveSetSecuritiesTimeFrameSettings()
    {
        try
        {
            string fileName = @"Engine\TestServerSecuritiesTf"
                + _sourceDataType.ToString()
                + TypeTesterData.ToString();

            if (_sourceDataType == TesterSourceDataType.Set)
            {
                if (string.IsNullOrEmpty(ActiveSet))
                {
                    return;
                }
                fileName += ActiveSet.RemoveExcessFromSecurityName();
            }
            else if (_sourceDataType == TesterSourceDataType.Folder)
            {
                if (string.IsNullOrEmpty(_pathToFolder))
                {
                    return;
                }
                fileName += _pathToFolder.RemoveExcessFromSecurityName();
            }

            fileName += ".txt";

            using (StreamWriter writer = new(fileName, false))
            {
                for (int i = 0; i < SecuritiesTester.Count; i++)
                {
                    writer.WriteLine(SecuritiesTester[i].Security.Name + "#" + SecuritiesTester[i].TimeFrame);
                }

                writer.Close();
            }
        }
        catch
        {
            // ignored
        }
    }

    private void LoadSetSecuritiesTimeFrameSettings()
    {
        string fileName = @"Engine\TestServerSecuritiesTf"
            + _sourceDataType.ToString()
            + TypeTesterData.ToString();

        if (_sourceDataType == TesterSourceDataType.Set)
        {
            if (string.IsNullOrEmpty(ActiveSet))
            {
                return;
            }
            fileName += ActiveSet.RemoveExcessFromSecurityName();
        }
        else if (_sourceDataType == TesterSourceDataType.Folder)
        {
            if (string.IsNullOrEmpty(_pathToFolder))
            {
                return;
            }
            fileName += _pathToFolder.RemoveExcessFromSecurityName();
        }

        fileName += ".txt";

        if (!File.Exists(fileName))
        {
            return;
        }

        try
        {
            using (StreamReader reader = new(fileName))
            {
                for (int i = 0; i < SecuritiesTester.Count; i++)
                {
                    if (reader.EndOfStream == true)
                    {
                        return;
                    }

                    string[] security = reader.ReadLine().Split('#');

                    if (SecuritiesTester[i].Security.Name != security[0])
                    {
                        return;
                    }

                    TimeFrame frame;

                    if (Enum.TryParse(security[1], out frame))
                    {
                        SecuritiesTester[i].TimeFrame = frame;
                    }
                }

                reader.Close();
            }
        }
        catch
        {
            // ignored
        }

    }
}
