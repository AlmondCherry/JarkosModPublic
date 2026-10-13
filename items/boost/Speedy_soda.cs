using System.Collections.Generic;
using jarkos.systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.items.boost
{
	public class Speedy_soda : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item2;
            Item.maxStack = 1;
            Item.consumable = true;
        }
        public override bool CanUseItem(Player player)
        {
            FishingBoostPlayer modPlayer = player.GetModPlayer<FishingBoostPlayer>();
            if (modPlayer.hasForestFishingBoost)
            {
                return false;
            }
            return true;
        }
        public override bool? UseItem(Player player)
        {
            FishingBoostPlayer modPlayer = player.GetModPlayer<FishingBoostPlayer>();
            modPlayer.hasForestFishingBoost = true;
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GrassBlades, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f), 100, default, 1.5f);
                dust.noGravity = true;
            }
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            FishingBoostPlayer modPlayer = player.GetModPlayer<FishingBoostPlayer>();
            if (modPlayer.hasForestFishingBoost)
            {
                tooltips.Add(new TooltipLine(Mod, "AlreadyConsumed", "[c/FF6666:Already used]"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "CanConsume", "[c/66FF66:You can eat this]"));
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 30)
                .AddIngredient(ItemID.Bass, 5)
                .AddIngredient(ItemID.Daybloom, 3)
                .AddIngredient(ItemID.FallenStar, 1)
                .AddTile(TileID.CookingPots)
                .Register();
        }
	}
	public class FishingBoostPlayer : ModPlayer
    {
        public bool hasForestFishingBoost = false;
        public override void SaveData(TagCompound tag)
        {
            if (hasForestFishingBoost)
            {
                tag["ForestFishingBoost"] = true;
            }
        }
        public override void LoadData(TagCompound tag)
        {
            hasForestFishingBoost = tag.ContainsKey("ForestFishingBoost") && tag.GetBool("ForestFishingBoost");
        }
        public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
        {
            if (hasForestFishingBoost && IsInForestSurface())
            {
                fishingLevel *= 1.25f;
            }
        }
        private bool IsInForestSurface()
        {
            return Player.ZoneForest && Player.ZoneOverworldHeight;
        }
        public override void Initialize()
        {
            hasForestFishingBoost = false;
        }
    }
    public class FishingBoostPlayer_Caves : ModPlayer
    {
        public bool hasCaveFishingBoost = false;
        public override void SaveData(TagCompound tag)
        {
            if (hasCaveFishingBoost)
            {
                tag["CaveFishingBoost"] = true;
            }
        }
        public override void LoadData(TagCompound tag)
        {
            hasCaveFishingBoost = tag.ContainsKey("CaveFishingBoost") && tag.GetBool("CaveFishingBoost");
        }
        public override void GetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel)
        {
            if (hasCaveFishingBoost && IsInCave())
            {
                fishingLevel *= 1.25f;
            }
        }
        private bool IsInCave()
        {
            return Player.ZoneNormalUnderground && Player.ZoneNormalCaverns;
        }
        public override void Initialize()
        {
            hasCaveFishingBoost = false;
        }
    }
	public class Speedy_soda_GlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
            if (item.fishingPole <= 0)
                return;
			Player player = Main.LocalPlayer;
            FishingBoostPlayer modPlayer = player.GetModPlayer<FishingBoostPlayer>();
			FishingBoostPlayer_Caves modPlayer2 = player.GetModPlayer<FishingBoostPlayer_Caves>();
            if (modPlayer.hasForestFishingBoost || modPlayer2.hasCaveFishingBoost)
			{
				bool isInForest = player.ZoneForest && player.ZoneOverworldHeight;
				bool isInCave = player.ZoneNormalUnderground && player.ZoneNormalCaverns;
				string bonusPower = "+ " + (item.fishingPole * 0.25f).ToString() + "% ";
				string bonusPowertext = isInForest ? bonusPower : null;
				if(isInForest)
				{
					tooltips.Add(new TooltipLine(Mod, "FruitForest", (string)LangSystem.ForestFruit_desc));
				}
				if(isInCave)
				{
					tooltips.Add(new TooltipLine(Mod, "FruitCaves", (string)LangSystem.CaveFruit_desc));
				}
				foreach (TooltipLine line in tooltips)
				{
					if (line.Mod == "Terraria" && line.Name == "FishingPower")
					{
						line.Text = item.fishingPole.ToString() + "% " + bonusPowertext + LangSystem.fishing_mastery;
					}
				}
			}
		}
	}
}
