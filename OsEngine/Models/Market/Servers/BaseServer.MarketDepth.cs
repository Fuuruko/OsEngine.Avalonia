using System;
using System.Collections.Generic;
using OsEngine.Models.Entity;
using OsEngine.Models.Entity.Server;

namespace OsEngine.Models.Market.Servers;

public partial class BaseServer
{
    /// <summary>
    /// last market depths by securities
    /// </summary>
    private List<MarketDepth> _depths = [];

    /// <summary>
    /// array blocker with market depths against multithreaded access
    /// </summary>
    private object _depthsArrayLocker = new();

    /// <summary>
    /// last bid and ask values by securities
    /// </summary>
    private List<BidAskSender> _lastBidAskValues = [];

    /// <summary>
    /// send the incoming market depth to the top
    /// </summary>
    private void TrySendMarketDepthEvent(MarketDepth newMarketDepth)
    {
        if (NewMarketDepthEvent == null
            || !_needToUseFullMarketDepth2.Value)
        {
            return;
        }

        _marketDepthsToSend.Enqueue(newMarketDepth);

        if (!_needToLoadBidAskInTrades2.Value) { return; }

        bool isInArray = false;

        for (int i = 0; i < _depths.Count; i++)
        {
            if (_depths[i].SecurityNameCode == newMarketDepth.SecurityNameCode)
            {
                _depths[i] = newMarketDepth;
                isInArray = true;
            }
        }

        if (isInArray == false)
        {
            lock (_depthsArrayLocker)
            {
                _depths.Add(newMarketDepth);
            }
        }
    }

    /// <summary>
    /// send the incoming bid ask values to the top
    /// </summary>
    private void TrySendBidAsk(MarketDepth newMarketDepth)
    {
        if (NewBidAscIncomeEvent == null)
        {
            return;
        }

        decimal bestBid = 0;
        if (newMarketDepth.Bids != null &&
            newMarketDepth.Bids.Count > 0)
        {
            bestBid = newMarketDepth.Bids[0].Price;
        }

        decimal bestAsk = 0;
        if (newMarketDepth.Asks != null &&
            newMarketDepth.Asks.Count > 0)
        {
            bestAsk = newMarketDepth.Asks[0].Price;
        }

        if (bestBid == 0 &&
            bestAsk == 0)
        {
            return;
        }

        Security sec = GetSecurityForName(newMarketDepth.SecurityNameCode, "");

        if (sec == null)
        {
            return;
        }

        for (int i = 0; i < _lastBidAskValues.Count; i++)
        {
            if (_lastBidAskValues[i].Security.Name == sec.Name)
            {
                if (_lastBidAskValues[i].Bid == bestBid &&
                    _lastBidAskValues[i].Ask == bestAsk)
                {
                    return;
                }
            }
        }

        BidAskSender newSender = new()
        {
            Bid = bestBid,
            Ask = bestAsk,
            Security = sec
        };

        _bidAskToSend.Enqueue(newSender);

        bool isInArray = false;

        for (int i = 0; i < _lastBidAskValues.Count; i++)
        {
            if (_lastBidAskValues[i].Security.Name == sec.Name)
            {
                _lastBidAskValues[i] = newSender;
                isInArray = true;
                break;
            }
        }

        if (isInArray == false)
        {
            _lastBidAskValues.Add(newSender);
        }
    }

    /// <summary>
    /// best bid or ask changed for the instrument
    /// </summary>
    public event Action<decimal, decimal, Security> NewBidAscIncomeEvent;

    /// <summary>
    /// new depth in the system
    /// </summary>
    public event Action<MarketDepth> NewMarketDepthEvent;
}
