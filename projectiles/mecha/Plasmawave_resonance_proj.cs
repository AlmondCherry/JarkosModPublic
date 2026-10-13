using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
	public class Plasmawave_resonance_proj : ModProjectile
	{
		private float waveProgress = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 100;
        }
        public override void AI()
        {
            waveProgress = 1f - (Projectile.timeLeft / 60f);
            Projectile.scale = 1f + waveProgress * 2f;
            Projectile.alpha = (int)(100 + waveProgress * 155);
            Projectile.velocity *= 0.97f;
            if (Main.rand.NextBool(3))
            {
                Color dustColor = Color.Lerp(Color.Cyan, Color.Magenta, waveProgress);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.FireworksRGB, Projectile.velocity * 0.5f, 0, dustColor, 1.5f);
                dust.noGravity = true;
            }
            float lightIntensity = 1f - waveProgress;
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * lightIntensity);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 120);
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Electric, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D waveTex = ModContent.Request<Texture2D>("jarkos/projectiles/mecha/Plasmamirror_proj").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - waveProgress;
            for (int i = 0; i < 3; i++)
            {
                float layerScale = Projectile.scale * (1f + i * 0.2f);
                float layerAlpha = alpha * (1f - i * 0.25f);
                Color waveColor = Color.Lerp(Color.Cyan, Color.Magenta, waveProgress + i * 0.1f);
                waveColor *= layerAlpha;
                spriteBatch.Draw(waveTex, drawPos, null, waveColor, Projectile.velocity.ToRotation(), waveTex.Size() / 2f, layerScale, SpriteEffects.None, 0f);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
	}
}
