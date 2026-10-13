using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace jarkos.items.usetiles
{
    public class UndergroundDrillItem : ModItem
    {
        public Item[] SavedOutputSlots = new Item[5];

        public UndergroundDrillItem()
        {
            for (int i = 0; i < 5; i++)
            {
                SavedOutputSlots[i] = new Item();
            }
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.maxStack = 1;
            Item.consumable = true;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
            Item.createTile = ModContent.TileType<tiles.UndergroundDrillTile>();
        }

        public override void SaveData(TagCompound tag)
        {
            for (int i = 0; i < 5; i++)
            {
                if (SavedOutputSlots[i] != null && !SavedOutputSlots[i].IsAir)
                {
                    tag[$"savedOutput{i}"] = ItemIO.Save(SavedOutputSlots[i]);
                }
            }
        }

        public override void LoadData(TagCompound tag)
        {
            for (int i = 0; i < 5; i++)
            {
                if (tag.ContainsKey($"savedOutput{i}"))
                {
                    SavedOutputSlots[i] = ItemIO.Load(tag.GetCompound($"savedOutput{i}"));
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            List<string> storedNames = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                if (SavedOutputSlots[i] != null && !SavedOutputSlots[i].IsAir)
                {
                    storedNames.Add($"{SavedOutputSlots[i].HoverName} x{SavedOutputSlots[i].stack}");
                }
            }

            if (storedNames.Count > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "StoredOres", "Contains ore: " + string.Join(", ", storedNames))
                {
                    OverrideColor = Color.Gold
                });
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 20)
                .AddIngredient(ItemID.Wire, 15)
                .AddIngredient(ItemID.Cog, 10)
                .AddIngredient(ItemID.Diamond, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
