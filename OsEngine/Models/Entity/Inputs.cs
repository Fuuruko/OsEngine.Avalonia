using System;
using System.Linq;
using Newtonsoft.Json;

namespace OsEngine.Models.Entity;

public interface IBaseInput
{
    string Name { get; }
    object Value { get; internal set; }
    [JsonIgnore]
    object DefaultValue { get; }
    [JsonIgnore]
    string ToolTip { get; }
}


// public abstract class BaseParameter<T> where T : IEquatable<T>
public abstract class BaseInput<T>(string name, T value) : IBaseInput
{
    protected T _value = value;

    public string Name { get; internal set; } = name;

    // NOTE: Not just string but ShareValue like Input.String
    // because there maybe several languages and description should be changed
    // when language changed
    public string ToolTip { get; init; }
    string IBaseInput.ToolTip => ToolTip;

    // private T _originalValue;

    object IBaseInput.Value { get => Value; set => Value = (T)value; }
    public virtual T Value
    {
        get => _value;
        internal set
        {
            if (_value.Equals(value)) { return; }
            _value = value;
            OnValueChanged();
        }
    }

    [JsonIgnore]
    // NOTE: Does DefaultValue needed?
    internal T DefaultValue { get; } = value;
    object IBaseInput.DefaultValue => DefaultValue;

    public Action ValueChangeAction
    {
        private get => throw new Exception($"{ValueChangeAction} used only for event initialization");
        init => ValueChanged += value;
    }

    public static implicit operator T(BaseInput<T> p) => p.Value;

    /// <summary>
    /// Tab name where setting will be show
    /// </summary>
    // public string TabName { get; set; }

    /// <summary>
    /// Get formatted string to save to file
    /// </summary>
    internal virtual string GetStringToSave() => $"{Name}#{Value}#";

    /// <summary>
    /// Load parameter from string
    /// </summary>
    /// <param name="save">line with saved parameters</param>
    internal virtual void LoadParamFromString(string[] save)
    {
        Value = (T)Convert.ChangeType(save[1], typeof(T));
    }

    internal void CopyValueFrom(BaseInput<T> p) => _value = p.Value;

    public static bool operator ==(BaseInput<T> b1, BaseInput<T> b2)
    {
        return b1.Value.Equals(b2.Value);
    }

    public static bool operator !=(BaseInput<T> b1, BaseInput<T> b2)
    {
        return !(b1 == b2);
    }

    // public virtual BaseInput<T> Copy() => new(Name, Value);

    // public void AcceptValue() => _originalValue = Value;
    // public void CancelValue() => Value = _originalValue;

    protected void OnValueChanged() => ValueChanged?.Invoke();

    // NOTE: Maybe not needed
    /// <summary>
    /// Event: parameter state changed
    /// </summary>
    public event Action ValueChanged;
}


internal class Button(string name, Action value) : BaseInput<Action>(name, value)
{
    public override Action Value
    {
        get;
        internal set => throw new Exception("Button Value can't be changed");
    } = value ?? throw new ArgumentNullException(nameof(value));
}


public static class Input
{
    public class Bool(string name, bool value)
        : BaseInput<bool>(name, value)
    { }

    // NOTE: Used for input string like e-mail, password etc.
    // Maybe add event that check input?
    public class String(string name, string value, bool isPassword = false)
        : BaseInput<string>(name, value)
    {
        public bool IsPassword { get; } = isPassword;
    }

    public class Int : BaseInput<int>
    {
        public (int Min, int Max, int Step) Optimize = (1, 10, 1);

        public Int(string name, int value) : base(name, value) {}

        public Int(string name, int value, int min, int max, int step) : base(name, value)
        {
            Optimize = (min, max, step);
        }


        // public static int operator +(Int d1, Int d2) =>
        //     d1.Value + d2;
        //
        // public static int operator -(Int d1, Int d2) =>
        //     d1.Value - d2;
        //
        // public static int operator *(Int d1, Int d2) =>
        //     d1.Value * d2;
        //
        // public static int operator /(Int d1, Int d2) =>
        //     d1.Value / d2;
        //
        // public static bool operator >(Int d1, Int d2) =>
        //     d1.Value > d2;
        //
        // public static bool operator <(Int d1, Int d2) =>
        //     d1.Value < d2;
        //
        // public static bool operator >=(Int d1, Int d2) =>
        //     d1.Value >= d2;
        //
        // public static bool operator <=(Int d1, Int d2) =>
        //     d1.Value <= d2;
    }

