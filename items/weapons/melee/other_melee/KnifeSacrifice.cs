using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.melee.other_melee;

namespace jarkos.items.weapons.melee.other_melee
{
	public class KnifeSacrifice : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.damage = 38;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 2.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Knife_sacrifice_proj>();
            Item.shootSpeed = 16f;

            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = Main.rand.NextFloat(-0.1f, 0.2f), Volume = 0.8f }, position);

            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5));
            Vector2 spawnOffset = velocity.SafeNormalize(Vector2.Zero) * 20f;

            Projectile.NewProjectile(source, position + spawnOffset, velocity, type, damage, knockback, player.whoAmI);

            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.1f, 0.3f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.GoldCoin, dustVel.X, dustVel.Y, 150, default, 0.7f);
                dust.noGravity = true;
            }
            return false;
        }
        
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2, 0);
        }
    }
}
