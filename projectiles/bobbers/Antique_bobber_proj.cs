using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
namespace jarkos.projectiles.bobbers
{
	public class Antique_bobber_proj : ModProjectile
	{
        private float glowPulse = 0f;
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BobberWooden);
            Projectile.width = 14;
            Projectile.height = 22;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            DrawOriginOffsetY = -8;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            glowPulse += 0.05f;
            bool inWater = Projectile.wet;
            if (inWater)
            {
                if (Main.rand.NextBool(8))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldCoin, 0f, -1f, 150, default, 0.8f);
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }
                Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.7f, 0.3f) * 0.4f);
            }
            else
            {
                Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.7f, 0.3f) * 0.2f);
            }
            if (inWater && Projectile.ai[0] != 0f)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, -2f, 100, default, 1.2f);
                }
            }
        }
        public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineColor = new Color(218, 165, 32, 200);
            lineOriginOffset = new Vector2(47, -32);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            float pulse = 0.8f + (float)Math.Sin(glowPulse) * 0.2f;
            Color glowColor = new Color(255, 200, 100, 0) * pulse * 0.5f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, Projectile.rotation, drawOrigin, Projectile.scale * 1.3f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
