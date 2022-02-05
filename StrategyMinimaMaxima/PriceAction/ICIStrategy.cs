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
        
        public CandleSeries ParrentCandleSeries { get; private set; }
        public CandleSeries ChildCandleSeries { get; private set; }

        private readonly PriceActionContainer ParrentContainer = new PriceActionContainer();
        private readonly PriceActionContainer ChildContainer = new PriceActionContainer();
        private PriceActionProcessor processor = new PriceActionProcessor();

        public ICIStrategy(CandleSeries parrentCandleSeries, CandleSeries childCandleSeries, long processLimit = 0)
        {
            ParrentCandleSeries = parrentCandleSeries;
            ChildCandleSeries = childCandleSeries;
            ParrentContainer.ProcessLimit = processLimit;
            ChildContainer.ProcessLimit = processLimit * 4;
        }

        protected override void OnStarted()
        {
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParrentCandleSeries);

            Connector.CandleSeriesProcessing += Connector_CandleSeriesProcessing;

            base.OnStarted();
        }

        private void Connector_CandleSeriesProcessing(CandleSeries candleSeries, Candle candle)
        {
            //-------------------------------------------------------------------------
            //--- Pass candles to ParrentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ParrentContainer.AddCandle(candle);
                    processor.ParrentContainer = ParrentContainer;
                    LogHelper.WriteCandleList(ParrentContainer.Candles, "_parrentCandles.txt");
                }
            }

            //-------------------------------------------------------------------------
            //--- Pass candles to ParrentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ChildContainer.AddCandle(candle);
                    processor.ChildContainer = ChildContainer;
                    LogHelper.WriteCandleList(ChildContainer.Candles, "_childCandles.txt");

                    if (ParrentContainer.Swings.Count > 0)
                    {
                        LogHelper.WriteSwingList(ParrentContainer.Swings.Values.ToList(), "_ParrentSwings.txt");
                        LogHelper.WriteSwingList(ChildContainer.Swings.Values.ToList(), "_ChildSwings.txt");
                        LogHelper.WriteSwingList(processor.GetChildSwingsOfLastParrent(LegStatus.Leg1), "_Leg1.txt");
                        LogHelper.WriteSwingList(processor.GetChildSwingsOfLastParrent(LegStatus.Leg2), "_Leg2.txt");
                        LogHelper.WriteSwingList(processor.GetChildSwingsOfLastParrent(LegStatus.Leg3), "_Leg3.txt");
                        LogHelper.WriteSwingList(processor.GetChildSwingsOfLastParrent(LegStatus.Leg3, true), "_Leg3_nonStop.txt");
                    }
                }
            }
        }
    }
}
