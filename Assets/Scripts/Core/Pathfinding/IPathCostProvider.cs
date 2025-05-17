using UnityEngine;

namespace Glade.Core.Pathfinding
{
    /// <summary>
    /// Lets A* query whether a tile is walkable and what it costs.
    /// Allows multiple map implementations (island, multi-level, etc.).
    /// </summary>
    public interface IPathCostProvider
    {
        bool Passable(Vector2Int tile);
        int  MoveCost(Vector2Int tile);
    }
}
