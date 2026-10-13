using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.ranged.firearm
{
    public class Jupiter_water_bullet : ModProjectile
    {
        private List<Vector2> trailPositions;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.WaterStream;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 4;
        }
        public override void OnSpawn(IEntitySource source)
        {
            trailPositions = new List<Vector2>();
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > 15)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 1.2f);
                d.noGravity = true;
                d.velocity *= 0.1f;
            }
            float homingRadius = 250f;
            NPC target = null;
            float closestDist = homingRadius;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
            }
            if (target != null)
            {
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * Projectile.velocity.Length(), 0.08f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 1.5f);
                d.noGravity = false;
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                try
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 5f);
                    effect.Parameters["uOpacity"]?.SetValue(1.2f);
                    effect.Parameters["uColor"]?.SetValue(new Color(50, 150, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 255).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(5f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
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
