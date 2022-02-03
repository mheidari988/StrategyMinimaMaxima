using System;
using System.Text;

namespace StrategyMinimaMaxima.TradeCore.PriceAction
{
    public class PriceActionLeg
    {
        #region Properties
        public PriceActionCandle BeginCandle { get; set; }
        public PriceActionCandle EndCandle { get; set; }
        public MomentumType MomentumType { get; private set; } = MomentumType.None;
        #endregion

        #region Constructors
        public PriceActionLeg(PriceActionCandle beginCandle, PriceActionCandle endCandle)
        {
            BeginCandle = beginCandle ?? throw new ArgumentNullException(nameof(beginCandle));
            EndCandle = endCandle ?? throw new ArgumentNullException(nameof(endCandle));

            if (beginCandle.PeakValleyType == endCandle.PeakValleyType)
                throw new ArgumentException("BeginCandle and EndCandle cannot have same PeakValleyType");
            if (beginCandle.PeakValleyType == PeakValleyType.None || endCandle.PeakValleyType == PeakValleyType.None)
                throw new ArgumentException("PeakValleyType cannot be None");

            if (beginCandle.PeakValleyType == PeakValleyType.Valley && endCandle.PeakValleyType == PeakValleyType.Peak)
                MomentumType = MomentumType.Bullish;
            else if (beginCandle.PeakValleyType == PeakValleyType.Peak && endCandle.PeakValleyType == PeakValleyType.Valley)
                MomentumType = MomentumType.Bearish;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine($"MomentumType: {MomentumType}");
            str.AppendLine($"BeginCandle: {BeginCandle.ToString()}");
            str.AppendLine($"EndCandle: {EndCandle.ToString()}");
            return str.ToString();
        }
        #endregion
    }
}
