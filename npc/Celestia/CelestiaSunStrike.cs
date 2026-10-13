using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaSunStrike : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private const int WarningTime = 180;
        private const int FireTime = 45;
        private const int FadeTime = 18;
        private const float BeamHeight = 1800f;
        private const float BeamMaxWidth = 260f;

        public ref float Seed => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.localAI[0];
        private static readonly Color GlowBlue = new Color(80, 170, 255);
        private static readonly Color DeepBlue = new Color(30, 80, 230);

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 254;
            Projectile.timeLeft = WarningTime + FireTime + FadeTime + 10;
        }

        public override bool ShouldUpdatePosition() => false;

        private float BeamPower
        {
            get
            {
                if (Timer <= WarningTime) return 0f;

                float up   = MathHelper.Clamp((Timer - WarningTime) / 8f, 0f, 1f);
                float down = MathHelper.Clamp((WarningTime + FireTime + FadeTime - Timer) / FadeTime, 0f, 1f);
                float p = Math.Min(up, down);

                return p * p * (3f - 2f * p);
            }
        }

        public override void AI()
        {
            Timer++;
            Projectile.alpha = 0;

            float warnProg = MathHelper.Clamp(Timer / WarningTime, 0f, 1f);

            if (Timer <= WarningTime)
            {
                if (Timer == 1)
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.3f }, Projectile.Center);

                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool(2))
                    {
                        float r = MathHelper.Lerp(170f, 35f, warnProg);
                        Vector2 p = Projectile.Center + Main.rand.NextVector2CircularEdge(r, r * 0.35f);
                        new Spark(p, (Projectile.Center - p) * 0.08f, GlowBlue, 18, 0.45f, 1f, 0f).Spawn();
                    }
                    if (Main.rand.NextBool(3))
                        new Stretching_particle(Projectile.Center + new Vector2(Main.rand.NextFloat(-26f, 26f), 0f), new Vector2(0f, -Main.rand.NextFloat(6f, 12f)), Color.Cyan * 0.7f, 20, 0.4f, 1f, 0f).Spawn();
                }
                Lighting.AddLight(Projectile.Center, 0.25f * warnProg, 0.55f * warnProg, 0.9f * warnProg);

                if (Timer == WarningTime - 60)
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.5f }, Projectile.Center);

                if (Timer > WarningTime - 60 && Timer % 6 == 0 && Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 4; i++)
                        new Spark(Projectile.Center, (-Vector2.UnitY).RotatedByRandom(0.5f) * Main.rand.NextFloat(4f, 9f), Color.White, 14, 0.5f, 1f, 0f).Spawn();
                }
            }

            if (Timer == WarningTime + 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);

                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, -Vector2.UnitY, 40f, 12f, 45, 3000f, FullName));
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Vector2.UnitX, 24f, 15f, 30, 3000f, FullName));

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 60; i++)
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, (-Vector2.UnitY).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(6f, 32f));
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1.6f, 3.8f);
                    }

                    for (int i = 0; i < 20; i++)
                        new Circleedge(Projectile.Center, Main.rand.NextVector2CircularEdge(8f, 3.5f), GlowBlue, 32, 1.4f, 1f, 0f).Spawn();

                    for (int i = 0; i < 10; i++)
                        new StarParticle(Projectile.Center, (-Vector2.UnitY).RotatedByRandom(1f) * Main.rand.NextFloat(8f, 18f), Color.White, 30, 0.6f, 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }
            }

            if (Timer > WarningTime && Timer <= WarningTime + FireTime)
            {
                float power = BeamPower;

                if (Timer % 3 == 0)
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 11f * power, 8f, 10, 3000f, FullName));

                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool())
                    {
                        Vector2 onBeam = Projectile.Center - Vector2.UnitY * Main.rand.NextFloat(50f, 900f);
                        new Spark(onBeam, new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(2f, 7f)), GlowBlue, 20, 0.55f, 1f, 0f).Spawn();
                    }
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueCrystalShard, (-Vector2.UnitY).RotatedByRandom(1.1f) * Main.rand.NextFloat(5f, 14f));
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1f, 2.4f) * power;
                    }
                }

                for (float h = 0f; h < 1000f; h += 200f)
                    Lighting.AddLight(Projectile.Center - Vector2.UnitY * h, 0.4f * power, 0.9f * power, 1.4f * power);
            }
            if (Timer > WarningTime + FireTime && Timer % 4 == 0)
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 5f, 6f, 8, 3000f, FullName));

            Projectile.timeLeft = 2;
            if (Timer >= WarningTime + FireTime + FadeTime)
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float power = BeamPower;
            if (Timer <= WarningTime || power < 0.4f)
                return false;

            float beamWidth = 130f * power;
            float point = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - Vector2.UnitY * BeamHeight, Projectile.Center, beamWidth, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float power = BeamPower;
            float warnProg = MathHelper.Clamp(Timer / WarningTime, 0f, 1f);
            bool warning = Timer <= WarningTime;

            Effect shader = ModContent.Request<Effect>("jarkos/Effects/CelestiaSunStrike", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["uSeed"]?.SetValue(Seed);

            float width;
            if (warning)
            {
                width = 18f + warnProg * 14f;
                shader.Parameters["uOpacity"]?.SetValue(0.35f + warnProg * 0.25f);
                shader.Parameters["uProgress"]?.SetValue(0f);
                shader.Parameters["uColor"]?.SetValue(new Vector3(0.35f, 0.75f, 1.0f) * 0.45f);
                shader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.05f, 0.25f, 0.9f) * 0.45f);
            }
            else
            {
                width = (BeamMaxWidth + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 22f + Seed) * 40f) * MathHelper.Max(power, 0.05f);
                shader.Parameters["uOpacity"]?.SetValue(1f);
                shader.Parameters["uProgress"]?.SetValue(power);
                shader.Parameters["uColor"]?.SetValue(new Vector3(0.35f, 0.75f, 1.0f));
                shader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.05f, 0.25f, 0.9f));
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, shader, Main.GameViewMatrix.ZoomMatrix);

            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            Vector2 skyPoint = Projectile.Center - Vector2.UnitY * BeamHeight - Main.screenPosition;
            Main.spriteBatch.Draw(blankTexture, skyPoint, null, Color.White, MathHelper.PiOver2, new Vector2(0f, 0.5f), new Vector2(BeamHeight, width), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            Vector2 bloomOrigin = bloom.Size() * 0.5f;
            Vector2 impact = Projectile.Center - Main.screenPosition;

            if (warning)
            {
                float pulseW = 1f + (float)Math.Sin(Timer * (0.25f + warnProg * 0.35f)) * 0.2f;
                float markScale = MathHelper.Lerp(0.45f, 1.15f, warnProg) * pulseW;
                Main.spriteBatch.Draw(bloom, impact, null, DeepBlue * (0.35f + warnProg * 0.45f), 0f, bloomOrigin, new Vector2(markScale * 1.7f, markScale * 0.35f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, impact, null, Color.White * (0.25f + warnProg * 0.35f), 0f, bloomOrigin, new Vector2(markScale * 0.85f, markScale * 0.18f), SpriteEffects.None, 0f);
            }
            else
            {
                float pulse = (1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f + Seed) * 0.15f) * power;
                Main.spriteBatch.Draw(bloom, impact, null, GlowBlue * power, 0f, bloomOrigin, 1.9f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, impact, null, Color.White * power, 0f, bloomOrigin, 0.9f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, impact, null, new Color(120, 200, 255) * power * 0.85f, 0f, bloomOrigin, new Vector2(3.2f, 0.5f) * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, impact - new Vector2(0f, 60f), null, GlowBlue * power * 0.6f, 0f, bloomOrigin, new Vector2(0.7f, 2.2f) * pulse, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
