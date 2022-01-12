using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.Algo.Candles;

namespace StrategyMinimaMaxima.PriceAction
{
    public class PriceSwingElement
    {
        private Candle candle;
        private PeakValleyMode elementPeakValleyMode = PeakValleyMode.None;
        private SwingMode elementSwingMode = SwingMode.None;
        private MomentumMode elementMomentumMode = MomentumMode.None;

        public PriceSwingElement(Candle _candle)
        {
            candle = _candle;
        }

        public MomentumMode ElementMomentumMode
        {
            get { return elementMomentumMode; }
            set { elementMomentumMode = value; }
        }

        public SwingMode ElementSwingMode
        {
            get { return elementSwingMode; }
            set { elementSwingMode = value; }
        }

        public PeakValleyMode ElementPeakValleyMode
        {
            get { return elementPeakValleyMode; }
            set { elementPeakValleyMode = value; }
        }

        public Candle Candle
        {
            get { return candle; }
            private set { candle = value; }
        }

    }
}
