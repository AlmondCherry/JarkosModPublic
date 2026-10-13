using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using jarkos.particles;
using ReLogic.Content;
using Terraria.DataStructures;
namespace jarkos.projectiles.enemy.punisher
{
	public class MegaBomb : ModProjectile
	{
        private ref float BombType => ref Projectile.ai[0];
        private ref float FuseTimer => ref Projectile.ai[1];
        private Vector2 targetPredictedPosition;
        private Vector2 lastPlayerVelocity;
        private float homingStrength = 0f;
        private bool isTracking = true;
        private int targetPlayerIndex = -1;
        private float ExplosionRadius => BombType == 1 ? 500f : 300f;
        private float TrackingDuration => BombType == 1 ? 90f : 60f;
        private float MaxHomingStrength => BombType == 1 ? 0.15f : 0.1f;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Initialize();
                Projectile.localAI[0] = 1;
            }
            FuseTimer--;
            if (isTracking && FuseTimer > (Projectile.ai[1] - TrackingDuration))
            {
                SmartTracking();
            }
            else
            {
                ApplyGravity();
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            CreateTrailEffects();
            CreateFuseEffects();
            HandleBlinking();
            if (FuseTimer <= 0)
            {
                Explode();
            }
             }
        private void Initialize()
        {
            float closestDist = float.MaxValue;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead) continue;
                float dist = Vector2.Distance(Projectile.Center, player.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    targetPlayerIndex = i;
                }
            }
            if (targetPlayerIndex >= 0)
            {
                Player target = Main.player[targetPlayerIndex];
                lastPlayerVelocity = target.velocity;
                targetPredictedPosition = PredictPlayerPosition(target, 45);
            }
            if (BombType == 1)
            {
                Projectile.scale = 1.5f;
                Projectile.width = 60;
                Projectile.height = 60;
            }
            SoundEngine.PlaySound(SoundID.Item61, Projectile.Center);
        }
        private void SmartTracking()
        {
            if (targetPlayerIndex < 0 || targetPlayerIndex >= Main.maxPlayers) return;
            Player target = Main.player[targetPlayerIndex];
            if (!target.active || target.dead)
            {
                isTracking = false;
                return;
            }
            Vector2 playerAcceleration = target.velocity - lastPlayerVelocity;
            lastPlayerVelocity = target.velocity;
            float distanceToPlayer = Vector2.Distance(Projectile.Center, target.Center);
            float projectileSpeed = Projectile.velocity.Length();
            float timeToReach = projectileSpeed > 0 ? distanceToPlayer / projectileSpeed : 30f;
            timeToReach = MathHelper.Clamp(timeToReach, 15f, 60f);
            targetPredictedPosition = PredictPlayerPositionAdvanced(target, playerAcceleration, (int)timeToReach);
            Vector2 directionToTarget = targetPredictedPosition - Projectile.Center;
            float distanceToPredicted = directionToTarget.Length();
            if (distanceToPredicted > 10f)
            {
                directionToTarget.Normalize();
                homingStrength = MathHelper.Lerp(homingStrength, MaxHomingStrength, 0.05f);
                Vector2 currentDirection = Projectile.velocity;
                currentDirection.Normalize();
                Vector2 newDirection = Vector2.Lerp(currentDirection, directionToTarget, homingStrength);
                newDirection.Normalize();
                float currentSpeed = Projectile.velocity.Length();
                float targetSpeed = BombType == 1 ? 14f : 12f;
                float newSpeed = MathHelper.Lerp(currentSpeed, targetSpeed, 0.02f);
                Projectile.velocity = newDirection * newSpeed;
            }
            if (playerAcceleration.Length() > 0.5f)
            {
                homingStrength = Math.Min(homingStrength + 0.02f, MaxHomingStrength * 1.5f);
            }
            Vector2 toPlayer = target.Center - Projectile.Center;
            float dotProduct = Vector2.Dot(Vector2.Normalize(Projectile.velocity), Vector2.Normalize(toPlayer));
            if (dotProduct < 0.3f && distanceToPlayer < 200f)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(toPlayer) * Projectile.velocity.Length(), 0.1f);
            }
            Projectile.velocity.Y += 0.03f;
        }
        private Vector2 PredictPlayerPosition(Player player, int ticks)
        {
            Vector2 predictedPos = player.Center;
            Vector2 velocity = player.velocity;
            for (int i = 0; i < ticks; i++)
            {
                predictedPos += velocity;
                if (velocity.Y < 15f && player.velocity.Y != 0)
                {
                    velocity.Y += 0.3f;
                }
            }
            return predictedPos;
        }
        private Vector2 PredictPlayerPositionAdvanced(Player player, Vector2 acceleration, int ticks)
        {
            Vector2 predictedPos = player.Center;
            Vector2 velocity = player.velocity;
            float horizontalBias = 0f;
            if (player.controlLeft) horizontalBias = -0.3f;
            if (player.controlRight) horizontalBias = 0.3f;
            float verticalBias = 0f;
            if (player.controlJump && player.velocity.Y != 0) verticalBias = -12f;
            if (player.controlDown) verticalBias = 0.5f;
            for (int i = 0; i < ticks; i++)
            {
                velocity.X += horizontalBias * 0.5f;
                velocity.X += acceleration.X * 0.3f;
                velocity.X = MathHelper.Clamp(velocity.X, -15f, 15f);
                if (i == 0 && verticalBias < 0)
                {
                    velocity.Y = verticalBias;
                }
                else if (player.velocity.Y != 0 || velocity.Y < 0)
                {
                    velocity.Y += 0.35f;
                    velocity.Y = Math.Min(velocity.Y, 16f);
                }
                predictedPos += velocity;
                acceleration *= 0.95f;
            }
            if (player.grappling[0] >= 0)
            {
                Projectile hook = Main.projectile[player.grappling[0]];
                if (hook.active)
                {
                    predictedPos = Vector2.Lerp(predictedPos, hook.Center, 0.5f);
                }
            }
            if (player.wingTime > 0 && player.controlJump)
            {
                predictedPos.Y -= ticks * 0.5f;
            }
            return predictedPos;
        }
        private void ApplyGravity()
        {
            isTracking = false;
            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 18f)
                Projectile.velocity.Y = 18f;
            Projectile.velocity.X *= 0.99f;
        }
        private void CreateTrailEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - Projectile.velocity, 0, 0, DustID.Torch, -Projectile.velocity.X * 0.3f, -Projectile.velocity.Y * 0.3f, 100, default, BombType == 1 ? 2.5f : 1.8f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 2, 0, 0, DustID.Smoke, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 150, default, 1.2f);
                smoke.noGravity = true;
            }
            if (BombType == 1 && Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.InfernoFork, 0f, 0f, 100, default, 1.5f);
                fire.noGravity = true;
            }
        }
        private void CreateFuseEffects()
        {
            Vector2 fusePos = Projectile.Center + new Vector2(0, -Projectile.height / 2 * Projectile.scale)
                .RotatedBy(Projectile.rotation - MathHelper.PiOver2);
            if (Main.rand.NextBool(2))
            {
                Dust Spark = Dust.NewDustDirect(fusePos, 0, 0, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f), 100, default, 1.5f);
                Spark.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 1f, 0.5f, 0.2f);
            Lighting.AddLight(fusePos, 1f, 0.8f, 0.3f);
        }
        private void HandleBlinking()
        {
            if (FuseTimer < 90)
            {
                int blinkRate;
                if (FuseTimer < 20) blinkRate = 2;
                else if (FuseTimer < 40) blinkRate = 4;
                else if (FuseTimer < 60) blinkRate = 6;
                else blinkRate = 10;
                Projectile.alpha = ((int)FuseTimer % blinkRate < blinkRate / 2) ? 100 : 0;
                if (FuseTimer < 40 && Main.rand.NextBool(3))
                {
                    Dust warning = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 2f);
                    warning.noGravity = true;
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.4f;
            }
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.4f;
            }
            isTracking = false;
            if (oldVelocity.Length() > 5f)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            }
            return false;
        }
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            if (BombType == 1)
            {
                SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            }
            float radius = ExplosionRadius;
            foreach (Player player in Main.player)
            {
                if (!player.active || player.dead) continue;
                float distance = Vector2.Distance(player.Center, Projectile.Center);
                if (distance < radius)
                {
                    float damageMultiplier = 1f - distance / radius * 0.6f;
                    int damage = (int)(Projectile.damage * damageMultiplier);
                    Vector2 knockbackDir = player.Center - Projectile.Center;
                    if (knockbackDir != Vector2.Zero)
                    {
                        knockbackDir.Normalize();
                        float knockbackPower = (BombType == 1 ? 20f : 12f) * damageMultiplier;
                        player.velocity += knockbackDir * knockbackPower;
                    }
                    player.Hurt(PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI), damage, knockbackDir.X > 0 ? 1 : -1);
                    player.AddBuff(BuffID.OnFire, 300);
                    if (BombType == 1)
                    {
                        player.AddBuff(BuffID.Burning, 180);
                    }
                }
            }
            int fireParticles = BombType == 1 ? 120 : 60;
            for (int i = 0; i < fireParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / fireParticles;
                float speed = Main.rand.NextFloat(8f, radius / 15f);
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(2f, 4f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 50; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(15f, 15f);
                Dust core = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.InfernoFork, velocity.X, velocity.Y, 50, default, 3f);
                core.noGravity = true;
            }
            int smokeParticles = BombType == 1 ? 80 : 40;
            for (int i = 0; i < smokeParticles; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(radius / 20f, radius / 20f);
                Dust smoke = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), 0, 0, DustID.Smoke, velocity.X, velocity.Y, 150, default, Main.rand.NextFloat(2f, 4f));
            }
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = radius * (0.3f + ring * 0.3f);
                int ringParticles = (int)(ringRadius / 5f);
                for (int i = 0; i < ringParticles; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringParticles;
                    Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ringRadius;
                    Dust ringDust = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, 0f, 0f, 100, default, 2f);
                    ringDust.noGravity = true;
                    ringDust.velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 3f;
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active) continue;
                    float dist = Vector2.Distance(player.Center, Projectile.Center);
                    if (dist < radius * 2f)
                    {
                        player.GetModPlayer<Screenshake_system>().stronger_shake++;
                    }
                }
            }
            Lighting.AddLight(Projectile.Center, 3f, 2f, 0.5f);
            Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("jarkos/textures/warning_punisher", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 origin2 = texture2.Size() / 2f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.OrangeRed * (1f - progress) * 0.5f;
                trailColor.A = 0;
                float trailScale = Projectile.scale * (1f - progress * 0.5f);
                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }
            float pulse = 1f + MathF.Sin(FuseTimer * 0.2f) * 0.15f;
            Color glowColor;
            if (FuseTimer < 60)
            {
                glowColor = Color.Red * 0.7f;
            }
            else
            {
                glowColor = Color.OrangeRed * 0.5f;
            }
            glowColor.A = 0;
            Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin2, Projectile.scale * 3.85f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * pulse * 1.4f, SpriteEffects.None, 0f);
            Color mainColor = Projectile.alpha > 0 ? Color.White * (1f - Projectile.alpha / 255f) : Color.White;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            if (isTracking && Main.netMode != NetmodeID.Server)
            {
                Vector2 indicatorDir = Vector2.Normalize(targetPredictedPosition - Projectile.Center);
                Vector2 indicatorPos = Projectile.Center + indicatorDir * 30f - Main.screenPosition;
                Dust indicator = Dust.NewDustDirect(Projectile.Center + indicatorDir * 40f, 0, 0, DustID.YellowTorch, 0f, 0f, 100, default, 0.8f);
                indicator.noGravity = true;
                indicator.velocity = Vector2.Zero;
            }
            return true;
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
