using jarkos.items.bags;
using jarkos.items.materials;
using jarkos.items.usable;
using jarkos.items.usetiles;
using jarkos.items.weapons.magic.toms;
using jarkos.items.weapons.magic.staffs;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.weapons.ranged.firing;
using jarkos.npc.Celestia;
using jarkos.npc.godofdeath;
using jarkos.npc.Punisher;
using jarkos.npc.tenderote;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Jarkos.items.usetiles;
namespace jarkos.systems
{
	public class BossChecklistSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			Punisher();
			GoD();
			Tenderote();
			Celestia();
		}
		private void Tenderote()
		{
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
			{
				return;
			}
			if (bossChecklistMod.Version < new Version(1, 6))
			{
				return;
			}
			string internalName = "Tenderote";
			float weight = 15.1f;
			Func<bool> downed = () => DownedBossSystem.downedTenderod;
			
			int bossType = ModContent.NPCType<Tenderote_head>();
			int spawnItem = ModContent.ItemType<Tenderote_summon>();

			List<int> collectibles = new List<int>()
			{
				ModContent.ItemType<BlackHole>(), ModContent.ItemType<AntiqueOre>(), ModContent.ItemType<AncientFragment>(), };
			var customPortrait = (SpriteBatch sb, Rectangle rect, Color color) =>
			{
				Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/boss_textures/tenderote").Value;
				Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
				sb.Draw(texture, centered, color);
			};
			bossChecklistMod.Call("LogBoss", Mod, internalName, weight, downed, bossType, new Dictionary<string, object>()
				{
					["spawnItems"] = spawnItem, ["collectibles"] = collectibles, ["customPortrait"] = customPortrait
				});
		}
		private void GoD()
		{
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
			{
				return;
			}
			if (bossChecklistMod.Version < new Version(1, 6))
			{
				return;
			}
			string internalName = "GodOfDeath";
			float weight = 5.7912f;
			Func<bool> downed = () => DownedBossSystem.downedGoD;
			int bossType = ModContent.NPCType<God_of_death>();
			int spawnItem = ModContent.ItemType<Godofdeath_summon>();
			List<int> collectibles = new List<int>()
			{
				ModContent.ItemType<Godofdeath_bag>(),
				ModContent.ItemType<FeatherBlack>(),
				ModContent.ItemType<Plague_slimemachine>(),
				ModContent.ItemType<KnifeSacrifice>(),
				ModContent.ItemType<Plague_magestaff>(),
				ModContent.ItemType<SpearOfDeath>(),
				ModContent.ItemType<GoDRelicItem>()
			};
			var customPortrait = (SpriteBatch sb, Rectangle rect, Color color) =>
			{
				Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/boss_textures/GoD").Value;
				Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
				sb.Draw(texture, centered, color);
			};
			bossChecklistMod.Call("LogBoss", Mod, internalName, weight, downed, bossType, new Dictionary<string, object>()
				{
					["spawnItems"] = spawnItem, ["collectibles"] = collectibles, ["customPortrait"] = customPortrait
				});
		}
		private void Punisher() {
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod)) {
				return;
			}
			if (bossChecklistMod.Version < new Version(1, 6)) {
				return;
			}
			string internalName = "Punisher";
			float weight = 11.5f;
			Func<bool> downed = () => DownedBossSystem.downedPunisher;
			int bossType = ModContent.NPCType<Punisher>();
			int spawnItem = ModContent.ItemType<Punisher_summon>();
			List<int> collectibles = new List<int>()
			{
				ModContent.ItemType<BlackHole>(), ModContent.ItemType<AntiqueOre>(), ModContent.ItemType<AncientFragment>(), };
			var customPortrait = (SpriteBatch sb, Rectangle rect, Color color) => {
				Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/boss_textures/Punisher_texture").Value;
				Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
				sb.Draw(texture, centered, color);
			};
			bossChecklistMod.Call("LogBoss", Mod, internalName, weight, downed, bossType, new Dictionary<string, object>() {
					["spawnItems"] = spawnItem, ["collectibles"] = collectibles, ["customPortrait"] = customPortrait
				});
		}
		private void Celestia() {
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod)) {
				return;
			}
			if (bossChecklistMod.Version < new Version(1, 6)) {
				return;
			}
			string internalName = "Celestia";
			float weight = 16.0f;
			Func<bool> downed = () => DownedBossSystem.downedCelestia;
			int bossType = ModContent.NPCType<jarkos.npc.Celestia.Celestia>();
			int spawnItem = ModContent.ItemType<Celestia_summon>();
			List<int> collectibles = new List<int>()
			{
				ModContent.ItemType<BlackHole>(), ModContent.ItemType<AntiqueOre>(), ModContent.ItemType<AncientFragment>(), };
			var customPortrait = (SpriteBatch sb, Rectangle rect, Color color) => {
				Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/boss_textures/celestia_texture").Value;
				Vector2 centered = new Vector2(rect.X + (rect.Width / 2) - (texture.Width / 2), rect.Y + (rect.Height / 2) - (texture.Height / 2));
				sb.Draw(texture, centered, color);
			};
			bossChecklistMod.Call("LogBoss", Mod, internalName, weight, downed, bossType, new Dictionary<string, object>() {
					["spawnItems"] = spawnItem, ["collectibles"] = collectibles, ["customPortrait"] = customPortrait
				});
		}
	}
}
