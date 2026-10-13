using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace Jarkos.UI
{
	internal class DraggableUIPanel : UIState
	{
		private bool isDragging = false;
        private Vector2 dragOffset;
        public Color BackgroundColor = Color.White;
        public int DragZoneHeight = 30;
        public delegate void DragEndHandler(Vector2 newPosition);
        public event DragEndHandler OnDragEnd;
        public DraggableUIPanel()
        {
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Rectangle rect = dims.ToRectangle();
            if (UITextures.ConverterBackground != null && UITextures.ConverterBackground.IsLoaded)
            {
                Texture2D bgTexture = UITextures.ConverterBackground.Value;
                spriteBatch.Draw(bgTexture, rect, null, BackgroundColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, new Color(73, 94, 171, 220));
            }
            if (isDragging)
            {
                Rectangle dragZone = new Rectangle(rect.X, rect.Y, rect.Width, DragZoneHeight);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, dragZone, new Color(255, 255, 255, 50));
            }
        }
        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            CalculatedStyle dims = GetDimensions();
            Rectangle dragZone = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, DragZoneHeight);
            if (dragZone.Contains(evt.MousePosition.ToPoint()))
            {
                isDragging = true;
                dragOffset = evt.MousePosition - new Vector2(dims.X, dims.Y);
            }
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (isDragging)
            {
                isDragging = false;
                CalculatedStyle dims = GetDimensions();
                Vector2 normalizedPos = new Vector2(dims.X / Main.screenWidth, dims.Y / Main.screenHeight);
                OnDragEnd?.Invoke(normalizedPos);
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (isDragging)
            {
                Vector2 newPos = Main.MouseScreen - dragOffset;
                CalculatedStyle dims = GetDimensions();
                newPos.X = MathHelper.Clamp(newPos.X, 0, Main.screenWidth - dims.Width);
                newPos.Y = MathHelper.Clamp(newPos.Y, 0, Main.screenHeight - dims.Height);
                Left.Set(newPos.X, 0f);
                Top.Set(newPos.Y, 0f);
                HAlign = 0f;
                VAlign = 0f;
                Recalculate();
            }
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
        public void SetNormalizedPosition(float x, float y)
        {
            CalculatedStyle dims = GetDimensions();
            float pixelX = x * Main.screenWidth - dims.Width / 2f;
            float pixelY = y * Main.screenHeight - dims.Height / 2f;

            pixelX = MathHelper.Clamp(pixelX, 0, Main.screenWidth - dims.Width);
            pixelY = MathHelper.Clamp(pixelY, 0, Main.screenHeight - dims.Height);

            Left.Set(pixelX, 0f);
            Top.Set(pixelY, 0f);
            
            HAlign = 0f;
            VAlign = 0f;
        }
        public void CenterOnScreen()
        {
            Left.Set(0, 0f);
            Top.Set(0, 0f);
            HAlign = 0.5f;
            VAlign = 0.5f;
        }
	}
}
