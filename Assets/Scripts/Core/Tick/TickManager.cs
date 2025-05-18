using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Tick
{
    /// <summary>
    /// Central heartbeat that emits ticks at a fixed real-time interval.
    /// Each tick represents one in-game minute by default.
    /// </summary>
    public class TickManager : MonoBehaviour
    {
        [Tooltip("Seconds of real time per simulation tick (in-game minute)")]
        [SerializeField] private float tickInterval = 1f;  // 1 second real = 1 game minute

        private readonly List<ITickable> subscribers = new();
        private float timer;

        public void Register(ITickable tickable) => subscribers.Add(tickable);
        public void Unregister(ITickable tickable) => subscribers.Remove(tickable);

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < tickInterval) return;
            timer = 0f;
            float dt = 1f;  // 1 game minute per tick
            // Broadcast tick to all subscribers in order of registration
            foreach (var sub in subscribers)
            {
                sub.Tick(dt);
            }
        }
    }
}
