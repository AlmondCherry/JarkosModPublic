using System.Collections.Generic;
using System.IO;
using jarkos.items.accs.fishing;
using jarkos.items.accs.general;
using jarkos.items.accs.melee;
using jarkos.items.accs.persts;
using jarkos.items.accs.ranged;
using jarkos.items.armor.ang;
using jarkos.items.armor.bastion;
using jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.items.weapons.magic.staffs;
using jarkos.items.weapons.melee.true_melee;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.items.weapons.ranged.firing;
using Jarkos.items.materials;
using Jarkos;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using jarkos.items.placeable;
using Terraria.Chat;
using Terraria.Localization;
using Jarkos.UI;

namespace jarkos.tiles.CradleTiles
{
	public sealed class PetrifiedAltarTileEntity : ModTileEntity
	{
		public const int RitualDurationTicks = 180;
		public const int SearchRadiusTiles = 5;
		public static List<PetrifiedAltarRecipe> Recipes { get; private set; }
		public static void LoadRecipes()
		{
			Recipes = new List<PetrifiedAltarRecipe>();

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<FragmentOfWeakness>(), 1)
				.AddStrictIngredient(ItemID.Bezoar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfFear>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<PulsatingFragment>(), 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<BleedingMoonAmulet>(), 1)
				.AddStrictIngredient(ItemID.HallowedBar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<DistortedStone>(), 1)
				.AddStrictIngredient(ItemID.HallowedBar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<RicochetForce>(), 1)
				.AddStrictIngredient(ItemID.MeteoriteBar, -3, -1)
				.AddStrictIngredient(ModContent.ItemType<GoldenCore>(), 4, -1)
				.AddStrictIngredient(ItemID.RangerEmblem, 5, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<RadiantGem>(), 1)
				.AddLeftIngredient(ModContent.ItemType<MagicalEcho>(), 1)
				.AddLeftIngredient(ModContent.ItemType<SemiorganicStone>(), 1)
				.AddLeftIngredient(ModContent.ItemType<ManaBattery>(), 1)
				.AddRightIngredient(ItemID.Diamond, 1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<ProtectiveStone>(), 1)
				.AddStrictIngredient(ItemID.HallowedBar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<OtherworldlyPlea>(), 1)
				.AddStrictIngredient(ItemID.HallowedBar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<Manacrys_staff>(), 1)
				.AddLeftIngredient(ModContent.ItemType<ManaBattery>(), 1)
				.AddRightIngredient(ItemID.DiamondStaff, 1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<Pyromelt>(), 1)
				.AddStrictIngredient(ItemID.FieryGreatsword, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<PulsatingFragment>(), 4, -1)
				.AddStrictIngredient(ItemID.SoulofFright, 5, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<Thermal_scythe>(), 1)
				.AddStrictIngredient(ItemID.HallowedBar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<Retribution>(), 1)
				.AddStrictIngredient(ItemID.HallowedBar, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfRage>(), 4, -1));

			
			Recipes.Add(new PetrifiedAltarRecipe(ItemID.RottenChunk, 1)
				.AddStrictIngredient(ItemID.Vertebrae, -4, -1, 2));

			Recipes.Add(new PetrifiedAltarRecipe(ItemID.Vertebrae, 1)
				.AddStrictIngredient(ItemID.RottenChunk, -4, -1, 2));

			Recipes.Add(new PetrifiedAltarRecipe(ItemID.CrimtaneBar, 1)
				.AddStrictIngredient(ItemID.DemoniteBar, -4, -1, 2));

