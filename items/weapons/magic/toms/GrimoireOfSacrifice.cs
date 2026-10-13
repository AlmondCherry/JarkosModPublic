using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.toms;
using jarkos.items.usetiles;
using Jarkos.items.materials;

namespace Jarkos.items.weapons.magic.toms
{
	public class GrimoireOfSacrifice : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Magic;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item43;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<GrimoireOfSacrifice_laser>();
            Item.shootSpeed = 1f;
            Item.mana = 15;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 20, 0);
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SpellTome, 1);
            recipe.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8);
            recipe.AddIngredient(ModContent.ItemType<ResidualShadow>(), 6);
            recipe.AddTile(TileID.Bookcases);
            recipe.Register();
        }
    }
}
