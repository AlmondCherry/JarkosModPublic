using jarkos.systems;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class DevilWingsSwingProj : ModProjectile {
        public override string Texture => "jarkos/items/weapons/melee/true_melee/Fiend_wings";
        private const int SwingTime = 26;
        private const int FadeTime = 22;
        private const float ArcDegrees = 250f;
        private const int MaxTrailPoints = 30;
        private float BladeLength => 74f * Projectile.scale;
        private readonly List<Vector2> tipPoints = new();
        private bool releasedShots;
        private bool initialized;
        private float baseAngle;
        private int swingDirSign;
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private int SwingDir => Projectile.ai[1] >= 0f ? 1 : -1;
        private Player Owner => Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = SwingTime + FadeTime;
            Projectile.scale = 1.1f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.35f, Volume = 0.9f }, Projectile.Center);
            }
        }
        private static float EaseOutQuart(float t) => 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t);
        public override void AI()
        {
            Player player = Owner;
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            if (!initialized)
            {
                initialized = true;
                baseAngle = Projectile.velocity.ToRotation();
                int dir = Projectile.velocity.X >= 0f ? 1 : -1;
                swingDirSign = SwingDir * dir;
            }
            Projectile.Center = player.MountedCenter;
            Timer++;
            float progress = Math.Clamp(Timer / SwingTime, 0f, 1f);
            float eased = EaseOutQuart(progress);
            float arc = MathHelper.ToRadians(ArcDegrees);
            float angle = baseAngle - swingDirSign * arc * 0.5f + swingDirSign * arc * eased;
            Projectile.rotation = angle;
            Vector2 tip = Projectile.Center + angle.ToRotationVector2() * BladeLength;
            if (Timer <= SwingTime)
            {
                player.heldProj = Projectile.whoAmI;
                player.itemTime = 2;
                player.itemAnimation = 2;
                player.ChangeDir(Projectile.velocity.X >= 0f ? 1 : -1);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle - MathHelper.PiOver2);
                tipPoints.Insert(0, tip);
                if (tipPoints.Count > MaxTrailPoints)
                    tipPoints.RemoveAt(tipPoints.Count - 1);
                SpawnBladeParticles(angle, tip, eased);
            }
            else
            {
                Projectile.friendly = false;
                if (tipPoints.Count > 0)
                    tipPoints.RemoveAt(tipPoints.Count - 1);
                if (tipPoints.Count > 1)
                    tipPoints.RemoveAt(tipPoints.Count - 1);
            }
            if (!releasedShots && progress >= 0.45f)
            {
                releasedShots = true;
                ReleaseHomingShots(tip, angle);
            }
            Lighting.AddLight(tip, 0.85f, 0.35f, 0.25f);
        }
        private void SpawnBladeParticles(float angle, Vector2 tip, float eased)
        {
            Vector2 along = angle.ToRotationVector2();
            int sparkCount = Timer < SwingTime * 0.5f ? 3 : 1;
            for (int i = 0; i < sparkCount; i++)
            {
                float t = Main.rand.NextFloat(0.45f, 1f);
                Vector2 pos = Projectile.Center + along * BladeLength * t;
                Vector2 vel = along.RotatedBy(MathHelper.PiOver2 * swingDirSign) * Main.rand.NextFloat(2f, 6f) * (1.4f - eased)
                              + Main.rand.NextVector2Circular(1f, 1f);
                Color c = Main.rand.NextBool(3) ? new Color(255, 200, 90) : new Color(255, 70, 70);
                new Spark(pos, vel, c, Main.rand.Next(18, 30), Main.rand.NextFloat(0.5f, 0.9f), 0.9f).Spawn();
            }
            if (Timer % 3 == 0)
                new Line_glow(tip, Vector2.Zero, new Color(255, 120, 100), 12, 0.45f, 0.7f, 0.1f).Spawn();
        }
        private void ReleaseHomingShots(Vector2 tip, float angle)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item9 with { Pitch = 0.3f, Volume = 0.8f }, tip);
                new Circular_glow(tip, Vector2.Zero, new Color(255, 215, 90), 14, 0.5f, 0.85f).Spawn();
                for (int i = 0; i < 4; i++)
                {
                    new StarParticle(tip, Main.rand.NextVector2Circular(3f, 3f), new Color(255, 230, 130), Main.rand.Next(14, 22), Main.rand.NextFloat(0.4f, 0.7f), 0.9f).Spawn();
                }
            }
            if (Main.myPlayer != Projectile.owner)
                return;
            int type = ModContent.ProjectileType<Fiend_wings_proj>();
            int damage = (int)(Projectile.damage * 0.5f);
            for (int i = 0; i < 4; i++)
            {
                float spread = MathHelper.Lerp(-0.55f, 0.55f, i / 3f) * swingDirSign;
                Vector2 vel = angle.ToRotationVector2().RotatedBy(spread) * Main.rand.NextFloat(7.5f, 9f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, vel, type, damage, 2f, Projectile.owner, 0f, i * MathHelper.PiOver2);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer > SwingTime)
                return false;
            Vector2 tip = Projectile.Center + Projectile.rotation.ToRotationVector2() * BladeLength;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, tip, 30f * Projectile.scale, ref collisionPoint);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCHit3 with { Volume = 0.6f }, target.Center);
                for (int i = 0; i < 6; i++)
                {
                    new Spark(target.Center, Main.rand.NextVector2Circular(5f, 5f), new Color(255, 90, 70), Main.rand.Next(16, 26), Main.rand.NextFloat(0.5f, 0.9f), 0.9f).Spawn();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTipTrail();
            DrawSword(lightColor);
            return false;
        }
        private void DrawTipTrail()
        {
            if (tipPoints.Count < 2)
                return;
            float fade = Timer <= SwingTime
                ? 1f
                : MathHelper.Clamp(1f - (Timer - SwingTime) / FadeTime, 0f, 1f);
            Effect effect = DevilShaders.AirSlice;
            Main.spriteBatch.End();
            if (effect != null)
            {
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                effect.Parameters["uFade"]?.SetValue(fade);
                effect.Parameters["uColor"]?.SetValue(new Color(215, 225, 255).ToVector4());
                TrailRenderer.Draw(effect, tipPoints, t => MathHelper.Lerp(36f, 6f, t) * Projectile.scale, t => Color.White * (1f - t), DevilShaders.Noise);
            }
            else if (DevilShaders.PointedTrail != null)
            {
                Effect fb = DevilShaders.PointedTrail;
                fb.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                fb.Parameters["uColor"]?.SetValue(new Color(200, 210, 255).ToVector4());
                fb.Parameters["uColorCore"]?.SetValue(Color.White.ToVector4());
                TrailRenderer.Draw(fb, tipPoints, t => MathHelper.Lerp(36f, 6f, t) * Projectile.scale, t => Color.White * fade * (1f - t), DevilShaders.Noise);
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawSword(Color lightColor)
        {
            if (Timer > SwingTime + 2)
                return;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            bool flip = Projectile.velocity.X < 0f;
            SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float rotation = flip
                ? Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2
                : Projectile.rotation + MathHelper.PiOver4;
            Vector2 origin = flip
                ? new Vector2(texture.Width, texture.Height)
                : new Vector2(0f, texture.Height);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, rotation, origin, Projectile.scale, effects, 0);
        }
    }
}
