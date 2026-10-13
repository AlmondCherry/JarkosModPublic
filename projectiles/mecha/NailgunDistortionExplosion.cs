using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using jarkos.particles;
namespace Jarkos.projectiles.mecha
{
    public class NailgunDistortionExplosion : ModProjectile
    {
        public override string Texture => "jarkos/particles/Circular_glow";
        private const int MaxTimeLeft = 20;
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.DamageType = DamageClass.Ranged;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == MaxTimeLeft)
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.5f }, Projectile.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 sparkVel = Main.rand.NextVector2Circular(15f, 15f);
                        new Spark(Projectile.Center, sparkVel, new Color(255, 120, 30), 20, Main.rand.NextFloat(0.8f, 1.5f), 1f).Spawn();
                    }
                }
            }
            Projectile.scale += 0.2f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float progress = 1f - (Projectile.timeLeft / (float)MaxTimeLeft);
            if (GameShaders.Misc.TryGetValue("BlackHole", out MiscShaderData shader))
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                shader.Apply();
                spriteBatch.Draw(texture, drawPos, null, Color.White * (1f - progress), 0f, texture.Size() / 2f, Projectile.scale * 1.5f, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            float coreScale = Projectile.scale * (1f - progress);
            spriteBatch.Draw(texture, drawPos, null, new Color(255, 100, 20) * (1f - progress), 0f, texture.Size() / 2f, coreScale * 2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * (1f - progress), 0f, texture.Size() / 2f, coreScale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
