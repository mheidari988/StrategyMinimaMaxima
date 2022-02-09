using StockSharp.Algo.Candles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionProcessor
    {


        #region Constructors
        public PriceActionProcessor()
        {
            Hierarchy = 4;
        }

        public PriceActionProcessor(PriceActionContainer parent, PriceActionContainer child)
        {
            ParentContainer = parent ?? throw new ArgumentNullException(nameof(parent));
            ChildContainer = child ?? throw new ArgumentNullException(nameof(child));
            Hierarchy = 4;
        }

        public PriceActionProcessor(PriceActionContainer parent, PriceActionContainer child, int hierarchy = 4)
        {
            ParentContainer = parent ?? throw new ArgumentNullException(nameof(parent));
            ChildContainer = child ?? throw new ArgumentNullException(nameof(child));
            Hierarchy = hierarchy;
        }

        #endregion

        #region Properties And Private Fields

        public PriceActionContainer? ParentContainer { get; set; }
        public PriceActionContainer? ChildContainer { get; set; }
        public int Hierarchy { get; set; }

        #endregion

        #region Private Methods

        private bool IsManagersReady() => (ChildContainer != null || ParentContainer != null);
        private PriceActionSwing? GetParentSwing(int level = 0)
        {
            if (level < 0) throw new ArgumentOutOfRangeException(nameof(level) + " must grater than 0");

            if (ParentContainer != null)
                return ParentContainer.Swings.SkipLast(level).LastOrDefault().Value;
            else
                return null;
        }

        #endregion

        #region Public Methods

        public List<PriceActionSwing>? GetChildSwings(LegStatus fromLeg, int level = 0, bool openEnd = false)
        {
            if (ParentContainer is null) return null;

            var result = ParentContainer.Swings.SkipLast(level).LastOrDefault().Value;

            if (result is not null)
            {
                switch (fromLeg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException($"LegStatus: {fromLeg} is not valid");

                    //-------------------------------------------------------
                    //--- Processing Leg1 in Bullish and Bearish scenario ---
                    //-------------------------------------------------------
                    case LegStatus.Leg1:
                        if (result.Leg1.MomentumType == MomentumType.Bullish)
                        {
                            decimal pLow = result.Leg1.BeginElement.Candle.LowPrice;
                            decimal pHigh = result.Leg1.EndElement.Candle.HighPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.LowPrice == pLow
                                                 select child.SeqNum).Last();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.HighPrice == pHigh
                                               select child.SeqNum).Last();

                                return generateChildList(openEnd, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                return null;
                            }
                        }
                        else if (result.Leg1.MomentumType == MomentumType.Bearish)
                        {
                            decimal pHigh = result.Leg1.BeginElement.Candle.HighPrice;
                            decimal pLow = result.Leg1.EndElement.Candle.LowPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.HighPrice == pHigh
                                                 select child.SeqNum).Last();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.LowPrice == pLow
                                               select child.SeqNum).Last();

                                return generateChildList(openEnd, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"LegMomentum: {result.Leg1.MomentumType} is not valid");
                        }

                    //-------------------------------------------------------
                    //--- Processing Leg2 in Bullish and Bearish scenario ---
                    //-------------------------------------------------------
                    case LegStatus.Leg2:

                        if (result.Leg2.MomentumType == MomentumType.Bullish)
                        {
                            decimal pLow = result.Leg2.BeginElement.Candle.LowPrice;
                            decimal pHigh = result.Leg2.EndElement.Candle.HighPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.LowPrice == pLow
                                                 select child.SeqNum).Last();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.HighPrice == pHigh
                                               select child.SeqNum).Last();

                                return generateChildList(openEnd, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                return null;
                            }
                        }
                        else if (result.Leg2.MomentumType == MomentumType.Bearish)
                        {
                            decimal pHigh = result.Leg2.BeginElement.Candle.HighPrice;
                            decimal pLow = result.Leg2.EndElement.Candle.LowPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.HighPrice == pHigh
                                                 select child.SeqNum).Last();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.LowPrice == pLow
                                               select child.SeqNum).Last();

                                return generateChildList(openEnd, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"LegMomentum: {result.Leg2.MomentumType} is not valid");
                        }

                    //-------------------------------------------------------
                    //--- Processing Leg3 in Bullish and Bearish scenario ---
                    //-------------------------------------------------------
                    case LegStatus.Leg3:

                        if (result.Leg3.MomentumType == MomentumType.Bullish)
                        {
                            decimal pLow = result.Leg3.BeginElement.Candle.LowPrice;
                            decimal pHigh = result.Leg3.EndElement.Candle.HighPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.LowPrice == pLow
                                                 select child.SeqNum).Last();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.HighPrice == pHigh
                                               select child.SeqNum).Last();

                                return generateChildList(openEnd, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                // This is because Start or End SeqNum not found
                                return null;
                            }
                        }
                        else if (result.Leg3.MomentumType == MomentumType.Bearish)
                        {
                            decimal pHigh = result.Leg3.BeginElement.Candle.HighPrice;
                            decimal pLow = result.Leg3.EndElement.Candle.LowPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.HighPrice == pHigh
                                                 select child.SeqNum).Last();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.LowPrice == pLow
                                               select child.SeqNum).Last();

                                return generateChildList(openEnd, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                // This is because Start or End SeqNum not found
                                return null!;
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"LegMomentum: {result.Leg3.MomentumType} is not valid");
                        }
                    default:
                        break;
                }
            }

            return null!;
        }

        private List<PriceActionSwing> generateChildList(bool nonStop, long cStartSeq, long cEndSeq)
        {
            if (!nonStop)
            {
                var childCandles = ChildContainer.Candles.Where(c => c.SeqNum >= cStartSeq && c.SeqNum <= cEndSeq).ToList();
                return PriceActionContainer.GenerateContainer(childCandles).Swings.Values.ToList();
            }
            else
            {
                var childCandles = ChildContainer.Candles.Where(c => c.SeqNum >= cStartSeq).ToList();
                return PriceActionContainer.GenerateContainer(childCandles).Swings.Values.ToList();
            }
        }

        #endregion
    }
}
