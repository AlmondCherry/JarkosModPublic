using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
    public class FrostFront : ModItem
    {
        private int swingCounter;
        public const int MaxActiveSeeds = 15;
        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 58;
            Item.damage = 80;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 42;
            Item.useAnimation = 42;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8.2f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FrostFront_proj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 50, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                return CountActiveSeeds(player) > 0;
            }
            return true;
        }
        public override float UseSpeedMultiplier(Player player)
        {
            return player.altFunctionUse == 2 ? 2.4f : 1f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                DetonateAllSeeds(player);
                return false;
            }
            swingCounter++;
            float swingDir = swingCounter % 2 == 0 ? 1f : -1f;
            bool canSpawnSeeds = CountActiveSeeds(player) < MaxActiveSeeds;
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, ai0: canSpawnSeeds ? 1f : 0f, ai1: swingDir);
            return false;
        }
        private static int CountActiveSeeds(Player player)
        {
            int count = 0;
            int seedType = ModContent.ProjectileType<FrostSeed_proj>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == seedType)
                    count++;
            }
            return count;
        }
        private static void DetonateAllSeeds(Player player)
        {
            int seedType = ModContent.ProjectileType<FrostSeed_proj>();
            bool anyDetonated = false;
            int delayStep = 0;
            
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != seedType)
                    continue;

                var seed = p.ModProjectile as FrostSeed_proj;

                if (seed == null) continue;
                seed.RequestDetonation(delayStep);
                delayStep += 3;
                anyDetonated = true;
            }
            if (anyDetonated)
            {
                SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.2f, Volume = 0.65f }, player.Center);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddRecipeGroup(nameof(ItemID.IronBar), 12)
            .AddIngredient(ItemID.FrostCore, 1)
            .AddIngredient(ModContent.ItemType<PulsatingFragment>(), 4)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        }
    }
}
