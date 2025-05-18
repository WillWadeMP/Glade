using System;
using UnityEngine;
using Glade.Core.Tick;

namespace Glade.Core.Tick
{
    /// <summary>
    /// Manages in-game time progression (minutes, hours, days, months, years).
    /// Configurable through external JSON for time ratios.
    /// </summary>
    public class TimeManager : MonoBehaviour, ITickable
    {
        [Header("Time Configuration (external)")]
        [Tooltip("JSON file name in StreamingAssets for time settings")]
        public string timeConfigFile = "time_config.json";

        // Hierarchical time state
        public int Minute { get; private set; } = 0;
        public int Hour   { get; private set; } = 0;
        public int Day    { get; private set; } = 1;
        public int Month  { get; private set; } = 1;
        public int Year   { get; private set; } = 1;

        // Configurable ratios (default values, can be overwritten by JSON)
        private int minutesPerHour = 20;
        private int hoursPerDay    = 8;
        private int daysPerMonth   = 30;
        private int monthsPerYear  = 12;

        // Events for time milestones
        public event Action OnMinuteTick;
        public event Action OnHourElapsed;
        public event Action OnDayElapsed;
        public event Action OnMonthElapsed;
        public event Action OnYearElapsed;

        private void Awake()
        {
            LoadConfig();
        }

        /// <summary>
        /// Advances time by one tick (one minute) and triggers events on hour/day transitions.
        /// </summary>
        public void Tick(float deltaTime)
        {
            // Advance one minute
            Minute += 1;
            OnMinuteTick?.Invoke();

            // Check for hour rollover
            if (Minute >= minutesPerHour)
            {
                Minute = 0;
                Hour += 1;
                OnHourElapsed?.Invoke();
            }
            // Check for day rollover
            if (Hour >= hoursPerDay)
            {
                Hour = 0;
                Day += 1;
                OnDayElapsed?.Invoke();
            }
            // Check for month rollover
            if (Day > daysPerMonth)
            {
                Day = 1;
                Month += 1;
                OnMonthElapsed?.Invoke();
            }
            // Check for year rollover
            if (Month > monthsPerYear)
            {
                Month = 1;
                Year += 1;
                OnYearElapsed?.Invoke();
            }
        }

        /// <summary>
        /// Loads time ratio settings from a JSON file in StreamingAssets.
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, timeConfigFile);
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    TimeConfig config = JsonUtility.FromJson<TimeConfig>(json);
                    if (config != null)
                    {
                        minutesPerHour = config.minutesPerHour;
                        hoursPerDay    = config.hoursPerDay;
                        daysPerMonth   = config.daysPerMonth;
                        monthsPerYear  = config.monthsPerYear;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"TimeManager: Could not load config file: {e.Message}");
            }
        }

        [Serializable]
        private class TimeConfig
        {
            public int minutesPerHour = 30;
            public int hoursPerDay    = 8;
            public int daysPerMonth   = 30;
            public int monthsPerYear  = 12;
        }
    }
}
