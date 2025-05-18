using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Trading.Data
{
    /// <summary>
    /// Runtime registry for Resource definitions, enabling lookup by ID and dynamic mod registration.
    /// </summary>
    public static class ResourceRegistry
    {
        private static readonly Dictionary<string, Resource> resources = new();

        public static void Register(Resource resource) => resources[resource.id] = resource;
        public static Resource Get(string id) => resources.TryGetValue(id, out var res) ? res : null;
        public static IEnumerable<Resource> All => resources.Values;

        /// <summary>
        /// Load all base game Resource assets from Resources/Resources folder.
        /// </summary>
        public static void LoadFromResourcesFolder()
        {
            foreach (Resource res in Resources.LoadAll<Resource>("Resources"))
            {
                Register(res);
            }
        }
    }
}
