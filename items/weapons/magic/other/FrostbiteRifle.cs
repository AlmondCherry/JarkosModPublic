using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.other;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.other
{
	public class FrostbiteRifle : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 22;
            Item.damage = 20;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.knockBack = 2.5f;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FrostPlasma>();
            Item.shootSpeed = 11f;
            Item.UseSound = SoundID.Item11 with { Pitch = 0.2f, Volume = 0.8f };

            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Green;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8f, 0f);
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzle = position + Vector2.Normalize(velocity) * 44f;
            if (Collision.CanHit(position, 0, 0, muzzle, 0, 0))
                position = muzzle;
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(1.5f));

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, Main.rand.NextFloat(MathHelper.TwoPi));

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(position, DustID.IceTorch, velocity * 0.06f + Main.rand.NextVector2Circular(1.2f, 1.2f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                dust.noGravity = true;
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EssentialPowder>(), 2)
                .AddIngredient(ItemID.IceBlock, 50)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
