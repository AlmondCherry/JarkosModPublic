using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
	public class Red_nova_proj : ModProjectile {
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<float> trailRotations = new List<float>();
        private const int MaxTrailLength = 35;
        private static readonly Color PrimaryColor = new Color(255, 100, 255);
        private static readonly Color SecondaryColor = new Color(120, 50, 220);
        private static readonly Color CoreColor = new Color(255, 220, 255);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        public override void AI()
        {
            UpdateTrail();
            Projectile.rotation += 0.15f * Math.Sign(Projectile.velocity.X);
            SpawnParticles();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.3f + 0.8f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.4f, 1.0f) * pulse);
            ApplyHoming();
            if (Main.rand.NextBool(60))
            {
                SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.2f, Pitch = 0.8f }, Projectile.Center);
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            trailRotations.Insert(0, Projectile.velocity.ToRotation());
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailRotations.RemoveAt(trailRotations.Count - 1);
            }
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, -Projectile.velocity.X * 0.3f, -Projectile.velocity.Y * 0.3f, 100, default, 1.8f);
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), 0, 0, DustID.PinkFairy, 0f, 0f, 150, default, 0.7f);
                Spark.noGravity = true;
                Spark.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Spark.fadeIn = 1.2f;
            }
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(12, 12);
                Dust star = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.RainbowMk2, -offset.X * 0.1f, -offset.Y * 0.1f, 0, new Color(255, 150, 255), 0.9f);
                star.noGravity = true;
            }
        }
        private void ApplyHoming()
        {
            float homingRange = 250f;
            float homingStrength = 0.04f;
            NPC target = FindClosestNPC(homingRange);
            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                float currentSpeed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * currentSpeed, homingStrength);
            }
        }
        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
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
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawTrail(spriteBatch, pixel);
            DrawOuterGlow(spriteBatch, pixel);
            DrawCore(spriteBatch, pixel);
            DrawOrbitalParticles(spriteBatch, pixel);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            if (trailPositions.Count < 2) return;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                Vector2 start = trailPositions[i] - Main.screenPosition;
                Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * 0.9f;
                float width = MathHelper.Lerp(14f, 2f, progress);
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, progress) * alpha;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                if (length > 0.5f)
                {
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.4f, rotation, new Vector2(0, 0.5f), new Vector2(length, width + 8f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.7f, rotation, new Vector2(0, 0.5f), new Vector2(length, width + 3f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), CoreColor * alpha, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.4f), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawOuterGlow(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float time = Main.GameUpdateCount * 0.1f;
            for (int i = 0; i < 6; i++)
            {
                float layerScale = 30f + i * 12f + (float)Math.Sin(time + i) * 4f;
                float layerAlpha = 0.12f - i * 0.018f;
                Color glowColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / 6f) * layerAlpha;
                spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), glowColor, 0f, new Vector2(0.5f, 0.5f), layerScale, SpriteEffects.None, 0f);
            }
        }
        private void DrawCore(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f);
            float coreScale = 16f + pulse * 3f;
            spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), PrimaryColor * 0.8f, 0f, new Vector2(0.5f, 0.5f), coreScale + 6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(255, 200, 255), 0f, new Vector2(0.5f, 0.5f), coreScale + 2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), coreScale, SpriteEffects.None, 0f);
        }
        private void DrawOrbitalParticles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float time = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 4; i++)
            {
                float angle = time + i * MathHelper.PiOver2;
                float radius = 20f + (float)Math.Sin(time * 2 + i) * 5f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                spriteBatch.Draw(pixel, Projectile.Center + offset - Main.screenPosition, new Rectangle(0, 0, 1, 1), CoreColor * 0.9f, 0f, new Vector2(0.5f, 0.5f), 6f, SpriteEffects.None, 0f);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                int shardCount = Main.rand.Next(2, 5);
                for (int i = 0; i < shardCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(7f, 7f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, velocity, ModContent.ProjectileType<NovaSparkProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
                }
            }
            SoundEngine.PlaySound(SoundID.Item25 with { Volume = 0.7f, Pitch = 0.4f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCHit5 with { Volume = 0.4f, Pitch = 0.6f }, Projectile.Center);
            CreateHitEffect(target.Center);
        }
        private void CreateHitEffect(Vector2 position)
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20 * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.PinkFairy, velocity.X, velocity.Y, 100, default, 1.6f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 12; i++)
            {
                Dust flash = Dust.NewDustDirect(position, 0, 0, DustID.RainbowMk2, 0f, 0f, 0, PrimaryColor, 2f);
                flash.noGravity = true;
                flash.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center);
            CreateDeathExplosion();
            if (Vector2.Distance(Main.LocalPlayer.Center, Projectile.Center) < 500f)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
            }
        }
        private void CreateDeathExplosion()
        {
            for (int i = 0; i < 40; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 2.2f);
                dust.noGravity = true;
                dust.velocity *= 1.3f;
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust star = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PinkTorch, velocity.X, velocity.Y, 150, default, 1.8f);
                star.noGravity = true;
                star.fadeIn = 1.5f;
            }
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi / 15 * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                Dust rainbow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RainbowMk2, velocity.X, velocity.Y, 0, new Color(255, 100, 255), 2.5f);
                rainbow.noGravity = true;
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(30, 30);
                Dust sparkle = Dust.NewDustDirect(pos, 0, 0, DustID.SparkForLightDisc, 0f, 0f, 0, default, 1.2f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.6f, Pitch = 0.7f }, Projectile.Center);
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PinkTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.4f);
                    dust.noGravity = true;
                }
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X * 0.85f;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y * 0.85f;
            }
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #region Nova Spark
    public class NovaSparkProjectile : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> miniTrail = new List<Vector2>();
        private const int MiniTrailLength = 15;
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            miniTrail.Insert(0, Projectile.Center);
            if (miniTrail.Count > MiniTrailLength)
                miniTrail.RemoveAt(miniTrail.Count - 1);
            Projectile.rotation += 0.3f;
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkFairy, 0f, 0f, 150, default, 0.7f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f + 0.6f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.7f, 0.3f, 0.9f) * pulse);
            Projectile.velocity *= 0.98f;
            Projectile.velocity.Y += 0.05f;
            NPC target = FindClosestNPC(150f);
            if (target != null)
            {
                Vector2 dir = target.Center - Projectile.Center;
                dir.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 6f, 0.06f);
            }
        }
        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
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
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (miniTrail.Count > 1)
            {
                for (int i = 0; i < miniTrail.Count - 1; i++)
                {
                    Vector2 start = miniTrail[i] - Main.screenPosition;
                    Vector2 end = miniTrail[i + 1] - Main.screenPosition;
                    float progress = (float)i / miniTrail.Count;
                    float alpha = 1f - progress;
                    float width = (1f - progress) * 6f;
                    Color trailColor = new Color(255, 150, 255) * alpha * 0.6f;
                    Vector2 direction = end - start;
                    float length = direction.Length();
                    float rotation = direction.ToRotation();
                    if (length > 0)
                    {
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                    }
                }
            }
            float glowSize = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 3f;
            spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(200, 100, 255) * 0.4f, 0f, new Vector2(0.5f, 0.5f), glowSize + 8f, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(255, 180, 255) * 0.7f, 0f, new Vector2(0.5f, 0.5f), glowSize + 3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), glowSize, SpriteEffects.None, 0f);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item25 with { Volume = 0.4f, Pitch = 0.8f }, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 1f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PinkTorch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.2f);
                dust.noGravity = true;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    #endregion
}
