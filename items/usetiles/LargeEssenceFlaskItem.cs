using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using jarkos.core.Data;
using jarkos.tiles;
using jarkos.items.materials;
using System.IO;

namespace jarkos.items.usetiles
{
    public class LargeEssenceFlaskItem : ModItem
    {
        public Dictionary<EssenceType, int> StoredEssences = new Dictionary<EssenceType, int>();
        public const int MaxCapacity = 10000;
        public const int MaxTypes = 5;
        public int TotalEssences => StoredEssences.Values.Sum();
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<LargeEssenceFlaskTile>();
            Item.value = Item.buyPrice(0, 0, 0, 5);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (StoredEssences.Count > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "FlaskStorageHeader", $"Stored Essences ({TotalEssences}/{MaxCapacity}):"));
                foreach (var kvp in StoredEssences)
                {
                    if (kvp.Value > 0)
                    {
                        Color color = EssencePalette.ColorOf(kvp.Key);
                        string name = EssencePalette.DisplayName(kvp.Key);
                        tooltips.Add(new TooltipLine(Mod, "FlaskStorage_" + name, $"- {name}: {kvp.Value}") { OverrideColor = color });
                    }
                }
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "FlaskStorage", $"Empty Large Flask (Max: {MaxCapacity})") { OverrideColor = Color.Gray });
            }
        }
        public override void SaveData(TagCompound tag)
        {
            var keys = StoredEssences.Keys.Select(k => (int)k).ToList();
            var vals = StoredEssences.Values.ToList();
            tag["essKeys"] = keys;
            tag["essVals"] = vals;
        }
        public override void LoadData(TagCompound tag)
        {
            StoredEssences.Clear();
            if (tag.ContainsKey("essKeys"))
            {
                var keys = tag.GetList<int>("essKeys");
                var vals = tag.GetList<int>("essVals");
                for (int i = 0; i < keys.Count && i < vals.Count; i++)
                    StoredEssences[(EssenceType)keys[i]] = vals[i];
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(StoredEssences.Count);
            foreach (var kvp in StoredEssences)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }
        }
        public override void NetReceive(BinaryReader reader)
        {
            StoredEssences.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                EssenceType type = (EssenceType)reader.ReadInt32();
                int amount = reader.ReadInt32();
                StoredEssences[type] = amount;
            }
        }

        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ItemID.Glass, 225)
			.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 2)
            .AddIngredient(ModContent.ItemType<PulsatingFragment>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
