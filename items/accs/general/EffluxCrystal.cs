using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.items.usetiles;
using jarkos.particles;

namespace jarkos.items.accs.general
{
	public class EffluxCrystal : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 70;
			Item.height = 70;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Efflux_Crystal_ModPlayer>();
			modPl.Efflux_Crystal = true;

			player.GetDamage(DamageClass.Generic) += 0.05f;
			player.statLifeMax2 += player.statDefense * 5;
			player.lifeRegen += 8;

			if (Main.bloodMoon)
			{
				player.GetDamage(DamageClass.Generic) += 0.10f;
				player.statDefense += 10;
			}
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			var jarkosplayer = player.GetModPlayer<Jarkos_player>();
			var distortedStone = player.GetModPlayer<Distorted_stone_heal>();
			
			if (!distortedStone.Distorted_stone && !jarkosplayer.bleeding_moon_amulet)
			{
				return true;
			}
			return false;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<DistortedStone>(), 1)
			.AddIngredient(ModContent.ItemType<BleedingMoonAmulet>(), 1)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
			.AddIngredient(ItemID.SoulofNight, 4)
			.AddIngredient(ItemID.SoulofLight, 4)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public class Efflux_Crystal_ModPlayer : ModPlayer
		{
			public bool Efflux_Crystal = false;
			public override void ResetEffects()
			{
				Efflux_Crystal = false;
			}
			public override void UpdateDead()
			{
				ResetEffects();
			}
			public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
			{
				if (quickHeal && Efflux_Crystal)
				{
					healValue += healValue * 75 / 100;
				}
			}
			public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
			{
				if (Efflux_Crystal && Main.rand.NextBool(12))
				{
					new Circleedge(target.Center, Vector2.Zero, Color.Red, 120, 0.35f, 2f, 0f).Spawn();
					Player.Heal(Main.rand.Next(1, 4));
				}
			}
		}
	}
}
