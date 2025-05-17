using System.Linq;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Engine;
using Glade.Core.Trading.Interfaces;
using Glade.Core.Population;
using Glade.Core.World;
using Glade.Core.Buildings;
using Glade.Core.Trading.Data;

namespace Glade.Core.GameSystems
{
    /// <summary>
    /// One-stop initialiser: wires managers together and seeds registries.
    /// Place this on an empty GameObject in your starting scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene-level singletons")]
        [SerializeField] private TickManager      tickManager;
        [SerializeField] private AuctionHouse     auctionHouse;
        [SerializeField] private PopulationManager populationManager;
        [SerializeField] private WorldGrid        worldGrid;

        [Header("Initial currency resource (drag asset)")]
        [SerializeField] private Resource defaultCurrency;

        void Awake()
        {
            // Load core data -------------------------------------------------
            ResourceRegistry.LoadFromResourcesFolder();
            CurrencyManager.SetDefault(defaultCurrency);

            // Register traders & citizens -----------------------------------
            foreach (var t in FindObjectsOfType<MonoBehaviour>(true).OfType<ITradingEntity>())
                auctionHouse.RegisterTrader(t);

            foreach (var c in FindObjectsOfType<Citizen>(true))
                populationManager.Register(c);

            // Mark buildings on grid ---------------------------------------
            foreach (var b in FindObjectsOfType<Building>(true))
                worldGrid.Occupy(b);

            // Hook managers into TickManager -------------------------------
            tickManager.Register(auctionHouse);
            tickManager.Register(populationManager);
            tickManager.Register(worldGrid.GetComponent<ITickable>()); // if any
        }
    }
}
