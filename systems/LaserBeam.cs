using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.systems
{
    public abstract class Laserbeam : ModProjectile
    {
        #region Auto-Properties
        public float RotationalSpeed
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float Time
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public float LaserLength
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public int MaxRicochets
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public int CurrentRicochet
        {
            get => (int)Projectile.localAI[2];
            set => Projectile.localAI[2] = value;
        }
        public Vector2[] RicochetPoints
        {
            get
            {
                if (_ricochetPoints == null)
                {
                    _ricochetPoints = new Vector2[Math.Max(MaxRicochets, 0) + 1];
                    _ricochetPoints[0] = Projectile.Center;
                }
                return _ricochetPoints;
            }
            set => _ricochetPoints = value;
        }
        private Vector2[] _ricochetPoints;
        #endregion
        #region Shader Registration
        private static readonly HashSet<string> _registeredShaderKeys = new HashSet<string>();
        private static readonly object _registerLock = new object();
        protected void EnsureShadersRegistered()
        {
            if (Main.dedServ)
                return;

            if (UseFlowShader)
                RegisterShaderIfNeeded(FlowShaderKey, FlowShaderPath, FlowShaderPass);

            if (UseGlowShader)
                RegisterShaderIfNeeded(GlowShaderKey, GlowShaderPath, GlowShaderPass);
        }
        private static void RegisterShaderIfNeeded(string key, string path, string pass)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(path) || string.IsNullOrEmpty(pass))
                return;
            lock (_registerLock)
            {
                if (_registeredShaderKeys.Contains(key))
                    return;
                if (GameShaders.Misc.ContainsKey(key))
                {
                    _registeredShaderKeys.Add(key);
                    return;
                }
                try
                {
                    Asset<Effect> effectAsset = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
                    GameShaders.Misc[key] = new MiscShaderData(new Ref<Effect>(effectAsset.Value), pass);
                    _registeredShaderKeys.Add(key);
                }
                catch
                {
                }
            }
        }
        #endregion
        #region Virtual Methods
        public virtual void Behavior()
        {
            AttachToSomething();
            Projectile.velocity = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
            Time++;

            if (Time >= Lifetime)
            {
                Projectile.Kill();
                return;
            }

            DetermineScale();
            UpdateLaserMotion();

            float idealLaserLength = DetermineLaserLength();
            LaserLength = MathHelper.Lerp(LaserLength, idealLaserLength, 0.9f);

            if (LightCastColor != Color.Transparent)
            {
                DelegateMethods.v3_1 = LightCastColor.ToVector3();
                Terraria.Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Projectile.width * Projectile.scale, DelegateMethods.CastLight);
            }
        }
        public virtual void UpdateLaserMotion()
        {
            if (CanRicochet && CurrentRicochet < MaxRicochets)
            {
                CheckForRicochet();
            }

            float updatedVelocityDirection = Projectile.velocity.ToRotation() + RotationalSpeed;
            
            Projectile.rotation = updatedVelocityDirection - MathHelper.PiOver2;
            Projectile.velocity = updatedVelocityDirection.ToRotationVector2();
        }
        public virtual void CheckForRicochet()
        {
            float maxDistance = LaserLength;
            Vector2 start = RicochetPoints[CurrentRicochet];
            Vector2 end = start + Projectile.velocity * maxDistance;
            if (FindRicochetPoint(start, end, out Vector2 collisionPoint, out Vector2 normal))
            {
                RicochetPoints[CurrentRicochet + 1] = collisionPoint;
                Vector2 newDirection = Vector2.Reflect(Projectile.velocity, normal);
                newDirection.Normalize();

                Projectile.Center = collisionPoint;
                Projectile.velocity = newDirection;
                CurrentRicochet++;
                LaserLength = 0;

                OnRicochet(collisionPoint, normal);
            }
        }
        protected virtual bool FindRicochetPoint(Vector2 start, Vector2 end, out Vector2 collisionPoint, out Vector2 normal)
        {
            collisionPoint = Vector2.Zero;
            normal = Vector2.Zero;
            float stepSize = 16f;

            Vector2 direction = Vector2.Normalize(end - start);
            float distance = Vector2.Distance(start, end);

            for (float i = 0; i < distance; i += stepSize)
            {
                Vector2 checkPoint = start + direction * i;
                Point tilePos = checkPoint.ToTileCoordinates();
                if (tilePos.X < 0 || tilePos.X >= Main.maxTilesX || tilePos.Y < 0 || tilePos.Y >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[tilePos.X, tilePos.Y];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    collisionPoint = checkPoint;
                    normal = DetermineSurfaceNormal(tilePos, checkPoint);
                    return true;
                }
            }
            return false;
        }
        protected virtual Vector2 DetermineSurfaceNormal(Point tilePos, Vector2 collisionPoint)
        {
            Vector2 tileCenter = tilePos.ToWorldCoordinates(8, 8);
            Vector2 toCollision = collisionPoint - tileCenter;

            if (Math.Abs(toCollision.X) > Math.Abs(toCollision.Y))
                return new Vector2(Math.Sign(toCollision.X), 0);

            return new Vector2(0, Math.Sign(toCollision.Y));
        }
        protected virtual void OnRicochet(Vector2 collisionPoint, Vector2 normal)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(collisionPoint, DustID.Electric);
                dust.velocity = normal.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 5f);
                dust.noGravity = true;
                dust.scale = 1.5f;
            }
            SoundEngine.PlaySound(SoundID.Item10, collisionPoint);
        }
        public virtual void DetermineScale()
        {
            Projectile.scale = (float)Math.Sin(Time / Lifetime * MathHelper.Pi) * ScaleExpandRate * MaxScale;
            if (Projectile.scale > MaxScale)
                Projectile.scale = MaxScale;
        }
        public virtual void AttachToSomething() { }
        public virtual float DetermineLaserLength()
        {
            if (CanRicochet && CurrentRicochet > 0)
            {
                Vector2 start = RicochetPoints[CurrentRicochet];
                return DetermineLaserLength_CollideWithTiles(10, start);
            }
            return MaxLaserLength;
        }
        public float DetermineLaserLength_CollideWithTiles(int samplePointCount, Vector2 startPoint)
        {
            float[] laserLengthSamplePoints = new float[samplePointCount];
            Collision.LaserScan(startPoint, Projectile.velocity, Projectile.scale, MaxLaserLength, laserLengthSamplePoints);
            return laserLengthSamplePoints.Average();
        }
        public virtual void ExtraBehavior() { }
        #endregion
        #region Helper Methods
        public float DetermineLaserLength_CollideWithTiles(int samplePointCount)
        {
            Vector2 startPoint = CanRicochet && CurrentRicochet > 0 ?
                RicochetPoints[CurrentRicochet] : Projectile.Center;
            return DetermineLaserLength_CollideWithTiles(samplePointCount, startPoint);
        }
        protected internal void DrawBeamWithColor(Color beamColor, float scale, int startFrame = 0, int middleFrame = 0, int endFrame = 0)
        {
            if (CanRicochet && CurrentRicochet > 0)
            {
                DrawBeamWithRicochets(beamColor, scale, startFrame, middleFrame, endFrame);
                return;
            }
            DrawSingleBeamSegment(Projectile.Center, beamColor, scale, startFrame, middleFrame, endFrame);
        }
        protected virtual void DrawBeamWithRicochets(Color beamColor, float scale, int startFrame = 0, int middleFrame = 0, int endFrame = 0)
        {
            for (int i = 0; i <= CurrentRicochet; i++)
            {
                Vector2 segmentStart = RicochetPoints[i];
                Vector2 segmentEnd;

                if (i == CurrentRicochet)
                    segmentEnd = segmentStart + Projectile.velocity * LaserLength;
                else
                    segmentEnd = RicochetPoints[i + 1];

                Vector2 originalCenter = Projectile.Center;
                Vector2 originalVelocity = Projectile.velocity;
                
                float originalRotation = Projectile.rotation;
                float originalLength = LaserLength;

                Projectile.Center = segmentStart;
                Projectile.velocity = Vector2.Normalize(segmentEnd - segmentStart);
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                LaserLength = Vector2.Distance(segmentStart, segmentEnd);

                DrawSingleBeamSegment(segmentStart, beamColor, scale, startFrame, middleFrame, endFrame);
                Projectile.Center = originalCenter;
                Projectile.velocity = originalVelocity;
                Projectile.rotation = originalRotation;
                LaserLength = originalLength;
            }
        }
        protected virtual void DrawSingleBeamSegment(Vector2 segmentStart, Color beamColor, float scale, int startFrame, int middleFrame, int endFrame)
        {
            Rectangle startFrameArea = LaserBeginTexture.Frame(1, Main.projFrames[Projectile.type], 0, startFrame);
            Rectangle middleFrameArea = LaserMiddleTexture.Frame(1, Main.projFrames[Projectile.type], 0, middleFrame);
            Rectangle endFrameArea = LaserEndTexture.Frame(1, Main.projFrames[Projectile.type], 0, endFrame);

            Main.EntitySpriteDraw(LaserBeginTexture, segmentStart - Main.screenPosition, startFrameArea, beamColor, Projectile.rotation, LaserBeginTexture.Size() / 2f, scale, SpriteEffects.None, 0);

            float laserBodyLength = LaserLength;
            laserBodyLength -= (startFrameArea.Height / 2 + endFrameArea.Height) * scale;
            Vector2 centerOnLaser = segmentStart;
            centerOnLaser += Projectile.velocity * scale * startFrameArea.Height / 2f;

            if (laserBodyLength > 0f)
            {
                float laserOffset = middleFrameArea.Height * scale;
                float incrementalBodyLength = 0f;

                while (incrementalBodyLength + 1f < laserBodyLength)
                {
                    Main.EntitySpriteDraw(LaserMiddleTexture, centerOnLaser - Main.screenPosition, middleFrameArea, beamColor, Projectile.rotation, LaserMiddleTexture.Width * 0.5f * Vector2.UnitX, scale, SpriteEffects.None, 0);
                    incrementalBodyLength += laserOffset;
                    centerOnLaser += Projectile.velocity * laserOffset;
                }
            }

            if (Math.Abs(LaserLength - DetermineLaserLength()) < 30f)
            {
                Vector2 laserEndCenter = centerOnLaser - Main.screenPosition;
                Main.EntitySpriteDraw(LaserEndTexture, laserEndCenter, endFrameArea, beamColor, Projectile.rotation, LaserEndTexture.Frame(1, 1, 0, 0).Top(), scale, SpriteEffects.None, 0);
            }

            bool spriteBatchSwitched = false;

            if (Glowing)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
                spriteBatchSwitched = true;

                Texture2D glow = ModContent.Request<Texture2D>(LegacyGlowTexturePath, AssetRequestMode.ImmediateLoad).Value;
                Color glowcolor = LightCastColor;
                float glowscale = Projectile.scale;

                Main.EntitySpriteDraw(glow, centerOnLaser - Main.screenPosition, middleFrameArea, glowcolor, Projectile.rotation, glow.Size() / 2, glowscale, SpriteEffects.None, 0);
            }

            if (UseFlowShader)
            {
                BeginShaderPass();
                spriteBatchSwitched = true;
                ApplyFlowShader();
                DrawShaderMiddleTiles(segmentStart, scale * FlowScaleMultiplier, middleFrameArea, FlowColor);
            }

            if (UseGlowShader)
            {
                BeginShaderPass();
                spriteBatchSwitched = true;
                ApplyGlowShader();
                DrawShaderMiddleTiles(segmentStart, scale * GlowScaleMultiplier, middleFrameArea, GlowColor);
            }

            if (spriteBatchSwitched)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            }
        }
        private static void BeginShaderPass()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
        }
        private void ApplyFlowShader()
        {
            if (!GameShaders.Misc.TryGetValue(FlowShaderKey, out MiscShaderData shader))
                return;

            shader
                .UseColor(FlowColor.ToVector3())
                .UseSecondaryColor(FlowSecondaryColor.ToVector3())
                .UseOpacity(FlowOpacity)
                .UseSaturation(FlowIntensity)
                .UseShaderSpecificData(new Vector4(FlowSpeed, FlowPatternScale, 0f, 0f));
            shader.Apply();
        }
        private void ApplyGlowShader()
        {
            if (!GameShaders.Misc.TryGetValue(GlowShaderKey, out MiscShaderData shader))
                return;

            shader
                .UseColor(GlowColor.ToVector3())
                .UseOpacity(GlowOpacity)
                .UseSaturation(GlowIntensity)
                .UseShaderSpecificData(new Vector4(GlowPulseSpeed, GlowPulseAmount, GlowFalloff, 0f));
            shader.Apply();
        }
        private void DrawShaderMiddleTiles(Vector2 segmentStart, float drawScale, Rectangle middleFrameArea, Color tint)
        {
            Rectangle startFrameArea = LaserBeginTexture.Frame(1, Main.projFrames[Projectile.type], 0, 0);
            Rectangle endFrameArea = LaserEndTexture.Frame(1, Main.projFrames[Projectile.type], 0, 0);

            float laserBodyLength = LaserLength - (startFrameArea.Height / 2 + endFrameArea.Height) * drawScale;
            Vector2 centerOnLaser = segmentStart + Projectile.velocity * drawScale * startFrameArea.Height / 2f;

            if (laserBodyLength <= 0f)
                return;

            float laserOffset = middleFrameArea.Height * drawScale;
            if (laserOffset <= 0f)
                return;

            float incrementalBodyLength = 0f;
            
            while (incrementalBodyLength + 1f < laserBodyLength)
            {
                Main.EntitySpriteDraw(LaserMiddleTexture, centerOnLaser - Main.screenPosition, middleFrameArea, tint, Projectile.rotation, LaserMiddleTexture.Width * 0.5f * Vector2.UnitX, drawScale, SpriteEffects.None, 0);
                incrementalBodyLength += laserOffset;
                centerOnLaser += Projectile.velocity * laserOffset;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CanRicochet && CurrentRicochet > 0)
            {
                for (int i = 0; i <= CurrentRicochet; i++)
                {
                    Vector2 segmentStart = RicochetPoints[i];
                    Vector2 segmentEnd;
                    if (i == CurrentRicochet)
                        segmentEnd = segmentStart + Projectile.velocity * LaserLength;
                    else
                        segmentEnd = RicochetPoints[i + 1];
                    float ColP = 0f;
                    if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), segmentStart, segmentEnd, Projectile.Size.Length() * Projectile.scale, ref ColP))
                    {
                        return true;
                    }
                }
                return false;
            }
            if (projHitbox.Intersects(targetHitbox))
                return true;
            float ColPP = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Projectile.Size.Length() * Projectile.scale, ref ColPP);
        }
        #endregion
        #region Hook Overrides
        public override void SetStaticDefaults()
        {
            EnsureShadersRegistered();
        }
        public override void AI()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
            
            Behavior();
            ExtraBehavior();
        }
        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackMelee;
            if (CanRicochet && CurrentRicochet > 0)
            {
                for (int i = 0; i <= CurrentRicochet; i++)
                {
                    Vector2 segmentStart = RicochetPoints[i];
                    Vector2 segmentEnd;

                    if (i == CurrentRicochet)
                        segmentEnd = segmentStart + Projectile.velocity * LaserLength;
                    else
                        segmentEnd = RicochetPoints[i + 1];

                    Terraria.Utils.PlotTileLine(segmentStart, segmentEnd, Projectile.Size.Length() * Projectile.scale, DelegateMethods.CutTiles);
                }
            }
            else
            {
                Terraria.Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * LaserLength, Projectile.Size.Length() * Projectile.scale, DelegateMethods.CutTiles);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
                return false;
                
            EnsureShadersRegistered();
            DrawBeamWithColor(LaserOverlayColor, Projectile.scale);
            return false;
        }
        public override bool ShouldUpdatePosition() => false;
        #endregion
        #region Overridable Properties
        public abstract float Lifetime { get; }
        public abstract float MaxScale { get; }
        public abstract float MaxLaserLength { get; }
        public abstract bool Glowing { get; }
        public abstract Texture2D LaserBeginTexture { get; }
        public abstract Texture2D LaserMiddleTexture { get; }
        public abstract Texture2D LaserEndTexture { get; }
        public virtual float ScaleExpandRate => 4f;
        public virtual Color LightCastColor => Color.White;
        public virtual Color LaserOverlayColor => Color.White * 0.9f;
        public virtual bool CanRicochet => false;
        public virtual string LegacyGlowTexturePath => "Jarkos/textures/glow_laser";
        public virtual bool UseFlowShader => false;
        public virtual string FlowShaderKey => $"Jarkos:LaserFlow:{GetType().FullName}";
        public virtual string FlowShaderPath => "Jarkos/Shaders/LaserFlow";
        public virtual string FlowShaderPass => "Pass1";
        public virtual float FlowSpeed => 0.06f;
        public virtual float FlowPatternScale => 1.0f;
        public virtual float FlowOpacity => 1.0f;
        public virtual float FlowIntensity => 1.0f;
        public virtual float FlowScaleMultiplier => 1.0f;
        public virtual Color FlowColor => new Color(255, 180, 60);
        public virtual Color FlowSecondaryColor => new Color(180, 40, 0);
        public virtual bool UseGlowShader => false;
        public virtual string GlowShaderKey => $"Jarkos:LaserGlow:{GetType().FullName}";
        public virtual string GlowShaderPath => "Jarkos/Shaders/LaserGlow";
        public virtual string GlowShaderPass => "Pass1";
        public virtual Color GlowColor => LightCastColor == Color.Transparent ? Color.White : LightCastColor;
        public virtual float GlowOpacity => 0.8f;
        public virtual float GlowIntensity => 1.0f;
        public virtual float GlowScaleMultiplier => 1.8f;
        public virtual float GlowPulseSpeed => 0.15f;
        public virtual float GlowPulseAmount => 0.15f;
        public virtual float GlowFalloff => 3.0f;
        #endregion
    }
}
