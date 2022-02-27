using StockSharp.Algo.Candles;
using System;
using System.Collections.Generic;

namespace TradeCore.PriceAction
{
    public class PriceActionSwing
    {
        public PriceActionSwing(PriceActionLeg leg1, PriceActionLeg leg2, PriceActionLeg leg3, List<Candle> candles)
        {
            Leg1 = leg1 ?? throw new ArgumentNullException(nameof(leg1));
            Leg2 = leg2 ?? throw new ArgumentNullException(nameof(leg2));
            Leg3 = leg3 ?? throw new ArgumentNullException(nameof(leg3));
            Candles = candles ?? throw new ArgumentNullException(nameof(candles));

            FindLowersAndHighers();
            if (!FindSwingType())
                PatternType = PatternType.Unknown;
        }

        private void FindLowersAndHighers()
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

        private bool FindSwingType()
        {
            bool tempResult = true;

            if (LowerLow == null || HigherLow == null || LowerHigh == null || HigherHigh == null)
                throw new ArgumentNullException("Neither of [ LL, HL, LH, HH ] acceptable as Null.");

            if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice >= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice >= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishICI;
                Leg1.BeginElement.HighLowType = HighLowType.LowerLow;
                Leg1.EndElement.HighLowType = HighLowType.HigherLow;
                Leg2.BeginElement.HighLowType = HighLowType.HigherLow;
                Leg2.EndElement.HighLowType = HighLowType.LowerHigh;
                Leg3.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg3.EndElement.HighLowType = HighLowType.HigherHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice >= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice >= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BullishCIC;
                Leg1.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg1.EndElement.HighLowType = HighLowType.LowerLow;
                Leg2.BeginElement.HighLowType = HighLowType.LowerLow;
                Leg2.EndElement.HighLowType = HighLowType.HigherHigh;
                Leg3.BeginElement.HighLowType = HighLowType.HigherHigh;
                Leg3.EndElement.HighLowType = HighLowType.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice >= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice <= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishICC;
                Leg1.BeginElement.HighLowType = HighLowType.LowerLow;
                Leg1.EndElement.HighLowType = HighLowType.HigherHigh;
                Leg2.BeginElement.HighLowType = HighLowType.HigherHigh;
                Leg2.EndElement.HighLowType = HighLowType.LowerHigh;
                Leg3.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg3.EndElement.HighLowType = HighLowType.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice <= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice >= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BullishCII;
                Leg1.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg1.EndElement.HighLowType = HighLowType.HigherLow;
                Leg2.BeginElement.HighLowType = HighLowType.HigherLow;
                Leg2.EndElement.HighLowType = HighLowType.LowerLow;
                Leg3.BeginElement.HighLowType = HighLowType.LowerLow;
                Leg3.EndElement.HighLowType = HighLowType.HigherHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice <= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice <= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishICI;
                Leg1.BeginElement.HighLowType = HighLowType.HigherHigh;
                Leg1.EndElement.HighLowType = HighLowType.LowerHigh;
                Leg2.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg2.EndElement.HighLowType = HighLowType.HigherLow;
                Leg3.BeginElement.HighLowType = HighLowType.HigherLow;
                Leg3.EndElement.HighLowType = HighLowType.LowerLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bullish
                && Leg3.BeginElement.Candle.LowPrice <= Leg1.BeginElement.Candle.LowPrice
                && Leg3.EndElement.Candle.HighPrice <= Leg1.EndElement.Candle.HighPrice)
            {
                PatternType = PatternType.BearishCIC;
                Leg1.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg1.EndElement.HighLowType = HighLowType.HigherHigh;
                Leg2.BeginElement.HighLowType = HighLowType.HigherHigh;
                Leg2.EndElement.HighLowType = HighLowType.LowerLow;
                Leg3.BeginElement.HighLowType = HighLowType.LowerLow;
                Leg3.EndElement.HighLowType = HighLowType.HigherLow;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice <= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice >= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishICC;
                Leg1.BeginElement.HighLowType = HighLowType.HigherHigh;
                Leg1.EndElement.HighLowType = HighLowType.LowerLow;
                Leg2.BeginElement.HighLowType = HighLowType.LowerLow;
                Leg2.EndElement.HighLowType = HighLowType.HigherLow;
                Leg3.BeginElement.HighLowType = HighLowType.HigherLow;
                Leg3.EndElement.HighLowType = HighLowType.LowerHigh;
            }
            else if (Leg1.MomentumType == MomentumType.Bearish
                && Leg3.BeginElement.Candle.HighPrice >= Leg1.BeginElement.Candle.HighPrice
                && Leg3.EndElement.Candle.LowPrice <= Leg1.EndElement.Candle.LowPrice)
            {
                PatternType = PatternType.BearishCII;
                Leg1.BeginElement.HighLowType = HighLowType.LowerHigh;
                Leg1.EndElement.HighLowType = HighLowType.HigherLow;
                Leg2.BeginElement.HighLowType = HighLowType.HigherLow;
                Leg2.EndElement.HighLowType = HighLowType.HigherHigh;
                Leg3.BeginElement.HighLowType = HighLowType.HigherHigh;
                Leg3.EndElement.HighLowType = HighLowType.LowerLow;
            }
            else
            {
                throw new Exception("Pattern Type not founded.");
            }
            return tempResult;
        }

