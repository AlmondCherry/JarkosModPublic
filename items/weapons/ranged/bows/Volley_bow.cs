using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.ranged.bow;
using Terraria.DataStructures;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.ranged.bows
{
	public class Volley_bow : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 52;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 28;
            Item.height = 52;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item125;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BloodRainBeamProj>();
            Item.shootSpeed = 1f;
            Item.useAmmo = AmmoID.None;
            Item.channel = false;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 beamDir = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitX);
            int projType = ModContent.ProjectileType<BloodRainBeamProj>();

            Projectile.NewProjectile(source, position, beamDir, projType, damage, knockback, player.whoAmI);
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.ZoneCrimson)
            {
                damage = (int)(damage * 1.15f);
            }
        }

		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.WoodenBow, 1)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
