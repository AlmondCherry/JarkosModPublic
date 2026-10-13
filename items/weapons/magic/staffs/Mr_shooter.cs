using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
namespace jarkos.items.weapons.magic.staffs
{
	public class Mr_shooter : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.useTime = 7;
			Item.DamageType = DamageClass.Magic;
            Item.width = 39;
            Item.height = 41;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 7;
            Item.damage = 12;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
            Item.mana = 4;
			Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Mushroom;
			Item.shootSpeed = 100f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Vector2 vel = 36f * Vector2.Normalize(velocity).RotatedBy((float)Math.PI / 6f * (Main.rand.NextBool() ? 1f : (-1f)));
            Projectile.NewProjectile(source, position, vel, type = 131, damage, knockback, player.whoAmI);
            return true;
        }
	}
}
