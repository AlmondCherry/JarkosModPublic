using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using ReLogic.Content;
using Jarkos.items.materials;
using Jarkos.items.usetiles;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Multiwave_Emitter : ModItem
	{
		private float glowIntensity = 0f;
        private bool increasing = true;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 72;
            Item.damage = 11;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 8f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Multiwave_Emitter_beam>();
            Item.shootSpeed = 12f;
            Item.channel = false;
            Item.UseSound = null;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center + direction * 40f, direction * Item.shootSpeed, type, damage, knockback, player.whoAmI, 0f, 0f);
            player.velocity -= direction * 3f;
            CreateMuzzleFlash(player.Center + direction * 60f);
            return false;
        }
        private void CreateMuzzleFlash(Vector2 position)
        {
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);
                Dust dust = Dust.NewDustPerfect(position, DustID.SolarFlare, dustVel, 0, new Color(255, 200, 50), Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, dustVel, 0, Color.White, 2f);
                dust.noGravity = true;
            }
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            if (increasing)
            {
                glowIntensity += 0.02f;
                if (glowIntensity >= 1f) increasing = false;
            }
            else
            {
                glowIntensity -= 0.02f;
                if (glowIntensity <= 0.3f) increasing = true;
            }
            Color glowColor = new Color(255, 150, 50) * glowIntensity * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.05f)
                    .ToRotationVector2() * 4f * glowIntensity;
                spriteBatch.Draw(texture, Item.Center - Main.screenPosition + offset, null, glowColor, rotation, texture.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }
		public override Vector2? HoldoutOffset() {
			return new Vector2(-25f, 0f);
		}
		public override void AddRecipes() 
        {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<SteelBar>(), 4)
            .AddIngredient(ModContent.ItemType<Battery>(), 2)
			.AddRecipeGroup(nameof(ItemID.Vertebrae), 4)
            .AddIngredient(ItemID.Bone, 40)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
