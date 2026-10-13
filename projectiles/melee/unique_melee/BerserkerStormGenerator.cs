using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace jarkos.projectiles.melee.unique_melee
{
    public class BerserkerStormGenerator : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0"; 

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45; 
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            
            Projectile.Center = player.Center;

            
            if (Projectile.timeLeft == 45)
            {
                
                player.itemAnimation = 45;
                player.itemTime = 45;

                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, player.Center);
                    float radius = 400f;
                    for (int i = 0; i < 60; i++)
                    {
                        float angle = MathHelper.TwoPi / 60 * i;
                        Vector2 offset = angle.ToRotationVector2() * radius;
                        Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Electric, offset.SafeNormalize(Vector2.Zero) * 3f, 0, Color.Cyan, 1.5f);
                        d.noGravity = true;
                        
                        Dust inner = Dust.NewDustPerfect(player.Center, DustID.Electric, angle.ToRotationVector2() * Main.rand.NextFloat(5f, 15f), 0, Color.Cyan, 2f);
                        inner.noGravity = true;
                    }
                }

                if (Projectile.owner == Main.myPlayer)
                {
                    float radius = 400f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            if (Vector2.Distance(player.Center, npc.Center) <= radius)
                            {
                                npc.SimpleStrikeNPC(Projectile.damage, player.direction, false, 5f, Projectile.DamageType);
                                if (Main.netMode != NetmodeID.Server)
                                {
                                    for(int j = 0; j < 10; j++)
                                    {
                                        Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric, 0f, 0f, 0, Color.Cyan, 1f);
                                    }
                                }
                            }
                        }
                    }
                }

                if (Projectile.owner == Main.myPlayer)
                {
                    List<int> validTargets = new List<int>();
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            if (Vector2.DistanceSquared(player.Center, npc.Center) <= 800f * 800f)
                            {
                                if (Collision.CanHitLine(player.Center, 1, 1, npc.Center, 1, 1))
                                {
                                    validTargets.Add(i);
                                }
                            }
                        }
                    }

                    int lightningsToSpawn = Math.Min(6, validTargets.Count); 
                    
                    if (lightningsToSpawn > 1) 
                    {
                        for (int i = 0; i < validTargets.Count; i++)
                        {
                            int temp = validTargets[i];
                            int randomIndex = Main.rand.Next(i, validTargets.Count);
                            validTargets[i] = validTargets[randomIndex];
                            validTargets[randomIndex] = temp;
                        }

                        for (int i = 0; i < lightningsToSpawn; i++)
                        {
                            NPC target = Main.npc[validTargets[i % validTargets.Count]];
                            Vector2 dir = (target.Center - player.Center).SafeNormalize(Vector2.UnitY);
                            int lightningDamage = 25;
                            
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(), 
                                player.Center + new Vector2(0, -30f), 
                                dir * 12f, 
                                ModContent.ProjectileType<BerserkerChainLightning>(), 
                                lightningDamage, 
                                0f, 
                                player.whoAmI,
                                target.whoAmI
                            );
                        }
                    }
                }
                
                
                    

                    
                    
                    
                
            }

            
            player.direction = Math.Sign(Main.MouseWorld.X - player.Center.X);
            if (player.direction == 0) player.direction = 1;
            player.itemRotation = -MathHelper.PiOver4 * player.direction;
            player.heldProj = Projectile.whoAmI;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
