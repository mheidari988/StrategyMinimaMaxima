using System;

namespace StrategyMinimaMaxima.TradeCore.PriceAction
{
    public class PriceActionSwing
    {
        #region Properties

        public PriceActionLeg Leg1 { get; }
        public PriceActionLeg Leg2 { get; }
        public PriceActionLeg Leg3 { get; }
        public PriceActionCandle? LowerLow { get; private set; }
        public PriceActionCandle? HigherLow { get; private set; }
        public PriceActionCandle? LowerHigh { get; private set; }
        public PriceActionCandle? HigherHigh { get; private set; }
        public SwingType SwingType { get; set; } = SwingType.Unknown;

        #endregion

        #region Constructors
        public PriceActionSwing(PriceActionLeg leg1, PriceActionLeg leg2, PriceActionLeg leg3)
        {
            Leg1 = leg1 ?? throw new ArgumentNullException(nameof(leg1));
            Leg2 = leg2 ?? throw new ArgumentNullException(nameof(leg2));
            Leg3 = leg3 ?? throw new ArgumentNullException(nameof(leg3));

            findLowersAndHighers();
        }
        #endregion

        #region Private Methods
        private void findLowersAndHighers()
        {
            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg2.MomentumType == MomentumType.Bearish
                && Leg3.MomentumType == MomentumType.Bullish)
            {
                LowerLow = Leg1.BeginCandle ?? throw new ArgumentNullException(nameof(Leg1.BeginCandle));
                Leg1.BeginCandle.PeakValleyType = PeakValleyType.Valley;

                HigherLow = Leg1.EndCandle ?? throw new ArgumentNullException(nameof(Leg1.EndCandle));
                Leg1.EndCandle.PeakValleyType = PeakValleyType.Peak;
                Leg2.BeginCandle.PeakValleyType = PeakValleyType.Peak;

                LowerHigh = Leg3.BeginCandle ?? throw new ArgumentNullException(nameof(Leg3.BeginCandle));
                Leg3.BeginCandle.PeakValleyType = PeakValleyType.Valley;
                Leg2.EndCandle.PeakValleyType = PeakValleyType.Valley;

                HigherHigh = Leg3.EndCandle ?? throw new ArgumentNullException(nameof(Leg3.EndCandle));
                Leg3.EndCandle.PeakValleyType = PeakValleyType.Peak;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg2.MomentumType == MomentumType.Bullish
                && Leg3.MomentumType == MomentumType.Bearish)
            {
                HigherHigh = Leg1.BeginCandle ?? throw new ArgumentNullException(nameof(Leg1.BeginCandle));
                Leg1.BeginCandle.PeakValleyType = PeakValleyType.Peak;

                LowerHigh = Leg1.EndCandle ?? throw new ArgumentNullException(nameof(Leg1.EndCandle));
                Leg1.EndCandle.PeakValleyType = PeakValleyType.Valley;
                Leg2.BeginCandle.PeakValleyType = PeakValleyType.Valley;

                HigherLow = Leg3.BeginCandle ?? throw new ArgumentNullException(nameof(Leg3.BeginCandle));
                Leg3.BeginCandle.PeakValleyType = PeakValleyType.Peak;
                Leg2.EndCandle.PeakValleyType = PeakValleyType.Peak;

                LowerLow = Leg3.EndCandle ?? throw new ArgumentNullException(nameof(Leg3.EndCandle));
                Leg3.EndCandle.PeakValleyType = PeakValleyType.Valley;
            }
            else
                throw new ArgumentException("Legs' MomentumType is not acceptable.");

