using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Pathfinding
{
    public class GridPathfinding
    {
        private readonly IPathCostProvider costProvider;

        public GridPathfinding(IPathCostProvider provider)
        {
            costProvider = provider;
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var openSet = new PriorityQueue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };

            openSet.Enqueue(start, 0);

            while (openSet.TryDequeue(out var current))
            {
                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                foreach (var n in Neighbours(current))
                {
                    if (!costProvider.Passable(n)) continue;

                    int tentativeG = gScore[current] + costProvider.MoveCost(n);
                    if (!gScore.TryGetValue(n, out int existingG) || tentativeG < existingG)
                    {
                        cameFrom[n] = current;
                        gScore[n] = tentativeG;
                        int fScore = tentativeG + Manhattan(n, goal);
                        openSet.Enqueue(n, fScore);
                    }
                }
            }

            return null; // no path
        }

        private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (cameFrom.TryGetValue(current, out current))
                path.Add(current);
            path.Reverse();
            return path;
        }

        private static IEnumerable<Vector2Int> Neighbours(Vector2Int pos)
        {
            yield return pos + Vector2Int.up;
            yield return pos + Vector2Int.right;
            yield return pos + Vector2Int.down;
            yield return pos + Vector2Int.left;
        }

        private static int Manhattan(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
