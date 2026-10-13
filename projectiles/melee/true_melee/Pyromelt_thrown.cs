using System;
using jarkos.dusts;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class Pyromelt_thrown : ModProjectile {
		private const int FrameCount = 4;
        private const int FrameSpeed = 2;

        private Vector2 targetPos;
        private Vector2 startPos;
        private float totalDist;
        private int flightTime = 20;
        private int timer = 0;
        private bool initialized = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FrameCount;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                startPos = Projectile.Center;
                targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                if (targetPos == Vector2.Zero)
                {
                    targetPos = startPos + new Vector2(Main.player[Projectile.owner].direction * 200f, 0);
                }

                totalDist = Vector2.Distance(startPos, targetPos);
                if (totalDist < 60f)
                {
                    Vector2 dir = (targetPos - startPos);
                    if (dir.Length() == 0) dir = new Vector2(Main.player[Projectile.owner].direction, 0);
                    dir.Normalize();
                    targetPos = startPos + dir * 60f;
                    totalDist = 60f;
                }

                flightTime = Math.Max(8, (int)(totalDist / 22f));
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FrameCount)
                    Projectile.frame = 0;
            }

            timer++;
            float progress = MathHelper.Clamp((float)timer / flightTime, 0f, 1f);

            
            Projectile.Center = Vector2.Lerp(startPos, targetPos, progress);
            Projectile.velocity = (targetPos - startPos).SafeNormalize(Vector2.UnitX) * 22f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            
            Projectile.alpha = (int)(255 * progress);

            
            float remainRatio = 1f - progress;
            if (remainRatio > 0.05f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(16f, 16f);
                    Vector2 dustVel = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);
                    Dust dust = Dust.NewDustDirect(dustPos - new Vector2(2), 4, 4, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 2.2f * remainRatio);
                    dust.noGravity = true;
                }

                if (Main.rand.NextBool(2))
                {
                    Vector2 dustVel = -Projectile.velocity * 0.2f;
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(4), 8, 8, DustID.SolarFlare, dustVel.X, dustVel.Y, 100, default, 1.8f * remainRatio);
                    d.noGravity = true;
                }

                Lighting.AddLight(Projectile.Center, 2.2f * remainRatio, 1.1f * remainRatio, 0.2f * remainRatio);
            }

            
            if (timer >= flightTime)
            {
                SpawnNuclearOrb();
                Projectile.Kill();
            }
        }

        private void SpawnNuclearOrb()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    targetPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<Pyromelt_nuclear_orb>(),
                    Projectile.damage * 2,
                    Projectile.knockBack * 4f,
                    Projectile.owner
                );
            }
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.3f, Volume = 1.4f }, targetPos);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / FrameCount;
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2, frameHeight / 2);

            float opacity = (255 - Projectile.alpha) / 255f;
            if (opacity <= 0.01f) return false;

            
            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailOp = (1f - (float)i / Projectile.oldPos.Length) * opacity;
                Color trailColor = new Color(255, 120, 20, 0) * trailOp * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Main.EntitySpriteDraw(texture, trailPos, sourceRect, trailColor, Projectile.oldRot[i], origin, Projectile.scale * opacity, SpriteEffects.None, 0);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color glowColor = new Color(255, 180, 50, 0) * opacity;
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, glowColor * 0.4f, Projectile.rotation, origin, Projectile.scale * 1.2f * opacity, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor * opacity, Projectile.rotation, origin, Projectile.scale * opacity, SpriteEffects.None, 0);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
}
