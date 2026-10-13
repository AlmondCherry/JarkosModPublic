using System;
using System.Collections.Generic;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.toms
{
    public class BloodLightningProj : ModProjectile
    {
        private List<Vector2> points = new List<Vector2>();

        public override string Texture => "Terraria/Images/Projectile_12";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 15; 
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
            Projectile.extraUpdates = 0;
            Projectile.ai[0] = -1; 
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;

                
                if (Projectile.ai[0] >= 0)
                {
                    NPC target = Main.npc[(int)Projectile.ai[0]];
                    if (target.active)
                    {
                        GenerateLightning(Projectile.Center, target.Center);
                    }
                }
            }
        }
        
        private void GenerateLightning(Vector2 start, Vector2 end)
        {
            points.Clear();
            points.Add(start);
            
            float distance = Vector2.Distance(start, end);
            Vector2 direction = Vector2.Normalize(end - start);
            int segments = (int)(distance / 30f);
            if (segments < 3) segments = 3;
            
            float step = distance / segments;
            
            Vector2 currentPos = start;
            for (int i = 1; i < segments; i++)
            {
                currentPos += direction * step;
                
                Vector2 offset = direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-20f, 20f);
                points.Add(currentPos + offset);
            }
            points.Add(end);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (points.Count < 2) return false;
            
            float collisionPoint = 0f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), points[i], points[i + 1], 10f, ref collisionPoint))
                {
                    return true;
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(target.Center, DustID.Blood, Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (points.Count < 2) return false;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Effect effect = HealShaders.BloodStream;
            if (effect != null && HealShaders.HealNoise != null)
            {
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f); 
                
                
                float lifeRatio = Projectile.timeLeft / 15f; 
                effect.Parameters["uOpacity"]?.SetValue(lifeRatio * 1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(180, 0, 20, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 50, 50, 255).ToVector4());
                
                TrailRenderer.Draw(effect, points.ToArray(), t => 12f * lifeRatio, t => Color.White, HealShaders.HealNoise);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
