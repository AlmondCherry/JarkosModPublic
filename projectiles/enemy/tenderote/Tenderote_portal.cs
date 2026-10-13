using jarkos.npc.tenderote;
using Jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_portal : ModProjectile
    {
        private int beatCount = 0;
        private int timer = 0;
        private float targetScale = 0f;
        private float currentScale = 0f;
        public override string Texture => "Terraria/Images/Projectile_438";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }
        public override void AI()
        {
            Player player = Main.player[Main.myPlayer];
            if (Projectile.localAI[0] == 0)
            {
                Projectile.Center = player.Center + new Vector2(0, -300);
                Projectile.localAI[0] = 1;
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, (player.Center + new Vector2(0, -250) - Projectile.Center).SafeNormalize(Vector2.Zero) * 2f, 0.05f);
            timer++;
            Jarkos.systems.TenderodScreenSystem.PortalPosition = Projectile.Center;
            if (timer % 60 == 0 && beatCount < 6)
            {
                beatCount++;
                targetScale = beatCount * 1.5f;
                TenderodScreenSystem.ScreenShakeAmount = 15f;
                TenderodScreenSystem.TargetDarken = beatCount / 6f;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.8f }, Projectile.Center);
            }
            currentScale = MathHelper.Lerp(currentScale, targetScale, 0.1f);
            Projectile.scale = currentScale;
            if (beatCount >= 6 && timer > 360 + 90)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.NewNPC(Projectile.GetSource_FromAI(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<Tenderote_head>());
                }
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, drawPos, new Rectangle(0,0,1,1), Color.Black, 0f, new Vector2(0.5f), currentScale * 150f, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.1f;
            Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, drawPos, new Rectangle(0,0,1,1), Color.Purple * 0.5f, 0f, new Vector2(0.5f), currentScale * 160f * pulse, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
