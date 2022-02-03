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
using StrategyMinimaMaxima.PriceAction;

namespace StrategyMinimaMaxima
{
    public class MomentumStrategy : Strategy
    {
        private bool isSignalByNewLeg = false;

        private readonly PriceActionContainer ParrentManager = new PriceActionContainer();
        private readonly PriceActionContainer ChildManager = new PriceActionContainer();
        private PriceActionProcessor processor = new PriceActionProcessor();
        private PriceActionPosition priceActionPosition = new PriceActionPosition();
        public CandleSeries ParrentCandleSeries { get; private set; }
        public CandleSeries ChildCandleSeries { get; private set; }
        public CandleSeries ExecCandleSeries { get; set; }
        private PriceActionSwing? ParrentSwing
        {
            get
            {
                try
                {
                    return ParrentManager.Swings.LastOrDefault().Value;
                }
                catch
                {
                    return null;
                }
            }
        }
        private PriceActionSwing? GrandParrentSwing
        {
            get
            {
                try
                {
                    return ParrentManager.Swings.SkipLast(1).LastOrDefault().Value;
                }
                catch 
                {
                    return null;
                }
            }
        }
        private PriceActionSwing? GrandGrandParrentSwing
        {
            get
            {
                try
                {
                    return ParrentManager.Swings.SkipLast(2).LastOrDefault().Value;
                }
                catch 
                {
                    return null;
                }
            }
        }

        public MomentumStrategy(CandleSeries parrentCandleSeries, CandleSeries childCandleSeries, CandleSeries execCandleSeries, long processLimit = 0)
        {
            ParrentCandleSeries = parrentCandleSeries;
            ChildCandleSeries = childCandleSeries;
            ExecCandleSeries = execCandleSeries;
            ParrentManager.ProcessLimit = processLimit;
            ChildManager.ProcessLimit = processLimit * 4;
        }

        protected override void OnStarted()
        {
            Connector.SubscribeCandles(ExecCandleSeries);
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParrentCandleSeries);

            Connector.WhenCandlesFinished(ExecCandleSeries).Do(ExecCandleFinished).Apply(this);
            Connector.WhenCandlesFinished(ChildCandleSeries).Do(ChildCandleFinished).Apply(this);
            Connector.WhenCandlesFinished(ParrentCandleSeries).Do(ParrentCandleFinished).Apply(this);
            
            Connector.CandleSeriesProcessing += Connector_CandleSeriesProcessing;

            base.OnStarted();
        }

