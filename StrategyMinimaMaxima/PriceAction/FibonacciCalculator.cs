using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction
{
    public class FibonacciCalculator
    {
        public FibonacciCalculator(decimal firstPoint, decimal secondPoint)
        {
            decimal ds = decimal.Subtract(secondPoint, firstPoint);
            Retracement382 = decimal.Subtract(secondPoint, decimal.Multiply(ds, 0.382M));
            Retracement500 = decimal.Subtract(secondPoint, decimal.Multiply(ds, 0.5M));
            Retracement618 = decimal.Subtract(secondPoint, decimal.Multiply(ds, 0.618M));
            Retracement786 = decimal.Subtract(secondPoint, decimal.Multiply(ds, 0.786M));
            Target272 = decimal.Add(decimal.Multiply(ds, 1.272M), firstPoint);
            Target618 = decimal.Add(decimal.Multiply(ds, 1.618M), firstPoint);
        }

        public decimal Retracement382 { get; set; }
        public decimal Retracement500 { get; set; }
        public decimal Retracement618 { get; set; }
        public decimal Retracement786 { get; set; }
        public decimal Target272 { get; set; }
        public decimal Target618 { get; set; }
    }
}
