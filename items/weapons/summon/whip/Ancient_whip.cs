using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;
using Terraria.Audio;

namespace jarkos.items.weapons.summon.whip
{
	public class Ancient_whip : ModItem
	{
		public const int MIN_CHARGE_TICKS = 10;
        public const int MAX_CHARGE_TICKS = 120;
        public const float MIN_DAMAGE_MULT = 0.3f;
        public const float MAX_DAMAGE_MULT = 3.0f;
        public const float MIN_RANGE_MULT = 0.4f;
        public const float MAX_RANGE_MULT = 2.2f;
        private int chargeTime = 0;
        private bool isCharging = false;
        private bool maxChargeReached = false;
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 65;
            Item.knockBack = 5f;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<Ancient_whip_proj>();
            Item.shootSpeed = 4f;

            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 3, 10, 0);
        }
        public override bool MeleePrefix() => true;
        public override bool CanUseItem(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].owner == player.whoAmI
                    && Main.projectile[i].type == ModContent.ProjectileType<Ancient_whip_proj>())
                    return false;
            }
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }
        public override void HoldItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI) return;
            bool whipActive = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].owner == player.whoAmI
                    && Main.projectile[i].type == ModContent.ProjectileType<Ancient_whip_proj>())
                {
                    whipActive = true;
                    break;
                }
            }
            if (whipActive)
            {
                chargeTime = 0;
                isCharging = false;
                maxChargeReached = false;
                return;
            }
            bool mouseDown = player.channel && player.HeldItem == Item;
            if (mouseDown)
            {
                if (!isCharging)
                {
                    isCharging = true;
                    chargeTime = 0;
                    maxChargeReached = false;
                    bool indicatorActive = false;

                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<AncientNatureChargeIndicator>())
                        {
                            indicatorActive = true;
                            break;
                        }
                    }
                    if (!indicatorActive)
                    {
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<AncientNatureChargeIndicator>(), 0, 0, player.whoAmI);
                    }
                }
                chargeTime++;
                if (chargeTime > MAX_CHARGE_TICKS)
                    chargeTime = MAX_CHARGE_TICKS;
                float chargeProgress = (float)chargeTime / MAX_CHARGE_TICKS;
                player.velocity.X *= MathHelper.Lerp(1f, 0.85f, chargeProgress);

                if (Main.MouseWorld.X > player.Center.X)
                    player.direction = 1;
                else
                    player.direction = -1;
                if (chargeTime == MIN_CHARGE_TICKS)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.3f, Pitch = -0.5f }, player.Center);
                }
                if (chargeTime == MAX_CHARGE_TICKS / 2)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0f }, player.Center);
                }
                if (chargeTime == MAX_CHARGE_TICKS && !maxChargeReached)
                {
                    maxChargeReached = true;
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.7f, Pitch = 0.3f }, player.Center);
                    for (int i = 0; i < 8; i++)
                    {
                        Dust d = Dust.NewDustDirect(player.Center, 1, 1, DustID.GreenTorch);
                        d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                        d.scale = Main.rand.NextFloat(1.0f, 1.6f);
                        d.noGravity = true;
                    }
                }
                if (chargeTime > MIN_CHARGE_TICKS && chargeTime < MAX_CHARGE_TICKS
                    && Main.rand.NextBool(8))
                {
                    SpawnChargeDust(player, chargeProgress);
                }
                player.itemTime = 2;
                player.itemAnimation = 2;
            }
            else if (isCharging)
            {
                isCharging = false;
                maxChargeReached = false;
                if (chargeTime >= MIN_CHARGE_TICKS)
                {
                    FireWhip(player);
                }
                chargeTime = 0;
            }
        }
        private void FireWhip(Player player)
        {
            float chargeProgress = (float)chargeTime / MAX_CHARGE_TICKS;
            float damageMult = MathHelper.Lerp(MIN_DAMAGE_MULT, MAX_DAMAGE_MULT, chargeProgress);
            float rangeMult = MathHelper.Lerp(MIN_RANGE_MULT, MAX_RANGE_MULT, chargeProgress);

            int finalDamage = (int)(Item.damage * damageMult);

            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Vector2 velocity = direction * Item.shootSpeed;

            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, velocity, ModContent.ProjectileType<Ancient_whip_proj>(), finalDamage, Item.knockBack * MathHelper.Lerp(0.5f, 2.0f, chargeProgress), player.whoAmI, ai0: chargeProgress, ai1: rangeMult);
            SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.5f + chargeProgress * 0.5f, Pitch = -0.3f + chargeProgress * 0.4f }, player.Center);
            if (chargeProgress >= 0.8f)
            {
                SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.6f, Pitch = -0.2f }, player.Center);
            }
            int dustCount = (int)(3 + chargeProgress * 8);

            for (int i = 0; i < dustCount; i++)
            {
                Dust d = Dust.NewDustDirect(player.Center, 1, 1, chargeProgress >= 0.8f ? DustID.GreenTorch : DustID.Grass);
                d.velocity = direction * Main.rand.NextFloat(2f, 6f * (1 + chargeProgress)) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                d.scale = Main.rand.NextFloat(0.7f, 1.2f + chargeProgress * 0.5f);
                d.noGravity = true;
            }
            player.itemTime = (int)(20 + (1f - chargeProgress) * 15);
            player.itemAnimation = player.itemTime;
            chargeTime = 0;
        }
        private void SpawnChargeDust(Player player, float progress)
        {
            float radius = MathHelper.Lerp(50f, 20f, progress);
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            
            Vector2 dustPos = player.Center + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
            Dust d = Dust.NewDustDirect(dustPos, 1, 1, DustID.Grass);
            d.velocity = (player.Center - dustPos) * 0.06f;
            d.scale = 0.5f + progress * 0.5f;
            d.noGravity = true;
            d.alpha = 100;
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.SwordWhip, 1)
			.AddIngredient(ItemID.ChlorophyteBar, 12)
			.AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
