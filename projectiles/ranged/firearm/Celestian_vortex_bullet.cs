using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using System.Collections.Generic;
namespace jarkos.projectiles.ranged.firearm
{
	public class Celestian_vortex_bullet : ModProjectile
	{
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.light = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        private float rotationAngle = 0f;
        public override void AI()
        {
            Projectile.rotation += 0.3f;
            rotationAngle += 0.15f;
            Projectile.ai[0]++;
            float pulse = (float)Math.Sin(Projectile.ai[0] * 0.1f) * 0.3f + 1f;
            Lighting.AddLight(Projectile.Center, 0.8f * pulse, 0.4f * pulse, 1.5f * pulse);
            for (int i = 0; i < 3; i++)
            {
                float angle = rotationAngle + i * MathHelper.TwoPi / 3f;
                float radius = 15f + (float)Math.Sin(Projectile.ai[0] * 0.2f + i) * 5f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust dust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.BlueTorch, 0, 0, 0, new Color(120, 200, 255), 1.5f);
                dust.noGravity = true;
                dust.velocity = offset.RotatedBy(MathHelper.PiOver2) * 0.05f;
            }
            for (int i = 0; i < 2; i++)
            {
                float angle = -rotationAngle * 1.5f + i * MathHelper.Pi;
                float radius = 20f + (float)Math.Cos(Projectile.ai[0] * 0.15f) * 8f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust dust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.PurpleTorch, 0, 0, 0, new Color(200, 100, 255), 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust center = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.VilePowder, 0, 0, 100, new Color(180, 220, 255), 0.8f);
                center.noGravity = true;
                center.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
            NPC target = FindClosestNPC(500f);
            if (target != null && Projectile.ai[0] > 10)
            {
                Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero)
                    * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.03f);
            }
        }
        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            DrawTrailLayer(spriteBatch, pixel, new Color(100, 20, 200, 0), 0.2f, 22f, 3f, 0f);
            DrawTrailLayer(spriteBatch, pixel, new Color(50, 150, 255, 0), 0.4f, 12f, 2f, 0f);
            DrawTrailLayer(spriteBatch, pixel, new Color(200, 230, 255, 0), 0.7f, 5f, 1f, 0f);
            DrawWavyTrail(spriteBatch, pixel, new Color(140, 80, 255, 0), 0.35f, 4f, 8f, 0.3f);
            DrawWavyTrail(spriteBatch, pixel, new Color(80, 200, 255, 0), 0.35f, 4f, 8f, -0.3f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float glowPulse = (float)Math.Sin(Projectile.ai[0] * 0.12f) * 0.3f + 0.8f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(100, 40, 200, 0) * 0.15f * glowPulse, Projectile.rotation, new Vector2(0.5f), new Vector2(60f) * glowPulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(80, 140, 255, 0) * 0.3f * glowPulse, Projectile.rotation, new Vector2(0.5f), new Vector2(35f) * glowPulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(150, 200, 255, 0) * 0.6f * glowPulse, Projectile.rotation, new Vector2(0.5f), new Vector2(18f) * glowPulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), new Color(240, 245, 255, 0) * 0.9f, Projectile.rotation, new Vector2(0.5f), new Vector2(8f), SpriteEffects.None, 0f);
            for (int i = 0; i < 6; i++)
            {
                float angle = rotationAngle + i * MathHelper.TwoPi / 6f;
                float rayLength = 25f + (float)Math.Sin(Projectile.ai[0] * 0.2f + i * 1.2f) * 10f;
                Vector2 rayDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 rayEnd = drawPos + rayDir * rayLength;
                Color rayColor = Color.Lerp(new Color(120, 200, 255, 0), new Color(200, 100, 255, 0), (float)Math.Sin(angle + Projectile.ai[0] * 0.05f) * 0.5f + 0.5f) * 0.5f;
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), rayColor, angle, new Vector2(0, 0.5f), new Vector2(rayLength, 3f), SpriteEffects.None, 0f);
            }
            return false;
        }
        private void DrawTrailLayer(SpriteBatch spriteBatch, Texture2D texture, Color color, float alphaMultiplier, float startWidth, float endWidth, float waveAmount)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color segColor = color * (1f - progress) * alphaMultiplier;
                float width = MathHelper.Lerp(startWidth, endWidth, progress);
                Vector2 pos1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 pos2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = pos2 - pos1;
                float length = direction.Length();
                if (length == 0) continue;
                float rotation = direction.ToRotation();
                spriteBatch.Draw(texture, pos1, new Rectangle(0, 0, 1, 1), segColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
            }
        }
        private void DrawWavyTrail(SpriteBatch spriteBatch, Texture2D texture, Color color, float alphaMultiplier, float width, float waveAmp, float waveSpeed)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color segColor = color * (1f - progress) * alphaMultiplier;
                float w = MathHelper.Lerp(width, 1f, progress);
                float wave = (float)Math.Sin(progress * 15f + Projectile.ai[0] * waveSpeed) * waveAmp * (1f - progress);
                Vector2 pos1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 pos2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 dir = (pos2 - pos1).SafeNormalize(Vector2.Zero);
                Vector2 perp = dir.RotatedBy(MathHelper.PiOver2) * wave;
                pos1 += perp;
                Vector2 direction = pos2 - pos1;
                float length = direction.Length();
                if (length == 0) continue;
                float rotation = direction.ToRotation();
                spriteBatch.Draw(texture, pos1, new Rectangle(0, 0, 1, 1), segColor, rotation, new Vector2(0, 0.5f), new Vector2(length, w), SpriteEffects.None, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 60; i++)
            {
                float angle = i * MathHelper.TwoPi / 60f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 10f);
                int dustType = Main.rand.NextBool() ? DustID.BlueTorch : DustID.PurpleTorch;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.VilePowder, vel.X, vel.Y, 100, new Color(180, 220, 255), 1.8f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, vel.X, vel.Y, 0, new Color(200, 200, 255), 1f);
                dust.noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                int shardCount = 8;
                for (int i = 0; i < shardCount; i++)
                {
                    float angle = i * MathHelper.TwoPi / shardCount;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Celestian_shard_bullet>(), Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && npc.whoAmI != target.whoAmI)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < 150f)
                    {
                        Player player = Main.player[Projectile.owner];
                        int aoeDamage = (int)(damageDone * 0.5f);
                        player.ApplyDamageToNPC(npc, aoeDamage, 2f, 0, false);
                    }
                }
            }
        }
    }
}
