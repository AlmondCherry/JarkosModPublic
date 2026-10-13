using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Breeze_epithonium : ModItem
	{
		public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 85;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<Breeze_epithonium_slash>();
            Item.shootSpeed = 12f;
            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
                return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == Item.shoot);
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == Item.shoot && !(n.ai[0] == 1 && n.ai[1] == 1));
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.8f }, player.Center);
            float state = 0;
            bool empoweredSlash = false;
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == player.whoAmI && p.type == Item.shoot && p.ai[0] == 1 && p.ai[1] == 1 && p.timeLeft > 180)
                {
                    empoweredSlash = true;
                    break;
                }
            }
            if (empoweredSlash)
            {
                state = 2;
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.owner != player.whoAmI || p.type != Item.shoot || p.ai[0] != 1 || p.ai[1] != 1)
                        continue;
                    p.timeLeft = 180;
                    p.netUpdate = true;
                    p.netSpam = 0;
                }
            }
            else if (player.altFunctionUse == 2)
            {
                state = 1;
            }
            Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI, state, 0);
            return false;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Water, 0f, 0f, 100, new Color(100, 200, 255), 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }
		public override void AddRecipes()
		{
			CreateRecipe()
            .AddIngredient(ItemID.HallowedBar, 12)
			.AddIngredient(ItemID.Coral, 5)
			.AddIngredient(ItemID.Seashell, 5)
			.AddIngredient(ItemID.WaterBucket, 2)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
