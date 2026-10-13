using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.DataStructures;
using jarkos.systems;

namespace jarkos.projectiles.ranged.firearm
{
	public class Ultimatum_bullet : ModProjectile
	{
        private bool IsEmpowered => Projectile.ai[0] == 1f;
        private bool IsChained => Projectile.ai[1] == 1f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MAX_TRAIL_LENGTH = 25;
        private float glowPulse = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void AI()
        {
            glowPulse += 0.15f;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MAX_TRAIL_LENGTH)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (IsChained)
            {
                ChainedHoming();
            }
            SpawnFlightDust();
            float baseLight = IsEmpowered ? 0.8f : 0.5f;
            Projectile.light = baseLight + (float)Math.Sin(glowPulse) * 0.15f;
        }
        private void ChainedHoming()
        {
            float homingRange = 300f;
            float homingStrength = 0.06f;
            NPC closest = null;
            float closestDist = homingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            if (closest != null)
            {
                Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float currentAngle = Projectile.velocity.ToRotation();
                float targetAngle = toTarget.ToRotation();
                float newAngle = currentAngle + MathHelper.WrapAngle(targetAngle - currentAngle) * homingStrength;
                float speed = Projectile.velocity.Length();
                Projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * speed;
            }
        }
        private void SpawnFlightDust()
        {
            if (IsEmpowered)
            {
                if (Main.rand.NextBool(10))
                {
                    Dust d2 = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 0, 0, DustID.WhiteTorch, 0, 0, 0, default, 0.5f);
                    d2.noGravity = true;
                    d2.velocity *= 0.05f;
                }
            }
            else if (IsChained)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0, 0, 60, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.1f;
                if (Main.rand.NextBool(3))
                {
                    Dust d2 = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, 0, 0, 0, default, 0.2f);
                    d2.noGravity = true;
                    d2.velocity *= 0.05f;
                }
            }
            else
            {
                if (Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, 0, 0, 80, default, 0.2f);
                    d.noGravity = true;
                    d.velocity *= 0.05f;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                int dustType = IsEmpowered ? DustID.WhiteTorch : DustID.Electric;
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 0, default, 0.4f);
                d.noGravity = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsEmpowered)
            {
                SpawnExplosion();
            }
            SpawnImpactDust(target.Center);
            if (target.life <= 0 && Main.myPlayer == Projectile.owner)
            {
                SpawnChainProjectile(target.Center);
            }
        }
        private void SpawnExplosion()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ultimatum_eff>(), (int)(Projectile.damage * 0.7f), Projectile.knockBack * 0.5f, Projectile.owner);
            }
        }
        private void SpawnChainProjectile(Vector2 fromPos)
        {
            float searchRange = 400f;
            int bestTarget = -1;
            float bestDist = searchRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(fromPos, npc.Center);
                if (dist < bestDist && dist > 20f)
                {
                    bestDist = dist;
                    bestTarget = i;
                }
            }
            if (bestTarget != -1)
            {
                NPC target = Main.npc[bestTarget];
                Vector2 dir = (target.Center - fromPos).SafeNormalize(Vector2.UnitX);
                Vector2 vel = dir * 16f;
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), fromPos, vel, Projectile.type, (int)(Projectile.damage * 0.8f), Projectile.knockBack * 0.5f, Projectile.owner);
                if (proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[0] = 0f;
                    Main.projectile[proj].ai[1] = 1f;
                }
                SpawnChainDust(fromPos, target.Center);
                SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.5f, Pitch = 0.3f }, fromPos);
            }
        }
        private void SpawnChainDust(Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            float dist = dir.Length();
            dir.Normalize();
            int steps = (int)(dist / 8f);
            for (int i = 0; i < steps; i++)
            {
                float progress = (float)i / steps;
                Vector2 pos = Vector2.Lerp(from, to, progress);
                pos += Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.GoldFlame, 0, 0, 0, default, 0.3f);
                d.noGravity = true;
                d.velocity *= 0.05f;
                d.fadeIn = 1f;
            }
        }
        private void SpawnImpactDust(Vector2 pos)
        {
            int count = IsEmpowered ? 8 : 4;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.3f, 1f);
                int dustType = IsEmpowered ? DustID.Electric : IsChained ? DustID.Electric : DustID.Electric;
                float scale = IsEmpowered ? 0.5f : 0.2f;
                Dust d = Dust.NewDustDirect(pos, 0, 0, dustType, vel.X, vel.Y, 0, default, scale);
                d.noGravity = true;
                d.fadeIn = scale + 0.3f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 projOrigin = projTexture.Size() * 0.5f;
            if (IsEmpowered)
            {
                if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
                {
                    Effect effect = HealShaders.Stream;
                    Main.spriteBatch.End();
                    try
                    {
                        effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                        effect.Parameters["uOpacity"]?.SetValue(1.5f);
                        effect.Parameters["uColor"]?.SetValue(new Color(50, 150, 255).ToVector4());
                        effect.Parameters["uColorCore"]?.SetValue(new Color(200, 240, 255).ToVector4());
                        TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(14f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                    }
                    finally
                    {
                        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }
            }
            else
            {
                DrawPlasmaTrail(magicPixel);
            }
            DrawProjectileGlow(projTexture, projOrigin);
            return false;
        }
        private void DrawPlasmaTrail(Texture2D magicPixel)
        {
            if (trailPositions.Count < 2) return;
            Color innerColor, outerColor, coreColor;
            if (IsEmpowered)
            {
                coreColor = Color.White;
                innerColor = new Color(100, 180, 255);
                outerColor = new Color(30, 80, 200);
            }
            else if (IsChained)
            {
                coreColor = new Color(255, 255, 200);
                innerColor = new Color(255, 160, 40);
                outerColor = new Color(255, 60, 20);
            }
            else
            {
                coreColor = new Color(200, 230, 255);
                innerColor = new Color(80, 150, 255);
                outerColor = new Color(30, 60, 180);
            }
            DrawTrailLayer(magicPixel, outerColor * 0.15f, 12f, 0.6f);
            DrawTrailLayer(magicPixel, outerColor * 0.25f, 8f, 0.7f);
            DrawTrailLayer(magicPixel, innerColor * 0.5f, 5f, 0.8f);
            DrawTrailLayer(magicPixel, innerColor * 0.7f, 3f, 0.9f);
            DrawTrailLayer(magicPixel, coreColor * 0.9f, 1.5f, 1f);
        }
        private void DrawTrailLayer(Texture2D pixel, Color color, float width, float alphaMultiplier)
        {
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float segmentAlpha = (1f - progress) * alphaMultiplier;
                float segmentWidth = width * (1f - progress * 0.8f);
                float pulse = 1f + (float)Math.Sin(glowPulse * 3f + i * 0.5f) * 0.15f;
                segmentWidth *= pulse;
                Vector2 diff = end - start;
                float length = diff.Length();
                if (length < 0.5f) continue;
                float rotation = diff.ToRotation();
                Color segColor = color * segmentAlpha;
                segColor.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), segColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, segmentWidth), SpriteEffects.None, 0);
            }
        }
        private void DrawProjectileGlow(Texture2D texture, Vector2 origin)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color innerColor, outerColor;
            if (IsEmpowered)
            {
                outerColor = new Color(80, 160, 255);
                innerColor = Color.White;
            }
            else if (IsChained)
            {
                outerColor = new Color(255, 120, 30);
                innerColor = new Color(255, 240, 180);
            }
            else
            {
                outerColor = new Color(60, 120, 255);
                innerColor = new Color(180, 220, 255);
            }
            float pulse = 1f + (float)Math.Sin(glowPulse * 2f) * 0.1f;
            Color outer = outerColor * 0.2f;
            outer.A = 0;
            Main.EntitySpriteDraw(texture, pos, null, outer, Projectile.rotation, origin, 3f * pulse, SpriteEffects.None, 0);
            Color mid = outerColor * 0.4f;
            mid.A = 0;
            Main.EntitySpriteDraw(texture, pos, null, mid, Projectile.rotation, origin, 2f * pulse, SpriteEffects.None, 0);
            Color main = innerColor;
            main.A = 200;
            Main.EntitySpriteDraw(texture, pos, null, main, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            Color core = Color.White * 0.8f;
            core.A = 0;
            Main.EntitySpriteDraw(texture, pos, null, core, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0);
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Color dotColor = Color.White;
            dotColor.A = 0;
            float dotSize = (IsEmpowered ? 6f : 4f) * pulse;
            Main.EntitySpriteDraw(magicPixel, pos - new Vector2(dotSize * 0.5f, dotSize * 0.5f), new Rectangle(0, 0, 1, 1), dotColor, 0f, Vector2.Zero, dotSize, SpriteEffects.None, 0);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (IsEmpowered)
                return new Color(150, 210, 255, 180);
            if (IsChained)
                return new Color(255, 180, 80, 180);
            return new Color(120, 180, 255, 180);
        }
    }
}
