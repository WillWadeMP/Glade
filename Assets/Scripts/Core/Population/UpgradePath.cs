namespace Glade.Core.Population
{
    [System.Serializable]
    public struct UpgradePath
    {
        public string nextPopulationId;
        public ResourceRequirement[] gate;   // all must be satisfied
    }
}
