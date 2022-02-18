namespace TradeCore.PriceAction.Signal
{
    public enum SignalState
    {
        None = 0,
        Ready = 1,
        EntryHitted = 2,
        StopHitted = 3,
        RiskFreeHitted = 4,
        BreakEvenHitted = 5,
        TakeProfitHitted = 6,
        MissedOut = 7
    }
}
