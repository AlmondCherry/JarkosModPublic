using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.staffs;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.staffs
{
    public class SanguineDownpour : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 85;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.width = 36;
            Item.height = 36;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<SanguineDownpourHoldout>();
            Item.shootSpeed = 16f;
            Item.autoReuse = false;
            Item.channel = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 50, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BloodDroplet>(), 10)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void HoldItem(Player player)
        {
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = player.MountedCenter + new Vector2(player.direction * 24f, -24f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Blood, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0f, 1f), 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = false;
                d.velocity.Y += 1.5f;
            }
        }
    }
}
