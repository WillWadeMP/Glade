using Glade.Core.Trading.Data;
using Glade.Core.Trading.Interfaces;


namespace Glade.Core.Trading.Engine
{
    public class Order
    {
        public readonly Resource item;
        public readonly Resource currency;
        public int quantity;
        public float unitPrice;
        public readonly ITradingEntity trader;

        public bool IsBuy => quantity > 0;   // sell orders pass negative qty

        public Order(Resource item, Resource currency, int qty, float price, ITradingEntity trader)
        {
            this.item = item;
            this.currency = currency;
            this.quantity = qty;
            this.unitPrice = price;
            this.trader = trader;
        }
    }
}
