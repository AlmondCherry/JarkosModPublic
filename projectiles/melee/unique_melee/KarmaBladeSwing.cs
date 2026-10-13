using System;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
namespace jarkos.projectiles.melee.unique_melee
{
    public class KarmaBladeSwing : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/KarmaBlade";
        public const int SWING_TIME = 18;
        public const int FINISHER_TIME = 38;
        private const int LINGER = 16;
        private const float BLADE_LEN = 132f;
        private const float ANTIC = 0.16f;
        private const float IAI_ANTIC = 0.52f;
        public static readonly Color ColorEdge = new(250, 130, 190);
        public static readonly Color ColorMid = new(150, 60, 235);
        public static readonly Color ColorCore = new(245, 230, 255);
        public static readonly Color ColorDeep = new(75, 25, 130);
        private static Asset<Effect> karmaShader;
        private static bool shaderTried;
        private int ComboStep => (int)Projectile.ai[0];
        private bool IsFinisher => ComboStep >= 2;
        private int SwingTimeTotal => IsFinisher ? FINISHER_TIME : SWING_TIME;
        private float Anticipation => IsFinisher ? IAI_ANTIC : ANTIC;
        private float SwingArc => IsFinisher ? 4.6f : 3.6f;
        private float VisualScale => IsFinisher ? 1.30f : 1.05f;
        private float StretchK
        {
            get
            {
                float t = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
                if (t <= Anticipation || timer > SwingTimeTotal)
                    return 1f;
                float s = (t - Anticipation) / (1f - Anticipation);
                float bump = (float)Math.Sin(MathHelper.Pi * MathHelper.Clamp(s * 1.6f, 0f, 1f));
                bump *= bump;
                return 1f + (IsFinisher ? 0.45f : 0.25f) * bump;
            }
        }
        private int angDir = 1;
        private float aimAngle;
        private int timer;
        private int freezeFrames;
        private bool strikeSoundPlayed;
        private float prevRot;
        private float lastAngSpeed;
        private float visSpan;
        private bool iaiFlashDone;
        private const int MAX_TRAIL = 40;
        private const float TRAIL_MAX_AGE = 12f;
        private int trailCount;
        private readonly float[] trailRot = new float[MAX_TRAIL];
        private readonly float[] trailAge = new float[MAX_TRAIL];
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 300;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
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
                aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
                angDir = (int)Projectile.ai[1];
                if (Math.Cos(aimAngle) < 0)
                    angDir = -angDir;
                Projectile.netUpdate = true;
            }

            prevRot = CurrentRotation();
            Projectile.rotation = prevRot;

            if (IsFinisher && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item101 with { Volume = 0.45f, Pitch = 0.6f }, Projectile.Center);
                new Circular_glow(Main.player[Projectile.owner].MountedCenter, Vector2.Zero, ColorMid, 16, 0.14f, 0.5f).Spawn();
            }
        }
        private float Eased(float t)
        {
            float antic = Anticipation;
            if (t < antic)
            {
                float a = t / antic;
                float depth = IsFinisher ? -0.025f : -0.085f;
                return depth * (float)Math.Sin(a * MathHelper.PiOver2);
            }
            float s = (t - antic) / (1f - antic);
            float power = IsFinisher ? 7.5f : 5.0f;
            float e = 1f - (float)Math.Pow(1f - s, power);
            float depth2 = IsFinisher ? -0.025f : -0.085f;
            return MathHelper.Lerp(depth2, 1f, e);
        }
        private float CurrentRotation()
        {
            float t = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
            return aimAngle + angDir * SwingArc * (Eased(t) - 0.5f);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = player.MountedCenter;
            Projectile.timeLeft = 2 + LINGER;
            if (freezeFrames > 0 && timer <= SwingTimeTotal)
            {
                freezeFrames--;
                HoldPlayer(player, true);
                return;
            }
            AgeTrail();
            if (timer > SwingTimeTotal + LINGER)
            {
                Projectile.Kill();
                return;
            }
            timer++;
            float rot = CurrentRotation();
            float angSpeed = MathHelper.WrapAngle(rot - prevRot);
            prevRot = rot;
            Projectile.rotation = rot;
            lastAngSpeed = angSpeed;
            visSpan = Math.Min(visSpan * 0.80f + Math.Abs(angSpeed) * 2.4f, 2.55f);
            if (timer > SwingTimeTotal)
                visSpan *= 0.86f;
            bool swinging = timer <= SwingTimeTotal;
            HoldPlayer(player, swinging);
            if (!swinging)
                return;
            float t = timer / (float)SwingTimeTotal;
            if (!strikeSoundPlayed && t >= Anticipation)
            {
                strikeSoundPlayed = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    if (IsFinisher)
                    {
                        SoundEngine.PlaySound(SoundID.Item71 with { Volume = 1f, Pitch = 0.55f }, Projectile.Center);
                        SoundEngine.PlaySound(SoundID.Item60 with { Volume = 0.5f, Pitch = 0.4f }, Projectile.Center);
                    }
                    else
                    {
                        float pitch = ComboStep == 0 ? 0.05f : 0.22f;
                        SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.85f, Pitch = pitch }, Projectile.Center);
                    }
                }
            }
            if (IsFinisher && !iaiFlashDone && t >= Anticipation)
            {
                iaiFlashDone = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    IaiBurst(rot);
                }
            }
            if (Math.Abs(angSpeed) > 0.03f)
                PushTrail(rot);
            if (Main.netMode != NetmodeID.Server)
                SpawnSwingParticles(rot, angSpeed);
            Lighting.AddLight(Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * 0.6f, ColorMid.ToVector3() * 0.7f);
        }
        private void HoldPlayer(Player player, bool swinging)
        {
            if (!swinging)
                return;
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
        }
        private void IaiBurst(float rot)
        {
            Vector2 center = Projectile.Center;
            Vector2 rotV = rot.ToRotationVector2();
            Vector2 tip = center + rotV * BLADE_LEN * VisualScale;
            new Circleedge(center, Vector2.Zero, ColorEdge, 20, 0.22f, 0.9f).Spawn();
            new Circular_glow(center, Vector2.Zero, ColorCore, 14, 0.20f, 0.8f).Spawn();
            new StarParticle(tip, Vector2.Zero, ColorCore, 20, 0.24f, 1f, 0.05f).Spawn();
            Vector2 tangent = rotV.RotatedBy(MathHelper.PiOver2 * angDir);
            for (int i = 0; i < 7; i++)
            {
                Vector2 vel = tangent.RotatedByRandom(0.5f) * Main.rand.NextFloat(7f, 13f);
                new Stretching_particle_out(tip, vel, i % 3 == 0 ? ColorCore : ColorEdge, 24, Main.rand.NextFloat(55f, 95f), Main.rand.NextFloat(4f, 6f), 0.02f).Spawn();
            }
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(center, rotV, 6f, 7f, 9, 900f, FullName));
        }
        private void SpawnSwingParticles(float rot, float angSpeed)
        {
            float speedFactor = MathHelper.Clamp(Math.Abs(angSpeed) / 0.4f, 0f, 1f);
            if (speedFactor < 0.25f)
                return;
            Vector2 rotV = rot.ToRotationVector2();
            Vector2 tangent = rotV.RotatedBy(MathHelper.PiOver2 * angDir);
            float reach = BLADE_LEN * VisualScale * StretchK;
            if (Main.rand.NextFloat() < speedFactor)
            {
                float along = Main.rand.NextFloat(0.55f, 1f);
                Vector2 pos = Projectile.Center + rotV * reach * along;
                new Spark(pos, tangent * Main.rand.NextFloat(3f, 8f) * speedFactor, Main.rand.NextBool(3) ? ColorCore : ColorEdge, 20, Main.rand.NextFloat(0.3f, 0.55f), 0.85f).Spawn();
            }
            if (Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.6f, 1f);
                Vector2 pos = Projectile.Center + rotV * reach * along;
                new Line_glow(pos, -tangent * Main.rand.NextFloat(1f, 3f), ColorMid, 14, Main.rand.NextFloat(0.4f, 0.65f), 0.6f, 0f).Spawn();
            }
            if (Main.rand.NextBool(3))
            {
                float along = Main.rand.NextFloat(0.5f, 1f);
                Vector2 pos = Projectile.Center + rotV * reach * along;
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, tangent * Main.rand.NextFloat(2f, 5f), 0, default, Main.rand.NextFloat(1f, 1.6f));
                d.noGravity = true;
            }
            if (IsFinisher && Main.rand.NextBool(2))
            {
                Vector2 tip = Projectile.Center + rotV * reach;
                new Stretching_particle_out(tip, tangent * Main.rand.NextFloat(6f, 11f), Main.rand.NextBool(3) ? ColorCore : ColorMid, 22, Main.rand.NextFloat(45f, 80f), Main.rand.NextFloat(3.5f, 5.5f), 0.02f).Spawn();
            }
        }
        private void PushTrail(float rot)
        {
            for (int i = Math.Min(trailCount, MAX_TRAIL - 1); i > 0; i--)
            {
                trailRot[i] = trailRot[i - 1];
                trailAge[i] = trailAge[i - 1];
            }
            trailRot[0] = rot;
            trailAge[0] = 0f;
            trailCount = Math.Min(trailCount + 1, MAX_TRAIL);
        }
        private void AgeTrail()
        {
            for (int i = 0; i < trailCount; i++)
                trailAge[i] += 1f;
            while (trailCount > 0 && trailAge[trailCount - 1] > TRAIL_MAX_AGE)
                trailCount--;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float t = timer / (float)SwingTimeTotal;
            if (t < Anticipation)
                return false;
            float reach = BLADE_LEN * VisualScale * StretchK;
            int steps = Math.Clamp((int)Math.Ceiling(Math.Abs(lastAngSpeed) / 0.22f), 1, 10);
            for (int i = 0; i <= steps; i++)
            {
                float rot = Projectile.rotation - lastAngSpeed * (i / (float)steps);
                float collisionPoint = 0f;
                Vector2 tip = Projectile.Center + rot.ToRotationVector2() * reach;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, tip, 30f, ref collisionPoint))
                    return true;
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (!IsFinisher && Projectile.owner == Main.myPlayer)
                player.GetModPlayer<KarmaBladePlayer>().AddKarma(1);
            freezeFrames = Math.Max(freezeFrames, IsFinisher ? 5 : 2);
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(target.Center, (target.Center - player.MountedCenter).SafeNormalize(Vector2.UnitX), IsFinisher ? 8f : 4f, 6f, IsFinisher ? 12 : 8, 900f, FullName));
                }
                Vector2 hitPos = target.Center;
                new Circleedge(hitPos, Vector2.Zero, ColorEdge, 16, IsFinisher ? 0.2f : 0.12f, 0.85f).Spawn();
                for (int i = 0; i < (IsFinisher ? 8 : 4); i++)
                {
                    Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2.5f, 7f);
                    new Spark(hitPos, vel, Main.rand.NextBool(3) ? ColorCore : ColorMid, 18, Main.rand.NextFloat(0.3f, 0.5f), 0.8f).Spawn();
                }
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.5f, Pitch = 0.7f }, hitPos);
            }
        }
        private static bool TryGetShader(out Effect fx)
        {
            fx = null;
            if (!shaderTried)
            {
                shaderTried = true;
                try
                {
                    karmaShader = ModContent.Request<Effect>("jarkos/Shaders/KarmaSlash", AssetRequestMode.ImmediateLoad);
                }
                catch
                {
                    karmaShader = null;
                }
            }
            if (karmaShader == null || !karmaShader.IsLoaded)
                return false;
            fx = karmaShader.Value;
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (TryGetShader(out Effect fx))
                DrawKarmaSlashQuad(player, fx);
            else
                DrawFallbackFan(player);
            return false;
        }
        private void DrawKarmaSlashQuad(Player player, Effect fx)
        {
            float swingT = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
            if (swingT <= Anticipation && timer <= SwingTimeTotal)
                return;
            float fade = timer <= SwingTimeTotal
                ? MathHelper.Clamp((swingT - Anticipation) / 0.10f, 0f, 1f)
                : MathHelper.Clamp(1f - (timer - SwingTimeTotal) / (float)LINGER, 0f, 1f);
            if (fade <= 0.03f)
                return;
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/KarmaNoise").Value;
            float maxLen = BLADE_LEN * VisualScale * (IsFinisher ? 1.8f : 1.5f);
            float quadHalf = maxLen * 1.45f;
            float quadScale = quadHalf * 2f / noise.Width;
            Vector2 lead = (timer <= SwingTimeTotal ? Projectile.rotation : trailRot[0]).ToRotationVector2();
            float span = Math.Min(visSpan, 2.55f);
            if (span < 0.20f)
                return;
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uLead"]?.SetValue(new Vector3(lead.X, lead.Y, angDir));
            fx.Parameters["uSpan"]?.SetValue(span);
            fx.Parameters["uR0"]?.SetValue(BLADE_LEN * VisualScale * StretchK / quadHalf);
            fx.Parameters["uR1"]?.SetValue(BLADE_LEN * VisualScale * 0.70f / quadHalf);
            fx.Parameters["uInner"]?.SetValue(34f / quadHalf);
            fx.Parameters["uStretch"]?.SetValue(1f + (StretchK - 1f) * (0.85f / 0.45f));
            fx.Parameters["uFinisher"]?.SetValue(IsFinisher ? 1f : 0f);
            fx.Parameters["uFade"]?.SetValue(fade * 0.95f);
            fx.Parameters["uColorEdge"]?.SetValue(ColorEdge.ToVector3());
            fx.Parameters["uColorMid"]?.SetValue(ColorMid.ToVector3());
            fx.Parameters["uColorCore"]?.SetValue(ColorCore.ToVector3());
            fx.Parameters["uNoiseTexture"]?.SetValue(noise);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noise, player.MountedCenter - Main.screenPosition, null, Color.White, 0f, noise.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawFallbackFan(Player player)
        {
            if (trailCount < 2)
                return;
            Texture2D glow = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 center = player.MountedCenter - Main.screenPosition;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < trailCount; i++)
            {
                float p = trailAge[i] / TRAIL_MAX_AGE;
                float a = (1f - p) * (1f - p) * 0.55f;
                Color c = Color.Lerp(ColorMid, ColorEdge, p) * a;
                Vector2 pos = center + trailRot[i].ToRotationVector2() * BLADE_LEN * VisualScale * 0.8f;
                Main.spriteBatch.Draw(glow, pos, null, c, trailRot[i], glow.Size() * 0.5f, new Vector2(1.6f, 0.5f), SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
