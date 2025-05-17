using UnityEngine;
using Glade.Core.Buildings.Data;
using Glade.Core.Tick;
using Glade.Core.Trading.Engine;
using Glade.Core.Trading.Interfaces;
using Glade.Core.Population;
using System.Collections.Generic;

namespace Glade.Core.Buildings
{
    /// <summary>Permeable tile that speeds movement if well maintained.</summary>
    public class Road : Building, ITradingEntity
    {
        [SerializeField] private BuildingLevel level;

        private float maintenanceBuffer;     // units on hand (days)
        private const float dayLength = 24f;
        private float timer;

        public override void Tick(float dt)
        {
            timer += dt;
            if (timer < dayLength) return;
            timer -= dayLength;

            maintenanceBuffer -= level.maintenancePerDay;
            moveCost = maintenanceBuffer >= 0 ? level.moveCost : level.moveCost * 4;
        }

        /* ---------- ITradingEntity ------------- */
        public IEnumerable<Order> GetSellOrders() { yield break; }

        public IEnumerable<Order> GetBuyOrders()
        {
            if (maintenanceBuffer < 2f && level.maintenanceResource != null)
            {
                int qty = Mathf.CeilToInt((2f - maintenanceBuffer) * level.maintenancePerDay);
                yield return new Order(level.maintenanceResource, CurrencyManager.Default, qty,
                                       level.maintenanceResource.baseValue, this);
            }
        }

        public void OnTradeExecuted(TradeResult tr)
        {
            if (tr.bid.trader == this && tr.bid.item == level.maintenanceResource)
                maintenanceBuffer += tr.matchedQty / level.maintenancePerDay;
        }

        /* helper for visual variant based on neighbours (N/E/S/W + X) */
        public void RefreshSprite(int bitmask) => 
            GetComponent<SpriteRenderer>().sprite = level.sprite; // replace with atlas lookup
    }
}
