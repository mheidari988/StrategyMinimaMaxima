using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using StockSharp.Algo.Candles;
using StockSharp.Logging;

namespace TradeCore.PriceAction
{
    public class PriceActionContainer
    {
        #region Private Fields

        private long _candleSeq = 0;
        private long _microSeq = 0;
        #endregion

        #region Public Properties

        public int TimeFrameMinutes { get; init; }
        public bool IsCapacityEnabled { get; set; } = false;
        public int CandleCapacity { get; set; } = 48;
        public int MicroCapacity { get => CandleCapacity * TimeFrameMinutes; }
        public long ProcessLimit { get; set; } = 0;
        public Dictionary<long, PriceActionElement> Elements { get; private set; }
        public Dictionary<long, PriceActionElement> ChainedElements { get; private set; }
        public Dictionary<long, PriceActionLeg> Legs { get; private set; }
        public Dictionary<long, PriceActionSwing> Swings { get; private set; }
        public List<Candle> MicroCandles { get; private set; }
        public List<Candle> Candles { get; private set; }
        public List<Candle> ValleyCandles { get; private set; }
        public List<Candle> PeakCandles { get; private set; }

        #endregion

        #region Constructors
        public PriceActionContainer(int timeFrameMinutes)
        {
            TimeFrameMinutes= timeFrameMinutes;
            MicroCandles = new List<Candle>();
            Candles = new List<Candle>();
            ValleyCandles = new List<Candle>();
            PeakCandles = new List<Candle>();
            Elements = new Dictionary<long, PriceActionElement>();
            ChainedElements = new Dictionary<long, PriceActionElement>();
            Legs = new Dictionary<long, PriceActionLeg>();
            Swings = new Dictionary<long, PriceActionSwing>();
        }

        #endregion

        #region Events
        public event EventHandler<Candle>? MicroCandleChanged;
        #endregion

        #region Public Methods

        public void AddCandle(Candle candle, bool autoSeqNum = true)
        {
            if (candle is null)
                throw new ArgumentNullException(nameof(candle));

            if (ProcessLimit == 0)
            {
                if (autoSeqNum)
                    candle.SeqNum = _candleSeq++;
                Candles.Add(candle);
                if (IsCapacityEnabled && Candles.Count >= CandleCapacity)
                    Candles.RemoveAt(Candles.Count - CandleCapacity);
                ProcessPeaksAndValleys();
                ProcessChainedElements();
                ProcessLegs();
                ProcessSwings();
            }
            else
            {
                if (_candleSeq < ProcessLimit)
                {
                    if (autoSeqNum)
                        candle.SeqNum = _candleSeq++;
                    Candles.Add(candle);
                    if (IsCapacityEnabled && Candles.Count >= CandleCapacity)
                        Candles.RemoveAt(Candles.Count - CandleCapacity);
                    ProcessPeaksAndValleys();
                    ProcessChainedElements();
                    ProcessLegs();
                    ProcessSwings();
                }
            }
        }
        public void AddMicroCandle(Candle candle, bool autoSeqNum = true)
        {
            if (autoSeqNum)
                candle.SeqNum = _microSeq++;
            MicroCandles.Add(candle);
            if (IsCapacityEnabled && MicroCandles.Count >= MicroCapacity)
                MicroCandles.RemoveAt(MicroCandles.Count - MicroCapacity);

            MicroCandleChanged?.Invoke(this, candle);
        }
        public static PriceActionContainer GenerateContainer(List<Candle> candles, List<Candle> microCandle)
        {
            if (candles is null)
                throw new ArgumentNullException(nameof(candles));
            if (microCandle is null)
                throw new ArgumentNullException(nameof(microCandle));

            var container = new PriceActionContainer(15);
            var selectedMicro =
                microCandle.Where(c => c.SeqNum >= candles.FirstOrDefault()!.SeqNum * container.TimeFrameMinutes).ToList();

            foreach (var item in selectedMicro)
                container.AddMicroCandle(item, false); // populate micro first.
            foreach (var item in candles)
                container.AddCandle(item, false); // false means: use old SeqNum

            return container;
        }
        public decimal GetSlope(long startSeqNum, long endSeqNum, MomentumType momentum)
        {
            if (Candles is null) throw new NullReferenceException(nameof(Candles));

            var startPoint = Candles.SingleOrDefault(x => x.SeqNum == startSeqNum);
            var endPoint = Candles.SingleOrDefault(x => x.SeqNum == endSeqNum);
            if (startPoint == null || endPoint == null) throw new ArgumentNullException(nameof(startPoint));

            var distance = (endSeqNum - startSeqNum) + 1;

            switch (momentum)
            {
                case MomentumType.Bullish:
                    return (BaseOneTruncate(endPoint.HighPrice) - BaseOneTruncate(startPoint.LowPrice)) / distance;
                case MomentumType.Bearish:
                    return (BaseOneTruncate(endPoint.LowPrice) - BaseOneTruncate(startPoint.HighPrice)) / distance;
                case MomentumType.None:
                    throw new ArgumentOutOfRangeException(nameof(momentum));
                default:
                    throw new ArgumentOutOfRangeException(nameof(momentum));
            }
        }
        public decimal GetAbsoluteSlope(long startSeqNum, long endSeqNum, MomentumType momentum)
            => Math.Abs(GetSlope(startSeqNum, endSeqNum, momentum));

