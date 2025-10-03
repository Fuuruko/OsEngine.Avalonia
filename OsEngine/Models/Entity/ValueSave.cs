using System.Collections.Generic;

namespace OsEngine.Models.Entity;

/// <summary>
/// object to store intermediate data by index
/// </summary>
public class ValueSave
{
    public string Name;
    public List<Candle> ValueCandles;
}
