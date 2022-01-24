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

        public PriceActionProcessor(PriceActionManager parrent, PriceActionManager child)
        {
            ParrentManager = parrent;
            ChildManager = child;
            Hierarchy = 4;
        }

        public PriceActionProcessor(PriceActionManager parrent, PriceActionManager child, int hierarchy = 4)
        {
            ParrentManager = parrent;
            ChildManager = child;
            Hierarchy = hierarchy;
        }

        #endregion

        #region Properties And Private Fields

        public PriceActionManager ParrentManager { get; set; }
        public PriceActionManager ChildManager { get; set; }
        public int Hierarchy { get; set; }

        #endregion

        #region Private Methods

        private void CheckNullObjects()
        {
            if (ChildManager == null || ParrentManager == null)
                throw new NullReferenceException("ParrentManager and ChildManager cannot be null.");
        }

        #endregion

        #region Public Methods

        public List<Candle>? GetChildCandlesOf(LegStatus leg)
        {
            CheckNullObjects();

            var lastParrentSwing = ParrentManager.Swings.LastOrDefault().Value;
            if (lastParrentSwing != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildManager.Candles
                                where child.SeqNum >= lastParrentSwing.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && child.SeqNum <= lastParrentSwing.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildManager.Candles
                                where child.SeqNum >= lastParrentSwing.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && child.SeqNum <= lastParrentSwing.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildManager.Candles
                                where child.SeqNum >= lastParrentSwing.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && child.SeqNum <= lastParrentSwing.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<Candle>? GetChildCandlesFrom(LegStatus leg)
        {
            CheckNullObjects();

            var lastParrentSwing = ParrentManager.Swings.LastOrDefault().Value;
            if (lastParrentSwing != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildManager.Candles
                                where child.SeqNum >= lastParrentSwing.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildManager.Candles
                                where child.SeqNum >= lastParrentSwing.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildManager.Candles
                                where child.SeqNum >= lastParrentSwing.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<PriceActionElement>? GetChildValleysOf(LegStatus leg)
        {
            CheckNullObjects();

            var lastParrentSwing = ParrentManager.Swings.LastOrDefault().Value;
            if (lastParrentSwing != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildManager.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Valley
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildManager.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Valley
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildManager.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Leg3.EndElement.Candle.SeqNum * Hierarchy
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
            CheckNullObjects();

            var lastParrentSwing = ParrentManager.Swings.LastOrDefault().Value;
            if (lastParrentSwing != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from child in ChildManager.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Peak
                                select child).ToList();
                    case LegStatus.Leg2:
                        return (from child in ChildManager.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Peak
                                select child).ToList();
                    case LegStatus.Leg3:
                        return (from child in ChildManager.ChainedElements.Values
                                where child.Candle.SeqNum >= lastParrentSwing.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && child.Candle.SeqNum <= lastParrentSwing.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                && child.PeakValleyType == PeakValleyType.Peak
                                select child).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<PriceActionSwing>? GetChildSwingsOf(LegStatus leg)
        {
            CheckNullObjects();

            var lastParrentSwing = ParrentManager.Swings.LastOrDefault().Value;
            if (lastParrentSwing != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from swing in ChildManager.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= ParrentManager.Swings.LastOrDefault().Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                && swing.Leg1.BeginElement.Candle.SeqNum
                                < ParrentManager.Swings.LastOrDefault().Value.Leg1.EndElement.Candle.SeqNum * Hierarchy
                                select swing).SkipLast(2).ToList();
                    case LegStatus.Leg2:
                        return (from swing in ChildManager.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= ParrentManager.Swings.LastOrDefault().Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                && swing.Leg1.BeginElement.Candle.SeqNum
                                < ParrentManager.Swings.LastOrDefault().Value.Leg2.EndElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg3:
                        return (from swing in ChildManager.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= ParrentManager.Swings.LastOrDefault().Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                && swing.Leg1.BeginElement.Candle.SeqNum
                                < ParrentManager.Swings.LastOrDefault().Value.Leg3.EndElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    default:
                        break;
                }
            }
            return null;
        }

        public List<PriceActionSwing>? GetChildSwingsFrom(LegStatus leg)
        {
            CheckNullObjects();

            var lastParrentSwing = ParrentManager.Swings.LastOrDefault().Value;
            if (lastParrentSwing != null)
            {
                switch (leg)
                {
                    case LegStatus.Unknown:
                        throw new ArgumentException("Legs should have status.");
                    case LegStatus.Leg1:
                        return (from swing in ChildManager.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= ParrentManager.Swings.LastOrDefault().Value.Leg1.BeginElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg2:
                        return (from swing in ChildManager.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= ParrentManager.Swings.LastOrDefault().Value.Leg2.BeginElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
                    case LegStatus.Leg3:
                        return (from swing in ChildManager.Swings.Values
                                where swing.Leg1.BeginElement.Candle.SeqNum
                                >= ParrentManager.Swings.LastOrDefault().Value.Leg3.BeginElement.Candle.SeqNum * Hierarchy
                                select swing).ToList();
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
