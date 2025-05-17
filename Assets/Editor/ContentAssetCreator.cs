#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Glade.Core.Trading.Data;
using Glade.Core.Population;
using Glade.Core.Buildings.Data;

public static class ContentAssetCreator
{
    [MenuItem("Glade/Create Starter Assets")]
    public static void CreatePopRes()
    {
        // Resources folder path
        string resDir = "Assets/Resources/Resources";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(resDir))
            AssetDatabase.CreateFolder("Assets/Resources", "Resources");

        // Coin
        var coin = ScriptableObject.CreateInstance<Resource>();
        coin.id = "coin";
        coin.isCurrency = true;
        AssetDatabase.CreateAsset(coin, $"{resDir}/coin.asset");

        // Food
        var food = ScriptableObject.CreateInstance<Resource>();
        food.id = "food";
        food.baseValue = 1;
        AssetDatabase.CreateAsset(food, $"{resDir}/food.asset");

        // Water
        var water = ScriptableObject.CreateInstance<Resource>();
        water.id = "water";
        water.baseValue = 1;
        AssetDatabase.CreateAsset(water, $"{resDir}/water.asset");

        // Human Settler pop
        var pop = ScriptableObject.CreateInstance<PopulationType>();
        pop.id = "human_settler";
        pop.needs = new()
        {
            new NeedDefinition{ resource=food,  tier=NeedTier.Need, perDay=1, happinessImpact=5 },
            new NeedDefinition{ resource=water, tier=NeedTier.Need, perDay=1, happinessImpact=5 },
        };
        AssetDatabase.CreateAsset(pop, $"{resDir}/human_settler.asset");

        AssetDatabase.SaveAssets();
        Debug.Log("Starter assets created in Resources/Resources/");
    }

    [MenuItem("Glade/Create Starter Buildings")]
    public static void CreateBuildings()
    {
        string dir = "Assets/Resources/Buildings";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Resources", "Buildings");

        var coin = Resources.Load<Resource>("Resources/coin"); // created earlier

        /* ROAD LEVEL -------------------------------------------------- */
        var road = ScriptableObject.CreateInstance<BuildingLevel>();
        road.id      = "road_lvl1";
        road.isPermeable = true;
        road.moveCost    = 20;
        road.maintenanceResource = coin;
        road.maintenancePerDay   = 0.1f;
        AssetDatabase.CreateAsset(road, $"{dir}/road_lvl1.asset");

        /* HOUSE LEVEL ------------------------------------------------- */
        var house = ScriptableObject.CreateInstance<BuildingLevel>();
        house.id       = "house_lvl1";
        house.capacity = 2;
        house.moveCost = 200;             // impassable, so high cost
        AssetDatabase.CreateAsset(house, $"{dir}/house_lvl1.asset");

        AssetDatabase.SaveAssets();
        Debug.Log("Starter building levels created in Resources/Buildings/");
    }
}
#endif
