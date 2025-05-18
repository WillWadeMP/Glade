using UnityEngine;
using Glade.Core.Buildings;

namespace Glade.Core.World
{
    /// <summary>
    /// Represents the game world as a grid and provides path cost queries. Updates when buildings are placed or changed.
    /// </summary>
    public class WorldGrid : MonoBehaviour, Pathfinding.IPathCostProvider
    {
        [SerializeField] private int width = 100;
        [SerializeField] private int height = 100;

        // Grid data
        private bool[,] blocked;
        private int[,] cost;

        private void Awake()
        {
            blocked = new bool[width, height];
            cost = new int[width, height];
            // Initialize default movement costs (e.g., terrain base cost)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    blocked[x, y] = false;
                    cost[x, y] = 100;  // default terrain movement cost
                }
            }
        }

        /// <summary>
        /// Mark a grid cell as occupied by a building, updating passability and move cost.
        /// </summary>
        public void Occupy(Building building)
        {
            Vector2Int p = building.GridPos;
            // Non-permeable buildings block movement entirely
            blocked[p.x, p.y] = !building.Level.isPermeable;
            // Set movement cost (if building is permeable, use its moveCost; if blocking, cost is irrelevant but set high)
            cost[p.x, p.y] = building.Level.moveCost;
        }

        // IPathCostProvider implementation:
        public bool Passable(Vector2Int tile)
        {
            return InBounds(tile) && !blocked[tile.x, tile.y];
        }
        public int MoveCost(Vector2Int tile)
        {
            return InBounds(tile) ? cost[tile.x, tile.y] : int.MaxValue;
        }
        private bool InBounds(Vector2Int v)
        {
            return v.x >= 0 && v.x < width && v.y >= 0 && v.y < height;
        }
    }
}
