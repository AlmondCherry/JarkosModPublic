using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace jarkos.items.accs.general
{
	public class ArcaneFlower : ModItem
	{
		public bool closed = false;
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			if (Main.dayTime)
			{
				player.lifeRegen += 5;
				player.manaRegen += 2;
				closed = false;
			}
			else
			{
				player.lifeRegen += 2;
				player.manaRegen += 5;
				closed = true;
			}
			float SpeedXofPlayer = Math.Abs(player.velocity.X) * 2f;
			player.lifeRegen += (int)SpeedXofPlayer;
			player.manaRegen += (int)SpeedXofPlayer;
		}
        public override void UpdateInventory(Player player)
        {
            if (Main.dayTime)
			{
				closed = false;
			}
			else
			{
				closed = true;
			}
        }
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frameI, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D texture;
			if (!closed)
			{
				texture = ModContent.Request<Texture2D>(Texture).Value;
				spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
			}
			else
			{
				texture = ModContent.Request<Texture2D>("jarkos/items/accs/general/ArcaneFlower_Closed").Value;
				spriteBatch.Draw(texture, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
			}
			return false;
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture;
			if (!closed)
			{
				texture = ModContent.Request<Texture2D>(Texture).Value;
				spriteBatch.Draw(texture, Item.position - Main.screenPosition, null, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}
			else
			{
				texture = ModContent.Request<Texture2D>("jarkos/items/accs/general/ArcaneFlower_Closed").Value;
				spriteBatch.Draw(texture, Item.position - Main.screenPosition, null, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
			}
			return false;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.BandofRegeneration, 1)
			.AddIngredient(ItemID.ManaRegenerationBand, 1)
			.AddIngredient(ItemID.Bezoar, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
