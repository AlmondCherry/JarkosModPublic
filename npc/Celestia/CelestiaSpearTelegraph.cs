using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using jarkos.particles;
using Terraria.GameContent;

namespace jarkos.npc.Celestia
{
    public class CelestiaSpearTelegraph : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_89";
        public ref float SlantAngle => ref Projectile.ai[0];
        public ref float CustomDuration => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                float duration = CustomDuration > 0f ? CustomDuration : 36f;
                Projectile.localAI[0] = duration;
                Projectile.timeLeft = (int)duration;
            }

            Projectile.rotation = SlantAngle;

            float totalTime = Projectile.localAI[0] > 0 ? Projectile.localAI[0] : 36f;
            float progress = 1f - Projectile.timeLeft / totalTime;
            float opacity = (float)Math.Sin(progress * MathHelper.Pi);

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                Vector2 dir = Vector2.UnitY.RotatedBy(SlantAngle);
                Vector2 spawnPos = Projectile.Center + dir * Main.rand.NextFloat(-1400f, 1400f) + dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-12f, 12f);
                new StarParticle(spawnPos, dir * Main.rand.NextFloat(6f, 18f), Color.Cyan * opacity, 18, MathHelper.Clamp(Main.rand.NextFloat(0.15f, 0.4f), 0.05f, 0.45f), 1f, 0f).Spawn();
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 shotDir = Vector2.UnitY.RotatedBy(SlantAngle);
                Vector2 spearSpawnPos = Projectile.Center - shotDir * 1200f;
                Vector2 spearVel = shotDir * 46f;

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), spearSpawnPos, spearVel, ModContent.ProjectileType<CelestiaSpear>(), 50, 1f, Main.myPlayer);
            }

            SoundEngine.PlaySound(SoundID.Item72 with { Pitch = 0.5f, Volume = 0.85f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D extra89 = TextureAssets.Extra[ExtrasID.SharpTears].Value;

            float totalTime = Projectile.localAI[0] > 0 ? Projectile.localAI[0] : 36f;
            float progress = 1f - Projectile.timeLeft / totalTime;
            float opacity = MathHelper.Clamp((float)Math.Sin(progress * MathHelper.Pi), 0.35f, 1f);
            
            float flash = progress > 0.7f ? 1.5f : 1.0f;

            Color outerColor = Color.DeepSkyBlue * (opacity * 0.75f * flash);
            Color midColor = Color.Cyan * (opacity * 0.9f * flash);
            Color coreColor = Color.White * (opacity * 1.0f * flash);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float drawRotation = SlantAngle;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            spriteBatch.Draw(extra89, drawPos, null, outerColor * 0.5f, drawRotation, new Vector2(extra89.Width * 0.5f, extra89.Height * 0.5f), new Vector2(4.5f, 9000f / extra89.Height), SpriteEffects.None, 0f);

            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), outerColor, drawRotation, new Vector2(0.5f, 0.5f), new Vector2(18f, 9000f), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), midColor, drawRotation, new Vector2(0.5f, 0.5f), new Vector2(9f, 9000f), SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), coreColor, drawRotation, new Vector2(0.5f, 0.5f), new Vector2(4f, 9000f), SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