        #endregion

        #region Private Methods
        private bool IsMinimaFirst(Candle target)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));

            if (target.SeqNum == 0) 
                return true;
            else
            {
                var query = MicroCandles.Where(x =>
                x.SeqNum >= target.SeqNum * TimeFrameMinutes - TimeFrameMinutes &&
                x.SeqNum <= target.SeqNum * TimeFrameMinutes);

                var high = query.Where(x => x.HighPrice == query.Max(x => x.HighPrice)).FirstOrDefault();
                var low = query.Where(x => x.LowPrice == query.Min(x => x.LowPrice)).FirstOrDefault();
                if (high != null && low != null)
                    return low.SeqNum <= high.SeqNum;
                else
                    return false;
            }
        }
        private void ProcessPeaksAndValleys()
        {
            ValleyCandles.Clear();
            PeakCandles.Clear();
            Elements.Clear();

            var _indexCounter = 0;

            if (Candles.Count < 3) return;

            //------------------If candle is bullish then add minima first------------------
            if (Candles[0].OpenPrice < Candles[0].ClosePrice)
            {
                if (Candles[0].LowPrice < Candles[1].LowPrice)
                {
                    PriceActionElement _min = new PriceActionElement(Candles[0])
                    {
                        PeakValleyType = PeakValleyType.Valley
                    };
                    Elements.Add(_indexCounter++, _min);

                    ValleyCandles.Add(Candles[0]);
                }
                if (Candles[0].HighPrice > Candles[1].HighPrice)
                {
                    PriceActionElement _max = new PriceActionElement(Candles[0])
                    {
                        PeakValleyType = PeakValleyType.Peak
                    };
                    Elements.Add(_indexCounter++, _max);

                    PeakCandles.Add(Candles[0]);
                }
            }
            else
            {
                if (Candles[0].HighPrice > Candles[1].HighPrice)
                {
                    PriceActionElement _max = new PriceActionElement(Candles[0])
                    {
                        PeakValleyType = PeakValleyType.Peak
                    };
                    Elements.Add(_indexCounter++, _max);

                    PeakCandles.Add(Candles[0]);
                }
                if (Candles[0].LowPrice < Candles[1].LowPrice)
                {
                    PriceActionElement min = new PriceActionElement(Candles[0])
                    {
                        PeakValleyType = PeakValleyType.Valley
                    };
                    Elements.Add(_indexCounter++, min);

                    ValleyCandles.Add(Candles[0]);
                }
            }

            //------------------Iterate over other Candles to find minima and maxima------------------
            for (int i = 1; i < Candles.Count - 1; i++)
            {
                if (Candles[i - 1].LowPrice >= Candles[i].LowPrice && Candles[i].LowPrice <= Candles[i + 1].LowPrice)
                {
                    PriceActionElement min = new PriceActionElement(Candles[i])
                    {
                        PeakValleyType = PeakValleyType.Valley
                    };
                    Elements.Add(_indexCounter++, min);

                    ValleyCandles.Add(Candles[i]);
                }
                if (Candles[i - 1].HighPrice <= Candles[i].HighPrice && Candles[i].HighPrice >= Candles[i + 1].HighPrice)
                {
                    PriceActionElement _max = new PriceActionElement(Candles[i])
                    {
                        PeakValleyType = PeakValleyType.Peak
                    };
                    Elements.Add(_indexCounter++, _max);

                    PeakCandles.Add(Candles[i]);
                }
            }
            //------------------If last candle is bullish then add minima last------------------
            if (Candles[Candles.Count - 1].OpenPrice < Candles[Candles.Count - 1].ClosePrice)
            {
                if (Candles[Candles.Count - 1].LowPrice < Candles[Candles.Count - 2].LowPrice)
                {
                    PriceActionElement min = new PriceActionElement(Candles[Candles.Count - 1])
                    {
                        PeakValleyType = PeakValleyType.Valley
                    };
                    Elements.Add(_indexCounter++, min);

                    ValleyCandles.Add(Candles[Candles.Count - 1]);
                }
                if (Candles[Candles.Count - 1].HighPrice > Candles[Candles.Count - 2].HighPrice)
                {
                    PriceActionElement _max = new PriceActionElement(Candles[Candles.Count - 1])
                    {
                        PeakValleyType = PeakValleyType.Peak
                    };
                    Elements.Add(_indexCounter++, _max);

                    PeakCandles.Add(Candles[Candles.Count - 1]);
                }
            }
            else if (Candles[Candles.Count - 1].OpenPrice > Candles[Candles.Count - 1].ClosePrice)
            {
                if (Candles[Candles.Count - 1].HighPrice > Candles[Candles.Count - 2].HighPrice)
                {
                    PriceActionElement _max = new PriceActionElement(Candles[Candles.Count - 1])
                    {
                        PeakValleyType = PeakValleyType.Peak
                    };
                    Elements.Add(_indexCounter++, _max);

                    PeakCandles.Add(Candles[Candles.Count - 1]);
                }
                if (Candles[Candles.Count - 1].LowPrice < Candles[Candles.Count - 2].LowPrice)
                {
                    PriceActionElement min = new PriceActionElement(Candles[Candles.Count - 1])
                    {
                        PeakValleyType = PeakValleyType.Valley
                    };
                    Elements.Add(_indexCounter++, min);

                    ValleyCandles.Add(Candles[Candles.Count - 1]);
                }
            }
        }
        private void ProcessChainedElements()
        {
            if (Elements == null)
                throw new NullReferenceException("SwingElements cannot be null.");

            if (Elements.Count == 0) return;

            long _chainIndex = 0;
            ChainedElements.Clear();

            ChainedElements.Add(_chainIndex++, Elements[0]);

            if (Elements.Count > 1)
            {
                for (int i = 1; i < Elements.Count; i++)
                {
                    if (Elements[i].PeakValleyType == PeakValleyType.Valley
                        && Elements[i - 1].PeakValleyType == PeakValleyType.Valley)
                    {
                        //----------------------If current element's LowPrice is lower than----------------
                        //----------------------previous element's LowPrice--------------------------------
                        if (Elements[i].Candle!.LowPrice < Elements[i - 1].Candle!.LowPrice)
                        {
                            ChainedElements.Remove(_chainIndex - 1);
                            ChainedElements.Add(_chainIndex - 1, Elements[i]);
                        }
                    }
                    else if (Elements[i].PeakValleyType == PeakValleyType.Peak
                        && Elements[i - 1].PeakValleyType == PeakValleyType.Peak)
                    {
                        //----------------------If current element's HighPrice is higher than----------------
                        //----------------------previous element's HighPrice: We ----------------------------------
                        if (Elements[i].Candle!.HighPrice > Elements[i - 1].Candle!.HighPrice)
                        {
                            ChainedElements.Remove(_chainIndex - 1);
                            ChainedElements.Add(_chainIndex - 1, Elements[i]);
                        }
                    }
                    else
                    {
                        //------------------Simply add the element to the ChainedElements-------------
                        ChainedElements.Add(_chainIndex++, Elements[i]);
                    }
                }
            }
        }
        private void ProcessLegs()
        {
            if (ChainedElements == null)
                throw new NullReferenceException("ChainedSwingElements cannot be null.");

            Legs.Clear();

            long _legIndex = 0;
            if (ChainedElements.Count >= 2)
            {
                for (int i = 1; i < ChainedElements.Count; i++)
                {
                    Legs.Add(_legIndex++, new PriceActionLeg(ChainedElements[i - 1], ChainedElements[i]));
                }
            }
        }
        private void ProcessSwings()
        {
            Swings.Clear();

            long _swingIndex = 0;

            if (Legs.Count < 2) return;
            for (int i = 1; i < Legs.Count - 1; i++)
            {
                Swings.Add(_swingIndex++, new PriceActionSwing(Legs[i - 1], Legs[i], Legs[i + 1]));
            }
        }
        private decimal BaseOneTruncate(decimal price)
        {
            if (price != 0 && price >= 1)
                while (Math.Truncate(price / 10) > 0) price /= 10;
            else if (price != 0 && price < 1)
                while ((price * 10) < 10) price *= 10;
            return price;
        }
        #endregion
    }
}

