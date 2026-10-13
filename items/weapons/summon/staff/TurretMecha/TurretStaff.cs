using jarkos.projectiles.summon.staff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.TurretMecha
{
	public class TurretStaff : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item78;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.damage = 80;
            Item.knockBack = 2f;
            Item.shoot = ModContent.ProjectileType<TurretBase>();
            Item.shootSpeed = 0f;
            Item.mana = 50;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = Main.MouseWorld;
            Point tilePos = spawnPos.ToTileCoordinates();
            bool foundGround = false;
            for (int i = 0; i < 20; i++)
            {
                Tile tile = Main.tile[tilePos.X, tilePos.Y + i];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    spawnPos = new Vector2(tilePos.X * 16 + 8, (tilePos.Y + i) * 16 - 40);
                    foundGround = true;
                    break;
                }
            }
            if (!foundGround)
                return false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<TurretBase>())
                {
                    proj.Kill();
                }
            }
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(spawnPos - new Vector2(20), 40, 40, DustID.Electric, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            return false;
        }
	}
}
