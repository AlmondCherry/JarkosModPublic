using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using jarkos.core.Data;
using System.Linq;
using Jarkos.tiles;
using Terraria.GameContent;

namespace Jarkos.UI
{
    public class EssenceBarElement : UIElement
    {
        public AlchemicalEnchantmentTE tileEntity;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            spriteBatch.Draw(pixel, dimensions.ToRectangle(), new Color(20, 20, 30, 200));
            spriteBatch.Draw(pixel, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, 2), Color.Black);
            spriteBatch.Draw(pixel, new Rectangle((int)dimensions.X, (int)(dimensions.Y + dimensions.Height) - 2, (int)dimensions.Width, 2), Color.Black);
            spriteBatch.Draw(pixel, new Rectangle((int)dimensions.X, (int)dimensions.Y, 2, (int)dimensions.Height), Color.Black);
            spriteBatch.Draw(pixel, new Rectangle((int)(dimensions.X + dimensions.Width) - 2, (int)dimensions.Y, 2, (int)dimensions.Height), Color.Black);

            if (tileEntity == null || tileEntity.StoredEssences == null) return;
            int maxEssence = 1000;
            int currentY = (int)(dimensions.Y + dimensions.Height) - 2;

            foreach (var kvp in tileEntity.StoredEssences.Where(kv => kv.Value > 0))
            {
                float fillRatio = (float)kvp.Value / maxEssence;
                int fillHeight = (int)((dimensions.Height - 4) * fillRatio);

                if (fillHeight > 0)
                {
                    currentY -= fillHeight;
                    Rectangle fillRect = new Rectangle((int)dimensions.X + 2, currentY, (int)dimensions.Width - 4, fillHeight);
                    Color essenceColor = EssencePalette.ColorOf(kvp.Key);
                    spriteBatch.Draw(pixel, fillRect, essenceColor);
                }
            }

            if (IsMouseHovering)
            {
                string text = "Stored Essences:\n";

                int total = 0;
                foreach (var kvp in tileEntity.StoredEssences.Where(kv => kv.Value > 0))
                {
                    text += $"{EssencePalette.DisplayName(kvp.Key)}: {kvp.Value}\n";
                    total += kvp.Value;
                }
                
                if (total == 0) text += "Empty\n";
                text += $"---\nTotal: {total} / {maxEssence}";
                Main.hoverItemName = text;
            }
        }
    }
}
