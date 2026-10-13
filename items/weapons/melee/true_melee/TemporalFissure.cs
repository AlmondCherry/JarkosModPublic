using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.melee.true_melee;

namespace jarkos.items.weapons.melee.true_melee
{
	public class TemporalFissure : ModItem
	{
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
		public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.autoReuse = true;
            Item.DamageType = DamageClass.Magic;
            Item.damage = 60;
            Item.knockBack = 3f;
            Item.mana = 12;
            Item.noMelee = true;

            Item.rare = ModContent.RarityType<jarkos.rare.StormRarity>();
            Item.value = Item.sellPrice(0, 5, 0, 0);

            Item.shoot = ModContent.ProjectileType<TemporalFissure_proj>();
            Item.shootSpeed = 15f;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
        }
	}
}
