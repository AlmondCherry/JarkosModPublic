using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using jarkos.items.tools;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.other_melee
{
    public class KharonHammer : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 1208;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 68;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.shootSpeed = 16f;
            Item.shoot = ModContent.ProjectileType<Kharon_hammerproj>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;

            Item.rare = ModContent.RarityType<rare.SpeedRarity>();
            Item.value = Item.sellPrice(2, 0, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<Kharon_hammerproj>()] == 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 launchVelocity = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitX) * 16f;
            Projectile.NewProjectile(source, position, launchVelocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<AntiqueHammer>(), 1)
            .AddIngredient(ItemID.LunarHamaxeSolar, 1)
            .AddIngredient(ModContent.ItemType<NitoziumPlate>(), 2)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
        }
    }
}
