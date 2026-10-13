using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using jarkos.projectiles.mecha;
using jarkos.items.materials;
using Terraria.DataStructures;
namespace jarkos.items.weapons.ranged.firing
{
	public class Glitchedmachinegun : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 7;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 84;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 7;
            Item.damage = 126;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Glitchedmachinegun_Holdout>();
			Item.shootSpeed = 26f;
            Item.useAmmo = AmmoID.Bullet;
            Item.channel = true;
            Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Glitchedmachinegun_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
        	Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/glowmasks/glitchedmachinegun_glowmask", AssetRequestMode.ImmediateLoad).Value;
 			spriteBatch.Draw(texture,new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f + 2f), new Rectangle(0, 0, texture.Width, texture.Height),Color.White,rotation,texture.Size() * 0.5f,scale, SpriteEffects.None, 0f);
        }
        public override Vector2? HoldoutOffset() {
			return new Vector2(-5f, 2f);
		}
        public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextFloat() >= 0.70f;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (type == ProjectileID.Bullet) 
			{
				type = 242;
			}
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Uzi, 1)
			.AddIngredient(ItemID.SpectreBar, 4)
			.AddIngredient(ModContent.ItemType<HeavenlyPowder>(), 2)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
