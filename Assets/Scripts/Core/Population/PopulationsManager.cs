using System.Collections.Generic;
using UnityEngine;
using Glade.Core.Tick;

namespace Glade.Core.Population
{
    public class PopulationManager : MonoBehaviour, ITickable
    {
        private readonly List<Citizen> citizens = new();

        public void Register(Citizen c) => citizens.Add(c);

        public void Tick(float dt)
        {
            // Could handle births, deaths, migrations, job assignment, upgrades
        }
    }
}
