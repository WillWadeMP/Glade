using UnityEngine;

namespace Glade.Core.Trading.Data
{
    /// <summary>
    /// Definition of a tradable resource or currency. Created as a ScriptableObject for easy data-driven content.
    /// </summary>
    [CreateAssetMenu(menuName = "Glade/Resource")]
    public class Resource : ScriptableObject
    {
        [Tooltip("Unique identifier (e.g. 'food', 'coin')")]
        public string id;
        public Sprite icon;
        public bool isCurrency;
        public bool perishable;
        public float baseValue = 1f;  // base price or value of one unit
    }
}
