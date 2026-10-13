using System.Collections.Generic;
using jarkos.core.Data;

namespace jarkos.core.Data
{
    public static class PuzzleRegistry
    {
        private static readonly Dictionary<string, PuzzleForm> _byId = new();
        
        internal static void Add(PuzzleForm form)
        {
            if (!_byId.ContainsKey(form.ID))
                _byId[form.ID] = form;
        }

        public static bool TryGet(string id, out PuzzleForm form) => _byId.TryGetValue(id, out form);
        
        public static PuzzleForm Get(string id) => _byId.TryGetValue(id, out var found) ? found : null;
    }
}
