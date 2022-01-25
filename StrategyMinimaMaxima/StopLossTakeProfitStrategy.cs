using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;
using StockSharp.BusinessEntities;
using StrategyMinimaMaxima.PriceAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using StockSharp.Algo.Strategies.Protective;
using StockSharp.Binance;

namespace StrategyMinimaMaxima
{
    public class StopLossTakeProfitStrategy : Strategy
    {
        private readonly CandleSeries ParrentCandleSeries;
        private readonly CandleSeries ChildCandleSeries;
        private readonly CandleSeries OneMinSeries;
        private readonly PriceActionManager ParrentManager = new PriceActionManager();
        private readonly PriceActionManager ChildManager = new PriceActionManager();
        private PriceActionProcessor processor = new PriceActionProcessor();

        private bool parrentReadyToSell = false;
        private bool parrentReadyToBuy = false;
        private bool childReadyToSell = false;
        private bool childReadyToBuy = false;
        private bool isSignalByNewLeg = false;

        private long orderNo = 0;

        private decimal _sl;
        private decimal _tp;

        public StopLossTakeProfitStrategy(CandleSeries parrentCandleSeries, CandleSeries childCandleSeries, CandleSeries oneMinSeries, long processLimit = 0)
        {
            ParrentCandleSeries = parrentCandleSeries;
            ChildCandleSeries = childCandleSeries;
            OneMinSeries = oneMinSeries;
            ParrentManager.ProcessLimit = processLimit;
            ChildManager.ProcessLimit = processLimit * 4;
        }

        protected override void OnStarted()
        {
            Connector.CandleSeriesProcessing += CandleManager_Processing;
            Connector.WhenCandlesFinished(ParrentCandleSeries).Do(ParrentCandleFinished).Apply(this);
            Connector.WhenCandlesFinished(ChildCandleSeries).Do(ChildCandleFinished).Apply(this);
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParrentCandleSeries);
            Connector.SubscribeCandles(OneMinSeries);
            base.OnStarted();
        }

