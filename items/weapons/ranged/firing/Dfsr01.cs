using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using jarkos.projectiles.ranged.firearm;
using jarkos.projectiles.mecha;

namespace jarkos.items.weapons.ranged.firing
{
	public class Dfsr01 : ModItem
	{
        public override void SetDefaults()
        {
            Item.damage = 35;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 24;
            Item.useTime = 9;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<SST01Player>();
            if (player.altFunctionUse == 2)
            {
                mp.fireMode = (mp.fireMode + 1) % 3;
                SoundEngine.PlaySound(SoundID.Unlock with
                {
                    Volume = 0.6f, Pitch = 0.2f + mp.fireMode * 0.15f
                }, player.Center);
                string mode = mp.GetModeName();
                CombatText.NewText(player.Hitbox, new Color(200, 200, 200), mode, false, false);
                return false;
            }
            switch (mp.fireMode)
            {
                case 0:
                    Item.useTime = 9;
                    Item.useAnimation = 9;
                    Item.autoReuse = true;
                    break;
                case 1:
                    Item.useTime = 18;
                    Item.useAnimation = 18;
                    Item.autoReuse = true;
                    break;
                case 2:
                    Item.useTime = 22;
                    Item.useAnimation = 22;
                    Item.autoReuse = false;
                    break;
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) return false;
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Dfsr01_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, -2f);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(nameof(ItemID.IronBar), 15);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddIngredient(ItemID.Musket, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class SST01Player : ModPlayer
    {
        public int fireMode = 0;
        public int burstShotsLeft = 0;
        public int burstTimer = 0;
        public Vector2 burstDirection = Vector2.Zero;
        public Vector2 burstSpawnPos = Vector2.Zero;
        public int burstDamage = 0;
        public float burstKnockback = 0f;
        public override void ResetEffects()
        {
            if (burstTimer > 0)
                burstTimer--;
            if (burstShotsLeft > 0 && burstTimer <= 0)
            {
                burstTimer = 4;
                burstShotsLeft--;
                FireBurstShot();
            }
        }
        private void FireBurstShot()
        {
            if (Main.myPlayer != Player.whoAmI) return;
            Vector2 vel = burstDirection.RotatedByRandom(MathHelper.ToRadians(0.8f));
            vel *= Main.rand.NextFloat(0.97f, 1.03f);
            int proj = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), burstSpawnPos, vel, ModContent.ProjectileType<Dfsr01_bullet>(), burstDamage, burstKnockback, Player.whoAmI);
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
                Main.projectile[proj].ai[1] = 1f;
            }
        }
        public string GetModeName()
        {
            return fireMode switch
            {
                0 => "Auto", 1 => "turn", 2 => "single", _ => "Auto"
            };
        }
    }
}
