/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
// using System.Windows.Forms;
// using OsEngine.Entity;

namespace OsEngine.Models.Entity
{
    /// <summary>
    /// Parameter interface
    /// </summary>
    public interface IStrategyParameter
    {
        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        string GetStringToSave();

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        void LoadParamFromString(string[] save);

        /// <summary>
        /// Parameter type
        /// </summary>
        StrategyParameterType Type { get; }

        /// <summary>
        /// Owner tab name
        /// </summary>
        string TabName { get; set; }

        /// <summary>
        /// Event: parameter state changed
        /// </summary>
        event Action ValueChange;
    }


    // TODO: Should change value only after accept
    public abstract class StrategyParameter<T>
    {
        public string Name { get; protected set; }

        private T _originalValue;

        public virtual T Value
        {
            get;
            set
            {
                if (field.Equals(value)) { return; }
                field = value;
                OnValueChange();
            }
        }

        public T DefaultValue { get; protected set; }

        /// <summary>
        /// Tab name where setting will be show
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public abstract string GetStringToSave();

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public abstract void LoadParamFromString(string[] save);

        public void AcceptValue() => _originalValue = Value;
        public void CancelValue() => Value = _originalValue;

        // public static implicit operator T(StrategyParameter<T> p)
        // {
        //     return p.Value;
        // }

        // NOTE: Maybe not needed
        /// <summary>
        /// Event: parameter state changed
        /// </summary>
        public event Action ValueChange;

        public void OnValueChange() => ValueChange?.Invoke();
    }

    // public class Parameter
    // {

        // public override string GetStringToSave() => $"{Name}#{Value}#{DefaultValue}#";
        //
        // public override void LoadParamFromString(string[] save)
        // {
        //     Name = save[0];
        //     Value = Convert.ToBoolean(save[1]);
        //
        //     try
        //     {
        //         DefaultValue = Convert.ToBoolean(save[2]);
        //     }
        //     catch
        //     {
        //         // ignore
        //     }
        // }
    // }

    public class BoolStrategyParameter : StrategyParameter<bool>
    {
        /// <param name="name">Parameter name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="tabName">Owner tab name</param>
        /// <exception cref="Exception">The parameter name of the robot contains a special character. This will cause errors. Take it away</exception>
        public BoolStrategyParameter(string name, bool defaultValue, string tabName = null)
        {
            if (name.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }
            Name = name;
            DefaultValue = defaultValue;
            Value = defaultValue;
            TabName = tabName;
        }

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public override string GetStringToSave() => $"{Name}#{Value}#{DefaultValue}#";

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public override void LoadParamFromString(string[] save)
        {
            Name = save[0];
            Value = Convert.ToBoolean(save[1]);

            try
            {
                DefaultValue = Convert.ToBoolean(save[2]);
            }
            catch
            {
                // ignore
            }
        }
    }

    public class IntStrategyParameter : StrategyParameter<int>
    {
        public IntStrategyParameter(string name, int defaultValue, string tabName = null)
        {
            if (name.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }

            // NOTE: This should be done inside Optimizer not parameter
            // if (start > stop)
            // {
            //     throw new Exception("The initial value of the parameter cannot be greater than the last");
            // }

            Name = name;
            Value = defaultValue;
            DefaultValue = defaultValue;
            TabName = tabName;
        }

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public override string GetStringToSave() => $"{Name}#{Value}#{DefaultValue}#";

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public override void LoadParamFromString(string[] save)
        {
            Value = Convert.ToInt32(save[1]);

            try
            {
                DefaultValue = Convert.ToInt32(save[2]);
            }
            catch
            {
                // ignore 
            }

        }
    }

