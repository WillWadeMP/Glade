using UnityEngine;
using Glade.Tests.Utils;

public class Scenario01_TimeSystemSetup : MonoBehaviour
    {
        private void Start()
        {
            TestFactory.WireCoreManagers();
            gameObject.AddComponent<TimeLogger>(); // Prints day/hour rollover
        }
    }
