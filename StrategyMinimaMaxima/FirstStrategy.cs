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
using System.IO;

namespace StrategyMinimaMaxima
{
    public class FirstStrategy : Strategy
    {
        private readonly CandleSeries _h1CandleSeries;
        private readonly CandleSeries _15mCandleSeries;
        private readonly PriceActionManager manager1h = new PriceActionManager();
        private readonly PriceActionManager manager15m = new PriceActionManager();
        private PriceActionProcessor processor = new PriceActionProcessor();

        public FirstStrategy(CandleSeries _h1CandleSeries, CandleSeries _15mCandleSeries, long processLimit = 0)
        {
            this._h1CandleSeries = _h1CandleSeries;
            this._15mCandleSeries = _15mCandleSeries;
            manager1h.ProcessLimit = processLimit;
            manager15m.ProcessLimit = processLimit * 4;
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

                manager15m.AddCandle(candle);
                manager15m.WriteLocalLog("_FirstStrategy_15m_Log.txt");
                processor.ChildManager = manager15m;
            }
            else if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State != CandleStates.Finished) return;

                manager1h.AddCandle(candle);
                manager1h.WriteLocalLog("_FirstStrategy_1h_Log.txt");
                processor.ParrentManager = manager1h;
            }


            if (manager1h.Swings.Count > 0)
            {
                //WriteCandleList(processor.GetChildCandlesFrom(LegStatus.Leg3));
                //WriteSwingDic(processor.GetChildSwingsOf(LegStatus.Leg1));
                //WriteSwingList(processor._GetChildSwingsOf(LegStatus.Leg1));
            }
        }

        #region Log
        public void WriteCandleList(List<Candle> cndl)
        {
            if (cndl != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Last Swing Candles ---------");
                foreach (var item in cndl)
                {
                    str.AppendLine($"SeqNo {item.SeqNum} -- Type: -- Values: Open:{item.OpenPrice} ," +
                        $" Close:{item.ClosePrice} High:{item.HighPrice} Low:{item.LowPrice}");
                }
                File.WriteAllText("_GetChildCandlesFrom.txt", str.ToString());
            }
        }
        public void WriteSwingDic(Dictionary<long,PriceActionSwing> swings)
        {
            if (swings != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Last Swing Candles ---------");
                foreach (var item in swings)
                {
                    str.AppendLine($"Index: {item.Key}" +
                        $" -- Impulse: {PriceActionHelper.GetImpulseType(item.Value)} " +
                        $" -- Correction: {PriceActionHelper.GetCorrectionType(item.Value)} " +
                        $" -- PatternType: {item.Value.PatternType}");
                }
                File.WriteAllText("_GetChildSwings.txt", str.ToString());
            }
        }
        public void WriteSwingList(List<PriceActionSwing> swings)
        {
            if (swings != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Last Swing Candles ---------");
                foreach (var item in swings)
                {
                    str.AppendLine(
                        $" -- PatternType: {item.PatternType}" +
                        $" -- Impulse: {PriceActionHelper.GetImpulseType(item)} " +
                        $" -- Correction: {PriceActionHelper.GetCorrectionType(item)} ");
                }
                File.WriteAllText("_GetChildSwings.txt", str.ToString());
            }
        }

        #endregion

    }
}
