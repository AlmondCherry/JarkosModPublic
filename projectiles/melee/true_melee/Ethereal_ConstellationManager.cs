using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Ethereal_ConstellationManager : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Ethereal_rift"; 
        
        public List<Vector2> StarOffsets = new List<Vector2>();
        
        public ref float TargetNPC => ref Projectile.ai[0];
        
        private const int ANIM_TIME = 60; 
        private const int TOTAL_LIFETIME = 120; 
        
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TOTAL_LIFETIME;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
        }

        public override void OnSpawn(IEntitySource source)
        {
            int numStars = Main.rand.Next(3, 5); 
            for (int i = 0; i < numStars; i++)
            {
                
                Vector2 offset = Main.rand.NextVector2Circular(150f, 150f);
                StarOffsets.Add(offset);
            }
        }

        public override void AI()
        {
            
            Projectile.velocity = Vector2.Zero;
            
            if (Projectile.timeLeft == 1) 
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < StarOffsets.Count; i++)
                    {
                        Vector2 spawnPos = Projectile.Center + StarOffsets[i];
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<Ethereal_StarProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner, TargetNPC, i, Projectile.ai[2]);
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (StarOffsets.Count == 0) return false;
            
            float timer = TOTAL_LIFETIME - Projectile.timeLeft; 
            
            Texture2D starTex = ModContent.Request<Texture2D>("jarkos/particles/StarParticle").Value;
            Texture2D lineTex = TextureAssets.MagicPixel.Value;
            
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            float timePerStar = ANIM_TIME / (float)StarOffsets.Count;
            
            for (int i = 0; i < StarOffsets.Count; i++)
            {
                float appearStartTime = i * timePerStar;
                if (timer < appearStartTime) continue; 
                
                float appearProgress = MathHelper.Clamp((timer - appearStartTime) / timePerStar, 0f, 1f);
                float easeOut = 1f - (1f - appearProgress) * (1f - appearProgress); 
                
                Vector2 currentPos = Projectile.Center + StarOffsets[i];
                
                
                if (i > 0)
                {
                    Vector2 prevPos = Projectile.Center + StarOffsets[i - 1];
                    float prevAppearProgress = MathHelper.Clamp((timer - (i - 1) * timePerStar) / timePerStar, 0f, 1f);
                    
                    if (prevAppearProgress >= 1f)
                    {
                        Vector2 diff = currentPos - prevPos;
                        float dist = diff.Length() * easeOut; 
                        float angle = diff.ToRotation();
                        
                        Color lineColor = Ethereal_rift_dante_swing.ColorCyan * 0.8f;
                        Main.spriteBatch.Draw(lineTex, prevPos - Main.screenPosition, new Rectangle(0, 0, 1, 1), lineColor, angle, new Vector2(0, 0.5f), new Vector2(dist, 2f), SpriteEffects.None, 0f);
                    }
                }
                
                
                Color starColor = Color.White;
                starColor.A = 0;
                
                float baseSize = 0.7f + (float)Math.Sin(i * 123.45f) * 0.4f;
                float starScale = easeOut * 0.5f * baseSize;
                
                
                if (appearProgress >= 1f)
                {
                    starScale += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f + i * 2f) * 0.15f * baseSize;
                }
                
                Main.spriteBatch.Draw(starTex, currentPos - Main.screenPosition, null, starColor, timer * 0.01f, starTex.Size() / 2f, starScale * 0.4f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(starTex, currentPos - Main.screenPosition, null, starColor, timer * 0.01f, starTex.Size() / 2f, starScale * 0.4f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(starTex, currentPos - Main.screenPosition, null, Ethereal_rift_dante_swing.ColorCyan * 0.8f, timer * -0.01f, starTex.Size() / 2f, starScale * 0.6f, SpriteEffects.None, 0f);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
}
