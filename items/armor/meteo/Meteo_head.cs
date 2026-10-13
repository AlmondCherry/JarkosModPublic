using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.materials;

namespace jarkos.items.armor.meteo
{
	[AutoloadEquip(EquipType.Head)]
	public class Meteo_head : ModItem
	{
	    public static LocalizedText SetBonusText { get; private set; }
	    public override void SetStaticDefaults()
        {
		    SetBonusText = this.GetLocalization(nameof(SetBonusText));
	    }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 28;
            Item.defense = 16;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
			return body.type == ModContent.ItemType<Meteo_breastplace>() && legs.type == ModContent.ItemType<Meteo_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
			player.setBonus = "Increases max mana by 200, mana regen by 15 and magic attack speed by 20%";
            player.statManaMax2 += 200;
            player.manaRegen += 15;
            player.GetAttackSpeed(DamageClass.Magic) += 0.20f;
            player.setBonus = SetBonusText.Value;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.10f;
            player.GetCritChance(DamageClass.Magic) += 5;
        }
         public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.MeteoriteBar, 20)
			.AddRecipeGroup(nameof(ItemID.TitaniumBar), 10)
            .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 4)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
