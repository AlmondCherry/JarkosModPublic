using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using jarkos.projectiles.magic.other;
using jarkos.items.materials;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace jarkos.items.weapons.magic.other
{
	public class MagicBow : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 15;
			Item.DamageType = DamageClass.Magic;
            Item.width = 84;
            Item.height = 160;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 15;
            Item.damage = 24;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 0.5f;
            Item.mana = 6;
			Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<Magic_arrow>();
			Item.shootSpeed = 4f;
			
			Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 60, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/MagicBowShoot") with { Volume = 0.55f, Pitch = Main.rand.NextFloat(-0.3f, 0.3f) }, player.Center);
            return true;
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Bone, 115)
			.AddIngredient(ModContent.ItemType<EnchantedFragment>(), 35)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 15)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
