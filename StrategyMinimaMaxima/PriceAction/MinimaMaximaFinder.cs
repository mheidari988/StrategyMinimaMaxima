using System.Collections.Generic;
using System.IO;
using System.Text;
using StockSharp.Algo.Candles;

namespace StrategyMinimaMaxima.PriceAction
{
    public class MinimaMaximaFinder
    {
        private int minimumToProcess = 3;
        private Dictionary<long,PriceSwingElement> swingElements = new Dictionary<long, PriceSwingElement>();
        private List<Candle> _minimas = new List<Candle>();
        private List<Candle> _maximas = new List<Candle>();
        private List<Candle> candles = new List<Candle>();
        long candleSeq = -1;
        public void AddCandle(Candle candle)
        {
            candle.SeqNum = ++candleSeq;
            candles.Add(candle);
            processMinimaMaxima();
        }

        public void WriteLog()
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in _maximas) str.AppendLine($"SeqNo {item.SeqNum} -- Maxima: {item.HighPrice}");
            str.AppendLine("---");
            foreach (var item in _minimas) str.AppendLine($"SeqNo {item.SeqNum} -- Minima: {item.LowPrice}");
            File.WriteAllText("_log.txt", str.ToString());
        }

        public void WriteDicLog()
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in swingElements)
            {
                if (item.Value.ElementPeakValleyMode == PeakValleyMode.Peak)
                    str.AppendLine($"Index: {item.Key} -- SeqNo {item.Value.Candle.SeqNum} -- Type: Peak -- Value: {item.Value.Candle.HighPrice}");
                else if (item.Value.ElementPeakValleyMode == PeakValleyMode.Valley)
                    str.AppendLine($"Index: {item.Key} -- SeqNo {item.Value.Candle.SeqNum} -- Type: Valley -- Value: {item.Value.Candle.LowPrice}");
            }
            str.AppendLine("---");
            File.WriteAllText("_dlog.txt", str.ToString());
        }

        private void processMinimaMaxima()
        {
            _minimas.Clear();
            _maximas.Clear();
            swingElements.Clear();

            var _indexCounter = 0;

            if (candles.Count < MinimumToProcess) return;

            //------------------If candle is bullish then add minima first------------------
            if (candles[0].OpenPrice < candles[0].ClosePrice)
            {
                if (candles[0].LowPrice < candles[1].LowPrice)
                {
                    PriceSwingElement _min = new PriceSwingElement(candles[0])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Valley
                    };
                    swingElements.Add(_indexCounter++, _min);

                    _minimas.Add(candles[0]);
                }
                if (candles[0].HighPrice > candles[1].HighPrice)
                {
                    PriceSwingElement _max = new PriceSwingElement(candles[0])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Peak
                    };
                    swingElements.Add(_indexCounter++, _max);

                    _maximas.Add(candles[0]);
                }
            }
            else
            {
                if (candles[0].HighPrice > candles[1].HighPrice)
                {
                    PriceSwingElement _max = new PriceSwingElement(candles[0])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Peak
                    };
                    swingElements.Add(_indexCounter++, _max);

                    _maximas.Add(candles[0]);
                }
                if (candles[0].LowPrice < candles[1].LowPrice)
                {
                    PriceSwingElement min = new PriceSwingElement(candles[0])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Valley
                    };
                    swingElements.Add(_indexCounter++, min);

                    _minimas.Add(candles[0]);
                }
            }

            //------------------Iterate over other candles to find minima and maxima------------------
            for (int i = 1; i < candles.Count - 1; i++)
            {
                if (candles[i - 1].LowPrice > candles[i].LowPrice && candles[i].LowPrice < candles[i + 1].LowPrice)
                {
                    PriceSwingElement min = new PriceSwingElement(candles[i])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Valley
                    };
                    swingElements.Add(_indexCounter++, min);

                    _minimas.Add(candles[i]);
                }
                else if (candles[i - 1].HighPrice < candles[i].HighPrice && candles[i].HighPrice > candles[i + 1].HighPrice)
                {
                    PriceSwingElement _max = new PriceSwingElement(candles[i])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Peak
                    };
                    swingElements.Add(_indexCounter++, _max);

                    _maximas.Add(candles[i]);
                }
            }

            //------------------If last candle is bullish then add minima last------------------
            if (candles[0].OpenPrice < candles[0].ClosePrice)
            {
                if (candles[candles.Count - 1].LowPrice < candles[candles.Count - 2].LowPrice)
                {
                    PriceSwingElement min = new PriceSwingElement(candles[candles.Count - 1])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Valley
                    };
                    swingElements.Add(_indexCounter++, min);

                    _minimas.Add(candles[candles.Count - 1]);
                }
                if (candles[candles.Count - 1].HighPrice > candles[candles.Count - 2].HighPrice)
                {
                    PriceSwingElement _max = new PriceSwingElement(candles[candles.Count - 1])
                    {
                        ElementPeakValleyMode = PeakValleyMode.Peak
                    };
                    swingElements.Add(_indexCounter++, _max);

                    _maximas.Add(candles[candles.Count - 1]);
                }
            }
        }

        public int MinimumToProcess
        {
            get { return minimumToProcess; }
            set 
            {
                if (value >= 3)
                    minimumToProcess = value;
            }
        }
        public List<Candle> Candles { get => candles; }
        public List<Candle> Minimas { get => _minimas; }
        public List<Candle> Maximas { get => _maximas; }
    }
}
