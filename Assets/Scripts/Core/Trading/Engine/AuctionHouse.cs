using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Data;
using Glade.Core.Trading.Interfaces;

namespace Glade.Core.Trading.Engine
{
    /// <summary>Collects orders from registered traders and settles them each tick.</summary>
    public class AuctionHouse : MonoBehaviour, ITickable
    {
        private readonly Dictionary<(Resource,Resource), OrderBook> _books = new();
        private readonly List<ITradingEntity> _traders = new();

        public void RegisterTrader(ITradingEntity t) => _traders.Add(t);
        public void UnregisterTrader(ITradingEntity t) => _traders.Remove(t);

        public void Tick(float dt)
        {
            _books.Clear();

            // Gather orders ---------------------------------------------------
            foreach (var t in _traders)
            {
                foreach (var o in t.GetSellOrders()) GetBook(o).AddAsk(o);
                foreach (var o in t.GetBuyOrders())  GetBook(o).AddBid(o);
            }

            // Settle each book -----------------------------------------------
            foreach (var book in _books.Values)
            {
                foreach (var tr in book.Clear())
                {
                    tr.ask.trader.OnTradeExecuted(tr);
                    tr.bid.trader.OnTradeExecuted(tr);
                }
            }
        }

        private OrderBook GetBook(Order o)
        {
            var key = (o.item, o.currency);
            if (!_books.TryGetValue(key, out var book))
                _books[key] = book = new OrderBook();
            return book;
        }
    }
}
