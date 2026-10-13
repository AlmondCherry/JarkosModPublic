using System;
using System.Collections.Generic;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.toms
{
    public class GrimoireOfSacrifice_laser : ModProjectile
    {
        private List<Vector2> ropeSegments = new List<Vector2>();
        private Vector2 controlPoint = Vector2.Zero;

        public override string Texture => "Terraria/Images/Projectile_12";

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ai[0] = -1; 
            Projectile.ai[1] = 0;  
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                if (player.channel)
                {
                    Projectile.timeLeft = 300; 

                    Vector2 aimDir = Main.MouseWorld - player.MountedCenter;
                    if (aimDir != Vector2.Zero) aimDir.Normalize();

                    if (Projectile.velocity != aimDir)
                    {
                        Projectile.velocity = aimDir;
                        Projectile.netUpdate = true;
                    }

                    Vector2 startPosTarget = player.MountedCenter + new Vector2(player.direction * 10f, 0).RotatedBy(aimDir.ToRotation());
                    float maxLength = 600f;
                    NPC closest = null;
                    float closestDist = 200f; 
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                        {
                            if (Vector2.Distance(startPosTarget, npc.Center) <= maxLength)
                            {
                                float dist = Vector2.Distance(Main.MouseWorld, npc.Center);
                                if (dist < closestDist && Collision.CanHitLine(player.MountedCenter, 1, 1, npc.Center, 1, 1))
                                {
                                    closestDist = dist;
                                    closest = npc;
                                }
                            }
                        }
                    }
                    
                    if (closest != null)
                    {
                        if (Projectile.ai[0] != closest.whoAmI)
                        {
                            Projectile.ai[0] = closest.whoAmI;
                            Projectile.ai[1] = 0; 
                            Projectile.netUpdate = true; 
                            SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.5f }, closest.Center);
                        }
                        
                        NPC target = closest;
                        Projectile.ai[1]++; 

                        int rampedManaCost = 3 + (int)(Projectile.ai[1] / 90f) * 2;
                        if (Projectile.localAI[1] % 20 == 0)
                        {
                            if (player.statMana < rampedManaCost)
                            {
                                player.channel = false; 
                                Projectile.Kill();
                                return;
                            }
                            player.statMana -= rampedManaCost;
                            player.manaRegenDelay = 180;
                        }

                        if (Projectile.localAI[1] % 10 == 0)
                        {
                            float damageMult = 1.0f + (Projectile.ai[1] / 60f) * 0.35f;
                            int currentDamage = (int)(Projectile.damage * damageMult);

                            NPC.HitInfo periodicHit = new NPC.HitInfo
                            {
                                Damage = currentDamage,
                                DamageType = DamageClass.Magic,
                                HitDirection = Math.Sign(target.Center.X - player.Center.X),
                                Knockback = 0f
                            };
                            target.StrikeNPC(periodicHit);

                            Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20, 20), DustID.Blood, Main.rand.NextVector2Circular(3, 3), 0, default, 1.2f + Math.Min(damageMult * 0.15f, 1.5f));
                            SoundEngine.PlaySound(SoundID.Item103 with { Volume = Math.Min(0.3f + damageMult * 0.08f, 0.8f), Pitch = Math.Min(0.1f + damageMult * 0.08f, 0.7f) }, target.Center);
                        }
                    }
                    else
                    {
                        if (Projectile.ai[0] != -1)
                        {
                            Projectile.ai[0] = -1; 
                            Projectile.ai[1] = 0;
                            Projectile.netUpdate = true; 
                        }
                    }
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
            }

            Projectile.Center = player.MountedCenter;
            player.itemTime = 2;
            player.itemAnimation = 2;
            if (Projectile.velocity != Vector2.Zero)
            {
                player.direction = (Projectile.velocity.X > 0) ? 1 : -1;
                player.itemRotation = Projectile.velocity.ToRotation();
                if (player.direction == -1) player.itemRotation += MathHelper.Pi;
            }

            Projectile.localAI[1]++; 

            Vector2 startPosClient = player.MountedCenter + new Vector2(player.direction * 10f, 0).RotatedBy(Projectile.velocity.ToRotation());
            float maxLengthDrawClient = 600f;
            Vector2 endPosClient = player.MountedCenter + Projectile.velocity * maxLengthDrawClient;
            
            if (Projectile.ai[0] != -1)
            {
                NPC target = Main.npc[(int)Projectile.ai[0]];
                if (target.active)
                {
                    endPosClient = target.Center;
                }
            }
            
            if (Vector2.Distance(startPosClient, endPosClient) > maxLengthDrawClient)
            {
                endPosClient = startPosClient + Vector2.Normalize(endPosClient - startPosClient) * maxLengthDrawClient;
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.5f);
                new jarkos.particles.Spark(endPosClient, sparkVel, Color.Red, 30, Main.rand.NextFloat(0.5f, 1.2f), 1f, 0f).Spawn();
            }

            if (ropeSegments.Count == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    ropeSegments.Add(Vector2.Zero);
                }
                controlPoint = Vector2.Lerp(startPosClient, endPosClient, 0.5f);
            }
            
            Vector2 idealMid = Vector2.Lerp(startPosClient, endPosClient, 0.5f);
            controlPoint = Vector2.Lerp(controlPoint, idealMid, 0.15f);
            
            for (int i = 0; i < 20; i++)
            {
                float t = i / 19f;
                ropeSegments[i] = (1 - t) * (1 - t) * startPosClient + 2 * (1 - t) * t * controlPoint + t * t * endPosClient;
            }
        }

        public override bool? CanDamage() => false; 

        public override bool PreDraw(ref Color lightColor)
        {
            if (ropeSegments.Count < 20) return false;
            
            Vector2[] points = ropeSegments.ToArray();
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Effect effect = HealShaders.BloodStream;
            if (effect != null && HealShaders.HealNoise != null)
            {
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(180, 0, 20, 255).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 50, 50, 255).ToVector4());
                
                TrailRenderer.Draw(effect, points, t => 14f, t => Color.White, HealShaders.HealNoise);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
