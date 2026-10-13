using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.staffs;

namespace jarkos.items.weapons.magic.staffs
{
	public class StaffOfDoomAndDespair : ModItem
	{
		public override string Texture => "jarkos/items/weapons/magic/staffs/staff_of_doom_and_despair";

		public override void SetStaticDefaults()
		{
			Item.staff[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 56;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 3;
			Item.width = 46;
			Item.height = 46;
			Item.useTime = 7;
			Item.useAnimation = 7;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 3f;
			Item.UseSound = SoundID.Item9 with { Pitch = 0.2f, Volume = 0.5f };
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<DoomStarProj>();
			Item.shootSpeed = 16f;

			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(0, 3, 50, 0);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(6f));
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ItemID.FallenStar, 10)
			.AddIngredient(ItemID.SoulofFright, 10)
			.AddIngredient(ItemID.SoulofNight, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
