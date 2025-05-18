using Glade.Core.Trading.Data;
using Glade.Core.Trading.Interfaces;

namespace Glade.Core.Trading.Engine
{
    /// <summary>
    /// Represents a trade order (buy or sell) for a certain resource.
    /// Quantity is positive for buy orders, negative for sell orders.
    /// </summary>
    public class Order
    {
        public readonly Resource item;         // the resource being traded
        public readonly Resource currency;     // the currency resource used for price
        public int quantity;                   // positive = buying qty, negative = selling qty
        public float unitPrice;                // price per unit in terms of currency
        public readonly ITradingEntity trader; // the entity placing this order

        public bool IsBuy => quantity > 0;

        public Order(Resource item, Resource currency, int qty, float price, ITradingEntity trader)
        {
            this.item = item;
            this.currency = currency;
            this.quantity = qty;
            this.unitPrice = price;
            this.trader = trader;
        }
    }

    /// <summary>
    /// Represents the result of a matched trade between a buy and sell order.
    /// </summary>
    public class TradeResult
    {
        public Order ask;   // the sell order
        public Order bid;   // the buy order
        public int matchedQty;
        public float price => ask.unitPrice;  // use ask price as trade price (assuming match)
        public TradeResult(Order ask, Order bid, int qty)
        {
            this.ask = ask;
            this.bid = bid;
            this.matchedQty = qty;
        }
    }
}
