using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles
{
    public class ResearchTableTileEntity : ModTileEntity
    {
        public string OwnerName = "";

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<ResearchTableTile>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            int topLeftX = i - 1;
            int topLeftY = j - 2;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 4, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
                
                ModPacket packet = ModContent.GetInstance<Jarkos.Jarkos_mod>().GetPacket();
                packet.Write((byte)Jarkos.JarkosPacketType.SetResearchTableOwner);
                packet.Write(topLeftX);
                packet.Write(topLeftY);
                packet.Write(Main.player[Main.myPlayer].name);
                packet.Send();
                return -1;
            }
            int id = Place(topLeftX, topLeftY);
            if (id != -1)
            {
                var te = (ResearchTableTileEntity)TileEntity.ByID[id];
                te.OwnerName = Main.player[Main.myPlayer].name;
            }
            return id;
        }

        public override void SaveData(TagCompound tag)
        {
            if (!string.IsNullOrEmpty(OwnerName))
                tag["ownerName"] = OwnerName;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("ownerName"))
                OwnerName = tag.GetString("ownerName");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(OwnerName ?? "");
        }

        public override void NetReceive(BinaryReader reader)
        {
            OwnerName = reader.ReadString();
        }
    }
}
