using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;
using jarkos.systems;
using System.Collections.Generic;

namespace Jarkos.projectiles.mecha
{
    public class MBlastCrossLaser : ModProjectile
    {
        public override string Texture => "jarkos/particles/Stretching_particle";
        private float RotatedRand = Main.rand.NextFloat(-2.3f, 2.3f);
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.rotation = RotatedRand;
        }
        public override void AI()
        {
            Projectile.rotation += 0.02f;
            Projectile.scale = (float)Math.Sin((MathHelper.Pi + Main.rand.NextFloat(1f, 2.5f)) * (1f - Projectile.timeLeft / 25f));
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            for (int i = 0; i < 4; i++)
            {
                float angle = Projectile.rotation + MathHelper.PiOver2 * i;
                Vector2 endPoint = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 300f * Projectile.scale;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, endPoint, 20f * Projectile.scale, ref point))
                {
                    return true;
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            float scaleBase = Projectile.scale;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(50, 255, 50).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 200).ToVector4());
                for (int i = 0; i < 4; i++)
                {
                    float angle = Projectile.rotation + MathHelper.PiOver2 * i;
                    Vector2 start = Projectile.Center;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    List<Vector2> points = new List<Vector2>();
                    int segmentCount = 10;
                    for (int j = 0; j < segmentCount; j++)
                    {
                        float t = (float)j / (segmentCount - 1);
                        points.Add(start + direction * 300f * scaleBase * t);
                    }
                    TrailRenderer.Draw(effect, points, t => MathHelper.Lerp(50f * scaleBase, 0f, t), t => Color.White * (1f - t) * scaleBase, HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(150, 255, 150) * scaleBase, 0f, glowTex.Size() / 2f, 1.5f * scaleBase, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * scaleBase, 0f, glowTex.Size() / 2f, 0.8f * scaleBase, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
