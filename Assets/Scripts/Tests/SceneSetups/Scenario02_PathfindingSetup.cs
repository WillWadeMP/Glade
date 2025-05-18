using UnityEngine;
using Glade.Core.Trading.Data;
using Glade.Tests.Utils;

namespace Glade.Tests.SceneSetups
{
    /// <summary>
    /// Straight 20-tile road (brown) between hut & food stand.  All other tiles are slow grass (green).
    /// A single citizen should walk along the fast road.
    /// </summary>
    public class Scenario02_PathfindingSetup : MonoBehaviour
    {
        void Start()
        {
            // core managers
            TestFactory.WireCoreManagers();

            // resources
            var coin   = TestFactory.CreateResource("coin",  true,  1f);
            var food   = TestFactory.CreateResource("food",  false, 1f);
            var labour = TestFactory.CreateResource("labour",false, 1f);

            // road & grass levels
            var roadLvl  = TestFactory.CreateRoadLevel("road_fast", 10, 0.2f, coin);
            var grassLvl = TestFactory.CreateRoadLevel("grass",     100, 0f,   null);

            // lay 20×5 grid
            for (int x = 0; x < 20; x++)
                for (int y = 0; y < 5; y++)
                {
                    var lvl = (y == 2) ? roadLvl : grassLvl;
                    TestFactory.SpawnBuilding(lvl, new Vector2Int(x, y));
                }

            // food stand at (19,2)
            var stallLvl = TestFactory.CreateHouseLevel("food_stand");
            stallLvl.isShop = true;                                      // <— mark as shop
            var stand = TestFactory.SpawnBuilding(stallLvl, new Vector2Int(19, 2));
            // preload food into stall inventory for citizen purchase
            stand.SellResource(food, -100);

            // Print debug info
            Debug.Log($"Food stand is supplier of food: {stand.IsSupplierOf(food)}");

            // hut at (0,2)
            var hutLvl = TestFactory.CreateHouseLevel("hut", 1);
            var hut    = TestFactory.SpawnBuilding(hutLvl, new Vector2Int(0, 2));

            // population
            var settler = TestFactory.CreatePopulationType("settler", food, null, labour, 5f);
            TestFactory.SpawnCitizen(settler, hut, 20f);
        }
    }
}