        private void Connector_CandleSeriesProcessing(CandleSeries arg1, Candle candle)
        {
            //--------------------------------------------------------------------------
            //--- Execute Trades if there is any signal and start protection of them ---
            //--------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(1))
            {

            }

            //-------------------------------------------------------------------------
            //--- Pass candles to ParrentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ChildManager.AddCandle(candle);
                    processor.ChildContainer = ChildManager;

                    //--- Write special log about our swings...
                    RecordLogs();
                }
            }

            //-------------------------------------------------------------------------
            //--- Pass candles to ParrentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ParrentManager.AddCandle(candle);
                    processor.ParrentContainer = ParrentManager;
                }
            }
        }

        private void ParrentCandleFinished(Candle parrentCandle)
        {
            //------------------------------------------------------------
            //--- If we already have a position available in portfolio ---
            //------------------------------------------------------------
            if (priceActionPosition.IsExecuted)
            {

            }
            //-----------------------------------------------------------
            //--- If we dont have any position available in portfolio ---
            //-----------------------------------------------------------
            else
            {
                //--- Reset Parrent confirmation and lets check it again.
                priceActionPosition.ParrentConfirmed = false;

                //--- Check if there is any confirm by Parrent and GrandParrent.
                if (ParrentSwing != null && GrandParrentSwing != null)
                {
                    if (GrandParrentSwing.PatternType == PatternType.BearishICI
                        && PriceActionHelper.GetImpulseType(ParrentSwing) == ImpulseType.Breakout)
                    {
                        if (ParrentSwing.PatternType == PatternType.BearishCIC &&
                            PriceActionHelper.GetCorrectionType(ParrentSwing) != CorrectionType.Brokeback)
                        {
                            priceActionPosition.Direction = Sides.Sell;
                            priceActionPosition.ParrentConfirmed = true;
                            isSignalByNewLeg = false;
                        }
                    }
                }

                //--- Check if there is any confirm by Parrent, GrandParrent and GrandGrandParrent
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
                                priceActionPosition.Direction = Sides.Sell;
                                priceActionPosition.ParrentConfirmed = true;
                                isSignalByNewLeg = true;
                            }
                        }
                    }
                }
            }
        }

        private void ChildCandleFinished(Candle childCandle)
        {
            //------------------------------------------------------------
            //--- If we already have a position available in portfolio ---
            //------------------------------------------------------------
            if (priceActionPosition.IsExecuted)
            {

            }
            //-----------------------------------------------------------
            //--- If we dont have any position available in portfolio ---
            //--- and at the sametime have confirmation by parrent.   ---
            //-----------------------------------------------------------
            else if (!priceActionPosition.IsExecuted && priceActionPosition.ParrentConfirmed)
            {
                //--- Reset child confirmation and lets check it again.
                priceActionPosition.ChildConfirmed = false;

                //-----------------------------------------------
                //--- If we have a potential SELL opportunity ---
                //-----------------------------------------------
                if (priceActionPosition.Direction == Sides.Sell)
                {
                    //--- Calculation of Fib 0.382
                    decimal fib3 = new FibonacciCalculator(ParrentSwing.Leg2.BeginElement.Candle.HighPrice,
                        ParrentSwing.Leg2.EndElement.Candle.LowPrice).Retracement382;

                    //--- If we have at least Fib 0.382 retracement.
                    if (childCandle.HighPrice >= fib3)
                    {
                        //--- Check if confirmation is by Parrent, GrandParrent and GrandGrandParrent
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
                                    priceActionPosition.EntryPrice =
                                        processor.GetChildSwingsFrom(LegStatus.Leg3).LastOrDefault().Leg2.BeginElement.Candle.LowPrice;
                                    priceActionPosition.StopLossPrice = ParrentSwing.Leg2.BeginElement.Candle.HighPrice;
                                    priceActionPosition.TakeProfitPrice = new FibonacciCalculator(ParrentSwing.Leg2.EndElement.Candle.HighPrice, ParrentSwing.Leg2.EndElement.Candle.LowPrice).Target618;
                                    priceActionPosition.ChildConfirmed = true;
                                }
                            }
                        }
                        //--- Check if confirmation is by Parrent, GrandParrent
                        else if (!isSignalByNewLeg)
                        {
                            if (processor.GetChildSwingsOf(LegStatus.Leg3).Any(
                                x => x.PatternType == PatternType.BullishICI ||
                                x.PatternType == PatternType.BullishCII))
                            {
                                if (processor.GetChildSwingsFrom(LegStatus.Leg3).Any(
                                x => x.PatternType == PatternType.BearishICI || x.PatternType == PatternType.BearishCII))
                                {
                                    priceActionPosition.EntryPrice =
                                        processor.GetChildSwingsFrom(LegStatus.Leg3).LastOrDefault().Leg2.BeginElement.Candle.LowPrice;
                                    priceActionPosition.StopLossPrice = ParrentSwing.Leg2.EndElement.Candle.HighPrice;
                                    priceActionPosition.TakeProfitPrice = new FibonacciCalculator(ParrentSwing.Leg2.EndElement.Candle.HighPrice, ParrentSwing.Leg2.EndElement.Candle.LowPrice).Target618;
                                    priceActionPosition.ChildConfirmed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExecCandleFinished(Candle candle)
        {
            //------------------------------------------------------------
            //--- If we already have a position available in portfolio ---
            //------------------------------------------------------------
            if (priceActionPosition.IsExecuted)
            {
                //--- If we IsExecuted is True but zero position then clear 
                //--- PriceActionPosition Object for next possible trades.
                if (Position == 0)
                {
                    priceActionPosition = new PriceActionPosition();
                }
                //--- Here we apply any process based on available position 
                //--- It can be RiskFree detection, etc.
                else
                {

                }
            }
            else
            {
                if (priceActionPosition.ParrentConfirmed
                    && priceActionPosition.ChildConfirmed
                    && priceActionPosition.Direction == Sides.Sell && Position == 0)
                {
                    var order = this.SellAtLimit(priceActionPosition.EntryPrice, 0.01m);
                    order.WhenNewTrade(Connector).Do(StartProtection).Until(() => order.State == OrderStates.Done).Apply(this);
                    order.WhenNewTrade(Connector).Do(() =>
                    {
                        priceActionPosition.IsExecuted = true;
                        LogHelper.WritePosition(priceActionPosition, $"_pos{order.Id}.txt");

                    }).Once().Apply(this);

                    ChildStrategies.ToList().ForEach(s => s.Stop());
                    RegisterOrder(order);
                }
            }
        }

        protected void StartProtection(MyTrade myTrade)
        {
            Unit stop = new Unit(priceActionPosition.GetStopLossLevel(), UnitTypes.Limit);
            Unit take = new Unit(priceActionPosition.GetTakeProfitLevel(), UnitTypes.Limit);

            var stopLoss = new StopLossStrategy(myTrade, 1000)
            {
                WaitAllTrades = true,
            };
            var takeProfit = new TakeProfitStrategy(myTrade, 2000)
            {
                WaitAllTrades = true,
            };
            var protectiveStrategies = new TakeProfitStopLossStrategy(takeProfit, stopLoss)
            {
                WaitAllTrades = true,
            };
            ChildStrategies.Add(protectiveStrategies);
        }

        private void RecordLogs()
        {
            if (ParrentSwing != null)
            {
                LogHelper.WriteSwingList(ParrentManager.Swings.Values.ToList(), "_AllSwings.txt");

                LogHelper.WriteSwing(ParrentSwing, "_ParrentSwing.txt");

                LogHelper.WriteSwingList(processor.GetChildSwingsOf(LegStatus.Leg1), "_GetChildSwingsOf_Leg1.txt");

                LogHelper.WriteSwingList(processor.GetChildSwingsOf(LegStatus.Leg2), "_GetChildSwingsOf_Leg2.txt");

                LogHelper.WriteSwingList(processor.GetChildSwingsOf(LegStatus.Leg3), "_GetChildSwingsOf_Leg3.txt");
            }
            if (GrandParrentSwing != null)
                LogHelper.WriteSwing(GrandParrentSwing, "_GrandParrentSwing.txt");

            if (GrandGrandParrentSwing != null)
                LogHelper.WriteSwing(GrandGrandParrentSwing, "_GrandGrandParrentSwing.txt");
        }
    }
}
