using Glade.Core.Trading.Data;

namespace Glade.Core.Population
{
    [System.Serializable]
    public struct NeedDefinition
    {
        public Resource  resource;
        public NeedTier  tier;
        public float     perDay;           // units per in-game day
        public float     happinessImpact;  // ± when met/unmet
    }
}
