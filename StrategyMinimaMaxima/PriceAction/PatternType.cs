using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction
{
    public enum PatternType
    {
        Unknown = 0,
        BullishICI = 1,
        BullishCIC = 2,
        BullishICC = 3,
        BullishCII = 4,
        BearishICI = 5,
        BearishCIC = 6,
        BearishICC = 7,
        BearishCII = 8
    }
}
