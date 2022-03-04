using StockSharp.Algo.Candles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TradeCore.PriceAction
{
    public class PriceActionContainer
    {
        #region Constants
        private const int DEF_CANDLE_CAPACITY = 48;
        private const int DEF_PROCESS_LIMIT = 0;
        #endregion

        #region Private Fields
        private long CandleSeqNum = 0;
        private long MicroSeqNum = 0;
        #endregion

        #region Constructors
        public PriceActionContainer(int timeFrameMinutes, int microTimeFrameMinutes = 3)
        {
            TimeFrameMinutes = timeFrameMinutes;
            MicroTimeFrameMinutes = microTimeFrameMinutes;
            MicroCandles = new();
            Candles = new();
            PeaksAndValleys = new();
            ChainedPeaksAndValleys = new();
            Legs = new();
            Swings = new();
        }

        #endregion

        #region Events
        public event EventHandler<Candle> MicroCandleAdded;
        public event EventHandler<Candle> CandleAdded;
        #endregion

        #region Public Properties
        public List<Candle> MicroCandles { get; private set; }
        public List<Candle> Candles { get; private set; }
        public Dictionary<long, PriceActionElement> PeaksAndValleys { get; private set; }
        public Dictionary<long, PriceActionElement> ChainedPeaksAndValleys { get; private set; }
        public Dictionary<long, PriceActionLeg> Legs { get; private set; }
        public Dictionary<long, PriceActionSwing> Swings { get; private set; }
        public int TimeFrameMinutes { get; init; }
        public int MicroTimeFrameMinutes { get; set; }
        public bool IsProcessByMicro { get; set; } = false;
        public bool IsLimitedCapacity { get; set; } = true;
        public int CandleCapacity { get; set; } = DEF_CANDLE_CAPACITY;
        public int MicroCapacity { get => CandleCapacity * TimeFrameMinutes; }
        public long ProcessLimit { get; set; } = DEF_PROCESS_LIMIT;

        #endregion

        #region Public Methods

        public void AddCandle(Candle candle, bool autoSeqNum = true)
        {
            if (candle is null)
            {
                throw new ArgumentNullException(nameof(candle));
            }

            if (ProcessLimit == 0)
            {
                StartAddCandleProcess(candle, autoSeqNum);
            }
            else if (CandleSeqNum < ProcessLimit)
            {
                StartAddCandleProcess(candle, autoSeqNum);
            }
        }
        public void AddCandleRange(IEnumerable<Candle> candles, bool autoSeqNum = true)
        {
            if (candles is null)
            {
                throw new ArgumentNullException(nameof(candles));
            }

            foreach (var candle in candles)
            {
                AddCandle(candle, autoSeqNum);
            }
        }
        public void AddMicroCandle(Candle candle, bool autoSeqNum = true)
        {
            if (autoSeqNum)
            {
                candle.SeqNum = MicroSeqNum++;
            }

            MicroCandles.Add(candle);
            if (IsLimitedCapacity && MicroCandles.Count >= MicroCapacity)
            {
                MicroCandles.RemoveAt(MicroCandles.Count - MicroCapacity);
            }

            MicroCandleAdded?.Invoke(this, candle);
        }
        public void AddMicroCandleRange(IEnumerable<Candle> candles, bool autoSeqNum = true)
        {
            if (candles is null)
            {
                throw new ArgumentNullException(nameof(candles));
            }

            foreach (var candle in candles)
            {
                AddMicroCandle(candle, autoSeqNum);
            }
        }

        #endregion

        #region Private Methods
        private void StartAddCandleProcess(Candle candle, bool autoSeqNum)
        {
            if (autoSeqNum)
            {
                candle.SeqNum = CandleSeqNum++;
            }

            Candles.Add(candle);
            CandleAdded?.Invoke(this, candle);
            if (IsLimitedCapacity && Candles.Count >= CandleCapacity)
            {
                Candles.RemoveAt(Candles.Count - CandleCapacity);
            }

            ProcessPeaksAndValleys();
            ProcessPeaksAndValleysChain();
            ProcessLegs();
            ProcessSwings();
        }
        private bool IsMinimaFirst(Candle target)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (target.SeqNum == 0)
            {
                return true;
            }
            else
            {
                return IsProcessByMicro ? !IsMinimaFirstBasedOnMicro(target) : IsBullishCandle(target);
            }
        }
        private bool IsMinimaFirstBasedOnMicro(Candle target)
        {
            var microTime = TimeFrameMinutes / MicroTimeFrameMinutes;
            var query = MicroCandles.Where(x =>
            x.SeqNum >= target.SeqNum * microTime - microTime &&
            x.SeqNum <= target.SeqNum * microTime);
            var high = query.Where(x => x.HighPrice == query.Max(x => x.HighPrice)).FirstOrDefault();
            var low = query.Where(x => x.LowPrice == query.Min(x => x.LowPrice)).FirstOrDefault();
            if (high != null && low != null)
            {
                return low.SeqNum <= high.SeqNum;
            }
            else
            {
                return false;
            }
        }
        private void AddMinima(int index, Candle candle)
        {
            PeaksAndValleys.Add(index, new PriceActionElement(candle)
            {
                PeakValleyType = PeakValleyType.Valley
            });
        }
        private void AddMaxima(int index, Candle candle)
        {
            PeaksAndValleys.Add(index, new PriceActionElement(candle)
            {
                PeakValleyType = PeakValleyType.Peak
            });
        }
        private void ProcessPeaksAndValleys()
        {
            if (Candles.Count < 3)
            {
                return;
            }

            var index = 0;
            PeaksAndValleys.Clear();

            // First Element check
            if (IsMinimaFirst(Candles[0]))
            {
                if (Candles[0].LowPrice < Candles[1].LowPrice)
                {
                    AddMinima(index++, Candles[0]);
                }

                if (Candles[0].HighPrice > Candles[1].HighPrice)
                {
                    AddMaxima(index++, Candles[0]);
                }
            }
            else
            {
                if (Candles[0].HighPrice > Candles[1].HighPrice)
                {
                    AddMaxima(index++, Candles[0]);
                }

                if (Candles[0].LowPrice < Candles[1].LowPrice)
                {
                    AddMinima(index++, Candles[0]);
                }
            }

            //Middle elements Loop
            for (int i = 1; i < Candles.Count - 1; i++)
            {
                if (IsMinimaFirst(Candles[i]))
                {
                    if (Candles[i - 1].LowPrice >= Candles[i].LowPrice && Candles[i].LowPrice <= Candles[i + 1].LowPrice)
                    {
                        AddMinima(index++, Candles[i]);
                    }

                    if (Candles[i - 1].HighPrice <= Candles[i].HighPrice && Candles[i].HighPrice >= Candles[i + 1].HighPrice)
                    {
                        AddMaxima(index++, Candles[i]);
                    }
                }
                else
                {
                    if (Candles[i - 1].HighPrice <= Candles[i].HighPrice && Candles[i].HighPrice >= Candles[i + 1].HighPrice)
                    {
                        AddMaxima(index++, Candles[i]);
                    }

                    if (Candles[i - 1].LowPrice >= Candles[i].LowPrice && Candles[i].LowPrice <= Candles[i + 1].LowPrice)
                    {
                        AddMinima(index++, Candles[i]);
                    }
                }
            }

            //End Candles Check
            if (IsMinimaFirst(Candles[^1]))
            {
                if (Candles[^1].LowPrice < Candles[^2].LowPrice)
                {
                    AddMinima(index++, Candles[^1]);
                }

                if (Candles[^1].HighPrice > Candles[^2].HighPrice)
                {
                    AddMaxima(index++, Candles[^1]);
                }
            }
            else
            {
                if (Candles[^1].HighPrice > Candles[^2].HighPrice)
                {
                    AddMaxima(index++, Candles[^1]);
                }

                if (Candles[^1].LowPrice < Candles[^2].LowPrice)
                {
                    AddMinima(index++, Candles[^1]);
                }
            }
        }
        private void ProcessPeaksAndValleysChain()
        {
            if (PeaksAndValleys.Count == 0)
            {
                return;
            }

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

            if (Legs.Count < 2)
            {
                return;
            }

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
        public static PriceActionContainer? GenerateContainer(int timeframe, int microTimeFrame,
            List<Candle> candles, List<Candle> microCandle)
        {
            if (candles is null)
            {
                throw new ArgumentNullException(nameof(candles));
            }

            if (microCandle is null)
            {
                throw new ArgumentNullException(nameof(microCandle));
            }

            if (candles.Count < 1)
            {
                return null;
            }

            var container = new PriceActionContainer(timeframe, microTimeFrame);
            var selectedMicro =
                microCandle.Where(c => c.SeqNum >= candles[0].SeqNum * container.TimeFrameMinutes).ToList();

            container.AddMicroCandleRange(selectedMicro, false);
            container.AddCandleRange(candles, false);

            return container;
        }

        #endregion
    }
}
