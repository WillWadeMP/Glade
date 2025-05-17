namespace Glade.Core.Trading.Engine
{
    public readonly struct TradeResult
    {
        public readonly Order ask;
        public readonly Order bid;
        public readonly int matchedQty;
        public readonly float settledUnitPrice;

        public TradeResult(Order ask, Order bid, int qty, float price)
        {
            this.ask = ask;
            this.bid = bid;
            this.matchedQty = qty;
            this.settledUnitPrice = price;
        }
    }
}
