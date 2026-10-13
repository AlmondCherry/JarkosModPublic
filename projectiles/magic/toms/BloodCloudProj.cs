using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.toms
{
    public class BloodCloudProj : ModProjectile
    {
        private Vector2 targetPos;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(targetPos.X);
            writer.Write(targetPos.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetPos.X = reader.ReadSingle();
            targetPos.Y = reader.ReadSingle();
        }

        public override string Texture => "jarkos/projectiles/magic/toms/BloodCloud";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 54;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 7200; 
            Projectile.alpha = 0;
            Projectile.ai[0] = 0; 
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                targetPos = Main.MouseWorld;
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5) 
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 6)
                {
                    Projectile.frame = 0;
                }
            }

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.ai[0] == 0) 
            {
                if (player.channel)
                {
                    Projectile.timeLeft = 7200;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        if (Vector2.Distance(targetPos, Main.MouseWorld) > 10f)
                        {
                            targetPos = Main.MouseWorld;
                            Projectile.netUpdate = true;
                        }
                    }

                    player.direction = (targetPos.X > player.Center.X) ? 1 : -1;
                    player.itemRotation = (targetPos - player.MountedCenter).ToRotation();
                    if (player.direction == -1) player.itemRotation += MathHelper.Pi;

                    
                    float speed = 12f;
                    float inertia = 10f;
                    
                    Vector2 direction = targetPos - Projectile.Center;
                    float distance = direction.Length();
                    
                    if (distance > 20f)
                    {
                        direction.Normalize();
                        direction *= speed;
                        Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                    }
                    else
                    {
                        Projectile.velocity *= 0.8f; 
                    }

                    
                    NPC closest = null;
                    float closestDist = 500f; 
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                        {
                            float dist = Vector2.Distance(Projectile.Center, npc.Center);
                            if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                            {
                                closestDist = dist;
                                closest = npc;
                            }
                        }
                    }

                    
                    if (closest != null)
                    {
                        Projectile.ai[1]++;
                        if (Projectile.ai[1] >= 12) 
                        {
                            Projectile.ai[1] = 0;

                            const int manaCost = 5;
                            if (Projectile.owner == Main.myPlayer)
                            {
                                if (player.statMana < manaCost)
                                {
                                    SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.Center);
                                    for (int i = 0; i < 15; i++)
                                    {
                                        Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(16), 32, 32, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                                        d.noGravity = true;
                                    }
                                    Projectile.Kill();
                                    return;
                                }
                                player.statMana -= manaCost;
                                player.manaRegenDelay = 180;

                                Vector2 velocity = Vector2.Normalize(closest.Center - Projectile.Center) * 16f;
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BloodLightningProj>(), Projectile.damage, Projectile.knockBack, player.whoAmI, closest.whoAmI);
                            }
                            if (Main.netMode != NetmodeID.Server)
                            {
                                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.4f }, Projectile.Center);
                            }
                        }
                    }
                }
                else
                {
                    
                    Projectile.ai[0] = 1; 
                    Projectile.ai[1] = 0; 
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[0] == 1) 
            {
                Projectile.ai[1]++; 

                
                Vector2 targetPos = player.MountedCenter;
                float dist = Vector2.Distance(Projectile.Center, targetPos);

                
                float returnSpeed = MathHelper.Lerp(2f, 25f, Math.Min(Projectile.ai[1] / 30f, 1f));
                Vector2 direction = targetPos - Projectile.Center;
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }

                if (dist > returnSpeed)
                {
                    Projectile.velocity = (Projectile.velocity * 4f + direction * returnSpeed) / 5f;
                }
                else
                {
                    Projectile.Kill();
                }

                
                Projectile.alpha += 8;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }
        
        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = sourceRect.Size() / 2f;
            Color drawColor = Projectile.GetAlpha(lightColor);
            
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
