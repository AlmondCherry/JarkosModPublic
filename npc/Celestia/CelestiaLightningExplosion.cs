using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Audio;
using jarkos.particles;

namespace jarkos.npc.Celestia
{
    public class CelestiaLightningExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private const int Lifetime = 55;
        private static Texture2D blankTexture;

        private float Progress => MathHelper.Clamp(1f - Projectile.timeLeft / (float)Lifetime, 0f, 1f);

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            float scaleMultiplier = Projectile.ai[0] > 0f ? Projectile.ai[0] : 1f;

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.1f, Pitch = -0.3f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.85f }, Projectile.Center);

                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 20f * scaleMultiplier, 8f, 35, 2500f, "CelestiaExplosion"));

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        new Spark(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(6f, 16f) * scaleMultiplier, i % 3 == 0 ? Color.White : new Color(80, 210, 255), 28, 0.7f * scaleMultiplier, 1f, 0f).Spawn();
                    }
                }
            }

            if (Main.netMode != NetmodeID.Server && Progress < 0.85f)
            {
                float ringR = (40f + Progress * 420f) * scaleMultiplier;
                for (int i = 0; i < 3; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(ringR, ringR), DustID.Electric, Main.rand.NextVector2Circular(2f, 2f));
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.9f, 1.7f);
                }
            }

            float fade = MathF.Sin((1f - Progress) * MathHelper.PiOver2);
            Lighting.AddLight(Projectile.Center, 1.2f * fade * scaleMultiplier, 1.9f * fade * scaleMultiplier, 2.8f * fade * scaleMultiplier);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Progress > 0.85f) return false;

            float scaleMultiplier = Projectile.ai[0] > 0f ? Projectile.ai[0] : 1f;
            float ringRadius = (35f + Progress * 430f) * scaleMultiplier;
            float ringWidth = 65f * scaleMultiplier;

            Vector2 closest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, closest);

            if (dist >= Math.Max(0f, ringRadius - ringWidth) && dist <= ringRadius + ringWidth)
            {
                return true;
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            float scaleMultiplier = Projectile.ai[0] > 0f ? Projectile.ai[0] : 1f;

            Effect shader = ModContent.Request<Effect>("jarkos/Effects/CelestiaLightningExplosion", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 3.77f);
            shader.Parameters["uProgress"]?.SetValue(Progress);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader, Main.GameViewMatrix.ZoomMatrix);

            float quadSize = (1000f + Progress * 350f) * scaleMultiplier;
            Main.spriteBatch.Draw(blankTexture, Projectile.Center - Main.screenPosition, null, Color.White, 0f, new Vector2(0.5f), new Vector2(quadSize), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
