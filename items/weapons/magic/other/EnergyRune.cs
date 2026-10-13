using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using jarkos.systems;
using jarkos.projectiles.bullet;
using jarkos.projectiles;
using jarkos.items.weapons.magic.scrolls;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.other
{
	public class EnergyRune : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 27;
			Item.DamageType = DamageClass.Magic;
			Item.crit = 6;
			Item.width = 16;
			Item.height = 18;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.mana = 5;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<Empty_proj>();
			Item.shootSpeed = 12f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Vector2 cursor = Main.MouseWorld;
			Vector2 start2 = player.Center;
			Vector2 end2 = cursor;
			
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/cafas_sound") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) }, player.position);
			LightningSystemLaser.MakeProjectileLighting(start2, end2, ModContent.ProjectileType<Beam_scroll>(), Item.damage, knockback, 0.75f, 120f, 1);
			return true;
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<EnergyScroll>(), 200)
			.AddIngredient(ItemID.FallenStar, 4)
			.Register();
		}
	}
}
