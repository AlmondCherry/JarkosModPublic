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
using jarkos.items.weapons.melee.other_melee;
using jarkos.particles;
using jarkos.buffs.debuff;

namespace jarkos.projectiles.melee.other_melee
{
    public struct SpearDeathTrailSample
    {
        public Vector2 Tip;
        public float Time;
        public float Speed;
    }

    public class Spear_death_proj : ModProjectile
    {
        private const int SwingTime = 24;
        private const int FollowThroughFrames = 5;
        private const float HandleOffset = -16f;
        private const float BladeLength = 128f;
        private const float CollisionWidth = 12f;

        public int Variation => (int)Projectile.ai[0];
        private float Timer => Projectile.ai[1];
        private float AimAngle => Projectile.ai[2];

        private float _bladeAngle;
        private float _scaleMul = 1f;
        private float _outDist = 1f;
        private float _alpha = 1f;

        private const int MaxTrailSamples = 32;
        private const float TrailDecayPerFrame = 1f / 7f;
        private readonly List<SpearDeathTrailSample> _trail = new(MaxTrailSamples);
        private Vector2 _lastTipOffset;
        private bool _hasLastTip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = SwingTime + FollowThroughFrames + 16;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.scale = 0.9f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void OnSpawn(IEntitySource source)
        {
            if (Projectile.velocity.LengthSquared() > 0.0001f)
            {
                Projectile.ai[2] = Projectile.velocity.ToRotation();
            }

            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = (Variation == 0) ? 0.1f : -0.1f, Volume = 0.85f }, Projectile.position);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            bool playerOk = player.active && !player.dead;
            bool animationDone = Timer >= SwingTime + FollowThroughFrames;

            if (!playerOk)
            {
                Projectile.Kill();
                return;
            }

            if (!animationDone)
            {
                float t = MathHelper.Clamp(Timer / SwingTime, 0f, 1f);
                float life = (Timer + FollowThroughFrames) / (SwingTime + FollowThroughFrames);

                ComputePose(t, out _bladeAngle, out _scaleMul, out _outDist);

                _alpha = MathHelper.Clamp(1.4f - life * 1.1f, 0f, 1f);

                Vector2 root = GetBladeRoot(player);
                float fullLength = BladeLength * _scaleMul * _outDist;
                Vector2 tip = root + _bladeAngle.ToRotationVector2() * fullLength;

                Projectile.Center = Vector2.Lerp(root, tip, 0.55f);
                Projectile.rotation = _bladeAngle;

                if (player.HeldItem?.ModItem is SpearOfDeath)
                {
                    player.heldProj = Projectile.whoAmI;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    int desiredDir = MathF.Cos(AimAngle) >= 0f ? 1 : -1;
                    player.ChangeDir(desiredDir);
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _bladeAngle - MathHelper.PiOver2);
                }

                if (t > 0.20f && t < 0.88f)
                {
                    EmitTrailDust(root, tip);
                }

                Lighting.AddLight(tip, 0.8f, 0.2f, 0.25f);
                DecayTrail();
                RecordTrailSample(tip, player.MountedCenter, t);

                Projectile.ai[1] += 1f;
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

