using jarkos.items.materials;
using jarkos.rare;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace jarkos.items.armor.miasma
{
    [AutoloadEquip(EquipType.Head)]
    public class MiasmaHelm : ModItem
    {
        public static LocalizedText SetBonusText { get; private set; }
	    public override void SetStaticDefaults()
		{
			SetBonusText = this.GetLocalization(nameof(SetBonusText));
	    }
        public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 22;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ModContent.RarityType<StormRarity>();
			Item.defense = 10;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<MiasmaBreastplate>()
				&& legs.type == ModContent.ItemType<MiasmaGreaves>();
		}

		public override void UpdateArmorSet(Player player)
		{
			player.GetModPlayer<MiasmaPlayer>().miasmaSet = true;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.08f;
		}

		public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpectreBar, 6)
                .AddIngredient(ModContent.ItemType<Vileplasm>(), 2)
                .AddIngredient(ModContent.ItemType<BulbSunEnergy>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
