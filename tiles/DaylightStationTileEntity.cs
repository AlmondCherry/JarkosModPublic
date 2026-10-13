using System.Collections.Generic;
using System.IO;
using System.Linq;
using jarkos.core.Data;
using jarkos.items.materials;
using jarkos.core.PlayerInteract;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.tiles
{
	public class DaylightStationTileEntity : ModTileEntity
	{
		public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
		public static int BaseMaxCapacity => 250;
		public static int MaxCapacity => (int)(BaseMaxCapacity * (Jarkos_config.Instance != null ? Jarkos_config.Instance.DaylightCapacityMultiplier : 1.0f));

		public Item[] FuelSlots = new Item[2];
		public int[] FuelBurnTimers = new int[2];

		public float ProcessTimer = 0f;
		public const float ReqTime = 1800f;
		public bool IgnoreLaser = false;

		public DaylightStationTileEntity()
		{
			for (int i = 0; i < FuelSlots.Length; i++)
			{
				FuelSlots[i] = new Item();
			}
		}

		public override bool IsTileValidForEntity(int x, int y)
		{
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<DaylightStationTile>();
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			int topLeftX = i;
			int topLeftY = j - 3;
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 6);
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

		public float GetSpeedMultiplier()
		{
			float boost = 0f;
			for (int i = 0; i < FuelSlots.Length; i++)
			{
				if (FuelSlots[i] != null && !FuelSlots[i].IsAir)
				{
					if (FuelSlots[i].type == ModContent.ItemType<CoolingFuel>())
					{
						boost += 0.20f;
					}
					else if (FuelSlots[i].type == ModContent.ItemType<SpectralFuel>())
					{
						boost += 0.70f;
					}
				}
			}
			float configSpeed = Jarkos_config.Instance != null ? Jarkos_config.Instance.DaylightProductionSpeed : 1.0f;
			return (1f + boost) * configSpeed;
		}

		public bool IsOperating => (!Main.dayTime || (Jarkos_config.Instance != null && Jarkos_config.Instance.DaylightAlwaysWorks)) && (Position.Y < Main.rockLayer) && GetTotalEssences() < MaxCapacity;

		public override void Update()
		{
			for (int i = 0; i < FuelSlots.Length; i++)
			{
				if (FuelSlots[i] != null && !FuelSlots[i].IsAir)
				{
					if (FuelSlots[i].type == ModContent.ItemType<CoolingFuel>() || FuelSlots[i].type == ModContent.ItemType<SpectralFuel>())
					{
						FuelBurnTimers[i]++;
						if (FuelBurnTimers[i] >= 300)
						{
							FuelBurnTimers[i] = 0;
							if (Main.netMode != NetmodeID.MultiplayerClient)
							{
								FuelSlots[i].stack--;
								if (FuelSlots[i].stack <= 0)
								{
									FuelSlots[i].TurnToAir();
								}
							}
						}
					}
					else
					{
						FuelBurnTimers[i] = 0;
					}
				}
				else
				{
					FuelBurnTimers[i] = 0;
				}
			}


			bool alwaysWorks = Jarkos_config.Instance != null && Jarkos_config.Instance.DaylightAlwaysWorks;
			bool isValidLocationAndNight = (!Main.dayTime || alwaysWorks) && (Position.Y < Main.rockLayer);

			if (isValidLocationAndNight && GetTotalEssences() < MaxCapacity)
			{
				float speedMult = GetSpeedMultiplier();
				ProcessTimer += speedMult;

				if (ProcessTimer >= ReqTime)
				{
					ProcessTimer = 0f;

					EssenceType generatedType = GetResearchedEssence();
					AddEssence(generatedType, 1);

					Vector2 centerPos = new Vector2(Position.X * 16 + 48, Position.Y * 16 + 32);

					if (Main.netMode != NetmodeID.Server)
					{
						string essenceName = EssencePalette.DisplayName(generatedType);
						Color essenceColor = EssencePalette.ColorOf(generatedType);
						CombatText.NewText(new Rectangle((int)centerPos.X - 24, (int)centerPos.Y - 24, 48, 48), essenceColor, $"+1 {essenceName}", false, false);

						for (int p = 0; p < 18; p++)
						{
							Vector2 particleVel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-3.2f, -1.5f));
							Dust d = Dust.NewDustPerfect(centerPos + Main.rand.NextVector2Circular(24, 24), DustID.GoldCoin, particleVel, Scale: 1.3f);
							d.noGravity = true;
						}
					}

					if (Main.netMode == NetmodeID.Server)
					{
						SyncData();
					}
				}
			}
		}

		private EssenceType GetResearchedEssence()
		{
			Player targetPlayer = Main.player[0];
			if (targetPlayer == null || !targetPlayer.active)
			{
				targetPlayer = Main.LocalPlayer;
			}

			var rp = targetPlayer.GetModPlayer<ResearchPlayer>();
			if (rp != null && rp.DiscoveredEssences.Count > 0)
			{
				List<EssenceType> validDiscovered = new List<EssenceType>();
				foreach (int id in rp.DiscoveredEssences)
				{
					EssenceType et = (EssenceType)id;
					if (System.Enum.IsDefined(typeof(EssenceType), et))
					{
						validDiscovered.Add(et);
					}
				}

				if (validDiscovered.Count > 0)
				{
					return validDiscovered[Main.rand.Next(validDiscovered.Count)];
				}
			}

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
			tag["fuel0"] = ItemIO.Save(FuelSlots[0]);
			tag["fuel1"] = ItemIO.Save(FuelSlots[1]);
			tag["ignoreLaser"] = IgnoreLaser;
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
				if (tag.TryGet("timer", out float fTimer))
					ProcessTimer = fTimer;
				else if (tag.TryGet("timer", out int iTimer))
					ProcessTimer = iTimer;
			}
			if (tag.ContainsKey("fuel0")) FuelSlots[0] = ItemIO.Load(tag.GetCompound("fuel0"));
			if (tag.ContainsKey("fuel1")) FuelSlots[1] = ItemIO.Load(tag.GetCompound("fuel1"));
			if (tag.ContainsKey("ignoreLaser")) IgnoreLaser = tag.GetBool("ignoreLaser");
		}

		public override void NetSend(BinaryWriter writer)
		{
			ItemIO.Send(FuelSlots[0], writer, true);
			ItemIO.Send(FuelSlots[1], writer, true);
			writer.Write(ProcessTimer);
			writer.Write(IgnoreLaser);
			writer.Write((byte)StoredEssences.Count);
			foreach (var kvp in StoredEssences)
			{
				writer.Write((byte)kvp.Key);
				writer.Write((ushort)kvp.Value);
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			FuelSlots[0] = ItemIO.Receive(reader, true);
			FuelSlots[1] = ItemIO.Receive(reader, true);
			ProcessTimer = reader.ReadSingle();
			IgnoreLaser = reader.ReadBoolean();
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
