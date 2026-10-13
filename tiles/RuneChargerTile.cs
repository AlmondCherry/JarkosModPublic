using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.items;
using jarkos.UI;
using Terraria.ID;
using Terraria.GameContent.ObjectInteractions;
namespace jarkos.tiles
{
    public class RuneChargerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 20 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.Origin = new Point16(1, 3);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(150, 150, 200), Terraria.Localization.Language.GetText("Rune Charger"));
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.HeldItem.ModItem is BaseRuneItem)
            {
                RuneChargerUISystem.OpenUI();
            }
            else
            {
                Main.NewText("You must hold a Rune to interact with the Rune Charger.", Color.Orange);
            }
            return true;
        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            if (player.HeldItem.ModItem is BaseRuneItem)
            {
                player.cursorItemIconID = player.HeldItem.type;
            }
            else
            {
                player.cursorItemIconID = ModContent.ItemType<InitialRuneItem>();
            }
        }
    }
}
