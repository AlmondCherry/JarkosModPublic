using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using Jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Kaliburn : ModItem
	{
		private int swingCounter = 0;
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 59;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Kaliburn_slash>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: 0f, ai1: swingCounter % 2 == 0 ? 1f : -1f);
            return false;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DiffusalBar>(), 12)
                .AddIngredient(ItemID.HellstoneBar, 8)
                .AddIngredient(ItemID.SoulofFright, 4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
