using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.particles.storm;
namespace jarkos.projectiles.melee.true_melee
{
    public class FireSwordSwing : ModProjectile
    {
        private Player Owner => Main.player[Projectile.owner];
        private ref float ComboLeft => ref Projectile.ai[1];
        private bool IsCombo => ComboLeft > 0f;
        private bool IsFinisher => Projectile.ai[0] == 1f;
        private int SwingTime => IsFinisher ? 14 : (IsCombo ? 9 : 16);
        private const int FadeTime = 12;
        private const int TrailPointLife = 13;
        private const float SpriteDiagonal = 74.4f;
        private float FinisherScale => IsFinisher ? 1.3f : 1f;
        private float BladeLength => SpriteDiagonal * FinisherScale;
        private float startAngle;
        private float endAngle;
        private bool initialized;
        private bool boltFired;
        private bool comboSpawned;
        private int dir = 1;
        private bool swingDown;
        private float lastTrailRot = float.NaN;
        private readonly List<float> trailRot = new();
        private readonly List<float> trailAge = new();
        private static readonly VertexStrip vertexStrip = new();
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(startAngle);
            writer.Write(endAngle);
            writer.Write((sbyte)dir);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            startAngle = reader.ReadSingle();
            endAngle = reader.ReadSingle();
            dir = reader.ReadSByte();
            initialized = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft = SwingTime + FadeTime;
            Projectile.scale = FinisherScale;
            if (Projectile.owner == Main.myPlayer)
            {
                InitAngles();
                Projectile.netUpdate = true;
            }
        }
        private void InitAngles()
        {
            Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            dir = aim.X >= 0f ? 1 : -1;
            float aimAngle = aim.ToRotation();
            swingDown = (ComboLeft % 2 == 0);
            float arc = IsFinisher
                ? MathHelper.ToRadians(200f + (Projectile.whoAmI * 7 % 30))
                : MathHelper.ToRadians(140f + (Projectile.whoAmI * 11 % 30));
            float offset = (Projectile.whoAmI * 13 % 30 - 15) / 100f;
            float half = arc * 0.5f;
            if (swingDown)
            {
                startAngle = aimAngle - dir * half + offset;
                endAngle   = aimAngle + dir * half + offset;
            }
            else
            {
                startAngle = aimAngle + dir * half + offset;
                endAngle   = aimAngle - dir * half + offset;
            }
            initialized = true;
        }
        private static float EaseSwing(float p)
        {
            float x = 1f - p;
            return 1f - x * x * x * x;
        }
        private float SwingProgress =>
            MathHelper.Clamp((SwingTime + FadeTime - Projectile.timeLeft) / (float)SwingTime, 0f, 1f);
        private bool SwingDone => Projectile.timeLeft <= FadeTime;
        public override void AI()
        {
            if (!initialized)
                return;
            if (Owner.dead || !Owner.active)
            {
                Projectile.Kill();
                return;
            }
            for (int i = 0; i < trailAge.Count; i++)
                trailAge[i]++;
            while (trailAge.Count > 0 && trailAge[^1] >= TrailPointLife)
            {
                trailAge.RemoveAt(trailAge.Count - 1);
                trailRot.RemoveAt(trailRot.Count - 1);
            }
            float progress = SwingProgress;
            float rotation = MathHelper.Lerp(startAngle, endAngle, EaseSwing(progress));
            Projectile.rotation = rotation;
            Projectile.Center = Owner.MountedCenter + rotation.ToRotationVector2() * (BladeLength * 0.5f);
            if (SwingDone)
            {
                Projectile.friendly = false;
                if (IsCombo && !comboSpawned && Main.myPlayer == Projectile.owner && !Owner.dead)
                {
                    comboSpawned = true;
                    Vector2 aim = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
                    float next = ComboLeft - 1;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, aim,
                        Type, Projectile.damage, Projectile.knockBack, Projectile.owner,
                        ai0: next == 0f ? 1f : 0f, ai1: next);
                }
                return;
            }
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Owner.direction = dir;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                rotation - MathHelper.PiOver2);
            if (float.IsNaN(lastTrailRot))
                lastTrailRot = rotation;
            float delta = rotation - lastTrailRot;
            int steps = Math.Max(1, (int)(Math.Abs(delta) / 0.07f));
            for (int s = 1; s <= steps; s++)
            {
                trailRot.Insert(0, lastTrailRot + delta * s / steps);
                trailAge.Insert(0, 0f);
            }
            lastTrailRot = rotation;
            if (Projectile.timeLeft == SwingTime + FadeTime - 1 && Main.netMode != NetmodeID.Server)
            {
                if (IsFinisher)
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.4f, Volume = 1.2f }, Projectile.Center);
                else
                    SoundEngine.PlaySound(IsCombo
                        ? SoundID.Item74 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f) }
                        : SoundID.Item71, Projectile.Center);
            }
            SpawnBladeDust(rotation, progress);
            if (IsCombo && !boltFired && progress >= 0.45f && Main.myPlayer == Projectile.owner)
            {
                boltFired = true;
                Vector2 shootDir = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX * dir);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    Owner.MountedCenter + shootDir * BladeLength * 0.7f,
                    shootDir * 11f,
                    ModContent.ProjectileType<FireSwordBolt>(),
                    (int)(Projectile.damage * 0.75f), Projectile.knockBack * 0.5f, Projectile.owner,
                    ai0: Main.rand.NextFloat(MathHelper.TwoPi));
                if (Main.netMode != NetmodeID.Server)
                    SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
            }
            if (IsFinisher && !boltFired && progress >= 0.5f && Main.myPlayer == Projectile.owner)
            {
                boltFired = true;
                Vector2 shootDir = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX * dir);
                for (int i = -1; i <= 1; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                        Owner.MountedCenter + shootDir * BladeLength * 0.7f,
                        shootDir.RotatedBy(i * 0.22f) * 12.5f,
                        ModContent.ProjectileType<FireSwordBolt>(),
                        (int)(Projectile.damage * 0.85f), Projectile.knockBack * 0.6f, Projectile.owner,
                        ai0: Main.rand.NextFloat(MathHelper.TwoPi));
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f }, Projectile.Center);
                    if (Main.LocalPlayer.whoAmI == Projectile.owner)
                    {
                        Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                            Projectile.Center, shootDir, 5f, 7f, 12, 900f));
                    }
                }
            }
            Lighting.AddLight(Projectile.Center, 1.5f, 0.75f, 0.15f);
        }
        private void SpawnBladeDust(float rotation, float progress)
        {
            float intensity = MathF.Sin(MathHelper.Clamp(progress, 0f, 1f) * MathHelper.Pi);
            int count = (int)(intensity * (IsFinisher ? 3 : (IsCombo ? 2 : 1)));
            float rotSign = MathF.Sign(endAngle - startAngle);
            Vector2 tangent = rotation.ToRotationVector2().RotatedBy(rotSign * MathHelper.PiOver2);
            for (int i = 0; i < count; i++)
            {
                float along = Main.rand.NextFloat(0.5f, 1f);
                Vector2 pos = Owner.MountedCenter + rotation.ToRotationVector2() * BladeLength * along;
                float speed = Main.rand.NextFloat(3f, 6f) * along;
                if (Main.netMode != NetmodeID.Server)
                {
                    new Fire_particle(
                        pos,
                        tangent * speed,
                        new Color(255, 140, 10),
                        lifetime: Main.rand.Next(6, 10),
                        scale: Main.rand.NextFloat(0.4f, 0.7f),
                        opacity: 0.8f,
                        rotationSpeed: Main.rand.NextFloat(-0.06f, 0.06f)
                    ).Spawn();
                }
                if (Main.rand.NextBool(5))
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        new Spark_gravit(
                            pos,
                            tangent * speed * 0.8f + Main.rand.NextVector2Circular(1f, 1f),
                            new Color(255, 140, 10),
                            lifetime: Main.rand.Next(8, 12),
                            scale: Main.rand.NextFloat(0.3f, 0.5f),
                            opacity: 0.9f
                        ).Spawn();
                    }
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (IsCombo && !comboSpawned && Main.myPlayer == Projectile.owner && !Owner.dead)
            {
                Vector2 aim = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
                float next = ComboLeft - 1;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, aim,
                    Type, Projectile.damage, Projectile.knockBack, Projectile.owner,
                    ai0: next == 0f ? 1f : 0f, ai1: next);
                comboSpawned = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (SwingDone)
                return false;
            float _ = 0f;
            Vector2 tip = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * BladeLength;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Owner.MountedCenter, tip, 26f, ref _);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsFinisher)
                modifiers.SourceDamage *= 1.5f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, IsCombo || IsFinisher ? 300 : 180);
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.5f, Pitch = 0.3f }, target.Center);
                int count = IsFinisher ? 8 : 4;
                for (int i = 0; i < count; i++)
                {
                    new Spark(
                        target.Center,
                        Main.rand.NextVector2Circular(5f, 5f),
                        new Color(255, 180, 20),
                        lifetime: Main.rand.Next(6, 11),
                        scale: Main.rand.NextFloat(0.4f, 0.7f),
                        opacity: 1f
                    ).Spawn();
                }
                if (IsFinisher && Main.LocalPlayer.whoAmI == Projectile.owner)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                        target.Center, Main.rand.NextVector2Unit(), 4f, 6f, 10, 700f));
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawFlameTrail();
            if (!SwingDone)
            {
                DrawBlade(lightColor);
                DrawTipGlow();
            }
            return false;
        }
        private void DrawFlameTrail()
        {
            int count = trailRot.Count;
            if (count < 2)
                return;
            var positions = new Vector2[count];
            var rotations = new float[count];
            float rotSign = MathF.Sign(endAngle - startAngle);
            Vector2 center = Owner.MountedCenter;
            for (int i = 0; i < count; i++)
            {
                positions[i] = center + trailRot[i].ToRotationVector2() * (BladeLength * 0.8f);
                rotations[i] = trailRot[i] + rotSign * MathHelper.PiOver2;
            }
            MiscShaderData shader = GameShaders.Misc["FlameLash"];
            shader.UseSaturation(-2.5f);
            shader.UseOpacity(IsFinisher ? 6f : 4.5f);
            shader.Apply();
            vertexStrip.PrepareStripWithProceduralPadding(
                positions, rotations, StripColor, StripWidth,
                -Main.screenPosition, includeBacksides: true);
            vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        private Color StripColor(float p)
        {
            int i = (int)(p * (trailRot.Count - 1));
            float ageFade = 1f - MathHelper.Clamp(trailAge[i] / TrailPointLife, 0f, 1f);
            Color c = p < 0.3f
                ? Color.Lerp(new Color(255, 245, 180), new Color(255, 150, 30), p / 0.3f)
                : Color.Lerp(new Color(255, 150, 30), new Color(180, 30, 0), (p - 0.3f) / 0.7f);
            return c * (ageFade * ageFade) * (1f - p * 0.6f);
        }
        private float StripWidth(float p)
        {
            float baseW = BladeLength * (IsFinisher ? 0.85f : 0.7f);
            return baseW * (1f - p * p);
        }
        private void DrawTipGlow()
        {
            Texture2D glow = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 tip = Owner.MountedCenter + Projectile.rotation.ToRotationVector2() * BladeLength
                - Main.screenPosition;
            float intensity = MathF.Sin(SwingProgress * MathHelper.Pi);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(glow, tip, null, new Color(255, 180, 60, 0) * (0.8f * intensity),
                Projectile.rotation, glow.Size() * 0.5f, 0.55f * FinisherScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, tip, null, new Color(255, 255, 200, 0) * (0.5f * intensity),
                Projectile.rotation, glow.Size() * 0.5f, 0.3f * FinisherScale, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawBlade(Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;
            float drawRot;
            Vector2 origin;
            SpriteEffects fx;
            if (dir >= 0)
            {
                drawRot = Projectile.rotation + MathHelper.PiOver4;
                origin = new Vector2(0f, tex.Height);
                fx = SpriteEffects.None;
            }
            else
            {
                drawRot = Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2;
                origin = new Vector2(tex.Width, tex.Height);
                fx = SpriteEffects.FlipHorizontally;
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            float pulse = 0.75f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 12f);
            for (int i = 0; i < 3; i++)
            {
                Vector2 off = (Main.GlobalTimeWrappedHourly * 8f + i * MathHelper.TwoPi / 3f)
                    .ToRotationVector2() * 3f;
                Main.spriteBatch.Draw(tex, drawPos + off, null,
                    new Color(255, 110, 20, 0) * 0.35f * pulse, drawRot, origin,
                    Projectile.scale * 1.05f, fx, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Color col = Color.Lerp(lightColor, Color.OrangeRed, 0.35f);
            Main.EntitySpriteDraw(tex, drawPos, null, col, drawRot, origin, Projectile.scale, fx);
        }
    }
}
