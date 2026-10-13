using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.DataStructures;
using jarkos.items.tools;
using System;
namespace jarkos.projectiles.bobbers
{
	public class Mechanical_bobber_proj : ModProjectile
	{
		public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BobberWooden);
            Projectile.width = 14;
            Projectile.height = 22;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            DrawOriginOffsetY = -8;
        }
        public override void PostAI()
        {
            if (Projectile.ai[0] < 0f)
            {
                if (Main.GameUpdateCount % 6 == 0)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(5, 5), 10, 10, DustID.Electric);
                    dust.noGravity = true;
                    dust.velocity *= 0.2f;
                    dust.scale = 1.3f;
                }
            }
            Main.NewText(Projectile.ai[0]);
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                dust.noGravity = true;
                dust.scale = 0.9f;
            }
        }
        public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(43, -30);
            lineColor = Color.Red;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0, DrawOriginOffsetY);
            Color drawColor = lightColor;
            if (Projectile.ai[0] < 0f)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.5f) * 0.5f + 0.5f;
                drawColor = Color.Lerp(lightColor, Color.Cyan, pulse * 0.8f);
            }
            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
                dust.velocity *= 0.6f;
                dust.noGravity = true;
            }
        }
    }
}
