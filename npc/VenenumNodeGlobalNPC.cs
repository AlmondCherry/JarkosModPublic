using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.core.PlayerInteract;

namespace jarkos.npc
{
    public class VenenumNodeGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        private int _poisonSpreadTimer = 0;
        private int _nextSpreadTime = 210;
        private int _witherDamageTimer = 0;

        public override void AI(NPC npc)
        {
            if (npc.friendly || !npc.active) return;

            int witherBuffType = ModContent.BuffType<buffs.debuff.WitherDebuff>();
            if (npc.HasBuff(witherBuffType))
            {
                _witherDamageTimer++;
                if (_witherDamageTimer >= 60)
                {
                    _witherDamageTimer = 0;

                    int damage = (int)(npc.lifeMax * 0.005f);
                    if (damage < 2) damage = 2;
                    if (npc.boss && damage > 120) damage = 120;

                    npc.life -= damage;
                    if (npc.life <= 0)
                    {
                        npc.checkDead();
                    }

                    CombatText.NewText(npc.getRect(), new Color(180, 255, 0), damage, dramatic: false, dot: false);

                    for (int d = 0; d < 4; d++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.CorruptionThorns, 0, -1f);
                        dust.noGravity = true;
                    }
                }
            }
            else
            {
                _witherDamageTimer = 0;
            }

            bool hasPoison = npc.HasBuff(BuffID.Poisoned) || npc.HasBuff(BuffID.Venom);

            if (!hasPoison)
            {
                _poisonSpreadTimer = 0;
                return;
            }

            bool anyPlayerHasVenenum6 = false;
            bool anyPlayerHasVenenum7 = false;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead)
                {
                    var rp = p.GetModPlayer<ResearchPlayer>();
                    if (rp.IsCompleted("jarkos:VenenumNode6")) anyPlayerHasVenenum6 = true;
                    if (rp.IsCompleted("jarkos:VenenumNode7")) anyPlayerHasVenenum7 = true;
                }
            }

            if (anyPlayerHasVenenum6)
            {
                if (npc.velocity.Length() > 0.5f)
                {
                    npc.velocity *= 0.85f;
                }
            }

            if (anyPlayerHasVenenum7)
            {
                _poisonSpreadTimer++;
                if (_poisonSpreadTimer >= _nextSpreadTime)
                {
                    _poisonSpreadTimer = 0;
                    _nextSpreadTime = Main.rand.Next(180, 301);

                    bool extendVenom = npc.HasBuff(BuffID.Venom);
                    int buffToApply = extendVenom ? BuffID.Venom : BuffID.Poisoned;
                    int buffDuration = Main.rand.Next(120, 181);

                    float spreadRadiusSq = 200f * 200f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC target = Main.npc[i];
                        if (target.active && !target.friendly && target.whoAmI != npc.whoAmI && Vector2.DistanceSquared(npc.Center, target.Center) <= spreadRadiusSq)
                        {
                            target.AddBuff(buffToApply, buffDuration);

                            int dmg = Main.rand.Next(10, 21);
                            NPC.HitInfo hit = new NPC.HitInfo
                            {
                                Damage = dmg,
                                Knockback = 0f,
                                HitDirection = 0,
                                Crit = false
                            };
                            target.StrikeNPC(hit);

                            for (int d = 0; d < 5; d++)
                            {
                                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Venom, 0, -1f);
                                dust.noGravity = true;
                            }
                        }
                    }
                }
            }
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (npc.HasBuff(BuffID.Poisoned) || npc.HasBuff(BuffID.Venom))
            {
                var rp = target.GetModPlayer<ResearchPlayer>();
                if (rp.IsCompleted("jarkos:VenenumNode6"))
                {
                    modifiers.SourceDamage *= 0.90f;
                }
            }
        }
    }
}
