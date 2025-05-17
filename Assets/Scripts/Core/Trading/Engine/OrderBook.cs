using System.Collections.Generic;
using System.Linq;
using Glade.Core.Trading.Data;
using Glade.Core.Trading.Interfaces;

namespace Glade.Core.Trading.Engine
{
    /// <summary>Order book for a single (item,currency) pair.</summary>
    internal class OrderBook
    {
        private readonly List<Order> _bids = new(); // buy  (qty > 0)  high→low
        private readonly List<Order> _asks = new(); // sell (qty < 0)  low →high

        public void AddBid(Order o) => _bids.Add(o);
        public void AddAsk(Order o) => _asks.Add(o);

        public IEnumerable<TradeResult> Clear()
        {
            _bids.Sort((a, b) => b.unitPrice.CompareTo(a.unitPrice));
            _asks.Sort((a, b) => a.unitPrice.CompareTo(b.unitPrice));

            int bi = 0, ai = 0;
            while (bi < _bids.Count && ai < _asks.Count && _bids[bi].unitPrice >= _asks[ai].unitPrice)
            {
                Order bid = _bids[bi];
                Order ask = _asks[ai];

                int qty = System.Math.Min(bid.quantity, -ask.quantity);
                float price = (bid.unitPrice + ask.unitPrice) * 0.5f;

                yield return new TradeResult(ask, bid, qty, price);

                bid.quantity -= qty;
                ask.quantity += qty;
                if (bid.quantity == 0) bi++;
                if (ask.quantity == 0) ai++;
            }

            _bids.Clear();
            _asks.Clear();
        }
    }
}
