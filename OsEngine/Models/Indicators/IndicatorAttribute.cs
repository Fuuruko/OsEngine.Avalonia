using System;

namespace OsEngine.Models.Indicators
{
    /// <summary>
    /// Attribute for applying indicators to terminal
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IndicatorAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}
