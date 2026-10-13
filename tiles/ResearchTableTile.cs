using Terraria.Localization;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.GameContent.ObjectInteractions;
using jarkos.UI;
using jarkos.items.usetiles;
using jarkos.core;
namespace jarkos.tiles
{
    public class ResearchTableTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 4, 0);
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ResearchTableTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            RegisterItemDrop(ModContent.ItemType<ResearchTableItem>());
            AddMapEntry(new Color(150, 130, 210), Language.GetText("Mods.ResearchTable.Tiles.ResearchTableTile.MapEntry"));
            DustType = DustID.PurpleTorch;
            AdjTiles = new int[] { TileID.WorkBenches };
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
        public override bool RightClick(int i, int j)
        {
            if (ResearchUISystem.CloseCooldown > 0) return true;
            Player player = Main.LocalPlayer;
            player.CloseSign();
            player.SetTalkNPC(-1);
            Main.npcChatCornerItem = 0;
            Main.npcChatText = "";
            if (!ResearchScreen.Instance.Active)
            {
                ResearchScreen.Instance.Open(player);
            }
            else ResearchScreen.Instance.Close();
            return true;
        }
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<ResearchTableTileEntity>().Kill(i, j);
        }
    }
}
