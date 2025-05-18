using System.Linq;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Engine;
using Glade.Core.Population;
using Glade.Core.World;
using Glade.Core.Buildings;
using Glade.Core.Trading.Data;
using Glade.Core.Modding;
using Glade.Core.Trading.Interfaces;

namespace Glade.Core.GameSystems
{
    /// <summary>
    /// Initializes core systems at game start: loads data, registers entities, and ensures managers are wired up.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private TickManager tickManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private WorldGrid worldGrid;
        [SerializeField] private AuctionHouse auctionHouse;
        [SerializeField] private PopulationManager populationManager;
        [SerializeField] private ModManager modManager;
        [Tooltip("Default currency resource for the simulation (assign a Resource asset, e.g. 'coin')")]
        [SerializeField] private Resource defaultCurrency;

        private void Awake()
        {
            // 1. Load core data from Resources
            ResourceRegistry.LoadFromResourcesFolder();
            BuildingLevelRegistry.LoadFromResourcesFolder();
            PopulationRegistry.LoadFromResourcesFolder();
            // Set default currency for trading system
            if (defaultCurrency != null)
            {
                CurrencyManager.SetDefault(defaultCurrency);
            }
            else
            {
                // If not assigned, default to first loaded currency resource
                var currency = ResourceRegistry.All.FirstOrDefault(r => r.isCurrency);
                if (currency != null)
                    CurrencyManager.SetDefault(currency);
            }

            // 2. Initialize world grid occupancy for any pre-placed buildings
            foreach (Building b in FindObjectsOfType<Building>())
            {
                worldGrid.Occupy(b);
            }

            // 3. Register trading entities (traders) with AuctionHouse
            foreach (var trader in FindObjectsOfType<MonoBehaviour>().OfType<ITradingEntity>())
            {
                auctionHouse.RegisterTrader(trader);
            }

            // 4. Register citizens with PopulationManager
            foreach (Citizen c in FindObjectsOfType<Citizen>())
            {
                populationManager.Register(c);
            }

            // 5. Hook up managers to TickManager for updates
            tickManager.Register(timeManager);
            tickManager.Register(auctionHouse);
            tickManager.Register(populationManager);
            // (Buildings and Citizens register themselves in Awake; WorldGrid has no per-tick update)

            // 6. Load mods (if any)
            if (modManager != null)
            {
                modManager.LoadMods();
                // After loading mod assets, you may want to instantiate or integrate new content as needed.
            }
        }
    }
}
