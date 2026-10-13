using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles
{
    public class UVlampTileEntity : ModTileEntity
    {
        public bool ShowRadius { get; set; } = false;

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<UVlampTile>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            Tile tile = Main.tile[i, j];
            int topLeftX = i - (tile.TileFrameX / 18);
            int topLeftY = j - (tile.TileFrameY / 18);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 3, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
                return -1;
            }
            return Place(topLeftX, topLeftY);
        }

        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["ShowRadius"] = ShowRadius;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("ShowRadius"))
            {
                ShowRadius = tag.GetBool("ShowRadius");
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(ShowRadius);
        }

        public override void NetReceive(BinaryReader reader)
        {
            ShowRadius = reader.ReadBoolean();
        }

        public void SyncData()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
            }
        }
    }
}
