using StockSharp.Algo.Candles;
using System;

namespace StrategyMinimaMaxima.TradeCore.PriceAction
{
    public class PriceActionCandle : TimeFrameCandle
    {
        #region Properties
        public PeakValleyType PeakValleyType { get; set; } = PeakValleyType.None;
        public HighLowType HighLowType { get; set; } = HighLowType.None;

        public MomentumType MomentumType
        {
            get
            {
                if (OpenPrice < ClosePrice)
                    return MomentumType.Bullish;
                else if (OpenPrice > ClosePrice)
                    return MomentumType.Bearish;
                else
                    return MomentumType.None;
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return base.ToString() + Environment.NewLine
                + $"PeakValleyType: {PeakValleyType}, HighLowType: {HighLowType}, MomentumType: {MomentumType}";
        }
        #endregion
    }
}
