using jarkos.items.materials;
using jarkos.projectiles;
using jarkos.rare;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable;
public sealed class PortalHook : ModItem
{
    public override string Texture => "Jarkos/textures/PortalHook/PortalHookItem";
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.GrapplingHook);
        Item.width = 28;
        Item.height = 28;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item1;
        Item.noMelee = true;
        Item.shoot = ModContent.ProjectileType<PortalHookProjectile>();
        Item.shootSpeed = 44f;
        Item.channel = true;
        Item.value = Item.sellPrice(gold: 99);
        Item.rare = ModContent.RarityType<CosmicRarity>();
        Item.maxStack = 1;
    }
    public override bool CanUseItem(Player player)
    {
        int hookType = ModContent.ProjectileType<PortalHookProjectile>();
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.type == hookType && p.owner == player.whoAmI)
                return false;
        }
        return true;
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile.NewProjectile(source, player.MountedCenter, velocity, type, Damage: 0, KnockBack: 0f, Owner: player.whoAmI);
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.LunarHook, 1)
            .AddIngredient(ModContent.ItemType<CelestiaStar>(), 1)
            .AddTile(TileID.LunarCraftingStation)
            .Register();
    }
}
