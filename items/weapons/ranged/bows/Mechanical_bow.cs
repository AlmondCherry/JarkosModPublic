using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.ranged.bow;
using Terraria.Audio;
using jarkos.items.materials;

namespace jarkos.items.weapons.ranged.bows
{
	public class Mechanical_bow : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.useTime = 2;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 76;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 6;
			Item.reuseDelay = 5;
			Item.damage = 11;
			Item.UseSound = new SoundStyle("jarkos/sounds/electro_bow_sound");
			Item.autoReuse = true;
			Item.scale = 0.80f;
			Item.noMelee = true;
			Item.channel = true;
			Item.shoot = ModContent.ProjectileType<Mechanical_bow_swing>();
			Item.shootSpeed = 16f;
			Item.useAmmo = AmmoID.Arrow;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 40, 0);
		}
		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[Item.shoot] <= 0;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return player.ownedProjectileCounts[Item.shoot] > 0;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = ModContent.ProjectileType<Mechanical_bow_swing>();
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.WoodenBow, 1)
			.AddIngredient(ModContent.ItemType<EssentialPowder>(), 2)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
