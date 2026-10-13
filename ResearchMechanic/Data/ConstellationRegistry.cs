using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace jarkos.core.Data
{
    public static class ConstellationRegistry
    {
        private static readonly Dictionary<string, ConstellationEntry> _byId = new();
        public static IReadOnlyDictionary<string, ConstellationEntry> All => _byId;
        
        internal static void Add(ConstellationEntry entry)
        {
            if (entry.DisplayName == "First Spark" || entry.DisplayName == "Second Spark") return;

            if (_byId.ContainsKey(entry.ID))
            {
                return;
            }
            _byId[entry.ID] = entry;
        }

        public static bool TryGet(string id, out ConstellationEntry entry) => _byId.TryGetValue(id, out entry);

        public static ConstellationEntry Get(string id) => _byId.TryGetValue(id, out var e) ? e : null;

        internal static void Clear() => _byId.Clear();

        public static Rectangle ComputeBounds(int padding = 600)
        {
            if (_byId.Count == 0) return new Rectangle(-padding, -padding, padding * 2, padding * 2);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

            foreach (var e in _byId.Values)
            {
                var p = e.WorldPosition;
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            return new Rectangle((int)(minX - padding), (int)(minY - padding), (int)(maxX - minX + padding * 2), (int)(maxY - minY + padding * 2));
        }
        public static void Validate()
        {
            var state = new Dictionary<string, int>();
            foreach (var id in _byId.Keys) state[id] = 0;

            bool HasCycle(string id, List<string> stack)
            {
                state[id] = 1;
                stack.Add(id);

                foreach (var pre in _byId[id].Prerequisites)
                {
                    if (!_byId.ContainsKey(pre)) continue;
                    if (state[pre] == 1)
                    {
                        return true;
                    }
                    if (state[pre] == 0 && HasCycle(pre, stack)) return true;
                }

                stack.RemoveAt(stack.Count - 1);
                state[id] = 2;

                return false;
            }

            var reachable = new HashSet<string>();
            var queue = new Queue<string>(_byId.Values.Where(e => e.IsRoot).Select(e => e.ID));
            var children = new Dictionary<string, List<string>>();

            foreach (var e in _byId.Values)
                foreach (var pre in e.Prerequisites)
                    (children.TryGetValue(pre, out var l) ? l : children[pre] = new List<string>()).Add(e.ID);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!reachable.Add(cur)) continue;
                
                if (children.TryGetValue(cur, out var kids))
                    foreach (var k in kids) queue.Enqueue(k);
            }
        }
    }
}
