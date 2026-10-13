using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Pyromelt_meteor : ModProjectile {
		private int targetNPC = -1;
        private bool exploded = false;
        private float meteorScale = 0.3f;
        private float glowTimer = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.scale = 1.5f;
        }
        public override void AI()
        {
            if (targetNPC == -1)
                targetNPC = (int)Projectile.ai[0];
            glowTimer += 0.1f;
            meteorScale = MathHelper.Lerp(meteorScale, 1.5f, 0.04f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (targetNPC >= 0 && targetNPC < Main.npc.Length && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();
                Vector2 direction = Vector2.Normalize(toTarget);
                float currentSpeed = Projectile.velocity.Length();
                float targetSpeed = Math.Max(currentSpeed, 30f);
                if (distance < 400f)
                    targetSpeed = Math.Max(targetSpeed, 35f);
                if (distance < 200f)
                    targetSpeed = Math.Max(targetSpeed, 40f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * targetSpeed, 0.1f);
                if (distance < 60f)
                {
                    Explode(target.Center);
                    return;
                }
            }
            else
            {
                Projectile.velocity.Y += 0.5f;
                if (Projectile.velocity.Length() > 35f)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 35f;
            }
            float lightPulse = 2.5f + 0.5f * (float)Math.Sin(glowTimer * 3f);
            Lighting.AddLight(Projectile.Center, lightPulse, lightPulse * 0.5f, lightPulse * 0.12f);
            if (Projectile.timeLeft % 8 == 0)
                SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = -0.8f, Volume = 0.4f }, Projectile.Center);
        }
        private void Explode(Vector2 center)
        {
            if (exploded) return;
            exploded = true;
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 2f }, center);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.5f }, center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.3f, Volume = 1.5f }, center);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.4f, Volume = 1.2f }, center);
            for (int wave = 0; wave < 3; wave++)
            {
                float waveRadius = 40f + wave * 50f;
                int waveCount = 30 + wave * 15;
                for (int i = 0; i < waveCount; i++)
                {
                    float angle = MathHelper.TwoPi / waveCount * i;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (8f + wave * 3f);
                    Vector2 pos = center + vel * (wave * 0.3f);
                    Color waveColor = Color.Lerp(new Color(255, 255, 220), new Color(255, 120, 20), (float)wave / 3f);
                    Dust magic = Dust.NewDustDirect(pos - new Vector2(1), 2, 2, DustID.MagicMirror, vel.X, vel.Y, 0, waveColor, 2f - wave * 0.3f);
                    magic.noGravity = true;
                    magic.fadeIn = 1.5f;
                    Lighting.AddLight(pos, waveColor.R / 255f, waveColor.G / 255f * 0.5f, 0.1f);
                }
            }
            for (int i = 0; i < 80; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(16f, 16f) * Main.rand.NextFloat(0.2f, 1f);
                Dust fire = Dust.NewDustDirect(center - new Vector2(20), 40, 40, DustID.Torch, vel.X, vel.Y, 40, default, Main.rand.NextFloat(2.5f, 4f));
                fire.noGravity = true;
                fire.fadeIn = 2.5f;
            }
            for (int i = 0; i < 50; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(12f, 12f) * Main.rand.NextFloat(0.3f, 1f);
                Dust solar = Dust.NewDustDirect(center - new Vector2(16), 32, 32, DustID.SolarFlare, vel.X, vel.Y, 80, default, Main.rand.NextFloat(2.5f, 3.5f));
                solar.noGravity = true;
                solar.fadeIn = 2f;
            }
            for (int i = 0; i < 50; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-22f, -6f));
                Dust pillar = Dust.NewDustDirect(center - new Vector2(40, 10), 80, 20, DustID.Torch, vel.X, vel.Y, 60, default, Main.rand.NextFloat(2f, 3.5f));
                pillar.noGravity = true;
                pillar.fadeIn = 2f;
            }
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-18f, -3f));
                Color sparkColor = Color.Lerp(new Color(255, 255, 200), new Color(255, 100, 20), Main.rand.NextFloat());
                Dust magic = Dust.NewDustDirect(center - new Vector2(30, 10) + Main.rand.NextVector2Circular(20f, 10f), 4, 4, DustID.MagicMirror, vel.X, vel.Y, 0, sparkColor, Main.rand.NextFloat(1f, 2.5f));
                magic.noGravity = true;
                magic.fadeIn = 1.5f;
                Lighting.AddLight(magic.position, sparkColor.R / 255f, sparkColor.G / 255f * 0.5f, 0.1f);
            }
            for (int i = 0; i < 60; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(10f, 10f) * Main.rand.NextFloat(0.1f, 1f);
                Color sphereColor = Color.Lerp(new Color(255, 240, 180), new Color(255, 80, 10), Main.rand.NextFloat());
                Dust sphere = Dust.NewDustDirect(center + Main.rand.NextVector2Circular(10f, 10f) - new Vector2(1), 2, 2, DustID.MagicMirror, vel.X, vel.Y, 0, sphereColor, Main.rand.NextFloat(1.2f, 2.8f));
                sphere.noGravity = true;
                sphere.fadeIn = 1f + Main.rand.NextFloat(1f);
                Lighting.AddLight(sphere.position, sphereColor.R / 400f, sphereColor.G / 500f, 0.05f);
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) + new Vector2(0, -3f);
                Dust smoke = Dust.NewDustDirect(center - new Vector2(30), 60, 60, DustID.Smoke, vel.X, vel.Y, 220, default, Main.rand.NextFloat(2f, 4f));
                smoke.velocity *= 1.5f;
            }
            for (int g = 0; g < 10; g++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(8f, 8f);
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), center, vel, Main.rand.Next(61, 64), Main.rand.NextFloat(1.2f, 2f));
                gore.velocity *= 1.2f;
            }
            int explosionRadius = 220;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Center.Distance(center) < explosionRadius)
                {
                    npc.AddBuff(BuffID.OnFire3, 900);
                }
            }
            Lighting.AddLight(center, 6f, 3f, 0.8f);
            Projectile.position = center - Projectile.Size / 2;
            Projectile.Damage();
            Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 900);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2;
            float pulse = 1f + 0.2f * (float)Math.Sin(glowTimer * 3f);
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float opacity = 1f - (float)i / Projectile.oldPos.Length;
                float trailScale = meteorScale * (0.3f + 0.7f * opacity);
                Color trailColor = Color.Lerp(new Color(255, 30, 5, 0), new Color(255, 220, 60, 0), opacity) * opacity * 0.5f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 40, 5, 0) * 0.15f, Projectile.rotation, origin, meteorScale * 2.2f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 80, 15, 0) * 0.25f, Projectile.rotation, origin, meteorScale * 1.7f * pulse, SpriteEffects.None, 0);
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = new Vector2(5 * pulse, 0).RotatedBy(MathHelper.TwoPi / 8 * i + glowTimer * 1.5f);
                Color rayColor = Color.Lerp(new Color(255, 150, 30, 0), new Color(255, 80, 10, 0), (float)i / 8f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, rayColor * 0.4f, Projectile.rotation, origin, meteorScale * 1.25f, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 220, 120, 0) * 0.35f, Projectile.rotation, origin, meteorScale * 1.3f * pulse, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, meteorScale, SpriteEffects.None, 0);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override void OnKill(int timeLeft)
        {
            if (!exploded)
            {
                Explode(Projectile.Center);
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int inflate = 40;
            hitbox.Inflate(inflate, inflate);
        }
	}
}
