using System;
using jarkos.items.weapons.melee.true_melee;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
namespace jarkos.projectiles.melee.true_melee
{
	public class AqueousFlowSwing : ModProjectile
    {
        private const bool ForceVanillaTrailShader = false;
        private const bool TransposeMatrixFix = false;

        public ref float ComboType => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];

        private int ComboIndex => (int)ComboType;
        private Player Owner => Main.player[Projectile.owner];

        private struct TrailPoint
        {
            public float Angle;
            public float SpawnTime;
            public float Bright;
        }
        
        private const int MaxTrailPoints = 90;
        private const float TrailLifetime = 15f;
        private readonly TrailPoint[] trail = new TrailPoint[MaxTrailPoints];
        private int trailHead;
        private int trailLen;
        private float lastRecordedAngle;
        private bool hasRecordedAngle;
        private readonly Vector2[] drawPositions = new Vector2[MaxTrailPoints];
        private readonly float[] drawRotations = new float[MaxTrailPoints];
        private readonly float[] drawFades = new float[MaxTrailPoints];
        private static readonly VertexStrip vertexStrip = new VertexStrip();
        private float prevAngle1, prevAngle2;
        private const int BladeFadeTime = 8;
        private float aimAngle;
        private int angDir = 1;
        private static Effect cachedEffect;
        private static EffectParameter pWvp, pColor, pSecondary, pOpacity, pSaturation, pTime;
        private static void CacheEffectParams(Effect fx)
        {
            if (ReferenceEquals(cachedEffect, fx))
                return;
            cachedEffect = fx;
            pWvp        = fx.Parameters["uWorldViewProjection"];
            pColor      = fx.Parameters["uColor"];
            pSecondary  = fx.Parameters["uSecondaryColor"];
            pOpacity    = fx.Parameters["uOpacity"];
            pSaturation = fx.Parameters["uSaturation"];
            pTime       = fx.Parameters["uTime"];
        }
        private static readonly float SpriteDiagonal = MathF.Sqrt(104f * 104f + 108f * 108f);
        private float VisualBladeLength => SpriteDiagonal * Projectile.scale;
        private float TrailTipRadius => VisualBladeLength * 0.94f;
        private float TrailInnerRadius => VisualBladeLength * 0.42f;
        private float TrailMidRadius => (TrailTipRadius + TrailInnerRadius) * 0.5f;
        private float TrailHalfWidth => (TrailTipRadius - TrailInnerRadius) * 0.5f;
        private int BaseDuration => ComboIndex switch { 0 => 22, 1 => 20, _ => 38 };
        private int SwingDuration =>
            Math.Max(10, (int)(BaseDuration / Owner.GetTotalAttackSpeed(DamageClass.Melee)));
        private float SwingArc => ComboIndex switch
        {
            0 => MathHelper.ToRadians(215f),
            1 => MathHelper.ToRadians(235f),
            _ => MathHelper.ToRadians(390f),
        };
        private float SwingDir
        {
            get
            {
                float facing = Projectile.velocity.X >= 0f ? 1f : -1f;
                return facing * (ComboIndex == 1 ? -1f : 1f);
            }
        }
        private Vector2 HoldCenter
        {
            get
            {
                float t = MathHelper.Clamp(Timer / SwingDuration, 0f, 1f);
                float thrust = MathHelper.Lerp(6f, 18f, MathF.Sin(t * MathHelper.Pi));
                return Owner.MountedCenter
                     + Projectile.rotation.ToRotationVector2() * thrust * 0.35f
                     + new Vector2(0f, Owner.gfxOffY);
            }
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 2;
            Projectile.netImportant = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(aimAngle);
            writer.Write(angDir);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            aimAngle = reader.ReadSingle();
            angDir = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                aimAngle = Projectile.velocity.ToRotation();
                float facing = Projectile.velocity.X >= 0f ? 1f : -1f;
                angDir = (int)(facing * (ComboIndex == 1 ? -1f : 1f));
                Projectile.netUpdate = true;
            }
        }
        private float SwingProgress(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float antAmount = ComboIndex == 2 ? 0.14f : 0.09f;
            float antEnd    = ComboIndex == 2 ? 0.20f : 0.12f;
            float strikeEnd = ComboIndex == 2 ? 0.80f : 0.74f;
            float overshoot = ComboIndex == 2 ? 0.07f : 0.045f;
            if (t < antEnd)
            {
                float k = t / antEnd;
                float s = k * k * k * (k * (6f * k - 15f) + 10f);
                return -antAmount * s;
            }
            if (t < strikeEnd)
            {
                float k = (t - antEnd) / (strikeEnd - antEnd);
                float e = 1f - MathF.Pow(1f - k, 5f);
                e += MathF.Sin(k * MathHelper.Pi) * 0.015f;
                return MathHelper.Lerp(-antAmount, 1f + overshoot, e);
            }
            float k2 = (t - strikeEnd) / (1f - strikeEnd);
            float osc = MathF.Exp(-4f * k2) * MathF.Cos(k2 * MathHelper.TwoPi * 1.1f);
            return 1f + overshoot * osc;
        }
        private float CurrentAngle(float t)
        {
            return aimAngle + angDir * SwingArc * (SwingProgress(t) - 0.5f);
        }
        private void PushPoint(float angle, float spawnTime, float bright)
        {
            trail[trailHead] = new TrailPoint { Angle = angle, SpawnTime = spawnTime, Bright = bright };
            trailHead = (trailHead + 1) % MaxTrailPoints;
            if (trailLen < MaxTrailPoints)
                trailLen++;
        }
        private void RecordTrail(float angle)
        {
            float now = Timer;
            if (!hasRecordedAngle)
            {
                hasRecordedAngle = true;
                lastRecordedAngle = angle;
                PushPoint(angle, now, 0.3f);
                return;
            }
            float delta = MathHelper.WrapAngle(angle - lastRecordedAngle);
            float absD = Math.Abs(delta);
            if (absD < 0.004f)
            {
                lastRecordedAngle = angle;
                return;
            }
            float bright = 0.25f + 0.75f * MathHelper.Clamp(absD / 0.22f, 0f, 1f);
            int steps = Math.Clamp((int)(absD / 0.045f) + 1, 1, 16);
            for (int i = 1; i <= steps; i++)
            {
                float f = i / (float)steps;
                PushPoint(lastRecordedAngle + delta * f, now - 1f + f, bright);
            }
            lastRecordedAngle = angle;
        }
        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead || player.CCed ||
                player.HeldItem.type != ModContent.ItemType<AqueousFlow>())
            {
                Projectile.Kill();
                return;
            }
            int duration = SwingDuration;
            if (Timer == 0f)
            {
                Projectile.scale = ComboIndex == 2 ? 1.22f : 1.02f;
                player.ChangeDir(Projectile.velocity.X >= 0f ? 1 : -1);
                if (Main.netMode != NetmodeID.Server)
                {
                    switch (ComboIndex)
                    {
                        case 0:
                            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.25f, Volume = 0.9f }, player.Center);
                            break;
                        case 1:
                            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.55f, Volume = 0.9f }, player.Center);
                            break;
                        default:
                            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.35f, Volume = 1.1f }, player.Center);
                            SoundEngine.PlaySound(SoundID.SplashWeak with { Pitch = 0.3f }, player.Center);
                            break;
                    }
                }
            }
            Projectile.Center = player.MountedCenter;
            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;
            float t = Timer / duration;
            if (Timer > duration + TrailLifetime)
            {
                Projectile.Kill();
                return;
            }
            prevAngle2 = prevAngle1;
            prevAngle1 = Projectile.rotation;
            float angle = CurrentAngle(t);
            Projectile.rotation = angle;
            float angVel = Math.Abs(MathHelper.WrapAngle(angle - prevAngle1));
            float speed01 = MathHelper.Clamp(angVel / 0.30f, 0f, 1f);
            float baseScale = ComboIndex == 2 ? 1.22f : 1.02f;
            Projectile.scale = baseScale * (1f + 0.06f * speed01);
            Projectile.Opacity = Timer <= duration
                ? 1f
                : 1f - MathHelper.Clamp((Timer - duration) / BladeFadeTime, 0f, 1f);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                angle - MathHelper.PiOver2);
            player.itemRotation = MathHelper.WrapAngle(angle - (player.direction < 0 ? MathHelper.Pi : 0f));
            if (Timer <= duration)
                RecordTrail(angle);
            if (Timer <= duration && speed01 > 0.15f)
            {
                Vector2 bladeDir = angle.ToRotationVector2();
                Vector2 tangent = (angle + SwingDir * MathHelper.PiOver2).ToRotationVector2();
                int dustCount = speed01 > 0.6f ? 2 : 1;
                for (int i = 0; i < dustCount; i++)
                {
                    float along = Main.rand.NextFloat(0.55f, 0.98f);
                    Dust d = Dust.NewDustPerfect(
                        HoldCenter + bladeDir * VisualBladeLength * along,
                        DustID.Water,
                        tangent * Main.rand.NextFloat(3f, 8f) * along * speed01,
                        0, default, Main.rand.NextFloat(0.9f, 1.5f));
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }
                if (speed01 > 0.5f && Main.rand.NextBool(4))
                {
                    Dust b = Dust.NewDustPerfect(
                        HoldCenter + bladeDir * VisualBladeLength * 0.97f,
                        DustID.BubbleBurst_Blue,
                        tangent * Main.rand.NextFloat(4f, 7f), 100, default,
                        Main.rand.NextFloat(0.8f, 1.2f));
                    b.noGravity = true;
                }
            }
            Lighting.AddLight(HoldCenter + angle.ToRotationVector2() * VisualBladeLength * 0.7f,
                0.25f, 0.55f, 0.95f);
            if (ComboIndex == 2 && Projectile.localAI[0] == 0f && t >= 0.45f &&
                Main.myPlayer == Projectile.owner)
            {
                Projectile.localAI[0] = 1f;
                Vector2 waveDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    player.MountedCenter + waveDir * VisualBladeLength * 0.6f, waveDir * 11f,
                    ModContent.ProjectileType<AqueousWave>(),
                    (int)(Projectile.damage * 0.6f), Projectile.knockBack * 0.5f, Projectile.owner);
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item21 with { Pitch = 0.4f }, player.Center);
                }
            }
            Timer++;
        }
        public override bool? CanDamage()
        {
            float t = Timer / SwingDuration;
            return (t > 0.12f && t < 0.85f && Timer <= SwingDuration) ? null : false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float cp = 0f;
            Vector2 start = HoldCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * VisualBladeLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 24f * Projectile.scale, ref cp);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (ComboIndex == 2)
                modifiers.Knockback *= 1.5f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Wet, 240);
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.SplashWeak with { Pitch = 0.5f, Volume = 0.8f }, target.Center);
                for (int i = 0; i < 14; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Water,
                        Main.rand.NextVector2Circular(6f, 6f), 0, default, Main.rand.NextFloat(1.2f, 2f));
                    d.noGravity = true;
                }
                if (ComboIndex == 2 && Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    var punch = new PunchCameraModifier(target.Center,
                        Main.rand.NextVector2Unit(), 6f, 8f, 12, 1000f, FullName);
                    Main.instance.CameraModifiers.Add(punch);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawWaterTrail();
            DrawBlade(lightColor);
            return false;
        }
        private void DrawWaterTrail()
        {
            int visible = 0;
            float now = Timer;
            Vector2 center = HoldCenter;
            float midRadius = TrailMidRadius;
            float dirSign = SwingDir;
            for (int i = 0; i < trailLen; i++)
            {
                int idx = (trailHead - 1 - i + MaxTrailPoints * 2) % MaxTrailPoints;
                float age = now - trail[idx].SpawnTime;
                if (age >= TrailLifetime)
                    break;
                float life = 1f - age / TrailLifetime;
                float smooth = life * life * (3f - 2f * life);
                drawPositions[visible] = center + trail[idx].Angle.ToRotationVector2() * midRadius;
                drawRotations[visible] = trail[idx].Angle + dirSign * MathHelper.PiOver2;
                drawFades[visible] = smooth * trail[idx].Bright;
                visible++;
            }
            if (visible < 2)
                return;
            int last = visible - 1;
            float FadeAt(float p) => drawFades[(int)(p * last + 0.5f)];
            Main.spriteBatch.End();
            GraphicsDevice gd = Main.instance.GraphicsDevice;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.BlendState = BlendState.Additive;
            bool customApplied = false;
            Effect fx = AqueousShaderSystem.TrailEffect;
            if (!ForceVanillaTrailShader && fx != null && AqueousShaderSystem.PerlinNoise != null)
            {
                try
                {
                    CacheEffectParams(fx);
                    Viewport vp = gd.Viewport;
                    Matrix ortho = Matrix.CreateOrthographicOffCenter(0f, vp.Width, vp.Height, 0f, 0f, 1f);
                    Matrix wvp = Main.GameViewMatrix.TransformationMatrix * ortho;
                    if (TransposeMatrixFix)
                        wvp = Matrix.Transpose(wvp);
                    pWvp?.SetValue(wvp);
                    pColor?.SetValue(new Color(130, 225, 255).ToVector3());
                    pSecondary?.SetValue(new Color(8, 55, 160).ToVector3());
                    pOpacity?.SetValue(1.25f);
                    pSaturation?.SetValue(SwingDir);
                    pTime?.SetValue(Main.GlobalTimeWrappedHourly);
                    gd.Textures[1] = AqueousShaderSystem.PerlinNoise;
                    gd.SamplerStates[1] = SamplerState.LinearWrap;
                    fx.CurrentTechnique.Passes[0].Apply();
                    customApplied = true;
                }
                catch
                {
                    customApplied = false;
                }
            }
            if (!customApplied)
            {
                MiscShaderData shader = GameShaders.Misc["MagicMissile"];
                shader.UseColor(new Color(90, 200, 255));
                shader.UseSecondaryColor(new Color(15, 70, 180));
                shader.UseSaturation(-2.5f);
                shader.UseOpacity(3f);
                shader.Apply();
            }
            float halfWidth = TrailHalfWidth;
            vertexStrip.PrepareStrip(drawPositions, drawRotations,
                p => Color.White * FadeAt(p),
                p => halfWidth * (0.30f + 0.70f * FadeAt(p)),
                -Main.screenPosition,
                visible, includeBacksides: true);
            vertexStrip.DrawTrail();
            float coreHalf = 9f * Projectile.scale;
            float coreMid = TrailTipRadius - coreHalf;
            for (int i = 0; i < visible; i++)
            {
                int idx = (trailHead - 1 - i + MaxTrailPoints * 2) % MaxTrailPoints;
                drawPositions[i] = center + trail[idx].Angle.ToRotationVector2() * coreMid;
            }
            vertexStrip.PrepareStrip(drawPositions, drawRotations,
                p => new Color(200, 245, 255) * (FadeAt(p) * 0.9f),
                p => coreHalf * (0.35f + 0.65f * FadeAt(p)),
                -Main.screenPosition,
                visible, includeBacksides: true);
            vertexStrip.DrawTrail();
            gd.Textures[1] = null;
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            gd.BlendState = BlendState.AlphaBlend;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawBlade(Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = HoldCenter - Main.screenPosition;
            Vector2 origin = new(0f, tex.Height);
            void DrawAt(float ang, Color col, float scaleMul)
            {
                float rot = ang + MathHelper.PiOver4;
                SpriteEffects eff = SpriteEffects.None;
                Vector2 org = origin;
                if (Owner.direction < 0)
                {
                    eff = SpriteEffects.FlipHorizontally;
                    org = new Vector2(tex.Width, tex.Height);
                    rot = ang + MathHelper.PiOver4 + MathHelper.PiOver2;
                }
                Main.EntitySpriteDraw(tex, drawPos, null, col, rot, org,
                    Projectile.scale * scaleMul, eff, 0);
            }
            float angVel = Math.Abs(MathHelper.WrapAngle(Projectile.rotation - prevAngle1));
            float blur = MathHelper.Clamp(angVel / 0.30f, 0f, 1f) * Projectile.Opacity;
            if (blur > 0.05f)
            {
                Color ghost = new Color(90, 200, 255, 0);
                DrawAt(prevAngle2, ghost * (0.18f * blur), 1f);
                DrawAt(prevAngle1, ghost * (0.35f * blur), 1f);
            }
            Color glow = new Color(90, 200, 255, 0) * 0.4f * Projectile.Opacity;
            for (int i = 0; i < 3; i++)
            {
                float a = MathHelper.TwoPi / 3f * i + Main.GlobalTimeWrappedHourly * 3f;
                Vector2 off = a.ToRotationVector2() * 2.5f;
                Main.EntitySpriteDraw(tex, drawPos + off, null, glow,
                    Owner.direction < 0
                        ? Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2
                        : Projectile.rotation + MathHelper.PiOver4,
                    Owner.direction < 0 ? new Vector2(tex.Width, tex.Height) : origin,
                    Projectile.scale * 1.03f,
                    Owner.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
            DrawAt(Projectile.rotation, lightColor * Projectile.Opacity, 1f);
        }
    }
}
