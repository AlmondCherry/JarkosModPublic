using System;
using jarkos.cradle_biome.system;
using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace jarkos.cradle_biome
{
	public class AbyssTownNPC : GlobalNPC
	{
		private const int ComplaintCount = 6;
		private const float ComplaintChance = 0.6f;
		public override void GetChat(NPC npc, ref string chat)
		{
			if (!npc.townNPC)
			{
				return;
			}
			if (!IsNpcInsideAbyss(npc))
			{
				return;
			}
			Vector2 npcCentre = npc.Center;

			if (CorruptionNeutralizerSystem.IsActiveWithin(npcCentre, CorruptionNeutralizerTileEntity.RadiusTiles))
			{
				return;
			}

			UnifiedRandom rng = Main.rand ?? new UnifiedRandom();
			if (rng.NextFloat() > ComplaintChance)
			{
				return;
			}

			int idx = rng.Next(ComplaintCount) + 1;
			string key = $"Mods.Jarkos.NpcComplaints.Generic{idx}";
			string complaint = Language.GetTextValue(key, npc.GivenOrTypeName);

			if (!string.IsNullOrEmpty(complaint) && !complaint.StartsWith("Mods.Jarkos"))
			{
				chat = complaint;
			}
		}
		private static bool IsNpcInsideAbyss(NPC npc)
		{
			int tileX = npc.homeTileX > 0 ? npc.homeTileX : (int)(npc.Center.X / 16f);
			int tileY = npc.homeTileY > 0 ? npc.homeTileY : (int)(npc.Center.Y / 16f);

			bool nearBuriedCenter = Math.Abs(tileX - Main.maxTilesX / 2) < Main.maxTilesX / 5 && tileY > Main.rockLayer && tileY < Main.UnderworldLayer;

			return nearBuriedCenter && PetrifiedAbyssSystem.generated;
		}
	}
}
