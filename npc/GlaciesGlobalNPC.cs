using Terraria;
using Terraria.ModLoader;
namespace jarkos.npc
{
    public class GlaciesGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool HasGlaciesSlow;
        public override void ResetEffects(NPC npc)
        {
            HasGlaciesSlow = false;
        }
        public override void PostAI(NPC npc)
        {
            if (HasGlaciesSlow)
            {
                npc.position -= npc.velocity * 0.25f;
            }
        }
    }
}
