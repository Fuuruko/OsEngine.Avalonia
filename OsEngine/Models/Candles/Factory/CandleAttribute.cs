namespace OsEngine.Models.Candles.Factory;

[System.AttributeUsage(System.AttributeTargets.Class)]
// TODO: Remove
public class CandleAttribute(string name) : System.Attribute
{
    public string Name { get; } = name;
}
