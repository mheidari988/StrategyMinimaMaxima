using StrategyMinimaMaxima.PriceAction.Signal;
using System;

namespace StrategyMinimaMaxima.PriceAction.Position
{
    public class PositionEntity
    {
        public Guid Id { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal EntryPoint { get; set; }
        public SignalPattern SignalPattern { get; set; }
        public SignalDirection SignalDirection { get; set; }
        public PositionState PositionState { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public decimal GetRRResult()
        {
            if (StopLoss != 0 && TakeProfit != 0 && EntryPoint != 0)
                return TradeCalc.GetRRByPosition(this);
            else
                return 0;
        }
    }
}
