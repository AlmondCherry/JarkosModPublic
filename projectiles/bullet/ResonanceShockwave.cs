using System;
using jarkos.buffs.debuff;
using jarkos.particles;
using jarkos.projectiles.mecha;
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
namespace jarkos.projectiles.bullet
{
    public class ResonanceShockwave : ModProjectile
    {
        private float currentRadius = 0f;
        private float maxRadius = 350f;
        private float expansionSpeed = 8f;
        private float shockwaveAlpha = 1f;
        private bool[] hitNPCs = new bool[Main.maxNPCs];
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            float intensity = Projectile.ai[0];
            if (intensity <= 0) intensity = 1f;
            maxRadius = 350f * intensity;
            currentRadius += expansionSpeed;
            expansionSpeed *= 0.96f;
            shockwaveAlpha = 1f - (currentRadius / maxRadius);
            shockwaveAlpha = Math.Max(shockwaveAlpha, 0f);
            if (currentRadius >= maxRadius || shockwaveAlpha <= 0.01f)
            {
                Projectile.Kill();
                return;
            }
            CheckNPCCollision(intensity);
            SpawnRingParticles();
        }
        private void CheckNPCCollision(float intensity)
        {
            Player owner = Main.player[Projectile.owner];
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (hitNPCs[i]) continue;
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist >= currentRadius - 30 && dist <= currentRadius + 15)
                {
                    hitNPCs[i] = true;
                    if (Main.myPlayer == Projectile.owner)
                    {
                        int direction = (npc.Center.X > Projectile.Center.X) ? 1 : -1;
                        npc.SimpleStrikeNPC(Projectile.damage, direction, false, 0f, null, false, 0, true);
                    }
                    npc.AddBuff(ModContent.BuffType<ResonanceDebuff>(), 480);
                    for (int d = 0; d < 10; d++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldFlame, 0, 0, 100, default, 1.3f);
                        dust.noGravity = true;
                        dust.velocity = Main.rand.NextVector2Circular(4, 4);
                    }
                }
            }
        }
        private void SpawnRingParticles()
        {
            if (shockwaveAlpha < 0.1f) return;
            int particleCount = (int)(12 * shockwaveAlpha);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle) * currentRadius, (float)Math.Sin(angle) * currentRadius);
                Dust dust = Dust.NewDustDirect(pos, 2, 2, DustID.GoldCoin, 0, 0, 100, default, 1.2f * shockwaveAlpha);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
                dust.fadeIn = 1f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (currentRadius < 5f) return false;
            Main.spriteBatch.End();
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float scale = currentRadius * 2f / texture.Width;
            Color ringColor = new Color(255, 200, 50) * shockwaveAlpha * 0.6f;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(texture, center, null, ringColor, 0f, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0);
            float innerScale = scale * 0.85f;
            Color innerColor = new Color(255, 255, 150) * shockwaveAlpha * 0.4f;
            Main.EntitySpriteDraw(texture, center, null, innerColor, 0f, new Vector2(texture.Width / 2, texture.Height / 2), innerScale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
    }
}
