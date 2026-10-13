using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Cirno
{
	public class Cirno_Idiotic_Spectre : ModItem
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/Cirno/Idiotic_Spectre";

		public override void SetStaticDefaults()
		{
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 172;
			Item.knockBack = 2.5f;
			Item.mana = 10;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = null;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = 0;
			Item.shoot = ProjectileID.None;
			Item.shootSpeed = 10f;
			Item.autoReuse = false;

			Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(gold: 2);
		}

		public override bool CanUseItem(Player player)
		{
			int minionType = ModContent.ProjectileType<Cirno_minion>();
			Projectile existingProj = null;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == minionType)
				{
					existingProj = p;
					break;
				}
			}

			float currentMinionSlots = existingProj != null ? existingProj.minionSlots : 0f;
			float otherMinionsSlots = Math.Max(0f, player.slotsMinions - currentMinionSlots);
			float availableForCirno = player.maxMinions - otherMinionsSlots;

			if (existingProj == null)
			{
				return availableForCirno >= 1f;
			}

			int currentLvl = (int)existingProj.ai[0];

			if (currentLvl >= 4)
			{
				return false;
			}

			int nextLvl = currentLvl + 1;
			float requiredForNextLvl = nextLvl * 1f;

			return availableForCirno >= requiredForNextLvl;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer) return true;

			int minionType = ModContent.ProjectileType<Cirno_minion>();
			Projectile existingProj = null;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == minionType)
				{
					existingProj = p;
					break;
				}
			}

			float currentMinionSlots = existingProj != null ? existingProj.minionSlots : 0f;
			float otherMinionsSlots = Math.Max(0f, player.slotsMinions - currentMinionSlots);
			float availableForCirno = player.maxMinions - otherMinionsSlots;

			if (existingProj == null)
			{
				if (availableForCirno >= 1f)
				{
					player.AddBuff(ModContent.BuffType<Cirno_buff>(), 3600);
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/baka_cirno") with { Pitch = 0.0f }, player.position);

					var source = player.GetSource_ItemUse(Item);
					Projectile proj = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, minionType, Item.damage, Item.knockBack, player.whoAmI, ai0: 1f);
					proj.originalDamage = Item.damage;
					proj.minionSlots = 1f;
				}
			}
			else
			{
				int currentLvl = (int)existingProj.ai[0];
				if (currentLvl < 4)
				{
					int nextLvl = currentLvl + 1;
					float requiredSlotsForNextLvl = nextLvl * 1f;

					if (availableForCirno >= requiredSlotsForNextLvl)
					{
						player.AddBuff(ModContent.BuffType<Cirno_buff>(), 3600);

						float soundPitch = (nextLvl - 1) * 0.1f;
						SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/baka_cirno") with { Pitch = soundPitch }, player.position);

						existingProj.ai[0] = nextLvl;
						existingProj.minionSlots = requiredSlotsForNextLvl;
						existingProj.netUpdate = true;
					}
				}
			}

			return true;
		}
	}
}
