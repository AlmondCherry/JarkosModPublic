using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.core.Data;
using jarkos.items.usetiles;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.tiles
{
    public class EssenceFlaskTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<EssenceFlaskTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            HitSound = SoundID.Shatter;
            DustType = DustID.Glass;
            AddMapEntry(new Color(150, 200, 255), CreateMapEntryName());
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is EssenceFlaskTileEntity flaskTe)
            {
                if ((int)flaskTe.storedType != -1 && flaskTe.storedAmount > 0)
                {
                    Texture2D liquidTex = ModContent.Request<Texture2D>("jarkos/tiles/EssenceFlaskTile_LiquidLevel").Value;
                    Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                    if (Main.drawToScreen) zero = Vector2.Zero;
                    Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
                    Color color = EssencePalette.ColorOf(flaskTe.storedType);
                    int frameY = 0;
                    if (flaskTe.storedAmount >= 750) frameY = 36;
                    else if (flaskTe.storedAmount >= 250) frameY = 18;
                    spriteBatch.Draw(liquidTex, pos, new Rectangle(0, frameY, 16, 16), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<EssenceFlaskTileEntity>().Kill(i, j);
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || effectOnly) return;
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is EssenceFlaskTileEntity flaskTe)
            {
                noItem = true;
                int itemIndex = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<EssenceFlaskItem>());
                if (Main.item[itemIndex].ModItem is EssenceFlaskItem flaskItem)
                {
                    flaskItem.StoredType = flaskTe.storedType;
                    flaskItem.StoredAmount = flaskTe.storedAmount;
                }
                ModContent.GetInstance<EssenceFlaskTileEntity>().Kill(i, j);
            }
        }
    }
}
