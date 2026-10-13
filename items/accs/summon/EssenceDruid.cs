using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
    public class EssenceDruidPlayer : ModPlayer
    {
        public bool hasEssenceDruid = false;

        public override void ResetEffects()
        {
            hasEssenceDruid = false;
        }

        public override void PostUpdateEquips()
        {
            if (!hasEssenceDruid)
                return;

            bool equippedPapyrus = false;
            bool equippedScroll = false;
            bool equippedBeetle = false;

            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                Item armor = Player.armor[i];
                if (armor != null && !armor.IsAir)
                {
                    if (armor.type == ItemID.PapyrusScarab) equippedPapyrus = true;
                    else if (armor.type == ItemID.NecromanticScroll) equippedScroll = true;
                    else if (armor.type == ItemID.HerculesBeetle) equippedBeetle = true;
                }
            }

            if (equippedPapyrus)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.15f;
                Player.maxMinions -= 1;
                Player.GetKnockback(DamageClass.Summon) -= 2f;
            }
            if (equippedScroll)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.10f;
                Player.maxMinions -= 1;
            }
            if (equippedBeetle)
            {
                Player.GetDamage(DamageClass.Summon) -= 0.15f;
                Player.GetKnockback(DamageClass.Summon) -= 2f;
            }
        }
    }

    public class EssenceDruid : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 10);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<EssenceDruidPlayer>().hasEssenceDruid = true;
            player.GetDamage(DamageClass.Summon) += 0.14f;
            player.maxMinions += 2;
            player.GetKnockback(DamageClass.Summon) += 2f;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.PygmyNecklace, 1)
                .AddIngredient(ItemID.PapyrusScarab, 1)
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
