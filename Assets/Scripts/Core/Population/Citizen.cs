using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Data;
using Glade.Core.Trading.Engine;
using Glade.Core.Trading.Interfaces;
using Glade.Core.Buildings;

namespace Glade.Core.Population
{
    public class Citizen : MonoBehaviour, ITickable, ITradingEntity
    {
        public PopulationType type;
        public Building home;
        public Building workplace;           // single job (simplified)

        public float happiness = 50f;        // 0-100
        private readonly Dictionary<Resource, NeedStatus> needs = new();
        private const float dayLength = 24f; // hours per in-game day

        private void Start()
        {
            foreach (var nd in type.needs)
                needs[nd.resource] = new NeedStatus { def = nd, buffer = 1f };
        }

        /* ---------------- ITickable -------------- */
        public void Tick(float dt)
        {
            foreach (var ns in needs.Values)
            {
                ns.timer += dt;
                if (ns.timer < dayLength) continue;
                ns.timer -= dayLength;

                ns.buffer -= ns.def.perDay;
                if (ns.buffer >= 0)
                    happiness += ns.def.tier == NeedTier.Want
                        ?  ns.def.happinessImpact       // luxury bonus
                        :  0;
                else
                    happiness -= ns.def.happinessImpact;

                ns.buffer = Mathf.Max(ns.buffer, -2f);   // clamp starvation
            }

            happiness = Mathf.Clamp(happiness, 0, 100);
        }

        /* ----------- ITradingEntity -------------- */
        public IEnumerable<Order> GetSellOrders() { yield break; }

        public IEnumerable<Order> GetBuyOrders()
        {
            foreach (var ns in needs.Values)
            {
                // Needs first, then expectations, then wants
                bool priority = ns.def.tier != NeedTier.Want || happiness > 70;
                if (!priority) continue;

                float desiredDays = 2f;
                if (ns.buffer < desiredDays)
                {
                    int qty = Mathf.CeilToInt((desiredDays - ns.buffer) * ns.def.perDay);
                    yield return new Order(ns.def.resource, Currency(), qty, ns.def.resource.baseValue, this);
                }
            }
        }

        public void OnTradeExecuted(TradeResult tr)
        {
            if ((object)tr.bid.trader != this) return;
            needs[tr.bid.item].buffer += tr.matchedQty / needs[tr.bid.item].def.perDay;
        }

        private Resource Currency() => CurrencyManager.CurrencyFor(type);
    }
}
