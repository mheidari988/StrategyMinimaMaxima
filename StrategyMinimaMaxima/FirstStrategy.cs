using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;
using StrategyMinimaMaxima.PriceAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace StrategyMinimaMaxima
{
    public class FirstStrategy : Strategy
    {
        private readonly CandleSeries _h1CandleSeries;
        private readonly CandleSeries _15mCandleSeries;
        private readonly PriceActionManager _1h_priceActionManager = new PriceActionManager();
        private readonly PriceActionManager _15m_priceActionManager = new PriceActionManager();
        private Dictionary<long, PriceActionSwing> _15Swings = new Dictionary<long, PriceActionSwing>();

        public FirstStrategy(CandleSeries _h1CandleSeries, CandleSeries _15mCandleSeries, long processLimit = 0)
        {
            this._h1CandleSeries = _h1CandleSeries;
            this._15mCandleSeries = _15mCandleSeries;
            _1h_priceActionManager.ProcessLimit = processLimit;
            _15m_priceActionManager.ProcessLimit = processLimit * 4;
        }

        protected override void OnStarted()
        {
            Connector.CandleSeriesProcessing += CandleManager_Processing;
            Connector.SubscribeCandles(_15mCandleSeries);
            Connector.SubscribeCandles(_h1CandleSeries);
            base.OnStarted();
        }

        private void CandleManager_Processing(CandleSeries candleSeries, Candle candle)
        {
            
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State != CandleStates.Finished) return;

                _15m_priceActionManager.AddCandle(candle);
                _15m_priceActionManager.WriteLocalLog("_FirstStrategy_15m_Log.txt");
            }
            else if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State != CandleStates.Finished) return;

                _1h_priceActionManager.AddCandle(candle);
                _1h_priceActionManager.WriteLocalLog("_FirstStrategy_1h_Log.txt");
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
