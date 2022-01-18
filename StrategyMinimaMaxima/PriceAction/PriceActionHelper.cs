using System;
using StockSharp.Algo.Candles;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionHelper
    {
        public ImpulseType GetImpulseType(PriceActionSwing swing)
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

        public CorrectionType GetCorrectionType(PriceActionSwing swing)
        {

            if (swing.Leg1 == null || swing.Leg2 == null || swing.Leg3 == null)
                throw new ArgumentNullException("Swing legs cannot be null");

            switch (swing.PatternType)
            {
                case PatternType.Unknown:
                    throw new ArgumentException("The ImpulseType of PriceActionSwing is unprocessable if swing holds PatternType.Unknown.");
                    break;
                case PatternType.BullishICI:
                    return CorrectionType.Unknown;
                    break;
                case PatternType.BullishCIC:
                    if (swing.Leg1.BeginElement.CandleMomentum == CandleMomentumType.Bullish)
                    {
                        if (swing.Leg3.EndElement.Candle.ClosePrice<swing.Leg1.BeginElement.Candle.ClosePrice)
                        {
                            return CorrectionType.Brokeback;
                        }
                        else if (swing.Leg3.EndElement.Candle.LowPrice<=swing.Leg1.BeginElement.Candle.HighPrice)
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
                        if (swing.Leg3.EndElement.Candle.ClosePrice<swing.Leg1.BeginElement.Candle.OpenPrice)
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
                    break;
                case PatternType.BullishICC:
                    return CorrectionType.Unknown;
                    break;
                case PatternType.BullishCII:
                    return CorrectionType.Unknown;
                    break;
                case PatternType.BearishICI:
                    return CorrectionType.Unknown;
                    break;
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
                    break;
                case PatternType.BearishICC:
                    return CorrectionType.Unknown;
                    break;
                case PatternType.BearishCII:
                    return CorrectionType.Unknown;
                    break;
                default:
                    return CorrectionType.Unknown;
                    break;
            }
        }
    }
}
