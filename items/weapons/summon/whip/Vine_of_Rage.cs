using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.rare;

namespace jarkos.items.weapons.summon.whip
{
	public class Vine_of_Rage : ModItem
	{
		public override void SetStaticDefaults() 
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() 
		{
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 90;
			Item.knockBack = 2;
			Item.shoot = ModContent.ProjectileType<Vine_of_Rage_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.rare = ModContent.RarityType<StormRarity>();
			Item.useTime = 34;
			Item.useAnimation = 34;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

            Item.value = Item.sellPrice(0, 4, 0, 0);
		}
	}
}
