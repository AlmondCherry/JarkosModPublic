using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Jarkos.projectiles.magic.toms;
using Terraria.DataStructures;
using Jarkos.items.materials;
using jarkos.items.materials;
using Terraria.Audio;

namespace Jarkos.items.weapons.magic.toms
{
	public class Tom_of_hell : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.damage = 18;
            Item.knockBack = 1.5f;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 3;
            Item.crit = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 4;
            Item.useAnimation = 4;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Hellstar>();
            Item.shootSpeed = 16f;
            Item.UseSound = null;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 20, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float spread = MathHelper.ToRadians(12f);
            Vector2 perturbedVel = velocity.RotatedByRandom(spread);
            perturbedVel *= Main.rand.NextFloat(0.85f, 1.15f);

            Vector2 spawnOffset = new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f));

            Projectile.NewProjectile(source, position + spawnOffset, perturbedVel, type, damage, knockback, player.whoAmI);

            if (Main.GameUpdateCount % 3 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.3f, Pitch = Main.rand.NextFloat(0.3f, 0.8f) }, position);
            }

            if (Main.GameUpdateCount % 12 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.2f, Pitch = -0.5f }, position);
            }

            for (int i = 0; i < 2; i++)
            {
                Dust bookDust = Dust.NewDustDirect(position, 0, 0, DustID.Torch, -velocity.X * 0.05f + Main.rand.NextFloat(-1f, 1f), -velocity.Y * 0.05f + Main.rand.NextFloat(-1f, 1f), 100, default, 1.2f);
                bookDust.noGravity = true;
                bookDust.fadeIn = 0.8f;
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpellTome, 1)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ModContent.ItemType<ResidualShadow>(), 8)
                .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 4)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
