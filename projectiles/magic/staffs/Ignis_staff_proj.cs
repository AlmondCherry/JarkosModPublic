using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.staffs
{
    public class Ignis_staff_proj : ModProjectile
    {
        private float DropletScale
        {
            get => Projectile.ai[0] == 0 ? 1f : Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private int DropletVariant => (int)Projectile.ai[1];
        private float LifeProgress => (float)Projectile.timeLeft / 180f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int TRAIL_LENGTH = 12;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            UpdateTrail();
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;


            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            Projectile.velocity.X *= 0.995f;
            CreateFireParticles();
            UpdateLighting();

            if (Projectile.wet)
            {
                CreateSteamEffect();
                Projectile.velocity *= 0.9f;
                Projectile.timeLeft -= 3;
            }
        }

        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            while (trailPositions.Count > TRAIL_LENGTH)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }

        private void CreateFireParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4), DustID.Torch, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 0, default, Main.rand.NextFloat(1.2f, 2f) * DropletScale);
                fire.noGravity = true;
                fire.fadeIn = 0.8f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3, 3), DustID.Smoke, -Projectile.velocity * 0.1f + new Vector2(0, -1f), 100, Color.DarkGray, Main.rand.NextFloat(0.5f, 1f));
                smoke.noGravity = true;
            }
            if (Main.rand.NextBool(6))
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.Lava, Projectile.velocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.2f, 0.5f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                Spark.noGravity = false;
            }
            if (Main.rand.NextBool(5))
            {
                Dust magic = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), DustID.InfernoFork, Main.rand.NextVector2Circular(2, 2), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                magic.noGravity = true;
            }
        }

        private void UpdateLighting()
        {
            float flicker = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.5f + Projectile.whoAmI) * 0.1f;
            float intensity = DropletScale * flicker * LifeProgress;
            Lighting.AddLight(Projectile.Center, 1f * intensity, 0.5f * intensity, 0.1f * intensity);
        }

        private void CreateSteamEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                Dust steam = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.Cloud, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f)), 150, Color.White, Main.rand.NextFloat(1f, 2f));
                steam.noGravity = true;
                steam.fadeIn = 1.5f;
            }
            if (Main.rand.NextBool(10))
            {
                SoundEngine.PlaySound(SoundID.Item85 with
                {
                    Volume = 0.3f, Pitch = Main.rand.NextFloat(0.8f, 1.2f)
                }, Projectile.Center);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180);
            if (Main.rand.NextBool(4))
            {
                target.AddBuff(BuffID.OnFire3, 120);
            }
            CreateHitEffects(target.Center);
            SoundEngine.PlaySound(SoundID.Item74 with
            {
                Volume = 0.7f, Pitch = Main.rand.NextFloat(-0.1f, 0.2f)
            }, target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateSplashEffects();
            SoundEngine.PlaySound(SoundID.Item45 with
            {
                Volume = 0.5f, Pitch = Main.rand.NextFloat(0.3f, 0.7f)
            }, Projectile.Center);
            return true;
        }

        private void CreateHitEffects(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust fire = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(10, 10), DustID.Torch, vel, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust lava = Dust.NewDustPerfect(position, DustID.Lava, vel, 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                lava.noGravity = false;
            }
            for (int i = 0; i < 5; i++)
            {
                Dust Spark = Dust.NewDustPerfect(position, DustID.InfernoFork, Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                Spark.noGravity = true;
            }
        }

        private void CreateSplashEffects()
        {
            Vector2 pos = Projectile.Center;
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))
                    * Main.rand.NextFloat(2f, 5f);
                vel.Y = Math.Min(vel.Y, -1f);
                Dust splash = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5, 5), DustID.Lava, vel, 0, default, Main.rand.NextFloat(1f, 1.8f));
                splash.noGravity = false;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust fire = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(1.5f, 2.2f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust smoke = Dust.NewDustPerfect(pos, DustID.Smoke, new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2f, -0.5f)), 100, Color.DarkGray, Main.rand.NextFloat(1f, 1.5f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Gore gore = Gore.NewGorePerfect(Projectile.GetSource_Death(), pos, Main.rand.NextVector2Circular(2f, 2f), GoreID.Smoke1);
                gore.timeLeft = 20;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tearTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawTearTrail(sb, tearTexture);
            DrawLayeredSharpTears(sb, tearTexture);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        private void DrawTearTrail(SpriteBatch sb, Texture2D texture)
        {
            if (trailPositions.Count < 2) return;
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = 1; i < trailPositions.Count; i++)
            {
                float progress = 1f - (float)i / trailPositions.Count;
                Vector2 pos = trailPositions[i] - Main.screenPosition;
                Vector2 prevPos = trailPositions[i - 1] - Main.screenPosition;
                Vector2 dir = prevPos - pos;
                float rotation = dir != Vector2.Zero ? dir.ToRotation() + MathHelper.PiOver2 : Projectile.rotation;

                float trailScale = progress * DropletScale * 0.85f;
                Color trailRed = new Color(255, 30, 30) * (0.6f * progress);
                Color trailYellow = new Color(255, 210, 40) * (0.5f * progress);

                Vector2 scaleRed = new Vector2(0.45f, 0.85f) * trailScale;
                Vector2 scaleYellow = new Vector2(0.28f, 0.55f) * trailScale;

                sb.Draw(texture, pos, null, trailRed, rotation, origin, scaleRed, SpriteEffects.None, 0f);
                sb.Draw(texture, pos, null, trailYellow, rotation, origin, scaleYellow, SpriteEffects.None, 0f);
            }
        }

        private void DrawLayeredSharpTears(SpriteBatch sb, Texture2D texture)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            float rotation = Projectile.rotation;
            float pulse = 0.95f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.whoAmI) * 0.08f;
            float scale = DropletScale * pulse;


            Color redColor = new Color(255, 25, 25) * 0.95f;
            Vector2 redScale = new Vector2(0.65f, 1.15f) * scale;
            sb.Draw(texture, drawPos, null, redColor, rotation, origin, redScale, SpriteEffects.None, 0f);
            sb.Draw(texture, drawPos, null, redColor * 0.5f, rotation, origin, redScale * 1.25f, SpriteEffects.None, 0f);


            Color yellowColor = new Color(255, 220, 30) * 0.95f;
            Vector2 yellowScale = new Vector2(0.48f, 0.85f) * scale;
            sb.Draw(texture, drawPos, null, yellowColor, rotation, origin, yellowScale, SpriteEffects.None, 0f);


            Color whiteColor = Color.White * 0.95f;
            Vector2 whiteScale = new Vector2(0.28f, 0.55f) * scale;
            sb.Draw(texture, drawPos, null, whiteColor, rotation, origin, whiteScale, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
