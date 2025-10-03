
namespace OsEngine.Models.Entity;

public enum SignalType
{
    Buy,

    Sell,

    /// <summary>
    /// CloseAll
    /// закрыть все позиции
    /// </summary>
    CloseAll,

    /// <summary>
    /// CloseOne position
    /// закрыть одну позицию
    /// </summary>
    CloseOne,

    None,

    /// <summary>
    /// set new stop
    /// выставить новый стоп
    /// </summary>
    ReloadStop,

    /// <summary>
    /// set new takeprofit
    /// выставить новый профит
    /// </summary>
    ReloadProfit,

    /// <summary>
    /// open new deal
    /// открыть новую сделку
    /// </summary>
    OpenNew,

    /// <summary>
    /// delete position
    /// удалить позицию
    /// </summary>
    DeletePos,

    /// <summary>
    /// delete all positions
    /// удалить все позиции
    /// </summary>
    DeleteAllPoses,

    FindPosition
}
