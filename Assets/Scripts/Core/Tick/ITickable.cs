using System;

namespace Glade.Core.Tick
{
    /// <summary>
    /// Anything that wants a simulation step implements this.
    /// TickManager guarantees deterministic ordering (by registration order).
    /// </summary>
    public interface ITickable
    {
        /// <param name="deltaTime">Seconds since last tick (real time, not game time).</param>
        void Tick(float deltaTime);
    }
}
