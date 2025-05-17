using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Trading.Data;
using Glade.Core.Population;

namespace Glade.Core.Buildings.Data
{
    [CreateAssetMenu(menuName="Glade/Building Level")]
    public class BuildingLevel : ScriptableObject
    {
        [Header("Generic")]
        public string id;                    // e.g. road_lvl1, house_lvl1
        public Sprite sprite;
        public int    moveCost = 100;
        public bool   isPermeable = false;

        [Header("Maintenance")]
        public Resource maintenanceResource; // e.g. coin
        public float    maintenancePerDay;   // units/day

        [Header("Residents / Workers")]
        public int capacity;                 // if >0 → housing slots

        [Header("Production / Consumption")]
        public List<Recipe> producesPerTick;
        public List<Recipe> consumesPerTick;
    }
}
