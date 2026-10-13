using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using Terraria.GameContent;

namespace jarkos.npc.Celestia
{
    public class CelestiaSpear : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 64;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 pVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color pColor = Main.rand.NextBool() ? Color.Cyan : Color.DeepSkyBlue;
                new StarParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), pVel, pColor, 20, Main.rand.NextFloat(0.1f, 0.3f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();

                new Spark(Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(3f, 3f), Color.White, 15, 0.6f, 1f, 0f).Spawn();
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 20; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(8f, 8f);
                    Color burstColor = Main.rand.NextBool() ? Color.Cyan : Color.White;
                    new StarParticle(Projectile.Center, burstVel, burstColor, 25, MathHelper.Clamp(Main.rand.NextFloat(0.2f, 0.45f), 0.05f, 0.45f), 1f, Main.rand.NextFloat(-0.3f, 0.3f)).Spawn();
                }

                for (int i = 0; i < 8; i++)
                {
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(10f, 10f), Color.DeepSkyBlue, 20, 0.8f, 1f, 0f).Spawn();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null,Main.GameViewMatrix.ZoomMatrix);

            spriteBatch.Draw(texture, drawPos, null, Color.Cyan * 0.9f, Projectile.rotation, drawOrigin, new Vector2(1.2f, 1.4f), SpriteEffects.None, 0f);

            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, Projectile.rotation, drawOrigin, new Vector2(0.8f, 1.1f), SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
