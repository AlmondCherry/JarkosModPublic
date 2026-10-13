using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaDeathray : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        private const float MaxLength = 2400f;
        private const int TelegraphTime = 60;
        private const int FireTime = 180;
        private const int OverchargeTime = 45;
        private const int FadeTime = 25;

        public ref float BossIndex => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.localAI[0];

        private float laserLength = MaxLength;
        private static Texture2D blankTexture;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.alpha = 254;
            Projectile.timeLeft = TelegraphTime + FireTime + FadeTime + 10;
        }

        public override bool ShouldUpdatePosition() => false;
        private float BeamPower
        {
            get
            {
                if (Timer <= TelegraphTime) return 0f;

                float up   = MathHelper.Clamp((Timer - TelegraphTime) / 14f, 0f, 1f);
                float down = MathHelper.Clamp((TelegraphTime + FireTime + FadeTime - Timer) / FadeTime, 0f, 1f);
                float p = Math.Min(up, down);

                return p * p * (3f - 2f * p);
            }
        }

        private bool Overcharged => Timer > TelegraphTime + FireTime - OverchargeTime && Timer <= TelegraphTime + FireTime;

        public override void AI()
        {
            NPC boss = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<Celestia>())
                {
                    boss = Main.npc[i];
                    BossIndex = i;
                    break;
                }
            }

            if (boss == null) 
            { 
                Projectile.Kill(); 
                return; 
            }

            Projectile.Center = boss.Center + new Vector2(46 * boss.direction, -47);
            Player player = Main.player[boss.target];

            Timer++;

            if (Timer <= TelegraphTime)
            {
                if (Timer < TelegraphTime - 20)
                {
                    float targetAngle = (player.Center - Projectile.Center).ToRotation();
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = targetAngle.ToRotationVector2();
                    else
                        Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(targetAngle, 0.15f).ToRotationVector2();
                }

                Projectile.alpha = (int)MathHelper.Lerp(254, 100, Timer / TelegraphTime);

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                {
                    Vector2 p = Projectile.Center + Main.rand.NextVector2CircularEdge(110f, 110f) * (1f - Timer / TelegraphTime * 0.6f);
                    new Spark(p, (Projectile.Center - p) * 0.1f, Color.HotPink, 16, 0.5f, 1f, 0f).Spawn();
                }
                if (Timer == TelegraphTime - 20)
                    SoundEngine.PlaySound(SoundID.Item117, Projectile.Center);
            }

            Vector2 beamDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float[] samples = new float[3];
            Collision.LaserScan(Projectile.Center, beamDir, 30f, MaxLength, samples);

            laserLength = (samples[0] + samples[1] + samples[2]) / 3f;
            Vector2 impactPoint = Projectile.Center + beamDir * laserLength;

            if (Timer == TelegraphTime + 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);

                for (int i = 0; i < 60; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, beamDir.RotatedByRandom(MathHelper.Pi / 5) * Main.rand.NextFloat(5f, 40f));
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(1.5f, 3.5f);
                    d.color = Color.Magenta;
                }

                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, beamDir, 28f, 10f, 35, 2000f, FullName));
            }

            if (Timer > TelegraphTime && Timer <= TelegraphTime + FireTime)
            {
                Projectile.alpha = 0;

                float fireT = (Timer - TelegraphTime) / FireTime;
                float maxRot = MathHelper.ToRadians(0.55f + fireT * 0.75f);

                if (Overcharged) maxRot *= 1.7f;

                float targetAngle = (player.Center - Projectile.Center).ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                float diff = MathHelper.Clamp(MathHelper.WrapAngle(targetAngle - currentAngle), -maxRot, maxRot);

                Projectile.velocity = (currentAngle + diff).ToRotationVector2();

                if (Timer % 3 == 0)
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), Overcharged ? 9f : 6f, 6f, 10, 2000f, FullName));

                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.FireworksRGB, beamDir.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 15f));
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1f, 2f);
                    }

                    if (Main.rand.NextBool())
                    {
                        Vector2 onBeam = Projectile.Center + beamDir * Main.rand.NextFloat(100f, laserLength);
                        Vector2 side = beamDir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1f, 1f);
                        new Spark(onBeam, side * 4f + beamDir * 2f, Color.HotPink, 20, 0.5f, 1f, 0f).Spawn();
                    }

                    if (laserLength < MaxLength - 10f)
                    {
                        for (int i = 0; i < (Overcharged ? 4 : 2); i++)
                        {
                            Dust d = Dust.NewDustPerfect(impactPoint, DustID.FireworksRGB, (-beamDir).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(4f, 14f));
                            d.noGravity = true;
                            d.scale = Main.rand.NextFloat(1.5f, 3f);
                            d.color = Color.Magenta;
                        }
                        Lighting.AddLight(impactPoint, 1.4f, 0.4f, 1.2f);
                    }
                }

                for (float l = 0f; l < laserLength; l += 200f)
                    Lighting.AddLight(Projectile.Center + beamDir * l, 1f, 0.3f, 0.9f);

                if (Timer == TelegraphTime + FireTime - OverchargeTime)
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.4f }, Projectile.Center);
            }
            if (Timer == TelegraphTime + FireTime)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f }, Projectile.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int rays = 12;
                    for (int i = 0; i < rays; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi / rays * i).ToRotationVector2() * 5f;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel, ModContent.ProjectileType<CelestiaStar>(), Projectile.damage / 3, 0f, Main.maxPlayers);
                    }
                }
            }

            if (Timer > TelegraphTime + FireTime)
                Projectile.alpha = (int)MathHelper.Lerp(0, 254,
                    (Timer - (TelegraphTime + FireTime)) / FadeTime);

            Projectile.timeLeft = 2;
            if (Timer >= TelegraphTime + FireTime + FadeTime)
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float power = BeamPower;
            if (Timer <= TelegraphTime || Timer >= TelegraphTime + FireTime || power < 0.35f)
                return false;

            float beamWidth = 80f * power * (Overcharged ? 1.25f : 1f);
            float point = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.Center + Projectile.velocity * laserLength, beamWidth, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 254) return false;

            float rotation = Projectile.velocity.ToRotation();
            float power = BeamPower;
            float opacity = 1f - Projectile.alpha / 255f;

            float width;
            if (Timer <= TelegraphTime)
                width = 12f;
            else
                width = (80f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 25f) * 15f) * MathHelper.Max(power, 0.05f) * (Overcharged ? 1.3f : 1f);

            Effect deathrayShader = ModContent.Request<Effect>(
                "jarkos/Effects/Deathray", AssetRequestMode.ImmediateLoad).Value;

            deathrayShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            deathrayShader.Parameters["uOpacity"]?.SetValue(opacity);
            deathrayShader.Parameters["uProgress"]?.SetValue(power);

            if (Timer <= TelegraphTime)
            {
                deathrayShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.2f, 0.6f) * 0.4f);
                deathrayShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.5f, 0.0f, 0.3f) * 0.4f);
            }
            else
            {
                float boost = Overcharged ? 1.25f : 1f;
                deathrayShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.2f, 0.6f) * boost);
                deathrayShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.8f, 0.0f, 0.8f) * boost);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, deathrayShader, Main.GameViewMatrix.ZoomMatrix);

            Texture2D blankTexture = ModContent.Request<Texture2D>("Jarkos/textures/blankTexture", AssetRequestMode.ImmediateLoad).Value;
            
            Main.spriteBatch.Draw(blankTexture, Projectile.Center - Main.screenPosition, null, Color.White, rotation, new Vector2(0, 0.5f), new Vector2(laserLength, width), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            if (Timer > TelegraphTime)
            {
                Texture2D bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
                Vector2 bloomOrigin = bloom.Size() * 0.5f;

                float pulse = (1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f) * 0.15f) * power;

                Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, new Color(255, 60, 180) * opacity, 0f, bloomOrigin, 1.0f * pulse, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, Color.White * opacity, 0f, bloomOrigin, 0.5f * pulse, SpriteEffects.None, 0f);

                if (laserLength < MaxLength - 10f)
                {
                    Vector2 impact = Projectile.Center + rotation.ToRotationVector2() * laserLength - Main.screenPosition;
                    
                    Main.spriteBatch.Draw(bloom, impact, null, new Color(255, 60, 180) * opacity * 0.9f, 0f, bloomOrigin, 0.8f * pulse, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(bloom, impact, null, Color.White * opacity * 0.8f, 0f, bloomOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }
    }
}
