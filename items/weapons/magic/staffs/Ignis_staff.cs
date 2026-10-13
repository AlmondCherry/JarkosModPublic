using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.staffs;
using Terraria.Audio;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.magic.staffs
{
    public class Ignis_staff : ModItem
    {
        private int shotCounter = 0;
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 5;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 5;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Ignis_staff_proj>();
            Item.shootSpeed = 14f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            if (shotCounter % 3 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item34 with
                {
                    Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.2f, 0.3f), MaxInstances = 5
                }, position);
            }
            if (shotCounter % 8 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item45 with
                {
                    Volume = 0.4f, Pitch = Main.rand.NextFloat(0.5f, 0.8f), MaxInstances = 3
                }, position);
            }
            Vector2 staffTip = position + Vector2.Normalize(velocity) * 45f;
            int dropletCount = Main.rand.Next(1, 3);
            for (int i = 0; i < dropletCount; i++)
            {
                float spread = MathHelper.ToRadians(Main.rand.NextFloat(-18f, 18f));
                Vector2 dropVelocity = velocity.RotatedBy(spread);
                dropVelocity *= Main.rand.NextFloat(0.8f, 1.2f);
                Vector2 spawnPos = staffTip + Main.rand.NextVector2Circular(8f, 8f);
                int proj = Projectile.NewProjectile(source, spawnPos, dropVelocity, type, damage, knockback, player.whoAmI, ai0: Main.rand.NextFloat(0.7f, 1.3f));
                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[1] = Main.rand.Next(3);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                Dust flame = Dust.NewDustPerfect(staffTip + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.5f, 2f), 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                flame.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Dust Spark = Dust.NewDustPerfect(staffTip, DustID.Lava, velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(1f, 3f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                Spark.noGravity = true;
            }
            return false;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-5, 0);
        }
        public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 8)
				.AddIngredient(ItemID.Topaz, 4)
				.AddTile(TileID.Anvils)
				.Register();
		}
    }
}
