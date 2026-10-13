using System;
using System.Collections.Generic;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.systems;
namespace jarkos.projectiles.melee.unique_melee
{
	public class Lightbolt_sb : ModProjectile {
        private List<Vector2> chainPoints = new List<Vector2>();
        private List<NPC> hitNPCs = new List<NPC>();
        private const int MAX_CHAIN = 4;
        private const float MAX_CHAIN_DIST = 600f;
        private const int LINGER_TIME = 20;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LINGER_TIME;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                BuildChain();
            }
            foreach (Vector2 pt in chainPoints)
            {
                if (Main.rand.NextBool(5))
                {
                    Lighting.AddLight(pt, 0.1f, 0.4f, 0.7f);
                }
            }
        }
        private void BuildChain()
        {
            Vector2 currentPos = Projectile.Center;
            int firstTargetIndex = (int)Projectile.ai[0];
            NPC firstTarget = null;
            if (firstTargetIndex >= 0 && firstTargetIndex < Main.maxNPCs)
            {
                firstTarget = Main.npc[firstTargetIndex];
            }
            if (firstTarget != null && firstTarget.active)
            {
                AddSegmentAndHit(ref currentPos, firstTarget);
                int chained = 1;
                while (chained < MAX_CHAIN)
                {
                    NPC nextTarget = FindNextClosest(currentPos, MAX_CHAIN_DIST);
                    if (nextTarget != null)
                    {
                        AddSegmentAndHit(ref currentPos, nextTarget);
                        chained++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                Vector2 endPos = currentPos + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 300f;
                AddJaggedSegment(currentPos, endPos);
            }
        }
        private void AddSegmentAndHit(ref Vector2 currentPos, NPC target)
        {
            AddJaggedSegment(currentPos, target.Center);
            currentPos = target.Center;
            hitNPCs.Add(target);
            NPC.HitInfo hitInfo = new NPC.HitInfo
            {
                Damage = Projectile.damage, Knockback = Projectile.knockBack, HitDirection = Math.Sign(target.Center.X - Projectile.Center.X), Crit = Main.rand.Next(100) < Main.player[Projectile.owner].GetTotalCritChance(DamageClass.Melee)
            };
            target.StrikeNPC(hitInfo);
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendStrikeNPC(target, hitInfo);
            }
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = Main.rand.NextFloat(0.3f, 0.7f), Volume = 0.7f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = Main.rand.NextFloat(0.5f, 0.8f), Volume = 0.5f }, target.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.Electric, Main.rand.NextVector2Circular(8f, 8f), 0, Color.Cyan, 1.2f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(5f, 10f);
                new Spark(target.Center, v, Main.rand.NextBool() ? Color.White : Color.Cyan, 16, 0.3f, 0.8f).Spawn();
            }
        }
        private void AddJaggedSegment(Vector2 start, Vector2 end)
        {
            float distance = Vector2.Distance(start, end);
            int segments = (int)(distance / 25f);
            segments = Math.Max(segments, 2);
            Vector2 dir = (end - start).SafeNormalize(Vector2.Zero);
            Vector2 normal = new Vector2(-dir.Y, dir.X);
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 pos = Vector2.Lerp(start, end, t);
                if (i > 0 && i < segments)
                {
                    float zigzagK = 1f - Math.Abs(t - 0.5f) * 2f;
                    pos += normal * Main.rand.NextFloat(-20f, 20f) * zigzagK;
                }
                if (chainPoints.Count == 0 || Vector2.DistanceSquared(chainPoints[^1], pos) > 1f)
                {
                    chainPoints.Add(pos);
                }
            }
        }
        private NPC FindNextClosest(Vector2 position, float maxDist)
        {
            NPC closest = null;
            float minDistSq = maxDist * maxDist;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.CanBeChasedBy() && !hitNPCs.Contains(npc))
                {
                    float distSq = Vector2.DistanceSquared(npc.Center, position);
                    if (distSq < minDistSq)
                    {
                        if (Collision.CanHitLine(position, 1, 1, npc.position, npc.width, npc.height))
                        {
                            minDistSq = distSq;
                            closest = npc;
                        }
                    }
                }
            }
            return closest;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (chainPoints.Count < 2) return false;
            Effect effect = HealShaders.Stream;
            if (effect != null && HealShaders.HealNoise != null)
            {
                Main.spriteBatch.End();
                float progress = Projectile.timeLeft / (float)LINGER_TIME;
                float opacity = MathF.Pow(progress, 0.5f);
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f);
                effect.Parameters["uOpacity"]?.SetValue(opacity);
                effect.Parameters["uColor"]?.SetValue(new Color(0, 150, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 255).ToVector4());
                Vector2[] points = chainPoints.ToArray();
                TrailRenderer.Draw(effect, points, t => 16f * opacity * (1f + Main.rand.NextFloat(-0.1f, 0.1f)), t => Color.White * opacity, HealShaders.HealNoise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                SpriteBatch sb = Main.spriteBatch;
                Texture2D px = TextureAssets.MagicPixel.Value;
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float progress = Projectile.timeLeft / (float)LINGER_TIME;
                for (int i = 0; i < chainPoints.Count - 1; i++)
                {
                    Vector2 p1 = chainPoints[i] - Main.screenPosition;
                    Vector2 p2 = chainPoints[i + 1] - Main.screenPosition;
                    Vector2 dir = p2 - p1;
                    float len = dir.Length();
                    if (len == 0) continue;
                    float rot = dir.ToRotation();
                    sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), Color.Cyan * progress, rot, new Vector2(0, 0.5f), new Vector2(len, 6f * progress), SpriteEffects.None, 0f);
                    sb.Draw(px, p1, new Rectangle(0, 0, 1, 1), Color.White * progress, rot, new Vector2(0, 0.5f), new Vector2(len, 2f * progress), SpriteEffects.None, 0f);
                }
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
	}
    public class MetalEnemySystem : GlobalNPC
    {
        private static HashSet<int> metallicNPCs = new HashSet<int> 
        {
            NPCID.TheDestroyer, 
            NPCID.TheDestroyerBody, 
            NPCID.TheDestroyerTail, 
            NPCID.SkeletronPrime, 
            NPCID.PrimeCannon, 
            NPCID.PrimeLaser, 
            NPCID.PrimeSaw, 
            NPCID.PrimeVice, 
            NPCID.Retinazer, 
            NPCID.Spazmatism, 
            NPCID.Probe, 
            NPCID.MartianSaucer, 
            NPCID.MartianSaucerCannon, 
            NPCID.MartianSaucerCore, 
            NPCID.MartianSaucerTurret, 
            NPCID.MartianDrone, 
            NPCID.MartianEngineer, 
            NPCID.MartianOfficer, 
            NPCID.MartianWalker, 
            NPCID.Scutlix, 
            NPCID.ScutlixRider, 
            NPCID.GigaZapper, 
            NPCID.MartianTurret, 
            NPCID.PirateCaptain, 
            NPCID.PirateCrossbower, 
            NPCID.PirateDeadeye, 
            NPCID.RustyArmoredBonesAxe, 
            NPCID.RustyArmoredBonesFlail, 
            NPCID.RustyArmoredBonesSword, 
            NPCID.RustyArmoredBonesSwordNoArmor, 
            NPCID.BlueArmoredBones, 
            NPCID.BlueArmoredBonesMace, 
            NPCID.BlueArmoredBonesNoPants, 
            NPCID.BlueArmoredBonesSword, 
            NPCID.HellArmoredBones, 
            NPCID.HellArmoredBonesMace, 
            NPCID.HellArmoredBonesSword, 
            NPCID.HellArmoredBonesSpikeShield, 
            NPCID.AngryNimbus, 
            NPCID.SolarCorite, 
            NPCID.SolarSolenian,
            NPCID.SolarDrakomire, 
            NPCID.VortexHornet, 
            NPCID.VortexLarva, 
            NPCID.VortexRifleman, 
            NPCID.VortexSoldier, 
            NPCID.Paladin, 
            NPCID.SkeletonSniper, 
            NPCID.SkeletonCommando, 
            NPCID.TacticalSkeleton, 
            };

        public static bool IsMetallic(NPC npc)
        {
            if (metallicNPCs.Contains(npc.type))
                return true;
            string name = npc.TypeName.ToLower();
            if (name.Contains("mech") || name.Contains("robot") || name.Contains("metal") || name.Contains("iron") || name.Contains("steel") || name.Contains("armor") || name.Contains("drone") || name.Contains("cyber"))
            {
                return true;
            }
            return false;
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (IsMetallic(npc) && npc.life <= 0)
            {
                if (projectile.type == ModContent.ProjectileType<Wordofberserker_proj>() || projectile.type == ModContent.ProjectileType<Lightbolt_sb>())
                {
                    for (int i = 0; i < 40; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Electric, Main.rand.NextVector2Circular(10f, 10f), 0, default, 1.5f);
                        dust.noGravity = true;
                    }
                }
            }
        }
    }
}
