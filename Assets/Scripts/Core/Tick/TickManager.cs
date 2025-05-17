using System.Collections.Generic;
using UnityEngine;

namespace Glade.Core.Tick
{
    /// <summary>
    /// Central heartbeat; broadcasts one tick every <see cref="tickInterval"/> seconds.
    /// </summary>
    public class TickManager : MonoBehaviour
    {
        [Tooltip("Seconds of real time per simulation tick")]
        [SerializeField] private float tickInterval = 1f;

        private readonly List<ITickable> _subscribers = new();
        private float _timer;

        public void Register(ITickable t) => _subscribers.Add(t);
        public void Unregister(ITickable t) => _subscribers.Remove(t);

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < tickInterval) return;

            _timer = 0f;
            float dt = tickInterval;

            // Note: no try/catch – let the crash surface during dev.
            foreach (var sub in _subscribers)
                sub.Tick(dt);
        }
    }
}