        private void DecayTrail()
        {
            for (int i = _trail.Count - 1; i >= 0; i--)
            {
                SpearDeathTrailSample s = _trail[i];
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

            _trail.Insert(0, new SpearDeathTrailSample
            {
                Tip = tipOffset,
                Time = 1f,
                Speed = speed,
            });

            if (_trail.Count > MaxTrailSamples)
            {
                _trail.RemoveAt(_trail.Count - 1);
            }

            _hasLastTip = true;
            _lastTipOffset = tipOffset;
        }

        private void ComputePose(float t, out float angle, out float scaleMul, out float outDist)
        {
            const float halfArc = 1.55f;
            float eased = EaseInOutCubic(t);

            switch (Variation)
            {
                case 0: 
                {
                    float wind = (t < 0.20f) ? -0.18f * (1f - t / 0.20f) : 0f;
                    angle = MathHelper.Lerp(AimAngle - halfArc, AimAngle + halfArc, eased) + wind;
                    outDist = 0.95f + ScalePulse(t) * 0.18f;
                    scaleMul = 0.95f + ScalePulse(t) * 0.22f;
                    return;
                }
                case 1: 
                default:
                {
                    float wind = (t < 0.20f) ? 0.18f * (1f - t / 0.20f) : 0f;
                    angle = MathHelper.Lerp(AimAngle + halfArc, AimAngle - halfArc, eased) + wind;
                    outDist = 0.95f + ScalePulse(t) * 0.18f;
                    scaleMul = 0.95f + ScalePulse(t) * 0.22f;
                    return;
                }
            }
        }

        private Vector2 GetBladeRoot(Player player)
        {
            return player.MountedCenter + _bladeAngle.ToRotationVector2() * HandleOffset;
        }

        private void EmitTrailDust(Vector2 root, Vector2 tip)
        {
            if (!Main.rand.NextBool(2))
            {
                return;
            }

            Vector2 pos = Vector2.Lerp(root, tip, Main.rand.NextFloat(0.35f, 1f));
            Vector2 vel = (_bladeAngle + MathHelper.PiOver2 * Main.rand.NextFloatDirection()).ToRotationVector2() * Main.rand.NextFloat(0.5f, 2.4f);
            Dust dust = Dust.NewDustPerfect(pos, DustID.RedTorch, vel, Scale: Main.rand.NextFloat(1.0f, 1.6f));
            dust.noGravity = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float t = MathHelper.Clamp(Timer / SwingTime, 0f, 1f);
            if (t < 0.15f || t > 0.92f)
            {
                return false;
            }

            Player player = Main.player[Projectile.owner];
            Vector2 start = GetBladeRoot(player);
            float fullLength = BladeLength * _scaleMul * _outDist;
            Vector2 end = start + _bladeAngle.ToRotationVector2() * fullLength;

            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, CollisionWidth, ref collisionPoint) ? true : null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnHitEffects(target);
            SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = 0.4f, Volume = 0.8f }, target.Center);
            target.AddBuff(ModContent.BuffType<SpearDeathDebuff>(), 360);
        }

        private void SpawnHitEffects(NPC target)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustVel = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(1f, 4f);
                Dust dust = Dust.NewDustDirect(target.Center - new Vector2(10, 10), 20, 20, DustID.Blood, dustVel.X, dustVel.Y, 100, default, 1.4f);
                dust.noGravity = Main.rand.NextBool();
            }

            var particle = new UpGlow_particle(
                target,
                Color.IndianRed,
                20,
                upDistance: 100f,
                scale: 0.8f,
                widthScale: 1.0f,
                initialVerticalScale: 0f
            );
            particle.Spawn();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_alpha <= 0.01f && _trail.Count == 0)
            {
                return false;
            }

            
            if (_trail.Count >= 2)
            {
                try
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                    Texture2D pixel = TextureAssets.MagicPixel.Value;
                    Rectangle pixelRect = new Rectangle(0, 0, 1, 1);
                    Vector2 pixelOrigin = new Vector2(0f, 0.5f);

                    Color innerColor = new Color(255, 160, 140);
                    Color outerColor = new Color(220, 30, 30);

                    const float baseWidth = 1f;
                    const float maxWidth = 6.5f;

                    Vector2 anchorWorld = Main.player[Projectile.owner].MountedCenter;

                    for (int i = 0; i < _trail.Count - 1; i++)
                    {
                        SpearDeathTrailSample a = _trail[i];
                        SpearDeathTrailSample b = _trail[i + 1];

                        Vector2 from = anchorWorld + a.Tip;
                        Vector2 to = anchorWorld + b.Tip;
                        Vector2 delta = to - from;
                        float length = delta.Length();
                        if (length < 0.01f)
                        {
                            continue;
                        }

                        float life = MathHelper.Clamp(a.Time, 0f, 1f);
                        float speedFactor = MathHelper.Clamp(a.Speed / 12f, 0f, 1f);
                        float thickness = MathHelper.Lerp(baseWidth, maxWidth, speedFactor) * life;
                        if (thickness < 0.5f)
                        {
                            continue;
                        }

                        Color color = Color.Lerp(outerColor, innerColor, life) * life;
                        float drawLength = length + 1f;

                        Main.spriteBatch.Draw(pixel, from - Main.screenPosition, pixelRect, color, delta.ToRotation(), pixelOrigin, new Vector2(drawLength, thickness), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            if (_alpha <= 0.01f)
            {
                return false;
            }

            Player player = Main.player[Projectile.owner];
            Asset<Texture2D> textureAsset = TextureAssets.Projectile[Projectile.type];
            Texture2D texture = textureAsset.Value;

            Vector2 origin = new Vector2(0f, texture.Height);
            float rotationOffset = MathHelper.PiOver4;
            float rotation = _bladeAngle + rotationOffset;
            float scale = Projectile.scale * _scaleMul * _outDist;

            Vector2 anchor = GetBladeRoot(player);
            Vector2 anchorScreen = anchor - Main.screenPosition;

            Color baseColor = Projectile.GetAlpha(lightColor) * _alpha;
            Main.EntitySpriteDraw(texture, anchorScreen, null, baseColor, rotation, origin, scale, SpriteEffects.None, 0);

            return false;
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
