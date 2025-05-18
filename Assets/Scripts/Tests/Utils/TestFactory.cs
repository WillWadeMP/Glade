using System.Linq;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Data;
using Glade.Core.Population;
using Glade.Core.Buildings;
using Glade.Core.Buildings.Data;
using Glade.Core.World;
using Glade.Core.Trading.Engine;

namespace Glade.Tests.Utils
{
    /// <summary>
    /// Helper methods for spawning core managers, ScriptableObjects,
    /// buildings, roads, grass tiles, and citizens with visible cubes.
    /// </summary>
    public static class TestFactory
    {
        // ——— Core singletons ———
        public static TickManager RequireTickManager(float tickInterval = 0.02f)
        {
            var t = Object.FindObjectOfType<TickManager>();
            if (t) return t;
            var go = new GameObject("TickManager");
            t = go.AddComponent<TickManager>();
            typeof(TickManager).GetField("tickInterval",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(t, tickInterval);
            return t;
        }

        public static TimeManager RequireTimeManager() =>
            Object.FindObjectOfType<TimeManager>()
            ?? new GameObject("TimeManager").AddComponent<TimeManager>();

        public static WorldGrid RequireWorldGrid(int w = 50, int h = 50)
        {
            var g = Object.FindObjectOfType<WorldGrid>();
            if (g) return g;
            var go = new GameObject("WorldGrid");
            var wg = go.AddComponent<WorldGrid>();
            typeof(WorldGrid).GetField("width",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(wg, w);
            typeof(WorldGrid).GetField("height",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(wg, h);
            return wg;
        }

        public static AuctionHouse RequireAuctionHouse() =>
            Object.FindObjectOfType<AuctionHouse>()
            ?? new GameObject("AuctionHouse").AddComponent<AuctionHouse>();

        public static PopulationManager RequirePopulationManager() =>
            Object.FindObjectOfType<PopulationManager>()
            ?? new GameObject("PopulationManager").AddComponent<PopulationManager>();

        public static void WireCoreManagers()
        {
            var tick = RequireTickManager();
            tick.Register(RequireTimeManager());
            tick.Register(RequireAuctionHouse());
            tick.Register(RequirePopulationManager());
            RequireWorldGrid();
        }

        // ——— ScriptableObjects ———
        public static Resource CreateResource(string id, bool currency = false, float baseVal = 1f)
        {
            var r = ResourceRegistry.Get(id);
            if (r) return r;
            r = ScriptableObject.CreateInstance<Resource>();
            r.id = id; r.isCurrency = currency; r.baseValue = baseVal;
            ResourceRegistry.Register(r);
            if (currency && CurrencyManager.Default == null)
                CurrencyManager.SetDefault(r);
            return r;
        }

        public static BuildingLevel CreateRoadLevel(string id, int moveCost, float maint = 0f, Resource maintRes = null)
        {
            var lvl = ScriptableObject.CreateInstance<BuildingLevel>();
            lvl.id                   = id;
            lvl.isPermeable          = true;
            lvl.moveCost             = moveCost;
            lvl.maintenanceResource  = maintRes;
            lvl.maintenancePerDay    = maint;
            BuildingLevelRegistry.Register(lvl);
            return lvl;
        }

        public static BuildingLevel CreateHouseLevel(string id, int cap = 4)
        {
            var lvl = ScriptableObject.CreateInstance<BuildingLevel>();
            lvl.id                   = id;
            lvl.capacity             = cap;
            lvl.moveCost             = 100;
            BuildingLevelRegistry.Register(lvl);
            return lvl;
        }

        public static PopulationType CreatePopulationType(string id, Resource food, Resource water, Resource labour, float wage)
        {
            var pt = PopulationRegistry.Get(id);
            if (pt) return pt;
            pt = ScriptableObject.CreateInstance<PopulationType>();
            pt.id       = id;
            pt.baseWage = wage;
            pt.needs    = new System.Collections.Generic.List<NeedDefinition>
            {
                new NeedDefinition{resource=food,  perDay=1f, happinessImpact=5f, tier=NeedTier.Need},
                new NeedDefinition{resource=water ?? food, perDay=1f, happinessImpact=5f, tier=NeedTier.Need}
            };
            PopulationRegistry.Register(pt);
            return pt;
        }

        // ——— Spawners with visuals ———
        private static GameObject MakeColoredCube(Color c, Vector3 scale)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().material.color = c;
            Object.Destroy(cube.GetComponent<Collider>());
            return cube;
        }

        public static Building SpawnBuilding(BuildingLevel lvl, Vector2Int pos)
        {
            GameObject root;
            if (lvl.id.Contains("road"))
                root = MakeColoredCube(new Color(0.4f,0.26f,0.13f), new Vector3(1f,0.2f,1f));
            else if (lvl.id.Contains("grass"))
                root = MakeColoredCube(Color.green, new Vector3(1f,0.1f,1f));
            else
                root = MakeColoredCube(Color.grey, new Vector3(1f,1f,1f));

            root.name = $"Building_{lvl.id}_{pos.x}_{pos.y}";
            root.transform.position = new Vector3(pos.x + .5f, 0f, pos.y + .5f);

            var b = root.AddComponent<Building>();
            b.Init(lvl, pos);
            return b;
        }

        public static Citizen SpawnCitizen(PopulationType pt, Building home, float money = 10f)
        {
            var vis = MakeColoredCube(Color.blue, new Vector3(0.2f,0.8f,0.2f));
            vis.name = $"Citizen_{pt.id}";
            vis.transform.position = new Vector3(home.GridPos.x + .5f, 0f, home.GridPos.y + .5f);

            var c = vis.AddComponent<Citizen>();
            c.Init(pt, home, money);
            home.AddResident(c);
            RequirePopulationManager().Register(c);
            return c;
        }
    }
}
