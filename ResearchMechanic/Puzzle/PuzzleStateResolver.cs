using System.Collections.Generic;
using System.Linq;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;

namespace jarkos.core.Puzzle
{
    public enum PuzzleNodeState
    {
        Locked, Available, Lit
    }

    public static class PuzzleStateResolver
    {
        public static Dictionary<int, PuzzleNodeState> Evaluate(PuzzleForm form, ResearchPlayer player)
        {
            var result = new Dictionary<int, PuzzleNodeState>();
            var parents = new Dictionary<int, List<int>>();

            if (form == null) return result;

            foreach (var e in form.Edges)
            {
                if (!parents.ContainsKey(e.ToID)) parents[e.ToID] = new List<int>();
                parents[e.ToID].Add(e.FromID);
            }
            
            foreach (var node in form.Nodes)
            {
                if (player.ActivePuzzleProgress.ContainsKey(node.ID))
                {
                    result[node.ID] = PuzzleNodeState.Lit;
                    continue;
                }

                if (node.Type == PuzzleNodeType.Start)
                {
                    result[node.ID] = PuzzleNodeState.Available;
                    continue;
                }

                bool hasParents = parents.TryGetValue(node.ID, out var parentList);

                if (!hasParents || parentList.Count == 0)
                {
                    result[node.ID] = PuzzleNodeState.Locked;
                    continue;
                }

                if (node.Type == PuzzleNodeType.OR)
                {
                    bool anyLit = parentList.Any(p => player.ActivePuzzleProgress.ContainsKey(p));
                    result[node.ID] = anyLit ? PuzzleNodeState.Available : PuzzleNodeState.Locked;
                }

                else
                {
                    bool allLit = parentList.All(p => player.ActivePuzzleProgress.ContainsKey(p));
                    result[node.ID] = allLit ? PuzzleNodeState.Available : PuzzleNodeState.Locked;
                }
            }
            return result;
        }
    }
}
