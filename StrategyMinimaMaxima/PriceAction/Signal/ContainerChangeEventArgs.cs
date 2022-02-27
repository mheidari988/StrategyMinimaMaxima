using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCore.PriceAction.Signal
{
    public class ContainerChangeEventArgs : EventArgs
    {
        public ContainerChangeEventArgs(PriceActionContainer ChildContainer, PriceActionContainer ParentContainer)
        {
            this.ParentContainer = ParentContainer;
            this.ChildContainer = ChildContainer;
        }
        public PriceActionContainer ChildContainer { get; set; }
        public PriceActionContainer ParentContainer { get; set; }
    }
}
