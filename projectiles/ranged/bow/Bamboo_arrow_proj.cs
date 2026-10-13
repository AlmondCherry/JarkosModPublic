using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
namespace jarkos.projectiles.ranged.bow
{
    public class Bamboo_arrow_proj : ModProjectile
    {
        private enum ArrowState { Flying = 0, StuckInEnemy = 1 }
        private ArrowState CurrentState
        {
            get => (ArrowState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        private int TargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private ArrowState oldState = ArrowState.Flying;
        private Vector2 stuckOffset;
        private int timer = 0;
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 1200;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (CurrentState == ArrowState.StuckInEnemy)
                return false;
            return base.CanHitNPC(target);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 0f;
            modifiers.HideCombatText();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentState == ArrowState.Flying)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    CurrentState = ArrowState.StuckInEnemy;
                    TargetWhoAmI = target.whoAmI;
                    Projectile.netUpdate = true;
                }
            }
        }
        public override bool PreAI()
        {
            if (CurrentState != oldState)
            {
                if (CurrentState == ArrowState.StuckInEnemy)
                {
                    if (TargetWhoAmI >= 0 && TargetWhoAmI < Main.maxNPCs && Main.npc[TargetWhoAmI].active)
                    {
                        stuckOffset = Projectile.position - Main.npc[TargetWhoAmI].position;
                    }
                    Projectile.velocity = Vector2.Zero;
                    Projectile.penetrate = -1;
                    Projectile.timeLeft = 600;
                    Projectile.aiStyle = 0;
                    Projectile.tileCollide = false;
                }
                oldState = CurrentState;
            }

            if (CurrentState == ArrowState.StuckInEnemy)
            {
                int npcIndex = TargetWhoAmI;
                if (npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active && !Main.npc[npcIndex].dontTakeDamage)
                {
                    NPC target = Main.npc[npcIndex];
                    Projectile.position = target.position + stuckOffset;
                    Projectile.gfxOffY = target.gfxOffY;
                    timer++;
                    if (timer >= 60)
                    {
                        timer = 0;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            int stuckCount = 0;
                            for (int i = 0; i < Main.maxProjectiles; i++)
                            {
                                Projectile p = Main.projectile[i];
                                if (p.active && p.type == Projectile.type)
                                {
                                    if (p.ai[0] == (int)ArrowState.StuckInEnemy && (int)p.ai[1] == npcIndex)
                                    {
                                        stuckCount++;
                                    }
                                }
                            }
                            int penaltyPercent = 0;
                            if (stuckCount > 1)
                            {
                                penaltyPercent = stuckCount * 5;
                                if (penaltyPercent > 50) penaltyPercent = 50;
                            }
                            float damageMult = 1f - (penaltyPercent / 100f);
                            int finalDamage = (int)(Projectile.damage * damageMult);
                            NPC.HitInfo hitInfo = new NPC.HitInfo()
                            {
                                Damage = finalDamage, Knockback = 0f, HitDirection = 0
                            };
                            target.StrikeNPC(hitInfo);
                            if (Main.netMode != NetmodeID.SinglePlayer)
                                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
                        }
                    }
                }
                else
                {
                    Projectile.Kill();
                }
                return false;
            }
            return true;
        }
    }
}
