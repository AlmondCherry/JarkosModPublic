using jarkos.dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class Particle_acc_proj : ModProjectile
	{
		private float energyPulse = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 3;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            energyPulse += 0.3f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            float lightIntensity = 0.6f + (float)Math.Sin(energyPulse) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.2f * lightIntensity, 0.5f * lightIntensity, 1f * lightIntensity);
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, 0f, 0f, 100, Color.Yellow, 0.3f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.15f;
                dust.fadeIn = 0.8f;
            }
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = new Vector2(0, Main.rand.NextFloat(3f, 6f)).RotatedBy(energyPulse * 2f);
                Dust orbitDust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.YellowTorch, 0f, 0f, 0, Color.Orange, 0.2f);
                orbitDust.noGravity = true;
                orbitDust.velocity = Projectile.velocity * 0.5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawTrail(spriteBatch, pixel);
            DrawCore(spriteBatch, pixel);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                float baseWidth = 5f * progress;
                float pulseWidth = baseWidth + (float)Math.Sin(energyPulse + i * 0.3f) * 1f * progress;
                Color innerColor = Color.Lerp(new Color(255, 249, 151), new Color(255, 98, 92), (float)i / Projectile.oldPos.Length);
                Color outerColor = Color.Lerp(new Color(255, 249, 151), new Color(255, 98, 92), (float)i / Projectile.oldPos.Length);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor * progress * 0.3f, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth * 2.5f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), innerColor * progress * 0.6f, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth * 1.5f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * progress * 0.9f, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth * 0.5f), SpriteEffects.None, 0f);
            }
        }
        private void DrawCore(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(energyPulse) * 0.25f;
            for (int i = 3; i >= 0; i--)
            {
                float scale = (4 - i) * 5f * pulse;
                float alpha = 0.15f - i * 0.03f;
                Color glowColor = new Color(255, 249, 151) * alpha;
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f), scale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 249, 151) * 0.7f, 0f, new Vector2(0.5f), 8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f), 4f * pulse, SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.4f, Pitch = 1.2f }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 100, default, 1f);
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                dust.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.8f, 1.2f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = 1.5f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 80, default, 1f);
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 0, default, 1.3f);
                dust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
    #region Charged Particle
    public class ChargedParticle : ModProjectile
    {
        private float energyPulse = 0f;
        private float rotationOffset = 0f;
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 2;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        public override void AI()
        {
            energyPulse += 0.4f;
            rotationOffset += 0.15f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            float lightIntensity = 0.9f + (float)Math.Sin(energyPulse) * 0.3f;
            Lighting.AddLight(Projectile.Center, 0.4f * lightIntensity, 0.7f * lightIntensity, 1.2f * lightIntensity);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, 0f, 0f, 50, default, 1f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
                dust.fadeIn = 1.1f;
            }
            for (int i = 0; i < 2; i++)
            {
                float angle = rotationOffset + i * MathHelper.Pi;
                Vector2 offset = new Vector2(0, 10f).RotatedBy(angle);
                Dust orbitDust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.YellowTorch, 0f, 0f, 0, Color.Orange, 0.2f);
                orbitDust.noGravity = true;
                orbitDust.velocity = Projectile.velocity * 0.3f;
            }
            if (Main.rand.NextBool(4))
            {
                Vector2 ringOffset = Main.rand.NextVector2CircularEdge(12f, 12f);
                Dust ringDust = Dust.NewDustDirect(Projectile.Center + ringOffset, 0, 0, DustID.YellowTorch, 0f, 0f, 100, Color.Yellow, 0.2f);
                ringDust.noGravity = true;
                ringDust.velocity = Projectile.velocity * 0.2f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawEnergyTrail(spriteBatch, pixel);
            DrawChargedCore(spriteBatch, pixel);
            return false;
        }
        private void DrawEnergyTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                float baseWidth = 10f * progress;
                float pulseWidth = baseWidth + (float)Math.Sin(energyPulse + i * 0.2f) * 3f * progress;
                Color coreColor = Color.White;
                Color innerColor = new Color(255, 249, 151);
                Color midColor = new Color(255, 255, 100);
                Color outerColor = new Color(199, 169, 80);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor * progress * 0.25f, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth * 3f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), midColor * progress * 0.4f, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth * 2f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), innerColor * progress * 0.7f, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor * progress, rotation, new Vector2(0, 0.5f), new Vector2(length, pulseWidth * 0.4f), SpriteEffects.None, 0f);
            }
        }
        private void DrawChargedCore(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(energyPulse) * 0.35f;
            for (int i = 0; i < 4; i++)
            {
                float angle = rotationOffset + i * MathHelper.PiOver2;
                Vector2 boltOffset = new Vector2(0, 15f * pulse).RotatedBy(angle);
                Vector2 boltEnd = drawPos + boltOffset;
                Vector2 boltDir = boltEnd - drawPos;
                float boltRot = boltDir.ToRotation();
                float boltLen = boltDir.Length();
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 249, 151) * 0.6f, boltRot, new Vector2(0, 0.5f), new Vector2(boltLen, 3f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White * 0.8f, boltRot, new Vector2(0, 0.5f), new Vector2(boltLen, 1f), SpriteEffects.None, 0f);
            }
            for (int i = 5; i >= 0; i--)
            {
                float scale = (6 - i) * 7f * pulse;
                float alpha = 0.12f - i * 0.015f;
                Color glowColor = Color.Lerp(new Color(255, 249, 151), new Color(255, 249, 151), (float)i / 5f) * alpha;
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f), scale, SpriteEffects.None, 0f);
            }
            float ringSize = 12f + (float)Math.Sin(energyPulse * 2f) * 3f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 249, 151) * 0.5f, 0f, new Vector2(0.5f), ringSize, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(200, 230, 255), 0f, new Vector2(0.5f), 8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f), 5f * pulse, SpriteEffects.None, 0f);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f, Pitch = 0.8f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCHit53 with { Volume = 0.4f, Pitch = 1.3f }, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                int particleCount = Main.rand.Next(3, 6);
                for (int i = 0; i < particleCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(8f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<SecondaryParticle>(), Projectile.damage / 3, 0.2f, Projectile.owner);
                }
            }
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 50, default, 1.3f);
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            Lighting.AddLight(Projectile.Center, 1f, 1.2f, 1.5f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.4f, Pitch = 0.8f }, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 50, default, 1.5f);
                dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 0, default, 1.8f);
                dust.velocity = Main.rand.NextVector2Circular(7f, 7f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 12; i++)
            {
                float angle = i * MathHelper.TwoPi / 12f;
                Vector2 velocity = new Vector2(6f, 0f).RotatedBy(angle);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Square_dust>(), velocity.X, velocity.Y, 80, Color.Yellow, 0.2f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #endregion
    #region Secondary Projectile
     public class SecondaryParticle : ModProjectile
    {
        private float lifeProgress = 0f;
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 45;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            lifeProgress += 1f / 45f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            float intensity = 1f - lifeProgress;
            Lighting.AddLight(Projectile.Center, 0.15f * intensity, 0.25f * intensity, 0.4f * intensity);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, 0f, 0f, 150, default, 0.4f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
            Projectile.velocity *= 0.96f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            float fadeOut = 1f - lifeProgress;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                float width = 3f * progress * fadeOut;
                Color trailColor = new Color(255, 249, 151) * progress * fadeOut * 0.8f;
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.4f, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * progress * fadeOut * 0.9f, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.3f), SpriteEffects.None, 0f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 249, 151) * fadeOut * 0.5f, 0f, new Vector2(0.5f), 8f * fadeOut, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 249, 151) * fadeOut * 0.7f, 0f, new Vector2(0.5f), 5f * fadeOut, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White * fadeOut, 0f, new Vector2(0.5f), 2.5f * fadeOut, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.2f, Pitch = 1.8f }, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 100, default, 0.5f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #endregion
}
