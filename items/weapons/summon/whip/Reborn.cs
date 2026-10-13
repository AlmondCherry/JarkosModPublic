using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;
namespace jarkos.items.weapons.summon.whip
{
	public class Reborn : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 35;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 2;
            Item.useAnimation = 2;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 5f;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<Reborn_proj>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<Reborn_proj>()] < 1;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ManaCrystal, 2);
            recipe.AddIngredient(ModContent.ItemType<ElementalPowder>(), 2);
            recipe.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
}
