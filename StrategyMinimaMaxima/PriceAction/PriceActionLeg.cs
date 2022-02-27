using StockSharp.Algo.Candles;
using System;
using System.Collections.Generic;

namespace TradeCore.PriceAction
{
    public class PriceActionLeg
    {
        public PriceActionLeg(PriceActionElement beginElement, PriceActionElement endElement, List<Candle> candles)
        {
            BeginElement = beginElement ?? throw new ArgumentNullException(nameof(beginElement));
            EndElement = endElement ?? throw new ArgumentNullException(nameof(endElement));
            Candles = candles ?? throw new ArgumentNullException(nameof(candles));

            if (beginElement.PeakValleyType == endElement.PeakValleyType)
                throw new ArgumentException("BeginElement and EndElement cannot have same PeakValleyMode.");
            if (beginElement.PeakValleyType == PeakValleyType.None || endElement.PeakValleyType == PeakValleyType.None)
                throw new ArgumentException("Neither BeginElement nor EndElement can have PeakValley.None");

            if (beginElement.PeakValleyType == PeakValleyType.Valley && endElement.PeakValleyType == PeakValleyType.Peak)
                MomentumType = MomentumType.Bullish;
            else if (beginElement.PeakValleyType == PeakValleyType.Peak && endElement.PeakValleyType == PeakValleyType.Valley)
                MomentumType = MomentumType.Bearish;
        }
        public PriceActionElement BeginElement { get; init; }
        public PriceActionElement EndElement { get; init; }
        public List<Candle> Candles { get; init; }
        public MomentumType MomentumType { get; private set; } = MomentumType.None;
    }
}