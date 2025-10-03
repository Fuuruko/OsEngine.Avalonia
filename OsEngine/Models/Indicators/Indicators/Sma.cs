using System.Collections.Generic;
using OsEngine.Models.Entity;

namespace OsEngine.Models.Indicators.Indicators
{
    public class SMA : BaseIndicator
    {
        [Parameter]
        public Input.Int Length = new("Length", 14);
        // [Parameter("Length")]
        // private Int _length = Parameter.Int("Length", 14);

        [Parameter]
        public Input.Options CandlePoint = new("Type", ["Close"]);
        // [Parameter("Type", ["Close"])]
        // private Options _candlePoint = Parameter.Options("Candle Point", ["Close"]);

        // NOTE: Can it be done with interface?
        // Like intreface add indexer for this Series
        [MainSeries]
        private BaseSeries Series = new("SMA")
        {
            ChartSeriesType = ChartSeriesType.Line,
            Color = Colors.DodgerBlue,
        };

        public override void OnStateChange(IndicatorState state)
        {
            // _candlePoint = CreateParameterStringCollection("Candle Point", "Close", Entity.CandlePointsArray);
        }

        public override bool PreProcess(List<Candle> candles) => candles.Count >= Length;

        public override void OnProcess(List<Candle> candles, int index)
        {
            if (Length > index) { return; }

            Series[index] = candles.Sum(index - Length, index, CandlePoint) / Length;
        }
    }
}
