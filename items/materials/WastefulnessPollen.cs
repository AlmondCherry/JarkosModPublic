using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace jarkos.items.materials
{
	public class WastefulnessPollen : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(copper: 5);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 0.9f, 0.4f);
        }
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			Vector2 origin = texture.Size() * 0.5f;
			
			for (int i = 1; i <= 3; i++)
			{
				Vector2 clonePos;
				if (Item.velocity.LengthSquared() > 0.2f)
				{
					clonePos = Item.Center - Item.velocity * (i * 2.5f);
				}
				else
				{
					float angle = Main.GlobalTimeWrappedHourly * 4f + (i * MathHelper.TwoPi / 3f);
					clonePos = Item.Center + new Vector2(0f, 10f).RotatedBy(angle);
				}

				Color cloneColor = new Color(255, 230, 80, 0) * 0.35f;
				spriteBatch.Draw(texture, clonePos - Main.screenPosition, null, cloneColor, rotation, origin, scale * 0.9f, SpriteEffects.None, 0f);
			}
			Vector2[] outlineOffsets = new Vector2[]
			{
				new Vector2(-1.5f, 0f), new Vector2(1.5f, 0f), new Vector2(0f, -1.5f), new Vector2(0f, 1.5f)
			};

			Color outlineColor = new Color(255, 215, 0, 0) * 0.55f;
			
			foreach (Vector2 offset in outlineOffsets)
			{
				spriteBatch.Draw(texture, Item.Center + offset - Main.screenPosition, null, outlineColor, rotation, origin, scale, SpriteEffects.None, 0f);
			}
			return true;
		}
	}
}