    public class Decimal : BaseInput<decimal>
    {
        public (decimal Min, decimal Max, decimal Step) Optimize = (1, 10, 1);

        public Decimal(string name, decimal value) : base(name, value) {}

        public Decimal(string name, decimal value, decimal min, decimal max, decimal step) : base(name, value)
        {
            Optimize = (min, max, step);
        }


        // public static decimal operator +(Decimal d1, Decimal d2) =>
        //     d1.Value + d2.Value;
        //
        // public static decimal operator -(Decimal d1, Decimal d2) =>
        //     d1.Value - d2.Value;
        //
        // public static decimal operator *(Decimal d1, Decimal d2) =>
        //     d1.Value * d2.Value;
        //
        // public static decimal operator /(Decimal d1, Decimal d2) =>
        //     d1.Value / d2.Value;
        //
        // public static bool operator >(Decimal d1, Decimal d2) =>
        //     d1.Value > d2.Value;
        //
        // public static bool operator <(Decimal d1, Decimal d2) =>
        //     d1.Value < d2.Value;
        //
        // public static bool operator >=(Decimal d1, Decimal d2) =>
        //     d1.Value >= d2.Value;
        //
        // public static bool operator <=(Decimal d1, Decimal d2) =>
        //     d1.Value <= d2.Value;
    }

    public class Options : BaseInput<string>
    {
        internal string[] Values { get; }

        public Options(string name, string value, string[] options)
            : base(name, value)
        {
            if (!options.Contains(value))
                options = (string[])options.Prepend(value);
            Values = options;
        }

        public Options(string name, string[] options)
            : base(name, options[0])
        {
            Values = options;
        }

        // public override BaseInput<string> Copy() => new Options(Name, Value, options);

        internal override void LoadParamFromString(string[] save)
        {
            if (!Values.Contains(save[1]))
            {
                MessageBox.Show($"Chosen option({save[1]}) changed to default value({Value}) because it is not in Options list");
                OnValueChanged();
                return;
            }
            _value = save[1];
        }
    }

    // NOTE: TimeSpan or DateTime?
    public class Time(string name, TimeSpan value) : BaseInput<TimeSpan>(name, value)
    {
    }

    public class Enum<T>(string name, T value) : BaseInput<T>(name, value)
                                                 where T : struct, Enum
    {
        internal T[] Values { get; } = Enum.GetValues<T>();

        internal override void LoadParamFromString(string[] save)
        {
            if (!Enum.TryParse<T>(save[1], out var value))
            {
                MessageBox.Show($"Chosen option({save[1]}) changed to default value({Value}) because it is not in Options list");
                OnValueChanged();
                return;
            }
            _value = value;
        }
    }
}

// internal class Button(string name, Action value) : IBaseInput
// {
//     public Action Value { get; } = value
//         ?? throw new ArgumentNullException(nameof(value));
//
//     public string Name => name;
//
//     public object DefaultValue => value;
//
//     public string ToolTip { get; set; }
//
//     string IBaseInput.Name => Name;
//
//     object IBaseInput.Value => Value;
//
//     object IBaseInput.DefaultValue => DefaultValue;
//
//     string IBaseInput.ToolTip => ToolTip;
// }
//

// public class ColorInput(string name, Color value) : BaseInput<Color>(name, value)
// {
//     public ColorInput(string name, string value) : this(name, ParseColor(value))
//     {
//     }
//
//     private static Color ParseColor(string value)
//     {
//         if (Color.TryParse(value, out Color color))
//             return color;
//         else
//             throw new Exception("Color string has wrong format: " + value);
//     }
// }

// NOTE: Turn to not static class?
// Make it static in the class?
public static class Parameter
{
    // private Action? ParameterChanged;
    // private Action? OnInitialization;

