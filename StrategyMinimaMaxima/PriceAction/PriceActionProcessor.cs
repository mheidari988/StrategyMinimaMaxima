﻿using StockSharp.Algo.Candles;
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

        public PriceActionProcessor(PriceActionContainer parrent, PriceActionContainer child)
        {
            ParrentContainer = parrent ?? throw new ArgumentNullException(nameof(parrent));
            ChildContainer = child ?? throw new ArgumentNullException(nameof(child));
            Hierarchy = 4;
        }

        public PriceActionProcessor(PriceActionContainer parrent, PriceActionContainer child, int hierarchy = 4)
        {
            ParrentContainer = parrent ?? throw new ArgumentNullException(nameof(parrent));
            ChildContainer = child ?? throw new ArgumentNullException(nameof(child));
            Hierarchy = hierarchy;
        }

        #endregion

        #region Properties And Private Fields

        public PriceActionContainer? ParrentContainer { get; set; }
        public PriceActionContainer? ChildContainer { get; set; }
        public int Hierarchy { get; set; }

        #endregion

        #region Private Methods

        private bool IsManagersReady() => (ChildContainer != null || ParrentContainer != null);

        #endregion

        #region Public Methods

        public List<PriceActionSwing>? GetChildSwingsOfLastParrent(LegStatus fromLeg, bool nonStop = false)
        {
            var result = ParrentContainer.Swings.LastOrDefault().Value;

            if (result != null)
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
                                                 select child.SeqNum).LastOrDefault();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.HighPrice == pHigh
                                               select child.SeqNum).LastOrDefault();

                                return generateChildList(nonStop, cStartSeq, cEndSeq);
                            }
                            catch (InvalidOperationException)
                            {
                                return null;
                            }
                        }
                        else if (result.Leg1.MomentumType == MomentumType.Bearish)
                        {
                            decimal pLow = result.Leg1.BeginElement.Candle.HighPrice;
                            decimal pHigh = result.Leg1.EndElement.Candle.LowPrice;

                            try
                            {
                                var cStartSeq = (from child in ChildContainer.Candles
                                                 where child.HighPrice == pHigh
                                                 select child.SeqNum).LastOrDefault();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.LowPrice == pLow
                                               select child.SeqNum).LastOrDefault();

                                return generateChildList(nonStop, cStartSeq, cEndSeq);
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
                                                 select child.SeqNum).LastOrDefault();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.HighPrice == pHigh
                                               select child.SeqNum).LastOrDefault();

                                return generateChildList(nonStop, cStartSeq, cEndSeq);
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
                                                 select child.SeqNum).LastOrDefault();
                                var cEndSeq = (from child in ChildContainer.Candles
                                               where child.LowPrice == pLow
                                               select child.SeqNum).LastOrDefault();

                                return generateChildList(nonStop, cStartSeq, cEndSeq);
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

                                return generateChildList(nonStop, cStartSeq, cEndSeq);
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

                                return generateChildList(nonStop, cStartSeq, cEndSeq);
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

        public PriceActionContainer GetChildContainerByIndex(long reverseIndex)
        {
            throw new NotImplementedException();
        }
        public PriceActionContainer GetChildContainerByParrentSwing(PriceActionSwing parrentSwing, LegStatus leg)
        {
            throw new NotImplementedException();
        }


        public List<PriceActionSwing>? GetChildSwingsOf(LegStatus leg)
        {
            if (!IsManagersReady()) return null;

            var lastParrentSwing = ParrentContainer.Swings.LastOrDefault();
            if (lastParrentSwing.Value != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from swing in ChildContainer.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= lastParrentSwing.Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && swing.Leg3.EndElement.Candle.SeqNum
                                <= lastParrentSwing.Value.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg2:
                        return (from swing in ChildContainer.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= lastParrentSwing.Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && swing.Leg3.EndElement.Candle.SeqNum
                                <= lastParrentSwing.Value.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg3:
                        return (from swing in ChildContainer.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= lastParrentSwing.Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && swing.Leg3.EndElement.Candle.SeqNum
                                <= lastParrentSwing.Value.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<PriceActionSwing>? GetChildSwingsFrom(LegStatus leg)
        {
            if (!IsManagersReady()) return null;

            var lastParrentSwing = ParrentContainer.Swings.LastOrDefault();
            if (lastParrentSwing.Value != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from swing in ChildContainer.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= lastParrentSwing.Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg2:
                        return (from swing in ChildContainer.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= lastParrentSwing.Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg3:
                        return (from swing in ChildContainer.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= lastParrentSwing.Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<Candle>? GetChildCandlesOf(LegStatus leg)
        {
            if (!IsManagersReady()) return null;

            var lastParrentSwing = ParrentContainer.Swings.LastOrDefault();
            if (lastParrentSwing.Value != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildContainer.Candles
                                where child.SeqNum >= lastParrentSwing.Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && child.SeqNum <= lastParrentSwing.Value.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildContainer.Candles
                                where child.SeqNum >= lastParrentSwing.Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && child.SeqNum <= lastParrentSwing.Value.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildContainer.Candles
                                where child.SeqNum >= lastParrentSwing.Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && child.SeqNum <= lastParrentSwing.Value.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<Candle>? GetChildCandlesFrom(LegStatus leg)
        {
            if (!IsManagersReady()) return null;

            var lastParrentSwing = ParrentContainer.Swings.LastOrDefault();
            if (lastParrentSwing.Value != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildContainer.Candles
                                where child.SeqNum >= lastParrentSwing.Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildContainer.Candles
                                where child.SeqNum >= lastParrentSwing.Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildContainer.Candles
                                where child.SeqNum >= lastParrentSwing.Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<PriceActionElement>? GetChildValleysOf(LegStatus leg)
        {
            if (!IsManagersReady()) return null;

            var lastParrentSwing = ParrentContainer.Swings.LastOrDefault();
            if (lastParrentSwing.Value != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildContainer.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Value.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Valley
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildContainer.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Value.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Valley
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildContainer.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Value.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Valley
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<PriceActionElement>? GetChildPeaksOf(LegStatus leg)
        {
            if (!IsManagersReady()) return null;

            var lastParrentSwing = ParrentContainer.Swings.LastOrDefault();
            if (lastParrentSwing.Value != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildContainer.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Value.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Peak
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildContainer.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Value.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Peak
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildContainer.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Value.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Peak
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public float GetSlopeOfParrentLeg(LegStatus leg)
        {
            throw new NotImplementedException();
        }
        public float GetChildValleysSlopesOf(LegStatus leg)
        {
            throw new NotImplementedException();
        }
        public float GetChildPeaksSlopesOf(LegStatus leg)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
