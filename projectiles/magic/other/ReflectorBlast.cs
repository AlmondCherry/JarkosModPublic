using jarkos.items.weapons.magic.other;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.other
{
    public class ReflectorBlast : ModProjectile
    {
        public const int LIFE = 46;
        public override string Texture => "jarkos/textures/PrismRing";
        private static Asset<Effect> blastShader;
        private static bool shaderLoadAttempted;
        private float MaxRadius => Projectile.ai[0];
        private float Charge01 => Projectile.ai[1];
        private float Progress => 1f - Projectile.timeLeft / (float)LIFE;
        private float Radius => MaxRadius * EaseOut(Progress);
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = LIFE;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.15f, Volume = 1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.5f, Volume = 0.9f }, Projectile.Center);

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 12f + 8f * Charge01, 8f, 24, 1600f, "CrystalReflector"));


            if (Main.netMode != NetmodeID.Server && Projectile.owner == Main.myPlayer)
            {
                SunfateScreenSystem.TriggerContrastEffect();
            }

            if (Main.netMode == NetmodeID.Server)
                return;

            new Circular_glow(Projectile.Center, Vector2.Zero, CrystalReflector.ColorWhite, 26, 2.2f, 1f).Spawn();
            new Circular_glow(Projectile.Center, Vector2.Zero, CrystalReflector.ColorIce, 30, 3.2f, 0.8f).Spawn();
            new Circleedge_tight(Projectile.Center, Vector2.Zero, CrystalReflector.ColorWhite, 24, 2f, 1f).Spawn();
            new Circleedge_novaburst(Projectile.Center, Vector2.Zero, CrystalReflector.ColorViolet, 30, 1.6f, 0.9f).Spawn();

            int shards = 34 + (int)(18 * Charge01);

            for (int i = 0; i < shards; i++)
            {
                Vector2 v = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 11f);
                new CrystalShardParticle(Projectile.Center, v, Main.rand.NextBool(4) ? CrystalReflector.ColorViolet : CrystalReflector.ColorIce, Main.rand.Next(36, 60), Main.rand.NextFloat(0.6f, 1.3f), 1f, Main.rand.NextFloat(-0.35f, 0.35f)).Spawn();
            }

            for (int i = 0; i < 22; i++)
            {
                Vector2 v = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f);
                new Stretching_particle_out(Projectile.Center, v, CrystalReflector.ColorIce, 24, Main.rand.NextFloat(60f, 130f), Main.rand.NextFloat(6f, 10f), 0.02f).Spawn();
            }

            for (int i = 0; i < 14; i++)
                new StarParticleBigger(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), Main.rand.NextVector2Circular(4f, 4f), CrystalReflector.ColorWhite, 30, Main.rand.NextFloat(0.2f, 0.4f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();

            for (int i = 0; i < 10; i++)
                new Cube_particle(Projectile.Center, Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 7f), CrystalReflector.ColorViolet, 28, Main.rand.NextFloat(0.2f, 0.4f), 0.9f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                
            for (int i = 0; i < 8; i++)
                new High_smoke(Projectile.Center + Main.rand.NextVector2Circular(24f, 24f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f), CrystalReflector.ColorDeep, 40, Main.rand.NextFloat(0.5f, 0.9f), 0.5f, Main.rand.NextFloat(-0.05f, 0.05f)).Spawn();
        }
        public override void AI()
        {
            float r = Radius;
            if (Projectile.timeLeft == LIFE - 8)
            {
                SoundEngine.PlaySound(SoundID.Item94 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 6f + 4f * Charge01, 6f, 16, 1600f, "CrystalReflector"));
            }
            if (Projectile.timeLeft == LIFE - 18 && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.7f, Volume = 0.6f }, Projectile.Center);
                new Circleedge_reducing(Projectile.Center, Vector2.Zero, CrystalReflector.ColorViolet, 20, 1.4f, 0.7f).Spawn();
            }
            if (Main.netMode != NetmodeID.Server)
            {
                int sparks = 3 + (int)(4 * Charge01);
                for (int i = 0; i < sparks; i++)
                {
                    float a = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Projectile.Center + a.ToRotationVector2() * r;
                    new Spark(pos, a.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f), Main.rand.NextBool(4) ? CrystalReflector.ColorViolet : CrystalReflector.ColorIce, 16, Main.rand.NextFloat(0.3f, 0.5f), 0.9f).Spawn();
                }
            }
            if (Main.netMode != NetmodeID.Server && Projectile.timeLeft > 8 && Main.rand.NextBool(2))
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(r * 0.8f, r * 0.8f);
                new StarParticle(pos, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.2f, 0.7f)), Main.rand.NextBool(3) ? CrystalReflector.ColorViolet : CrystalReflector.ColorWhite, Main.rand.Next(40, 70), Main.rand.NextFloat(0.12f, 0.22f), 0.85f).Spawn();
            }
            float pillarLight = 1f - Progress;
            Lighting.AddLight(Projectile.Center, 0.7f + pillarLight, 0.9f + pillarLight, 1.2f + pillarLight);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float r = Radius;
            Vector2 nearest = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            float dist = Vector2.Distance(Projectile.Center, nearest);
            return dist <= r + 12f && dist >= r - 40f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = target.Center.X >= Projectile.Center.X ? 1 : -1;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            new Circleedge_tight(target.Center, Vector2.Zero, CrystalReflector.ColorWhite, 14, 0.7f, 0.9f).Spawn();
            for (int k = 0; k < 5; k++)
                new CrystalShardParticle(target.Center, Main.rand.NextVector2Circular(4f, 4f), CrystalReflector.ColorIce, 30, Main.rand.NextFloat(0.5f, 0.9f), 0.95f, Main.rand.NextFloat(-0.25f, 0.25f)).Spawn();
        }
        private void DrawLightPillar(Vector2 drawPos)
        {
            float t = Progress / 0.6f;
            if (t >= 1f)
                return;
                
            Texture2D strip = ModContent.Request<Texture2D>("jarkos/projectiles/magic/other/BeamStrip", AssetRequestMode.ImmediateLoad).Value;
            float alpha = (1f - t) * (1f - t) * (0.85f + 0.35f * Charge01);
            float width = (60f + 50f * Charge01) * (1f - 0.5f * t);

            const float pillarLen = 1400f;
            Vector2 origin = new Vector2(0f, strip.Height / 2f);
            Vector2 scaleUp = new Vector2(pillarLen / strip.Width, width / strip.Height);

            for (int dir = -1; dir <= 1; dir += 2)
            {
                float rot = dir * MathHelper.PiOver2;
                Main.spriteBatch.Draw(strip, drawPos, null, new Color(CrystalReflector.ColorIce.R, CrystalReflector.ColorIce.G, CrystalReflector.ColorIce.B, 0) * (alpha * 0.55f), rot, origin, scaleUp, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(strip, drawPos, null, new Color(255, 255, 255, 0) * (alpha * 0.8f), rot, origin, scaleUp * new Vector2(0.85f, 0.38f), SpriteEffects.None, 0f);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                try
                {
                    blastShader = ModContent.Request<Effect>("jarkos/Shaders/CrystalBlast", AssetRequestMode.ImmediateLoad);
                }
                catch { blastShader = null; }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float half = tex.Width / 2f;
            float scale = MaxRadius / (half * 0.82f);

            if (blastShader != null && blastShader.Value != null)
            {
                Effect fx = blastShader.Value;
                fx.Parameters["uProgress"]?.SetValue(Progress);
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uCharge"]?.SetValue(Charge01);
                fx.Parameters["uColor"]?.SetValue(CrystalReflector.ColorIce.ToVector3());
                fx.Parameters["uSecondaryColor"]?.SetValue(CrystalReflector.ColorViolet.ToVector3());
                fx.Parameters["uNoiseTexture"]?.SetValue(ModContent.Request<Texture2D>("jarkos/projectiles/magic/other/CrystalNoise", AssetRequestMode.ImmediateLoad).Value);
                Main.spriteBatch.End();

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, new Vector2(half, half), scale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float alpha = (1f - Progress) * (0.7f + 0.3f * Charge01);
                Main.spriteBatch.Draw(tex, drawPos, null, new Color(CrystalReflector.ColorIce.R, CrystalReflector.ColorIce.G, CrystalReflector.ColorIce.B, 0) * alpha, Progress * 0.6f, new Vector2(half, half), scale * EaseOut(Progress), SpriteEffects.None, 0f);
            }
            DrawLightPillar(drawPos);
            return false;
        }
    }
}
