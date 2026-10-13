using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;

namespace jarkos.projectiles.melee.unique_melee
{
    public class BerserkerChainLightning : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamFriendly;

        private List<Vector2> trail = new List<Vector2>();
        private int lastHitNPC = -1;
        private int targetNPC = -1;

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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; 
            Projectile.extraUpdates = 3; 
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; 
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; 
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                targetNPC = (int)Projectile.ai[0];
                Projectile.localAI[0] = 1;
            }

            trail.Add(Projectile.Center);
            if (trail.Count > 30)
            {
                trail.RemoveAt(0);
            }

            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.8f);

            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1f);
                d.noGravity = true;
                d.velocity *= 0.5f;
            }

            
            if (targetNPC == -1 || !Main.npc[targetNPC].active || Main.npc[targetNPC].life <= 0)
            {
                FindNewTarget();
            }

            
            if (targetNPC == -1)
            {
                Projectile.alpha += 15;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
                return;
            }

            
            NPC target = Main.npc[targetNPC];
            Vector2 dir = (target.Center - Projectile.Center);
            float dist = dir.Length();
            dir.Normalize();

            float speed = 12f;
            Projectile.velocity = dir * speed;

            
            if (dist < 20f)
            {
                
                if (Projectile.localNPCImmunity[targetNPC] == 0 && target.immune[Projectile.owner] == 0)
                {
                    target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, Projectile.DamageType);
                    Projectile.localNPCImmunity[targetNPC] = Projectile.localNPCHitCooldown;
                    
                    
                    for (int i = 0; i < 10; i++)
                    {
                        Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Electric, 0f, 0f, 100, Color.Cyan, 1.2f);
                        d.noGravity = true;
                        d.velocity *= 2f;
                    }

                    lastHitNPC = targetNPC;
                    targetNPC = -1; 
                    FindNewTarget();
                    
                    
                    if (targetNPC == -1)
                    {
                        Projectile.Kill();
                    }
                }
            }
        }

        private void FindNewTarget()
        {
            float minDistSq = 1200f * 1200f; 
            int bestTarget = -1;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    
                    if (i != lastHitNPC)
                    {
                        float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                        
                        if (distSq < minDistSq && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                        {
                            minDistSq = distSq;
                            bestTarget = i;
                        }
                    }
                }
            }

            
            if (bestTarget == -1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        if (i != lastHitNPC)
                        {
                            float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                            if (distSq < minDistSq)
                            {
                                minDistSq = distSq;
                                bestTarget = i;
                            }
                        }
                    }
                }
            }

            targetNPC = bestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (trail.Count < 2) return false;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect pixelShader = Main.pixelShader;

            List<Vector2> renderPoints = new List<Vector2>();
            
            for(int i = 0; i < trail.Count; i++)
            {
                if (i == 0 || i == trail.Count - 1) 
                {
                    renderPoints.Add(trail[i]);
                } 
                else 
                {
                    Vector2 offset = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    renderPoints.Add(trail[i] + offset);
                }
            }

            TrailRenderer.Draw(
                pixelShader, 
                renderPoints, 
                t => MathHelper.Lerp(10f, 0f, t), 
                t => Color.Cyan * (1f - t) * (1f - Projectile.alpha / 255f), 
                HealShaders.HealNoise 
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false; 
        }
    }
}
