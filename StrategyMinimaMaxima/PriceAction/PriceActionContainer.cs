using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StockSharp.Algo.Candles;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceActionContainer
    {
        //private PriceActionHelper _priceActionHelper;
        long _candleSeq = 0;
        public PriceActionContainer()
        {
            //_priceActionHelper = new PriceActionHelper();
            Candles = new List<Candle>();
            ValleyCandles = new List<Candle>();
            PeakCandles = new List<Candle>();
            Elements = new Dictionary<long, PriceActionElement>();
            ChainedElements = new Dictionary<long, PriceActionElement>();
            Legs = new Dictionary<long, PriceActionLeg>();
            Swings = new Dictionary<long, PriceActionSwing>();
        }

        public void AddCandle(Candle candle, bool autoSeqNum = true)
        {
            if (ProcessLimit == 0)
            {
                if (autoSeqNum)
                    candle.SeqNum = _candleSeq++;
                Candles.Add(candle);
                processPeaksAndValleys();
                processChainedElements();
                processLegs();
                processSwings();
            }
            else
            {
                if (_candleSeq < ProcessLimit)
                {
                    if (autoSeqNum)
                        candle.SeqNum = _candleSeq++;
                    Candles.Add(candle);
                    processPeaksAndValleys();
                    processChainedElements();
                    processLegs();
                    processSwings();
                }
            }
        }

        #region Private Methods

        private void processPeaksAndValleys()
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
                if (Candles[i - 1].LowPrice > Candles[i].LowPrice && Candles[i].LowPrice < Candles[i + 1].LowPrice)
                {
                    PriceActionElement min = new PriceActionElement(Candles[i])
                    {
                        PeakValleyType = PeakValleyType.Valley
                    };
                    Elements.Add(_indexCounter++, min);

                    ValleyCandles.Add(Candles[i]);
                }
                if (Candles[i - 1].HighPrice < Candles[i].HighPrice && Candles[i].HighPrice > Candles[i + 1].HighPrice)
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
            if (Candles[0].OpenPrice < Candles[0].ClosePrice)
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
            else
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

        private void processChainedElements()
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
                    if (Elements[i].PeakValleyType == PeakValleyType.Valley && Elements[i - 1].PeakValleyType == PeakValleyType.Valley)
                    {
                        //----------------------If current element's LowPrice is lower than----------------
                        //----------------------previous element's LowPrice--------------------------------
                        if (Elements[i].Candle.LowPrice > Elements[i - 1].Candle.LowPrice)
                        {
                            continue;
                        }
                        else if (Elements[i].Candle.LowPrice < Elements[i - 1].Candle.LowPrice)
                        {
                            ChainedElements.Remove(_chainIndex - 1);
                            ChainedElements.Add(_chainIndex - 1, Elements[i]);
                        }
                    }
                    else if (Elements[i].PeakValleyType == PeakValleyType.Peak && Elements[i - 1].PeakValleyType == PeakValleyType.Peak)
                    {
                        //----------------------If current element's HighPrice is higher than----------------
                        //----------------------previous element's HighPrice: We ----------------------------------
                        if (Elements[i].Candle.HighPrice < Elements[i - 1].Candle.HighPrice)
                        {
                            continue;
                        }
                        if (Elements[i].Candle.HighPrice > Elements[i - 1].Candle.HighPrice)
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

        private void processLegs()
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

        private void processSwings()
        {
            Swings.Clear();

            long _swingIndex = 0;

            if (Legs.Count < 2) return;
            for (int i = 1; i < Legs.Count - 1; i++)
            {
                Swings.Add(_swingIndex++, new PriceActionSwing(Legs[i - 1], Legs[i], Legs[i + 1]));
            }
        }

        #endregion


        //----------------- Public methods ----------------------
        public static PriceActionContainer GenerateContainer(List<Candle> candles, bool resetSeqNum = false)
        {
            if (candles is null)
            {
                throw new ArgumentNullException(nameof(candles));
            }

            var container = new PriceActionContainer();
            foreach (var item in candles)
            {
                container.AddCandle(item, false); // false means: use old SeqNum
            }
            return container;
        }

        public List<Candle>? GetLastSwingsCandles()
        {
            if (Swings == null) return null;
            if (Swings.Count < 1) return null;
            var _lastSwing = Swings[Swings.Count - 1];
            var _beginCandleSeqNum = _lastSwing.Leg1.BeginElement.Candle.SeqNum;
            var _endCandleSeqNum = _lastSwing.Leg3.EndElement.Candle.SeqNum;
            return Candles.FindAll(s => s.SeqNum >= _beginCandleSeqNum && s.SeqNum <= _endCandleSeqNum);
        }

        //----------------- Public Properties ----------------------

        #region Public Properties

        public long ProcessLimit { get; set; } = 0;
        public Dictionary<long, PriceActionElement> Elements { get; private set; }

        public Dictionary<long, PriceActionElement> ChainedElements { get; private set; }

        public Dictionary<long, PriceActionLeg> Legs { get; private set; }

        public Dictionary<long, PriceActionSwing> Swings { get; private set; }

        public List<Candle> Candles { get; private set; }
        public List<Candle> ValleyCandles { get; private set; }
        public List<Candle> PeakCandles { get; private set; }

        #endregion
    }
}
    //public class PriceActionContainer
    //{
    //    long _candleSeq = 0;

    //    public PriceActionContainer()
    //    {
    //        Candles = new List<TimeFrameCandle>();
    //        Elements = new Dictionary<long, PriceActionElement>();
    //        ChainedElements = new Dictionary<long, PriceActionElement>();
    //        Legs = new Dictionary<long, PriceActionLeg>();
    //        Swings = new Dictionary<long, PriceActionSwing>();
    //    }

    //    public void PushCandle(Candle candle)
    //    {
    //        if (ProcessLimit == 0)
    //        {
    //            beginProcess(candle);
    //        }
    //        else
    //        {
    //            if (_candleSeq < ProcessLimit)
    //            {
    //                beginProcess(candle);
    //            }
    //        }
    //    }

    //    public static PriceActionContainer GenerateContainer(List<TimeFrameCandle> candles,bool resetSeqNum=false)
    //    {
    //        if (candles is null)
    //        {
    //            throw new ArgumentNullException(nameof(candles));
    //        }

    //        var container = new PriceActionContainer();
    //        foreach (var item in candles)
    //        {
    //            container.PushCandle(item);
    //        }
    //        return container;
    //    }

    //    #region Private Methods
    //    private void beginProcess(Candle candle)
    //    {
    //        candle.SeqNum = _candleSeq++;
    //        Candles.Add((TimeFrameCandle)candle);
    //        processPeaksAndValleys();
    //        processChainedElements();
    //        processLegs();
    //        processSwings();
    //    }
    //    private void processPeaksAndValleys()
    //    {
    //        Elements.Clear();

    //        var _indexCounter = 0;

    //        if (Candles.Count < 3) return;

    //        //------------------If candle is bullish then add minima first------------------
    //        if (Candles[0].OpenPrice < Candles[0].ClosePrice)
    //        {
    //            if (Candles[0].LowPrice < Candles[1].LowPrice)
    //            {
    //                PriceActionElement _min = new PriceActionElement(Candles[0])
    //                {
    //                    PeakValleyType = PeakValleyType.Valley
    //                };
    //                Elements.Add(_indexCounter++, _min);
    //            }
    //            if (Candles[0].HighPrice > Candles[1].HighPrice)
    //            {
    //                PriceActionElement _max = new PriceActionElement(Candles[0])
    //                {
    //                    PeakValleyType = PeakValleyType.Peak
    //                };
    //                Elements.Add(_indexCounter++, _max);
    //            }
    //        }
    //        //------------------otherwise add maxima------------------
    //        else
    //        {
    //            if (Candles[0].HighPrice > Candles[1].HighPrice)
    //            {
    //                PriceActionElement _max = new PriceActionElement(Candles[0])
    //                {
    //                    PeakValleyType = PeakValleyType.Peak
    //                };
    //                Elements.Add(_indexCounter++, _max);
    //            }
    //            if (Candles[0].LowPrice < Candles[1].LowPrice)
    //            {
    //                PriceActionElement min = new PriceActionElement(Candles[0])
    //                {
    //                    PeakValleyType = PeakValleyType.Valley
    //                };
    //                Elements.Add(_indexCounter++, min);
    //            }
    //        }

    //        //------------------Iterate over other Candles to find minima and maxima------------------
    //        for (int i = 1; i < Candles.Count - 1; i++)
    //        {
    //            if (Candles[i - 1].LowPrice > Candles[i].LowPrice && Candles[i].LowPrice < Candles[i + 1].LowPrice)
    //            {
    //                PriceActionElement min = new PriceActionElement(Candles[i])
    //                {
    //                    PeakValleyType = PeakValleyType.Valley
    //                };
    //                Elements.Add(_indexCounter++, min);
    //            }
    //            if (Candles[i - 1].HighPrice < Candles[i].HighPrice && Candles[i].HighPrice > Candles[i + 1].HighPrice)
    //            {
    //                PriceActionElement _max = new PriceActionElement(Candles[i])
    //                {
    //                    PeakValleyType = PeakValleyType.Peak
    //                };
    //                Elements.Add(_indexCounter++, _max);
    //            }
    //        }

    //        //------------------If last candle is bullish then add minima first------------------
    //        if (Candles[Candles.Count - 1].OpenPrice < Candles[Candles.Count - 1].ClosePrice)
    //        {
    //            if (Candles[Candles.Count - 1].LowPrice < Candles[Candles.Count - 2].LowPrice)
    //            {
    //                PriceActionElement min = new PriceActionElement(Candles[Candles.Count - 1])
    //                {
    //                    PeakValleyType = PeakValleyType.Valley
    //                };
    //                Elements.Add(_indexCounter++, min);
    //            }
    //            if (Candles[Candles.Count - 1].HighPrice > Candles[Candles.Count - 2].HighPrice)
    //            {
    //                PriceActionElement _max = new PriceActionElement(Candles[Candles.Count - 1])
    //                {
    //                    PeakValleyType = PeakValleyType.Peak
    //                };
    //                Elements.Add(_indexCounter++, _max);
    //            }
    //        }
    //        //------------------otherwise add maxima first------------------
    //        else
    //        {
    //            if (Candles[Candles.Count - 1].HighPrice > Candles[Candles.Count - 2].HighPrice)
    //            {
    //                PriceActionElement _max = new PriceActionElement(Candles[Candles.Count - 1])
    //                {
    //                    PeakValleyType = PeakValleyType.Peak
    //                };
    //                Elements.Add(_indexCounter++, _max);
    //            }
    //            if (Candles[Candles.Count - 1].LowPrice < Candles[Candles.Count - 2].LowPrice)
    //            {
    //                PriceActionElement min = new PriceActionElement(Candles[Candles.Count - 1])
    //                {
    //                    PeakValleyType = PeakValleyType.Valley
    //                };
    //                Elements.Add(_indexCounter++, min);
    //            }
    //        }
    //    }

    //    private void processChainedElements()
    //    {
    //        if (Elements == null)
    //            throw new NullReferenceException("SwingElements cannot be null.");

    //        if (Elements.Count == 0) return;

    //        long _chainIndex = 0;
    //        ChainedElements.Clear();

    //        ChainedElements.Add(_chainIndex++, Elements[0]);

    //        if (Elements.Count > 1)
    //        {
    //            for (int i = 1; i < Elements.Count; i++)
    //            {
    //                if (Elements[i].PeakValleyType == PeakValleyType.Valley && Elements[i - 1].PeakValleyType == PeakValleyType.Valley)
    //                {
    //                    //----------------------If current element's LowPrice is lower than----------------
    //                    //----------------------previous element's LowPrice--------------------------------
    //                    if (Elements[i].Candle.LowPrice > Elements[i - 1].Candle.LowPrice)
    //                    {
    //                        continue;
    //                    }
    //                    else if (Elements[i].Candle.LowPrice < Elements[i - 1].Candle.LowPrice)
    //                    {
    //                        ChainedElements.Remove(_chainIndex - 1);
    //                        ChainedElements.Add(_chainIndex - 1, Elements[i]);
    //                    }
    //                }
    //                else if (Elements[i].PeakValleyType == PeakValleyType.Peak && Elements[i - 1].PeakValleyType == PeakValleyType.Peak)
    //                {
    //                    //----------------------If current element's HighPrice is higher than----------------
    //                    //----------------------previous element's HighPrice: We ----------------------------------
    //                    if (Elements[i].Candle.HighPrice < Elements[i - 1].Candle.HighPrice)
    //                    {
    //                        continue;
    //                    }
    //                    if (Elements[i].Candle.HighPrice > Elements[i - 1].Candle.HighPrice)
    //                    {
    //                        ChainedElements.Remove(_chainIndex - 1);
    //                        ChainedElements.Add(_chainIndex - 1, Elements[i]);
    //                    }
    //                }
    //                else
    //                {
    //                    //------------------Simply add the element to the ChainedElements-------------
    //                    ChainedElements.Add(_chainIndex++, Elements[i]);
    //                }
    //            }
    //        }
    //    }

    //    private void processLegs()
    //    {
    //        if (ChainedElements == null)
    //            throw new NullReferenceException("ChainedSwingElements cannot be null.");

    //        Legs.Clear();

    //        long _legIndex = 0;
    //        if (ChainedElements.Count >= 2)
    //        {
    //            for (int i = 1; i < ChainedElements.Count; i++)
    //            {
    //                Legs.Add(_legIndex++, new PriceActionLeg(ChainedElements[i - 1], ChainedElements[i]));
    //            }
    //        }
    //    }

    //    private void processSwings()
    //    {
    //        Swings.Clear();

    //        long _swingIndex = 0;

    //        if (Legs.Count < 2) return;
    //        for (int i = 1; i < Legs.Count - 1; i++)
    //        {
    //            Swings.Add(_swingIndex++, new PriceActionSwing(Legs[i - 1], Legs[i], Legs[i + 1]));
    //        }
    //    }

    //    #endregion

    //    //----------------- Public Properties ----------------------

    //    #region Public Properties

    //    public long ProcessLimit { get; set; } = 0;
    //    public Dictionary<long,PriceActionElement> Elements { get; private set; }
    //    public Dictionary<long, PriceActionElement> ChainedElements { get; private set; }
    //    public Dictionary<long,PriceActionLeg> Legs { get; private set; }
    //    public Dictionary<long, PriceActionSwing> Swings { get; private set; }
    //    public List<TimeFrameCandle> Candles { get; private set; }

    //    #endregion
    //}

