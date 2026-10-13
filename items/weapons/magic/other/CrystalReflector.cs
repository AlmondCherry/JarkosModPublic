using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.magic.other;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.other
{
    public class CrystalReflector : ModItem
    {
        public static readonly Color ColorIce = new Color(154, 208, 255);
        public static readonly Color ColorDeep = new Color(90, 140, 183);
        public static readonly Color ColorWhite = new Color(235, 248, 255);
        public static readonly Color ColorViolet = new Color(178, 140, 255);
        public const float CAST_RANGE = 60f * 16f;
        public override void SetDefaults()
        {
            Item.damage = 120;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 140;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.knockBack = 5f;
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ReflectorBarrier>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 80, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<ReflectorBarrier>()] == 0;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 target = Main.MouseWorld;
            Vector2 fromPlayer = target - player.MountedCenter;

            if (fromPlayer.Length() > CAST_RANGE)
                target = player.MountedCenter + fromPlayer.SafeNormalize(Vector2.UnitX) * CAST_RANGE;
                
            Projectile.NewProjectile(source, target, Vector2.Zero, ModContent.ProjectileType<ReflectorBarrier>(), (int)(damage * 0.4f), knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrystalShard, 18);
            recipe.AddIngredient(ItemID.SoulofLight, 10);
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
