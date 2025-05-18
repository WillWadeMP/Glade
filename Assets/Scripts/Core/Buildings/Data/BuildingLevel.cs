using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Trading.Data;
using Glade.Core.Population;

namespace Glade.Core.Buildings.Data
{
    /// <summary>
    /// Defines a building's properties for a specific level/stage (data-driven building template).
    /// </summary>
    [CreateAssetMenu(menuName = "Glade/Building Level")]
    public class BuildingLevel : ScriptableObject
    {
        [Header("Identity & Placement")]
        public string   id;                   // unique identifier (e.g. "road_lvl1", "house_lvl1")
        public Sprite   sprite;
        public int      moveCost = 100;       // base movement cost for this tile (lower = easier to traverse)
        public bool     isPermeable = false;  // whether agents can pass through this tile (roads/paths = true)

        [Header("Shop Settings")]
        public bool     isShop = false;       // can this building sell stocked resources?

        [Header("Maintenance")]
        public Resource maintenanceResource;  // resource needed for upkeep (e.g. coin or labor unit)
        public float    maintenancePerDay;    // amount of maintenanceResource required per day

        [Header("Housing / Workers")]
        public int      capacity = 0;         // if > 0, acts as housing capacity for that many citizens

        [Header("Production & Consumption per Tick")]
        public List<Recipe> producesPerTick;  // resources produced each tick (could be multiple recipes)
        public List<Recipe> consumesPerTick;  // resources consumed each tick as input (for processing buildings)
    }
}
