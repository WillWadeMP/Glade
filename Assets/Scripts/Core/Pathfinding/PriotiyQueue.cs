using System.Collections.Generic;

namespace Glade.Core.Pathfinding
{
    /// <summary>Simple binary-heap min-priority queue.</summary>
    internal class PriorityQueue<T>
    {
        private readonly List<(T item,int prio)> heap = new();

        public int Count => heap.Count;

        public void Enqueue(T item, int prio)
        {
            heap.Add((item, prio));
            int c = heap.Count - 1;
            while (c > 0 && heap[c].prio < heap[(c - 1) / 2].prio)
            {
                (heap[c], heap[(c - 1) / 2]) = (heap[(c - 1) / 2], heap[c]);
                c = (c - 1) / 2;
            }
        }

        public bool TryDequeue(out T item)
        {
            if (heap.Count == 0) { item = default; return false; }

            item = heap[0].item;
            var last = heap[^1];
            heap.RemoveAt(heap.Count - 1);
            if (heap.Count == 0) return true;

            heap[0] = last;
            int i = 0;
            while (true)
            {
                int l = i * 2 + 1, r = l + 1, smallest = i;
                if (l < heap.Count && heap[l].prio < heap[smallest].prio) smallest = l;
                if (r < heap.Count && heap[r].prio < heap[smallest].prio) smallest = r;
                if (smallest == i) break;
                (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                i = smallest;
            }
            return true;
        }
    }
}
