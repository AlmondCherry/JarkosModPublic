using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;

namespace jarkos.items.weapons.melee.true_melee
{
    public class Fiend_wings : ModItem
    {
       public override void SetDefaults() {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 140;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.knockBack = 6.5f;
            Item.shoot = ModContent.ProjectileType<AegisSwing>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player) {
            if (player.altFunctionUse == 2) {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<AegisDrone>()] > 0 ||
                    player.ownedProjectileCounts[ModContent.ProjectileType<AegisScan>()] > 0)
                    return false;
                Item.useTime = Item.useAnimation = 160;
                Item.UseSound = null;
            }
            else {
                Item.useTime = Item.useAnimation = 28;
                Item.UseSound = SoundID.Item1;
            }
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback) {
            if (player.altFunctionUse == 2) {
                Projectile.NewProjectile(source, player.Center, Vector2.Zero,
                    ModContent.ProjectileType<AegisScan>(), damage, 0f, player.whoAmI);
            }
            else {
                int dir = player.GetModPlayer<AegisPlayer>().NextSwingDirection();
                Projectile.NewProjectile(source, player.Center, velocity,
                    ModContent.ProjectileType<AegisSwing>(), damage, knockback, player.whoAmI, 0f, dir);
            }
            return false;
        }
	}
    public class AegisPlayer : ModPlayer
    {
        public int swingCombo;
        public int NextSwingDirection() {
            swingCombo ^= 1;
            return swingCombo == 0 ? 1 : -1;
        }
    }
}
