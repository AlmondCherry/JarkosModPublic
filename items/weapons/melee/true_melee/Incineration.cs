using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Incineration : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.damage = 20;
            Item.knockBack = 4f;
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Incineration_proj>();
            Item.shootSpeed = 14f;
            Item.UseSound = SoundID.Item1;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool CanUseItem(Player player)
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active
                    && Main.projectile[i].owner == player.whoAmI
                    && Main.projectile[i].type == ModContent.ProjectileType<Incineration_proj>())
                {
                    count++;
                }
            }
            return count < 2;
        }
        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(nameof(ItemID.IronBar), 8)
                .AddIngredient(ItemID.HellstoneBar, 8)
                .AddIngredient(ItemID.Obsidian, 25)
                .AddTile(TileID.Anvils)
                .Register();
        }
	}
}
