using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class CollapsarSwing : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/melee/true_melee/Collapsar";

        private const int Duration = 32;
        private const float SwingArc = 5.2f;
        private const float BladeLength = 132f;
        private const float Scale = 1.45f;

        private const float SlowStart = 0.46f;
        private const float SlowEnd = 0.52f;
        private const float SlowPoint = 0.55f;

        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];
        private float SwingDir => Projectile.ai[1];

        private bool _lightningFired;

        private readonly List<float> _rotCache = new();
        private const int RotCacheMax = 26;

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.netImportant = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.rotation);
            writer.Write(_lightningFired);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.rotation = reader.ReadSingle();
            _lightningFired = reader.ReadBoolean();
        }

        private float SwingProgress(float t)
        {
            if (t < 0.12f)
            {
                float x = t / 0.12f;
                return -0.07f * MathF.Sin(x * MathHelper.PiOver2);
            }

            if (t < SlowStart)
            {
                float x = (t - 0.12f) / (SlowStart - 0.12f);
                float ease = x * x * (3f - 2f * x);
                return -0.07f + ease * (SlowPoint + 0.07f);
            }

            if (t < SlowEnd)
            {
                float x = (t - SlowStart) / (SlowEnd - SlowStart);
                return SlowPoint + x * 0.015f;
            }

            if (t < 0.85f)
            {
                float x = (t - SlowEnd) / (0.85f - SlowEnd);
                float ease = 1f - MathF.Pow(1f - x, 3.2f);
                return (SlowPoint + 0.015f) + ease * (1.05f - SlowPoint - 0.015f);
            }

            float y = (t - 0.85f) / 0.15f;

            return 1.05f - 0.05f * (y * y * (3f - 2f * y));
        }

        private float BladeRotation(float progress)
        {
            float aim = Projectile.velocity.ToRotation();
            float start = aim - SwingArc * 0.5f * SwingDir;
            return start + SwingArc * progress * SwingDir;
        }

        public override void AI()
        {
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            Projectile.Center = Owner.MountedCenter;
            Owner.ChangeDir(Projectile.velocity.X >= 0 ? 1 : -1);

            float t = Timer / Duration;
            float progress = SwingProgress(t);
            float rot = BladeRotation(progress);
            Projectile.rotation = rot;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                rot - MathHelper.PiOver2);

            if (_rotCache.Count == 0 || MathF.Abs(_rotCache[0] - rot) > 0.02f)
            {
                _rotCache.Insert(0, rot);
                if (_rotCache.Count > RotCacheMax)
                    _rotCache.RemoveAt(_rotCache.Count - 1);
            }

            Vector2 tip = Owner.MountedCenter + rot.ToRotationVector2() * (BladeLength * Scale);

            if (t >= SlowStart && !_lightningFired)
            {
                _lightningFired = true;
                FireLightning(tip);
            }

            if (t >= SlowEnd && t < 0.85f && Main.rand.NextBool(2))
            {
                Vector2 onBlade = Owner.MountedCenter +
                    rot.ToRotationVector2() * Main.rand.NextFloat(40f, BladeLength * Scale);
                Dust d = Dust.NewDustPerfect(onBlade, DustID.Vortex,
                    rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * SwingDir) * 5f,
                    0, default, 1.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(tip, 0.25f, 0.7f, 1f);

            if (Timer == 0)
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CollapsarSlash") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);

            Timer++;
            if (Timer >= Duration)
                Projectile.Kill();
        }

        private void FireLightning(Vector2 tip)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CollapsarSmallLightning") with { Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, tip);

            if (Main.myPlayer != Projectile.owner)
                return;

            var targets = new List<NPC>();
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy(Projectile) &&
                    Vector2.Distance(npc.Center, tip) < 480f)
                    targets.Add(npc);
            }
            targets.Sort((a, b) =>
                Vector2.DistanceSquared(a.Center, tip).CompareTo(Vector2.DistanceSquared(b.Center, tip)));

            int count = Math.Min(4, targets.Count);

            for (int i = 0; i < count; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip,
                    Vector2.Zero, ModContent.ProjectileType<CollapsarLightning>(),
                    (int)(Projectile.damage * 0.75f), 2f, Projectile.owner,
                    targets[i].whoAmI, Main.rand.Next(100000));
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (BladeLength * Scale);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(), start, end, 30f, ref _);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Timer / (float)Duration >= SlowEnd)
                modifiers.SourceDamage *= 1.35f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElectroStun>(), 25);
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Electric,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawBlade(lightColor);
            return false;
        }

        private void DrawBlade(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            float drawRot = Projectile.rotation + MathHelper.PiOver4;
            Vector2 origin = new(0f, tex.Height);

            SpriteEffects fx = SpriteEffects.None;
            if (SwingDir * Owner.direction < 0)
            {
                fx = SpriteEffects.FlipHorizontally;
                drawRot = Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2;
                origin = new Vector2(tex.Width, tex.Height);
            }

            float t = Timer / (float)Duration;
            float stretch = 1f;
            if (t >= SlowEnd && t < 0.85f)
                stretch = 1f + 0.08f * MathF.Sin((t - SlowEnd) / (0.85f - SlowEnd) * MathHelper.Pi);

            Main.EntitySpriteDraw(tex, Owner.MountedCenter - Main.screenPosition,
                null, lightColor, drawRot, origin, Scale * stretch, fx, 0);
        }

        private void DrawTrail()
        {
            if (_rotCache.Count < 3)
                return;

            Effect shader = ModContent.Request<Effect>(
                "Jarkos/Shaders/CollapsarTrail",
                AssetRequestMode.ImmediateLoad).Value;

            float t = Timer / (float)Duration;
            float intensity = MathHelper.Clamp(1.2f - MathF.Abs(t - 0.68f) * 2f, 0.35f, 1f);

            shader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            shader.Parameters["uColor"]?.SetValue(new Vector3(0.30f, 0.95f, 1.00f));
            shader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.45f, 0.10f, 1.00f));
            shader.Parameters["uOpacity"]?.SetValue(intensity);

            Vector2 center = Owner.MountedCenter - Main.screenPosition;
            float outerR = BladeLength * Scale;
            float innerR = outerR * 0.35f;

            var verts = new VertexPositionColorTexture[_rotCache.Count * 2];
            for (int i = 0; i < _rotCache.Count; i++)
            {
                float f = i / (float)(_rotCache.Count - 1);
                Vector2 dir = _rotCache[i].ToRotationVector2();

                float taper = 1f - f * 0.35f;
                Vector2 outer = center + dir * outerR * taper;
                Vector2 inner = center + dir * innerR;
                Color c = Color.White * (1f - f);
                verts[i * 2] = new VertexPositionColorTexture(new Vector3(outer, 0f), c, new Vector2(f, 0f));
                verts[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(inner, 0f), c, new Vector2(f, 1f));
            }

            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            Matrix proj = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            shader.Parameters["uWorldViewProjection"]?.SetValue(view * proj);

            Main.spriteBatch.End();

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            gd.BlendState = BlendState.Additive;
            gd.RasterizerState = RasterizerState.CullNone;
            gd.DepthStencilState = DepthStencilState.None;

            foreach (EffectPass pass in shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, verts.Length - 2);
            }

            gd.BlendState = BlendState.AlphaBlend;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
