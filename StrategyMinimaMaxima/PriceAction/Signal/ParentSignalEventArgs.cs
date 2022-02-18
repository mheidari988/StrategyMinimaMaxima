using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCore.PriceAction.Signal
{
    public class ParentSignalEventArgs : EventArgs
    {
        public SignalPattern ParentPatternType { get; set; } = SignalPattern.None;
        public PriceActionContainer? ParentContainer { get; set; }
        public decimal ImpulseSlope { get; init; }
    }
}
