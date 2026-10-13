using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.systems;
using System;
namespace jarkos.projectiles.ranged.firearm
{
    public class Celestian_vortex_laser : ModProjectile
    {
        private List<Vector2> trailPositions1 = new List<Vector2>();
        private List<Vector2> trailPositions2 = new List<Vector2>();
        private float timer = 0f;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;
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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            float orbitRadius = 8f;
            float spinSpeed = 0.4f;
            float wave = (float)Math.Sin(timer * spinSpeed) * orbitRadius;
            Vector2 pos1 = Projectile.Center + perp * wave;
            Vector2 pos2 = Projectile.Center - perp * wave;
            trailPositions1.Insert(0, pos1);
            if (trailPositions1.Count > 15)
                trailPositions1.RemoveAt(trailPositions1.Count - 1);
            trailPositions2.Insert(0, pos2);
            if (trailPositions2.Count > 15)
                trailPositions2.RemoveAt(trailPositions2.Count - 1);
            Lighting.AddLight(Projectile.Center, 0.2f, 0.6f, 0.8f);
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 100, default, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Vortex, 0f, 0f, 100, default, 1.2f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions1.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(0, 200, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());
                    TrailRenderer.Draw(effect, trailPositions1, t => MathHelper.Lerp(4f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                    TrailRenderer.Draw(effect, trailPositions2, t => MathHelper.Lerp(4f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                finally
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return false;
        }
    }
}
