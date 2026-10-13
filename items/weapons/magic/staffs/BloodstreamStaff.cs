using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Jarkos.projectiles.magic.staffs;
using Terraria.DataStructures;
using System.Collections.Generic;

namespace jarkos.items.weapons.magic.staffs
{
    public class BloodstreamStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 28;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = SoundID.Item8;
            Item.shoot = ModContent.ProjectileType<BloodstreamBolt>();
            Item.shootSpeed = 15f;
            Item.autoReuse = true;
            Item.staff[Item.type] = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 50, 0);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
            position += velocity.SafeNormalize(Vector2.Zero) * 20f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 12)
                .AddIngredient(ItemID.TissueSample, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
