using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.other;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.other
{
	public class Meteorite_gun : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 25;
			Item.DamageType = DamageClass.Magic;
			Item.width = 136;
			Item.height = 36;
			Item.useAnimation = 38;
			Item.useTime = 38;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 2;
			Item.mana = 8;
			Item.UseSound = SoundID.Item158;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Meteoritegun_proj>();
			Item.shootSpeed = 18f;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-32f, 0f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var velocity1 = velocity.RotatedByRandom(MathHelper.ToRadians(10));
			var velocity2 = velocity.RotatedByRandom(MathHelper.ToRadians(5));

			Projectile.NewProjectile(source, position, velocity1, type, damage, knockback);
			Projectile.NewProjectile(source, position, velocity2, type, damage, knockback);
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
        	Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/glowmasks/meteorite_gun_glowmask", AssetRequestMode.ImmediateLoad).Value;
 			spriteBatch.Draw(texture,new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f + 2f), new Rectangle(0, 0, texture.Width, texture.Height),Color.White,rotation,texture.Size() * 0.5f,scale, SpriteEffects.None, 0f);
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Magic_cannon_vtwo>(), 1)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 18)
			.AddIngredient(ItemID.MeteoriteBar, 20)
			.AddIngredient(ItemID.Amber, 6)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
