using System.Collections.Generic;
using Terraria.ModLoader;

namespace jarkos.core.Data
{
    public abstract class PuzzleForm : ModType
    {
        public virtual string ID => $"{Mod.Name}:{Name}";
        
        public abstract List<PuzzleNodeData> Nodes { get; }
        public abstract List<PuzzleEdgeData> Edges { get; }

        protected sealed override void Register()
        {
            ModTypeLookup<PuzzleForm>.Register(this);
            PuzzleRegistry.Add(this);
        }
        public sealed override void SetupContent() => SetStaticDefaults();
    }
}
