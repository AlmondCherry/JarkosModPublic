using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using jarkos;
using Jarkos.projectiles.mecha;
using Jarkos.items.usetiles;

namespace Jarkos.items.weapons.ranged.firing
{
	public class Laser_one : ModItem
	{
		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 21;
			Item.width = 82;
			Item.height = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 21;
            Item.damage = 14;
			Item.UseSound = new SoundStyle?(Jarkos_sounds.laser_mkI_shoot);
			Item.scale = 0.9f;
            Item.autoReuse = true;
			Item.shoot = ProjectileType<Laser_one_proj>();
			Item.shootSpeed = 24f;
			Item.noMelee = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
		 public override Vector2? HoldoutOffset() {
			return new Vector2(-15f, 0f);
		}
		public override void AddRecipes() 
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<SteelBar>(), 8)
			.AddIngredient(ItemID.Ruby, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
