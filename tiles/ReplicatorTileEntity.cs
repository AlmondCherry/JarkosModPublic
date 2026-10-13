using System.Collections.Generic;
using System.IO;
using System.Linq;
using jarkos.core.Data;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles
{
	public class ReplicatorTileEntity : ModTileEntity
	{
		public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
		public const int MaxCapacity = 200;

		public Item InputSlot = new Item();
		public Item[] OutputSlots = new Item[2];

		public bool IsCloning = false;
		public int CloneTimer = 0;
		public const int CloneReqTime = 8400;

		public ReplicatorTileEntity()
		{
			InputSlot = new Item();
			for (int i = 0; i < OutputSlots.Length; i++)
			{
				OutputSlots[i] = new Item();
			}
		}

		public override bool IsTileValidForEntity(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<ReplicatorTile>();
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			int topLeftX = i - 1;
			int topLeftY = j - 2;
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 4, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
				return -1;
			}
			return Place(topLeftX, topLeftY);
		}

		public int GetTotalEssences()
		{
			return StoredEssences.Values.Sum();
		}

		public int GetChitinusCount()
		{
			return StoredEssences.TryGetValue(EssenceType.Chitinus, out int val) ? val : 0;
		}

		public bool AreOutputSlotsEmpty()
		{
			return (OutputSlots[0] == null || OutputSlots[0].IsAir) && (OutputSlots[1] == null || OutputSlots[1].IsAir);
		}

		public bool CanStartCloning()
		{
			if (IsCloning) return false;
			if (InputSlot == null || InputSlot.IsAir) return false;
			if (GetChitinusCount() < 75) return false;
			if (!AreOutputSlotsEmpty()) return false;
			return true;
		}

		public bool StartCloning()
		{
			if (!CanStartCloning()) return false;

			StoredEssences[EssenceType.Chitinus] -= 75;
			IsCloning = true;
			CloneTimer = 0;

			if (Main.netMode == NetmodeID.Server)
			{
				SyncData();
			}
			return true;
		}

		public override void Update()
		{
			if (IsCloning)
			{
				CloneTimer++;
				if (CloneTimer >= CloneReqTime)
				{
					IsCloning = false;
					CloneTimer = 0;

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						if (InputSlot != null && !InputSlot.IsAir)
						{
							OutputSlots[0] = InputSlot.Clone();
							OutputSlots[1] = InputSlot.Clone();
							InputSlot.TurnToAir();

							Vector2 centerPos = new Vector2(Position.X * 16 + 24, Position.Y * 16 + 24);
							if (Main.netMode != NetmodeID.Server)
							{
								CombatText.NewText(new Rectangle((int)centerPos.X - 20, (int)centerPos.Y - 20, 40, 40), Color.Purple, "Cloned!", true);
							}
						}
						SyncData();
					}
				}
			}
		}

		public override void SaveData(TagCompound tag)
		{
			var keys = StoredEssences.Keys.Select(k => (int)k).ToList();
			var vals = StoredEssences.Values.ToList();
			tag["keys"] = keys;
			tag["vals"] = vals;
			tag["isCloning"] = IsCloning;
			tag["cloneTimer"] = CloneTimer;
			tag["inputSlot"] = ItemIO.Save(InputSlot);
			tag["outputSlot0"] = ItemIO.Save(OutputSlots[0]);
			tag["outputSlot1"] = ItemIO.Save(OutputSlots[1]);
		}

		public override void LoadData(TagCompound tag)
		{
			StoredEssences.Clear();
			if (tag.ContainsKey("keys"))
			{
				var keys = tag.GetList<int>("keys");
				var vals = tag.GetList<int>("vals");
				for (int i = 0; i < keys.Count && i < vals.Count; i++)
				{
					StoredEssences[(EssenceType)keys[i]] = vals[i];
				}
			}
			if (tag.ContainsKey("isCloning")) IsCloning = tag.GetBool("isCloning");
			if (tag.ContainsKey("cloneTimer")) CloneTimer = tag.GetInt("cloneTimer");
			InputSlot = tag.ContainsKey("inputSlot") ? ItemIO.Load(tag.GetCompound("inputSlot")) : new Item();
			OutputSlots[0] = tag.ContainsKey("outputSlot0") ? ItemIO.Load(tag.GetCompound("outputSlot0")) : new Item();
			OutputSlots[1] = tag.ContainsKey("outputSlot1") ? ItemIO.Load(tag.GetCompound("outputSlot1")) : new Item();
		}

		public override void NetSend(BinaryWriter writer)
		{
			InputSlot ??= new Item();
			OutputSlots[0] ??= new Item();
			OutputSlots[1] ??= new Item();
			ItemIO.Send(InputSlot, writer, true);
			ItemIO.Send(OutputSlots[0], writer, true);
			ItemIO.Send(OutputSlots[1], writer, true);
			writer.Write(IsCloning);
			writer.Write((ushort)CloneTimer);
			writer.Write((byte)StoredEssences.Count);
			foreach (var kvp in StoredEssences)
			{
				writer.Write((byte)kvp.Key);
				writer.Write((ushort)kvp.Value);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			InputSlot = ItemIO.Receive(reader, true);
			OutputSlots[0] = ItemIO.Receive(reader, true);
			OutputSlots[1] = ItemIO.Receive(reader, true);
			IsCloning = reader.ReadBoolean();
			CloneTimer = reader.ReadUInt16();
			StoredEssences.Clear();
			byte count = reader.ReadByte();
			for (int i = 0; i < count; i++)
			{
				EssenceType key = (EssenceType)reader.ReadByte();
				int val = reader.ReadUInt16();
				StoredEssences[key] = val;
			}
		}

		public void SyncData()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
			}
		}
	}
}
