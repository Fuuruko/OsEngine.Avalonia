/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

// using OsEngine.Market.Servers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OsEngine.Language;
using OsEngine.Models.Market;

namespace OsEngine.Models.Entity;

public partial class Position
{
    /// <summary>
    /// Commission type for the position
    /// </summary>
    [Obsolete($"Use {nameof(Commission)}.{nameof(Commission.Type)} instead")]
    public CommissionType CommissionType
    {
        get => Commission.Type;
        set => Commission.Type = value;
    }

    /// <summary>
    /// commission value
    /// </summary>
    [Obsolete($"Use {nameof(Commission)}.{nameof(Commission.Value)} instead")]
    public decimal CommissionValue
    {
        get => Commission.Value;
        set => Commission.Value = value;
    }

    /// <summary>
    /// Signal type to open
    /// </summary>
    [Obsolete($"Use {nameof(Signals)}.{nameof(Signals.Open)} instead")]
    public string SignalTypeOpen;

    /// <summary>
    /// Closing signal type
    /// </summary>
    [Obsolete($"Use {nameof(Signals)}.{nameof(Signals.Close)} instead")]
    public string SignalTypeClose;

    /// <summary>
    /// Closing signal type if a stop order is triggered
    /// </summary>
    [JsonIgnore]
    [Obsolete($"Use {nameof(Signals)}.{nameof(Signals.Stop)} instead")]
    public string SignalTypeStop;


    /// <summary>
    /// Closing signal type if a profit order is triggered
    /// </summary>
    [JsonIgnore]
    [Obsolete($"Use {nameof(Signals)}.{nameof(Signals.Profit)} instead")]
    public string SignalTypeProfit;



    [Obsolete("Obsolete. Use ProfitPortfolioAbs")]
    public decimal ProfitPortfolioPunkt => ProfitPortfolioAbs;

    [Obsolete("Obsolete. Use ProfitPortfolioPercent")]
    public decimal ProfitOperationPersent => ProfitPortfolioPercent;

    [Obsolete("Obsolete. Use StopOrderIsActive")]
    public bool StopOrderIsActiv
    {
        get => StopOrderIsActive;
        set => StopOrderIsActive = value;
    }

    [Obsolete("Obsolete. Use ProfitOrderIsActive")]
    public bool ProfitOrderIsActiv
    {
        get => ProfitOrderIsActive;
        set => ProfitOrderIsActive = value;
    }

    [Obsolete("Obsolete. Use CloseActive")]
    public bool CloseActiv => CloseActive;

    [Obsolete("Obsolete. Use OpenActive")]
    public bool OpenActiv => OpenActive;
}
