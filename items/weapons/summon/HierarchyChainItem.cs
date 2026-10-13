using jarkos.projectiles.summon;
using Jarkos.items.materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon
{
    public class HierarchyChainItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 28;
            Item.DamageType = DamageClass.Summon;
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<HierarchyChainProjectile>();
            Item.shootSpeed = 0f;
            Item.mana = 10;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[type] == 0)
            {
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Chain, 50);
            recipe.AddIngredient(ModContent.ItemType<ResidualShadow>(), 8);
            recipe.AddIngredient(ItemID.ManaCrystal, 1);
            recipe.AddIngredient(ItemID.SoulofNight, 8);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
    public class HierarchyChainPlayer : ModPlayer
    {
        public const float SummonDamageBonus = 1.00f;
        public override void PostUpdateEquips()
        {
            int controllerType = ModContent.ProjectileType<HierarchyChainProjectile>();
            if (Player.ownedProjectileCounts[controllerType] == 0)
            {
                return;
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != controllerType || p.owner != Player.whoAmI)
                {
                    continue;
                }
                if (p.ModProjectile is HierarchyChainProjectile chain && chain.HasChainedEnemies)
                {
                    Player.GetDamage(DamageClass.Summon) += SummonDamageBonus;
                    break;
                }
            }
        }
    }
}