        public static ImpulseType GetImpulseType(PriceActionSwing swing)
        {
            if (swing.Leg1 == null || swing.Leg2 == null || swing.Leg3 == null)
                throw new ArgumentNullException("Swing legs cannot be null");

            switch (swing.PatternType)
            {
                case PatternType.Unknown:
                    throw new ArgumentException("The ImpulseType of PriceActionSwing is unprocessable if swing holds PatternType.Unknown.");
                    break;
                case PatternType.BullishICI:
                    if (swing.Leg3.EndElement.Candle.ClosePrice > swing.Leg1.EndElement.Candle.HighPrice)
                        return ImpulseType.Breakout;
                    else
                        return ImpulseType.Fakeout;
                    break;
                case PatternType.BullishCIC:
                    if (swing.Leg3.BeginElement.Candle.ClosePrice > swing.Leg1.BeginElement.Candle.HighPrice)
                        return ImpulseType.Breakout;
                    else
                        return ImpulseType.Fakeout;
                    break;
                case PatternType.BullishICC:
                    return ImpulseType.Unknown;
                    break;
                case PatternType.BullishCII:
                    if (swing.Leg3.EndElement.Candle.ClosePrice > swing.Leg1.EndElement.Candle.HighPrice)
                        return ImpulseType.Breakout;
                    else
                        return ImpulseType.Fakeout;
                    break;
                case PatternType.BearishICI:
                    if (swing.Leg3.EndElement.Candle.ClosePrice < swing.Leg1.EndElement.Candle.LowPrice)
                        return ImpulseType.Breakout;
                    else
                        return ImpulseType.Fakeout;
                    break;
                case PatternType.BearishCIC:
                    if (swing.Leg3.BeginElement.Candle.ClosePrice < swing.Leg1.BeginElement.Candle.LowPrice)
                        return ImpulseType.Breakout;
                    else
                        return ImpulseType.Fakeout;
                    break;
                case PatternType.BearishICC:
                    return ImpulseType.Unknown;
                    break;
                case PatternType.BearishCII:
                    if (swing.Leg3.EndElement.Candle.ClosePrice < swing.Leg1.EndElement.Candle.LowPrice)
                        return ImpulseType.Breakout;
                    else
                        return ImpulseType.Fakeout;
                    break;
                default:
                    return ImpulseType.Unknown;
                    break;
            }
        }

