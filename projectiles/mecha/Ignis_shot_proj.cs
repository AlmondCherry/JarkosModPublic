using jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;

namespace Jarkos.projectiles.mecha
{
	public class Ignis_shot_proj : ModProjectile
	{
        private List<Vector2> oldPositions = new List<Vector2>();
        private const int TrailLength = 20;
        private float pulseTimer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1.2f;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            pulseTimer += 0.15f;
            Projectile.ai[0]++;


            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }


            Projectile.velocity *= 0.94f;


            if (Projectile.velocity.Length() <= 0.35f)
            {
                Projectile.velocity = Vector2.Zero;
                Explode();
                return;
            }


            if (Main.rand.NextBool(20))
            {
                Dust lavaDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 0.8f);
                lavaDust.noGravity = true;
                lavaDust.velocity *= 0.5f;
            }
            if (Main.rand.NextBool(30))
            {
                Dust sparkDust = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 0, 0, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 0.5f);
                sparkDust.noGravity = true;
            }
            if (Main.rand.NextBool(40))
            {
                Dust smokeDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, -1f, 150, default, 0.5f);
                smokeDust.velocity = -Projectile.velocity * 0.3f + new Vector2(0, -2f);
            }
            if (Projectile.ai[0] % 30 == 0)
            {
                float angle = Projectile.ai[0] * 0.3f;
                Vector2 ringOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
                Dust ringDust = Dust.NewDustDirect(Projectile.Center + ringOffset, 0, 0, DustID.SolarFlare, 0, 0, 0, default, 0.4f);
                ringDust.noGravity = true;
                ringDust.velocity = Vector2.Zero;
            }


            if (Main.rand.NextBool(2))
            {
                Color fireColor = Color.Lerp(new Color(255, 120, 20), new Color(255, 200, 40), Main.rand.NextFloat());
                Vector2 sparkVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                new Spark(Projectile.Center + Main.rand.NextVector2Circular(12, 12), sparkVel, fireColor, Main.rand.Next(18, 30), Main.rand.NextFloat(0.3f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            if (Main.rand.NextBool(4))
            {
                Color fireColor = Color.Lerp(new Color(255, 90, 15), new Color(255, 180, 30), Main.rand.NextFloat());
                Vector2 sparkVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                new Spark_gravit(Projectile.Center, sparkVel, fireColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.3f, 0.5f), 1f).Spawn();
            }

            if (Projectile.ai[0] % 30 == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Volume = 0.3f, Pitch = -0.2f }, Projectile.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;


            float speed = Projectile.velocity.Length();
            float stretchX = 1f + speed * 0.045f;
            float squashY = Math.Max(0.35f, 1f - speed * 0.04f);
            Vector2 drawScale = new Vector2(stretchX, squashY) * Projectile.scale;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.6f;
                Vector2 trailScale = drawScale * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(Color.OrangeRed, Color.Yellow, progress * 0.5f) * trailAlpha;
                trailColor.A = 0;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            for (int j = 0; j < 4; j++)
            {
                Vector2 glowScale = drawScale * (1.2f + j * 0.15f);
                Color glowColor = Color.OrangeRed * (0.15f - j * 0.03f);
                glowColor.A = 0;
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
            }

            Color mainColor = Color.White;
            if (Projectile.alpha > 0)
                mainColor *= (255 - Projectile.alpha) / 255f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, origin, drawScale, SpriteEffects.None, 0f);

            Color coreColor = Color.Yellow * 0.8f;
            coreColor.A = 0;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin, drawScale * 0.6f, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Explode();
        }

        private void Explode()
        {
            if (Projectile.ai[1] == 1) return;
            Projectile.ai[1] = 1;
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1.2f, Pitch = -0.3f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.8f }, Projectile.Center);

            if (Main.LocalPlayer.Distance(Projectile.Center) < 600f)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
            }

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Ignisium_lava_wave>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack * 1.5f, Projectile.owner);
            }


            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi / 4 * i;
                Vector2 ringVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 12f;
                Dust ring = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.SolarFlare, ringVel.X, ringVel.Y, 0, default, 0.8f);
                ring.noGravity = true;
            }

            for (int i = 0; i < 2; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(30, 30), 0, 0, DustID.Smoke, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -2f), 200, default, 0.6f);
            }


            for (int i = 0; i < 20; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(12f, 12f);
                Color fireColor = Color.Lerp(new Color(255, 100, 15), new Color(255, 210, 40), Main.rand.NextFloat());
                new Spark(Projectile.Center, sparkVel, fireColor, Main.rand.Next(20, 40), Main.rand.NextFloat(0.4f, 0.8f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            for (int i = 0; i < 15; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(10f, 10f) + new Vector2(0f, -3f);
                Color fireColor = Color.Lerp(new Color(255, 80, 10), new Color(255, 180, 30), Main.rand.NextFloat());
                new Spark_gravit(Projectile.Center, sparkVel, fireColor, Main.rand.Next(25, 45), Main.rand.NextFloat(0.35f, 0.7f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            for (int i = 0; i < 6; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(8f, 8f), GoreID.Smoke1 + Main.rand.Next(3), 1.5f);
                gore.velocity *= 0.5f;
            }

            int dropCount = Main.rand.Next(8, 14);
            for (int i = 0; i < dropCount; i++)
            {
                float angle = MathHelper.TwoPi / dropCount * i + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(6f, 14f);
                Vector2 dropVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                dropVel += Main.rand.NextVector2Circular(2f, 2f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, dropVel, ModContent.ProjectileType<IgniziumDrop>(), Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
            }
            Projectile.Kill();
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * ((255 - Projectile.alpha) / 255f);
        }
	}

	#region Lava Drop
	public class IgniziumDrop : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/mecha/IgniziumDrop";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.light = 0.6f;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.4f) * 0.15f;
            Projectile.scale = 0.8f * pulse;


            if (Main.rand.NextBool(20))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 0.5f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }

            if (Main.rand.NextBool(2))
            {
                Color fireColor = Color.Lerp(new Color(255, 110, 15), new Color(255, 190, 30), Main.rand.NextFloat());
                new Spark(Projectile.Center, -Projectile.velocity * 0.1f, fireColor, Main.rand.Next(15, 25), Main.rand.NextFloat(0.2f, 0.45f), 1f).Spawn();
            }

            if (Projectile.wet)
            {
                SoundEngine.PlaySound(SoundID.LiquidsWaterLava with { Volume = 0.5f }, Projectile.Center);
                for (int i = 0; i < 2; i++)
                {
                    Dust steam = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -1f), 150, default, 0.5f);
                    steam.velocity *= 0.8f;
                }
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.5f;
                float trailScale = Projectile.scale * (1f - progress * 0.7f);
                Color trailColor = Color.Lerp(Color.Orange, Color.DarkRed, progress) * trailAlpha;
                trailColor.A = 0;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            for (int j = 0; j < 3; j++)
            {
                Color glowColor = Color.Orange * (0.2f - j * 0.05f);
                glowColor.A = 0;
                float glowScale = Projectile.scale * (1.3f + j * 0.2f);
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            Color coreColor = Color.Yellow * 0.7f;
            coreColor.A = 0;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin, Projectile.scale * 0.4f, SpriteEffects.None, 0f);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180);
            for (int i = 0; i < 1; i++)
            {
                Dust hitDust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Lava, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 0.5f);
                hitDust.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Color fireColor = Color.Lerp(new Color(255, 120, 20), new Color(255, 200, 40), Main.rand.NextFloat());
                new Spark(target.Center, Main.rand.NextVector2Circular(4f, 4f), fireColor, Main.rand.Next(15, 25), Main.rand.NextFloat(0.25f, 0.45f), 1f).Spawn();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
            for (int i = 0; i < 1; i++)
            {
                Dust splatter = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, -1f), 0, default, 0.6f);
                splatter.noGravity = false;
            }
            for (int i = 0; i < 5; i++)
            {
                Color fireColor = Color.Lerp(new Color(255, 120, 20), new Color(255, 200, 40), Main.rand.NextFloat());
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), fireColor, Main.rand.Next(15, 25), Main.rand.NextFloat(0.25f, 0.45f), 1f).Spawn();
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 1; i++)
            {
                Dust death = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Lava, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 0.4f);
                death.noGravity = Main.rand.NextBool();
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * ((255 - Projectile.alpha) / 255f);
        }
    }
}
#endregion
