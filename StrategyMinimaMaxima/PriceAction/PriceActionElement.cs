using StockSharp.Algo.Candles;

namespace TradeCore.PriceAction
{
    public class PriceActionElement
    {
        public PriceActionElement(Candle _candle)
        {
            Candle = _candle ?? throw new System.ArgumentNullException(nameof(_candle));
        }

        public CandleMomentumType CandleMomentum
        {
            get
            {
                if (Candle!.OpenPrice < Candle.ClosePrice)
                    return CandleMomentumType.Bullish;
                else
                    return CandleMomentumType.Bearish;
            }
        }

        public MomentumType MomentumType { get; set; }

        public HighLowType HighLowType { get; set; }

        public PeakValleyType PeakValleyType { get; set; }

        public Candle Candle { get; private set; }

        public override string ToString()
        {
            return $"SeqNum:{Candle!.SeqNum}{System.Environment.NewLine}Open DateTime: {Candle.OpenTime.DateTime.ToString("dd-MM-yyyy @ HH:mm")}{System.Environment.NewLine}" +
                $"Open: {Candle.OpenPrice} - Close:{Candle.ClosePrice} - High:{Candle.HighPrice} - Low:{Candle.LowPrice}{System.Environment.NewLine}" +
                $"CandleMomentum:{CandleMomentum} - PeakValleyType:{PeakValleyType} - HighLowType:{HighLowType}";
        }
    }
}
