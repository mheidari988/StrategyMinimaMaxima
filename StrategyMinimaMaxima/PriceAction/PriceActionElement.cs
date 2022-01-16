using StockSharp.Algo.Candles;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionElement
    {
        public PriceActionElement(Candle _candle)
        {
            Candle = _candle;
        }

        public MomentumType MomentumType { get; set; }

        public PeakValleyStatus PeakValleyStatus { get; set; }

        public PeakValleyType PeakValleyType { get; set; }

        public Candle? Candle { get; private set; }

    }
}
