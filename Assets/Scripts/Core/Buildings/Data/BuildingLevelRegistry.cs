using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Buildings.Data;

namespace Glade.Core.Buildings
{
    /// <summary>
    /// Registry for BuildingLevel definitions (for lookup by id and mod integration).
    /// </summary>
    public static class BuildingLevelRegistry
    {
        private static readonly Dictionary<string, BuildingLevel> levels = new();

        public static void Register(BuildingLevel level) => levels[level.id] = level;
        public static BuildingLevel Get(string id) => levels.TryGetValue(id, out var lvl) ? lvl : null;
        public static IEnumerable<BuildingLevel> All => levels.Values;

        public static void LoadFromResourcesFolder()
        {
            foreach (BuildingLevel lvl in Resources.LoadAll<BuildingLevel>("Buildings"))
            {
                Register(lvl);
            }
        }
    }
}
