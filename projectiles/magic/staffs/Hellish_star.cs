using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Jarkos.projectiles.magic.toms;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;
using jarkos.systems;
namespace Jarkos.projectiles.magic.staffs
{
  public class Hellish_star : ModProjectile
  {
      public override string Texture => "jarkos/textures/MagicSphereProj";
      private const int TrailMax = 35;
        private Vector2[] trailCache = new Vector2[TrailMax];
        private bool trailInitialized = false;
        private int splitTimer = 42;
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailMax; i++)
                    trailCache[i] = Projectile.Center;
                trailInitialized = true;
            }
            for (int i = TrailMax - 1; i > 0; i--)
                trailCache[i] = trailCache[i - 1];
            trailCache[0] = Projectile.Center;
            Projectile.ai[0]++;
            Projectile.rotation += 0.12f;
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 0.5f * pulse, 0.6f * pulse, 1.0f * pulse);
            SpawnStarDust();
            splitTimer--;
            if (splitTimer <= 0)
            {
                Split();
                return;
            }
            Projectile.ai[1] = 1f + (float)Math.Sin(Projectile.ai[0] * 0.2f) * 0.15f;
        }
        private void SpawnStarDust()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = Main.rand.NextBool() ? new Color(130, 80, 255) : new Color(150, 210, 255);
                new jarkos.particles.StarParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), vel, col, 25, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
        }
        private void Split()
        {
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.3f }, Projectile.Center);
            if (Projectile.owner == Main.myPlayer)
            {
                int count = 6;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i + Main.rand.NextFloat(-0.1f, 0.1f);
                    float speed = Main.rand.NextFloat(5f, 8f);
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Hellish_bolt>(), (int)(Projectile.damage * 0.55f), Projectile.knockBack * 0.4f, Projectile.owner);
                }
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = Main.rand.NextBool() ? new Color(130, 80, 255) : new Color(150, 210, 255);
                new jarkos.particles.StarParticle(Projectile.Center, vel, col, 35, Main.rand.NextFloat(0.05f, 0.15f), 1f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 1.2f, 1.3f, 2f);
            Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!trailInitialized)
                return false;
            SpriteBatch sb = Main.spriteBatch;
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < TrailMax; i++)
            {
                if (trailCache[i] != Vector2.Zero)
                    points.Add(trailCache[i]);
            }
            if (points.Count >= 2 && HealShaders.Stream != null)
            {
                sb.End();
                Effect effect = HealShaders.Stream;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
                effect.Parameters["uOpacity"]?.SetValue(1f);
                effect.Parameters["uColor"]?.SetValue(new Color(110, 50, 220).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(160, 220, 255).ToVector4());
                TrailRenderer.Draw(
                    effect,
                    points,
                    t => 28f * (1f - t) + 0.5f,
                    t => Color.White * (1f - t),
                    HealShaders.HealNoise
                );
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Projectile.ai[0] * 0.15f) * 0.15f;
            Color drawCol = new Color(130, 80, 255);
            sb.Draw(tex, pos, null, drawCol * 0.4f * pulse, Projectile.rotation, origin, Projectile.scale * 1.4f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, drawCol, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, Color.White * 0.8f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);
        }
        public override void OnKill(int timeLeft)
        {
            if (splitTimer > 0)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    int count = 6;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi / count * i;
                        float speed = Main.rand.NextFloat(5f, 8f);
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                        Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel, ModContent.ProjectileType<Hellish_bolt>(), (int)(Projectile.damage * 0.55f), Projectile.knockBack * 0.4f, Projectile.owner);
                    }
                }
            }
            SoundEngine.PlaySound(SoundID.Item25, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = Main.rand.NextBool() ? new Color(130, 80, 255) : new Color(150, 210, 255);
                new jarkos.particles.StarParticle(Projectile.Center, vel, col, 35, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 1f, 1.1f, 1.8f);
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
  }
}
