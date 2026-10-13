using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.magic.toms;
using jarkos.items.usetiles;

namespace Jarkos.items.weapons.magic.toms
{
	public class TomeOfBloodNeedles : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Magic;

            Item.mana = 6;
            Item.width = 28;
            Item.height = 32;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.Item8 with { Pitch = 0.4f, Volume = 0.6f };
            Item.shoot = ModContent.ProjectileType<BloodNeedle_tome_proj>();
            Item.shootSpeed = 18f;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(8f));
            position += velocity.SafeNormalize(Vector2.Zero) * 20f;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
                .AddIngredient(ItemID.Stinger, 6)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
