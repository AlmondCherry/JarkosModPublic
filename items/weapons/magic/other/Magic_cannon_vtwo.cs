using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.magic.other;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.other
{
	public class Magic_cannon_vtwo : ModItem
	{
		public float shoots = 0f;
		public override void SetDefaults()
		{
			Item.damage = 14;
			Item.DamageType = DamageClass.Magic;
			Item.crit = 4;
			Item.width = 64;
			Item.height = 22;
			Item.UseSound = new SoundStyle?(Jarkos_sounds.meteorite_rifle_shoot);
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 9;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 2;
			Item.mana = 7;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Magicprojvtwo>();
			Item.shootSpeed = 20f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-25f, 0f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(3));
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			shoots++;
			if (shoots > 3)
			{
				Item.reuseDelay = 10;
				shoots = 0;
			}
			else
			{
				Item.reuseDelay = 0;
			}
			return true;
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.SpaceGun, 1)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
