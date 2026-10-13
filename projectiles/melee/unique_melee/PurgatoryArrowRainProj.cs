using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatoryArrowRainProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Misc/Perlin";
        private const int Lifetime = 60 * 3;
        private const int SpawnInterval = 3;
        private const int ArrowsPerWave = 2;
        private const float AreaRadius = 140f;
        private const float SpawnHeight = 380f;
        private const float ArrowSpawnSpeedY = 32f;
        private const int FadeFrames = 14;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 255;
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            if (Projectile.timeLeft % 6 == 0)
            {
                float angle = MathHelper.TwoPi * Main.rand.NextFloat();
                Vector2 edge = Projectile.Center + angle.ToRotationVector2() * AreaRadius;
                Vector2 toCenter = (Projectile.Center - edge).SafeNormalize(Vector2.Zero) * 3.5f;
                Dust Spark = Dust.NewDustPerfect(edge, DustID.YellowStarDust, toCenter, Scale: 1.3f);
                Spark.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.85f, 0.7f, 0.25f);
            if (Projectile.owner != Main.myPlayer)
            {
                return;
            }
            float life = 1f - Projectile.timeLeft / (float)Lifetime;
            if (Projectile.timeLeft % SpawnInterval != 0)
            {
                return;
            }
            if (life > 0.95f)
            {
                return;
            }
            Player player = Main.player[Projectile.owner];
            int arrowDamage = (int)Math.Max(1, Projectile.damage * 0.65f);
            int arrowType = ModContent.ProjectileType<PurgatoryHealingArrowProj>();
            var source = new EntitySource_Parent(Projectile);
            for (int i = 0; i < ArrowsPerWave; i++)
            {
                float offX = Main.rand.NextFloat(-AreaRadius, AreaRadius);
                Vector2 spawnPos = Projectile.Center + new Vector2(offX, -SpawnHeight);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.6f, 1.6f), ArrowSpawnSpeedY);
                Projectile.NewProjectile(source, spawnPos, vel, arrowType, arrowDamage, 1.5f, player.whoAmI);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTelegraphLane();
            return false;
        }
        private void DrawTelegraphLane()
        {
            int age = Lifetime - Projectile.timeLeft;
            float scaleX;
            if (age < FadeFrames)
            {
                scaleX = SmoothStep(age / (float)FadeFrames);
            }
            else if (Projectile.timeLeft < FadeFrames)
            {
                scaleX = SmoothStep(Projectile.timeLeft / (float)FadeFrames);
            }
            else
            {
                scaleX = 1f;
            }
            float progress = MathHelper.Clamp(age / (float)Lifetime, 0f, 1f);
            float laneWidth = AreaRadius * 2f;
            float laneHeight = SpawnHeight;
            Vector2 laneCenter = Projectile.Center + new Vector2(0f, -laneHeight * 0.5f);
            Vector2 drawPos = laneCenter - Main.screenPosition;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            MiscShaderData shaderData = GameShaders.Misc[PurgatoryShaders.ArrowRainTelegraphKey];
            Effect effect = shaderData.Shader;
            effect.Parameters["uBaseColor"]?.SetValue(new Vector3(0.55f, 0.10f, 0.18f));
            effect.Parameters["uAccentColor"]?.SetValue(new Vector3(1.00f, 0.45f, 0.62f));
            effect.Parameters["uGlowColor"]?.SetValue(new Vector3(1.00f, 0.85f, 0.45f));
            effect.Parameters["uProgress"]?.SetValue(progress);
            effect.Parameters["uPixelMix"]?.SetValue(0f);
            effect.Parameters["uOpacity"]?.SetValue(0.85f * scaleX);
            shaderData.UseOpacity(0.85f * scaleX).Apply();
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 origin = new Vector2(0.5f, 0.5f);
            Vector2 scale = new Vector2(laneWidth * scaleX, laneHeight);
            Main.spriteBatch.Draw(pixel, drawPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private static float SmoothStep(float x)
        {
            x = MathHelper.Clamp(x, 0f, 1f);
            return x * x * (3f - 2f * x);
        }
    }
}
