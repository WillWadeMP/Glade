using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Buildings;

namespace Glade.Core.Population
{
    /// <summary>
    /// Oversees the population as a whole: births, deaths, and upgrades, and manages daily labor and economy aspects.
    /// </summary>
    public class PopulationManager : MonoBehaviour, ITickable
    {
        private readonly List<Citizen> citizens = new();

        // City economy/maintenance
        public float cityBudget = 0f;          // city treasury for maintenance, funded by taxes
        [Range(0f, 1f)] public float taxRate = 0.3f;
        private int laborPool;                // available labor count (unused citizens) for maintenance

        private TimeManager timeManager;

        private void Start()
        {
            timeManager = FindObjectOfType<TimeManager>();
            if (timeManager != null)
            {
                // Subscribe to daily tick for population lifecycle events and maintenance
                timeManager.OnDayElapsed += OnNewDay;
            }
        }

        public void Register(Citizen citizen) => citizens.Add(citizen);
        public void Unregister(Citizen citizen) => citizens.Remove(citizen);

        public void Tick(float deltaTime)
        {
            // PopulationManager doesn't need to do per-minute updates (lifecycle handled daily in OnNewDay).
            // (This Tick is kept for interface consistency.)
        }

        /// <summary>
        /// Called at the end of each day to handle births, deaths, upgrades, and city maintenance.
        /// </summary>
        private void OnNewDay()
        {
            // 1. Economy: collect taxes and pay wages
            cityBudget = 0f;
            foreach (Citizen c in citizens)
            {
                float wage = c.type.baseWage;
                // Tax a portion of wages to city
                float taxAmount = wage * taxRate;
                cityBudget += taxAmount;
                // Give the remainder to the citizen as income
                c.money += (wage - taxAmount);
            }

            // 2. Maintenance: allocate labor and coin to maintain buildings
            laborPool = citizens.Count;  // simplistic assumption: each citizen can perform 1 unit of maintenance work per day
            foreach (Building b in Building.AllBuildings)
            {
                if (b.HasMaintenance())
                {
                    // Each maintenance unit presumably requires one labor and some coin
                    float neededCoin = b.Level.maintenancePerDay;
                    if (cityBudget >= neededCoin && laborPool > 0)
                    {
                        cityBudget -= neededCoin;
                        laborPool -= 1;
                        b.PerformMaintenance(neededCoinSupplied: neededCoin);
                    }
                    else
                    {
                        // Not enough coin or labor to maintain this building today
                        b.PerformMaintenance(neededCoinSupplied: 0f);
                    }
                }
            }

            // 3. Population Upgrades: check if any citizens qualify to upgrade to a higher population tier
            foreach (Citizen c in new List<Citizen>(citizens))
            {
                if (c.type.upgrades != null && c.type.upgrades.Count > 0)
                {
                    // Example criterion: high happiness
                    if (c.happiness > 80f)
                    {
                        PopulationType newType = c.type.upgrades[0].targetType;
                        UpgradeCitizen(c, newType);
                    }
                }
            }

            // 4. Births: potentially add new citizens if housing is available
            int totalCapacity = 0, occupied = 0;
            foreach (Building b in Building.AllBuildings)
            {
                if (b.Level.capacity > 0)
                {
                    totalCapacity += b.Level.capacity;
                    occupied += b.CurrentOccupancy;
                }
            }
            int freeCapacity = totalCapacity - occupied;
            if (freeCapacity > 0)
            {
                // Example: 50% chance each day to spawn a new citizen if capacity allows
                if (Random.value < 0.5f)
                {
                    CreateNewCitizen();
                }
            }

            // 5. Deaths: remove citizens (natural or due to starvation)
            foreach (Citizen c in new List<Citizen>(citizens))
            {
                bool died = false;
                if (c.happiness <= 0f)
                {
                    // Citizen perishes due to extreme unhappiness (starvation or other)
                    died = true;
                }
                else
                {
                    // Random natural death chance (very low per day)
                    if (Random.value < 0.001f) died = true;
                }
                if (died)
                {
                    RemoveCitizen(c);
                }
            }
        }

        /// <summary>
        /// Spawns a new Citizen in the world, if possible (finds a home with free capacity).
        /// </summary>
        private void CreateNewCitizen()
        {
            // Determine base population type for new births (use first available type)
            PopulationType baseType = null;
            foreach (PopulationType pt in PopulationRegistry.All)
            {
                baseType = pt;
                break;
            }
            if (baseType == null) return;
            // Find a house with space
            Building homeWithSpace = null;
            foreach (Building b in Building.AllBuildings)
            {
                if (b.Level.capacity > 0 && b.CurrentOccupancy < b.Level.capacity)
                {
                    homeWithSpace = b;
                    break;
                }
            }
            if (homeWithSpace == null) return;
            // Instantiate new citizen
            GameObject go = new GameObject("Citizen");
            Citizen newCitizen = go.AddComponent<Citizen>();
            newCitizen.type = baseType;
            newCitizen.home = homeWithSpace;
            newCitizen.happiness = 50f;
            newCitizen.money = baseType.baseWage;  // start with some money (one day's wage)
            // Place the citizen at their home position
            go.transform.position = new Vector3(homeWithSpace.GridPos.x + 0.5f, 0f, homeWithSpace.GridPos.y + 0.5f);
            // Register citizen in systems
            homeWithSpace.AddResident(newCitizen);
            citizens.Add(newCitizen);
        }

        /// <summary>
        /// Upgrades a citizen to a new PopulationType, resetting their needs and attributes accordingly.
        /// </summary>
        private void UpgradeCitizen(Citizen citizen, PopulationType newType)
        {
            citizen.type = newType;
            citizen.happiness = Mathf.Min(citizen.happiness, 50f);  // upon upgrade, reset happiness to a baseline (e.g. 50)
            // Reinitialize need buffers for new needs
            // (Old need buffers and resources are not carried over to new social class)
            citizen.GetType(); // just to illustrate reinit; actual implementation would refresh Citizen.needBuffer
            foreach (NeedDefinition need in newType.needs)
            {
                citizen.needBuffer[need.resource] = need.perDay;
            }
        }

        /// <summary>
        /// Removes a citizen from the simulation (due to death).
        /// </summary>
        private void RemoveCitizen(Citizen citizen)
        {
            citizens.Remove(citizen);
            // Free up their home occupancy
            if (citizen.home != null)
            {
                citizen.home.RemoveResident(citizen);
            }
            // Destroy the citizen game object
            Destroy(citizen.gameObject);
        }
    }
}
