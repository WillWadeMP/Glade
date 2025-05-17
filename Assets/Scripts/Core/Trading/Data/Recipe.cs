using System.Collections.Generic;

namespace Glade.Core.Trading.Data
{
    [System.Serializable]
    public struct Recipe
    {
        public Resource output;
        public int outputAmount;

        public List<ResourceAmount> inputs;

        [System.Serializable]
        public struct ResourceAmount
        {
            public Resource resource;
            public int amount;
        }
    }
}
