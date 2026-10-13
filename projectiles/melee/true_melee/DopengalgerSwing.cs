using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.core.particles;
using jarkos.particles.storm;
using Terraria.DataStructures;
namespace jarkos.projectiles.melee.true_melee
{
    public class DopengalgerSwing : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Doppelganger";
        private float SwingTime = 28;
        private const float FadeTime = 16;
        private const float SwingArc = MathHelper.Pi * 1.6f;
        private const float HoldRadius = 60f;
        private const float BladeLength = 70f;
        private const int MaxTrailPoints = 30;
        private struct TrailPoint
        {
            public Vector2 Tip;
            public Vector2 Base;
        }
        private readonly List<TrailPoint> trail = new();
        private Player Owner => Main.player[Projectile.owner];
        private float SwingDir => Projectile.ai[0] == 1f ? -1f : 1f;
        private ref float Timer => ref Projectile.ai[1];
        private static Asset<Effect> trailShader;
        private static readonly RasterizerState TrailRasterizer = new()
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };
        public override void Load()
        {
            if (!Main.dedServ)
                trailShader = ModContent.Request<Effect>(
                    "jarkos/Shaders/DopengalgerTrail",
                    AssetRequestMode.ImmediateLoad);
        }
        public override void Unload() => trailShader = null;
        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (int)SwingTime;
            Projectile.timeLeft = (int)SwingTime + (int)FadeTime;
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

            SwingTime = baseSwingTime / meleeSpeedMultiplier;
        }
        public override void AI()
        {
            Player player = Owner;

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (Timer == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f }, Projectile.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.velocity = (Main.MouseWorld - player.MountedCenter)
                        .SafeNormalize(Vector2.UnitX * player.direction);
                    Projectile.netUpdate = true;
                }
            }
            Timer++;

            if (Timer > SwingTime)
            {
                Projectile.friendly = false;
                for (int k = 0; k < 2 && trail.Count > 0; k++)
                    trail.RemoveAt(trail.Count - 1);
                Lighting.AddLight(Projectile.Center, 0.25f, 0.15f, 0.5f);
                return;
            }

            float progress = Timer / SwingTime;
            float eased = EaseSwing(progress);
            float baseAngle = Projectile.velocity.ToRotation();
            float startAngle = baseAngle - SwingArc * 0.5f * SwingDir * player.direction;
            float currentAngle = startAngle + SwingArc * eased * SwingDir * player.direction;

            Projectile.rotation = currentAngle;
            Projectile.Center = player.MountedCenter + currentAngle.ToRotationVector2() * HoldRadius;
            player.heldProj = Projectile.whoAmI;

            player.itemTime = 2;
            player.itemAnimation = 2;

            int dir = Math.Sign(Projectile.velocity.X);

            player.ChangeDir(dir == 0 ? player.direction : dir);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);
            Vector2 unit = currentAngle.ToRotationVector2();

            trail.Insert(0, new TrailPoint
            {
                Tip  = player.MountedCenter + unit * (HoldRadius + BladeLength),
                Base = player.MountedCenter + unit * (HoldRadius * 0.35f)
            });

            if (trail.Count > MaxTrailPoints)
                trail.RemoveAt(trail.Count - 1);

            float speedFactor = 1f - MathF.Abs(progress - 0.5f) * 2f;
            Vector2 tip = trail[0].Tip;

            if (speedFactor > 0.3f)
            {
                if (Main.rand.NextBool(2) && Main.netMode != NetmodeID.Server)
                {
                    new Fire_particle(
                        tip + Main.rand.NextVector2Circular(8f, 8f),
                        unit.RotatedBy(MathHelper.PiOver2 * SwingDir * player.direction) * 3f,
                        new Color(130, 80, 255),
                        lifetime: Main.rand.Next(10, 16),
                        scale: Main.rand.NextFloat(0.5f, 0.8f),
                        opacity: 0.8f,
                        rotationSpeed: Main.rand.NextFloat(-0.06f, 0.06f)
                    ).Spawn();
                }

                if (Main.rand.NextBool(5) && Main.netMode != NetmodeID.Server)
                {
                    new Spark_gravit(
                        tip,
                        Main.rand.NextVector2Circular(2f, 2f),
                        new Color(100, 180, 255),
                        lifetime: Main.rand.Next(12, 18),
                        scale: Main.rand.NextFloat(0.3f, 0.5f),
                        opacity: 0.9f
                    ).Spawn();
                }
            }
            Lighting.AddLight(Projectile.Center, 0.35f, 0.25f, 0.9f);
        }
        private static float EaseSwing(float t)
        {
            float s = t * t * (3f - 2f * t);
            return s * s * (3f - 2f * s);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer > SwingTime)
                return false;
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * (HoldRadius + 75f);
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 30f, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.5f }, target.Center);
            if (Main.netMode != NetmodeID.Server)
            {
                new SmallExplosionStarParticle(
                    target.Center,
                    Vector2.Zero,
                    new Color(160, 100, 255),
                    lifetime: 14,
                    scale: 0.9f,
                    opacity: 1f,
                    rotationSpeed: Main.rand.NextFloat(-0.06f, 0.06f)
                ).Spawn();
                for (int i = 0; i < 6; i++)
                {
                    new Spark(
                        target.Center,
                        Main.rand.NextVector2Circular(6f, 6f),
                        new Color(100, 160, 255),
                        lifetime: Main.rand.Next(8, 14),
                        scale: Main.rand.NextFloat(0.4f, 0.7f),
                        opacity: 0.9f
                    ).Spawn();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            if (Timer <= SwingTime)
                DrawBlade(lightColor);
            return false;
        }
        private static List<TrailPoint> Smooth(List<TrailPoint> src)
        {
            List<TrailPoint> result = new();
            for (int i = 0; i < src.Count - 1; i++)
            {
                TrailPoint p0 = src[Math.Max(i - 1, 0)];
                TrailPoint p1 = src[i];
                TrailPoint p2 = src[i + 1];
                TrailPoint p3 = src[Math.Min(i + 2, src.Count - 1)];
                result.Add(p1);
                result.Add(new TrailPoint
                {
                    Tip  = Vector2.CatmullRom(p0.Tip,  p1.Tip,  p2.Tip,  p3.Tip,  0.5f),
                    Base = Vector2.CatmullRom(p0.Base, p1.Base, p2.Base, p3.Base, 0.5f)
                });
            }
            result.Add(src[^1]);
            return result;
        }
        private void DrawTrail()
        {
            if (trail.Count < 3 || trailShader == null || !trailShader.IsLoaded)
                return;
            Effect effect = trailShader.Value;
            if (effect == null || effect.IsDisposed)
                return;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None, TrailRasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
            float globalFade = Timer > SwingTime ? 1f - (Timer - SwingTime) / FadeTime : 1f;
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            effect.Parameters["uWorldViewProjection"]?.SetValue(view * projection);
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            effect.Parameters["uOpacity"]?.SetValue(globalFade);
            effect.Parameters["uColorHot"]?.SetValue(new Vector3(0.55f, 0.40f, 1.00f));
            effect.Parameters["uColorMid"]?.SetValue(new Vector3(0.25f, 0.10f, 0.55f));
            effect.Parameters["uColorCold"]?.SetValue(new Vector3(0.05f, 0.01f, 0.14f));
            List<TrailPoint> pts = Smooth(trail);
            List<VertexPositionColorTexture> verts = new();
            Vector2 screen = Main.screenPosition;
            int count = pts.Count;
            for (int i = 0; i < count - 1; i++)
            {
                float t1 = i / (float)(count - 1);
                float t2 = (i + 1) / (float)(count - 1);
                float taper1 = MathHelper.Lerp(0f, 0.85f, t1 * t1);
                float taper2 = MathHelper.Lerp(0f, 0.85f, t2 * t2);
                Vector2 outer1 = pts[i].Tip;
                Vector2 outer2 = pts[i + 1].Tip;
                Vector2 inner1 = Vector2.Lerp(pts[i].Base, outer1, taper1);
                Vector2 inner2 = Vector2.Lerp(pts[i + 1].Base, outer2, taper2);
                Color c1 = Color.White * (1f - t1) * globalFade;
                Color c2 = Color.White * (1f - t2) * globalFade;
                verts.Add(V(inner1 - screen, c1, new Vector2(t1, 0f)));
                verts.Add(V(outer1 - screen, c1, new Vector2(t1, 1f)));
                verts.Add(V(inner2 - screen, c2, new Vector2(t2, 0f)));
                verts.Add(V(inner2 - screen, c2, new Vector2(t2, 0f)));
                verts.Add(V(outer1 - screen, c1, new Vector2(t1, 1f)));
                verts.Add(V(outer2 - screen, c2, new Vector2(t2, 1f)));
            }
            if (verts.Count >= 3)
            {
                GraphicsDevice gd = Main.instance.GraphicsDevice;
                gd.Textures[0] = TextureAssets.MagicPixel.Value;
                gd.RasterizerState = TrailRasterizer;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList,
                        verts.ToArray(), 0, verts.Count / 3);
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }
        private static VertexPositionColorTexture V(Vector2 pos, Color c, Vector2 uv)
            => new(new Vector3(pos, 0f), c, uv);
        private void DrawBlade(Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float rotation = Projectile.rotation + MathHelper.PiOver4;
            SpriteEffects fx = SpriteEffects.None;
            Vector2 origin = new(0, tex.Height);
            if (Owner.direction == -1)
            {
                fx = SpriteEffects.FlipHorizontally;
                rotation = Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2;
                origin = new Vector2(tex.Width, tex.Height);
            }
            Main.EntitySpriteDraw(tex, Owner.MountedCenter - Main.screenPosition,
                null, lightColor, rotation, origin, Projectile.scale * 1.15f, fx, 0);
        }
    }
}
