using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace jarkos.tiles.CradleTiles
{
	public sealed class CorruptionNeutralizerTileEntity : ModTileEntity
	{
		public const int RadiusTiles = 1200;
		public bool IsActive;
		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasTile && tile.TileType == ModContent.TileType<CorruptionNeutralizerTile>();
		}
		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i, j - 2, 2, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j - 2, number3: Type);
				return -1;
			}
			return Place(i, j - 2);
		}
		public override void OnNetPlace() {
			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
			}
		}
		public override void SaveData(TagCompound tag) {
			tag["active"] = IsActive;
		}
		public override void LoadData(TagCompound tag) {
			IsActive = tag.GetBool("active");
		}
		public override void NetSend(BinaryWriter writer) {
			writer.Write(IsActive);
		}
		public override void NetReceive(BinaryReader reader) {
			IsActive = reader.ReadBoolean();
		}
	}
}
