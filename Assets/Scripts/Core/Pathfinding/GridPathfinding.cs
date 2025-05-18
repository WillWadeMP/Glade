using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Pathfinding
{
    /// <summary>
    /// Standard A* implementation over an <see cref="IPathCostProvider"/>.
    /// • Always allows stepping onto <c>goal</c> even if Passable == false.  
    /// • Safety-caps node expansions with <c>maxIterations</c>.  
    /// • Uses an explicit <b>closedSet</b> to stop revisiting tiles and eliminates
    ///   the self-loop that previously caused OOM.  
    /// • Clamps every step cost to at least 1 to prevent zero-cost cycles.
    /// </summary>
    public class GridPathfinding
    {
        private readonly IPathCostProvider costProvider;
        private readonly int               maxIterations;

        public GridPathfinding(IPathCostProvider provider, int maxIterations = 10_000)
        {
            costProvider   = provider;
            this.maxIterations = Mathf.Max(100, maxIterations);
        }

        // ─────────────────────────────────────────────────────────────────── A* ──
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            var openSet   = new PriorityQueue<Vector2Int>();
            var cameFrom  = new Dictionary<Vector2Int, Vector2Int>();
            var gScore    = new Dictionary<Vector2Int, int> { [start] = 0 };
            var closedSet = new HashSet<Vector2Int>();

            int iterations = 0;
            openSet.Enqueue(start, 0);

            while (openSet.TryDequeue(out var current))
            {
                // ─── Safety guard ───
                if (++iterations > maxIterations)
                {
                    Debug.LogWarning($"[GridPathfinding] Aborting after {maxIterations} expansions");
                    return null;
                }

                // ─── Goal reached ───
                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                // skip if we've already evaluated this node
                if (!closedSet.Add(current))
                    continue;

                // ─── Explore 4-way neighbours ───
                foreach (var dir in new[]
                {
                    Vector2Int.up, Vector2Int.down,
                    Vector2Int.left, Vector2Int.right
                })
                {
                    var neighbour = current + dir;

                    // Skip impassable tiles (but always allow the goal itself)
                    if (neighbour != goal && !costProvider.Passable(neighbour))
                        continue;

                    // Ignore neighbour we've already processed
                    if (closedSet.Contains(neighbour))
                        continue;

                    // Clamp step cost to ≥1 to avoid infinite zero-cost loops
                    int stepCost   = Mathf.Max(1, costProvider.MoveCost(neighbour));
                    int tentativeG = gScore[current] + stepCost;

                    if (!gScore.TryGetValue(neighbour, out int currentG) || tentativeG < currentG)
                    {
                        cameFrom[neighbour] = current;
                        gScore[neighbour]   = tentativeG;
                        int priority        = tentativeG + Manhattan(neighbour, goal);
                        openSet.Enqueue(neighbour, priority);
                    }
                }
            }

            // exhausted search without reaching goal
            return null;
        }

        // ────────────────────────────────────────────────────── Helpers ──
        private List<Vector2Int> ReconstructPath(
            Dictionary<Vector2Int, Vector2Int> cameFrom,
            Vector2Int current)
        {
            var path    = new List<Vector2Int> { current };
            var visited = new HashSet<Vector2Int> { current };

            // Walk backwards to the start; break if we ever detect a loop
            while (cameFrom.TryGetValue(current, out var parent))
            {
                if (!visited.Add(parent))
                {
                    Debug.LogError($"[Pathfinding] Loop detected during path reconstruction at {parent}");
                    break;
                }
                path.Insert(0, parent);
                current = parent;
            }

            return path;
        }

        private int Manhattan(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // ───────────────────────────────────────────────────────────────── Interfaces
    public interface IPathCostProvider
    {
        bool Passable  (Vector2Int tile);
        int  MoveCost  (Vector2Int tile);   // must return ≥ 0; 0 will be treated as 1
    }
}