    public class DecimalStrategyParameter : StrategyParameter<decimal>
    {
        /// <summary>
        /// Designer for creating a parameter storing Decimal type variables
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Default value</param>
        /// <param name="start">First value in optimization</param>
        /// <param name="stop">last value in optimization</param>
        /// <param name="step">Step change in optimization</param>
        public DecimalStrategyParameter(string name, decimal value, string tabName = null)
        {
            if (name.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }
            // if (start > stop)
            // {
            //     throw new Exception("The initial value of the parameter cannot be greater than the last");
            // }

            Name = name;
            Value = value;
            DefaultValue = value;
            TabName = tabName;
        }

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public override string GetStringToSave() => $"{Name}#{Value}#{DefaultValue}#";

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public override void LoadParamFromString(string[] save)
        {
            try
            {
                Value = save[1].ToDecimal();
                DefaultValue = save[2].ToDecimal();
            }
            catch
            {
                // ignore
            }
        }
    }

    public class ChoiceStrategyParameter : StrategyParameter<string>
    {
        public List<string> Choices { get; protected set; }

        /// <param name="name">Parameter name</param>
        /// <param name="value">Default value</param>
        /// <param name="choices">Possible value options</param>
        public ChoiceStrategyParameter(string name, string value, List<string> choices, string tabName = null)
        {
            if (name.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }

            if (!choices.Contains(value)) { choices.Add(value); }

            Name = name;
            Value = value;
            DefaultValue = value;
            Choices = choices;
            TabName = tabName;
        }

        /// <param name="name">Parameter name</param>
        /// <param name="value">Default value</param>
        public ChoiceStrategyParameter(string name, string value, string tabName = null) : this(name, value, [], tabName) {  }

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public override string GetStringToSave()
        {
            string save = $"{Name}#{Value}#";

            for (int i = 0; i < Choices.Count; i++)
            {
                save += $"{Choices[i]}#";
            }

            return save;
        }

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public override void LoadParamFromString(string[] save)
        {
            Value = save[1];

            Choices = [];
            for (int i = 2; i < save.Length; i++)
            {
                if (string.IsNullOrEmpty(save[i])) { continue; }
                Choices.Add(save[i]);
            }
        }
    }

    public class TimeStrategyParameter : StrategyParameter<TimeOnly>
    {
        public override TimeOnly Value { get; set; }

        /// <summary>
        /// Constructor to create a parameter storing TimeOfDay variables
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="tabName">Owner tab name</param>
        /// <exception cref="Exception">The parameter name of the robot contains a special character. This will cause errors. Take it away</exception>
        public TimeStrategyParameter(string name, TimeOnly time, string tabName = null)
        {
            if (name.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }
            Name = name;
            Value = time;
            DefaultValue = time;
            TabName = tabName;
        }


        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public override string GetStringToSave()
        {
            string save = $"{Name}#{Value.ToString(CultureInfo.InvariantCulture)}#";

            return save;
        }

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public override void LoadParamFromString(string[] save)
        {
            // NOTE: Should be checked
            bool parsed = TimeOnly.TryParse(save[1], out TimeOnly value);
            if (parsed)
            {
                Value = value;
                OnValueChange();
            }

            // if (Value.LoadFromString(save[1]) &&
            //         ValueChange != null)
            // {
            //     ValueChange();
            // }
        }
    }

    /// <summary>
    /// Parameter for label type strategy
    /// </summary>
    public class StrategyParameterLabel : IStrategyParameter
    {
        /// <summary>
        ///  Constructor to create a parameter storing Int variables
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="label">Displayed label</param>
        /// <param name="value">Displayed value</param>
        /// <param name="rowHeight">Row height</param>
        /// <param name="textHeight">Text height</param>
        /// <param name="color">Displayed color</param>
        /// <param name="tabName">Owner tab name</param>
        /// <exception cref="Exception">the parameter name of the robot contains a special character. This will cause errors. Take it away</exception>
        public StrategyParameterLabel(string name, string label, string value, int rowHeight, int textHeight, 
            System.Drawing.Color color, string tabName = null)
        {
            if (name.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }

            _name = name;
            Label = label;
            Value = value;
            TabName = tabName;
            RowHeight = rowHeight;
            TextHeight = textHeight;
            Color = color;
        }

