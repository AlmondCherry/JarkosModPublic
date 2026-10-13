using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Blood_tromb_arrow : ModProjectile
	{
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.08f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(24))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 100, default, 1.2f);
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                Vector2 particleVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                new Electric_particle(Projectile.Center, particleVel, redColor, 18, Main.rand.NextFloat(0.25f, 0.4f), 1f, 0.93f).Spawn();
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.05f, 0.05f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int particleCount = Main.rand.Next(4, 9);
            for (int i = 0; i < particleCount; i++)
            {
                float xProgress = (float)i / Math.Max(1, particleCount - 1);
                float spawnX = MathHelper.Lerp(target.position.X, target.position.X + target.width, xProgress) + Main.rand.NextFloat(-2f, 2f);
                Vector2 spawnPos = new Vector2(spawnX, target.position.Y + target.height);

                float ascentSpeed = Main.rand.NextFloat(2f, 4.5f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -ascentSpeed);
                float scale = Main.rand.NextFloat(0.35f, 0.65f);
                int lifetime = Main.rand.Next(35, 55);

                new BloodAscentParticle(spawnPos, vel, new Color(230, 20, 40), lifetime, scale, target.position.Y).Spawn();
            }

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BloodHealBeam>(),
                    0,
                    0f,
                    Projectile.owner,
                    ai0: target.whoAmI,
                    ai1: target.Center.X,
                    ai2: target.Center.Y
                );
            }

            for (int i = 0; i < 1; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.4f);
                dust.noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                new Electric_particle(target.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.3f, 0.5f), 1f, 0.92f).Spawn();
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0f, -2f);
                Color redColor = Color.Lerp(new Color(255, 40, 40), new Color(200, 10, 10), Main.rand.NextFloat());
                new Spark_gravit(target.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.Item171 with { Volume = 0.6f, Pitch = 0.2f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D arrowTexture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = arrowTexture.Size() * 0.5f;

            if (glowTex != null && Projectile.velocity != Vector2.Zero)
            {
                SpriteBatch sb = Main.spriteBatch;
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 backOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 6f;
                Vector2 glowPos = drawPos + backOffset;
                Color softRedGlow = new Color(220, 20, 30) * 0.35f;

                sb.Draw(glowTex, glowPos, null, softRedGlow, 0f, glowTex.Size() * 0.5f, 0.35f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Main.spriteBatch.Draw(arrowTexture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 1; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-2.5f, 2.5f), 100, default, 1.2f);
                dust.noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                new Electric_particle(Projectile.Center, vel, redColor, Main.rand.Next(15, 25), Main.rand.NextFloat(0.25f, 0.45f), 1f, 0.92f).Spawn();
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }

	public class BloodHealBeam : ModProjectile
	{
		public override string Texture => "jarkos/projectiles/bullet/Bloody_bolt_impulse";
        private int animationTimer = 0;
        private const int MAX_LIFETIME = 90;

        private int TargetNPC => (int)Projectile.ai[0];
        private Vector2 FallbackStart => new Vector2(Projectile.ai[1], Projectile.ai[2]);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MAX_LIFETIME;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            animationTimer++;

            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[TargetNPC];
            Vector2 start = (target.active && target.whoAmI == TargetNPC) ? target.Center : FallbackStart;

            if (animationTimer == 1)
            {
                Projectile.Center = start;
            }

            Vector2 toPlayer = player.Center - Projectile.Center;
            float dist = toPlayer.Length();

            if (dist < 24f)
            {
                int healAmount = Main.rand.Next(1, 3);
                player.Heal(healAmount);
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.5f }, player.Center);

                for (int i = 0; i < 1; i++)
                {
                    Dust d = Dust.NewDustPerfect(player.Center, DustID.Blood, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);
                    d.noGravity = true;
                }

                Projectile.Kill();
                return;
            }

            float speed = MathHelper.Clamp(dist * 0.15f + 8f, 8f, 24f);
            Vector2 targetVel = toPlayer.SafeNormalize(Vector2.Zero) * speed;
            Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, targetVel, 0.3f);
            Projectile.Center += Projectile.velocity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active) return false;

            NPC target = Main.npc[TargetNPC];
            Vector2 start = (target.active && target.whoAmI == TargetNPC) ? target.Center : FallbackStart;
            Vector2 head = Projectile.Center;

            const int segments = 10;
            List<Vector2> points = new List<Vector2>(segments + 1);
            Vector2 diff = head - start;

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 pos = start + diff * t;
                float curve = MathF.Sin(t * MathF.PI) * 6f * MathF.Sin(animationTimer * 0.2f + i * 0.4f);
                Vector2 normal = new Vector2(-diff.Y, diff.X).SafeNormalize(Vector2.Zero);
                points.Add(pos + normal * curve);
            }

            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < points.Count - 1; i++)
            {
                float t0 = (float)i / (points.Count - 1);
                float t1 = (float)(i + 1) / (points.Count - 1);
                float tAvg = (t0 + t1) * 0.5f;

                float outerWidth = MathHelper.Lerp(14f, 2.5f, tAvg);
                float coreWidth  = MathHelper.Lerp(5.5f, 1.0f, tAvg);
                float innerWidth = MathHelper.Lerp(2.2f, 0.5f, tAvg);

                Color outerColor = new Color(220, 20, 40) * (0.45f * (1f - tAvg * 0.3f));
                Color coreColor  = new Color(255, 80, 100) * (0.85f * (1f - tAvg * 0.2f));
                Color whiteCore  = Color.White * 0.95f;

                DrawSegmentLine(sb, pixel, points[i], points[i + 1], outerWidth, outerColor);
                DrawSegmentLine(sb, pixel, points[i], points[i + 1], coreWidth, coreColor);
                DrawSegmentLine(sb, pixel, points[i], points[i + 1], innerWidth, whiteCore);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 0.1f) return;
            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
