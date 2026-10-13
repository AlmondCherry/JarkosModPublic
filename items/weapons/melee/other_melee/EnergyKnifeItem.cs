using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using jarkos.items.usetiles;
using jarkos.items.materials;
using jarkos.particles;

namespace jarkos.items.weapons.melee.other_melee
{
	public class EnergyKnifeItem : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 52;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Energy_knife_proj01>();
            Item.shootSpeed = 18f;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(3f));
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            for (int i = 0; i < 3; i++)
            {
                Vector2 particleVel = velocity.RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(0.15f, 0.5f);
                new Spark(position, particleVel, Color.Cyan, Main.rand.Next(15, 30), Main.rand.NextFloat(0.25f, 0.35f), 1.0f, 0.92f).Spawn();
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<PenetrationKnife>(), 1)
            .AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 4)
            .AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        }
    }
}
