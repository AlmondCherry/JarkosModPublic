using jarkos.npc.tenderote;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Jarkos.filters
{
    public class Jarkos_modscene : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<TenderoteEmptyBackgroundStyle>();
        private bool IsPortalActive()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<Jarkos.projectiles.enemy.tenderote.Tenderote_portal>())
                    return true;
            }
            return false;
        }
        public override bool IsSceneEffectActive(Player player)
        {
            if (!ModContent.GetInstance<jarkos.Jarkos_config>().EnableBossBackground) return false;
            return NPC.AnyNPCs(ModContent.NPCType<Tenderote_head>()) || IsPortalActive();
        }
        public override void SpecialVisuals(Player player, bool isActive)
        {
            bool shouldUseTenderoteSky = isActive && NPC.AnyNPCs(ModContent.NPCType<Tenderote_head>());
            CustomSky sky = SkyManager.Instance[TenderoteSkySystem.SkyKey];
        }
    }
}
