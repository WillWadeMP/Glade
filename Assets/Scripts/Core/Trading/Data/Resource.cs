using UnityEngine;

namespace Glade.Core.Trading.Data
{
    /// <summary>
    /// Immutable definition of a tradable good or currency.
    /// Designers author these as ScriptableObjects; mods load them at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Glade/Resource")]
    public class Resource : ScriptableObject
    {
        [Tooltip("Unique ID e.g. \"food\", \"blood\"")]
        public string id;

        public Sprite icon;
        public bool isCurrency;
        public bool perishable;
        public float baseValue = 1f;
    }
}
