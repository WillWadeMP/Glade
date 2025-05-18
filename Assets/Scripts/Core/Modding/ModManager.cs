using System.IO;
using UnityEngine;
using Glade.Core.Trading.Data;
using Glade.Core.Buildings;
using Glade.Core.Population;
using Glade.Core.Buildings.Data;

namespace Glade.Core.Modding
{
    /// <summary>
    /// Handles loading external mod asset bundles and registering new content at runtime.
    /// </summary>
    public class ModManager : MonoBehaviour
    {
        [Tooltip("Relative path to Mods folder containing asset bundles")]
        public string modFolderPath = "Mods/";

        public void LoadMods()
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, modFolderPath);
            if (!Directory.Exists(fullPath)) return;
            foreach (string file in Directory.GetFiles(fullPath, "*.bundle"))
            {
                var bundle = AssetBundle.LoadFromFile(file);
                if (bundle == null) continue;
                // Load all ScriptableObjects from the bundle and register them
                ScriptableObject[] assets = bundle.LoadAllAssets<ScriptableObject>();
                foreach (ScriptableObject asset in assets)
                {
                    if (asset is Resource res)        ResourceRegistry.Register(res);
                    else if (asset is BuildingLevel lvl) BuildingLevelRegistry.Register(lvl);
                    else if (asset is PopulationType pop) PopulationRegistry.Register(pop);
                    // Additional asset types (e.g. UpgradePath) could be handled here
                }
                bundle.Unload(false);
            }
        }
    }
}
