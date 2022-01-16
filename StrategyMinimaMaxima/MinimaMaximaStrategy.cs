using System;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;
using StrategyMinimaMaxima.PriceAction;

namespace StrategyMinimaMaxima
{
    internal class MinimaMaximaStrategy : Strategy
    {
        private readonly CandleSeries _candleSeries;
        private readonly PriceActionManager _minimaMaximaFinder = new PriceActionManager();
        public MinimaMaximaStrategy(CandleSeries candleSeries)
        {
            _candleSeries = candleSeries;
        }

        protected override void OnStarted()
        {
            this.Connector.CandleSeriesProcessing += CandleManager_Processing;
            this.Connector.SubscribeCandles(_candleSeries);
            base.OnStarted();
        }

        private void CandleManager_Processing(CandleSeries candleSeries, Candle candle)
        {
            if (candle.State != CandleStates.Finished) return;

            _minimaMaximaFinder.AddCandle(candle);
            {
                _minimaMaximaFinder.WriteLocalLog();
            }

            //if (candle.OpenPrice < candle.ClosePrice && Position >= 0)
            //{
            //    RegisterOrder(this.SellAtMarket(Volume + Math.Abs(Position)));
            //}

            //else
            //if (candle.OpenPrice > candle.ClosePrice && Position <= 0)
            //{
            //    RegisterOrder(this.BuyAtMarket(Volume + Math.Abs(Position)));
            //}
        }
    }
}
