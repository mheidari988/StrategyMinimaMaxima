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
        private readonly PriceActionContainer manager1h = new PriceActionContainer();
        private readonly PriceActionContainer manager15m = new PriceActionContainer();
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
                if (candle.State == CandleStates.Finished)
                {
                    manager15m.AddCandle(candle);
                    processor.ChildContainer = manager15m;
                }
            }
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State == CandleStates.Finished)
                {
                    manager1h.AddCandle(candle);
                    processor.ParrentContainer = manager1h;
                }
            }
            if (manager1h.Swings.Count > 0)
            {
                WriteSwingList(processor.GetChildSwingsOf(LegStatus.Leg1));
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
                File.WriteAllText("_GetChildCandles.txt", str.ToString());
            }
        }
        public void WriteSwingList(List<PriceActionSwing> swings)
        {
            if (swings != null && swings.Count>0)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Last Swing Information ---------");
                str.AppendLine($"Total Swing Count: {swings.Count}" +
                    $" - Swing Begining Candle SeqNum: {swings[0].Leg1.BeginElement.Candle.SeqNum}" +
                    $" - Swing Ending Candle SeqNum: {swings[swings.Count - 1].Leg3.EndElement.Candle.SeqNum}");
                foreach (var item in swings)
                {
                    str.AppendLine();
                    str.AppendLine("######################################################################");
                    str.AppendLine(
                        $" -- PatternType: {item.PatternType}" +
                        $" -- Impulse: {PriceActionHelper.GetImpulseType(item)} " +
                        $" -- Correction: {PriceActionHelper.GetCorrectionType(item)} ");
                    str.AppendLine("######################################################################");
                    str.AppendLine();
                    str.AppendLine($"**Leg1 Begin Element: {item.Leg1.BeginElement}{System.Environment.NewLine}" +
                        $"**Leg1 End Element: {item.Leg1.EndElement}");
                    str.AppendLine();
                    str.AppendLine($"**Leg2 Begin Element: {item.Leg2.BeginElement}{System.Environment.NewLine}" +
                        $"**Leg2 End Element: {item.Leg2.EndElement}");
                    str.AppendLine();
                    str.AppendLine($"**Leg3 Begin Element: {item.Leg3.BeginElement}{System.Environment.NewLine}" +
                        $"**Leg3 End Element: {item.Leg3.EndElement}");
                    str.AppendLine();
                }
                File.WriteAllText("_GetChildSwings.txt", str.ToString());
            }
        }

        #endregion

    }
}
