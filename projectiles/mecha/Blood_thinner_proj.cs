using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using Terraria.DataStructures;

namespace jarkos.projectiles.mecha
{
    public class Blood_thinner_proj : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        public float ChargeLevel
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
            Projectile.light = 0.3f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.08f;
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, default, 1.0f + ChargeLevel * 0.1f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (ChargeLevel >= 4)
            {
                Lighting.AddLight(Projectile.Center, 0.6f, 0.05f, 0.05f);
            }
            float wave = (float)Math.Sin(Projectile.timeLeft * 0.3f) * 0.3f;
            Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
            Projectile.position += perpendicular * wave;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null && trailPositions.Count > 1)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f + Projectile.identity * 0.05f);
                effect.Parameters["uOpacity"]?.SetValue(1.3f + ChargeLevel * 0.2f);
                effect.Parameters["uColor"]?.SetValue(new Color(220, 20, 20).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 100, 100).ToVector4());
                List<Vector2> currentTrail = new List<Vector2>();
                currentTrail.Add(Projectile.Center);
                currentTrail.AddRange(trailPositions);
                TrailRenderer.Draw(effect, currentTrail, t => (16f + ChargeLevel * 4f) * MathF.Pow(1f - t, 0.4f), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 50, 50, 200), Projectile.rotation, texture.Size() / 2f, 0.8f + ChargeLevel * 0.15f, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnHitEffect(target.Center);
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 0.8f }, target.Center);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SpawnHitEffect(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item85 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            SpawnHitEffect(Projectile.Center);
        }
        private void SpawnHitEffect(Vector2 position)
        {
            int particleCount = (int)(10 + ChargeLevel * 5);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 8f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust blood = Dust.NewDustDirect(position, 1, 1, DustID.Blood, vel.X, vel.Y, 50, default, Main.rand.NextFloat(1f, 2f));
                blood.noGravity = false;
                blood.velocity = vel;
                blood.fadeIn = 0.8f;
            }
            for (int i = 0; i < particleCount / 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust glow = Dust.NewDustDirect(position, 1, 1, DustID.RedTorch, vel.X, vel.Y, 100, new Color(255, 50, 50, 0), Main.rand.NextFloat(0.5f, 1.2f));
                glow.noGravity = true;
                glow.velocity = vel;
            }
            if (ChargeLevel >= 3 && Main.myPlayer == Projectile.owner)
            {
                int splashCount = (int)(ChargeLevel - 2) * 2;
                for (int i = 0; i < splashCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    vel.Y -= Main.rand.NextFloat(1f, 4f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, vel, ModContent.ProjectileType<BloodDropletProjectile>(), Projectile.damage / 3, 0f, Projectile.owner);
                }
            }
        }
    }
    public class BloodDropletProjectile : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Blood_thinner_proj";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.alpha = 50;
            Projectile.light = 0.1f;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.25f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 100, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2[] oldPos = Projectile.oldPos;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (oldPos[i] == Vector2.Zero || oldPos[i + 1] == Vector2.Zero)
                    continue;
                Vector2 start = oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 diff = end - start;
                float length = diff.Length();
                float rot = diff.ToRotation();
                float progress = (float)i / oldPos.Length;
                float alpha = MathHelper.Lerp(0.7f, 0f, progress);
                float width = MathHelper.Lerp(3f, 0.5f, progress);
                Color c = new Color(200, 0, 0, 50) * alpha;
                spriteBatch.Draw(magicPixel, start, new Rectangle(0, 0, 1, 1), c, rot, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
            }
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(220, 20, 20, 150), Projectile.rotation, tex.Size() / 2f, 0.8f, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 50, default, 0.8f);
                d.noGravity = false;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(220, 20, 20, 150);
        }
    }
}
