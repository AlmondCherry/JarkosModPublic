using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.unique_melee;

namespace jarkos.items.weapons.melee.other_melee
{
	public class GloveOfCelestia : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 280;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.knockBack = 9f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.5f;
            Item.shoot = ModContent.ProjectileType<Glove_of_celestia_held>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<jarkos.rare.CosmicRarity>();
        }
	}
}
