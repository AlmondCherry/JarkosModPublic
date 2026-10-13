using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using Terraria.Audio;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.other_melee
{
	public class DarkPeddlerTrue : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 15));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
	  public override void SetDefaults()
        {
            Item.damage = 56;
            Item.DamageType = DamageClass.Melee;
            Item.width = 34;
            Item.height = 52;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.reuseDelay = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.shootSpeed = 18;
            Item.UseSound = null;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Darkped_trueproj>();
            Item.autoReuse = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 3, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/DPShoot") with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f)}, player.position);

            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.2f, 0.5f);
                Dust dust = Dust.NewDustDirect(position, 10, 10, DustID.LifeDrain, dustVel.X, dustVel.Y, 100, Color.Red, 1.3f);
                dust.noGravity = true;
            }
            return true;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position += velocity.SafeNormalize(Vector2.Zero) * 20f;
        }
		public override void AddRecipes()
		{
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<DarkPeddler>(), 1)
            .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 12)
			.AddIngredient(ItemID.SoulofMight, 20)
            .AddIngredient(ItemID.SoulofFright, 20)
            .AddIngredient(ItemID.SoulofSight, 20)
            .AddTile(TileID.MythrilAnvil)
        	.Register();
		}
	}
}
