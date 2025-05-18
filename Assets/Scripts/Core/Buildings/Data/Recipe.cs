using System;
using System.Collections.Generic;
using Glade.Core.Trading.Data;

namespace Glade.Core.Buildings.Data
{
    /// <summary>
    /// Defines an input-output resource recipe (for building production or consumption).
    /// </summary>
    [Serializable]
    public struct Recipe
    {
        public Resource output;
        public int outputAmount;
        public List<ResourceAmount> inputs;   // list of required input resources and amounts

        [Serializable]
        public struct ResourceAmount
        {
            public Resource resource;
            public int amount;
        }
    }
}
