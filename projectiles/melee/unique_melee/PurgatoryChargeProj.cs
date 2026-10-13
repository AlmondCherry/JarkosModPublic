using System;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee
{
    public class PurgatoryChargeProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Misc/Perlin";
        private const int MaxLifetime = 60 * 8;
        private bool _readyPingPlayed;
        private bool _released;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxLifetime;
            Projectile.netImportant = true;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (_released || !player.active || player.dead || player.HeldItem?.ModItem is not PurgatorySpear)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = player.Center;
            Projectile.velocity = Vector2.Zero;
            player.itemAnimation = 2;
            player.itemTime = 2;
            int chargeTime = (int)Projectile.ai[0];
            chargeTime++;
            Projectile.ai[0] = chargeTime;
            DrawChargeVisuals(player, chargeTime);
            if (!_readyPingPlayed && chargeTime >= PurgatorySpear.ArrowRainChargeFrames)
            {
                _readyPingPlayed = true;
                if (player.whoAmI == Main.myPlayer)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
                }
            }
            if (player.whoAmI == Main.myPlayer && !Main.mouseRight)
            {
                Release(player, chargeTime);
                return;
            }
            if (chargeTime >= MaxLifetime - 2)
            {
                Release(player, chargeTime);
            }
        }
        private void DrawChargeVisuals(Player player, int chargeTime)
        {
            float pulse = 0.5f + 0.5f * MathF.Sin(chargeTime * 0.25f);
            int auraDustCount = chargeTime >= PurgatorySpear.ArrowRainChargeFrames ? 3 : 1;
            for (int i = 0; i < auraDustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 28f + pulse * 8f;
                Vector2 pos = player.Center + angle.ToRotationVector2() * radius;
                int dustType = chargeTime >= PurgatorySpear.ArrowRainChargeFrames
                    ? DustID.GoldFlame
                    : DustID.Enchanted_Pink;
                Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, Scale: 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
            if (player.whoAmI == Main.myPlayer && chargeTime % 3 == 0)
            {
                float t = MathHelper.Clamp(chargeTime / (float)PurgatorySpear.ArrowRainChargeFrames, 0f, 1f);
                float ringRadius = 24f + t * 96f;
                int dustType = t >= 1f ? DustID.GoldFlame : DustID.Enchanted_Pink;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * (i / 6f) + chargeTime * 0.05f;
                    Vector2 pos = Main.MouseWorld + angle.ToRotationVector2() * ringRadius;
                    Dust dust = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, Scale: 1f);
                    dust.noGravity = true;
                }
            }
        }
        private void Release(Player player, int chargeTime)
        {
            if (_released)
            {
                return;
            }
            _released = true;
            if (player.whoAmI != Main.myPlayer)
            {
                Projectile.Kill();
                return;
            }
            var modPlayer = player.GetModPlayer<PurgatoryPlayer>();
            modPlayer.AbilityCooldown = PurgatorySpear.RightClickCooldownFrames;
            if (chargeTime >= PurgatorySpear.ArrowRainChargeFrames)
            {
                TriggerArrowRain(player);
            }
            else
            {
                TriggerDashAndStars(player);
            }
            Projectile.netUpdate = true;
            Projectile.Kill();
        }
        private void TriggerDashAndStars(Player player)
        {
            Vector2 cursor = Main.MouseWorld;
            Vector2 dir = (cursor - player.Center).SafeNormalize(new Vector2(player.direction, 0f));
            Vector2 preDashCenter = player.Center;
            const float dashSpeed = 19f;
            player.velocity = dir * dashSpeed;
            player.immune = true;
            player.immuneTime = 24;
            player.immuneNoBlink = false;
            SoundEngine.PlaySound(SoundID.Item92, player.Center);
            SoundEngine.PlaySound(SoundID.NPCDeath59, player.Center);
            if (Main.netMode != NetmodeID.Server)
            {
                int pulseType = ModContent.ProjectileType<PurgatoryDashPulseProj>();
                Projectile.NewProjectile(new EntitySource_Parent(Projectile), preDashCenter, Vector2.Zero, pulseType, 0, 0f, player.whoAmI);
            }
            int starCount = 12;
            int starType = ModContent.ProjectileType<PurgatoryStarProj>();
            int starDamage = (int)(Projectile.damage * 0.9f);
            float starKb = 2f;
            var source = new EntitySource_Parent(Projectile);
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * (i / (float)starCount);
                Vector2 vel = angle.ToRotationVector2() * 6f;
                Projectile.NewProjectile(source, player.Center, vel, starType, starDamage, starKb, player.whoAmI);
            }
            for (int i = 0; i < 28; i++)
            {
                Dust dust = Dust.NewDustPerfect(player.Center, DustID.GoldFlame, -dir.RotatedByRandom(0.55f) * Main.rand.NextFloat(2.5f, 7f), Scale: Main.rand.NextFloat(1.3f, 1.8f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 14; i++)
            {
                Vector2 pos = player.Center + dir * Main.rand.NextFloat(0f, 80f);
                Dust streak = Dust.NewDustPerfect(pos, DustID.YellowStarDust, dir * Main.rand.NextFloat(2f, 5f), Scale: Main.rand.NextFloat(1.0f, 1.6f));
                streak.noGravity = true;
            }
            player.GetModPlayer<PurgatoryPlayer>().AddShake(7.5f, 18);
        }
        private void TriggerArrowRain(Player player)
        {
            Vector2 cursor = Main.MouseWorld;
            int rainType = ModContent.ProjectileType<PurgatoryArrowRainProj>();
            int rainDamage = Projectile.damage;
            var modPlayer = player.GetModPlayer<PurgatoryPlayer>();
            modPlayer.ArrowRainHealedThisCast = 0;
            var source = new EntitySource_Parent(Projectile);
            Projectile.NewProjectile(source, cursor, Vector2.Zero, rainType, rainDamage, 4f, player.whoAmI);
            SoundEngine.PlaySound(SoundID.Item105, cursor);
            for (int i = 0; i < 32; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust dust = Dust.NewDustPerfect(cursor, DustID.GoldFlame, vel, Scale: Main.rand.NextFloat(1.4f, 2.0f));
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            modPlayer.AddShake(5f, 14);
        }
    }
}
