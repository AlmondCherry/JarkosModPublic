using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using Terraria.Audio;
using System;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Ethereal_rift : ModItem
	{
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 6));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }
		private int swingCounter = 0;
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 2953;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<jarkos.rare.CosmicRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Ethereal_rift_dante_swing>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<Ethereal_rift_domain>()] > 0)
            {
                return false;
            }

            if (player.altFunctionUse == 2)
            {
                if (player.velocity.Y != 0f || Math.Abs(player.velocity.X) > 0.1f)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<Ethereal_rift_stab>(), damage * 2, knockback, player.whoAmI);
                return false;
            }

            swingCounter++;
            bool isMegaSwing = swingCounter % 5 == 0;
            int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: isMegaSwing ? 2f : swingCounter % 2, ai1: swingCounter % 2 == 0 ? 1f : -1f);
            
            if (isMegaSwing)
            {
                SoundEngine.PlaySound(SoundID.Item105, player.Center);
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustDirect(player.Center, 0, 0, DustID.BlueFairy, dustVel.X, dustVel.Y, 150, default, 1.8f);
                    d.noGravity = true;
                }
            }
            return false;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = new Vector2(hitbox.X + Main.rand.Next(hitbox.Width), hitbox.Y + Main.rand.Next(hitbox.Height));
                Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.BlueFairy, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
    }
}
