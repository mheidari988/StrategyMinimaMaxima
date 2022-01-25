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

        private readonly PriceActionManager ParrentManager = new PriceActionManager();
        private readonly PriceActionManager ChildManager = new PriceActionManager();
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
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParrentCandleSeries);
            Connector.SubscribeCandles(ExecCandleSeries);

            Connector.WhenCandlesFinished(ParrentCandleSeries).Do(ParrentCandleFinished).Apply(this);
            Connector.WhenCandlesFinished(ChildCandleSeries).Do(ChildCandleFinished).Apply(this);
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
                        
                        WriteSwingList(ParrentManager.Swings.Values.ToList(), "_parrentSwings.txt");
                        WriteSwingList(ChildManager.Swings.Values.ToList(), "_childSwings.txt");

                        var order = this.SellAtMarket(0.01m);
                        order.WhenNewTrade(Connector).Do(StartProtection).Until(() => order.State == OrderStates.Done).Apply(this);
                        order.WhenNewTrade(Connector).Do(() =>
                        {
                            priceActionPosition.IsExecuted = true;
                        }).Once().Apply(this);

                        ChildStrategies.ToList().ForEach(s => s.Stop());
                        RegisterOrder(order);
                    }
                }
            }
            //-------------------------------------------------------------------------
            //--- Pass candles to ParrentManager to process and add it to processor ---
            //-------------------------------------------------------------------------
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State == CandleStates.Finished)
                {
                    ChildManager.AddCandle(candle);
                    processor.ChildManager = ChildManager;
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
                    processor.ParrentManager = ParrentManager;
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
                                    priceActionPosition.StopLossPrice = ParrentSwing.Leg2.BeginElement.Candle.HighPrice;
                                    priceActionPosition.TakeProfitPrice = new FibonacciCalculator(ParrentSwing.Leg2.EndElement.Candle.HighPrice, ParrentSwing.Leg2.EndElement.Candle.LowPrice).Target618;
                                    priceActionPosition.ChildConfirmed = true;
                                }
                            }
                        }
                    }
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
        protected void StartProtection(MyTrade myTrade)
        {
            var stopLoss = new StopLossStrategy(myTrade, new Unit(priceActionPosition.StopLossPrice, UnitTypes.Limit))
            {
                WaitAllTrades = true,
            };
            var takeProfit = new TakeProfitStrategy(myTrade, new Unit(priceActionPosition.TakeProfitPrice, UnitTypes.Limit))
            {
                WaitAllTrades = true,
            };
            var protectiveStrategies = new TakeProfitStopLossStrategy(takeProfit, stopLoss)
            {
                WaitAllTrades = true,
            };
            ChildStrategies.Add(protectiveStrategies);
        }

        #region Log
        public void WriteCandleList(List<Candle> candle,string filePath)
        {
            if (candle != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Last Swing Candles ---------");
                foreach (var item in candle)
                {
                    str.AppendLine($"SeqNo {item.SeqNum} -- Type: -- Values: Open:{item.OpenPrice} ," +
                        $" Close:{item.ClosePrice} High:{item.HighPrice} Low:{item.LowPrice}");
                }
                File.WriteAllText(filePath, str.ToString());
            }
        }
        public void WriteSwingList(List<PriceActionSwing> swings,string filePath)
        {
            if (swings != null && swings.Count > 0)
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
                File.WriteAllText(filePath, str.ToString());
            }
        }

        #endregion
    }

    public class PriceActionPosition
    {
        public bool IsExecuted { get; set; } = false;
        public bool ParrentConfirmed { get; set; } = false;
        public bool ChildConfirmed { get; set; } = false;
        public Sides Direction { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal StopLossPrice { get; set; }
        public decimal TakeProfitPrice { get; set; }
    }
}
