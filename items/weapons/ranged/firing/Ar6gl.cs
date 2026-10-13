using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using jarkos.projectiles.ranged.firearm;
using jarkos.projectiles.bullet;

namespace jarkos.items.weapons.ranged.firing
{
	public class Ar6gl : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 7;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 64;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 7;
            Item.damage = 10;
            Item.noMelee = true;
			Item.scale = 1f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Ar6gl_round>();
			Item.shootSpeed = 20f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override Vector2? HoldoutOffset() {
			return new Vector2(-5f, 2f);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(2));
		}
        public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool CanUseItem(Player player) 
		{
			if (player.altFunctionUse == 2)
			{
				Item.useTime = 36;
				Item.useAnimation = 36;
				Item.damage = 20;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.shoot = ModContent.ProjectileType<Grenade_ar6gl>();
				Item.shootSpeed = 12f;
				Item.autoReuse = true;
			}
			else
			{
				Item.useAnimation = 7;
				Item.useTime = 7;
				Item.damage = 9;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.shoot = ModContent.ProjectileType<Ar6gl_round>();
				Item.shootSpeed = 20f;
				Item.autoReuse = true;
			}
			return base.CanUseItem(player);
		}
        public override bool? UseItem(Player player)
        {
			if (player.altFunctionUse == 2)
			{
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/grenade_launch_sound") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, player.Center);
			}
			else
			{
				SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Ar6gl_shooting") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, player.Center);
			}
            return base.UseItem(player);
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 25)
			.AddRecipeGroup(nameof(ItemID.CrimtaneBar), 10)
			.AddIngredient(ItemID.Silk, 4)
			.AddIngredient(ItemID.Chain, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
