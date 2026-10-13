using System;
using System.Collections.Generic;
using jarkos.items.materials;
using jarkos.rare;
using Jarkos.projectiles.summon.staff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
namespace jarkos.projectiles.summon.staff
{
	public class TurretBase : ModProjectile
	{
		private bool isActivated = false;
        private float heatLevel = 0f;
        private const float MAX_HEAT = 100f;
        private const float HEAT_PER_SHOT = 10f;
        private const float COOLING_RATE = 0.1f;
        private const float OVERHEAT_COOLING_RATE = 1.2f;
        private bool isOverheated = false;
        private int shootTimer = 0;
        private const int SHOOT_DELAY = 10;
        private float barrelRotation = 0f;
        private float targetRotation = 0f;
        private float recoilOffset = 0f;
        private float glowIntensity = 0f;
        private int targetNPC = -1;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 36000;
            Projectile.DamageType = DamageClass.Summon;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Vector2 distance_to_cursor = Main.MouseWorld - Projectile.Center;
            if(distance_to_cursor.Length() < 50)
            {
                string textToDisplay = $"{heatLevel}";
                Main.instance.MouseText(textToDisplay, 0, 0, -1, -1, -1, -1);
            }
            ApplyGravityAndPosition();
            FindTarget();
            UpdateBarrelRotation();
            UpdateHeatSystem();
            if (isActivated && !isOverheated)
            {
                UpdateShooting();
            }
            UpdateAnimation();
            SpawnIdleEffects();
        }
        private void ApplyGravityAndPosition()
        {
            Projectile.velocity.Y += 0.3f;
            if (Projectile.velocity.Y > 12f)
                Projectile.velocity.Y = 12f;
            Vector2 nextPos = Projectile.position + Projectile.velocity;
            Point tileCheck = (nextPos + new Vector2(Projectile.width / 2, Projectile.height)).ToTileCoordinates();
            if (Main.tile[tileCheck.X, tileCheck.Y].HasTile && Main.tileSolid[Main.tile[tileCheck.X, tileCheck.Y].TileType])
            {
                Projectile.velocity.Y = 0;
                Projectile.position.Y = tileCheck.Y * 16 - Projectile.height;
            }
        }
        private void FindTarget()
        {
            float maxRange = 800f;
            float closestDist = maxRange;
            targetNPC = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
                {
                    closestDist = dist;
                    targetNPC = i;
                }
            }
        }
        private void UpdateBarrelRotation()
        {
            if (targetNPC != -1 && Main.npc[targetNPC].active)
            {
                Vector2 toTarget = Main.npc[targetNPC].Center - GetBarrelPosition();
                targetRotation = toTarget.ToRotation();
            }
            else
            {
                targetRotation = 0f;
            }
            float rotationDiff = MathHelper.WrapAngle(targetRotation - barrelRotation);
            barrelRotation += rotationDiff * 0.1f;
            barrelRotation = MathHelper.WrapAngle(barrelRotation);
        }
        private void UpdateHeatSystem()
        {
            if (isOverheated)
            {
                heatLevel -= OVERHEAT_COOLING_RATE;
                glowIntensity = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.3f;
                if (Main.rand.NextBool(5))
                {
                    Dust smoke = Dust.NewDustDirect(GetBarrelPosition(), 10, 10, DustID.Smoke, 0, -2f, 100, default, 1.5f);
                    smoke.noGravity = true;
                }
                if (heatLevel <= 20f)
                {
                    isOverheated = false;
                    SoundEngine.PlaySound(SoundID.Item149, Projectile.Center);
                }
            }
            else
            {
                if (heatLevel > 0)
                    heatLevel -= COOLING_RATE;
                glowIntensity = heatLevel / MAX_HEAT;
            }
            heatLevel = MathHelper.Clamp(heatLevel, 0, MAX_HEAT);
        }
        private void UpdateShooting()
        {
            shootTimer++;
            if (shootTimer >= SHOOT_DELAY && targetNPC != -1)
            {
                shootTimer = 0;
                ShootRocket();
                heatLevel += HEAT_PER_SHOT;
                if (heatLevel >= MAX_HEAT)
                {
                    isOverheated = true;
                    SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
                    for (int i = 0; i < 15; i++)
                    {
                        Dust steam = Dust.NewDustDirect(GetBarrelPosition(), 20, 20, DustID.Smoke, Main.rand.NextFloat(-3, 3), -3f, 150, default, 2f);
                        steam.noGravity = true;
                    }
                }
            }
        }
        private void ShootRocket()
        {
            Vector2 barrelPos = GetBarrelPosition();
            Vector2 direction = barrelRotation.ToRotationVector2();
            Vector2 velocity = direction * 8f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelPos + direction * 30f, velocity, ModContent.ProjectileType<HomingTurretRocket>(), Projectile.damage, Projectile.knockBack, Projectile.owner, targetNPC);
            recoilOffset = 8f;
            SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.8f, Pitch = 0.2f }, barrelPos);
            for (int i = 0; i < 10; i++)
            {
                Dust flash = Dust.NewDustDirect(barrelPos + direction * 25f, 5, 5, DustID.Torch, direction.X * 5f, direction.Y * 5f, 0, default, 2f);
                flash.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust smoke = Dust.NewDustDirect(barrelPos + direction * 20f, 10, 10, DustID.Smoke, direction.X * 2f + Main.rand.NextFloat(-1, 1), direction.Y * 2f + Main.rand.NextFloat(-1, 1), 100, default, 1.2f);
            }
        }
        private void UpdateAnimation()
        {
            if (recoilOffset > 0)
                recoilOffset *= 0.85f;
        }
        private void SpawnIdleEffects()
        {
            if (!isActivated)
            {
                if (Main.rand.NextBool(30))
                {
                    Dust idle = Dust.NewDustDirect(Projectile.Center - new Vector2(10), 20, 20, DustID.Electric, 0, 0, 100, Color.Gray, 0.5f);
                    idle.noGravity = true;
                }
            }
            else if (!isOverheated)
            {
                if (Main.rand.NextBool(10))
                {
                    Dust active = Dust.NewDustDirect(GetBarrelPosition(), 10, 10, DustID.Electric, 0, 0, 0, default, 0.8f);
                    active.noGravity = true;
                }
            }
            if (heatLevel > 50f)
            {
                Lighting.AddLight(GetBarrelPosition(), new Vector3(1f, 0.3f, 0.1f) * (heatLevel / MAX_HEAT));
            }
        }
        public Vector2 GetBarrelPosition()
        {
            return Projectile.Center + new Vector2(0, -20);
        }
        public void Activate()
        {
            if (!isActivated)
            {
                isActivated = true;
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f }, Projectile.Center);
                for (int i = 0; i < 25; i++)
                {
                    Dust activation = Dust.NewDustDirect(Projectile.Center - new Vector2(25), 50, 50, DustID.Electric, 0, 0, 0, Color.Cyan, 1.5f);
                    activation.noGravity = true;
                    activation.velocity = (activation.position - Projectile.Center).SafeNormalize(Vector2.Zero) * 3f;
                }
            }
        }
        public bool IsActivated => isActivated;
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D baseTexture = ModContent.Request<Texture2D>("jarkos/projectiles/summon/staff/TurretBase").Value;
            Texture2D barrelTexture = ModContent.Request<Texture2D>("jarkos/projectiles/summon/staff/TurretBarrel").Value;
            Vector2 baseOrigin = new Vector2(baseTexture.Width / 2, baseTexture.Height);
            Vector2 barrelOrigin = new Vector2(10, barrelTexture.Height / 2);
            Vector2 Orig = new Vector2(60, 80);
            float uiScale = Main.UIScale;
            Rectangle mouseHitbox = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);
			Rectangle TurretHitbox = Terraria.Utils.CenteredRectangle(Projectile.Center, Orig * uiScale);
            if (TurretHitbox.Intersects(mouseHitbox))
            {
				string textToDisplay = $"{heatLevel}";
                Main.instance.MouseText(textToDisplay, 0, 0, -1, -1, -1, -1);
			}
            Color drawColor = lightColor;
            if (isOverheated)
            {
                drawColor = Color.Lerp(lightColor, Color.OrangeRed, 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f);
            }
            else if (heatLevel > 50f)
            {
                drawColor = Color.Lerp(lightColor, Color.Orange, (heatLevel - 50f) / 50f * 0.4f);
            }
            if (!isActivated)
            {
                drawColor = Color.Lerp(drawColor, Color.Gray, 0.5f);
            }
            Vector2 basePos = Projectile.Bottom - Main.screenPosition;
            sb.Draw(baseTexture, basePos, null, drawColor, 0f, baseOrigin, 1f, SpriteEffects.None, 0f);
            Vector2 barrelPos = GetBarrelPosition() - Main.screenPosition;
            Vector2 recoil = barrelRotation.ToRotationVector2() * -recoilOffset;
            SpriteEffects barrelEffect = SpriteEffects.None;
            float drawRotation = barrelRotation;
            if (Math.Abs(barrelRotation) > MathHelper.PiOver2)
            {
                barrelEffect = SpriteEffects.FlipVertically;
            }
            sb.Draw(barrelTexture, barrelPos + recoil, null, drawColor, drawRotation, barrelOrigin, 1f, barrelEffect, 0f);
            if (heatLevel > 30f && !isOverheated)
            {
                Color glowColor = Color.OrangeRed * ((heatLevel - 30f) / 70f * 0.5f);
                glowColor.A = 0;
                sb.Draw(barrelTexture, barrelPos + recoil, null, glowColor, drawRotation, barrelOrigin, 1.1f, barrelEffect, 0f);
            }
            return false;
        }
        public override bool? CanDamage() => false;
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 40; i++)
            {
                Dust debris = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-6, -2), 100, default, 1.5f);
            }
            for (int i = 0; i < 20; i++)
            {
                Dust Spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 0, default, 1f);
                Spark.noGravity = true;
            }
        }
    }
}
