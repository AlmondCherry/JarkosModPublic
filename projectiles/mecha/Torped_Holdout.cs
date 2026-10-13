using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.items.weapons.ranged.firing;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace jarkos.projectiles.mecha
{
    public class Torped_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Torped";
        public override float MuzzleForwardOffset => 100f;
        public override float MuzzleVerticalOffset => -5f;
        public override int FirstShotDelay => 30;
        public override Vector2 HoldoutOrigin => new Vector2(24f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.netImportant = true;
        }
        public override void AI()
        {
            Player player = Owner;
            if (player.dead || !player.active || !player.channel || player.noItems || player.CCed)
            {
                var modPlayer = player.GetModPlayer<TorpedoCannonPlayer>();
                modPlayer.IsActive = false;
                if (!modPlayer.IsOverheated)
                {
                    if (modPlayer.BurstCounter > 0 && Main.GameUpdateCount % 10 == 0)
                    {
                        modPlayer.BurstCounter--;
                    }
                    modPlayer.FireTimer = 0;
                }
                else
                {
                    modPlayer.CooldownTimer--;
                    if (modPlayer.CooldownTimer <= 0)
                    {
                        modPlayer.IsOverheated = false;
                        modPlayer.BurstCounter = 0;
                    }
                }
                Projectile.Kill();
                return;
            }
            Projectile.timeLeft = 2;
            Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter);
            Projectile.Center = mountedCenter;
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 aimDirection = GetAimDirection(player, mountedCenter);
                if (Vector2.DistanceSquared(Projectile.velocity, aimDirection) > 0.001f || Main.GameUpdateCount % 10 == 0)
                {
                    Projectile.velocity = aimDirection;
                    Projectile.netUpdate = true;
                }
                Projectile.direction = aimDirection.X > 0 ? 1 : -1;
            }
            else
            {
                Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            }
            Projectile.spriteDirection = Projectile.direction;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            var torpPlayer = player.GetModPlayer<TorpedoCannonPlayer>();
            if (player.channel && Main.mouseLeft)
            {
                torpPlayer.IsActive = true;
                if (torpPlayer.IsOverheated)
                {
                    torpPlayer.CooldownTimer--;
                    if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                    {
                        Vector2 gunTip = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                        Dust smoke = Dust.NewDustPerfect(gunTip, DustID.Smoke, new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f)), 150, default, 1.5f);
                        smoke.noGravity = true;
                    }
                    float heatGlow = (float)torpPlayer.CooldownTimer / torpPlayer.MaxCooldown;
                    Lighting.AddLight(player.Center, 0.6f * heatGlow, 0.2f * heatGlow, 0.1f);
                    if (torpPlayer.CooldownTimer == torpPlayer.MaxCooldown - 1 && Main.netMode != NetmodeID.Server)
                    {
                        SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.3f }, player.Center);
                    }
                    if (torpPlayer.CooldownTimer <= 0)
                    {
                        torpPlayer.IsOverheated = false;
                        torpPlayer.BurstCounter = 0;
                        torpPlayer.FireTimer = 0;
                        if (Main.netMode != NetmodeID.Server)
                        {
                            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.2f }, player.Center);
                            for (int i = 0; i < 8; i++)
                            {
                                Vector2 gunTip = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                                Dust ready = Dust.NewDustPerfect(gunTip, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, new Color(100, 200, 255), 1f);
                                ready.noGravity = true;
                            }
                        }
                    }
                }
                else
                {
                    torpPlayer.FireTimer++;
                    if (torpPlayer.FireTimer >= torpPlayer.FireRate)
                    {
                        torpPlayer.FireTimer = 0;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            FireRocket(player, Projectile.velocity);
                        }
                        torpPlayer.BurstCounter++;
                        if (torpPlayer.BurstCounter >= torpPlayer.BurstSize)
                        {
                            torpPlayer.IsOverheated = true;
                            torpPlayer.CooldownTimer = torpPlayer.MaxCooldown;
                            if (Main.netMode != NetmodeID.Server)
                            {
                                for (int i = 0; i < 20; i++)
                                {
                                    Vector2 gunTip = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                                    Dust steam = Dust.NewDustPerfect(gunTip, DustID.Cloud, new Vector2(Main.rand.NextFloat(-2f, 2f) + Projectile.direction * 2f, Main.rand.NextFloat(-4f, -1f)), 200, default, Main.rand.NextFloat(1.5f, 2.5f));
                                    steam.noGravity = true;
                                }
                            }
                        }
                    }
                    float pulse = 0.3f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
                    Lighting.AddLight(player.Center, 0.1f, 0.4f * pulse, 0.7f * pulse);
                }
            }
            recoil *= RecoilDecay;
        }
        private void FireRocket(Player player, Vector2 aimDirection)
        {
            float shootSpeed = player.HeldItem.shootSpeed;
            Vector2 velocity = aimDirection * shootSpeed;
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(6));
            Vector2 spawnPos = Projectile.Center + aimDirection * MuzzleForwardOffset + aimDirection.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
            int damage = player.GetWeaponDamage(player.HeldItem);
            float knockback = player.HeldItem.knockBack;
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocity, ModContent.ProjectileType<Torpedo>(), damage, knockback, player.whoAmI);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item92 with
                {
                    Pitch = Main.rand.NextFloat(-0.15f, 0.15f), Volume = 0.8f
                }, player.Center);
                for (int i = 0; i < 3; i++)
                {
                    Dust smoke = Dust.NewDustPerfect(spawnPos, DustID.Smoke, aimDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat(1f, 3f), 100, default, 0.8f);
                    smoke.noGravity = true;
                }
            }
            recoil = 12f;
            player.velocity -= aimDirection * 1.2f;
        }
    }
}
