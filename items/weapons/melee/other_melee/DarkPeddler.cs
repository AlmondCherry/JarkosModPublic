using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using Terraria.Audio;

namespace jarkos.items.weapons.melee.other_melee
{
	public class DarkPeddler : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 13));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
	  public override void SetDefaults()
        {
            Item.damage = 27;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 60;
            Item.useTime = 9;
            Item.useAnimation = 36;
            Item.reuseDelay = 25;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<Darkpedproj>();
            Item.UseSound = null;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Mage_slayer_shooting") with
            {
                Volume = 0.8f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f), PitchVariance = 0.1f
            }, player.Center);
            
            for (int i = 1; i <= 1; i++)
            {
                int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: i);
                if (proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].netUpdate = true;
                }
            }
            return false;
        }
		public override void AddRecipes()
		{
    		CreateRecipe()
			.AddTile(TileID.Anvils)
        	.AddIngredient(ItemID.MoltenHamaxe, 1)
			.AddIngredient(ItemID.Muramasa, 1)
            .AddIngredient(ItemID.MeteorHamaxe, 1)
			.AddRecipeGroup(nameof(ItemID.GoldAxe), 1)
        	.Register();
		}
	}
}