            findSwingType();
        }
        private void findSwingType()
        {
            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginCandle.LowPrice >= Leg1.BeginCandle.LowPrice
                && Leg3.EndCandle.HighPrice >= Leg1.EndCandle.HighPrice)
            {
                SwingType = SwingType.BullishICI;
                Leg1.BeginCandle.HighLowType = HighLowType.LowerLow;
                Leg1.EndCandle.HighLowType = HighLowType.HigherLow;
                Leg2.BeginCandle.HighLowType = HighLowType.HigherLow;
                Leg2.EndCandle.HighLowType = HighLowType.LowerHigh;
                Leg3.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg3.EndCandle.HighLowType = HighLowType.HigherHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginCandle.HighPrice >= Leg1.BeginCandle.HighPrice
                && Leg3.EndCandle.LowPrice >= Leg1.EndCandle.LowPrice)
            {
                SwingType = SwingType.BullishCIC;
                Leg1.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg1.EndCandle.HighLowType = HighLowType.LowerLow;
                Leg2.BeginCandle.HighLowType = HighLowType.LowerLow;
                Leg2.EndCandle.HighLowType = HighLowType.HigherHigh;
                Leg3.BeginCandle.HighLowType = HighLowType.HigherHigh;
                Leg3.EndCandle.HighLowType = HighLowType.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginCandle.LowPrice >= Leg1.BeginCandle.LowPrice
                && Leg3.EndCandle.HighPrice <= Leg1.EndCandle.HighPrice)
            {
                SwingType = SwingType.BullishICC;
                Leg1.BeginCandle.HighLowType = HighLowType.LowerLow;
                Leg1.EndCandle.HighLowType = HighLowType.HigherHigh;
                Leg2.BeginCandle.HighLowType = HighLowType.HigherHigh;
                Leg2.EndCandle.HighLowType = HighLowType.LowerHigh;
                Leg3.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg3.EndCandle.HighLowType = HighLowType.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginCandle.LowPrice <= Leg1.BeginCandle.LowPrice
                && Leg3.EndCandle.HighPrice >= Leg1.EndCandle.HighPrice)
            {
                SwingType = SwingType.BullishCII;
                Leg1.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg1.EndCandle.HighLowType = HighLowType.HigherLow;
                Leg2.BeginCandle.HighLowType = HighLowType.HigherLow;
                Leg2.EndCandle.HighLowType = HighLowType.LowerLow;
                Leg3.BeginCandle.HighLowType = HighLowType.LowerLow;
                Leg3.EndCandle.HighLowType = HighLowType.HigherHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginCandle.HighPrice <= Leg1.BeginCandle.HighPrice
                && Leg3.EndCandle.LowPrice <= Leg1.EndCandle.LowPrice)
            {
                SwingType = SwingType.BearishICI;
                Leg1.BeginCandle.HighLowType = HighLowType.HigherHigh;
                Leg1.EndCandle.HighLowType = HighLowType.LowerHigh;
                Leg2.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg2.EndCandle.HighLowType = HighLowType.HigherLow;
                Leg3.BeginCandle.HighLowType = HighLowType.HigherLow;
                Leg3.EndCandle.HighLowType = HighLowType.LowerLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginCandle.LowPrice <= Leg1.BeginCandle.LowPrice
                && Leg3.EndCandle.HighPrice <= Leg1.EndCandle.HighPrice)
            {
                SwingType = SwingType.BearishCIC;
                Leg1.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg1.EndCandle.HighLowType = HighLowType.HigherHigh;
                Leg2.BeginCandle.HighLowType = HighLowType.HigherHigh;
                Leg2.EndCandle.HighLowType = HighLowType.LowerLow;
                Leg3.BeginCandle.HighLowType = HighLowType.LowerLow;
                Leg3.EndCandle.HighLowType = HighLowType.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginCandle.HighPrice <= Leg1.BeginCandle.HighPrice
                && Leg3.EndCandle.LowPrice >= Leg1.EndCandle.LowPrice)
            {
                SwingType = SwingType.BearishICC;
                Leg1.BeginCandle.HighLowType = HighLowType.HigherHigh;
                Leg1.EndCandle.HighLowType = HighLowType.LowerLow;
                Leg2.BeginCandle.HighLowType = HighLowType.LowerLow;
                Leg2.EndCandle.HighLowType = HighLowType.HigherLow;
                Leg3.BeginCandle.HighLowType = HighLowType.HigherLow;
                Leg3.EndCandle.HighLowType = HighLowType.LowerHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginCandle.HighPrice >= Leg1.BeginCandle.HighPrice
                && Leg3.EndCandle.LowPrice <= Leg1.EndCandle.LowPrice)
            {
                SwingType = SwingType.BearishCII;
                Leg1.BeginCandle.HighLowType = HighLowType.LowerHigh;
                Leg1.EndCandle.HighLowType = HighLowType.HigherLow;
                Leg2.BeginCandle.HighLowType = HighLowType.HigherLow;
                Leg2.EndCandle.HighLowType = HighLowType.HigherHigh;
                Leg3.BeginCandle.HighLowType = HighLowType.HigherHigh;
                Leg3.EndCandle.HighLowType = HighLowType.LowerLow;
            }
            else 
                throw new ArgumentException("SwingType is not recognizable");
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"SwingType: {SwingType}";
        }
        #endregion
    }
}
