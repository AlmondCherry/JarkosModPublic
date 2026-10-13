using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.materials;
using Jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Pyromelt : ModItem
	{
		public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 102;
            Item.height = 122;
            Item.damage = 55;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<Pyromelt_slash>();
            Item.shootSpeed = 1f;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<PyromeltPlayer>();
                if (modPlayer.throwCooldown > 0)
                    return false;

                Item.useTime = 45;
                Item.useAnimation = 45;
                Item.UseSound = SoundID.Item73;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Pyromelt_thrown>()] > 0)
                    return false;
            }
            else
            {
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.UseSound = null;
                if (player.ownedProjectileCounts[ModContent.ProjectileType<Pyromelt_slash>()] > 0)
                    return false;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<PyromeltPlayer>();
                modPlayer.throwCooldown = 3600;

                Vector2 targetPos = Main.MouseWorld;
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<Pyromelt_thrown>(), damage, knockback, player.whoAmI, ai0: targetPos.X, ai1: targetPos.Y);
            }
            else
            {
                var combo = player.GetModPlayer<PyromeltPlayer>();
                int step = combo.UseSwing();
                int angDir = step == 1 ? -1 : 1;
                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
                Projectile.NewProjectile(source, player.MountedCenter, dir, ModContent.ProjectileType<Pyromelt_slash>(), damage, knockback, player.whoAmI, step, angDir);
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 12);
            recipe.AddIngredient(ModContent.ItemType<ResidualShadow>(), 10);
            recipe.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}

    public class PyromeltPlayer : ModPlayer
    {
        public const int COMBO_WINDOW = 70;
        public int comboStep;
        public int comboTimer;
        public int throwCooldown;

        public override void PostUpdate()
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer == 0)
                    comboStep = 0;
            }

            if (throwCooldown > 0)
            {
                throwCooldown--;
            }
        }

        public int UseSwing()
        {
            int step = comboStep;
            comboStep = comboStep >= 1 ? 0 : comboStep + 1;
            comboTimer = COMBO_WINDOW;
            return step;
        }
    }
}
