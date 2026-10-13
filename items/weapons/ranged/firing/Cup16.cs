using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.bullet;
using jarkos.buffs.buff;
namespace jarkos.items.weapons.ranged.firing
{
	public class Cup16 : ModItem
	{
		public override void SetDefaults()
		{
			Item.autoReuse = false;
			Item.useTime = 12;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 46;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 12;
            Item.damage = 11;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = SoundID.Item41;
            Item.shoot = ModContent.ProjectileType<Cup16_round>();
			Item.shootSpeed = 20f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }
        public override bool? UseItem(Player player)
        {
			Item.autoReuse = false;
            return base.UseItem(player);
        }
        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-2f, 3f);
		}
        public override float UseSpeedMultiplier(Player player)
        {
			if(player.HasBuff(ModContent.BuffType<Cupsix_buff>()))
            {
                return 1.50f;
            }
			else
            {
                return 1f;
            }
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
			if(player.HasBuff(ModContent.BuffType<Cupsix_buff>()))
			{
            velocity = velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)));
			}
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 16)
			.AddIngredient(ItemID.Silk, 10)
			.AddIngredient(ItemID.Chain, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