        public static CorrectionType GetCorrectionType(PriceActionSwing swing, PriceActionElement? preSwingElement = null)
        {
            if (swing.Leg1 == null || swing.Leg2 == null || swing.Leg3 == null)
                throw new ArgumentException("Swing legs cannot be null");

            switch (swing.PatternType)
            {
                case PatternType.Unknown:
                    throw new ArgumentException("The ImpulseType of PriceActionSwing is unprocessable if swing holds PatternType.Unknown.");
                case PatternType.BullishICI:
                    return CorrectionType.Unknown;
                case PatternType.BullishCIC:
                    if (swing.Leg1.BeginElement.CandleMomentum == CandleMomentumType.Bullish)
                    {
                        if (swing.Leg3.EndElement.Candle.ClosePrice < swing.Leg1.BeginElement.Candle.ClosePrice)
                        {
                            return CorrectionType.Brokeback;
                        }
                        else if (swing.Leg3.EndElement.Candle.LowPrice <= swing.Leg1.BeginElement.Candle.HighPrice)
                        {
                            return CorrectionType.Retested;
                        }
                        else
                        {
                            return CorrectionType.NotRetested;
                        }
                    }
                    else
                    {
                        if (swing.Leg3.EndElement.Candle.ClosePrice < swing.Leg1.BeginElement.Candle.OpenPrice)
                        {
                            return CorrectionType.Brokeback;
                        }
                        else if (swing.Leg3.EndElement.Candle.LowPrice <= swing.Leg1.BeginElement.Candle.HighPrice)
                        {
                            return CorrectionType.Retested;
                        }
                        else
                        {
                            return CorrectionType.NotRetested;
                        }
                    }
                case PatternType.BullishICC:
                    if (preSwingElement is null) 
                        return CorrectionType.Unknown;
                    else
                    {
                        if (preSwingElement.CandleMomentum == CandleMomentumType.Bullish)
                        {
                            if (swing.Leg3.EndElement.Candle.ClosePrice > preSwingElement.Candle.ClosePrice)
                                return CorrectionType.Retested;
                            else
                                return CorrectionType.Brokeback;
                        }
                        else
                        {
                            if (swing.Leg3.EndElement.Candle.ClosePrice > preSwingElement.Candle.OpenPrice)
                                return CorrectionType.Retested;
                            else
                                return CorrectionType.Brokeback;
                        }
                    }
                case PatternType.BullishCII:
                    return CorrectionType.Unknown;
                case PatternType.BearishICI:
                    return CorrectionType.Unknown;
                case PatternType.BearishCIC:
                    if (swing.Leg1.BeginElement.CandleMomentum == CandleMomentumType.Bullish)
                    {
                        if (swing.Leg3.EndElement.Candle.ClosePrice > swing.Leg1.BeginElement.Candle.OpenPrice)
                        {
                            return CorrectionType.Brokeback;
                        }
                        else if (swing.Leg3.EndElement.Candle.HighPrice >= swing.Leg1.BeginElement.Candle.LowPrice)
                        {
                            return CorrectionType.Retested;
                        }
                        else
                        {
                            return CorrectionType.NotRetested;
                        }
                    }
                    else
                    {
                        if (swing.Leg3.EndElement.Candle.ClosePrice > swing.Leg1.BeginElement.Candle.ClosePrice)
                        {
                            return CorrectionType.Brokeback;
                        }
                        else if (swing.Leg3.EndElement.Candle.HighPrice >= swing.Leg1.BeginElement.Candle.LowPrice)
                        {
                            return CorrectionType.Retested;
                        }
                        else
                        {
                            return CorrectionType.NotRetested;
                        }
                    }
                case PatternType.BearishICC:
                    if (preSwingElement is null)
                        return CorrectionType.Unknown;
                    else
                    {
                        if (preSwingElement.CandleMomentum == CandleMomentumType.Bullish)
                        {
                            if (swing.Leg3.EndElement.Candle.ClosePrice < preSwingElement.Candle.OpenPrice)
                                return CorrectionType.Retested;
                            else
                                return CorrectionType.Brokeback;
                        }
                        else
                        {
                            if (swing.Leg3.EndElement.Candle.ClosePrice < preSwingElement.Candle.ClosePrice)
                                return CorrectionType.Retested;
                            else
                                return CorrectionType.Brokeback;
                        }
                    }
                case PatternType.BearishCII:
                    return CorrectionType.Unknown;
                default:
                    return CorrectionType.Unknown;
            }
        }

        public PriceActionLeg Leg1 { get; private set; }
        public PriceActionLeg Leg2 { get; private set; }
        public PriceActionLeg Leg3 { get; private set; }
        public PriceActionElement? LowerLow { get; private set; }
        public PriceActionElement? HigherLow { get; private set; }
        public PriceActionElement? LowerHigh { get; private set; }
        public PriceActionElement? HigherHigh { get; private set; }
        public List<Candle> Candles { get; set; }
        public PatternType PatternType { get; set; }
    }
}
