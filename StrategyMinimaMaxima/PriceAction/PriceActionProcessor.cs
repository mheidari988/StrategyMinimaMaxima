using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using TradeCore.PriceAction.Position;
using TradeCore.PriceAction.Signal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeCore.PriceAction
{
    public class PriceActionProcessor
    {
        private PriceActionContainer? parentContainer;
        private PriceActionContainer? childContainer;
        
        private decimal lastImpulseSlope;

        #region Events
        public event EventHandler<PriceActionContainer>? ParentChanged;
        public event EventHandler<PriceActionContainer>? ChildChanged;
        public event EventHandler<ParentSignalEventArgs>? ParentSignalChanged;
        public event EventHandler<ParentSignalEventArgs>? BullishICI;
        public event EventHandler<ParentSignalEventArgs>? BearishICI;
        public event EventHandler<SignalEntity?>? ChildSignalChanged;
        public event EventHandler<SignalEntity?>? BullishICISignal;
        public event EventHandler<SignalEntity?>? BearishICISignal;
        #endregion

        #region Constructors

        public PriceActionProcessor(PriceActionContainer parent,
                                    PriceActionContainer child,
                                    int hierarchy = 4)
        {
            ParentContainer = parent ?? throw new ArgumentNullException(nameof(parent));
            ChildContainer = child ?? throw new ArgumentNullException(nameof(child));
            ChildContainer.MicroCandleChanged += ChildContainer_MicroCandleChanged;
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
                WhenParentChanged();
            }
        }
        public PriceActionContainer? ChildContainer
        {
            get => childContainer;
            set
            {
                childContainer = value;
                WhenChildChanged();
            }
        }
        public int Hierarchy { get; set; }
        public SignalPattern CurrentSignalPattern { get; private set; } = SignalPattern.None;
        public SignalEntity? CurrentSignal { get; private set; }
        public List<PositionEntity> PositionHistory { get; } = new();
        public decimal RiskReward { get; set; } = 3M;
        public bool CheckTwoCloseBreak { get; set; } = false;

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

                                return GenerateChildList(openEnd, cStartSeq, cEndSeq);
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

                                return GenerateChildList(openEnd, cStartSeq, cEndSeq);
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

                                return GenerateChildList(openEnd, cStartSeq, cEndSeq);
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

                                return GenerateChildList(openEnd, cStartSeq, cEndSeq);
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

                                return GenerateChildList(openEnd, cStartSeq, cEndSeq);
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

                                return GenerateChildList(openEnd, cStartSeq, cEndSeq);
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

        private void WhenParentChanged()
        {
            if (parentContainer is not null)
            {
                if (parentContainer.Swings.Count >= 4)
                {
                    //---------------------------------------------------------------
                    //--- LEVEL 1 : Two swing scenario in Bullish and Bearish ICI ---
                    //---------------------------------------------------------------
                    if ((GetParentSwing(1)!.PatternType == PatternType.BullishICI
                        || GetParentSwing(1)!.PatternType == PatternType.BullishCII)
                        && GetParentSwing()!.PatternType == PatternType.BullishCIC
                        && PriceActionSwing.GetImpulseType(GetParentSwing(1)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(GetParentSwing()!) != CorrectionType.Brokeback)
                    {
                        if (ValidateTwoCloseBreak(GetParentSwing(1)!))
                        {
                            if (CurrentSignalPattern != SignalPattern.Bullish_ICI_CIC)
                            {
                                CurrentSignalPattern = SignalPattern.Bullish_ICI_CIC;

                                if (GetParentSwing(1)!.PatternType == PatternType.BullishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(1)!.Leg3.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(1)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bullish);
                                else if (GetParentSwing(1)!.PatternType == PatternType.BullishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(1)!.Leg2.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(1)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bullish);


                                ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                                {
                                    ParentPatternType = CurrentSignalPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BullishICI?.Invoke(this, new ParentSignalEventArgs
                            {
                                ParentPatternType = CurrentSignalPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else if ((GetParentSwing(1)!.PatternType == PatternType.BearishICI
                        || GetParentSwing(1)!.PatternType == PatternType.BearishCII)
                        && GetParentSwing()!.PatternType == PatternType.BearishCIC
                        && PriceActionSwing.GetImpulseType(GetParentSwing(1)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(GetParentSwing()!) != CorrectionType.Brokeback)
                    {
                        if (ValidateTwoCloseBreak(GetParentSwing(1)!))
                        {
                            if (CurrentSignalPattern != SignalPattern.Bearish_ICI_CIC)
                            {
                                CurrentSignalPattern = SignalPattern.Bearish_ICI_CIC;

                                if (GetParentSwing(1)!.PatternType == PatternType.BearishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(1)!.Leg3.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(1)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bearish);
                                else if (GetParentSwing(1)!.PatternType == PatternType.BearishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(1)!.Leg2.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(1)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bearish);

                                ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                                {
                                    ParentPatternType = CurrentSignalPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BearishICI?.Invoke(this, new ParentSignalEventArgs
                            {
                                ParentPatternType = CurrentSignalPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    //-----------------------------------------------------------------
                    //--- LEVEL 2 : Three swing scenario in Bullish and Bearish ICI ---
                    //-----------------------------------------------------------------
                    else if ((GetParentSwing(2)!.PatternType == PatternType.BullishICI
                        || GetParentSwing(2)!.PatternType == PatternType.BullishCII)
                        && GetParentSwing(1)!.PatternType == PatternType.BullishCIC
                        && GetParentSwing()!.PatternType == PatternType.BullishICC
                        && PriceActionSwing.GetImpulseType(GetParentSwing(2)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(GetParentSwing()!,
                            GetParentSwing(2)!.Leg1.EndElement) != CorrectionType.Brokeback)
                    {
                        if (ValidateTwoCloseBreak(GetParentSwing(2)!))
                        {
                            if (CurrentSignalPattern != SignalPattern.Bullish_ICI_CIC_ICC)
                            {
                                CurrentSignalPattern = SignalPattern.Bullish_ICI_CIC_ICC;

                                if (GetParentSwing(2)!.PatternType == PatternType.BullishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(2)!.Leg3.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(2)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bullish);
                                else if (GetParentSwing(2)!.PatternType == PatternType.BullishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(2)!.Leg2.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(2)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bullish);

                                ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                                {
                                    ParentPatternType = CurrentSignalPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BullishICI?.Invoke(this, new ParentSignalEventArgs
                            {
                                ParentPatternType = CurrentSignalPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else if ((GetParentSwing(2)!.PatternType == PatternType.BearishICI
                        || GetParentSwing(2)!.PatternType == PatternType.BearishCII)
                        && GetParentSwing(1)!.PatternType == PatternType.BearishCIC
                        && GetParentSwing()!.PatternType == PatternType.BearishICC
                        && PriceActionSwing.GetImpulseType(GetParentSwing(2)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(GetParentSwing()!,
                            GetParentSwing(2)!.Leg1.EndElement) != CorrectionType.Brokeback)
                    {
                        if (ValidateTwoCloseBreak(GetParentSwing(2)!))
                        {
                            if (CurrentSignalPattern != SignalPattern.Bearish_ICI_CIC_ICC)
                            {
                                CurrentSignalPattern = SignalPattern.Bearish_ICI_CIC_ICC;

                                if (GetParentSwing(2)!.PatternType == PatternType.BearishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(2)!.Leg3.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(2)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bearish);
                                else if (GetParentSwing(2)!.PatternType == PatternType.BearishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(2)!.Leg2.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(2)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bearish);

                                ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                                {
                                    ParentPatternType = CurrentSignalPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BearishICI?.Invoke(this, new ParentSignalEventArgs
                            {
                                ParentPatternType = CurrentSignalPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    //-----------------------------------------------------------------
                    //--- LEVEL 3 : Four swing scenario in Bullish and Bearish ICI ----
                    //-----------------------------------------------------------------
                    else if ((GetParentSwing(3)!.PatternType == PatternType.BullishICI
                        || GetParentSwing(3)!.PatternType == PatternType.BullishCII)
                        && GetParentSwing(2)!.PatternType == PatternType.BullishCIC
                        && GetParentSwing(1)!.PatternType == PatternType.BullishICC
                        & GetParentSwing()!.PatternType == PatternType.BearishICC
                        && PriceActionSwing.GetImpulseType(GetParentSwing(3)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(GetParentSwing(2)!) != CorrectionType.Brokeback)
                    {
                        if (ValidateTwoCloseBreak(GetParentSwing(3)!))
                        {
                            if (CurrentSignalPattern != SignalPattern.Bullish_ICI_CIC_T_ICC)
                            {
                                CurrentSignalPattern = SignalPattern.Bullish_ICI_CIC_T_ICC;

                                if (GetParentSwing(3)!.PatternType == PatternType.BullishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(3)!.Leg3.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(3)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bullish);
                                else if (GetParentSwing(3)!.PatternType == PatternType.BullishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(3)!.Leg2.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(3)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bullish);

                                ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                                {
                                    ParentPatternType = CurrentSignalPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BullishICI?.Invoke(this, new ParentSignalEventArgs
                            {
                                ParentPatternType = CurrentSignalPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else if ((GetParentSwing(3)!.PatternType == PatternType.BearishICI
                        || GetParentSwing(3)!.PatternType == PatternType.BearishCII)
                        && GetParentSwing(2)!.PatternType == PatternType.BearishCIC
                        && GetParentSwing(1)!.PatternType == PatternType.BearishICC
                        && GetParentSwing()!.PatternType==PatternType.BullishICC
                        && PriceActionSwing.GetImpulseType(GetParentSwing(3)!) == ImpulseType.Breakout
                        && PriceActionSwing.GetCorrectionType(GetParentSwing(2)!) != CorrectionType.Brokeback)
                    {
                        if (ValidateTwoCloseBreak(GetParentSwing(3)!))
                        {
                            if (CurrentSignalPattern != SignalPattern.Bearish_ICI_CIC_T_ICC)
                            {
                                CurrentSignalPattern = SignalPattern.Bearish_ICI_CIC_T_ICC;

                                if (GetParentSwing(3)!.PatternType == PatternType.BearishICI)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(3)!.Leg3.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(3)!.Leg3.EndElement.Candle!.SeqNum, MomentumType.Bearish);
                                else if (GetParentSwing(3)!.PatternType == PatternType.BearishCII)
                                    LastImpulseSlope = parentContainer.GetSlope(
                                        GetParentSwing(3)!.Leg2.BeginElement.Candle!.SeqNum,
                                        GetParentSwing(3)!.Leg2.EndElement.Candle!.SeqNum, MomentumType.Bearish);

                                ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                                {
                                    ParentPatternType = CurrentSignalPattern,
                                    ParentContainer = parentContainer,
                                });
                            }
                            BearishICI?.Invoke(this, new ParentSignalEventArgs
                            {
                                ParentPatternType = CurrentSignalPattern,
                                ParentContainer = parentContainer,
                                ImpulseSlope = LastImpulseSlope
                            });
                        }
                    }
                    else
                    {
                        CurrentSignalPattern = SignalPattern.None;
                        ParentSignalChanged?.Invoke(this, new ParentSignalEventArgs
                        {
                            ParentPatternType = CurrentSignalPattern,
                            ParentContainer = parentContainer,
                            ImpulseSlope = 0
                        });
                    }
                }
                ParentChanged?.Invoke(this, parentContainer);
            }
        }
        private void WhenChildChanged()
        {
            if (ChildContainer is null) throw new ArgumentNullException(nameof(ChildContainer));
            if (ParentContainer is null) throw new ArgumentNullException(nameof(ParentContainer));

            if (CurrentSignalPattern == SignalPattern.Bearish_ICI_CIC)
            {
                var childSwings = GetChildSwings(LegStatus.Leg3, openEnd: true);
                if (childSwings is null) return;

                var lastICI = childSwings.LastOrDefault();

                if (lastICI is not null
                    && lastICI.PatternType == PatternType.BearishICI
                    && PriceActionSwing.GetImpulseType(lastICI) == ImpulseType.Breakout)
                {
                    decimal stop = lastICI.Leg3.BeginElement.Candle.HighPrice;
                    decimal take = TradeCalc.T272(
                            GetParentSwing()!.Leg3.EndElement.Candle.HighPrice,
                            GetParentSwing()!.Leg3.BeginElement.Candle.LowPrice);
                    decimal entry = TradeCalc.GetEntryByRR(stop, take, RiskReward)
                        < lastICI.Leg3.EndElement.Candle.ClosePrice
                        ? lastICI.Leg3.EndElement.Candle.ClosePrice
                        : TradeCalc.GetEntryByRR(stop, take, RiskReward);

                    var signal = new SignalEntity
                    {
                        Id = Guid.NewGuid(),
                        SignalDirection = SignalDirection.Sell,
                        SignalPattern = CurrentSignalPattern,
                        StopLoss = stop,
                        TakeProfit = take,
                        EntryPoint = entry,
                        ParentContainer = ParentContainer,
                        ChildContainer = ChildContainer
                    };

                    if (CurrentSignal is null)
                    {
                        CurrentSignal = signal;
                        CurrentSignal.SignalState = SignalState.Ready;
                        ChildSignalChanged?.Invoke(this, signal);
                    }

                    BearishICISignal?.Invoke(this, signal);
                }
            }
            else if (CurrentSignalPattern == SignalPattern.Bearish_ICI_CIC_ICC)
            {
                var childSwings = GetChildSwings(LegStatus.Leg2, openEnd: true);
                if (childSwings is null) return;
                var lastICI = childSwings.LastOrDefault();

                if (lastICI is not null
                    && lastICI.PatternType == PatternType.BearishICI
                    && PriceActionSwing.GetImpulseType(lastICI) == ImpulseType.Breakout)
                {
                    decimal stop = lastICI.Leg3.BeginElement.Candle.HighPrice;
                    decimal take = TradeCalc.T272(
                            GetParentSwing()!.Leg2.EndElement.Candle.HighPrice,
                            GetParentSwing()!.Leg2.BeginElement.Candle.LowPrice);
                    decimal entry = TradeCalc.GetEntryByRR(stop, take, RiskReward)
                        < lastICI.Leg3.EndElement.Candle.ClosePrice
                        ? lastICI.Leg3.EndElement.Candle.ClosePrice
                        : TradeCalc.GetEntryByRR(stop, take, RiskReward);

                    var signal = new SignalEntity
                    {
                        Id = Guid.NewGuid(),
                        SignalDirection = SignalDirection.Sell,
                        SignalPattern = CurrentSignalPattern,
                        StopLoss = stop,
                        TakeProfit = take,
                        EntryPoint = entry,
                        ParentContainer = ParentContainer,
                        ChildContainer = ChildContainer
                    };

                    if (CurrentSignal is null)
                    {
                        CurrentSignal = signal;
                        CurrentSignal.SignalState = SignalState.Ready;
                        ChildSignalChanged?.Invoke(this, signal);
                    }

                    BearishICISignal?.Invoke(this, signal);
                }
            }
            else if (CurrentSignalPattern == SignalPattern.Bullish_ICI_CIC)
            {
                var childSwings = GetChildSwings(LegStatus.Leg3, openEnd: true);
                if (childSwings is null) return;
                var lastICI = childSwings.LastOrDefault();

                if (lastICI is not null
                    && lastICI.PatternType == PatternType.BullishICI
                    && PriceActionSwing.GetImpulseType(lastICI) == ImpulseType.Breakout)
                {
                    decimal stop = lastICI.Leg3.BeginElement.Candle.LowPrice;
                    decimal take = TradeCalc.T272(
                            GetParentSwing()!.Leg3.EndElement.Candle.LowPrice,
                            GetParentSwing()!.Leg3.BeginElement.Candle.HighPrice);
                    decimal entry = TradeCalc.GetEntryByRR(stop, take, RiskReward)
                        > lastICI.Leg3.EndElement.Candle.ClosePrice
                        ? lastICI.Leg3.EndElement.Candle.ClosePrice
                        : TradeCalc.GetEntryByRR(stop, take, RiskReward);

                    var signal = new SignalEntity
                    {
                        Id = Guid.NewGuid(),
                        SignalDirection = SignalDirection.Buy,
                        SignalPattern = CurrentSignalPattern,
                        StopLoss = stop,
                        TakeProfit = take,
                        EntryPoint = entry,
                        ParentContainer = ParentContainer,
                        ChildContainer = ChildContainer
                    };

                    if (CurrentSignal is null)
                    {
                        CurrentSignal = signal;
                        CurrentSignal.SignalState = SignalState.Ready;
                        ChildSignalChanged?.Invoke(this, signal);
                    }

                    BullishICISignal?.Invoke(this, signal);
                }
            }
            else if (CurrentSignalPattern == SignalPattern.Bullish_ICI_CIC_ICC)
            {
                var childSwings = GetChildSwings(LegStatus.Leg2, openEnd: true);
                if (childSwings is null) return;
                var lastICI = childSwings.LastOrDefault();

                if (lastICI is not null &&
                    lastICI.PatternType == PatternType.BullishICI
                    && PriceActionSwing.GetImpulseType(lastICI) == ImpulseType.Breakout)
                {
                    decimal stop = lastICI.Leg3.BeginElement.Candle.LowPrice;
                    decimal take = TradeCalc.T272(
                            GetParentSwing()!.Leg2.EndElement.Candle.LowPrice,
                            GetParentSwing()!.Leg2.BeginElement.Candle.HighPrice);
                    decimal entry = TradeCalc.GetEntryByRR(stop, take, RiskReward)
                        > lastICI.Leg3.EndElement.Candle.ClosePrice
                        ? lastICI.Leg3.EndElement.Candle.ClosePrice
                        : TradeCalc.GetEntryByRR(stop, take, RiskReward);

                    var signal = new SignalEntity
                    {
                        Id = Guid.NewGuid(),
                        SignalDirection = SignalDirection.Buy,
                        SignalPattern = CurrentSignalPattern,
                        StopLoss = stop,
                        TakeProfit = take,
                        EntryPoint = entry,
                        ParentContainer = ParentContainer,
                        ChildContainer = ChildContainer
                    };

                    if (CurrentSignal is null)
                    {
                        CurrentSignal = signal;
                        CurrentSignal.SignalState = SignalState.Ready;
                        ChildSignalChanged?.Invoke(this, signal);
                    }

                    BullishICISignal?.Invoke(this, signal);
                }
            }
            ChildChanged?.Invoke(this, childContainer!);
        }
        private void ChildContainer_MicroCandleChanged(object? sender, Candle e)
        {
            if (CurrentSignal?.SignalDirection == SignalDirection.Buy)
            {
                if (CurrentSignal?.SignalState == SignalState.Ready)
                {
                    if (e.LowPrice <= CurrentSignal?.EntryPoint)
                        CurrentSignal.SignalState = SignalState.EntryHitted;

                    else if (e.HighPrice >= CurrentSignal?.TakeProfit)
                    {
                        CurrentSignal.SignalState = SignalState.MissedOut;
                        PositionHistory.Add(new PositionEntity
                        {
                            Id = CurrentSignal.Id,
                            SignalDirection = CurrentSignal.SignalDirection,
                            EntryPoint = CurrentSignal.EntryPoint,
                            StopLoss = CurrentSignal.StopLoss,
                            TakeProfit = CurrentSignal.TakeProfit,
                            PositionState = PositionState.Missedout,
                            SignalPattern = CurrentSignal.SignalPattern,
                        });
                        CurrentSignal = null;
                    }
                }
                else if (CurrentSignal?.SignalState == SignalState.EntryHitted)
                {
                    if (e.LowPrice <= CurrentSignal?.StopLoss)
                    {
                        CurrentSignal.SignalState = SignalState.StopHitted;
                        PositionHistory.Add(new PositionEntity
                        {
                            Id = CurrentSignal.Id,
                            SignalDirection= CurrentSignal.SignalDirection,
                            EntryPoint = CurrentSignal.EntryPoint,
                            StopLoss = CurrentSignal.StopLoss,
                            TakeProfit = CurrentSignal.TakeProfit,
                            PositionState = PositionState.Stop,
                            SignalPattern = CurrentSignal.SignalPattern,
                        });
                        CurrentSignal = null;
                    }
                    else if (e.HighPrice >= CurrentSignal?.TakeProfit)
                    {
                        CurrentSignal.SignalState = SignalState.TakeProfitHitted;
                        PositionHistory.Add(new PositionEntity
                        {
                            Id = CurrentSignal.Id,
                            SignalDirection = CurrentSignal.SignalDirection,
                            EntryPoint = CurrentSignal.EntryPoint,
                            StopLoss = CurrentSignal.StopLoss,
                            TakeProfit = CurrentSignal.TakeProfit,
                            PositionState = PositionState.Profit,
                            SignalPattern = CurrentSignal.SignalPattern,
                        });
                        CurrentSignal = null;
                    }
                }
            }
            else if (CurrentSignal?.SignalDirection == SignalDirection.Sell)
            {
                if (CurrentSignal.SignalState == SignalState.Ready)
                {
                    if (e.HighPrice >= CurrentSignal?.EntryPoint)
                        CurrentSignal.SignalState = SignalState.EntryHitted;

                    if (e.LowPrice <= CurrentSignal?.TakeProfit)
                    {
                        CurrentSignal.SignalState = SignalState.MissedOut;
                        PositionHistory.Add(new PositionEntity
                        {
                            Id = CurrentSignal.Id,
                            SignalDirection = CurrentSignal.SignalDirection,
                            EntryPoint = CurrentSignal.EntryPoint,
                            StopLoss = CurrentSignal.StopLoss,
                            TakeProfit = CurrentSignal.TakeProfit,
                            PositionState = PositionState.Missedout,
                            SignalPattern = CurrentSignal.SignalPattern,
                        });
                        CurrentSignal = null;
                    }
                }
                else if (CurrentSignal.SignalState == SignalState.EntryHitted)
                {
                    if (e.HighPrice >= CurrentSignal?.StopLoss)
                    {
                        CurrentSignal.SignalState = SignalState.StopHitted;
                        PositionHistory.Add(new PositionEntity
                        {
                            Id = CurrentSignal.Id,
                            SignalDirection = CurrentSignal.SignalDirection,
                            EntryPoint = CurrentSignal.EntryPoint,
                            StopLoss = CurrentSignal.StopLoss,
                            TakeProfit = CurrentSignal.TakeProfit,
                            PositionState = PositionState.Stop,
                            SignalPattern = CurrentSignal.SignalPattern,
                        });
                        CurrentSignal = null;
                    }
                    else if (e.LowPrice <= CurrentSignal?.TakeProfit)
                    {
                        CurrentSignal.SignalState = SignalState.TakeProfitHitted;
                        PositionHistory.Add(new PositionEntity
                        {
                            Id = CurrentSignal.Id,
                            SignalDirection = CurrentSignal.SignalDirection,
                            EntryPoint = CurrentSignal.EntryPoint,
                            StopLoss = CurrentSignal.StopLoss,
                            TakeProfit = CurrentSignal.TakeProfit,
                            PositionState = PositionState.Profit,
                            SignalPattern = CurrentSignal.SignalPattern,
                        });
                        CurrentSignal = null;
                    }
                }
            }
        }

        private PriceActionSwing? GetParentSwing(int level = 0)
        {
            if (level < 0) throw new ArgumentOutOfRangeException(nameof(level) + " must grater than 0");

            if (ParentContainer != null)
                return ParentContainer.Swings.SkipLast(level).LastOrDefault().Value;
            else
                return null;
        }
        private List<PriceActionSwing>? GenerateChildList(bool nonStop, long cStartSeq, long cEndSeq)
        {
            if (!nonStop)
            {
                var childCandles = ChildContainer!.Candles.Where(c => c.SeqNum >= cStartSeq && c.SeqNum <= cEndSeq).ToList();
                return PriceActionContainer.GenerateContainer(childCandles, childContainer!.MicroCandles).Swings.Values.ToList();
            }
            else
            {
                var childCandles = ChildContainer!.Candles.Where(c => c.SeqNum >= cStartSeq).ToList();
                return PriceActionContainer.GenerateContainer(childCandles, childContainer!.MicroCandles).Swings.Values.ToList();
            }
        }
        private bool ValidateTwoCloseBreak(PriceActionSwing swing)
        {
            if (!CheckTwoCloseBreak) return true;

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

        #endregion
    }
}
