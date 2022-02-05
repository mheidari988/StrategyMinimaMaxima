using System;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionLeg
    {
        public PriceActionLeg(PriceActionElement beginElement, PriceActionElement endElement)
        {
            if (beginElement.PeakValleyType == endElement.PeakValleyType)
                throw new ArgumentException("BeginElement and EndElement cannot have same PeakValleyMode.");
            if (beginElement.PeakValleyType == PeakValleyType.None || endElement.PeakValleyType == PeakValleyType.None)
                throw new ArgumentException("Neither BeginElement nor EndElement can have PeakValley.None");

            BeginElement = beginElement;
            EndElement = endElement;

            if (beginElement.PeakValleyType == PeakValleyType.Valley && endElement.PeakValleyType == PeakValleyType.Peak)
                MomentumType = MomentumType.Bullish;
            else if (beginElement.PeakValleyType == PeakValleyType.Peak && endElement.PeakValleyType == PeakValleyType.Valley)
                MomentumType = MomentumType.Bearish;
        }
        public PriceActionElement BeginElement { get; set; }
        public PriceActionElement EndElement { get; set; }

        public MomentumType MomentumType { get; private set; } = MomentumType.None;
    }
}