			Recipes.Add(new PetrifiedAltarRecipe(ItemID.DemoniteBar, 1)
				.AddStrictIngredient(ItemID.CrimtaneBar, -4, -1, 2));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<GoldenCore>(), 3)
				.AddStrictIngredient(ItemID.FallenStar, -4, -1)
				.AddStrictIngredient(ItemID.Topaz, -3, -1)
				.AddStrictIngredient(ItemID.Diamond, 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<SemiorganicStone>(), 1)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfFear>(), -4, -1, 2)
				.AddStrictIngredient(ModContent.ItemType<FragmentOfNature>(), -3, -1, 10)
				.AddStrictIngredient(ItemID.StoneBlock, 4, -1, 75)
				.AddStrictIngredient(ItemID.Diamond, 5, -1));

			

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<DaylightGeneratorItem>(), 1)
				.AddStrictIngredient(ItemID.ManaCrystal, -4, -1)
				.AddStrictIngredient(ModContent.ItemType<PulsatingFragment>(), -5, -1)
				.AddStrictIngredient(ModContent.ItemType<SemiorganicStone>(), -4, -1)
				.AddStrictIngredient(ModContent.ItemType<DiffusalBar>(), 5, -1, 6));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<DistillerItem>(), 1)
				.AddStrictIngredient(ModContent.ItemType<PulsatingFragment>(), -5, -1)
				.AddStrictIngredient(ModContent.ItemType<SemiorganicStone>(), -4, -1)
				.AddStrictIngredient(ModContent.ItemType<ElementalPowder>(), -3, -1, 2)
				.AddStrictIngredient(ItemID.Solidifier, 4, -1)
				.AddStrictIngredient(ModContent.ItemType<DiffusalBar>(), 5, -1, 6));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<DaylightStationItem>(), 1)
				.AddStrictIngredient(ModContent.ItemType<PulsatingFragment>(), -5, -1)
				.AddStrictIngredient(ModContent.ItemType<SemiorganicStone>(), -4, -1)
				.AddStrictIngredient(ModContent.ItemType<ElementalPowder>(), -3, -1, 2)
				.AddStrictIngredient(ItemID.Solidifier, 4, -1)
				.AddStrictIngredient(ModContent.ItemType<DiffusalBar>(), 5, -1, 6));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<CorruptionNeutralizerItem>(), 1)
				.AddStrictIngredient(ItemID.PurificationPowder, -4, -1, 5)
				.AddStrictIngredient(ItemID.FallenStar, -3, -1, 3)
				.AddStrictIngredient(ItemID.Acorn, 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<EssenceCombinerItem>(), 1)
				.AddStrictIngredient(ItemID.Glass, -4, -1, 10)
				.AddStrictIngredient(ItemID.Wood, -3, -1, 250)
				.AddStrictIngredient(ItemID.GoldBar, 4, -1, 20));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<RuneAltarItem>(), 1)
				.AddStrictIngredient(ItemID.Obsidian, -4, -1, 25)
				.AddStrictIngredient(ItemID.GoldBar, -3, -1, 5)
				.AddStrictIngredient(ItemID.Diamond, 4, -1));

			Recipes.Add(new PetrifiedAltarRecipe(ModContent.ItemType<RuneChargerItem>(), 1)
				.AddStrictIngredient(ItemID.Obsidian, -4, -1, 50)
				.AddStrictIngredient(ItemID.StoneBlock, -3, -1, 150)
				.AddStrictIngredient(ItemID.GoldBar, 4, -1, 10));
		}
		public static string GetRequiredNode(int resultItemType)
		{

			if (resultItemType == ModContent.ItemType<Pyromelt>()) return "jarkos:PyrosNode5";

			if (resultItemType == ModContent.ItemType<DaylightGeneratorItem>()) return "jarkos:AquaNode2";
			if (resultItemType == ModContent.ItemType<DistillerItem>()) return "jarkos:AquaNode3";
			if (resultItemType == ModContent.ItemType<DaylightStationItem>()) return "jarkos:AquaNode4";

			if (resultItemType == ModContent.ItemType<BleedingMoonAmulet>()) return "jarkos:VitaNode2";
			if (resultItemType == ModContent.ItemType<DistortedStone>()) return "jarkos:VitaNode3";
			if (resultItemType == ModContent.ItemType<Retribution>()) return "jarkos:VitaNode4";

			if (resultItemType == ModContent.ItemType<ProtectiveStone>()) return "jarkos:UmbraNode2";
			if (resultItemType == ModContent.ItemType<OtherworldlyPlea>()) return "jarkos:UmbraNode3";
			if (resultItemType == ModContent.ItemType<Thermal_scythe>()) return "jarkos:UmbraNode4";

			if (resultItemType == ModContent.ItemType<FragmentOfWeakness>()) return "jarkos:VenenumNode2";

			return null;
		}
		public int RitualTimer;
		public int PlannedDropType;
		public int PlannedDropStack;
		public bool IsAnimating => RitualTimer > 0;
		public override bool IsTileValidForEntity(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			return tile.HasTile && tile.TileType == ModContent.TileType<PetrifiedAltarTile>();
		}
		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
		{
			int topLeftX = i - 1;
			int topLeftY = j - 1;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(Main.myPlayer, topLeftX, topLeftY, 4, 2);
				NetMessage.SendData(MessageID.TileEntityPlacement, number: topLeftX, number2: topLeftY, number3: Type);
				return -1;
			}
			return Place(topLeftX, topLeftY);
		}
		public override void OnNetPlace() {
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
			}
		}
		public override void Update() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}
			if (RitualTimer > 0)
			{
				RitualTimer--;
				if (RitualTimer == 0)
				{
					CompleteRitual();
				}
			}
		}
		public bool TryStartRitual(Player player = null)
		{
			if (IsAnimating)
			{
				return false;
			}
			int centerX = Position.X + 1;
			int centerY = Position.Y + 1;

			List<PetrifiedMinorAltarTileEntity> leftAltars = new List<PetrifiedMinorAltarTileEntity>();
			List<PetrifiedMinorAltarTileEntity> rightAltars = new List<PetrifiedMinorAltarTileEntity>();
			List<PetrifiedMinorAltarTileEntity> allAltars = new List<PetrifiedMinorAltarTileEntity>();

			int minorTile = ModContent.TileType<PetrifiedMinorAltarTile>();
			for (int dy = -SearchRadiusTiles; dy <= SearchRadiusTiles; dy++)
			{
				for (int dx = -SearchRadiusTiles; dx <= SearchRadiusTiles; dx++)
				{
					int tx = centerX + dx;
					int ty = centerY + dy;

					Tile tile = Framing.GetTileSafely(tx, ty);

					if (!tile.HasTile || tile.TileType != minorTile || tile.TileFrameY != 0)
					{
						continue;
					}
					PetrifiedMinorAltarTileEntity minor = PetrifiedMinorAltarTile.GetOrCreate(tx, ty);

					if (minor is null || !minor.HasItem)
					{
						continue;
					}
					int realDx = minor.Position.X - centerX;
					if (realDx < 0)
					{
						leftAltars.Add(minor);
					}

					else if (realDx > 0)
					{
						rightAltars.Add(minor);
					}
					allAltars.Add(minor);
				}
			}
			List<PetrifiedAltarRecipe> matchedRecipes = new List<PetrifiedAltarRecipe>();
			List<Dictionary<PetrifiedMinorAltarTileEntity, int>> matchedConsumedAltars = new List<Dictionary<PetrifiedMinorAltarTileEntity, int>>();
			
			if (Recipes != null)
			{
				foreach (var recipe in Recipes)
				{
					if (recipe.TryMatch(centerX, centerY, allAltars, leftAltars, rightAltars, out var consumed))
					{
						matchedRecipes.Add(recipe);
						matchedConsumedAltars.Add(consumed);
					}
				}
			}
			if (matchedRecipes.Count == 0)
			{
				return false;
			}
			if (matchedRecipes.Count > 1)
			{
				if (Main.netMode != NetmodeID.Server)
				{
					AltarUISystem.OpenRecipeSelect(this, matchedRecipes);
				}
				return true;
			}
			PetrifiedAltarRecipe matchedRecipe = matchedRecipes[0];
			int recipeIndex = Recipes.IndexOf(matchedRecipe);

			if (Main.netMode == NetmodeID.MultiplayerClient) 
			{
				ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
				packet.Write((byte)JarkosPacketType.MessageType_StartSpecificAltarRitual);
				packet.Write((int)Position.X);
				packet.Write((int)Position.Y);
				packet.Write(recipeIndex);
				packet.Send();

				Vector2 netCenter = new Vector2(Position.X * 16 + 32, centerY * 16);
				SoundEngine.PlaySound(SoundID.Item37, netCenter);

				return true;
			}
			var consumedAltars = matchedConsumedAltars[0];
			if (player != null)
			{
				string reqNode = GetRequiredNode(matchedRecipe.ResultItem);
				if (reqNode != null)
				{
					var plr = player.GetModPlayer<core.PlayerInteract.ResearchPlayer>();
					if (plr != null && !plr.IsCompleted(reqNode))
					{
						if (Main.netMode == NetmodeID.Server)
						{
							ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("This recipe has not been researched yet."), Color.SlateGray, player.whoAmI);
						} else {
							Main.NewText("This recipe has not been researched yet.", Color.SlateGray);
						}
						return false;
					}
				}
			}

			foreach (var kvp in consumedAltars) kvp.Key.TakeItemAmount(kvp.Value);

			PlannedDropType = matchedRecipe.ResultItem;
			PlannedDropStack = matchedRecipe.ResultStack;

			RitualTimer = RitualDurationTicks;

			Vector2 exactCenter = new Vector2(Position.X * 16 + 32, centerY * 16);
			SoundEngine.PlaySound(SoundID.Item29, exactCenter);
			
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int projIdx = Projectile.NewProjectile(new EntitySource_TileEntity(this), exactCenter, Vector2.Zero, ModContent.ProjectileType<PetrifiedAltarBeamProj>(), 0, 0f, Main.myPlayer);
				if (projIdx >= 0 && projIdx < Main.maxProjectiles)
				{
					Main.projectile[projIdx].timeLeft = RitualDurationTicks;
				}
			}
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
				foreach (var kvp in consumedAltars)
				{
					var altar = kvp.Key;
					NetMessage.SendData(MessageID.TileEntitySharing, number: altar.ID, number2: altar.Position.X, number3: altar.Position.Y);
				}
			}
			return true;
		}
		public void StartRitualWithRecipeIndex(int index, Player player)
		{
			if (Recipes == null || index < 0 || index >= Recipes.Count) return;

			int centerX = Position.X + 1;
			int centerY = Position.Y + 1;
			
			List<PetrifiedMinorAltarTileEntity> leftAltars = new List<PetrifiedMinorAltarTileEntity>();
			List<PetrifiedMinorAltarTileEntity> rightAltars = new List<PetrifiedMinorAltarTileEntity>();
			List<PetrifiedMinorAltarTileEntity> allAltars = new List<PetrifiedMinorAltarTileEntity>();

			for (int x = centerX - SearchRadiusTiles; x <= centerX + SearchRadiusTiles; x++)
			{
				for (int y = centerY - SearchRadiusTiles; y <= centerY + SearchRadiusTiles; y++)
				{
					if (!ByPosition.TryGetValue(new Point16(x, y), out TileEntity rawEntity) || rawEntity is not PetrifiedMinorAltarTileEntity minor)
					{
						continue;
					}

					if (minor.Position.X < Position.X) leftAltars.Add(minor);
					else if (minor.Position.X > Position.X + 3) rightAltars.Add(minor);
					allAltars.Add(minor);
				}
			}
			if (Recipes[index].TryMatch(centerX, centerY, allAltars, leftAltars, rightAltars, out var consumedAltars))
			{
				foreach (var kvp in consumedAltars) kvp.Key.TakeItemAmount(kvp.Value);

				PlannedDropType = Recipes[index].ResultItem;
				PlannedDropStack = Recipes[index].ResultStack;

				RitualTimer = RitualDurationTicks;

				Vector2 exactCenter = new Vector2(Position.X * 16 + 32, centerY * 16);
				SoundEngine.PlaySound(SoundID.Item29, exactCenter);

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int projIdx = Projectile.NewProjectile(new EntitySource_TileEntity(this), exactCenter, Vector2.Zero, ModContent.ProjectileType<PetrifiedAltarBeamProj>(), 0, 0f, Main.myPlayer);
					if (projIdx >= 0 && projIdx < Main.maxProjectiles)
					{
						Main.projectile[projIdx].timeLeft = RitualDurationTicks;
					}
				}
				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
					foreach (var kvp in consumedAltars)
					{
						var altar = kvp.Key;
						NetMessage.SendData(MessageID.TileEntitySharing, number: altar.ID, number2: altar.Position.X, number3: altar.Position.Y);
					}
				}
			}
		}
		private void CompleteRitual()
		{
			int centerX = Position.X + 1;
			int centerY = Position.Y + 1;

			Vector2 exactCenter = new Vector2(Position.X * 16 + 32, centerY * 16);

			if (PlannedDropType > 0)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					int spawnX = Position.X * 16 + 24;
					int spawnY = centerY * 16 - 28;

					int idx = Item.NewItem(new EntitySource_TileEntity(this), spawnX, spawnY, 16, 16, PlannedDropType, PlannedDropStack, noBroadcast: false);
					if (idx >= 0 && idx < Main.maxItems)
					{
						Main.item[idx].velocity = new Vector2(0f, -1.5f);
						Main.item[idx].noGrabDelay = 0;
					}
				}
				SoundEngine.PlaySound(SoundID.Item37, exactCenter);
			}
			PlannedDropType = 0;
			PlannedDropStack = 0;
			RitualTimer = 0;
			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
			}
		}
		public override void SaveData(TagCompound tag)
		{
			tag["ritualTimer"] = RitualTimer;
			tag["plannedDrop"] = PlannedDropType;
			tag["plannedDropStack"] = PlannedDropStack;
		}
		public override void LoadData(TagCompound tag)
		{
			RitualTimer = tag.GetInt("ritualTimer");
			PlannedDropType = tag.GetInt("plannedDrop");
			PlannedDropStack = tag.ContainsKey("plannedDropStack") ? tag.GetInt("plannedDropStack") : 1;
		}
		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(RitualTimer);
			writer.Write(PlannedDropType);
			writer.Write(PlannedDropStack);
		}
		public override void NetReceive(BinaryReader reader)
		{
			RitualTimer = reader.ReadInt32();
			PlannedDropType = reader.ReadInt32();
			PlannedDropStack = reader.ReadInt32();
		}
	}
}
