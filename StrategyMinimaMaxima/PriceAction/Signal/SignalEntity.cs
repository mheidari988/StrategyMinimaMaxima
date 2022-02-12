using StockSharp.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction.Signal
{
    public class SignalEntity
    {
        public SignalEntity(Security security) => Security = security ?? throw new ArgumentNullException(nameof(security));
        public Guid Id { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal EntryPrice { get; set; }
        public ParentPatternType SignalPattern { get; set; }
        public SignalDirection SignalDirection { get; set; }
        public SignalState SignalState { get; set; }
        public Security Security { get; }
    }
}
