using System.Collections.Generic;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;

namespace jarkos.core
{
    public static class GraphState
    {
        public static readonly Dictionary<string, NodeState> States = new();
        private static Dictionary<string, List<string>> _children;
        private static void EnsureChildIndex()
        {
            if (_children != null) return;
            _children = new Dictionary<string, List<string>>();
            foreach (var e in ConstellationRegistry.All.Values)
                foreach (var pre in e.Prerequisites)
                {
                    if (!_children.TryGetValue(pre, out var l))
                        _children[pre] = l = new List<string>();
                    l.Add(e.ID);
                }
        }
        public static void RecomputeAll(ResearchPlayer plr)
        {
            EnsureChildIndex();
            States.Clear();
            foreach (var id in ConstellationRegistry.All.Keys)
                States[id] = Evaluate(plr, id);
        }
        public static void RecomputeFrom(ResearchPlayer plr, string completedId)
        {
            EnsureChildIndex();
            States[completedId] = Evaluate(plr, completedId);
            if (_children.TryGetValue(completedId, out var kids))
                foreach (var k in kids) States[k] = Evaluate(plr, k);
        }
        public static NodeState Get(string id) => States.TryGetValue(id, out var s) ? s : NodeState.Hidden;
        private static NodeState Evaluate(ResearchPlayer plr, string id)
        {
            if (plr.IsCompleted(id)) return NodeState.Completed;
            if (plr.ActiveResearchID == id) return NodeState.Active;
            var entry = ConstellationRegistry.Get(id);
            if (entry == null) return NodeState.Hidden;
            if (entry.IsRoot) return NodeState.Revealed;
            if (entry.Prerequisites.Count == 0) return NodeState.Revealed;
            if (entry.RequireAllParents)
            {
                foreach (var pre in entry.Prerequisites)
                    if (!plr.IsCompleted(pre)) return NodeState.Hidden;
                return NodeState.Revealed;
            }
            else
            {
                foreach (var pre in entry.Prerequisites)
                    if (plr.IsCompleted(pre)) return NodeState.Revealed;
                return NodeState.Hidden;
            }
        }
    }
}
