using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Population
{
    /// <summary>
    /// Registry for PopulationType assets for quick lookup and mod registration.
    /// </summary>
    public static class PopulationRegistry
    {
        private static readonly Dictionary<string, PopulationType> types = new();

        public static void Register(PopulationType popType) => types[popType.id] = popType;
        public static PopulationType Get(string id) => types.TryGetValue(id, out var pt) ? pt : null;
        public static IEnumerable<PopulationType> All => types.Values;

        public static void LoadFromResourcesFolder()
        {
            foreach (PopulationType pop in Resources.LoadAll<PopulationType>("Resources"))
            {
                Register(pop);
            }
        }
    }
}