    // NOTE: I dont think it should be done here,
    // like maybe after class constructor through reflection?
    // public Parameter(Action parameterChanged)
    // {
    //     ParameterChanged = parameterChanged;
    // }
    //
    // public Parameter()
    // {
    // }

    // public static BoolInput Bool(string name, bool value, string toolTip = null)
    // {
    //     BoolInput p = new(name, value);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // public static IntInput Int(string name, int value)
    // {
    //     IntInput p = new(name, value);
    //     EnumInput<TimeFrame> e = new("name", TimeFrame.Min1);
    //     var b = e.Value;
    //     Console.WriteLine(b == e);
    //
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // public static DecimalInput Decimal(string name, decimal value)
    // {
    //     DecimalInput p = new(name, value);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // public static String String(string name, string value = "")
    // {
    //     // String p = new(name, value);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // public static Options Options(string name, string value, string[] options)
    // {
    //     // Options p = new(name, value, options);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // public static Options Options(string name, string[] options)
    // {
    //     // Options p = new(name, options);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // // NOTE: Maybe not TimeSpan
    // public static TimeInput Time(string name, TimeSpan value)
    // {
    //     TimeInput p = new(name, value);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
    //
    // // public static ColorInput Color(string name, Color color)
    // // {
    // //     ColorInput p = new(name, color);
    // //     // p.ValueChanged += ParameterChanged;
    // //     return p;
    // // }
    // //
    // // public static ColorInput Color(string name, string color)
    // // {
    // //     ColorInput p = new(name, color);
    // //     // p.ValueChanged += ParameterChanged;
    // //     return p;
    // // }
    //
    // internal static Button Button(string name, Action action)
    // {
    //     Button p = new(name, action);
    //     // p.ValueChanged += ParameterChanged;
    //     return p;
    // }
}

[AttributeUsage(AttributeTargets.Field)]
public class ParameterAttribute : Attribute
{
    // public Type type;
    // public dynamic[] parameters;
    public string Name;
    public string[] Options;
    public decimal[] Optimize;



    public ParameterAttribute()
    { }

    // public ParameterAttribute(string name)
    // {
    //     Name = name;
    // }
    //
    // public ParameterAttribute(string name, decimal min = 1, decimal max = 10, decimal step = 1)
    // {
    //     Name = name;
    //     Optimize = [min, max, step];
    // }
    //
    // public ParameterAttribute(string[] options)
    // {
    //     // Name = name;
    //     Options = options;
    // }

    // public ParameterAttribute(string name, bool value)
    // {
    //     type = typeof(Bool);
    //     parameters = [name, value];
    // }
    //
    // public ParameterAttribute(string name, int value)
    // {
    //     type = typeof(Int);
    //     parameters = [name, value];
    // }
    //
    // public ParameterAttribute(string name, decimal value)
    // {
    //     type = typeof(Decimal);
    //     parameters = [name, value];
    // }
    //
    // public ParameterAttribute(string name, string value = "")
    // {
    //     type = typeof(String);
    //     parameters = [name, value];
    // }

    // public ParameterAttribute(string name, string value, string[] options)
    // {
    //     type = typeof(Options);
    //     parameters = [name, value, options];
    // }

    //
    // public ParameterAttribute(string name, string[] options)
    // {
    //     type = typeof(Options);
    //     parameters = [name, options];
    // }
    //
    // // NOTE: Maybe not TimeSpan
    // public ParameterAttribute(string name, TimeSpan value)
    // {
    //     type = typeof(Time);
    //     parameters = [name, value];
    // }
    //
    // public ParameterAttribute(string name, Color color)
    // {
    //     type = typeof(ColorParameter);
    //     parameters = [name, color];
    // }

    // public ParameterAttribute(string name, string color)
    // {
    //     ColorParameter p = new(name, color);
    //     // p.ValueChanged += ParameterChanged;
    //     // return p;
    // }

    // internal ParameterAttribute(string name, Action action)
    // {
    //     type = typeof(Button);
    //     parameters = [name, action];
    //     // p.ValueChanged += ParameterChanged;
    //     // return p;
    // }
}

[AttributeUsage(AttributeTargets.Field)]
public class InputAttribute : Attribute {  }
