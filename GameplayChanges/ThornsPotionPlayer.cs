using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.GameplayChanges
{
    public class ThornsPotionPlayer : ModPlayer
    {
        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.HasBuff(BuffID.Thorns) && info.Damage > 0)
            {
                NPC targetNPC = null;

                if (info.DamageSource.SourceNPCIndex >= 0 && info.DamageSource.SourceNPCIndex < Main.maxNPCs)
                {
                    targetNPC = Main.npc[info.DamageSource.SourceNPCIndex];
                }
                else if (info.DamageSource.TryGetCausingEntity(out Entity entity) && entity is NPC npcEntity)
                {
                    targetNPC = npcEntity;
                }
                else if (info.DamageSource.SourceProjectileLocalIndex >= 0 && info.DamageSource.SourceProjectileLocalIndex < Main.maxProjectiles)
                {
                    Projectile proj = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];
                    if (proj.active)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC n = Main.npc[i];
                            if (n.active && !n.friendly && n.Hitbox.Intersects(proj.Hitbox))
                            {
                                targetNPC = n;
                                break;
                            }
                        }
                    }
                }

                if (targetNPC != null && targetNPC.active && !targetNPC.friendly)
                {
                    bool isBoss = targetNPC.boss || (targetNPC.realLife >= 0 && targetNPC.realLife < Main.maxNPCs && Main.npc[targetNPC.realLife].boss)
                        || NPCID.Sets.BossHeadTextures[targetNPC.type] != -1;

                    float multiplier = isBoss ? 2.5f : 3.0f;

                    float extraMultiplier = multiplier - 1.0f;
                    int extraDamage = (int)(info.Damage * extraMultiplier);

                    if (extraDamage < 1)
                    {
                        extraDamage = 1;
                    }

                    Player.ApplyDamageToNPC(targetNPC, extraDamage, 0f, 0, false);
                }
            }
        }
    }
}
