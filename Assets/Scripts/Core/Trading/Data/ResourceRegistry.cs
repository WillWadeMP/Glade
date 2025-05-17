using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Trading.Data
{
    /// <summary>Runtime lookup of Resource assets by ID.</summary>
    public static class ResourceRegistry
    {
        private static readonly Dictionary<string, Resource> _map = new();

        public static void Register(Resource r) => _map[r.id] = r;
        public static Resource Get(string id)    => _map.TryGetValue(id, out var r) ? r : null;
        public static IEnumerable<Resource> All  => _map.Values;

        /// <summary>Convenience loader for boot-up: scans Resources/Resources/*.asset</summary>
        public static void LoadFromResourcesFolder()
        {
            foreach (var r in Resources.LoadAll<Resource>("Resources"))
                Register(r);
        }
    }
}
