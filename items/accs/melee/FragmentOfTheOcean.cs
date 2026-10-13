using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;

namespace jarkos.items.accs.melee
{
	public class FragmentOfTheOcean : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 20;
			Item.accessory = true;
            Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.fishingSkill += 25;
            if (Main.raining)
            {
                player.fishingSkill += 50;
            }

            float time = Main.GameUpdateCount / 60f;
            float chaoticLuck = (float)(Math.Sin(time * 1.73) * 0.12 + Math.Sin(time * 4.19) * 0.08 + Math.Sin(time * 7.31 + player.whoAmI) * 0.05);
            player.luck += chaoticLuck;

            player.GetModPlayer<TidalFragmentPlayer>().tidalFragmentEquipped = true;
            player.GetModPlayer<TidalFragmentPlayer>().currentLuck = player.luck;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            float time = Main.GameUpdateCount / 60f;

            float currentEffect = (float)(Math.Sin(time * 1.73) * 0.12 + Math.Sin(time * 4.19) * 0.08 + Math.Sin(time * 7.31 + player.whoAmI) * 0.05);
            string luckText = currentEffect >= 0 ? $"+{currentEffect:P0}" : $"{currentEffect:P0}";

            Color luckColor = currentEffect >= 0 ? Color.LimeGreen : Color.OrangeRed;

            var luckLine = new TooltipLine(Mod, "LuckStatus", $"Current luck: {luckText}")
            {
                OverrideColor = luckColor
            };
            tooltips.Add(luckLine);
        }
	}
	public class TidalFragmentPlayer : ModPlayer
    {
        public bool tidalFragmentEquipped;
        public float currentLuck;
        public override void ResetEffects()
        {
            tidalFragmentEquipped = false;
            currentLuck = 0f;
        }
        public override void ModifyFishingAttempt(ref FishingAttempt attempt)
        {
            if (!tidalFragmentEquipped)
                return;
            float crateBonus = 0.10f + Math.Max(0, currentLuck) * 0.25f;

			if(Main.rand.NextFloat() < crateBonus)
			{
				attempt.crate = true;
			}
        }
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            if (!tidalFragmentEquipped)
                return;
            float crateChance = 0.08f + Math.Clamp(currentLuck * 0.20f, -0.05f, 0.30f);
            if (itemDrop > 0 && !IsCrate(itemDrop) && Main.rand.NextFloat() < crateChance)
            {
                itemDrop = SelectAppropriateCrate(attempt);
            }
        }
        private bool IsCrate(int itemID)
        {
            return 
            itemID == ItemID.WoodenCrate 
            || itemID == ItemID.WoodenCrateHard 
            || itemID == ItemID.IronCrate 
            || itemID == ItemID.IronCrateHard 
            || itemID == ItemID.GoldenCrate 
            || itemID == ItemID.GoldenCrateHard 
            || itemID == ItemID.JungleFishingCrate 
            || itemID == ItemID.JungleFishingCrateHard 
            || itemID == ItemID.FloatingIslandFishingCrate 
            || itemID == ItemID.FloatingIslandFishingCrateHard 
            || itemID == ItemID.CorruptFishingCrate 
            || itemID == ItemID.CorruptFishingCrateHard 
            || itemID == ItemID.CrimsonFishingCrate 
            || itemID == ItemID.CrimsonFishingCrateHard 
            || itemID == ItemID.HallowedFishingCrate 
            || itemID == ItemID.HallowedFishingCrateHard 
            || itemID == ItemID.DungeonFishingCrate 
            || itemID == ItemID.DungeonFishingCrateHard 
            || itemID == ItemID.FrozenCrate 
            || itemID == ItemID.FrozenCrateHard 
            || itemID == ItemID.OasisCrate 
            || itemID == ItemID.OasisCrateHard 
            || itemID == ItemID.OceanCrate 
            || itemID == ItemID.OceanCrateHard 
            || itemID == ItemID.LavaCrate 
            || itemID == ItemID.LavaCrateHard;
        }
        private int SelectAppropriateCrate(FishingAttempt attempt)
        {
            bool hardmode = Main.hardMode;
            int roll = Main.rand.Next(100);
            float luckBonus = Math.Clamp(currentLuck * 20f, -10f, 30f);

            if (roll < 5 + luckBonus)
                return hardmode ? ItemID.GoldenCrateHard : ItemID.GoldenCrate;
            else if (roll < 30 + luckBonus)
                return hardmode ? ItemID.IronCrateHard : ItemID.IronCrate;
            else
                return hardmode ? ItemID.WoodenCrateHard : ItemID.WoodenCrate;
        }
    }
}
