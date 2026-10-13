using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Lazatep_sword_lightning : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_12"; 

        private List<Vector2> points = new List<Vector2>();
        private List<int> hitNPCs = new List<int>();

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (points.Count == 0)
            {
                points.Add(Projectile.Center);
                hitNPCs.Add((int)Projectile.ai[0]);
                Projectile.ai[2] = Projectile.ai[1]; 
                Projectile.ai[1] = 0; 
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
            }

            if (Projectile.ai[2] > 0)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] >= 4)
                {
                    Projectile.ai[1] = 0;
                    
                    NPC closest = null;
                    float closestDist = 500f;
                    Vector2 lastPoint = points[points.Count - 1];

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !hitNPCs.Contains(i))
                        {
                            float dist = Vector2.Distance(lastPoint, npc.Center);
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closest = npc;
                            }
                        }
                    }

                    if (closest != null)
                    {
                        points.Add(closest.Center);
                        hitNPCs.Add(closest.whoAmI);
                        
                        float damageMult = (float)Math.Pow(0.95, 7 - Projectile.ai[2]);
                        int dmg = (int)(Projectile.damage * damageMult);
                        
                        NPC.HitInfo hitInfo = new NPC.HitInfo
                        {
                            Damage = dmg,
                            DamageType = DamageClass.Melee,
                            HitDirection = Math.Sign(closest.Center.X - lastPoint.X),
                            Knockback = Projectile.knockBack
                        };
                        closest.StrikeNPC(hitInfo);
                        
                        SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.3f, Pitch = 0.5f }, closest.Center);
                        
                        for (int i = 0; i < 15; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.5f);
                            Dust dust = Dust.NewDustPerfect(closest.Center, DustID.Electric, vel, 0, default, 1.2f);
                            dust.noGravity = true;
                        }

                        Projectile.ai[2]--;
                    }
                    else
                    {
                        Projectile.ai[2] = 0;
                    }
                }
            }
            else
            {
                Projectile.alpha += 15;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }

        public override bool? CanDamage() => false; 

        public override bool PreDraw(ref Color lightColor)
        {
            if (points.Count < 2) return false;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float opacity = (255f - Projectile.alpha) / 255f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[i + 1];
                
                float dist = Vector2.Distance(start, end);
                float rot = (end - start).ToRotation();
                
                Vector2 drawPos = start - Main.screenPosition;
                Vector2 origin = new Vector2(0, 0.5f);

                
                Color outerColor = new Color(0, 200, 255, 255) * opacity;
                Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), outerColor, rot, origin, new Vector2(dist, 8f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), outerColor * 0.5f, rot, origin, new Vector2(dist, 16f), SpriteEffects.None, 0f);

                
                Color coreColor = new Color(255, 255, 255, 255) * opacity;
                Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), coreColor, rot, origin, new Vector2(dist, 3f), SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
