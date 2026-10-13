using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using System;

namespace jarkos.items.accs.special
{
	public class ChargerHealth : ModItem
	{
		public override void SetStaticDefaults()
		{
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 7));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 46;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ChargerHealthPlayer>().chargerHealthEquipped = true;
		}
	}

	public class ChargerHealthPlayer : ModPlayer
	{
		public bool chargerHealthEquipped = false;
		public int chargerHealthCooldown = 0;
		public const int CooldownTime = 900;

		public int activeTimer = 0;
		public int manaRampStep = 0;

		public override void ResetEffects()
		{
			chargerHealthEquipped = false;
		}

		public override void PostUpdate()
		{
			if (chargerHealthCooldown > 0)
				chargerHealthCooldown--;

			if (!chargerHealthEquipped)
			{
				activeTimer = 0;
				return;
			}

			if (activeTimer > 0)
			{
				activeTimer--;
				
				if (Main.rand.NextBool(2))
				{
					Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
					Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.BlueTorch, speed, 0, default, 1.6f);
					d.noGravity = true;
				}

				if (activeTimer % 60 == 0)
				{
					if (Player.statMana < 20)
					{
						activeTimer = 0;
						chargerHealthCooldown = CooldownTime;
						SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = -0.3f }, Player.Center);
						CombatText.NewText(Player.Hitbox, Colors.RarityRed, "Low Mana!", false, false);
						return;
					}

					int manaCost = 15 + manaRampStep * 5;
					manaRampStep++;

					Player.statMana = Math.Max(0, Player.statMana - manaCost);
					Player.manaRegenDelay = 180;

					Player.statLife += 5;
					if (Player.statLife > Player.statLifeMax2)
						Player.statLife = Player.statLifeMax2;
					Player.HealEffect(5);

					SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f, Pitch = 0.2f }, Player.Center);
				}

				if (activeTimer == 0)
				{
					chargerHealthCooldown = CooldownTime;
				}
			}
		}

		public void ActivateChargerHealth()
		{
			if (chargerHealthEquipped && chargerHealthCooldown <= 0 && activeTimer <= 0)
			{
				if (Player.statMana < 20)
				{
					CombatText.NewText(Player.Hitbox, Colors.RarityRed, "Low Mana!", false, false);
					return;
				}

				activeTimer = 600;
				manaRampStep = 0;
				
				SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f, Pitch = 0.2f }, Player.Center);
				CombatText.NewText(Player.Hitbox, Colors.RarityBlue, "Charger Active", false, false);
			}
		}
	}
}
