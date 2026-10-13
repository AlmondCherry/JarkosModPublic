using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.projectiles.melee.true_melee;
using jarkos.projectiles.melee.unique_melee;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Mage_slayer : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 157;
			Item.DamageType = DamageClass.Melee;
			Item.width = 108;
			Item.height = 54;
			Item.useTime = 24;
			Item.crit = 30;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 0;
			Item.rare = ModContent.RarityType<jarkos.rare.StormRarity>();
			Item.autoReuse = true;
			Item.shootSpeed = 30;
			Item.shoot = ModContent.ProjectileType<Mage_slayer_proj>();
			Item.mana = 0;
            Item.value = Item.sellPrice(0, 3, 50, 0);
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2)
			{
				Item.UseSound = new SoundStyle?(Jarkos_sounds.mage_slayer_shoot);
				Item.useTime = 36;
				Item.useAnimation = 36;
				Item.damage = 100;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.shoot = ModContent.ProjectileType<Mage_slayer_alt_attack>();
				Item.shootSpeed = 24f;
				Item.autoReuse = false;
				Item.mana = 20;
			}
			else
			{
				Item.UseSound = new SoundStyle?(Jarkos_sounds.mage_slayer_shoot);
				Item.useAnimation = 12;
				Item.useTime = 6;
				Item.reuseDelay = 24;
				Item.damage = 100;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.shoot = ModContent.ProjectileType<Mage_slayer_proj>();
				Item.shootSpeed = 16f;
				Item.autoReuse = false;
				Item.mana = 0;
			}
			return base.CanUseItem(player);
		}
    }
}