        /// <summary>
        /// Uniq parameter name
        /// </summary>
        public string Name { get { return _name; } }

        private string _name;

        /// <summary>
        /// Displayed label
        /// </summary>
        public string Label;

        /// <summary>
        /// Displayed value
        /// </summary>
        public string Value;

        /// <summary>
        /// Row height
        /// </summary>
        public int RowHeight;

        /// <summary>
        /// Text height
        /// </summary>
        public int TextHeight;

        /// <summary>
        /// Displayed color
        /// </summary>
        public System.Drawing.Color Color;

        /// <summary>
        /// Parameter type
        /// </summary>
        public StrategyParameterType Type { get { return StrategyParameterType.Label; } }

        /// <summary>
        /// Owner tab name
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// Event: parameter state changed
        /// </summary>
        public event Action ValueChange;

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public string GetStringToSave()
        {
            string save = _name + "#";

            save += Label + "#";
            save += Value + "#";
            save += RowHeight + "#";
            save += TextHeight + "#";
            save += Color.ToArgb() + "#";

            return save;
        }

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public void LoadParamFromString(string[] save)
        {
            try
            {
                Label = save[1];
                Value = save[2];
                RowHeight = Convert.ToInt32(save[3]);
                TextHeight = Convert.ToInt32(save[4]);
                Color = System.Drawing.Color.FromArgb(Convert.ToInt32(save[5]));
            }
            catch
            {
                // ignore 
            }
        }
    }

    /// <summary>
    /// Represents a time of day without a date
    /// </summary>
    public class TimeOfDay
    {
        public int Hour;

        public int Minute;

        public int Second;

        public int Millisecond;

        public override string ToString()
        {
            string result = Hour + ":";
            result += Minute + ":";
            result += Second + ":";
            result += Millisecond;

            return result;
        }

        /// <summary>
        /// Download settings from the save file
        /// </summary>
        /// <param name="save">Data array from storage</param>
        public bool LoadFromString(string save)
        {
            string[] array = save.Split(':');

            bool paramUpdated = false;

            if (Hour != Convert.ToInt32(array[0]))
            {
                Hour = Convert.ToInt32(array[0]);
                paramUpdated = true;
            }
            if (Minute != Convert.ToInt32(array[1]))
            {
                Minute = Convert.ToInt32(array[1]);
                paramUpdated = true;
            }
            if (Second != Convert.ToInt32(array[2]))
            {
                Second = Convert.ToInt32(array[2]);
                paramUpdated = true;
            }
            if (Millisecond != Convert.ToInt32(array[3]))
            {
                Millisecond = Convert.ToInt32(array[3]);
                paramUpdated = true;
            }

            return paramUpdated;
        }

        public static bool operator >(TimeOfDay c1, DateTime c2)
        {
            if (c1.Hour > c2.Hour)
            {
                return true;
            }

            if (c1.Hour >= c2.Hour
                && c1.Minute > c2.Minute)
            {
                return true;
            }

            if (c1.Hour >= c2.Hour
                && c1.Minute >= c2.Minute
                && c1.Second > c2.Second)
            {
                return true;
            }

            if (c1.Hour >= c2.Hour
                && c1.Minute >= c2.Minute
                && c1.Second >= c2.Second
                && c1.Millisecond > c2.Millisecond)
            {
                return true;
            }

            return false;
        }

        public static bool operator <(TimeOfDay c1, DateTime c2)
        {
            if (c1.Hour < c2.Hour)
            {
                return true;
            }

            if (c1.Hour == c2.Hour
                && c1.Minute < c2.Minute)
            {
                return true;
            }

            if (c1.Hour == c2.Hour
                && c1.Minute == c2.Minute
                && c1.Second < c2.Second)
            {
                return true;
            }

            if (c1.Hour == c2.Hour
                && c1.Minute == c2.Minute
                && c1.Second == c2.Second
                && c1.Millisecond < c2.Millisecond)
            {
                return true;
            }

            return false;
        }