        private void CandleManager_Processing(CandleSeries candleSeries, Candle candle)
        {
            //----------------- Here we check to protect position based on TP or SL ---------------------

            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(1))
            {
                if (candle.State == CandleStates.Finished)
                {
                    if (Position >= 0)
                    {
                        var order = this.SellAtMarket(0.01m);
                        order.WhenNewTrade(Connector).Do(NewOderTrade).Until(() => order.State == OrderStates.Done).Apply(this);
                        ChildStrategies.ToList().ForEach(s => s.Stop());
                        RegisterOrder(order);
                    }
                }
            }
            //---------------- Checking the state of Finished for Child Candle -------------------

            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ChildManager.AddCandle(candle);
                    processor.ChildManager = ChildManager;
                }
            }
            
            //---------------- Checking the state of Finished for Parrent Candle -------------------

            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ParrentManager.AddCandle(candle);
                    processor.ParrentManager = ParrentManager;
                }
            }

            //----------------------------Check available trades count, and if there is no trade, start one.

        }

        protected void NewOderTrade(MyTrade myTrade)
        {
            var takeProfit = new TakeProfitStrategy(myTrade, 500)
            {
                WaitAllTrades = true,
            };
            var stopLoss = new StopLossStrategy(myTrade, 500)
            {
                WaitAllTrades = true,
            };

            var protectiveStrategies = new TakeProfitStopLossStrategy(takeProfit, stopLoss)
            {
                WaitAllTrades = true,
            };
            ChildStrategies.Add(protectiveStrategies);
        }

        private void ChildCandleFinished(Candle candle)
        {
            if (parrentReadyToSell)
            {
                decimal fib3 = new FibonacciCalculator(ParrentSwing.Leg2.BeginElement.Candle.HighPrice, ParrentSwing.Leg2.EndElement.Candle.LowPrice).Retracement382;
                if (candle.HighPrice >= fib3)
                {
                    if (isSignalByNewLeg)
                    {
                        if (processor.GetChildSwingsOf(LegStatus.Leg2).Any(
                            x => x.PatternType == PatternType.BullishICI ||
                            x.PatternType == PatternType.BullishCII))
                        {
                            if (processor.GetChildSwingsFrom(LegStatus.Leg3).Any(
                            x => (x.PatternType == PatternType.BearishICI && PriceActionHelper.GetImpulseType(x) == ImpulseType.Breakout)
                            || (x.PatternType == PatternType.BearishCII && PriceActionHelper.GetImpulseType(x) == ImpulseType.Breakout)))
                            {
                                _sl = ParrentSwing.Leg2.BeginElement.Candle.HighPrice;
                                _tp = new FibonacciCalculator(ParrentSwing.Leg2.EndElement.Candle.HighPrice, ParrentSwing.Leg2.EndElement.Candle.LowPrice).Target618;
                                childReadyToSell = true;
                            }
                        }
                    }
                    else
                    {
                        if (processor.GetChildSwingsOf(LegStatus.Leg3).Any(
                            x => x.PatternType == PatternType.BullishICI ||
                            x.PatternType == PatternType.BullishCII))
                        {
                            if (processor.GetChildSwingsFrom(LegStatus.Leg3).Any(
                            x => x.PatternType == PatternType.BearishICI || x.PatternType == PatternType.BearishCII))
                            {
                                _sl = ParrentSwing.Leg2.BeginElement.Candle.HighPrice;
                                _tp = new FibonacciCalculator(ParrentSwing.Leg2.EndElement.Candle.HighPrice, ParrentSwing.Leg2.EndElement.Candle.LowPrice).Target618;
                                childReadyToSell = true;
                            }
                            else
                            {
                                childReadyToSell = false;
                            }
                        }
                    }
                }
            }
        }

        private void ParrentCandleFinished(Candle candle)
        {
            if (ParrentSwing != null && GrandParrentSwing != null)
            {
                if (GrandParrentSwing.PatternType == PatternType.BearishICI
                    && PriceActionHelper.GetImpulseType(ParrentSwing) == ImpulseType.Breakout)
                {
                    if (ParrentSwing.PatternType == PatternType.BearishCIC &&
                        PriceActionHelper.GetCorrectionType(ParrentSwing) != CorrectionType.Brokeback)
                    {
                        parrentReadyToSell = true;
                        isSignalByNewLeg = false;
                    }
                }
            }
            else
            {
                parrentReadyToSell = false;
                isSignalByNewLeg = false;
            }
            if (ParrentSwing != null && GrandParrentSwing != null && GrandGrandParrentSwing != null)
            {
                if (GrandGrandParrentSwing.PatternType == PatternType.BearishICI
                    && PriceActionHelper.GetImpulseType(GrandGrandParrentSwing) == ImpulseType.Breakout)
                {
                    if (GrandParrentSwing.PatternType == PatternType.BearishCIC &&
                        PriceActionHelper.GetCorrectionType(GrandParrentSwing) != CorrectionType.Brokeback)
                    {
                        if (ParrentSwing.PatternType == PatternType.BearishICC || ParrentSwing.PatternType == PatternType.BearishCII)
                        {
                            parrentReadyToSell = true;
                            isSignalByNewLeg = true;
                        }
                    }
                }
            }
            else
            {
                parrentReadyToSell = false;
                isSignalByNewLeg = false;
            }
        }

        #region Properties

        private PriceActionSwing ParrentSwing
        {
            get
            {
                return ParrentManager.Swings.LastOrDefault().Value;
            }
        }
        private PriceActionSwing GrandParrentSwing
        {
            get
            {
                return ParrentManager.Swings.SkipLast(1).LastOrDefault().Value;
            }
        }
        private PriceActionSwing GrandGrandParrentSwing
        {
            get
            {
                return ParrentManager.Swings.SkipLast(2).LastOrDefault().Value;
            }
        }

        #endregion
    }
}