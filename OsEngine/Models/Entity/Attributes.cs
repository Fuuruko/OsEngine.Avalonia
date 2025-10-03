using System;

namespace OsEngine.Models.Entity;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public class NameAttribute(string name) : Attribute
{
    public string Name = name;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public class DescriptionAttribute(string description) : Attribute
{
    public string Description = description;
}
