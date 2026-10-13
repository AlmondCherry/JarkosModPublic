using System;
using jarkos.particles;
using jarkos.UI;
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

namespace jarkos.projectiles.melee.true_melee
{
	public class Pyromelt_slash : ModProjectile {
		public override string Texture => "jarkos/items/weapons/melee/true_melee/Pyromelt";
        public float SWING_TIME = 26;
        public float FINISHER_TIME = 44;
        private const int LINGER = 14;
        private const float BLADE_LEN = 148f;
        private const float ANTIC = 0.18f;
        
        public static readonly Color ColorBlade = new(255, 90, 10);
        public static readonly Color ColorCyan = new(255, 160, 30);
        public static readonly Color ColorGold = new(255, 210, 50);
        public static readonly Color ColorCore = new(255, 245, 190);

        private static Asset<Effect> riftShader;
        private static bool shaderTried;
        private static BasicEffect basicFallback;

        private int ComboStep => (int)Projectile.ai[0];
        private float StretchK
        {
            get
            {
                float t = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
                if (t <= ANTIC || timer > SwingTimeTotal)
                    return 1f;
                float s = (t - ANTIC) / (1f - ANTIC);
                float bump = (float)Math.Sin(MathHelper.Pi * s);
                bump *= bump;
                return 1f + (IsFinisher ? 0.38f : 0.22f) * bump;
            }
        }
        private bool IsFinisher => ComboStep >= 2;
        private float SwingTimeTotal => IsFinisher ? FINISHER_TIME : SWING_TIME;
        private float SwingArc => IsFinisher ? MathHelper.TwoPi + 1.5f : 4.5f;
        private float VisualScale => IsFinisher ? 1.22f : 1.08f;
        private int angDir = 1;
        private float aimAngle;
        private int timer;
        private int freezeFrames;
        private bool strikeSoundPlayed;
        private float prevRot;
        private float lastAngSpeed;
        private const int MAX_TRAIL = 48;
        private const float TRAIL_MAX_AGE = 13f;
        private int trailCount;
        private readonly float[] trailRot = new float[MAX_TRAIL];
        private readonly float[] trailAge = new float[MAX_TRAIL];
        private readonly float[] trailLen = new float[MAX_TRAIL];
        private bool stretchFlashDone;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
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

        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            float baseSwingTime = 26f;
            float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
            if (player.HeldItem != null && !player.HeldItem.IsAir)
            {
                meleeSpeedMultiplier *= player.HeldItem.GetGlobalItem<CalibratedWeaponGlobalItem>().UseSpeedMultiplier(player.HeldItem, player);
            }
            if (meleeSpeedMultiplier < 0.01f) meleeSpeedMultiplier = 0.01f;

            SWING_TIME = baseSwingTime / meleeSpeedMultiplier;
            FINISHER_TIME = (baseSwingTime * 1.5f) / meleeSpeedMultiplier;

            aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
            angDir = (int)Projectile.ai[1];
            if (Math.Cos(aimAngle) < 0)
                angDir = -angDir;
            prevRot = CurrentRotation();
            Projectile.rotation = prevRot;
            new Circular_glow(Main.player[Projectile.owner].MountedCenter, Vector2.Zero, ColorGold, 14, 0.16f, 0.6f).Spawn();
            if (IsFinisher)
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.55f, Pitch = 0.2f }, Projectile.Center);
        }

        private float Eased(float t)
        {
            if (t < ANTIC)
            {
                float a = t / ANTIC;
                return -0.085f * (float)Math.Sin(a * MathHelper.PiOver2);
            }
            float s = (t - ANTIC) / (1f - ANTIC);
            float power = IsFinisher ? 3.4f : 4.6f;
            float e = 1f - (float)Math.Pow(1f - s, power);
            return MathHelper.Lerp(-0.085f, 1f, e);
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
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }

            timer++;
            float rot = CurrentRotation();
            float angSpeed = MathHelper.WrapAngle(rot - prevRot);
            prevRot = rot;
            Projectile.rotation = rot;
            lastAngSpeed = angSpeed;

            bool swinging = timer <= SwingTimeTotal;
            HoldPlayer(player, swinging);

            if (!swinging)
                return;

            float t = timer / (float)SwingTimeTotal;
            if (!strikeSoundPlayed && t >= ANTIC)
            {
                strikeSoundPlayed = true;
                float pitch = ComboStep == 0 ? -0.1f : ComboStep == 1 ? 0.12f : -0.4f;
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.9f, Pitch = pitch }, Projectile.Center);
                if (IsFinisher)
                    SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.7f, Pitch = 0.1f }, Projectile.Center);
            }

            if (Math.Abs(angSpeed) > 0.035f)
                PushTrail(rot, BLADE_LEN * VisualScale * StretchK + 6f);

            if (!stretchFlashDone && StretchK > (IsFinisher ? 1.32f : 1.18f))
            {
                stretchFlashDone = true;
                Vector2 tip = Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * VisualScale * StretchK;
                SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.5f, Pitch = 0.5f }, tip);
                new StarParticle(tip, Vector2.Zero, ColorCore, 18, 0.2f, 1f, 0.06f).Spawn();
                new Circleedge(tip, Vector2.Zero, ColorCyan, 14, 0.13f, 0.8f).Spawn();
                Vector2 tangent2 = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * angDir);
                for (int i = 0; i < 4; i++)
                    new Stretching_particle_out(tip, tangent2 * Main.rand.NextFloat(7f, 12f), i == 0 ? ColorCore : ColorCyan, 20, Main.rand.NextFloat(50f, 90f), Main.rand.NextFloat(4f, 6f), 0.02f).Spawn();
            }

            SpawnSwingParticles(rot, angSpeed);
            Lighting.AddLight(Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * 0.6f, ColorCyan.ToVector3() * 0.75f);
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

        private void SpawnSwingParticles(float rot, float angSpeed)
        {
            float speedFactor = MathHelper.Clamp(Math.Abs(angSpeed) / 0.35f, 0f, 1f);
            if (speedFactor < 0.2f)
                return;

            Vector2 rotV = rot.ToRotationVector2();
            Vector2 tangent = rotV.RotatedBy(MathHelper.PiOver2 * angDir);
            float stretch = StretchK;

            
            if (Main.rand.NextFloat() < speedFactor)
            {
                float along = Main.rand.NextFloat(0.4f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretch * along;
                new Spark(pos, tangent * Main.rand.NextFloat(3f, 8f) * speedFactor, Main.rand.NextBool(3) ? ColorCore : ColorCyan, 22, Main.rand.NextFloat(0.35f, 0.65f), 0.85f).Spawn();
            }

            
            if (Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.5f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretch * along;
                Vector2 vel = tangent * Main.rand.NextFloat(4f, 10f) + Main.rand.NextVector2Circular(2f, 2f);
                new Spark_gravit(pos, vel, ColorGold, 240, Main.rand.NextFloat(0.8f, 1.4f), 1f, 0.15f).Spawn();
            }

            
            if (Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.4f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretch * along;
                Dust d = Dust.NewDustPerfect(pos, Main.rand.NextBool(3) ? DustID.SolarFlare : DustID.Torch, tangent * Main.rand.NextFloat(2f, 6f), 0, default, Main.rand.NextFloat(1.2f, 2f));
                d.noGravity = true;
            }

            
            if (stretch > 1.12f && Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.6f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretch * along;
                new Line_glow(pos, -tangent * Main.rand.NextFloat(1f, 4f), ColorCyan, 14, Main.rand.NextFloat(0.4f, 0.7f), 0.65f, 0f).Spawn();
            }

            
            if (IsFinisher && Main.rand.NextBool(2))
            {
                Vector2 tip = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretch;
                new Stretching_particle_out(tip, tangent * Main.rand.NextFloat(6f, 12f), Main.rand.NextBool(3) ? ColorGold : ColorBlade, 26, Main.rand.NextFloat(40f, 80f), Main.rand.NextFloat(5f, 8f), 0.02f).Spawn();
            }
        }

        private void PushTrail(float rot, float len)
        {
            int last = Math.Min(trailCount, MAX_TRAIL - 1);
            for (int i = last; i > 0; i--)
            {
                trailRot[i] = trailRot[i - 1];
                trailAge[i] = trailAge[i - 1];
                trailLen[i] = trailLen[i - 1];
            }
            trailRot[0] = rot;
            trailAge[0] = 0f;
            trailLen[0] = len;
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
            if (timer > SwingTimeTotal)
                return false;
            float t = timer / (float)SwingTimeTotal;
            if (t < ANTIC || t > 0.95f)
                return false;
            float point = 0f;
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Vector2 start = Projectile.Center + dir * 18f;
            Vector2 end = Projectile.Center + dir * BLADE_LEN * VisualScale * StretchK;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 44f * VisualScale, ref point);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
            if (IsFinisher)
                modifiers.Knockback *= 1.5f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            freezeFrames = IsFinisher ? 6 : 3;
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(target.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), IsFinisher ? 9f : 5f, 7f, IsFinisher ? 16 : 10, 900f, FullName));
            
            Vector2 hitPos = target.Center;
            Vector2 dirOut = (hitPos - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            new Circleedge(hitPos, Vector2.Zero, ColorCyan, 18, 0.16f, 0.85f).Spawn();
            new StarParticle(hitPos, Vector2.Zero, ColorCore, 26, 0.14f, 1f, 0.05f).Spawn();
            
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(5f, 12f);
                new Spark_gravit(hitPos, v, ColorGold, 250, Main.rand.NextFloat(0.9f, 1.5f), 1f, 0.1f).Spawn();
            }
            for (int i = 0; i < 4; i++)
            {
                new Line_glow(hitPos, Vector2.Zero, ColorBlade, 16, Main.rand.NextFloat(0.5f, 0.8f), 0.7f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            target.AddBuff(BuffID.OnFire3, 240);

            
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, -2f));
                Dust fire = Dust.NewDustDirect(target.Center - new Vector2(10), 20, 20, DustID.Torch, vel.X, vel.Y, 80, default, Main.rand.NextFloat(2f, 3f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-10f, -3f));
                Dust solar = Dust.NewDustDirect(target.Center - new Vector2(8), 16, 16, DustID.SolarFlare, vel.X, vel.Y, 100, default, 2.2f);
                solar.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.3f, Volume = 0.85f }, hitPos);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            DrawFinisherRing(player);
            DrawTrail(player);
            if (timer <= SwingTimeTotal)
                DrawBlade(player, lightColor);
            return false;
        }

        private void DrawFinisherRing(Player player)
        {
            if (!IsFinisher || timer > SwingTimeTotal)
                return;
            float t = timer / (float)SwingTimeTotal;
            float fade = MathHelper.Clamp(t / 0.25f, 0f, 1f) * MathHelper.Clamp((1f - t) / 0.2f, 0f, 1f);
            if (fade <= 0.02f)
                return;
            Texture2D ring = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftRing").Value;
            Vector2 drawPos = player.MountedCenter - Main.screenPosition;
            Vector2 origin = ring.Size() / 2f;
            if (TryGetSpinShader(out Effect sfx))
            {
                float radius = BLADE_LEN * VisualScale * 1.35f;
                float quadScale = radius * 2f / (ring.Width * 0.95f);
                float g = MathHelper.Clamp((t - ANTIC) / (1f - ANTIC), 0f, 1f);
                float grow = 1f - (1f - g) * (1f - g);
                sfx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                sfx.Parameters["uFade"]?.SetValue(fade);
                sfx.Parameters["uGrow"]?.SetValue(grow);
                sfx.Parameters["uDir"]?.SetValue((float)angDir);
                sfx.Parameters["uNoiseTexture"]?.SetValue(ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, sfx, Main.GameViewMatrix.TransformationMatrix);
                Main.spriteBatch.Draw(ring, drawPos, null, Color.White, 0f, origin, quadScale, SpriteEffects.None, 0f);
                Main.spriteBatch.End();
                RestartSpriteBatch();
                return;
            }
            float spin = Main.GlobalTimeWrappedHourly * 5f;
            float scale = BLADE_LEN * VisualScale * 2.1f / ring.Width;
            Main.spriteBatch.Draw(ring, drawPos, null, new Color(ColorCyan.R, ColorCyan.G, ColorCyan.B, 0) * (0.5f * fade), spin, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(ring, drawPos, null, new Color(ColorGold.R, ColorGold.G, ColorGold.B, 0) * (0.38f * fade), -spin * 0.7f, origin, scale * 0.78f, SpriteEffects.None, 0f);
        }

        private static Asset<Effect> spinShader;
        private static bool spinShaderTried;
        private static bool TryGetSpinShader(out Effect fx)
        {
            if (!spinShaderTried)
            {
                spinShaderTried = true;
                try
                {
                    spinShader = ModContent.Request<Effect>("jarkos/Shaders/SpinRift", AssetRequestMode.ImmediateLoad);
                }
                catch { spinShader = null; }
            }
            fx = spinShader != null ? spinShader.Value : null;
            return fx != null;
        }

        private static bool TryGetShader(out Effect fx)
        {
            if (!shaderTried)
            {
                shaderTried = true;
                try
                {
                    riftShader = ModContent.Request<Effect>("jarkos/Shaders/RiftSlash", AssetRequestMode.ImmediateLoad);
                }
                catch { riftShader = null; }
            }
            fx = riftShader != null ? riftShader.Value : null;
            return fx != null;
        }

        private static BasicEffect GetBasicFallback(GraphicsDevice gd)
        {
            if (basicFallback == null || basicFallback.IsDisposed)
                basicFallback = new BasicEffect(gd) { TextureEnabled = true, VertexColorEnabled = true };
            return basicFallback;
        }

        private void DrawTrail(Player player)
        {
            if (trailCount < 2)
                return;
            bool useShader = TryGetShader(out Effect fx);
            const int SUB = 3;
            int segs = (trailCount - 1) * SUB;
            var verts = new VertexPositionColorTexture[(segs + 1) * 2];
            Vector2 center = player.MountedCenter;
            float innerR = 26f;
            int v = 0;
            for (int i = 0; i <= segs; i++)
            {
                float fi = i / (float)SUB;
                int i0 = (int)fi;
                int i1 = Math.Min(i0 + 1, trailCount - 1);
                float fr = fi - i0;
                float rot = trailRot[i0] + MathHelper.WrapAngle(trailRot[i1] - trailRot[i0]) * fr;
                float age = MathHelper.Lerp(trailAge[i0], trailAge[i1], fr);
                float outerR = MathHelper.Lerp(trailLen[i0], trailLen[i1], fr);
                float p = MathHelper.Clamp(age / TRAIL_MAX_AGE, 0f, 1f);
                float fade = (1f - p) * (1f - p);
                Vector2 dir = rot.ToRotationVector2();
                Vector2 inner = center + dir * innerR;
                Vector2 outer = center + dir * outerR;
                Color cIn = ColorBlade * (0.45f * fade);
                Color cOut = Color.Lerp(ColorCyan, ColorGold, p * 0.8f) * (0.9f * fade);
                float u = i / (float)segs * 1.6f;
                verts[v] = new VertexPositionColorTexture(new Vector3(inner, 0f), cIn, new Vector2(u, 0f));
                verts[v + 1] = new VertexPositionColorTexture(new Vector3(outer, 0f), cOut, new Vector2(u, 1f));
                v += 2;
            }
            Main.spriteBatch.End();
            GraphicsDevice gd = Main.instance.GraphicsDevice;
            gd.BlendState = BlendState.Additive;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DepthStencilState = DepthStencilState.None;
            gd.SamplerStates[0] = SamplerState.LinearWrap;
            Matrix wvp = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)
                * Main.GameViewMatrix.TransformationMatrix
                * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value;
            BasicEffect be = GetBasicFallback(gd);
            be.World = Matrix.Identity;
            be.View = Matrix.Identity;
            be.Projection = wvp;
            be.Texture = noise;
            be.CurrentTechnique.Passes[0].Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, verts.Length - 2);
            RestartSpriteBatch();
            if (useShader)
                DrawRiftSlashQuad(player, fx, noise);
        }

        private void DrawRiftSlashQuad(Player player, Effect fx, Texture2D noise)
        {
            float swingT = MathHelper.Clamp(timer / (float)SwingTimeTotal, 0f, 1f);
            if (swingT <= ANTIC && timer <= SwingTimeTotal)
                return;
            float fade = timer <= SwingTimeTotal
                ? MathHelper.Clamp((swingT - ANTIC) / 0.12f, 0f, 1f)
                : MathHelper.Clamp(1f - (timer - SwingTimeTotal) / (float)LINGER, 0f, 1f);
            if (fade <= 0.03f)
                return;
            float maxLen = BLADE_LEN * VisualScale * (IsFinisher ? 1.85f : 1.55f);
            float quadHalf = maxLen * 1.25f;
            float quadScale = quadHalf * 2f / noise.Width;
            Vector2 lead = (timer <= SwingTimeTotal ? Projectile.rotation : trailRot[0]).ToRotationVector2();
            float span = Math.Min(Math.Abs(lastAngSpeed) * TRAIL_MAX_AGE, 2.9f);
            if (timer > SwingTimeTotal)
                span *= fade;
            if (span < 0.25f)
                return;
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uLead"]?.SetValue(new Vector3(lead.X, lead.Y, angDir));
            fx.Parameters["uSpan"]?.SetValue(span);
            fx.Parameters["uR0"]?.SetValue(BLADE_LEN * VisualScale * StretchK / quadHalf);
            fx.Parameters["uR1"]?.SetValue(BLADE_LEN * VisualScale * 0.72f / quadHalf);
            fx.Parameters["uInner"]?.SetValue(30f / quadHalf);
            fx.Parameters["uStretch"]?.SetValue(1f + (StretchK - 1f) * (0.85f / 0.38f));
            fx.Parameters["uFinisher"]?.SetValue(IsFinisher ? 1f : 0f);
            fx.Parameters["uFade"]?.SetValue(fade * 0.9f);
            fx.Parameters["uColorEdge"]?.SetValue(ColorGold.ToVector3());
            fx.Parameters["uColorMid"]?.SetValue(ColorCyan.ToVector3());
            fx.Parameters["uColorCore"]?.SetValue(ColorCore.ToVector3());
            fx.Parameters["uNoiseTexture"]?.SetValue(noise);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(noise, player.MountedCenter - Main.screenPosition, null, Color.White, 0f, noise.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void RestartSpriteBatch()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawBlade(Player player, Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameCount = 4;
            int frameHeight = tex.Height / frameCount;
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);

            Vector2 drawPos = player.MountedCenter - Main.screenPosition;
            bool flip = Math.Cos(aimAngle) < 0;
            SpriteEffects fxFlip = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = flip
                ? new Vector2(tex.Width - 6f, frameHeight - 6f)
                : new Vector2(6f, frameHeight - 6f);
            float rotOffset = flip ? MathHelper.Pi * 0.75f : MathHelper.PiOver4;
            float texDiag = (float)Math.Sqrt(tex.Width * tex.Width + frameHeight * frameHeight);
            float scale = BLADE_LEN * VisualScale / texDiag * 1.08f;
            float stretch = StretchK;
            float speedFactor = MathHelper.Clamp(Math.Abs(lastAngSpeed) / 0.3f, 0f, 1f);
            int ghosts = (int)(speedFactor * 5f);
            for (int i = Math.Min(ghosts, trailCount - 1); i >= 1; i--)
            {
                float a = (1f - i / 6f) * 0.3f * speedFactor;
                Color ghost = new Color(ColorCyan.R, ColorCyan.G, ColorCyan.B, 0) * a;
                Main.spriteBatch.Draw(tex, drawPos, sourceRect, ghost, trailRot[i] + rotOffset, origin, scale * MathHelper.Lerp(1f, stretch, 0.35f) * (1f - i * 0.015f), fxFlip, 0f);
            }
            if (speedFactor > 0.1f)
            {
                Color glow = new Color(ColorBlade.R, ColorBlade.G, ColorBlade.B, 0) * (0.45f * speedFactor);
                Main.spriteBatch.Draw(tex, drawPos, sourceRect, glow, Projectile.rotation + rotOffset, origin, scale * MathHelper.Lerp(1f, stretch, 0.35f) * 1.07f, fxFlip, 0f);
            }
            float metalScale = scale * MathHelper.Lerp(1f, stretch, 0.35f);
            Main.spriteBatch.Draw(tex, drawPos, sourceRect, lightColor, Projectile.rotation + rotOffset, origin, metalScale, fxFlip, 0f);
            if (stretch > 1.02f)
            {
                float over = (stretch - 1f) / 0.38f;
                for (int i = 1; i <= 4; i++)
                {
                    float k = i / 4f;
                    float ext = scale * MathHelper.Lerp(MathHelper.Lerp(1f, stretch, 0.35f), stretch, k);
                    Color c = Color.Lerp(ColorCyan, ColorCore, k);
                    Color energy = new Color(c.R, c.G, c.B, 0) * ((1f - k * 0.55f) * 0.5f * over);
                    Main.spriteBatch.Draw(tex, drawPos, sourceRect, energy, Projectile.rotation + rotOffset, origin, ext, fxFlip, 0f);
                }
            }
            if (speedFactor > 0.55f)
            {
                Color flash = new Color(ColorCore.R, ColorCore.G, ColorCore.B, 0)
                    * ((speedFactor - 0.55f) / 0.45f * 0.5f);
                Main.spriteBatch.Draw(tex, drawPos, sourceRect, flash, Projectile.rotation + rotOffset, origin, metalScale, fxFlip, 0f);
            }
        }
	}
}
