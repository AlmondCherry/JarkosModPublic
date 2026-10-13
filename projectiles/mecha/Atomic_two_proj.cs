using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.mecha
{
	public class Atomic_two_proj : ModProjectile
	{
        private Vector2[] oldPositions = new Vector2[20];
        private float[] oldRotations = new float[20];
        private int decayTimer = 0;
        private bool isDecaying = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            for (int i = oldPositions.Length - 1; i > 0; i--)
            {
                oldPositions[i] = oldPositions[i - 1];
                oldRotations[i] = oldRotations[i - 1];
            }
            oldPositions[0] = Projectile.Center;
            oldRotations[0] = Projectile.rotation;
            Projectile.rotation = Projectile.velocity.ToRotation();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 0.2f * pulse, 1f * pulse, 0.3f * pulse);
            decayTimer++;
            if (decayTimer % 10 == 0)
            {
                EmitDecayParticles();
            }
            if (decayTimer % 25 == 0 && decayTimer > 15)
            {
                SpawnDecayFragment();
            }
            if (Main.rand.NextBool(8))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 0, new Color(100, 255, 100), 0.8f);
                Spark.noGravity = true;
                Spark.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
            if (decayTimer > 20)
            {
                NPC target = FindClosestNPC(300f);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), 0.02f);
                }
            }
        }
        private void EmitDecayParticles()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 particleVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, particleVel, 0, default, 0.9f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        private void SpawnDecayFragment()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 fragmentVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fragmentVel, ModContent.ProjectileType<AtomicFragment>(), Projectile.damage / 3, Projectile.knockBack * 0.5f, Projectile.owner);
                SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
            }
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
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.Venom, 180);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.YellowTorch, dustVel, 0, default, 1.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit53 with { Volume = 0.5f }, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            AtomicExplosion();
        }
        private void AtomicExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = -0.3f }, Projectile.Center);
            Lighting.AddLight(Projectile.Center, 2f, 3f, 1f);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20 * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, velocity, 0, default, 2f);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, velocity, 0, default, 1.8f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.YellowTorch, velocity, 0, new Color(100, 255, 150), 1f);
                Spark.noGravity = true;
            }
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi / 5 * i;
                    Vector2 fragmentVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fragmentVel, ModContent.ProjectileType<AtomicFragment>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawTrail(spriteBatch, pixel);
            DrawCore(spriteBatch, pixel);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 1; i < oldPositions.Length; i++)
            {
                if (oldPositions[i] == Vector2.Zero || oldPositions[i - 1] == Vector2.Zero)
                    continue;
                Vector2 start = oldPositions[i - 1] - Main.screenPosition;
                Vector2 end = oldPositions[i] - Main.screenPosition;
                float progress = (float)i / oldPositions.Length;
                float alpha = 1f - progress;
                float width = MathHelper.Lerp(8f, 2f, progress);
                Color trailColor = Color.Lerp(new Color(100, 255, 100), new Color(0, 100, 50), progress) * alpha;
                DrawLine(spriteBatch, pixel, start, end, trailColor, width);
                Color glowColor = Color.Lerp(Color.White, new Color(150, 255, 150), progress) * alpha * 0.7f;
                DrawLine(spriteBatch, pixel, start, end, glowColor, width * 0.4f);
            }
        }
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            float rotation = diff.ToRotation();
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0, 0.5f);
            Vector2 scale = new Vector2(length, width);
            spriteBatch.Draw(pixel, start, sourceRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
        }
        private void DrawCore(SpriteBatch spriteBatch, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.2f + 1f;
            float size = 14f * pulse;
            Color glowOuter = new Color(50, 200, 50) * 0.3f;
            DrawCenteredRect(spriteBatch, pixel, drawPos, size * 2.5f, glowOuter);
            Color glowMid = new Color(100, 255, 100) * 0.5f;
            DrawCenteredRect(spriteBatch, pixel, drawPos, size * 1.5f, glowMid);
            Color coreColor = new Color(200, 255, 200);
            DrawCenteredRect(spriteBatch, pixel, drawPos, size, coreColor);
            Color centerColor = Color.White;
            DrawCenteredRect(spriteBatch, pixel, drawPos, size * 0.5f, centerColor);
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi / 3 * i;
                float sparkDist = size * 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + i) * 3f;
                Vector2 sparkPos = drawPos + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * sparkDist;
                Color sparkColor = new Color(150, 255, 200) * 0.8f;
                DrawCenteredRect(spriteBatch, pixel, sparkPos, 3f, sparkColor);
            }
        }
        private void DrawCenteredRect(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float size, Color color)
        {
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0.5f, 0.5f);
            Vector2 scale = new Vector2(size, size);
            spriteBatch.Draw(pixel, center, sourceRect, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
	}
}
