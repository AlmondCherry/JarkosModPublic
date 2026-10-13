using jarkos.items.materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Nature_arrow : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 40;
			Item.damage = 6;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.knockBack = 1f;
			Item.shoot = ModContent.ProjectileType<projectiles.bullet.Nature_arrow_proj>();
            Item.ammo = AmmoID.Arrow;
		}
		public override void AddRecipes() 
		{
			CreateRecipe(125)
				.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
