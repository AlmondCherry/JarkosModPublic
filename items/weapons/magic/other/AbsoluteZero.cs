using jarkos.projectiles.magic.other;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.magic.other
{
    public class AbsoluteZero : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 28;
            Item.knockBack = 1.5f;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 3;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<AbsoluteZeroProj>();
            Item.shootSpeed = 22f;
            Item.UseSound = null;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float spread = MathHelper.ToRadians(14f);
            Vector2 perturbedVel = velocity.RotatedByRandom(spread);
            perturbedVel *= Main.rand.NextFloat(0.8f, 1.2f);

            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            Vector2 spawnOffset = dir * Main.rand.NextFloat(0f, 10f) + perp * Main.rand.NextFloat(-8f, 8f);

            float visualIndex = Main.rand.NextFloat(MathHelper.TwoPi);

            Projectile.NewProjectile(source, position + spawnOffset, perturbedVel, type, damage, knockback, player.whoAmI, ai1: visualIndex);

            SoundEngine.PlaySound(SoundID.Item28 with
            {
                Volume = 0.2f, Pitch = Main.rand.NextFloat(0.6f, 0.9f)
            }, position);

            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.Snow;
                Dust d = Dust.NewDustDirect(position + spawnOffset, 0, 0, dustType, -velocity.X * 0.03f + Main.rand.NextFloat(-1f, 1f), -velocity.Y * 0.03f + Main.rand.NextFloat(-1f, 1f), 120, default, Main.rand.NextFloat(0.7f, 1.3f));
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FrostCore, 1)
                .AddIngredient(ItemID.SoulofLight, 12)
                .AddIngredient(ItemID.FrostStaff, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
