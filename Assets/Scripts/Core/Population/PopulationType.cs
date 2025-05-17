using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Population
{
    [CreateAssetMenu(menuName="Glade/Population Type")]
    public class PopulationType : ScriptableObject
    {
        public string id;
        public Sprite portrait;
        public float  baseWage = 1f;

        public List<NeedDefinition> needs;
        public List<UpgradePath>    upgrades;
    }
}
