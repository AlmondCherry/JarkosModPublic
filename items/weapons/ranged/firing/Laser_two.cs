using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using jarkos;
using Jarkos.projectiles.mecha;
using jarkos.items.materials;

namespace Jarkos.items.weapons.ranged.firing
{
	public class Laser_two : ModItem
	{
		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 20;
			Item.width = 82;
			Item.height = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 20;
            Item.damage = 42;
			Item.UseSound = new SoundStyle?(Jarkos_sounds.laser_mkI_shoot);
			Item.scale = 0.9f;
            Item.autoReuse = true;
			Item.shoot = ProjectileType<Laser_two_proj>();
			Item.shootSpeed = 20f;
			Item.noMelee = true;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
		public override Vector2? HoldoutOffset() {
			return new Vector2(-15f, 0f);
		}
		public override void AddRecipes() 
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Laser_one>(), 1)
			.AddIngredient(ModContent.ItemType<EssentialPowder>(), 2)
			.AddIngredient(ItemID.SoulofNight, 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
