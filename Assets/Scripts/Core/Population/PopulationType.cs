using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Population
{
    /// <summary>
    /// Definition of a population category/tier (e.g. settlers, nobles), including needs and upgrade paths.
    /// </summary>
    [CreateAssetMenu(menuName = "Glade/Population Type")]
    public class PopulationType : ScriptableObject
    {
        public string id;
        public Sprite portrait;
        public float baseWage = 1f;           // base coin income per day for one citizen of this type
        public List<NeedDefinition> needs;    // list of needs for this population (food, water, etc.)
        public List<UpgradePath> upgrades;    // possible upgrade paths to higher population tiers
    }

    [System.Serializable]
    public struct UpgradePath
    {
        public PopulationType targetType;
        // (Additional upgrade requirements could be added here, e.g. minimum happiness or resources)
    }
}
