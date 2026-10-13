using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace jarkos.items.usable
{
    public class BundleOfMedicines : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3]
            {
                new Color(100, 220, 150), new Color(150, 240, 180), new Color(200, 255, 200)
            };
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 5, 0);
            Item.buffType = ModContent.BuffType<buffs.debuff.BundleOfMedicinesBuff>();
            Item.buffTime = 10800;
        }
    }
}