        public static bool operator ==(TimeOfDay c1, DateTime c2)
        {
            if (c1.Hour != c2.Hour)
            {
                return false;
            }
            if (c1.Minute != c2.Minute)
            {
                return false;
            }
            if (c1.Second != c2.Second)
            {
                return false;
            }
            if (c1.Millisecond != c2.Millisecond)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(TimeOfDay c1, DateTime c2)
        {
            if (c1.Hour != c2.Hour)
            {
                return true;
            }
            if (c1.Minute != c2.Minute)
            {
                return true;
            }
            if (c1.Second != c2.Second)
            {
                return true;
            }
            if (c1.Millisecond != c2.Millisecond)
            {
                return true;
            }

            return false;
        }

        public static bool operator >(TimeOfDay c1, TimeOfDay c2)
        {
            if (c1.Hour > c2.Hour)
            {
                return true;
            }

            if (c1.Hour >= c2.Hour
                && c1.Minute > c2.Minute)
            {
                return true;
            }

            if (c1.Hour >= c2.Hour
                && c1.Minute >= c2.Minute
                && c1.Second > c2.Second)
            {
                return true;
            }

            if (c1.Hour >= c2.Hour
                && c1.Minute >= c2.Minute
                && c1.Second >= c2.Second
                && c1.Millisecond > c2.Millisecond)
            {
                return true;
            }

            return false;
        }

        public static bool operator <(TimeOfDay c1, TimeOfDay c2)
        {
            if (c1.Hour < c2.Hour)
            {
                return true;
            }

            if (c1.Hour == c2.Hour
                && c1.Minute < c2.Minute)
            {
                return true;
            }

            if (c1.Hour == c2.Hour
                && c1.Minute == c2.Minute
                && c1.Second < c2.Second)
            {
                return true;
            }

            if (c1.Hour == c2.Hour
                && c1.Minute == c2.Minute
                && c1.Second == c2.Second
                && c1.Millisecond < c2.Millisecond)
            {
                return true;
            }

            return false;
        }

        public static bool operator ==(TimeOfDay c1, TimeOfDay c2)
        {
            if (c1.Hour != c2.Hour)
            {
                return false;
            }
            if (c1.Minute != c2.Minute)
            {
                return false;
            }
            if (c1.Second != c2.Second)
            {
                return false;
            }
            if (c1.Millisecond != c2.Millisecond)
            {
                return false;
            }

            return true;
        }

        public static bool operator !=(TimeOfDay c1, TimeOfDay c2)
        {
            if (c1.Hour != c2.Hour)
            {
                return true;
            }
            if (c1.Minute != c2.Minute)
            {
                return true;
            }
            if (c1.Second != c2.Second)
            {
                return true;
            }
            if (c1.Millisecond != c2.Millisecond)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Represents the time interval since the beginning of the day
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                TimeSpan time = new TimeSpan(0, Hour, Minute, Second);

                return time;
            }
        }
    }

    /// <summary>
    /// A strategy parameter to button click
    /// </summary>
    public class StrategyParameterButton : IStrategyParameter
    {
        /// <summary>
        /// Designer for creating a parameter storing Button type variables
        /// </summary>
        /// <param name="buttonLabel">Button content</param>
        /// <param name="tabName">Owner tab name</param>
        /// <exception cref="Exception">The parameter name of the robot contains a special character. This will cause errors. Take it away</exception>
        public StrategyParameterButton(string buttonLabel, string tabName = null)
        {
            if (buttonLabel.HaveExcessInString())
            {
                throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
            }
            _name = buttonLabel;
            _type = StrategyParameterType.Button;
            TabName = tabName;
        }

        /// <summary>
        /// Owner tab name
        /// </summary>
        public string TabName { get; set; }

