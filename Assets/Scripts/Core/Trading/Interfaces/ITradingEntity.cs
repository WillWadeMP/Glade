using System.Collections.Generic;
using Glade.Core.Tick;
using Glade.Core.Trading.Engine;

namespace Glade.Core.Trading.Interfaces
{
    /// <summary>
    /// Implemented by anything that posts buy / sell orders to an AuctionHouse.
    /// Usually attached to a Building.
    /// </summary>
    public interface ITradingEntity : ITickable
    {
        IEnumerable<Order> GetSellOrders();
        IEnumerable<Order> GetBuyOrders();
        void OnTradeExecuted(TradeResult result);
    }
}
