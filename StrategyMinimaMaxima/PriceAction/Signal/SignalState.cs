namespace StrategyMinimaMaxima.PriceAction.Signal
{
    public enum SignalState
    {
        Ready = 0,
        EntryHitted = 1,
        StopHitted = 2,
        RiskFreeHitted = 3,
        BreakEvenHitted = 4,
        TakeProfitHitted = 5
    }
}
