using StockSharp.Messages;
using System;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionPosition
    {
        public bool IsExecuted { get; set; } = false;
        public bool ParrentConfirmed { get; set; } = false;
        public bool ChildConfirmed { get; set; } = false;
        public Sides Direction { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal StopLossPrice { get; set; }
        public decimal TakeProfitPrice { get; set; }

        public decimal GetStopLossLevel() => Direction == Sides.Sell ?
            StopLossPrice - EntryPrice : EntryPrice - StopLossPrice;

        public decimal GetTakeProfitLevel() => Direction == Sides.Sell ?
            EntryPrice - TakeProfitPrice : TakeProfitPrice - EntryPrice;
    }
}
