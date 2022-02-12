using StockSharp.Algo.Candles;
using StrategyMinimaMaxima.PriceAction.Signal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionProcessor
    {
        private PriceActionContainer? parentContainer;
        private PriceActionContainer? childContainer;
        private decimal lastImpulseSlope;

        public event EventHandler<PriceActionContainer>? ParentChanged;
        public event EventHandler<PriceActionContainer>? ChildChanged;
        public event EventHandler<ParentPatternEventArgs>? SignalPatternChanged;
        public event EventHandler<ParentPatternEventArgs>? BullishICI;
        public event EventHandler<ParentPatternEventArgs>? BearishICI;
        public event EventHandler<SignalEntity>? BullishSignal;
        public event EventHandler<SignalEntity>? BearishSignal;


        #region Constructors

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

        #region Properties

        public decimal LastImpulseSlope { get => lastImpulseSlope; private set => lastImpulseSlope = Math.Round(value, 5); }

        public PriceActionContainer? ParentContainer
        {
            get => parentContainer;
            set
            {
                parentContainer = value;
                parentChanged();
            }
        }
        public PriceActionContainer? ChildContainer
        {
            get => childContainer;
            set
            {
                childContainer = value;
                childChanged();
            }
        }
        public int Hierarchy { get; set; }
        public ParentPatternType CurrentPattern { get; private set; } = ParentPatternType.None;

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

        #endregion

        #region Private Methods

        private bool IsTwoCloseBreak(PriceActionSwing swing)
        {
            #region Null Check
            if (swing is null)
                throw new ArgumentNullException(nameof(swing));

            if (parentContainer is null)
                throw new ArgumentNullException(nameof(parentContainer));
            #endregion

            if (PriceActionSwing.GetImpulseType(swing) != ImpulseType.Breakout)
            {
                return false;
            }
            else
            {
                if (swing.PatternType == PatternType.BullishICI || swing.PatternType == PatternType.BullishCII)
                {
                    var targetCandle = swing.Leg1.EndElement.Candle!;
                    var currentCandle = swing.Leg3.EndElement.Candle!;

                    var prevCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum - 1);
                    var nextCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum + 1);

                    if (prevCandle is not null && prevCandle.ClosePrice > targetCandle.HighPrice)
                        return true;
                    else if (nextCandle is not null && nextCandle.ClosePrice > targetCandle.HighPrice)
                        return true;
                    else
                        return false;
                }
                else if (swing.PatternType == PatternType.BullishCIC)
                {
                    var targetCandle = swing.Leg1.BeginElement.Candle!;
                    var currentCandle = swing.Leg2.EndElement.Candle!;

                    var prevCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum - 1);
                    var nextCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum + 1);

                    if (prevCandle is not null && prevCandle.ClosePrice > targetCandle.HighPrice)
                        return true;
                    else if (nextCandle is not null && nextCandle.ClosePrice > targetCandle.HighPrice)
                        return true;
                    else
                        return false;
                }
                else if (swing.PatternType == PatternType.BearishICI || swing.PatternType == PatternType.BearishCII)
                {
                    var targetCandle = swing.Leg1.EndElement.Candle!;
                    var currentCandle = swing.Leg3.EndElement.Candle!;

                    var prevCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum - 1);
                    var nextCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum + 1);

                    if (prevCandle is not null && prevCandle.ClosePrice < targetCandle.LowPrice)
                        return true;
                    else if (nextCandle is not null && nextCandle.ClosePrice < targetCandle.LowPrice)
                        return true;
                    else
                        return false;
                }
                else if (swing.PatternType == PatternType.BearishCIC)
                {
                    var targetCandle = swing.Leg1.BeginElement.Candle!;
                    var currentCandle = swing.Leg2.EndElement.Candle!;

                    var prevCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum - 1);
                    var nextCandle = parentContainer.Candles.SingleOrDefault(
                        cndl => cndl.SeqNum == currentCandle.SeqNum + 1);

                    if (prevCandle is not null && prevCandle.ClosePrice < targetCandle.LowPrice)
                        return true;
                    else if (nextCandle is not null && nextCandle.ClosePrice < targetCandle.LowPrice)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
        }
        private void parentChanged()
        {
            if (parentContainer is not null)
            {
                if (parentContainer.Swings.Count >= 3)
                {
                    //--------------------------------------------------------------
                    //--- STEP 1 : Two level scenario in Bullish and Bearish ICI ---
                    //--------------------------------------------------------------
                    if ((getParentSwing(1)!.PatternType == PatternType.BullishICI
                        || getParentSwing(1)!.PatternType == PatternType.BullishCII)
                        && getParentSwing()!.PatternType == PatternType.BullishCIC
                        && PriceActionSwing.GetImpulseType(getParentSwing(1)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(getParentSwing()!) != CorrectionType.Brokeback)
                    {
                        if (IsTwoCloseBreak(getParentSwing(1)!))
                        {
                            if (CurrentPattern != ParentPatternType.Bullish_ICI_CIC)
                            {
                                CurrentPattern = ParentPatternType.Bullish_ICI_CIC;

                                if (getParentSwing(1)!.PatternType == PatternType.BullishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(1)!.Leg3.BeginElement.Candle!.SeqNum,
                                        getParentSwing(1)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bullish);
                                else if (getParentSwing(1)!.PatternType == PatternType.BullishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(1)!.Leg2.BeginElement.Candle!.SeqNum,
                                        getParentSwing(1)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bullish);


                                SignalPatternChanged?.Invoke(this, new ParentPatternEventArgs
                                {
                                    ParentPatternType = CurrentPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BullishICI?.Invoke(this, new ParentPatternEventArgs
                            {
                                ParentPatternType = CurrentPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else if ((getParentSwing(1)!.PatternType == PatternType.BearishICI
                        || getParentSwing(1)!.PatternType == PatternType.BearishCII)
                        && getParentSwing()!.PatternType == PatternType.BearishCIC
                        && PriceActionSwing.GetImpulseType(getParentSwing(1)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(getParentSwing()!) != CorrectionType.Brokeback)
                    {
                        if (IsTwoCloseBreak(getParentSwing(1)!))
                        {
                            if (CurrentPattern != ParentPatternType.Bearish_ICI_CIC)
                            {
                                CurrentPattern = ParentPatternType.Bearish_ICI_CIC;

                                if (getParentSwing(1)!.PatternType == PatternType.BearishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(1)!.Leg3.BeginElement.Candle!.SeqNum,
                                        getParentSwing(1)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bearish);
                                else if (getParentSwing(1)!.PatternType == PatternType.BearishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(1)!.Leg2.BeginElement.Candle!.SeqNum,
                                        getParentSwing(1)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bearish);

                                SignalPatternChanged?.Invoke(this, new ParentPatternEventArgs
                                {
                                    ParentPatternType = CurrentPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BearishICI?.Invoke(this, new ParentPatternEventArgs
                            {
                                ParentPatternType = CurrentPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    //----------------------------------------------------------------
                    //--- STEP 2 : Three level scenario in Bullish and Bearish ICI ---
                    //----------------------------------------------------------------
                    else if ((getParentSwing(2)!.PatternType == PatternType.BullishICI
                        || getParentSwing(2)!.PatternType == PatternType.BullishCII)
                        && getParentSwing(1)!.PatternType == PatternType.BullishCIC
                        && getParentSwing()!.PatternType == PatternType.BullishICC
                        && PriceActionSwing.GetImpulseType(getParentSwing(2)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(getParentSwing(1)!) != CorrectionType.Brokeback)
                    {
                        if (IsTwoCloseBreak(getParentSwing(2)!))
                        {
                            if (CurrentPattern != ParentPatternType.Bullish_ICI_CIC_ICC)
                            {
                                CurrentPattern = ParentPatternType.Bullish_ICI_CIC_ICC;

                                if (getParentSwing(2)!.PatternType == PatternType.BullishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(2)!.Leg3.BeginElement.Candle!.SeqNum,
                                        getParentSwing(2)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bullish);
                                else if (getParentSwing(2)!.PatternType == PatternType.BullishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(2)!.Leg2.BeginElement.Candle!.SeqNum,
                                        getParentSwing(2)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bullish);

                                SignalPatternChanged?.Invoke(this, new ParentPatternEventArgs
                                {
                                    ParentPatternType = CurrentPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BullishICI?.Invoke(this, new ParentPatternEventArgs
                            {
                                ParentPatternType = CurrentPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else if ((getParentSwing(2)!.PatternType == PatternType.BearishICI
                        || getParentSwing(2)!.PatternType == PatternType.BearishCII)
                        && getParentSwing(1)!.PatternType == PatternType.BearishCIC
                        && getParentSwing()!.PatternType == PatternType.BearishICC
                        && PriceActionSwing.GetImpulseType(getParentSwing(2)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(getParentSwing(1)!) != CorrectionType.Brokeback)
                    {
                        if (IsTwoCloseBreak(getParentSwing(2)!))
                        {
                            if (CurrentPattern != ParentPatternType.Bearish_ICI_CIC_ICC)
                            {
                                CurrentPattern = ParentPatternType.Bearish_ICI_CIC_ICC;

                                if (getParentSwing(2)!.PatternType == PatternType.BearishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(2)!.Leg3.BeginElement.Candle!.SeqNum,
                                        getParentSwing(2)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bearish);
                                else if (getParentSwing(2)!.PatternType == PatternType.BearishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        getParentSwing(2)!.Leg2.BeginElement.Candle!.SeqNum,
                                        getParentSwing(2)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bearish);

                                SignalPatternChanged?.Invoke(this, new ParentPatternEventArgs
                                {
                                    ParentPatternType = CurrentPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BearishICI?.Invoke(this, new ParentPatternEventArgs
                            {
                                ParentPatternType = CurrentPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else
                    {
                        CurrentPattern = ParentPatternType.None;
                        SignalPatternChanged?.Invoke(this, new ParentPatternEventArgs
                        {
                            ParentPatternType = CurrentPattern,
                            ParentContainer = parentContainer,
                            ImpulseSlope = 0
                        });
                    }
                }
                ParentChanged?.Invoke(this, parentContainer);
            }
        }
        private void childChanged()
        {
            ChildChanged?.Invoke(this, childContainer);
        }
        private PriceActionSwing? getParentSwing(int level = 0)
        {
            if (level < 0) throw new ArgumentOutOfRangeException(nameof(level) + " must grater than 0");

            if (ParentContainer != null)
                return ParentContainer.Swings.SkipLast(level).LastOrDefault().Value;
            else
                return null;
        }
        private List<PriceActionSwing>? generateChildList(bool nonStop, long cStartSeq, long cEndSeq)
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
