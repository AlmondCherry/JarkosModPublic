using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using jarkos.systems;
using jarkos.projectiles.bullet;
using jarkos.projectiles;
using jarkos.items.materials;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.scrolls
{
	public class EnergyScroll : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 31;
            Item.maxStack = 9999;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.noMelee = true;
      		Item.noUseGraphic = true;
			Item.consumable = true;
			Item.shoot = ModContent.ProjectileType<Empty_proj>();
			Item.shootSpeed = 16f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 0, 50);
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
			CreateRecipe(10)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 1)
			.Register();
		}
	}
}
