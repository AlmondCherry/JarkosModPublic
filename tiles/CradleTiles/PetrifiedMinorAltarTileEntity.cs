using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles.CradleTiles
{
	public sealed class PetrifiedMinorAltarTileEntity : ModTileEntity
	{
		public Item HeldItem { get; private set; } = new Item();
		public bool HasItem => HeldItem is not null && !HeldItem.IsAir && HeldItem.stack > 0;
		public override bool IsTileValidForEntity(int x, int y) 
		{
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasTile && tile.TileType == ModContent.TileType<PetrifiedMinorAltarTile>();
		}
		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) 
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) 
			{
				NetMessage.SendTileSquare(Main.myPlayer, i, j - 1, 1, 2);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j - 1, number3: Type);
				return -1;
			}
			return Place(i, j - 1);
		}
		public override void OnNetPlace() 
		{
			if (Main.netMode == NetmodeID.Server) 
			{
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
			}
		}
		public Item TakeItem() 
		{
			Item item = HeldItem;
			HeldItem = new Item();
			return item;
		}
		public void TakeItemAmount(int amount) 
		{
			if (HasItem) 
			{
				if (HeldItem.stack > amount) 
				{
					HeldItem.stack -= amount;
				} else 
				{
					HeldItem = new Item();
				}
			}
		}
		public void SetItem(Item item) 
		{
			HeldItem = item ?? new Item();
		}
		public override void SaveData(TagCompound tag) 
		{
			if (HasItem) 
			{
				tag["item"] = ItemIO.Save(HeldItem);
			}
		}
		public override void LoadData(TagCompound tag) 
		{
			if (tag.ContainsKey("item")) 
			{
				HeldItem = ItemIO.Load(tag.GetCompound("item"));
			}
			else 
			{
				HeldItem = new Item();
			}
		}
		public override void NetSend(BinaryWriter writer) 
		{
			writer.Write(HasItem);
			if (HasItem) 
			{
				ItemIO.Send(HeldItem, writer, writeStack: true);
			}
		}
		public override void NetReceive(BinaryReader reader) 
		{
			bool hasItem = reader.ReadBoolean();
			if (hasItem) 
			{
				HeldItem = ItemIO.Receive(reader, readStack: true);
			}
			else 
			{
				HeldItem = new Item();
			}
		}
	}
}
