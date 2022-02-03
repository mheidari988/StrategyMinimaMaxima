using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionSwing
    {
        public PriceActionSwing(PriceActionLeg leg1, PriceActionLeg leg2, PriceActionLeg leg3)
        {
            Leg1 = leg1 ?? throw new ArgumentNullException(nameof(leg1));
            Leg2 = leg2 ?? throw new ArgumentNullException(nameof(leg2));
            Leg3 = leg3 ?? throw new ArgumentNullException(nameof(leg3));
            findLowersAndHighers();
            if (!findSwingType())
                PatternType = PatternType.Unknown;
        }

        private void findLowersAndHighers()
        {
            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg2.MomentumType == MomentumType.Bearish
                && Leg3.MomentumType == MomentumType.Bullish)
            {
                LowerLow = Leg1.BeginElement;
                Leg1.BeginElement.PeakValleyType = PeakValleyType.Valley;

                HigherLow = Leg1.EndElement;
                Leg1.EndElement.PeakValleyType = PeakValleyType.Peak;
                Leg2.BeginElement.PeakValleyType = PeakValleyType.Peak;

                LowerHigh = Leg3.BeginElement;
                Leg3.BeginElement.PeakValleyType = PeakValleyType.Valley;
                Leg2.EndElement.PeakValleyType = PeakValleyType.Valley;

                HigherHigh = Leg3.EndElement;
                Leg3.EndElement.PeakValleyType = PeakValleyType.Peak;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg2.MomentumType == MomentumType.Bullish
                && Leg3.MomentumType == MomentumType.Bearish)
            {
                HigherHigh = Leg1.BeginElement;
                Leg1.BeginElement.PeakValleyType = PeakValleyType.Peak;

                LowerHigh = Leg1.EndElement;
                Leg1.EndElement.PeakValleyType = PeakValleyType.Valley;
                Leg2.BeginElement.PeakValleyType = PeakValleyType.Valley;

                HigherLow = Leg3.BeginElement;
                Leg3.BeginElement.PeakValleyType = PeakValleyType.Peak;
                Leg2.EndElement.PeakValleyType = PeakValleyType.Peak;

                LowerLow = Leg3.EndElement;
                Leg3.EndElement.PeakValleyType = PeakValleyType.Valley;
            }
            else
                throw new ArgumentOutOfRangeException("Legs' MomentumMode is not acceptable.");
        }

        private bool findSwingType()
        {
            bool tempResult = true;

            if (LowerLow == null || HigherLow == null || LowerHigh == null || HigherHigh == null)
                throw new ArgumentNullException("Neither of [ LL, HL, LH, HH ] acceptable as Null.");

            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice >= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice >= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishICI;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice >= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice >= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BullishCIC;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice >= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice <= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishICC;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice <= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice >= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishCII;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice <= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice <= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishICI;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice <= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice <= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BearishCIC;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice <= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice >= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishICC;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice >= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice <= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishCII;
                Leg1.BeginElement.PeakValleyStatus = PeakValleyStatus.LowerHigh;
                Leg1.EndElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg2.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherLow;
                Leg2.EndElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg3.BeginElement.PeakValleyStatus = PeakValleyStatus.HigherHigh;
                Leg3.EndElement.PeakValleyStatus = PeakValleyStatus.LowerLow;
            }
            else
            {
                throw new Exception("Pattern Type not founded.");
            }
            return tempResult;
        }

        public PriceActionLeg Leg1 { get; private set; }
        public PriceActionLeg Leg2 { get; private set; }
        public PriceActionLeg Leg3 { get; private set; }
        public PriceActionElement? LowerLow { get; private set; }
        public PriceActionElement? HigherLow { get; private set; }
        public PriceActionElement? LowerHigh { get; private set; }
        public PriceActionElement? HigherHigh { get; private set; }
        public PatternType PatternType { get; set; }
    }
}
