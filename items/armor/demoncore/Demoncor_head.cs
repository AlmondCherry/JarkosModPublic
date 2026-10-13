using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace jarkos.items.armor.demoncore
{
	[AutoloadEquip(EquipType.Head)]
	public class Demoncor_head : ModItem
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
            Item.defense = 10;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
			return body.type == ModContent.ItemType<Demoncor_breast>() && legs.type == ModContent.ItemType<Demoncor_legs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.GetModPlayer<Jarkos_player>();
            player.lavaTime = 1500;
            player.setBonus = SetBonusText.Value;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
            player.maxMinions += 1;
            player.maxTurrets += 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 4)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 10)
			.AddIngredient(ItemID.HellstoneBar, 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
