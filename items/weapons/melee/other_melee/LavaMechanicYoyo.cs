using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.other_melee;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using jarkos.items.weapons.melee.true_melee;
using Terraria.DataStructures;

namespace jarkos.items.weapons.melee.other_melee
{
	public class LavaMechanicYoyo : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LavaMechanicYoyo_proj>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.Item23 with { Pitch = 0.3f }, player.Center);

            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0.2f, 0.5f);
                Dust.NewDust(position, 10, 10, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 1.2f);
            }
            return true;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Incineration>(), 1);
            recipe.AddIngredient(ItemID.HallowedBar, 6);
            recipe.AddIngredient(ItemID.SoulofMight, 8);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}
