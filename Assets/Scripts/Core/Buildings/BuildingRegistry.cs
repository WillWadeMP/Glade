using System.Collections.Generic;

namespace Glade.Core.Buildings
{
    public static class BuildingRegistry
    {
        private static readonly Dictionary<string, Building> prototypes = new();

        public static void Register(string id, Building prefab) => prototypes[id] = prefab;

        public static Building Get(string id)
            => prototypes.TryGetValue(id, out var prefab) ? prefab : null;

        public static IEnumerable<string> AllIds => prototypes.Keys;
    }
}
