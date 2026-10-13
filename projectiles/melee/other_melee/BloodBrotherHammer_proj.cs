using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.weapons.melee.other_melee;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using jarkos.particles;
using ReLogic.Content;
using Terraria.DataStructures;
using jarkos.buffs.buff;
using Terraria.Graphics.CameraModifiers;
namespace jarkos.projectiles.melee.other_melee
{
    public class BloodyHammer_proj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/other_melee/BloodBrotherHammer";
        private const int BaseSwingTime = 56;
        private const int FollowThroughFrames = 7;
        private const float WindupEnd = 0.54f;
        private const float SnapEnd   = 0.82f;
        private const float HandleOffset = -69f;
        private const float HammerLength = 190f;
        private const float CollisionWidth = 44f;
        private const int MaxTrailSamples = 38;
        private const float TrailDecayPerFrame = 1f / 11f;
        private int SwingDir => Projectile.ai[1] >= 0f ? 1 : -1;
        private float AimAngle
        {
            get => Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        private float Timer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private float _angle;
        private float _scaleMul = 1f;
        private float _outDist  = 1f;
        private float _alpha    = 1f;
        private float _charge   = 0f;
        private bool  _snapTriggered;
        private readonly List<TrailSample> _trail = new(MaxTrailSamples);
        private Vector2 _lastTipOffset;
        private bool _hasLastTip;
        private struct TrailSample
        {
            public Vector2 Tip;
            public float Time;
            public float Speed;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.scale = 1.4f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = BaseSwingTime + FollowThroughFrames + 42;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.velocity.LengthSquared() > 0.0001f)
                AimAngle = Projectile.velocity.ToRotation();
            else
                AimAngle = (Main.MouseWorld - Main.player[Projectile.owner].MountedCenter).ToRotation();
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.45f, Volume = 0.65f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.55f, Volume = 0.6f }, Projectile.position);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player == null || !player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            float swingTime = BaseSwingTime;
            bool animationDone = Timer >= swingTime + FollowThroughFrames;
            if (!animationDone)
            {
                float t = MathHelper.Clamp(Timer / swingTime, 0f, 1f);
                float life = (Timer + FollowThroughFrames) / (swingTime + FollowThroughFrames);
                ComputePose(t, out _angle, out _scaleMul, out _outDist);
                _alpha = MathHelper.Clamp(1.4f - life * 1.1f, 0f, 1f);
                _charge = ComputeCharge(t);
                Vector2 root = GetHammerRoot(player);
                float length = HammerLength * _scaleMul * _outDist;
                Vector2 head = root + _angle.ToRotationVector2() * length;
                Projectile.Center = Vector2.Lerp(root, head, 0.55f);
                Projectile.rotation = _angle;
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                int desiredDir = MathF.Cos(AimAngle) >= 0f ? 1 : -1;
                player.ChangeDir(desiredDir);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _angle - MathHelper.PiOver2);
                if (t < WindupEnd && Main.rand.NextFloat() < _charge * 0.55f)
                    EmitChargeDust(head);
                if (t > WindupEnd && t < 0.92f)
                    EmitTrailDust(root, head);
                if (!_snapTriggered && t >= WindupEnd)
                {
                    _snapTriggered = true;
                    TriggerSnapBurst(root, head);
                }
                float lightK = 0.30f + _charge * 0.40f;
                Lighting.AddLight(head, lightK * 1.8f, lightK * 0.10f, lightK * 0.15f);
                Lighting.AddLight(Projectile.Center, lightK * 0.7f, 0.03f, 0.05f);
                DecayTrail();
                RecordTrailSample(head, player.MountedCenter, t);
                Timer += 1f;
            }
            else
            {
                _alpha = 0f;
                DecayTrail();
                if (_trail.Count == 0)
                    Projectile.Kill();
            }
        }
        private void ComputePose(float t, out float angle, out float scaleMul, out float outDist)
        {
            float eased = EaseSwing(t);
            const float halfArc = 1.55f;
            angle = MathHelper.Lerp(AimAngle - halfArc * SwingDir, AimAngle + halfArc * SwingDir, eased);
            if (t < 0.18f)
            {
                float windK = 1f - (t / 0.18f);
                angle -= 0.18f * SwingDir * windK;
            }
            float snapMid = (WindupEnd + SnapEnd) * 0.5f;
            float pulse = SnapPulse(t, snapMid, 0.18f);
            outDist  = 0.95f + pulse * 0.22f;
            scaleMul = 0.95f + pulse * 0.30f;
        }
        private Vector2 GetHammerRoot(Player player)
        {
            return player.MountedCenter + _angle.ToRotationVector2() * HandleOffset;
        }
        private void EmitTrailDust(Vector2 root, Vector2 head)
        {
            if (!Main.rand.NextBool(2))
                return;
            Vector2 pos = Vector2.Lerp(root, head, Main.rand.NextFloat(0.55f, 1.05f));
            Vector2 perp = (_angle + MathHelper.PiOver2 * Main.rand.NextFloatDirection()).ToRotationVector2();
            Vector2 vel = perp * Main.rand.NextFloat(0.5f, 2.6f);
            Dust dust = Dust.NewDustPerfect(pos, DustID.Blood, vel, 80, default, Main.rand.NextFloat(1.0f, 1.7f));
            dust.noGravity = Main.rand.NextBool(2);
        }
        private void EmitChargeDust(Vector2 head)
        {
            Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
            Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.4f, 1.4f));
            Dust dust = Dust.NewDustPerfect(head + offset, DustID.Blood, vel, 40, default, Main.rand.NextFloat(1.0f, 1.6f));
            dust.noGravity = false;
        }
        private void TriggerSnapBurst(Vector2 root, Vector2 head)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath19 with { Pitch = -0.6f, Volume = 0.65f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.20f, Volume = 0.55f }, Projectile.Center);
            for (int i = 0; i < 22; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(head, DustID.Blood, dv, 60, default, Main.rand.NextFloat(1.3f, 2.0f));
                dust.noGravity = Main.rand.NextBool(2);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(head, DustID.LifeDrain, dv, 100, default, 1.3f);
                dust.noGravity = true;
            }
        }
        private static float ComputeCharge(float t)
        {
            if (t < WindupEnd)
            {
                float u = t / WindupEnd;
                return MathF.Pow(u, 0.7f);
            }
            else if (t < SnapEnd)
            {
                float u = (t - WindupEnd) / (SnapEnd - WindupEnd);
                return MathF.Max(0f, 1f - u);
            }
            return 0f;
        }
        private void DecayTrail()
        {
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                TrailSample s = _trail[i];
                s.Time -= TrailDecayPerFrame;
                if (s.Time <= 0f)
                    _trail.RemoveAt(i);
                else
                    _trail[i] = s;
            }
        }
        private void RecordTrailSample(Vector2 tipWorld, Vector2 anchor, float t)
        {
            Vector2 tipOffset = tipWorld - anchor;
            if (t < 0.06f || t > 1f)
            {
                _hasLastTip = true;
                _lastTipOffset = tipOffset;
                return;
            }
            float speed = _hasLastTip ? Vector2.Distance(tipOffset, _lastTipOffset) : 0f;
            _trail.Insert(0, new TrailSample { Tip = tipOffset, Time = 1f, Speed = speed });
            if (_trail.Count > MaxTrailSamples)
                _trail.RemoveAt(_trail.Count - 1);
            _hasLastTip = true;
            _lastTipOffset = tipOffset;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float t = MathHelper.Clamp(Timer / BaseSwingTime, 0f, 1f);
            if (t < WindupEnd || t > 0.95f)
                return false;
            Player player = Main.player[Projectile.owner];
            Vector2 start = GetHammerRoot(player);
            float length = HammerLength * _scaleMul * _outDist;
            Vector2 end = start + _angle.ToRotationVector2() * length;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint) ? true : null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            for (int i = 0; i < 22; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(7f, 7f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Blood, dv, 60, default, Main.rand.NextFloat(1.2f, 1.9f));
                dust.noGravity = Main.rand.NextBool(2);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.LifeDrain, dv, 100, default, 1.2f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath19 with { Pitch = -0.3f, Volume = 0.85f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.55f }, target.Center);
            Vector2 impactPos = Projectile.Center + new Vector2(0f, 36f);
            if (Projectile.owner == Main.myPlayer)
            {
                var modifier = new PunchCameraModifier(impactPos, Vector2.UnitY, 9.0f, 7.0f, 30, 2000f, "StormAnchorPlantImpact");
                Main.instance.CameraModifiers.Add(modifier);
            }
            player.AddBuff(ModContent.BuffType<BloodBrotherHammer_buff>(), 180);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (_alpha <= 0.01f && _trail.Count == 0)
                return false;
            DrawTipRibbon();
            if (_alpha <= 0.01f)
                return false;
            DrawHammer(lightColor);
            return false;
        }
        private void DrawTipRibbon()
        {
            if (_trail.Count < 2)
                return;
            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Rectangle pixelRect = new Rectangle(0, 0, 1, 1);
                Vector2 pixelOrigin = new Vector2(0f, 0.5f);
                Color innerColor = new Color(255, 180, 180);
                Color outerColor = new Color(140, 10, 20);
                const float baseWidth = 2f;
                const float maxWidth  = 11f;
                Vector2 anchorWorld = Main.player[Projectile.owner].MountedCenter;
                for (int i = 0; i < _trail.Count - 1; i++)
                {
                    TrailSample a = _trail[i];
                    TrailSample b = _trail[i + 1];
                    Vector2 from = anchorWorld + a.Tip;
                    Vector2 to   = anchorWorld + b.Tip;
                    Vector2 delta = to - from;
                    float length = delta.Length();
                    if (length < 0.01f)
                        continue;
                    float life = MathHelper.Clamp(a.Time, 0f, 1f);
                    float speedFactor = MathHelper.Clamp(a.Speed / 16f, 0f, 1f);
                    float thickness = MathHelper.Lerp(baseWidth, maxWidth, speedFactor) * life;
                    if (thickness < 0.7f)
                        continue;
                    Color color = Color.Lerp(outerColor, innerColor, life) * life;
                    float drawLength = length + 1f;
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color * 0.35f, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness * 2.5f), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, new Color(255, 220, 200) * (life * 0.75f), delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness * 0.40f), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawHammer(Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Asset<Texture2D> texAsset = TextureAssets.Projectile[Projectile.type];
            if (!texAsset.IsLoaded)
                return;
            Texture2D texture = texAsset.Value;
            Vector2 origin = new Vector2(0f, texture.Height);
            float rotationOffset = MathHelper.PiOver4;
            float rotation = _angle + rotationOffset;
            float scale = Projectile.scale * _scaleMul * _outDist;
            Vector2 anchor = GetHammerRoot(player);
            Vector2 anchorScreen = anchor - Main.screenPosition;
            float t1 = (float)Main.GameUpdateCount * 0.10f;
            float t2 = (float)Main.GameUpdateCount * 0.17f + 1.3f;
            float pulseSlow = 0.5f + 0.5f * MathF.Sin(t1);
            float pulseFast = 0.5f + 0.5f * MathF.Sin(t2);
            float glowIntensity = (0.55f + _charge * 0.85f) * _alpha;
            DrawPulsingHalo(texture, anchorScreen, rotation, origin, scale, pulseSlow, pulseFast, glowIntensity);
            Color baseColor = Projectile.GetAlpha(lightColor) * _alpha;
            Main.EntitySpriteDraw(texture, anchorScreen, null, baseColor, rotation, origin, scale, SpriteEffects.None, 0);
        }
        private void DrawPulsingHalo(Texture2D texture, Vector2 anchorScreen, float rotation, Vector2 origin, float scale, float pulseSlow, float pulseFast, float glowIntensity)
        {
            if (glowIntensity <= 0.001f)
                return;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            try
            {
                DrawHaloLayers(texture, anchorScreen, rotation, origin, scale, pulseSlow, pulseFast, glowIntensity);
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawHaloLayers(Texture2D texture, Vector2 anchorScreen, float rotation, Vector2 origin, float scale, float pulseSlow, float pulseFast, float glowIntensity)
        {
            {
                float outerScale = scale * (1.32f + 0.16f * pulseSlow);
                float outerAlpha = (0.20f + 0.22f * pulseSlow) * glowIntensity;
                Color outerColor = new Color(150, 12, 22, 0) * outerAlpha;
                const int copies = 4;
                float radius = 1.2f + 1.0f * pulseSlow;
                for (int i = 0; i < copies; i++)
                {
                    float ang = MathHelper.TwoPi * i / copies;
                    Vector2 off = ang.ToRotationVector2() * radius;
                    Main.EntitySpriteDraw(texture, anchorScreen + off, null, outerColor, rotation, origin, outerScale, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(texture, anchorScreen, null, outerColor, rotation, origin, outerScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, anchorScreen, null, outerColor * 0.8f, rotation, origin, outerScale * 0.92f, SpriteEffects.None, 0);
            }
            {
                float midScale = scale * (1.14f + 0.10f * pulseFast);
                float midAlpha = (0.30f + 0.32f * pulseFast) * glowIntensity;
                Color midColor = new Color(220, 28, 38, 0) * midAlpha;
                const int copies = 4;
                float radius = 0.6f + 0.6f * pulseFast;
                for (int i = 0; i < copies; i++)
                {
                    float ang = MathHelper.TwoPi * i / copies + MathHelper.PiOver4;
                    Vector2 off = ang.ToRotationVector2() * radius;
                    Main.EntitySpriteDraw(texture, anchorScreen + off, null, midColor, rotation, origin, midScale, SpriteEffects.None, 0);
                }
            }
            {
                float innerPulse = 0.7f + 0.3f * pulseFast;
                float innerScale = scale * (1.04f + 0.05f * innerPulse);
                float innerAlpha = (0.38f + 0.28f * innerPulse) * glowIntensity;
                Color innerColor = new Color(255, 90, 70, 0) * innerAlpha;
                Main.EntitySpriteDraw(texture, anchorScreen, null, innerColor, rotation, origin, innerScale, SpriteEffects.None, 0);
            }
        }
        private static float EaseSwing(float x)
        {
            if (x < WindupEnd)
            {
                float u = x / WindupEnd;
                return 0.08f * u * u;
            }
            else if (x < SnapEnd)
            {
                float u = (x - WindupEnd) / (SnapEnd - WindupEnd);
                float inv = 1f - u;
                float k = 1f - inv * inv * inv * inv;
                return 0.08f + 0.78f * k;
            }
            else
            {
                float u = (x - SnapEnd) / (1f - SnapEnd);
                float inv = 1f - u;
                float k = 1f - inv * inv * inv;
                return 0.86f + 0.14f * k;
            }
        }
        private static float SnapPulse(float t, float center, float halfWidth)
        {
            float u = (t - center) / halfWidth;
            return MathF.Max(0f, 1f - u * u);
        }
    }
}
