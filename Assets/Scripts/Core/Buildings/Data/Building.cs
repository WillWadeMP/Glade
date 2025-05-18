using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Data;
using Glade.Core.Trading.Engine;
using Glade.Core.Trading.Interfaces;
using Glade.Core.Buildings.Data;
using Glade.Core.World;
using Glade.Core.Population;

namespace Glade.Core.Buildings
{
    /// <summary>
    /// Generalised building MonoBehaviour. All placement / maintenance /
    /// production / trading logic remains intact.  Must be initialised via
    /// <see cref="Init"/> immediately after AddComponent so that Awake never
    /// runs with null references.
    /// </summary>
    public class Building : MonoBehaviour, ITickable, ITradingEntity
    {
        //--------------- public API -----------------
        public BuildingLevel Level   => level;
        public Vector2Int    GridPos { get; private set; }

        /// <summary>Call straight after creating the component.</summary>
        public void Init(BuildingLevel lvl, Vector2Int pos)
        {
            level       = lvl;
            GridPos     = pos;
            initialised = true;

            AllBuildings.Add(this);

            // register with TickManager
            FindObjectOfType<TickManager>()?.Register(this);

            // if maintenance resource exists, start with 1 day buffer
            if (level.maintenanceResource != null && level.maintenancePerDay > 0)
                maintenanceBuffer = level.maintenancePerDay;

            // mark on grid
            FindObjectOfType<WorldGrid>()?.Occupy(this);
        }

        //--------------- Private data -----------------
        [SerializeField] private BuildingLevel level; // injected by Init

        private readonly Inventory      inventory         = new Inventory();
        private float                    maintenanceBuffer;
        private readonly List<Citizen>   residents         = new List<Citizen>();
        private bool                     initialised;

        public static readonly List<Building> AllBuildings = new List<Building>();

        //--------------- Unity Lifecycle -----------------
        void Awake()
        {
            // Intentionally empty: everything happens in Init()
        }

        void Start()
        {
            if (!initialised)
            {
                Debug.LogError($"Building `{name}` started without Init(). " +
                               "Spawn through TestFactory or call Init manually.");
                enabled = false;
            }
        }

        void OnDestroy()
        {
            AllBuildings.Remove(this);
            FindObjectOfType<TickManager>()?.Unregister(this);
        }

        //--------------- Tick behaviour -----------------
        public void Tick(float deltaTime)
        {
            // ----- production -----
            if (level.producesPerTick != null)
            {
                foreach (var recipe in level.producesPerTick)
                {
                    bool canProduce = true;
                    if (recipe.inputs != null)
                    {
                        foreach (var input in recipe.inputs)
                        {
                            if (!inventory.ContainsKey(input.resource) ||
                                inventory[input.resource] < input.amount)
                            {
                                canProduce = false;
                                break;
                            }
                        }
                    }
                    if (!canProduce) continue;

                    // consume inputs
                    if (recipe.inputs != null)
                    {
                        foreach (var input in recipe.inputs)
                        {
                            inventory[input.resource] -= input.amount;
                        }
                    }

                    // add outputs
                    if (!inventory.ContainsKey(recipe.output))
                        inventory[recipe.output] = 0;
                    inventory[recipe.output] += recipe.outputAmount;
                }
            }

            // ----- pure consumption (no outputs) -----
            if (level.consumesPerTick != null && (level.producesPerTick == null || level.producesPerTick.Count == 0))
            {
                foreach (var recipe in level.consumesPerTick)
                {
                    foreach (var input in recipe.inputs)
                    {
                        if (inventory.ContainsKey(input.resource))
                        {
                            inventory[input.resource] =
                                Mathf.Max(0, inventory[input.resource] - input.amount);
                        }
                    }
                }
            }
        }

        //--------------- Auction-House interface -----------------
        public IEnumerable<Order> GetSellOrders()
        {
            var coin = CurrencyManager.Default;
            foreach (var kv in inventory)
            {
                if (kv.Key.isCurrency || kv.Value <= 0) continue;
                yield return new Order(kv.Key, coin, -kv.Value, kv.Key.baseValue, this);
            }
        }

        public IEnumerable<Order> GetBuyOrders()
        {
            var coin = CurrencyManager.Default;
            if (level.consumesPerTick != null)
            {
                foreach (var recipe in level.consumesPerTick)
                {
                    foreach (var input in recipe.inputs)
                    {
                        int have = inventory.ContainsKey(input.resource) ? inventory[input.resource] : 0;
                        if (have < input.amount)
                        {
                            int needed = input.amount - have;
                            yield return new Order(input.resource, coin, needed,
                                                   input.resource.baseValue, this);
                        }
                    }
                }
            }
        }

        public void OnTradeExecuted(TradeResult trade)
        {
            var item = trade.ask.item;
            int qty  = trade.matchedQty;

            // bought
            if (trade.bid.trader == this)
            {
                if (!inventory.ContainsKey(item)) inventory[item] = 0;
                inventory[item] += qty;
            }

            // sold
            if (trade.ask.trader == this)
            {
                if (inventory.ContainsKey(item))
                {
                    inventory[item] = Mathf.Max(0, inventory[item] - qty);
                }
            }
        }

        //--------------- Maintenance -----------------
        public bool HasMaintenance()
            => level.maintenanceResource != null && level.maintenancePerDay > 0;

        public void PerformMaintenance(float neededCoinSupplied)
        {
            if (!HasMaintenance()) return;

            maintenanceBuffer += neededCoinSupplied / Mathf.Max(level.maintenancePerDay, 0.0001f);
            maintenanceBuffer -= 1f; // consumed today

            // Debug stats
            Debug.Log("Building: " + name + " has received " +
                      $"{neededCoinSupplied} maintenance, buffer is now {maintenanceBuffer}");

            if (maintenanceBuffer < 0f)
            {
                if (level.isPermeable)
                {
                    level.moveCost = level.moveCost * 4;
                    FindObjectOfType<WorldGrid>()?.Occupy(this);
                }
            }
            else
            {
                if (level.isPermeable)
                {
                    level.moveCost = Mathf.Max(level.moveCost / 4, 1);
                    FindObjectOfType<WorldGrid>()?.Occupy(this);
                }
            }

            maintenanceBuffer = Mathf.Clamp(maintenanceBuffer, -2f, 2f);
        }

        //--------------- Utility used by Citizens -----------------
        public bool ProducesResource(Resource r)
            => level.producesPerTick?.Exists(rec => rec.output == r) ?? false;

        public int GetInventoryAmount(Resource r)
            => inventory.ContainsKey(r) ? inventory[r] : 0;

        public void SellResource(Resource r, int amount /* negative adds */)
        {
            if (!inventory.ContainsKey(r)) inventory[r] = 0;
            inventory[r] = Mathf.Max(0, inventory[r] - amount);
        }

        /// <summary>
        /// Returns true if this building may act as a supplier of the given resource:
        /// either it produces it each tick, or it is flagged as a shop with stock on hand.
        /// </summary>
        public bool IsSupplierOf(Resource r)
        {
            if (ProducesResource(r)) return true;
            if (level.isShop && GetInventoryAmount(r) > 0) return true;
            return false;
        }

        //--------------- Housing -----------------
        public int CurrentOccupancy => residents.Count;
        public void AddResident(Citizen c)
        {
            if (level.capacity > 0 && residents.Count < level.capacity)
                residents.Add(c);
        }
        public void RemoveResident(Citizen c) => residents.Remove(c);
    }
}
