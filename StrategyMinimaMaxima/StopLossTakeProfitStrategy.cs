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
        private readonly PriceActionManager ParrentManager = new PriceActionManager();
        private readonly PriceActionManager ChildManager = new PriceActionManager();
        private PriceActionProcessor processor = new PriceActionProcessor();

        private bool parrentReadyToSell = false;
        private bool parrentReadyToBuy = false;
        private bool childReadyToSell = false;
        private bool childReadyToBuy = false;
        private bool isSignalByNewLeg = false;

        private decimal _sl;
        private decimal _tp;

        public StopLossTakeProfitStrategy(CandleSeries parrentCandleSeries, CandleSeries childCandleSeries, long processLimit = 0)
        {
            ParrentCandleSeries = parrentCandleSeries;
            ChildCandleSeries = childCandleSeries;
            ParrentManager.ProcessLimit = processLimit;
            ChildManager.ProcessLimit = processLimit * 4;
        }

        protected override void OnStarted()
        {
            Connector.CandleSeriesProcessing += CandleManager_Processing;
            Connector.NewMyTrade += Connector_NewMyTrade;
            Connector.WhenCandlesFinished(ParrentCandleSeries).Do(ParrentCandleFinished).Apply(this);
            Connector.WhenCandlesFinished(ChildCandleSeries).Do(ChildCandleFinished).Apply(this);
            Connector.SubscribeCandles(ChildCandleSeries);
            Connector.SubscribeCandles(ParrentCandleSeries);
            base.OnStarted();
        }

        private void CheckIfWeCanTrade()
        {
            if (MyTrades.Count() < 1)
            {
                if (parrentReadyToBuy && childReadyToBuy)
                {

                }
                else if (parrentReadyToSell && childReadyToSell && Position >= 0)
                {
                    var sellOrder = new Order()
                    {
                        Type = OrderTypes.Market,
                        Portfolio = Portfolio,
                        Volume = 0.01M,
                        Security = Security,
                        Direction = Sides.Sell
                    };
                    var sellOrderStopLoss = new Order()
                    {
                        Type = OrderTypes.Conditional,
                        Portfolio = Portfolio,
                        Volume = 0.01M,
                        Security = Security,
                        Price = Math.Round(_sl, 2),
                        Condition = new BinanceOrderCondition()
                        {
                            Type = BinanceOrderConditionTypes.StopLoss,
                            StopPrice = Math.Round(_sl, 2)
                        }
                    };
                    var sellOrderTakeProfit = new Order()
                    {
                        Type = OrderTypes.Conditional,
                        Portfolio = Portfolio,
                        Volume = 0.01M,
                        Security = Security,
                        Price = Math.Round(_tp, 2),
                        Condition = new BinanceOrderCondition()
                        {
                            Type = BinanceOrderConditionTypes.TakeProfit,
                            StopPrice = Math.Round(_tp, 2)
                        }
                    };
                    RegisterOrder(sellOrder);
                    RegisterOrder(sellOrderStopLoss);
                    RegisterOrder(sellOrderTakeProfit);
                }
            }
        }

        private void CandleManager_Processing(CandleSeries candleSeries, Candle candle)
        {
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(15))
            {
                if (candle.State != CandleStates.Finished) return;

                ChildManager.AddCandle(candle);
                processor.ChildManager = ChildManager;
            }
            else if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                if (candle.State != CandleStates.Finished) return;

                ParrentManager.AddCandle(candle);
                processor.ParrentManager = ParrentManager;
            }

            //----------------------------Check available trades count, and if there is no trade, start one.

            CheckIfWeCanTrade();

            //----------------------------Protect Current Order
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

        private void ParrentCandleFinished(Candle cndl)
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
        }

        private void Connector_NewMyTrade(MyTrade myTrade)
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in Orders)
            {
                str.AppendLine(item.ToString());
            }
            File.WriteAllText("_orders.txt", str.ToString());
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