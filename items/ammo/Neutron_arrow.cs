using jarkos.projectiles.bullet;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Neutron_arrow : ModItem
	{
		public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 40;
            Item.damage = 22;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(copper: 25);
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.shoot = ModContent.ProjectileType<Neutron_arrow_proj>();
            Item.shootSpeed = 5f;
            Item.ammo = AmmoID.Arrow;
        }
        public override void AddRecipes()
        {
            CreateRecipe(100)
                .AddIngredient(ItemID.WoodenArrow, 100)
                .AddIngredient(ItemID.SoulofSight, 1)
                .AddIngredient(ItemID.Meteorite, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
