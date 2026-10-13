using System;
using System.Collections.Generic;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.particles;
using jarkos.systems;
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
    public class PurgatorySwingProj : ModProjectile
    {
        public override string Texture => "Jarkos/items/weapons/melee/unique_melee/PurgatorySpear";
        private const int SwingTime = 28;
        private const int FollowThroughFrames = 5;
        private const float HandleOffset = -98f;
        private const float BladeLength = 226f;
        private const float CollisionWidth = 16f;
        public int Variation => (int)Projectile.ai[0];
        private float Timer => Projectile.ai[1];
        private float AimAngle => Projectile.ai[2];
        private float _bladeAngle;
        private float _scaleMul = 1f;
        private float _outDist = 1f;
        private float _alpha = 1f;
        private bool[] _supercritHits;
        private const int MaxTrailSamples = 38;
        private const float TrailDecayPerFrame = 1f / 8f;
        private readonly List<PurgatoryTrailSample> _trail = new(MaxTrailSamples);
        private Vector2 _lastTipOffset;
        private bool _hasLastTip;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 28;
            ProjectileID.Sets.TrailingMode[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = SwingTime + FollowThroughFrames + 22;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.scale = 2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void OnSpawn(IEntitySource source)
        {
            _supercritHits = new bool[Main.maxNPCs + 1];
            if (Projectile.velocity.LengthSquared() > 0.0001f)
            {
                Projectile.ai[2] = Projectile.velocity.ToRotation();
            }
            switch (Variation)
            {
                case 0:
                case 1:
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PurgatorySlash") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
                    break;
                case 2:
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PurgatorySlash") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
                    break;
            }
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
                if (player.HeldItem?.ModItem is PurgatorySpear)
                {
                    player.heldProj = Projectile.whoAmI;
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    int desiredDir = MathF.Cos(AimAngle) >= 0f ? 1 : -1;
                    player.ChangeDir(desiredDir);
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, _bladeAngle - MathHelper.PiOver2);
                }
                if (t > 0.30f && t < 0.85f)
                {
                    EmitTrailDust(root, tip);
                }
                Lighting.AddLight(tip, 0.55f, 0.42f, 0.18f);
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
                PurgatoryTrailSample s = _trail[i];
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
            _trail.Insert(0, new PurgatoryTrailSample            {
                Tip = tipOffset, Time = 1f, Speed = speed, });
            if (_trail.Count > MaxTrailSamples)
            {
                _trail.RemoveAt(_trail.Count - 1);
            }
            _hasLastTip = true;
            _lastTipOffset = tipOffset;
        }
        private void ComputePose(float t, out float angle, out float scaleMul, out float outDist)
        {
            switch (Variation)
            {
                case 0:
                {
                    const float halfArc = 1.55f;
                    float eased = MathSystem.EaseInOutCubic(t);
                    float wind = (t < 0.20f) ? -0.18f * (1f - t / 0.20f) : 0f;
                    angle = MathHelper.Lerp(AimAngle - halfArc, AimAngle + halfArc, eased) + wind;
                    outDist = 0.95f + MathSystem.ScalePulse(t) * 0.18f;
                    scaleMul = 0.95f + MathSystem.ScalePulse(t) * 0.22f;
                    return;
                }
                case 1:
                {
                    const float halfArc = 1.55f;
                    float eased = MathSystem.EaseInOutCubic(t);
                    float wind = (t < 0.20f) ? 0.18f * (1f - t / 0.20f) : 0f;
                    angle = MathHelper.Lerp(AimAngle + halfArc, AimAngle - halfArc, eased) + wind;
                    outDist = 0.95f + MathSystem.ScalePulse(t) * 0.18f;
                    scaleMul = 0.95f + MathSystem.ScalePulse(t) * 0.22f;
                    return;
                }
                default:
                {
                    float twist;
                    if (t < 0.28f)
                    {
                        float u = t / 0.28f;
                        outDist = MathHelper.Lerp(0.85f, 0.55f, MathSystem.EaseOutCubic(u));
                        twist = MathHelper.Lerp(0f, -0.20f, MathSystem.EaseOutCubic(u));
                    }
                    else if (t < 0.62f)
                    {
                        float u = (t - 0.28f) / (0.62f - 0.28f);
                        outDist = MathHelper.Lerp(0.55f, 1.45f, MathSystem.EaseOutCubic(u));
                        twist = MathHelper.Lerp(-0.20f, 0f, MathSystem.EaseOutCubic(u));
                    }
                    else
                    {
                        float u = (t - 0.62f) / (1f - 0.62f);
                        outDist = MathHelper.Lerp(1.45f, 0.80f, MathSystem.EaseInCubic(u));
                        twist = MathHelper.Lerp(0f, 0.40f, MathSystem.EaseInCubic(u));
                    }
                    angle = AimAngle + twist;
                    scaleMul = 0.95f + MathSystem.ScalePulse(t) * 0.18f;
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
            Dust dust = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, Scale: Main.rand.NextFloat(0.95f, 1.55f));
            dust.noGravity = true;
            dust.fadeIn = 0.6f;
        }
        private void EmitHitStarParticles(NPC target)
        {
            float swingAngle = Variation switch
            { 
                0 => _bladeAngle + MathHelper.PiOver2, 
                1 => _bladeAngle - MathHelper.PiOver2, 
                _ => _bladeAngle,
            };

                const int count = 3;
                const float spreadHalf = 1.75f;
                float radiusX = target.width * 0.35f;
                float radiusY = target.height * 0.35f;

                for (int i = 0; i < count; i++)
                {
                    float angle = swingAngle + Main.rand.NextFloatDirection() * spreadHalf;
                    Vector2 dir = angle.ToRotationVector2();
                    float speed = Main.rand.NextFloat(3.5f, 7.5f);
                    Vector2 vel = dir * speed;
                    Vector2 pos = target.Center + Main.rand.NextVector2Circular(radiusX, radiusY);
                    new StarParticle(pos, vel, Color.Yellow, 250, Main.rand.NextFloat(0.005f, 0.2f), 1.75f, Main.rand.NextFloat(-0.25f, 0.25f)).Spawn();
                }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float t = MathHelper.Clamp(Timer / SwingTime, 0f, 1f);

            if (t < 0.18f || t > 0.92f)
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
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Main.rand.NextFloat() < 0.15f)
            {
                modifiers.SourceDamage *= 3.5f;
                if (_supercritHits != null && target.whoAmI < _supercritHits.Length)
                {
                    _supercritHits[target.whoAmI] = true;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            EmitHitStarParticles(target);
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PurgatoryHit") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
            if (_supercritHits == null || target.whoAmI >= _supercritHits.Length)
            {
                return;
            }
            if (!_supercritHits[target.whoAmI])
            {
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GoldFlame, 0f, 0f, Scale: 1.1f);
                    dust.noGravity = true;
                }
                return;
            }
            _supercritHits[target.whoAmI] = false;
            CombatText.NewText(target.Hitbox, new Color(255, 215, 96), "Super-critical!", true);
            for (int i = 0; i < 22; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2.5f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, Scale: Main.rand.NextFloat(1.3f, 1.9f));
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PurgatoryHitNPC") with { Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
            for (int i = 0; i < 12; i++)
            {
                Vector2 ToMouse = Main.MouseWorld - Projectile.Center;
                ToMouse.Normalize();
                new Spark(target.Center, ToMouse.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-90f, 90f))) * Main.rand.NextFloat(6f, 18f), Color.Red, 250, Main.rand.NextFloat(0.5f, 1.25f), 1.75f, 0f).Spawn();
            }
            Lighting.AddLight(target.Center, 1.4f, 1.0f, 0.4f);
            if (Projectile.owner == Main.myPlayer)
            {
                Player player = Main.player[Projectile.owner];
                player.GetModPlayer<PurgatoryPlayer>().AddShake(6f, 16);
            }
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
                    Color innerColor = new Color(255, 235, 170);
                    Color outerColor = new Color(255, 130, 60);

                    const float baseWidth = 1f;
                    const float maxWidth = 6f;

                    Vector2 anchorWorld = Main.player[Projectile.owner].MountedCenter;

                    for (int i = 0; i < _trail.Count - 1; i++)
                    {
                        PurgatoryTrailSample a = _trail[i];
                        PurgatoryTrailSample b = _trail[i + 1];

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
    }
}
