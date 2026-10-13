using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{

    public class CelestiaSuckedStar : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FallingStar;
        private static readonly Color StarGold = new Color(255, 220, 130);
        private static readonly Color Violet = new Color(150, 60, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Vector2 sink = new Vector2(Projectile.ai[0], Projectile.ai[1]);

            Vector2 toSink = (sink - Projectile.Center).SafeNormalize(Vector2.Zero);
            
            float speed = Projectile.velocity.Length();
            speed = MathHelper.Min(speed * 1.022f + 0.08f, 26f);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toSink, 0.09f).SafeNormalize(Vector2.Zero) * speed;
            Projectile.rotation += 0.25f * Math.Sign(Projectile.velocity.X == 0 ? 1 : Projectile.velocity.X);

            if (Vector2.DistanceSquared(Projectile.Center, sink) < 70f * 70f)
            {
                if (Main.netMode != NetmodeID.Server)
                    for (int i = 0; i < 6; i++)
                        new Spark(Projectile.Center,Main.rand.NextVector2Circular(4f, 4f), Violet, 16, 0.5f, 1f, 0f).Spawn();
                Projectile.Kill();
                return;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.YellowStarDust, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.1f, 1.8f);
            }

            Lighting.AddLight(Projectile.Center, 1.4f, 1.2f, 0.7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float t = 1f - i / (float)Projectile.oldPos.Length;
                Color c = Color.Lerp(Violet, StarGold, t) * (t * 0.85f);
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null, c, Projectile.oldRot[i], origin, t * 1.3f, SpriteEffects.None, 0f);
            }

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 10f + Projectile.whoAmI) * 0.15f;

            Main.spriteBatch.Draw(bloom, drawPos, null, StarGold * 0.95f, 0f, bloom.Size() * 0.5f, 1.6f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(bloom, drawPos, null, Color.White * 0.85f, MathHelper.PiOver4, bloom.Size() * 0.5f, 0.9f * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(bloom, drawPos, null, Color.White * 0.6f, Projectile.rotation * 0.3f, bloom.Size() * 0.5f, new Vector2(2.4f, 0.35f) * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(bloom, drawPos, null, Color.White * 0.6f, Projectile.rotation * 0.3f + MathHelper.PiOver2, bloom.Size() * 0.5f, new Vector2(2.4f, 0.35f) * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, 1.6f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
