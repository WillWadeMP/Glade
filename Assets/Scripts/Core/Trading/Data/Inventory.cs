using System.Collections.Generic;

namespace Glade.Core.Trading.Data
{
    /// <summary>
    /// Simple wrapper so we can extend later (events, pooling, serialisation).
    /// </summary>
    public class Inventory : Dictionary<Resource, int> { }
}
