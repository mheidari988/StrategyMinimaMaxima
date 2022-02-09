using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Strategies.Protective;
using StockSharp.BusinessEntities;
using StockSharp.Messages;

namespace StrategyMinimaMaxima.PriceAction
{
    public class ICIStrategy : Strategy
    {

        public CandleSeries ParentCandleSeries { get; private set; }
        public CandleSeries ChildCandleSeries { get; private set; }

        private readonly PriceActionContainer ParentContainer = new PriceActionContainer();
        private readonly PriceActionContainer ChildContainer = new PriceActionContainer();
        private PriceActionProcessor processor = new PriceActionProcessor();

        public ICIStrategy(CandleSeries parentCandleSeries, CandleSeries childCandleSeries, long processLimit = 0)
        {
            ParentCandleSeries = parentCandleSeries;
            ChildCandleSeries = childCandleSeries;
            ParentContainer.ProcessLimit = processLimit;
            ChildContainer.ProcessLimit = processLimit * 4;
        }

        public string GetChildReport(LegStatus parentLeg, int parentLevel = 0, bool openEnd = false)
        {
            if (ParentContainer is null) return string.Empty;

            return LogHelper.ReportSwingList(processor.GetChildSwings(parentLeg, parentLevel, openEnd));
        }
        public string GetParentReport()
        {
            if (ParentContainer is null) return string.Empty;

            return LogHelper.ReportSwingList(ParentContainer.Swings.Values.ToList());
        }
        protected override void OnStarted()
        {
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParentCandleSeries);

            Connector.CandleSeriesProcessing += Connector_CandleSeriesProcessing;

            base.OnStarted();
        }

        private void Connector_CandleSeriesProcessing(CandleSeries candleSeries, Candle candle)
        {
            //-------------------------------------------------------------------------
            //--- Pass candles to ParentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ParentContainer.AddCandle(candle);
                    processor.ParentContainer = ParentContainer;
                }
            }

            //-------------------------------------------------------------------------
            //--- Pass candles to ParentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ChildContainer.AddCandle(candle);
                    processor.ChildContainer = ChildContainer;
                }
            }
        }
    }
}
