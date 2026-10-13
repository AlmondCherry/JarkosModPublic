using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_grid_square : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 153;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (Projectile.ai[1] > 0)
            {
                Projectile.ai[1]--;
                Projectile.timeLeft++;
                return;
            }
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] == 90)
            {
                Projectile.hostile = true;
            }
            if (Projectile.localAI[0] > 110)
            {
                Projectile.hostile = false;
            }
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<buffs.debuff.TenderodWeaknessDebuff>(), 60 * 20);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[1] > 0) return false;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = TextureAssets.MagicPixel.Value;
            float opacity = 1f;
            float scale = 1f;
            if (Projectile.localAI[0] < 90)
            {
                opacity = (float)Math.Sin((Projectile.localAI[0] / 90f) * MathHelper.Pi) * 0.8f;
                scale = 1f;
            }
            else if (Projectile.localAI[0] < 110)
            {
                opacity = 1f;
                scale = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 30f) * 0.03f;
            }
            else
            {
                float progress = (Projectile.localAI[0] - 110f) / 43f;
                opacity = 1f - progress;
                scale = MathHelper.Lerp(1f, 0f, progress);
                if (opacity < 0f) opacity = 0f;
                if (scale < 0f) scale = 0f;
            }
            Vector2 centerPos = Projectile.position + new Vector2(Projectile.width / 2f, Projectile.height / 2f) - Main.screenPosition;
            Vector2 drawSize = new Vector2(Projectile.width * scale, Projectile.height * scale);
            spriteBatch.Draw(tex, centerPos, new Rectangle(0,0,1,1), Color.Black * opacity, 0f, new Vector2(0.5f), drawSize, SpriteEffects.None, 0f);
            Color outlineColor = Color.White * opacity;
            float borderThickness = 3f * scale;
            if (borderThickness < 1f) borderThickness = 1f;
            Vector2 topleft = centerPos - new Vector2(drawSize.X / 2f, drawSize.Y / 2f);
            spriteBatch.Draw(tex, topleft, new Rectangle(0,0,1,1), outlineColor, 0f, Vector2.Zero, new Vector2(drawSize.X, borderThickness), SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, topleft + new Vector2(0, drawSize.Y - borderThickness), new Rectangle(0,0,1,1), outlineColor, 0f, Vector2.Zero, new Vector2(drawSize.X, borderThickness), SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, topleft, new Rectangle(0,0,1,1), outlineColor, 0f, Vector2.Zero, new Vector2(borderThickness, drawSize.Y), SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, topleft + new Vector2(drawSize.X - borderThickness, 0), new Rectangle(0,0,1,1), outlineColor, 0f, Vector2.Zero, new Vector2(borderThickness, drawSize.Y), SpriteEffects.None, 0f);
            return false;
        }
    }
}
