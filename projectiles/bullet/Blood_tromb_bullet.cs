using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ReLogic.Content;
using Terraria.GameContent;
using jarkos.particles;
using System;

namespace jarkos.projectiles.bullet
{
	public class Blood_tromb_bullet : ModProjectile
	{
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.CloneDefaults(ProjectileID.Bullet);
			AIType = ProjectileID.Bullet;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 400;
            Projectile.extraUpdates = 1;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(16))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            if (Main.rand.NextBool(6))
            {
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 10, 10), Main.rand.NextFloat());
                Vector2 particleVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                new Electric_particle(Projectile.Center, particleVel, redColor, 18, Main.rand.NextFloat(0.25f, 0.4f), 1f, 0.93f).Spawn();
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.08f, 0.08f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = new Color(220, 20, 20) * (1f - progress) * 0.6f;
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                sb.Draw(texture, drawPos, null, trailColor, Projectile.rotation, texture.Size() / 2, Projectile.scale * (1f - progress * 0.3f), SpriteEffects.None, 0);
            }

            if (glowTex != null)
            {
                Vector2 headPos = Projectile.Center - Main.screenPosition;
                sb.Draw(glowTex, headPos, null, new Color(240, 20, 20) * 0.5f, 0f, glowTex.Size() * 0.5f, 0.4f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 bulletPos = Projectile.Center - Main.screenPosition;
            sb.Draw(texture, bulletPos, null, Color.White, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 18f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<Bloody_piercing_stream>(), (int)(Projectile.damage * 0.8f), Projectile.knockBack * 0.5f, Projectile.owner);
            }

            for (int i = 0; i < 1; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, Projectile.velocity.X * 0.2f + Main.rand.NextFloat(-2f, 2f), Projectile.velocity.Y * 0.2f + Main.rand.NextFloat(-2f, 2f), 100, default, 1.3f);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                new Spark_gravit(target.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.3f, 0.5f), 1f, 0.92f).Spawn();
            }

            SoundEngine.PlaySound(SoundID.NPCHit18, target.position);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 1; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1f);
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }

	public class Bloody_piercing_stream : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/bullet/Burst_hitbox";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.light = 0.6f;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.position + Projectile.velocity * Main.rand.NextFloat(-0.5f, 0f);
                Dust dust = Dust.NewDustDirect(dustPos, Projectile.width, Projectile.height, DustID.Blood, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 120, default, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            if (Main.rand.NextBool())
            {
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                Vector2 particleVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                new Spark(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), particleVel, redColor, 16, Main.rand.NextFloat(0.3f, 0.5f), 1f, 0.92f).Spawn();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.7f, 0.08f, 0.08f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

            List<Vector2> points = new List<Vector2>();
            points.Add(Projectile.Center);
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                points.Add(Projectile.oldPos[i] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f));
            }

            if (points.Count >= 2)
            {
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 origin = new Vector2(0f, 0.5f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < points.Count - 1; i++)
                {
                    float progress = (float)i / (points.Count - 1);
                    float alpha = (1f - progress) * Projectile.Opacity;
                    Vector2 start = points[i] - Main.screenPosition;
                    Vector2 end = points[i + 1] - Main.screenPosition;
                    Vector2 diff = end - start;
                    float length = diff.Length();
                    if (length < 0.1f) continue;
                    float rotation = diff.ToRotation();

                    float redWidth = 10f * (1f - progress * 0.6f) * Projectile.scale;
                    Color redColor = new Color(220, 20, 20) * (0.8f * alpha);

                    sb.Draw(magicPixel, start, sourceRect, redColor, rotation, origin, new Vector2(length, redWidth), SpriteEffects.None, 0f);
                }

                for (int i = 0; i < points.Count - 1; i++)
                {
                    float progress = (float)i / (points.Count - 1);
                    float alpha = (1f - progress) * Projectile.Opacity;
                    Vector2 start = points[i] - Main.screenPosition;
                    Vector2 end = points[i + 1] - Main.screenPosition;
                    Vector2 diff = end - start;
                    float length = diff.Length();
                    if (length < 0.1f) continue;
                    float rotation = diff.ToRotation();

                    float whiteWidth = 3.5f * (1f - progress * 0.5f) * Projectile.scale;
                    Color whiteColor = new Color(255, 235, 235) * (0.9f * alpha);

                    sb.Draw(magicPixel, start, sourceRect, whiteColor, rotation, origin, new Vector2(length, whiteWidth), SpriteEffects.None, 0f);
                }

                if (glowTex != null)
                {
                    Vector2 headPos = Projectile.Center - Main.screenPosition;
                    Vector2 glowOrigin = glowTex.Size() * 0.5f;
                    sb.Draw(glowTex, headPos, null, new Color(255, 30, 30) * 0.5f, 0f, glowOrigin, 0.4f * Projectile.scale, SpriteEffects.None, 0f);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustVel = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0.2f, 0.5f);
                Dust dust = Dust.NewDustDirect(target.Center - new Vector2(4, 4), 8, 8, DustID.Blood, dustVel.X + Main.rand.NextFloat(-3f, 3f), dustVel.Y + Main.rand.NextFloat(-3f, 3f), 100, default, 1.4f);
                dust.noGravity = Main.rand.NextBool();
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6.5f, 6.5f);
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                new Electric_particle(target.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.05f, 0.25f), 1f, 0.92f).Spawn();
            }

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f) + new Vector2(0f, -2.5f);
                Color redColor = Color.Lerp(new Color(255, 40, 40), new Color(200, 10, 10), Main.rand.NextFloat());
                new Spark_gravit(target.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.7f }, target.position);
            Projectile.damage = (int)(Projectile.damage * 0.9f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 2; i++)
            {
                float angle = MathHelper.TwoPi * i / 2f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle) * 4f, (float)Math.Sin(angle) * 4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.Blood, velocity.X, velocity.Y, 100, default, 1.4f);
                dust.noGravity = false;
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Color redColor = Color.Lerp(new Color(255, 30, 30), new Color(180, 0, 0), Main.rand.NextFloat());
                new Electric_particle(Projectile.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.35f, 0.55f), 1f, 0.92f).Spawn();
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0f, -2f);
                Color redColor = Color.Lerp(new Color(255, 40, 40), new Color(200, 10, 10), Main.rand.NextFloat());
                new Spark_gravit(Projectile.Center, vel, redColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 1; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, -oldVelocity.X * 0.3f + Main.rand.NextFloat(-2f, 2f), -oldVelocity.Y * 0.3f + Main.rand.NextFloat(-2f, 2f), 100, default, 1.3f);
            }
            return true;
        }
    }
}
