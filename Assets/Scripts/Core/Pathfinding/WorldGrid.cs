using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Pathfinding;
using Glade.Core.Buildings;

namespace Glade.Core.World
{
    /// <summary>Simple square-grid map; acts as global IPathCostProvider.</summary>
    public class WorldGrid : MonoBehaviour, IPathCostProvider
    {
        [SerializeField] private int width  = 64;
        [SerializeField] private int height = 64;
        private bool[,] _blocked;
        private int[,]  _cost;

        void Awake()
        {
            _blocked = new bool[width, height];
            _cost    = new int [width, height];

            // default cost
            for (int x=0;x<width;x++)
                for (int y=0;y<height;y++)
                    _cost[x,y] = 100;
        }

        public void Occupy(Building b)
        {
            var p = b.GridPos;
            _blocked[p.x, p.y] = !b.isPermeable;
            _cost   [p.x, p.y] = b.moveCost;
        }

        /* ------------ IPathCostProvider ------------ */
        public bool Passable(Vector2Int tile) =>
            InBounds(tile) && !_blocked[tile.x, tile.y];

        public int MoveCost(Vector2Int tile) =>
            InBounds(tile) ? _cost[tile.x, tile.y] : int.MaxValue;

        private bool InBounds(Vector2Int v) =>
            v.x >=0 && v.x < width && v.y>=0 && v.y<height;
    }
}
