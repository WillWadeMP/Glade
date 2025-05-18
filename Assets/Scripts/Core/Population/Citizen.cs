using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Data;
using Glade.Core.Buildings;
using Glade.Core.Pathfinding;
using Glade.Core.World;

namespace Glade.Core.Population
{
    /// <summary>
    /// Simulates a single citizen, who fulfills needs by traveling to buildings and purchasing resources.
    /// Must be initialised via <see cref="Init"/> immediately after AddComponent.
    /// </summary>
    public class Citizen : MonoBehaviour, ITickable
    {
        // ——— Public fields ———
        public PopulationType type;
        public Building home;
        public Building       workplace;      // optional, not used here
        public float          happiness = 50f;
        public float money;

        // ——— Movement planning ———
        private const int DailyBudget = 480;
        private int         remainingMovement;
        private List<Vector2Int> currentPath;
        private int         pathIndex;
        private float       stepProgress;
        private Building    targetBuilding;
        private NeedDefinition? targetNeed;
        private bool hasTriedPlanning = false;


        // ——— Need tracking ———
        public Dictionary<Resource, float> needBuffer { get; private set; }

        // ——— Pathfinding ———
        private WorldGrid       worldGrid;
        private GridPathfinding pathfinder;

        // ——— Init flag ———
        private bool initialised;

        /// <summary>
        /// Call immediately after AddComponent to configure this citizen.
        /// </summary>
        public void Init(PopulationType type, Building home, float startingMoney)
        {
            this.type  = type;
            this.home  = home;
            this.money = startingMoney;

            initialised       = true;
            remainingMovement = DailyBudget;

            // fill one-day buffer
            needBuffer = new Dictionary<Resource, float>();
            foreach (var need in type.needs)
                needBuffer[need.resource] = need.perDay;

            // register for minute ticks
            FindObjectOfType<TickManager>()?.Register(this);

            // subscribe to day rollover
            var tm = FindObjectOfType<TimeManager>();
            if (tm != null)
                tm.OnDayElapsed += OnNewDay;

            // pathfinding setup
            worldGrid  = FindObjectOfType<WorldGrid>();
            pathfinder = new GridPathfinding(worldGrid);
        }

        void Awake()
        {
            // Intentionally empty: all setup in Init()
        }

        void Start()
        {
            if (!initialised)
            {
                Debug.LogError($"Citizen `{name}` started without Init().");
                enabled = false;
            }
        }

        /// <summary>Called each minute (1 tick).</summary>
        public void Tick(float deltaTime)
        {
            if (currentPath != null)
            {
                if (pathIndex < currentPath.Count - 1)
                {
                    Vector2Int nextTile = currentPath[pathIndex + 1];
                    int tileCost = worldGrid.MoveCost(nextTile);
                    stepProgress += 1f;
                    remainingMovement -= 1;
                    if (stepProgress >= tileCost)
                    {
                        pathIndex++;
                        stepProgress = 0f;
                        if (pathIndex == currentPath.Count - 1)
                            OnArriveAtBuilding();
                    }
                }
                else
                {
                    currentPath   = null;
                    targetNeed    = null;
                    targetBuilding = null;
                }
            }
            else if (remainingMovement > 0 && !hasTriedPlanning)
            {
                hasTriedPlanning = true;
                PlanNextNeed();
            }
        }

        private void OnNewDay()
        {
            hasTriedPlanning = false;
            remainingMovement = DailyBudget;
            foreach (var need in type.needs)
            {
                var res = need.resource;
                needBuffer[res] -= need.perDay;
                if (needBuffer[res] < 0f)
                {
                    happiness -= need.happinessImpact;
                    needBuffer[res] = Mathf.Max(needBuffer[res], -2f * need.perDay);
                }
                else if (need.tier == NeedTier.Want)
                {
                    happiness = Mathf.Min(100f, happiness + need.happinessImpact);
                }
            }
            happiness = Mathf.Clamp(happiness, 0f, 100f);
        }

        private void PlanNextNeed()
        {
            NeedDefinition? chosen = null;
            foreach (var n in type.needs)
            {
                bool okTier = (n.tier != NeedTier.Want) || (happiness > 70f);
                if (!okTier) continue;
                float currentDays = needBuffer[n.resource] / n.perDay;
                if (currentDays < 2f) { chosen = n; break; }
            }
            if (chosen == null) return;

            var nd      = chosen.Value;
            int desired = Mathf.Max(1, Mathf.CeilToInt((2f * nd.perDay) - needBuffer[nd.resource]));
            float price = nd.resource.baseValue;
            int maxAff  = Mathf.FloorToInt(money / price);
            int qty     = Mathf.Min(desired, maxAff);
            if (qty <= 0) return;

            var supplier = FindNearestSupplier(nd.resource);
            if (supplier == null) return;

            var path = pathfinder.FindPath(home.GridPos, supplier.GridPos);
            if (path == null) return;
            int costTo = PathCost(path);
            if (costTo * 2 > remainingMovement) return;

            currentPath     = path;
            pathIndex       = 0;
            stepProgress    = 0f;
            targetBuilding  = supplier;
            targetNeed      = nd;
        }

        private void OnArriveAtBuilding()
        {
            var nd = targetNeed;
            if (nd == null || targetBuilding == null) return;
            var res        = nd.Value.resource;
            int maxNeed    = Mathf.CeilToInt((2f * nd.Value.perDay) - needBuffer[res]);
            int avail      = targetBuilding.GetInventoryAmount(res);
            int affordable = Mathf.FloorToInt(money / res.baseValue);
            int qty        = Mathf.Clamp(Mathf.Min(maxNeed, affordable), 1, avail);
            if (avail > 0 && qty > 0)
            {
                float cost    = qty * res.baseValue;
                money        -= cost;
                targetBuilding.SellResource(res, qty);
                needBuffer[res] += qty;
            }

            var returnPath = pathfinder.FindPath(targetBuilding.GridPos, home.GridPos);
            if (returnPath != null)
            {
                currentPath      = returnPath;
                pathIndex        = 0;
                stepProgress     = 0f;
                targetBuilding   = null;
                targetNeed       = null;
            }
            else
            {
                currentPath = null;
            }
        }

        private Building FindNearestSupplier(Resource resource)
        {
            Building best = null;
            int bestCost  = int.MaxValue;
            foreach (var b in Building.AllBuildings)
            {
                if (!b.IsSupplierOf(resource)) continue;
                var path = pathfinder.FindPath(home.GridPos, b.GridPos);
                if (path == null) continue;
                int c = PathCost(path);
                if (c < bestCost)
                {
                    bestCost  = c;
                    best      = b;
                }
            }
            return best;
        }

        private int PathCost(List<Vector2Int> path)
        {
            int sum = 0;
            for (int i = 1; i < path.Count; i++)
                sum += worldGrid.MoveCost(path[i]);
            return sum;
        }
    }
}
