using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.ranged.bow;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.ranged.bows
{
	public class Death_bow : ModItem
	{
		public override void SetDefaults()
		{
            Item.width = 16;
            Item.height = 48;
            Item.damage = 28;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 2.5f;
            Item.useTime = 20;
			Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<DeathBowHoldout>();
			Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Arrow;
			Item.channel = true;
			Item.noUseGraphic = true;
            Item.UseSound = null;

            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] > 0;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<DeathBowHoldout>();
        }

        public override Vector2? HoldoutOffset() => new Vector2(-2, 0);

		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
