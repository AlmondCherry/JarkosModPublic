using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.particles;

namespace Jarkos.projectiles.mecha
{
    public class Atom_proj : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 12;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = MaxTrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
            AIType = ProjectileID.MeteorShot;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.scale = 0.8f;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0f, 0.6f, 0.2f);
            UpdateTrail();
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                dust.velocity = -Projectile.velocity * 0.15f;
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = new Color(0, 255, 120);
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, speed, 0, default, 1.3f);
                d.noGravity = true;
                d.color = new Color(0, 255, 100);
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                new Spark(Projectile.Center, speed, new Color(0, 255, 120), 12, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.Defense *= 0f;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, speed, 0, default, 1.1f);
                d.noGravity = true;
                d.color = new Color(0, 255, 100);
            }
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null && trailPositions.Count > 1)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f + Projectile.identity * 0.05f);
                effect.Parameters["uOpacity"]?.SetValue(1.3f);
                effect.Parameters["uColor"]?.SetValue(new Color(0, 255, 80).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 180).ToVector4());
                TrailRenderer.Draw(effect, trailPositions, t => 12f * MathF.Pow(1f - t, 0.4f), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Color drawColor = new Color(200, 255, 200, 200);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }
    }
}
