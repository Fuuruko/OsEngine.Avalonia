/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.IO;
using OsEngine.Models.Candles;
using OsEngine.Models.Entity;
using OsEngine.Models.Market.Servers.Tester;

namespace OsEngine.Models.Market.Connectors;

public partial class ConnectorCandles
{
    /// <summary>
    /// upload settings
    /// </summary>
    private void Load()
    {
        if (!File.Exists(@"Engine\" + UniqueName + @"ConnectorPrime.txt"))
        {
            return;
        }
        try
        {
            using (StreamReader reader = new(@"Engine\" + UniqueName + @"ConnectorPrime.txt"))
            {

                PortfolioName = reader.ReadLine();
                EmulatorIsOn = Convert.ToBoolean(reader.ReadLine());
                _securityName = reader.ReadLine();
                Enum.TryParse(reader.ReadLine(), true, out ServerType);
                _securityClass = reader.ReadLine();

                if (reader.EndOfStream == false)
                {
                    _eventsIsOn = Convert.ToBoolean(reader.ReadLine());
                }
                else
                {
                    _eventsIsOn = true;
                }

                if (reader.EndOfStream == false)
                {
                    ServerFullName = reader.ReadLine();
                }
                else
                {
                    ServerFullName = ServerType.ToString();
                }

                reader.Close();
            }
        }
        catch
        {
            _eventsIsOn = true;
            // ignore
        }
    }

    /// <summary>
    /// save settings in file
    /// </summary>
    public void Save()
    {
        if (_canSave == false)
        {
            return;
        }
        if (StartProgram == StartProgram.IsOsOptimizer)
        {
            return;
        }
        try
        {
            using (StreamWriter writer = new(@"Engine\" + UniqueName + @"ConnectorPrime.txt", false))
            {
                writer.WriteLine(PortfolioName);
                writer.WriteLine(EmulatorIsOn);
                writer.WriteLine(SecurityName);
                writer.WriteLine(ServerType);
                writer.WriteLine(SecurityClass);
                writer.WriteLine(EventsIsOn);
                writer.WriteLine(ServerFullName);

                writer.Close();
            }
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// delete object and clear memory
    /// </summary>
    public void Delete()
    {
        // if (_ui != null)
        // {
        //     _ui.Close();
        // }

        _needToStopThread = true;

        if (StartProgram != StartProgram.IsOsOptimizer)
        {
            try
            {
                if (File.Exists(@"Engine\" + UniqueName + @"ConnectorPrime.txt"))
                {
                    File.Delete(@"Engine\" + UniqueName + @"ConnectorPrime.txt");
                }
            }
            catch
            {
                // ignore
            }

            // FIX:
            // ServerMaster.RevokeOrderToEmulatorEvent -= ServerMaster_RevokeOrderToEmulatorEvent;
        }

        if (CandleSeries != null)
        {
            MyServer?.StopThisSecurity(CandleSeries);

            CandleSeries.CandleUpdateEvent -= MySeries_CandleUpdateEvent;
            CandleSeries.CandleFinishedEvent -= MySeries_CandleFinishedEvent;
            CandleSeries.Stop();
            CandleSeries.Clear();
            CandleSeries = null;
        }

        if (_emulator != null)
        {
            _emulator.MyTradeEvent -= ConnectorBot_NewMyTradeEvent;
            _emulator.OrderChangeEvent -= ConnectorBot_NewOrderIncomeEvent;
        }

        if (MyServer != null)
        {
            if (MyServer is TesterServer testerServer)
            {
                testerServer.TestingEndEvent -= Connector_TestingEndEvent;
                testerServer.TestingStartEvent -= Connector_TestingStartEvent;
            }

            UnSubscribeOnServer(MyServer);
            MyServer = null;
        }

        if (TimeFrameBuilder != null)
        {
            TimeFrameBuilder = null;
        }

        _securityName = null;
        OptionMarketData = null;
        Funding = null;
        SecurityVolumes = null;

    }
}
