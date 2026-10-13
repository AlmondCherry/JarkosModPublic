using System;
using System.Collections.Generic;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;

namespace jarkos.projectiles.melee.unique_melee
{
    public class Galvanic_sword_swing : ModProjectile 
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Uranium_sword";
        public float SWING_TIME = 35;
        private float DELAY_TIME = 15;
        private const int LINGER = 14;
        private const float BLADE_LEN = 110f;
        private const float ANTIC = 0.18f;
        
        public static readonly Color ColorBlade = new(0, 255, 255);
        public static readonly Color ColorCyan = new(0, 255, 255);
        public static readonly Color ColorGold = new(255, 255, 0);
        public static readonly Color ColorCore = new(200, 255, 200);
        
        private static Asset<Effect> riftShader;
        private static bool shaderTried;
        private static BasicEffect basicFallback;
        
        private int ComboStep => (int)Projectile.ai[0];
        private float stretchK;
        
        private float SwingArc => 4.5f;
        private float VisualScale => 1.08f;
        
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
        
        
        private List<Vector2> energyNodes = new List<Vector2>();
        private List<Vector2> energyNodeVels = new List<Vector2>();

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 60;
            Projectile.height = 60;
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
            Player player = Main.player[Projectile.owner];

            float baseSwingTime = 36f;

