using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using jarkos.core.Data;
using jarkos.tiles;
using jarkos.items.materials;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace jarkos.items.usetiles
{
    public class EssenceFlaskItem : ModItem
    {
        public EssenceType StoredType = (EssenceType)(-1);
        public int StoredAmount = 0;
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 32;
            Item.maxStack = 1;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<EssenceFlaskTile>();
            Item.value = Item.buyPrice(copper: 5);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (StoredAmount > 0 && (int)StoredType != -1)
            {
                Color color = EssencePalette.ColorOf(StoredType);
                string name = EssencePalette.DisplayName(StoredType);
                string text = $"Stored: {name} Essence ({StoredAmount}/1000)";
                tooltips.Add(new TooltipLine(Mod, "FlaskStorage", text) { OverrideColor = color });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "FlaskStorage", "Empty Flask (Max: 1000)") { OverrideColor = Color.Gray });
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag["storedType"] = (int)StoredType;
            tag["storedAmount"] = StoredAmount;
        }
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("storedType"))
                StoredType = (EssenceType)tag.GetInt("storedType");
            if (tag.ContainsKey("storedAmount"))
                StoredAmount = tag.GetInt("storedAmount");
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)StoredType);
            writer.Write(StoredAmount);
        }
        public override void NetReceive(BinaryReader reader)
        {
            StoredType = (EssenceType)reader.ReadInt32();
            StoredAmount = reader.ReadInt32();
        }
        private int GetFillFrame()
        {
            if (StoredAmount <= 0) return 0;
            if (StoredAmount < 250) return 1;
            if (StoredAmount < 500) return 2;
            return 3;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (ModContent.HasAsset("jarkos/items/usetiles/EssenceFlaskItem_Fill"))
            {
                var fillTex = ModContent.Request<Texture2D>("jarkos/items/usetiles/EssenceFlaskItem_Fill").Value;
                int frameY = GetFillFrame();
                Rectangle fillFrame = new Rectangle(0, frameY * 30, 32, 30);
                Color color = drawColor;
                if (StoredAmount > 0 && (int)StoredType != -1)
                {
                    color = EssencePalette.ColorOf(StoredType);
                }
                Vector2 fillOrigin = new Vector2(16, 15);
                spriteBatch.Draw(fillTex, position, fillFrame, color, 0f, fillOrigin, scale, SpriteEffects.None, 0f);
            }
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (ModContent.HasAsset("jarkos/items/usetiles/EssenceFlaskItem_Fill"))
            {
                var fillTex = ModContent.Request<Texture2D>("jarkos/items/usetiles/EssenceFlaskItem_Fill").Value;
                int frameY = GetFillFrame();
                Rectangle fillFrame = new Rectangle(0, frameY * 30, 32, 30);
                Color color = lightColor;
                if (StoredAmount > 0 && (int)StoredType != -1)
                {
                    Color essenceColor = EssencePalette.ColorOf(StoredType);
                    color = new Color((int)(essenceColor.R * (lightColor.R / 255f)), (int)(essenceColor.G * (lightColor.G / 255f)), (int)(essenceColor.B * (lightColor.B / 255f)), alphaColor.A);
                }
                Vector2 fillOrigin = new Vector2(16, 15);
                Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2, Item.height - fillFrame.Height / 2);
                spriteBatch.Draw(fillTex, position, fillFrame, color, rotation, fillOrigin, scale, SpriteEffects.None, 0f);
            }
        }

        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Glass, 75)
            .AddIngredient(ModContent.ItemType<PulsatingFragment>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
