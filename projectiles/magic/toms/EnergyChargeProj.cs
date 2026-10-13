using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.items.weapons.magic.toms;
using Terraria.GameContent;

namespace jarkos.projectiles.magic.toms
{
    public class EnergyChargeProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/magic/toms/EnergyNoise";
        private const int RETURN = 16;
        private static Asset<Effect> chargeShader;
        private static bool loadAttempted;
        private int lastSec = -1;
        private int popTimer;
        private Player Owner => Main.player[Projectile.owner];
        private float Total => Math.Max(Projectile.ai[0], 1f);
        private float Progress => MathHelper.Clamp(1f - Projectile.timeLeft / Total, 0f, 1f);
        private float Return01 => 1f - MathHelper.Clamp(Projectile.timeLeft / (float)RETURN, 0f, 1f);
        private Vector2 HoloOffset => new Vector2(-Owner.direction * 58f, -56f + 6f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.7f));
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }
        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.timeLeft = (int)Total;
                Projectile.Center = Owner.MountedCenter + HoloOffset * 0.3f;
            }
            if (Return01 > 0f)
            {
                Projectile.velocity = (Owner.MountedCenter - Projectile.Center) * (0.22f + 0.5f * Return01);
            }
            else
            {
                Vector2 target = Owner.MountedCenter + HoloOffset;
                Vector2 to = target - Projectile.Center;
                float dist = to.Length();

                float speed = MathHelper.Clamp(dist * dist * 0.0011f, 0.25f, 19f);
                Vector2 want = dist > 0.5f ? to * (speed / dist) : Vector2.Zero;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, want, 0.085f);
                if (dist > 700f)
                    Projectile.Center = target;
            }
            Lighting.AddLight(Projectile.Center, 0.12f, 0.28f, 0.45f);
            int sec = (int)Math.Ceiling(Projectile.timeLeft / 60f);

            if (sec != lastSec)
            {
                lastSec = sec;
                popTimer = 9;
            }
            if (popTimer > 0)
                popTimer--;

            if (Main.myPlayer == Projectile.owner && Projectile.timeLeft % 30 == 0)
            {
                Projectile.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(16) && Return01 <= 0f)
                new Spark(Projectile.Center + Main.rand.NextVector2Circular(12f, 10f), new Vector2(0f, Main.rand.NextFloat(0.5f, 1.1f)), EnergyShield.ColorCyan, 18, Main.rand.NextFloat(0.18f, 0.28f), 0.85f).Spawn();

            if (Main.netMode != NetmodeID.Server && Return01 > 0f && Main.rand.NextBool(2))
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(0.8f, 0.8f), EnergyShield.ColorWhite, 12, Main.rand.NextFloat(0.22f, 0.34f), 0.9f).Spawn();
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server || timeLeft > 2)
                return;
                
            Vector2 c = Owner.MountedCenter;
            SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.4f }, c);

            new Circular_glow(c, Vector2.Zero, EnergyShield.ColorWhite, 10, 0.5f, 0.95f).Spawn();
            new Circleedge_reducing(c, Vector2.Zero, EnergyShield.ColorCyan, 14, 0.55f, 0.85f).Spawn();

            for (int i = 0; i < 10; i++)
            {
                Vector2 v = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * Main.rand.NextFloat(2.6f, 3.4f);
                new Spark(c, v, EnergyShield.ColorCyan, 18, Main.rand.NextFloat(0.24f, 0.34f), 0.9f).Spawn();
            }

            for (int i = 0; i < 4; i++)
                new StarParticle(c, (MathHelper.PiOver2 * i + 0.4f).ToRotationVector2() * 1.6f, EnergyShield.ColorWhite, 16, 0.35f, 0.9f).Spawn();
            for (int i = 0; i < 3; i++)
                new EnergyHexParticle(c, Main.rand.NextVector2Circular(1.2f, 1.2f) - new Vector2(0f, 0.8f), EnergyShield.ColorCyan, Main.rand.Next(18, 26), Main.rand.NextFloat(0.35f, 0.55f), 0.85f).Spawn();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!loadAttempted)
            {
                loadAttempted = true;
                try
                {
                    chargeShader = ModContent.Request<Effect>("jarkos/Shaders/ShieldCharge", AssetRequestMode.ImmediateLoad);
                }
                catch { chargeShader = null; }
            }
            if (chargeShader == null || chargeShader.Value == null)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    Vector2 hPos = Projectile.Center - Main.screenPosition;
                    int s = (int)Math.Ceiling(Projectile.timeLeft / 60f);

                    Texture2D qTex = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
                    float p = 1f + 0.16f * (popTimer / 9f);
                    float r = 1f - 0.82f * Return01;
                    float sc = 36f * 2f / qTex.Width * p * r;

                    Main.spriteBatch.Draw(qTex, hPos, null, EnergyShield.ColorCyan * 0.6f, 0f, qTex.Size() * 0.5f, sc, SpriteEffects.None, 0f);
                    
                    string text = s.ToString();
                    Vector2 sz = FontAssets.MouseText.Value.MeasureString(text);
                    Terraria.Utils.DrawBorderString(Main.spriteBatch, text, hPos - sz * 0.5f, EnergyShield.ColorWhite * (1f - Return01));
                }
                return false;
            }

            Vector2 holoPos = Projectile.Center;
            Texture2D beam = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/BeamStrip", AssetRequestMode.ImmediateLoad).Value;
            Vector2 toHolo = holoPos - Owner.MountedCenter;

            float holoDist = toHolo.Length();
            float popNow = 1f + 0.16f * (popTimer / 9f);
            float shrinkNow = 1f - 0.82f * Return01;
            float halfQuad = 36f * popNow * shrinkNow;

            Vector2 halfExt = new Vector2(halfQuad * 0.74f, halfQuad * 0.56f);

            if (holoDist > 8f)
            {
                Vector2 dn = toHolo / holoDist;
                float tx = halfExt.X / Math.Max(Math.Abs(dn.X), 0.001f);
                float ty = halfExt.Y / Math.Max(Math.Abs(dn.Y), 0.001f);
                float beamLen = Math.Max(holoDist - Math.Min(tx, ty), 0f);
                if (beamLen > 4f)
                {
                    float pulse = 0.5f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 9f);
                    Color bc = EnergyShield.ColorCyan; bc = new Color(bc.R, bc.G, bc.B, 0) * (pulse * (1f - Return01));
                    Main.spriteBatch.Draw(beam, Owner.MountedCenter - Main.screenPosition, null, bc, toHolo.ToRotation(), new Vector2(0f, beam.Height * 0.5f), new Vector2(beamLen / beam.Width, 5f / beam.Height), SpriteEffects.None, 0f);
                }
            }
            Texture2D quadTex = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
            Texture2D digits = ModContent.Request<Texture2D>("jarkos/textures/DigitAtlas", AssetRequestMode.ImmediateLoad).Value;

            int sec = (int)Math.Ceiling(Projectile.timeLeft / 60f);
            Effect fx = chargeShader.Value;

            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uProgress"]?.SetValue(Progress);
            fx.Parameters["uReturn"]?.SetValue(Return01);
            fx.Parameters["uNumber"]?.SetValue((float)sec);
            fx.Parameters["uColor"]?.SetValue(EnergyShield.ColorCyan.ToVector3());
            fx.Parameters["uSecondaryColor"]?.SetValue(EnergyShield.ColorWhite.ToVector3());
            fx.Parameters["uDigitTexture"]?.SetValue(digits);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            
            float pop = 1f + 0.16f * (popTimer / 9f);
            float shrink = 1f - 0.82f * Return01;
            float scale = 36f * 2f / quadTex.Width * pop * shrink;

            Main.spriteBatch.Draw(quadTex, holoPos - Main.screenPosition, null, Color.White, 0f, quadTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
