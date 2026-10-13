using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Ethereal_rift_planted : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Ethereal_rift";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = false; 
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600 + 30; 
            Projectile.netImportant = true;
        }
        public ref float PlantedRotation => ref Projectile.ai[0];
        
        private Vector2 originalPos;
        private bool initialized = false;

        public override void AI()
        {
            if (!initialized)
            {
                originalPos = Projectile.Center;
                initialized = true;
            }

            Player player = Main.player[Projectile.owner];
            Projectile.rotation = PlantedRotation;

            if (Projectile.timeLeft > 30)
            {
                Projectile.Center = originalPos;
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
            }
            else
            {
                float t = 1f - (Projectile.timeLeft / 30f); 
                float easeOut = MathSystem.EaseOutCubic(t);
                
                Projectile.Center = Vector2.Lerp(originalPos, player.MountedCenter, easeOut);
                
                Vector2 diff = player.MountedCenter - originalPos;
                float targetRotation = diff.ToRotation() + MathHelper.PiOver4;
                
                Projectile.rotation = MathHelper.Lerp(PlantedRotation, targetRotation, easeOut);
                
                if (Main.rand.NextBool(2))
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy, 0f, 0f, 100, default, 1.2f);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / 6;
            Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);
            Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, frameHeight * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            Main.spriteBatch.Draw(tex, drawPos, frameRect, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            
            Main.spriteBatch.Draw(tex, drawPos, frameRect, new Color(0, 204, 255) * 0.5f, Projectile.rotation, drawOrigin, Projectile.scale * 1.1f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
