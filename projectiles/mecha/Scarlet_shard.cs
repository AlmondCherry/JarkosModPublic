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
using Jarkos.UI;
namespace jarkos.projectiles.mecha
{
	public class Scarlet_shard : ModProjectile
	{
		private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 15;
        private float rotate_speed = Main.rand.NextFloat(-0.1f, 0.1f);
        private ref float HomingDelay => ref Projectile.ai[0];
        private ref float TargetIndex => ref Projectile.ai[1];
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 50;
            Projectile.light = 0.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            Projectile.rotation += rotate_speed;
            HomingDelay++;
            if (HomingDelay > 10)
            {
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.12f);
                    if (Projectile.velocity.Length() < 12f)
                    {
                        Projectile.velocity *= 1.02f;
                    }
                }
            }
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, Projectile.velocity * -0.1f, 50, default, 0.8f);
                dust.noGravity = true;
            }
            Projectile.alpha = 50 + (int)(Math.Sin(HomingDelay * 0.3f) * 30);
        }
        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item25 with { Volume = 0.4f, Pitch = 0.8f }, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, vel, 0, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawCore();
            return false;
        }
        private void DrawTrail()
        {
            if (trailPositions.Count < 2) return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.6f;
                float width = MathHelper.Lerp(5f, 1f, progress);
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Color trailColor = Color.Lerp(new Color(255, 100, 150), new Color(200, 50, 80), progress) * alpha;
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 scale = new Vector2(length, width);
                Main.spriteBatch.Draw(pixel, start, sourceRect, trailColor, rotation, new Vector2(0, 0.5f), scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawCore()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Texture2D starTex = ModContent.Request<Texture2D>("jarkos/textures/StarTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 starOrigin = starTex.Size() / 2f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.15f;
            float rotation = Projectile.rotation + MathHelper.PiOver2;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Color glowColor = new Color(255, 20, 20) * 0.40f;
            spriteBatch.Draw(glowTex, drawPos, null, glowColor, 0f, glowOrigin, Main.rand.NextFloat(0.25f, 0.45f) * pulse, SpriteEffects.None, 0f);
            Color starColor = new Color(255, 120, 150) * 0.95f;
            spriteBatch.Draw(starTex, drawPos, null, starColor, rotation, starOrigin, Main.rand.NextFloat(0.05f, 0.15f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(starTex, drawPos, null, Color.White * 0.9f, rotation, starOrigin, Main.rand.NextFloat(0.01f, 0.08f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 150, 150, 150);
        }
	}
}
