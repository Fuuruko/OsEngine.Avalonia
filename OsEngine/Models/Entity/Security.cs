/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;

namespace OsEngine.Models.Entity
{
    public class Security
    {
        public string Name;
        public string FullName;

        [Obsolete(nameof(FullName))]
        public string NameFull;

        /// <summary>
        /// class code
        /// код класса
        /// </summary>
        // NOTE: Or ClassCode?
        public string ClassName;
        [Obsolete(nameof(ClassName))]
        public string NameClass;

        /// <summary>
        /// Unique tool identifier.
        /// It is used in some platforms as the main instrument key in the trading system.
        /// Уникальный идентификатор инструмента.
        ///  Используется в некоторых платформах как главный ключ инструмента в торговой системе.
        /// </summary>
        public string NameId;

        /// <summary>
        /// Биржа на которой торгуется инструмент
        /// </summary>
        public string Exchange
        {
            get;
            set => field = string.Intern(value);
        }

        /// <summary>
        /// the trading status of this instrument on the stock exchange
        /// состояние торгов этим инструментом на бирже
        /// </summary>
        // NOTE: Maybe not needed
        public SecurityStateType State;

        /// <summary>
        /// price step, i.e. minimal price change for the instrument
        /// шаг цены, т.е. минимальное изменение цены для инструмента
        /// </summary>
        public decimal PriceStep
        {
            get;
            set
            {
                field = value;

                if (field >= 1 || field == 0)
                {
                    _decimals = 0;
                    return;
                }

                string step = field.ToString(CultureInfo.InvariantCulture);
                string[] parts = step.Split('.');

                _decimals = parts.Length > 1 ? parts[1].TrimEnd('0').Length : 0;
            }
        }

        public decimal Lot;

        /// <summary>
        /// the cost of a step of the price, i.e. how much profit is dripping on the deposit for one step of the price
        /// стоимость шага цены, т.е. сколько профита капает на депозит за один шаг цены
        /// </summary>
        public decimal PriceStepCost;

        /// <summary>
        /// warranty coverage
        /// гарантийное обеспечение
        /// </summary>
        public decimal Margin;
        [Obsolete($"Use {nameof(Margin)} instead")]
        public decimal Go;

        /// <summary>
        /// security type
        /// тип бумаги
        /// </summary>
        public SecurityType Type;
        [Obsolete($"Use {nameof(Type)} instead")]
        public SecurityType SecurityType;

        /// <summary>
        /// open the Paper Settings window
        /// вызвать окно настроек бумаги
        /// </summary>
        public void ShowDialog()
        {
            // FIX: Disabled
            // SecurityUi ui = new SecurityUi(this);
            // ui.ShowDialog();
        }

        /// <summary>
        /// the number of decimal places of the instrument price.
        /// if the price step is higher, or the raver 1, for example 10, then it still returns 0
        /// количество знаков после запятой цены инструмента.
        /// если шаг цены больше, либо равер 1, например 10, то возвращается всё равно 0
        /// </summary>
        public int Decimals
        {
            get
            {
                if (_decimals != 0)
                {
                    return _decimals;
                }

                if (PriceStep >= 1
                        || PriceStep == 0)
                {
                    return 0;
                }

                string numString = PriceStep.ToString(CultureInfo.InvariantCulture);
                string[] parts = numString.Split('.');

                if (parts.Length > 1)
                {
                    _decimals = parts[1].TrimEnd('0').Length;
                }
                return _decimals;
            }
            // NOTE: Maybe not needed as
            // when pricestep changes decimal can be defined
            set
            {
                if (value >= 0)
                {
                    _decimals = value;
                }
            }
        }
        private int _decimals = -1;

        /// <summary>
        /// the number of decimal places of the instrument volume
        /// количество знаков после запятой объёма инструмента
        /// </summary>
        public int DecimalsVolume;

        /// <summary>
        /// volume step by asset
        /// шаг объёма для актива
        /// </summary>
        public decimal VolumeStep;

        /// <summary>
        /// minimum order volume by asset
        /// минимальный объём ордера по активу
        /// </summary>
        public decimal MinTradeAmount = 1;

        /// <summary>
        /// type field MinTradeAmount. Contracts / Contract currency
        /// тип поля MinTradeAmount. Контракты / Валюта контракта
        /// </summary>
        public MinTradeAmountType MinTradeAmountType;

        /// <summary>
        /// Lower price limit for bids. If you place an order with a price lower - the system will reject
        /// Нижний лимит цены для заявок. Если выставить ордер с ценой ниже - система отвергнет
        /// </summary>
        [Obsolete($"Use {nameof(PriceLowLimit)} instead")]
        public decimal PriceLimitLow
        {
            get => PriceLowLimit;
            set => PriceLowLimit = value;
        }
        public decimal PriceLowLimit = 0;

        /// <summary>
        /// Upper price limit for bids. If you place an order with a price higher - the system will reject
        /// Верхний лимит цены для заявок. Если выставить ордер с ценой выше - система отвергнет
        /// </summary>
        [Obsolete($"Use {nameof(PriceHighLimit)} instead")]
        public decimal PriceLimitHigh
        {
            get => PriceHighLimit;
            set => PriceHighLimit = value;
        }
        public decimal PriceHighLimit = decimal.MaxValue;

