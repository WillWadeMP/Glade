using System.IO;
using UnityEngine;

namespace Glade.Core.Modding
{
    public class ModManager : MonoBehaviour
    {
        public string modFolderPath = "Mods/";

        public void LoadMods()
        {
            foreach (var file in Directory.GetFiles(modFolderPath, "*.bundle"))
            {
                var bundle = AssetBundle.LoadFromFile(file);
                foreach (var res in bundle.LoadAllAssets<ScriptableObject>())
                {
                    // You'd route each type here:
                    // if (res is Resource r) ResourceRegistry.Register(r);
                }
            }
        }
    }
}
