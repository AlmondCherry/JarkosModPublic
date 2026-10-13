using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.core.Data
{
    public abstract class ConstellationEntry : ModType
    {
        public virtual string ID => $"{Mod.Name}:{Name}";

        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public abstract string IconTexture { get; }

        public abstract Vector2 WorldPosition { get; }

        public virtual List<string> Prerequisites => new();
        public virtual bool RequireAllParents => true;
        public virtual bool IsRoot => false;
        public virtual string PuzzleShapeID => "default";
        public abstract EssenceType Tier { get; }

        public virtual void OnUnlock(Player player) { }

        public virtual string PuzzleRequirementText => "";
        public virtual string RewardItemName => "Mysterious";

        protected sealed override void Register()
        {
            ModTypeLookup<ConstellationEntry>.Register(this);
            ConstellationRegistry.Add(this);
        }
        
        public sealed override void SetupContent() => SetStaticDefaults();
    }
}
