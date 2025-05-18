using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Trading.Data;
using Glade.Core.Trading.Interfaces;
using Glade.Core.Tick;

namespace Glade.Core.Trading.Engine
{
    /// <summary>
    /// Global market that collects buy/sell orders from all traders each tick and matches them.
    /// Uses a simple auction mechanism to clear trades.
    /// </summary>
    public class AuctionHouse : MonoBehaviour, ITickable
    {
        // Order books keyed by (item, currency) pair
        private readonly Dictionary<(Resource, Resource), OrderBook> books = new();
        private readonly List<ITradingEntity> traders = new();

        public void RegisterTrader(ITradingEntity trader) => traders.Add(trader);
        public void UnregisterTrader(ITradingEntity trader) => traders.Remove(trader);

        /// <summary>Called each tick to gather all orders and execute matches.</summary>
        public void Tick(float dt)
        {
            books.Clear();
            // 1. Collect all orders from traders
            foreach (var trader in traders)
            {
                foreach (Order sellOrder in trader.GetSellOrders()) AddAsk(sellOrder);
                foreach (Order buyOrder  in trader.GetBuyOrders())  AddBid(buyOrder);
            }
            // 2. Match orders in each book
            foreach (OrderBook book in books.Values)
            {
                foreach (TradeResult trade in book.ClearTrades())
                {
                    // Notify traders of executed trade
                    trade.ask.trader.OnTradeExecuted(trade);
                    trade.bid.trader.OnTradeExecuted(trade);
                }
            }
        }

        private void AddAsk(Order order)
        {
            var key = (order.item, order.currency);
            if (!books.TryGetValue(key, out var book))
            {
                book = new OrderBook();
                books[key] = book;
            }
            book.AddAsk(order);
        }

        private void AddBid(Order order)
        {
            var key = (order.item, order.currency);
            if (!books.TryGetValue(key, out var book))
            {
                book = new OrderBook();
                books[key] = book;
            }
            book.AddBid(order);
        }

        /// <summary>
        /// Simple order book collecting asks and bids and matching them.
        /// (For brevity, matching is simplified: it matches all possible trades at ask price.)
        /// </summary>
        private class OrderBook
        {
            private readonly List<Order> asks = new();
            private readonly List<Order> bids = new();
            public void AddAsk(Order ask) => asks.Add(ask);
            public void AddBid(Order bid) => bids.Add(bid);
            public IEnumerable<TradeResult> ClearTrades()
            {
                // Sort asks by price ascending and bids by price descending for matching
                asks.Sort((a, b) => a.unitPrice.CompareTo(b.unitPrice));
                bids.Sort((a, b) => b.unitPrice.CompareTo(a.unitPrice));
                var results = new List<TradeResult>();
                int i = 0, j = 0;
                while (i < asks.Count && j < bids.Count)
                {
                    Order ask = asks[i];
                    Order bid = bids[j];
                    if (ask.unitPrice <= bid.unitPrice)
                    {
                        // Trade occurs at ask.unitPrice
                        int tradedQty = Mathf.Min(-ask.quantity, bid.quantity);
                        // Adjust quantities
                        ask.quantity += tradedQty;    // ask.quantity is negative (selling)
                        bid.quantity -= tradedQty;
                        // Record trade result
                        results.Add(new TradeResult(ask, bid, tradedQty));
                        // Remove fulfilled orders
                        if (ask.quantity == 0) i++;
                        if (bid.quantity == 0) j++;
                    }
                    else
                    {
                        // Highest bid is lower than lowest ask, no further matches
                        break;
                    }
                }
                return results;
            }
        }
    }
}
