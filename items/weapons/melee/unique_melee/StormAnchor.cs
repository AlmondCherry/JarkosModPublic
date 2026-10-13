using jarkos.projectiles.melee.unique_melee;
using Jarkos.items.materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.melee.unique_melee
{
    public class StormAnchor : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 115;
            Item.DamageType = DamageClass.Melee;
            Item.width = 32;
            Item.height = 96;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<StormAnchorStab>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 50, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<StormAnchorCharge>()] > 0
                || player.ownedProjectileCounts[ModContent.ProjectileType<StormAnchorThrown>()] > 0
                || player.ownedProjectileCounts[ModContent.ProjectileType<StormAnchorSpin>()] > 0)
                return false;
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.UseSound = null;
            }
            else
            {
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.UseSound = SoundID.Item1;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<StormAnchorCharge>(), damage, knockback, player.whoAmI);
                return false;
            }
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            var combo = player.GetModPlayer<StormAnchorPlayer>();
            if (combo.RegisterStab())
            {
                Projectile.NewProjectile(source, player.MountedCenter, dir, ModContent.ProjectileType<StormAnchorSpin>(), (int)(damage * 1.1f), knockback, player.whoAmI);
                return false;
            }
            float easeVariant = Main.rand.Next(3);
            float reach = Main.rand.NextFloat(44f, 68f);
            float wobble = Main.rand.NextFloat(-0.07f, 0.07f);
            Projectile.NewProjectile(source, player.MountedCenter, dir, ModContent.ProjectileType<StormAnchorStab>(), damage, knockback, player.whoAmI, easeVariant, reach, wobble);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<DiffusalBar>(), 12);
            recipe.AddIngredient(ItemID.Seashell, 10);
            recipe.AddIngredient(ItemID.SharkFin, 4);
            recipe.AddIngredient(ItemID.SoulofMight, 2);
            recipe.AddIngredient(ItemID.SoulofFright, 2);
            recipe.AddIngredient(ItemID.SoulofSight, 2);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
    public class StormAnchorPlayer : ModPlayer
    {
        public const int COMBO_HITS = 3;
        public const int COMBO_WINDOW = 75;
        public int comboCount;
        public int comboTimer;
        public override void PostUpdate()
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer == 0)
                    comboCount = 0;
            }
        }
        public bool RegisterStab()
        {
            comboCount++;
            comboTimer = COMBO_WINDOW;
            if (comboCount > COMBO_HITS)
            {
                comboCount = 0;
                comboTimer = 0;
                return true;
            }
            return false;
        }
    }
}
