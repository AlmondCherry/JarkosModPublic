using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using jarkos.projectiles.ranged.other;
using jarkos.items.materials;
using jarkos.items.usetiles;
using Terraria.GameContent.ItemDropRules;
using jarkos.items.accs.melee;
using Terraria.Localization;
using jarkos.buffs.buff;
namespace jarkos
{
	public class Jarkos_globalitem : GlobalItem
	{
		#region SetDefaults
		public override void SetDefaults(Item entity)
		{
			if (entity.DamageType == DamageClass.Melee && ModContent.GetInstance<Heavenly_flame_player>().Heavenly_flame_accs)
			{
				entity.mana = 2 + (entity.damage * 25 / 100);
			}
		}
		#endregion

		#region ExtractinatorUse
		public void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
		{
		}
		#endregion
		#region ModifyItemLoot
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			if (item.type == ItemID.WoodenCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.BandofRegeneration, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.BandofStarpower, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.GoldWatch, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.SilverWatch, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.TinWatch, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.CopperWatch, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.DepthMeter, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.Compass, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.TallyCounter, 8, 1, 1));
			}
			if (item.type == ItemID.IronCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.AdhesiveBandage, 6, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.PocketMirror, 6, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.CobaltShield, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.LifeformAnalyzer, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.Radar, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.MetalDetector, 8, 1, 1));
			}
			if (item.type == ItemID.GoldenCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.CobaltShield, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.DPSMeter, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.Stopwatch, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.Sextant, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.WeatherRadio, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.FishermansGuide, 8, 1, 1));
			}
			if (item.type == ItemID.OceanCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.TackleBox, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.AnglerEarring, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.HighTestFishingLine, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.LavaFishingHook, 8, 1, 1));
			}
			if (item.type == ItemID.OasisCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.Blindfold, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.Megaphone, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.TrifoldMap, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.FastClock, 8, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.PocketMirror, 8, 1, 1));
			}
			if (item.type == ItemID.CrimsonFishingCrate || item.type == ItemID.CorruptFishingCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.Vitamins, 4, 1, 1));
			}
			if (item.type == ItemID.DungeonFishingCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.Nazar, 4, 1, 1));
				itemLoot.Add(ItemDropRule.Common(ItemID.ArmorPolish, 4, 1, 1));
			}
			if (item.type == ItemID.JungleFishingCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.Bezoar, 4, 1, 1));
			}
			if (item.type == ItemID.FrozenCrate)
			{
				itemLoot.Add(ItemDropRule.Common(ItemID.HandWarmer, 4, 1, 1));
			}
		}
		#endregion
		#region Shoot
		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			if (item.type == ItemID.AmethystStaff && modPl.gemstaffs_buff == true || item.type == ItemID.AmethystStaff && modPl.amethyst_shard)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.AmethystBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.type == ItemID.TopazStaff && modPl.gemstaffs_buff == true || item.type == ItemID.TopazStaff && modPl.topaz_shard)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.TopazBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.type == ItemID.SapphireStaff && modPl.gemstaffs_buff == true || item.type == ItemID.SapphireStaff && modPl.sapphire_shard)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.SapphireBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.type == ItemID.EmeraldStaff && modPl.gemstaffs_buff == true || item.type == ItemID.EmeraldStaff && modPl.emerald_shard)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.EmeraldBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.type == ItemID.RubyStaff && modPl.gemstaffs_buff == true || item.type == ItemID.RubyStaff && modPl.ruby_shard)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.RubyBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.type == ItemID.DiamondStaff && modPl.gemstaffs_buff == true || item.type == ItemID.DiamondStaff && modPl.diamond_shard)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.DiamondBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.type == ItemID.AmberStaff && modPl.gemstaffs_buff == true)
			{
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.position) * 16f;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(source, position: pos, vel / i, ProjectileID.AmberBolt, 10, 1, player.whoAmI);
				}
			}
			if (item.DamageType == DamageClass.Ranged && player.GetModPlayer<Jarkos_player>().rocketmk1 == true)
			{
				var dmg = (item.damage * 40) / 100;
				Vector2 pos = player.Center;
				Vector2 vel = Vector2.Normalize(Main.MouseWorld - player.Center) * 16f;
				Projectile.NewProjectile(source, position: pos, vel / 3, ModContent.ProjectileType<Rocket_mk1>(), dmg, 1, player.whoAmI);
			}
			return true;
		}
		#endregion
		#region OnHitNPC
		public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (player.GetModPlayer<Jarkos_player>().goldengorn == true && item.DamageType == DamageClass.Melee)
			{
				player.GetModPlayer<Jarkos_player>().goldengornstrength++;
			}
			else
			{
				player.GetModPlayer<Jarkos_player>().goldengornstrength = 1;
			}
			if (player.GetModPlayer<Jarkos_player>().bloodyamulet == true && item.DamageType == DamageClass.Melee)
			{
				player.Heal(Main.rand.Next(1, 5));
			}
		}
		#endregion
		#region ModifyShootStats
		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			if (type == ProjectileID.Bullet && modPl.rifle_scope02)
			{
				type = ProjectileID.CrystalBullet;
			}
			if (type == ProjectileID.WoodenArrowFriendly && modPl.rifle_scope02)
			{
				type = ProjectileID.HolyArrow;
			}
			if (type == ProjectileID.Bullet && modPl.rifle_scope03)
			{
				type = ProjectileID.MoonlordBullet;
			}
			if (type == ProjectileID.WoodenArrowFriendly && modPl.rifle_scope03)
			{
				type = ProjectileID.MoonlordArrow;
			}
		}
		#endregion
		#region CanUseItem
		public override bool CanUseItem(Item item, Player player)
		{
			if (item.DamageType == DamageClass.Melee && player.GetModPlayer<Heavenly_flame_player>().Heavenly_flame_accs)
			{
				int mana_usage = 2 + (item.damage * 25 / 100);
				if (mana_usage > 80)
				{
					mana_usage = 80;
				}
				player.statMana -= mana_usage;
				player.manaRegenDelay = 60;
				if (player.statMana < 10)
				{
					player.statMana = -1;
					if (player.manaFlower)
					{
						player.QuickMana();
						player.AddBuff(ModContent.BuffType<Mana_melee_sickness>(), 1200);
					}
					return false;
				}
			}
			return true;
		}
		#endregion
		#region UseItem
        #endregion
		#region ModifyTooltips
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			Player player = Main.LocalPlayer;
			int mana_usage = 2 + (item.damage * 25 / 100);
			if (mana_usage > 80)
			{
				mana_usage = 80;
			}
			if (item.DamageType == DamageClass.Melee && player.GetModPlayer<Heavenly_flame_player>().Heavenly_flame_accs)
			{
				tooltips.Add(new TooltipLine(Mod, "ShiftHint", (string)Mana_Usage_Tooltip.manausage_melee_text + " " + mana_usage));
			}
			#region Fishing Rod Tooltip
            #endregion
		}
		#endregion
	}
	#region Mana_Usage_Tooltip
	public class Mana_Usage_Tooltip : ModSystem
	{
		public static LocalizedText manausage_melee_text { get; private set; }
		public override void Load()
		{
			string category = "ManaUsageMeleeTooltip";
			manausage_melee_text ??= Mod.GetLocalization($"{category}.manausage_melee_text");
		}
	}
	#endregion
	#region Reactor_Hint_Tooltip
	public class Reactor_Hint_Tooltip : ModSystem
	{
		public static LocalizedText reactor_hint_text { get; private set; }
		public static LocalizedText reactor_hint_text2 { get; private set; }
		public override void Load()
		{
			string category = "ReactorHintTooltip";
			reactor_hint_text ??= Mod.GetLocalization($"{category}.reactor_hint_text");
			reactor_hint_text2 ??= Mod.GetLocalization($"{category}.reactor_hint_text2");
		}
	}
	#endregion
}
