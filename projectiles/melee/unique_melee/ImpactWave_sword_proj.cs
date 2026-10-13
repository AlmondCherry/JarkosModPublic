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
namespace jarkos.projectiles.melee.unique_melee
{
    public class ImpactWave_sword_proj : ModProjectile
    {
        public override string Texture => "jarkos/items/weapons/melee/unique_melee/ImpactWave_sword";
        private const int BaseSwingTime = 49;
        private const int FollowThroughFrames = 5;
        private const float HandleOffset = -12f;
        private const float BladeLength  = 205f;
        private const float CollisionWidth = 28f;
        private const int MaxTrailSamples = 36;
        private const float TrailDecayPerFrame = 1f / 27f;
        private int RawVariant => (int)Projectile.ai[0];
        public bool IsMega => RawVariant >= 10;
        public int AttackStyle => RawVariant % 10;
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
        private float _bladeAngle;
        private float _scaleMul = 1f;
        private float _outDist  = 1f;
        private float _alpha    = 1f;
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
            ProjectileID.Sets.TrailCacheLength[Type] = 38;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width  = 160;
            Projectile.height = 160;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile  = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.scale = 1.2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = BaseSwingTime + FollowThroughFrames + 36;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.velocity.LengthSquared() > 0.0001f)
            {
                AimAngle = Projectile.velocity.ToRotation();
            }
            else
            {
                Player p = Main.player[Projectile.owner];
                AimAngle = (Main.MouseWorld - p.MountedCenter).ToRotation();
            }
            if (IsMega)
            {
                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.position);
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.3f, Volume = 0.6f }, Projectile.position);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = AttackStyle == 1 ? 0.30f : -0.20f, Volume = 0.85f }, Projectile.position);
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            bool playerOk = player != null && player.active && !player.dead;
            if (!playerOk)
            {
                Projectile.Kill();
                return;
            }
            float swingTime = BaseSwingTime * (IsMega ? 1.35f : 1f);
            bool animationDone = Timer >= swingTime + FollowThroughFrames;
            if (!animationDone)
            {
                float t = MathHelper.Clamp(Timer / swingTime, 0f, 1f);
                float life = (Timer + FollowThroughFrames) / (swingTime + FollowThroughFrames);
                ComputePose(t, out _bladeAngle, out _scaleMul, out _outDist);
                _alpha = MathHelper.Clamp(1.4f - life * 1.1f, 0f, 1f);
                Vector2 root = GetBladeRoot(player);
                float fullLength = BladeLength * _scaleMul * _outDist;
                Vector2 tip = root + _bladeAngle.ToRotationVector2() * fullLength;
                Projectile.Center = Vector2.Lerp(root, tip, 0.55f);
                Projectile.rotation = _bladeAngle;
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                int desiredDir = MathF.Cos(AimAngle) >= 0f ? 1 : -1;
                player.ChangeDir(desiredDir);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _bladeAngle - MathHelper.PiOver2);
                if (t > 0.25f && t < 0.88f)
                {
                    EmitTrailDust(root, tip);
                }
                Lighting.AddLight(tip, 0.30f, 0.50f, 0.95f);
                Lighting.AddLight(Projectile.Center, 0.18f, 0.32f, 0.65f);
                DecayTrail();
                RecordTrailSample(tip, player.MountedCenter, t);
                Timer += 1f;
            }
            else
            {
                _alpha = 0f;
                DecayTrail();
                if (_trail.Count == 0)
                {
                    Projectile.Kill();
                }
            }
        }
        private void ComputePose(float t, out float angle, out float scaleMul, out float outDist)
        {
            float eased = EaseInOutCubic(t);
            float halfArc = IsMega ? 1.85f : 1.55f;
            if (AttackStyle == 0)
            {
                float wind = (t < 0.20f) ? -0.20f * (1f - t / 0.20f) : 0f;
                angle = MathHelper.Lerp(AimAngle - halfArc * SwingDir, AimAngle + halfArc * SwingDir, eased) + wind * SwingDir;
            }
            else
            {
                float wind = (t < 0.20f) ? 0.20f * (1f - t / 0.20f) : 0f;
                angle = MathHelper.Lerp(AimAngle + halfArc * SwingDir, AimAngle - halfArc * SwingDir, eased) + wind * SwingDir;
            }
            float pulse = ScalePulse(t);
            outDist  = 0.95f + pulse * (IsMega ? 0.30f : 0.18f);
            scaleMul = 0.95f + pulse * (IsMega ? 0.32f : 0.22f);
        }
        private Vector2 GetBladeRoot(Player player)
        {
            return player.MountedCenter + _bladeAngle.ToRotationVector2() * HandleOffset;
        }
        private void EmitTrailDust(Vector2 root, Vector2 tip)
        {
            if (!Main.rand.NextBool(IsMega ? 1 : 2))
                return;
            Vector2 pos = Vector2.Lerp(root, tip, Main.rand.NextFloat(0.35f, 1f));
            Vector2 perp = (_bladeAngle + MathHelper.PiOver2 * Main.rand.NextFloatDirection()).ToRotationVector2();
            Vector2 vel = perp * Main.rand.NextFloat(0.5f, 2.4f);
            int dustId = IsMega
                ? (Main.rand.NextBool(3) ? DustID.PinkTorch : DustID.BlueFairy)
                : DustID.BlueFairy;
            Dust dust = Dust.NewDustPerfect(pos, dustId, vel, 120, default, Main.rand.NextFloat(0.95f, 1.55f));
            dust.noGravity = true;
            dust.fadeIn = 0.6f;
        }
        private void DecayTrail()
        {
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                TrailSample s = _trail[i];
                s.Time -= TrailDecayPerFrame;
                if (s.Time <= 0f)
                {
                    _trail.RemoveAt(i);
                }
                else
                {
                    _trail[i] = s;
                }
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
            float speed = 0f;
            if (_hasLastTip)
            {
                speed = Vector2.Distance(tipOffset, _lastTipOffset);
            }
            _trail.Insert(0, new TrailSample            {
                Tip = tipOffset, Time = 1f, Speed = speed, });
            if (_trail.Count > MaxTrailSamples)
            {
                _trail.RemoveAt(_trail.Count - 1);
            }
            _hasLastTip = true;
            _lastTipOffset = tipOffset;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float t = MathHelper.Clamp(Timer / (BaseSwingTime * (IsMega ? 1.35f : 1f)), 0f, 1f);
            if (t < 0.18f || t > 0.92f)
                return false;
            Player player = Main.player[Projectile.owner];
            Vector2 start = GetBladeRoot(player);
            float fullLength = BladeLength * _scaleMul * _outDist;
            Vector2 end = start + _bladeAngle.ToRotationVector2() * fullLength;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth * (IsMega ? 1.2f : 1f), ref collisionPoint) ? true : null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            float swingTangent = AttackStyle == 0
                ? _bladeAngle + MathHelper.PiOver2 * SwingDir
                : _bladeAngle - MathHelper.PiOver2 * SwingDir;
            for (int i = 0; i < (IsMega ? 28 : 16); i++)
            {
                Vector2 dustVel = (swingTangent + Main.rand.NextFloatDirection() * 0.9f).ToRotationVector2() * Main.rand.NextFloat(2f, 7f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.BlueFairy, dustVel, 120, default, 1.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < (IsMega ? 12 : 6); i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PinkTorch, dv, 100, default, 1.2f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, target.Center);
            if (Main.myPlayer != Projectile.owner)
                return;
            int starCount = IsMega ? 4 : 2;
            int starDamage = Projectile.damage;
            for (int i = 0; i < starCount; i++)
            {
                float a = swingTangent + MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.20f, 0.20f);
                Vector2 vel = a.ToRotationVector2() * Main.rand.NextFloat(7.5f, 11.5f);
                Projectile.NewProjectile(player.GetSource_FromThis(), target.Center, vel, ModContent.ProjectileType<AntiCollapsarSingularity_proj>(), starDamage, 2f, player.whoAmI, ai0: 0f, ai1: target.whoAmI);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (_alpha <= 0.01f && _trail.Count == 0)
                return false;
            DrawTipRibbon();
            if (_alpha <= 0.01f)
                return false;
            DrawBlade(lightColor);
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
                Color outerColor = new Color(70, 110, 230);
                if (IsMega)
                {
                    innerColor = new Color(255, 230, 255);
                    outerColor = new Color(150, 90, 230);
                }
                const float baseWidth = 1f;
                float maxWidth = IsMega ? 9f : 6f;
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
                    float speedFactor = MathHelper.Clamp(a.Speed / 14f, 0f, 1f);
                    float thickness = MathHelper.Lerp(baseWidth, maxWidth, speedFactor) * life;
                    if (thickness < 0.5f)
                        continue;
                    Color color = Color.Lerp(outerColor, innerColor, life) * life;
                    float drawLength = length + 1f;
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color * 0.35f, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness * 2.4f), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness), SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, Color.White * (life * 0.85f), delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness * 0.45f), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawBlade(Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Asset<Texture2D> texAsset = TextureAssets.Projectile[Projectile.type];
            if (!texAsset.IsLoaded)
                return;
            Texture2D texture = texAsset.Value;
            Vector2 origin = new Vector2(0f, texture.Height);
            float rotationOffset = MathHelper.PiOver4;
            float rotation = _bladeAngle + rotationOffset;
            float scale = Projectile.scale * _scaleMul * _outDist;
            Vector2 anchor = GetBladeRoot(player);
            Vector2 anchorScreen = anchor - Main.screenPosition;
            Color glow = (IsMega
                ? new Color(180, 140, 255, 0)
                : new Color(120, 180, 255, 0)) * (0.55f * _alpha);
            for (int i = 0; i < 4; i++)
            {
                float a = i / 4f;
                Vector2 jitter = (MathHelper.TwoPi * a + (float)Main.GameUpdateCount * 0.07f).ToRotationVector2() * 1.6f;
                Main.EntitySpriteDraw(texture, anchorScreen + jitter, null, glow, rotation, origin, scale * 1.15f, SpriteEffects.None, 0);
            }
            Color baseColor = Projectile.GetAlpha(lightColor) * _alpha;
            Main.EntitySpriteDraw(texture, anchorScreen, null, baseColor, rotation, origin, scale, SpriteEffects.None, 0);
        }
        private static float EaseInOutCubic(float x)
        {
            return x < 0.5f
                ? 4f * x * x * x
                : 1f - MathF.Pow(-2f * x + 2f, 3f) / 2f;
        }
        private static float ScalePulse(float t)
        {
            float u = t * 2f - 1f;
            return MathF.Max(0f, 1f - u * u);
        }
    }
}
