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
            if (leg1 == null || leg2 == null || leg3 == null)
                throw new ArgumentNullException("Null elements could not process");
            Leg1 = leg1;
            Leg2 = leg2;
            Leg3 = leg3;
            findLowersAndHighers();
            if (!findBullishSwingType())
                PatternType = PatternType.Unknown;
        }

        private void findLowersAndHighers()
        {
            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg2.MomentumType == MomentumType.Bearish
                && Leg3.MomentumType == MomentumType.Bullish)
            {
                LowerLow = Leg1.BeginElement;
                HigherLow = Leg1.EndElement;
                LowerHigh = Leg3.BeginElement;
                HigherHigh = Leg3.EndElement;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg2.MomentumType == MomentumType.Bullish
                && Leg3.MomentumType == MomentumType.Bearish)
            {
                HigherHigh = Leg1.BeginElement;
                LowerHigh = Leg1.EndElement;
                HigherLow = Leg3.BeginElement;
                LowerLow = Leg3.EndElement;
            }
            else
                throw new ArgumentOutOfRangeException("Legs' MomentumMode is not acceptable.");
        }

        private bool findBullishSwingType()
        {
            bool tempResult = true;

            if (LowerLow == null || HigherLow == null || LowerHigh == null || HigherHigh == null)
                throw new ArgumentNullException("Neither of [ LL, HL, LH, HH ] acceptable as Null.");

            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice > Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice > Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishICI;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice > Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice > Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BullishCIC;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice > Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice < Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishICC;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice < Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice > Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishCII;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice < Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice < Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishICI;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice < Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice < Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BearishCIC;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice < Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice > Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishICC;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice > Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice < Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishCII;
            }
            else
            {
                tempResult = false;
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
