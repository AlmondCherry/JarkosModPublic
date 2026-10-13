using System;
using System.Collections.Generic;
using jarkos.items.weapons.melee.true_melee;
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
	public class Hemostasis_bolt : ModProjectile {
		private const int AbsoluteMaxTrail = 60;
        private Vector2[] oldPositions = new Vector2[AbsoluteMaxTrail];
        private float[] oldRotations = new float[AbsoluteMaxTrail];
        private bool[] oldPosActive = new bool[AbsoluteMaxTrail];
        public float ProjectileScale
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = AbsoluteMaxTrail;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            float scale = Math.Max(ProjectileScale, 0.5f);
            for (int i = AbsoluteMaxTrail - 1; i > 0; i--)
            {
                oldPositions[i] = oldPositions[i - 1];
                oldRotations[i] = oldRotations[i - 1];
                oldPosActive[i] = oldPosActive[i - 1];
            }
            oldPositions[0] = Projectile.Center;
            oldRotations[0] = Projectile.rotation;
            oldPosActive[0] = true;
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position - new Vector2(4), Projectile.width + 8, Projectile.height + 8, DustID.Blood, 0, 0, 150, default, scale * 0.8f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f);
            }
            if (Main.rand.NextBool(4))
            {
                Dust drip = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, 0, 2f, 100, default, scale * 0.5f);
                drip.noGravity = false;
            }
            float homingRange = 200f;
            NPC closest = null;
            float closestDist = homingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            if (closest != null)
            {
                Vector2 desired = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.03f);
            }
        }
        private int GetTrailLength()
        {
            float speed = Projectile.velocity.Length();
            int length = (int)(speed * 2.5f);
            return Math.Clamp(length, 5, AbsoluteMaxTrail);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            DrawTrail(sb);
            DrawMainProjectile(sb);
            return false;
        }
        private void DrawTrail(SpriteBatch sb)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int trailLength = GetTrailLength();
            float scale = Math.Max(ProjectileScale, 0.5f);
            int activeCount = 0;
            for (int i = 0; i < trailLength; i++)
            {
                if (!oldPosActive[i]) break;
                activeCount++;
            }
            if (activeCount < 2) return;
            for (int i = activeCount - 1; i > 0; i--)
            {
                Vector2 currentPos = oldPositions[i] - Main.screenPosition;
                Vector2 nextPos = oldPositions[i - 1] - Main.screenPosition;
                Vector2 diff = nextPos - currentPos;
                float segmentLength = diff.Length();
                if (segmentLength < 0.5f) continue;
                float rotation = diff.ToRotation();
                float progress = (float)i / activeCount;
                float baseWidth = 14f * scale;
                float width = MathHelper.Lerp(baseWidth, 0.5f, progress);
                float alpha = MathHelper.Lerp(0.85f, 0f, progress * progress);
                Color outerGlow = new Color(120, 0, 10, 0) * alpha * 0.35f;
                float outerWidth = width * 3.2f;
                sb.Draw(pixel, currentPos, new Rectangle(0, 0, 1, 1), outerGlow, rotation, new Vector2(0f, 0.5f), new Vector2(segmentLength, outerWidth), SpriteEffects.None, 0f);
                Color midColor = Color.Lerp(new Color(220, 15, 15, 0), new Color(100, 0, 25, 0), progress) * alpha * 0.55f;
                float midWidth = width * 1.8f;
                sb.Draw(pixel, currentPos, new Rectangle(0, 0, 1, 1), midColor, rotation, new Vector2(0f, 0.5f), new Vector2(segmentLength, midWidth), SpriteEffects.None, 0f);
                Color coreColor = Color.Lerp(new Color(255, 170, 170, 0), new Color(220, 40, 40, 0), progress) * alpha * 0.9f;
                float coreWidth = width * 0.7f;
                sb.Draw(pixel, currentPos, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0f, 0.5f), new Vector2(segmentLength, coreWidth), SpriteEffects.None, 0f);
                if (progress < 0.5f)
                {
                    Color whiteCore = new Color(255, 230, 230, 0) * alpha * (1f - progress * 2f) * 0.7f;
                    float whiteWidth = width * 0.3f;
                    sb.Draw(pixel, currentPos, new Rectangle(0, 0, 1, 1), whiteCore, rotation, new Vector2(0f, 0.5f), new Vector2(segmentLength, whiteWidth), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawMainProjectile(SpriteBatch sb)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float scale = Math.Max(ProjectileScale, 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.25f + 0.75f;
            float outerSize = 32f * scale * pulse;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(150, 0, 0, 0) * 0.25f, Projectile.rotation, new Vector2(0.5f), outerSize, SpriteEffects.None, 0f);
            float midSize = 20f * scale;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(230, 20, 20, 0) * 0.5f, Projectile.rotation + MathHelper.PiOver4, new Vector2(0.5f), midSize, SpriteEffects.None, 0f);
            float coreSize = 12f * scale;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(255, 160, 160, 0) * 0.85f, Projectile.rotation, new Vector2(0.5f), coreSize, SpriteEffects.None, 0f);
            float whiteSize = 6f * scale * pulse;
            sb.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), Color.White * 0.8f * pulse, 0f, new Vector2(0.5f), whiteSize, SpriteEffects.None, 0f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath21 with { Volume = 1f, Pitch = -0.5f }, Projectile.Center);
            float scale = Math.Max(ProjectileScale, 0.5f);
             for (int i = 0; i < (int)(40 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f) * scale;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, vel.X, vel.Y, 100, default, Main.rand.NextFloat(1f, 2.5f) * scale);
                dust.noGravity = Main.rand.NextBool();
            }
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) * scale;
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3), scale * 0.5f);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
}
