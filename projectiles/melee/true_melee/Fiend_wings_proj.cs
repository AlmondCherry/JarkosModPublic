using System;
using jarkos.particles;
using jarkos.particles.storm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class Fiend_wings_proj : ModProjectile {
		private const float HomingRange = 850f;
        private const float Speed = 6.5f;
        private const int HomingDelay = 18;
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float WavePhase => Projectile.ai[1];
        private static ReLogic.Content.Asset<Texture2D> trailGlow;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 36;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            if (!Main.dedServ)
                trailGlow = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/TrailGlow");
        }
        public override void Unload()
        {
            trailGlow = null;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.7f, 0.55f, 0.15f);
            NPC target = FindTarget();
            float distance = target != null ? Projectile.Distance(target.Center) : HomingRange;
            if (target != null && Timer > HomingDelay)
            {
                float currentAngle = Projectile.velocity.ToRotation();
                float targetAngle = (target.Center - Projectile.Center).ToRotation();
                float turnStrength = MathHelper.Lerp(0.018f, 0.055f, Terraria.Utils.GetLerpValue(HomingDelay, 150f, Timer, true));
                float newAngle = Terraria.Utils.AngleLerp(currentAngle, targetAngle, turnStrength);
                Projectile.velocity = newAngle.ToRotationVector2() * Speed;
            }
            float weaveStrength = 0.085f * Terraria.Utils.GetLerpValue(45f, 200f, distance, true);
            if (weaveStrength > 0.001f)
                Projectile.velocity = Projectile.velocity.RotatedBy(MathF.Sin(Timer * 0.11f + WavePhase) * weaveStrength);
            if (Timer % 4 == 0)
            {
                new Spark(Projectile.Center, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f), new Color(255, 210, 80), Main.rand.Next(14, 24), Main.rand.NextFloat(0.35f, 0.6f), 0.85f).Spawn();
            }
            if (Main.rand.NextBool(14))
            {
                new StarParticle(Projectile.Center, Main.rand.NextVector2Circular(1.2f, 1.2f), new Color(255, 235, 150), Main.rand.Next(12, 20), Main.rand.NextFloat(0.25f, 0.45f), 0.8f).Spawn();
            }
        }
        private NPC FindTarget()
        {
            NPC best = null;
            float bestDistance = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy(Projectile))
                    continue;
                float distance = Projectile.Distance(npc.Center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = npc;
                }
            }
            return best;
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.45f, Pitch = 0.5f }, Projectile.Center);
                new Circular_glow(Projectile.Center, Vector2.Zero, new Color(255, 220, 100), 16, 0.6f, 0.9f).Spawn();
                new Circleedge(Projectile.Center, Vector2.Zero, new Color(255, 200, 70), 18, 0.25f, 0.8f).Spawn();
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.6f, 1f);
                    new Energy_burst(Projectile.Center, vel, new Color(255, 215, 90), Main.rand.Next(14, 22), Main.rand.NextFloat(0.4f, 0.7f), 0.9f).Spawn();
                }
                for (int i = 0; i < 8; i++)
                {
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), new Color(255, 225, 110), Main.rand.Next(16, 28), Main.rand.NextFloat(0.4f, 0.8f), 0.95f).Spawn();
                }
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                    dust.noGravity = true;
                }
            }
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<EnergyRingProj>(), 0, 0f, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawHead();
            return false;
        }
        private void DrawTrail()
        {
            Texture2D glow = trailGlow?.Value;
            if (glow == null)
                return;
            Vector2 half = Projectile.Size * 0.5f;
            Vector2 glowOrigin = glow.Size() * 0.5f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            int length = Projectile.oldPos.Length;
            int steps = length * 2 - 1;
            for (int s = steps - 1; s >= 0; s--)
            {
                int i = s / 2;
                Vector2 posWorld;
                if ((s & 1) == 0)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero)
                        continue;
                    posWorld = Projectile.oldPos[i];
                }
                else
                {
                    if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                        continue;
                    posWorld = Vector2.Lerp(Projectile.oldPos[i], Projectile.oldPos[i + 1], 0.5f);
                }
                float t = s / (float)steps;
                float taper = MathF.Pow(1f - t, 1.45f);
                Vector2 pos = posWorld + half - Main.screenPosition;
                Main.spriteBatch.Draw(glow, pos, null, new Color(255, 195, 45) * (taper * 0.28f), 0f, glowOrigin, 0.85f * taper + 0.05f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(glow, pos, null, new Color(255, 250, 215) * (taper * taper * 0.3f), 0f, glowOrigin, 0.42f * taper + 0.03f, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawHead()
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float pulse = 1f + 0.12f * MathF.Sin(Main.GlobalTimeWrappedHourly * 12f + WavePhase);
            Main.EntitySpriteDraw(texture, drawPos, null, new Color(255, 200, 60, 0) * 0.55f, Projectile.rotation, origin, 1.7f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, new Color(255, 255, 255, 40), Projectile.rotation, origin, pulse, SpriteEffects.None, 0);
        }
	}
}
