using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Jarkos.items.materials;
using jarkos.items.materials;

namespace jarkos.items.armor.panel
{
	[AutoloadEquip(EquipType.Head)]
	public class Panel_head : ModItem
	{
	    public static LocalizedText SetBonusText { get; private set; }
	    public override void SetStaticDefaults()
        {
		    SetBonusText = this.GetLocalization(nameof(SetBonusText));
	    }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.defense = 10;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
			return body.type == ModContent.ItemType<Panel_breast>() && legs.type == ModContent.ItemType<Panel_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = SetBonusText.Value;
            player.GetModPlayer<Jarkos_player>().diffusalSet = true;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.04f;
            player.GetCritChance(DamageClass.Generic) += 5f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<DiffusalBar>(), 6)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
