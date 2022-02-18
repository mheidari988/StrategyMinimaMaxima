using StockSharp.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCore.PriceAction.Signal
{
    public class SignalEntity
    {
        public Guid Id { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal EntryPoint { get; set; }
        public SignalPattern SignalPattern { get; set; }
        public SignalState SignalState { get; set; } = SignalState.None;
        public SignalDirection SignalDirection { get; set; }
        public PriceActionContainer? ParentContainer { get; set; }
        public PriceActionContainer? ChildContainer { get; set; }
    }
}
