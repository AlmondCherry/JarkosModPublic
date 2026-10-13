using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.items.weapons.summon.staff.Defender;
using jarkos.rare;

namespace jarkos.items.armor.anticvar
{
	[AutoloadEquip(EquipType.Head)]
	public class Anticvar_head : ModItem
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
            Item.defense = 36;
            Item.rare = ModContent.RarityType<SpeedRarity>();
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
			return body.type == ModContent.ItemType<Anticvar_breastplace>() && legs.type == ModContent.ItemType<Anticvar_boots>();
        }
        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.GetModPlayer<Jarkos_player>();
			player.setBonus = "Summon own defender";

            if (player.ownedProjectileCounts[ModContent.ProjectileType<Defender_01>()] < 1)
            {
                Projectile.NewProjectile(null, player.position.X, player.position.Y, 2, 2, ModContent.ProjectileType<Defender_01>(), 325, 0f, player.whoAmI, 0f, 0f);
			}

            player.AddBuff(ModContent.BuffType<Defender01buff>(), 3);
            modPlayer.anticvarSet = true;

            player.setBonus = SetBonusText.Value;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.10f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
            player.maxMinions += 2;
            player.maxTurrets += 2;
            player.ammoCost80 = true;
            player.manaCost -= 0.05f;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 40)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 40)
            .AddIngredient(ModContent.ItemType<ElementalFluid>(), 240)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
