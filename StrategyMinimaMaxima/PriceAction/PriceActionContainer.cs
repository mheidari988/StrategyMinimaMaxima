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

        private long CandleSeqNum = 0;
        private long MicroSeqNum = 0;
        #endregion

        #region Public Properties

        public bool PeakValleyByMicro { get; set; } = false;
        public int TimeFrameMinutes { get; init; }
        public bool IsCapacityEnabled { get; set; } = false;
        public int CandleCapacity { get; set; } = 48;
        public int MicroCapacity { get => CandleCapacity * TimeFrameMinutes; }
        public long ProcessLimit { get; set; } = 0;
        public Dictionary<long, PriceActionElement> PeaksAndValleys { get; private set; }
        public Dictionary<long, PriceActionElement> ChainedPeaksAndValleys { get; private set; }
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
            MicroCandles = new();
            Candles = new();
            ValleyCandles = new();
            PeakCandles = new();
            PeaksAndValleys = new();
            ChainedPeaksAndValleys = new();
            Legs = new();
            Swings = new();
            List<int> lst = new();
        }

        #endregion

        #region Events
        public event EventHandler<Candle>? MicroCandleAdded;
        public event EventHandler<Candle>? CandleAdded;
        #endregion

        #region Public Methods
        public void AddCandle(Candle candle, bool autoSeqNum = true)
        {
            if (candle is null)
                throw new ArgumentNullException(nameof(candle));

            if (ProcessLimit == 0)
            {
                StartAddCandleProcess(candle, autoSeqNum);
            }
            else if (CandleSeqNum < ProcessLimit)
            {
                StartAddCandleProcess(candle, autoSeqNum);
            }
        }
        public void AddMicroCandle(Candle candle, bool autoSeqNum = true)
        {
            if (autoSeqNum)
                candle.SeqNum = MicroSeqNum++;
            MicroCandles.Add(candle);
            if (IsCapacityEnabled && MicroCandles.Count >= MicroCapacity)
                MicroCandles.RemoveAt(MicroCandles.Count - MicroCapacity);

            MicroCandleAdded?.Invoke(this, candle);
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
        private void StartAddCandleProcess(Candle candle, bool autoSeqNum)
        {
            if (autoSeqNum)
                candle.SeqNum = CandleSeqNum++;
            Candles.Add(candle);
            CandleAdded?.Invoke(this, candle);
            if (IsCapacityEnabled && Candles.Count >= CandleCapacity)
                Candles.RemoveAt(Candles.Count - CandleCapacity);
            ProcessPeaksAndValleys();
            ProcessPeaksAndValleysChain();
            ProcessLegs();
            ProcessSwings();
        }
        private bool IsMinimaFirst(Candle target)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));

            if (target.SeqNum == 0) 
                return true;
            else
                return PeakValleyByMicro ? IsMinimaFirstBasedOnMicro(target) : IsBullishCandle(target);
        }
        private bool IsMinimaFirstBasedOnMicro(Candle target)
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
        private void AddMinima(int index, Candle candle)
        {
            PeaksAndValleys.Add(index, new PriceActionElement(candle)
            {
                PeakValleyType = PeakValleyType.Valley
            });
            ValleyCandles.Add(candle);
        }
        private void AddMaxima(int index, Candle candle)
        {
            PeaksAndValleys.Add(index, new PriceActionElement(candle)
            {
                PeakValleyType = PeakValleyType.Peak
            });
            PeakCandles.Add(candle);
        }
        private void ProcessPeaksAndValleys()
        {
            if (Candles.Count < 3) return;

            var index = 0;
            ValleyCandles.Clear();
            PeakCandles.Clear();
            PeaksAndValleys.Clear();

            // First Element check
            if (IsMinimaFirst(Candles[0]))
            {
                if (Candles[0].LowPrice < Candles[1].LowPrice)
                    AddMinima(index++, Candles[0]);

                if (Candles[0].HighPrice > Candles[1].HighPrice)
                    AddMaxima(index++, Candles[0]);
            }
            else
            {
                if (Candles[0].HighPrice > Candles[1].HighPrice)
                    AddMaxima(index++, Candles[0]);

                if (Candles[0].LowPrice < Candles[1].LowPrice)
                    AddMinima(index++, Candles[0]);
            }

            //Middle elements Loop
            for (int i = 1; i < Candles.Count - 1; i++)
            {
                if (IsMinimaFirst(Candles[i]))
                {
                    if (Candles[i - 1].LowPrice >= Candles[i].LowPrice && Candles[i].LowPrice <= Candles[i + 1].LowPrice)
                        AddMinima(index++, Candles[i]);

                    if (Candles[i - 1].HighPrice <= Candles[i].HighPrice && Candles[i].HighPrice >= Candles[i + 1].HighPrice)
                        AddMaxima(index++, Candles[i]);
                }
                else
                {
                    if (Candles[i - 1].HighPrice <= Candles[i].HighPrice && Candles[i].HighPrice >= Candles[i + 1].HighPrice)
                        AddMaxima(index++, Candles[i]);
                    if (Candles[i - 1].LowPrice >= Candles[i].LowPrice && Candles[i].LowPrice <= Candles[i + 1].LowPrice)
                        AddMinima(index++, Candles[i]);
                }
            }

            //End Candles Check
            if (IsMinimaFirst(Candles[^1]))
            {
                if (Candles[^1].LowPrice < Candles[^2].LowPrice)
                    AddMinima(index++, Candles[^1]);

                if (Candles[^1].HighPrice > Candles[^2].HighPrice)
                    AddMaxima(index++, Candles[^1]);
            }
            else if (Candles[^1].OpenPrice > Candles[^1].ClosePrice)
            {
                if (Candles[^1].HighPrice > Candles[^2].HighPrice)
                    AddMaxima(index++, Candles[^1]);

                if (Candles[^1].LowPrice < Candles[^2].LowPrice)
                    AddMinima(index++, Candles[^1]);
            }
        }
        private void ProcessPeaksAndValleysChain()
        {
            if (PeaksAndValleys.Count == 0) return;

            long _chainIndex = 0;
            ChainedPeaksAndValleys.Clear();

            ChainedPeaksAndValleys.Add(_chainIndex++, PeaksAndValleys[0]);

            if (PeaksAndValleys.Count > 1)
            {
                for (int i = 1; i < PeaksAndValleys.Count; i++)
                {
                    if (PeaksAndValleys[i].PeakValleyType == PeakValleyType.Valley
                        && PeaksAndValleys[i - 1].PeakValleyType == PeakValleyType.Valley)
                    {
                        //----------------------If current element's LowPrice is lower than----------------
                        //----------------------previous element's LowPrice--------------------------------
                        if (PeaksAndValleys[i].Candle!.LowPrice < PeaksAndValleys[i - 1].Candle!.LowPrice)
                        {
                            ChainedPeaksAndValleys.Remove(_chainIndex - 1);
                            ChainedPeaksAndValleys.Add(_chainIndex - 1, PeaksAndValleys[i]);
                        }
                    }
                    else if (PeaksAndValleys[i].PeakValleyType == PeakValleyType.Peak
                        && PeaksAndValleys[i - 1].PeakValleyType == PeakValleyType.Peak)
                    {
                        //----------------------If current element's HighPrice is higher than----------------
                        //----------------------previous element's HighPrice: We ----------------------------------
                        if (PeaksAndValleys[i].Candle!.HighPrice > PeaksAndValleys[i - 1].Candle!.HighPrice)
                        {
                            ChainedPeaksAndValleys.Remove(_chainIndex - 1);
                            ChainedPeaksAndValleys.Add(_chainIndex - 1, PeaksAndValleys[i]);
                        }
                    }
                    else
                    {
                        //------------------Simply add the element to the ChainedElements-------------
                        ChainedPeaksAndValleys.Add(_chainIndex++, PeaksAndValleys[i]);
                    }
                }
            }
        }
        private void ProcessLegs()
        {
            long LegIndex = 0;
            Legs.Clear();

            if (ChainedPeaksAndValleys.Count >= 2)
            {
                for (int i = 1; i < ChainedPeaksAndValleys.Count; i++)
                {
                    var beginElement = ChainedPeaksAndValleys[i - 1];
                    var endElement = ChainedPeaksAndValleys[i];
                    var candles = Candles.Where(c => c.SeqNum >= beginElement.Candle.SeqNum && c.SeqNum <= endElement.Candle.SeqNum);
                    Legs.Add(LegIndex++, new PriceActionLeg(beginElement, endElement, Candles.ToList()));
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
                var leg1 = Legs[i - 1];
                var leg2 = Legs[i];
                var leg3 = Legs[i + 1];
                var candles = Candles.Where(c => c.SeqNum >= leg1.BeginElement.Candle.SeqNum && c.SeqNum <= leg3.EndElement.Candle.SeqNum);
                Swings.Add(_swingIndex++, new PriceActionSwing(leg1, leg2, leg3, candles.ToList()));
            }
        }
        #endregion

        #region Static Methods
        private static bool IsBullishCandle(Candle target)
            => (target.ClosePrice > target.OpenPrice);
        private static decimal BaseOneTruncate(decimal price)
        {
            if (price != 0 && price >= 1)
                while (Math.Truncate(price / 10) > 0) price /= 10;
            else if (price != 0 && price < 1)
                while ((price * 10) < 10) price *= 10;
            return price;
        }
        public static PriceActionContainer? GenerateContainer(int timeframe, List<Candle> candles, List<Candle> microCandle)
        {
            if (candles is null)
                throw new ArgumentNullException(nameof(candles));
            if (microCandle is null)
                throw new ArgumentNullException(nameof(microCandle));
            if (candles.Count < 1) return null;

            var container = new PriceActionContainer(timeframe);
            var selectedMicro =
                microCandle.Where(c => c.SeqNum >= candles[0].SeqNum * container.TimeFrameMinutes).ToList();

            foreach (var item in selectedMicro)
                container.AddMicroCandle(item, false); // populate micro first.
            foreach (var item in candles)
                container.AddCandle(item, false); // false means: use old SeqNum

            return container;
        }
        #endregion
    }
}