        /// <summary>
        /// Blank. it is impossible to create a variable of StrategyParameter type with an empty constructor
        /// </summary>
        private StrategyParameterButton()
        {

        }

        /// <summary>
        /// Get formatted string to save to file
        /// </summary>
        public string GetStringToSave()
        {
            string save = _name + "#";

            return save;
        }

        /// <summary>
        /// Load parameter from string
        /// </summary>
        /// <param name="save">line with saved parameters</param>
        public void LoadParamFromString(string[] save)
        {
        }

        /// <summary>
        /// Uniq parameter name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        private string _name;

        /// <summary>
        /// Parameter type
        /// </summary>
        public StrategyParameterType Type
        {
            get { return _type; }
        }

        private StrategyParameterType _type;

        /// <summary>
        /// Event: parameter state changed
        /// </summary>
        public event Action ValueChange;

        /// <summary>
        /// Trigger a button click event
        /// </summary>
        public void Click()
        {
            UserClickOnButtonEvent?.Invoke();
        }

        /// <summary>
        /// Event: click on button
        /// </summary>
        public event Action UserClickOnButtonEvent;
    }

    /// <summary>
    /// A strategy parameter to check box
    /// </summary>
    // public class StrategyParameterCheckBox : IStrategyParameter
    // {
    //     /// <summary>
    //     /// Designer for creating a parameter storing CheckBox type variables
    //     /// </summary>
    //     /// <param name="checkBoxLabel">Displayed name</param>
    //     /// <param name="isChecked">Current value</param>
    //     /// <param name="tabName">Owner tab name</param>
    //     /// <exception cref="Exception">The parameter name of the robot contains a special character. This will cause errors. Take it away</exception>
    //     public StrategyParameterCheckBox(string checkBoxLabel, bool isChecked, string tabName = null)
    //     {
    //
    //         if (checkBoxLabel.HaveExcessInString())
    //         {
    //             throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
    //         }
    //         _name = checkBoxLabel;
    //         _type = StrategyParameterType.CheckBox;
    //
    //         if (isChecked == true)
    //         {
    //             _checkState = CheckState.Checked;
    //         }
    //         else
    //         {
    //             _checkState = CheckState.Unchecked;
    //         }
    //
    //         TabName = tabName;
    //     }
    //
    //     /// <summary>
    //     /// Owner tab name
    //     /// </summary>
    //     public string TabName { get; set; }
    //
    //     /// <summary>
    //     /// Blank. it is impossible to create a variable of StrategyParameter type with an empty constructor
    //     /// </summary>
    //     private StrategyParameterCheckBox()
    //     {
    //
    //     }
    //
    //     /// <summary>
    //     /// Get formatted string to save to file
    //     /// </summary>
    //     public string GetStringToSave()
    //     {
    //         string save = _name + "#";
    //
    //         if (_checkState == CheckState.Checked)
    //         {
    //             save += "true" + "#";
    //         }
    //         else
    //         {
    //             save += "false" + "#";
    //         }
    //
    //         return save;
    //     }
    //
    //     /// <summary>
    //     /// Load parameter from string
    //     /// </summary>
    //     /// <param name="save">line with saved parameters</param>
    //     public void LoadParamFromString(string[] save)
    //     {
    //         _name = save[0];
    //
    //         try
    //         {
    //             if (save[1] == "true")
    //             {
    //                 _checkState = CheckState.Checked;
    //             }
    //             else
    //             {
    //                 _checkState = CheckState.Unchecked;
    //             }
    //         }
    //         catch
    //         {
    //             // ignore
    //         }
    //     }
    //
    //     /// <summary>
    //     /// Uniq parameter name
    //     /// </summary>
    //     public string Name
    //     {
    //         get { return _name; }
    //     }
    //
    //     private string _name;
    //
    //     /// <summary>
    //     /// Parameter state
    //     /// </summary>
    //     public CheckState CheckState
    //     {
    //         get
    //         {
    //             return _checkState;
    //         }
    //         set
    //         {
    //             if (_checkState == value)
    //             {
    //                 return;
    //             }
    //             _checkState = value;
    //             if (ValueChange != null)
    //             {
    //                 ValueChange();
    //             }
    //         }
    //     }
    //
    //     private CheckState _checkState;
    //
    //     /// <summary>
    //     /// Parameter type
    //     /// </summary>
    //     public StrategyParameterType Type
    //     {
    //         get { return _type; }
    //     }
    //
    //     private StrategyParameterType _type;
    //
    //     /// <summary>
    //     /// Event: parameter state changed
    //     /// </summary>
    //     public event Action ValueChange;
    // }

