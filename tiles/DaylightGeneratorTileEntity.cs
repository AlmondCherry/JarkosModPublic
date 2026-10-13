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
	public class DaylightGeneratorTileEntity : ModTileEntity
	{
		public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
		public static int BaseMaxCapacity => 75;
		public static int MaxCapacity => (int)(BaseMaxCapacity * (Jarkos_config.Instance != null ? Jarkos_config.Instance.DaylightCapacityMultiplier : 1.0f));
		public float ProcessTimer = 0f;
		public const float ReqTime = 1800f;
		public bool IgnoreLaser = false;

		public override bool IsTileValidForEntity(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<DaylightGeneratorTile>();
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			int topLeftX = i;
			int topLeftY = j - 3;
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, topLeftX, topLeftY, Type);
				return -1;
			}
			return Place(topLeftX, topLeftY);
		}

		public int GetTotalEssences()
		{
			return StoredEssences.Values.Sum();
		}

		public void AddEssence(EssenceType type, int amount)
		{
			if (GetTotalEssences() >= MaxCapacity) return;
			int spaceLeft = MaxCapacity - GetTotalEssences();
			int toAdd = amount > spaceLeft ? spaceLeft : amount;
			if (StoredEssences.ContainsKey(type))
				StoredEssences[type] += toAdd;
			else
				StoredEssences[type] = toAdd;
		}

		public void ClearEssences()
		{
			StoredEssences.Clear();
		}

		public bool IsOperating => (Main.dayTime || (Jarkos_config.Instance != null && Jarkos_config.Instance.DaylightAlwaysWorks)) && Position.Y < Main.rockLayer && GetTotalEssences() < MaxCapacity;

		public override void Update()
		{

			bool alwaysWorks = Jarkos_config.Instance != null && Jarkos_config.Instance.DaylightAlwaysWorks;
			bool isValidLocationAndDay = (Main.dayTime || alwaysWorks) && (Position.Y < Main.rockLayer);

			if (isValidLocationAndDay && GetTotalEssences() < MaxCapacity)
			{
				float speedMult = Jarkos_config.Instance != null ? Jarkos_config.Instance.DaylightProductionSpeed : 1.0f;
				ProcessTimer += speedMult;
				if (ProcessTimer >= ReqTime)
				{
					ProcessTimer = 0f;

					EssenceType generatedType = GetRandomBasicEssence();
					AddEssence(generatedType, 1);


					Vector2 centerPos = new Vector2(Position.X * 16 + 16, Position.Y * 16 + 16);

					if (Main.netMode != NetmodeID.Server)
					{
						string essenceName = EssencePalette.DisplayName(generatedType);
						Color essenceColor = EssencePalette.ColorOf(generatedType);
						CombatText.NewText(new Rectangle((int)centerPos.X - 16, (int)centerPos.Y - 16, 32, 32), essenceColor, $"+1 {essenceName}", false, false);

						for (int p = 0; p < 12; p++)
						{
							Vector2 particleVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-2.8f, -1.2f));
							Dust d = Dust.NewDustPerfect(centerPos + Main.rand.NextVector2Circular(10, 10), DustID.GoldCoin, particleVel, Scale: 1.2f);
							d.noGravity = true;
						}
					}

					if (Main.netMode == NetmodeID.Server)
					{
						SyncData();
					}
				}
			}
			else
			{

			}
		}

		private EssenceType GetRandomBasicEssence()
		{
			EssenceType[] basicEssences = new EssenceType[]
			{
				EssenceType.Aer,
				EssenceType.Aqua,
				EssenceType.Terra,
				EssenceType.Vita
			};
			return basicEssences[Main.rand.Next(basicEssences.Length)];
		}

		public override void SaveData(TagCompound tag)
		{
			var keys = StoredEssences.Keys.Select(k => (int)k).ToList();
			var vals = StoredEssences.Values.ToList();
			tag["keys"] = keys;
			tag["vals"] = vals;
			tag["timer"] = ProcessTimer;
			tag["ignorelaser"] = IgnoreLaser;
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

			if (tag.ContainsKey("timer"))
			{
				if (tag.TryGet<float>("timer", out float fTimer))
					ProcessTimer = fTimer;
				else if (tag.TryGet<int>("timer", out int iTimer))
					ProcessTimer = iTimer;
			}
			if (tag.ContainsKey("ignoreLaser")) IgnoreLaser = tag.GetBool("ignoreLaser");
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(ProcessTimer);
			writer.Write((byte)StoredEssences.Count);
			writer.Write(IgnoreLaser);
			foreach (var kvp in StoredEssences)
			{
				writer.Write((byte)kvp.Key);
				writer.Write((ushort)kvp.Value);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			ProcessTimer = reader.ReadSingle();
			StoredEssences.Clear();
			IgnoreLaser = reader.ReadBoolean();
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
