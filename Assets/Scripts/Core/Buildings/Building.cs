using UnityEngine;
using Glade.Core.Tick;

namespace Glade.Core.Buildings
{
    /// <summary>
    /// MonoBehaviour placed on every in-world structure.  
    /// Handles grid snap and terrain pass-through flags.
    /// </summary>
    public abstract class Building : MonoBehaviour, ITickable
    {
        [Tooltip("Roads or footpaths = true; blocking buildings = false")]
        public bool isPermeable = false;

        [Tooltip("Base movement cost through this tile (1 = road, 100 = default)")]
        public int moveCost = 100;

        public Vector2Int GridPos { get; private set; }

        protected virtual void Awake()
        {
            // Auto-register with TickManager if present
            FindObjectOfType<TickManager>()?.Register(this);
        }

        public void Init(Vector2Int gridPos)
        {
            GridPos = gridPos;
            transform.position = new Vector3(gridPos.x + 0.5f, 0f, gridPos.y + 0.5f);
        }

        // Default tick does nothing; override in subclasses or via composition
        public virtual void Tick(float deltaTime) { }
    }
}
