using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Laser_beam_arrow : ModProjectile
	{
        private const int MAX_BOUNCES = 5;
        private const int TRAIL_LENGTH = 30;
        private const float BEAM_WIDTH = 6f;
        private const float BEAM_SPEED = 18f;
        private const int LIFETIME = 300;

        private ref float BounceCount => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.localAI[0];

        private Vector2[] trailPos = new Vector2[TRAIL_LENGTH];
        private bool trailInit = false;

        private static readonly Color CoreColor = new(255, 255, 255);
        private static readonly Color InnerColor = new(100, 200, 255);
        private static readonly Color OuterColor = new(40, 120, 255);

        private Color CurrentInner => Color.Lerp(InnerColor, new Color(150, 100, 255), BounceCount / MAX_BOUNCES);
        private Color CurrentOuter => Color.Lerp(OuterColor, new Color(100, 50, 220), BounceCount / MAX_BOUNCES);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = LIFETIME;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
            Projectile.light = 0f;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            UpdateTrail();
            Lighting.AddLight(Projectile.Center, CurrentInner.ToVector3() * 0.5f);

            if (Main.rand.NextBool(2))
            {
                Color particleCol = Color.Lerp(CurrentInner, CurrentOuter, Main.rand.NextFloat());
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                new Spark(Projectile.Center, vel, particleCol, 16, Main.rand.NextFloat(0.3f, 0.5f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            if (Main.rand.NextBool(4))
            {
                Color particleCol = Color.Lerp(CurrentOuter, Color.Purple, Main.rand.NextFloat());
                new Solid_particle(Projectile.Center, -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f), particleCol, 12, Main.rand.NextFloat(0.3f, 0.5f), 1f, Main.rand.NextFloat(-0.08f, 0.08f)).Spawn();
            }

            if (Projectile.timeLeft < 20)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 20f));
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (BounceCount >= MAX_BOUNCES)
            {
                SpawnBounceEffect(true);
                return true;
            }
            BounceCount++;

            if (MathF.Abs(Projectile.velocity.X - oldVelocity.X) > 0.1f)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (MathF.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0.1f)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * BEAM_SPEED;

            SpawnBounceEffect(false);
            float pitch = -0.2f + BounceCount * 0.15f;
            SoundEngine.PlaySound(SoundID.Item10 with
            {
                Volume = 0.4f, Pitch = MathHelper.Clamp(pitch, -0.3f, 0.6f)
            }, Projectile.Center);
            return false;
        }

        private void SpawnBounceEffect(bool isFinal)
        {
            int count = isFinal ? 8 : 4;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(CurrentInner, CurrentOuter, Main.rand.NextFloat());
                new Spark(Projectile.Center, vel, col, Main.rand.Next(15, 25), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            Lighting.AddLight(Projectile.Center, CurrentInner.ToVector3() * 1.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f);
                Color col = Color.Lerp(CurrentInner, CurrentOuter, Main.rand.NextFloat());
                new Spark(target.Center, vel, col, Main.rand.Next(18, 30), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0f, -1.5f);
                Color col = Color.Lerp(CurrentOuter, Color.Purple, Main.rand.NextFloat());
                new Spark_gravit(target.Center, vel, col, Main.rand.Next(18, 30), Main.rand.NextFloat(0.35f, 0.55f), 1f).Spawn();
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Color col = Color.Lerp(CurrentInner, Color.Cyan, Main.rand.NextFloat());
                new Solid_particle(target.Center, vel, col, Main.rand.Next(14, 22), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.Item94 with
            {
                Volume = 0.25f, Pitch = 0.5f
            }, target.Center);

            if (Projectile.ai[1] != 1f && Main.rand.NextFloat() < 0.20f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawnPos = target.Center + angle.ToRotationVector2() * Main.rand.NextFloat(130f, 170f);

                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<ChronoPortal>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: target.whoAmI);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = Color.Lerp(CurrentInner, CurrentOuter, Main.rand.NextFloat());
                new Spark(Projectile.Center, vel, col, Main.rand.Next(18, 30), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }

        private void UpdateTrail()
        {
            if (!trailInit)
            {
                for (int i = 0; i < TRAIL_LENGTH; i++)
                    trailPos[i] = Projectile.Center;
                trailInit = true;
            }

            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
                trailPos[i] = trailPos[i - 1];
            trailPos[0] = Projectile.Center;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float opacity = 1f - Projectile.alpha / 255f;

            if (opacity <= 0.01f) return false;
            SpriteBatch sb = Main.spriteBatch;

            DrawBeamTrail(sb, opacity);
            DrawBeamHead(sb, opacity);

            return false;
        }

        private void DrawBeamTrail(SpriteBatch sb, float opacity)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 1; i < TRAIL_LENGTH; i++)
            {
                Vector2 curr = trailPos[i - 1];
                Vector2 prev = trailPos[i];
                if (curr == prev) continue;

                float progress = (float)i / TRAIL_LENGTH;
                Vector2 diff = curr - prev;
                float segLen = diff.Length();
                float rot = diff.ToRotation();
                float segAlpha = (1f - progress) * opacity;
                float pulse = 1f + MathF.Sin(Timer * 0.15f + i * 0.6f) * 0.12f;
                float outerWidth = BEAM_WIDTH * 3.5f * (1f - progress * 0.5f) * pulse;

                Color outerCol = CurrentOuter * segAlpha * 0.2f;

                sb.Draw(pixel, curr - Main.screenPosition, new Rectangle(0, 0, 1, 1), outerCol, rot, new Vector2(0.5f, 0.5f), new Vector2(segLen, outerWidth), SpriteEffects.None, 0f);
                float midWidth = BEAM_WIDTH * 1.4f * (1f - progress * 0.4f) * pulse;
                Color midCol = CurrentInner * segAlpha * 0.5f;

                sb.Draw(pixel, curr - Main.screenPosition, new Rectangle(0, 0, 1, 1), midCol, rot, new Vector2(0.5f, 0.5f), new Vector2(segLen, midWidth), SpriteEffects.None, 0f);
                float coreWidth = BEAM_WIDTH * 0.4f * (1f - progress * 0.6f) * pulse;
                Color coreCol = CoreColor * segAlpha * 0.7f;

                sb.Draw(pixel, curr - Main.screenPosition, new Rectangle(0, 0, 1, 1), coreCol, rot, new Vector2(0.5f, 0.5f), new Vector2(segLen, coreWidth), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBeamHead(SpriteBatch sb, float opacity)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float headPulse = 1f + MathF.Sin(Timer * 0.2f) * 0.15f;
            float outerSize = 18f * headPulse * opacity;

            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), CurrentOuter * 0.3f * opacity, Projectile.rotation + MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(outerSize, outerSize), SpriteEffects.None, 0f);
            float midSize = 10f * headPulse * opacity;

            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), CurrentInner * 0.5f * opacity, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(midSize, midSize), SpriteEffects.None, 0f);
            float coreSize = 4f * headPulse * opacity;

            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), CoreColor * 0.8f * opacity, 0f, new Vector2(0.5f, 0.5f), new Vector2(coreSize, coreSize), SpriteEffects.None, 0f);
            float flareLen = 14f * headPulse * opacity;
            float flareWidth = 3f * opacity;
            
            sb.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), CoreColor * 0.4f * opacity, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(flareLen, flareWidth), SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
