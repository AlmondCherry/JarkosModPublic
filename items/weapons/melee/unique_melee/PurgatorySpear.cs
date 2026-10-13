using jarkos.items.materials;
using jarkos.projectiles.melee.unique_melee;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.melee.unique_melee
{
    public class PurgatorySpear : ModItem
    {
        public const int ArrowRainChargeFrames = 120;
        public const int RightClickCooldownFrames = 60 * 60;
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.scale = 1.1f;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.autoReuse = true;
            Item.UseSound = null;
            Item.damage = 493;
            Item.knockBack = 4.5f;
            Item.crit = 6;
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<PurgatorySwingProj>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;

            Item.rare = ModContent.RarityType<rare.CosmicRarity>();
            Item.value = Item.sellPrice(2, 0, 20, 0);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<PurgatoryRaiseProj>()] > 0)
            {
                return false;
            }

            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<PurgatoryPlayer>();
                if (modPlayer.AbilityCooldown > 0)
                {
                    return false;
                }
                if (player.ownedProjectileCounts[ModContent.ProjectileType<PurgatorySanctuaryProj>()] > 0)
                {
                    return false;
                }
            }
            return base.CanUseItem(player);
        }
        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                return 2f;
            }
            return 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<PurgatoryPlayer>();
            if (player.altFunctionUse == 2)
            {
                modPlayer.AbilityCooldown = RightClickCooldownFrames;
                if (player.whoAmI == Main.myPlayer)
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/PurgatoryAuraSound") with { Volume = 1f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, player.Center);
                    Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<PurgatorySanctuaryProj>(), 0, 0f, player.whoAmI);
                    Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<PurgatoryRaiseProj>(), 0, 0f, player.whoAmI);
                }
                return false;
            }

            int variation = modPlayer.SwingVariation;
            modPlayer.SwingVariation = (variation + 1) % 3;
            Vector2 aim = (Main.MouseWorld - player.Center).SafeNormalize(new Vector2(player.direction, 0f));

            if (player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(source, player.Center, aim, ModContent.ProjectileType<PurgatorySwingProj>(), damage, knockback, player.whoAmI, variation);
            }
            return false;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentSolar, 15)
                .AddIngredient(ModContent.ItemType<CelestiaStar>(), 2)
                .AddIngredient(ModContent.ItemType<NitoziumPlate>(), 1)
                .AddIngredient(ItemID.LunarBar, 5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
