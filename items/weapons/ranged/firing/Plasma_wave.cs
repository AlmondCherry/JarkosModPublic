using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jarkos.projectiles.mecha;
using jarkos.items.materials;
using Jarkos.items.usetiles;
using Jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Plasma_wave : ModItem
	{
        public override void SetDefaults()
        {
            Item.damage = 28;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 24;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Plasma_wave_Holdout>();
            Item.shootSpeed = 24f;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Color glowColor = new Color(0, 255, 200, 0) * 0.5f;
            spriteBatch.Draw(texture, position, null, glowColor, rotation, texture.Size() / 2f, scale * 1.1f, SpriteEffects.None, 0f);
        }
		public override void AddRecipes() {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<SteelBar>(), 4)
            .AddIngredient(ModContent.ItemType<Battery>(), 2)
            .AddIngredient(ItemID.Sapphire, 1)
            .AddIngredient(ItemID.Emerald, 1)
            .AddRecipeGroup(nameof(ItemID.TissueSample), 6)
            .AddTile(TileID.Anvils)
            .Register();
		}
	}
}
