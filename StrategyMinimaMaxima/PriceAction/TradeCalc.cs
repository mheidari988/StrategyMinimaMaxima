using StrategyMinimaMaxima.PriceAction.Position;
using StrategyMinimaMaxima.PriceAction.Signal;
using System;

namespace StrategyMinimaMaxima.PriceAction
{
    public static class TradeCalc
    {
        public static decimal T272(decimal firstPoint, decimal secondPoint)
            => decimal.Add(decimal.Multiply(decimal.Subtract(secondPoint, firstPoint), 1.272M), firstPoint);
        public static decimal T618(decimal firstPoint, decimal secondPoint) 
            => decimal.Add(decimal.Multiply(decimal.Subtract(secondPoint, firstPoint), 1.618M), firstPoint);
        public static decimal R382(decimal firstPoint,decimal secondPoint) 
            => decimal.Subtract(secondPoint, decimal.Multiply(decimal.Subtract(secondPoint, firstPoint), 0.382M));
        public static decimal R500(decimal firstPoint, decimal secondPoint)
            => decimal.Subtract(secondPoint, decimal.Multiply(decimal.Subtract(secondPoint, firstPoint), 0.5M));
        public static decimal R618(decimal firstPoint, decimal secondPoint)
            => decimal.Subtract(secondPoint, decimal.Multiply(decimal.Subtract(secondPoint, firstPoint), 0.618M));
        public static decimal R786(decimal firstPoint, decimal secondPoint)
            => decimal.Subtract(secondPoint, decimal.Multiply(decimal.Subtract(secondPoint, firstPoint), 0.786M));
        public static decimal GetEntryByRR(decimal stopLoss, decimal takeProfit, decimal RR = 3M)
            => decimal.Add(stopLoss, decimal.Divide(decimal.Subtract(takeProfit, stopLoss), RR + 1));
        public static decimal GetRRByPosition(PositionEntity entity)
        {
            if (entity.PositionState == PositionState.Stop)
            {
                return -1;
            }
            else if (entity.PositionState == PositionState.Profit)
            {
                if (entity.SignalDirection == SignalDirection.Buy)
                    return Math.Round((entity.TakeProfit - entity.EntryPoint) / (entity.EntryPoint - entity.StopLoss), 2);
                else
                    return Math.Round((entity.EntryPoint - entity.TakeProfit) / (entity.StopLoss - entity.EntryPoint), 2);
            }
            else
            {
                return 0;
            }
        }
    }
}
