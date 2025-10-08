using System;

namespace OsEngine.Models.Market.Servers.Tester;

public partial class TesterServer
{
    public string ActiveSet { get; private set; }

    public bool ProfitMarketIsOn
    {
        get { return _profitMarketIsOn; }
        set
        {
            _profitMarketIsOn = value;
            // Save();
            OnPropertyChanged(nameof(ProfitMarketIsOn));
        }
    }
    private bool _profitMarketIsOn = true;

    private DateTime _timeMin;
    private DateTime _timeMax;
    private DateTime _timeStart
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ProgressMax));
        }
    }
    private DateTime _timeEnd
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ProgressMax));
        }
    }

    private DateTime _timeNow
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged(nameof(ProgressValue));
        }
    }

    // NOTE: Maybe can be moved to ViewModel
    public double ProgressMax => (_timeEnd - _timeStart).TotalMinutes;
    public double ProgressValue => (_timeNow - _timeStart).TotalMinutes;


    public TesterDataType TypeTesterData
    {
        get => _typeTesterData;
        set
        {
            if (_typeTesterData == value)
            {
                return;
            }

            if (_candleManager != null)
            {
                _candleManager.Clear();
                _candleManager.TypeTesterData = value;
            }
            _typeTesterData = value;
            Save();
            ReloadSecurities();
        }

    }
    private TesterDataType _typeTesterData = TesterDataType.Candle;

    public OrderExecutionType OrderExecutionType
    {
        get { return _orderExecutionType; }
        set
        {
            _orderExecutionType = value;
            Save();
            OnPropertyChanged(nameof(OrderExecutionType));
        }
    }
    private OrderExecutionType _orderExecutionType;

    public int SlippageToSimpleOrder
    {
        get { return _slippageToSimpleOrder; }
        set
        {
            _slippageToSimpleOrder = value;
            Save();
            OnPropertyChanged(nameof(SlippageToSimpleOrder));
        }
    }
    private int _slippageToSimpleOrder = 0;

    public int SlippageToStopOrder
    {
        get { return _slippageToStopOrder; }
        set
        {
            _slippageToStopOrder = value;
            Save();
            OnPropertyChanged(nameof(SlippageToStopOrder));
        }
    }
    private int _slippageToStopOrder = 0;


    public void TestingFastOnOff()
    {
        // if (TesterRegime == TesterRegime.NotActive)
        // {
        //     return;
        // }
        if (_dataIsActive == false)
        {
            return;
        }

        Regime = TesterRegime.Play;

        if (TestingFastIsActivate == false)
        {
            TestingFastIsActivate = true;
        }
        else
        {
            TestingFastIsActivate = false;
        }

        TestingFastEvent?.Invoke();
    }

    public void TestingPausePlay()
    {
        if (Regime == TesterRegime.NotActive)
        {
            return;
        }
        if (Regime == TesterRegime.Play)
        {
            Regime = TesterRegime.Pause;
        }
        else
        {
            Regime = TesterRegime.Play;
        }
    }

    public void TestingPlusOne()
    {
        if (Regime == TesterRegime.NotActive)
        {
            return;
        }
        Regime = TesterRegime.PlusOne;
    }

    public void ToNextPositionActionTestingFast()
    {
        if (Regime == TesterRegime.NotActive)
        {
            return;
        }
        _waitSomeActionInPosition = true;

        if (TestingFastIsActivate == false)
        {
            TestingFastOnOff();
        }
    }
}
