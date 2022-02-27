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
using StockSharp.Logging;
using StockSharp.Messages;

namespace TradeCore.PriceAction
{
    public class ICIStrategy : Strategy
    {
        private readonly PriceActionContainer ParentContainer = new PriceActionContainer(60);
        private readonly PriceActionContainer ChildContainer = new PriceActionContainer(15);
        private PriceActionProcessor processor;

        public int Hierarchy { get; set; } = 4;
        public CandleSeries ParentCandleSeries { get; }
        public CandleSeries ChildCandleSeries { get; }
        public CandleSeries MicroCandleSeries { get; }
        public PriceActionProcessor Processor { get => processor; private set => processor = value; }

        public ICIStrategy(CandleSeries parentCandleSeries,
                           CandleSeries childCandleSeries,
                           CandleSeries microCandleSeries,
                           long processLimit = 0)
        {
            ParentCandleSeries = parentCandleSeries;
            ChildCandleSeries = childCandleSeries;
            MicroCandleSeries = microCandleSeries;

            ParentContainer.IsCapacityEnabled = true;
            ParentContainer.CandleCapacity = 36;
            ChildContainer.IsCapacityEnabled = true;
            ChildContainer.CandleCapacity = ParentContainer.CandleCapacity * Hierarchy;

            ParentContainer.ProcessLimit = processLimit;
            ChildContainer.ProcessLimit = processLimit * Hierarchy;
            processor = new PriceActionProcessor(ParentContainer, ChildContainer);
        }

        public string GetChildReport(LegStatus parentLeg, int parentLevel = 0, bool openEnd = false)
        {
            if (ParentContainer is null) return string.Empty;

            return LogHelper.ReportSwingList(Processor.GetChildSwings(parentLeg, parentLevel, openEnd));
        }
        public string GetParentReport()
        {
            if (ParentContainer is null) return string.Empty;

            return LogHelper.ReportSwingList(ParentContainer.Swings.Values.ToList());
        }
        protected override void OnStarted()
        {
            Connector.SubscribeCandles(MicroCandleSeries);
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParentCandleSeries);
            processor.ParentChanged += Processor_ParentChanged;

            Connector.CandleSeriesProcessing += Connector_CandleSeriesProcessing;

            base.OnStarted();
        }

        private void Processor_ParentChanged(object? sender, Signal.ContainerChangeEventArgs e)
        {
            this.AddInfoLog($"Parent Swings Count: {e.ParentContainer.Swings.Count}");
            this.AddInfoLog($"Child Swings Count: {e.ChildContainer.Swings.Count}");
        }

        private void Connector_CandleSeriesProcessing(CandleSeries candleSeries, Candle candle)
        {
            //----------------------------------------------------------------------------
            //--- Pass micro candles to processor to evaluate positions on lower times ---
            //----------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(1))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ChildContainer.AddMicroCandle(candle);
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
                    Processor.ChildContainer = ChildContainer;
                }
            }
            //-------------------------------------------------------------------------
            //--- Pass candles to ParentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ParentContainer.AddCandle(candle);
                    Processor.ParentContainer = ParentContainer;
                }
            }
        }
    }
}
