using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.particles;

namespace jarkos.projectiles.magic.toms
{
    public class BloodClotFragment : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.2f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, 0, 0, 100, default, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 speed = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 8f);
                if (speed == Vector2.Zero) speed = Main.rand.NextVector2Circular(4f, 4f);
                new Spark(target.Center, speed, new Color(255, 30, 30), 15, Main.rand.NextFloat(0.5f, 1f), 1f, 0f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 1.2f);
                d.noGravity = false;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.2f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 30, 30).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 120, 120).ToVector4());
                int numPoints = Projectile.oldPos.Length;
                Vector2[] points = new Vector2[numPoints];
                Vector2 half = Projectile.Size * 0.5f;
                for (int i = 0; i < numPoints; i++)
                {
                    points[i] = Projectile.oldPos[i] == Vector2.Zero ? Projectile.Center : Projectile.oldPos[i] + half;
                }
                TrailRenderer.Draw(effect, points, t => 16f * MathF.Pow(1f - t, 0.3f), t => Color.White * (1f - t), HealShaders.HealNoise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
