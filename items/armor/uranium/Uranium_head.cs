using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.usetiles;

namespace jarkos.items.armor.uranium
{
	[AutoloadEquip(EquipType.Head)]
	public class Uranium_head : ModItem
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
            Item.defense = 16;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
			return body.type == ModContent.ItemType<Uranium_breast>() && legs.type == ModContent.ItemType<Uranium_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
			player.setBonus = "Increases move speed by 10%, luck by 25%, gives effect of flower boots";
            player.flowerBoots = true;
            player.moveSpeed += 0.10f;
            player.luck += 0.25f;
            player.setBonus = SetBonusText.Value;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.15f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 8)
            .AddIngredient(ItemID.SoulofNight, 6)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
