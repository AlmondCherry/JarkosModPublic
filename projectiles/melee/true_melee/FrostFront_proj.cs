using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class FrostFront_proj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/FrostFront";
        private const int BaseSwingTime = 42;
        private const int FollowThroughFrames = 6;
        private const float WindupEnd = 0.18f;
        private const float HandleOffset = -16f;
        private const float SwordLength = 124f;
        private const float CollisionWidth = 38f;
        private const float HalfArc = 1.45f;
        private const int MaxTrailSamples = 36;
        private const float TrailDecayPerFrame = 1f / 36f;
        private const float SeedSpawnT = 0.55f;
        private bool _seedsSpawned;
        private int SwingDir => Projectile.ai[1] >= 0f ? 1 : -1;
        private float AimAngle
        {
            get => Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        private bool CanSpawnSeeds => Projectile.ai[0] >= 0.5f;
        private float Timer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private float _angle;
        private float _scaleMul = 1f;
        private float _outDist = 1f;
        private float _alpha = 1f;
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
            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.scale = 1.35f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = BaseSwingTime + FollowThroughFrames + 36;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.velocity.LengthSquared() > 0.0001f)
                AimAngle = Projectile.velocity.ToRotation();
            else
                AimAngle = (Main.MouseWorld - Main.player[Projectile.owner].MountedCenter).ToRotation();
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.30f, Volume = 0.50f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.35f,  Volume = 0.45f }, Projectile.position);
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
                Vector2 root = GetSwordRoot(player);
                float length = SwordLength * _scaleMul * _outDist;
                Vector2 head = root + _angle.ToRotationVector2() * length;
                Projectile.Center = Vector2.Lerp(root, head, 0.55f);
                Projectile.rotation = _angle;
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                int desiredDir = MathF.Cos(AimAngle) >= 0f ? 1 : -1;
                player.ChangeDir(desiredDir);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _angle - MathHelper.PiOver2);
                if (t > WindupEnd && t < 0.95f)
                    EmitTrailDust(root, head);
                if (!_seedsSpawned && t >= SeedSpawnT)
                {
                    _seedsSpawned = true;
                    if (CanSpawnSeeds)
                        SpawnSeeds(player, head);
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.6f, Volume = 0.55f }, head);
                }
                Lighting.AddLight(head, 0.30f, 0.55f, 0.85f);
                Lighting.AddLight(Projectile.Center, 0.20f, 0.40f, 0.65f);
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
            angle = MathHelper.Lerp(AimAngle - HalfArc * SwingDir, AimAngle + HalfArc * SwingDir, eased);
            if (t < WindupEnd)
            {
                float windK = 1f - (t / WindupEnd);
                angle -= 0.22f * SwingDir * windK;
            }
            float pulse = SnapPulse(t, 0.55f, 0.22f);
            outDist  = 0.95f + pulse * 0.18f;
            scaleMul = 0.95f + pulse * 0.20f;
        }
        private Vector2 GetSwordRoot(Player player)
        {
            return player.MountedCenter + _angle.ToRotationVector2() * HandleOffset;
        }
        private void EmitTrailDust(Vector2 root, Vector2 head)
        {
            if (!Main.rand.NextBool(2))
                return;
            Vector2 pos = Vector2.Lerp(root, head, Main.rand.NextFloat(0.55f, 1.05f));
            Vector2 perp = (_angle + MathHelper.PiOver2 * Main.rand.NextFloatDirection()).ToRotationVector2();
            Vector2 vel = perp * Main.rand.NextFloat(0.4f, 2.0f);
            int dustId = Main.rand.NextBool(3) ? DustID.SnowflakeIce : DustID.IceTorch;
            Dust dust = Dust.NewDustPerfect(pos, dustId, vel, 60, default, Main.rand.NextFloat(1.0f, 1.6f));
            dust.noGravity = true;
        }
        private void SpawnSeeds(Player player, Vector2 head)
        {
            if (Projectile.owner != Main.myPlayer)
                return;
            int seedCount = Main.rand.Next(2, 5);
            float swingTangent = _angle + MathHelper.PiOver2 * SwingDir;
            int seedDamage = Math.Max(1, (int)(Projectile.damage * 0.55f));
            for (int i = 0; i < seedCount; i++)
            {
                float angleSpread = Main.rand.NextFloat(-0.85f, 0.85f);
                Vector2 dir = (swingTangent + angleSpread).ToRotationVector2();
                Vector2 vel = dir * Main.rand.NextFloat(6.5f, 10.5f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2.0f);
                Projectile.NewProjectile(player.GetSource_FromThis(), head + Main.rand.NextVector2Circular(8f, 8f), vel, ModContent.ProjectileType<FrostSeed_proj>(), seedDamage, 1.0f, player.whoAmI);
            }
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
            if (t < 0.08f || t > 1f)
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
            if (t < WindupEnd + 0.05f || t > 0.95f)
                return false;
            Player player = Main.player[Projectile.owner];
            Vector2 start = GetSwordRoot(player);
            float length = SwordLength * _scaleMul * _outDist;
            Vector2 end = start + _angle.ToRotationVector2() * length;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint) ? true : null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 18; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.SnowflakeIce, dv, 60, default, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = Main.rand.NextBool(2);
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.IceTorch, dv, 100, default, 1.2f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit5 with { Pitch = -0.20f, Volume = 0.85f }, target.Center);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (_alpha <= 0.01f && _trail.Count == 0)
                return false;
            DrawTipRibbon();
            if (_alpha <= 0.01f)
                return false;
            DrawSword(lightColor);
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
                Color innerColor = new Color(220, 240, 255);
                Color outerColor = new Color(60, 130, 200);
                const float baseWidth = 2f;
                const float maxWidth = 10f;
                Vector2 anchorWorld = Main.player[Projectile.owner].MountedCenter;
                for (int i = 0; i < _trail.Count - 1; i++)
                {
                    TrailSample a = _trail[i];
                    TrailSample b = _trail[i + 1];
                    Vector2 from = anchorWorld + a.Tip;
                    Vector2 to = anchorWorld + b.Tip;
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
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color * 0.32f, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness * 2.5f), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, new Color(240, 250, 255) * (life * 0.75f), delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness * 0.40f), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawSword(Color lightColor)
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
            Vector2 anchor = GetSwordRoot(player);
            Vector2 anchorScreen = anchor - Main.screenPosition;
            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float t1 = (float)Main.GameUpdateCount * 0.10f;
                float t2 = (float)Main.GameUpdateCount * 0.16f + 1.0f;
                float pulseSlow = 0.5f + 0.5f * MathF.Sin(t1);
                float pulseFast = 0.5f + 0.5f * MathF.Sin(t2);
                float glow = (0.45f + 0.35f * pulseSlow) * _alpha;
                Color outerColor = new Color(70, 140, 220, 0) * (0.22f + 0.18f * pulseSlow) * glow;
                Color midColor = new Color(150, 210, 255, 0) * (0.28f + 0.22f * pulseFast) * glow;
                Color innerColor = new Color(230, 245, 255, 0) * (0.30f * _alpha);
                float baseScale = scale;
                float outerScale = baseScale * (1.22f + 0.10f * pulseSlow);
                float midScale = baseScale * (1.10f + 0.06f * pulseFast);
                for (int i = 0; i < 4; i++)
                {
                    float ang = MathHelper.TwoPi * i / 4f;
                    Vector2 off = ang.ToRotationVector2() * (1.0f + 0.6f * pulseSlow);
                    Main.EntitySpriteDraw(texture, anchorScreen + off, null, outerColor, rotation, origin, outerScale, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(texture, anchorScreen, null, midColor, rotation, origin, midScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, anchorScreen, null, innerColor, rotation, origin, baseScale * 1.04f, SpriteEffects.None, 0);
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Color baseColor = Projectile.GetAlpha(lightColor) * _alpha;
            Main.EntitySpriteDraw(texture, anchorScreen, null, baseColor, rotation, origin, scale, SpriteEffects.None, 0);
        }
        private static float EaseSwing(float x)
        {
            if (x < WindupEnd)
            {
                float u = x / WindupEnd;
                return 0.05f * u * u;
            }
            float v = (x - WindupEnd) / (1f - WindupEnd);
            float k = v < 0.5f
                ? 4f * v * v * v
                : 1f - MathF.Pow(-2f * v + 2f, 3f) / 2f;
            return 0.05f + 0.95f * k;
        }
        private static float SnapPulse(float t, float center, float halfWidth)
        {
            float u = (t - center) / halfWidth;
            return MathF.Max(0f, 1f - u * u);
        }
    }
}
