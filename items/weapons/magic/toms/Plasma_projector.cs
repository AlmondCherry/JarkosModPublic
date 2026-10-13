using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic;
using jarkos.particles;
using jarkos;

namespace jarkos.items.weapons.magic.toms
{
    public class PlasmaProjectorPlayer : ModPlayer
    {
        public int cooldown = 0;

        public override void ResetEffects()
        {
            if (cooldown > 0)
            {
                cooldown--;
            }
        }
    }

    public class Plasma_projector : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 5));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 185;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 40;
            Item.height = 42;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.reuseDelay = 30;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.shootSpeed = 18f;
            Item.shoot = ModContent.ProjectileType<PlasmaProjectorHoldout>();
            Item.channel = true;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 10, 0, 0);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.GetModPlayer<PlasmaProjectorPlayer>().cooldown > 0)
                return false;

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<PlasmaProjectorHoldout>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(nameof(ItemID.IronBar), 20)
                .AddIngredient(ItemID.SpectreBar, 12)
                .AddIngredient(ItemID.Ectoplasm, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
