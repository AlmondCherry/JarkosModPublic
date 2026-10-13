using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Jarkos.projectiles.summon.staff
{
	public class HomingTurretRocket : ModProjectile
	{
        private const float TURN_SPEED = 0.08f;
        private const float MAX_SPEED = 16f;
        private const float ACCELERATION = 0.3f;
        private float wavePhase = 0f;
        private float waveAmplitude = 3f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MAX_TRAIL_LENGTH = 15;
        private int targetNPC = -1;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                targetNPC = (int)Projectile.ai[0];
                Projectile.localAI[0] = 1;
                wavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            UpdateTrail();
            UpdateTarget();
            HomeTowardsTarget();
            ApplyWaveMotion();
            SpawnEffects();
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 0.2f) * 0.5f);
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MAX_TRAIL_LENGTH)
                trailPositions.RemoveAt(trailPositions.Count - 1);
        }
        private void UpdateTarget()
        {
            if (targetNPC == -1 || !Main.npc[targetNPC].active || Main.npc[targetNPC].dontTakeDamage)
            {
                FindNewTarget();
            }
        }
        private void FindNewTarget()
        {
            float maxRange = 600f;
            float closestDist = maxRange;
            targetNPC = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetNPC = i;
                }
            }
        }
        private void HomeTowardsTarget()
        {
            if (targetNPC != -1 && Main.npc[targetNPC].active)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 toTarget = target.Center - Projectile.Center;
                float targetAngle = toTarget.ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                float distance = toTarget.Length();
                float adjustedTurnSpeed = TURN_SPEED * (1f + (300f - Math.Min(distance, 300f)) / 300f);
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                float newAngle = currentAngle + MathHelper.Clamp(angleDiff, -adjustedTurnSpeed, adjustedTurnSpeed);
                float speed = Projectile.velocity.Length();
                speed = MathHelper.Lerp(speed, MAX_SPEED, ACCELERATION * 0.1f);
                Projectile.velocity = newAngle.ToRotationVector2() * speed;
            }
            else
            {
                Projectile.velocity.Y += 0.05f;
            }
        }
        private void ApplyWaveMotion()
        {
            wavePhase += 0.3f;
            Vector2 perpendicular = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
            float waveOffset = (float)Math.Sin(wavePhase) * waveAmplitude * 0.5f;
            Projectile.position += perpendicular * waveOffset * 0.1f;
            if (Projectile.timeLeft < 250)
                waveAmplitude = MathHelper.Lerp(waveAmplitude, 1f, 0.02f);
        }
        private void SpawnEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustDirect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f, 4, 4, DustID.Torch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.5f);
                fire.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 15f, 6, 6, DustID.Smoke, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 150, default, 1f);
            }
            if (Main.rand.NextBool(8))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 2, 2, DustID.OrangeTorch, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 0, default, 0.8f);
                Spark.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }
        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 0) return;
            Explode();
        }
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.9f, Pitch = 0.3f }, Projectile.Center);
             for (int i = 0; i < 25; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust explosion = Dust.NewDustDirect(Projectile.Center - new Vector2(16), 32, 32, DustID.Torch, vel.X, vel.Y, 100, default, 2f);
                explosion.noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center - new Vector2(20), 40, 40, DustID.Smoke, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-6, 2), 150, default, 2f);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center - new Vector2(10), 20, 20, DustID.Electric, Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6), 0, default, 1.2f);
                Spark.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.7f, 0.3f) * 2f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("jarkos/projectiles/summon/staff/HomingTurretRocket").Value;
            if (trailPositions.Count > 1)
            {
                for (int i = 1; i < trailPositions.Count; i++)
                {
                    float progress = 1f - (float)i / trailPositions.Count;
                    Color trailColor = Color.Lerp(Color.OrangeRed, Color.Yellow, progress) * progress * 0.6f;
                    trailColor.A = 0;
                    float scale = progress * 0.8f;
                    Vector2 trailPos = trailPositions[i] - Main.screenPosition;
                    Vector2 direction = Vector2.Zero;
                    if (i > 0)
                        direction = trailPositions[i - 1] - trailPositions[i];
                    float trailRotation = direction != Vector2.Zero ? direction.ToRotation() : Projectile.rotation;
                    sb.Draw(texture, trailPos, null, trailColor, trailRotation, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 0f);
                }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = Color.White;
            Color glowColor = Color.OrangeRed * 0.5f;
            glowColor.A = 0;
            sb.Draw(texture, drawPos, null, glowColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / 2), 1.3f, SpriteEffects.None, 0f);
            sb.Draw(texture, drawPos, null, drawColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / 2), 1f, SpriteEffects.None, 0f);
            return false;
        }
	}
}