    /// <summary>
    /// The parameter of the Decimal type strategy with CheckBox
    /// </summary>
    // public class StrategyParameterDecimalCheckBox : IStrategyParameter
    // {
    //     /// <summary>
    //     /// Designer for creating a parameter storing Decimal type variables
    //     /// </summary>
    //     /// <param name="name">Parameter name</param>
    //     /// <param name="value">Default value</param>
    //     /// <param name="start">First value in optimization</param>
    //     /// <param name="stop">last value in optimization</param>
    //     /// <param name="step">Step change in optimization</param>
    //     /// <param name="isChecked">is it active</param>
    //     public StrategyParameterDecimalCheckBox(string name, decimal value, decimal start, decimal stop, decimal step, 
    //         bool isChecked, string tabName = null)
    //     {
    //         if (name.HaveExcessInString())
    //         {
    //             throw new Exception("The parameter name of the robot contains a special character. This will cause errors. Take it away");
    //         }
    //         if (start > stop)
    //         {
    //             throw new Exception("The initial value of the parameter cannot be greater than the last");
    //         }
    //
    //         _name = name;
    //         _valueDecimal = value;
    //         _valueDecimalDefolt = value;
    //         _valueDecimalStart = start;
    //         _valueDecimalStop = stop;
    //         _valueDecimalStep = step;
    //
    //         if (isChecked == true)
    //         {
    //             _checkState = CheckState.Checked;
    //         }
    //         else
    //         {
    //             _checkState = CheckState.Unchecked;
    //         }
    //
    //         _type = StrategyParameterType.DecimalCheckBox;
    //         TabName = tabName;
    //     }
    //
    //     /// <summary>
    //     /// Blank. it is impossible to create a variable of StrategyParameter type with an empty constructor
    //     /// </summary>
    //     private StrategyParameterDecimalCheckBox()
    //     {
    //
    //     }
    //
    //     /// <summary>
    //     /// Get formatted string to save to file
    //     /// </summary>
    //     public string GetStringToSave()
    //     {
    //         string save = _name + "#";
    //         save += _valueDecimal + "#";
    //         save += _valueDecimalDefolt + "#";
    //         save += _valueDecimalStart + "#";
    //         save += _valueDecimalStop + "#";
    //         save += _valueDecimalStep + "#";
    //
    //         if (_checkState == CheckState.Checked)
    //         {
    //             save += "true" + "#";
    //         }
    //         else
    //         {
    //             save += "false" + "#";
    //         }
    //
    //         return save;
    //     }
    //
    //     /// <summary>
    //     /// Load parameter from string
    //     /// </summary>
    //     /// <param name="save">line with saved parameters</param>
    //     public void LoadParamFromString(string[] save)
    //     {
    //         _valueDecimal = save[1].ToDecimal();
    //
    //         try
    //         {
    //             _valueDecimalDefolt = save[2].ToDecimal();
    //             _valueDecimalStart = save[3].ToDecimal();
    //             _valueDecimalStop = save[4].ToDecimal();
    //             _valueDecimalStep = save[5].ToDecimal();
    //
    //             if (save[6] == "true")
    //             {
    //                 _checkState = CheckState.Checked;
    //             }
    //             else
    //             {
    //                 _checkState = CheckState.Unchecked;
    //             }
    //         }
    //         catch
    //         {
    //             // ignore
    //         }
    //     }
    //
    //     /// <summary>
    //     /// Uniq parameter name
    //     /// </summary>
    //     public string Name
    //     {
    //         get { return _name; }
    //     }
    //
    //     private string _name;
    //
    //     /// <summary>
    //     /// Owner tab name
    //     /// </summary>
    //     public string TabName
    //     {
    //         get; set;
    //     }
    //
    //     /// <summary>
    //     /// Parameter type
    //     /// </summary>
    //     public StrategyParameterType Type
    //     {
    //         get { return _type; }
    //     }
    //
    //     private StrategyParameterType _type;
    //
    //     /// <summary>
    //     /// Current value of the Decimal parameter
    //     /// </summary>
    //     public decimal ValueDecimal
    //     {
    //         get
    //         {
    //             return _valueDecimal;
    //         }
    //         set
    //         {
    //             if (_valueDecimal == value)
    //             {
    //                 return;
    //             }
    //             _valueDecimal = value;
    //             if (ValueChange != null)
    //             {
    //                 ValueChange();
    //             }
    //         }
    //     }
    //
    //     private decimal _valueDecimal;
    //
    //     /// <summary>
    //     /// Default value for the Decimal type
    //     /// </summary>
    //     public decimal ValueDecimalDefolt
    //     {
    //         get
    //         {
    //             return _valueDecimalDefolt;
    //         }
    //     }
    //
    //     private decimal _valueDecimalDefolt;
    //
    //     /// <summary>
    //     /// Initial value of the Decimal type parameter
    //     /// </summary>
    //     public decimal ValueDecimalStart
    //     {
    //         get
    //         {
    //             return _valueDecimalStart;
    //         }
    //     }
    //
    //     private decimal _valueDecimalStart;
    //
    //     /// <summary>
    //     /// The last value of the Decimal type parameter
    //     /// </summary>
    //     public decimal ValueDecimalStop
    //     {
    //         get
    //         {
    //             return _valueDecimalStop;
    //         }
    //     }
    //
    //     private decimal _valueDecimalStop;
    //
    //     /// <summary>
    //     /// Incremental step of the Decimal type parameter
    //     /// </summary>
    //     public decimal ValueDecimalStep
    //     {
    //         get
    //         {
    //             return _valueDecimalStep;
    //         }
    //     }
    //
    //     private decimal _valueDecimalStep;
    //
    //     /// <summary>
    //     /// CheckBox is it active
    //     /// </summary>
    //     public CheckState CheckState
    //     {
    //         get
    //         {
    //             return _checkState;
    //         }
    //         set
    //         {
    //             if (_checkState == value)
    //             {
    //                 return;
    //             }
    //             _checkState = value;
    //             if (ValueChange != null)
    //             {
    //                 ValueChange();
    //             }
    //         }
    //     }
    //
    //     private CheckState _checkState;
    //
    //     /// <summary>
    //     /// Event: parameter state changed
    //     /// </summary>
    //     public event Action ValueChange;
    // }

    /// <summary>
    /// Parameter type
    /// </summary>
    public enum StrategyParameterType
    {
        /// <summary>
        /// An integer number with the type Int
        /// </summary>
        Int,

        /// <summary>
        /// A floating point number of the decimal type
        /// </summary>
        Decimal,

        /// <summary>
        /// String
        /// </summary>
        String,

        /// <summary>
        /// Boolean value
        /// </summary>
        Bool,

        /// <summary>
        /// Time of day
        /// </summary>
        TimeOfDay,

        /// <summary>
        /// Pressing a button
        /// </summary>
        Button,

        /// <summary>
        /// inscription in the parameters window
        /// </summary>
        Label,

        /// <summary>
        /// checkbox in the parameters window
        /// </summary>
        CheckBox,

        /// <summary>
        /// A floating point number of the decimal type with CheckBox
        /// </summary>
        DecimalCheckBox
		
    }
}
