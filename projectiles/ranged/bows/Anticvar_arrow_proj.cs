using System;
using System.Collections.Generic;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.ranged.bows
{
    public class Anticvar_arrow_proj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.IchorArrow;

        private List<Vector2> trail = new List<Vector2>();
        private bool hitInitialTarget = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 20; 
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
        }

        public override void AI()
        {
            trail.Add(Projectile.Center);
            if (trail.Count > 30)
            {
                trail.RemoveAt(0);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 0.5f);

            
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }

            
            NPC target = null;
            float minDistSq = 800f * 800f; 
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    
                    if (Projectile.localNPCImmunity[i] == 0)
                    {
                        float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                        
                        if (distSq < minDistSq && (Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1) || hitInitialTarget))
                        {
                            minDistSq = distSq;
                            target = npc;
                        }
                    }
                }
            }

            if (target != null)
            {
                Vector2 targetPos = target.Center;
                Vector2 dir = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                float speed = Projectile.velocity.Length();
                if (speed < 15f) speed += 0.5f; 
                
                
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.08f);
            }
            else
            {
                
                Projectile.velocity.Y += 0.1f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitInitialTarget = true;
            
            for (int i = 0; i < 15; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GoldFlame, 0f, 0f, 100, default, 2f);
                d.noGravity = true;
                d.velocity *= 3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (trail.Count < 2) return false;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            
            Effect pixelShader = Main.pixelShader;

            List<Vector2> reverseTrail = new List<Vector2>(trail);
            reverseTrail.Reverse(); 

            TrailRenderer.Draw(
                pixelShader, 
                reverseTrail, 
                t => MathHelper.Lerp(12f, 0f, t), 
                t => Color.Gold * (1f - t) * 0.8f, 
                HealShaders.HealNoise 
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, Projectile.height * 0.5f);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
