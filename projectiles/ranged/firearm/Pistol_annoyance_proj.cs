using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using System.Collections.Generic;
using jarkos.systems;
namespace jarkos.projectiles.ranged.firearm
{
	public class Pistol_annoyance_proj : ModProjectile
	{
        private float lifeTimer = 0f;
        private List<Vector2> trail = new List<Vector2>();
        private const int TRAIL_MAX = 24;
        private const float WAVE_AMP = 2.8f;
        private const float WAVE_FREQ = 0.3f;
        private const int HEAL_AMOUNT = 2;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.3f;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            lifeTimer++;
            ApplyWave();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trail.Insert(0, Projectile.Center);
            if (trail.Count > TRAIL_MAX)
                trail.RemoveAt(trail.Count - 1);
            SpawnFlightDust();
            float pulse = (float)Math.Sin(lifeTimer * 0.2f) * 0.1f;
            Projectile.light = 0.3f + pulse;
        }
        private void ApplyWave()
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-forward.Y, forward.X);
            float phase = Projectile.ai[0];
            float current = (float)Math.Sin(lifeTimer * WAVE_FREQ + phase) * WAVE_AMP;
            float prev = (float)Math.Sin((lifeTimer - 1f) * WAVE_FREQ + phase) * WAVE_AMP;
            Projectile.position += perp * (current - prev);
        }
        private void SpawnFlightDust()
        {
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), 0, 0, DustID.Blood, 0, 0, 100, default, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.03f;
                d.fadeIn = 0.8f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch, 0, 0, 0, default, 0.4f);
                d.noGravity = true;
                d.velocity *= 0.02f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.active && !owner.dead)
            {
                owner.Heal(HEAL_AMOUNT);
                SpawnHealDust(target.Center, owner.Center);
            }
            var bp = owner.GetModPlayer<BloodRiflePlayer>();
            bp.AddBloodHit();
            if (bp.ConsumeBlood() && Main.myPlayer == Projectile.owner)
            {
                SpawnBloodBurst(target.Center, target);
            }
            SpawnImpactDust(target.Center);
            if (target.life <= 0)
            {
                SpawnDeathFountain(target.Center);
                if (owner.active && !owner.dead)
                {
                    owner.Heal(8);
                }
            }
        }
        private void SpawnHealDust(Vector2 from, Vector2 to)
        {
            int steps = 6;
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / steps;
                Vector2 pos = Vector2.Lerp(from, to, t);
                pos += Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.LifeDrain, 0, -1f, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }
        }
        private void SpawnBloodBurst(Vector2 pos, NPC source)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1 with
            {
                Volume = 0.7f, Pitch = 0.3f
            }, pos);
            int shardCount = 6;
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi / shardCount * i
                    + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, vel, ModContent.ProjectileType<Pistol_annoyance_proj_shard>(), (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.3f, Projectile.owner);
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f)
                    * Main.rand.NextFloat(0.4f, 1f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Blood, vel.X, vel.Y, 80, default, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.8f;
            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch, vel.X, vel.Y, 0, default, 1.3f);
                d.noGravity = true;
            }
        }
        private void SpawnImpactDust(Vector2 pos)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f)
                    * Main.rand.NextFloat(0.3f, 1f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Blood, vel.X, vel.Y, 60, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch, vel.X, vel.Y, 0, default, 0.9f);
                d.noGravity = true;
            }
        }
        private void SpawnDeathFountain(Vector2 pos)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath11 with
            {
                Volume = 0.5f, Pitch = -0.2f
            }, pos);
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-8f, -3f));
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Blood, vel.X, vel.Y, 60, default, 1.5f);
                d.noGravity = false;
                d.fadeIn = 1.8f;
            }
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Blood, vel.X, vel.Y, 40, default, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.LifeDrain, vel.X, vel.Y, 0, default, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 80, default, 0.7f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                spriteBatch.End();
                Effect effect =HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.0f);
                effect.Parameters["uColor"]?.SetValue(new Vector4(0.8f, 0.1f, 0.1f, 1f));
                effect.Parameters["uColorCore"]?.SetValue(new Vector4(1f, 0.6f, 0.6f, 1f));
                if (trail.Count > 1)
                {
                    TrailRenderer.Draw(effect, trail, t => MathHelper.Lerp(12f, 0f, t), t => Color.White, HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(220, 50, 50, 180);
        }
    }
    public class BloodRiflePlayer : ModPlayer
    {
        public int bloodHitCounter = 0;
        public const int BLOOD_THRESHOLD = 12;
        public void AddBloodHit()
        {
            bloodHitCounter++;
        }
        public bool ConsumeBlood()
        {
            if (bloodHitCounter >= BLOOD_THRESHOLD)
            {
                bloodHitCounter = 0;
                return true;
            }
            return false;
        }
    }
}
