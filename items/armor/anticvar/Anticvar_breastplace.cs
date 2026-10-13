using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using jarkos.rare;

namespace jarkos.items.armor.anticvar
{
	[AutoloadEquip(EquipType.Body)]
	public class Anticvar_breastplace : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 26;
            Item.defense = 48;
            Item.rare = ModContent.RarityType<SpeedRarity>();
        }
        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.30f;
            if(player.velocity == Vector2.Zero)
            {
            	player.GetDamage(DamageClass.Magic) += 0.10f;
            }
           	else
            {
            	player.endurance += 0.15f;
           	}
            player.lifeRegen += 15;
            player.manaRegen += 15;
        }
        public override void AddRecipes()
        {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 60)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 60)
            .AddIngredient(ModContent.ItemType<ElementalFluid>(), 420)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
