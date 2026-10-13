using System;
using jarkos.buffs.debuff;
using jarkos.core.PlayerInteract;
using jarkos.cradle_biome;
using jarkos.tiles.CradleTiles;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace PetrifiedAbyss.Common.Players
{
	public class AbyssSicknessPlayer : ModPlayer
	{
		public const int MaxStack = 95;
		private const int BuildUpTicks = 90;
		private const int DecayTicks = BuildUpTicks / 3;

		public int SicknessStack;
		private int buildUpTimer;
		private int decayTimer;

		public override void SaveData(TagCompound tag)
		{
			if (SicknessStack > 0) {
				tag["sicknessStack"] = SicknessStack;
				tag["buildUpTimer"] = buildUpTimer;
				tag["decayTimer"] = decayTimer;
			}
		}

		public override void LoadData(TagCompound tag)
		{
			SicknessStack = tag.GetInt("sicknessStack");
			buildUpTimer = tag.GetInt("buildUpTimer");
			decayTimer = tag.GetInt("decayTimer");
		}

		private bool HasLanternEquipped()
		{
			if (Player == null || !Player.active) return false;
			int lanternType = ModContent.ItemType<jarkos.items.usable.CradleLanternItem>();

			for (int i = 3; i < 10; i++)
			{
				if (Player.armor[i] != null && Player.armor[i].type == lanternType)
					return true;
			}
			return false;
		}

		public override void PostUpdateMiscEffects()
		{
			if (Main.dedServ)
			{
				return;
			}

			if (Player.HasBuff(ModContent.BuffType<BundleOfMedicinesBuff>()))
			{
				SicknessStack = 0;
				buildUpTimer = 0;
				decayTimer = 0;
				ApplyOrRefreshBuff();
				return;
			}

			bool effectiveBiome = IsEffectiveBiome();
			int currentBuildUpTicks = BuildUpTicks;
			int currentDecayTicks = DecayTicks;

			if (Player.TryGetModPlayer(out ResearchPlayer rp) && rp.IsCompleted("jarkos:UmbraNode4"))
			{
				currentBuildUpTicks = (int)(BuildUpTicks * 1.25f);
				currentDecayTicks = (int)(DecayTicks * 0.4f);
			}

			if (HasLanternEquipped())
			{
				currentBuildUpTicks = (int)(currentBuildUpTicks * 2.20f);
			}

			if (effectiveBiome)
			{
				decayTimer = 0;
				if (SicknessStack < MaxStack)
				{
					if (SicknessStack <= 0)
					{
						SicknessStack = 1;
						buildUpTimer = 0;
					}
					else
					{
						buildUpTimer++;
						if (buildUpTimer >= currentBuildUpTicks)
						{
							buildUpTimer = 0;
							SicknessStack++;

							if (SicknessStack > MaxStack)
							{
								SicknessStack = MaxStack;
							}
						}
					}
				}
				else
				{
					buildUpTimer = 0;
				}
			}
			else if (SicknessStack > 0)
			{
				buildUpTimer = 0;
				decayTimer++;

				if (decayTimer >= currentDecayTicks)
				{
					decayTimer = 0;
					SicknessStack--;

					if (SicknessStack <= 0)
					{
						SicknessStack = 0;
					}
				}
			}
			else
			{
				buildUpTimer = 0;
				decayTimer = 0;
			}
			ApplyOrRefreshBuff();
			ApplySicknessStatPenalties();
		}

		private bool IsEffectiveBiome()
		{
			if (Main.gameMenu || Player is null || !Player.active || Player.dead)
			{
				return false;
			}
			if (!Player.InModBiome<PetrifiedAbyssBiome>())
			{
				return false;
			}
			if (CorruptionNeutralizerSystem.IsActiveWithin(Player.Center, CorruptionNeutralizerTileEntity.RadiusTiles))
			{
				return false;
			}
			return true;
		}

		private void ApplyOrRefreshBuff()
		{
			int buffType = ModContent.BuffType<PetrifiedSicknessBuff>();
			int idx = Player.FindBuffIndex(buffType);
			if (SicknessStack > 0)
			{
				if (idx == -1)
				{
					Player.AddBuff(buffType, 60 * 60, quiet: true);
				}
				else
				{
					Player.buffTime[idx] = 60 * 60;
				}
			}
			else if (idx >= 0)
			{
				Player.DelBuff(idx);
			}
		}

		private void ApplySicknessStatPenalties()
		{
			if (SicknessStack <= 0)
			{
				return;
			}
			
			int stack = Math.Clamp(SicknessStack, 0, MaxStack);
			int reduction = (int)(Player.statLifeMax2 * stack / 100f);
			Player.statLifeMax2 -= reduction;

			if (Player.statLifeMax2 < 1) 
			{
				Player.statLifeMax2 = 1;
			}

			if (Player.statLife > Player.statLifeMax2) 
			{
				Player.statLife = Player.statLifeMax2;
			}
			
			Player.lifeRegen -= (int)(Player.lifeRegen * stack / 200f) * 2;
			if (stack >= 50 && Player.lifeRegen > 0) 
			{
				Player.lifeRegen -= (stack - 49) / 5;
			}
		}
	}
}
