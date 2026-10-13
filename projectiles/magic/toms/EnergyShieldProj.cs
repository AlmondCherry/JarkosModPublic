using System;
using System.IO;
using jarkos.items.weapons.magic.toms;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.magic.toms
{
    public class EnergyShieldProj : ModProjectile
    {
        public const int OPEN = 24;
        public const int DURATION = 720;
        public const int CLOSE = 20;
        public const int BREAK = 34;
        public const float MAX_DUR = 160f;
        public const float RADIUS = 58f;
        public const float ARC_DEG = 75f;
        public const int IMPACT_LIFE = 22;
        public override string Texture => "jarkos/projectiles/magic/toms/EnergyNoise";
        private static Asset<Effect> shieldShader;
        private static Asset<Effect> impactShader;
        private static Asset<Effect> breakShader;
        private static bool shaderLoadAttempted;
        private float Age => Projectile.ai[0];
        private bool Breaking => Projectile.ai[1] == 1f;
        private float Durability { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
        private float Health01 => MathHelper.Clamp(Durability / MAX_DUR, 0f, 1f);
        private float Glitch01 => 1f - Health01;
        private bool manualCancel;
        private float tilt;
        private float tiltVel;
        private float facing;
        private bool facingInit;
        private bool hasPlayedBreakEffects;
        private readonly Vector2[] impactDirs = new Vector2[4];
        private readonly int[] impactAges = { 999, 999, 999, 999 };
        private Player Owner => Main.player[Projectile.owner];
        private float ArcCos => (float)Math.Cos(MathHelper.ToRadians(ARC_DEG));

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(facing);
            writer.Write(Durability);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            facing = reader.ReadSingle();
            Durability = reader.ReadSingle();
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = OPEN + DURATION + CLOSE + 10;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Durability = MAX_DUR;
            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.3f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.Center);

            if (Main.netMode == NetmodeID.Server)
                return;

            new Circular_glow(Owner.MountedCenter, Vector2.Zero, EnergyShield.ColorWhite, 16, 1f, 0.9f).Spawn();

            for (int i = 0; i < 12; i++)
            {
                Vector2 from = Owner.MountedCenter + Main.rand.NextVector2CircularEdge(90f, 90f);
                Vector2 vIn = (Owner.MountedCenter - from).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(6f, 10f);
                new Stretching_particle_out(from, vIn, EnergyShield.ColorCyan, 14, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(3f, 6f), 0.02f).Spawn();
            }
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.Center = Owner.MountedCenter;

            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            if (Breaking)
            {
                if (!hasPlayedBreakEffects)
                {
                    hasPlayedBreakEffects = true;
                    StartBreak();
                }
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] >= BREAK)
                    Projectile.Kill();
                return;
            }
            if (Main.myPlayer == Projectile.owner && Main.mouseRight && !manualCancel && Age > OPEN + 8 && Age < OPEN + DURATION)
            {
                manualCancel = true;
                Projectile.ai[0] = OPEN + DURATION;
                Projectile.timeLeft = CLOSE + 8;
                Projectile.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.25f, Volume = 0.7f }, Projectile.Center);
            }

            UpdateFacing();
            UpdateTiltSpring();
            for (int i = 0; i < 4; i++)
                impactAges[i]++;

            if (Age >= OPEN && Age < OPEN + DURATION)
                ReflectProjectiles();

            EmitAmbient();
            Vector2 arcMid = Projectile.Center + facing.ToRotationVector2() * RADIUS;
            Lighting.AddLight(arcMid, 0.35f, 0.7f, 1.1f);
        }
        private void UpdateFacing()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                float target = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
                if (!facingInit)
                {
                    facing = target;
                    facingInit = true;
                }
                int skip = Glitch01 > 0.7f ? 3 : Glitch01 > 0.45f ? 2 : 1;
                
                if ((int)Age % skip != 0)
                    return;

                float speed = 0.25f * (0.35f + 0.65f * Health01) * skip;
                float oldFacing = facing;

                facing = facing.AngleLerp(target, speed);

                if (Math.Abs(MathHelper.WrapAngle(facing - oldFacing)) > 0.01f || (int)Age % 15 == 0)
                {
                    Projectile.netUpdate = true;
                }
            }
        }
        private void UpdateTiltSpring()
        {
            tiltVel += -tilt * 0.03f;
            tiltVel *= 0.86f;
            tilt += tiltVel;

            tilt = MathHelper.Clamp(tilt, -0.9f, 0.9f);
        }
        private float ShieldRotation => facing + tilt;
        private void ReflectProjectiles()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || !p.hostile || p.friendly || p.damage <= 0)
                    continue;

                Vector2 fromPlayer = p.Center - Projectile.Center;
                float dist = fromPlayer.Length();
                if (dist < RADIUS - 26f || dist > RADIUS + 20f)
                    continue;

                Vector2 dirN = fromPlayer / Math.Max(dist, 0.001f);
                if (Vector2.Dot(dirN, ShieldRotation.ToRotationVector2()) < ArcCos)
                    continue;

                if (Vector2.Dot(p.velocity, dirN) >= 0f)
                    continue;

                Vector2 reflected = p.velocity - 2f * Vector2.Dot(p.velocity, dirN) * dirN;
                p.velocity = reflected * 1.05f;
                p.hostile = false;
                p.friendly = true;

                p.owner = Projectile.owner;
                p.damage = Projectile.damage;

                p.DamageType = DamageClass.Magic;
                p.netUpdate = true;
                OnReflect(p, dirN);
            }
        }
        private void OnReflect(Projectile p, Vector2 dirN)
        {
            float cost = 13f + Math.Min(17f, p.damage * 0.12f);
            Durability -= cost;

            float signedAngle = (float)Math.Atan2(facing.ToRotationVector2().X * dirN.Y - facing.ToRotationVector2().Y * dirN.X, Vector2.Dot(facing.ToRotationVector2(), dirN));
            tiltVel += signedAngle * 0.45f + Main.rand.NextFloat(-0.04f, 0.04f);
            int slot = 0;

            for (int k = 1; k < 4; k++)
                if (impactAges[k] > impactAges[slot])
                    slot = k;

            impactDirs[slot] = dirN.RotatedBy(-ShieldRotation);
            impactAges[slot] = 0;

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.8f }, p.Center);
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.2f, Volume = 0.5f }, p.Center);

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 hit = Projectile.Center + dirN * RADIUS;
                new Circular_glow(hit, Vector2.Zero, EnergyShield.ColorWhite, 10, 0.5f, 0.6f).Spawn();
                new Circleedge_tight(hit, Vector2.Zero, EnergyShield.ColorCyan, 12, 0.4f, 0.55f).Spawn();
                new StarParticleBigger(hit, Vector2.Zero, EnergyShield.ColorWhite, 12, 0.25f, 0.65f, 0.05f).Spawn();

                for (int k = 0; k < 6; k++)
                    new Spark(hit, dirN.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * Main.rand.NextFloat(3f, 7f), Main.rand.NextBool(3) ? EnergyShield.ColorDeep : EnergyShield.ColorCyan, 15, Main.rand.NextFloat(0.22f, 0.38f), 0.55f).Spawn();
                int hexes = 3 + (int)(4 * Glitch01);
                for (int k = 0; k < hexes; k++)
                    new EnergyHexParticle(hit + Main.rand.NextVector2Circular(10f, 10f), dirN * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1.5f, 1.5f), Health01 > 0.4f ? EnergyShield.ColorCyan : EnergyShield.ColorDanger, Main.rand.Next(22, 36), Main.rand.NextFloat(0.35f, 0.6f), 0.5f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
            }
            if (Durability <= 0f)
            {
                Durability = 0f;
                StartBreak();
            }
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.netUpdate = true;
            }
        }
        private void StartBreak()
        {
            Projectile.ai[1] = 1f;
            Projectile.localAI[1] = 0f;
            Projectile.timeLeft = BREAK + 5;
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.netUpdate = true;
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.4f, Volume = 1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = -0.2f, Volume = 0.7f }, Projectile.Center);
            if (Main.netMode == NetmodeID.Server)
                return;
            if (Main.LocalPlayer.Distance(Projectile.Center) < 1200f)
            {
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Unit(), 7f, 6f, 14, 1200f, "EnergyShield"));
            }
            if (Main.netMode == NetmodeID.Server)
                return;
            for (int i = 0; i < 26; i++)
            {
                float a = ShieldRotation + MathHelper.ToRadians(Main.rand.NextFloat(-ARC_DEG, ARC_DEG));
                Vector2 pos = Projectile.Center + a.ToRotationVector2() * (RADIUS + Main.rand.NextFloat(-8f, 8f));
                Vector2 vOut = a.ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
                new EnergyHexParticle(pos, vOut, Main.rand.NextBool(3) ? EnergyShield.ColorDanger : EnergyShield.ColorCyan, Main.rand.Next(30, 55), Main.rand.NextFloat(0.6f, 1.1f), 0.95f, Main.rand.NextFloat(-0.3f, 0.3f)).Spawn();

                if (Main.rand.NextBool(2))
                    new Spark(pos, vOut * 1.4f, EnergyShield.ColorWhite, 20, Main.rand.NextFloat(0.3f, 0.5f), 0.9f).Spawn();
            }
            new Circleedge_novaburst(Projectile.Center + ShieldRotation.ToRotationVector2() * RADIUS * 0.5f, Vector2.Zero, EnergyShield.ColorDanger, 22, 1.2f, 0.85f).Spawn();
        }
        private void EmitAmbient()
        {
            if (Main.netMode == NetmodeID.Server || Age < OPEN)
                return;
            if (Main.rand.NextBool(Glitch01 > 0.5f ? 3 : 7))
            {
                float a = ShieldRotation + MathHelper.ToRadians(Main.rand.NextFloat(-ARC_DEG, ARC_DEG));
                Vector2 pos = Projectile.Center + a.ToRotationVector2() * RADIUS;
                new Spark(pos, a.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.5f), Glitch01 > 0.6f && Main.rand.NextBool(2) ? EnergyShield.ColorDanger : EnergyShield.ColorCyan, 14, Main.rand.NextFloat(0.18f, 0.3f), 0.45f).Spawn();
            }
            if (Main.rand.NextBool(18))
            {
                float a = ShieldRotation + MathHelper.ToRadians(Main.rand.NextFloat(-ARC_DEG, ARC_DEG));
                Vector2 pos = Projectile.Center + a.ToRotationVector2() * RADIUS;
                new EnergyHexParticle(pos, a.ToRotationVector2() * 0.6f, EnergyShield.ColorCyan, Main.rand.Next(20, 32), Main.rand.NextFloat(0.25f, 0.45f), 0.35f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
            if (Main.rand.NextBool(10))
            {
                float end = Main.rand.NextBool() ? ARC_DEG : -ARC_DEG;
                Vector2 pos = Projectile.Center + (ShieldRotation + MathHelper.ToRadians(end)).ToRotationVector2() * RADIUS;
                new StarParticle(pos, Vector2.Zero, EnergyShield.ColorWhite, 10, Main.rand.NextFloat(0.12f, 0.2f), 0.45f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                int cd = 150 + (int)(450 * Glitch01) + (Breaking ? 90 : 0);
                if (manualCancel)
                {
                    float discount = MathHelper.Lerp(0.92f, 0.55f, Health01);
                    cd = (int)(cd * discount);
                }
                Owner.GetModPlayer<EnergyShieldPlayer>().ShieldCooldown = cd;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, Vector2.Zero, ModContent.ProjectileType<EnergyChargeProj>(), 0, 0f, Projectile.owner, cd);
            }
            if (!Breaking && Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    float a = ShieldRotation + MathHelper.ToRadians(Main.rand.NextFloat(-ARC_DEG, ARC_DEG));
                    Vector2 pos = Projectile.Center + a.ToRotationVector2() * RADIUS;
                    new EnergyHexParticle(pos, a.ToRotationVector2() * 0.8f, EnergyShield.ColorCyan, Main.rand.Next(20, 34), Main.rand.NextFloat(0.4f, 0.7f), 0.7f, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                try
                {
                    shieldShader = ModContent.Request<Effect>("jarkos/Shaders/EnergyShield", AssetRequestMode.ImmediateLoad);
                }
                catch { shieldShader = null; }
                try
                {
                    impactShader = ModContent.Request<Effect>("jarkos/Shaders/ShieldImpact", AssetRequestMode.ImmediateLoad);
                }
                catch { impactShader = null; }
                try
                {
                    breakShader = ModContent.Request<Effect>("jarkos/Shaders/ShieldBreak", AssetRequestMode.ImmediateLoad);
                }
                catch { breakShader = null; }
            }
            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
            if (Breaking)
            {
                DrawBreak(noiseTex);
                return false;
            }
            Vector2 jitter = Vector2.Zero;
            if (Glitch01 > 0.3f && Main.rand.NextFloat() < Glitch01 * 0.6f)
                jitter = Main.rand.NextVector2Circular(4f * Glitch01, 4f * Glitch01);
            if (Glitch01 > 0.6f && Main.rand.NextBool(9))
                return false;
            DrawShield(noiseTex, jitter);
            DrawImpacts(noiseTex);
            return false;
        }
        private float OpenProgress => MathHelper.Clamp(Age / OPEN, 0f, 1f);
        private float CloseProgress =>
            MathHelper.Clamp((Age - (OPEN + DURATION)) / (float)CLOSE, 0f, 1f);
        private void DrawShield(Texture2D noiseTex, Vector2 jitter)
        {
            float quadScale = RADIUS * 1.45f * 2f / noiseTex.Width;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + jitter;
            if (shieldShader == null || shieldShader.Value == null)
            {
                DrawShieldFallback(drawPos);
                return;
            }
            Effect fx = shieldShader.Value;
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uOpen"]?.SetValue(OpenProgress);
            fx.Parameters["uClose"]?.SetValue(CloseProgress);
            fx.Parameters["uHealth"]?.SetValue(Health01);
            fx.Parameters["uGlitch"]?.SetValue(Glitch01);
            fx.Parameters["uColor"]?.SetValue(EnergyShield.ColorCyan.ToVector3());
            fx.Parameters["uSecondaryColor"]?.SetValue(EnergyShield.ColorDanger.ToVector3());
            fx.Parameters["uImpactA"]?.SetValue(new Vector4(impactDirs[0].X, impactDirs[0].Y, impactDirs[1].X, impactDirs[1].Y));
            fx.Parameters["uImpactB"]?.SetValue(new Vector4(impactDirs[2].X, impactDirs[2].Y, impactDirs[3].X, impactDirs[3].Y));
            fx.Parameters["uImpactAges"]?.SetValue(new Vector4(Math.Min(1f, impactAges[0] / (float)IMPACT_LIFE), Math.Min(1f, impactAges[1] / (float)IMPACT_LIFE), Math.Min(1f, impactAges[2] / (float)IMPACT_LIFE), Math.Min(1f, impactAges[3] / (float)IMPACT_LIFE)));
            fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noiseTex, drawPos, null, Color.White, ShieldRotation, noiseTex.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawShieldFallback(Vector2 drawPos)
        {
            Texture2D strip = ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/BeamStrip", AssetRequestMode.ImmediateLoad).Value;
            float open = EaseOutBack(OpenProgress);
            float fade = (1f - CloseProgress) * (0.55f + 0.45f * Health01);
            int segs = 14;

            for (int i = 0; i < segs; i++)
            {
                float t = i / (float)(segs - 1) * 2f - 1f;
                float a = ShieldRotation + MathHelper.ToRadians(ARC_DEG * open) * t;
                
                Vector2 pos = drawPos + a.ToRotationVector2() * RADIUS;
                Color c = Health01 > 0.4f ? EnergyShield.ColorCyan : EnergyShield.ColorDanger;

                Main.spriteBatch.Draw(strip, pos, null, new Color(c.R, c.G, c.B, 0) * fade, a + MathHelper.PiOver2, new Vector2(strip.Width / 2f, strip.Height / 2f), new Vector2(0.09f, 1.6f), SpriteEffects.None, 0f);
            }
        }
        private void DrawImpacts(Texture2D noiseTex)
        {
            if (impactShader == null || impactShader.Value == null)
                return;
            for (int i = 0; i < 4; i++)
            {
                if (impactAges[i] >= IMPACT_LIFE)
                    continue;

                float prog = impactAges[i] / (float)IMPACT_LIFE;
                Vector2 worldDir = impactDirs[i].RotatedBy(ShieldRotation);
                Vector2 hit = Projectile.Center + worldDir * RADIUS - Main.screenPosition;
                Effect fx = impactShader.Value;

                fx.Parameters["uProgress"]?.SetValue(prog);
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fx.Parameters["uColor"]?.SetValue(EnergyShield.ColorCyan.ToVector3());
                fx.Parameters["uSecondaryColor"]?.SetValue(EnergyShield.ColorWhite.ToVector3());
                fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);

                float scale = 80f * 2f / noiseTex.Width;
                Main.spriteBatch.Draw(noiseTex, hit, null, Color.White, worldDir.ToRotation(), noiseTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawBreak(Texture2D noiseTex)
        {
            if (breakShader == null || breakShader.Value == null)
                return;

            float prog = MathHelper.Clamp(Projectile.localAI[1] / BREAK, 0f, 1f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float quadScale = RADIUS * 2.2f * 2f / noiseTex.Width;

            Effect fx = breakShader.Value;
            fx.Parameters["uProgress"]?.SetValue(prog);
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uColor"]?.SetValue(EnergyShield.ColorCyan.ToVector3());
            fx.Parameters["uSecondaryColor"]?.SetValue(EnergyShield.ColorDanger.ToVector3());
            fx.Parameters["uNoiseTexture"]?.SetValue(noiseTex);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            
            Main.spriteBatch.Draw(noiseTex, drawPos, null, Color.White, ShieldRotation, noiseTex.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            t -= 1f;
            return 1f + c3 * t * t * t + c1 * t * t;
        }
    }
}
