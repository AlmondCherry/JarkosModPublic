using System.Collections.Generic;
using jarkos.core.Data;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
namespace jarkos.tiles.Constellations
{
    public abstract class StarBase : ConstellationEntry
    {
        public override string IconTexture => "jarkos/UI/Textures/StarLarge";
    }
    public class FirstSpark : StarBase
    {
        public override string DisplayName => "First Spark";
        public override string Description => "The beginning of the research path.";
        public override bool IsRoot => true;
        public override List<string> Prerequisites => new List<string>();
        public override EssenceType Tier => EssenceType.Lux;
        public override string PuzzleShapeID => "jarkos:basic_triangle";
        public override string PuzzleRequirementText => "3 nodes: Lux, Lux, Lux";
        public override Vector2 WorldPosition => new Vector2(0, 0);
        public override string IconTexture => "jarkos/UI/Textures/StarLarge";
        public override void OnUnlock(Player player) { }
    }
}
