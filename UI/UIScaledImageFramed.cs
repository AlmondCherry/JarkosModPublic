using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace jarkos.UI
{
	public class UIScaledImageFramed : UIElement
	{
		private Asset<Texture2D> _texture;
		private Rectangle _frame;
		public Color Color = Color.White;
		public float ImageScale = 1f;
		public UIScaledImageFramed(Asset<Texture2D> texture, Rectangle frame)
		{
			_texture = texture;
			_frame = frame;
		}
		public void SetImage(Asset<Texture2D> texture, Rectangle frame)
		{
			_texture = texture;
			_frame = frame;
		}
		public void SetFrame(Rectangle frame)
		{
			_frame = frame;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			CalculatedStyle dimensions = GetDimensions();
			Vector2 center = dimensions.Position() + new Vector2(dimensions.Width / 2f, dimensions.Height / 2f);
			Vector2 origin = new Vector2(_frame.Width, _frame.Height) / 2f;
			
			spriteBatch.Draw(_texture.Value, center, _frame, Color, 0f, origin, ImageScale, SpriteEffects.None, 0f);
		}
	}
}