            float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);

            if(player.HeldItem != null && !player.HeldItem.IsAir)
            {
                meleeSpeedMultiplier *= player.HeldItem.GetGlobalItem<CalibratedWeaponGlobalItem>().UseSpeedMultiplier(player.HeldItem, player);
            }

            if (meleeSpeedMultiplier < 0.01f) meleeSpeedMultiplier = 0.01f;

            SWING_TIME = baseSwingTime / meleeSpeedMultiplier;
            DELAY_TIME = baseSwingTime / meleeSpeedMultiplier;

            if (Projectile.owner == Main.myPlayer)
            {
                aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
                angDir = (int)Projectile.ai[1];
                if (Math.Cos(aimAngle) < 0) angDir = -angDir;
                Projectile.netUpdate = true;
            }

            prevRot = CurrentRotation();
            Projectile.rotation = prevRot;

            if (Main.netMode != NetmodeID.Server)
            {
                new Circular_glow(Main.player[Projectile.owner].MountedCenter, Vector2.Zero, ColorGold, 14, 0.16f, 0.6f).Spawn();
            }
        }

        private float CurrentRotation()
        {
            float t = MathHelper.Clamp(timer / SWING_TIME, 0f, 1f);
            return aimAngle + angDir * SwingArc * (MathSystem.BaseEasingSwing(t, ANTIC, 4.6f) - 0.5f);
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
            Projectile.timeLeft = 2 + LINGER + (int)DELAY_TIME;
            
            if (freezeFrames > 0 && timer <= SWING_TIME)
            {
                freezeFrames--;
                HoldPlayer(player, true);
                return;
            }
            
            AgeTrail();
            
            if (timer > SWING_TIME)
            {
                Projectile.Kill();
                return;
            }
            
            timer++;
            
            float rot = CurrentRotation();
            float angSpeed = MathHelper.WrapAngle(rot - prevRot);

            float t = MathSystem.GetProgressValue(timer, SWING_TIME);
            float stretch_k = MathSystem.BaseStretchSwing(t, ANTIC, timer, SWING_TIME);

            stretchK = stretch_k;

            prevRot = rot;
            Projectile.rotation = rot;
            lastAngSpeed = angSpeed;

            Vector2 tip1 = Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * VisualScale * stretch_k;

            if(t > ANTIC)
            {
                energyNodes.Add(tip1);
            }
            if(energyNodes.Count > 30)
            {
                energyNodes.RemoveAt(0);
            }
            
            bool swinging = timer <= SWING_TIME;
            HoldPlayer(player, swinging);
            
            if (!swinging)
                return;

            if (!strikeSoundPlayed && t >= ANTIC)
            {
                strikeSoundPlayed = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    float pitch = ComboStep == 0 ? -0.1f : 0.12f;
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.9f, Pitch = pitch }, Projectile.Center);
                }
            }
            
            if (Math.Abs(angSpeed) > 0.035f)
                PushTrail(rot, BLADE_LEN * VisualScale * stretch_k + 6f);
                
            if (!stretchFlashDone && stretch_k > 1.18f)
            {
                stretchFlashDone = true;
                if (Main.netMode != NetmodeID.Server)
                {
                    Vector2 tip = Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * VisualScale * stretch_k;
                    SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.5f, Pitch = 0.5f }, tip);

                    new StarParticle(tip, Vector2.Zero, ColorCore, 18, 0.2f, 1f, 0.06f).Spawn();
                    new Circleedge(tip, Vector2.Zero, ColorCyan, 14, 0.13f, 0.8f).Spawn();

                    Vector2 tangent2 = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * angDir);

                    for (int i = 0; i < 4; i++)
                        new Stretching_particle_out(tip, tangent2 * Main.rand.NextFloat(7f, 12f), i == 0 ? ColorCore : ColorCyan, 20, Main.rand.NextFloat(50f, 90f), Main.rand.NextFloat(4f, 6f), 0.02f).Spawn();
                }
            }
            
            if (Main.netMode != NetmodeID.Server)
                SpawnSwingParticles(rot, angSpeed);
            Lighting.AddLight(Projectile.Center + rot.ToRotationVector2() * BLADE_LEN * 0.6f, ColorCyan.ToVector3() * 0.55f);
        }

        private void HoldPlayer(Player player, bool swinging)
        {
            if (!swinging) return;
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
        }

        private void SpawnSwingParticles(float rot, float angSpeed)
        {
            float speedFactor = MathHelper.Clamp(Math.Abs(angSpeed) / 0.35f, 0f, 1f);
            if (speedFactor < 0.25f) return;
            
            Vector2 rotV = rot.ToRotationVector2();
            Vector2 tangent = rotV.RotatedBy(MathHelper.PiOver2 * angDir);
            
            if (Main.rand.NextFloat() < speedFactor)
            {
                float along = Main.rand.NextFloat(0.5f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretchK * along;
                new Spark(pos, tangent * Main.rand.NextFloat(3f, 7f) * speedFactor, Main.rand.NextBool(3) ? ColorCore : ColorCyan, 22, Main.rand.NextFloat(0.35f, 0.6f), 0.85f).Spawn();
            }
            if (Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.55f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretchK * along;
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, tangent * Main.rand.NextFloat(2f, 5f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }
            if (stretchK > 1.12f && Main.rand.NextBool(2))
            {
                float along = Main.rand.NextFloat(0.7f, 1f);
                Vector2 pos = Projectile.Center + rotV * BLADE_LEN * VisualScale * stretchK * along;
                new Line_glow(pos, -tangent * Main.rand.NextFloat(1f, 3f), ColorCyan, 14, Main.rand.NextFloat(0.4f, 0.7f), 0.65f, 0f).Spawn();
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
            for (int i = 0; i < trailCount; i++) trailAge[i] += 1f;
            while (trailCount > 0 && trailAge[trailCount - 1] > TRAIL_MAX_AGE) trailCount--;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (timer > SWING_TIME) return false;
            float t = timer / SWING_TIME;
            if (t < ANTIC || t > 0.95f) return false;
            
            float point = 0f;
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Vector2 start = Projectile.Center + dir * 18f;
            Vector2 end = Projectile.Center + dir * BLADE_LEN * VisualScale * stretchK;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 44f * VisualScale, ref point);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            freezeFrames = 3;
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(target.Center, (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 5f, 7f, 10, 900f, FullName));
                }
                
                Vector2 hitPos = target.Center;
                Vector2 dirOut = (hitPos - Projectile.Center).SafeNormalize(Vector2.UnitX);
                
                new Circleedge(hitPos, Vector2.Zero, ColorCyan, 18, 0.16f, 0.85f).Spawn();
                new StarParticle(hitPos, Vector2.Zero, ColorCore, 26, 0.14f, 1f, 0.05f).Spawn();
                
                for (int i = 0; i < 8; i++)
                {
                    Vector2 v = dirOut.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(4f, 9f);
                    new Spark(hitPos, v, Main.rand.NextBool(3) ? ColorCore : ColorCyan, 24, Main.rand.NextFloat(0.4f, 0.7f), 0.9f).Spawn();
                }

                for (int i = 0; i < 3; i++)
                    new Line_glow(hitPos, Vector2.Zero, ColorBlade, 16, Main.rand.NextFloat(0.5f, 0.8f), 0.7f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            DrawTrail(player);

            if (energyNodes.Count >= 2)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                Effect effect = HealShaders.Stream;

                if (effect != null && HealShaders.HealNoise != null)
                {
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                    float fade = 1f;

                    if (timer > SWING_TIME) fade = 0.5f + 0.5f * (float)Math.Sin(timer * 0.5f);
                    
                    effect.Parameters["uOpacity"]?.SetValue(fade);
                    effect.Parameters["uColor"]?.SetValue(ColorCyan.ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(ColorCyan.ToVector4());
                    
                    List<Vector2> drawNodes = new List<Vector2>();

                    if (energyNodes.Count >= 2)
                    {
                        for (int i = 0; i < energyNodes.Count - 1; i++)
                        {
                            Vector2 p0 = energyNodes[i] - Main.screenPosition;
                            Vector2 p1 = energyNodes[i + 1] - Main.screenPosition;
                            
                            Vector2 dir = p1 - p0;
                            float dist = dir.Length();
                            
                            drawNodes.Add(p0);
                            
                            if (dist > 5f)
                            {
                                dir /= dist;
                                Vector2 normal = new Vector2(-dir.Y, dir.X);
                                
                                float wave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 15f + i) * Math.Min(dist * 0.4f, 15f);
                                
                                Vector2 cp1 = p0 + dir * (dist * 0.33f) + normal * wave;
                                Vector2 cp2 = p1 - dir * (dist * 0.33f) - normal * wave;
                                
                                int pointsPerSegment = 4;
                                for (int j = 1; j < pointsPerSegment; j++)
                                {
                                    float t = j / (float)pointsPerSegment;
                                    float u = 1f - t;
                                    
                                    
                                    float uuu = u * u * u;
                                    float uut = 3f * u * u * t;
                                    float utt = 3f * u * t * t;
                                    float ttt = t * t * t;
                                    
                                    Vector2 pos = uuu * p0 + uut * cp1 + utt * cp2 + ttt * p1;
                                    drawNodes.Add(pos);
                                }
                            }
                        }
                        drawNodes.Add(energyNodes[energyNodes.Count - 1]);
                    }
                    
                    if (drawNodes.Count >= 2)
                    {
                        TrailRenderer.Draw(effect, drawNodes.ToArray(), t => 12f, t => ColorCyan, HealShaders.HealNoise);
                    }
                }
                
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            
            Texture2D bloom = ModContent.Request<Texture2D>("jarkos/textures/CircleGlow").Value;
            
            
                
                
                
            
            
            if (timer <= SWING_TIME)
                DrawBlade(player, lightColor);
                
            return false;
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
            if (trailCount < 2) return;

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

            Matrix wvp = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) * Main.GameViewMatrix.TransformationMatrix * Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
            Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value;
            BasicEffect be = GetBasicFallback(gd);
            
            be.World = Matrix.Identity;
            be.View = Matrix.Identity;
            be.Projection = wvp;
            be.Texture = noise;
            be.CurrentTechnique.Passes[0].Apply();
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, verts.Length - 2);

            RestartSpriteBatch();
            
            if (useShader) DrawRiftSlashQuad(player, fx, noise);
        }

        private void DrawRiftSlashQuad(Player player, Effect fx, Texture2D noise)
        {
            float swingT = MathHelper.Clamp(timer / SWING_TIME, 0f, 1f);
            if (swingT <= ANTIC && timer <= SWING_TIME) return;
            
            float fade = timer <= SWING_TIME ? MathHelper.Clamp((swingT - ANTIC) / 0.12f, 0f, 1f)
                : MathHelper.Clamp(1f - (timer - SWING_TIME) / LINGER, 0f, 1f);
            if (fade <= 0.03f) return;
            
            float maxLen = BLADE_LEN * VisualScale * 1.55f;
            float quadHalf = maxLen * 1.25f;
            float quadScale = quadHalf * 2f / noise.Width;
            Vector2 lead = (timer <= SWING_TIME ? Projectile.rotation : trailRot[0]).ToRotationVector2();
            
            float span = Math.Min(Math.Abs(lastAngSpeed) * TRAIL_MAX_AGE, 2.9f);
            if (timer > SWING_TIME) span *= fade;
            if (span < 0.25f) return;
            
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uLead"]?.SetValue(new Vector3(lead.X, lead.Y, angDir));
            fx.Parameters["uSpan"]?.SetValue(span);
            fx.Parameters["uR0"]?.SetValue(BLADE_LEN * VisualScale * stretchK / quadHalf);
            fx.Parameters["uR1"]?.SetValue(BLADE_LEN * VisualScale * 0.72f / quadHalf);
            fx.Parameters["uInner"]?.SetValue(30f / quadHalf);
            fx.Parameters["uStretch"]?.SetValue(1f + (stretchK - 1f) * (0.85f / 0.38f));
            fx.Parameters["uFinisher"]?.SetValue(0f);
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
            Vector2 drawPos = player.MountedCenter - Main.screenPosition;

            bool flip = Math.Cos(aimAngle) < 0;

            SpriteEffects fxFlip = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 origin = flip ? new Vector2(tex.Width - 6f, tex.Height - 6f)
                : new Vector2(6f, tex.Height - 6f);

            float rotOffset = flip ? MathHelper.Pi * 0.75f : MathHelper.PiOver4;
            float texDiag = (float)Math.Sqrt(tex.Width * tex.Width + tex.Height * tex.Height);
            float scale = BLADE_LEN * VisualScale / texDiag * 1.08f;
            float stretch = stretchK;

            float speedFactor = MathHelper.Clamp(Math.Abs(lastAngSpeed) / 0.3f, 0f, 1f);
            int ghosts = (int)(speedFactor * 5f);

            for (int i = Math.Min(ghosts, trailCount - 1); i >= 1; i--)
            {
                float a = (1f - i / 6f) * 0.3f * speedFactor;
                Color ghost = new Color(ColorCyan.R, ColorCyan.G, ColorCyan.B, 0) * a;
                Main.spriteBatch.Draw(tex, drawPos, null, ghost, trailRot[i] + rotOffset, origin, scale * MathHelper.Lerp(1f, stretch, 0.35f) * (1f - i * 0.015f), fxFlip, 0f);
            }

            if (speedFactor > 0.1f)
            {
                Color glow = new Color(ColorBlade.R, ColorBlade.G, ColorBlade.B, 0) * (0.45f * speedFactor);
                Main.spriteBatch.Draw(tex, drawPos, null, glow, Projectile.rotation + rotOffset, origin, scale * MathHelper.Lerp(1f, stretch, 0.35f) * 1.07f, fxFlip, 0f);
            }

            float metalScale = scale * MathHelper.Lerp(1f, stretch, 0.35f);
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation + rotOffset, origin, metalScale, fxFlip, 0f);

            if (stretch > 1.02f)
            {
                float over = (stretch - 1f) / 0.38f;
                for (int i = 1; i <= 4; i++)
                {
                    float k = i / 4f;
                    float ext = scale * MathHelper.Lerp(MathHelper.Lerp(1f, stretch, 0.35f), stretch, k);

                    Color c = Color.Lerp(ColorCyan, ColorCore, k);
                    Color energy = new Color(c.R, c.G, c.B, 0) * ((1f - k * 0.55f) * 0.5f * over);
                    Main.spriteBatch.Draw(tex, drawPos, null, energy, Projectile.rotation + rotOffset, origin, ext, fxFlip, 0f);
                }
            }
            if (speedFactor > 0.55f)
            {
                Color flash = new Color(ColorCore.R, ColorCore.G, ColorCore.B, 0) * ((speedFactor - 0.55f) / 0.45f * 0.5f);
                Main.spriteBatch.Draw(tex, drawPos, null, flash, Projectile.rotation + rotOffset, origin, metalScale, fxFlip, 0f);
            }
        }
    }
    
    public class GalvanicNodeExplosion : ModProjectile
    {
        public override string Texture => "jarkos/textures/CircleGlow"; 
        
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 210;
            Projectile.height = 210;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2; 
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; 
            Projectile.alpha = 255; 
        }
    }
}