        /// <summary>
        /// expiration date
        /// дата экспирации
        /// </summary>
        public DateTime Expiration;

        // For options

        // NOTE: Create if SecurityType is Option
        public SecurityOption Option { get => field ??= new(); }

        public OptionType OptionType;

        public decimal Strike;

        /// <summary>
        /// Underlying asset (for Options)
        /// Базовый актив (для опционов)
        /// </summary>
        public string UnderlyingAsset;


        // save and load
        // сохранение и загрузка

        /// <summary>
        /// upload from the line
        /// загрузить из строки
        /// </summary>
        public void LoadFromString(string save)
        {
            save = save.Replace("\r", "");

            string[] array = save.Split('\n');

            Name = array[0];
            NameClass = array[1];
            NameFull = array[2];
            NameId = array[3];
            Enum.TryParse(array[4],out State);
            PriceStep = array[5].ToDecimal();
            Lot = array[6].ToDecimal();
            PriceStepCost = array[7].ToDecimal();
            Go = array[8].ToDecimal();
            Enum.TryParse(array[9],out SecurityType);
            _decimals = Convert.ToInt32(array[10]);
            PriceLimitLow = array[11].ToDecimal();
            PriceLimitHigh = array[12].ToDecimal();
            Enum.TryParse(array[13], out OptionType);
            Strike = array[14].ToDecimal();
            Expiration = Convert.ToDateTime(array[15]);
            DecimalsVolume = Convert.ToInt32(array[16]);
            MinTradeAmount = array[17].ToDecimal();

            if (array.Length > 18)
            {
                VolumeStep = array[18].ToDecimal();
            }
            if (array.Length > 19)
            {
                Enum.TryParse(array[19], out MinTradeAmountType);
            }
        }

        /// <summary>
        /// save the line
        /// взять строку сохранения
        /// </summary>
        public string GetSaveStr()
        {
            string result = Name + "\n";
            result += NameClass + "\n";
            result += NameFull + "\n";
            result += NameId + "\n";
            result += State + "\n";
            result += PriceStep + "\n";
            result += Lot + "\n";
            result += PriceStepCost + "\n";
            result += Go + "\n";
            result += SecurityType + "\n";
            result += _decimals + "\n";
            result += PriceLimitLow + "\n";
            result += PriceLimitHigh + "\n";
            result += OptionType + "\n";
            result += Strike + "\n";
            result += Expiration + "\n";
            result += DecimalsVolume + "\n";
            result += MinTradeAmount + "\n";
            result += VolumeStep +"\n";
            result += MinTradeAmountType;

            return result;
        }

        public class SecurityOption
        {
            public OptionType Type;

            public decimal Strike;

            /// <summary>
            /// Underlying asset (for Options)
            /// Базовый актив (для опционов)
            /// </summary>
            // NOTE: For some reason used for futeres in aexchange
            public string UnderlyingAsset;
        }
    }

    public enum MinTradeAmountType
    {
        /// <summary>
        /// The minimum volume is specified in the contracts
        /// </summary>
        Contract,

        /// <summary>
        /// Minimum volume is specified in contract currency (USDT / RUB)
        /// </summary>
        C_Currency
    }

    /// <summary>
    /// stock market conditions
    /// состояние бумаги на бирже
    /// </summary>
    /// NOTE: Mostly not used. Only to show that something is tradable or not?
    public enum SecurityStateType
    {
        /// <summary>
        /// we don't know if the bidding's going on
        /// неизвестно, идут ли торги
        /// </summary>
        // NOTE: Used only quikdde
        Unknown,
        // TODO: Delete
        UnKnown = Unknown,

        /// <summary>
        /// trading on the paper is active
        /// торги по бумаге активны
        /// </summary>
        Active,
        // TODO: Delete
        Activ = Active,

        /// <summary>
        /// paper auction is closed.
        /// торги по бумаге закрыты
        /// </summary>
        Close
    }

    /// <summary>
    /// instrumental type
    /// тип инструмента
    /// </summary>
    public enum SecurityType
    {
        None,

        /// <summary>
        /// currency. Including crypt
        /// валюта. В т.ч. и крипта
        /// </summary>
        CurrencyPair,

        /// <summary>
        /// акция
        /// </summary>
        Stock,

        /// <summary>
        /// облигация
        /// </summary>
        Bond,

        /// <summary>
        /// futures
        /// фьючерс
        /// </summary>
        Futures,

        /// <summary>
        /// option
        /// опцион
        /// </summary>
        Option,

        /// <summary>
        /// index индекс
        /// </summary>
        Index,

        /// <summary>
        /// exchange fund
        /// </summary>
        Fund
    }

    /// <summary>
    /// option type
    /// тип опциона
    /// </summary>
    public enum OptionType
    {
        // NOTE: Not used anywhere
        None,
        Put,
        Call
    }
}
