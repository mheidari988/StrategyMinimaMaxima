using StockSharp.Algo.Candles;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionElement
    {
        public PriceActionElement(Candle _candle)
        {
            Candle = _candle;
        }

        public CandleMomentumType CandleMomentum
        {
            get
            {
                if (Candle.OpenPrice < Candle.ClosePrice)
                    return CandleMomentumType.Bullish;
                else
                    return CandleMomentumType.Bearish;
            }
        }
        public MomentumType MomentumType { get; set; }

        public PeakValleyStatus PeakValleyStatus { get; set; }

        public PeakValleyType PeakValleyType { get; set; }

        public Candle? Candle { get; private set; }

        public override string ToString()
        {
            return $"SeqNum:{Candle.SeqNum}{System.Environment.NewLine}Open Time:{Candle.OpenTime}{System.Environment.NewLine}" +
                $"Open: {Candle.OpenPrice} - Close:{Candle.ClosePrice} - High:{Candle.HighPrice} - Low:{Candle.LowPrice}{System.Environment.NewLine}" +
                $"Momentum:{CandleMomentum} - PeakValleyType:{PeakValleyType} - PeakValleyStatus:{PeakValleyStatus}";
        }
    }
}
