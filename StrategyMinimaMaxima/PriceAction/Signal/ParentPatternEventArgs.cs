using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction.Signal
{
    public class ParentPatternEventArgs : EventArgs
    {
        public ParentPatternType ParentPatternType { get; set; } = ParentPatternType.None;
        public PriceActionContainer? ParentContainer { get; set; }
        public decimal ImpulseSlope { get; init; }
    }
}
