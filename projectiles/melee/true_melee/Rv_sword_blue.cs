using System;
using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Rv_sword_blue : ModProjectile
	{
        public enum LunarAttackType
        {
            MoonBlade, StarRain
        }
        public LunarAttackType AttackType
        {
            get => (LunarAttackType)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        public float Timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private Vector2 targetPosition;
        private bool initialized = false;
        private float pulseTimer = 0f;
        private float curveAngle = 0f;
        private int starsSpawned = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 45;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Timer++;
            pulseTimer += 0.12f;
            if (!initialized)
            {
                targetPosition = Main.MouseWorld;
                initialized = true;
                if (AttackType == LunarAttackType.MoonBlade)
                {
                    Vector2 toTarget = targetPosition - Projectile.Center;
                    curveAngle = toTarget.ToRotation();
                }
            }
            if (Timer < 15 && Projectile.owner == Main.myPlayer)
            {
                targetPosition = Main.MouseWorld;
            }
            float pulse = 1f + MathF.Sin(pulseTimer * 1.5f) * 0.12f;
            Projectile.scale = pulse * GetBaseScale();
            switch (AttackType)
            {
                case LunarAttackType.MoonBlade:
                    AI_MoonBlade();
                    break;
                case LunarAttackType.StarRain:
                    AI_StarRain();
                    break;
            }
            SpawnParticles();
            UpdateLighting();
        }
        private void AI_MoonBlade()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.penetrate = -1;
            Vector2 toTarget = targetPosition - Projectile.Center;
            float distance = toTarget.Length();
            if (distance > 30f)
            {
                float speed = 16f;
                Vector2 desiredVel = Vector2.Normalize(toTarget) * speed;
                float curve = MathF.Sin(Timer * 0.15f) * 0.08f;
                desiredVel = desiredVel.RotatedBy(curve);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.12f);
            }
            else
            {
                Projectile.velocity *= 0.95f;
                if (Projectile.velocity.Length() < 2f)
                {
                    Projectile.Kill();
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }
        private void AI_StarRain()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Vector2 toTarget = targetPosition - Projectile.Center;
            float distance = toTarget.Length();
            if (distance > 50f)
            {
                float speed = 14f;
                Vector2 desiredVel = Vector2.Normalize(toTarget) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.1f);
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            Projectile.rotation += 0.15f;
            if (Timer % 8 == 0 && starsSpawned < 8 && Projectile.owner == Main.myPlayer)
            {
                SpawnFallingStar();
                starsSpawned++;
            }
            if (starsSpawned >= 8 && Timer > 60)
            {
                Projectile.alpha += 10;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }
        private void SpawnFallingStar()
        {
            Vector2 starTarget = targetPosition + Main.rand.NextVector2Circular(80, 80);
            Vector2 starStart = starTarget + new Vector2(Main.rand.NextFloat(-100, 100), -600);
            Vector2 starVel = (starTarget - starStart);
            starVel.Normalize();
            starVel *= 18f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), starStart, starVel, ModContent.ProjectileType<Falling_star_rv>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            SoundEngine.PlaySound(SoundID.Item9 with { Pitch = 0.5f, Volume = 0.6f }, starTarget);
        }
        private float GetBaseScale()
        {
            return AttackType switch
            {
                LunarAttackType.MoonBlade => 1.3f, LunarAttackType.StarRain => 0.9f, _ => 1.0f
            };
        }
        private void SpawnParticles()
        {
            if (AttackType == LunarAttackType.MoonBlade)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust moon = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(10, 10), 0, 0, DustID.IceTorch, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 100, default, 1.5f);
                    moon.noGravity = true;
                    moon.fadeIn = 1.2f;
                }
                if (Main.rand.NextBool(4))
                {
                    Dust star = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 0, 0, DustID.BlueTorch, 0, 0, 150, default, 0.8f);
                    star.noGravity = true;
                    star.velocity *= 0.2f;
                }
            }
            else
            {
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = new Vector2(15, 0).RotatedBy(angle);
                    Dust aura = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.IceTorch, 0, 0, 100, default, 1.2f);
                    aura.noGravity = true;
                    aura.velocity = -offset * 0.05f;
                }
            }
        }
        private void UpdateLighting()
        {
            float intensity = 0.8f + MathF.Sin(pulseTimer * 2) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.3f * intensity, 0.5f * intensity, 1f * intensity);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<FrostSlowDebuff>(), 120);
            target.AddBuff(BuffID.Frostburn2, 90);
            if (Main.rand.NextBool(6))
            {
                target.AddBuff(BuffID.Frozen, 45);
            }
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.IceTorch, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 100, default, 1.5f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = TextureAssets.Extra[98].Value;
            Texture2D proj = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glow.Size() / 2;
            Vector2 projOrigin = proj.Size() / 2;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                float scale = Projectile.scale * (1f - progress * 0.4f);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Color trailColor = Color.Lerp(new Color(180, 220, 255), new Color(50, 100, 200), progress) * alpha;
                sb.Draw(glow, trailPos, null, trailColor, 0, glowOrigin, scale, SpriteEffects.None, 0);
            }
            float pulse = 1f + MathF.Sin(pulseTimer * 2) * 0.15f;
            for (int i = 4; i >= 0; i--)
            {
                float layerScale = (1.5f + i * 0.5f) * Projectile.scale * pulse;
                Color layerColor = new Color(100, 180, 255) * (0.18f - i * 0.03f);
                sb.Draw(glow, pos, null, layerColor, pulseTimer * 0.3f * (i % 2 == 0 ? 1 : -1), glowOrigin, layerScale, SpriteEffects.None, 0);
            }
            sb.Draw(glow, pos, null, new Color(200, 230, 255) * 0.9f, 0, glowOrigin, Projectile.scale * 0.4f, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, Color.White * 0.85f, 0, glowOrigin, Projectile.scale * 0.2f, SpriteEffects.None, 0);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
	}
}
