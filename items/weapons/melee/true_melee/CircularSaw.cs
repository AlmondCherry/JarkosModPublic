using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.true_melee
{
	public class CircularSaw : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 30;
            Item.damage = 35;
            Item.knockBack = 3f;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 2;
            Item.useAnimation = 2;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Circular_saw_handle>();
            Item.shootSpeed = 1f;
            Item.UseSound = null;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }

        public override bool MeleePrefix() => true;

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.autoReuse = false;
                Item.channel = false;
                Item.shoot = ModContent.ProjectileType<Circular_saw_slash>();
                Item.shootSpeed = 12f;
            }
            else
            {
                Item.useTime = 2;
                Item.useAnimation = 2;
                Item.autoReuse = true;
                Item.channel = true;
                Item.shoot = ModContent.ProjectileType<Circular_saw_handle>();
                Item.shootSpeed = 1f;
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].owner == player.whoAmI
                    && (Main.projectile[i].type == ModContent.ProjectileType<Circular_saw_handle>()
                     || Main.projectile[i].type == ModContent.ProjectileType<Circular_saw_slash>()))
                    return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 10)
            .AddRecipeGroup(nameof(ItemID.TissueSample), 16)
            .AddRecipeGroup(nameof(ItemID.Vertebrae), 8)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}
