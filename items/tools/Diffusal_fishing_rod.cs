using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.projectiles.bobbers;
namespace jarkos.items.tools
{
	public class Diffusal_fishing_rod : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.fishingPole = 30;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<Diffusal_orange_bobber>();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 2, silver: 50);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int bobberCount = 5;
            float spreadAngle = 15f;
            for (int i = 0; i < bobberCount; i++)
            {
                if(Main.rand.NextBool(2))
                {
                    type = ModContent.ProjectileType<Diffusal_blue_bobber>();
                }
                else
                {
                    type = ModContent.ProjectileType<Diffusal_orange_bobber>();
                }
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(spreadAngle));
                newVelocity *= 1f - Main.rand.NextFloat(0.1f);
                Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
	}
}
