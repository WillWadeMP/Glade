using UnityEngine;
using Glade.Core.Tick;
using Glade.Core.Trading.Engine;
using Glade.Core.Population;

namespace Glade.Tests.Utils
{
    /// <summary>Simple console loggers to visualise activity during tests.</summary>
    public class TimeLogger : MonoBehaviour
    {
        private void Start()
        {
            var tm = FindObjectOfType<TimeManager>();
            tm.OnMinuteTick  += () => { /* too spammy */ };
            tm.OnHourElapsed  += () => Debug.Log($"[Time] Hour {tm.Hour} Complete");
            tm.OnDayElapsed   += () => Debug.Log($"[Time] Day {tm.Day}/{tm.Month}/{tm.Year} Complete");
        }
    }

    public class TradeLogger : MonoBehaviour
    {
        private void Start()
        {
            var ah = FindObjectOfType<AuctionHouse>();
            // Monkey-patch AuctionHouse order-matching to emit logs
            var clearMethod = typeof(AuctionHouse).GetNestedType("OrderBook",
                    System.Reflection.BindingFlags.NonPublic).GetMethod("ClearTrades");
            // Not altering IL here; easiest: periodic poll counts in Update
        }
    }

    public class PopLogger : MonoBehaviour, ITickable
    {
        public void Tick(float dt)
        {
            if (Random.Range(0, 600) == 0) // roughly every 10s real-time
            {
                int count = FindObjectsOfType<Citizen>().Length;
                Debug.Log($"[Pop] Citizens: {count}");
            }
        }
    }
}
